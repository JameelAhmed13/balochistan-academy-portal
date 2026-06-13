import { Router } from 'express'
import bcrypt from 'bcryptjs'
import { z } from 'zod'
import db from '../db/connection.js'
import { parse } from '../util/validate.js'
import { signToken, requireAuth } from '../middleware/auth.js'

const router = Router()

const publicUser = (u) => ({
  id: u.id, username: u.username, role: u.role, name: u.name, phone: u.phone, email: u.email,
  grade: u.grade_code, gradeCode: u.grade_code, medium: u.medium, institute: u.institute,
  board: u.board, coins: u.coins, avatar: null,
})

const loginSchema = z.object({
  username: z.string().min(1),
  password: z.string().min(1),
})

router.post('/login', (req, res) => {
  const { username, password } = parse(loginSchema, req.body)
  const u = db.prepare('SELECT * FROM users WHERE username = ? AND active = 1').get(username.trim())
  if (!u || !bcrypt.compareSync(password, u.password_hash)) {
    return res.status(401).json({ error: 'Invalid username or password' })
  }
  db.prepare("UPDATE users SET last_login_at = datetime('now') WHERE id = ?").run(u.id)
  db.prepare('INSERT INTO login_history (user_id, status) VALUES (?, ?)').run(u.id, 'ok')
  res.json({ token: signToken(u), user: publicUser(u) })
})

const registerSchema = z.object({
  name: z.string().min(1),
  username: z.string().min(3).max(40),
  password: z.string().min(4),
  gradeCode: z.string().min(1),
  phone: z.string().optional(),
  email: z.string().optional(),
  medium: z.string().optional(),
  institute: z.string().optional(),
})

router.post('/register', (req, res) => {
  const b = parse(registerSchema, req.body)
  const grade = db.prepare('SELECT code FROM grades WHERE code = ? AND enabled = 1').get(b.gradeCode)
  if (!grade) return res.status(400).json({ error: 'Unknown or disabled grade' })
  if (db.prepare('SELECT id FROM users WHERE username = ?').get(b.username.trim())) {
    return res.status(409).json({ error: 'Username already taken' })
  }
  const info = db
    .prepare(
      `INSERT INTO users (username,password_hash,role,name,phone,email,grade_code,medium,institute,coins)
       VALUES (?,?,'student',?,?,?,?,?,?,0)`,
    )
    .run(b.username.trim(), bcrypt.hashSync(b.password, 10), b.name, b.phone ?? null,
      b.email ?? null, b.gradeCode, b.medium ?? 'English', b.institute ?? null)
  const u = db.prepare('SELECT * FROM users WHERE id = ?').get(info.lastInsertRowid)
  res.status(201).json({ token: signToken(u), user: publicUser(u) })
})

router.get('/me', requireAuth, (req, res) => {
  const u = db.prepare('SELECT * FROM users WHERE id = ? AND active = 1').get(req.user.id)
  if (!u) return res.status(401).json({ error: 'User not found' })
  res.json({ user: publicUser(u) })
})

// student sets/changes their own grade (used by the SelectGrade gate)
router.put('/me/grade', requireAuth, (req, res) => {
  const { gradeCode } = parse(z.object({ gradeCode: z.string().min(1) }), req.body)
  const grade = db.prepare('SELECT code FROM grades WHERE code = ? AND enabled = 1').get(gradeCode)
  if (!grade) return res.status(400).json({ error: 'Unknown or disabled grade' })
  db.prepare("UPDATE users SET grade_code = ? WHERE id = ? AND role = 'student'").run(gradeCode, req.user.id)
  const u = db.prepare('SELECT * FROM users WHERE id = ?').get(req.user.id)
  res.json({ user: publicUser(u) })
})

router.post('/logout', requireAuth, (req, res) => {
  db.prepare('INSERT INTO login_history (user_id, status) VALUES (?, ?)').run(req.user.id, 'logout')
  res.json({ ok: true })
})

export default router
export { publicUser }
