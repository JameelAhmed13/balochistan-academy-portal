<template>
  <div class="animate-fade-in space-y-5" v-if="subject">
    <div class="flex items-center gap-4">
      <button @click="$router.push('/app/preparation/' + bookId)" class="btn-ghost"><ArrowLeft class="w-4 h-4" /> Back</button>
      <div>
        <h2 class="section-title">{{ subject.name }} — Subjective Paper</h2>
        <p class="text-sm" style="color:var(--t-text3)">Study mode with model answers</p>
      </div>
    </div>

    <!-- Board pattern banner -->
    <div class="sp-board">
      <ScrollText class="w-4 h-4 shrink-0" />
      <span><strong>{{ boardName }}</strong> · {{ pattern.label }} — short {{ pattern.short.marksEach }}m · long {{ pattern.long.marksEach }}m</span>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- Filters -->
      <div class="card p-5 space-y-4 h-fit">
        <h3 class="font-semibold text-sm" style="color:var(--t-text1)">Paper Configuration</h3>
        <div>
          <label class="label">Question Types</label>
          <div class="space-y-2">
            <label v-for="t in ['Short', 'Long']" :key="t" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.type" :value="t" class="sp-check rounded" />
              <span class="text-sm" style="color:var(--t-text2)">{{ t }} Questions</span>
            </label>
          </div>
        </div>
        <div>
          <label class="label">Difficulty</label>
          <div class="space-y-1">
            <label v-for="d in ['Easy','Medium','Hard']" :key="d" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.difficulty" :value="d" class="sp-check rounded" />
              <span class="text-sm" style="color:var(--t-text2)">{{ d }}</span>
            </label>
          </div>
        </div>
        <div>
          <label class="label">Count per type</label>
          <div class="flex gap-2 items-center">
            <button @click="filters.limit = Math.max(1, filters.limit - 1)" class="btn-ghost p-1">-</button>
            <span class="w-8 text-center font-medium" style="color:var(--t-text2)">{{ filters.limit }}</span>
            <button @click="filters.limit = Math.min(50, filters.limit + 1)" class="btn-ghost p-1">+</button>
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
            <button @click="generate" :disabled="aiLoading" class="btn-secondary flex-1 justify-center text-xs">
              <Zap class="w-4 h-4" /> Question Bank
            </button>
            <RouterLink :to="'/app/preparation/' + bookId + '/test/subjective'" class="btn-secondary flex-1 justify-center text-xs">
              <Timer class="w-4 h-4" /> Timed Test
            </RouterLink>
          </div>
        </div>
      </div>

      <!-- Questions -->
      <div class="lg:col-span-2 space-y-4">
        <div v-if="aiLoading" class="card p-16 text-center" style="color:var(--t-text3)">
          <Sparkles class="w-16 h-16 mx-auto mb-4 sp-spin" style="color:var(--t-accent)" />
          <p class="text-sm">{{ boardName }} paper setter is writing your {{ aiMode === 'predicted' ? 'predicted ' : '' }}questions…</p>
          <p class="text-xs mt-1">Running {{ ollamaConfig.MODEL }} locally — this can take a moment.</p>
        </div>
        <div v-else-if="!generated" class="card p-16 text-center" style="color:var(--t-text3)">
          <BookOpen class="w-16 h-16 mx-auto mb-4 opacity-30" />
          <p class="text-sm">Configure and click <strong>AI Paper</strong> or <strong>Question Bank</strong> to see questions</p>
        </div>
        <template v-else>
          <div class="flex items-center justify-between flex-wrap gap-2">
            <span class="text-sm font-medium" style="color:var(--t-text2)">{{ questions.length }} questions</span>
            <span v-if="sourceBadge" :class="sourceBadge.cls">{{ sourceBadge.text }}</span>
          </div>
          <p v-if="sourceNote" class="text-xs" style="color:var(--t-text3)">{{ sourceNote }}</p>
          <div v-for="(q, i) in questions" :key="q.id" class="card p-5 space-y-3">
            <div class="flex items-center gap-2 flex-wrap">
              <span class="text-sm font-semibold" style="color:var(--t-text3)">#{{ i + 1 }}</span>
              <span :class="q.type === 'Short' ? 'badge-blue' : 'badge-purple'">{{ q.type }}</span>
              <span :class="diffClass(q.difficulty)">{{ q.difficulty }}</span>
              <span class="badge-indigo">{{ q.cognitiveLevel }}</span>
              <span class="badge-amber">{{ q.marks }} marks</span>
            </div>
            <p class="font-medium" style="color:var(--t-text1)">{{ q.stem }}</p>
            <button @click="toggleAnswer(q.id)" class="btn-secondary text-xs">
              <Eye class="w-3.5 h-3.5" /> {{ shown.has(q.id) ? 'Hide' : 'Show' }} Answer
            </button>
            <Transition name="fade-in">
              <div v-if="shown.has(q.id)" class="sp-answer">
                <div class="text-xs font-semibold mb-1">Model Answer</div>
                <p class="text-sm leading-relaxed">{{ q.modelAnswer }}</p>
              </div>
            </Transition>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { ArrowLeft, Zap, Timer, BookOpen, Eye, Sparkles, Wand2, ScrollText } from '@lucide/vue'
