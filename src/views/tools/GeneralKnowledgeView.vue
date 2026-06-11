<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">General Knowledge</h2>
    <div class="gk-filters">
      <button v-for="cat in categories" :key="cat.id" @click="activeCategory = cat.id"
        class="gk-cat-btn" :class="activeCategory === cat.id ? 'gk-cat-active' : ''">
        {{ cat.icon }} {{ cat.name }}
      </button>
    </div>
    <div class="gk-quiz-card" v-if="currentQ">
      <div class="gk-q-num">Question {{ currentIdx + 1 }} / {{ filtered.length }}</div>
      <div class="gk-q-text">{{ currentQ.question }}</div>
      <div class="gk-options">
        <button v-for="(opt, oi) in currentQ.options" :key="oi" @click="selectAnswer(oi)"
          :class="['gk-opt', answered && oi === currentQ.correct ? 'gk-correct' : '', answered && selected === oi && oi !== currentQ.correct ? 'gk-wrong' : '', answered && selected !== oi && oi !== currentQ.correct ? 'gk-faded' : '']">
          {{ String.fromCharCode(65 + oi) }}) {{ opt }}
        </button>
      </div>
      <div v-if="answered" class="gk-feedback" :class="selected === currentQ.correct ? 'gk-fb-correct' : 'gk-fb-wrong'">
        {{ selected === currentQ.correct ? '✅ Correct!' : '❌ Wrong. Correct answer: ' + currentQ.options[currentQ.correct] }}
      </div>
      <div class="gk-nav">
        <button @click="prev" :disabled="currentIdx === 0" class="gk-nav-btn">← Prev</button>
        <button @click="next" :disabled="currentIdx === filtered.length - 1" class="gk-nav-btn">Next →</button>
      </div>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const categories = [
  { id: 'all', name: 'All', icon: '🌍' },
  { id: 'pakistan', name: 'Pakistan', icon: '🇵🇰' },
  { id: 'science', name: 'Science', icon: '🔬' },
  { id: 'history', name: 'History', icon: '📜' },
]
const activeCategory = ref('all')
const currentIdx = ref(0)
const selected = ref(null)
const answered = ref(false)

const questions = [
  { q: 'What is the capital of Pakistan?', options: ['Lahore', 'Karachi', 'Islamabad', 'Peshawar'], correct: 2, cat: 'pakistan', question: 'What is the capital of Pakistan?' },
  { q: 'Who was the founder of Pakistan?', options: ['Allama Iqbal', 'Quaid-e-Azam Muhammad Ali Jinnah', 'Liaquat Ali Khan', 'Sir Syed Ahmed Khan'], correct: 1, cat: 'pakistan', question: 'Who was the founder of Pakistan?' },
  { q: 'What is the speed of light?', options: ['3×10⁶ m/s', '3×10⁸ m/s', '3×10¹⁰ m/s', '3×10⁴ m/s'], correct: 1, cat: 'science', question: 'What is the speed of light?' },
  { q: 'Which element has atomic number 6?', options: ['Nitrogen', 'Oxygen', 'Carbon', 'Silicon'], correct: 2, cat: 'science', question: 'Which element has atomic number 6?' },
  { q: 'In which year did Pakistan gain independence?', options: ['1945', '1946', '1947', '1948'], correct: 2, cat: 'pakistan', question: 'In which year did Pakistan gain independence?' },
  { q: 'Who wrote "Shikwa" and "Jawab-e-Shikwa"?', options: ['Mirza Ghalib', 'Faiz Ahmed Faiz', 'Allama Iqbal', 'Josh Malihabadi'], correct: 2, cat: 'pakistan', question: 'Who wrote "Shikwa" and "Jawab-e-Shikwa"?' },
  { q: 'What is the powerhouse of the cell?', options: ['Nucleus', 'Ribosome', 'Mitochondria', 'Golgi Apparatus'], correct: 2, cat: 'science', question: 'What is the powerhouse of the cell?' },
  { q: 'Who discovered penicillin?', options: ['Louis Pasteur', 'Alexander Fleming', 'Robert Koch', 'Edward Jenner'], correct: 1, cat: 'science', question: 'Who discovered penicillin?' },
]

const filtered = computed(() => activeCategory.value === 'all' ? questions : questions.filter(q => q.cat === activeCategory.value))
const currentQ = computed(() => filtered.value[currentIdx.value])

function selectAnswer(oi) {
  if (answered.value) return
  selected.value = oi
  answered.value = true
}
function next() { if (currentIdx.value < filtered.value.length - 1) { currentIdx.value++; selected.value = null; answered.value = false } }
function prev() { if (currentIdx.value > 0) { currentIdx.value--; selected.value = null; answered.value = false } }
</script>

<style scoped>
.gk-filters { display: flex; gap: 0.4rem; flex-wrap: wrap; }
.gk-cat-btn { padding: 0.35rem 0.85rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.78rem; cursor: pointer; }
.gk-cat-active { background: #4caf50; color: white; border-color: #4caf50; }
.gk-quiz-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 14px; padding: 1.5rem; }
.gk-q-num { font-size: 0.75rem; color: var(--t-text3); margin-bottom: 0.75rem; }
.gk-q-text { font-size: 1rem; font-weight: 700; color: var(--t-text1); margin-bottom: 1.25rem; line-height: 1.5; }
.gk-options { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 1rem; }
.gk-opt { padding: 0.75rem 1rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; text-align: left; cursor: pointer; transition: all 0.15s; }
.gk-opt:hover:not(.gk-correct):not(.gk-wrong) { background: var(--t-hover); }
.gk-correct { background: #e8f5e9 !important; border-color: #4caf50 !important; color: #2e7d32; font-weight: 700; }
.gk-wrong { background: #ffebee !important; border-color: #ef5350 !important; color: #c62828; }
.gk-faded { opacity: 0.4; }
html.dark .gk-correct { background: rgba(46,125,50,0.25) !important; border-color: #4caf50 !important; color: #a5d6a7; }
html.dark .gk-wrong { background: rgba(183,28,28,0.25) !important; border-color: #ef5350 !important; color: #ef9a9a; }
.gk-feedback { padding: 0.75rem 1rem; border-radius: 8px; font-size: 0.875rem; font-weight: 600; margin-bottom: 0.75rem; }
.gk-fb-correct { background: #e8f5e9; color: #2e7d32; }
.gk-fb-wrong { background: #ffebee; color: #c62828; }
html.dark .gk-fb-correct { background: rgba(46,125,50,0.2); color: #a5d6a7; }
html.dark .gk-fb-wrong { background: rgba(183,28,28,0.2); color: #ef9a9a; }
.gk-nav { display: flex; gap: 0.5rem; }
.gk-nav-btn { padding: 0.45rem 1rem; border-radius: 7px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.82rem; cursor: pointer; }
.gk-nav-btn:disabled { opacity: 0.3; cursor: not-allowed; }
</style>
