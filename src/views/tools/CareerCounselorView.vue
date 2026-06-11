<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Career Counselor</h2>
    <div v-if="!showResult" class="cc-quiz">
      <div class="cc-q-card">
        <div class="cc-q-num">Question {{ step + 1 }} / {{ questions.length }}</div>
        <div class="cc-q-text">{{ questions[step].text }}</div>
        <div class="cc-options">
          <button v-for="opt in questions[step].options" :key="opt.value"
            @click="answer(opt.value)" class="cc-opt">
            {{ opt.label }}
          </button>
        </div>
      </div>
    </div>
    <div v-else class="cc-result">
      <div class="cc-result-title">🎓 Your Career Recommendations</div>
      <div class="cc-careers">
        <div v-for="career in recommendations" :key="career.name" class="cc-career-card">
          <div class="cc-career-icon">{{ career.icon }}</div>
          <div class="cc-career-info">
            <div class="cc-career-name">{{ career.name }}</div>
            <div class="cc-career-field">{{ career.field }}</div>
            <div class="cc-career-desc">{{ career.desc }}</div>
          </div>
          <div class="cc-career-match">{{ career.match }}% match</div>
        </div>
      </div>
      <button @click="restart" class="cc-restart-btn">Take Again</button>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const step = ref(0)
const answers = ref([])
const showResult = ref(false)

const questions = [
  { text: 'Which subject do you enjoy the most?', options: [{ label: 'Mathematics & Physics', value: 'stem' }, { label: 'Biology & Chemistry', value: 'bio' }, { label: 'Literature & Urdu', value: 'arts' }, { label: 'Computer Science', value: 'cs' }] },
  { text: 'How do you prefer to solve problems?', options: [{ label: 'Analytically with formulas', value: 'analytical' }, { label: 'Creatively with ideas', value: 'creative' }, { label: 'Practically hands-on', value: 'practical' }, { label: 'Researching and reading', value: 'research' }] },
  { text: 'What kind of work environment appeals to you?', options: [{ label: 'Lab or research setting', value: 'lab' }, { label: 'Office or corporate', value: 'office' }, { label: 'Hospital or clinic', value: 'medical' }, { label: 'Outdoors or field work', value: 'field' }] },
]

const allCareers = [
  { name: 'Software Engineer', field: 'Technology', icon: '💻', desc: 'Build apps, systems, and platforms', match: 95, for: ['cs', 'stem'] },
  { name: 'Medical Doctor', field: 'Healthcare', icon: '🩺', desc: 'Diagnose and treat patients', match: 92, for: ['bio', 'medical'] },
  { name: 'Data Scientist', field: 'Technology/AI', icon: '📊', desc: 'Analyze data and build AI models', match: 90, for: ['cs', 'analytical'] },
  { name: 'Civil Engineer', field: 'Engineering', icon: '🏗️', desc: 'Design infrastructure and buildings', match: 88, for: ['stem', 'practical'] },
  { name: 'Lawyer', field: 'Law', icon: '⚖️', desc: 'Advocate and interpret the law', match: 85, for: ['arts', 'research'] },
  { name: 'Journalist', field: 'Media', icon: '📰', desc: 'Report news and tell stories', match: 82, for: ['arts', 'creative'] },
]

const recommendations = ref([])

function answer(value) {
  answers.value.push(value)
  if (step.value < questions.length - 1) step.value++
  else {
    const matched = allCareers.filter(c => c.for.some(f => answers.value.includes(f))).slice(0, 3)
    recommendations.value = matched.length ? matched : allCareers.slice(0, 3)
    showResult.value = true
  }
}
function restart() { step.value = 0; answers.value = []; showResult.value = false }
</script>

<style scoped>
.cc-quiz { max-width: 560px; margin: 0 auto; }
.cc-q-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 14px; padding: 1.5rem; }
.cc-q-num { font-size: 0.75rem; color: var(--t-text3); margin-bottom: 0.75rem; }
.cc-q-text { font-size: 1rem; font-weight: 700; color: var(--t-text1); margin-bottom: 1.25rem; line-height: 1.5; }
.cc-options { display: flex; flex-direction: column; gap: 0.5rem; }
.cc-opt { padding: 0.75rem 1rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; text-align: left; cursor: pointer; transition: all 0.15s; }
.cc-opt:hover { background: var(--t-hover); border-color: #4caf50; }
.cc-result-title { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); margin-bottom: 1rem; }
.cc-careers { display: flex; flex-direction: column; gap: 0.75rem; }
.cc-career-card { display: flex; align-items: center; gap: 1rem; padding: 1rem 1.25rem; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; }
.cc-career-icon { font-size: 2rem; flex-shrink: 0; }
.cc-career-info { flex: 1; }
.cc-career-name { font-size: 0.9rem; font-weight: 700; color: var(--t-text1); }
.cc-career-field { font-size: 0.72rem; color: var(--t-text3); }
.cc-career-desc { font-size: 0.78rem; color: var(--t-text2); margin-top: 0.2rem; }
.cc-career-match { font-size: 1rem; font-weight: 800; color: #4caf50; flex-shrink: 0; }
.cc-restart-btn { padding: 0.65rem 1.5rem; border-radius: 9px; background: #f57c00; color: white; border: none; font-weight: 700; cursor: pointer; font-size: 0.875rem; margin-top: 0.5rem; }
</style>
