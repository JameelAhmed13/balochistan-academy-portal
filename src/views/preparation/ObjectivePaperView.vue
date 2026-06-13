<template>
  <div class="animate-fade-in space-y-5" v-if="subject">
    <!-- Header -->
    <div class="flex items-center gap-4">
      <button @click="$router.push('/app/preparation/' + bookId)" class="btn-ghost"><ArrowLeft class="w-4 h-4" /> Back</button>
      <div>
        <h2 class="section-title">{{ subject.name }} — Objective Paper</h2>
        <p class="text-sm" style="color:var(--t-text3)">Configure your paper, then generate it with AI or the question bank</p>
      </div>
    </div>

    <!-- Board pattern banner -->
    <div class="op-board">
      <ScrollText class="w-4 h-4 shrink-0" />
      <span><strong>{{ boardName }}</strong> · {{ pattern.label }} pattern — {{ patternLine }}</span>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- Filters Panel -->
      <div class="lg:col-span-1 card p-5 space-y-5 h-fit">
        <h3 class="font-semibold text-sm" style="color:var(--t-text1)">Paper Configuration</h3>

        <!-- Entrance Exam -->
        <div>
          <label class="label">Entrance Exam</label>
          <div class="flex gap-2">
            <button v-for="v in [null, true, false]" :key="String(v)"
              @click="filters.entranceExam = v"
              :class="['btn-secondary text-xs flex-1', filters.entranceExam === v ? 'op-on' : '']"
            >{{ v === null ? 'All' : v ? 'Yes' : 'No' }}</button>
          </div>
        </div>

        <!-- Question Type -->
        <div>
          <label class="label">Question Type</label>
          <div class="flex gap-2 flex-wrap">
            <label v-for="t in ['Exercise', 'Conceptual']" :key="t" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.type" :value="t" class="op-check rounded" />
              <span class="text-sm" style="color:var(--t-text2)">{{ t }}</span>
            </label>
          </div>
        </div>

        <!-- Difficulty -->
        <div>
          <label class="label">Difficulty Level</label>
          <div class="flex gap-2 flex-wrap">
            <label v-for="d in ['Easy', 'Medium', 'Hard']" :key="d" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.difficulty" :value="d" class="op-check rounded" />
              <span class="text-sm" style="color:var(--t-text2)">{{ d }}</span>
            </label>
          </div>
        </div>

        <!-- Cognitive Level Quantities -->
        <div>
          <label class="label">Cognitive Level Count</label>
          <div class="space-y-2">
            <div v-for="cl in cogLevels" :key="cl.key" class="flex items-center gap-3">
              <span class="text-xs w-28" style="color:var(--t-text3)">{{ cl.label }}</span>
              <input type="number" v-model.number="filters.cogCount[cl.key]" min="0" max="50"
                class="input w-20 text-center text-sm py-1.5" />
            </div>
          </div>
        </div>

        <!-- Units tree -->
        <div>
          <label class="label">Units / Topics</label>
          <div class="max-h-48 overflow-y-auto space-y-1">
            <label class="op-row flex items-center gap-2 cursor-pointer p-1 rounded">
              <input type="checkbox" v-model="allUnits" class="op-check rounded" />
              <span class="text-sm font-medium" style="color:var(--t-text2)">All Units</span>
            </label>
            <div v-for="u in units" :key="u.id">
              <label class="op-row flex items-center gap-2 cursor-pointer p-1 rounded">
                <input type="checkbox" v-model="filters.units" :value="u.id" class="op-check rounded" />
                <span class="text-sm" style="color:var(--t-text2)">{{ u.name }}</span>
              </label>
            </div>
          </div>
        </div>

        <div class="space-y-2">
          <div class="flex gap-2">
            <button @click="generateWithAI('paper')" :disabled="aiLoading" class="btn-primary flex-1 justify-center">
              <Sparkles class="w-4 h-4" /> {{ aiLoading && aiMode === 'paper' ? 'Generating…' : 'Saathi Paper' }}
            </button>
            <button @click="generateWithAI('predicted')" :disabled="aiLoading" class="btn-secondary flex-1 justify-center text-xs">
              <Wand2 class="w-4 h-4" /> {{ aiLoading && aiMode === 'predicted' ? 'Predicting…' : 'Predicted' }}
            </button>
          </div>
          <div class="flex gap-2">
            <button @click="generateBoard" :disabled="aiLoading" class="btn-primary flex-1 justify-center text-xs">
              <ScrollText class="w-4 h-4" /> Board Questions
            </button>
            <button @click="startTest" :disabled="aiLoading" class="btn-secondary flex-1 justify-center text-xs">
              <Timer class="w-4 h-4" /> Graded Test
            </button>
          </div>
        </div>
      </div>

      <!-- Questions View -->
      <div class="lg:col-span-2 space-y-4">
        <!-- AI loading -->
        <div v-if="aiLoading" class="card p-16 flex flex-col items-center" style="color:var(--t-text3)">
          <Sparkles class="w-16 h-16 mb-4 op-spin" style="color:var(--t-accent)" />
          <p class="text-sm">{{ aiMode === 'board' ? 'Loading real board questions…' : (boardName + ' paper setter is generating your ' + (aiMode === 'predicted' ? 'predicted ' : '') + 'questions…') }}</p>
          <p v-if="aiMode !== 'board'" class="text-xs mt-1">Running {{ ollamaConfig.MODEL }} locally — this can take a moment.</p>
        </div>

        <div v-else-if="!generated" class="card p-16 flex flex-col items-center" style="color:var(--t-text3)">
          <BookOpen class="w-16 h-16 mb-4 opacity-30" />
          <p class="text-sm">Configure your filters, then click <strong>AI Paper</strong> or <strong>Question Bank</strong></p>
        </div>

        <template v-else>
          <div class="flex items-center justify-between flex-wrap gap-2">
            <span class="text-sm font-medium" style="color:var(--t-text2)">{{ questions.length }} questions</span>
            <div class="flex gap-2 flex-wrap">
              <span v-if="sourceBadge" :class="sourceBadge.cls">{{ sourceBadge.text }}</span>
              <span class="badge-blue">Study Mode</span>
              <span class="badge-green">Answers Shown</span>
            </div>
          </div>
          <p v-if="sourceNote" class="text-xs" style="color:var(--t-text3)">{{ sourceNote }}</p>

          <ObjectiveQuestionCard
            v-for="(q, i) in questions"
            :key="q.id"
            :question="q"
            :index="i"
            :show-answer="true"
            :test-mode="false"
          />
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ArrowLeft, Zap, Timer, BookOpen, Sparkles, Wand2, ScrollText } from '@lucide/vue'
import { useContentStore } from '@/stores/content'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { useAuthStore } from '@/stores/auth'
import { patternFor, patternSummary } from '@/data/boardPattern'
import { generateObjectiveQuestions, generatePredictedPaper, ollamaConfig } from '@/services/ollamaService'
import { getRealQuestions, realContextSnippet } from '@/services/assessmentBank'
import ObjectiveQuestionCard from '@/components/test/ObjectiveQuestionCard.vue'

