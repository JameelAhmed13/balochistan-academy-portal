<template>
  <div class="sp">
    <!-- Header -->
    <div class="sp-head">
      <span class="sp-head-icon" :class="subject?.color || 'grad-violet'">{{ subject?.icon || '🎬' }}</span>
      <div class="sp-head-info">
        <div class="sp-eyebrow">Daikho · Watch</div>
        <h1 class="sp-title">{{ subject?.name || 'Video Lectures' }}</h1>
        <p class="sp-sub">Expert teacher lectures — watch right here, no page-hopping.</p>
      </div>
    </div>

    <!-- ── Course grid ── -->
    <template v-if="!activeCourse">
      <div class="sp-toolbar">
        <span class="sp-scope-note" v-if="scoped">
          <Film class="w-3.5 h-3.5" /> {{ courses.length }} course{{ courses.length === 1 ? '' : 's' }} · {{ lessonCount }} lessons
        </span>
        <span class="sp-scope-note" v-else>
          <Film class="w-3.5 h-3.5" /> No {{ subject?.name }} lectures yet — showing the full video library
        </span>
      </div>

      <div class="sp-grid">
        <button v-for="c in courses" :key="c.id" type="button" class="sp-card" @click="openCourse(c)">
          <span class="sp-card-icon">{{ c.icon || '🎬' }}</span>
          <span class="sp-card-title">{{ c.title }}</span>
          <span class="sp-card-desc">{{ c.description || (c.totalChapters + ' chapters of guided lessons') }}</span>
          <span class="sp-card-foot">
            <span class="sp-chip">{{ c.gradeLabel || ('Grade ' + c.grade) }}</span>
            <span class="sp-go">{{ c.totalLessons }} lessons →</span>
          </span>
        </button>
      </div>
    </template>

    <!-- ── Course player ── -->
    <template v-else>
      <button type="button" class="sp-back" @click="closeCourse">
        <ArrowLeft class="w-3.5 h-3.5" /> All video courses
      </button>

      <div class="vp-layout">
        <!-- Player -->
        <div class="vp-main">
          <div class="sp-player">
            <div class="sp-player-frame">
              <iframe
                :src="embedSrc"
                :title="currentLesson?.title || activeCourse.title"
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                allowfullscreen
                referrerpolicy="strict-origin-when-cross-origin" />
            </div>
            <div class="sp-player-meta">
              <div class="sp-player-title">{{ currentLesson?.title || 'Select a lesson' }}</div>
              <div class="sp-player-sub">
                {{ activeCourse.title }}
                <template v-if="currentLesson?.duration"> · {{ currentLesson.duration }}</template>
              </div>
            </div>
          </div>
        </div>

        <!-- Playlist -->
        <aside class="vp-playlist" aria-label="Lessons">
          <div class="vp-pl-hd">{{ activeCourse.totalLessons }} lessons · {{ activeCourse.totalChapters }} chapters</div>
          <div class="vp-pl-scroll">
            <div v-for="ch in activeCourse.chapters" :key="ch.number" class="vp-chapter">
              <div class="vp-chapter-hd">Ch {{ ch.number }} · {{ ch.title }}</div>
              <button
                v-for="ls in ch.lessons" :key="ls.id"
                type="button"
                class="vp-lesson"
                :class="{ 'is-playing': currentLesson?.id === ls.id }"
                @click="playLesson(ls)">
                <span class="vp-lesson-ic">
                  <Play v-if="currentLesson?.id !== ls.id" class="w-3 h-3" />
                  <AudioLines v-else class="w-3 h-3" />
                </span>
                <span class="vp-lesson-title">{{ ls.title }}</span>
                <span v-if="ls.duration" class="vp-lesson-dur">{{ ls.duration }}</span>
              </button>
            </div>
          </div>
        </aside>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeft, Play, Film, AudioLines } from '@lucide/vue'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import allCourses from '@/assets/data/courses.json'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()

const subject = computed(() => findPrepSubject(props.bookId ?? route.params.bookId))

// Subject-scoped courses; fall back to the full library so the panel is never empty.
const scoped = computed(() => allCourses.some((c) => c.subject === subject.value?.name))
const courses = computed(() =>
  scoped.value ? allCourses.filter((c) => c.subject === subject.value?.name) : allCourses,
)
const lessonCount = computed(() => courses.value.reduce((n, c) => n + (c.totalLessons || 0), 0))

const activeCourse = ref(null)
const currentLesson = ref(null)
const autoplay = ref(false)

const embedSrc = computed(() => {
  const id = currentLesson.value?.videoId
  if (!id) return 'about:blank'
  return `https://www.youtube.com/embed/${id}?rel=0&modestbranding=1${autoplay.value ? '&autoplay=1' : ''}`
})

function firstLesson(course) {
  for (const ch of course.chapters || []) {
    if (ch.lessons?.length) return ch.lessons[0]
  }
  return null
}
function openCourse(c) {
  activeCourse.value = c
  currentLesson.value = firstLesson(c)
  autoplay.value = false
}
function closeCourse() {
  activeCourse.value = null
  currentLesson.value = null
}
function playLesson(ls) {
  currentLesson.value = ls
  autoplay.value = true
}
</script>

<style scoped>
.vp-layout { display: grid; grid-template-columns: 1fr 300px; gap: 1rem; align-items: start; }
.vp-main { min-width: 0; }
.vp-playlist {
  border: 1px solid var(--t-border); border-radius: 14px; background: var(--t-surface);
  display: flex; flex-direction: column; max-height: 540px; overflow: hidden;
}
.vp-pl-hd {
  padding: 0.7rem 0.9rem; font-size: 0.7rem; font-weight: 700; color: var(--t-text3);
  text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid var(--t-border);
}
.vp-pl-scroll { overflow-y: auto; padding: 0.4rem; }
.vp-chapter { margin-bottom: 0.4rem; }
.vp-chapter-hd { font-size: 0.68rem; font-weight: 700; color: var(--t-text3); padding: 0.4rem 0.5rem 0.2rem; }
.vp-lesson {
  width: 100%; display: flex; align-items: center; gap: 0.5rem; padding: 0.45rem 0.5rem;
  border-radius: 8px; border: none; background: none; cursor: pointer; text-align: left;
  transition: background 0.12s;
}
.vp-lesson:hover { background: var(--t-hover); }
.vp-lesson.is-playing { background: color-mix(in srgb, var(--t-accent) 12%, transparent); }
.vp-lesson-ic {
  width: 22px; height: 22px; border-radius: 6px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  background: var(--t-hover2); color: var(--t-text2);
}
.vp-lesson.is-playing .vp-lesson-ic { background: var(--t-accent); color: #fff; }
.vp-lesson-title {
  flex: 1; min-width: 0; font-size: 0.74rem; color: var(--t-text2); line-height: 1.3;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
}
.vp-lesson.is-playing .vp-lesson-title { color: var(--t-text1); font-weight: 600; }
.vp-lesson-dur { font-size: 0.66rem; color: var(--t-text3); flex-shrink: 0; }

@media (max-width: 860px) {
  .vp-layout { grid-template-columns: 1fr; }
  .vp-playlist { max-height: 340px; }
}
</style>
