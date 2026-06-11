<template>
  <div class="h-[calc(100vh-8rem)] flex gap-4 animate-fade-in">
    <!-- Chat History Sidebar -->
    <Transition name="sidebar">
      <div v-show="historyVisible" class="w-60 shrink-0 card flex flex-col">
        <div class="flex items-center justify-between p-3 cv-div-b">
          <span class="text-sm font-semibold" style="color:var(--t-text1)">
            {{ isUrduMode ? 'گفتگو کی تاریخ' : 'Chat History' }}
          </span>
          <button @click="clearHistory" class="text-xs text-red-500 hover:text-red-700 transition-colors">
            {{ isUrduMode ? 'صاف کریں' : 'Clear All' }}
          </button>
        </div>
        <div class="flex-1 overflow-y-auto p-2 space-y-1">
          <button v-for="s in sessions" :key="s.id"
            @click="loadSession(s)"
            :class="['cv-session w-full text-left px-3 py-2 rounded-xl text-xs truncate',
              activeSession === s.id ? 'cv-session-active font-medium' : '',
              isUrduMode ? 'urdu text-right' : '']">
            {{ s.preview }}
          </button>
          <div v-if="!sessions.length" class="text-xs text-center py-6" style="color:var(--t-text3)">
            {{ isUrduMode ? 'ابھی تک کوئی گفتگو نہیں۔' : 'No chat history yet.' }}
          </div>
        </div>
        <div class="p-2 cv-div-t">
          <button @click="historyVisible = false" class="btn-ghost text-xs w-full justify-center">
            <EyeOff class="w-3.5 h-3.5" />
            {{ isUrduMode ? 'تاریخ چھپائیں' : 'Hide History' }}
          </button>
        </div>
      </div>
    </Transition>

    <!-- Main chat -->
    <div class="flex-1 flex flex-col card overflow-hidden">
      <!-- Chat header -->
      <div class="flex items-center gap-3 p-4 cv-div-b">
        <button v-if="!historyVisible" @click="historyVisible = true" class="btn-ghost p-2"><History class="w-4 h-4" /></button>
        <div class="w-9 h-9 rounded-xl flex items-center justify-center text-xl shrink-0" :class="tutor?.color">{{ tutor?.icon }}</div>
        <div>
          <div class="font-bold text-sm" style="color:var(--t-text1)">
            {{ tutor?.persona }}
            <span v-if="isUrduMode" class="cv-urdu-badge">اردو</span>
          </div>
          <div class="text-xs" style="color:var(--t-text3)">{{ tutor?.subject }} · {{ tutor?.desc }}</div>
        </div>
        <div class="ml-auto flex items-center gap-2">
          <span class="badge-green text-xs flex items-center gap-1"><div class="w-1.5 h-1.5 rounded-full bg-emerald-400" />Online</span>
          <RouterLink to="/app/ai-tutor" class="btn-ghost text-xs"><ArrowLeft class="w-3.5 h-3.5" /> Back</RouterLink>
        </div>
      </div>

      <!-- Messages -->
      <div ref="messagesEl" class="flex-1 overflow-y-auto p-4 space-y-4">
        <!-- Welcome message -->
        <div v-if="!messages.length" class="flex gap-3">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-lg shrink-0" :class="tutor?.color">{{ tutor?.icon }}</div>
          <div class="cv-bubble-ai rounded-2xl rounded-tl-sm px-4 py-3 max-w-[80%]">
            <p class="text-sm" :class="isUrduMode ? 'urdu leading-loose' : ''" style="color:var(--t-text1)">
              <template v-if="isUrduMode">
                السلام علیکم! میں <strong>{{ tutor?.persona }}</strong> ہوں۔ میں آپ کو <strong>{{ tutor?.subject }}</strong> پڑھانے کے لیے حاضر ہوں۔ آپ اردو یا انگریزی میں سوال پوچھ سکتے ہیں!
              </template>
              <template v-else>
                Hello! I am <strong>{{ tutor?.persona }}</strong>. I am here to help you master <strong>{{ tutor?.subject }}</strong>. Ask me anything — from basic concepts to exam-level questions!
              </template>
            </p>
            <div class="mt-2 flex flex-wrap gap-2" :class="isUrduMode ? 'flex-row-reverse justify-end' : ''">
              <button v-for="s in starters" :key="s"
                @click="sendMessage(s)"
                :class="['cv-starter text-xs px-2.5 py-1 rounded-full', isUrduMode ? 'urdu' : '']">
                {{ s }}
              </button>
            </div>
          </div>
        </div>

        <!-- Messages -->
        <div v-for="msg in messages" :key="msg.id"
          :class="['flex gap-3', msg.role === 'user' ? 'flex-row-reverse' : '']">
          <div class="w-8 h-8 rounded-full flex items-center justify-center shrink-0"
            :class="msg.role === 'user' ? 'cv-user-avatar text-sm font-bold' : tutor?.color + ' text-lg'">
            {{ msg.role === 'user' ? initials : tutor?.icon }}
          </div>
          <div :class="['rounded-2xl px-4 py-3 max-w-[80%] text-sm leading-relaxed',
            msg.role === 'user' ? 'cv-bubble-user rounded-tr-sm' : 'cv-bubble-ai rounded-tl-sm',
            isUrduMode ? 'urdu leading-loose' : '']">
            <div v-if="msg.thinking" class="flex items-center gap-2" style="color:var(--t-text3)">
              <div class="flex gap-1">
                <div v-for="i in 3" :key="i" class="w-1.5 h-1.5 rounded-full cv-dot animate-bounce" :style="{ animationDelay: (i * 0.15) + 's' }" />
              </div>
              {{ isUrduMode ? 'سوچ رہا ہوں…' : 'Thinking…' }}
            </div>
            <div v-else v-html="msg.role === 'assistant' ? formatResponse(msg.content) : msg.content" />

            <!-- Action chips (assistant only) -->
            <div v-if="msg.role === 'assistant' && !msg.thinking" class="mt-3 flex flex-wrap gap-2" :class="isUrduMode ? 'flex-row-reverse' : ''">
              <button @click="followUp('DeepDive', msg.content)" class="chip-action">
                <Layers class="w-3 h-3" />
                {{ isUrduMode ? 'گہرائی میں' : 'Deep Dive' }}
              </button>
              <button @click="followUp('Related', msg.content)" class="chip-action">
                <Link class="w-3 h-3" />
                {{ isUrduMode ? 'متعلقہ موضوعات' : 'Related Topics' }}
              </button>
              <button @click="followUp('Practice', msg.content)" class="chip-action">
                <Pencil class="w-3 h-3" />
                {{ isUrduMode ? 'مشق' : 'Practice' }}
              </button>
              <!-- Urdu mode: translate to English. Normal mode: translate to Urdu -->
              <template v-if="isUrduMode">
                <button v-if="!msg.engText" @click="translateToEng(msg)" class="cv-green-chip">
                  🌐 Translate to English
                </button>
                <button v-else @click="msg.showEng = !msg.showEng" class="cv-green-chip">
                  {{ msg.showEng ? '↩ اردو دکھائیں' : '🌐 English' }}
                </button>
              </template>
              <template v-else>
                <button v-if="!msg.urdu" @click="translateMsg(msg)" class="cv-green-chip">🌐 Translate to Urdu</button>
                <button v-else @click="msg.urdu = false" class="cv-green-chip">↩ Show Original</button>
              </template>
            </div>

            <!-- Urdu translation panel (for non-Urdu subjects) -->
            <div v-if="!isUrduMode && msg.urdu && msg.urduText" class="cv-urdu mt-3 p-3 rounded-xl">
              <div class="cv-urdu-label text-xs font-medium mb-2">اردو ترجمہ</div>
              <p class="cv-urdu-text urdu text-sm leading-loose">{{ msg.urduText }}</p>
            </div>

            <!-- English translation panel (for Urdu subjects) -->
            <div v-if="isUrduMode && msg.showEng && msg.engText" class="cv-eng mt-3 p-3 rounded-xl">
              <div class="cv-eng-label text-xs font-medium mb-1">English Translation</div>
              <p class="cv-eng-text text-sm leading-relaxed">{{ msg.engText }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Input area -->
      <div class="p-4 cv-div-t">
        <!-- Urdu input hint -->
        <div v-if="isUrduMode" class="cv-urdu-hint">
          <span class="urdu text-xs">اردو یا انگریزی میں لکھیں</span>
          <span class="text-xs" style="color:var(--t-text3)">— Type in Urdu or English</span>
        </div>
        <form @submit.prevent="send" class="flex gap-2 mt-1">
          <input v-model="input"
            :placeholder="isUrduMode ? 'یہاں اپنا سوال لکھیں…' : `Ask ${tutor?.persona}…`"
            :class="['input flex-1', isUrduMode ? 'cv-input-urdu' : '']"
            :dir="isUrduMode ? 'rtl' : 'ltr'"
            :disabled="loading" />
          <button type="submit" :disabled="!input.trim() || loading" class="btn-primary px-4">
            <Send class="w-4 h-4" />
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue'
import { ArrowLeft, Send, History, EyeOff, Layers, Link, Pencil } from '@lucide/vue'
import { AI_TUTORS } from '@/stores/content'
import { useAuthStore } from '@/stores/auth'

const props = defineProps({ subject: String })
const auth = useAuthStore()

const tutor = computed(() => AI_TUTORS.find(t => t.slug === props.subject))
const initials = computed(() => auth.user?.name?.split(' ').map(w => w[0]).slice(0,2).join('') || 'U')

// Slugs whose AI should respond in Urdu
const URDU_SLUGS = new Set(['ghalib', 'ghazali'])
const isUrduMode = computed(() => URDU_SLUGS.has(props.subject))

const GEMINI_KEY = import.meta.env.VITE_GEMINI_API_KEY || ''

const messages = ref([])
const input = ref('')
const loading = ref(false)
const historyVisible = ref(true)
const sessions = ref(JSON.parse(localStorage.getItem(`estudy_chat_${props.subject}`) || '[]'))
const activeSession = ref(null)
const messagesEl = ref(null)

const starters = computed(() => ({
  einstein:    ['What is Newton\'s 3rd law?', 'Explain gravity', 'What is relativity?'],
  khwarizmi:   ['Solve quadratic equations', 'What is algebra?', 'Explain trigonometry'],
  curie:       ['Explain atomic structure', 'What is periodic table?', 'Define oxidation'],
  ibnsina:     ['Explain cell division', 'What is photosynthesis?', 'DNA vs RNA?'],
  turing:      ['What is an algorithm?', 'Explain binary numbers', 'What is a loop?'],
  iqbal:       ['Pakistan geography', 'Who was Quaid-e-Azam?', 'Two-Nation Theory'],
  shakespeare: ['What is a metaphor?', 'Explain tenses', 'Improve my writing'],
  ghalib:      ['اردو شاعری کیا ہے؟', 'غالب کی مشہور غزل سنائیں', 'اردو گرامر سمجھائیں', 'محاورہ کی مثال دیں'],
  ghazali:     ['نماز کی اہمیت کیا ہے؟', 'قرآن پاک کی تعلیمات', 'اسلام میں اخلاق', 'توحید کا مطلب'],
}[props.subject] || ['Ask me anything!']))

function buildSystemInstruction() {
  if (isUrduMode.value) {
    return `آپ ${tutor.value?.persona} ہیں، ${tutor.value?.subject} کے AI استاد۔ ہمیشہ خالص اردو میں جواب دیں۔ آسان، واضح اور تعلیمی انداز اختیار کریں جو پاکستان (بلوچستان بورڈ) کے گریڈ 9 کے طلباء کے لیے مناسب ہو۔ جہاں ضروری ہو نکات اور مثالیں استعمال کریں۔ اگر طالب علم انگریزی میں پوچھے تو بھی اردو میں جواب دیں۔`
  }
  return `You are ${tutor.value?.persona}, an AI tutor for ${tutor.value?.subject}. Respond in character, in first person, with enthusiasm. Keep answers educational, clear, and suitable for Grade 9 Pakistani students (Balochistan board). Format with bullet points and examples where helpful.`
}

async function callGemini(prompt) {
  if (!GEMINI_KEY) {
    if (isUrduMode.value) {
      return `[ڈیمو موڈ] ${tutor.value?.persona} کی طرف سے: یہ ایک اہم سوال ہے! "${prompt}" کے بارے میں ${tutor.value?.subject} میں گہری معلومات موجود ہیں۔ اصل Gemini API key کے ساتھ آپ کو مکمل اردو جواب ملے گا۔`
    }
    return `[Demo Mode] As ${tutor.value?.persona}, I would say: This is a fascinating question! ${prompt} relates to core concepts in ${tutor.value?.subject}. In a real deployment with a Gemini API key, I would provide a detailed, persona-styled answer here.`
  }
  const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=${GEMINI_KEY}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      contents: [{ parts: [{ text: prompt }] }],
      systemInstruction: { parts: [{ text: buildSystemInstruction() }] },
    }),
  })
  const data = await res.json()
  return data.candidates?.[0]?.content?.parts?.[0]?.text || (isUrduMode.value ? 'معذرت، جواب نہیں مل سکا۔' : 'Sorry, I could not generate a response.')
}

