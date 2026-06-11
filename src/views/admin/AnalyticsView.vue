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
      <KpiCard label="Total Users"   :value="48"    icon="👥"  color="blue"   />
      <KpiCard label="Tests Taken"   :value="312"   icon="📝"  color="purple" />
      <KpiCard label="AI Sessions"   :value="128"   icon="🤖"  color="green"  />
      <KpiCard label="Coins Issued"  :value="15200" icon="🪙"  color="amber"  />
      <KpiCard label="Active Today"  :value="18"    icon="⚡"  color="teal"   />
      <KpiCard label="Avg Score"     value="74%"    icon="🎯"  color="rose"   />
    </div>

    <!-- Charts Row 1: Subject Popularity (full width) -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <TrendingUp class="w-3.5 h-3.5" style="color:var(--t-accent);" />
        Subject Popularity — Tests Attempted
      </div>
      <div class="ag-card-body">
        <apexchart type="bar" height="260" :options="subjectOptions" :series="subjectSeries" />
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
          <apexchart type="donut" height="240" :options="donutOptions" :series="donutSeries" />
        </div>
      </div>
    </div>

    <!-- Top Students Leaderboard -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <Users class="w-3.5 h-3.5" style="color:var(--t-accent);" />
        Top Students Leaderboard
      </div>
      <div class="overflow-x-auto">
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
import { computed } from 'vue'
import { ArrowLeft, TrendingUp, Users, BookOpen } from '@lucide/vue'
import KpiCard from '@/components/ui/KpiCard.vue'
import { useThemeStore } from '@/stores/theme'

const theme = useThemeStore()

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
  xaxis:       { categories: ['Urdu', 'English', 'Biology', 'Chemistry', 'Physics', 'Math', 'CS'], labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  yaxis:       { labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  colors:      [PRIMARY, '#10b981', '#f59e0b', '#06b6d4', '#e11d48', '#a855f7', '#f97316'],
  dataLabels:  { enabled: false },
  grid:        { borderColor: gridBorderColor.value, strokeDashArray: 3 },
  plotOptions: { bar: { borderRadius: 6, columnWidth: '54%', distributed: true } },
  legend:      { show: false },
  tooltip:     { theme: theme.isDark ? 'dark' : 'light' },
}))
const subjectSeries = [{ name: 'Tests', data: [45, 38, 62, 55, 71, 48, 33] }]

/* ── Monthly Trend ── */
const trendOptions = computed(() => ({
  chart:      { toolbar: { show: false }, background: chartBg, theme: chartTheme.value },
  stroke:     { curve: 'smooth', width: 2.5 },
  fill:       { type: 'gradient', gradient: { opacityFrom: 0.28, opacityTo: 0.03, colorStops: [{ offset: 0, color: PRIMARY, opacity: 0.28 }, { offset: 100, color: PRIMARY, opacity: 0.03 }] } },
  colors:     [PRIMARY],
  xaxis:      { categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'], labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  yaxis:      { labels: { formatter: v => v + ' tests', style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  grid:       { borderColor: gridBorderColor.value, strokeDashArray: 3 },
  dataLabels: { enabled: false },
  markers:    { size: 4, colors: [PRIMARY], strokeColors: theme.isDark ? '#0d1526' : '#fff', strokeWidth: 2 },
  tooltip:    { theme: theme.isDark ? 'dark' : 'light' },
}))
const trendSeries = [{ name: 'Tests Taken', data: [18, 28, 35, 47, 60, 54] }]

/* ── Donut ── */
const donutOptions = computed(() => ({
  chart:     { background: chartBg, theme: chartTheme.value },
  labels:    ['Objective', 'Subjective', 'Combined', 'Past Papers'],
  colors:    [PRIMARY, '#10b981', '#f59e0b', '#ef4444'],
  legend:    { position: 'bottom', labels: { colors: theme.isDark ? '#94a3b8' : '#64748b' } },
  plotOptions: { pie: { donut: { size: '65%', labels: { show: true, total: { show: true, label: 'Total', color: theme.isDark ? '#94a3b8' : '#64748b' } } } } },
  dataLabels: { enabled: false },
  stroke:    { width: 0 },
  tooltip:   { theme: theme.isDark ? 'dark' : 'light' },
}))
const donutSeries = [145, 87, 56, 24]

/* ── Leaderboard ── */
const topStudents = [
  { name: 'Ahmed Khan',    tests: 28, avg: 87, coins: 1250 },
  { name: 'Fatima Ali',    tests: 24, avg: 82, coins: 980  },
  { name: 'Zainab Malik',  tests: 21, avg: 79, coins: 850  },
  { name: 'Bilal Hussain', tests: 18, avg: 74, coins: 720  },
  { name: 'Sara Qureshi',  tests: 15, avg: 68, coins: 560  },
]

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
