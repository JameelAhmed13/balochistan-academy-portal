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

    <!-- Loading -->
    <div v-if="loading" class="rs-state"><div class="rs-spinner" /><span>Loading resources…</span></div>

    <!-- Error -->
    <div v-else-if="loadErr" class="rs-state rs-state--error">
      <span>Couldn't load resources for this subject.</span>
      <button type="button" class="btn-ghost rs-retry" @click="load">Retry</button>
    </div>

    <!-- Empty -->
    <div v-else-if="!units.length" class="rs-state">
      <span>No units published for {{ subject?.name || 'this subject' }} yet.</span>
    </div>

    <template v-else>
      <!-- Textbook banner -->
      <div class="rs-textbook">
        <div class="rs-tb-cover" :class="subject?.color || 'grad-blue'">{{ subject?.icon || '📘' }}</div>
        <div class="rs-tb-info">
          <div class="rs-tb-title">{{ subject?.name }} — {{ gradeLabel }}</div>
          <div class="rs-tb-meta">{{ units.length }} units · {{ totalTopics }} topics</div>
        </div>
        <button type="button" class="btn-secondary rs-tb-btn" @click="expandAll">
          <BookOpen class="w-4 h-4" /> {{ allOpen ? 'Collapse all' : 'Open full contents' }}
        </button>
      </div>

      <!-- Chapter (unit) list -->
      <div class="sp-list">
        <div v-for="(u, ci) in units" :key="u.id" class="sp-item" :class="{ 'is-open': open.has(ci) }">
          <button type="button" class="sp-item-hd" @click="toggle(ci)">
            <span class="sp-item-num">{{ ci + 1 }}</span>
            <span class="sp-item-title">{{ u.name }}</span>
            <span class="sp-item-meta">{{ u.resourceCount ?? 0 }} resources</span>
            <ChevronRight class="sp-item-chev" />
          </button>
          <div v-show="open.has(ci)" class="sp-item-body">
            <div v-if="!resourcesByUnit[u.id]" class="rs-intro">Loading resources…</div>
            <div v-else-if="!resourcesByUnit[u.id].length" class="rs-intro">No resources published for this unit yet.</div>
            <a
              v-for="r in resourcesByUnit[u.id]"
              :key="r.id"
              :href="r.url"
              target="_blank"
              rel="noopener"
              class="sp-point rs-resource-link"
            >
              <span class="sp-point-dot" />
              <span class="rs-resource-title">{{ r.title }}</span>
              <span v-if="r.kind" class="rs-resource-kind">{{ r.kind }}</span>
            </a>
            <div class="rs-actions">
              <RouterLink :to="`/app/preparation/${bid}/videos`" class="sp-pill">
                <Video class="w-3.5 h-3.5" /> Watch lectures
              </RouterLink>
              <RouterLink :to="`/app/preparation/${bid}/keynotes`" class="sp-pill">
                <FileText class="w-3.5 h-3.5" /> Revision checklist
              </RouterLink>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ChevronRight, BookOpen, Video, FileText } from '@lucide/vue'
import { findPrepSubject, gradeLabelFor } from '@/views/preparation/prepSubjects'
import { useAuthStore } from '@/stores/auth'
import api from '@/services/api'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()
const auth = useAuthStore()

const bid = computed(() => props.bookId ?? route.params.bookId)
const subject = computed(() => findPrepSubject(bid.value))
const gradeLabel = computed(() => gradeLabelFor(bid.value))

// Live backend hierarchy: subject -> book(s) -> units (with resourceCount) -> resources (lazy per unit).
const units = ref([])
const resourcesByUnit = reactive({})
const loading = ref(true)
const loadErr = ref(false)

async function load() {
  if (!bid.value) return
  loading.value = true
  loadErr.value = false
  Object.keys(resourcesByUnit).forEach((k) => delete resourcesByUnit[k])
  try {
    const books = (await api.get(`/subjects/${bid.value}/books`, { params: { gradeCode: auth.user?.gradeCode } })).data
    const book = books?.[0]
    units.value = book ? (await api.get(`/books/${book.id}/units`)).data : []
  } catch {
    units.value = []
    loadErr.value = true
  }
  loading.value = false
}
onMounted(load)
watch(bid, load)

const totalTopics = computed(() => units.value.reduce((n, u) => n + (u.topicCount || 0), 0))
const open = reactive(new Set([0]))
const allOpen = ref(false)

async function ensureResources(unit) {
  if (!unit || resourcesByUnit[unit.id]) return
  try {
    resourcesByUnit[unit.id] = (await api.get(`/units/${unit.id}/resources`)).data
  } catch {
    resourcesByUnit[unit.id] = []
  }
}
function toggle(ci) {
  if (open.has(ci)) { open.delete(ci); return }
  open.add(ci)
  ensureResources(units.value[ci])
}
function expandAll() {
  allOpen.value = !allOpen.value
  open.clear()
  if (allOpen.value) {
    units.value.forEach((u, i) => { open.add(i); ensureResources(u) })
  } else {
    open.add(0)
    ensureResources(units.value[0])
  }
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
.rs-resource-link { text-decoration: none; color: inherit; cursor: pointer; }
.rs-resource-title { flex: 1; }
.rs-resource-kind {
  font-size: 0.64rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em;
  color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 10%, transparent);
  border-radius: 99px; padding: 0.1rem 0.5rem; flex-shrink: 0;
}
.rs-state {
  display: flex; flex-direction: column; align-items: center; gap: 0.75rem;
  padding: 3rem 1rem; color: var(--t-text3); font-size: 0.85rem; text-align: center;
}
.rs-state--error { color: #ef4444; }
.rs-retry { margin-top: 0.25rem; }
.rs-spinner {
  width: 24px; height: 24px; border-radius: 50%;
  border: 3px solid var(--t-border); border-top-color: var(--t-accent);
  animation: rs-spin 0.7s linear infinite;
}
@keyframes rs-spin { to { transform: rotate(360deg); } }
</style>