async function sendMessage(text) {
  input.value = text
  await send()
}

async function send() {
  const text = input.value.trim()
  if (!text || loading.value) return
  input.value = ''
  messages.value.push({ id: Date.now(), role: 'user', content: text })
  const thinkMsg = { id: Date.now() + 1, role: 'assistant', thinking: true, content: '' }
  messages.value.push(thinkMsg)
  loading.value = true
  await nextTick(); scrollDown()

  const reply = await callGemini(text)
  const idx = messages.value.indexOf(thinkMsg)
  messages.value[idx] = { id: thinkMsg.id, role: 'assistant', thinking: false, content: reply, urdu: false, urduText: '', engText: '', showEng: false }
  loading.value = false
  await nextTick(); scrollDown()
  saveSessions()
}

async function followUp(type, context) {
  const prompts = isUrduMode.value ? {
    'DeepDive': `پچھلے موضوع پر مزید تفصیل سے بتائیں: ${context.slice(0, 200)}`,
    'Related':  `پانچ متعلقہ موضوعات بتائیں جو گریڈ 9 کے طالب علم کو جاننے چاہئیں: ${context.slice(0, 200)}`,
    'Practice': `اس موضوع پر 3 مشق کے سوالات دیں: ${context.slice(0, 200)}`,
  } : {
    'DeepDive': `Go deeper on the topic you just explained: ${context.slice(0, 200)}`,
    'Related':  `List 5 related topics a Grade 9 student should know related to: ${context.slice(0, 200)}`,
    'Practice': `Give me 3 practice questions related to: ${context.slice(0, 200)}`,
  }
  await sendMessage(prompts[type])
}

