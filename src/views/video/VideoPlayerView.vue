<template>
  <div class="animate-fade-in vpv-layout" v-if="lesson">
    <!-- Main player -->
    <div class="vpv-main">
      <div class="vpv-player-wrap">
        <iframe
          :src="`https://www.youtube.com/embed/${lesson.videoId}?autoplay=0&rel=0`"
          frameborder="0"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowfullscreen
          class="vpv-iframe"
        />
      </div>
      <div class="vpv-info">
        <div class="vpv-lesson-title">{{ lesson.title }}</div>
        <div class="vpv-lesson-meta">{{ course?.title }} · {{ lesson.duration }}</div>
        <div class="vpv-nav">
          <RouterLink v-if="prevLesson" :to="`/app/video/player/${course?.id}/${prevLesson.id}`" class="vpv-nav-btn">← Previous</RouterLink>
          <RouterLink v-if="nextLesson" :to="`/app/video/player/${course?.id}/${nextLesson.id}`" class="vpv-nav-btn vpv-nav-next">Next →</RouterLink>
        </div>
      </div>
    </div>

    <!-- Sidebar playlist -->
    <div class="vpv-sidebar">
      <div class="vpv-sidebar-title">Course Playlist</div>
      <div class="vpv-playlist">
        <template v-for="ch in course?.chapters" :key="ch.number">
          <div class="vpv-ch-label">Chapter {{ ch.number }}: {{ ch.title }}</div>
          <RouterLink
            v-for="l in ch.lessons"
            :key="l.id"
            :to="`/app/video/player/${course?.id}/${l.id}`"
            class="vpv-pl-item"
            :class="{ 'vpv-pl-active': l.id === lesson.id }"
          >
            <div class="vpv-pl-icon">{{ l.id === lesson.id ? '▶' : '○' }}</div>
            <div class="vpv-pl-title">{{ l.title }}</div>
          </RouterLink>
        </template>
      </div>
    </div>
  </div>
  <div v-else class="animate-fade-in p-8 text-center text-t-text3">Lesson not found.</div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import courses from '@/assets/data/courses.json'

const route = useRoute()
const course = computed(() => courses.find(c => c.id === route.params.courseId))

const allLessons = computed(() => course.value?.chapters?.flatMap(ch => ch.lessons) || [])
const lesson = computed(() => allLessons.value.find(l => l.id === route.params.lessonId))
const lessonIdx = computed(() => allLessons.value.findIndex(l => l.id === route.params.lessonId))
const prevLesson = computed(() => lessonIdx.value > 0 ? allLessons.value[lessonIdx.value - 1] : null)
const nextLesson = computed(() => lessonIdx.value < allLessons.value.length - 1 ? allLessons.value[lessonIdx.value + 1] : null)
</script>

<style scoped>
.vpv-layout { display: flex; gap: 1.25rem; align-items: flex-start; }
.vpv-main { flex: 1; min-width: 0; }
.vpv-player-wrap { position: relative; padding-bottom: 56.25%; height: 0; border-radius: 12px; overflow: hidden; background: #000; }
.vpv-iframe { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
.vpv-info { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; padding: 1.25rem; margin-top: 0.75rem; }
.vpv-lesson-title { font-size: 1rem; font-weight: 700; color: var(--t-text1); margin-bottom: 0.25rem; }
.vpv-lesson-meta { font-size: 0.78rem; color: var(--t-text3); margin-bottom: 0.75rem; }
.vpv-nav { display: flex; gap: 0.5rem; }
.vpv-nav-btn { padding: 0.45rem 1rem; border-radius: 7px; font-size: 0.8rem; font-weight: 600; text-decoration: none; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); }
.vpv-nav-next { background: #4caf50; color: white; border-color: #4caf50; margin-left: auto; }

.vpv-sidebar { width: 280px; flex-shrink: 0; background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; max-height: calc(100vh - 120px); overflow-y: auto; }
.vpv-sidebar-title { padding: 0.85rem 1rem; font-size: 0.85rem; font-weight: 700; color: var(--t-text1); border-bottom: 1px solid var(--t-border); position: sticky; top: 0; background: var(--t-surface); z-index: 1; }
.vpv-ch-label { padding: 0.5rem 1rem; font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: var(--t-text3); background: var(--t-bg); border-bottom: 1px solid var(--t-border); }
.vpv-pl-item { display: flex; align-items: flex-start; gap: 0.5rem; padding: 0.6rem 1rem; text-decoration: none; border-bottom: 1px solid var(--t-border); }
.vpv-pl-item:hover { background: var(--t-hover); }
.vpv-pl-active { background: rgba(76,175,80,0.1) !important; }
.vpv-pl-icon { color: #4caf50; font-size: 0.7rem; flex-shrink: 0; margin-top: 2px; }
.vpv-pl-title { font-size: 0.78rem; color: var(--t-text1); line-height: 1.4; }

@media (max-width: 768px) { .vpv-layout { flex-direction: column; } .vpv-sidebar { width: 100%; max-height: 300px; } }
</style>
