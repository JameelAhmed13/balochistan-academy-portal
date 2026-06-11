<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Parent Tests</h2>
    <StatCards :total="tests.length" :pending="pendingCount" :taken="takenCount" />

    <TestTable :empty="!tests.length">
      <template #actions>
        <button @click="createNew" class="btn-create">+ Create New</button>
      </template>
      <template #rows>
        <tr v-for="(t, i) in tests" :key="t.id">
          <td>{{ i + 1 }}</td>
          <td>{{ t.subject }}</td>
          <td>{{ t.date }}</td>
          <td>{{ t.result || '' }}</td>
          <td>{{ t.coins || '' }}</td>
          <td>
            <RouterLink v-if="!t.taken" :to="`/app/parent-tests/${t.id}/take`" class="btn-attempt">ATTEMPT</RouterLink>
            <RouterLink v-else :to="`/app/parent-tests/${t.id}/result`" class="btn-view">VIEW DETAILS</RouterLink>
          </td>
        </tr>
      </template>
    </TestTable>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import StatCards from '@/components/platform/StatCards.vue'
import TestTable from '@/components/platform/TestTable.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const router = useRouter()

const tests = ref([
  { id: 'pt1', subject: 'Urdu 9', date: '10-06-2026', taken: false },
  { id: 'pt2', subject: 'Computer Science 9', date: '09-02-2026', taken: false },
])

const pendingCount = computed(() => tests.value.filter(t => !t.taken).length)
const takenCount = computed(() => tests.value.filter(t => t.taken).length)

function createNew() {
  // In demo, add a new test
  const subjects = ['Mathematics Eng Medium 9', 'Biology Eng Medium 9', 'Physics Eng Medium 9', 'English 9']
  const s = subjects[tests.value.length % subjects.length]
  const d = new Date()
  tests.value.push({
    id: 'pt' + Date.now(),
    subject: s,
    date: d.toLocaleDateString('en-GB').replace(/\//g, '-'),
    taken: false,
  })
}
</script>
