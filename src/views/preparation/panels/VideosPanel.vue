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

    <!-- Loading -->
    <div v-if="loading" class="vl-state"><div class="vl-spinner" /><span>Loading courses…</span></div>

    <!-- Error -->
    <div v-else-if="loadErr" class="vl-state vl-state--error">
      <span>Couldn't load video lectures.</span>
      <button type="button" class="btn-ghost vl-retry" @click="loadCourses">Retry</button>
    </div>

    <!-- Empty -->
    <div v-else-if="!courses.length" class="vl-state">
      <span>No video lectures published for {{ subject?.name || 'this subject' }} yet.</span>
    </div>

    <template v-else>
      <!-- ── Course grid ── -->
      <template v-if="!activeCourse">
        <div class="sp-toolbar">
          <span class="sp-scope-note">
            <Film class="w-3.5 h-3.5" /> {{ courses.length }} course{{ courses.length === 1 ? '' : 's' }}
          </span>
        </div>

        <div class="sp-grid">
          <button v-for="c in courses" :key="c.id" type="button" class="sp-card" @click="openCourse(c)">
            <span class="sp-card-icon">🎬</span>
            <span class="sp-card-title">{{ c.title }}</span>
            <span class="sp-card-desc">{{ c.description || (c.lessonCount + ' lessons') }}</span>
            <span class="sp-card-foot">
              <span v-if="c.tutorName" class="sp-chip">{{ c.tutorName }}</span>
              <span class="sp-go">{{ c.lessonCount }} lessons →</span>
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
                <div class="sp-player-sub">{{ activeCourse.title }}</div>
              </div>
            </div>
          </div>

          <!-- Playlist -->
          <aside class="vp-playlist" aria-label="Lessons">
            <div class="vp-pl-hd">{{ lessons.length }} lessons</div>
            <div v-if="lessonsLoading" class="vl-state" style="padding:1.5rem 0.5rem;"><div class="vl-spinner" /></div>
            <div v-else-if="!lessons.length" class="vl-state" style="padding:1.5rem 0.5rem;">No lessons yet.</div>
            <div v-else class="vp-pl-scroll">
              <button
                v-for="ls in lessons" :key="ls.id"
                type="button"
                class="vp-lesson"
                :class="{ 'is-playing': currentLesson?.id === ls.id }"
                @click="playLesson(ls)">
                <span class="vp-lesson-ic">
                  <Play v-if="currentLesson?.id !== ls.id" class="w-3 h-3" />
                  <AudioLines v-else class="w-3 h-3" />
                </span>
                <span class="vp-lesson-title">{{ ls.title }}</span>
                <span v-if="ls.durationSec" class="vp-lesson-dur">{{ Math.round(ls.durationSec / 60) }}m</span>
              </button>
            </div>
          </aside>
        </div>
      </template>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeft, Play, Film, AudioLines } from '@lucide/vue'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { useAuthStore } from '@/stores/auth'
import api from '@/services/api'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()
const auth = useAuthStore()

const bid = computed(() => props.bookId ?? route.params.bookId)
const subject = computed(() => findPrepSubject(bid.value))

// Live backend: GET /api/content/courses (list) + GET /api/content/courses/{id} (lessons).
const courses = ref([])
const loading = ref(true)
const loadErr = ref(false)

async function loadCourses() {
  if (!bid.value) return
  loading.value = true
  loadErr.value = false
  try {
    const { data } = await api.get('/content/courses', {
      params: { gradeCode: auth.user?.gradeCode, subjectId: +bid.value },
    })
    courses.value = data || []
  } catch {
    courses.value = []
    loadErr.value = true
  }
  loading.value = false
}
onMounted(loadCourses)
watch(bid, loadCourses)

const activeCourse = ref(null)
const lessons = ref([])
const lessonsLoading = ref(false)
const currentLesson = ref(null)
const autoplay = ref(false)

// Accepts a bare YouTube id, a youtu.be/watch link, or an already-embeddable URL.
function toEmbedUrl(url) {
  if (!url) return 'about:blank'
  if (url.includes('/embed/')) return url
  const auto = autoplay.value ? '&autoplay=1' : ''
  if (/^[\w-]{11}$/.test(url)) return `https://www.youtube.com/embed/${url}?rel=0&modestbranding=1${auto}`
  const m = url.match(/(?:youtu\.be\/|[?&]v=)([\w-]{11})/)
  if (m) return `https://www.youtube.com/embed/${m[1]}?rel=0&modestbranding=1${auto}`
  return url
}
const embedSrc = computed(() => toEmbedUrl(currentLesson.value?.videoUrl))

async function openCourse(c) {
  activeCourse.value = c
  currentLesson.value = null
  autoplay.value = false
  lessons.value = []
  lessonsLoading.value = true
  try {
    const { data } = await api.get(`/content/courses/${c.id}`)
    lessons.value = (data?.lessons || []).slice().sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
    currentLesson.value = lessons.value[0] || null
  } catch {
    lessons.value = []
  }
  lessonsLoading.value = false
}
function closeCourse() {
  activeCourse.value = null
  currentLesson.value = null
  lessons.value = []
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

.vl-state {
  display: flex; flex-direction: column; align-items: center; gap: 0.75rem;
  padding: 3rem 1rem; color: var(--t-text3); font-size: 0.85rem; text-align: center;
}
.vl-state--error { color: #ef4444; }
.vl-retry { margin-top: 0.25rem; }
.vl-spinner {
  width: 24px; height: 24px; border-radius: 50%;
  border: 3px solid var(--t-border); border-top-color: var(--t-accent);
  animation: vl-spin 0.7s linear infinite;
}
@keyframes vl-spin { to { transform: rotate(360deg); } }

@media (max-width: 860px) {
  .vp-layout { grid-template-columns: 1fr; }
  .vp-playlist { max-height: 340px; }
}
</style>
