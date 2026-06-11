<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Daily Practice Hub</h2>

    <!-- Stat cards -->
    <StatCards
      :total="allRecords.length"
      :pending="pending"
      :taken="taken"
    />

    <!-- Analytics banner -->
    <div class="dph-analytics-row">
      <div class="dph-stat">
        <div class="dph-stat-val">{{ allRecords.length }}</div>
        <div class="dph-stat-lbl">Tests Created</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">{{ taken }}</div>
        <div class="dph-stat-lbl">Tests Taken</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">{{ avgScore }}%</div>
        <div class="dph-stat-lbl">Avg Score</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">{{ uniqueSubjects }}</div>
        <div class="dph-stat-lbl">Subjects Tested</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">{{ streak }}</div>
        <div class="dph-stat-lbl">Day Streak</div>
      </div>
    </div>

    <!-- Generate Daily Combined Challenge card -->
    <div class="dph-challenge-card">
      <div class="dph-cc-left">
        <div class="dph-cc-icon">⚡</div>
        <div>
          <div class="dph-cc-title">GENERATE DAILY COMBINED CHALLENGE</div>
          <div class="dph-cc-sub">Multi-subject timed exam · Objective + Subjective · Track your streak</div>
        </div>
      </div>
      <RouterLink to="/app/daily-tests/combined" class="dph-cc-btn">Generate Now</RouterLink>
    </div>

    <!-- Test History -->
    <TestTable :empty="!allRecords.length">
      <template #rows>
        <tr v-for="(r, i) in allRecords" :key="r.id">
          <td>{{ i + 1 }}</td>
          <td>{{ r.subject }}</td>
          <td>{{ formatDate(r.date) }}</td>
          <td>{{ r.score !== undefined ? r.score + '/' + r.total : '—' }}</td>
          <td>
            <span v-if="r.score !== undefined" :class="r.score/r.total >= 0.5 ? 'status-pending' : 'status-notq'">
              {{ r.score/r.total >= 0.5 ? 'Qualified' : 'not qualified' }}
            </span>
            <span v-else class="status-notq">pending</span>
          </td>
          <td>
            <RouterLink v-if="r.score !== undefined" :to="`/app/daily-tests/result/${r.id}`" class="btn-view">VIEW DETAILS</RouterLink>
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

const allRecords = computed(() => student.testRecords)
const taken = computed(() => allRecords.value.filter(r => r.score !== undefined).length)
const pending = computed(() => allRecords.value.length - taken.value)
const uniqueSubjects = computed(() => new Set(allRecords.value.map(t => t.subject)).size)
const streak = 3

const avgScore = computed(() => {
  const scored = allRecords.value.filter(r => r.score !== undefined && r.total)
  if (!scored.length) return 0
  return Math.round(scored.reduce((a, r) => a + r.score / r.total, 0) / scored.length * 100)
})

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: 'numeric' }).replace(/\//g, '-')
}
</script>

<style scoped>
.dph-analytics-row {
  display: grid; grid-template-columns: repeat(5, 1fr); gap: 0.75rem;
}
@media (max-width: 640px) { .dph-analytics-row { grid-template-columns: repeat(3, 1fr); } }
.dph-stat {
  background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px;
  padding: 0.9rem 1rem; text-align: center;
}
.dph-stat-val { font-size: 1.35rem; font-weight: 800; color: var(--t-text1); }
.dph-stat-lbl { font-size: 0.7rem; color: var(--t-text3); margin-top: 0.2rem; }

.dph-challenge-card {
  display: flex; align-items: center; justify-content: space-between; gap: 1rem; flex-wrap: wrap;
  padding: 1.25rem 1.5rem; border-radius: 14px;
  background: linear-gradient(135deg, #0d1b2a 0%, #1a3a5c 100%);
  border: 1px solid rgba(0, 188, 212, 0.25);
}
.dph-cc-left { display: flex; align-items: center; gap: 1rem; }
.dph-cc-icon {
  width: 48px; height: 48px; border-radius: 12px; background: rgba(0, 188, 212, 0.15);
  display: flex; align-items: center; justify-content: center; font-size: 1.5rem; flex-shrink: 0;
}
.dph-cc-title { font-size: 0.9rem; font-weight: 800; color: #fff; letter-spacing: 0.02em; }
.dph-cc-sub { font-size: 0.75rem; color: rgba(255,255,255,0.55); margin-top: 0.2rem; }
.dph-cc-btn {
  display: inline-block; padding: 0.6rem 1.5rem; border-radius: 9px; font-weight: 700; font-size: 0.85rem;
  background: #00bcd4; color: white; text-decoration: none; white-space: nowrap; flex-shrink: 0;
}
</style>
