<template>
  <div class="animate-fade-in space-y-5" v-if="subject">
    <!-- Test header -->
    <div class="card p-4 flex items-center gap-4 flex-wrap">
      <button v-if="!started && !finished" @click="$router.go(-1)" class="btn-ghost"><ArrowLeft class="w-4 h-4" /></button>
      <div class="flex-1">
        <h2 class="section-title">{{ subject.name }} — Objective Test</h2>
        <p class="text-xs text-slate-500">{{ questions.length }} questions · Graded mode</p>
      </div>
      <!-- Timer -->
      <div v-if="started && !finished" class="flex items-center gap-2 px-4 py-2 rounded-xl bg-slate-50 border border-slate-200">
        <Timer class="w-4 h-4 text-brand-600" />
        <span class="font-mono text-lg font-bold" :class="timeLeft < 60 ? 'text-red-600' : 'text-slate-800'">{{ formatTime(timeLeft) }}</span>
      </div>
      <!-- Score -->
      <div v-if="finished" class="flex items-center gap-2">
        <div class="text-center px-4 py-2 rounded-xl" :class="scorePercent >= 50 ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-700'">
          <div class="text-2xl font-bold">{{ score }}/{{ questions.length }}</div>
          <div class="text-xs font-medium">{{ scorePercent }}%</div>
        </div>
      </div>
    </div>

    <!-- Not started -->
    <div v-if="!started" class="card p-10 flex flex-col items-center gap-4 text-center">
      <div class="text-5xl">📝</div>
      <h3 class="text-lg font-semibold text-slate-800">Ready to test your knowledge?</h3>
      <p class="text-sm text-slate-500 max-w-md">This is a graded test with {{ questions.length }} objective questions. Answers are hidden until you submit each question.</p>
      <button @click="startTest" class="btn-primary text-base px-6 py-3"><Play class="w-5 h-5" /> Start Test</button>
    </div>

    <!-- Test in progress -->
    <template v-else-if="!finished">
      <!-- Progress bar -->
      <div class="h-2 rounded-full bg-slate-100 overflow-hidden">
        <div class="h-full bg-gradient-to-r from-brand-500 to-brand-400 rounded-full transition-all duration-500"
          :style="{ width: (answered / questions.length * 100) + '%' }" />
      </div>
      <div class="text-xs text-slate-500 text-right">{{ answered }} / {{ questions.length }} answered</div>

      <ObjectiveQuestionCard
        v-for="(q, i) in questions"
        :key="q.id"
        :question="q"
        :index="i"
        :test-mode="true"
        @answer="onAnswer"
      />

      <div class="flex justify-end">
        <button @click="finishTest" class="btn-primary"><CheckCircle class="w-4 h-4" /> Finish Test</button>
      </div>
    </template>

    <!-- Results -->
    <template v-else>
      <div class="card p-6 space-y-4">
        <div class="text-center">
          <div class="text-5xl mb-2">{{ scorePercent >= 70 ? '🏆' : scorePercent >= 50 ? '👍' : '📚' }}</div>
          <h3 class="text-xl font-bold text-slate-800">{{ scorePercent >= 50 ? 'Test Passed!' : 'Keep Practicing!' }}</h3>
          <p class="text-slate-500 text-sm mt-1">You scored {{ score }} out of {{ questions.length }} ({{ scorePercent }}%)</p>
        </div>
        <div class="grid grid-cols-3 gap-3 text-center">
          <div class="p-3 rounded-xl bg-emerald-50">
            <div class="text-2xl font-bold text-emerald-700">{{ score }}</div>
            <div class="text-xs text-emerald-600">Correct</div>
          </div>
          <div class="p-3 rounded-xl bg-red-50">
            <div class="text-2xl font-bold text-red-700">{{ questions.length - score }}</div>
            <div class="text-xs text-red-600">Wrong</div>
          </div>
          <div class="p-3 rounded-xl bg-blue-50">
            <div class="text-2xl font-bold text-blue-700">{{ scorePercent }}%</div>
            <div class="text-xs text-blue-600">Score</div>
          </div>
        </div>
        <div class="flex gap-3 justify-center">
          <button @click="retake" class="btn-secondary"><RefreshCw class="w-4 h-4" /> Retake Test</button>
          <RouterLink to="/app/preparation" class="btn-primary"><BookOpen class="w-4 h-4" /> Back to Library</RouterLink>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onUnmounted } from 'vue'
import { ArrowLeft, Timer, Play, CheckCircle, RefreshCw, BookOpen } from '@lucide/vue'
import { SUBJECTS, useContentStore } from '@/stores/content'
import { useStudentStore } from '@/stores/student'
import ObjectiveQuestionCard from '@/components/test/ObjectiveQuestionCard.vue'

const props = defineProps({ bookId: String })
const content = useContentStore()
const student = useStudentStore()

const subject = computed(() => SUBJECTS.find(s => s.id === +props.bookId))
const questions = ref(content.getQuestions(+props.bookId, { limit: 35 }))
const answers = ref({})
const started = ref(false)
const finished = ref(false)
const timeLeft = ref(35 * 60)
let timer = null

const answered  = computed(() => Object.keys(answers.value).length)
const score     = computed(() => Object.values(answers.value).filter(a => a.selected === a.correct).length)
const scorePercent = computed(() => Math.round(score.value / questions.value.length * 100))

function startTest() {
  started.value = true
  timer = setInterval(() => {
    if (timeLeft.value > 0) timeLeft.value--
    else finishTest()
  }, 1000)
}

function onAnswer(data) {
  answers.value[data.questionId] = data
}

function finishTest() {
  clearInterval(timer)
  finished.value = true
  student.saveTest({
    subject: subject.value?.name,
    type: 'Objective Test',
    score: score.value,
    total: questions.value.length,
    bookId: +props.bookId,
  })
}

function retake() {
  questions.value = content.getQuestions(+props.bookId, { limit: 35 })
  answers.value = {}
  started.value = false
  finished.value = false
  timeLeft.value = 35 * 60
}

function formatTime(s) {
  const m = Math.floor(s / 60).toString().padStart(2, '0')
  const sec = (s % 60).toString().padStart(2, '0')
  return `${m}:${sec}`
}

onUnmounted(() => clearInterval(timer))
</script>

