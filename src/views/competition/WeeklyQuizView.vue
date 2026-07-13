<template>
  <div class="wq-root">
    <AIHelper context="User is doing weekly quiz for board exam preparation" :chips="['Explain the correct answer', 'Which chapter is this from?', 'Give similar practice questions', 'What should I study more?']" />

    <div class="wq-hero">
      <div class="wq-hero-badge">📅 Week {{ weekNumber }}</div>
      <h1 class="wq-hero-title">Weekly Quiz</h1>
      <p class="wq-hero-sub">New quizzes every Monday · Subject-wise · Track your progress week by week</p>
    </div>

    <!-- Setup -->
    <div v-if="phase === 'setup'">
      <div class="wq-week-info">
        <div class="wq-week-card">
          <div class="wq-week-dates">📅 Week {{ weekNumber }} · {{ weekRange }}</div>
          <div class="wq-week-streak">🔥 {{ streak }}-week streak</div>
        </div>
      </div>

      <div class="wq-subjects-grid">
        <div v-for="subj in quizSubjects" :key="subj.id" class="wq-subj-card">
          <div class="wq-subj-header">
            <div class="wq-subj-icon">{{ subj.icon }}</div>
            <div>
              <div class="wq-subj-name">{{ subj.name }}</div>
              <div class="wq-subj-meta">{{ subj.questions }} questions · {{ subj.duration }} min</div>
            </div>
            <div class="wq-subj-status" :class="getStatusClass(subj.id)">{{ getStatusText(subj.id) }}</div>
          </div>
          <div v-if="weeklyScores[subj.id]" class="wq-subj-score">
            <div class="wq-score-bar">
              <div class="wq-score-fill" :style="{width: weeklyScores[subj.id].pct + '%', background: weeklyScores[subj.id].pct >= 70 ? '#4caf50' : '#f59e0b'}"/>
            </div>
            <span class="wq-score-label">{{ weeklyScores[subj.id].pct }}%</span>
          </div>
          <button @click="startQuiz(subj)" class="wq-start-btn" :class="{ done: weeklyScores[subj.id] }">
            {{ weeklyScores[subj.id] ? '🔄 Retake' : '▶ Start Quiz' }}
          </button>
        </div>
      </div>

      <!-- Weekly summary -->
      <div class="wq-summary">
        <div class="wq-summary-title">📊 This Week's Progress</div>
        <div class="wq-summary-stats">
          <div class="wq-stat">
            <div class="wq-stat-val">{{ completedCount }}/{{ quizSubjects.length }}</div>
            <div class="wq-stat-label">Completed</div>
          </div>
          <div class="wq-stat">
            <div class="wq-stat-val">{{ weeklyAvg }}%</div>
            <div class="wq-stat-label">Average</div>
          </div>
          <div class="wq-stat">
            <div class="wq-stat-val">{{ streak }}</div>
            <div class="wq-stat-label">Week Streak</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="phase === 'loading'" class="wq-loading">
      <div class="wq-loading-spinner">⏳ Loading questions…</div>
    </div>

    <!-- Taking -->
    <div v-if="phase === 'taking'" class="wq-taking">
      <div class="wq-take-header">
        <button @click="phase='setup'" class="wq-back-btn">← Back</button>
        <div class="wq-take-title">{{ activeSubject?.name }}</div>
        <div class="wq-take-timer" :class="{ warn: timeLeft <= 10 }">⏱ {{ Math.floor(timeLeft/60) }}:{{ String(timeLeft%60).padStart(2,'0') }}</div>
      </div>
      <div class="wq-take-progress">
        <div class="wq-take-bar"><div class="wq-take-fill" :style="{width: ((currentIdx+1)/questions.length*100)+'%'}"/></div>
        <span>{{ currentIdx+1 }}/{{ questions.length }}</span>
      </div>
      <div v-if="currentQ" ref="questionEl" class="wq-question">
        <div class="wq-q-num">Question {{ currentIdx + 1 }}</div>
        <div class="wq-q-text" v-html="prepareMath(currentQ.question)" />
        <div class="wq-q-opts">
          <button v-for="(opt, i) in currentQ.options" :key="i" @click="answer(i)" class="wq-opt"
            :class="{
              selected: selectedAns === i && !showAns,
              correct: showAns && i === currentQ.correct,
              wrong: showAns && selectedAns === i && i !== currentQ.correct,
            }" :disabled="showAns">
            <span class="wq-opt-letter">{{ 'ABCD'[i] }}</span>
            <span v-html="prepareMath(opt)" />
          </button>
        </div>
        <div v-if="showAns" class="wq-ans-feedback">
          <div :class="selectedAns === currentQ.correct ? 'wq-correct' : 'wq-wrong'">
            <span v-if="selectedAns === currentQ.correct">✅ Correct!</span>
            <span v-else>❌ Wrong — Correct: <span v-html="prepareMath(currentQ.options[currentQ.correct])" /></span>
          </div>
          <button @click="next" class="wq-next-btn">{{ currentIdx < questions.length - 1 ? 'Next →' : 'Finish Quiz →' }}</button>
        </div>
      </div>
    </div>

    <!-- Result -->
    <div v-if="phase === 'result'" class="wq-result">
      <div class="wq-result-emoji">{{ resultData.pct >= 80 ? '🏆' : resultData.pct >= 60 ? '🌟' : '📚' }}</div>
      <div class="wq-result-title">{{ activeSubject?.name }} — Week {{ weekNumber }}</div>
      <div class="wq-result-big">{{ resultData.pct }}%</div>
      <div class="wq-result-detail">{{ resultData.correct }} out of {{ resultData.total }} correct</div>
      <div class="wq-result-message">{{ resultData.pct >= 80 ? 'Excellent work! You\'re well prepared.' : resultData.pct >= 60 ? 'Good effort! Review the topics you missed.' : 'Keep practicing! Try again to improve.' }}</div>
      <div class="wq-result-btns">
        <button @click="phase='setup'" class="wq-result-btn secondary">← All Subjects</button>
        <RouterLink to="/app/competition/leaderboard" class="wq-result-btn primary">🏆 Leaderboard</RouterLink>
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import { useStudentStore } from '@/stores/student'
import { getRealQuestions } from '@/services/assessmentBank'
import api from '@/services/api'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'
import AIHelper from '@/components/platform/AIHelper.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const auth = useAuthStore()
const catalog = useCatalogStore()
const student = useStudentStore()
const phase = ref('setup')
const activeSubject = ref(null)
const questions = ref([])
const currentIdx = ref(0)
const selectedAns = ref(null)
const showAns = ref(false)
const answers = ref([])
const timeLeft = ref(600)
const questionEl = ref(null)
let timer = null

