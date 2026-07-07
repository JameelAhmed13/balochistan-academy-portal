<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📊</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Platform Analytics</div>
        <div class="ag-banner-sub">Real-time platform insights and performance metrics</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat" style="background:rgba(16,185,129,0.12);border-color:rgba(16,185,129,0.25);color:#34d399;">
          <span style="width:7px;height:7px;border-radius:50%;background:#34d399;display:inline-block;animation:pulse-dot 1.5s ease-in-out infinite;"></span>
          Live
        </span>
      </div>
      <RouterLink to="/app/admin" class="btn-ghost" style="cursor:pointer;">
        <ArrowLeft class="w-4 h-4" />
      </RouterLink>
    </div>

    <!-- 6 KPI Cards -->
    <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
      <KpiCard v-for="kpi in kpiCards" :key="kpi.label" :label="kpi.label" :value="kpi.value" :icon="kpi.icon" :color="kpi.color" />
    </div>

    <!-- Charts Row 1: Subject Popularity (full width) -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <TrendingUp class="w-3.5 h-3.5" style="color:var(--t-accent);" />
        Subject Popularity — Tests Attempted
      </div>
      <div class="ag-card-body">
        <div v-if="!loading && !analytics.subjectPopularity.length" class="ag-empty" style="padding:24px;"><p>No test attempts yet.</p></div>
        <apexchart v-else type="bar" height="260" :options="subjectOptions" :series="subjectSeries" />
      </div>
    </div>

    <!-- Charts Row 2: Area (60%) + Donut (40%) -->
    <div class="charts-row">
      <!-- Monthly Activity Trend -->
      <div class="ag-card" style="flex: 3;">
        <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
          <TrendingUp class="w-3.5 h-3.5" style="color:var(--t-accent);" />
          Monthly Activity Trend
        </div>
        <div class="ag-card-body">
          <apexchart type="area" height="240" :options="trendOptions" :series="trendSeries" />
        </div>
      </div>
      <!-- Test Types Donut -->
      <div class="ag-card" style="flex: 2;">
        <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
          <BookOpen class="w-3.5 h-3.5" style="color:var(--t-accent);" />
          Test Type Distribution
        </div>
        <div class="ag-card-body">
          <div v-if="!loading && !analytics.testTypeDistribution.length" class="ag-empty" style="padding:24px;"><p>No test attempts yet.</p></div>
          <apexchart v-else type="donut" height="240" :options="donutOptions" :series="donutSeries" />
        </div>
      </div>
    </div>

    <!-- Top Students Leaderboard -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <Users class="w-3.5 h-3.5" style="color:var(--t-accent);" />
        Top Students Leaderboard
      </div>
      <div v-if="loading" class="ag-empty" style="padding:40px;"><div class="ag-empty-icon">⏳</div><p>Loading…</p></div>
      <div v-else-if="!topStudents.length" class="ag-empty" style="padding:40px;"><p>No students have earned coins yet.</p></div>
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>Rank</th>
              <th>Student</th>
              <th>Tests</th>
              <th>Avg Score</th>
              <th>Coins</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(s, i) in topStudents" :key="s.name">
              <td>
                <span v-if="i < 3" style="font-size:1.2rem;line-height:1;">{{ ['🥇','🥈','🥉'][i] }}</span>
                <span v-else style="font-weight:700;color:var(--t-text3);">{{ i + 1 }}</span>
              </td>
              <td>
                <div style="display:flex;align-items:center;gap:0.6rem;">
                  <div class="ag-avatar" :style="avatarGrad(s.name)">{{ initials(s.name) }}</div>
                  <span style="font-weight:500;color:var(--t-text1);">{{ s.name }}</span>
                </div>
              </td>
              <td style="color:var(--t-text2);">{{ s.tests }}</td>
              <td>
                <span :class="s.avg >= 70 ? 'badge-green' : s.avg >= 50 ? 'badge-amber' : 'badge-red'">{{ s.avg }}%</span>
              </td>
              <td style="color:var(--t-gold);font-weight:700;">{{ s.coins }} 🪙</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ArrowLeft, TrendingUp, Users, BookOpen } from '@lucide/vue'
import KpiCard from '@/components/ui/KpiCard.vue'
import { useThemeStore } from '@/stores/theme'
import api from '@/services/api'

const theme = useThemeStore()
const loading = ref(false)

const analytics = ref({
  totalUsers: 0, totalAttempts: 0, aiSessions: 0, coinsIssued: 0, activeToday: 0, avgScore: 0,
  subjectPopularity: [], monthlyTrend: [], testTypeDistribution: [],
})
const topStudents = ref([])

function fmt(n) {
  const v = n ?? 0
  return v >= 10000 ? Math.round(v / 1000) + 'K' : v >= 1000 ? (v / 1000).toFixed(1) + 'K' : String(v)
}

async function loadAnalytics() {
  loading.value = true
  try {
    const [{ data: a }, { data: lb }] = await Promise.all([
      api.get('/admin/analytics'),
      api.get('/tests/leaderboard', { params: { top: 5 } }),
    ])
    analytics.value = a
    topStudents.value = lb.map(s => ({ name: s.name, tests: s.totalAttempts, avg: Math.round(s.avgPercent || 0), coins: s.coins }))
  } catch { /* leave defaults on failure */ }
  finally { loading.value = false }
}
onMounted(loadAnalytics)

