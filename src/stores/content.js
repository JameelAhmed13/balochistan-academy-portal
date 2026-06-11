import { defineStore } from 'pinia'
import { ref } from 'vue'

export const SUBJECTS = [
  { id: 1, name: 'Urdu',                nameUr: 'اردو',           color: 'grad-violet', icon: '📖', medium: 'urdu' },
  { id: 2, name: 'Mutalia Pakistan',    nameUr: 'مطالعہ پاکستان', color: 'grad-green',  icon: '🌍', medium: 'urdu' },
  { id: 3, name: 'English',             nameUr: 'انگریزی',        color: 'grad-blue',   icon: '✍️', medium: 'english' },
  { id: 4, name: 'Biology',             nameUr: 'حیاتیات',        color: 'grad-emerald',icon: '🧬', medium: 'english' },
  { id: 5, name: 'Chemistry',           nameUr: 'کیمیاء',         color: 'grad-rose',   icon: '⚗️', medium: 'english' },
  { id: 6, name: 'Physics',             nameUr: 'طبیعیات',        color: 'grad-amber',  icon: '⚡', medium: 'english' },
  { id: 7, name: 'Mathematics',         nameUr: 'ریاضی',          color: 'grad-teal',   icon: '📐', medium: 'english' },
  { id: 8, name: 'Pakistan Studies',    nameUr: 'پاکستان سٹڈیز',  color: 'grad-pink',   icon: '🏛️', medium: 'english' },
  { id: 9, name: 'Islamiyat Lazmi',     nameUr: 'اسلامیات لازمی', color: 'grad-green',  icon: '☪️', medium: 'urdu' },
  { id: 10, name: 'Computer Science',   nameUr: 'کمپیوٹر سائنس',  color: 'grad-cyan',   icon: '💻', medium: 'english' },
]

export const AI_TUTORS = [
  { subject: 'Physics',          persona: 'Albert Einstein',  slug: 'einstein',   icon: '⚡', color: 'grad-amber',   desc: 'Theoretical Physics Master' },
  { subject: 'Mathematics',      persona: 'Al-Khwarizmi',     slug: 'khwarizmi',  icon: '📐', color: 'grad-teal',    desc: 'Father of Algebra' },
  { subject: 'Chemistry',        persona: 'Marie Curie',      slug: 'curie',      icon: '⚗️', color: 'grad-rose',    desc: 'Nobel Prize Chemist' },
  { subject: 'Biology',          persona: 'Ibn Sina',         slug: 'ibnsina',    icon: '🧬', color: 'grad-emerald', desc: 'Father of Early Medicine' },
  { subject: 'Computer Science', persona: 'Alan Turing',      slug: 'turing',     icon: '💻', color: 'grad-blue',    desc: 'Father of Computing' },
  { subject: 'Pakistan Studies', persona: 'Allama Iqbal',     slug: 'iqbal',      icon: '🏛️', color: 'grad-violet',  desc: 'Poet-Philosopher of Pakistan' },
  { subject: 'English',          persona: 'William Shakespeare', slug: 'shakespeare', icon: '✍️', color: 'grad-cyan', desc: 'Greatest English Writer' },
  { subject: 'Urdu',             persona: 'Mirza Ghalib',     slug: 'ghalib',     icon: '📖', color: 'grad-pink',    desc: 'Master of Urdu Poetry' },
  { subject: 'Islamiyat Lazmi', persona: 'Imam Al-Ghazali',  slug: 'ghazali',    icon: '🕌', color: 'grad-green',   desc: 'Islamic Scholar & Philosopher' },
]

// Mock question bank
function makeObjectiveQ(id, subject, unit, topic) {
  const options = ['Option A — First answer choice', 'Option B — Second answer choice', 'Option C — Third answer choice', 'Option D — Fourth answer choice']
  return {
    id, subject, unit, topic,
    stem: `Sample question ${id} about ${topic} in ${subject}?`,
    options,
    correct: Math.floor(Math.random() * 4),
    type: ['Exercise', 'Conceptual'][id % 2],
    difficulty: ['Easy', 'Medium', 'Hard'][id % 3],
    cognitiveLevel: ['Knowledge', 'Understanding', 'Applying'][id % 3],
    slo: `SLO ${id}.${unit}.${topic.slice(0,2)}`,
    entranceExam: id % 5 === 0,
    reason: `This is the explanation for why the correct answer is correct. The concept relates to fundamental principles of ${subject}.`,
  }
}

function makeSubjectiveQ(id, subject, unit) {
  return {
    id, subject, unit,
    stem: `Explain the concept of topic ${id} in ${subject} with reference to unit ${unit}.`,
    type: id % 2 === 0 ? 'Short' : 'Long',
    difficulty: ['Easy', 'Medium', 'Hard'][id % 3],
    cognitiveLevel: ['Knowledge', 'Understanding', 'Applying'][id % 3],
    marks: id % 2 === 0 ? 3 : 7,
    modelAnswer: `This is a comprehensive model answer for question ${id}. It covers the key concepts, provides examples, and demonstrates understanding of the topic at an appropriate level for Grade 9.`,
    predicted: id % 7 === 0,
  }
}

export const useContentStore = defineStore('content', () => {
  const subjects = ref(SUBJECTS)

  // Generate mock question banks per subject
  const objectiveBank = ref({})
  const subjectiveBank = ref({})

  SUBJECTS.forEach(s => {
    objectiveBank.value[s.id] = Array.from({ length: 120 }, (_, i) =>
      makeObjectiveQ(s.id * 1000 + i, s.name, Math.floor(i / 12) + 1, `Topic ${(i % 12) + 1}`)
    )
    subjectiveBank.value[s.id] = Array.from({ length: 60 }, (_, i) =>
      makeSubjectiveQ(s.id * 100 + i, s.name, Math.floor(i / 6) + 1)
    )
  })

  const units = ref({})
  SUBJECTS.forEach(s => {
    units.value[s.id] = Array.from({ length: 10 }, (_, i) => ({
      id: i + 1,
      name: `Unit ${i + 1}`,
      topics: Array.from({ length: 8 }, (_, j) => ({ id: j + 1, name: `Topic ${j + 1}` })),
    }))
  })

  function getQuestions(subjectId, filters = {}) {
    let qs = objectiveBank.value[subjectId] || []
    if (filters.type?.length)       qs = qs.filter(q => filters.type.includes(q.type))
    if (filters.difficulty?.length) qs = qs.filter(q => filters.difficulty.includes(q.difficulty))
    if (filters.entranceExam != null) qs = qs.filter(q => q.entranceExam === filters.entranceExam)
    if (filters.units?.length)      qs = qs.filter(q => filters.units.includes(q.unit))
    if (filters.limit)              qs = qs.slice(0, filters.limit)
    return qs
  }

  function getSubjectiveQuestions(subjectId, filters = {}) {
    let qs = subjectiveBank.value[subjectId] || []
    if (filters.type?.length)       qs = qs.filter(q => filters.type.includes(q.type))
    if (filters.difficulty?.length) qs = qs.filter(q => filters.difficulty.includes(q.difficulty))
    if (filters.limit)              qs = qs.slice(0, filters.limit)
    return qs
  }

  return { subjects, units, objectiveBank, subjectiveBank, getQuestions, getSubjectiveQuestions }
})
