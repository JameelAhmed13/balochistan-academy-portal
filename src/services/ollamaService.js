/**
 * ollamaService.js — one dynamic entry point for Ollama /api/chat and /api/generate,
 * adapted for the browser (Vite SPA) from a Node helper pattern.
 *
 * Differences from the Node original:
 *   - Calls go through the same-origin "/ollama" Vite dev proxy (avoids CORS);
 *     override with VITE_OLLAMA_URL. Config comes from import.meta.env.
 *   - Adds education USE_CASES that emit strict JSON (test/predicted question
 *     generation) alongside prose use-cases (qa) — `json:true` toggles format.
 *
 * Low-level:   llmCall("generate" | "chat", useCase, ctx, opts) -> raw model text
 * High-level:  generateObjectiveQuestions / generateSubjectiveQuestions /
 *              generatePredictedPaper -> normalized question arrays ([] on failure,
 *              so callers fall back to the local question bank — never a dead screen).
 */

import { buildPrompt } from './studentAssistantPrompts'
import { buildRolePrompt } from './saathiRoles'

// ----------------------------------------------------------------------
// CONFIG (browser / Vite)
// ----------------------------------------------------------------------
// Default to the same-origin dev proxy "/ollama" (see vite.config.js). Set
// VITE_OLLAMA_URL only if you want to hit a host directly (CORS permitting).
const BASE = (import.meta.env.VITE_OLLAMA_URL || '/ollama').replace(/\/$/, '')
const DEFAULT_MODEL = import.meta.env.VITE_OLLAMA_MODEL || 'llama3'
const TIMEOUT_MS = Number(import.meta.env.VITE_OLLAMA_TIMEOUT_MS || 30000)
const OLLAMA_API_KEY = (import.meta.env.VITE_OLLAMA_API_KEY || '').trim()
const DEFAULT_OPTIONS = { temperature: 0.3, top_p: 0.9, num_predict: 1500 }

// Gemini is used as the cloud fallback when local Ollama is unreachable/too slow.
const GEMINI_KEY = (import.meta.env.VITE_GEMINI_API_KEY || '').trim()
const GEMINI_MODEL = 'gemini-2.5-flash-lite'

let lastError = null
let _idSeq = 700000
const nextId = () => ++_idSeq

function levelLabel(grade) {
  return Number(grade) >= 11 ? 'HSSC (Intermediate)' : 'SSC (Matric)'
}
function paperSetterRole(c) {
  return `a senior paper setter for the ${c.board || 'Balochistan Board (BISE)'}, Pakistan, ${levelLabel(c.grade)} Grade ${c.grade || 9} ${c.subject}`
}

