<template>
  <div class="sp">
    <!-- Header (hidden by PreparationLayout's :deep(.sp-head) rule on desktop) -->
    <div class="sp-head">
      <span class="sp-head-icon" :class="subject?.color || 'grad-indigo'">{{ subject?.icon || '📘' }}</span>
      <div class="sp-head-info">
        <div class="sp-eyebrow">Curriculum · Units &amp; Topics</div>
        <h1 class="sp-title">{{ subject?.name || 'Syllabus' }}</h1>
        <p class="sp-sub">{{ gradeLabel }} syllabus — all units, topics and learning objectives.</p>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="sl-state">
      <div class="sl-spinner" />
      <span>Loading syllabus…</span>
    </div>

    <!-- Empty -->
    <div v-else-if="!units.length" class="sl-state">
      <span>Syllabus for {{ subject?.name }} is being prepared.</span>
    </div>

    <!-- Units tree -->
    <div v-else class="sl-units">
      <article v-for="(u, i) in units" :key="u.id" class="sl-unit">
        <!-- Unit header — click to expand/collapse -->
        <button type="button" class="sl-unit-hd" @click="toggle(u.id)" :aria-expanded="open.has(u.id)">
          <span class="sl-num">{{ String(i + 1).padStart(2, '0') }}</span>
          <div class="sl-unit-info">
            <span class="sl-unit-name">{{ u.name }}</span>
            <span v-if="u.description" class="sl-unit-desc">{{ u.description }}</span>
          </div>
          <div class="sl-unit-meta">
            <span v-if="u.topics?.length" class="sl-badge">{{ u.topics.length }} topic{{ u.topics.length !== 1 ? 's' : '' }}</span>
          </div>
          <ChevronDown :class="['sl-chev', { 'is-open': open.has(u.id) }]" aria-hidden="true" />
        </button>

        <!-- Expanded body -->
        <Transition name="sl-expand">
          <div v-if="open.has(u.id)" class="sl-unit-body">
            <!-- Topics as chips -->
            <div v-if="u.topics?.length" class="sl-topics">
              <span class="sl-section-lbl">Topics</span>
              <div class="sl-topic-chips">
                <span v-for="t in u.topics" :key="t.id" class="sl-topic-chip">{{ t.name }}</span>
              </div>
            </div>

            <!-- Learning objectives (unit-level: topicId == null) -->
            <div v-if="unitObjectives(u).length" class="sl-objs">
              <span class="sl-section-lbl">You'll learn to</span>
              <ul class="sl-obj-list">
                <li v-for="o in unitObjectives(u)" :key="o.id">{{ o.text }}</li>
              </ul>
            </div>

            <!-- Practice CTA -->
            <RouterLink
              :to="`/app/preparation/${bid}/objective`"
              class="sl-practice"
            >
              Practice this unit →
            </RouterLink>
          </div>
        </Transition>
      </article>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ChevronDown } from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import { findPrepSubject, gradeLabelFor } from '@/views/preparation/prepSubjects'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()
const auth = useAuthStore()
const catalog = useCatalogStore()

const bid = computed(() => props.bookId ?? route.params.bookId)
const subject = computed(() => findPrepSubject(bid.value))
const gradeLabel = computed(() => gradeLabelFor(bid.value))

const units = ref([])
const loading = ref(false)
const open = ref(new Set())

// Objectives that belong directly to the unit (no topic parent)
function unitObjectives(u) {
  return (u.objectives || []).filter((o) => !o.topicId && !o.topic_id)
}

function toggle(id) {
  const s = new Set(open.value)
  s.has(id) ? s.delete(id) : s.add(id)
  open.value = s
}

async function load() {
  if (!bid.value) return
  loading.value = true
  units.value = []
  const gradeCode = auth.user?.gradeCode
  if (!gradeCode) { loading.value = false; return }
  try {
    units.value = await catalog.fetchSyllabus(gradeCode, bid.value)
    // Auto-open first unit
    if (units.value.length) open.value = new Set([units.value[0].id])
  } catch {
    units.value = []
  } finally {
    loading.value = false
  }
}

onMounted(load)
watch(bid, load)
</script>