const kpiCards = computed(() => [
  { label: 'Total Users',  value: fmt(analytics.value.totalUsers),    icon: '👥', color: 'blue'   },
  { label: 'Tests Taken',  value: fmt(analytics.value.totalAttempts), icon: '📝', color: 'purple' },
  { label: 'AI Sessions',  value: fmt(analytics.value.aiSessions),    icon: '🤖', color: 'green'  },
  { label: 'Coins Issued', value: fmt(analytics.value.coinsIssued),   icon: '🪙', color: 'amber'  },
  { label: 'Active Today', value: fmt(analytics.value.activeToday),  icon: '⚡', color: 'teal'   },
  { label: 'Avg Score',    value: `${analytics.value.avgScore}%`,     icon: '🎯', color: 'rose'   },
])

/* ── Chart helpers ── */
const gridBorderColor = computed(() =>
  theme.isDark ? 'rgba(255,255,255,0.06)' : '#e2e8f0'
)
const chartTheme = computed(() => ({ mode: theme.isDark ? 'dark' : 'light' }))
const chartBg    = 'transparent'
const PRIMARY    = '#7c6af5'

/* ── Subject Popularity ── */
const subjectOptions = computed(() => ({
  chart:       { toolbar: { show: false }, background: chartBg, theme: chartTheme.value },
  xaxis:       { categories: analytics.value.subjectPopularity.map(s => s.subject), labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  yaxis:       { labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  colors:      [PRIMARY, '#10b981', '#f59e0b', '#06b6d4', '#e11d48', '#a855f7', '#f97316', '#14b8a6'],
  dataLabels:  { enabled: false },
  grid:        { borderColor: gridBorderColor.value, strokeDashArray: 3 },
  plotOptions: { bar: { borderRadius: 6, columnWidth: '54%', distributed: true } },
  legend:      { show: false },
  tooltip:     { theme: theme.isDark ? 'dark' : 'light' },
}))
const subjectSeries = computed(() => [{ name: 'Tests', data: analytics.value.subjectPopularity.map(s => s.attempts) }])

/* ── Monthly Trend ── */
const trendOptions = computed(() => ({
  chart:      { toolbar: { show: false }, background: chartBg, theme: chartTheme.value },
  stroke:     { curve: 'smooth', width: 2.5 },
  fill:       { type: 'gradient', gradient: { opacityFrom: 0.28, opacityTo: 0.03, colorStops: [{ offset: 0, color: PRIMARY, opacity: 0.28 }, { offset: 100, color: PRIMARY, opacity: 0.03 }] } },
  colors:     [PRIMARY],
  xaxis:      { categories: analytics.value.monthlyTrend.map(m => m.month), labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  yaxis:      { labels: { formatter: v => v + ' tests', style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  grid:       { borderColor: gridBorderColor.value, strokeDashArray: 3 },
  dataLabels: { enabled: false },
  markers:    { size: 4, colors: [PRIMARY], strokeColors: theme.isDark ? '#0d1526' : '#fff', strokeWidth: 2 },
  tooltip:    { theme: theme.isDark ? 'dark' : 'light' },
}))
const trendSeries = computed(() => [{ name: 'Tests Taken', data: analytics.value.monthlyTrend.map(m => m.attempts) }])

/* ── Donut ── */
const TYPE_LABELS = {
  'self-test': 'Self Test', 'parent-test': 'Parent Test', challenge: 'Challenge',
  weekly: 'Weekly Quiz', monthly: 'Monthly Exam', daily: 'Daily Test',
  objective: 'Objective', subjective: 'Subjective',
}
const donutOptions = computed(() => ({
  chart:     { background: chartBg, theme: chartTheme.value },
  labels:    analytics.value.testTypeDistribution.map(t => TYPE_LABELS[t.type] ?? t.type),
  colors:    [PRIMARY, '#10b981', '#f59e0b', '#ef4444', '#06b6d4', '#a855f7', '#f97316', '#14b8a6'],
  legend:    { position: 'bottom', labels: { colors: theme.isDark ? '#94a3b8' : '#64748b' } },
  plotOptions: { pie: { donut: { size: '65%', labels: { show: true, total: { show: true, label: 'Total', color: theme.isDark ? '#94a3b8' : '#64748b' } } } } },
  dataLabels: { enabled: false },
  stroke:    { width: 0 },
  tooltip:   { theme: theme.isDark ? 'dark' : 'light' },
}))
const donutSeries = computed(() => analytics.value.testTypeDistribution.map(t => t.count))

const gradients = [
  'linear-gradient(135deg,#7c6af5,#5b43cc)',
  'linear-gradient(135deg,#10b981,#0891b2)',
  'linear-gradient(135deg,#f59e0b,#ea580c)',
  'linear-gradient(135deg,#e11d48,#db2777)',
  'linear-gradient(135deg,#0891b2,#6366f1)',
]
function initials(name) {
  return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase()
}
function avatarGrad(name) {
  return `background: ${gradients[name.charCodeAt(0) % gradients.length]};`
}
</script>

<style scoped>
.charts-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}
.charts-row > * { min-width: 0; }
@media (max-width: 767px) {
  .charts-row > * { flex: 1 1 100% !important; }
}

@keyframes pulse-dot {
  0%, 100% { opacity: 1; transform: scale(1); }
  50%       { opacity: 0.5; transform: scale(0.75); }
}
</style>