// ----------------------------------------------------------------------
// USE-CASE DEFINITIONS  (add new ones here — nothing else changes)
//   role/task: string or (ctx) => string   |   json:true -> strict JSON via `schema`
// ----------------------------------------------------------------------
export const USE_CASES = {
  objective_questions: {
    json: true,
    role: paperSetterRole,
    task: (c) =>
      `Generate ${c.count || 10} multiple-choice questions on "${c.topic || 'the full syllabus'}".` +
      (c.difficulty ? ` Target ${c.difficulty} difficulty.` : ' Mix Easy, Medium and Hard difficulty.'),
    rules: [
      'Use only curriculum-appropriate, unambiguous questions for the stated grade and board.',
      'Exactly 4 options per question.',
      '"correct" is the 0-based index of the correct option.',
      'Do not include any commentary outside the JSON.',
    ],
    schema:
      '{"questions":[{"stem":"string","options":["opt1","opt2","opt3","opt4"],"correct":0,"difficulty":"Easy|Medium|Hard","reason":"why the answer is correct"}]}',
  },
  subjective_questions: {
    json: true,
    role: paperSetterRole,
    task: (c) =>
      `Generate ${c.count || 6} subjective questions on "${c.topic || 'the full syllabus'}".` +
      (c.type ? ` All questions must be ${c.type} questions.` : ' Mix Short (2 marks) and Long (5 marks) questions.'),
    rules: [
      'Use only curriculum-appropriate questions for the stated grade and board.',
      'Provide a concise model answer for each question.',
      'Do not include any commentary outside the JSON.',
    ],
    schema:
      '{"questions":[{"stem":"string","type":"Short|Long","marks":2,"difficulty":"Easy|Medium|Hard","modelAnswer":"concise model answer"}]}',
  },
  predicted_objective: {
    json: true,
    role: paperSetterRole,
    task: (c) =>
      `Based on past board papers and the most frequently-examined topics, predict the ${c.count || 10} multiple-choice questions MOST LIKELY to appear in the upcoming annual exam.`,
    rules: [
      'Exactly 4 options per question.',
      '"correct" is the 0-based index of the correct option.',
      'Do not include any commentary outside the JSON.',
    ],
    schema:
      '{"questions":[{"stem":"string","options":["opt1","opt2","opt3","opt4"],"correct":0,"difficulty":"Easy|Medium|Hard","reason":"string"}]}',
  },
  predicted_subjective: {
    json: true,
    role: paperSetterRole,
    task: (c) =>
      `Based on past board papers and key topics, predict the ${c.count || 8} subjective questions MOST LIKELY to appear in the upcoming annual exam.`,
    rules: [
      'Provide a concise model answer for each question.',
      'Do not include any commentary outside the JSON.',
    ],
    schema:
      '{"questions":[{"stem":"string","type":"Short|Long","marks":2,"difficulty":"Easy|Medium|Hard","modelAnswer":"string"}]}',
  },
  // Prose use-case — usable via mode "chat" with history (e.g. syllabus Q&A).
  qa: {
    json: false,
    role: (c) =>
      `a knowledgeable ${c.subject || 'subject'} teacher for Grade ${c.grade || 9} students (${c.board || 'Balochistan Board (BISE)'}, Pakistan)`,
    task: () => "Answer the student's question clearly and accurately, with a simple example where it helps.",
    rules: [
      'Use only the information in the provided context plus standard curriculum knowledge.',
      'If the question is in Urdu, answer in Urdu; otherwise answer in English.',
      "If it is outside the syllabus, say so briefly.",
    ],
    sections: [],
  },
}

// ----------------------------------------------------------------------
// INTERNAL: prompt structuring per endpoint
// ----------------------------------------------------------------------
const resolve = (v, ctx) => (typeof v === 'function' ? v(ctx) : v)

function rulesBlock(cfg) {
  const rules = [...(cfg.rules || [])]
  if (cfg.json && cfg.schema) rules.push(`Return ONLY valid JSON of exactly this shape: ${cfg.schema}`)
  return rules.map((r) => `- ${r}`).join('\n')
}
function sectionsBlock(cfg) {
  return cfg.sections?.length ? cfg.sections.map((s, i) => `${i + 1}. ${s}`).join('\n') : ''
}

function systemPrompt(cfg, ctx) {
  let out = `You are ${resolve(cfg.role, ctx)}.\n\nTASK:\n${resolve(cfg.task, ctx)}\n\nRULES:\n${rulesBlock(cfg)}`
  const sec = sectionsBlock(cfg)
  if (sec) out += `\n\nOUTPUT SECTIONS (use exactly these headings):\n${sec}`
  return out
}

function flatPrompt(cfg, ctx, question) {
  const blocks = [
    `### ROLE\nYou are ${resolve(cfg.role, ctx)}.`,
    `### TASK\n${resolve(cfg.task, ctx)}`,
    `### RULES\n${rulesBlock(cfg)}`,
  ]
  const sec = sectionsBlock(cfg)
  if (sec) blocks.push(`### OUTPUT FORMAT\n${sec}`)
  if (ctx && Object.keys(ctx).length) blocks.push(`### CONTEXT\n${JSON.stringify(ctx, null, 2)}`)
  if (question) blocks.push(`### QUESTION\n${question}`)
  blocks.push(cfg.json ? '### JSON' : '### RESPONSE')
  return blocks.join('\n\n')
}

function userMessage(ctx, question) {
  let msg = 'Context:\n\n' + JSON.stringify(ctx, null, 2)
  if (question) msg += `\n\nQuestion: ${question}`
  return msg
}

// ----------------------------------------------------------------------
// PUBLIC: the one dynamic helper
// ----------------------------------------------------------------------
/**
 * @param {"chat"|"generate"} mode
 * @param {string} useCase  key from USE_CASES
 * @param {object} ctx      parameters consumed by role/task (subject, grade, board, count, topic, …)
 * @param {object} [opts]   { question, history, model, options, numPredict }
 * @returns {Promise<string>} raw model text (JSON string for json use-cases)
 */
