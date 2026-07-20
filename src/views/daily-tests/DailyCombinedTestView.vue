<template>
  <div class="animate-fade-in space-y-5">
    <!-- Setup screen -->
    <div v-if="phase === 'setup'">
      <h2 class="section-title">Daily Combined Challenge — Setup</h2>
      <div class="dct-setup-card">
        <div class="dct-setup-section">
          <div class="dct-setup-label">Select Subjects</div>
          <div class="dct-subject-grid">
            <label v-for="s in subjects" :key="s.id" class="dct-subj-item"
              :class="{ 'dct-subj-active': selectedSubjects.includes(s.id) }">
              <input type="checkbox" v-model="selectedSubjects" :value="s.id" />
              <span class="dct-subj-icon">{{ s.icon }}</span>
              <span>{{ s.name }}</span>
            </label>
          </div>
        </div>
        <div class="dct-setup-row">
          <div class="dct-setup-section">
            <div class="dct-setup-label">MCQs per Subject</div>
            <div class="dct-btn-row">
              <button v-for="n in [5, 10, 15, 20]" :key="n" @click="mcqCount = n"
                class="dct-opt-btn" :class="mcqCount === n ? 'dct-opt-active' : ''">{{ n }}</button>
            </div>
          </div>
          <div class="dct-setup-section">
            <div class="dct-setup-label">Subjective Qs per Subject</div>
            <div class="dct-btn-row">
              <button v-for="n in [0, 2, 3, 5]" :key="n" @click="subjCount = n"
                class="dct-opt-btn" :class="subjCount === n ? 'dct-opt-active' : ''">{{ n }}</button>
            </div>
          </div>
          <div class="dct-setup-section">
            <div class="dct-setup-label">Time Limit (minutes)</div>
            <div class="dct-btn-row">
              <button v-for="m in [15, 30, 45, 60]" :key="m" @click="timeLimit = m"
                class="dct-opt-btn" :class="timeLimit === m ? 'dct-opt-active' : ''">{{ m }}</button>
            </div>
          </div>
        </div>
        <button @click="startExam" :disabled="!selectedSubjects.length" class="dct-start-btn">
          ⚡ Generate Challenge
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="phase === 'loading'" class="dct-loading">
      <div class="dct-loading-text">⏳ Loading questions…</div>
    </div>

    <!-- Objective MCQ phase -->
    <div v-if="phase === 'objective'">
      <div class="dct-exam-header">
        <div class="dct-exam-title-row">
          <div>
            <div class="dct-exam-sub-tag">{{ currentSubjectName }} — Objective</div>
            <div class="dct-exam-progress">Subject {{ currentSubjectIdx + 1 }} / {{ selectedSubjects.length }}</div>
          </div>
          <div class="dct-timer" :class="timeLeft < 60 ? 'dct-timer-red' : ''">{{ formatTime(timeLeft) }}</div>
        </div>
        <!-- Palette -->
        <div class="dct-palette">
          <div v-for="(q, i) in currentObjectiveQs" :key="i"
            @click="currentQIdx = i"
            :class="['dct-pal-btn', objAnswers[currentSubjectIdx]?.[i] !== undefined ? 'dct-pal-done' : '', i === currentQIdx ? 'dct-pal-cur' : '']">
            {{ i + 1 }}
          </div>
        </div>
      </div>
      <div class="dct-q-card" ref="mathContainer">
        <div class="dct-q-num">Question {{ currentQIdx + 1 }}</div>
        <CognitiveBadge :level="currentQ.cognitiveLevel" />
        <DifficultyBadge :level="currentQ.difficulty" />
        <div class="dct-q-stem" :dir="currentQ.isUrdu ? 'rtl' : 'ltr'" v-html="prepareMath(currentQ.stem)" />
        <div class="dct-options">
          <label v-for="(opt, oi) in currentQ.options" :key="oi" class="dct-opt"
            :class="{ 'dct-opt-sel': objAnswers[currentSubjectIdx]?.[currentQIdx] === oi }">
            <input type="radio" :name="`obj_${currentSubjectIdx}_${currentQIdx}`" :value="oi"
              @change="setObjAnswer(oi)" />
            <span v-html="prepareMath(opt)" />
          </label>
        </div>
      </div>
      <div class="dct-nav">
        <button @click="currentQIdx--" :disabled="currentQIdx === 0" class="dct-nav-btn">← Prev</button>
        <button v-if="currentQIdx < currentObjectiveQs.length - 1" @click="currentQIdx++" class="dct-nav-btn">Next →</button>
        <button v-else @click="finishObjective" class="dct-next-subj-btn">
          {{ subjCount > 0 ? 'Next: Subjective →' : (hasMoreSubjects ? 'Next Subject →' : 'Finish Test') }}
        </button>
      </div>
    </div>

    <!-- Subjective phase -->
    <div v-if="phase === 'subjective'">
      <div class="dct-exam-header">
        <div class="dct-exam-title-row">
          <div>
            <div class="dct-exam-sub-tag">{{ currentSubjectName }} — Subjective</div>
          </div>
          <div class="dct-timer" :class="timeLeft < 60 ? 'dct-timer-red' : ''">{{ formatTime(timeLeft) }}</div>
        </div>
      </div>
      <div v-for="(q, qi) in currentSubjectiveQs" :key="qi" class="dct-subj-q-card">
        <div class="dct-q-num">Q{{ qi + 1 }}</div>
        <CognitiveBadge :level="q.cognitiveLevel" />
        <DifficultyBadge :level="q.difficulty" />
        <div class="dct-q-stem" v-html="prepareMath(q.stem)" />
        <div class="dct-subj-input-row">
          <textarea v-model="subjAnswers[currentSubjectIdx][qi]" placeholder="Write your answer here..."
            class="dct-subj-textarea" rows="3" />
          <button class="dct-mic-btn" title="Voice input">🎤</button>
        </div>
      </div>
      <div class="dct-nav">
        <button @click="finishSubjective" class="dct-next-subj-btn">
          {{ hasMoreSubjects ? 'Next Subject →' : 'Finish Test' }}
        </button>
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'
import { useContentStore } from '@/stores/content'
import { useStudentStore } from '@/stores/student'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import api from '@/services/api'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const router = useRouter()
const content = useContentStore()
const student = useStudentStore()
const auth = useAuthStore()
const catalog = useCatalogStore()

