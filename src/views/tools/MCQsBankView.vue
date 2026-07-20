<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">MCQs Bank</h2>
    <div class="mcqb-filters">
      <select v-model="selectedSubject" class="mcqb-sel">
        <option :value="null">All Subjects</option>
        <option v-for="s in SUBJECTS" :key="s.id" :value="s.id">{{ s.icon }} {{ s.name }}</option>
      </select>
      <select v-model="filterDifficulty" class="mcqb-sel">
        <option value="">All Difficulty</option>
        <option>Easy</option><option>Medium</option><option>Hard</option>
      </select>
      <select v-model="filterCognitive" class="mcqb-sel">
        <option value="">All Cognitive</option>
        <option>Knowledge</option><option>Understanding</option><option>Applying</option>
      </select>
      <input v-model="search" type="text" placeholder="Search question..." class="mcqb-search" />
    </div>
    <div class="mcqb-count">Showing {{ filtered.length }} questions</div>
    <div class="mcqb-list" ref="listEl">
      <div v-for="(q, i) in filtered.slice(0, 50)" :key="q.id" class="mcqb-item">
        <div class="mcqb-item-header">
          <div class="mcqb-badges">
            <CognitiveBadge :level="q.cognitiveLevel" />
            <DifficultyBadge :level="q.difficulty" />
          </div>
          <div class="mcqb-num">#{{ i + 1 }}</div>
        </div>
        <div class="mcqb-stem" v-html="prepareMath(q.stem)" />
        <div class="mcqb-opts">
          <div v-for="(opt, oi) in q.options" :key="oi"
            :class="['mcqb-opt', oi === q.correct ? 'mcqb-correct' : '']">
            {{ String.fromCharCode(65 + oi) }}) <span v-html="prepareMath(opt)" />
          </div>
        </div>
        <div v-if="q.reason" class="mcqb-reason">📝 <span v-html="prepareMath(q.reason)" /></div>
      </div>
    </div>
    <div v-if="filtered.length > 50" class="mcqb-more">Showing first 50 of {{ filtered.length }} results</div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { SUBJECTS, useContentStore } from '@/stores/content'
import renderMathInElement from 'katex/contrib/auto-render'
import 'katex/dist/katex.min.css'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const content = useContentStore()
const listEl = ref(null)
const KATEX_OPTS = { delimiters: [{ left: '\\[', right: '\\]', display: true }, { left: '\\(', right: '\\)', display: false }, { left: '$$', right: '$$', display: true }, { left: '$', right: '$', display: false }], throwOnError: false }
const ENV_RE = /\\begin\{(align\*?|equation\*?|gather\*?|multline\*?|cases|matrix|pmatrix|bmatrix|vmatrix)\}[\s\S]*?\\end\{\1\}/g
function prepareMath(t) { if (!t) return ''; return String(t).replace(ENV_RE, (m) => `\\[${m}\\]`) }
function renderMath() { nextTick(() => { if (listEl.value) renderMathInElement(listEl.value, KATEX_OPTS) }) }

const selectedSubject = ref(null)
const filterDifficulty = ref('')
const filterCognitive = ref('')
const search = ref('')

const allQuestions = computed(() => {
  const ids = selectedSubject.value ? [selectedSubject.value] : SUBJECTS.map(s => s.id)
  return ids.flatMap(id => content.getQuestions(id, { limit: 30 }))
})

const filtered = computed(() => allQuestions.value.filter(q => {
  if (filterDifficulty.value && q.difficulty !== filterDifficulty.value) return false
  if (filterCognitive.value && q.cognitiveLevel !== filterCognitive.value) return false
  if (search.value && !q.stem.toLowerCase().includes(search.value.toLowerCase())) return false
  return true
}))
watch(filtered, renderMath)
</script>

<style scoped>
.mcqb-filters { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.mcqb-sel { padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-surface); color: var(--t-text1); font-size: 0.82rem; }
.mcqb-search { flex: 1; min-width: 200px; padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-surface); color: var(--t-text1); font-size: 0.82rem; }
.mcqb-count { font-size: 0.78rem; color: var(--t-text3); }
.mcqb-list { display: flex; flex-direction: column; gap: 0.75rem; }
.mcqb-item { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; padding: 1rem 1.25rem; }
.mcqb-item-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.5rem; }
.mcqb-badges { display: flex; gap: 0.4rem; flex-wrap: wrap; }
.mcqb-num { font-size: 0.72rem; color: var(--t-text3); }
.mcqb-stem { font-size: 0.875rem; color: var(--t-text1); line-height: 1.5; margin-bottom: 0.75rem; }
.mcqb-opts { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.3rem; }
.mcqb-opt { font-size: 0.78rem; color: var(--t-text2); padding: 0.3rem 0.5rem; border-radius: 5px; border: 1px solid var(--t-border); }
.mcqb-correct { background: #e8f5e9; color: #2e7d32; border-color: #a5d6a7; font-weight: 700; }
html.dark .mcqb-correct { background: rgba(46,125,50,0.2); color: #a5d6a7; border-color: rgba(46,125,50,0.4); }
.mcqb-reason { font-size: 0.75rem; color: var(--t-text3); margin-top: 0.5rem; padding-top: 0.5rem; border-top: 1px solid var(--t-border); }
.mcqb-more { text-align: center; font-size: 0.8rem; color: var(--t-text3); padding: 0.5rem; }
</style>