export async function llmCall(mode, useCase, ctx = {}, opts = {}) {
  const { question = null, history = null, model = DEFAULT_MODEL, options = {}, numPredict } = opts
  const cfg = USE_CASES[useCase]
  if (!cfg) throw new Error(`Unknown useCase '${useCase}'. Available: ${Object.keys(USE_CASES).join(', ')}`)
  const mergedOptions = { ...DEFAULT_OPTIONS, ...(numPredict ? { num_predict: numPredict } : {}), ...options }

  let endpoint, payload, extract
  if (mode === 'chat') {
    const messages = [{ role: 'system', content: systemPrompt(cfg, ctx) }]
    if (history?.length) messages.push(...history)
    messages.push({ role: 'user', content: userMessage(ctx, question) })
    endpoint = '/api/chat'
    payload = { model, messages, stream: false, options: mergedOptions, ...(cfg.json ? { format: 'json' } : {}) }
    extract = (j) => j.message?.content || ''
  } else if (mode === 'generate') {
    endpoint = '/api/generate'
    payload = { model, prompt: flatPrompt(cfg, ctx, question), stream: false, options: mergedOptions, ...(cfg.json ? { format: 'json' } : {}) }
    extract = (j) => j.response || ''
  } else {
    throw new Error(`mode must be 'chat' or 'generate', got '${mode}'`)
  }

  return extract(await _ollamaPost(endpoint, payload))
}

// Shared POST with timeout + friendly error mapping (used by llmCall and chat).
async function _ollamaPost(endpoint, payload, timeoutMs = TIMEOUT_MS) {
  const controller = new AbortController()
  const timer = setTimeout(() => controller.abort(), timeoutMs)
  try {
    const headers = { 'Content-Type': 'application/json' }
    if (OLLAMA_API_KEY) headers['Authorization'] = `Bearer ${OLLAMA_API_KEY}`
    const res = await fetch(BASE + endpoint, {
      method: 'POST',
      headers,
      body: JSON.stringify(payload),
      signal: controller.signal,
    })
    if (!res.ok) {
      const body = await res.text().catch(() => '')
      throw new Error(`Ollama HTTP ${res.status}: ${body.slice(0, 200)}`)
    }
    const json = await res.json()
    if (json.error) throw new Error(json.error)
    return json
  } catch (err) {
    if (err.name === 'AbortError') {
      throw new Error(`Ollama timed out after ${Math.round(timeoutMs / 1000)}s — the model may be too slow on this machine.`)
    }
    if (err.cause?.code === 'ECONNREFUSED' || /fetch failed|Failed to fetch|NetworkError/i.test(err.message)) {
      throw new Error(`Cannot reach Ollama at ${BASE} — is it running? (try: ollama serve)`)
    }
    throw err
  } finally {
    clearTimeout(timer)
  }
}

/**
 * Raw chat turn with a custom system prompt (used by Saathi). Returns reply text.
 */
export async function chat({ system, messages = [], model = DEFAULT_MODEL, options = {}, format, timeoutMs } = {}) {
  const msgs = system ? [{ role: 'system', content: system }, ...messages] : [...messages]
  const json = await _ollamaPost('/api/chat', {
    model,
    messages: msgs,
    stream: false,
    options: { ...DEFAULT_OPTIONS, ...options },
    ...(format ? { format } : {}),
  }, timeoutMs)
  return json.message?.content || ''
}

// Cloud fallback: same chat shape, via Google Gemini (system prompt + multi-turn).
async function geminiChat({ system, messages = [], options = {} }) {
  if (!GEMINI_KEY) throw new Error('no-gemini-key')
  const contents = messages.map((m) => ({
    role: m.role === 'assistant' ? 'model' : 'user',
    parts: [{ text: m.content }],
  }))
  const body = { contents }
  if (system) body.systemInstruction = { parts: [{ text: system }] }
  if (options.temperature != null) body.generationConfig = { temperature: options.temperature }
  const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/${GEMINI_MODEL}:generateContent?key=${GEMINI_KEY}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) {
    const errBody = await res.text().catch(() => '')
    throw new Error(`Gemini HTTP ${res.status}: ${errBody.slice(0, 300)}`)
  }
  const d = await res.json()
  return d.candidates?.[0]?.content?.parts?.[0]?.text || ''
}

/**
 * Chat with local Ollama first; on timeout/unreachable/error, fall back to Gemini.
 * Sets the engine used on the returned object via `lastEngine`.
 */
