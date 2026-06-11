<template>
  <div class="animate-fade-in space-y-4" v-if="tutor">
    <!-- Top bar -->
    <div class="card p-4 flex items-center gap-3 flex-wrap">
      <RouterLink to="/app/ai-tutor" class="btn-ghost text-sm"><ArrowLeft class="w-4 h-4" /> Back</RouterLink>
      <div class="w-9 h-9 rounded-xl flex items-center justify-center text-2xl" :class="tutor.color">{{ tutor.icon }}</div>
      <div>
        <div class="font-bold uppercase text-sm tracking-wide" style="color:var(--t-text1)">{{ tutor.persona }} — {{ tutor.subject }} Master</div>
        <div class="text-xs" style="color:var(--t-text3)">{{ tutor.desc }}</div>
      </div>
      <div class="ml-auto flex items-center gap-2 flex-wrap">
        <button @click="topicModal = true" class="btn-secondary text-xs"><BookOpen class="w-3.5 h-3.5" /> Select Topic</button>
        <button @click="openMindMap" :disabled="!selectedTopic" class="btn-secondary text-xs"><Network class="w-3.5 h-3.5" /> MindMap</button>
        <select v-model="speed" class="vv-select text-xs">
          <option value="0.5">Slow</option>
          <option value="1">1x</option>
          <option value="1.25">1.25x</option>
          <option value="1.5">1.5x</option>
          <option value="2">2x</option>
        </select>
        <div class="vv-lang-group">
          <span class="vv-lang-label">Speak</span>
          <select v-model="speakLang" class="vv-select text-xs" title="Lesson speaking language">
            <option value="en">English</option>
            <option value="ur">اردو</option>
          </select>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Teacher stage -->
      <div class="lg:col-span-2 space-y-4">
        <div class="card overflow-hidden">
          <div class="aspect-video relative flex items-center justify-center" :class="tutor.color">
            <div class="absolute inset-0 opacity-10" style="background-image:radial-gradient(circle at 50% 50%,rgba(255,255,255,0.5) 0%,transparent 70%)" />
            <!-- Live badge -->
            <div v-if="liveMode" class="vv-live-badge"><span class="vv-live-dot" /> LIVE CLASS</div>

            <div class="relative text-center text-white">
              <!-- Avatar with speaking animation -->
              <div class="vv-avatar" :class="{ 'is-speaking': isPlaying }">
                <div class="text-8xl">{{ tutor.icon }}</div>
                <div v-if="isPlaying" class="vv-bars" aria-hidden="true"><span/><span/><span/><span/><span/></div>
              </div>

              <div v-if="!isPlaying && !isLoading && !lessonStarted" class="space-y-3 mt-2">
                <div class="text-lg font-bold">{{ tutor.persona }}</div>
                <div class="text-sm opacity-80">{{ selectedTopic ? 'Topic: ' + selectedTopic.name : 'Select a topic to begin your class' }}</div>
                <button v-if="selectedTopic" @click="startLesson" class="px-5 py-2.5 rounded-xl bg-white/20 hover:bg-white/30 font-semibold text-sm transition-all backdrop-blur-sm">
                  <Play class="w-4 h-4 inline mr-1" /> Start Live Lesson
                </button>
              </div>
              <div v-else-if="isLoading" class="text-white text-sm animate-pulse mt-2">{{ tutor.persona }} is preparing your lesson…</div>
              <div v-else class="space-y-2 mt-2">
                <div class="text-sm font-medium">{{ selectedTopic?.name }}</div>
                <div class="text-xs opacity-80">{{ isPlaying ? (speakLang === 'ur' ? 'پڑھا رہے ہیں…' : 'Teaching…') : 'Paused' }}</div>
                <div class="flex gap-2 justify-center">
                  <button v-if="isPlaying" @click="pauseAudio" class="px-3 py-1.5 rounded-xl bg-white/20 hover:bg-white/30 text-sm transition-all"><Pause class="w-4 h-4 inline" /> Pause</button>
                  <button v-else @click="resumeOrRepeat" class="px-3 py-1.5 rounded-xl bg-white/20 hover:bg-white/30 text-sm transition-all"><Play class="w-4 h-4 inline" /> Resume</button>
                  <button @click="stopAudio" class="px-3 py-1.5 rounded-xl bg-white/20 hover:bg-white/30 text-sm transition-all"><Square class="w-4 h-4 inline" /> Stop</button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Class conversation (live teacher Q&A thread) -->
        <div class="card p-4" v-if="lessonStarted || qaThread.length">
          <div class="flex items-center justify-between mb-3">
            <div class="text-sm font-semibold" style="color:var(--t-text1)">
              {{ speakLang === 'ur' ? 'کلاس گفتگو' : 'Class Conversation' }}
            </div>
            <button v-if="lessonText" @click="speak(lastTeacherMsg)" class="btn-ghost text-xs" :title="speakLang === 'ur' ? 'دوبارہ سنیں' : 'Replay'">
              <Volume2 class="w-3.5 h-3.5" /> {{ speakLang === 'ur' ? 'سنیں' : 'Replay' }}
            </button>
          </div>

          <div class="vv-thread" ref="threadEl">
            <div v-for="(m, i) in qaThread" :key="i" class="vv-msg" :class="m.role === 'student' ? 'vv-msg-student' : 'vv-msg-teacher'">
              <div class="vv-msg-ava" :class="m.role === 'student' ? 'vv-ava-student' : tutor.color">
                <User v-if="m.role === 'student'" class="w-3.5 h-3.5" />
                <span v-else>{{ tutor.icon }}</span>
              </div>
              <div class="vv-msg-body"
                :class="speakLang === 'ur' ? 'urdu leading-loose' : ''"
                :dir="speakLang === 'ur' ? 'rtl' : 'ltr'">{{ m.text }}</div>
            </div>
            <div v-if="isAsking" class="vv-msg vv-msg-teacher">
              <div class="vv-msg-ava" :class="tutor.color"><span>{{ tutor.icon }}</span></div>
              <div class="vv-msg-body vv-typing"><span/><span/><span/></div>
            </div>
          </div>

          <!-- Ask the teacher -->
          <form class="vv-ask" @submit.prevent="askTutor()">
            <input v-model="askText" class="vv-ask-input"
              :placeholder="speakLang === 'ur' ? 'استاد سے سوال پوچھیں…' : 'Ask your teacher a question…'"
              :dir="speakLang === 'ur' ? 'rtl' : 'ltr'" />
            <button type="button" @click="toggleVoice" :class="['vv-ask-mic', isListening ? 'is-live' : '']" :title="speakLang === 'ur' ? 'بول کر پوچھیں' : 'Ask by voice'">
              <Mic class="w-4 h-4" />
            </button>
            <button type="submit" class="vv-ask-send" :disabled="!askText.trim() || isAsking"><Send class="w-4 h-4" /></button>
          </form>
          <p v-if="isListening" class="text-xs mt-1.5 italic" style="color:var(--t-accent)">{{ speakLang === 'ur' ? 'سن رہے ہیں…' : 'Listening…' }}</p>
        </div>
      </div>

      <!-- Right rail -->
      <div class="space-y-4">
        <!-- Live Mode -->
        <div class="card p-4 space-y-3">
          <div class="flex items-center justify-between">
            <div>
              <span class="text-sm font-semibold" style="color:var(--t-text1)">Live Mode</span>
              <p class="text-xs mt-0.5" style="color:var(--t-text3)">{{ liveMode ? 'Teacher speaks answers aloud' : 'Text-only (silent)' }}</p>
            </div>
            <button class="vv-toggle" :class="{ 'is-on': liveMode }" role="switch" :aria-checked="liveMode" @click="liveMode = !liveMode">
              <div class="vv-toggle-knob" />
            </button>
          </div>
          <div class="pt-1 border-t" style="border-color:var(--t-border)">
            <div class="text-xs font-medium mb-2 mt-2" style="color:var(--t-text2)">Voice Input Language</div>
            <div class="flex gap-2">
              <button @click="toggleVoice" :class="['btn-secondary text-xs flex-1 justify-center', isListening ? 'vv-listening' : '']">
                <Mic class="w-3.5 h-3.5" /> {{ isListening ? 'Stop' : 'Ask by Voice' }}
              </button>
              <select v-model="voiceLang" class="vv-select text-xs">
                <option value="en-US">EN</option>
                <option value="ur-PK">اردو</option>
              </select>
            </div>
          </div>
        </div>

        <!-- Suggested questions -->
        <div class="card p-4" v-if="selectedTopic">
          <div class="text-sm font-semibold mb-2" style="color:var(--t-text1)">Ask about this topic</div>
          <div class="space-y-1.5">
            <button v-for="(q, i) in suggestedQuestions" :key="i" @click="askTutor(q)" :disabled="isAsking"
              class="vv-suggest">{{ q }}</button>
          </div>
        </div>

        <!-- Recent sessions -->
        <div class="card p-4">
          <div class="text-sm font-semibold mb-3" style="color:var(--t-text1)">Recent Sessions</div>
          <div v-if="recentSessions.length" class="space-y-1">
            <div v-for="s in recentSessions" :key="s.time" class="flex items-start gap-2 text-xs py-1" style="color:var(--t-text2)">
              <Clock class="w-3.5 h-3.5 shrink-0 mt-0.5" />
              <div>
                <div class="font-medium" style="color:var(--t-text1)">{{ s.topic }}</div>
                <div>{{ s.time }}</div>
              </div>
            </div>
          </div>
          <div v-else class="text-xs text-center py-4" style="color:var(--t-text3)">No recent sessions yet.</div>
        </div>
      </div>
    </div>

    <!-- Topic Select Modal -->
    <Dialog v-model:visible="topicModal" header="Select a Topic" :modal="true" :style="{ width: '420px' }">
      <div class="space-y-3">
        <div>
          <label class="label">Unit</label>
          <select v-model="selectedUnit" @change="selectedTopic = null" class="input">
            <option :value="null">— Select Unit —</option>
            <option v-for="u in units" :key="u.id" :value="u">{{ u.name }}</option>
          </select>
        </div>
        <div v-if="selectedUnit">
          <label class="label">Topic</label>
          <select v-model="selectedTopic" class="input">
            <option :value="null">— Select Topic —</option>
            <option v-for="t in selectedUnit.topics" :key="t.id" :value="t">{{ t.name }}</option>
          </select>
        </div>
        <div class="flex gap-2 pt-2">
          <button @click="topicModal = false; if(selectedTopic) startLesson()" class="btn-primary flex-1 justify-center" :disabled="!selectedTopic">
            <Play class="w-4 h-4" /> Start Lesson
          </button>
          <button @click="topicModal = false" class="btn-secondary">Cancel</button>
        </div>
      </div>
    </Dialog>

    <!-- MindMap Modal -->
    <Dialog v-model:visible="mindMapModal" :header="'MindMap: ' + (selectedTopic?.name || '')" :modal="true" :style="{ width: '720px' }" :breakpoints="{ '768px': '95vw' }">
      <div class="mm-wrap">
        <div v-if="mindMapLoading" class="mm-state">
          <Sparkles class="w-10 h-10 mb-3 mm-spin" />
          <p class="text-sm">{{ tutor.persona }} is mapping out the key ideas…</p>
        </div>

        <div v-else-if="mindMapError" class="mm-state">
          <Network class="w-12 h-12 mb-3 opacity-30" />
          <p class="text-sm mb-3">{{ mindMapError }}</p>
          <button @click="generateMindMap" class="btn-secondary text-xs"><RefreshCw class="w-3.5 h-3.5" /> Try again</button>
        </div>

        <div v-else-if="mindMap" class="mm-map">
          <div class="mm-central" :class="tutor.color">{{ mindMap.central }}</div>
          <div class="mm-branches">
            <div v-for="(b, i) in mindMap.branches" :key="i" class="mm-branch" :style="{ '--mm-accent': branchColor(i) }">
              <div class="mm-branch-title">{{ b.title }}</div>
              <ul class="mm-points">
                <li v-for="(p, pi) in b.points" :key="pi">{{ p }}</li>
              </ul>
            </div>
          </div>
          <div class="mm-foot">
            <button @click="generateMindMap" class="btn-ghost text-xs"><RefreshCw class="w-3.5 h-3.5" /> Regenerate</button>
            <span class="text-xs" style="color:var(--t-text3)">Generated by {{ tutor.persona }} · Gemini</span>
          </div>
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { ArrowLeft, Play, Pause, Square, Mic, Network, BookOpen, Clock, Send, Sparkles, User, Volume2, RefreshCw } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import { AI_TUTORS, useContentStore, SUBJECTS } from '@/stores/content'