const props = defineProps({ bookId: String })
const router = useRouter()
const content = useContentStore()
const auth = useAuthStore()

const subject = computed(() => findPrepSubject(props.bookId))
const units = computed(() => content.units[+props.bookId] || [])

const boardName = computed(() => (auth.user?.board ? `${auth.user.board} Board` : 'Balochistan Board') + ' (BISE)')
const pattern = computed(() => patternFor({ grade: auth.user?.grade, bookId: +props.bookId }))
const patternLine = computed(() => patternSummary(pattern.value))

const filters = ref({
  entranceExam: null,
  type: ['Exercise', 'Conceptual'],
  difficulty: ['Easy', 'Medium', 'Hard'],
  cogCount: { Knowledge: 10, Understanding: 10, Applying: 5 },
  units: [],
})

const allUnits = ref(true)
const generated = ref(false)
const questions = ref([])
const aiLoading = ref(false)
const aiMode = ref('')          // 'paper' | 'predicted'
const sourceNote = ref('')
const sourceBadge = ref(null)

const cogLevels = [
  { key: 'Knowledge', label: 'Knowledge' },
  { key: 'Understanding', label: 'Understanding' },
  { key: 'Applying', label: 'Applying' },
]

watch(allUnits, v => { if (v) filters.value.units = [] })

const selectedTopicLabel = computed(() => {
  if (!filters.value.units.length) return ''
  return units.value.filter(u => filters.value.units.includes(u.id)).map(u => u.name).join(', ')
})

