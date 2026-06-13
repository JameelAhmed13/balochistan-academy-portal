import { Router } from 'express'
import db from '../db/connection.js'

const router = Router()

// GET /api/grades — all enabled grades with subject counts
router.get('/grades', (_req, res) => {
  const grades = db.prepare('SELECT code, label, band, sort_order FROM grades WHERE enabled = 1 ORDER BY sort_order').all()
  const counts = db.prepare('SELECT grade_code, COUNT(*) c FROM grade_subjects GROUP BY grade_code').all()
  const cmap = Object.fromEntries(counts.map((r) => [r.grade_code, r.c]))
  res.json(grades.map((g) => ({ ...g, subjectCount: cmap[g.code] || 0 })))
})

// GET /api/grades/:code/subjects
router.get('/grades/:code/subjects', (req, res) => {
  const subs = db
    .prepare(
      `SELECT s.* FROM subjects s
       JOIN grade_subjects gs ON gs.subject_id = s.id
       WHERE gs.grade_code = ? ORDER BY s.id`,
    )
    .all(req.params.code)
  res.json(subs)
})

// GET /api/grades/:code/subjects/:subjectId/syllabus — units → topics → objectives
router.get('/grades/:code/subjects/:subjectId/syllabus', (req, res) => {
  const { code, subjectId } = req.params
  const units = db
    .prepare('SELECT * FROM units WHERE grade_code = ? AND subject_id = ? ORDER BY sort_order, number')
    .all(code, subjectId)
  const topics = db.prepare('SELECT * FROM topics WHERE unit_id = ? ORDER BY sort_order')
  const objs = db.prepare('SELECT id, topic_id, code, text, cognitive_level FROM learning_objectives WHERE unit_id = ?')
  res.json(
    units.map((u) => {
      const allObjs = objs.all(u.id)
      const ts = topics.all(u.id).map((t) => ({
        ...t,
        objectives: allObjs.filter((o) => o.topic_id === t.id),
      }))
      return { ...u, topics: ts, objectives: allObjs.filter((o) => o.topic_id == null) }
    }),
  )
})

// GET /api/tutors?grade=&subject=
router.get('/tutors', (req, res) => {
  const { grade, subject } = req.query
  let sql = `SELECT t.*, s.name AS subject_name FROM ai_tutors t
             LEFT JOIN subjects s ON s.id = t.subject_id WHERE t.active = 1`
  const args = []
  if (grade) {
    // tutors whose subject is taught in that grade (or grade-agnostic)
    sql += ` AND (t.grade_code IS NULL OR t.grade_code = ?)
             AND (t.subject_id IS NULL OR t.subject_id IN
                  (SELECT subject_id FROM grade_subjects WHERE grade_code = ?))`
    args.push(grade, grade)
  }
  if (subject) {
    sql += ' AND s.name = ?'
    args.push(subject)
  }
  res.json(db.prepare(sql).all(...args))
})

export default router
