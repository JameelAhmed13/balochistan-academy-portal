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

    <!-- Loading -->
    <div v-if="loading" class="kn-state"><div class="kn-spinner" /><span>Loading syllabus…</span></div>

    <!-- Error -->
    <div v-else-if="loadErr" class="kn-state kn-state--error">
      <span>Couldn't load the syllabus for this subject.</span>
      <button type="button" class="btn-ghost kn-retry" @click="loadSyllabus">Retry</button>
    </div>

    <!-- Empty -->
    <div v-else-if="!chapters.length" class="kn-state">
      <span>No syllabus published for {{ subject?.name || 'this subject' }} yet.</span>
    </div>

    <template v-else>
      <!-- Progress -->
      <div class="kn-progress">
        <div class="kn-progress-bar"><span :style="{ width: pct + '%' }" /></div>
        <span class="kn-progress-lbl">{{ checked.size }} / {{ totalPoints }} topics revised ({{ pct }}%)</span>
      </div>

      <!-- Chapters -->
      <div class="sp-list">
        <div v-for="(ch, ci) in chapters" :key="ch.k" class="sp-item" :class="{ 'is-open': open === ci }">
          <button type="button" class="sp-item-hd" @click="open = open === ci ? -1 : ci">
            <span class="sp-item-num">{{ ci + 1 }}</span>
            <span class="sp-item-title">{{ ch.title }}</span>
            <span class="sp-item-meta">{{ doneIn(ch) }}/{{ ch.points.length }}</span>
            <ChevronRight class="sp-item-chev" />
          </button>
          <div v-show="open === ci" class="sp-item-body">
            <label v-for="pt in ch.points" :key="pt.key" class="kn-point">
              <input type="checkbox" class="kn-check" :checked="checked.has(pt.key)" @change="toggle(pt.key)" />
              <span class="kn-point-text" :class="{ 'is-done': checked.has(pt.key) }">{{ pt.label }}</span>
            </label>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ChevronRight } from '@lucide/vue'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import api from '@/services/api'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()
const auth = useAuthStore()
const catalog = useCatalogStore()

const bid = computed(() => props.bookId ?? route.params.bookId)
const subject = computed(() => findPrepSubject(bid.value))
const gradeCode = computed(() => auth.user?.gradeCode)

// Real syllabus tree (units -> topics) instead of the bundled courses.json chapter list.
const units = ref([])
const loading = ref(true)
const loadErr = ref(false)

async function loadSyllabus() {
  if (!bid.value || !gradeCode.value) { units.value = []; loading.value = false; return }
  loading.value = true
  loadErr.value = false
  try {
    units.value = await catalog.fetchSyllabus(gradeCode.value, +bid.value)
  } catch {
    units.value = []
    loadErr.value = true
  }
  loading.value = false
}
onMounted(loadSyllabus)
watch(bid, loadSyllabus)

const chapters = computed(() => units.value.map((u) => ({
  k: String(u.id),
  title: u.name,
  points: (u.topics || []).map((t) => ({ key: `${u.id}:${t.id}`, label: t.name })),
})))
const totalPoints = computed(() => chapters.value.reduce((n, c) => n + c.points.length, 0))

// ── Persisted checklist state — one StudentNote per subject, content = JSON array of checked keys.
// StudentNote (title/content/tags) is the only per-student persistence endpoint the backend exposes
// for this; there's no dedicated "progress" entity, so we repurpose it rather than losing state on reload.
const NOTE_TITLE = computed(() => `__keynotes_checklist_subject_${bid.value}`)
const noteId = ref(null)
const checked = reactive(new Set())
const open = ref(0)
const pct = computed(() => (totalPoints.value ? Math.round((checked.size / totalPoints.value) * 100) : 0))

async function loadChecklist() {
  checked.clear()
  noteId.value = null
  try {
    const { data } = await api.get('/content/notes')
    const note = (data || []).find((n) => n.title === NOTE_TITLE.value)
    if (note) {
      noteId.value = note.id
      try { JSON.parse(note.content || '[]').forEach((k) => checked.add(k)) } catch { /* corrupt/legacy content, ignore */ }
    }
  } catch { /* offline — checklist still works locally for this session, just won't persist */ }
}
onMounted(loadChecklist)
watch(bid, loadChecklist)

let saveDebounce = null
function persistChecklist() {
  clearTimeout(saveDebounce)
  saveDebounce = setTimeout(async () => {
    const payload = { title: NOTE_TITLE.value, content: JSON.stringify([...checked]), tags: 'keynotes' }
    try {
      if (noteId.value) {
        await api.put(`/content/notes/${noteId.value}`, payload)
      } else {
        const { data } = await api.post('/content/notes', payload)
        noteId.value = data.id
      }
    } catch { /* best-effort save — state still holds locally this session */ }
  }, 600)
}

function toggle(key) {
  if (checked.has(key)) checked.delete(key)
  else checked.add(key)
  persistChecklist()
}
function doneIn(ch) {
  let n = 0
  ch.points.forEach((pt) => { if (checked.has(pt.key)) n++ })
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
.kn-state {
  display: flex; flex-direction: column; align-items: center; gap: 0.75rem;
  padding: 3rem 1rem; color: var(--t-text3); font-size: 0.85rem; text-align: center;
}
.kn-state--error { color: #ef4444; }
.kn-retry { margin-top: 0.25rem; }
.kn-spinner {
  width: 24px; height: 24px; border-radius: 50%;
  border: 3px solid var(--t-border); border-top-color: var(--t-accent);
  animation: kn-spin 0.7s linear infinite;
}
@keyframes kn-spin { to { transform: rotate(360deg); } }
</style>