const props = defineProps({ subject: String })
const content = useContentStore()

const tutor = computed(() => AI_TUTORS.find(t => t.slug === props.subject))
const subjectObj = computed(() => SUBJECTS.find(s => s.name === tutor.value?.subject))
const units = computed(() => content.units[subjectObj.value?.id] || [])

const speed = ref('1')
const speakLang = ref('en')   // tutor speaking language: 'en' | 'ur'
const topicModal = ref(false)
const mindMapModal = ref(false)
const selectedUnit = ref(null)
const selectedTopic = ref(null)
const isLoading = ref(false)
const isPlaying = ref(false)
const lessonStarted = ref(false)
const lessonText = ref('')
const voiceLang = ref('en-US')
const isListening = ref(false)
const liveMode = ref(true)
const recentSessions = ref([])

// Live class conversation
const qaThread = ref([])
const askText = ref('')
const isAsking = ref(false)
const threadEl = ref(null)

// MindMap
const mindMap = ref(null)
const mindMapLoading = ref(false)
const mindMapError = ref('')

let audio = null
let recognition = null

const GEMINI_KEY = (import.meta.env.VITE_GEMINI_API_KEY || '').trim()
const TTS_KEY = (import.meta.env.VITE_TTS_API_KEY || '').trim()

const lastTeacherMsg = computed(() => [...qaThread.value].reverse().find(m => m.role === 'teacher')?.text || lessonText.value)

