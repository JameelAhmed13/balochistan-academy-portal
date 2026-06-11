<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Objective Test</h2>
    <StatCards :total="allRecords.length" :pending="pending" :taken="taken" />
    <TestTable :empty="!allRecords.length">
      <template #rows>
        <tr v-for="(r, i) in allRecords" :key="r.id">
          <td>{{ i + 1 }}</td>
          <td>{{ r.subject }} {{ r.grade || 9 }}</td>
          <td>{{ formatDate(r.date) }}</td>
          <td>{{ r.score !== undefined ? r.score + '/' + r.total : '' }}</td>
          <td>
            <span v-if="r.score !== undefined" :class="isPassed(r) ? 'status-pending' : 'status-notq'">
              {{ isPassed(r) ? 'Qualified' : 'not qualified' }}
            </span>
          </td>
          <td>
            <RouterLink v-if="r.score !== undefined" :to="`/app/objective-tests/${r.id}/result`" class="btn-view">VIEW DETAILS</RouterLink>
            <RouterLink v-else :to="`/app/preparation/${r.bookId}/test/objective`" class="btn-attempt">ATTEMPT</RouterLink>
          </td>
        </tr>
      </template>
    </TestTable>
    <PageFooter />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useStudentStore } from '@/stores/student'
import StatCards from '@/components/platform/StatCards.vue'
import TestTable from '@/components/platform/TestTable.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const student = useStudentStore()

// Mix of seeded demo records + real records
const DEMO = [
  { id: 'd1', subject: 'Mathematics Eng Medium', grade: 9, date: '2026-06-09T00:00:00Z', score: 0, total: 136, bookId: 7 },
  { id: 'd2', subject: 'Mathematics Eng Medium', grade: 9, date: '2026-06-09T00:00:00Z', score: undefined, total: 136, bookId: 7 },
  { id: 'd3', subject: 'Biology Eng Medium',     grade: 9, date: '2026-06-05T00:00:00Z', score: 4, total: 20,  bookId: 4 },
  { id: 'd4', subject: 'Physics Eng Medium',     grade: 9, date: '2026-06-05T00:00:00Z', score: 9, total: 15,  bookId: 6 },
  { id: 'd5', subject: 'Physics Eng Medium',     grade: 9, date: '2026-06-05T00:00:00Z', score: 0, total: 15,  bookId: 6 },
  { id: 'd6', subject: 'Biology Eng Medium',     grade: 9, date: '2026-05-18T00:00:00Z', score: undefined, total: 20, bookId: 4 },
  { id: 'd7', subject: 'Urdu',                   grade: 9, date: '2026-05-13T00:00:00Z', score: 1, total: 10,  bookId: 1 },
  { id: 'd8', subject: 'Urdu',                   grade: 9, date: '2026-04-17T00:00:00Z', score: 11, total: 15, bookId: 1 },
]
const realRecords = computed(() => student.testRecords.filter(t => t.type === 'Objective Test'))
const allRecords = computed(() => [...DEMO, ...realRecords.value])
const taken = computed(() => allRecords.value.filter(r => r.score !== undefined).length)
const pending = computed(() => allRecords.value.length - taken.value)

function isPassed(r) { return (r.score / r.total) >= 0.5 }
function formatDate(iso) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: 'numeric' }).replace(/\//g, '-')
}
</script>
