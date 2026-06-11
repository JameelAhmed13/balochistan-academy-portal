<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">SLOs Mapping</h2>
    <div class="slo-intro">Student Learning Outcomes (SLOs) mapping aligns your test performance to the official curriculum objectives defined by the Pakistan National Curriculum.</div>
    <div class="slo-filters">
      <select v-model="selectedSubject" class="slo-sel">
        <option :value="null">Select Subject</option>
        <option v-for="s in SUBJECTS" :key="s.id" :value="s">{{ s.icon }} {{ s.name }}</option>
      </select>
    </div>
    <div v-if="selectedSubject" class="slo-table-wrap">
      <table class="slo-table">
        <thead><tr><th>#</th><th>Unit</th><th>Topic</th><th>Cognitive Level</th><th>Difficulty</th><th>Test Coverage</th></tr></thead>
        <tbody>
          <tr v-for="(slo, i) in slos" :key="i">
            <td>{{ i + 1 }}</td>
            <td>{{ slo.unit }}</td>
            <td>{{ slo.topic }}</td>
            <td><CognitiveBadge :level="slo.cognitive" /></td>
            <td><DifficultyBadge :level="slo.difficulty" /></td>
            <td><div class="slo-cov-bar"><div :style="{ width: slo.coverage + '%' }" class="slo-cov-fill" /></div></td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-else class="slo-empty">Select a subject to view its SLOs mapping.</div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { SUBJECTS } from '@/stores/content'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const selectedSubject = ref(null)
const cogLevels = ['Knowledge', 'Understanding', 'Applying']
const difficulties = ['Easy', 'Medium', 'Hard']

const slos = computed(() => {
  if (!selectedSubject.value) return []
  return Array.from({ length: 20 }, (_, i) => ({
    unit: `Unit ${Math.floor(i / 4) + 1}`,
    topic: `Topic ${(i % 4) + 1}: ${selectedSubject.value.name} concept ${i + 1}`,
    cognitive: cogLevels[i % 3],
    difficulty: difficulties[i % 3],
    coverage: Math.floor((i * 37 + 23) % 101),
  }))
})
</script>

<style scoped>
.slo-intro { font-size: 0.875rem; color: var(--t-text2); padding: 0.75rem 1rem; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; line-height: 1.6; }
.slo-filters { display: flex; gap: 0.5rem; }
.slo-sel { padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-surface); color: var(--t-text1); font-size: 0.875rem; }
.slo-table-wrap { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: auto; }
.slo-table { width: 100%; border-collapse: collapse; font-size: 0.82rem; }
.slo-table thead th { background: #4caf50; color: white; padding: 0.65rem 1rem; text-align: left; font-weight: 600; font-size: 0.78rem; }
.slo-table td { padding: 0.65rem 1rem; border-bottom: 1px solid var(--t-border); color: var(--t-text1); vertical-align: middle; }
.slo-cov-bar { height: 8px; background: var(--t-border); border-radius: 99px; min-width: 80px; }
.slo-cov-fill { height: 100%; border-radius: 99px; background: #4caf50; }
.slo-empty { text-align: center; padding: 3rem; color: var(--t-text3); }
</style>