const suggestedQuestions = computed(() => {
  const t = selectedTopic.value?.name || 'this topic'
  return speakLang.value === 'ur'
    ? [`${t} کی آسان مثال دیں`, `${t} میں عام غلطیاں کیا ہیں؟`, `${t} پر بورڈ کا اہم سوال`]
    : [`Give me a simple example of ${t}`, `What mistakes do students make in ${t}?`, `Ask me a question on ${t}`]
})

function branchColor(i) {
  return ['#6d54e8', '#0891b2', '#059669', '#d97706', '#db2777', '#2563eb'][i % 6]
}

// ── Gemini helper ──────────────────────────────────────────────
async function callGemini(prompt, { json = false } = {}) {
  if (!GEMINI_KEY) throw new Error('no-key')
  const body = { contents: [{ parts: [{ text: prompt }] }] }
  if (json) body.generationConfig = { responseMimeType: 'application/json' }
  const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=${GEMINI_KEY}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error('http-' + res.status)
  const d = await res.json()
  return d.candidates?.[0]?.content?.parts?.[0]?.text || ''
}

async function scrollThread() {
  await nextTick()
  if (threadEl.value) threadEl.value.scrollTop = threadEl.value.scrollHeight
}

// ── Lesson ─────────────────────────────────────────────────────
async function startLesson() {
  if (!selectedTopic.value) return
  topicModal.value = false
  isLoading.value = true
  lessonStarted.value = true
  qaThread.value = []

  const isUrdu = speakLang.value === 'ur'
  let text = isUrdu
    ? `السلام علیکم! میں ${tutor.value?.persona} ہوں۔ آئیے ${tutor.value?.subject} میں "${selectedTopic.value.name}" کو سمجھتے ہیں۔ یہ ایک اہم موضوع ہے؛ غور سے سنیں اور بلا جھجھک سوال پوچھیں۔`
    : `Assalam-o-Alaikum! I'm ${tutor.value?.persona}, and today we'll explore "${selectedTopic.value.name}" in ${tutor.value?.subject}. Listen closely, and ask me anything whenever you like.`

  try {
    const prompt = isUrdu
      ? `آپ ${tutor.value?.persona} ہیں، بلوچستان بورڈ کے گریڈ 9 کے ${tutor.value?.subject} کے استاد۔ "${selectedTopic.value.name}" پر ایک واضح، دلچسپ 2 منٹ کا سبق دیں جیسے آپ کلاس میں پڑھا رہے ہوں۔ پہلے شخص میں، کردار کے مطابق، اور مکمل طور پر اردو رسم الخط میں بات کریں۔`
      : `You are ${tutor.value?.persona}, a Grade 9 ${tutor.value?.subject} teacher for the Balochistan Board, Pakistan. Give a clear, engaging 2-minute spoken lesson on "${selectedTopic.value.name}" as if teaching live in class. Speak in first person, in character, entirely in English. Start with a warm greeting.`
    const out = await callGemini(prompt)
    if (out) text = out
  } catch { /* keep greeting fallback */ }

  lessonText.value = text
  qaThread.value.push({ role: 'teacher', text })
  isLoading.value = false
  scrollThread()
  if (liveMode.value) speak(text)

  recentSessions.value.unshift({
    topic: selectedTopic.value.name,
    time: new Date().toLocaleTimeString('en-PK', { hour: '2-digit', minute: '2-digit' }),
  })
}

