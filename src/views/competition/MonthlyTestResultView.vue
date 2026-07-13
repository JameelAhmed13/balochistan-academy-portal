<template>
  <div class="animate-fade-in space-y-5" ref="resultEl">
    <ExamCompleted :score="scorePercent" label="Monthly Exam Complete!" :subtitle="`${correctCount} / ${totalCount} Correct`" />
    <div class="kua-grid">
      <div class="kua-card kua-blue"><div class="kua-val">{{ knowledgeScore }}</div><div class="kua-lbl">Knowledge: {{ knowledgePct }}</div></div>
      <div class="kua-card kua-green"><div class="kua-val">{{ understandingScore }}</div><div class="kua-lbl">Understanding: {{ understandingPct }}</div></div>
      <div class="kua-card kua-rose"><div class="kua-val">{{ applyingScore }}</div><div class="kua-lbl">Applying: {{ applyingPct }}</div></div>
      <div class="kua-card kua-amber"><div class="kua-val">{{ coinsEarned }}</div><div class="kua-lbl">Coins Earned</div></div>
    </div>
    <div v-if="!questions.length" class="qt-empty">
      Question-level breakdown is not available for this attempt. Score recorded: {{ correctCount }}/{{ totalCount }}.
    </div>
    <div v-else class="qt-wrap">
      <table class="qt-table">
        <thead><tr class="qt-thead">
          <th>Sr No</th><th>Question</th><th>Your Answer</th><th>Correct Answer</th><th>Reason</th>
        </tr></thead>
        <tbody>
          <tr v-for="(q, i) in questions" :key="q.id">
            <td>{{ i + 1 }}</td>
            <td class="qt-q-cell">
              <div class="qt-badges"><CognitiveBadge :level="q.cognitiveLevel" /><DifficultyBadge :level="q.difficulty" /></div>
              <div class="qt-stem" v-html="prepareMath(q.stem)" />
            </td>
            <td>
              <div class="qt-answer">
                <span v-html="prepareMath(q.userAnswer !== undefined ? q.options[q.userAnswer] : 'Skipped')" />
                <span v-if="q.userAnswer !== undefined && q.userAnswer !== q.correct" class="qt-wrong">Incorrect</span>
                <span v-else-if="q.userAnswer === undefined" class="qt-skipped">Skipped</span>
                <span v-else class="qt-correct-pill">Correct</span>
              </div>
            </td>
            <td v-html="prepareMath(q.options[q.correct])" />
            <td class="qt-reason" v-html="prepareMath(q.reason)" />
          </tr>
        </tbody>
      </table>
    </div>
    <div class="mtr-actions">
      <RouterLink to="/app/competition" class="mtr-btn-back">← Back to Hub</RouterLink>
      <RouterLink to="/app/competition/monthly-test" class="mtr-btn-retry">Retry Monthly Exam</RouterLink>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { useStudentStore } from '@/stores/student'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'
import ExamCompleted from '@/components/platform/ExamCompleted.vue'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const route = useRoute()
const student = useStudentStore()

const resultEl = ref(null)
const KATEX_OPTS = { delimiters: [{ left: '\\[', right: '\\]', display: true }, { left: '\\(', right: '\\)', display: false }, { left: '$$', right: '$$', display: true }, { left: '$', right: '$', display: false }], throwOnError: false }
const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(t) { if (!t) return ''; return String(t).replace(ENV_RE, (m) => `\\[${m}\\]`) }
onMounted(() => nextTick(() => { if (resultEl.value) renderMathInElement(resultEl.value, KATEX_OPTS) }))