export let lastEngine = null
export async function chatWithFallback({ system, messages = [], model, options = {}, format } = {}) {
  // 1. Try Ollama (normal timeout)
  try {
    const out = await chat({ system, messages, model, options, format })
    lastEngine = 'ollama'
    return out
  } catch (ollamaErr) {
    lastError = ollamaErr?.message || String(ollamaErr)
  }

  // 2. Try Gemini
  let gemErr = null
  try {
    const out = await geminiChat({ system, messages, options })
    lastEngine = 'gemini'
    return out
  } catch (e) {
    gemErr = e
  }

  // 3. If Gemini failed with billing/quota (429), retry Ollama with 3× timeout
  const gemMsg = gemErr?.message || ''
  if (gemMsg.includes('429') || /quota|billing/i.test(gemMsg)) {
    try {
      const out = await chat({ system, messages, model, options, format, timeoutMs: TIMEOUT_MS * 3 })
      lastEngine = 'ollama'
      return out
    } catch (retryErr) {
      lastEngine = null
      throw new Error(`Both engines failed — Ollama (extended): ${retryErr?.message || retryErr}; Gemini: ${gemMsg}`)
    }
  }

  lastEngine = null
  throw new Error(`Both engines failed — Ollama: ${lastError}; Gemini: ${gemMsg}`)
}

/**
 * Saathi AI (student companion) — grade-band system prompt for a STUDENT_USE_CASES
 * key, then one chat turn (Ollama → Gemini fallback).
 * @param {string} useCase  tutor_session | study_planner | quiz_generator | exam_readiness_report | …
 * @param {string|number} grade  "ECD" or 1–12
 * @param {object} injected  prompt placeholder values (student_name, syllabus_json, …)
 * @param {string} userText  the student's message
 * @param {Array}  history   prior chat turns [{role, content}]
 * @returns {Promise<string>} Saathi's reply
 */
export async function saathiChat(useCase, grade, injected = {}, userText = '', history = [], opts = {}) {
  const system = buildPrompt(useCase, grade, injected)
  return chatWithFallback({ system, messages: [...history, { role: 'user', content: userText }], ...opts })
}

/**
 * Saathi AI (role/engine) — teacher_assistant | paper_generator | question_bank |
 * adaptive_engine | learning_path | exam_prediction | career_guidance |
 * attendance_analysis | monthly_review | weak_topic_recovery.
 * Same fallback chain. Pass the role's inputs via `injected`.
 */
export async function saathiRole(role, injected = {}, userText = '', history = [], opts = {}) {
  const system = buildRolePrompt(role, injected)
  return chatWithFallback({ system, messages: [...history, { role: 'user', content: userText }], ...opts })
}

/**
 * AI-grade subjective answers against model answers.
 * @param {Array} items [{ id, question, answer, modelAnswer, marks }]
 * @returns {Promise<Array>} [{ id, marks, feedback }] (empty on failure)
 */
export async function gradeSubjectiveAnswers(items = [], { grade, subject } = {}) {
  if (!items.length) return []
  const body = items.map((it, i) =>
    `Q${i + 1} [id:${it.id}, max ${it.marks} marks]\nQuestion: ${it.question}\nStudent answer: ${it.answer || '(left blank)'}\nModel answer: ${it.modelAnswer || '(not provided)'}`,
  ).join('\n\n')
  const system = `You are a fair, encouraging ${subject || ''} examiner for Grade ${grade || 9} (Balochistan Board). ` +
    `Grade each student answer against its model answer and award whole marks out of that question's maximum. ` +
    `A blank answer scores 0. Give one or two sentences of specific, kind feedback. ` +
    `Return ONLY JSON: {"grades":[{"id":<id>,"marks":<number>,"feedback":"<text>"}]}`
  try {
    const txt = await chatWithFallback({ system, messages: [{ role: 'user', content: body }], format: 'json' })
    const m = String(txt).match(/\{[\s\S]*\}/)
    const j = m ? JSON.parse(m[0]) : {}
    return Array.isArray(j.grades) ? j.grades : []
  } catch {
    return []
  }
}

// Lightweight reachability probe.
export async function isOllamaAvailable(timeout = 3000) {
  try {
    const controller = new AbortController()
    const timer = setTimeout(() => controller.abort(), timeout)
    const res = await fetch(`${BASE}/api/tags`, { signal: controller.signal })
    clearTimeout(timer)
    return res.ok
  } catch {
    return false
  }
}

export function getLastOllamaError() {
  return lastError
}