// ── Ask the teacher (live Q&A) ─────────────────────────────────
async function askTutor(qRaw) {
  const q = (typeof qRaw === 'string' ? qRaw : askText.value).trim()
  if (!q || isAsking.value) return
  if (!lessonStarted.value) lessonStarted.value = true
  qaThread.value.push({ role: 'student', text: q })
  askText.value = ''
  isAsking.value = true
  scrollThread()

  const isUrdu = speakLang.value === 'ur'
  let answer = ''
  try {
    const ctx = lessonText.value ? `You are teaching this lesson: "${lessonText.value.slice(0, 700)}". ` : ''
    const prompt = `You are ${tutor.value?.persona}, a Grade 9 ${tutor.value?.subject} teacher for the Balochistan Board, Pakistan. ${ctx}A student asks: "${q}". Answer as the teacher — clear, encouraging, with a simple example if helpful. Speak in first person${isUrdu ? ', entirely in Urdu script' : ', in English'}. Keep it under 130 words.`
    answer = await callGemini(prompt)
  } catch { /* fall through */ }

  if (!answer) {
    answer = isUrdu
      ? 'معذرت، ابھی AI سے رابطہ نہیں ہو سکا۔ براہِ کرم دوبارہ کوشش کریں۔ (VITE_GEMINI_API_KEY چیک کریں)'
      : "Sorry, I couldn't reach the AI just now — please try again. (Check VITE_GEMINI_API_KEY in .env)"
  }
  qaThread.value.push({ role: 'teacher', text: answer })
  isAsking.value = false
  scrollThread()
  if (liveMode.value) speak(answer)
}

