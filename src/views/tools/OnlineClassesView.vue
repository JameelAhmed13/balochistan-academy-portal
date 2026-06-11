<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Online Classes</h2>
    <div class="oc-upcoming">
      <div class="oc-up-title">📅 Upcoming Live Sessions</div>
      <div class="oc-session-list">
        <div v-for="s in sessions" :key="s.id" class="oc-session">
          <div class="oc-session-time">{{ s.time }}</div>
          <div class="oc-session-info">
            <div class="oc-session-title">{{ s.title }}</div>
            <div class="oc-session-teacher">{{ s.teacher }}</div>
          </div>
          <div class="oc-session-status" :class="s.live ? 'oc-live' : 'oc-scheduled'">{{ s.live ? '🔴 LIVE' : 'Scheduled' }}</div>
          <button v-if="s.live" @click="joinSession(s)" class="oc-join-btn">Join Now</button>
          <button v-else class="oc-remind-btn">Set Reminder</button>
        </div>
      </div>
    </div>
    <div class="oc-recordings">
      <div class="oc-rec-title">🎬 Recorded Sessions</div>
      <div class="oc-rec-grid">
        <div v-for="r in recordings" :key="r.id" class="oc-rec-card">
          <div class="oc-rec-icon">{{ r.icon }}</div>
          <div class="oc-rec-info">
            <div class="oc-rec-title-sm">{{ r.title }}</div>
            <div class="oc-rec-meta">{{ r.teacher }} · {{ r.duration }}</div>
          </div>
          <div class="oc-rec-views">{{ r.views }} views</div>
        </div>
      </div>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import PageFooter from '@/components/platform/PageFooter.vue'

const sessions = [
  { id: 1, time: 'Today, 3:00 PM', title: 'Mathematics Chapter 5 — Factorization', teacher: 'Sir Ahmed', live: true },
  { id: 2, time: 'Today, 5:00 PM', title: 'Physics — Force and Motion', teacher: 'Sir Nawaz', live: false },
  { id: 3, time: 'Tomorrow, 10:00 AM', title: 'Chemistry — Periodic Table', teacher: 'Ma\'am Farah', live: false },
  { id: 4, time: 'Jun 12, 2:00 PM', title: 'Biology — Cell Division', teacher: 'Sir Tariq', live: false },
]

const recordings = [
  { id: 1, title: 'Algebra Basics', teacher: 'Sir Ahmed', duration: '1h 20min', views: '1.2k', icon: '📐' },
  { id: 2, title: 'Newton\'s Laws', teacher: 'Sir Nawaz', duration: '55min', views: '986', icon: '⚡' },
  { id: 3, title: 'Organic Chemistry Intro', teacher: 'Ma\'am Farah', duration: '1h 5min', views: '743', icon: '🧪' },
]

function joinSession(s) { alert('Joining: ' + s.title + '\n(Live session feature requires institute integration)') }
</script>

<style scoped>
.oc-upcoming, .oc-recordings { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.oc-up-title, .oc-rec-title { padding: 0.85rem 1.25rem; font-size: 0.875rem; font-weight: 700; color: var(--t-text1); border-bottom: 1px solid var(--t-border); }
.oc-session { display: flex; align-items: center; gap: 0.75rem; padding: 0.85rem 1.25rem; border-bottom: 1px solid var(--t-border); flex-wrap: wrap; }
.oc-session:last-child { border-bottom: none; }
.oc-session-time { font-size: 0.72rem; color: var(--t-text3); width: 120px; flex-shrink: 0; }
.oc-session-info { flex: 1; min-width: 160px; }
.oc-session-title { font-size: 0.82rem; font-weight: 700; color: var(--t-text1); }
.oc-session-teacher { font-size: 0.72rem; color: var(--t-text3); }
.oc-session-status { font-size: 0.72rem; font-weight: 700; }
.oc-live { color: #ef5350; }
.oc-scheduled { color: var(--t-text3); }
.oc-join-btn { padding: 0.35rem 0.85rem; border-radius: 6px; background: #ef5350; color: white; border: none; font-size: 0.75rem; font-weight: 700; cursor: pointer; }
.oc-remind-btn { padding: 0.35rem 0.85rem; border-radius: 6px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.75rem; cursor: pointer; }
.oc-rec-grid { padding: 0.75rem; display: flex; flex-direction: column; gap: 0.5rem; }
.oc-rec-card { display: flex; align-items: center; gap: 0.75rem; padding: 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; }
.oc-rec-icon { font-size: 1.5rem; }
.oc-rec-info { flex: 1; }
.oc-rec-title-sm { font-size: 0.82rem; font-weight: 600; color: var(--t-text1); }
.oc-rec-meta { font-size: 0.7rem; color: var(--t-text3); }
.oc-rec-views { font-size: 0.7rem; color: var(--t-text3); }
</style>
