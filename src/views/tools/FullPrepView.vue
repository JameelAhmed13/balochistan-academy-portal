<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Full Preparation</h2>
    <div class="fp-intro">Complete your exam preparation with comprehensive subject coverage across all topics, units, and chapters.</div>
    <div class="fp-subject-grid">
      <RouterLink v-for="s in SUBJECTS" :key="s.id" :to="`/app/preparation/${s.id}`" class="fp-subject-card">
        <div class="fp-subject-icon">{{ s.icon }}</div>
        <div class="fp-subject-name">{{ s.name }}</div>
        <div class="fp-subject-meta">{{ s.units || 10 }} Units · {{ (s.units || 10) * 8 }} Topics</div>
        <div class="fp-start-btn">Start Prep →</div>
      </RouterLink>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { SUBJECTS } from '@/stores/content'
import PageFooter from '@/components/platform/PageFooter.vue'
</script>

<style scoped>
.fp-intro { font-size: 0.875rem; color: var(--t-text2); padding: 0.75rem 1rem; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; }
.fp-subject-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 1rem; }
.fp-subject-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem; text-decoration: none; display: flex; flex-direction: column; gap: 0.4rem; transition: transform 0.15s; }
.fp-subject-card:hover { transform: translateY(-2px); border-color: #4caf50; }
.fp-subject-icon { font-size: 2rem; }
.fp-subject-name { font-size: 0.9rem; font-weight: 700; color: var(--t-text1); }
.fp-subject-meta { font-size: 0.72rem; color: var(--t-text3); }
.fp-start-btn { font-size: 0.78rem; font-weight: 700; color: #4caf50; margin-top: 0.25rem; }
</style>