// ── MindMap ────────────────────────────────────────────────────
function openMindMap() {
  if (!selectedTopic.value) return
  mindMapModal.value = true
  if (!mindMap.value && !mindMapLoading.value) generateMindMap()
}

async function generateMindMap() {
  if (!selectedTopic.value) return
  mindMapLoading.value = true
  mindMapError.value = ''
  mindMap.value = null
  const isUrdu = speakLang.value === 'ur'
  try {
    const prompt = `Create a concise study mind map for "${selectedTopic.value.name}" in ${tutor.value?.subject} for Grade 9 (Balochistan Board, Pakistan). Return ONLY valid JSON of the form {"central":"<short topic title>","branches":[{"title":"<subtopic>","points":["<key point>","<key point>"]}]}. Give 4 to 6 branches, each with 2 to 4 short points (max ~8 words each). ${isUrdu ? 'Write all titles and points in Urdu script.' : 'Write in English.'}`
    const txt = await callGemini(prompt, { json: true })
    mindMap.value = parseMindMap(txt, selectedTopic.value.name)
  } catch (e) {
    mindMapError.value = e?.message === 'no-key'
      ? 'Add a valid VITE_GEMINI_API_KEY to .env (then restart the dev server) to generate mind maps.'
      : 'Could not generate the mind map just now. Please try again.'
  }
  mindMapLoading.value = false
}

