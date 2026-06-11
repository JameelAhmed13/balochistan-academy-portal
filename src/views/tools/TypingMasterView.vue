<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Typing Master</h2>
    <div class="tm-stats-row">
      <div class="tm-stat"><div class="tm-stat-val">{{ wpm }}</div><div class="tm-stat-lbl">WPM</div></div>
      <div class="tm-stat"><div class="tm-stat-val">{{ accuracy }}%</div><div class="tm-stat-lbl">Accuracy</div></div>
      <div class="tm-stat"><div class="tm-stat-val">{{ timeLeft }}s</div><div class="tm-stat-lbl">Time Left</div></div>
    </div>
    <div v-if="!started && !finished" class="tm-ready">
      <div class="tm-ready-text">Start typing the text below. Timer begins on first keystroke.</div>
      <div class="tm-level-row">
        <button v-for="l in levels" :key="l.id" @click="level = l.id; resetTest()"
          :class="['tm-level-btn', level === l.id ? 'tm-level-active' : '']">{{ l.name }}</button>
      </div>
    </div>
    <div class="tm-text-display" v-if="!finished">
      <span v-for="(ch, i) in textChars" :key="i"
        :class="{ 'tm-ch-correct': i < typed.length && typed[i] === ch, 'tm-ch-wrong': i < typed.length && typed[i] !== ch, 'tm-ch-current': i === typed.length }">
        {{ ch }}
      </span>
    </div>
    <textarea v-if="!finished" v-model="typed" @input="onType" ref="inputRef"
      :disabled="finished" class="tm-input" placeholder="Start typing here..."
      rows="3" autocorrect="off" autocapitalize="off" spellcheck="false" />
    <div v-if="finished" class="tm-finished">
      <div class="tm-fin-title">✅ Test Complete!</div>
      <div class="tm-fin-stats">
        <div class="tm-fin-stat"><span>WPM</span><strong>{{ wpm }}</strong></div>
        <div class="tm-fin-stat"><span>Accuracy</span><strong>{{ accuracy }}%</strong></div>
        <div class="tm-fin-stat"><span>Characters</span><strong>{{ typed.length }}</strong></div>
      </div>
      <button @click="resetTest" class="tm-retry-btn">Try Again</button>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed, onUnmounted } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const levels = [
  { id: 'easy', name: 'Easy', text: 'The quick brown fox jumps over the lazy dog. Pakistan is a beautiful country with diverse culture and history.' },
  { id: 'medium', name: 'Medium', text: 'Photosynthesis is the process by which green plants use sunlight to synthesize nutrients from carbon dioxide and water.' },
  { id: 'hard', name: 'Hard', text: 'The mitochondria are membrane-bound organelles found in the cytoplasm of eukaryotic cells that generate most of the cell\'s supply of adenosine triphosphate.' },
]

const level = ref('easy')
const fullText = computed(() => levels.find(l => l.id === level.value)?.text || '')
const textChars = computed(() => fullText.value.split(''))
const typed = ref('')
const started = ref(false)
const finished = ref(false)
const timeLeft = ref(60)
const correctChars = ref(0)
let timer = null

const wpm = computed(() => {
  if (!started.value) return 0
  const elapsed = (60 - timeLeft.value) || 1
  return Math.round((correctChars.value / 5) / (elapsed / 60))
})
const accuracy = computed(() => {
  if (!typed.value.length) return 100
  const correct = typed.value.split('').filter((ch, i) => ch === fullText.value[i]).length
  return Math.round(correct / typed.value.length * 100)
})

function onType() {
  if (!started.value) {
    started.value = true
    timer = setInterval(() => {
      timeLeft.value--
      if (timeLeft.value <= 0) { clearInterval(timer); finished.value = true }
    }, 1000)
  }
  correctChars.value = typed.value.split('').filter((ch, i) => ch === fullText.value[i]).length
  if (typed.value === fullText.value) { clearInterval(timer); finished.value = true }
}

function resetTest() {
  clearInterval(timer)
  typed.value = ''; started.value = false; finished.value = false; timeLeft.value = 60; correctChars.value = 0
}

onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.tm-stats-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem; }
.tm-stat { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; padding: 0.85rem; text-align: center; }
.tm-stat-val { font-family: 'Orbitron', monospace; font-size: 1.5rem; font-weight: 700; color: var(--t-text1); }
.tm-stat-lbl { font-size: 0.7rem; color: var(--t-text3); margin-top: 0.2rem; }
.tm-ready { text-align: center; }
.tm-ready-text { font-size: 0.875rem; color: var(--t-text2); margin-bottom: 0.75rem; }
.tm-level-row { display: flex; gap: 0.4rem; justify-content: center; }
.tm-level-btn { padding: 0.35rem 0.85rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.78rem; cursor: pointer; }
.tm-level-active { background: #4caf50; color: white; border-color: #4caf50; }
.tm-text-display { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; padding: 1rem 1.25rem; font-size: 1rem; line-height: 1.8; letter-spacing: 0.02em; }
.tm-ch-correct { color: #4caf50; }
.tm-ch-wrong { color: #ef5350; text-decoration: underline; }
.tm-ch-current { background: rgba(76,175,80,0.3); border-radius: 2px; }
.tm-input { width: 100%; padding: 0.75rem; border: 1px solid var(--t-border); border-radius: 9px; background: var(--t-bg); color: var(--t-text1); font-size: 0.9rem; resize: none; }
.tm-finished { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.5rem; text-align: center; }
.tm-fin-title { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); margin-bottom: 1rem; }
.tm-fin-stats { display: flex; justify-content: center; gap: 2rem; margin-bottom: 1rem; }
.tm-fin-stat { display: flex; flex-direction: column; gap: 0.25rem; }
.tm-fin-stat span { font-size: 0.72rem; color: var(--t-text3); }
.tm-fin-stat strong { font-size: 1.25rem; font-weight: 800; color: var(--t-text1); }
.tm-retry-btn { padding: 0.6rem 1.5rem; border-radius: 9px; background: #4caf50; color: white; border: none; font-weight: 700; cursor: pointer; }
</style>
