<template>
  <div class="animate-fade-in space-y-5">
    <!-- Loading -->
    <div v-if="phase === 'loading'" class="mt-loading">
      <div class="mt-loading-text">⏳ Loading questions…</div>
    </div>

    <!-- Setup screen -->
    <div v-if="phase === 'setup'">
      <h2 class="section-title">Monthly Combined Exam — Setup</h2>
      <div class="mt-setup-card">
        <div class="mt-setup-section">
          <div class="mt-setup-label">Select Subjects</div>
          <div class="mt-subject-grid">
            <label v-for="s in subjects" :key="s.id" class="mt-subj-item"
              :class="{ 'mt-subj-active': selectedSubjects.includes(s.id) }">
              <input type="checkbox" v-model="selectedSubjects" :value="s.id" />
              <span>{{ s.icon }}</span>
              <span>{{ s.name }}</span>
            </label>
          </div>
        </div>
        <div class="mt-setup-row">
          <div class="mt-setup-section">
            <div class="mt-setup-label">MCQs per Subject</div>
            <div class="mt-btn-row">
              <button v-for="n in [10, 15, 20, 30]" :key="n" @click="mcqCount = n"
                class="mt-opt-btn" :class="mcqCount === n ? 'mt-opt-active' : ''">{{ n }}</button>
            </div>
          </div>
          <div class="mt-setup-section">
            <div class="mt-setup-label">Subjective Qs per Subject</div>
            <div class="mt-btn-row">
              <button v-for="n in [2, 3, 5, 0]" :key="n" @click="subjCount = n"
                class="mt-opt-btn" :class="subjCount === n ? 'mt-opt-active' : ''">{{ n || 'None' }}</button>
            </div>
          </div>
          <div class="mt-setup-section">
            <div class="mt-setup-label">Time Limit (minutes)</div>
            <div class="mt-btn-row">
              <button v-for="m in [30, 60, 90, 120]" :key="m" @click="timeLimit = m"
                class="mt-opt-btn" :class="timeLimit === m ? 'mt-opt-active' : ''">{{ m }}</button>
            </div>
          </div>
        </div>
        <button @click="startExam" :disabled="!selectedSubjects.length" class="mt-start-btn">
          🎯 Start Monthly Exam
        </button>
      </div>
    </div>

    <!-- Objective phase -->
    <div v-if="phase === 'objective'">
      <div class="mt-exam-header">
        <div class="mt-exam-title-row">
          <div>
            <div class="mt-exam-sub-tag">{{ currentSubjectName }} — Objective MCQs</div>
            <div class="mt-exam-progress">Subject {{ currentSubjectIdx + 1 }} / {{ selectedSubjects.length }}</div>
          </div>
          <div class="mt-timer" :class="timeLeft < 120 ? 'mt-timer-red' : ''">{{ formatTime(timeLeft) }}</div>
        </div>
        <div class="mt-palette">
          <div v-for="(q, i) in currentObjectiveQs" :key="i"
            @click="currentQIdx = i"
            :class="['mt-pal-btn', objAnswers[currentSubjectIdx]?.[i] !== undefined ? 'mt-pal-done' : '', i === currentQIdx ? 'mt-pal-cur' : '']">
            {{ i + 1 }}
          </div>
        </div>
      </div>
      <div class="mt-q-card" ref="mathContainer">
        <div class="mt-q-num">Question {{ currentQIdx + 1 }}</div>
        <CognitiveBadge :level="currentQ.cognitiveLevel" />
        <DifficultyBadge :level="currentQ.difficulty" />
        <div class="mt-q-stem" :dir="currentQ.isUrdu ? 'rtl' : 'ltr'" v-html="prepareMath(currentQ.stem)" />
        <div class="mt-options">
          <label v-for="(opt, oi) in currentQ.options" :key="oi" class="mt-opt"
            :class="{ 'mt-opt-sel': objAnswers[currentSubjectIdx]?.[currentQIdx] === oi }">
            <input type="radio" :name="`obj_${currentSubjectIdx}_${currentQIdx}`" :value="oi"
              @change="setObjAnswer(oi)" />
            <span v-html="prepareMath(opt)" />
          </label>
        </div>
      </div>
      <div class="mt-nav">
        <button @click="currentQIdx--" :disabled="currentQIdx === 0" class="mt-nav-btn">← Prev</button>
        <button v-if="currentQIdx < currentObjectiveQs.length - 1" @click="currentQIdx++" class="mt-nav-btn">Next →</button>
        <button v-else @click="finishObjective" class="mt-next-btn">
          {{ subjCount > 0 ? 'Subjective →' : (hasMoreSubjects ? 'Next Subject →' : 'Finish Exam') }}
        </button>
      </div>
    </div>

    <!-- Subjective phase -->
    <div v-if="phase === 'subjective'">
      <div class="mt-exam-header">
        <div class="mt-exam-title-row">
          <div class="mt-exam-sub-tag">{{ currentSubjectName }} — Subjective</div>
          <div class="mt-timer" :class="timeLeft < 120 ? 'mt-timer-red' : ''">{{ formatTime(timeLeft) }}</div>
        </div>
      </div>
      <div v-for="(q, qi) in currentSubjectiveQs" :key="qi" class="mt-subj-q-card">
        <div class="mt-q-num">Q{{ qi + 1 }}</div>
        <CognitiveBadge :level="q.cognitiveLevel" />
        <DifficultyBadge :level="q.difficulty" />
        <div class="mt-q-stem" v-html="prepareMath(q.stem)" />
        <div class="mt-subj-input-row">
          <textarea v-model="subjAnswers[currentSubjectIdx][qi]" placeholder="Write your answer..."
            class="mt-subj-textarea" rows="4" />
          <button class="mt-mic-btn" title="Voice input">🎤</button>
        </div>
      </div>
      <div class="mt-nav">
        <button @click="finishSubjective" class="mt-next-btn">
          {{ hasMoreSubjects ? 'Next Subject →' : 'Finish Exam' }}
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
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import { useStudentStore } from '@/stores/student'
import { getRealQuestions } from '@/services/assessmentBank'
import api from '@/services/api'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const router = useRouter()
const auth = useAuthStore()
const catalog = useCatalogStore()
const content = useContentStore()
const student = useStudentStore()

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
const mcqCount = ref(15)
const subjCount = ref(3)
const timeLimit = ref(60)
const timeLeft = ref(60 * 60)
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
  const loadPromises = selectedSubjects.value.map(async (id) => {
    const subjectName = subjects.value.find(s => s.id === id)?.name || ''
    let qs = []
    try {
      const res = await api.get('/questions/random', { params: { subjectId: id, grade: gradeCode.value, limit: mcqCount.value } })
      qs = (res.data || []).map(normalizeObjQ)
    } catch { /* fall through */ }
    if (!qs.length && subjectName) {
      try {
        const real = await getRealQuestions({ grade: gradeCode.value, subject: subjectName, limit: mcqCount.value })
        qs = real.map(normalizeObjQ)
      } catch { qs = [] }
    }
    if (!qs.length) {
      qs = content.getQuestions(id, { limit: mcqCount.value }).map(normalizeObjQ)
    }
    allObjQs.value[id] = qs
    allSubjQs.value[id] = content.getSubjectiveQuestions(id, { limit: subjCount.value })
    objAnswers.value[id] = {}
    subjAnswers.value[id] = {}
  })
  await Promise.all(loadPromises)
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
function finishObjective() { subjCount.value > 0 ? (phase.value = 'subjective') : advanceSubject() }
function finishSubjective() { advanceSubject() }
function advanceSubject() {
  if (hasMoreSubjects.value) { currentSubjectIdx.value++; currentQIdx.value = 0; phase.value = 'objective' }
  else finish()
}
function finish() {
  clearInterval(timer)
  router.push('/app/competition/result/new')
}
function formatTime(s) {
  const m = Math.floor(s / 60); const sec = s % 60
  return String(m).padStart(2, '0') + ':' + String(sec).padStart(2, '0')
}
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.mt-setup-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 14px; padding: 1.5rem; }
.mt-setup-section { margin-bottom: 1.25rem; }
.mt-setup-label { font-size: 0.82rem; font-weight: 700; color: var(--t-text2); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.6rem; }
.mt-subject-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 0.5rem; }
.mt-subj-item { display: flex; align-items: center; gap: 0.5rem; padding: 0.55rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; cursor: pointer; font-size: 0.82rem; color: var(--t-text1); }
.mt-subj-item input { accent-color: #9c27b0; }
.mt-subj-active { border-color: #9c27b0; background: rgba(156,39,176,0.1); }
.mt-setup-row { display: flex; gap: 1rem; flex-wrap: wrap; }
.mt-setup-row .mt-setup-section { flex: 1; min-width: 180px; }
.mt-btn-row { display: flex; gap: 0.4rem; }
.mt-opt-btn { flex: 1; padding: 0.45rem 0; border-radius: 7px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text2); font-size: 0.82rem; font-weight: 600; cursor: pointer; }
.mt-opt-active { background: #9c27b0; color: white; border-color: #9c27b0; }
.mt-start-btn { width: 100%; margin-top: 0.5rem; padding: 0.85rem; border-radius: 10px; font-weight: 800; font-size: 0.95rem; background: #9c27b0; color: white; border: none; cursor: pointer; }
.mt-start-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.mt-exam-header { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1rem 1.25rem; }
.mt-exam-title-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.75rem; }
.mt-exam-sub-tag { font-size: 0.9rem; font-weight: 700; color: var(--t-text1); }
.mt-exam-progress { font-size: 0.75rem; color: var(--t-text3); margin-top: 0.2rem; }
.mt-timer { font-family: 'Orbitron', monospace; font-size: 1.1rem; font-weight: 700; color: #9c27b0; }
.mt-timer-red { color: #ef5350; }
.mt-palette { display: flex; flex-wrap: wrap; gap: 0.3rem; }
.mt-pal-btn { width: 30px; height: 30px; border-radius: 5px; border: 1px solid var(--t-border); background: #555; color: white; font-size: 0.72rem; font-weight: 700; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.mt-pal-done { background: #9c27b0 !important; }
.mt-pal-cur { outline: 2px solid #00bcd4; outline-offset: 1px; }
.mt-q-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem 1.5rem; display: flex; flex-direction: column; gap: 0.6rem; }
.mt-q-num { font-size: 0.8rem; color: var(--t-text3); }
.mt-q-stem { font-size: 1rem; color: var(--t-text1); line-height: 1.6; }
.mt-options { display: flex; flex-direction: column; border-top: 1px solid var(--t-border); margin-top: 0.25rem; }
.mt-opt { display: flex; align-items: center; gap: 0.75rem; padding: 0.65rem 0; border-bottom: 1px solid var(--t-border); cursor: pointer; color: var(--t-text1); font-size: 0.9rem; }
.mt-opt:last-child { border-bottom: none; }
.mt-opt-sel { background: rgba(156,39,176,0.08); }
.mt-nav { display: flex; gap: 0.5rem; }
.mt-nav-btn { padding: 0.5rem 1.25rem; border-radius: 8px; font-size: 0.85rem; font-weight: 600; background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2); cursor: pointer; }
.mt-nav-btn:disabled { opacity: 0.3; cursor: not-allowed; }
.mt-next-btn { padding: 0.6rem 1.5rem; border-radius: 8px; font-size: 0.85rem; font-weight: 700; background: #9c27b0; color: white; border: none; cursor: pointer; margin-left: auto; }
.mt-subj-q-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem 1.5rem; display: flex; flex-direction: column; gap: 0.5rem; }
.mt-subj-input-row { display: flex; gap: 0.75rem; align-items: flex-start; }
.mt-subj-textarea { flex: 1; padding: 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; resize: vertical; }
.mt-mic-btn { padding: 0.5rem 0.75rem; border-radius: 8px; border: 1px solid var(--t-border); background: var(--t-surface); cursor: pointer; font-size: 1.1rem; flex-shrink: 0; }
.mt-loading { text-align: center; padding: 3rem 1rem; }
.mt-loading-text { font-size: 1.1rem; color: var(--t-text3); }

.mt-q-card :deep(.katex-display) { margin: 0.4rem 0; overflow-x: auto; }
.mt-q-card :deep(.katex) { font-size: 1em; color: var(--t-text1); }
</style>
