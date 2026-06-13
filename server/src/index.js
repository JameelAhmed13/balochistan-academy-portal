import express from 'express'
import cors from 'cors'
import 'dotenv/config'

import authRoutes from './routes/auth.routes.js'
import catalogRoutes from './routes/catalog.routes.js'
import studentRoutes from './routes/student.routes.js'
import adminRoutes from './routes/admin.routes.js'
import aiRoutes from './routes/ai.routes.js'

const app = express()
const PORT = process.env.PORT || 3000

app.use(cors()) // harmless in dev (Vite proxies same-origin); needed if API is hosted cross-origin
app.use(express.json({ limit: '2mb' }))

app.get('/api/health', (_req, res) => res.json({ ok: true, service: 'estudy-server', ts: new Date().toISOString() }))

app.use('/api/auth', authRoutes)
app.use('/api', catalogRoutes) // /api/grades, /api/tutors (public reads)
app.use('/api', studentRoutes) // /api/practice, /api/attempts, /api/leaderboard ...
app.use('/api/admin', adminRoutes)
app.use('/api/ai', aiRoutes)

// 404 + error handlers
app.use('/api', (_req, res) => res.status(404).json({ error: 'Not found' }))
app.use((err, _req, res, _next) => {
  if (err?.type === 'validation') return res.status(400).json({ error: err.message, issues: err.issues })
  console.error('[server] error:', err)
  res.status(err.status || 500).json({ error: err.message || 'Internal server error' })
})

app.listen(PORT, () => console.log(`[estudy-server] listening on http://localhost:${PORT}`))
