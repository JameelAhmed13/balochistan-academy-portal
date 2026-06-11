<template>
  <div class="animate-fade-in space-y-5" v-if="subject">
    <!-- Header -->
    <div class="flex items-center gap-4">
      <button @click="$router.push('/app/preparation/' + bookId)" class="btn-ghost"><ArrowLeft class="w-4 h-4" /> Back</button>
      <div>
        <h2 class="section-title">{{ subject.name }} â€” Objective Paper</h2>
        <p class="text-sm text-slate-500">Configure your paper then start preparation</p>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- Filters Panel -->
      <div class="lg:col-span-1 card p-5 space-y-5 h-fit">
        <h3 class="font-semibold text-slate-700 text-sm">Paper Configuration</h3>

        <!-- Entrance Exam -->
        <div>
          <label class="label">Entrance Exam</label>
          <div class="flex gap-2">
            <button v-for="v in [null, true, false]" :key="String(v)"
              @click="filters.entranceExam = v"
              :class="['btn-secondary text-xs flex-1', filters.entranceExam === v ? 'bg-brand-600 text-white border-brand-600' : '']"
            >{{ v === null ? 'All' : v ? 'Yes' : 'No' }}</button>
          </div>
        </div>

        <!-- Question Type -->
        <div>
          <label class="label">Question Type</label>
          <div class="flex gap-2 flex-wrap">
            <label v-for="t in ['Exercise', 'Conceptual']" :key="t" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.type" :value="t" class="op-check rounded" />
              <span class="text-sm text-slate-700">{{ t }}</span>
            </label>
          </div>
        </div>

        <!-- Difficulty -->
        <div>
          <label class="label">Difficulty Level</label>
          <div class="flex gap-2 flex-wrap">
            <label v-for="d in ['Easy', 'Medium', 'Hard']" :key="d" class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="filters.difficulty" :value="d" class="op-check rounded" />
              <span class="text-sm text-slate-700">{{ d }}</span>
            </label>
          </div>
        </div>

        <!-- Cognitive Level Quantities -->
        <div>
          <label class="label">Cognitive Level Count</label>
          <div class="space-y-2">
            <div v-for="cl in cogLevels" :key="cl.key" class="flex items-center gap-3">
              <span class="text-xs text-slate-600 w-28">{{ cl.label }}</span>
              <input type="number" v-model.number="filters.cogCount[cl.key]" min="0" max="50"
                class="input w-20 text-center text-sm py-1.5" />
            </div>
          </div>
        </div>

        <!-- Units tree -->
        <div>
          <label class="label">Units / Topics</label>
          <div class="max-h-48 overflow-y-auto space-y-1">
            <label class="flex items-center gap-2 cursor-pointer p-1 rounded hover:bg-slate-50">
              <input type="checkbox" v-model="allUnits" class="op-check rounded" />
              <span class="text-sm font-medium text-slate-700">All Units</span>
            </label>
            <div v-for="u in units" :key="u.id">
              <label class="flex items-center gap-2 cursor-pointer p-1 rounded hover:bg-slate-50">
                <input type="checkbox" v-model="filters.units" :value="u.id" class="op-check rounded" />
                <span class="text-sm text-slate-700">{{ u.name }}</span>
              </label>
            </div>
          </div>
        </div>

        <div class="flex gap-2">
          <button @click="generatePaper" class="btn-primary flex-1 justify-center">
            <Zap class="w-4 h-4" /> Start Preparation
          </button>
          <button @click="startTest" class="btn-secondary flex-1 justify-center text-xs">
            <Timer class="w-4 h-4" /> Graded Test
          </button>
        </div>
      </div>

      <!-- Questions View -->
      <div class="lg:col-span-2 space-y-4">
        <div v-if="!generated" class="card p-16 flex flex-col items-center text-slate-400">
          <BookOpen class="w-16 h-16 mb-4 opacity-30" />
          <p class="text-sm">Configure your filters and click <strong>Start Preparation</strong></p>
        </div>

        <template v-else>
          <div class="flex items-center justify-between">
            <span class="text-sm text-slate-600 font-medium">{{ questions.length }} questions generated</span>
            <div class="flex gap-2">
              <span class="badge-blue">Study Mode</span>
              <span class="badge-green">Answers Shown</span>
            </div>
          </div>

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
import { ArrowLeft, Zap, Timer, BookOpen } from '@lucide/vue'
import { SUBJECTS, useContentStore } from '@/stores/content'
import ObjectiveQuestionCard from '@/components/test/ObjectiveQuestionCard.vue'

const props = defineProps({ bookId: String })
const router = useRouter()
const content = useContentStore()

const subject = computed(() => SUBJECTS.find(s => s.id === +props.bookId))
const units = computed(() => content.units[+props.bookId] || [])

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

const cogLevels = [
  { key: 'Knowledge', label: 'Knowledge' },
  { key: 'Understanding', label: 'Understanding' },
  { key: 'Applying', label: 'Applying' },
]

watch(allUnits, v => { if (v) filters.value.units = [] })

function generatePaper() {
  questions.value = content.getQuestions(+props.bookId, {
    ...filters.value,
    limit: Object.values(filters.value.cogCount).reduce((a, b) => a + b, 0) || 30,
  })
  generated.value = true
}

function startTest() {
  router.push(`/app/preparation/${props.bookId}/test/objective`)
}
</script>

