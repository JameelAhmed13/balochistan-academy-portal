<template>
  <div class="animate-fade-in" v-if="!loading && test">
    <!-- Top bar -->
    <div class="take-topbar">
      <button v-if="!started" @click="$router.go(-1)" class="btn-ghost"><ArrowLeft class="w-4 h-4" /></button>
      <div class="take-subject">
        <div>
          <div class="take-subj-name">{{ test.title }}</div>
          <div class="take-subj-grade">{{ test.gradeCode ? `Grade ${test.gradeCode}` : 'All grades' }} · Institute-scheduled {{ test.kind }} test</div>
        </div>
      </div>
      <div v-if="started && !finished" class="take-qinfo">Question {{ currentIdx + 1 }}/{{ questions.length }}</div>
    </div>

    <!-- Empty (no questions attached yet) -->
    <div v-if="!questions.length" class="take-start-card">
      <div class="text-4xl mb-2">📭</div>
      <p style="color:var(--t-text2)">This test doesn't have any questions attached yet — check back once your institute publishes them.</p>
    </div>

    <!-- Not started -->
    <div v-else-if="!started" class="take-start-card">
      <div class="text-5xl mb-3">📝</div>
      <h3 class="text-lg font-semibold" style="color:var(--t-text1)">Ready to start "{{ test.title }}"?</h3>
      <p class="text-sm mt-1 max-w-sm" style="color:var(--t-text3)">
        {{ questions.length }} questions · {{ test.durationMin }} minutes · Answers hidden until you submit
      </p>
      <button @click="startTest" class="btn-primary mt-5 px-8 py-3 text-base gap-2">
        <Play class="w-5 h-5" /> Start Test
      </button>
    </div>

    <!-- In progress -->
    <div v-else-if="!finished" class="take-body">
      <div class="take-main" ref="testEl">
        <div class="take-q-header">
          <DifficultyBadge :level="currentQ.difficulty" />
        </div>
        <div class="take-q-text" v-html="prepareMath(currentQ.stem)" />
        <div class="take-options">
          <label v-for="(opt, oi) in currentQ.options" :key="oi"
            class="take-opt" :class="{ selected: answers[currentIdx] === oi }">
            <input type="radio" :name="'q' + currentIdx" :value="oi"
              v-model="answers[currentIdx]" class="take-radio" />
            {{ ['A','B','C','D'][oi] }}. <span v-html="prepareMath(opt)" />
          </label>
        </div>
        <div class="take-nav">
          <button type="button" @click="go('first')" :disabled="currentIdx === 0" class="nav-btn">FIRST</button>
          <button type="button" @click="go('prev')"  :disabled="currentIdx === 0" class="nav-btn">PREVIOUS</button>
          <button type="button" @click="go('next')"  :disabled="currentIdx === questions.length - 1" class="nav-btn">NEXT</button>
          <button type="button" @click="go('last')"  :disabled="currentIdx === questions.length - 1" class="nav-btn">LAST</button>
          <button type="button" @click="finishTest" class="nav-btn nav-finish">FINISH TEST</button>
        </div>
      </div>

      <div class="take-sidebar">
        <div class="palette-section">
          <div class="palette-title">Questions</div>
          <div class="palette-grid">
            <button v-for="(q, i) in questions" :key="i"
              type="button"
              @click="currentIdx = i"
              :class="['pal-btn', answers[i] !== undefined ? 'pal-answered' : '', currentIdx === i ? 'pal-current' : '']">
              {{ i + 1 }}
            </button>
          </div>
        </div>
        <div class="timer-section">
          <div class="timer-lbl">Time Remaining</div>
          <div class="timer-val" :class="timeLeft < 60 ? 'timer-urgent' : ''">{{ formatTime(timeLeft) }}</div>
        </div>
      </div>
    </div>

    <!-- Saving -->
    <div v-else class="take-start-card">
      <div class="text-4xl mb-2">⏳</div>
      <p style="color:var(--t-text2)">Saving your results…</p>
    </div>
  </div>
  <div v-else-if="loading" class="take-start-card">
    <div class="text-4xl mb-2">⏳</div>
    <p style="color:var(--t-text2)">Loading test…</p>
  </div>
  <div v-else class="take-start-card">
    <div class="text-4xl mb-2">❌</div>
    <p style="color:var(--t-text2)">This test couldn't be found — it may have been unpublished.</p>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'