async function translateMsg(msg) {
  if (msg.urduText) { msg.urdu = true; return }
  loading.value = true
  const text = await callGemini(`Translate this to Urdu (keep technical terms in Urdu script): ${msg.content}`)
  msg.urduText = text
  msg.urdu = true
  loading.value = false
}

async function translateToEng(msg) {
  if (msg.engText) { msg.showEng = true; return }
  loading.value = true
  msg.engText = await callGemini(`Translate this Urdu text to English (plain translation): ${msg.content}`)
  msg.showEng = true
  loading.value = false
}

function formatResponse(text) {
  return text
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.*?)\*/g, '<em>$1</em>')
    .replace(/^- (.+)$/gm, '<li class="ml-4 list-disc">$1</li>')
    .replace(/\n\n/g, '</p><p class="mt-2">')
    .replace(/\n/g, '<br/>')
}

function scrollDown() {
  if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}

function saveSessions() {
  if (messages.value.length < 2) return
  const first = messages.value.find(m => m.role === 'user')
  const preview = first?.content?.slice(0, 50) || 'Chat session'
  const existing = sessions.value.find(s => s.id === activeSession.value)
  if (existing) { existing.preview = preview; existing.messages = messages.value }
  else {
    const s = { id: Date.now(), preview, messages: messages.value }
    sessions.value.unshift(s)
    activeSession.value = s.id
  }
  localStorage.setItem(`estudy_chat_${props.subject}`, JSON.stringify(sessions.value.slice(0, 20)))
}

