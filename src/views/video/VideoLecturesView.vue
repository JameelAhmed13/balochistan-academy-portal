<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Video Lectures</h2>
    <div class="vlv-search-row">
      <input v-model="search" type="text" placeholder="Search courses..." class="vlv-search" />
    </div>
    <div class="vlv-grid">
      <RouterLink
        v-for="course in filteredCourses"
        :key="course.id"
        :to="`/app/video/course/${course.id}`"
        class="vlv-card"
        :style="{ '--course-color': course.color || '#4caf50' }"
      >
        <div class="vlv-card-top">
          <div class="vlv-icon">{{ course.icon }}</div>
          <div class="vlv-grade-badge">{{ course.gradeLabel }}</div>
        </div>
        <div class="vlv-title">{{ course.title }}</div>
        <div class="vlv-meta">
          <span>📚 {{ course.totalChapters }} Chapters</span>
          <span>🎥 {{ course.totalLessons }} Lessons</span>
        </div>
      </RouterLink>
    </div>
    <div v-if="!filteredCourses.length" class="vlv-empty">
      No courses match "{{ search }}"
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import courses from '@/assets/data/courses.json'
import PageFooter from '@/components/platform/PageFooter.vue'

const search = ref('')
const filteredCourses = computed(() => {
  if (!search.value) return courses
  const q = search.value.toLowerCase()
  return courses.filter(c => c.title.toLowerCase().includes(q) || c.subject.toLowerCase().includes(q))
})
</script>

<style scoped>
.vlv-search-row { display: flex; gap: 0.75rem; }
.vlv-search { flex: 1; max-width: 380px; padding: 0.6rem 1rem; border: 1px solid var(--t-border); border-radius: 9px; background: var(--t-surface); color: var(--t-text1); font-size: 0.875rem; }
.vlv-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem; }
.vlv-card {
  background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 14px; padding: 1.25rem;
  text-decoration: none; display: block; transition: transform 0.15s, box-shadow 0.15s;
  border-top: 3px solid var(--course-color, #4caf50);
}
.vlv-card:hover { transform: translateY(-2px); box-shadow: 0 4px 20px rgba(0,0,0,0.12); }
.vlv-card-top { display: flex; align-items: center; justify-content: space-between; margin-bottom: 0.75rem; }
.vlv-icon { font-size: 1.75rem; }
.vlv-grade-badge { font-size: 0.7rem; font-weight: 700; padding: 0.2rem 0.6rem; border-radius: 99px; background: color-mix(in srgb, var(--course-color, #4caf50) 15%, transparent); color: var(--course-color, #4caf50); border: 1px solid color-mix(in srgb, var(--course-color, #4caf50) 30%, transparent); }
.vlv-title { font-size: 0.9rem; font-weight: 700; color: var(--t-text1); margin-bottom: 0.5rem; line-height: 1.3; }
.vlv-meta { display: flex; gap: 0.75rem; font-size: 0.72rem; color: var(--t-text3); }
.vlv-empty { text-align: center; padding: 3rem; color: var(--t-text3); }
</style>
