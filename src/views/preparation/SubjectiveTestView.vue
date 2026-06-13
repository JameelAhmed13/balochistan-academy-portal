<template>
  <div class="animate-fade-in space-y-4" v-if="subject">
    <!-- Header -->
    <div class="card p-4 flex items-center gap-4 flex-wrap">
      <button v-if="!started" @click="$router.go(-1)" class="btn-ghost"><ArrowLeft class="w-4 h-4" /></button>
      <div class="flex-1">
        <h2 class="section-title">{{ subject.name }} — Subjective Self Test</h2>
        <p class="text-xs text-slate-500">{{ questions.length }} questions · Rich text answers · AI graded</p>
      </div>
      <div v-if="started && !finished" class="flex items-center gap-2 px-4 py-2 rounded-xl bg-slate-50 border border-slate-200">
        <Timer class="w-4 h-4 text-brand-600" />
        <span class="font-mono text-lg font-bold" :class="timeLeft < 120 ? 'text-red-600' : 'text-slate-800'">{{ formatTime(timeLeft) }}</span>
      </div>
    </div>

    <!-- Not started screen -->
    <div v-if="!started" class="card p-10 text-center flex flex-col items-center gap-4">
      <div class="text-5xl">✍️</div>
      <h3 class="text-lg font-semibold text-slate-800">Subjective Self Test</h3>
      <p class="text-sm text-slate-500 max-w-md">Answer {{ shortQs.length }} short questions and {{ longQs.length }} long questions. Your answers will be AI-graded against model answers.</p>
      <div class="flex gap-3 text-sm flex-wrap justify-center">
        <div class="badge-blue">Short: {{ shortQs.length }}</div>
        <div class="badge-purple">Long: {{ longQs.length }}</div>
        <div class="badge-amber">Time: {{ formatTime(timeLeft) }}</div>
      </div>
      <button @click="started = true" class="btn-primary text-base px-6 py-3"><Play class="w-5 h-5" /> Start Test</button>
    </div>

    <!-- Test layout -->
    <div v-else-if="!finished" class="flex gap-5 items-start">
      <!-- Main question -->
      <div class="flex-1 space-y-4">
        <div class="card p-5 space-y-4">
          <!-- Q meta -->
          <div class="flex items-center gap-2 flex-wrap">
            <span class="text-sm font-semibold text-slate-500">Q{{ currentIndex + 1 }}</span>
            <span :class="currentQ.type === 'Short' ? 'badge-blue' : 'badge-purple'">{{ currentQ.type }}</span>
            <span :class="diffBadge(currentQ.difficulty)">{{ currentQ.difficulty }}</span>
            <span class="badge-indigo">{{ currentQ.cognitiveLevel }}</span>
            <span class="badge-amber">{{ currentQ.marks }} marks</span>
          </div>
          <p class="text-slate-800 font-medium leading-relaxed text-base">{{ currentQ.stem }}</p>

          <!-- TipTap Editor -->
          <div class="border border-slate-200 rounded-xl overflow-hidden">
            <div class="flex items-center gap-1 px-3 py-2 bg-slate-50 border-b border-slate-200 flex-wrap">
              <button @click="editor?.chain().focus().toggleBold().run()" class="editor-btn" :class="editor?.isActive('bold') ? 'bg-slate-200' : ''"><Bold class="w-3.5 h-3.5" /></button>
              <button @click="editor?.chain().focus().toggleItalic().run()" class="editor-btn" :class="editor?.isActive('italic') ? 'bg-slate-200' : ''"><Italic class="w-3.5 h-3.5" /></button>
              <button @click="editor?.chain().focus().toggleBulletList().run()" class="editor-btn"><List class="w-3.5 h-3.5" /></button>
              <div class="h-4 w-px bg-slate-300 mx-1" />
              <button @click="toggleVoice" :class="['editor-btn', isListening ? 'bg-red-100 text-red-600' : '']" title="Voice input">
                <Mic class="w-3.5 h-3.5" />
              </button>
              <select v-model="voiceLang" class="text-xs border border-slate-200 rounded px-1 py-0.5 bg-white">
                <option value="en-US">EN</option>
                <option value="ur-PK">اردو</option>
              </select>
            </div>
            <EditorContent :editor="editor" class="p-3 min-h-[140px] text-sm text-slate-800 prose prose-sm max-w-none" />
          </div>
        </div>

        <!-- Navigation -->
        <div class="flex items-center justify-between">
          <div class="flex gap-2">
            <button @click="go(0)" :disabled="currentIndex === 0" class="btn-secondary text-xs"><ChevronsLeft class="w-3.5 h-3.5" /> First</button>
            <button @click="go(currentIndex - 1)" :disabled="currentIndex === 0" class="btn-secondary text-xs"><ChevronLeft class="w-3.5 h-3.5" /> Prev</button>
          </div>
          <span class="text-xs text-slate-500">{{ currentIndex + 1 }} / {{ questions.length }}</span>
          <div class="flex gap-2">
            <button @click="go(currentIndex + 1)" :disabled="currentIndex === questions.length - 1" class="btn-secondary text-xs">Next <ChevronRight class="w-3.5 h-3.5" /></button>
            <button @click="go(questions.length - 1)" :disabled="currentIndex === questions.length - 1" class="btn-secondary text-xs">Last <ChevronsRight class="w-3.5 h-3.5" /></button>
            <button @click="finishTest" class="btn-primary text-xs"><CheckCircle class="w-3.5 h-3.5" /> Finish Test</button>
          </div>
        </div>
      </div>

      <!-- Right navigator -->
      <div class="w-52 shrink-0 card p-3 space-y-3 sticky top-4">
        <div class="text-xs font-semibold text-slate-600">Questions</div>
        <div>
          <div class="text-xs text-slate-500 mb-1">Short questions ({{ shortQs.length }})</div>
          <div class="flex flex-wrap gap-1">
            <button v-for="(q, i) in shortQs" :key="q.id" @click="goToQuestion(q)"
              :class="['w-7 h-7 rounded-lg text-xs font-medium transition-colors',
                currentIndex === questions.indexOf(q) ? 'bg-brand-600 text-white' :
                answers[q.id] ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-600 hover:bg-slate-200']"
            >{{ i + 1 }}</button>
          </div>
        </div>
        <div>
          <div class="text-xs text-slate-500 mb-1">Long questions ({{ longQs.length }})</div>
          <div class="flex flex-wrap gap-1">
            <button v-for="(q, i) in longQs" :key="q.id" @click="goToQuestion(q)"
              :class="['w-7 h-7 rounded-lg text-xs font-medium transition-colors',
                currentIndex === questions.indexOf(q) ? 'bg-brand-600 text-white' :
                answers[q.id] ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-600 hover:bg-slate-200']"
            >{{ i + 1 }}</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Results / AI Grading -->
    <div v-else class="space-y-4">
      <div class="card p-5 flex items-center gap-4 flex-wrap">
        <div class="text-3xl">🤖</div>
        <div class="flex-1">
          <div class="font-semibold text-slate-800">Test Submitted — AI Grading in Progress</div>
          <div class="text-sm text-slate-500">Gemini is evaluating your answers against model answers…</div>
        </div>
        <span v-if="!gradingDone" class="badge-amber animate-pulse">Grading…</span>
        <span v-else class="badge-green">Graded</span>
      </div>

      <div v-for="(q, i) in questions" :key="q.id" class="card p-5 space-y-3">
        <div class="flex items-center gap-2 flex-wrap">
          <span class="text-sm font-semibold text-slate-500">Q{{ i + 1 }}</span>
          <span :class="q.type === 'Short' ? 'badge-blue' : 'badge-purple'">{{ q.type }}</span>
          <span :class="diffBadge(q.difficulty)">{{ q.difficulty }}</span>
          <span class="badge-amber">{{ q.marks }} marks</span>
          <span v-if="gradingDone && grades[q.id]" class="ml-auto badge-green">{{ grades[q.id].marks }}/{{ q.marks }}</span>
        </div>
        <p class="text-slate-800 font-medium">{{ q.stem }}</p>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div class="p-3 rounded-xl bg-blue-50 border border-blue-100">
            <div class="text-xs font-semibold text-blue-700 mb-1">Your Answer</div>
            <div class="text-sm text-blue-800 prose prose-sm max-w-none" v-html="answers[q.id] || '<em class=\'text-slate-400\'>No answer given</em>'" />
          </div>
          <div class="p-3 rounded-xl bg-emerald-50 border border-emerald-100">
            <div class="text-xs font-semibold text-emerald-700 mb-1">Model Answer</div>
            <div class="text-sm text-emerald-800">{{ q.modelAnswer }}</div>
          </div>
        </div>
        <div v-if="gradingDone && grades[q.id]" class="p-3 rounded-xl bg-purple-50 border border-purple-100">
          <div class="text-xs font-semibold text-purple-700 mb-1">AI Feedback</div>
          <p class="text-sm text-purple-800">{{ grades[q.id].feedback }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onBeforeUnmount } from 'vue'