function parseMindMap(txt, fallbackCentral) {
  if (!txt) throw new Error('empty')
  let s = String(txt).trim().replace(/^```json\s*/i, '').replace(/^```\s*/, '').replace(/```\s*$/, '').trim()
  let j
  try { j = JSON.parse(s) }
  catch {
    const m = s.match(/\{[\s\S]*\}/)
    if (m) j = JSON.parse(m[0])
  }
  if (!j || !Array.isArray(j.branches) || !j.branches.length) throw new Error('shape')
  return {
    central: String(j.central || fallbackCentral).slice(0, 60),
    branches: j.branches.slice(0, 6).map(b => ({
      title: String(b.title || '').slice(0, 70),
      points: (Array.isArray(b.points) ? b.points : []).slice(0, 5).map(p => String(p)),
    })),
  }
}

// ── Speech (Google TTS or browser fallback) ────────────────────
async function speak(text) {
  stopAudio()
  if (!text) return
  if (TTS_KEY) { await playTTS(text); return }
  const isUrdu = speakLang.value === 'ur'
  const utt = new SpeechSynthesisUtterance(String(text).slice(0, 600))
  utt.lang = isUrdu ? 'ur-PK' : 'en-US'
  utt.rate = parseFloat(speed.value)
  const match = speechSynthesis.getVoices().find(v => v.lang?.toLowerCase().startsWith(isUrdu ? 'ur' : 'en'))
  if (match) utt.voice = match
  utt.onend = () => { isPlaying.value = false }
  speechSynthesis.speak(utt)
  isPlaying.value = true
}

async function playTTS(text) {
  isPlaying.value = true
  const voice = speakLang.value === 'ur'
    ? { languageCode: 'ur-PK', ssmlGender: 'MALE' }
    : { languageCode: 'en-US', name: 'en-US-Neural2-D', ssmlGender: 'MALE' }
  try {
    const res = await fetch(`https://texttospeech.googleapis.com/v1/text:synthesize?key=${TTS_KEY}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        input: { text: String(text).slice(0, 800) },
        voice,
        audioConfig: { audioEncoding: 'MP3', speakingRate: parseFloat(speed.value) },
      }),
    })
    const d = await res.json()
    if (d.audioContent) {
      audio = new Audio('data:audio/mp3;base64,' + d.audioContent)
      audio.onended = () => { isPlaying.value = false }
      audio.play()
    } else { isPlaying.value = false }
  } catch { isPlaying.value = false }
}

function pauseAudio() { audio?.pause(); speechSynthesis.pause(); isPlaying.value = false }
function resumeOrRepeat() {
  if (audio) { audio.play(); isPlaying.value = true }
  else if (speechSynthesis.paused) { speechSynthesis.resume(); isPlaying.value = true }
  else speak(lastTeacherMsg.value)
}
function stopAudio() { audio?.pause(); audio = null; speechSynthesis.cancel(); isPlaying.value = false }

// Stop any in-progress playback when the speaking language is switched
watch(speakLang, stopAudio)

// ── Voice input → ask the teacher ──────────────────────────────
function toggleVoice() {
  if (isListening.value) { recognition?.stop(); isListening.value = false; return }
  const SR = window.SpeechRecognition || window.webkitSpeechRecognition
  if (!SR) { alert('Voice input is not supported in this browser.'); return }
  recognition = new SR()
  recognition.lang = voiceLang.value
  recognition.continuous = false
  recognition.onresult = e => {
    const transcript = e.results[0][0].transcript
    askText.value = transcript
    isListening.value = false
    askTutor(transcript)
  }
  recognition.onerror = recognition.onend = () => { isListening.value = false }
  recognition.start()
  isListening.value = true
}
</script>

