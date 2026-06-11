<template>
  <div class="sp">
    <!-- Header -->
    <div class="sp-head">
      <span class="sp-head-icon" :class="subject?.color || 'grad-teal'">{{ subject?.icon || '📝' }}</span>
      <div class="sp-head-info">
        <div class="sp-eyebrow">Parho · Read</div>
        <h1 class="sp-title">{{ subject?.name || 'Key Notes' }}</h1>
        <p class="sp-sub">Chapter-wise revision checklist — tick topics as you master them.</p>
      </div>
    </div>

    <!-- Progress -->
    <div class="kn-progress">
      <div class="kn-progress-bar"><span :style="{ width: pct + '%' }" /></div>
      <span class="kn-progress-lbl">{{ checked.size }} / {{ totalPoints }} topics revised ({{ pct }}%)</span>
    </div>

    <!-- Chapters -->
    <div class="sp-list">
      <div v-for="(ch, ci) in chapters" :key="ci" class="sp-item" :class="{ 'is-open': open === ci }">
        <button type="button" class="sp-item-hd" @click="open = open === ci ? -1 : ci">
          <span class="sp-item-num">{{ ci + 1 }}</span>
          <span class="sp-item-title">{{ ch.title }}</span>
          <span class="sp-item-meta">{{ doneIn(ch) }}/{{ ch.points.length }}</span>
          <ChevronRight class="sp-item-chev" />
        </button>
        <div v-show="open === ci" class="sp-item-body">
          <label v-for="(pt, pi) in ch.points" :key="pi" class="kn-point">
            <input type="checkbox" class="kn-check" :checked="checked.has(ch.k + ':' + pi)" @change="toggle(ch.k + ':' + pi)" />
            <span class="kn-point-text" :class="{ 'is-done': checked.has(ch.k + ':' + pi) }">{{ pt }}</span>
          </label>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ChevronRight } from '@lucide/vue'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import allCourses from '@/assets/data/courses.json'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()

const subject = computed(() => findPrepSubject(props.bookId ?? route.params.bookId))

function cleanTitle(t) {
  return String(t || '')
    .replace(/\s*\[[^\]]*\]\s*/g, ' ')        // drop [Lecture-01] style tags
    .replace(/lecture\s*[-#]?\s*\d+/gi, '')   // drop "Lecture 1"
    .replace(/\s{2,}/g, ' ')
    .trim()
}

// Build chapters from real course data when the subject has lectures;
// otherwise fall back to the canonical syllabus-unit structure.
const chapters = computed(() => {
  const subjCourses = allCourses.filter((c) => c.subject === subject.value?.name)
  if (subjCourses.length) {
    const out = []
    subjCourses.forEach((course) => {
      ;(course.chapters || []).forEach((ch) => {
        const points = (ch.lessons || []).map((l) => cleanTitle(l.title)).filter(Boolean)
        out.push({
          k: `${course.id}-${ch.number}`,
          title: subjCourses.length > 1 ? `${course.gradeLabel || ('Grade ' + course.grade)} · ${ch.title}` : ch.title,
          points: points.length ? points : [`Review ${ch.title}`],
        })
      })
    })
    if (out.length) return out
  }
  // Fallback: 10 syllabus units × 8 topics (matches the app's content model)
  return Array.from({ length: 10 }, (_, i) => ({
    k: `u${i + 1}`,
    title: `Unit ${i + 1}`,
    points: Array.from({ length: 8 }, (_, j) => `Topic ${i + 1}.${j + 1}`),
  }))
})

const totalPoints = computed(() => chapters.value.reduce((n, c) => n + c.points.length, 0))
const open = ref(0)
const checked = reactive(new Set())
const pct = computed(() => (totalPoints.value ? Math.round((checked.size / totalPoints.value) * 100) : 0))

function toggle(key) {
  if (checked.has(key)) checked.delete(key)
  else checked.add(key)
}
function doneIn(ch) {
  let n = 0
  ch.points.forEach((_, pi) => { if (checked.has(ch.k + ':' + pi)) n++ })
  return n
}
</script>

<style scoped>
.kn-progress { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; }
.kn-progress-bar { flex: 1; height: 8px; border-radius: 99px; background: var(--t-hover2); overflow: hidden; }
.kn-progress-bar span { display: block; height: 100%; border-radius: 99px; background: linear-gradient(90deg, var(--t-accent), var(--t-accent2)); transition: width 0.25s; }
.kn-progress-lbl { font-size: 0.72rem; font-weight: 600; color: var(--t-text3); white-space: nowrap; }
.kn-point { display: flex; gap: 0.6rem; align-items: flex-start; padding: 0.35rem 0; cursor: pointer; }
.kn-check { margin-top: 0.2rem; width: 15px; height: 15px; accent-color: var(--t-accent); flex-shrink: 0; cursor: pointer; }
.kn-point-text { font-size: 0.82rem; color: var(--t-text2); line-height: 1.5; }
.kn-point-text.is-done { color: var(--t-text3); text-decoration: line-through; }
</style>
