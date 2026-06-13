import { Router } from 'express'
import { z } from 'zod'
import db from '../db/connection.js'
import { parse } from '../util/validate.js'
import { requireAuth } from '../middleware/auth.js'

const router = Router()

// GET /api/practice?grade=&subject=&unit=&topic=&limit= — pull real bank questions
router.get('/practice', requireAuth, (req, res) => {
  const grade = req.query.grade
  const subjectName = req.query.subject
  const unit = req.query.unit
  const limit = Math.min(Number(req.query.limit) || 20, 100)
  const sid = subjectName ? db.prepare('SELECT id FROM subjects WHERE name = ?').get(subjectName)?.id : null

  let sql = "SELECT * FROM questions WHERE kind = 'objective'"
  const args = []
  if (grade) { sql += ' AND grade_code = ?'; args.push(grade) }
  if (sid) { sql += ' AND subject_id = ?'; args.push(sid) }
  if (unit) { sql += ' AND (topic_text = ? OR unit_id IN (SELECT id FROM units WHERE name = ?))'; args.push(unit, unit) }
  sql += ' ORDER BY RANDOM() LIMIT ?'
  args.push(limit)

  const rows = db.prepare(sql).all(...args)
  res.json(rows.map(toClientQuestion))
})

function toClientQuestion(q, { withAnswer = true } = {}) {
  const base = {
    id: q.id, subject: q.subject_id, unit: q.topic_text || '', topic: q.topic_text || '',
    stem: q.stem, kind: q.kind,
    options: q.options_json ? JSON.parse(q.options_json) : [],
    type: q.q_type || 'Exercise', difficulty: q.difficulty || 'Medium',
    marks: q.marks || null, real: true,
  }
  if (withAnswer) {
    base.correct = q.correct_index
    base.modelAnswer = q.model_answer || ''
    base.reason = q.feedback || ''
  }
  return base
}

// GET /api/grades/:code/tests — published tests for a grade
router.get('/grades/:code/tests', requireAuth, (req, res) => {
  const tests = db
    .prepare(
      `SELECT t.*, s.name AS subject_name,
              (SELECT COUNT(*) FROM test_questions tq WHERE tq.test_id = t.id) AS question_count
       FROM tests t LEFT JOIN subjects s ON s.id = t.subject_id
       WHERE t.grade_code = ? AND t.is_published = 1 ORDER BY t.created_at DESC`,
    )
    .all(req.params.code)
  res.json(tests)
})

// GET /api/tests/:id — test with questions (answers stripped)
router.get('/tests/:id', requireAuth, (req, res) => {
  const test = db.prepare('SELECT * FROM tests WHERE id = ?').get(req.params.id)
  if (!test) return res.status(404).json({ error: 'Test not found' })
  const qs = db
    .prepare(
      `SELECT q.* FROM questions q JOIN test_questions tq ON tq.question_id = q.id
       WHERE tq.test_id = ? ORDER BY tq.sort_order`,
    )
    .all(test.id)
  res.json({ ...test, questions: qs.map((q) => toClientQuestion(q, { withAnswer: false })) })
})

// POST /api/attempts — submit + grade an attempt, award coins
const attemptSchema = z.object({
  testId: z.number().nullable().optional(),
  grade: z.string().optional(),
  subject: z.string().optional(),
  durationSec: z.number().optional(),
  answers: z.array(z.object({ questionId: z.number(), chosen: z.number().nullable() })),
})

router.post('/attempts', requireAuth, (req, res) => {
  const b = parse(attemptSchema, req.body)
  const getQ = db.prepare('SELECT correct_index FROM questions WHERE id = ?')
  let score = 0
  const graded = b.answers.map((a) => {
    const q = getQ.get(a.questionId)
    const correct = q?.correct_index ?? -1
    const ok = a.chosen != null && a.chosen === correct
    if (ok) score++
    return { questionId: a.questionId, chosen: a.chosen, correct, ok }
  })
  const total = b.answers.length
  const percent = total ? Math.round((score / total) * 1000) / 10 : 0
  const sid = b.subject ? db.prepare('SELECT id FROM subjects WHERE name = ?').get(b.subject)?.id : null
  const coins = score * 5

  const info = db
    .prepare(
      `INSERT INTO test_attempts (user_id,test_id,grade_code,subject_id,score,total,percent,duration_sec,answers_json,submitted_at)
       VALUES (?,?,?,?,?,?,?,?,?,datetime('now'))`,
    )
    .run(req.user.id, b.testId ?? null, b.grade ?? null, sid ?? null, score, total, percent,
      b.durationSec ?? null, JSON.stringify(graded))
  if (coins) {
    db.prepare('UPDATE users SET coins = coins + ? WHERE id = ?').run(coins, req.user.id)
    db.prepare('INSERT INTO coin_ledger (user_id,amount,reason) VALUES (?,?,?)').run(req.user.id, coins, 'test')
  }
  res.status(201).json({ id: info.lastInsertRowid, score, total, percent, coinsAwarded: coins, graded })
})

// GET /api/attempts/me
router.get('/attempts/me', requireAuth, (req, res) => {
  res.json(
    db.prepare('SELECT * FROM test_attempts WHERE user_id = ? ORDER BY submitted_at DESC LIMIT 200').all(req.user.id),
  )
})

// GET /api/me/stats
router.get('/me/stats', requireAuth, (req, res) => {
  const s = db
    .prepare(
      `SELECT COUNT(*) attempts, COALESCE(ROUND(AVG(percent),1),0) avg_percent,
              COALESCE(SUM(CASE WHEN percent >= 40 THEN 1 ELSE 0 END),0) passed
       FROM test_attempts WHERE user_id = ?`,
    )
    .get(req.user.id)
  const coins = db.prepare('SELECT coins FROM users WHERE id = ?').get(req.user.id)?.coins ?? 0
  res.json({ ...s, coins })
})

// GET /api/leaderboard?grade=&scope= — peers in the same grade
router.get('/leaderboard', requireAuth, (req, res) => {
  const grade = req.query.grade
  let sql = 'SELECT user_id, name, grade_code, coins, attempts, avg_percent FROM leaderboard'
  const args = []
  if (grade) { sql += ' WHERE grade_code = ?'; args.push(grade) }
  sql += ' ORDER BY avg_percent DESC, coins DESC LIMIT 100'
  const rows = db.prepare(sql).all(...args).map((r, i) => ({ ...r, rank: i + 1 }))
  res.json(rows)
})

export default router
