import { Router } from 'express'
import { z } from 'zod'
import db from '../db/connection.js'
import { parse } from '../util/validate.js'
import { requireAuth } from '../middleware/auth.js'
import { buildSchedule, daysBetween } from '../services/schedule.engine.js'

const router = Router()
router.use(requireAuth)

const today = () => new Date().toISOString().slice(0, 10)

function gradeOf(userId) {
  return db.prepare('SELECT grade_code FROM users WHERE id = ?').get(userId)?.grade_code || null
}
function unitsFor(grade, subjectId) {
  return db
    .prepare('SELECT id, name, number, sort_order, difficulty, marks_weightage FROM units WHERE grade_code = ? AND subject_id = ? ORDER BY sort_order, number')
    .all(grade, subjectId)
}
// units table has no difficulty/marks columns in base schema — tolerate their absence
function unitsForSafe(grade, subjectId) {
  try { return unitsFor(grade, subjectId) }
  catch { return db.prepare('SELECT id, name, number, sort_order FROM units WHERE grade_code = ? AND subject_id = ? ORDER BY sort_order, number').all(grade, subjectId) }
}

function hydrate(schedule) {
  const days = db.prepare('SELECT * FROM schedule_days WHERE schedule_id = ? ORDER BY day_date, sort_order').all(schedule.id)
  const subject = db.prepare('SELECT id, name, name_ur, color, icon FROM subjects WHERE id = ?').get(schedule.subject_id)
  const unitCount = new Set(days.map((d) => d.unit_id)).size
  const doneUnits = new Set(days.filter((d) => d.status === 'done').map((d) => d.unit_id)).size
  const done = days.filter((d) => d.status === 'done').length
  const missed = days.filter((d) => d.status === 'missed').length
  const past = days.filter((d) => d.day_date < today() && d.status === 'pending').length
  const compliance = days.length ? Math.round((done / days.length) * 100) : 0
  const coverage = unitCount ? Math.round((doneUnits / unitCount) * 100) : 0
  const daysToExam = Math.max(0, daysBetween(today(), schedule.exam_date))
  return {
    ...schedule,
    subject,
    days,
    stats: {
      totalDays: days.length, done, missed, pendingPast: past,
      compliance, coverage, daysToExam,
      readiness: Math.round(coverage * 0.6 + compliance * 0.4),
    },
  }
}

// GET /api/schedule — all of the student's plans
router.get('/', (req, res) => {
  const rows = db.prepare('SELECT * FROM student_schedules WHERE user_id = ? ORDER BY exam_date').all(req.user.id)
  res.json(rows.map(hydrate))
})

const genSchema = z.object({
  subjectId: z.coerce.number().int(),
  examDate: z.string().min(8),
  dailyHours: z.coerce.number().min(0.5).max(12).optional(),
})

// POST /api/schedule/generate — (re)build a plan for one subject
router.post('/generate', (req, res) => {
  const b = parse(genSchema, req.body)
  const grade = gradeOf(req.user.id)
  if (!grade) return res.status(400).json({ error: 'Select your grade first' })
  const units = unitsForSafe(grade, b.subjectId)
  if (!units.length) return res.status(400).json({ error: 'No syllabus units found for this subject' })

  const { days, meta } = buildSchedule({
    units, startDate: today(), examDate: b.examDate, dailyHours: b.dailyHours ?? 2, weakUnitIds: [],
  })
  if (!days.length) return res.status(400).json({ error: meta.examPassed ? 'Exam date is in the past' : 'Could not build a plan' })

  const tx = db.transaction(() => {
    const existing = db.prepare('SELECT id FROM student_schedules WHERE user_id = ? AND subject_id = ?').get(req.user.id, b.subjectId)
    let scheduleId
    if (existing) {
      scheduleId = existing.id
      db.prepare('UPDATE student_schedules SET exam_date=?, daily_hours=?, grade_code=?, updated_at=datetime(\'now\') WHERE id=?')
        .run(b.examDate, b.dailyHours ?? 2, grade, scheduleId)
      db.prepare('DELETE FROM schedule_days WHERE schedule_id = ?').run(scheduleId)
    } else {
      scheduleId = db.prepare('INSERT INTO student_schedules (user_id, subject_id, grade_code, exam_date, daily_hours) VALUES (?,?,?,?,?)')
        .run(req.user.id, b.subjectId, grade, b.examDate, b.dailyHours ?? 2).lastInsertRowid
    }
    const ins = db.prepare(`INSERT INTO schedule_days (schedule_id, day_date, unit_id, unit_title, task, mode, est_minutes, self_test, status, sort_order)
      VALUES (@schedule_id,@day_date,@unit_id,@unit_title,@task,@mode,@est_minutes,@self_test,@status,@sort_order)`)
    for (const d of days) ins.run({ ...d, schedule_id: scheduleId })
    return scheduleId
  })
  const id = tx()
  const schedule = db.prepare('SELECT * FROM student_schedules WHERE id = ?').get(id)
  res.status(201).json({ schedule: hydrate(schedule), meta })
})

