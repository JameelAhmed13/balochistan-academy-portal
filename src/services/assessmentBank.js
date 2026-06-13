/**
 * assessmentBank.js — access to the REAL question bank ingested from
 * docs/Digital Assessments (see scripts/ingest_assessments.py).
 *
 * 17.5k+ real Balochistan board questions, grouped per grade+subject and
 * lazy-loaded on demand (each bank file is a separate Vite chunk, fetched
 * only when that grade+subject is opened). This is the source of truth that
 * replaces the old mock/dummy question banks, and the corpus the AI grounds
 * on (RAG) for generating new, on-syllabus tests.
 */
import index from '../assets/data/assessments/index.json'

// Lazy importers for every per-grade-subject bank file.
const loaders = import.meta.glob('../assets/data/assessments/bank/*.json')
const cache = {}

function slug(s) {
  return String(s || '').toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '')
}
function shuffle(arr) {
  const r = arr.slice()
  for (let i = r.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[r[i], r[j]] = [r[j], r[i]]
  }
  return r
}

export const assessmentCatalogue = index

export function gradesAvailable() { return Object.keys(index) }
export function subjectsForGrade(grade) { return Object.keys(index[String(grade)] || {}) }
export function hasRealBank(grade, subject) { return !!index[String(grade)]?.[subject] }
export function unitsFor(grade, subject) { return index[String(grade)]?.[subject]?.units || [] }
export function topicsFor(grade, subject) { return index[String(grade)]?.[subject]?.topics || [] }
export function countFor(grade, subject) { return index[String(grade)]?.[subject]?.count || 0 }

// Map app subject names → ingested bank subject names where they differ.
const SUBJECT_ALIASES = {
  Math: 'Mathematics',
  'Pakistan Studies': 'Pak Studies',
  'Mutalia Pakistan': 'Pak Studies',
  'Islamiyat Lazmi': 'Islamiat',
}
export function resolveSubject(grade, subject) {
  const g = index[String(grade)] || {}
  if (g[subject]) return subject
  if (SUBJECT_ALIASES[subject] && g[SUBJECT_ALIASES[subject]]) return SUBJECT_ALIASES[subject]
  // case-insensitive match
  const hit = Object.keys(g).find((k) => k.toLowerCase() === String(subject).toLowerCase())
  return hit || null
}

/** Load (and cache) the raw bank array for a grade+subject. */
export async function loadBank(grade, subject) {
  const g = String(grade)
  const subj = resolveSubject(g, subject)
  if (!subj) return []
  const key = `${g}__${subj}`
  if (cache[key]) return cache[key]
  const file = `${slug(g)}__${slug(subj)}.json`
  const path = Object.keys(loaders).find((p) => p.endsWith('/' + file))
  if (!path) return []
  try {
    const mod = await loaders[path]()
    cache[key] = mod.default || mod
    return cache[key]
  } catch {
    return []
  }
}

/**
 * Get real questions normalized to the app's objective-question shape.
 * @param {object} o { grade, subject, unit?, topic?, limit? }
 * @returns {Promise<Array>} normalized questions (empty if no real bank)
 */
export async function getRealQuestions({ grade, subject, unit, topic, limit = 20 } = {}) {
  let qs = await loadBank(grade, subject)
  if (!qs.length) return []
  if (unit) qs = qs.filter((q) => q.unit === unit)
  if (topic) {
    const t = String(topic).toLowerCase()
    qs = qs.filter((q) => (q.topic || '').toLowerCase().includes(t))
  }
  qs = shuffle(qs)
  if (limit) qs = qs.slice(0, limit)
  return qs.map((q, i) => ({
    id: 9000000 + i,
    subject: q.subject,
    unit: q.unit,
    topic: q.topic || q.unit || q.subject,
    stem: q.q,
    options: q.options,
    correct: q.correct,
    type: 'Exercise',
    difficulty: 'Medium',
    cognitiveLevel: 'Understanding',
    slo: q.skill || '',
    entranceExam: false,
    reason: q.feedback || 'Refer to your textbook for the detailed explanation.',
    article: q.article || null,
    real: true,
  }))
}

/**
 * A compact, plain-text knowledge snippet for grounding the AI (RAG):
 * a sample of real questions for a grade+subject(+unit/topic) the LLM can
 * imitate in style and stay on-syllabus.
 */
export async function realContextSnippet({ grade, subject, unit, topic, sample = 8 } = {}) {
  const qs = await getRealQuestions({ grade, subject, unit, topic, limit: sample })
  if (!qs.length) return ''
  const lines = qs.map((q, i) =>
    `${i + 1}. ${q.stem}\n   Options: ${q.options.join(' | ')}\n   Correct: ${q.options[q.correct]}` +
    (q.topic ? `\n   Topic: ${q.topic}` : ''))
  return `Here are real ${subject} board questions for Grade ${grade}${unit ? ' (' + unit + ')' : ''}. ` +
    `Match THIS style, difficulty and topic coverage:\n\n${lines.join('\n\n')}`
}
