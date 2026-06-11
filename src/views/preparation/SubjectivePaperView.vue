<template>
  <div class="animate-fade-in space-y-5" v-if="subject">
    <div class="flex items-center gap-4">
      <button @click="$router.push('/app/preparation/' + bookId)" class="btn-ghost"><ArrowLeft class="w-4 h-4" /> Back</button>
      <div>
        <h2 class="section-title">{{ subject.name }} â€” Subjective Paper</h2>
        <p class="text-sm text-slate-500">Study mode with model answers</p>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- Filters -->
      <div class="card p-5 space-y-4 h-fit">
        <h3 class="font-semibold text-slate-700 text-sm">Paper Configuration</h3>
        <div>
          <label class="label">Question Types</label>
          <div class="space-y-2">
            <label v-for="t in ['Short', 'Long']" :key="t" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.type" :value="t" class="rounded text-brand-600" />
              <span class="text-sm text-slate-700">{{ t }} Questions</span>
            </label>
          </div>
        </div>
        <div>
          <label class="label">Difficulty</label>
          <div class="space-y-1">
            <label v-for="d in ['Easy','Medium','Hard']" :key="d" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.difficulty" :value="d" class="rounded text-brand-600" />
              <span class="text-sm text-slate-700">{{ d }}</span>
            </label>
          </div>
        </div>
        <div>
          <label class="label">Count per type</label>
          <div class="flex gap-2 items-center">
            <button @click="filters.limit = Math.max(1, filters.limit - 1)" class="btn-ghost p-1">-</button>
            <span class="w-8 text-center font-medium text-slate-700">{{ filters.limit }}</span>
            <button @click="filters.limit = Math.min(50, filters.limit + 1)" class="btn-ghost p-1">+</button>
          </div>
        </div>
        <div class="flex gap-2">
          <button @click="generate" class="btn-primary flex-1 justify-center">
            <Zap class="w-4 h-4" /> Generate
          </button>
          <RouterLink :to="'/app/preparation/' + bookId + '/test/subjective'" class="btn-secondary flex-1 justify-center text-xs">
            <Timer class="w-4 h-4" /> Timed Test
          </RouterLink>
        </div>
      </div>

      <!-- Questions -->
      <div class="lg:col-span-2 space-y-4">
        <div v-if="!generated" class="card p-16 text-center text-slate-400">
          <BookOpen class="w-16 h-16 mx-auto mb-4 opacity-30" />
          <p class="text-sm">Configure and click <strong>Generate</strong> to see questions</p>
        </div>
        <template v-else>
          <div class="flex items-center justify-between">
            <span class="text-sm font-medium text-slate-600">{{ questions.length }} questions</span>
          </div>
          <div v-for="(q, i) in questions" :key="q.id" class="card p-5 space-y-3">
            <div class="flex items-center gap-2 flex-wrap">
              <span class="text-sm font-semibold text-slate-400">#{{ i + 1 }}</span>
              <span :class="q.type === 'Short' ? 'badge-blue' : 'badge-purple'">{{ q.type }}</span>
              <span :class="diffClass(q.difficulty)">{{ q.difficulty }}</span>
              <span class="badge-indigo">{{ q.cognitiveLevel }}</span>
              <span class="badge-amber">{{ q.marks }} marks</span>
            </div>
            <p class="font-medium text-slate-800">{{ q.stem }}</p>
            <button @click="toggleAnswer(q.id)" class="btn-secondary text-xs">
              <Eye class="w-3.5 h-3.5" /> {{ shown.has(q.id) ? 'Hide' : 'Show' }} Answer
            </button>
            <Transition name="fade-in">
              <div v-if="shown.has(q.id)" class="p-3 rounded-xl bg-emerald-50 border border-emerald-100">
                <div class="text-xs font-semibold text-emerald-700 mb-1">Model Answer</div>
                <p class="text-sm text-emerald-800 leading-relaxed">{{ q.modelAnswer }}</p>
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
import { ArrowLeft, Zap, Timer, BookOpen, Eye } from '@lucide/vue'
import { SUBJECTS, useContentStore } from '@/stores/content'

const props = defineProps({ bookId: String })
const content = useContentStore()
const subject = computed(() => SUBJECTS.find(s => s.id === +props.bookId))

const filters = ref({ type: ['Short', 'Long'], difficulty: ['Easy', 'Medium', 'Hard'], limit: 10 })
const questions = ref([])
const generated = ref(false)
const shown = ref(new Set())

function generate() {
  questions.value = content.getSubjectiveQuestions(+props.bookId, filters.value)
  generated.value = true
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

