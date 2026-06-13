import jwt from 'jsonwebtoken'
import 'dotenv/config'

const SECRET = process.env.JWT_SECRET || 'dev-insecure-secret'
export const JWT_EXPIRES = process.env.JWT_EXPIRES || '7d'

export function signToken(user) {
  return jwt.sign({ id: user.id, username: user.username, role: user.role }, SECRET, { expiresIn: JWT_EXPIRES })
}

export function requireAuth(req, res, next) {
  const header = req.headers.authorization || ''
  const token = header.startsWith('Bearer ') ? header.slice(7) : null
  if (!token) return res.status(401).json({ error: 'Missing token' })
  try {
    req.user = jwt.verify(token, SECRET)
    next()
  } catch {
    res.status(401).json({ error: 'Invalid or expired token' })
  }
}

export function requireAdmin(req, res, next) {
  requireAuth(req, res, () => {
    if (req.user?.role !== 'admin') return res.status(403).json({ error: 'Admin only' })
    next()
  })
}
