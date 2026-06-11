// Ollama LLM service — generates exam questions on-demand for tests,
// predictive papers and "generate test" flows.
//
// Configured to match the user's environment:
//   model    : llama3   (VITE_OLLAMA_MODEL)
//   endpoint : http://localhost:11434  (proxied via Vite as /ollama to avoid CORS)
//   timeout  : 30000ms  (VITE_OLLAMA_TIMEOUT_MS)
//
// Every generator degrades gracefully: if Ollama is unreachable, slow, or
// returns malformed output, the caller falls back to the local question bank,
// so a test is ALWAYS produced (never a dead/empty screen).

// In dev we call the Vite proxy path "/ollama" (same-origin → no CORS).
// Set VITE_OLLAMA_URL to call a host directly instead.
const BASE = (import.meta.env.VITE_OLLAMA_URL || '/ollama').replace(/\/$/, '')
const MODEL = import.meta.env.VITE_OLLAMA_MODEL || 'llama3'
const TIMEOUT = Number(import.meta.env.VITE_OLLAMA_TIMEOUT_MS || 30000)

let _idSeq = 700000

function nextId() { return ++_idSeq }

async function ollamaGenerate(prompt, { json = true, timeout = TIMEOUT, numPredict } = {}) {
  const ctrl = new AbortController()
  const timer = setTimeout(() => ctrl.abort(), timeout)
  try {
    const res = await fetch(`${BASE}/api/generate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      signal: ctrl.signal,
      body: JSON.stringify({
        model: MODEL,
        prompt,
        stream: false,
        ...(json ? { format: 'json' } : {}),
        options: { temperature: 0.5, ...(numPredict ? { num_predict: numPredict } : {}) },
      }),
    })
    if (!res.ok) throw new Error(`ollama-http-${res.status}`)
    const data = await res.json()
    if (data.error) throw new Error(data.error)
    return data.response || ''
  } finally {
    clearTimeout(timer)
  }
}

// Lightweight reachability probe (used to decide whether to even try).
export async function isOllamaAvailable(timeout = 3000) {
  try {
    const ctrl = new AbortController()
    const timer = setTimeout(() => ctrl.abort(), timeout)
    const res = await fetch(`${BASE}/api/tags`, { signal: ctrl.signal })
    clearTimeout(timer)
    return res.ok
  } catch {
    return false
  }
}

// ── JSON extraction (llama output can wrap or fence JSON) ───────
function extractJson(txt) {
  if (!txt) return null
  let s = String(txt).trim().replace(/^```json\s*/i, '').replace(/^```\s*/, '').replace(/```\s*$/, '').trim()
  try { return JSON.parse(s) } catch { /* fall through */ }
  const m = s.match(/[[{][\s\S]*[\]}]/)
  if (m) { try { return JSON.parse(m[0]) } catch { /* noop */ } }
  return null
}

function asQuestionArray(parsed) {
  if (!parsed) return []
  if (Array.isArray(parsed)) return parsed
  if (Array.isArray(parsed.questions)) return parsed.questions
  if (Array.isArray(parsed.mcqs)) return parsed.mcqs
  if (Array.isArray(parsed.items)) return parsed.items
  return []
}

const DIFFS = ['Easy', 'Medium', 'Hard']
function cleanDiff(d) { return DIFFS.includes(d) ? d : 'Medium' }

function normalizeObjective(q, ctx) {
  const options = Array.isArray(q.options) ? q.options.map(String).slice(0, 4) : []
  while (options.length < 4) options.push('—')
  let correct = Number.isInteger(q.correct) ? q.correct : 0
  if (correct < 0 || correct > 3) correct = 0
  return {
    id: nextId(),
    subject: ctx.subject,
    unit: ctx.unit ?? 1,
    topic: ctx.topic || 'AI Generated',
    stem: String(q.stem || q.question || 'Question').trim(),
    options,
    correct,
    type: q.type === 'Conceptual' ? 'Conceptual' : 'Exercise',
    difficulty: cleanDiff(q.difficulty),
    cognitiveLevel: q.cognitiveLevel || 'Understanding',
    slo: q.slo || 'AI',
    entranceExam: !!q.entranceExam,
    reason: String(q.reason || q.explanation || 'Refer to your textbook for the detailed explanation.').trim(),
    aiGenerated: true,
  }
}

function normalizeSubjective(q, ctx) {
  const type = q.type === 'Long' ? 'Long' : (q.type === 'Short' ? 'Short' : (ctx.type || 'Short'))
  return {
    id: nextId(),
    subject: ctx.subject,
    unit: ctx.unit ?? 1,
    stem: String(q.stem || q.question || 'Question').trim(),
    type,
    difficulty: cleanDiff(q.difficulty),
    cognitiveLevel: q.cognitiveLevel || 'Understanding',
    marks: Number(q.marks) || (type === 'Long' ? 5 : 2),
    modelAnswer: String(q.modelAnswer || q.answer || 'Model answer not available — refer to your textbook.').trim(),
    predicted: !!q.predicted,
    aiGenerated: true,
  }
}

function ctxLine({ board, grade, subject }) {
  const lvl = Number(grade) >= 11 ? 'HSSC (Intermediate)' : 'SSC (Matric)'
  return `You are a senior paper setter for the ${board || 'Balochistan Board (BISE)'}, Pakistan, ${lvl} Grade ${grade || 9} ${subject}.`
}

/**
 * Generate objective (MCQ) questions.
 * Returns [] on any failure so the caller can fall back to the local bank.
 */
export async function generateObjectiveQuestions({ subject, topic, unit, grade, board, count = 10, difficulty } = {}) {
  const diffLine = difficulty ? `Target ${difficulty} difficulty.` : 'Mix Easy, Medium and Hard difficulty.'
  const prompt = `${ctxLine({ board, grade, subject })} Generate ${count} multiple-choice questions on "${topic || 'the full syllabus'}". ${diffLine}
Return ONLY JSON: {"questions":[{"stem":"...","options":["opt1","opt2","opt3","opt4"],"correct":0,"difficulty":"Easy|Medium|Hard","reason":"why the answer is correct"}]}.
Rules: exactly 4 options each; "correct" is the 0-based index of the correct option; questions must be unambiguous and curriculum-appropriate.`
  try {
    const txt = await ollamaGenerate(prompt, { json: true, numPredict: Math.min(2048, 180 * count) })
    const arr = asQuestionArray(extractJson(txt))
    return arr.map((q) => normalizeObjective(q, { subject, unit, topic })).filter((q) => q.stem && q.options.length === 4)
  } catch {
    return []
  }
}

/**
 * Generate subjective (short/long) questions with model answers.
 */
export async function generateSubjectiveQuestions({ subject, topic, unit, grade, board, count = 6, type, difficulty } = {}) {
  const typeLine = type ? `All questions should be ${type} questions.` : 'Mix Short (2 marks) and Long (5 marks) questions.'
  const prompt = `${ctxLine({ board, grade, subject })} Generate ${count} subjective questions on "${topic || 'the full syllabus'}". ${typeLine}
Return ONLY JSON: {"questions":[{"stem":"...","type":"Short|Long","marks":2,"difficulty":"Easy|Medium|Hard","modelAnswer":"a concise model answer"}]}.`
  try {
    const txt = await ollamaGenerate(prompt, { json: true, numPredict: Math.min(2048, 220 * count) })
    const arr = asQuestionArray(extractJson(txt))
    return arr.map((q) => normalizeSubjective(q, { subject, unit, topic, type })).filter((q) => q.stem)
  } catch {
    return []
  }
}

/**
 * Predictive / "guess paper" — the most likely-to-come exam questions.
 * kind: 'objective' | 'subjective'
 */
export async function generatePredictedPaper({ subject, grade, board, count = 10, kind = 'objective' } = {}) {
  const intro = `${ctxLine({ board, grade, subject })} Based on past board papers and the most important, frequently-examined topics, predict the ${count} questions MOST LIKELY to appear in the upcoming annual exam.`
  if (kind === 'subjective') {
    const prompt = `${intro}
Return ONLY JSON: {"questions":[{"stem":"...","type":"Short|Long","marks":2,"difficulty":"Easy|Medium|Hard","modelAnswer":"..."}]}.`
    try {
      const arr = asQuestionArray(extractJson(await ollamaGenerate(prompt, { json: true, numPredict: Math.min(2048, 220 * count) })))
      return arr.map((q) => ({ ...normalizeSubjective(q, { subject }), predicted: true }))
    } catch { return [] }
  }
  const prompt = `${intro}
Return ONLY JSON: {"questions":[{"stem":"...","options":["opt1","opt2","opt3","opt4"],"correct":0,"difficulty":"Easy|Medium|Hard","reason":"..."}]}. Exactly 4 options; "correct" is 0-based.`
  try {
    const arr = asQuestionArray(extractJson(await ollamaGenerate(prompt, { json: true, numPredict: Math.min(2048, 180 * count) })))
    return arr.map((q) => ({ ...normalizeObjective(q, { subject }), entranceExam: true })).filter((q) => q.options.length === 4)
  } catch { return [] }
}

export const ollamaConfig = { BASE, MODEL, TIMEOUT }
