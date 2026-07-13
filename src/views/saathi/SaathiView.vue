<template>
  <div class="animate-fade-in sa-wrap">
    <!-- Header -->
    <div class="sa-head card">
      <div class="sa-head-icon grad-violet">🤝</div>
      <div class="sa-head-info">
        <h1 class="sa-title">Saathi AI <span class="sa-urdu urdu">ساتھی</span></h1>
        <p class="sa-sub">Your AI study companion — Watch · Listen · Read</p>
      </div>
      <div class="sa-head-right">
        <span class="sa-grade-badge">Grade {{ grade }}</span>
        <select v-model="lang" class="sa-lang" title="Reply language">
          <option value="english">EN</option>
          <option value="urdu">اردو</option>
          <option value="mixed">Mixed</option>
        </select>
      </div>
    </div>

    <!-- Mode selector -->
    <div class="sa-modes">
      <button v-for="m in modes" :key="m.key" type="button"
        @click="selectMode(m.key)"
        :class="['sa-mode-pill', mode === m.key ? 'is-on' : '']">
        <component :is="m.icon" class="w-3.5 h-3.5" />
        {{ m.label }}
      </button>
    </div>

    <!-- Optional topic input (tutor / quiz) -->
    <div v-if="activeMode.needsTopic" class="sa-topic-row">
      <input v-model="topic" class="input sa-topic" :placeholder="activeMode.topicHint || 'Topic or chapter…'" />
    </div>

    <!-- Chat -->
    <div class="card sa-chat">
      <div ref="threadEl" class="sa-thread">
        <!-- Empty state -->
        <div v-if="!messages.length && !loading" class="sa-empty">
          <component :is="activeMode.icon" class="w-12 h-12 mb-2" style="color:var(--t-accent)" />
          <p class="sa-empty-title">{{ activeMode.label }}</p>
          <p class="sa-empty-sub">{{ activeMode.blurb }}</p>
          <button type="button" class="btn-primary mt-3" @click="kickoff" :disabled="loading">
            <Sparkles class="w-4 h-4" /> {{ activeMode.cta }}
          </button>
        </div>

        <!-- Messages -->
        <div v-for="(msg, i) in messages" :key="i" class="sa-msg" :class="'sa-msg-' + msg.role">
          <div class="sa-msg-ava" :class="msg.role === 'student' ? 'sa-ava-student' : 'grad-violet'">
            <User v-if="msg.role === 'student'" class="w-3.5 h-3.5" />
            <AlertTriangle v-else-if="msg.role === 'error'" class="w-3.5 h-3.5" />
            <span v-else>🤝</span>
          </div>
          <div v-if="msg.role === 'saathi'" class="sa-msg-wrap">
            <div
              class="sa-msg-body"
              :class="lang === 'urdu' ? 'urdu' : ''"
              :dir="lang === 'urdu' ? 'rtl' : 'ltr'"
              v-html="formatResponse(msg.text)" />
            <span v-if="msg.engine" class="sa-msg-badge" :class="msg.engine === 'gemini' ? 'sa-badge-gemini' : 'sa-badge-ollama'">
              {{ msg.engine === 'gemini' ? '✦ Gemini' : '⚙ Ollama' }} · {{ msg.model }}
            </span>
          </div>
          <div v-else
            class="sa-msg-body"
            :class="[lang === 'urdu' ? 'urdu' : '', msg.role === 'error' ? 'sa-msg-err' : '']"
            :dir="lang === 'urdu' ? 'rtl' : 'ltr'">{{ msg.text }}</div>
        </div>

        <!-- Typing -->
        <div v-if="loading" class="sa-msg sa-msg-saathi">
          <div class="sa-msg-ava grad-violet"><span>🤝</span></div>
          <div class="sa-msg-body sa-typing"><span/><span/><span/></div>
        </div>
      </div>

      <!-- Input bar -->
      <form class="sa-input-bar" @submit.prevent="send()">
        <input v-model="draft" class="sa-input" :placeholder="lang === 'urdu' ? 'ساتھی سے پوچھیں…' : 'Message Saathi…'"
          :dir="lang === 'urdu' ? 'rtl' : 'ltr'" :disabled="loading" />
        <button type="submit" class="sa-send" :disabled="!draft.trim() || loading"><Send class="w-4 h-4" /></button>
      </form>
      <div class="sa-foot">
        <span v-if="engine" class="sa-engine">
          <Cpu class="w-3 h-3" /> answered by {{ engine === 'ollama' ? ollamaModel : 'Gemini (fallback)' }}
        </span>
        <div class="sa-foot-right">
          <span class="sa-foot-note">Saathi follows the Grade {{ grade }} PTB/BBISE syllabus · won't invent your scores</span>
          <button v-if="messages.length" type="button" @click="clearMode" class="sa-clear-btn">Clear</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import { GraduationCap, CalendarDays, ListChecks, Gauge, Users, Sparkles, Send, User, AlertTriangle, Cpu } from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'
