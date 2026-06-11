<template>
  <div class="animate-fade-in ptt-page">
    <!-- Profile card (left) -->
    <div class="ptt-profile">
      <div class="ptt-avatar">👤</div>
      <div class="ptt-name">Balochistan Board Demo</div>
      <div class="ptt-detail">📍 Behria Town Phase IV Islamabad</div>
      <div class="ptt-detail">📱 03000000000</div>
      <div class="ptt-detail">✉️ balochistandemo@gmail.com</div>
      <div class="ptt-divider"/>
      <div class="ptt-detail">📅 Joined: 02/03/2025</div>
      <div class="ptt-detail">📅 Expiry: 03/02/2026</div>
      <img src="/src/assets/hero.png" alt="" class="ptt-banner-img" />
    </div>

    <!-- Test area (right) -->
    <div class="ptt-main">
      <div class="ptt-header">
        <div class="ptt-title">Parent Test</div>
        <div class="ptt-meta">
          <span class="ptt-count-badge">MCQs {{ questions.length }}</span>
          <span class="ptt-timer" :class="{ 'ptt-timer-low': timeLeft < 60 }">⏱ {{ formatTime(timeLeft) }} rem...</span>
        </div>
      </div>

      <!-- Palette (numbered) -->
      <div class="ptt-palette">
        <div v-for="(q, i) in questions" :key="i"
          @click="currentIdx = i"
          :class="['ptt-pal-btn', answers[i] !== undefined ? 'ptt-answered' : '', currentIdx === i ? 'ptt-current' : '']">
          {{ i + 1 }}
        </div>
      </div>

      <div class="ptt-subject-tag">{{ subjectName }}</div>

      <!-- Current question -->
      <div class="ptt-q-area">
        <div class="ptt-q-num">Question {{ currentIdx + 1 }}</div>
        <div class="ptt-q-text urdu" dir="rtl">{{ currentQ.stem }}</div>
        <div class="ptt-options" dir="rtl">
          <label v-for="(opt, oi) in currentQ.options" :key="oi" class="ptt-opt"
            :class="{ 'ptt-selected': answers[currentIdx] === oi }">
            <input type="radio" :name="'pq' + currentIdx" :value="oi"
              v-model="answers[currentIdx]" /> {{ opt }}
          </label>
        </div>
      </div>

      <div class="ptt-nav">
        <button @click="currentIdx = 0" :disabled="currentIdx === 0" class="ptt-nav-btn">« First</button>
        <button @click="go(-1)" :disabled="currentIdx === 0" class="ptt-nav-btn">‹ Prev</button>
        <button @click="go(1)" :disabled="currentIdx === questions.length - 1" class="ptt-nav-btn">Next ›</button>
        <button @click="currentIdx = questions.length - 1" :disabled="currentIdx === questions.length - 1" class="ptt-nav-btn">Last »</button>
        <button @click="finish" class="ptt-finish-btn">Finish Test</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { SUBJECTS, useContentStore } from '@/stores/content'

const route = useRoute()
const router = useRouter()
const content = useContentStore()

const testId = computed(() => route.params.id)
// Pick a subject based on test ID for demo
const SUBJECT_MAP = { pt1: 1, pt2: 10 }
const subjectId = computed(() => SUBJECT_MAP[testId.value] || 1)
const subject = computed(() => SUBJECTS.find(s => s.id === subjectId.value))
const subjectName = computed(() => (subject.value?.name || 'Urdu') + ' - ' + 9)

const questions = ref(content.getQuestions(subjectId.value, { limit: 13 }))
const currentIdx = ref(0)
const answers = ref({})
const timeLeft = ref(90 * 60)
let timer = null

const currentQ = computed(() => questions.value[currentIdx.value])

function go(dir) {
  const next = currentIdx.value + dir
  if (next >= 0 && next < questions.value.length) currentIdx.value = next
}
function formatTime(s) {
  const m = Math.floor(s / 60)
  const sec = s % 60
  return String(m).padStart(2, '0') + ':' + String(sec).padStart(2, '0')
}
function finish() {
  clearInterval(timer)
  router.push('/app/parent-tests')
}

onMounted(() => {
  timer = setInterval(() => {
    if (timeLeft.value > 0) timeLeft.value--
    else finish()
  }, 1000)
})
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.ptt-page { display: flex; gap: 1.5rem; align-items: flex-start; min-height: 80vh; }

