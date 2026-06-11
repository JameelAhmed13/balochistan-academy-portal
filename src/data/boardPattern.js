// Balochistan Board (BISE) paper patterns.
// Used to shape AI-generated and mock tests so they match the real board
// exam structure (MCQ count, short/long split, marks distribution).

export const BALOCHISTAN_BOARD = {
  name: 'Balochistan Board (BISE)',
  // SSC — Secondary (Grade 9 & 10 / Matric)
  ssc: {
    label: 'SSC (Matric)',
    objective: { count: 12, marksEach: 1 },          // Section A — MCQs
    short:     { attempt: 6, of: 8, marksEach: 2 },  // Section B — Short questions
    long:      { attempt: 2, of: 3, marksEach: 5 },  // Section C — Long questions
  },
  // HSSC — Higher Secondary (Grade 11 & 12 / Intermediate / F.Sc)
  hssc: {
    label: 'HSSC (Intermediate)',
    objective: { count: 17, marksEach: 1 },
    short:     { attempt: 8, of: 12, marksEach: 2 },
    long:      { attempt: 3, of: 5,  marksEach: 5 },
  },
}

// Grade → exam level. 9/10 = SSC, 11/12 = HSSC.
export function levelForGrade(grade) {
  return +grade >= 11 ? 'hssc' : 'ssc'
}

// Map a Preparation bookId (1–10 core, 101–120 F.Sc) to a board level.
export function levelForBookId(bookId) {
  return +bookId >= 101 ? 'hssc' : 'ssc'
}

export function patternFor({ grade, bookId } = {}) {
  const level = bookId != null ? levelForBookId(bookId) : levelForGrade(grade ?? 9)
  return { level, ...BALOCHISTAN_BOARD[level] }
}

export function totalMarks(p) {
  return (
    p.objective.count * p.objective.marksEach +
    p.short.attempt * p.short.marksEach +
    p.long.attempt * p.long.marksEach
  )
}

// Human-readable one-liner, e.g. "12 MCQs · 6/8 short (2) · 2/3 long (5) · 34 marks"
export function patternSummary(p) {
  return `${p.objective.count} MCQs · ${p.short.attempt}/${p.short.of} short (${p.short.marksEach}m) · ${p.long.attempt}/${p.long.of} long (${p.long.marksEach}m) · ${totalMarks(p)} marks`
}
