/**
 * assessmentBank.js — access to the real question bank.
 *
 * Questions are stored in the backend database (seeded via `npm run seed-bank`).
 * This module calls the API instead of loading static JSON files.
 *
 * The index.json catalogue (grades/subjects/topics available) is still read
 * from the bundled asset — it is small metadata that never changes without a
 * new deployment.
 */
import index from '../assets/data/assessments/index.json'
import api from './api'

export const assessmentCatalogue = index

export function gradesAvailable()          { return Object.keys(index) }
export function subjectsForGrade(grade)    { return Object.keys(index[String(grade)] || {}) }
export function hasRealBank(grade, subject){ return !!index[String(grade)]?.[resolveSubject(grade, subject)] }
export function unitsFor(grade, subject)   { return index[String(grade)]?.[resolveSubject(grade, subject)]?.units  || [] }
export function topicsFor(grade, subject)  { return index[String(grade)]?.[resolveSubject(grade, subject)]?.topics || [] }
export function countFor(grade, subject)   { return index[String(grade)]?.[resolveSubject(grade, subject)]?.count  || 0 }

// Map frontend subject names → backend DB subject names where they differ.
const SUBJECT_ALIASES = {
  'Math':              'Mathematics',
  'Pakistan Studies':  'Pakistan Studies',
  'Mutalia Pakistan':  'Pakistan Studies',
  'Islamiyat Lazmi':   'Islamiat',
  'Science':           'General Science',
  'General Knowledge': 'General Science',
  'Pak Studies':       'Pakistan Studies',
}

export function resolveSubject(grade, subject) {
  const g = index[String(grade)] || {}
  if (g[subject]) return subject
  if (SUBJECT_ALIASES[subject] && g[SUBJECT_ALIASES[subject]]) return SUBJECT_ALIASES[subject]
  const hit = Object.keys(g).find(k => k.toLowerCase() === String(subject).toLowerCase())
  return hit || SUBJECT_ALIASES[subject] || subject
}

// ── Subject ID cache (name → id, resolved via public /api/grades/{code}/subjects) ──

const _subjectIdCache  = new Map()   // subjectName.toLowerCase() → id
const _fetchedGrades   = new Set()   // grades already fetched

function _applyAliases() {
  const gs = _subjectIdCache.get('general science')
  if (gs) { _subjectIdCache.set('science', gs); _subjectIdCache.set('general knowledge', gs) }
  const ps = _subjectIdCache.get('pakistan studies')
  if (ps) { _subjectIdCache.set('pak studies', ps); _subjectIdCache.set('mutalia pakistan', ps) }
  const is = _subjectIdCache.get('islamiat')
  if (is) _subjectIdCache.set('islamiyat lazmi', is)
  const ma = _subjectIdCache.get('mathematics')
  if (ma) _subjectIdCache.set('math', ma)
}

async function ensureGradeSubjects(grade) {
  const key = String(grade || '9')
  if (_fetchedGrades.has(key)) return
  _fetchedGrades.add(key)
  try {
    const { data } = await api.get(`/grades/${key}/subjects`)
    const list = Array.isArray(data) ? data : []
    list.forEach(s => { if (s.id && s.name) _subjectIdCache.set(s.name.toLowerCase(), s.id) })
    _applyAliases()
  } catch { /* fall through — subjectId will be null */ }
}

async function resolveSubjectId(subject, grade) {
  if (!subject) return null
  await ensureGradeSubjects(grade)
  const id = _subjectIdCache.get(subject.toLowerCase())
  if (id) return id
  // If still not found, fetch a broader grade (9 covers most subjects)
  await ensureGradeSubjects('9')
  return _subjectIdCache.get(subject.toLowerCase()) ?? null
}

/** Normalize a QuestionDto from the backend into the app's question shape. */
function normalize(dto) {
  let options = []
  try { options = JSON.parse(dto.optionsJson || dto.OptionsJson || '[]') } catch { /**/ }
  return {
    id:             dto.id            || dto.Id,
    subject:        dto.gradeCode     || dto.GradeCode || '',
    unit:           null,
    topic:          null,
    stem:           dto.stem          || dto.Stem          || '',
    options,
    correct:        dto.correctIndex  ?? dto.CorrectIndex  ?? 0,
    type:           'Exercise',
    difficulty:     dto.difficulty    || dto.Difficulty    || 'Medium',
    cognitiveLevel: dto.cognitiveLevel|| dto.CognitiveLevel|| 'Understanding',
    slo:            dto.sloCode       || dto.SloCode       || '',
    entranceExam:   dto.isEntranceExam|| dto.IsEntranceExam|| false,
    reason:         dto.feedback      || dto.Feedback      || 'Refer to your textbook for the detailed explanation.',
    real:           true,
  }
}

/**
 * Fetch random questions for a grade+subject from the backend.
 * Falls back to an empty array (caller in content.js will use the local mock bank).
 */
export async function getRealQuestions({ grade, subject, unit, topic, limit = 20 } = {}) {
  const resolved   = resolveSubject(grade, subject) || subject
  const subjectId  = await resolveSubjectId(resolved, grade)
  const params = new URLSearchParams({ kind: 'objective', count: String(limit) })
  if (grade)      params.set('gradeCode',  String(grade))
  if (subjectId)  params.set('subjectId',  String(subjectId))

  try {
    const { data } = await api.get(`/questions/random?${params}`)
    const questions = Array.isArray(data) ? data : (data.items || [])
    return questions.map(normalize)
  } catch {
    return []
  }
}

/**
 * Load all questions for a grade+subject (paginated, up to 200).
 * Used by MCQsBank and similar views that want the full list.
 */
export async function loadBank(grade, subject) {
  const resolved   = resolveSubject(grade, subject) || subject
  const subjectId  = await resolveSubjectId(resolved, grade)
  const params = new URLSearchParams({ kind: 'objective', pageSize: '200', page: '1' })
  if (grade)     params.set('gradeCode',  String(grade))
  if (subjectId) params.set('subjectId',  String(subjectId))

  try {
    const { data } = await api.get(`/questions?${params}`)
    const items = Array.isArray(data) ? data : (data.items || [])
    return items.map(normalize)
  } catch {
    return []
  }
}

/**
 * A compact knowledge snippet for AI grounding (RAG).
 * Returns a plain-text sample of real questions to keep the LLM on-syllabus.
 */
export async function realContextSnippet({ grade, subject, unit, topic, sample = 8 } = {}) {
  const qs = await getRealQuestions({ grade, subject, unit, topic, limit: sample })
  if (!qs.length) return ''
  const lines = qs.map((q, i) =>
    `${i + 1}. ${q.stem}\n   Options: ${q.options.join(' | ')}\n   Correct: ${q.options[q.correct]}` +
    (q.topic ? `\n   Topic: ${q.topic}` : ''))
  return `Here are real ${subject} board questions for Grade ${grade}. ` +
    `Match THIS style, difficulty and topic coverage:\n\n${lines.join('\n\n')}`
}
