<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Monthly Exam Powerhouse</h2>

    <!-- Stat cards -->
    <StatCards
      :total="instituteTests.length"
      :pending="pendingCount"
      :taken="takenCount"
    />

    <!-- Analytics row -->
    <div class="dph-analytics-row">
      <div class="dph-stat">
        <div class="dph-stat-val">{{ instituteTests.length }}</div>
        <div class="dph-stat-lbl">Total Tests</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">{{ takenCount }}</div>
        <div class="dph-stat-lbl">Tests Taken</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">{{ pendingCount }}</div>
        <div class="dph-stat-lbl">Pending</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">{{ expiredCount }}</div>
        <div class="dph-stat-lbl">Expired</div>
      </div>
      <div class="dph-stat">
        <div class="dph-stat-val">🏆</div>
        <div class="dph-stat-lbl">Rank This Month</div>
      </div>
    </div>

    <!-- Quick action cards row -->
    <div class="mep-quick-actions">
      <div class="mep-challenge-card">
        <div class="mep-cc-left">
          <div class="mep-cc-icon">🎯</div>
          <div>
            <div class="mep-cc-title">MONTHLY COMBINED EXAM</div>
            <div class="mep-cc-sub">Full syllabus · Multi-subject · Official scoring</div>
          </div>
        </div>
        <RouterLink to="/app/competition/monthly-test" class="mep-cc-btn">Generate Now</RouterLink>
      </div>
      <div class="mep-challenge-card" style="background:linear-gradient(135deg,rgba(245,158,11,0.08),rgba(239,68,68,0.06));">
        <div class="mep-cc-left">
          <div class="mep-cc-icon">⚔️</div>
          <div>
            <div class="mep-cc-title">CHALLENGE QUIZ</div>
            <div class="mep-cc-sub">Timed · Competitive · Earn leaderboard points</div>
          </div>
        </div>
        <RouterLink to="/app/competition/challenge" class="mep-cc-btn" style="background:linear-gradient(135deg,#f59e0b,#ef4444)">Start Challenge</RouterLink>
      </div>
      <div class="mep-challenge-card" style="background:linear-gradient(135deg,rgba(76,175,80,0.08),rgba(0,188,212,0.06));">
        <div class="mep-cc-left">
          <div class="mep-cc-icon">📅</div>
          <div>
            <div class="mep-cc-title">WEEKLY QUIZ</div>
            <div class="mep-cc-sub">Subject-wise · New every Monday · Build streaks</div>
          </div>
        </div>
        <RouterLink to="/app/competition/weekly" class="mep-cc-btn" style="background:linear-gradient(135deg,#4caf50,#00bcd4)">Take Quiz</RouterLink>
      </div>
      <div class="mep-challenge-card" style="background:linear-gradient(135deg,rgba(245,158,11,0.12),rgba(109,84,232,0.06));">
        <div class="mep-cc-left">
          <div class="mep-cc-icon">🏆</div>
          <div>
            <div class="mep-cc-title">STUDENT LEADERBOARD</div>
            <div class="mep-cc-sub">Top rankers · Badges · Weekly standings</div>
          </div>
        </div>
        <RouterLink to="/app/competition/leaderboard" class="mep-cc-btn" style="background:linear-gradient(135deg,#f59e0b,#d97706)">View Rankings</RouterLink>
      </div>
    </div>

    <!-- Official Institute Monthly Tests -->
    <div class="mep-table-card">
      <div class="mep-table-header">
        <div class="mep-table-title">Official Institute Monthly Tests</div>
        <div class="mep-table-sub">From Teacher / Institute</div>
      </div>
      <div class="mep-table-wrap">
        <table class="mep-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Test Title</th>
              <th>Opening Date</th>
              <th>Closing Date</th>
              <th>Status</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(t, i) in instituteTests" :key="t.id">
              <td>{{ i + 1 }}</td>
              <td>{{ t.title }}</td>
              <td>{{ formatDate(t.opens) }}</td>
              <td>{{ formatDate(t.closes) }}</td>
              <td>
                <span :class="['mep-status', statusCss(t)]">{{ statusLabel(t) }}</span>
              </td>
              <td>
                <RouterLink v-if="!t.taken && isAvailable(t)" :to="`/app/competition/monthly-test`" class="btn-attempt">ATTEMPT</RouterLink>
                <RouterLink v-else-if="t.taken" :to="`/app/competition/result/${t.id}`" class="btn-view">VIEW DETAILS</RouterLink>
                <span v-else class="status-notq">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import StatCards from '@/components/platform/StatCards.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const instituteTests = ref([
  { id: 1, title: 'June Monthly Combined Test — All Subjects', opens: '2026-06-01', closes: '2026-06-30', taken: false },
  { id: 2, title: 'May Science Test — Biology, Chemistry, Physics', opens: '2026-05-01', closes: '2026-05-31', taken: true },
  { id: 3, title: 'April Urdu + Pakistan Studies', opens: '2026-04-01', closes: '2026-04-30', taken: false },
])

