<template>
  <div class="animate-fade-in str-wrap">
    <!-- Score summary -->
    <div class="card str-score">
      <div class="str-ring" :style="{ '--pct': pct }">
        <div class="str-ring-inner">
          <div class="str-ring-pct">{{ pct }}%</div>
          <div class="str-ring-sub">{{ score }}/{{ total }}</div>
        </div>
      </div>
      <div class="str-score-info">
        <div class="str-verdict" :class="verdict.cls">{{ verdict.label }}</div>
        <h2 class="str-title">{{ subjectName }} — Self Test Result</h2>
        <p class="str-msg">{{ verdict.msg }}</p>
        <div v-if="coinsEarned > 0" class="str-coins">
          <span class="str-coins-icon">🪙</span>
          <span class="str-coins-val">+{{ coinsEarned }} coins earned!</span>
        </div>
        <div class="str-actions">
          <RouterLink :to="`/app/self-test/${bookId}/config`" class="btn-secondary text-sm"><RotateCcw class="w-4 h-4" /> Retake</RouterLink>
          <RouterLink to="/app/report" class="btn-primary text-sm"><FileBarChart class="w-4 h-4" /> View Full Report</RouterLink>
        </div>
      </div>
    </div>

    <!-- Per-question review -->
    <div class="str-review-title">Answer Review <span class="str-review-hint">{{ correctCount }} correct · {{ questions.length - correctCount }} incorrect</span></div>
    <div ref="reviewEl">
    <div v-for="(q, i) in questions" :key="q.id || i" class="card str-q">
      <div class="str-q-head">
        <span class="str-q-no">Q{{ i + 1 }}</span>
        <span :class="answers[i] === q.correct ? 'badge-green' : (answers[i] === undefined ? 'badge-amber' : 'badge-red')">
          {{ answers[i] === q.correct ? 'Correct' : (answers[i] === undefined ? 'Skipped' : 'Incorrect') }}
        </span>
        <span v-if="q.topic" class="str-q-topic">{{ q.topic }}</span>
      </div>
      <p class="str-q-stem" :class="medium === 'urdu' ? 'urdu' : ''" :dir="medium === 'urdu' ? 'rtl' : 'ltr'" v-html="prepareMath(q.stem)" />
      <div class="str-opts" :dir="medium === 'urdu' ? 'rtl' : 'ltr'">
        <div v-for="(opt, oi) in q.options" :key="oi" class="str-opt"
          :class="{ 'str-opt-correct': oi === q.correct, 'str-opt-wrong': answers[i] === oi && oi !== q.correct }">
          <span class="str-opt-key">{{ ['A','B','C','D','E'][oi] }}</span>
          <span class="str-opt-text" v-html="prepareMath(opt)" />
          <Check v-if="oi === q.correct" class="w-4 h-4 str-ic-ok" />
          <X v-else-if="answers[i] === oi" class="w-4 h-4 str-ic-no" />
        </div>
      </div>
      <div v-if="q.reason" class="str-reason"><strong>Why:</strong> <span v-html="prepareMath(q.reason)" /></div>
    </div>
    </div>

    <div class="flex gap-3 justify-center pt-1">
      <RouterLink to="/app/self-test" class="btn-secondary">← Back to Self Test</RouterLink>
      <RouterLink to="/app/report" class="btn-primary"><FileBarChart class="w-4 h-4" /> Full Preparation Report</RouterLink>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { computed, ref, nextTick, onMounted, watch } from 'vue'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'
import { useRoute } from 'vue-router'
import { Check, X, RotateCcw, FileBarChart } from '@lucide/vue'
import { useStudentStore } from '@/stores/student'
import { useCatalogStore } from '@/stores/catalog'
import PageFooter from '@/components/platform/PageFooter.vue'

const route = useRoute()
const student = useStudentStore()
const catalog = useCatalogStore()