.ptt-profile {
  width: 200px; flex-shrink: 0; background: var(--t-surface); border: 1px solid var(--t-border);
  border-radius: 14px; padding: 1.25rem; display: flex; flex-direction: column; gap: 0.4rem; align-items: center;
  text-align: center;
}
.ptt-avatar { font-size: 3rem; background: #ff7043; border-radius: 50%; width: 80px; height: 80px; display: flex; align-items: center; justify-content: center; }
.ptt-name { font-weight: 700; font-size: 0.9rem; color: var(--t-text1); margin-top: 0.5rem; }
.ptt-detail { font-size: 0.7rem; color: var(--t-text3); }
.ptt-divider { width: 100%; height: 1px; background: var(--t-border); margin: 0.3rem 0; }
.ptt-banner-img { width: 100%; border-radius: 8px; margin-top: 0.5rem; object-fit: cover; height: 60px; }

.ptt-main { flex: 1; min-width: 0; }
.ptt-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; flex-wrap: wrap; gap: 0.5rem; }
.ptt-title { font-size: 1.2rem; font-weight: 700; color: var(--t-text1); }
.ptt-meta { display: flex; align-items: center; gap: 1rem; }
.ptt-count-badge { padding: 0.3rem 0.8rem; border-radius: 6px; background: var(--t-surface); border: 1px solid var(--t-border); font-size: 0.82rem; font-weight: 600; color: var(--t-text1); }
.ptt-timer { font-size: 0.82rem; font-weight: 600; color: var(--t-text2); background: var(--t-surface); border: 1px solid var(--t-border); padding: 0.3rem 0.8rem; border-radius: 6px; font-variant-numeric: tabular-nums; }
.ptt-timer-low { color: #ef4444; border-color: rgba(239,68,68,0.5); animation: pttBlink 1s steps(2,start) infinite; }
@keyframes pttBlink { 50% { opacity: 0.55; } }

.ptt-palette { display: flex; flex-wrap: wrap; gap: 0.35rem; margin-bottom: 0.75rem; padding: 0.75rem; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; }
.ptt-pal-btn { width: 34px; height: 34px; border-radius: 6px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.78rem; font-weight: 700; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all 0.12s; }
.ptt-pal-btn:hover { border-color: color-mix(in srgb, var(--t-accent) 40%, transparent); color: var(--t-text1); }
.ptt-answered { background: #3b82f6 !important; color: #fff !important; border-color: #3b82f6 !important; }
.ptt-current { outline: 2px solid #f59e0b; outline-offset: 1px; }

.ptt-subject-tag { display: inline-block; padding: 0.2rem 0.8rem; border-radius: 6px; font-size: 0.75rem; font-weight: 700; background: #ffebee; color: #c62828; border: 1px solid #ef9a9a; margin-bottom: 0.75rem; }
html.dark .ptt-subject-tag { background: rgba(183,28,28,0.2); color: #ef9a9a; border-color: rgba(183,28,28,0.4); }

.ptt-q-area { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem 1.5rem; }
.ptt-q-num { font-size: 0.85rem; color: var(--t-text3); margin-bottom: 0.5rem; }
.ptt-q-text { font-size: 1rem; color: var(--t-text1); line-height: 1.7; margin-bottom: 1rem; }
.ptt-options { display: flex; flex-direction: column; gap: 0; }
.ptt-opt { display: flex; align-items: center; gap: 0.75rem; padding: 0.65rem 1rem; border-bottom: 1px solid var(--t-border); cursor: pointer; color: var(--t-text1); font-size: 0.9rem; transition: background 0.1s; }
.ptt-opt:hover { background: var(--t-hover); }
.ptt-selected { background: var(--t-acc-alpha-sm) !important; }

.ptt-nav { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-top: 1rem; align-items: center; }
.ptt-nav-btn { padding: 0.5rem 1rem; border-radius: 8px; font-size: 0.85rem; font-weight: 600; background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2); cursor: pointer; transition: all 0.12s; }
.ptt-nav-btn:hover:not(:disabled) { border-color: color-mix(in srgb, var(--t-accent) 45%, transparent); color: var(--t-text1); }
.ptt-nav-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.ptt-finish-btn { margin-left: auto; padding: 0.55rem 1.5rem; border-radius: 8px; font-weight: 700; font-size: 0.9rem; background: #4caf50; color: white; border: none; cursor: pointer; transition: filter 0.12s; }
.ptt-finish-btn:hover { filter: brightness(1.08); }

@media (max-width: 640px) { .ptt-profile { display: none; } }
</style>
