<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Logarithm Table</h2>
    <div class="lt-controls">
      <label class="lt-ctrl-label">Type:</label>
      <button @click="tableType = 'log'" :class="['lt-type-btn', tableType === 'log' ? 'lt-type-active' : '']">Log (base 10)</button>
      <button @click="tableType = 'antilog'" :class="['lt-type-btn', tableType === 'antilog' ? 'lt-type-active' : '']">Antilog</button>
    </div>
    <div class="lt-calc-box">
      <div class="lt-calc-label">Quick Calculate</div>
      <div class="lt-calc-row">
        <span>{{ tableType === 'log' ? 'log₁₀(' : 'antilog(' }}</span>
        <input v-model.number="calcInput" type="number" step="0.01" min="0" class="lt-input" />
        <span>) =</span>
        <strong class="lt-calc-result">{{ calcResult }}</strong>
      </div>
    </div>
    <div class="lt-table-wrap">
      <table class="lt-table">
        <thead>
          <tr>
            <th>x</th>
            <th v-for="d in 10" :key="d-1">{{ d - 1 }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in tableRows" :key="row.x">
            <td class="lt-x">{{ row.x.toFixed(1) }}</td>
            <td v-for="(val, i) in row.values" :key="i">{{ val }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const tableType = ref('log')
const calcInput = ref(2)

const calcResult = computed(() => {
  if (!calcInput.value || calcInput.value <= 0) return '—'
  const v = tableType.value === 'log' ? Math.log10(calcInput.value) : Math.pow(10, calcInput.value)
  return v.toFixed(4)
})

const tableRows = computed(() => {
  const rows = []
  for (let x = 1.0; x <= 9.9; x = Math.round((x + 0.1) * 10) / 10) {
    const values = []
    for (let d = 0; d <= 9; d++) {
      const v = x + d * 0.01
      const result = tableType.value === 'log' ? Math.log10(v) : Math.pow(10, v)
      values.push(result.toFixed(4))
    }
    rows.push({ x, values })
  }
  return rows
})
</script>

<style scoped>
.lt-controls { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
.lt-ctrl-label { font-size: 0.82rem; font-weight: 600; color: var(--t-text2); }
.lt-type-btn { padding: 0.4rem 1rem; border-radius: 7px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.82rem; cursor: pointer; }
.lt-type-active { background: #4caf50; color: white; border-color: #4caf50; }
.lt-calc-box { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; padding: 1rem 1.25rem; }
.lt-calc-label { font-size: 0.75rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; margin-bottom: 0.5rem; }
.lt-calc-row { display: flex; align-items: center; gap: 0.5rem; font-size: 0.9rem; color: var(--t-text1); flex-wrap: wrap; }
.lt-input { width: 100px; padding: 0.4rem 0.6rem; border: 1px solid var(--t-border); border-radius: 6px; background: var(--t-bg); color: var(--t-text1); font-size: 0.9rem; }
.lt-calc-result { font-size: 1.15rem; color: #4caf50; }
.lt-table-wrap { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: auto; max-height: 60vh; }
.lt-table { width: 100%; border-collapse: collapse; font-size: 0.72rem; }
.lt-table thead th { background: #4caf50; color: white; padding: 0.5rem 0.4rem; text-align: center; position: sticky; top: 0; }
.lt-table td { padding: 0.35rem 0.4rem; text-align: center; border-bottom: 1px solid var(--t-border); color: var(--t-text2); }
.lt-x { font-weight: 700; color: var(--t-text1); background: var(--t-bg); position: sticky; left: 0; }
.lt-table tr:hover td { background: var(--t-hover); }
</style>