<style scoped>
/* ── State ──────────────────────────────────────────────── */
.sl-state {
  display: flex; flex-direction: column; align-items: center; gap: 0.75rem;
  padding: 3rem 1rem; color: var(--t-text3); font-size: 0.875rem; text-align: center;
}
.sl-spinner {
  width: 24px; height: 24px; border-radius: 50%;
  border: 3px solid var(--t-border); border-top-color: var(--t-accent);
  animation: sl-spin 0.7s linear infinite;
}
@keyframes sl-spin { to { transform: rotate(360deg); } }

/* ── Units list ─────────────────────────────────────────── */
.sl-units { display: flex; flex-direction: column; gap: 0.5rem; }

.sl-unit {
  border: 1px solid var(--t-border);
  border-radius: 14px;
  background: var(--t-surface);
  overflow: hidden;
  transition: border-color 0.15s;
}
.sl-unit:has(.sl-unit-hd[aria-expanded="true"]) {
  border-color: color-mix(in srgb, var(--t-accent) 40%, transparent);
}

/* ── Unit header button ─────────────────────────────────── */
.sl-unit-hd {
  width: 100%; display: flex; align-items: center; gap: 0.9rem;
  padding: 0.9rem 1rem; background: none; border: none; cursor: pointer;
  text-align: left; transition: background 0.15s;
}
.sl-unit-hd:hover { background: var(--t-hover); }

.sl-num {
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.3rem;
  color: transparent; -webkit-text-stroke: 1.5px var(--t-accent);
  flex-shrink: 0; width: 2.2rem;
}

.sl-unit-info { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 0.1rem; }
.sl-unit-name {
  font-size: 0.925rem; font-weight: 700; color: var(--t-text1);
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.sl-unit-desc { font-size: 0.78rem; color: var(--t-text3); }

.sl-unit-meta { display: flex; align-items: center; gap: 0.4rem; flex-shrink: 0; }
.sl-badge {
  font-size: 0.68rem; font-weight: 700; padding: 0.15rem 0.5rem;
  border-radius: 99px; background: color-mix(in srgb, var(--t-accent) 12%, transparent);
  color: var(--t-accent); border: 1px solid color-mix(in srgb, var(--t-accent) 25%, transparent);
}

.sl-chev {
  width: 16px; height: 16px; color: var(--t-text3); flex-shrink: 0;
  transition: transform 0.2s ease;
}
.sl-chev.is-open { transform: rotate(180deg); color: var(--t-accent); }

/* ── Expanded body ──────────────────────────────────────── */
.sl-unit-body {
  padding: 0 1rem 1rem;
  border-top: 1px solid var(--t-border);
  display: flex; flex-direction: column; gap: 0.85rem;
  padding-top: 0.85rem;
}

.sl-section-lbl {
  display: block; font-size: 0.66rem; font-weight: 800; letter-spacing: 0.1em;
  text-transform: uppercase; color: var(--t-text3); margin-bottom: 0.45rem;
}

/* Topics */
.sl-topic-chips { display: flex; flex-wrap: wrap; gap: 0.4rem; }
.sl-topic-chip {
  padding: 0.3rem 0.7rem; border-radius: 99px;
  font-size: 0.76rem; font-weight: 600;
  color: var(--t-accent);
  background: color-mix(in srgb, var(--t-accent) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-accent) 22%, transparent);
}

/* Objectives */
.sl-obj-list {
  margin: 0; padding-left: 1.2rem;
  display: flex; flex-direction: column; gap: 0.3rem;
}
.sl-obj-list li { font-size: 0.85rem; color: var(--t-text2); line-height: 1.5; }

/* Practice link */
.sl-practice {
  display: inline-flex; align-items: center;
  font-size: 0.82rem; font-weight: 700;
  color: var(--t-accent); text-decoration: none;
  transition: opacity 0.15s;
}
.sl-practice:hover { opacity: 0.75; }

/* ── Expand animation ───────────────────────────────────── */
.sl-expand-enter-active { transition: max-height 0.25s ease, opacity 0.2s ease; overflow: hidden; max-height: 600px; }
.sl-expand-leave-active { transition: max-height 0.2s ease, opacity 0.15s ease; overflow: hidden; max-height: 600px; }
.sl-expand-enter-from, .sl-expand-leave-to { max-height: 0; opacity: 0; }
</style>