const takenCount = computed(() => instituteTests.value.filter(t => t.taken).length)
const pendingCount = computed(() => instituteTests.value.filter(t => !t.taken && isAvailable(t)).length)
const expiredCount = computed(() => instituteTests.value.filter(t => !t.taken && !isAvailable(t) && new Date() > new Date(t.closes)).length)

function isAvailable(t) {
  const now = new Date(); const o = new Date(t.opens); const c = new Date(t.closes)
  return now >= o && now <= c
}
function statusLabel(t) {
  if (t.taken) return 'Taken'
  if (isAvailable(t)) return 'Opening'
  if (new Date() > new Date(t.closes)) return 'Expired-Waiting'
  return 'Upcoming'
}
function statusCss(t) {
  if (t.taken) return 'mep-taken'
  if (isAvailable(t)) return 'mep-opening'
  if (new Date() > new Date(t.closes)) return 'mep-expired'
  return 'mep-upcoming'
}
function formatDate(iso) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: 'numeric' }).replace(/\//g, '-')
}
</script>

<style scoped>
.dph-analytics-row { display: grid; grid-template-columns: repeat(5, 1fr); gap: 0.75rem; }
@media (max-width: 640px) { .dph-analytics-row { grid-template-columns: repeat(3, 1fr); } }
.dph-stat { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; padding: 0.9rem 1rem; text-align: center; }
.dph-stat-val { font-size: 1.35rem; font-weight: 800; color: var(--t-text1); }
.dph-stat-lbl { font-size: 0.7rem; color: var(--t-text3); margin-top: 0.2rem; }

.mep-quick-actions { display: flex; flex-direction: column; gap: 0.75rem; }
.mep-challenge-card {
  display: flex; align-items: center; justify-content: space-between; gap: 1rem; flex-wrap: wrap;
  padding: 1.25rem 1.5rem; border-radius: 14px;
  background: linear-gradient(135deg, #1a0533 0%, #3d1a6e 100%);
  border: 1px solid rgba(156, 39, 176, 0.3);
}
.mep-cc-left { display: flex; align-items: center; gap: 1rem; }
.mep-cc-icon { width: 48px; height: 48px; border-radius: 12px; background: rgba(156,39,176,0.2); display: flex; align-items: center; justify-content: center; font-size: 1.5rem; flex-shrink: 0; }
.mep-cc-title { font-size: 0.9rem; font-weight: 800; color: #fff; letter-spacing: 0.02em; }
.mep-cc-sub { font-size: 0.75rem; color: rgba(255,255,255,0.5); margin-top: 0.2rem; }
.mep-cc-btn { display: inline-block; padding: 0.6rem 1.5rem; border-radius: 9px; font-weight: 700; font-size: 0.85rem; background: #9c27b0; color: white; text-decoration: none; white-space: nowrap; flex-shrink: 0; }

.mep-table-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.mep-table-header { padding: 1rem 1.25rem; border-bottom: 1px solid var(--t-border); }
.mep-table-title { font-size: 0.95rem; font-weight: 700; color: var(--t-text1); }
.mep-table-sub { font-size: 0.75rem; color: var(--t-text3); margin-top: 0.15rem; }
.mep-table-wrap { overflow-x: auto; }
.mep-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.mep-table thead th { background: #4caf50; color: white; padding: 0.75rem 1rem; text-align: left; font-weight: 600; font-size: 0.8rem; white-space: nowrap; }
.mep-table td { padding: 0.8rem 1rem; color: var(--t-text1); border-bottom: 1px solid var(--t-border); }
.mep-table tr:hover td { background: var(--t-hover); }
.mep-status { display: inline-block; padding: 0.2rem 0.6rem; border-radius: 99px; font-size: 0.72rem; font-weight: 700; }
.mep-opening { background: #e8f5e9; color: #2e7d32; }
.mep-taken   { background: #e3f2fd; color: #1565c0; }
.mep-expired { background: #ffebee; color: #c62828; }
.mep-upcoming{ background: #fff8e1; color: #e65100; }
html.dark .mep-opening { background: rgba(46,125,50,0.2); color: #a5d6a7; }
html.dark .mep-taken   { background: rgba(21,101,192,0.2); color: #90caf9; }
html.dark .mep-expired { background: rgba(183,28,28,0.2); color: #ef9a9a; }
html.dark .mep-upcoming{ background: rgba(230,81,0,0.2); color: #ffb74d; }
.btn-attempt { display: inline-block; padding: 0.35rem 0.9rem; border-radius: 6px; font-size: 0.75rem; font-weight: 700; background: #4caf50; color: white; text-decoration: none; }
.btn-view { display: inline-block; padding: 0.35rem 0.9rem; border-radius: 6px; font-size: 0.75rem; font-weight: 700; background: #00bcd4; color: white; text-decoration: none; }
.status-notq { color: var(--t-text3); font-size: 0.8rem; }
</style>