const mathContainer = ref(null)
const KATEX_OPTS = {
  delimiters: [
    { left: '\\[', right: '\\]', display: true },
    { left: '\\(', right: '\\)', display: false },
    { left: '$$', right: '$$', display: true },
    { left: '$', right: '$', display: false },
  ],
  throwOnError: false,
}
const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(text) {
  if (!text) return ''
  return String(text).replace(ENV_RE, (m) => `\\[${m}\\]`)
}
function renderMath() {
  nextTick(() => { if (mathContainer.value) renderMathInElement(mathContainer.value, KATEX_OPTS) })
}

const gradeCode = computed(() => auth.user?.gradeCode || auth.user?.grade || '9')
const subjects = computed(() => catalog.subjectsForGrade(gradeCode.value))

onMounted(async () => {
  if (!subjects.value.length) {
    await catalog.fetchSubjectsForGrade(gradeCode.value).catch(() => {})
  }
})

function normalizeObjQ(q) {
  if (q.optionsJson !== undefined) {
    let opts = []
    try { opts = JSON.parse(q.optionsJson || '[]') } catch { opts = [] }
    return { stem: q.stem, options: opts, correct: q.correctIndex ?? 0, cognitiveLevel: 'Knowledge', difficulty: 'Medium', isUrdu: false }
  }
  return {
    stem: q.stem || q.question,
    options: q.options || [],
    correct: q.correct ?? 0,
    cognitiveLevel: q.cognitiveLevel || 'Knowledge',
    difficulty: q.difficulty || 'Medium',
    isUrdu: q.isUrdu || false,
  }
}

const phase = ref('setup')
const selectedSubjects = ref([])
const mcqCount = ref(10)
const subjCount = ref(2)
const timeLimit = ref(30)
const timeLeft = ref(30 * 60)
let timer = null

const currentSubjectIdx = ref(0)
const currentQIdx = ref(0)
const objAnswers = ref({})
const subjAnswers = ref({})

const allObjQs = ref({})
const allSubjQs = ref({})

