<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Parent Test Follow-up</h2>
    <StatCards :total="records.length" :pending="pending" :taken="taken" />

    <TestTable :empty="!records.length">
      <template #empty>
        <div class="ptf-empty">
          <div class="ptf-empty-icon">📋</div>
          <div class="ptf-empty-title">No Follow-up Tests Yet</div>
          <div class="ptf-empty-msg">
            After you attempt a Parent Test, follow-up tests will be automatically generated
            based on your weak areas. These targeted tests help reinforce concepts you found difficult.
          </div>
          <RouterLink to="/app/parent-tests" class="ptf-empty-cta">Go to Parent Tests</RouterLink>
        </div>
      </template>
      <template #rows>
        <tr v-for="(r, i) in records" :key="r.id">
          <td>{{ i + 1 }}</td>
          <td>{{ r.subject }}</td>
          <td>{{ formatDate(r.date) }}</td>
          <td>{{ r.score !== undefined ? r.score + '/' + r.total : '' }}</td>
          <td><span v-if="r.score !== undefined" :class="r.score/r.total >= 0.5 ? 'status-pending' : 'status-notq'">
            {{ r.score/r.total >= 0.5 ? 'Qualified' : 'not qualified' }}
          </span></td>
          <td>
            <RouterLink v-if="r.score !== undefined" :to="`/app/parent-tests/${r.id}/result`" class="btn-view">VIEW DETAILS</RouterLink>
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
const records = computed(() => student.testRecords.filter(t => t.type === 'Parent Test Follow-up'))
const taken = computed(() => records.value.filter(r => r.score !== undefined).length)
const pending = computed(() => records.value.length - taken.value)

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('en-GB', { timeZone: 'Asia/Karachi', day: '2-digit', month: '2-digit', year: 'numeric' }).replace(/\//g, '-')
}
</script>

<style scoped>
.ptf-empty { text-align: center; padding: 3rem 2rem; }
.ptf-empty-icon { font-size: 3rem; margin-bottom: 1rem; }
.ptf-empty-title { font-size: 1.1rem; font-weight: 700; color: var(--t-text1); margin-bottom: 0.75rem; }
.ptf-empty-msg { font-size: 0.875rem; color: var(--t-text2); max-width: 420px; margin: 0 auto 1.5rem; line-height: 1.6; }
.ptf-empty-cta {
  display: inline-block; padding: 0.6rem 1.5rem; border-radius: 8px;
  background: #f57c00; color: white; font-weight: 700; font-size: 0.85rem;
  text-decoration: none;
}
</style>
