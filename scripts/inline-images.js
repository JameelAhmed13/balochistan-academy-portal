#!/usr/bin/env node
/**
 * scripts/inline-images.js
 *
 * Replaces all external <img src="https://..."> URLs inside question
 * stems and optionsJson with base64 data-URIs so the DB is self-contained.
 *
 * Safe to re-run — images already inlined are skipped, cached downloads
 * are never re-fetched.
 *
 * Usage (deployed):  node scripts/inline-images.js
 * Usage (local):     SEED_URL=http://localhost:5000 node scripts/inline-images.js
 *
 * Env vars:
 *   SEED_URL        API base URL  (default: http://116.203.172.111:7014)
 *   SEED_USERNAME   Admin user    (default: admin)
 *   SEED_PASSWORD   Admin pass    (default: Admin@123)
 *   DL_CONCURRENCY  Parallel downloads (default: 15)
 *   UP_CONCURRENCY  Parallel DB updates (default: 8)
 */

import fs   from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname     = path.dirname(fileURLToPath(import.meta.url))
const BASE_URL      = (process.env.SEED_URL      || 'http://116.203.172.111:7014').replace(/\/$/, '')
const USERNAME      = process.env.SEED_USERNAME  || 'admin'
const PASSWORD      = process.env.SEED_PASSWORD  || 'Admin@123'
const DL_CONC       = parseInt(process.env.DL_CONCURRENCY || '15', 10)
const UP_CONC       = parseInt(process.env.UP_CONCURRENCY || '8',  10)
const BANK_DIR      = path.join(__dirname, '../src/assets/data/assessments/bank')
const CACHE_FILE    = path.join(__dirname, '../.image-cache.json')
const FAILED_FILE   = path.join(__dirname, '../.image-failed.json')

// Matches http(s) URLs that look like image files inside src= attributes
const SRC_RE = /\bsrc=(["'])(https?:\/\/[^"'<>\s]+\.(?:png|jpg|jpeg|gif|webp|svg)[^"'<>\s]*)\1/gi

function extractUrls(text) {
  if (!text || !text.includes('http')) return []
  const urls = []
  let m
  SRC_RE.lastIndex = 0
  while ((m = SRC_RE.exec(text)) !== null) urls.push(m[2])
  return urls
}

function replaceUrls(text, cache) {
  if (!text || !text.includes('http')) return { text, changed: false }
  let changed = false
  const result = text.replace(SRC_RE, (_match, q, url) => {
    if (cache[url] && !cache[url].startsWith('FAIL:')) {
      changed = true
      return `src=${q}${cache[url]}${q}`
    }
    return _match
  })
  return { text: result, changed }
}

// ── HTTP helpers ──────────────────────────────────────────────────────────────

async function apiFetch(method, endpoint, body, token) {
  const res = await fetch(`${BASE_URL}${endpoint}`, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: body ? JSON.stringify(body) : undefined,
  })
  if (!res.ok) {
    const txt = await res.text().catch(() => '')
    throw new Error(`${method} ${endpoint} → HTTP ${res.status}: ${txt.slice(0, 200)}`)
  }
  if (res.status === 204) return null
  return res.json()
}

async function downloadImage(url) {
  const res = await fetch(url, {
    signal: AbortSignal.timeout(20000),
    headers: { 'User-Agent': 'Mozilla/5.0 (compatible; BAP-Image-Inliner/1.0)' },
  })
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  const buf  = await res.arrayBuffer()
  const mime = (res.headers.get('content-type') || 'image/png').split(';')[0].trim()
  const b64  = Buffer.from(buf).toString('base64')
  return `data:${mime};base64,${b64}`
}

// ── Concurrency pool ──────────────────────────────────────────────────────────

async function runPool(tasks, concurrency) {
  let ok = 0, fail = 0
  const queue = [...tasks]
  async function worker() {
    while (queue.length) {
      const task = queue.shift()
      try { await task(); ok++ } catch { fail++ }
    }
  }
  await Promise.all(Array.from({ length: concurrency }, worker))
  return { ok, fail }
}

// ── Main ──────────────────────────────────────────────────────────────────────