import { chatWithFallback, getLastEngine, ollamaConfig } from '@/services/ollamaService'
import { buildPrompt } from '@/services/studentAssistantPrompts'

const auth = useAuthStore()
const grade = computed(() => auth.user?.gradeCode ?? 9)
const board = computed(() => (auth.user?.board ? `${auth.user.board} Board` : 'Balochistan Board') + ' (BISE)')

const lang = ref('english')
const topic = ref('')
const draft = ref('')
const messages = ref([])
const loading = ref(false)
const engine = ref(null)
const ollamaModel = ollamaConfig.MODEL
const threadEl = ref(null)

const modes = [
  { key: 'tutor_session',         label: 'Tutor',          icon: GraduationCap, needsTopic: true,  topicHint: 'Which topic should I teach?',
    blurb: 'I teach hint-first, with examples, and check you understood each step.', cta: 'Start the lesson' },
  { key: 'study_planner',         label: 'Study Plan',     icon: CalendarDays,
    blurb: 'I build a realistic weekly plan around your time and weak topics.', cta: 'Make my weekly plan' },
  { key: 'quiz_generator',        label: 'Quiz Me',        icon: ListChecks,    needsTopic: true,  topicHint: 'Quiz topic or chapter…',
    blurb: 'A short syllabus-locked quiz, one question at a time, graded gently.', cta: 'Start the quiz' },
  { key: 'exam_readiness_report', label: 'Exam Readiness', icon: Gauge,
    blurb: 'A readiness report from your real coverage and scores (when available).', cta: 'Check my readiness' },
  { key: 'parent_report',         label: 'Parent Report',  icon: Users,
    blurb: 'A warm progress summary written for a parent.', cta: 'Write the parent report' },
]
const mode = ref('tutor_session')
const activeMode = computed(() => modes.find((m) => m.key === mode.value) || modes[0])

function loadModeMessages(modeKey) {
  try { return JSON.parse(localStorage.getItem(`bap_saathi_${modeKey}`) || '[]') } catch { return [] }
}
function saveModeMessages() {
  localStorage.setItem(`bap_saathi_${mode.value}`, JSON.stringify(messages.value.slice(0, 100)))
}
function clearMode() {
  messages.value = []
  engine.value = null
  localStorage.removeItem(`bap_saathi_${mode.value}`)
}

function formatResponse(text) {
  if (!text) return ''
  return String(text)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.*?)\*/g, '<em>$1</em>')
    .replace(/^#{1,3}\s+(.+)$/gm, '<strong class="sa-md-h">$1</strong>')
    .replace(/^- (.+)$/gm, '<li class="sa-md-li">$1</li>')
    .replace(/(<li[\s\S]*?<\/li>)/g, '<ul class="sa-md-ul">$1</ul>')
    .replace(/\n\n/g, '</p><p class="sa-md-p">')
    .replace(/\n/g, '<br/>')
}

onMounted(() => { messages.value = loadModeMessages(mode.value) })

function selectMode(key) {
  if (mode.value === key) return
  mode.value = key
  messages.value = loadModeMessages(key)
  engine.value = null
}

function today() {
  try { return new Date().toISOString().slice(0, 10) } catch { return '' }
}

function injectedFor() {
  const base = { student_name: auth.user?.name || 'Student', language: lang.value, grade: grade.value, board: board.value, today_date: today() }
  switch (mode.value) {
    case 'study_planner':
      return { ...base, study_days_per_week: 5, preferred_study_time: '17:00', syllabus_json: {}, progress_json: {} }
    case 'quiz_generator':
      return { ...base, quiz_topic: topic.value || 'the current chapter', question_count: 5 }
    case 'exam_readiness_report':
      return { ...base, exam_json: {}, syllabus_json: {}, progress_json: {}, activity_log_json: {} }
    case 'parent_report':
      return { ...base, report_period_days: 30, progress_json: {}, activity_log_json: {} }
    case 'tutor_session':
    default:
      return { ...base, current_topic: topic.value || 'the next topic in the syllabus', progress_json: {} }
  }
}

async function scrollThread() {
  await nextTick()
  if (threadEl.value) threadEl.value.scrollTop = threadEl.value.scrollHeight
}

