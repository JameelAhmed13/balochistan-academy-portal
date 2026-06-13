import Database from 'better-sqlite3'
import { fileURLToPath } from 'node:url'
import { dirname, resolve, isAbsolute } from 'node:path'
import { mkdirSync } from 'node:fs'
import 'dotenv/config'

const __dirname = dirname(fileURLToPath(import.meta.url))
// server/src/db -> server/
const serverRoot = resolve(__dirname, '..', '..')

const rawPath = process.env.DB_PATH || './data/estudy.db'
export const dbPath = isAbsolute(rawPath) ? rawPath : resolve(serverRoot, rawPath)

mkdirSync(dirname(dbPath), { recursive: true })

export const db = new Database(dbPath)
db.pragma('journal_mode = WAL')
db.pragma('foreign_keys = ON')

export default db