const gradeCode = computed(() => auth.user?.gradeCode || auth.user?.grade || '9')
const subjects = computed(() => catalog.subjectsForGrade(gradeCode.value))

onMounted(async () => {
  if (!subjects.value.length) {
    await catalog.fetchSubjectsForGrade(gradeCode.value).catch(() => {})
  }
})

const now = new Date()
const weekNumber = computed(() => {
  const start = new Date(now.getFullYear(), 0, 1)
  return Math.ceil((((now - start) / 86400000) + start.getDay() + 1) / 7)
})
const weekRange = computed(() => {
  const day = now.getDay()
  const mon = new Date(now); mon.setDate(now.getDate() - day + 1)
  const sun = new Date(mon); sun.setDate(mon.getDate() + 6)
  return `${mon.toLocaleDateString('en-PK', { month: 'short', day: 'numeric' })} – ${sun.toLocaleDateString('en-PK', { month: 'short', day: 'numeric' })}`
})

const weeklyScoresKey = `bap_weekly_${weekNumber.value}`
const weeklyScores = ref(JSON.parse(localStorage.getItem(weeklyScoresKey) || '{}'))
const streak = computed(() => JSON.parse(localStorage.getItem('bap_weekly_streak') || '1'))

const quizSubjects = computed(() => subjects.value.slice(0, 6).map(s => ({ ...s, icon: s.icon || '📚', questions: 10, duration: 15 })))
const completedCount = computed(() => Object.keys(weeklyScores.value).length)
const weeklyAvg = computed(() => {
  const vals = Object.values(weeklyScores.value)
  if (!vals.length) return 0
  return Math.round(vals.reduce((s, v) => s + v.pct, 0) / vals.length)
})

function getStatusText(id) { return weeklyScores.value[id] ? `${weeklyScores.value[id].pct}% ✓` : 'Pending' }
function getStatusClass(id) { return weeklyScores.value[id] ? 'done' : 'pending' }

const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(html) {
  if (!html) return ''
  const s = String(html)
  return s.replace(ENV_RE, (match, _env, offset) => {
    const before = s.slice(0, offset)
    const opens = (before.match(/\\\[/g) || []).length
    const closes = (before.match(/\\\]/g) || []).length
    return opens > closes ? match : `\\[${match}\\]`
  })
}
const KATEX_OPTS = {
  delimiters: [
    { left: '\\[', right: '\\]', display: true },
    { left: '\\(', right: '\\)', display: false },
    { left: '$', right: '$', display: false },
  ],
  throwOnError: false,
}
async function renderMath() {
  await nextTick()
  if (questionEl.value) renderMathInElement(questionEl.value, KATEX_OPTS)
}
watch(currentIdx, renderMath)
watch(phase, renderMath)