function loadSession(s) { messages.value = s.messages; activeSession.value = s.id }
function clearHistory() { sessions.value = []; localStorage.removeItem(`estudy_chat_${props.subject}`) }
</script>

<style scoped>
.cv-div-b { border-bottom: 1px solid var(--t-border); }
.cv-div-t { border-top: 1px solid var(--t-border); }

.cv-session { color: var(--t-text2); transition: background 0.15s, color 0.15s; }
.cv-session:hover { background: var(--t-hover); color: var(--t-text1); }
.cv-session-active { background: var(--t-acc-alpha-sm) !important; color: var(--t-accent) !important; }

.cv-bubble-ai { background: var(--t-hover); color: var(--t-text1); }
.cv-bubble-user { background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); color: #fff; }
.cv-user-avatar { background: var(--t-acc-alpha-md); color: var(--t-accent); }
.cv-dot { background: var(--t-text3); }

/* Urdu mode badge */
.cv-urdu-badge {
  display: inline-block;
  font-family: 'Noto Nastaliq Urdu', serif;
  font-size: 0.65rem;
  padding: 0.1rem 0.45rem;
  border-radius: 99px;
  background: color-mix(in srgb, #22c55e 14%, transparent);
  border: 1px solid color-mix(in srgb, #22c55e 35%, transparent);
  color: #16a34a;
  margin-right: 0.4rem;
  vertical-align: middle;
}
html.dark .cv-urdu-badge { color: #4ade80; }

/* Urdu input hint */
.cv-urdu-hint {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 0.4rem;
  padding: 0.3rem 0.5rem;
  border-radius: 8px;
  background: color-mix(in srgb, var(--t-accent) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-accent) 20%, transparent);
}
.cv-urdu-hint .urdu { color: var(--t-accent); font-weight: 600; }

/* Urdu input */
.cv-input-urdu {
  font-family: 'Noto Nastaliq Urdu', serif !important;
  font-size: 1rem !important;
  line-height: 2 !important;
  text-align: right;
}

.cv-starter {
  background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2);
  transition: all 0.15s; cursor: pointer;
}
.cv-starter:hover { border-color: color-mix(in srgb, var(--t-accent) 45%, transparent); color: var(--t-accent); }

.chip-action {
  display: inline-flex; align-items: center; gap: 0.25rem;
  padding: 0.25rem 0.625rem; border-radius: 99px; font-size: 0.75rem; font-weight: 500;
  background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2);
  transition: all 0.15s; cursor: pointer;
}
.chip-action:hover { background: var(--t-hover); border-color: var(--t-text3); color: var(--t-text1); }