import { ArrowLeft, Timer, Play, CheckCircle, Bold, Italic, List, Mic, ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from '@lucide/vue'
import { useEditor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Placeholder from '@tiptap/extension-placeholder'
import { useContentStore } from '@/stores/content'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { useStudentStore } from '@/stores/student'

const props = defineProps({ bookId: String })
const content = useContentStore()
const student = useStudentStore()

const subject = computed(() => findPrepSubject(props.bookId))
const questions = ref(content.getSubjectiveQuestions(+props.bookId, { limit: 20 }))
const shortQs = computed(() => questions.value.filter(q => q.type === 'Short'))
const longQs  = computed(() => questions.value.filter(q => q.type === 'Long'))

const started = ref(false)
const finished = ref(false)
const currentIndex = ref(0)
const answers = ref({})
const grades = ref({})
const gradingDone = ref(false)
const timeLeft = ref(17 * 60)
const voiceLang = ref('en-US')
const isListening = ref(false)
let timer = null
let recognition = null

const currentQ = computed(() => questions.value[currentIndex.value])

const editor = useEditor({
  extensions: [
    StarterKit,
    Placeholder.configure({ placeholder: 'Write your answer here…' }),
  ],
  content: '',
  onUpdate({ editor }) {
    if (currentQ.value) answers.value[currentQ.value.id] = editor.getHTML()
  },
})

watch(currentIndex, () => {
  if (editor.value && currentQ.value) {
    editor.value.commands.setContent(answers.value[currentQ.value.id] || '')
  }
})

watch(started, v => {
  if (v) timer = setInterval(() => { if (timeLeft.value > 0) timeLeft.value--; else finishTest() }, 1000)
})

function go(i) {
  if (i >= 0 && i < questions.value.length) currentIndex.value = i
}

function goToQuestion(q) {
  currentIndex.value = questions.value.indexOf(q)
}

async function finishTest() {
  clearInterval(timer)
  finished.value = true
  // AI Grading (mock — real impl calls Gemini API)
  setTimeout(() => {
    questions.value.forEach(q => {
      const hasAnswer = answers.value[q.id] && answers.value[q.id] !== '<p></p>'
      grades.value[q.id] = {
        marks: hasAnswer ? Math.round(q.marks * 0.7) : 0,
        feedback: hasAnswer
          ? 'Good answer! You covered the main points. Consider adding more specific examples to strengthen your response.'
          : 'No answer was provided for this question.',
      }
    })
    gradingDone.value = true
    const totalMarks = questions.value.reduce((a, q) => a + q.marks, 0)
    const earned = Object.values(grades.value).reduce((a, g) => a + g.marks, 0)
    student.saveTest({ subject: subject.value?.name, type: 'Subjective Test', score: earned, total: totalMarks, bookId: +props.bookId })
  }, 2500)
}

function toggleVoice() {
  if (!('webkitSpeechRecognition' in window || 'SpeechRecognition' in window)) {
    alert('Voice input not supported in this browser.')
    return
  }
  if (isListening.value) {
    recognition?.stop()
    isListening.value = false
    return
  }
  const SR = window.SpeechRecognition || window.webkitSpeechRecognition
  recognition = new SR()
  recognition.lang = voiceLang.value
  recognition.continuous = true
  recognition.interimResults = false
  recognition.onresult = e => {
    const text = Array.from(e.results).map(r => r[0].transcript).join(' ')
    editor.value?.commands.insertContent(text + ' ')
  }
  recognition.onerror = () => { isListening.value = false }
  recognition.onend = () => { isListening.value = false }
  recognition.start()
  isListening.value = true
}

function formatTime(s) {
  return `${Math.floor(s/60).toString().padStart(2,'0')}:${(s%60).toString().padStart(2,'0')}`
}

function diffBadge(d) {
  return { Easy: 'badge-green', Medium: 'badge-amber', Hard: 'badge-red' }[d] || 'badge-indigo'
}

onBeforeUnmount(() => { clearInterval(timer); recognition?.stop(); editor.value?.destroy() })
</script>

<style>
.editor-btn { @apply p-1.5 rounded-lg hover:bg-slate-200 text-slate-600 transition-colors; }
</style>

