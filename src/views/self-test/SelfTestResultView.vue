<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Subjective Test Result</h2>

    <div v-for="(q, i) in questions" :key="q.id" class="result-card">
      <div class="rc-header">
        <div class="rc-qno">Q No. {{ i + 1 }}</div>
        <div class="rc-badges">
          <CognitiveBadge :level="q.cognitiveLevel" />
          <DifficultyBadge :level="q.difficulty" />
        </div>
        <div class="rc-cat">{{ q.category || q.type }}</div>
      </div>
      <div class="rc-stem">{{ q.stem }}</div>
    </div>

    <div class="flex gap-3 justify-center pt-2">
      <RouterLink :to="`/app/self-test`" class="btn-secondary">← Back to Books</RouterLink>
      <RouterLink :to="`/app/self-test/follow-up`" class="btn-primary">View Follow-Up Tests →</RouterLink>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { SUBJECTS, useContentStore } from '@/stores/content'
import { useStudentStore } from '@/stores/student'
import CognitiveBadge from '@/components/platform/CognitiveBadge.vue'
import DifficultyBadge from '@/components/platform/DifficultyBadge.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const route = useRoute()
const content = useContentStore()
const student = useStudentStore()

const bookId = computed(() => +route.params.bookId)
const questions = computed(() => {
  const last = student.testRecords.find(t => t.bookId === bookId.value && t.type?.includes('Self Test'))
  if (last?.questions) return last.questions
  return content.getSubjectiveQuestions(bookId.value, { limit: 10 })
})
</script>

<style scoped>
.result-card {
  background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px;
  padding: 1.25rem 1.5rem; space-y: 0.75rem;
}
.rc-header { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; margin-bottom: 0.75rem; }
.rc-qno { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); min-width: 60px; }
.rc-badges { display: flex; gap: 0.35rem; flex-wrap: wrap; flex: 1; }
.rc-cat { font-size: 0.82rem; font-weight: 600; color: var(--t-text2); }
.rc-stem { font-size: 0.875rem; color: var(--t-text2); line-height: 1.5; }
</style>
