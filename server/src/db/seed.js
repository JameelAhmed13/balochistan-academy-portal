import { readFileSync, existsSync, readFileSync as rf } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { dirname, resolve, isAbsolute } from 'node:path'
import bcrypt from 'bcryptjs'
import 'dotenv/config'
import db from './connection.js'
import { GRADES, SUBJECTS, GRADE_SUBJECTS, TUTORS } from './catalog.seed.js'

const __dirname = dirname(fileURLToPath(import.meta.url))
const serverRoot = resolve(__dirname, '..', '..')

// Ensure schema exists (safe to run before migrate)
db.exec(readFileSync(resolve(__dirname, 'schema.sql'), 'utf-8'))

const norm = (s) => (s == null ? '' : String(s)).trim().toLowerCase().replace(/\s+/g, ' ')

// ── 1. grades ────────────────────────────────────────────────────────
const upGrade = db.prepare(
  `INSERT INTO grades (code,label,band,sort_order,enabled) VALUES (@code,@label,@band,@sort_order,1)
   ON CONFLICT(code) DO UPDATE SET label=@label, band=@band, sort_order=@sort_order`,
)
db.transaction(() => GRADES.forEach((g) => upGrade.run(g)))()

// ── 2. subjects ──────────────────────────────────────────────────────
const upSubject = db.prepare(
  `INSERT INTO subjects (name,name_ur,icon,color,medium) VALUES (@name,@name_ur,@icon,@color,@medium)
   ON CONFLICT(name) DO UPDATE SET name_ur=@name_ur, icon=@icon, color=@color, medium=@medium`,
)
db.transaction(() => SUBJECTS.forEach((s) => upSubject.run(s)))()
const subjectId = (name) => db.prepare('SELECT id FROM subjects WHERE name=?').get(name)?.id

// ── 3. grade_subjects ────────────────────────────────────────────────
const upGS = db.prepare('INSERT OR IGNORE INTO grade_subjects (grade_code,subject_id) VALUES (?,?)')
db.transaction(() => {
  for (const [code, names] of Object.entries(GRADE_SUBJECTS)) {
    for (const n of names) {
      const sid = subjectId(n)
      if (sid) upGS.run(code, sid)
    }
  }
})()

// ── 4. ai_tutors ─────────────────────────────────────────────────────
const upTutor = db.prepare(
  `INSERT INTO ai_tutors (subject_id,grade_code,persona,slug,icon,color,description,system_prompt,active)
   VALUES (@subject_id,NULL,@persona,@slug,@icon,@color,@description,@system_prompt,1)
   ON CONFLICT(slug) DO UPDATE SET subject_id=@subject_id, persona=@persona, icon=@icon,
     color=@color, description=@description, system_prompt=@system_prompt`,
)
db.transaction(() => TUTORS.forEach((t) => upTutor.run({ ...t, subject_id: subjectId(t.subject) ?? null })))()

// ── 5. initial admin ─────────────────────────────────────────────────
const adminUser = process.env.ADMIN_USERNAME || 'admin'
const adminPass = process.env.ADMIN_PASSWORD || 'admin123'
const exists = db.prepare('SELECT id FROM users WHERE username=?').get(adminUser)
if (!exists) {
  db.prepare(
    `INSERT INTO users (username,password_hash,role,name) VALUES (?,?,'admin','Administrator')`,
  ).run(adminUser, bcrypt.hashSync(adminPass, 10))
  console.log(`[seed] created admin '${adminUser}' (password from ADMIN_PASSWORD)`)
} else {
  console.log(`[seed] admin '${adminUser}' already exists`)
}

// ── 6. content from the Python ingester (questions.json + syllabus.json) ─
const ingestDir = isAbsolute(process.env.INGEST_DIR || '')
  ? process.env.INGEST_DIR
  : resolve(serverRoot, process.env.INGEST_DIR || '../scripts/ingest/out')

