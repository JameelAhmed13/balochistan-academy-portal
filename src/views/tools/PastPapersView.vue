<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Past Papers</h2>
    <div class="pp-filters">
      <select v-model="filterSubject" class="pp-sel">
        <option value="">All Subjects</option>
        <option v-for="s in SUBJECTS" :key="s.id" :value="s.name">{{ s.name }}</option>
      </select>
      <select v-model="filterYear" class="pp-sel">
        <option value="">All Years</option>
        <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
      </select>
      <select v-model="filterGrade" class="pp-sel">
        <option value="">All Grades</option>
        <option value="9">Class 9</option>
        <option value="10">Class 10</option>
        <option value="11">F.Sc Part 1</option>
        <option value="12">F.Sc Part 2</option>
      </select>
    </div>
    <TestTable :empty="!filtered.length">
      <template #rows>
        <tr v-for="(p, i) in filtered" :key="p.id">
          <td>{{ i + 1 }}</td>
          <td>{{ p.subject }}</td>
          <td>{{ p.grade }}</td>
          <td>{{ p.year }}</td>
          <td><span class="pp-board-badge">{{ p.board }}</span></td>
          <td>
            <RouterLink :to="`/app/preparation/${p.bookId}/test/objective?type=past`" class="btn-attempt">ATTEMPT</RouterLink>
          </td>
        </tr>
      </template>
    </TestTable>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { SUBJECTS } from '@/stores/content'
import TestTable from '@/components/platform/TestTable.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const filterSubject = ref('')
const filterYear = ref('')
const filterGrade = ref('')
const years = [2024, 2023, 2022, 2021, 2020, 2019, 2018]

const papers = [
  { id: 1, subject: 'Mathematics', grade: 'Class 9',  year: 2024, board: 'Federal Board', bookId: 7 },
  { id: 2, subject: 'Physics',     grade: 'Class 9',  year: 2024, board: 'Federal Board', bookId: 6 },
  { id: 3, subject: 'Chemistry',   grade: 'Class 9',  year: 2024, board: 'Federal Board', bookId: 5 },
  { id: 4, subject: 'Biology',     grade: 'Class 9',  year: 2024, board: 'Federal Board', bookId: 4 },
  { id: 5, subject: 'Urdu',        grade: 'Class 9',  year: 2024, board: 'Federal Board', bookId: 1 },
  { id: 6, subject: 'English',     grade: 'Class 9',  year: 2024, board: 'Federal Board', bookId: 3 },
  { id: 7, subject: 'Mathematics', grade: 'Class 10', year: 2024, board: 'Federal Board', bookId: 7 },
  { id: 8, subject: 'Physics',     grade: 'Class 10', year: 2024, board: 'Federal Board', bookId: 6 },
  { id: 9, subject: 'Chemistry',   grade: 'Class 10', year: 2023, board: 'Punjab Board', bookId: 5 },
  { id: 10, subject: 'Biology',    grade: 'Class 10', year: 2023, board: 'Punjab Board', bookId: 4 },
  { id: 11, subject: 'Mathematics',grade: 'F.Sc Part 1', year: 2023, board: 'Federal Board', bookId: 7 },
  { id: 12, subject: 'Physics',    grade: 'F.Sc Part 1', year: 2023, board: 'Punjab Board', bookId: 6 },
]

const filtered = computed(() => papers.filter(p => {
  if (filterSubject.value && p.subject !== filterSubject.value) return false
  if (filterYear.value && p.year !== Number(filterYear.value)) return false
  if (filterGrade.value) {
    const g = { '9': 'Class 9', '10': 'Class 10', '11': 'F.Sc Part 1', '12': 'F.Sc Part 2' }[filterGrade.value]
    if (p.grade !== g) return false
  }
  return true
}))
</script>

<style scoped>
.pp-filters { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.pp-sel { padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-surface); color: var(--t-text1); font-size: 0.82rem; }
.pp-board-badge { display: inline-block; padding: 0.2rem 0.6rem; border-radius: 99px; background: #e3f2fd; color: #1565c0; font-size: 0.72rem; font-weight: 700; }
html.dark .pp-board-badge { background: rgba(21,101,192,0.2); color: #90caf9; }
.btn-attempt { display: inline-block; padding: 0.35rem 0.9rem; border-radius: 6px; font-size: 0.75rem; font-weight: 700; background: #4caf50; color: white; text-decoration: none; }
</style>
