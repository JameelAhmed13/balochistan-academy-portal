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
        <button @click="mindMapModal = true" :disabled="!selectedTopic" class="btn-secondary text-xs"><Network class="w-3.5 h-3.5" /> MindMap</button>
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
      <!-- Avatar / video panel -->
      <div class="lg:col-span-2 space-y-4">
        <div class="card overflow-hidden">
          <div class="aspect-video relative flex items-center justify-center" :class="tutor.color">
            <div class="absolute inset-0 opacity-10" style="background-image:radial-gradient(circle at 50% 50%,rgba(255,255,255,0.5) 0%,transparent 70%)" />
            <div class="relative text-center text-white">
              <div class="text-8xl mb-4">{{ tutor.icon }}</div>
              <div v-if="!isPlaying && !isLoading" class="space-y-3">
                <div class="text-lg font-bold">{{ tutor.persona }}</div>
                <div class="text-sm opacity-80">{{ selectedTopic ? 'Topic: ' + selectedTopic.name : 'Select a topic to begin' }}</div>
                <button v-if="selectedTopic" @click="startLesson" class="px-5 py-2.5 rounded-xl bg-white/20 hover:bg-white/30 font-semibold text-sm transition-all backdrop-blur-sm">
                  <Play class="w-4 h-4 inline mr-1" /> Start Lesson
                </button>
              </div>
              <div v-else-if="isLoading" class="text-white text-sm animate-pulse">Generating lesson…</div>
              <div v-else class="space-y-2">
                <div class="text-sm font-medium">{{ selectedTopic?.name }}</div>
                <div class="flex gap-2 justify-center">
                  <button @click="pauseAudio" class="px-3 py-1.5 rounded-xl bg-white/20 hover:bg-white/30 text-sm transition-all"><Pause class="w-4 h-4 inline" /> Pause</button>
                  <button @click="stopAudio"  class="px-3 py-1.5 rounded-xl bg-white/20 hover:bg-white/30 text-sm transition-all"><Square class="w-4 h-4 inline" /> Stop</button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Lesson text -->
        <div v-if="lessonText" class="card p-5">
          <div class="text-sm font-semibold mb-2" style="color:var(--t-text1)">{{ speakLang === 'ur' ? 'سبق کا متن' : 'Lesson Transcript' }}</div>
          <div class="text-sm leading-relaxed whitespace-pre-line"
            :class="speakLang === 'ur' ? 'urdu leading-loose text-right' : ''"
            :dir="speakLang === 'ur' ? 'rtl' : 'ltr'"
            style="color:var(--t-text2)">{{ lessonText }}</div>
        </div>
      </div>

      <!-- Right rail -->
      <div class="space-y-4">
        <!-- Mode & Voice -->
        <div class="card p-4 space-y-3">
          <div class="flex items-center justify-between">
            <span class="text-sm font-semibold" style="color:var(--t-text1)">Live Mode</span>
            <div class="vv-toggle"><div class="vv-toggle-knob" /></div>
          </div>
          <div>
            <div class="text-xs font-medium mb-2" style="color:var(--t-text2)">Voice Input</div>
            <div class="flex gap-2">
              <button @click="toggleVoice" :class="['btn-secondary text-xs flex-1 justify-center', isListening ? 'vv-listening' : '']">
                <Mic class="w-3.5 h-3.5" /> {{ isListening ? 'Stop' : 'Voice' }}
              </button>
              <select v-model="voiceLang" class="vv-select text-xs">
                <option value="en-US">EN</option>
                <option value="ur-PK">اردو</option>
              </select>
            </div>
            <p v-if="voiceText" class="text-xs mt-2 italic" style="color:var(--t-text2)">{{ voiceText }}</p>
          </div>
        </div>

        <!-- Recent Chats -->
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
          <div v-else class="text-xs text-center py-4" style="color:var(--t-text3)">No recent sessions.</div>
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
    <Dialog v-model:visible="mindMapModal" :header="'MindMap: ' + (selectedTopic?.name || '')" :modal="true" :style="{ width: '600px' }">
      <div class="text-center py-8" style="color:var(--t-text3)">
        <Network class="w-16 h-16 mx-auto mb-3 opacity-30" />
        <p class="text-sm">MindMap generation requires an active Gemini API key.</p>
      </div>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { ArrowLeft, Play, Pause, Square, Mic, Network, BookOpen, Clock } from '@lucide/vue'
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
const lessonText = ref('')
const voiceLang = ref('en-US')
const isListening = ref(false)
const voiceText = ref('')
const recentSessions = ref([])
let audio = null

