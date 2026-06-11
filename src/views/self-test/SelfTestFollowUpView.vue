<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Self Test Follow-up</h2>
    <StatCards :total="records.length" :pending="pending" :taken="taken" />

    <TestTable :empty="!records.length">
      <template #rows>
        <tr v-for="(r, i) in records" :key="r.id">
          <td>{{ i + 1 }}</td>
          <td>{{ r.subject }} {{ r.grade || 9 }}</td>
          <td>{{ formatDate(r.date) }}</td>
          <td>{{ r.score !== undefined ? r.score + '/' + r.total : '' }}</td>
          <td><span v-if="r.score !== undefined" class="status-notq">not qualified</span></td>
          <td>
            <RouterLink v-if="r.score !== undefined" :to="`/app/self-test/${r.bookId}/result`" class="btn-view">VIEW DETAILS</RouterLink>
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
const records = computed(() => student.testRecords.filter(t => t.type?.includes('Self Test') || t.type?.includes('Objective Self Test')))
const taken = computed(() => records.value.filter(r => r.score !== undefined).length)
const pending = computed(() => records.value.length - taken.value)

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: 'numeric' }).replace(/\//g, '-')
}
</script>