// PUT /api/schedule/day/:id — mark a day done / needs_review / missed
const daySchema = z.object({ status: z.enum(['pending', 'done', 'missed', 'needs_review']) })
router.put('/day/:id', (req, res) => {
  const { status } = parse(daySchema, req.body)
  const ok = db.prepare(`SELECT d.id FROM schedule_days d JOIN student_schedules s ON s.id = d.schedule_id
    WHERE d.id = ? AND s.user_id = ?`).get(req.params.id, req.user.id)
  if (!ok) return res.status(404).json({ error: 'Day not found' })
  db.prepare('UPDATE schedule_days SET status = ? WHERE id = ?').run(status, req.params.id)
  res.json({ ok: true })
})

// PUT /api/schedule/day/:id/move — drag a day entry to a new date
router.put('/day/:id/move', (req, res) => {
  const { toDate } = parse(z.object({ toDate: z.string().min(8) }), req.body)
  const row = db.prepare(`SELECT d.id, d.schedule_id FROM schedule_days d JOIN student_schedules s ON s.id = d.schedule_id
    WHERE d.id = ? AND s.user_id = ?`).get(req.params.id, req.user.id)
  if (!row) return res.status(404).json({ error: 'Day not found' })
  db.prepare('UPDATE schedule_days SET day_date = ? WHERE id = ?').run(toDate.slice(0, 10), req.params.id)
  res.json({ ok: true })
})

// POST /api/schedule/:subjectId/recompute — adaptive re-plan of the remaining days (spec §9)
router.post('/:subjectId/recompute', (req, res) => {
  const sched = db.prepare('SELECT * FROM student_schedules WHERE user_id = ? AND subject_id = ?').get(req.user.id, req.params.subjectId)
  if (!sched) return res.status(404).json({ error: 'No plan to recompute' })
  const grade = gradeOf(req.user.id)
  const units = unitsForSafe(grade, sched.subject_id)

  // Weak units = anything marked needs_review or missed → boosted & pulled earlier.
  const weakRows = db.prepare("SELECT DISTINCT unit_id FROM schedule_days WHERE schedule_id = ? AND status IN ('needs_review','missed') AND unit_id IS NOT NULL").all(sched.id)
  const doneRows = db.prepare("SELECT DISTINCT unit_id FROM schedule_days WHERE schedule_id = ? AND status = 'done' AND unit_id IS NOT NULL").all(sched.id)
  const doneUnitIds = new Set(doneRows.map((r) => r.unit_id))
  const weakUnitIds = weakRows.map((r) => r.unit_id)

  // Re-plan only units not yet completed (keep past history untouched).
  const remainingUnits = units.filter((u) => !doneUnitIds.has(u.id))
  const planUnits = remainingUnits.length ? remainingUnits : units

  // Re-plan from tomorrow so today's work and all completed history are preserved.
  const tomorrow = new Date(Date.now() + 86400000).toISOString().slice(0, 10)
  const { days, meta } = buildSchedule({
    units: planUnits, startDate: tomorrow, examDate: sched.exam_date, dailyHours: sched.daily_hours, weakUnitIds,
  })

  const tx = db.transaction(() => {
    // Drop only future *pending* rows; keep done/needs_review/missed history (incl. today).
    db.prepare("DELETE FROM schedule_days WHERE schedule_id = ? AND day_date > ? AND status = 'pending'").run(sched.id, today())
    const ins = db.prepare(`INSERT INTO schedule_days (schedule_id, day_date, unit_id, unit_title, task, mode, est_minutes, self_test, status, sort_order)
      VALUES (@schedule_id,@day_date,@unit_id,@unit_title,@task,@mode,@est_minutes,@self_test,@status,@sort_order)`)
    for (const d of days) ins.run({ ...d, schedule_id: sched.id })
    // Overdue pending days (before today) become 'missed' (spec §9 missed-day handling).
    db.prepare("UPDATE schedule_days SET status='missed' WHERE schedule_id=? AND day_date < ? AND status='pending'").run(sched.id, today())
    db.prepare("UPDATE student_schedules SET updated_at=datetime('now') WHERE id=?").run(sched.id)
  })
  tx()
  res.json({ schedule: hydrate(db.prepare('SELECT * FROM student_schedules WHERE id=?').get(sched.id)), meta })
})

// DELETE /api/schedule/:subjectId
router.delete('/:subjectId', (req, res) => {
  db.prepare('DELETE FROM student_schedules WHERE user_id = ? AND subject_id = ?').run(req.user.id, req.params.subjectId)
  res.json({ ok: true })
})

export default router
