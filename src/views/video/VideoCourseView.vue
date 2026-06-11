<template>
  <div class="animate-fade-in space-y-5" v-if="course">
    <!-- Course header -->
    <div class="vcv-header" :style="{ borderLeftColor: course.color || '#4caf50' }">
      <div class="vcv-header-icon">{{ course.icon }}</div>
      <div>
        <div class="vcv-header-title">{{ course.title }}</div>
        <div class="vcv-header-meta">{{ course.totalChapters }} Chapters · {{ course.totalLessons }} Lessons · {{ course.gradeLabel }}</div>
        <div class="vcv-header-desc">{{ course.description }}</div>
      </div>
    </div>

    <!-- Chapter accordion -->
    <div class="vcv-chapters">
      <div v-for="ch in course.chapters" :key="ch.number" class="vcv-chapter">
        <div class="vcv-ch-header" @click="toggleChapter(ch.number)">
          <div class="vcv-ch-title">Chapter {{ ch.number }}: {{ ch.title }}</div>
          <div class="vcv-ch-meta">{{ ch.lessons.length }} lessons</div>
          <div class="vcv-ch-arrow" :class="{ 'vcv-open': openChapters.has(ch.number) }">▾</div>
        </div>
        <div v-if="openChapters.has(ch.number)" class="vcv-lesson-list">
          <RouterLink
            v-for="(lesson, li) in ch.lessons"
            :key="lesson.id"
            :to="`/app/video/player/${course.id}/${lesson.id}`"
            class="vcv-lesson"
          >
            <div class="vcv-lesson-num">{{ li + 1 }}</div>
            <div class="vcv-lesson-info">
              <div class="vcv-lesson-title">{{ lesson.title }}</div>
              <div class="vcv-lesson-meta">{{ lesson.duration || 'Video' }}</div>
            </div>
            <div class="vcv-play-btn">▶</div>
          </RouterLink>
        </div>
      </div>
    </div>

    <PageFooter />
  </div>
  <div v-else class="animate-fade-in p-8 text-center text-t-text3">Course not found.</div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import courses from '@/assets/data/courses.json'
import PageFooter from '@/components/platform/PageFooter.vue'

const route = useRoute()
const course = computed(() => courses.find(c => c.id === route.params.courseId))
const openChapters = ref(new Set([1]))

function toggleChapter(n) {
  if (openChapters.value.has(n)) openChapters.value.delete(n)
  else openChapters.value.add(n)
  openChapters.value = new Set(openChapters.value)
}
</script>

<style scoped>
.vcv-header { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem 1.5rem; display: flex; gap: 1.25rem; align-items: flex-start; border-left: 4px solid #4caf50; }
.vcv-header-icon { font-size: 2.5rem; flex-shrink: 0; }
.vcv-header-title { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); }
.vcv-header-meta { font-size: 0.78rem; color: var(--t-text3); margin-top: 0.25rem; }
.vcv-header-desc { font-size: 0.82rem; color: var(--t-text2); margin-top: 0.35rem; line-height: 1.5; }
.vcv-chapters { display: flex; flex-direction: column; gap: 0.5rem; }
.vcv-chapter { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; overflow: hidden; }
.vcv-ch-header { display: flex; align-items: center; gap: 0.75rem; padding: 0.85rem 1.25rem; cursor: pointer; user-select: none; }
.vcv-ch-header:hover { background: var(--t-hover); }
.vcv-ch-title { flex: 1; font-size: 0.875rem; font-weight: 700; color: var(--t-text1); }
.vcv-ch-meta { font-size: 0.72rem; color: var(--t-text3); }
.vcv-ch-arrow { font-size: 1rem; color: var(--t-text3); transition: transform 0.2s; }
.vcv-open { transform: rotate(180deg); }
.vcv-lesson-list { border-top: 1px solid var(--t-border); }
.vcv-lesson { display: flex; align-items: center; gap: 0.75rem; padding: 0.7rem 1.25rem; text-decoration: none; border-bottom: 1px solid var(--t-border); }
.vcv-lesson:last-child { border-bottom: none; }
.vcv-lesson:hover { background: var(--t-hover); }
.vcv-lesson-num { width: 24px; height: 24px; border-radius: 50%; background: var(--t-bg); border: 1px solid var(--t-border); display: flex; align-items: center; justify-content: center; font-size: 0.7rem; font-weight: 700; color: var(--t-text3); flex-shrink: 0; }
.vcv-lesson-info { flex: 1; min-width: 0; }
.vcv-lesson-title { font-size: 0.825rem; font-weight: 600; color: var(--t-text1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.vcv-lesson-meta { font-size: 0.7rem; color: var(--t-text3); margin-top: 0.1rem; }
.vcv-play-btn { width: 30px; height: 30px; border-radius: 50%; background: #4caf50; color: white; display: flex; align-items: center; justify-content: center; font-size: 0.65rem; flex-shrink: 0; }
</style>
