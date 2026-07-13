<template>
  <div class="oq-card" ref="cardEl">
    <!-- Meta badges -->
    <div class="oq-badges">
      <span class="oq-num">#{{ index + 1 }}</span>
      <span :class="diffBadge">{{ question.difficulty }}</span>
      <span class="badge-indigo">{{ question.cognitiveLevel }}</span>
      <span class="badge-purple">{{ question.type }}</span>
      <span v-if="question.entranceExam" class="oq-badge-entrance">Entrance</span>
      <span class="oq-slo">{{ question.slo }}</span>
    </div>

    <!-- Stem (may contain HTML + LaTeX) -->
    <div class="oq-stem" v-html="prepareMath(question.stem)" />

    <!-- Options -->
    <div class="oq-options">
      <div
        v-for="(opt, i) in question.options" :key="i"
        :class="['oq-opt', optState(i)]"
        @click="!showAnswer && !testMode ? null : handleSelect(i)"
      >
        <span class="oq-opt-letter" :class="letterState(i)">{{ 'ABCD'[i] }}</span>
        <span class="oq-opt-text" v-html="prepareMath(opt)" />
        <Check v-if="showAnswer && i === question.correct" class="oq-icon oq-ok" />
        <X v-if="testMode && submitted && selected === i && i !== question.correct" class="oq-icon oq-bad" />
      </div>
    </div>

    <!-- Explanation -->
    <Transition name="oq-fade">
      <div v-if="(showAnswer || (testMode && submitted)) && question.reason" class="oq-reason">
        <div class="oq-reason-label">Explanation</div>
        <div class="oq-reason-text" v-html="prepareMath(question.reason)" />
      </div>
    </Transition>

    <!-- Test mode submit -->
    <div v-if="testMode && !submitted" class="oq-submit">
      <button @click="submitAnswer" :disabled="selected === null" class="btn-primary text-xs">
        <CheckCircle class="w-3.5 h-3.5" /> Confirm Answer
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted } from 'vue'
import { Check, X, CheckCircle } from '@lucide/vue'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'

// Wrap bare LaTeX environments (\begin{align}...\end{align} etc.) in \[...\]
// so KaTeX auto-render can find and process them.
const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(html) {
  if (!html) return ''
  return String(html).replace(ENV_RE, (m) => `\\[${m}\\]`)
}

const KATEX_OPTS = {
  delimiters: [
    { left: '\\[', right: '\\]', display: true },
    { left: '\\(', right: '\\)', display: false },
    { left: '$$', right: '$$', display: true },
    { left: '$', right: '$', display: false },
  ],
  throwOnError: false,
}

const props = defineProps({
  question: Object,
  index: Number,
  showAnswer: { type: Boolean, default: false },
  testMode: { type: Boolean, default: false },
})
const emit = defineEmits(['answer'])

const selected = ref(null)
const submitted = ref(false)
const cardEl = ref(null)

function renderMath() {
  nextTick(() => {
    if (cardEl.value) renderMathInElement(cardEl.value, KATEX_OPTS)
  })
}

onMounted(renderMath)
watch(() => props.question, renderMath, { deep: true })
watch([() => props.showAnswer, submitted], renderMath)

function handleSelect(i) {
  if (!submitted.value) selected.value = i
}
function submitAnswer() {
  submitted.value = true
  emit('answer', { questionId: props.question.id, selected: selected.value, correct: props.question.correct })
}

// Option row state
function optState(i) {
  if (props.showAnswer && !props.testMode) {
    return i === props.question.correct ? 'oq-opt--correct' : 'oq-opt--idle'
  }
  if (props.testMode) {
    if (!submitted.value) return selected.value === i ? 'oq-opt--selected' : 'oq-opt--idle oq-opt--clickable'
    if (i === props.question.correct) return 'oq-opt--correct'
    if (selected.value === i) return 'oq-opt--wrong'
    return 'oq-opt--idle oq-opt--dim'
  }
  return 'oq-opt--idle'
}

function letterState(i) {
  if (props.showAnswer && !props.testMode) return i === props.question.correct ? 'oq-letter--ok' : 'oq-letter--idle'
  if (selected.value === i) return 'oq-letter--sel'
  return 'oq-letter--idle'
}

const diffBadge = computed(() => ({
  Easy: 'badge-green', Medium: 'badge-amber', Hard: 'badge-red',
}[props.question?.difficulty] || 'badge-indigo'))
</script>

