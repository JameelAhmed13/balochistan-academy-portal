import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { dirname, resolve } from 'node:path'
import db, { dbPath } from './connection.js'

const __dirname = dirname(fileURLToPath(import.meta.url))
const schema = readFileSync(resolve(__dirname, 'schema.sql'), 'utf-8')

db.exec(schema)
console.log(`[migrate] schema applied → ${dbPath}`)

const tables = db.prepare("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name").all()
console.log('[migrate] tables:', tables.map((t) => t.name).join(', '))