.cv-green-chip {
  display: inline-flex; align-items: center; gap: 0.25rem;
  padding: 0.25rem 0.625rem; border-radius: 99px; font-size: 0.75rem; font-weight: 600;
  background: color-mix(in srgb, #22c55e 14%, transparent);
  border: 1px solid color-mix(in srgb, #22c55e 35%, transparent);
  color: #16a34a; transition: all 0.15s; cursor: pointer;
}
.cv-green-chip:hover { background: color-mix(in srgb, #22c55e 22%, transparent); }
html.dark .cv-green-chip { color: #4ade80; }

/* Urdu translation block (non-Urdu subjects) */
.cv-urdu {
  background: color-mix(in srgb, #22c55e 10%, transparent);
  border: 1px solid color-mix(in srgb, #22c55e 28%, transparent);
}
.cv-urdu-label { color: #16a34a; }
.cv-urdu-text { color: var(--t-text1); }
html.dark .cv-urdu-label { color: #4ade80; }

/* English translation block (Urdu subjects) */
.cv-eng {
  background: color-mix(in srgb, #3b82f6 10%, transparent);
  border: 1px solid color-mix(in srgb, #3b82f6 28%, transparent);
}
.cv-eng-label { color: #2563eb; }
.cv-eng-text { color: var(--t-text1); }
html.dark .cv-eng-label { color: #60a5fa; }

.sidebar-enter-active, .sidebar-leave-active { transition: all 0.3s ease; }
.sidebar-enter-from, .sidebar-leave-to { opacity: 0; width: 0; }
</style>