async function send(forceText) {
  const text = (typeof forceText === 'string' ? forceText : draft.value).trim()
  if (!text || loading.value) return
  const history = messages.value
    .filter((m) => m.role === 'student' || m.role === 'saathi')
    .map((m) => ({ role: m.role === 'student' ? 'user' : 'assistant', content: m.text }))
  messages.value.push({ role: 'student', text })
  draft.value = ''
  loading.value = true
  await scrollThread()
  try {
    const system = buildPrompt(mode.value, grade.value, injectedFor())
    const reply = await chatWithFallback({
      system,
      messages: [...history, { role: 'user', content: text }],
    })
    const eng = getLastEngine()
    const modelName = eng === 'gemini' ? ollamaConfig.GEMINI_MODEL : ollamaModel
    messages.value.push({ role: 'saathi', text: reply || '…', engine: eng, model: modelName })
    engine.value = eng
    saveModeMessages()
  } catch (e) {
    const msg = e?.message || ''
    const friendly = msg.includes('Both engines failed') || msg.includes('All AI engines failed')
      ? 'Both Ollama and Gemini are unavailable right now. Check your connection and try again.'
      : msg.includes('503')
        ? 'AI Tutor is currently disabled by the administrator.'
        : 'Saathi is unavailable right now. Please try again in a moment.'
    messages.value.push({ role: 'error', text: friendly })
    console.error('[Saathi]', msg)
  }
  loading.value = false
  scrollThread()
}

function kickoff() {
  const t = topic.value ? ` on "${topic.value}"` : ''
  const starters = {
    tutor_session: `Please teach me${t}.`,
    study_planner: 'Please make my weekly study plan.',
    quiz_generator: `Quiz me${t}.`,
    exam_readiness_report: 'Show me my exam readiness report.',
    parent_report: 'Write the progress report for my parent.',
  }
  send(starters[mode.value] || 'Let\'s begin.')
}
</script>

<style scoped>
.sa-wrap { max-width: 860px; margin: 0 auto; display: flex; flex-direction: column; gap: 0.85rem; }
.sa-head { display: flex; align-items: center; gap: 0.85rem; padding: 0.9rem 1.1rem; }
.sa-head-icon { width: 46px; height: 46px; border-radius: 13px; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; flex-shrink: 0; }
.sa-head-info { flex: 1; min-width: 0; }
.sa-title { font-size: 1.2rem; font-weight: 800; color: var(--t-text1); font-family: 'Syne', sans-serif; line-height: 1.1; display: flex; align-items: baseline; gap: 0.4rem; }
.sa-urdu { font-size: 1rem; color: var(--t-accent); }
.sa-sub { font-size: 0.76rem; color: var(--t-text3); margin-top: 0.1rem; }
.sa-head-right { display: flex; align-items: center; gap: 0.5rem; flex-shrink: 0; }
.sa-grade-badge { font-size: 0.68rem; font-weight: 800; color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 12%, transparent); border: 1px solid color-mix(in srgb, var(--t-accent) 25%, transparent); border-radius: 99px; padding: 0.2rem 0.6rem; }
.sa-lang { border: 1px solid var(--t-border); border-radius: 10px; padding: 0.3rem 0.45rem; background: var(--t-surface); color: var(--t-text1); font-size: 0.76rem; }

.sa-modes { display: flex; gap: 0.4rem; flex-wrap: wrap; }
.sa-mode-pill { display: inline-flex; align-items: center; gap: 0.4rem; padding: 0.42rem 0.85rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.78rem; font-weight: 600; cursor: pointer; transition: all 0.15s; }
.sa-mode-pill:hover { border-color: var(--t-accent); color: var(--t-accent); }
.sa-mode-pill.is-on { background: color-mix(in srgb, var(--t-accent) 12%, transparent); border-color: color-mix(in srgb, var(--t-accent) 40%, transparent); color: var(--t-accent); }

.sa-topic-row { display: flex; }
.sa-topic { font-size: 0.85rem; }

.sa-chat { display: flex; flex-direction: column; padding: 0; overflow: hidden; }
.sa-thread { min-height: 320px; max-height: 460px; overflow-y: auto; padding: 1rem; display: flex; flex-direction: column; gap: 0.85rem; }
.sa-empty { margin: auto; text-align: center; display: flex; flex-direction: column; align-items: center; color: var(--t-text3); padding: 1.5rem; }
.sa-empty-title { font-size: 0.95rem; font-weight: 700; color: var(--t-text1); }
.sa-empty-sub { font-size: 0.8rem; max-width: 360px; margin-top: 0.2rem; }