async function startQuiz(subj) {
  activeSubject.value = subj
  phase.value = 'loading'
  let qs = []
  try {
    const res = await api.get('/questions/random', { params: { subjectId: subj.id, grade: gradeCode.value, limit: 10 } })
    qs = (res.data || []).map(q => {
      let opts = []
      try { opts = JSON.parse(q.optionsJson || '[]') } catch { opts = [] }
      return { question: q.stem || q.question, options: opts, correct: q.correctIndex ?? 0 }
    })
  } catch { /* fall through */ }

  if (!qs.length) {
    try {
      const real = await getRealQuestions({ grade: gradeCode.value, subject: subj.name, limit: 10 })
      qs = real.map(q => ({ question: q.stem || q.question, options: q.options || [], correct: q.correct ?? q.correctIndex ?? 0 }))
    } catch { qs = [] }
  }

  if (!qs.length) { phase.value = 'setup'; return }

  questions.value = qs
  answers.value = new Array(qs.length).fill(null)
  currentIdx.value = 0; selectedAns.value = null; showAns.value = false
  timeLeft.value = subj.duration * 60
  phase.value = 'taking'
  timer = setInterval(() => { timeLeft.value--; if (timeLeft.value <= 0) { clearInterval(timer); finish() } }, 1000)
}

