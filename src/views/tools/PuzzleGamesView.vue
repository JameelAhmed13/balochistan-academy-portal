<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Puzzle Games</h2>
    <p class="pg-intro">Sharpen your mind with educational puzzles and brain teasers based on your curriculum.</p>
    <div v-if="!activeGame" class="pg-grid">
      <div v-for="game in games" :key="game.id" class="pg-card" @click="startGame(game)">
        <div class="pg-icon">{{ game.icon }}</div>
        <div class="pg-name">{{ game.name }}</div>
        <div class="pg-desc">{{ game.desc }}</div>
        <div class="pg-start">Play →</div>
      </div>
    </div>
    <!-- Word Scramble Game -->
    <div v-if="activeGame === 'scramble'" class="pg-game">
      <button @click="activeGame = null" class="pg-back-btn">← Back to Games</button>
      <div class="pg-game-title">Word Scramble</div>
      <div class="pg-scrambled">{{ scrambled }}</div>
      <input v-model="guess" type="text" placeholder="Unscramble the word..." class="pg-guess-input" @keyup.enter="checkGuess" />
      <div v-if="result" class="pg-result" :class="result === 'Correct!' ? 'pg-correct' : 'pg-wrong'">{{ result }}</div>
      <div class="pg-hint">Hint: {{ current.hint }}</div>
      <div class="pg-game-actions">
        <button @click="checkGuess" class="pg-check-btn">Check</button>
        <button @click="nextWord" class="pg-next-btn">Next Word →</button>
      </div>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const games = [
  { id: 'scramble', name: 'Word Scramble', icon: '🔤', desc: 'Unscramble science and subject terms' },
  { id: 'memory', name: 'Memory Match', icon: '🧠', desc: 'Match formula pairs from textbooks' },
  { id: 'crossword', name: 'Crossword', icon: '✏️', desc: 'Fill in curriculum-based crosswords' },
]

const words = [
  { word: 'PHOTOSYNTHESIS', hint: 'Process by which plants make food' },
  { word: 'MITOCHONDRIA', hint: 'Powerhouse of the cell' },
  { word: 'CHROMOSOME', hint: 'Carries genetic information' },
  { word: 'GRAVITATIONAL', hint: 'Type of force between masses' },
  { word: 'PERIODIC', hint: 'Type of table for elements' },
]

const activeGame = ref(null)
const wordIdx = ref(0)
const guess = ref('')
const result = ref('')

const current = computed(() => words[wordIdx.value])
const scrambled = computed(() => current.value.word.split('').sort(() => 0.5 - Math.sin(wordIdx.value * 7)).join(''))

function startGame(g) { activeGame.value = g.id; guess.value = ''; result.value = '' }
function checkGuess() {
  result.value = guess.value.toUpperCase() === current.value.word ? 'Correct!' : 'Try again!'
  if (result.value === 'Correct!') setTimeout(() => nextWord(), 1200)
}
function nextWord() { wordIdx.value = (wordIdx.value + 1) % words.length; guess.value = ''; result.value = '' }
</script>

<style scoped>
.pg-intro { font-size: 0.875rem; color: var(--t-text2); }
.pg-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 1rem; }
.pg-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem; cursor: pointer; display: flex; flex-direction: column; gap: 0.4rem; transition: transform 0.15s; }
.pg-card:hover { transform: translateY(-2px); border-color: #4caf50; }
.pg-icon { font-size: 2.5rem; }
.pg-name { font-size: 0.9rem; font-weight: 700; color: var(--t-text1); }
.pg-desc { font-size: 0.75rem; color: var(--t-text2); flex: 1; }
.pg-start { font-size: 0.78rem; font-weight: 700; color: #4caf50; }
.pg-game { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 14px; padding: 1.5rem; text-align: center; }
.pg-back-btn { display: inline-block; margin-bottom: 1rem; font-size: 0.78rem; font-weight: 600; color: var(--t-text3); border: none; background: none; cursor: pointer; }
.pg-game-title { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); margin-bottom: 1rem; }
.pg-scrambled { font-size: 2rem; font-weight: 900; letter-spacing: 0.3em; color: var(--t-accent); margin-bottom: 1rem; }
.pg-guess-input { width: 100%; max-width: 300px; padding: 0.65rem 1rem; border: 1px solid var(--t-border); border-radius: 9px; background: var(--t-bg); color: var(--t-text1); font-size: 1rem; text-transform: uppercase; text-align: center; margin-bottom: 0.75rem; }
.pg-result { padding: 0.5rem 1rem; border-radius: 7px; font-weight: 700; font-size: 0.9rem; display: inline-block; margin-bottom: 0.75rem; }
.pg-correct { background: #e8f5e9; color: #2e7d32; }
.pg-wrong { background: #ffebee; color: #c62828; }
.pg-hint { font-size: 0.78rem; color: var(--t-text3); margin-bottom: 1rem; font-style: italic; }
.pg-game-actions { display: flex; gap: 0.5rem; justify-content: center; }
.pg-check-btn { padding: 0.5rem 1.25rem; border-radius: 8px; background: #4caf50; color: white; border: none; font-weight: 700; cursor: pointer; font-size: 0.85rem; }
.pg-next-btn { padding: 0.5rem 1.25rem; border-radius: 8px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.85rem; cursor: pointer; }
</style>
