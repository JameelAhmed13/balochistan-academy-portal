// Deterministic study-schedule engine (spec §4–5, §9).
// Pure functions, no DB/LLM — easy to test and to later refine with an LLM pass.

const DAY_MS = 24 * 60 * 60 * 1000
const DIFFICULTY_FACTOR = { easy: 1, medium: 1.5, hard: 2 }
const WEAK_BOOST = 1.6

function toDate(iso) {
  // Normalise to UTC midnight so day math is stable regardless of timezone.
  const [y, m, d] = String(iso).slice(0, 10).split('-').map(Number)
  return new Date(Date.UTC(y, m - 1, d))
}
function isoDay(date) { return date.toISOString().slice(0, 10) }
function addDays(date, n) { return new Date(date.getTime() + n * DAY_MS) }
export function daysBetween(fromIso, toIso) {
  return Math.round((toDate(toIso) - toDate(fromIso)) / DAY_MS)
}

// Largest-remainder allocation so the day counts sum exactly to `slots`.
function allocate(weights, slots) {
  const total = weights.reduce((a, b) => a + b, 0) || 1
  const raw = weights.map((w) => (w / total) * slots)
  const base = raw.map(Math.floor)
  let used = base.reduce((a, b) => a + b, 0)
  const order = raw
    .map((r, i) => ({ i, frac: r - Math.floor(r) }))
    .sort((a, b) => b.frac - a.frac)
  let k = 0
  while (used < slots && k < order.length) { base[order[k].i] += 1; used += 1; k += 1 }
  return base
}

/**
 * Build a day-by-day study plan.
 * @param {Object} o
 * @param {Array}  o.units        - [{ id, name, number, sort_order, difficulty?, marks_weightage? }]
 * @param {string} o.startDate    - ISO date (today)
 * @param {string} o.examDate     - ISO date
 * @param {number} [o.dailyHours]
 * @param {number[]} [o.weakUnitIds]
 * @returns {{ days: Array, meta: Object }}
 */
export function buildSchedule({ units, startDate, examDate, dailyHours = 2, weakUnitIds = [] }) {
  const weak = new Set(weakUnitIds)
  const totalDays = daysBetween(startDate, examDate)
  const estMinutes = Math.max(30, Math.round(dailyHours * 60))

  if (!units.length || totalDays <= 0) {
    return { days: [], meta: { totalDays, studyDays: 0, revisionDays: 0, uncovered: units.length, examPassed: totalDays <= 0 } }
  }

  // Reserve the final stretch for revision only (spec §4.2: last 7 days = revision).
  let revisionDays = Math.min(7, Math.max(0, totalDays - 1))
  let studyDays = totalDays - revisionDays
  if (studyDays < 1) { studyDays = totalDays; revisionDays = 0 }

  // Weight: marks weightage × difficulty × weak-topic boost.
  const weighted = units.map((u) => {
    const diff = DIFFICULTY_FACTOR[(u.difficulty || 'medium').toLowerCase()] || 1.5
    const marks = Number(u.marks_weightage) || 0
    const boost = weak.has(u.id) ? WEAK_BOOST : 1
    return { unit: u, weight: (1 + marks / 20) * diff * boost }
  })

  // Allocate study days. Guarantee ≥1 day per unit when there's room.
  let counts
  let uncovered = 0
  if (studyDays >= units.length) {
    const extra = allocate(weighted.map((w) => w.weight), studyDays - units.length)
    counts = weighted.map((_, i) => 1 + extra[i])
  } else {
    // More units than study days — cover the highest-weight units; rest fall to revision.
    const top = [...weighted].sort((a, b) => b.weight - a.weight).slice(0, studyDays)
    const topIds = new Set(top.map((w) => w.unit.id))
    counts = weighted.map((w) => (topIds.has(w.unit.id) ? 1 : 0))
    uncovered = units.length - studyDays
  }

  // Study order: weak units first, then natural syllabus order.
  const ordered = weighted
    .map((w, i) => ({ ...w, days: counts[i] }))
    .filter((w) => w.days > 0)
    .sort((a, b) => {
      const aw = weak.has(a.unit.id) ? 0 : 1
      const bw = weak.has(b.unit.id) ? 0 : 1
      if (aw !== bw) return aw - bw
      return (a.unit.sort_order || a.unit.number || 0) - (b.unit.sort_order || b.unit.number || 0)
    })

  const start = toDate(startDate)
  const days = []
  let cursor = 0

  for (const w of ordered) {
    for (let part = 1; part <= w.days; part += 1) {
      const label = w.days > 1 ? `${w.unit.name} (part ${part}/${w.days})` : w.unit.name
      days.push({
        day_date: isoDay(addDays(start, cursor)),
        unit_id: w.unit.id,
        unit_title: w.unit.name,
        task: `Study: ${label}`,
        mode: 'study',
        est_minutes: estMinutes,
        self_test: 1,
        status: 'pending',
        sort_order: cursor,
      })
      cursor += 1
    }
  }

  // Revision tail — cycle through all units.
  for (let r = 0; r < revisionDays; r += 1) {
    const u = units[r % units.length]
    days.push({
      day_date: isoDay(addDays(start, cursor)),
      unit_id: u.id,
      unit_title: u.name,
      task: `Revision: ${u.name}`,
      mode: 'revision',
      est_minutes: estMinutes,
      self_test: r % 2 === 0 ? 1 : 0,
      status: 'pending',
      sort_order: cursor,
    })
    cursor += 1
  }

  return { days, meta: { totalDays, studyDays, revisionDays, uncovered } }
}