function generatePaper() {
  questions.value = content.getQuestions(+props.bookId, {
    ...filters.value,
    limit: Object.values(filters.value.cogCount).reduce((a, b) => a + b, 0) || 30,
  })
  generated.value = true
  sourceBadge.value = { text: 'Question Bank', cls: 'badge-indigo' }
  sourceNote.value = ''
}

async function generateWithAI(mode) {
  aiLoading.value = true
  aiMode.value = mode
  sourceNote.value = ''
  sourceBadge.value = null
  const count = pattern.value.objective.count
  const diff = filters.value.difficulty.length === 1 ? filters.value.difficulty[0] : undefined

  let qs = []
  try {
    if (mode === 'predicted') {
      qs = await generatePredictedPaper({
        subject: subject.value.name,
        grade: auth.user?.grade || 9,
        board: boardName.value,
        count,
        kind: 'objective',
      })
    } else {
      // Ground the AI on REAL board questions for this grade+subject (RAG).
      let examples = ''
      try { examples = await realContextSnippet({ grade: auth.user?.grade || 9, subject: subject.value.name, sample: 6 }) } catch { examples = '' }
      qs = await generateObjectiveQuestions({
        subject: subject.value.name,
        grade: auth.user?.grade || 9,
        board: boardName.value,
        topic: selectedTopicLabel.value,
        count,
        difficulty: diff,
        examples,
      })
    }
  } catch { qs = [] }

  if (qs.length) {
    questions.value = qs
    sourceBadge.value = { text: mode === 'predicted' ? '🔮 AI Predicted' : '✨ AI Generated', cls: 'badge-purple' }
    sourceNote.value = `Generated by ${ollamaConfig.MODEL} to the ${boardName.value} ${pattern.value.label} pattern.`
  } else {
    // Graceful fallback — always produce a usable paper.
    const fb = mode === 'predicted' ? { entranceExam: true } : {}
    questions.value = content.getQuestions(+props.bookId, { ...filters.value, ...fb, limit: count })
    sourceBadge.value = { text: 'Question Bank', cls: 'badge-indigo' }
    sourceNote.value = 'AI generator was unavailable (start `ollama serve` and ensure llama3 is pulled) — used the question bank instead.'
  }
  generated.value = true
  aiLoading.value = false
}

async function generateBoard() {
  aiLoading.value = true
  aiMode.value = 'board'
  sourceNote.value = ''
  sourceBadge.value = null
  const count = pattern.value.objective.count
  let qs = []
  try { qs = await getRealQuestions({ grade: auth.user?.grade || 9, subject: subject.value.name, limit: count }) } catch { qs = [] }
  if (qs.length) {
    questions.value = qs
    sourceBadge.value = { text: '📋 Real Board Questions', cls: 'badge-green' }
    sourceNote.value = `Real ${boardName.value} questions for Grade ${auth.user?.grade || 9} ${subject.value.name} — from the ingested assessment bank.`
  } else {
    questions.value = content.getQuestions(+props.bookId, { ...filters.value, limit: count })
    sourceBadge.value = { text: 'Question Bank', cls: 'badge-indigo' }
    sourceNote.value = `No ingested board bank for Grade ${auth.user?.grade || 9} ${subject.value.name} yet — used the local bank.`
  }
  generated.value = true
  aiLoading.value = false
}

function startTest() {
  router.push(`/app/preparation/${props.bookId}/test/objective`)
}
</script>

<style scoped>
.op-board {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.6rem 0.9rem; border-radius: 10px; font-size: 0.78rem;
  color: var(--t-text2);
  background: color-mix(in srgb, var(--t-accent) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-accent) 22%, transparent);
}
.op-board svg { color: var(--t-accent); }
.op-on {
  background: var(--t-accent) !important;
  color: #fff !important;
  border-color: var(--t-accent) !important;
}
.op-check { accent-color: var(--t-accent); width: 15px; height: 15px; }
.op-row:hover { background: var(--t-hover); }
.op-spin { animation: opSpin 1.4s linear infinite; }
@keyframes opSpin { to { transform: rotate(360deg); } }
@media (prefers-reduced-motion: reduce) { .op-spin { animation: none; } }
</style>
