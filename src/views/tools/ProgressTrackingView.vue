<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Progress Tracking</h2>
    <!-- Summary row -->
    <div class="prog-summary">
      <div class="prog-stat">
        <div class="prog-val">{{ student.testRecords.length }}</div>
        <div class="prog-lbl">Total Tests</div>
      </div>
      <div class="prog-stat">
        <div class="prog-val">{{ avgScore }}%</div>
        <div class="prog-lbl">Avg Score</div>
      </div>
      <div class="prog-stat">
        <div class="prog-val">{{ student.testRecords.filter(t => t.score/t.total >= 0.5).length }}</div>
        <div class="prog-lbl">Passed</div>
      </div>
      <div class="prog-stat">
        <div class="prog-val">{{ uniqueSubjects }}</div>
        <div class="prog-lbl">Subjects</div>
      </div>
    </div>
    <!-- Per-subject breakdown -->
    <div class="prog-subjects">
      <div v-for="s in subjectStats" :key="s.name" class="prog-sub-row">
        <div class="prog-sub-name">{{ s.icon }} {{ s.name }}</div>
        <div class="prog-bar-wrap">
          <div class="prog-bar" :style="{ width: s.pct + '%', background: s.pct >= 60 ? '#4caf50' : s.pct >= 40 ? '#f57c00' : '#ef5350' }" />
        </div>
        <div class="prog-sub-pct">{{ s.pct }}%</div>
        <div class="prog-sub-count">{{ s.count }} tests</div>
      </div>
      <div v-if="!subjectStats.length" class="prog-empty">No test records yet. Start taking tests to see your progress!</div>
    </div>
    <!-- Recent records -->
    <TestTable :empty="!student.testRecords.length">
      <template #rows>
        <tr v-for="(r, i) in student.testRecords.slice(-20).reverse()" :key="r.id">
          <td>{{ i + 1 }}</td>
          <td>{{ r.subject }}</td>
          <td>{{ formatDate(r.date) }}</td>
          <td>{{ r.score }}/{{ r.total }}</td>
          <td>
            <span :class="r.score/r.total >= 0.5 ? 'prog-pass' : 'prog-fail'">
              {{ r.score/r.total >= 0.5 ? 'Pass' : 'Fail' }}
            </span>
          </td>
          <td></td>
        </tr>
      </template>
    </TestTable>
    <PageFooter />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { SUBJECTS } from '@/stores/content'
import { useStudentStore } from '@/stores/student'
import TestTable from '@/components/platform/TestTable.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const student = useStudentStore()

const avgScore = computed(() => {
  const scored = student.testRecords.filter(r => r.score !== undefined && r.total)
  if (!scored.length) return 0
  return Math.round(scored.reduce((a, r) => a + r.score / r.total, 0) / scored.length * 100)
})
const uniqueSubjects = computed(() => new Set(student.testRecords.map(t => t.subject)).size)

const subjectStats = computed(() => {
  return SUBJECTS.map(s => {
    const records = student.testRecords.filter(r => r.subject === s.name && r.total)
    if (!records.length) return null
    const pct = Math.round(records.reduce((a, r) => a + r.score / r.total, 0) / records.length * 100)
    return { name: s.name, icon: s.icon, pct, count: records.length }
  }).filter(Boolean)
})

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('en-GB', { timeZone: 'Asia/Karachi', day: '2-digit', month: '2-digit', year: 'numeric' }).replace(/\//g, '-')
}
</script>

<style scoped>
.prog-summary { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; }
@media (max-width: 480px) { .prog-summary { grid-template-columns: repeat(2, 1fr); } }
.prog-stat { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; padding: 1rem; text-align: center; }
.prog-val { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); }
.prog-lbl { font-size: 0.7rem; color: var(--t-text3); margin-top: 0.2rem; }
.prog-subjects { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem; display: flex; flex-direction: column; gap: 0.85rem; }
.prog-sub-row { display: flex; align-items: center; gap: 0.75rem; }
.prog-sub-name { font-size: 0.82rem; font-weight: 600; color: var(--t-text1); width: 160px; flex-shrink: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.prog-bar-wrap { flex: 1; height: 8px; background: var(--t-border); border-radius: 99px; overflow: hidden; }
.prog-bar { height: 100%; border-radius: 99px; transition: width 0.5s; }
.prog-sub-pct { font-size: 0.78rem; font-weight: 700; color: var(--t-text1); width: 36px; text-align: right; }
.prog-sub-count { font-size: 0.7rem; color: var(--t-text3); width: 50px; }
.prog-empty { text-align: center; padding: 1.5rem; color: var(--t-text3); font-size: 0.875rem; }
.prog-pass { color: #2e7d32; font-size: 0.78rem; font-weight: 700; }
.prog-fail { color: #c62828; font-size: 0.78rem; font-weight: 700; }
</style>