// Which engine served the most recent chatWithFallback() call: 'ollama' | 'gemini' | null
export function getLastEngine() {
  return lastEngine
}

// ----------------------------------------------------------------------
// INTERNAL: JSON extraction + normalization to the content-store shapes
// ----------------------------------------------------------------------
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
  for (const k of ['questions', 'mcqs', 'items']) if (Array.isArray(parsed[k])) return parsed[k]
  return []
}
const DIFFS = ['Easy', 'Medium', 'Hard']
const cleanDiff = (d) => (DIFFS.includes(d) ? d : 'Medium')

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

// ----------------------------------------------------------------------
// PUBLIC: high-level generators (return [] on failure → caller falls back)
// ----------------------------------------------------------------------
// One-shot JSON generation via Gemini (used as a fast fallback to local Ollama).
async function geminiGenerate(prompt, json = true) {
  if (!GEMINI_KEY) throw new Error('no-gemini-key')
  const body = { contents: [{ role: 'user', parts: [{ text: prompt }] }] }
  if (json) body.generationConfig = { responseMimeType: 'application/json' }
  const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/${GEMINI_MODEL}:generateContent?key=${GEMINI_KEY}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) {
    const errBody = await res.text().catch(() => '')
    throw new Error(`Gemini HTTP ${res.status}: ${errBody.slice(0, 300)}`)
  }
  const d = await res.json()
  return d.candidates?.[0]?.content?.parts?.[0]?.text || ''
}

// Generate JSON: try local Ollama, fall back to Gemini so generation completes
// even when Ollama is slow/unreachable. Same prompt (incl. real-data grounding) either way.
async function generateJSON(useCase, ctx, opts = {}) {
  const cfg = USE_CASES[useCase]
  try {
    const txt = await llmCall('generate', useCase, ctx, opts)
    if (txt && extractJson(txt)) { lastEngine = 'ollama'; return txt }
  } catch (e) { lastError = e?.message || String(e) }
  const txt = await geminiGenerate(flatPrompt(cfg, ctx), cfg.json)
  lastEngine = 'gemini'
  return txt
}

export async function generateObjectiveQuestions({ subject, topic, unit, grade, board, count = 10, difficulty, examples } = {}) {
  try {
    lastError = null
    const ctx = { subject, grade, board, topic: topic || 'the full syllabus', count, difficulty }
    if (examples) ctx.realExamplesToMatch = examples
    const txt = await generateJSON('objective_questions', ctx,
      { numPredict: Math.min(2048, 180 * count) })
    return asQuestionArray(extractJson(txt))
      .map((q) => normalizeObjective(q, { subject, unit, topic }))
      .filter((q) => q.stem && q.options.length === 4)
  } catch (e) { lastError = e?.message || String(e); return [] }
}

export async function generateSubjectiveQuestions({ subject, topic, unit, grade, board, count = 6, type, difficulty } = {}) {
  try {
    lastError = null
    const txt = await generateJSON('subjective_questions',
      { subject, grade, board, topic: topic || 'the full syllabus', count, type, difficulty },
      { numPredict: Math.min(2048, 220 * count) })
    return asQuestionArray(extractJson(txt))
      .map((q) => normalizeSubjective(q, { subject, unit, topic, type }))
      .filter((q) => q.stem)
  } catch (e) { lastError = e?.message || String(e); return [] }
}

/** Predictive / "guess paper". kind: 'objective' | 'subjective' */
export async function generatePredictedPaper({ subject, grade, board, count = 10, kind = 'objective' } = {}) {
  try {
    lastError = null
    if (kind === 'subjective') {
      const txt = await generateJSON('predicted_subjective', { subject, grade, board, count }, { numPredict: Math.min(2048, 220 * count) })
      return asQuestionArray(extractJson(txt)).map((q) => ({ ...normalizeSubjective(q, { subject }), predicted: true }))
    }
    const txt = await generateJSON('predicted_objective', { subject, grade, board, count }, { numPredict: Math.min(2048, 180 * count) })
    return asQuestionArray(extractJson(txt))
      .map((q) => ({ ...normalizeObjective(q, { subject }), entranceExam: true }))
      .filter((q) => q.options.length === 4)
  } catch (e) { lastError = e?.message || String(e); return [] }
}

export const ollamaConfig = { BASE, MODEL: DEFAULT_MODEL, TIMEOUT_MS, GEMINI_MODEL }
