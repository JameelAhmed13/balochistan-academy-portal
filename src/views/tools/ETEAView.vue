<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">ETEA Preparation</h2>
    <div class="etea-intro">Educational Testing and Evaluation Agency (ETEA) preparation for KPK engineering, medical, and other competitive exams.</div>
    <div class="etea-grid">
      <RouterLink v-for="sub in subjects" :key="sub.name" :to="`/app/preparation/${sub.id}`" class="etea-card">
        <div class="etea-icon">{{ sub.icon }}</div>
        <div class="etea-name">{{ sub.name }}</div>
        <div class="etea-q">{{ sub.questions }} MCQs</div>
        <div class="etea-link">Start Practice →</div>
      </RouterLink>
    </div>
    <div class="etea-banner">
      <div class="etea-banner-info">
        <div class="etea-banner-title">📝 ETEA Mock Exam</div>
        <div class="etea-banner-sub">Full syllabus · 100 MCQs · 90 minutes</div>
      </div>
      <RouterLink to="/app/daily-tests/combined" class="etea-start-btn">Start Mock Exam</RouterLink>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import PageFooter from '@/components/platform/PageFooter.vue'

const subjects = [
  { id: 7, name: 'Mathematics', icon: '📐', questions: 40 },
  { id: 6, name: 'Physics',     icon: '⚡', questions: 30 },
  { id: 5, name: 'Chemistry',   icon: '🧪', questions: 20 },
  { id: 3, name: 'English',     icon: '🔤', questions: 10 },
]
</script>

<style scoped>
.etea-intro { font-size: 0.875rem; color: var(--t-text2); padding: 0.75rem 1rem; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; }
.etea-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; }
@media (max-width: 640px) { .etea-grid { grid-template-columns: repeat(2, 1fr); } }
.etea-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem; text-decoration: none; text-align: center; display: flex; flex-direction: column; gap: 0.35rem; transition: transform 0.15s; }
.etea-card:hover { transform: translateY(-2px); }
.etea-icon { font-size: 2rem; }
.etea-name { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); }
.etea-q { font-size: 0.72rem; color: var(--t-text3); }
.etea-link { font-size: 0.78rem; font-weight: 700; color: #9c27b0; }
.etea-banner { display: flex; align-items: center; justify-content: space-between; gap: 1rem; padding: 1.25rem 1.5rem; border-radius: 14px; background: linear-gradient(135deg, #1a0533, #3d1a6e); flex-wrap: wrap; }
.etea-banner-title { font-size: 0.95rem; font-weight: 800; color: #fff; }
.etea-banner-sub { font-size: 0.75rem; color: rgba(255,255,255,0.55); margin-top: 0.2rem; }
.etea-start-btn { padding: 0.6rem 1.5rem; border-radius: 9px; background: #9c27b0; color: white; font-weight: 700; font-size: 0.875rem; text-decoration: none; }
</style>