const currentSubjectId = computed(() => selectedSubjects.value[currentSubjectIdx.value])
const currentSubjectName = computed(() => subjects.value.find(s => s.id === currentSubjectId.value)?.name || '')
const currentObjectiveQs = computed(() => allObjQs.value[currentSubjectId.value] || [])
const currentSubjectiveQs = computed(() => allSubjQs.value[currentSubjectId.value] || [])
const currentQ = computed(() => currentObjectiveQs.value[currentQIdx.value] || {})
const hasMoreSubjects = computed(() => currentSubjectIdx.value < selectedSubjects.value.length - 1)
watch(currentQ, renderMath, { deep: true })

async function startExam() {
  phase.value = 'loading'
  for (const id of selectedSubjects.value) {
    const subjectName = subjects.value.find(s => s.id === id)?.name || ''
    let qs = []
    try {
      const res = await api.get('/questions/random', { params: { subjectId: id, gradeCode: gradeCode.value, count: mcqCount.value } })
      qs = (res.data || []).map(normalizeObjQ)
    } catch { /* fall through */ }
    if (!qs.length) {
      qs = (await content.getQuestionsReal(id, { grade: gradeCode.value, subjectName, limit: mcqCount.value })).map(normalizeObjQ)
    }
    allObjQs.value[id] = qs
    allSubjQs.value[id] = content.getSubjectiveQuestions(id, { limit: subjCount.value })
    objAnswers.value[id] = {}
    subjAnswers.value[id] = {}
  }
  timeLeft.value = timeLimit.value * 60
  phase.value = 'objective'
  currentSubjectIdx.value = 0
  currentQIdx.value = 0
  timer = setInterval(() => { if (timeLeft.value > 0) timeLeft.value--; else finish() }, 1000)
}

function setObjAnswer(oi) {
  const sid = currentSubjectId.value
  objAnswers.value[sid] = { ...objAnswers.value[sid], [currentQIdx.value]: oi }
}

function finishObjective() {
  if (subjCount.value > 0) {
    phase.value = 'subjective'
    currentQIdx.value = 0
  } else {
    advanceSubject()
  }
}

function finishSubjective() { advanceSubject() }

function advanceSubject() {
  if (hasMoreSubjects.value) {
    currentSubjectIdx.value++
    currentQIdx.value = 0
    phase.value = 'objective'
  } else {
    finish()
  }
}

// Flattens the per-subject objective question/answer maps into a single ordered list so the
// result screen can render a combined breakdown, and scores each answer against its real
// correct index (the old code just counted how many questions were answered, not correctness).
function buildAttemptPayload() {
  const questions = []
  const answers = {}
  selectedSubjects.value.forEach((id) => {
    const qs = allObjQs.value[id] || []
    const subjectAnswers = objAnswers.value[id] || {}
    qs.forEach((q, i) => {
      const flatIdx = questions.length
      questions.push({ ...q, id: flatIdx })
      if (subjectAnswers[i] !== undefined) answers[flatIdx] = subjectAnswers[i]
    })
  })
  const score = questions.reduce((total, q, i) => total + (answers[i] === q.correct ? 1 : 0), 0)
  return { questions, answers, score, total: questions.length }
}

function finish() {
  clearInterval(timer)
  const { questions, answers, score, total } = buildAttemptPayload()
  const subjectNames = selectedSubjects.value
    .map(id => subjects.value.find(s => s.id === id)?.name)
    .filter(Boolean)
    .join(', ')
  const durationSec = (timeLimit.value * 60) - timeLeft.value

  const id = student.saveTest({
    subject: subjectNames || 'Daily Combined Challenge',
    type: 'daily',
    score,
    total,
    durationSec,
    questions,
    answers,
    attemptType: 'daily',
  })
  router.push(`/app/daily-tests/result/${id}`)
}

function formatTime(s) {
  const m = Math.floor(s / 60); const sec = s % 60
  return String(m).padStart(2, '0') + ':' + String(sec).padStart(2, '0')
}

onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.dct-setup-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 14px; padding: 1.5rem; }
.dct-setup-section { margin-bottom: 1.25rem; }
.dct-setup-label { font-size: 0.82rem; font-weight: 700; color: var(--t-text2); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.6rem; }
.dct-subject-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 0.5rem; }
.dct-subj-item { display: flex; align-items: center; gap: 0.5rem; padding: 0.55rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; cursor: pointer; font-size: 0.82rem; color: var(--t-text1); }
.dct-subj-item input { accent-color: #4caf50; }
.dct-subj-active { border-color: #4caf50; background: rgba(76,175,80,0.1); }
.dct-subj-icon { font-size: 1rem; }
.dct-setup-row { display: flex; gap: 1rem; flex-wrap: wrap; }
.dct-setup-row .dct-setup-section { flex: 1; min-width: 180px; }
.dct-btn-row { display: flex; gap: 0.4rem; }
.dct-opt-btn { flex: 1; padding: 0.45rem 0; border-radius: 7px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text2); font-size: 0.82rem; font-weight: 600; cursor: pointer; }
.dct-opt-active { background: #4caf50; color: white; border-color: #4caf50; }
.dct-start-btn { width: 100%; margin-top: 0.5rem; padding: 0.85rem; border-radius: 10px; font-weight: 800; font-size: 0.95rem; background: #4caf50; color: white; border: none; cursor: pointer; letter-spacing: 0.03em; }
.dct-start-btn:disabled { opacity: 0.4; cursor: not-allowed; }

.dct-exam-header { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1rem 1.25rem; }
.dct-exam-title-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.75rem; }
.dct-exam-sub-tag { font-size: 0.9rem; font-weight: 700; color: var(--t-text1); }
.dct-exam-progress { font-size: 0.75rem; color: var(--t-text3); margin-top: 0.2rem; }
.dct-timer { font-family: 'Orbitron', monospace; font-size: 1.1rem; font-weight: 700; color: #4caf50; }
.dct-timer-red { color: #ef5350; }
.dct-palette { display: flex; flex-wrap: wrap; gap: 0.3rem; margin-top: 0.5rem; }
.dct-pal-btn { width: 30px; height: 30px; border-radius: 5px; border: 1px solid var(--t-border); background: #555; color: white; font-size: 0.72rem; font-weight: 700; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.dct-pal-done { background: #4caf50 !important; }
.dct-pal-cur { outline: 2px solid #00bcd4; outline-offset: 1px; }

.dct-q-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem 1.5rem; display: flex; flex-direction: column; gap: 0.6rem; }
.dct-q-num { font-size: 0.8rem; color: var(--t-text3); }
.dct-q-stem { font-size: 1rem; color: var(--t-text1); line-height: 1.6; }
.dct-options { display: flex; flex-direction: column; gap: 0; border-top: 1px solid var(--t-border); margin-top: 0.25rem; }
.dct-opt { display: flex; align-items: center; gap: 0.75rem; padding: 0.65rem 0; border-bottom: 1px solid var(--t-border); cursor: pointer; color: var(--t-text1); font-size: 0.9rem; }
.dct-opt:last-child { border-bottom: none; }
.dct-opt-sel { background: rgba(76,175,80,0.1); }
.dct-nav { display: flex; gap: 0.5rem; }
.dct-nav-btn { padding: 0.5rem 1.25rem; border-radius: 8px; font-size: 0.85rem; font-weight: 600; background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2); cursor: pointer; }
.dct-nav-btn:disabled { opacity: 0.3; cursor: not-allowed; }
.dct-next-subj-btn { padding: 0.6rem 1.5rem; border-radius: 8px; font-size: 0.85rem; font-weight: 700; background: #4caf50; color: white; border: none; cursor: pointer; margin-left: auto; }

.dct-subj-q-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem 1.5rem; display: flex; flex-direction: column; gap: 0.5rem; }
.dct-subj-input-row { display: flex; gap: 0.75rem; align-items: flex-start; }
.dct-subj-textarea { flex: 1; padding: 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; resize: vertical; }
.dct-mic-btn { padding: 0.5rem 0.75rem; border-radius: 8px; border: 1px solid var(--t-border); background: var(--t-surface); cursor: pointer; font-size: 1.1rem; flex-shrink: 0; }
.dct-loading { text-align: center; padding: 3rem 1rem; }
.dct-loading-text { font-size: 1.1rem; color: var(--t-text3); }

.dct-q-card :deep(.katex-display) { margin: 0.4rem 0; overflow-x: auto; }
.dct-q-card :deep(.katex) { font-size: 1em; color: var(--t-text1); }
</style>
