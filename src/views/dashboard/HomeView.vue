<template>
  <div class="space-y-6 animate-fade-in">
    <!-- KPI Strip -->
    <div class="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-7 gap-3">
      <KpiCard v-for="k in kpis" :key="k.label" v-bind="k" />
    </div>

    <!-- Promo Banner -->
    <div class="rounded-2xl bg-gradient-to-r from-amber-500 via-orange-500 to-rose-500 p-4 md:p-5 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-glow-amber">
      <div class="text-center sm:text-left">
        <div class="text-white font-bold text-lg">🎓 Full Access — Only Rs. 999/year</div>
        <div class="text-white/80 text-sm mt-0.5">Unlock all AI features, unlimited tests, and coin rewards</div>
      </div>
      <button type="button" class="btn-secondary text-amber-700 font-semibold shrink-0">Activate Now</button>
    </div>

    <!-- Getting Started Guide (shown only to new users) -->
    <div v-if="student.totalTests === 0" class="gs-guide" role="region" aria-label="Getting started guide">
      <div class="gs-header">
        <div class="gs-header-icon" aria-hidden="true">🚀</div>
        <div>
          <div class="gs-header-title">Welcome to Balochistan Academy Portal! Here's how to get started:</div>
          <div class="gs-header-sub">Complete these steps to unlock your full potential</div>
        </div>
      </div>
      <div class="gs-steps">
        <RouterLink v-for="step in startSteps" :key="step.num" :to="step.to" class="gs-step">
          <div class="gs-step-num" aria-hidden="true">{{ step.num }}</div>
          <div class="gs-step-icon" aria-hidden="true">{{ step.icon }}</div>
          <div>
            <div class="gs-step-title">{{ step.title }}</div>
            <div class="gs-step-desc">{{ step.desc }}</div>
          </div>
          <div class="gs-step-arrow" aria-hidden="true">→</div>
        </RouterLink>
      </div>
    </div>

    <!-- Action Cards Grid -->
    <div>
      <h2 class="section-title mb-4">Quick Actions</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        <ActionCard v-for="card in actionCards" :key="card.title" v-bind="card" />
      </div>
    </div>

    <!-- Performance Chart -->
    <div class="card p-5">
      <div class="flex items-center justify-between mb-4">
        <h2 class="section-title">Performance Trend</h2>
        <span class="badge-indigo badge">Last 20 Tests</span>
      </div>
      <div v-if="student.testRecords.length">
        <apexchart type="area" height="220" :options="chartOptions" :series="student.chartData.series" />
      </div>
      <div v-else class="flex flex-col items-center justify-center py-16 text-slate-400">
        <TrendingUp class="w-12 h-12 mb-3 opacity-30" />
        <p class="text-sm">No test records yet. Start a test to see your performance chart!</p>
        <RouterLink to="/app/preparation" class="btn-primary mt-3 text-sm">Start Preparation</RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { TrendingUp } from '@lucide/vue'
import { useStudentStore } from '@/stores/student'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'
import KpiCard from '@/components/ui/KpiCard.vue'
import ActionCard from '@/components/ui/ActionCard.vue'

const theme = useThemeStore()

const student = useStudentStore()
const auth = useAuthStore()

const kpis = computed(() => [
  { label: 'Total Tests',   value: student.totalTests,   icon: '📝', color: 'blue' },
  { label: 'Passed',        value: student.passedTests,  icon: '✅', color: 'green' },
  { label: 'Avg Score',     value: student.avgPercent + '%', icon: '📊', color: 'purple' },
  { label: 'This Month',    value: student.monthlyTests, icon: '📅', color: 'amber' },
  { label: 'Parent Tests',  value: 0,                    icon: '👨‍👩‍👧', color: 'rose' },
  { label: 'Performance',   value: student.avgPercent + '%', icon: '🏆', color: 'teal' },
  { label: 'Coins',         value: auth.user?.coins || 0, icon: '🪙', color: 'amber' },
])

const actionCards = [
  { title: 'Full Syllabus',      subtitle: 'Prep all subjects',      grad: 'grad-violet',  icon: '📚', to: '/app/preparation',  sub: [] },
  { title: 'Self Test',          subtitle: 'Objective / Subjective', grad: 'grad-blue',    icon: '✍️', to: null,                sub: [
    { label: 'Objective Test', to: '/app/preparation' },
    { label: 'Subjective Test', to: '/app/preparation' },
  ]},
  { title: 'Parent/Teacher Test',subtitle: 'Assigned assessments',   grad: 'grad-emerald', icon: '👨‍🏫', to: '/app/daily-tests',  sub: [] },
  { title: 'Test Records',       subtitle: 'Results & Follow-ups',   grad: 'grad-rose',    icon: '📋', to: null,                sub: [
    { label: 'Self Tests', to: '/app/reports' },
    { label: 'Parent Tests Follow-up', to: '/app/reports' },
  ]},
  { title: 'Daily Test',         subtitle: 'Today\'s practice',      grad: 'grad-amber',   icon: '📅', to: '/app/daily-tests',  sub: [] },
  { title: 'Monthly Test',       subtitle: 'Monthly challenge',      grad: 'grad-teal',    icon: '🏆', to: '/app/competition',  sub: [] },
  { title: 'Result Reports',     subtitle: 'Charts & analysis',      grad: 'grad-pink',    icon: '📈', to: null,                sub: [
    { label: 'Parent Test Result', to: '/app/reports' },
    { label: 'General Test Result', to: '/app/reports' },
    { label: 'Result in Graphs', to: '/app/reports' },
  ]},
  { title: 'Complaints',         subtitle: 'Suggestions & feedback', grad: 'grad-orange',  icon: '💬', to: '/app/complaints',   sub: [] },
]

const startSteps = [
  { num: 1, icon: '📚', title: 'Browse Your Subjects', desc: 'Go to Preparation to see all subject chapters', to: '/app/preparation' },
  { num: 2, icon: '✍️', title: 'Take a Self Test', desc: 'Test yourself on any subject or chapter', to: '/app/self-test' },
  { num: 3, icon: '🎬', title: 'Watch Video Lectures', desc: 'Learn from expert teachers for free', to: '/app/video' },
  { num: 4, icon: '🤖', title: 'Ask the AI Tutor', desc: 'Get instant explanations for anything', to: '/app/ai-tutor' },
]

const chartOptions = computed(() => {
  const isDark = theme.isDark
  return {
    chart: { toolbar: { show: false }, background: 'transparent' },
    theme: { mode: isDark ? 'dark' : 'light' },
    stroke: { curve: 'smooth', width: 2.5 },
    fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.35, opacityTo: 0.04, stops: [0, 90, 100] } },
    colors: ['#7c6af5'],
    grid: { borderColor: isDark ? 'rgba(255,255,255,0.06)' : '#e2e8f0', strokeDashArray: 4 },
    xaxis: {
      categories: student.chartData.labels,
      labels: { style: { fontSize: '11px', colors: isDark ? '#64748b' : '#94a3b8' } },
      axisBorder: { show: false }, axisTicks: { show: false },
    },
    yaxis: {
      min: 0, max: 100,
      labels: { formatter: v => v + '%', style: { fontSize: '11px', colors: isDark ? '#64748b' : '#94a3b8' } },
    },
    tooltip: {
      theme: isDark ? 'dark' : 'light',
      y: { formatter: v => v + '%' },
    },
    dataLabels: { enabled: false },
    markers: { size: 4, colors: ['#7c6af5'], strokeColors: isDark ? '#0d1526' : '#fff', strokeWidth: 2 },
  }
})
</script>