<style scoped>
.vv-select {
  border: 1px solid var(--t-border); border-radius: 12px; padding: 0.375rem 0.5rem;
  background: var(--t-surface); color: var(--t-text1);
}
.vv-lang-group {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding-left: 0.5rem; border-left: 1px solid var(--t-border);
}
.vv-lang-label {
  font-size: 0.7rem; font-weight: 600; letter-spacing: 0.04em;
  text-transform: uppercase; color: var(--t-text3);
}

/* Live badge + avatar */
.vv-live-badge {
  position: absolute; top: 0.75rem; left: 0.75rem;
  display: inline-flex; align-items: center; gap: 0.35rem;
  padding: 0.2rem 0.6rem; border-radius: 99px;
  background: rgba(0,0,0,0.35); color: #fff; font-size: 0.62rem; font-weight: 800; letter-spacing: 0.08em;
  backdrop-filter: blur(4px);
}
.vv-live-dot { width: 7px; height: 7px; border-radius: 50%; background: #ef4444; animation: vvPulse 1.4s ease-in-out infinite; }
@keyframes vvPulse { 0%,100% { opacity: 1; transform: scale(1); } 50% { opacity: 0.4; transform: scale(0.7); } }

.vv-avatar { position: relative; display: inline-block; transition: transform 0.3s; }
.vv-avatar.is-speaking { animation: vvBob 1.8s ease-in-out infinite; }
@keyframes vvBob { 0%,100% { transform: translateY(0); } 50% { transform: translateY(-6px); } }
.vv-bars { display: flex; gap: 3px; justify-content: center; align-items: flex-end; height: 16px; margin-top: 6px; }
.vv-bars span {
  width: 3px; background: rgba(255,255,255,0.9); border-radius: 2px;
  animation: vvBar 0.9s ease-in-out infinite;
}
.vv-bars span:nth-child(1) { animation-delay: 0s; }
.vv-bars span:nth-child(2) { animation-delay: 0.15s; }
.vv-bars span:nth-child(3) { animation-delay: 0.3s; }
.vv-bars span:nth-child(4) { animation-delay: 0.45s; }
.vv-bars span:nth-child(5) { animation-delay: 0.6s; }
@keyframes vvBar { 0%,100% { height: 4px; } 50% { height: 16px; } }

/* Conversation thread */
.vv-thread { max-height: 320px; overflow-y: auto; display: flex; flex-direction: column; gap: 0.75rem; padding-right: 0.25rem; }
.vv-msg { display: flex; gap: 0.6rem; align-items: flex-start; }
.vv-msg-student { flex-direction: row-reverse; }
.vv-msg-ava {
  width: 30px; height: 30px; border-radius: 50%; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center; font-size: 0.95rem; color: #fff;
}
.vv-ava-student { background: var(--t-hover2); color: var(--t-text2); }
.vv-msg-body {
  max-width: 80%; padding: 0.6rem 0.85rem; border-radius: 14px; font-size: 0.84rem; line-height: 1.55;
  background: var(--t-hover); color: var(--t-text1); white-space: pre-line;
}
.vv-msg-student .vv-msg-body {
  background: color-mix(in srgb, var(--t-accent) 14%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-accent) 25%, transparent);
}
.vv-typing { display: flex; gap: 4px; align-items: center; }
.vv-typing span { width: 6px; height: 6px; border-radius: 50%; background: var(--t-text3); animation: vvType 1.2s ease-in-out infinite; }
.vv-typing span:nth-child(2) { animation-delay: 0.2s; }
.vv-typing span:nth-child(3) { animation-delay: 0.4s; }
@keyframes vvType { 0%,80%,100% { transform: scale(0.6); opacity: 0.4; } 40% { transform: scale(1); opacity: 1; } }

