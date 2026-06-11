<template>
  <div class="animate-fade-in space-y-6">
    <!-- Tabs -->
    <div class="flex gap-2 border-b border-slate-200 pb-0">
      <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
        :class="['px-4 py-2.5 text-sm font-medium border-b-2 transition-colors -mb-px',
          activeTab === tab.id ? 'border-brand-600 text-brand-700' : 'border-transparent text-slate-500 hover:text-slate-700']"
      >{{ tab.label }}</button>
    </div>

    <!-- Self Tests -->
    <div v-if="activeTab === 'self'" class="space-y-4">
      <div class="card p-5">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold text-slate-700">Self Test Results</h3>
          <span class="badge-blue">{{ selfTests.length }} tests</span>
        </div>
        <table v-if="selfTests.length" class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-xs text-slate-500 uppercase tracking-wider">
              <th class="px-3 py-2 text-left">#</th>
              <th class="px-3 py-2 text-left">Subject</th>
              <th class="px-3 py-2 text-left">Type</th>
              <th class="px-3 py-2 text-left">Score</th>
              <th class="px-3 py-2 text-left">%</th>
              <th class="px-3 py-2 text-left">Date</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="(t, i) in selfTests" :key="t.id">
              <td class="px-3 py-2 text-slate-400">{{ i + 1 }}</td>
              <td class="px-3 py-2 font-medium text-slate-700">{{ t.subject }}</td>
              <td class="px-3 py-2"><span class="badge-indigo text-xs">{{ t.type }}</span></td>
              <td class="px-3 py-2">{{ t.score }}/{{ t.total }}</td>
              <td class="px-3 py-2"><span :class="perf(t) >= 50 ? 'badge-green' : 'badge-red'">{{ perf(t) }}%</span></td>
              <td class="px-3 py-2 text-xs text-slate-500">{{ fmt(t.date) }}</td>
            </tr>
          </tbody>
        </table>
        <div v-else class="text-center py-10 text-slate-400 text-sm">No self tests recorded yet.</div>
      </div>
    </div>

    <!-- Graphs -->
    <div v-if="activeTab === 'graphs'" class="space-y-5">
      <!-- Subject breakdown -->
      <div class="card p-5">
        <h3 class="font-semibold text-slate-700 mb-4">Subject-wise Performance</h3>
        <apexchart type="bar" height="280" :options="barOptions" :series="barSeries" />
      </div>
      <!-- Trend -->
      <div class="card p-5">
        <h3 class="font-semibold text-slate-700 mb-4">Score Trend Over Time</h3>
        <apexchart v-if="student.testRecords.length" type="line" height="250" :options="lineOptions" :series="student.chartData.series" />
        <div v-else class="text-center py-10 text-slate-400 text-sm">Take some tests to see your trend.</div>
      </div>
      <!-- Donut breakdown by type -->
      <div class="card p-5">
        <h3 class="font-semibold text-slate-700 mb-4">Test Types Distribution</h3>
        <apexchart type="donut" height="240" :options="donutOptions" :series="donutSeries" />
      </div>
    </div>

    <!-- Parent Tests -->
    <div v-if="activeTab === 'parent'" class="card p-10 text-center text-slate-400 text-sm">
      No parent-assigned test results found.
    </div>

    <!-- General Tests -->
    <div v-if="activeTab === 'general'" class="card p-10 text-center text-slate-400 text-sm">
      No general test results found.
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useStudentStore } from '@/stores/student'
import { SUBJECTS } from '@/stores/content'

const student = useStudentStore()
const activeTab = ref('self')

const tabs = [
  { id: 'self',    label: 'Self Test Results' },
  { id: 'parent',  label: 'Parent Test Result' },
  { id: 'general', label: 'General Test Result' },
  { id: 'graphs',  label: 'Result in Graphs' },
]

const selfTests = computed(() => student.testRecords)

function perf(t) { return Math.round((t.score / t.total) * 100) }
function fmt(iso) { return new Date(iso).toLocaleDateString('en-PK', { day: 'numeric', month: 'short' }) }

// Bar chart: subject-wise avg
const subjectStats = computed(() => {
  const map = {}
  SUBJECTS.forEach(s => { map[s.name] = [] })
  student.testRecords.forEach(t => { if (map[t.subject]) map[t.subject].push(perf(t)) })
  return SUBJECTS.map(s => ({ name: s.name, avg: map[s.name].length ? Math.round(map[s.name].reduce((a,b)=>a+b,0)/map[s.name].length) : 0 }))
})

const barOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent' },
  xaxis: { categories: subjectStats.value.map(s => s.name.slice(0,10)), labels: { style: { fontSize: '11px' } } },
  yaxis: { max: 100, labels: { formatter: v => v + '%' } },
  colors: ['#6366f1'],
  dataLabels: { enabled: false },
  grid: { borderColor: '#f1f5f9' },
  plotOptions: { bar: { borderRadius: 6 } },
}))
const barSeries = computed(() => [{ name: 'Avg Score', data: subjectStats.value.map(s => s.avg) }])

const lineOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent' },
  stroke: { curve: 'smooth', width: 2.5 },
  colors: ['#6366f1'],
  xaxis: { categories: student.chartData.labels, labels: { style: { fontSize: '11px' } } },
  yaxis: { min: 0, max: 100, labels: { formatter: v => v + '%' } },
  grid: { borderColor: '#f1f5f9' },
  markers: { size: 4 },
  dataLabels: { enabled: false },
}))

const typeMap = computed(() => {
  const m = {}
  student.testRecords.forEach(t => { m[t.type] = (m[t.type] || 0) + 1 })
  return m
})
const donutOptions = computed(() => ({
  chart: { background: 'transparent' },
  labels: Object.keys(typeMap.value),
  legend: { position: 'bottom' },
  colors: ['#6366f1','#10b981','#f59e0b','#ef4444','#8b5cf6'],
  dataLabels: { formatter: (v) => v.toFixed(0) + '%' },
}))
const donutSeries = computed(() => Object.values(typeMap.value))
</script>