<style scoped>
/* ── Card shell ────────────────────────────────────────────────── */
.oq-card {
  padding: 1.25rem;
  border-radius: 1rem;
  border: 1px solid var(--t-border);
  background: var(--t-surface);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  animation: oq-in 0.2s ease;
}
@keyframes oq-in { from { opacity: 0; transform: translateY(6px); } }

/* ── Badge row ─────────────────────────────────────────────────── */
.oq-badges { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
.oq-num    { font-size: 0.75rem; font-weight: 600; color: var(--t-text3); }
.oq-slo    { font-size: 0.72rem; color: var(--t-text3); margin-left: auto; }
.oq-badge-entrance {
  font-size: 0.68rem; font-weight: 700; padding: 0.18rem 0.55rem; border-radius: 99px;
  background: rgba(220, 38, 38, 0.12); color: #ef4444; border: 1px solid rgba(220,38,38,0.2);
}

/* ── Stem ──────────────────────────────────────────────────────── */
.oq-stem {
  font-size: 0.925rem;
  font-weight: 600;
  color: var(--t-text1);
  line-height: 1.7;
}
/* images inside stem */
.oq-stem :deep(img) { max-width: 100%; border-radius: 8px; margin: 0.5rem 0; display: block; }

/* ── Options ───────────────────────────────────────────────────── */
.oq-options { display: flex; flex-direction: column; gap: 0.5rem; }

.oq-opt {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border-radius: 0.75rem;
  border: 1px solid var(--t-border);
  background: var(--t-surface);
  font-size: 0.875rem;
  color: var(--t-text2);
  transition: border-color 0.15s, background 0.15s;
}
.oq-opt--clickable { cursor: pointer; }
.oq-opt--clickable:hover {
  border-color: var(--t-accent);
  background: var(--t-hover);
}
.oq-opt--idle   { /* default, already handled above */ }
.oq-opt--dim    { opacity: 0.5; }
.oq-opt--selected {
  border-color: var(--t-accent);
  background: var(--t-acc-alpha-sm);
  color: var(--t-text1);
}
.oq-opt--correct {
  border-color: #22c55e;
  background: rgba(34, 197, 94, 0.1);
  color: #15803d;
}
html.dark .oq-opt--correct { color: #4ade80; }
.oq-opt--wrong {
  border-color: #ef4444;
  background: rgba(239, 68, 68, 0.09);
  color: #b91c1c;
}
html.dark .oq-opt--wrong { color: #f87171; }

/* ── Option letter circle ──────────────────────────────────────── */
.oq-opt-letter {
  width: 1.5rem; height: 1.5rem; border-radius: 50%; border: 2px solid;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.7rem; font-weight: 700; flex-shrink: 0;
}
.oq-letter--idle { border-color: var(--t-border); color: var(--t-text3); }
.oq-letter--sel  { border-color: var(--t-accent); color: var(--t-accent); background: var(--t-acc-alpha-sm); }
.oq-letter--ok   { border-color: #22c55e; color: #15803d; background: rgba(34,197,94,0.12); }
html.dark .oq-letter--ok { color: #4ade80; }

.oq-opt-text { flex: 1; min-width: 0; }

/* ── Icons ─────────────────────────────────────────────────────── */
.oq-icon     { width: 1rem; height: 1rem; flex-shrink: 0; margin-left: auto; }
.oq-ok       { color: #22c55e; }
.oq-bad      { color: #ef4444; }

/* ── Explanation ───────────────────────────────────────────────── */
.oq-reason {
  padding: 0.75rem 1rem;
  border-radius: 0.75rem;
  background: var(--t-acc-alpha-sm);
  border: 1px solid var(--t-acc-alpha-md);
}
.oq-reason-label { font-size: 0.72rem; font-weight: 700; color: var(--t-accent); margin-bottom: 0.25rem; }
.oq-reason-text  { font-size: 0.85rem; color: var(--t-text1); line-height: 1.6; }
.oq-reason-text :deep(img) { max-width: 100%; border-radius: 6px; margin-top: 0.5rem; }

/* ── Submit ────────────────────────────────────────────────────── */
.oq-submit { padding-top: 0.25rem; }

/* ── Transition ────────────────────────────────────────────────── */
.oq-fade-enter-active, .oq-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.oq-fade-enter-from, .oq-fade-leave-to { opacity: 0; transform: translateY(-4px); }

/* ── KaTeX display math sizing ─────────────────────────────────── */
.oq-card :deep(.katex-display) { margin: 0.5rem 0; overflow-x: auto; }
.oq-card :deep(.katex) { font-size: 1em; color: var(--t-text1); }
</style>
