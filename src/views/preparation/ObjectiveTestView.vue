<template>
  <div class="animate-fade-in" v-if="subject">
    <!-- Top bar -->
    <div class="take-topbar">
      <button v-if="!started" @click="$router.go(-1)" class="btn-ghost">
        <ArrowLeft class="w-4 h-4" />
      </button>
      <div class="take-subject">
        <span class="take-subj-icon">{{ subject.icon }}</span>
        <div>
          <div class="take-subj-name">{{ subject.name }}</div>
          <div class="take-subj-grade">Grade {{ gradeCode }} | Objective Test</div>
        </div>
      </div>
      <div class="take-badges">
        <span class="tbadge tbadge-blue">📝 {{ questions.length }} Questions</span>
        <span class="tbadge tbadge-orange">⏱ Graded Mode</span>
      </div>
      <div v-if="started && !finished" class="take-qinfo">
        Question {{ currentIdx + 1 }}/{{ questions.length }}
      </div>
    </div>

    <!-- Not started -->
    <div v-if="!started" class="take-start-card">
      <div class="text-5xl mb-3">📝</div>
      <h3 class="text-lg font-semibold" style="color:var(--t-text1)">Ready to test your knowledge?</h3>
      <p class="text-sm mt-1 max-w-sm" style="color:var(--t-text3)">
        {{ questions.length }} objective questions · {{ questions.length }} minutes · Answers hidden until you submit
      </p>
      <button @click="startTest" class="btn-primary mt-5 px-8 py-3 text-base gap-2">
        <Play class="w-5 h-5" /> Start Test
      </button>
    </div>

    <!-- In progress -->
    <div v-else-if="!finished" class="take-body">
      <!-- Question area -->
      <div class="take-main" ref="testEl">
        <div class="take-q-header">
          <CognitiveBadge :level="currentQ.cognitiveLevel" />
          <DifficultyBadge :level="currentQ.difficulty" />
        </div>
        <div class="take-q-text"
             :class="subject.medium === 'urdu' ? 'urdu' : ''"
             :dir="subject.medium === 'urdu' ? 'rtl' : 'ltr'"
             v-html="prepareMath(currentQ.stem)" />
        <div class="take-options" :dir="subject.medium === 'urdu' ? 'rtl' : 'ltr'">
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

      <!-- Sidebar: palette + timer -->
      <div class="take-sidebar">
        <div class="palette-section">
          <div class="palette-title">Questions</div>
          <div class="palette-grid">
            <button v-for="(q, i) in questions" :key="i"
              type="button"
              @click="currentIdx = i"
              :class="['pal-btn', answers[i] !== undefined ? 'pal-answered' : '', currentIdx === i ? 'pal-current' : '']"
              :aria-label="`Q${i+1}${answers[i] !== undefined ? ' (answered)' : ''}`">
              {{ i + 1 }}
            </button>
          </div>
          <div class="pal-legend">
            <span class="pal-leg-item pal-answered-item"></span>Answered
            <span class="pal-leg-item"></span>Unanswered
          </div>
        </div>
        <div class="timer-section">
          <div class="timer-lbl">Time Remaining</div>
          <div class="timer-val" :class="timeLeft < 60 ? 'timer-urgent' : ''">{{ formatTime(timeLeft) }}</div>
        </div>
      </div>
    </div>

    <!-- Saving state -->
    <div v-else class="take-start-card">
      <div class="text-4xl mb-2">⏳</div>
      <p style="color:var(--t-text2)">Saving your results…</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onUnmounted, nextTick, watch } from 'vue'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'
import { ArrowLeft, Play } from '@lucide/vue'
import { useRouter } from 'vue-router'
import { useContentStore } from '@/stores/content'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { useStudentStore } from '@/stores/student'
import { useAuthStore } from '@/stores/auth'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'

const props = defineProps({ bookId: String })
const router = useRouter()
const content = useContentStore()
const student = useStudentStore()
const auth = useAuthStore()

const subject = computed(() => findPrepSubject(props.bookId))
const gradeCode = computed(() => auth.user?.gradeCode || 9)

const testEl = ref(null)
const KATEX_OPTS = { delimiters: [{ left: '\\[', right: '\\]', display: true }, { left: '\\(', right: '\\)', display: false }, { left: '$$', right: '$$', display: true }, { left: '$', right: '$', display: false }], throwOnError: false }
const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(t) { if (!t) return ''; return String(t).replace(ENV_RE, (m) => `\\[${m}\\]`) }

const questions = ref(content.getQuestions(+props.bookId, { limit: 35 }))
const currentIdx = ref(0)
const answers = ref({})
const started = ref(false)
const finished = ref(false)
const timeLeft = ref(35 * 60)
let timer = null

const currentQ = computed(() => questions.value[currentIdx.value])
watch(currentQ, () => nextTick(() => { if (testEl.value) renderMathInElement(testEl.value, KATEX_OPTS) }), { deep: true })

function startTest() {
  started.value = true
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
  const elapsed = 35 * 60 - timeLeft.value
  const savedId = student.saveTest({
    subject:     subject.value?.name,
    type:        'Objective Test',
    score,
    total:       questions.value.length,
    bookId:      +props.bookId,
    questions:   questions.value,
    answers:     answers.value,
    durationSec: elapsed,
  })
  router.push(`/app/objective-tests/${savedId}/result`)
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
.take-subj-icon { font-size: 1.5rem; }
.take-subj-name { font-weight: 700; font-size: 0.9rem; color: var(--t-text1); }
.take-subj-grade { font-size: 0.7rem; color: var(--t-text3); }
.take-badges { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.tbadge { padding: 0.25rem 0.65rem; border-radius: 99px; font-size: 0.7rem; font-weight: 600; }
.tbadge-blue   { background: #e3f2fd; color: #1565c0; border: 1px solid #90caf9; }
.tbadge-orange { background: #fff3e0; color: #e65100; border: 1px solid #ffcc80; }
html.dark .tbadge-blue   { background: rgba(21,101,192,0.2); color: #90caf9; border-color: rgba(21,101,192,0.4); }
html.dark .tbadge-orange { background: rgba(230,81,0,0.2); color: #ffb74d; border-color: rgba(230,81,0,0.4); }
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
.pal-legend {
  display: flex; gap: 0.75rem; margin-top: 0.65rem;
  font-size: 0.65rem; color: var(--t-text3); align-items: center; flex-wrap: wrap;
}
.pal-leg-item {
  width: 12px; height: 12px; border-radius: 3px; display: inline-block;
  background: var(--t-hover); border: 1px solid var(--t-border);
}
.pal-answered-item { background: #4caf50; border-color: #4caf50; }

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