import { ArrowLeft, Play } from '@lucide/vue'
import api from '@/services/api'
import { useStudentStore } from '@/stores/student'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'

// Takes a real, institute/admin-scheduled Test row by id (GET /api/tests/{id}) — the counterpart
// to the fully-generated Daily/Monthly flows, for whichever attemptType/result route the caller wants.
const props = defineProps({
  id: { type: [String, Number], default: null },
  attemptType: { type: String, default: 'daily' },
  resultBase: { type: String, default: '/app/daily-tests/result' },
})
const route = useRoute()
const router = useRouter()
const student = useStudentStore()

const testId = computed(() => props.id ?? route.params.id)

const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(t) { if (!t) return ''; return String(t).replace(ENV_RE, (m) => `\\[${m}\\]`) }
const KATEX_OPTS = { delimiters: [{ left: '\\[', right: '\\]', display: true }, { left: '\\(', right: '\\)', display: false }, { left: '$$', right: '$$', display: true }, { left: '$', right: '$', display: false }], throwOnError: false }

function normalizeQuestion(q) {
  let options = []
  try { options = JSON.parse(q.optionsJson || '[]') } catch { options = [] }
  return { id: q.id, stem: q.stem, options, correct: q.correctIndex ?? 0, difficulty: q.difficulty || 'Medium' }
}

const test = ref(null)
const questions = ref([])
const loading = ref(true)

async function load() {
  loading.value = true
  try {
    const { data } = await api.get(`/tests/${testId.value}`)
    test.value = data
    questions.value = (data.questions || []).map(normalizeQuestion)
  } catch {
    test.value = null
    questions.value = []
  }
  loading.value = false
}
onMounted(load)

const testEl = ref(null)
const currentIdx = ref(0)
const answers = ref({})
const started = ref(false)
const finished = ref(false)
const timeLeft = ref(0)
let timer = null

const currentQ = computed(() => questions.value[currentIdx.value])
watch(currentQ, () => nextTick(() => { if (testEl.value) renderMathInElement(testEl.value, KATEX_OPTS) }), { deep: true })

function startTest() {
  started.value = true
  timeLeft.value = (test.value.durationMin || 30) * 60
  timer = setInterval(() => {
    if (timeLeft.value > 0) timeLeft.value--
    else finishTest()
  }, 1000)
}

function go(dir) {
  if (dir === 'first')     currentIdx.value = 0
  else if (dir === 'last') currentIdx.value = questions.value.length - 1
  else if (dir === 'prev' && currentIdx.value > 0)                          currentIdx.value--
  else if (dir === 'next' && currentIdx.value < questions.value.length - 1) currentIdx.value++
}

function formatTime(s) {
  const m = Math.floor(s / 60).toString().padStart(2, '0')
  return `${m}:${(s % 60).toString().padStart(2, '0')}`
}

function finishTest() {
  clearInterval(timer)
  finished.value = true
  let score = 0
  questions.value.forEach((q, i) => { if (answers.value[i] === q.correct) score++ })
  const elapsed = (test.value.durationMin || 30) * 60 - timeLeft.value
  const savedId = student.saveTest({
    subject:     test.value.title,
    subjectId:   test.value.subjectId ?? null,
    testId:      test.value.id,
    type:        props.attemptType,
    score,
    total:       questions.value.length,
    questions:   questions.value,
    answers:     answers.value,
    durationSec: elapsed,
    attemptType: props.attemptType,
  })
  router.push(`${props.resultBase}/${savedId}`)
}

onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.take-topbar {
  display: flex; align-items: center; gap: 1.25rem; flex-wrap: wrap;
  padding: 0.75rem 1.25rem; margin-bottom: 1rem;
  background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px;
}
.take-subject { display: flex; align-items: center; gap: 0.75rem; }
.take-subj-name { font-weight: 700; font-size: 0.9rem; color: var(--t-text1); }
.take-subj-grade { font-size: 0.7rem; color: var(--t-text3); }
.take-qinfo { margin-left: auto; font-size: 0.82rem; color: var(--t-text3); font-weight: 600; }

.take-start-card {
  background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px;
  padding: 3rem 2rem; display: flex; flex-direction: column; align-items: center; text-align: center;
}

.take-body { display: flex; gap: 1.5rem; align-items: flex-start; }

.take-main {
  flex: 1; min-width: 0; background: var(--t-surface);
  border: 1px solid var(--t-border); border-radius: 12px; padding: 1.5rem;
}
.take-q-header { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1rem; }
.take-q-text { font-size: 1rem; color: var(--t-text1); line-height: 1.7; margin: 1rem 0; font-weight: 500; }
.take-options { display: flex; flex-direction: column; gap: 0.6rem; }
.take-opt {
  display: flex; align-items: center; gap: 0.75rem; padding: 0.7rem 1rem;
  border-radius: 8px; border: 1px solid var(--t-border); cursor: pointer;
  font-size: 0.9rem; color: var(--t-text1); transition: all 0.15s;
}
.take-opt:hover { border-color: var(--t-accent); background: var(--t-hover); }
.take-opt.selected { border-color: var(--t-accent); background: var(--t-acc-alpha-sm); }
.take-radio { accent-color: var(--t-accent); }

.take-nav {
  display: flex; gap: 0.5rem; flex-wrap: wrap; margin-top: 1.5rem;
  padding-top: 1rem; border-top: 1px solid var(--t-border);
}
.nav-btn {
  padding: 0.5rem 1rem; border-radius: 8px; font-size: 0.78rem; font-weight: 700;
  background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2);
  cursor: pointer; transition: all 0.15s;
}
.nav-btn:hover:not(:disabled) { background: var(--t-hover2); color: var(--t-text1); }
.nav-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.nav-finish { background: #4caf50; color: white; border-color: #4caf50; }
.nav-finish:hover:not(:disabled) { background: #388e3c; }

.take-sidebar { width: 200px; flex-shrink: 0; display: flex; flex-direction: column; gap: 1rem; }
.palette-section {
  background: var(--t-surface); border: 1px solid var(--t-border);
  border-radius: 12px; padding: 1rem;
}
.palette-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text2); margin-bottom: 0.75rem; }
.palette-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 0.3rem; }
.pal-btn {
  width: 30px; height: 30px; border-radius: 6px; font-size: 0.72rem; font-weight: 600;
  border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2);
  cursor: pointer; transition: all 0.1s;
}
.pal-btn:hover { border-color: var(--t-accent); color: var(--t-text1); }
.pal-answered { background: #4caf50 !important; color: white !important; border-color: #4caf50 !important; }
.pal-current { outline: 2px solid var(--t-accent); outline-offset: 1px; }

.timer-section {
  background: #1a2235; border-radius: 12px; padding: 1rem;
  text-align: center; border: 1px solid rgba(124,106,245,0.2);
}
.timer-lbl { font-size: 0.7rem; color: rgba(255,255,255,0.5); margin-bottom: 0.25rem; }
.timer-val { font-size: 1.6rem; font-weight: 800; color: #4caf50; font-family: 'Orbitron', monospace; }
.timer-urgent { color: #f44336; animation: timerPulse 1s ease-in-out infinite; }
@keyframes timerPulse { 0%,100% { opacity: 1; } 50% { opacity: 0.5; } }

@media (max-width: 768px) { .take-sidebar { display: none; } }
</style>