import { useContentStore } from '@/stores/content'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { useAuthStore } from '@/stores/auth'
import { patternFor } from '@/data/boardPattern'
import { generateSubjectiveQuestions, generatePredictedPaper, ollamaConfig } from '@/services/ollamaService'

const props = defineProps({ bookId: String })
const content = useContentStore()
const auth = useAuthStore()
const subject = computed(() => findPrepSubject(props.bookId))

const boardName = computed(() => (auth.user?.board ? `${auth.user.board} Board` : 'Balochistan Board') + ' (BISE)')
const pattern = computed(() => patternFor({ grade: auth.user?.gradeCode, bookId: +props.bookId }))

const filters = ref({ type: ['Short', 'Long'], difficulty: ['Easy', 'Medium', 'Hard'], limit: 10 })
const questions = ref([])
const generated = ref(false)
const shown = ref(new Set())
const aiLoading = ref(false)
const aiMode = ref('')
const sourceNote = ref('')
const sourceBadge = ref(null)

function generate() {
  questions.value = content.getSubjectiveQuestions(+props.bookId, filters.value)
  generated.value = true
  sourceBadge.value = { text: 'Question Bank', cls: 'badge-indigo' }
  sourceNote.value = ''
}

async function generateWithAI(mode) {
  aiLoading.value = true
  aiMode.value = mode
  sourceNote.value = ''
  sourceBadge.value = null
  const type = filters.value.type.length === 1 ? filters.value.type[0] : undefined
  const count = type === 'Long' ? pattern.value.long.of : (type === 'Short' ? pattern.value.short.of : pattern.value.short.of + pattern.value.long.of)
  const diff = filters.value.difficulty.length === 1 ? filters.value.difficulty[0] : undefined

  let qs = []
  try {
    if (mode === 'predicted') {
      qs = await generatePredictedPaper({ subject: subject.value.name, grade: auth.user?.gradeCode || 9, board: boardName.value, count, kind: 'subjective' })
    } else {
      qs = await generateSubjectiveQuestions({ subject: subject.value.name, grade: auth.user?.gradeCode || 9, board: boardName.value, count, type, difficulty: diff })
    }
  } catch { qs = [] }

  if (qs.length) {
    questions.value = qs
    sourceBadge.value = { text: mode === 'predicted' ? '🔮 AI Predicted' : '✨ AI Generated', cls: 'badge-purple' }
    sourceNote.value = `Generated by ${ollamaConfig.MODEL} to the ${boardName.value} ${pattern.value.label} pattern.`
  } else {
    let fb = content.getSubjectiveQuestions(+props.bookId, { ...filters.value, limit: count })
    if (mode === 'predicted') {
      const pred = fb.filter(q => q.predicted)
      if (pred.length) fb = pred
    }
    questions.value = fb
    sourceBadge.value = { text: 'Question Bank', cls: 'badge-indigo' }
    sourceNote.value = 'AI generator was unavailable (start `ollama serve` and ensure llama3 is pulled) — used the question bank instead.'
  }
  generated.value = true
  aiLoading.value = false
}

function toggleAnswer(id) {
  const s = new Set(shown.value)
  s.has(id) ? s.delete(id) : s.add(id)
  shown.value = s
}

function diffClass(d) {
  return { Easy: 'badge-green', Medium: 'badge-amber', Hard: 'badge-red' }[d] || 'badge-indigo'
}
</script>

<style scoped>
.sp-board {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.6rem 0.9rem; border-radius: 10px; font-size: 0.78rem; color: var(--t-text2);
  background: color-mix(in srgb, var(--t-accent) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-accent) 22%, transparent);
}
.sp-board svg { color: var(--t-accent); }
.sp-check { accent-color: var(--t-accent); width: 15px; height: 15px; }
.sp-spin { animation: spSpin 1.4s linear infinite; }
@keyframes spSpin { to { transform: rotate(360deg); } }
.sp-answer {
  padding: 0.75rem; border-radius: 12px;
  background: color-mix(in srgb, #10b981 12%, transparent);
  border: 1px solid color-mix(in srgb, #10b981 28%, transparent);
}
.sp-answer .text-xs { color: #059669; }
html.dark .sp-answer .text-xs { color: #34d399; }
.sp-answer p { color: var(--t-text1); }
.fade-in-enter-active { transition: opacity 0.2s, transform 0.2s; }
.fade-in-enter-from { opacity: 0; transform: translateY(-4px); }
@media (prefers-reduced-motion: reduce) { .sp-spin { animation: none; } }
</style>
