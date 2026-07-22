#!/usr/bin/env node
/**
 * scripts/seed-bank.js
 *
 * Seeds all 21 k+ real Balochistan board questions from
 * src/assets/data/assessments/bank/*.json into the backend database
 * via POST /api/questions (existing endpoint, admin-only).
 *
 * Idempotent: fetches existing stems before each file and skips dupes.
 *
 * Usage:
 *   node scripts/seed-bank.js
 *
 * Env vars (all optional):
 *   SEED_URL       Backend base URL  (default: http://116.203.172.111:7014)
 *   SEED_USERNAME  Admin username    (default: admin)
 *   SEED_PASSWORD  Admin password    (default: Admin@123)
 *   CONCURRENCY    Parallel requests (default: 15)
 */

import fs   from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname   = path.dirname(fileURLToPath(import.meta.url))
const BASE_URL    = (process.env.SEED_URL      || 'http://116.203.172.111:7014').replace(/\/$/, '')
const USERNAME    = process.env.SEED_USERNAME  || 'admin'
const PASSWORD    = process.env.SEED_PASSWORD  || 'Admin@123'
const CONCURRENCY = parseInt(process.env.CONCURRENCY || '15', 10)
const BANK_DIR    = path.join(__dirname, '../src/assets/data/assessments/bank')
const INDEX_PATH  = path.join(__dirname, '../src/assets/data/assessments/index.json')

// Valid grade codes in the DB
const VALID_GRADES = new Set(['1','2','3','4','5','6','7','8','9','10','11','12','entry'])

// JSON subject name → DB Subject.Name mapping
const SUBJECT_ALIASES = {
  'science':          'General Science',
  'general knowledge':'General Science',
  'pak studies':      'Pakistan Studies',
}

// ── HTTP helpers ──────────────────────────────────────────────────────────────

async function request(method, endpoint, body, token) {
  const res = await fetch(`${BASE_URL}${endpoint}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
  })
  if (!res.ok) {
    const txt = await res.text().catch(() => '')
    throw new Error(`HTTP ${res.status}: ${txt.slice(0, 300)}`)
  }
  return res.json()
}

const get  = (ep, token)       => request('GET',  ep, null, token)
const post = (ep, body, token) => request('POST', ep, body, token)

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

// ── Fetch existing stems for a grade+subject (for dedup on re-run) ─────────────

async function fetchExistingStems(gradeCode, subjectId, token) {
  const stems = new Set()
  let page = 1
  while (true) {
    const data = await get(
      `/api/questions?gradeCode=${gradeCode}&subjectId=${subjectId}&pageSize=500&page=${page}`,
      token
    )
    const items = Array.isArray(data) ? data : (data.items || [])
    items.forEach(q => stems.add((q.stem || '').trim()))
    const total = data.totalCount ?? items.length
    if (page * 500 >= total || !items.length) break
    page++
  }
  return stems
}

// ── Main ──────────────────────────────────────────────────────────────────────

async function main() {
  // 1. Login
  console.log(`🔑  Logging in as "${USERNAME}" at ${BASE_URL} …`)
  const auth = await post('/api/auth/login', { username: USERNAME, password: PASSWORD })
  const token = auth.accessToken || auth.token
  if (!token) throw new Error('No token in login response')
  console.log('    ✓ Authenticated\n')

  // 2. Fetch subjects → build name→ID map (case-insensitive)
  const subjects = await get('/api/admin/subjects', token)
  const subjectIdMap = {}
  subjects.forEach(s => { subjectIdMap[s.name.toLowerCase()] = s.id })
  Object.entries(SUBJECT_ALIASES).forEach(([alias, canonical]) => {
    if (subjectIdMap[canonical.toLowerCase()])
      subjectIdMap[alias] = subjectIdMap[canonical.toLowerCase()]
  })

  function resolveId(name) {
    return subjectIdMap[(name || '').toLowerCase()] ?? null
  }

  const files = fs.readdirSync(BANK_DIR).filter(f => f.endsWith('.json')).sort()
  console.log(`📂  ${files.length} bank files\n`)

  let grandTotal = 0, grandInserted = 0, grandFailed = 0, grandSkipped = 0

  for (const file of files) {
    const label    = file.replace('.json', '')
    const raw      = JSON.parse(fs.readFileSync(path.join(BANK_DIR, file), 'utf8'))
    const [gradeCode, ...subjectParts] = label.split('__')
    const subjectSlug = subjectParts.join('__')

    // Skip grade codes not in the DB (e.g. "ecd")
    if (!VALID_GRADES.has(gradeCode)) {
      console.log(`  ⏩  ${label.padEnd(28)} grade "${gradeCode}" not in DB — skip`)
      continue
    }

    // Resolve SubjectId
    const sampleSubject = raw[0]?.subject || ''
    let subjectId = resolveId(sampleSubject)
    if (!subjectId) subjectId = resolveId(subjectSlug.replace(/-/g, ' '))
    if (!subjectId) {
      console.log(`  ⚠️  ${label.padEnd(28)} cannot resolve subjectId — skip`)
      continue
    }

    // Fetch existing stems to skip already-inserted questions
    const existingStems = await fetchExistingStems(gradeCode, subjectId, token)

    // Filter: only valid (has q + options) and not yet in DB
    const items = raw.filter(q =>
      q.q && Array.isArray(q.options) && q.options.length >= 2 &&
      !existingStems.has(q.q.trim())
    )

    if (!items.length) {
      console.log(`  ⏩  ${label.padEnd(28)} all ${raw.length} already seeded`)
      grandSkipped += raw.length
      grandTotal   += raw.length
      continue
    }

    const tasks = items.map(q => () => post('/api/questions', {
      kind:          'objective',
      stem:          q.q.slice(0, 1999),
      gradeCode:     gradeCode || null,
      subjectId,
      optionsJson:   JSON.stringify(q.options),
      correctIndex:  typeof q.correct === 'number' ? q.correct : null,
      difficulty:    'Medium',
      cognitiveLevel:'Understanding',
      feedback:      q.feedback ? q.feedback.slice(0, 999) : null,
      sloCode:       q.skill    ? q.skill.slice(0, 49)     : null,
      isEntranceExam:false,
      isAiGenerated: false,
    }, token))

    process.stdout.write(`  ⏳  ${label.padEnd(28)} inserting ${items.length} questions…`)
    const { ok, fail } = await runPool(tasks, CONCURRENCY)
    process.stdout.write(`\r  ✓   ${label.padEnd(28)} ${String(ok).padStart(5)} inserted  ${String(fail).padStart(4)} failed\n`)

    grandTotal    += raw.length
    grandInserted += ok
    grandFailed   += fail
    grandSkipped  += (raw.length - items.length) // previously seeded
  }

  console.log(`\n${'─'.repeat(55)}`)
  console.log(`✅  Seeding complete!`)
  console.log(`    Total     : ${grandTotal}`)
  console.log(`    Inserted  : ${grandInserted}`)
  console.log(`    Failed    : ${grandFailed}`)
  console.log(`    Pre-seeded: ${grandSkipped}`)
  console.log(`${'─'.repeat(55)}\n`)
}

main().catch(err => { console.error('\n❌', err.message); process.exit(1) })