const record = computed(() => route.params.id === 'new' ? null : student.testRecords.find(r => String(r.id) === String(route.params.id)))
const questions = computed(() => {
  const qs = record.value?.questions || []
  return qs.map((q, i) => ({ ...q, userAnswer: record.value?.answers?.[i] }))
})
const totalCount = computed(() => record.value?.total || questions.value.length || 0)
const correctCount = computed(() => {
  if (questions.value.length) return questions.value.filter(q => q.userAnswer === q.correct).length
  return record.value?.score || 0
})
const scorePercent = computed(() => {
  if (!totalCount.value) return 0
  if (record.value?.score !== undefined && record.value?.total) return Math.round(record.value.score / record.value.total * 100)
  return Math.round(correctCount.value / totalCount.value * 100)
})
const knowledgeQs = computed(() => questions.value.filter(q => q.cognitiveLevel === 'Knowledge'))
const understandingQs = computed(() => questions.value.filter(q => q.cognitiveLevel === 'Understanding'))
const applyingQs = computed(() => questions.value.filter(q => q.cognitiveLevel === 'Applying'))
function cs(qs) { return qs.filter(q => q.userAnswer === q.correct).length + '/' + qs.length }
function cp(qs) { if (!qs.length) return '-'; return Math.round(qs.filter(q => q.userAnswer === q.correct).length / qs.length * 100) + '%' }
const knowledgeScore = computed(() => cs(knowledgeQs.value))
const knowledgePct = computed(() => cp(knowledgeQs.value))
const understandingScore = computed(() => cs(understandingQs.value))
const understandingPct = computed(() => cp(understandingQs.value))
const applyingScore = computed(() => cs(applyingQs.value))
const applyingPct = computed(() => cp(applyingQs.value))
const coinsEarned = computed(() => record.value?.coins ?? 0)
</script>

<style scoped>
.kua-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; }
@media (max-width: 768px) { .kua-grid { grid-template-columns: repeat(2, 1fr); } }
.kua-card { padding: 1rem; border-radius: 12px; text-align: center; }
.kua-blue  { background: #e3f2fd; border: 1px solid #90caf9; }
.kua-green { background: #e8f5e9; border: 1px solid #a5d6a7; }
.kua-rose  { background: #ffebee; border: 1px solid #ef9a9a; }
.kua-amber { background: #fff8e1; border: 1px solid #ffe082; }
html.dark .kua-blue  { background: rgba(21,101,192,0.2); border-color: rgba(21,101,192,0.4); }
html.dark .kua-green { background: rgba(46,125,50,0.2); border-color: rgba(46,125,50,0.4); }
html.dark .kua-rose  { background: rgba(183,28,28,0.2); border-color: rgba(183,28,28,0.4); }
html.dark .kua-amber { background: rgba(245,127,23,0.2); border-color: rgba(245,127,23,0.4); }
.kua-val { font-size: 1.4rem; font-weight: 800; color: var(--t-text1); }
.kua-lbl { font-size: 0.7rem; color: var(--t-text2); margin-top: 0.2rem; }
.qt-empty { padding: 1.25rem 1.5rem; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; font-size: 0.85rem; color: var(--t-text3); }
.qt-wrap { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.qt-table { width: 100%; border-collapse: collapse; font-size: 0.82rem; }
.qt-thead th { background: #15803d; color: white; padding: 0.75rem 1rem; text-align: left; font-weight: 600; }
.qt-table td { padding: 0.85rem 1rem; color: var(--t-text1); border-bottom: 1px solid var(--t-border); vertical-align: top; }
.qt-badges { display: flex; gap: 0.3rem; flex-wrap: wrap; margin-bottom: 0.4rem; }
.qt-stem { line-height: 1.4; }
.qt-answer { display: flex; flex-direction: column; gap: 0.25rem; }
.qt-wrong { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 4px; background: #ffcdd2; color: #c62828; font-size: 0.7rem; font-weight: 700; width: fit-content; }
.qt-skipped { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 4px; background: #fff9c4; color: #f57f17; font-size: 0.7rem; font-weight: 700; width: fit-content; }
.qt-correct-pill { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 4px; background: #e8f5e9; color: #388e3c; font-size: 0.7rem; font-weight: 700; width: fit-content; }
.qt-reason { color: var(--t-text2); font-size: 0.8rem; line-height: 1.4; }
.mtr-actions { display: flex; gap: 0.75rem; }
.mtr-btn-back { padding: 0.6rem 1.25rem; border-radius: 8px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); font-size: 0.85rem; font-weight: 600; text-decoration: none; }
.mtr-btn-retry { padding: 0.6rem 1.5rem; border-radius: 8px; background: #9c27b0; color: white; font-size: 0.85rem; font-weight: 700; text-decoration: none; }
</style>