.sa-msg { display: flex; gap: 0.6rem; align-items: flex-start; }
.sa-msg-student { flex-direction: row-reverse; }
.sa-msg-wrap { display: flex; flex-direction: column; gap: 0.25rem; max-width: 80%; }
.sa-msg-badge { display: inline-flex; align-items: center; gap: 0.2rem; font-size: 0.6rem; font-weight: 600; padding: 0.15rem 0.5rem; border-radius: 99px; width: fit-content; letter-spacing: 0.01em; }
.sa-badge-gemini { background: color-mix(in srgb, #8b5cf6 12%, transparent); color: #7c3aed; border: 1px solid color-mix(in srgb, #8b5cf6 25%, transparent); }
.sa-badge-ollama  { background: color-mix(in srgb, #0ea5e9 12%, transparent); color: #0284c7; border: 1px solid color-mix(in srgb, #0ea5e9 25%, transparent); }
html.dark .sa-badge-gemini { color: #a78bfa; border-color: color-mix(in srgb, #8b5cf6 35%, transparent); }
html.dark .sa-badge-ollama  { color: #38bdf8; border-color: color-mix(in srgb, #0ea5e9 35%, transparent); }
.sa-msg-ava { width: 30px; height: 30px; border-radius: 50%; flex-shrink: 0; display: flex; align-items: center; justify-content: center; font-size: 0.95rem; color: #fff; }
.sa-ava-student { background: var(--t-hover2); color: var(--t-text2); }
.sa-msg-body { max-width: 80%; padding: 0.65rem 0.9rem; border-radius: 14px; font-size: 0.86rem; line-height: 1.6; background: var(--t-hover); color: var(--t-text1); white-space: pre-wrap; word-break: break-word; }
.sa-msg-student .sa-msg-body { background: color-mix(in srgb, var(--t-accent) 14%, transparent); border: 1px solid color-mix(in srgb, var(--t-accent) 25%, transparent); }
.sa-msg-err { background: var(--t-danger-bg) !important; color: var(--t-danger) !important; }
.sa-msg-body.urdu { text-align: right; line-height: 2; }

.sa-typing { display: flex; gap: 4px; align-items: center; }
.sa-typing span { width: 6px; height: 6px; border-radius: 50%; background: var(--t-text3); animation: saType 1.2s ease-in-out infinite; }
.sa-typing span:nth-child(2) { animation-delay: 0.2s; }
.sa-typing span:nth-child(3) { animation-delay: 0.4s; }
@keyframes saType { 0%, 80%, 100% { transform: scale(0.6); opacity: 0.4; } 40% { transform: scale(1); opacity: 1; } }

.sa-input-bar { display: flex; gap: 0.5rem; padding: 0.75rem; border-top: 1px solid var(--t-border); }
.sa-input { flex: 1; padding: 0.6rem 0.9rem; border-radius: 12px; font-size: 0.86rem; background: var(--t-input-bg); border: 1px solid var(--t-input-border); color: var(--t-text1); }
.sa-input:focus { outline: none; border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-input-focus); }
.sa-send { width: 40px; height: 40px; border-radius: 11px; flex-shrink: 0; display: flex; align-items: center; justify-content: center; border: none; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); color: #fff; cursor: pointer; }
.sa-send:disabled { opacity: 0.4; cursor: not-allowed; }
.sa-foot { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.4rem; padding: 0 0.85rem 0.7rem; }
.sa-foot-right { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.sa-engine { display: inline-flex; align-items: center; gap: 0.3rem; font-size: 0.66rem; font-weight: 600; color: var(--t-accent); }
.sa-foot-note { font-size: 0.64rem; color: var(--t-text3); }
.sa-clear-btn { font-size: 0.64rem; font-weight: 600; color: var(--t-text3); background: none; border: none; cursor: pointer; padding: 0; text-decoration: underline; }
.sa-clear-btn:hover { color: var(--t-danger, #ef4444); }

.sa-msg-body :deep(.sa-md-h) { display: block; margin-top: 0.5rem; margin-bottom: 0.2rem; font-size: 0.9rem; }
.sa-msg-body :deep(.sa-md-ul) { margin: 0.2rem 0; padding-left: 0; list-style: none; }
.sa-msg-body :deep(.sa-md-li) { position: relative; padding-left: 1rem; margin: 0.15rem 0; }
.sa-msg-body :deep(.sa-md-li)::before { content: '•'; position: absolute; left: 0.2rem; color: var(--t-accent); }
.sa-msg-body :deep(.sa-md-p) { display: block; margin-top: 0.4rem; }

@media (prefers-reduced-motion: reduce) { .sa-typing span { animation: none; } }
</style>