const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(html) {
  if (!html) return ''
  const s = String(html)
  return s.replace(ENV_RE, (match, _env, offset) => {
    const before = s.slice(0, offset)
    const opens = (before.match(/\\\[/g) || []).length
    const closes = (before.match(/\\\]/g) || []).length
    return opens > closes ? match : `\\[${match}\\]`
  })
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

const reviewEl = ref(null)
function renderMath() {
  nextTick(() => { if (reviewEl.value) renderMathInElement(reviewEl.value, KATEX_OPTS) })
}

const bookId = computed(() => +route.params.bookId)

const subject = computed(() => {
  const all = Object.values(catalog.gradeSubjects).flat()
  return all.find(s => String(s.id) === String(bookId.value)) || null
})
const subjectName = computed(() => subject.value?.name || 'Subject')
const medium = computed(() => subject.value?.medium || 'english')

const record = computed(() =>
  student.testRecords.find(t => t.bookId === bookId.value && (t.type || '').includes('Self Test')) || null)

const questions = computed(() => record.value?.questions || [])
const answers = computed(() => record.value?.answers || {})
const total = computed(() => record.value?.total ?? +route.query.total ?? questions.value.length)
const correctCount = computed(() =>
  questions.value.filter((q, i) => answers.value[i] === q.correct).length)
const score = computed(() => record.value?.score ?? +route.query.score ?? correctCount.value)
const pct = computed(() => total.value ? Math.round((score.value / total.value) * 100) : 0)

const coinsEarned = computed(() => record.value?.coins ?? 0)

const verdict = computed(() => {
  const p = pct.value
  if (p >= 85) return { label: 'Excellent', cls: 'str-v-good', msg: 'Outstanding — you have strong command of this topic.' }
  if (p >= 60) return { label: 'Passed', cls: 'str-v-ok', msg: 'Good work. Review the ones you missed to push higher.' }
  if (p >= 40) return { label: 'Needs Work', cls: 'str-v-warn', msg: 'Keep going — revise the weak areas and retake.' }
  return { label: 'Keep Practising', cls: 'str-v-bad', msg: "Revisit the lessons, then try again — you will improve fast." }
})

watch(questions, renderMath)
onMounted(renderMath)
</script>

<style scoped>
.str-wrap { max-width: 860px; margin: 0 auto; display: flex; flex-direction: column; gap: 1rem; }
.str-score { display: flex; align-items: center; gap: 1.5rem; padding: 1.5rem; flex-wrap: wrap; }
.str-ring {
  width: 110px; height: 110px; border-radius: 50%; flex-shrink: 0;
  background: conic-gradient(var(--t-accent) calc(var(--pct) * 1%), var(--t-hover2) 0);
  display: flex; align-items: center; justify-content: center;
}
.str-ring-inner { width: 84px; height: 84px; border-radius: 50%; background: var(--t-surface); display: flex; flex-direction: column; align-items: center; justify-content: center; }
.str-ring-pct { font-size: 1.5rem; font-weight: 800; color: var(--t-accent); font-family: 'Syne', sans-serif; }
.str-ring-sub { font-size: 0.72rem; color: var(--t-text3); }
.str-score-info { flex: 1; min-width: 220px; }
.str-verdict { display: inline-block; font-size: 0.68rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.05em; padding: 0.2rem 0.6rem; border-radius: 99px; margin-bottom: 0.3rem; }
.str-v-good { background: var(--t-success-bg); color: var(--t-success); }
.str-v-ok { background: var(--t-success-bg); color: var(--t-success); }
.str-v-warn { background: color-mix(in srgb, var(--t-gold) 14%, transparent); color: var(--t-warn); }
.str-v-bad { background: var(--t-danger-bg); color: var(--t-danger); }
.str-title { font-size: 1.15rem; font-weight: 800; color: var(--t-text1); font-family: 'Syne', sans-serif; }
.str-msg { font-size: 0.84rem; color: var(--t-text3); margin: 0.25rem 0 0.75rem; }
.str-coins { display: inline-flex; align-items: center; gap: 0.4rem; background: color-mix(in srgb, var(--t-gold) 15%, transparent); border: 1px solid color-mix(in srgb, var(--t-gold) 40%, transparent); padding: 0.3rem 0.75rem; border-radius: 99px; margin-bottom: 0.65rem; }
.str-coins-icon { font-size: 1rem; line-height: 1; }
.str-coins-val { font-size: 0.8rem; font-weight: 700; color: var(--t-warn); }
.str-actions { display: flex; gap: 0.5rem; flex-wrap: wrap; }

.str-review-title { display: flex; align-items: center; gap: 0.5rem; font-size: 0.95rem; font-weight: 700; color: var(--t-text1); }
.str-review-hint { font-size: 0.72rem; font-weight: 500; color: var(--t-text3); }
.str-q { padding: 1.1rem 1.25rem; }
.str-q-head { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 0.5rem; }
.str-q-no { font-size: 0.82rem; font-weight: 700; color: var(--t-text2); }
.str-q-topic { font-size: 0.7rem; color: var(--t-text3); margin-left: auto; }
.str-q-stem { font-size: 0.92rem; font-weight: 500; color: var(--t-text1); line-height: 1.6; margin-bottom: 0.75rem; }
.str-opts { display: flex; flex-direction: column; gap: 0.4rem; }
.str-opt { display: flex; align-items: center; gap: 0.6rem; padding: 0.55rem 0.8rem; border-radius: 9px; border: 1px solid var(--t-border); font-size: 0.85rem; color: var(--t-text1); }
.str-opt-correct { background: var(--t-success-bg); border-color: color-mix(in srgb, var(--t-success) 35%, transparent); }
.str-opt-wrong { background: var(--t-danger-bg); border-color: color-mix(in srgb, var(--t-danger) 35%, transparent); }
.str-opt-key { width: 22px; height: 22px; border-radius: 6px; background: var(--t-hover2); display: flex; align-items: center; justify-content: center; font-size: 0.72rem; font-weight: 700; flex-shrink: 0; }
.str-opt-text { flex: 1; }
.str-ic-ok { color: var(--t-success); flex-shrink: 0; }
.str-ic-no { color: var(--t-danger); flex-shrink: 0; }
.str-reason { margin-top: 0.7rem; font-size: 0.8rem; color: var(--t-text2); background: var(--t-hover); border-radius: 9px; padding: 0.6rem 0.8rem; line-height: 1.5; }
</style>
