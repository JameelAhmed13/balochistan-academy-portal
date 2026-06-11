<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">MDCAT Preparation</h2>
    <div class="mdcat-intro">Comprehensive MDCAT preparation — Biology, Chemistry, Physics, and English with past papers, mock tests, and subject review.</div>
    <div class="mdcat-grid">
      <RouterLink v-for="sub in subjects" :key="sub.name" :to="`/app/preparation/${sub.id}/test/objective?type=mdcat`" class="mdcat-card">
        <div class="mdcat-icon">{{ sub.icon }}</div>
        <div class="mdcat-name">{{ sub.name }}</div>
        <div class="mdcat-weight">{{ sub.weight }}% weightage</div>
        <div class="mdcat-q">{{ sub.questions }} MCQs</div>
      </RouterLink>
    </div>
    <div class="mdcat-banner">
      <div class="mdcat-banner-title">⏱ Full MDCAT Mock Test</div>
      <div class="mdcat-banner-sub">220 MCQs · 150 minutes · All subjects · Official syllabus</div>
      <RouterLink to="/app/daily-tests/combined" class="mdcat-start-btn">Start Mock Test</RouterLink>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import PageFooter from '@/components/platform/PageFooter.vue'

const subjects = [
  { id: 4,  name: 'Biology',   icon: '🧬', weight: 38, questions: 84 },
  { id: 5,  name: 'Chemistry', icon: '🧪', weight: 27, questions: 60 },
  { id: 6,  name: 'Physics',   icon: '⚡', weight: 22, questions: 48 },
  { id: 3,  name: 'English',   icon: '🔤', weight: 13, questions: 28 },
]
</script>

<style scoped>
.mdcat-intro { font-size: 0.875rem; color: var(--t-text2); padding: 0.75rem 1rem; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; }
.mdcat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; }
@media (max-width: 640px) { .mdcat-grid { grid-template-columns: repeat(2, 1fr); } }
.mdcat-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem; text-decoration: none; text-align: center; display: flex; flex-direction: column; gap: 0.35rem; transition: transform 0.15s; }
.mdcat-card:hover { transform: translateY(-2px); }
.mdcat-icon { font-size: 2rem; }
.mdcat-name { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); }
.mdcat-weight { font-size: 0.72rem; color: var(--t-text3); }
.mdcat-q { font-size: 0.75rem; font-weight: 700; color: #4caf50; }
.mdcat-banner { display: flex; align-items: center; justify-content: space-between; gap: 1rem; padding: 1.25rem 1.5rem; border-radius: 14px; background: linear-gradient(135deg, #0d1b2a, #1a3a5c); flex-wrap: wrap; }
.mdcat-banner-title { font-size: 0.95rem; font-weight: 800; color: #fff; }
.mdcat-banner-sub { font-size: 0.75rem; color: rgba(255,255,255,0.55); margin-top: 0.2rem; }
.mdcat-start-btn { padding: 0.6rem 1.5rem; border-radius: 9px; background: #4caf50; color: white; font-weight: 700; font-size: 0.875rem; text-decoration: none; }
</style>