async function main() {
  // ── Phase 1: Collect all unique image URLs from JSON bank files ───────────
  console.log('🔍  Scanning bank JSON files for image URLs…')
  const bankFiles = fs.readdirSync(BANK_DIR).filter(f => f.endsWith('.json'))
  const allUrls   = new Set()

  for (const f of bankFiles) {
    const raw = JSON.parse(fs.readFileSync(path.join(BANK_DIR, f), 'utf8'))
    for (const q of raw) {
      const texts = [q.q, ...(q.options || []), q.feedback].filter(Boolean)
      texts.forEach(t => extractUrls(t).forEach(u => allUrls.add(u)))
    }
  }
  console.log(`    Found ${allUrls.size} unique image URLs in ${bankFiles.length} files\n`)

  // ── Phase 2: Download missing images into local cache ─────────────────────
  const cache  = fs.existsSync(CACHE_FILE)  ? JSON.parse(fs.readFileSync(CACHE_FILE,  'utf8')) : {}
  const failed = fs.existsSync(FAILED_FILE) ? JSON.parse(fs.readFileSync(FAILED_FILE, 'utf8')) : {}

  const toDownload = [...allUrls].filter(u => !cache[u] && !failed[u])
  console.log(`📥  Downloading ${toDownload.length} new images (${Object.keys(cache).length} already cached, ${Object.keys(failed).length} known failures)…`)

  let dlDone = 0
  const dlTasks = toDownload.map(url => async () => {
    try {
      cache[url] = await downloadImage(url)
    } catch (e) {
      failed[url] = e.message
    }
    dlDone++
    if (dlDone % 100 === 0 || dlDone === toDownload.length)
      process.stdout.write(`\r    ${dlDone}/${toDownload.length} downloaded…`)
  })

  await runPool(dlTasks, DL_CONC)
  if (toDownload.length) process.stdout.write('\n')

  // Persist cache and failures
  fs.writeFileSync(CACHE_FILE,  JSON.stringify(cache,  null, 0))
  fs.writeFileSync(FAILED_FILE, JSON.stringify(failed, null, 0))

  const successCount = Object.keys(cache).length
  const failCount    = Object.keys(failed).length
  console.log(`    ✓ Cache: ${successCount} images ready, ${failCount} permanently failed\n`)

  if (successCount === 0) {
    console.log('❌  No images downloaded — check network access. Aborting DB update.')
    return
  }

  // ── Phase 3: Login ────────────────────────────────────────────────────────
  console.log(`🔑  Logging in as "${USERNAME}" at ${BASE_URL} …`)
  const auth  = await apiFetch('POST', '/api/auth/login', { username: USERNAME, password: PASSWORD })
  const token = auth.accessToken || auth.token
  if (!token) throw new Error('No token in login response')
  console.log('    ✓ Authenticated\n')

  // ── Phase 4: Paginate through all DB questions ────────────────────────────
  console.log('📖  Fetching all questions from DB…')
  const PAGE_SIZE = 200
  let page = 1, totalFetched = 0
  const toUpdate = [] // { id, stem?, optionsJson? }

  while (true) {
    const data  = await apiFetch('GET', `/api/questions?pageSize=${PAGE_SIZE}&page=${page}`, null, token)
    const items = Array.isArray(data) ? data : (data.items || [])
    if (!items.length) break

    for (const q of items) {
      const { text: newStem,    changed: sc } = replaceUrls(q.stem,        cache)
      const { text: newOptions, changed: oc } = replaceUrls(q.optionsJson, cache)
      if (sc || oc) {
        toUpdate.push({
          id:          q.id,
          stem:        sc ? newStem    : undefined,
          optionsJson: oc ? newOptions : undefined,
        })
      }
    }

    totalFetched += items.length
    process.stdout.write(`\r    Scanned ${totalFetched} questions, ${toUpdate.length} need updating…`)
    if (items.length < PAGE_SIZE) break
    page++
  }
  process.stdout.write('\n')
  console.log(`\n🖊️   Updating ${toUpdate.length} questions with inlined images…`)

  // ── Phase 5: Patch questions in DB ────────────────────────────────────────
  // Uses PATCH /api/questions/{id}/inline which has no MaximumLength validator,
  // so base64-encoded images (100 KB+) are accepted without error.
  let upDone = 0
  const upTasks = toUpdate.map(q => async () => {
    const body = {}
    if (q.stem        !== undefined) body.stem        = q.stem
    if (q.optionsJson !== undefined) body.optionsJson = q.optionsJson
    await apiFetch('PATCH', `/api/questions/${q.id}/inline`, body, token)
    upDone++
    if (upDone % 50 === 0 || upDone === toUpdate.length)
      process.stdout.write(`\r    ${upDone}/${toUpdate.length} updated…`)
  })

  const { ok, fail } = await runPool(upTasks, UP_CONC)
  process.stdout.write('\n')

  console.log(`\n${'─'.repeat(55)}`)
  console.log(`✅  Done!`)
  console.log(`    Images cached : ${successCount}`)
  console.log(`    Images failed : ${failCount}`)
  console.log(`    DB updated    : ${ok}`)
  console.log(`    DB failed     : ${fail}`)
  console.log(`${'─'.repeat(55)}\n`)

  if (failCount > 0) {
    console.log(`⚠️  ${failCount} image URLs could not be downloaded.`)
    console.log(`   Their original URLs remain in the DB.`)
    console.log(`   See .image-failed.json for the list.\n`)
  }
}

main().catch(err => { console.error('\n❌', err.message); process.exit(1) })