const currentQ = computed(() => questions.value[currentIdx.value])
function answer(idx) { selectedAns.value = idx; showAns.value = true; answers.value[currentIdx.value] = idx }
function next() {
  if (currentIdx.value < questions.value.length - 1) { currentIdx.value++; selectedAns.value = null; showAns.value = false }
  else { clearInterval(timer); finish() }
}
function finish() {
  clearInterval(timer)
  phase.value = 'result'
  const correct = answers.value.filter((a, i) => a === questions.value[i]?.correct).length
  const pct = Math.round(correct / questions.value.length * 100)
  weeklyScores.value[activeSubject.value.id] = { correct, total: questions.value.length, pct }
  localStorage.setItem(weeklyScoresKey, JSON.stringify(weeklyScores.value))
  student.saveTest({ subject: activeSubject.value.name, type: 'weekly', score: correct, total: questions.value.length, bookId: 'weekly' })
}
const resultData = computed(() => {
  const correct = answers.value.filter((a, i) => a === questions.value[i]?.correct).length
  return { correct, total: questions.value.length, pct: Math.round(correct / questions.value.length * 100) }
})
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.wq-root { max-width: 760px; margin: 0 auto; padding: 1.5rem; }
.wq-hero { text-align: center; padding: 1.5rem 0; }
.wq-hero-badge { display: inline-block; padding: 0.3rem 0.85rem; background: rgba(76,175,80,0.1); color: #4caf50; border-radius: 99px; font-size: 0.8rem; font-weight: 700; margin-bottom: 0.5rem; }
.wq-hero-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.wq-hero-sub { color: var(--t-text3); font-size: 0.875rem; }
.wq-week-info { margin-bottom: 1.5rem; }
.wq-week-card { display: flex; justify-content: space-between; align-items: center; padding: 0.85rem 1.25rem; border: 1px solid var(--t-border); border-radius: 14px; background: var(--t-surface); }
.wq-week-dates { font-size: 0.875rem; color: var(--t-text2); font-weight: 600; }
.wq-week-streak { font-size: 0.875rem; color: #f59e0b; font-weight: 700; }
.wq-subjects-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
.wq-subj-card { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.25rem; background: var(--t-surface); }
.wq-subj-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; }
.wq-subj-icon { font-size: 1.75rem; flex-shrink: 0; }
.wq-subj-name { font-weight: 700; color: var(--t-text1); font-size: 0.9rem; }
.wq-subj-meta { font-size: 0.72rem; color: var(--t-text3); }
.wq-subj-status { margin-left: auto; font-size: 0.72rem; font-weight: 700; padding: 0.2rem 0.55rem; border-radius: 99px; white-space: nowrap; }
.wq-subj-status.done { background: rgba(76,175,80,0.1); color: #4caf50; }
.wq-subj-status.pending { background: var(--t-hover); color: var(--t-text3); }
.wq-subj-score { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.75rem; }
.wq-score-bar { flex: 1; height: 5px; background: var(--t-border); border-radius: 99px; overflow: hidden; }
.wq-score-fill { height: 100%; border-radius: 99px; transition: width 0.5s; }
.wq-score-label { font-size: 0.72rem; font-weight: 700; color: var(--t-text2); }
.wq-start-btn { width: 100%; padding: 0.6rem; border-radius: 10px; background: linear-gradient(135deg, #4caf50, #00bcd4); color: white; border: none; font-weight: 700; cursor: pointer; font-size: 0.85rem; }
.wq-start-btn.done { background: var(--t-hover); color: var(--t-text2); border: 1px solid var(--t-border); }
.wq-summary { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.25rem; background: var(--t-surface); }
.wq-summary-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 1rem; }
.wq-summary-stats { display: flex; gap: 2rem; }
.wq-stat-val { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); }
.wq-stat-label { font-size: 0.72rem; color: var(--t-text3); }
.wq-take-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; }
.wq-back-btn { padding: 0.4rem 0.85rem; background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 99px; color: var(--t-text2); font-size: 0.8rem; cursor: pointer; }
.wq-take-title { flex: 1; font-weight: 800; color: var(--t-text1); }
.wq-take-timer { font-size: 0.9rem; font-weight: 800; padding: 0.3rem 0.75rem; border-radius: 99px; background: var(--t-hover); color: var(--t-text1); }
.wq-take-timer.warn { background: rgba(239,68,68,0.1); color: #ef4444; }
.wq-take-progress { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.25rem; }
.wq-take-bar { flex: 1; height: 5px; background: var(--t-border); border-radius: 99px; overflow: hidden; }
.wq-take-fill { height: 100%; background: linear-gradient(90deg, #4caf50, #00bcd4); border-radius: 99px; transition: width 0.3s; }
.wq-take-progress > span { font-size: 0.78rem; color: var(--t-text3); white-space: nowrap; }
.wq-question { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.5rem; background: var(--t-surface); }
.wq-q-num { font-size: 0.72rem; font-weight: 700; color: var(--t-text2); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.5rem; }
.wq-q-text { font-size: 1.05rem; font-weight: 700; color: var(--t-text1); line-height: 1.5; margin-bottom: 1.25rem; }
.wq-q-opts { display: flex; flex-direction: column; gap: 0.5rem; }
.wq-opt { display: flex; align-items: center; gap: 0.75rem; padding: 0.65rem 1rem; border: 1px solid var(--t-border); border-radius: 12px; background: var(--t-surface); color: var(--t-text1); text-align: left; cursor: pointer; font-size: 0.9rem; transition: all 0.1s; }
.wq-opt:hover:not(:disabled) { border-color: #4caf50; }
.wq-opt.selected { border-color: #4caf50; background: rgba(76,175,80,0.06); }
.wq-opt.correct { border-color: var(--t-success); background: var(--t-success-bg); color: var(--t-success); font-weight: 700; }
.wq-opt.wrong { border-color: var(--t-danger); background: var(--t-danger-bg); color: var(--t-danger); }
.wq-opt-letter { width: 26px; height: 26px; border-radius: 50%; background: var(--t-hover2); color: var(--t-text2); font-size: 0.75rem; font-weight: 800; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.wq-ans-feedback { margin-top: 1rem; display: flex; flex-direction: column; gap: 0.5rem; }
.wq-correct { padding: 0.6rem 1rem; border-radius: 10px; background: var(--t-success-bg); color: var(--t-success); font-weight: 700; font-size: 0.9rem; }
.wq-wrong { padding: 0.6rem 1rem; border-radius: 10px; background: var(--t-danger-bg); color: var(--t-danger); font-weight: 700; font-size: 0.9rem; }
.wq-next-btn { align-self: flex-end; padding: 0.6rem 1.25rem; background: linear-gradient(135deg, #4caf50, #00bcd4); color: white; border: none; border-radius: 10px; font-weight: 700; cursor: pointer; }
.wq-result { text-align: center; padding: 2rem 1rem; }
.wq-result-emoji { font-size: 4rem; margin-bottom: 0.5rem; }
.wq-result-title { font-size: 1.1rem; font-weight: 700; color: var(--t-text2); margin-bottom: 0.25rem; }
.wq-result-big { font-size: 4rem; font-weight: 800; color: var(--t-text1); }
.wq-result-detail { color: var(--t-text3); font-size: 0.9rem; margin: 0.25rem 0 0.75rem; }
.wq-result-message { color: var(--t-text2); font-size: 0.9rem; margin-bottom: 1.5rem; }
.wq-result-btns { display: flex; gap: 0.75rem; justify-content: center; }
.wq-result-btn { padding: 0.75rem 1.5rem; border-radius: 14px; font-weight: 700; font-size: 0.9rem; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; }
.wq-result-btn.secondary { border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text1); }
.wq-result-btn.primary { background: linear-gradient(135deg, #4caf50, #00bcd4); color: white; border: none; }
.wq-loading { text-align: center; padding: 3rem 1rem; }
.wq-loading-spinner { font-size: 1.1rem; color: var(--t-text3); }
</style>
