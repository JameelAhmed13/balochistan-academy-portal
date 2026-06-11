<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Objective Test Result</h2>

    <!-- Score header -->
    <div class="otr-header">
      <div class="otr-header-left">
        <div class="otr-quizlbl">Quiz Result</div>
        <div class="otr-meta">
          <span class="otr-book-tag">📚 {{ record?.subject || 'Mathematics Eng Medium' }} — Class {{ record?.grade || 9 }}</span>
          <span class="otr-correct">Correct: {{ record?.score || 0 }}/{{ record?.total || 136 }}</span>
        </div>
      </div>
    </div>

    <!-- KUA Score cards -->
    <div class="kua-grid">
      <div class="kua-card kua-blue">
        <div class="kua-val">{{ knowledgeScore }}</div>
        <div class="kua-lbl">Knowledge Score: {{ knowledgePercent }}</div>
      </div>
      <div class="kua-card kua-green">
        <div class="kua-val">{{ understandingScore }}</div>
        <div class="kua-lbl">Understanding Score: {{ understandingPercent }}</div>
      </div>
      <div class="kua-card kua-rose">
        <div class="kua-val">{{ applyingScore }}</div>
        <div class="kua-lbl">Applying Score: {{ applyingPercent }}</div>
      </div>
      <div class="kua-card kua-amber">
        <div class="kua-val">{{ coinsEarned }}</div>
        <div class="kua-lbl">Coins Earned</div>
      </div>
    </div>

    <!-- Follow-up banner -->
    <div class="followup-banner">
      <span>A follow-up test is created for your weak areas in this test.</span>
      <RouterLink to="/app/self-test/follow-up" class="fu-btn">VIEW DETAILS</RouterLink>
    </div>

    <!-- Per-question table -->
    <div class="qt-wrap">
      <table class="qt-table">
        <thead>
          <tr class="qt-thead">
            <th>Sr No</th>
            <th>Question</th>
            <th>Your Answer</th>
            <th>Correct Answer</th>
            <th>Reason</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(q, i) in questions" :key="q.id">
            <td>{{ i + 1 }}</td>
            <td class="qt-q-cell">
              <div class="qt-badges">
                <CognitiveBadge :level="q.cognitiveLevel" />
                <DifficultyBadge :level="q.difficulty" />
              </div>
              <div class="qt-stem" v-html="renderMath(q.stem)" />
            </td>
            <td>
              <div class="qt-answer">
                <span>{{ q.userAnswer !== undefined ? q.options[q.userAnswer] : 'Skipped' }}</span>
                <span v-if="q.userAnswer !== undefined" class="qt-wrong">Incorrect</span>
                <span v-else class="qt-skipped">Skipped</span>
              </div>
            </td>
            <td>{{ q.options[q.correct] }}</td>
            <td class="qt-reason" v-html="renderMath(q.reason)" />
          </tr>
        </tbody>
      </table>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useStudentStore } from '@/stores/student'
import { useContentStore } from '@/stores/content'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const route = useRoute()
const student = useStudentStore()
const content = useContentStore()

const recordId = computed(() => route.params.id)
const record = computed(() => student.testRecords.find(r => String(r.id) === String(recordId.value)))

const questions = computed(() => {
  const qs = record.value?.questions || content.getQuestions(7, { limit: 15 })
  return qs.map((q, i) => ({ ...q, userAnswer: record.value?.answers?.[i] }))
})

const knowledgeQs = computed(() => questions.value.filter(q => q.cognitiveLevel === 'Knowledge'))
const understandingQs = computed(() => questions.value.filter(q => q.cognitiveLevel === 'Understanding'))
const applyingQs = computed(() => questions.value.filter(q => q.cognitiveLevel === 'Applying'))

function pctOf(qs) {
  if (!qs.length) return '-'
  const correct = qs.filter(q => q.userAnswer === q.correct).length
  return Math.round(correct / qs.length * 100) + '%'
}

const knowledgeScore = computed(() => knowledgeQs.value.filter(q => q.userAnswer === q.correct).length + '/' + knowledgeQs.value.length)
const knowledgePercent = computed(() => pctOf(knowledgeQs.value))
const understandingScore = computed(() => understandingQs.value.filter(q => q.userAnswer === q.correct).length + '/' + understandingQs.value.length)
const understandingPercent = computed(() => pctOf(understandingQs.value))
const applyingScore = computed(() => applyingQs.value.filter(q => q.userAnswer === q.correct).length + '/' + applyingQs.value.length)
const applyingPercent = computed(() => pctOf(applyingQs.value))
const coinsEarned = computed(() => record.value?.coins || 0)

function renderMath(text) {
  if (!text) return ''
  // Basic math formatting - bold superscripts, etc.
  return text.replace(/\b([A-Z])\^([A-Z0-9]+)/g, '<b>$1<sup>$2</sup></b>')
             .replace(/\b(matrix|transpose|adjoint|determinant)\b/gi, '<strong>$1</strong>')
}
</script>

<style scoped>
.otr-header { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem 1.5rem; }
.otr-quizlbl { font-size: 0.78rem; color: var(--t-text3); margin-bottom: 0.35rem; }
.otr-meta { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.otr-book-tag { display: inline-flex; align-items: center; gap: 0.3rem; padding: 0.3rem 0.75rem; border-radius: 99px; font-size: 0.78rem; font-weight: 600; background: #fff3e0; color: #e65100; border: 1px solid #ffcc80; }
html.dark .otr-book-tag { background: rgba(230,81,0,0.2); color: #ffb74d; border-color: rgba(230,81,0,0.4); }
.otr-correct { font-size: 0.82rem; color: var(--t-text3); }

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

.followup-banner {
  display: flex; align-items: center; justify-content: space-between; gap: 1rem; flex-wrap: wrap;
  background: linear-gradient(135deg, #1565c0, #283593); color: white;
  padding: 1rem 1.5rem; border-radius: 12px;
}
.followup-banner span { font-size: 0.875rem; }
.fu-btn {
  display: inline-block; padding: 0.45rem 1.25rem; border-radius: 8px; font-weight: 700; font-size: 0.82rem;
  background: #f57c00; color: white; text-decoration: none; white-space: nowrap;
}

.qt-wrap { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.qt-table { width: 100%; border-collapse: collapse; font-size: 0.82rem; }
.qt-thead th { background: #4caf50; color: white; padding: 0.75rem 1rem; text-align: left; font-weight: 600; }
.qt-table td { padding: 0.85rem 1rem; color: var(--t-text1); border-bottom: 1px solid var(--t-border); vertical-align: top; }
.qt-badges { display: flex; gap: 0.3rem; flex-wrap: wrap; margin-bottom: 0.4rem; }
.qt-stem { color: var(--t-text1); line-height: 1.4; }
.qt-answer { display: flex; flex-direction: column; gap: 0.25rem; }
.qt-wrong { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 4px; background: #ffcdd2; color: #c62828; font-size: 0.7rem; font-weight: 700; width: fit-content; }
.qt-skipped { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 4px; background: #fff9c4; color: #f57f17; font-size: 0.7rem; font-weight: 700; width: fit-content; }
.qt-reason { color: var(--t-text2); font-size: 0.8rem; line-height: 1.4; }
</style>