/* Ask bar */
.vv-ask { display: flex; gap: 0.5rem; align-items: center; margin-top: 0.85rem; }
.vv-ask-input {
  flex: 1; padding: 0.55rem 0.85rem; border-radius: 12px; font-size: 0.84rem;
  background: var(--t-input-bg); border: 1px solid var(--t-input-border); color: var(--t-text1);
}
.vv-ask-input:focus { outline: none; border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-input-focus); }
.vv-ask-mic, .vv-ask-send {
  width: 38px; height: 38px; border-radius: 11px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center; cursor: pointer; border: 1px solid var(--t-border);
  background: var(--t-surface); color: var(--t-text2); transition: all 0.15s;
}
.vv-ask-mic:hover, .vv-ask-send:hover { color: var(--t-accent); border-color: var(--t-accent); }
.vv-ask-mic.is-live { background: color-mix(in srgb, #ef4444 12%, transparent); color: #ef4444; border-color: rgba(239,68,68,0.4); }
.vv-ask-send { background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); color: #fff; border: none; }
.vv-ask-send:disabled { opacity: 0.4; cursor: not-allowed; }

/* Suggested questions */
.vv-suggest {
  width: 100%; text-align: left; padding: 0.5rem 0.7rem; border-radius: 10px; font-size: 0.76rem;
  background: var(--t-hover); border: 1px solid var(--t-border); color: var(--t-text2); cursor: pointer; transition: all 0.15s;
}
.vv-suggest:hover { border-color: var(--t-accent); color: var(--t-accent); }
.vv-suggest:disabled { opacity: 0.5; cursor: not-allowed; }

/* Live Mode toggle */
.vv-toggle {
  width: 2.5rem; height: 1.5rem; border-radius: 99px; position: relative; cursor: pointer; flex-shrink: 0;
  border: none; background: var(--t-hover2); transition: background 0.2s;
}
.vv-toggle.is-on { background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.vv-toggle-knob {
  width: 1.25rem; height: 1.25rem; border-radius: 50%; background: #fff;
  box-shadow: 0 1px 3px rgba(0,0,0,0.3); position: absolute; top: 0.125rem; left: 0.125rem; transition: left 0.2s;
}
.vv-toggle.is-on .vv-toggle-knob { left: 1.125rem; }
.vv-listening {
  border-color: rgba(239,68,68,0.5) !important; color: #ef4444 !important;
  background: color-mix(in srgb, #ef4444 12%, transparent) !important;
}

/* MindMap */
.mm-wrap { min-height: 220px; }
.mm-state { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 2.5rem 1rem; text-align: center; color: var(--t-text3); }
.mm-spin { color: var(--t-accent); animation: mmSpin 1.4s linear infinite; }
@keyframes mmSpin { to { transform: rotate(360deg); } }
.mm-map { display: flex; flex-direction: column; align-items: center; }
.mm-central {
  padding: 0.7rem 1.4rem; border-radius: 14px; color: #fff; font-weight: 800; font-size: 1rem;
  text-align: center; max-width: 90%; margin-bottom: 0.5rem; box-shadow: var(--t-shadow);
}
.mm-branches { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 0.75rem; width: 100%; margin-top: 0.75rem; }
.mm-branch { border: 1px solid var(--t-border); border-left: 4px solid var(--mm-accent, var(--t-accent)); border-radius: 12px; padding: 0.85rem; background: var(--t-surface); }
.mm-branch-title { font-size: 0.85rem; font-weight: 700; color: var(--t-text1); margin-bottom: 0.4rem; }
.mm-points { margin: 0; padding-left: 1.05rem; display: flex; flex-direction: column; gap: 0.25rem; }
.mm-points li { font-size: 0.78rem; color: var(--t-text2); line-height: 1.45; }
.mm-foot { display: flex; align-items: center; justify-content: space-between; width: 100%; margin-top: 1rem; padding-top: 0.75rem; border-top: 1px solid var(--t-border); }

@media (prefers-reduced-motion: reduce) {
  .vv-avatar.is-speaking, .vv-bars span, .vv-live-dot, .vv-typing span, .mm-spin { animation: none; }
}
</style>