const GEMINI_KEY = import.meta.env.VITE_GEMINI_API_KEY || ''
const TTS_KEY = import.meta.env.VITE_TTS_API_KEY || ''

async function startLesson() {
  if (!selectedTopic.value) return
  topicModal.value = false
  isLoading.value = true

  const isUrdu = speakLang.value === 'ur'
  let text = isUrdu
    ? `[ڈیمو] میں ${tutor.value?.persona} ہوں۔ آئیے ${tutor.value?.subject} میں "${selectedTopic.value.name}" کو سمجھتے ہیں۔ یہاں اصل Gemini API ایک تفصیلی، کردار کے مطابق سبق تیار کرے گا۔ براہِ کرم VITE_GEMINI_API_KEY ترتیب دیں۔`
    : `[Demo] As ${tutor.value?.persona}, let me explain ${selectedTopic.value.name} in ${tutor.value?.subject}. This is where the Gemini API would generate a detailed, persona-styled lesson. Configure VITE_GEMINI_API_KEY for live AI lessons.`

  if (GEMINI_KEY) {
    const prompt = isUrdu
      ? `آپ ${tutor.value?.persona} ہیں۔ گریڈ 9 کے طلباء کے لیے ${tutor.value?.subject} میں "${selectedTopic.value.name}" پر ایک واضح، دلچسپ 2 منٹ کا سبق دیں۔ پہلے شخص میں، کردار کے مطابق، اور مکمل طور پر اردو رسم الخط میں بات کریں۔`
      : `As ${tutor.value?.persona}, give a clear, engaging 2-minute lesson on "${selectedTopic.value.name}" in ${tutor.value?.subject} for Grade 9 students. Speak in first person, in character, entirely in English.`
    try {
      const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=${GEMINI_KEY}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          contents: [{ parts: [{ text: prompt }] }],
        }),
      })
      const d = await res.json()
      text = d.candidates?.[0]?.content?.parts?.[0]?.text || text
    } catch {}
  }

  lessonText.value = text
  isLoading.value = false

  if (TTS_KEY) {
    await playTTS(text)
  } else {
    // Browser TTS fallback — match the chosen speaking language
    const utt = new SpeechSynthesisUtterance(text.slice(0, 500))
    utt.lang = isUrdu ? 'ur-PK' : 'en-US'
    utt.rate = parseFloat(speed.value)
    const match = speechSynthesis.getVoices().find(v => v.lang?.toLowerCase().startsWith(isUrdu ? 'ur' : 'en'))
    if (match) utt.voice = match
    utt.onend = () => { isPlaying.value = false }
    speechSynthesis.speak(utt)
    isPlaying.value = true
  }

  recentSessions.value.unshift({
    topic: selectedTopic.value.name,
    time: new Date().toLocaleTimeString('en-PK', { hour: '2-digit', minute: '2-digit' }),
  })
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
        input: { text: text.slice(0, 800) },
        voice,
        audioConfig: { audioEncoding: 'MP3', speakingRate: parseFloat(speed.value) },
      }),
    })
    const d = await res.json()
    if (d.audioContent) {
      audio = new Audio('data:audio/mp3;base64,' + d.audioContent)
      audio.onended = () => { isPlaying.value = false }
      audio.play()
    }
  } catch { isPlaying.value = false }
}

function pauseAudio() { audio?.pause(); speechSynthesis.pause() }
function stopAudio()  { audio?.pause(); audio = null; speechSynthesis.cancel(); isPlaying.value = false }

// Stop any in-progress playback when the speaking language is switched
watch(speakLang, stopAudio)

function toggleVoice() {
  if (isListening.value) { recognition?.stop(); isListening.value = false; return }
  const SR = window.SpeechRecognition || window.webkitSpeechRecognition
  if (!SR) { alert('Voice input not supported.'); return }
  const r = new SR(); r.lang = voiceLang.value; r.continuous = false
  r.onresult = e => { voiceText.value = e.results[0][0].transcript }
  r.onerror = r.onend = () => { isListening.value = false }
  r.start(); isListening.value = true
}
let recognition = null
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
.vv-toggle {
  width: 2.5rem; height: 1.5rem; border-radius: 99px; position: relative; cursor: pointer;
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
}
.vv-toggle-knob {
  width: 1.25rem; height: 1.25rem; border-radius: 50%; background: #fff;
  box-shadow: 0 1px 3px rgba(0,0,0,0.3); position: absolute; top: 0.125rem; right: 0.125rem;
}
.vv-listening {
  border-color: rgba(239,68,68,0.5) !important; color: #ef4444 !important;
  background: color-mix(in srgb, #ef4444 12%, transparent) !important;
}
</style>
