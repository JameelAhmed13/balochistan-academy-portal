// Shared subject catalogue for the Preparation workspace.
// Grades 9 & 10 use the core SUBJECTS (ids 1–10); grades 11 & 12 (F.Sc)
// use dedicated lists (ids 101–110 and 111–120). Centralised here so the
// layout, the book-home view and every study panel resolve subjects the
// same way — single source of truth.
import { SUBJECTS } from '@/stores/content'
import { useCatalogStore } from '@/stores/catalog'
import { useAuthStore } from '@/stores/auth'

export const FSC_PART1 = [
  { id: 101, name: 'Physics',          nameUr: 'طبیعیات',           icon: '⚡', color: 'grad-blue',    medium: 'english' },
  { id: 102, name: 'Chemistry',        nameUr: 'کیمیا',              icon: '🧪', color: 'grad-rose',    medium: 'english' },
  { id: 103, name: 'Biology',          nameUr: 'حیاتیات',            icon: '🌿', color: 'grad-emerald', medium: 'english' },
  { id: 104, name: 'Mathematics',      nameUr: 'ریاضی',              icon: '📐', color: 'grad-teal',    medium: 'english' },
  { id: 105, name: 'Computer Science', nameUr: 'کمپیوٹر سائنس',      icon: '💻', color: 'grad-cyan',    medium: 'english' },
  { id: 106, name: 'English',          nameUr: 'انگریزی',            icon: '📖', color: 'grad-amber',   medium: 'english' },
  { id: 107, name: 'Urdu',             nameUr: 'اردو',               icon: '✍️', color: 'grad-violet',  medium: 'urdu' },
  { id: 108, name: 'Islamiat',         nameUr: 'اسلامیات',           icon: '🕌', color: 'grad-green',   medium: 'urdu' },
  { id: 109, name: 'Pakistan Studies', nameUr: 'پاکستان کا مطالعہ',  icon: '🗺️', color: 'grad-pink',    medium: 'urdu' },
  { id: 110, name: 'Statistics',       nameUr: 'اعداد و شمار',       icon: '📊', color: 'grad-orange',  medium: 'english' },
]

export const FSC_PART2 = FSC_PART1.map((s) => ({ ...s, id: s.id + 10 }))

export const GRADE_SUBJECTS = {
  9:  SUBJECTS,
  10: SUBJECTS,
  11: FSC_PART1,
  12: FSC_PART2,
}

// Fallback pool excludes the base SUBJECTS (ids 1–10): those ids collide with
// the catalog's DB subject ids (e.g. legacy Urdu=1 vs catalog English=1), so for
// 1–11 we resolve ONLY via the catalog. Legacy FSC ids (101–120) don't collide.
const ALL_PREP_SUBJECTS = [...FSC_PART1, ...FSC_PART2]

// Resolve a subject record from any preparation bookId. Prefers the live catalog
// (DB subjects for the student's grade) so ECD–12 all resolve; falls back to the
// legacy hardcoded lists if the catalog isn't loaded.
export function findPrepSubject(id) {
  try {
    const cat = useCatalogStore()
    const pools = [cat.subjects, ...Object.values(cat.gradeSubjects)]
    for (const pool of pools) {
      const hit = pool?.find((s) => String(s.id) === String(id))
      if (hit) return { ...hit, nameUr: hit.nameUr ?? hit.name_ur }
    }
  } catch { /* pinia not ready / backend offline */ }
  const n = +id
  return ALL_PREP_SUBJECTS.find((s) => s.id === n) ?? null
}

// Grade label for headers / past-paper matching. Uses the logged-in student's
// grade from the catalog when available; falls back to the legacy id-range guess.
export function gradeLabelFor(id) {
  try {
    const auth = useAuthStore()
    const cat = useCatalogStore()
    const code = auth.user?.gradeCode
    if (code) return cat.gradeByCode(code)?.label || `Grade ${code}`
  } catch { /* ignore */ }
  const n = +id
  if (n >= 111) return 'F.Sc Part 2'
  if (n >= 101) return 'F.Sc Part 1'
  return 'Class 9–10'
}