function loadSyllabus() {
  const f = resolve(ingestDir, 'syllabus.json')
  if (!existsSync(f)) return 0
  const data = JSON.parse(rf(f, 'utf-8'))
  const insUnit = db.prepare(
    `INSERT INTO units (grade_code,subject_id,name,number,sort_order,description,source)
     VALUES (?,?,?,?,?,?,?)`,
  )
  const insTopic = db.prepare('INSERT INTO topics (unit_id,name,sort_order) VALUES (?,?,?)')
  const insLO = db.prepare(
    'INSERT INTO learning_objectives (unit_id,topic_id,code,text,cognitive_level,source,ai_structured) VALUES (?,?,?,?,?,?,?)',
  )
  let units = 0
  db.transaction(() => {
    // wipe prior syllabus so re-seed is clean
    db.exec('DELETE FROM learning_objectives; DELETE FROM topics; DELETE FROM units;')
    for (const row of data) {
      const sid = subjectId(row.subject)
      if (!sid) continue
      const u = insUnit.run(row.grade_code ?? row.grade, sid, row.name, row.number ?? null,
        row.sort_order ?? units, row.description ?? '', row.source ?? 'pdf')
      const unitId = u.lastInsertRowid
      units++
      ;(row.topics || []).forEach((t, ti) => {
        const tp = insTopic.run(unitId, t.name, ti)
        const topicId = tp.lastInsertRowid
        ;(t.objectives || []).forEach((o) =>
          insLO.run(unitId, topicId, o.code ?? null, typeof o === 'string' ? o : o.text,
            o.cognitive_level ?? null, row.source ?? 'pdf', row.ai_structured ? 1 : 0))
      })
      ;(row.objectives || []).forEach((o) =>
        insLO.run(unitId, null, o.code ?? null, typeof o === 'string' ? o : o.text,
          o.cognitive_level ?? null, row.source ?? 'pdf', row.ai_structured ? 1 : 0))
    }
  })()
  return units
}

function loadQuestions() {
  const f = resolve(ingestDir, 'questions.json')
  if (!existsSync(f)) return 0
  const data = JSON.parse(rf(f, 'utf-8'))
  // resolve unit_id by (grade_code, subject_id, normalized unit name)
  const unitLookup = new Map()
  for (const u of db.prepare('SELECT id,grade_code,subject_id,name FROM units').all()) {
    unitLookup.set(`${u.grade_code}|${u.subject_id}|${norm(u.name)}`, u.id)
  }
  const ins = db.prepare(
    `INSERT OR IGNORE INTO questions
       (grade_code,subject_id,unit_id,kind,stem,options_json,correct_index,q_type,marks,
        model_answer,topic_text,skill,difficulty,cognitive_level,feedback,article,
        source_paper,source_format,content_hash)
     VALUES (@grade_code,@subject_id,@unit_id,@kind,@stem,@options_json,@correct_index,@q_type,@marks,
        @model_answer,@topic_text,@skill,@difficulty,@cognitive_level,@feedback,@article,
        @source_paper,@source_format,@content_hash)`,
  )
  let n = 0
  db.transaction(() => {
    db.exec("DELETE FROM questions WHERE source_format != 'admin'") // refresh ingested, keep admin-authored
    for (const q of data) {
      const sid = subjectId(q.subject)
      const unitId = unitLookup.get(`${q.grade}|${sid}|${norm(q.unit)}`) ?? null
      ins.run({
        grade_code: q.grade, subject_id: sid ?? null, unit_id: unitId,
        kind: q.kind || 'objective', stem: q.q ?? q.stem,
        options_json: q.options ? JSON.stringify(q.options) : null,
        correct_index: q.correct ?? null,
        q_type: q.q_type ?? null, marks: q.marks ?? null, model_answer: q.model_answer ?? null,
        topic_text: q.topic ?? '', skill: q.skill ?? '', difficulty: q.difficulty ?? null,
        cognitive_level: q.cognitive_level ?? null, feedback: q.feedback ?? '', article: q.article ?? null,
        source_paper: q.paper ?? null, source_format: q.source_format ?? 'xlsx',
        content_hash: q.content_hash,
      })
      n++
    }
  })()
  return n
}

const sylUnits = loadSyllabus()
const qN = loadQuestions()

const counts = {
  grades: db.prepare('SELECT COUNT(*) c FROM grades').get().c,
  subjects: db.prepare('SELECT COUNT(*) c FROM subjects').get().c,
  grade_subjects: db.prepare('SELECT COUNT(*) c FROM grade_subjects').get().c,
  tutors: db.prepare('SELECT COUNT(*) c FROM ai_tutors').get().c,
  units: db.prepare('SELECT COUNT(*) c FROM units').get().c,
  questions: db.prepare('SELECT COUNT(*) c FROM questions').get().c,
}
console.log('[seed] done:', counts, `(syllabus units +${sylUnits}, questions loaded ${qN})`)
if (!qN) console.log(`[seed] note: no questions.json at ${ingestDir} yet — run the Python ingester (Phase 3) then re-seed.`)
