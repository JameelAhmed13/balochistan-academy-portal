<template>
  <div class="sp">
    <!-- Header -->
    <div class="sp-head">
      <span class="sp-head-icon" :class="subject?.color || 'grad-blue'">{{ subject?.icon || '📂' }}</span>
      <div class="sp-head-info">
        <div class="sp-eyebrow">Academic · Library</div>
        <h1 class="sp-title">{{ subject?.name || 'Academic Resources' }}</h1>
        <p class="sp-sub">Textbook, chapter handouts and study materials — all in one place.</p>
      </div>
    </div>

    <!-- Textbook banner -->
    <div class="rs-textbook">
      <div class="rs-tb-cover" :class="subject?.color || 'grad-blue'">{{ subject?.icon || '📘' }}</div>
      <div class="rs-tb-info">
        <div class="rs-tb-title">{{ subject?.name }} — {{ gradeLabel }}</div>
        <div class="rs-tb-meta">Federal Board · {{ chapters.length }} chapters · {{ totalTopics }} topics</div>
      </div>
      <button type="button" class="btn-secondary rs-tb-btn" @click="expandAll">
        <BookOpen class="w-4 h-4" /> {{ allOpen ? 'Collapse all' : 'Open full contents' }}
      </button>
    </div>

    <!-- Chapter handouts -->
    <div class="sp-list">
      <div v-for="(ch, ci) in chapters" :key="ch.k" class="sp-item" :class="{ 'is-open': open.has(ci) }">
        <button type="button" class="sp-item-hd" @click="toggle(ci)">
          <span class="sp-item-num">{{ ci + 1 }}</span>
          <span class="sp-item-title">{{ ch.title }}</span>
          <span class="sp-item-meta">{{ ch.points.length }} topics</span>
          <ChevronRight class="sp-item-chev" />
        </button>
        <div v-show="open.has(ci)" class="sp-item-body">
          <div class="rs-intro">This chapter covers the following topics:</div>
          <div v-for="(pt, pi) in ch.points" :key="pi" class="sp-point">
            <span class="sp-point-dot" />
            <span>{{ pt }}</span>
          </div>
          <div class="rs-actions">
            <RouterLink v-if="hasVideos" :to="`/app/preparation/${bid}/videos`" class="sp-pill">
              <Video class="w-3.5 h-3.5" /> Watch lectures
            </RouterLink>
            <RouterLink :to="`/app/preparation/${bid}/keynotes`" class="sp-pill">
              <FileText class="w-3.5 h-3.5" /> Revision checklist
            </RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ChevronRight, BookOpen, Video, FileText } from '@lucide/vue'
import { findPrepSubject, gradeLabelFor } from '@/views/preparation/prepSubjects'
import allCourses from '@/assets/data/courses.json'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()

const bid = computed(() => props.bookId ?? route.params.bookId)
const subject = computed(() => findPrepSubject(bid.value))
const gradeLabel = computed(() => gradeLabelFor(bid.value))

function cleanTitle(t) {
  return String(t || '')
    .replace(/\s*\[[^\]]*\]\s*/g, ' ')
    .replace(/lecture\s*[-#]?\s*\d+/gi, '')
    .replace(/\s{2,}/g, ' ')
    .trim()
}

const subjCourses = computed(() => allCourses.filter((c) => c.subject === subject.value?.name))
const hasVideos = computed(() => subjCourses.value.length > 0)

const chapters = computed(() => {
  if (subjCourses.value.length) {
    const out = []
    subjCourses.value.forEach((course) => {
      ;(course.chapters || []).forEach((ch) => {
        const points = (ch.lessons || []).map((l) => cleanTitle(l.title)).filter(Boolean)
        out.push({
          k: `${course.id}-${ch.number}`,
          title: subjCourses.value.length > 1 ? `${course.gradeLabel || ('Grade ' + course.grade)} · ${ch.title}` : ch.title,
          points: points.length ? points : [`Overview of ${ch.title}`],
        })
      })
    })
    if (out.length) return out
  }
  return Array.from({ length: 10 }, (_, i) => ({
    k: `u${i + 1}`,
    title: `Unit ${i + 1}`,
    points: Array.from({ length: 8 }, (_, j) => `Topic ${i + 1}.${j + 1}`),
  }))
})

const totalTopics = computed(() => chapters.value.reduce((n, c) => n + c.points.length, 0))
const open = reactive(new Set([0]))
const allOpen = ref(false)

function toggle(ci) {
  if (open.has(ci)) open.delete(ci)
  else open.add(ci)
}
function expandAll() {
  allOpen.value = !allOpen.value
  open.clear()
  if (allOpen.value) chapters.value.forEach((_, i) => open.add(i))
  else open.add(0)
}
</script>

<style scoped>
.rs-textbook {
  display: flex; align-items: center; gap: 1rem; flex-wrap: wrap;
  padding: 1rem 1.1rem; border: 1px solid var(--t-border); border-radius: 14px;
  background: var(--t-surface); margin-bottom: 1.1rem;
}
.rs-tb-cover { width: 46px; height: 58px; border-radius: 6px; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; color: #fff; flex-shrink: 0; }
.rs-tb-info { flex: 1; min-width: 0; }
.rs-tb-title { font-size: 0.95rem; font-weight: 800; color: var(--t-text1); }
.rs-tb-meta { font-size: 0.74rem; color: var(--t-text3); margin-top: 0.15rem; }
.rs-tb-btn { font-size: 0.78rem; }
.rs-intro { font-size: 0.76rem; color: var(--t-text3); margin-bottom: 0.4rem; }
.rs-actions { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-top: 0.7rem; }
.rs-actions .sp-pill { display: inline-flex; align-items: center; gap: 0.35rem; text-decoration: none; }
</style>
