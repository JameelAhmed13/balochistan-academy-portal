// Canonical curriculum seed — grades (ECD–12), subjects, per-grade subject map,
// and AI tutor personas. Derived from the docs inventory + the app's original
// SUBJECTS / AI_TUTORS constants. Loaded by seed.js (idempotent upserts).

export const GRADES = [
  { code: 'ECD', label: 'ECD', band: 'ECD-5', sort_order: 0 },
  { code: '1', label: 'Grade 1', band: 'ECD-5', sort_order: 1 },
  { code: '2', label: 'Grade 2', band: 'ECD-5', sort_order: 2 },
  { code: '3', label: 'Grade 3', band: 'ECD-5', sort_order: 3 },
  { code: '4', label: 'Grade 4', band: 'ECD-5', sort_order: 4 },
  { code: '5', label: 'Grade 5', band: 'ECD-5', sort_order: 5 },
  { code: '6', label: 'Grade 6', band: '6-8', sort_order: 6 },
  { code: '7', label: 'Grade 7', band: '6-8', sort_order: 7 },
  { code: '8', label: 'Grade 8', band: '6-8', sort_order: 8 },
  { code: '9', label: 'Grade 9', band: '9-12', sort_order: 9 },
  { code: '10', label: 'Grade 10', band: '9-12', sort_order: 10 },
  { code: '11', label: 'Grade 11', band: '9-12', sort_order: 11 },
  { code: '12', label: 'Grade 12', band: '9-12', sort_order: 12 },
]

// canonical subject names must match what the Python ingester emits (subject_from_path)
export const SUBJECTS = [
  { name: 'English', name_ur: 'انگریزی', icon: '✍️', color: 'grad-blue', medium: 'english' },
  { name: 'Mathematics', name_ur: 'ریاضی', icon: '📐', color: 'grad-teal', medium: 'english' },
  { name: 'General Knowledge', name_ur: 'عمومی معلومات', icon: '🌟', color: 'grad-amber', medium: 'english' },
  { name: 'Urdu', name_ur: 'اردو', icon: '📖', color: 'grad-violet', medium: 'urdu' },
  { name: 'Science', name_ur: 'سائنس', icon: '🔬', color: 'grad-emerald', medium: 'english' },
  { name: 'Biology', name_ur: 'حیاتیات', icon: '🧬', color: 'grad-emerald', medium: 'english' },
  { name: 'Chemistry', name_ur: 'کیمیا', icon: '⚗️', color: 'grad-rose', medium: 'english' },
  { name: 'Physics', name_ur: 'طبیعیات', icon: '⚡', color: 'grad-amber', medium: 'english' },
  { name: 'Pak Studies', name_ur: 'مطالعہ پاکستان', icon: '🏛️', color: 'grad-pink', medium: 'urdu' },
  { name: 'Computer Science', name_ur: 'کمپیوٹر سائنس', icon: '💻', color: 'grad-cyan', medium: 'english' },
  { name: 'Islamiyat', name_ur: 'اسلامیات', icon: '☪️', color: 'grad-green', medium: 'urdu' },
]

const PRIMARY = ['English', 'Mathematics', 'General Knowledge', 'Urdu']
const MIDDLE = ['English', 'Mathematics', 'Science', 'Urdu']
const SECONDARY = ['Biology', 'Chemistry', 'Physics', 'English', 'Mathematics', 'Urdu', 'Pak Studies', 'Computer Science', 'Islamiyat']
const HIGHER = ['Biology', 'Chemistry', 'Physics', 'English', 'Mathematics', 'Urdu', 'Pak Studies']

export const GRADE_SUBJECTS = {
  ECD: PRIMARY, 1: PRIMARY, 2: PRIMARY, 3: PRIMARY, 4: PRIMARY, 5: PRIMARY,
  6: MIDDLE, 7: MIDDLE, 8: MIDDLE,
  9: SECONDARY, 10: SECONDARY,
  11: HIGHER, 12: HIGHER,
}

// persona prompt is the grounding/character instruction injected as the chat system prompt
const tutorPrompt = (persona, subject) =>
  `You are ${persona}, a warm, patient AI tutor for ${subject} on the Balochistan Academy Portal. ` +
  `Teach in simple, encouraging language and adapt to the student's grade. Explain step by step, give ` +
  `one worked example, then a short practice question. Use Urdu or English to match the student. ` +
  `Only use the provided syllabus/curriculum context; never invent facts or reveal full test answers before the student attempts them.`

export const TUTORS = [
  { subject: 'Physics', persona: 'Albert Einstein', slug: 'einstein', icon: '⚡', color: 'grad-amber', description: 'Theoretical Physics Master' },
  { subject: 'Mathematics', persona: 'Al-Khwarizmi', slug: 'khwarizmi', icon: '📐', color: 'grad-teal', description: 'Father of Algebra' },
  { subject: 'Chemistry', persona: 'Marie Curie', slug: 'curie', icon: '⚗️', color: 'grad-rose', description: 'Nobel Prize Chemist' },
  { subject: 'Biology', persona: 'Ibn Sina', slug: 'ibnsina', icon: '🧬', color: 'grad-emerald', description: 'Father of Early Medicine' },
  { subject: 'Computer Science', persona: 'Alan Turing', slug: 'turing', icon: '💻', color: 'grad-blue', description: 'Father of Computing' },
  { subject: 'Pak Studies', persona: 'Allama Iqbal', slug: 'iqbal', icon: '🏛️', color: 'grad-violet', description: 'Poet-Philosopher of Pakistan' },
  { subject: 'English', persona: 'William Shakespeare', slug: 'shakespeare', icon: '✍️', color: 'grad-cyan', description: 'Greatest English Writer' },
  { subject: 'Urdu', persona: 'Mirza Ghalib', slug: 'ghalib', icon: '📖', color: 'grad-pink', description: 'Master of Urdu Poetry' },
  { subject: 'Islamiyat', persona: 'Imam Al-Ghazali', slug: 'ghazali', icon: '☪️', color: 'grad-green', description: 'Islamic Scholar & Philosopher' },
  { subject: 'Science', persona: 'Ibn al-Haytham', slug: 'alhaytham', icon: '🔬', color: 'grad-emerald', description: 'Father of the Scientific Method' },
  { subject: 'General Knowledge', persona: 'Professor Curio', slug: 'curio', icon: '🌟', color: 'grad-amber', description: 'Your Friendly Explorer Guide' },
].map((t) => ({ ...t, system_prompt: tutorPrompt(t.persona, t.subject) }))
