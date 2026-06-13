import { Router } from 'express'
import db from '../db/connection.js'
import { requireAuth } from '../middleware/auth.js'

const router = Router()

// GET /api/ai/context?grade=&subject=&unit=&sample= — DB-sourced RAG grounding
// Mirrors the old assessmentBank.realContextSnippet(), now from the DB so Ollama
// is grounded on the real syllabus + sample questions for that grade/subject.
router.get('/context', requireAuth, (req, res) => {
  const { grade, subject } = req.query
  const sample = Math.min(Number(req.query.sample) || 6, 20)
  const sid = subject ? db.prepare('SELECT id FROM subjects WHERE name = ?').get(subject)?.id : null

  // syllabus units + objectives
  let syllabus = []
  if (grade && sid) {
    const units = db.prepare('SELECT id,name,description FROM units WHERE grade_code=? AND subject_id=? ORDER BY sort_order,number').all(grade, sid)
    const objs = db.prepare('SELECT text FROM learning_objectives WHERE unit_id=? LIMIT 12')
    syllabus = units.map((u) => ({ unit: u.name, description: u.description, objectives: objs.all(u.id).map((o) => o.text) }))
  }

  // sample questions
  let qSql = "SELECT stem, options_json, correct_index, topic_text FROM questions WHERE kind='objective'"
  const args = []
  if (grade) { qSql += ' AND grade_code=?'; args.push(grade) }
  if (sid) { qSql += ' AND subject_id=?'; args.push(sid) }
  qSql += ' ORDER BY RANDOM() LIMIT ?'
  args.push(sample)
  const sampleQuestions = db.prepare(qSql).all(...args).map((q) => ({
    stem: q.stem,
    options: q.options_json ? JSON.parse(q.options_json) : [],
    correct: q.correct_index,
    topic: q.topic_text,
  }))

  // flat text snippet the browser can inject into an Ollama prompt
  const lines = []
  if (syllabus.length) {
    lines.push(`Syllabus for Grade ${grade} ${subject}:`)
    syllabus.forEach((u) => {
      lines.push(`- ${u.unit}${u.description ? ': ' + u.description : ''}`)
      u.objectives.slice(0, 5).forEach((o) => lines.push(`    • ${o}`))
    })
  }
  if (sampleQuestions.length) {
    lines.push('', 'Representative real exam questions (style/difficulty reference):')
    sampleQuestions.forEach((q, i) => lines.push(`${i + 1}. ${q.stem}`))
  }

  res.json({ grade, subject, syllabus, sampleQuestions, contextText: lines.join('\n') })
})

export default router
