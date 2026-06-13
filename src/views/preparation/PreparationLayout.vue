<template>
  <div class="pl-shell">

    <!-- ═══ PERSISTENT LEFT SIDEBAR ═══ -->
    <aside class="pl-sidebar" aria-label="Preparation navigation">

      <!-- Grade / Class Selector -->
      <div class="pl-grade-section">
        <span class="pl-sec-lbl">Class</span>
        <div class="pl-grade-grid">
          <button v-for="g in grades" :key="g"
            type="button"
            @click="selectGrade(g)"
            :class="['pl-gtab', selectedGrade === g ? 'is-on' : '']"
            :aria-pressed="selectedGrade === g"
            :aria-label="'Grade ' + g">
            {{ g }}
          </button>
        </div>
      </div>

      <!-- Library / Book List -->
      <div class="pl-lib-section">
        <div class="pl-lib-hd">
          <span class="pl-sec-lbl">Library</span>
          <div class="pl-medium-tabs">
            <button v-for="t in mediumTabs" :key="t.key" type="button"
              @click="mediumFilter = t.key"
              :class="['pl-mtab', mediumFilter === t.key ? 'is-on' : '']">
              {{ t.label }}
            </button>
          </div>
        </div>
        <nav class="pl-book-list" aria-label="Subject books">
          <button v-for="subj in filteredSubjects" :key="subj.id"
            type="button"
            @click="selectBook(subj)"
            :class="['pl-book-row', selectedBook?.id === subj.id ? 'is-active' : '']"
            :aria-pressed="selectedBook?.id === subj.id"
            :aria-label="subj.name">
            <span class="pl-bicon" :class="subj.color" aria-hidden="true">{{ subj.icon }}</span>
            <span class="pl-binfo">
              <span class="pl-bname">{{ subj.name }}</span>
              <span class="pl-bsub">{{ subj.medium === 'urdu' ? 'Urdu' : 'English' }}</span>
            </span>
            <ChevronRight v-if="selectedBook?.id === subj.id" class="pl-bchev" aria-hidden="true" />
          </button>
        </nav>
      </div>

    </aside>

    <!-- ═══ RIGHT: ROUTED CONTENT ═══ -->
    <div class="pl-content">

      <!-- Mobile controls (hidden on desktop) -->
      <div class="pl-mobile-controls">
        <!-- Grade row -->
        <div class="pl-mob-row">
          <button v-for="g in grades" :key="g" type="button"
            @click="selectGrade(g)"
            :class="['pl-mob-gtab', selectedGrade === g ? 'is-on' : '']">
            Class {{ g }}
          </button>
        </div>
        <!-- Books row -->
        <div class="pl-mob-scroll">
          <button v-for="subj in filteredSubjects" :key="subj.id" type="button"
            @click="selectBook(subj)"
            :class="['pl-mob-chip', selectedBook?.id === subj.id ? 'is-on' : '']">
            <span aria-hidden="true">{{ subj.icon }}</span>
            <span>{{ subj.name.split(' ')[0] }}</span>
          </button>
          <span class="pl-mob-div" aria-hidden="true">|</span>
          <button v-for="t in mediumTabs" :key="'mt'+t.key" type="button"
            @click="mediumFilter = t.key"
            :class="['pl-mob-chip pl-mob-chip-sm', mediumFilter === t.key ? 'is-filter' : '']">
            {{ t.label }}
          </button>
        </div>
        <!-- Options row -->
        <div v-if="selectedBook" class="pl-mob-scroll pl-mob-opts">
          <button v-for="opt in studyOptions" :key="opt.action" type="button"
            @click="handleOption(opt)"
            :class="['pl-mob-optchip', activeOption === opt.action ? 'is-on' : '']">
            <component :is="opt.icon" class="w-3 h-3" aria-hidden="true" />
            {{ opt.title }}
          </button>
        </div>
      </div>

      <!-- Desktop study-options bar — sits at the top of the workspace,
           not buried below the library, so options are always one click away -->
      <nav v-if="selectedBook" class="pl-optbar" aria-label="Study options">
        <button v-for="opt in studyOptions" :key="opt.action"
          type="button"
          @click="handleOption(opt)"
          :class="['pl-optbar-pill', activeOption === opt.action ? 'is-active' : '']"
          :aria-pressed="activeOption === opt.action"
          :aria-label="opt.title">
          <component :is="opt.icon" class="pl-optbar-svg" aria-hidden="true" />
          <span>{{ opt.title }}</span>
        </button>
      </nav>

      <!-- Page content via nested router -->
      <RouterView v-slot="{ Component, route: cr }">
        <Transition name="pl-fade" mode="out-in">
          <component :is="Component" :key="cr.path" />
        </Transition>
      </RouterView>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  ChevronRight, ArrowRight,
  FolderOpen, Video, FileText, FlaskConical,
  TestTube2, CheckSquare, PenLine, FileStack,
} from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'

const router = useRouter()
const route  = useRoute()
const auth = useAuthStore()
const catalog = useCatalogStore()

// ── Grades ────────────────────────────────────────────────────
// A student is locked to their own grade; an admin (no grade) may browse all.
const isStudent = computed(() => auth.user?.role === 'student')
const grades = computed(() => {
  if (isStudent.value && auth.user?.gradeCode) return [auth.user.gradeCode]
  const all = catalog.enabledGrades.map((g) => g.code)
  return all.length ? all : [auth.user?.gradeCode || '9']
})
const selectedGrade = ref(auth.user?.gradeCode || '9')

async function ensureSubjects(code) {
  if (code && !catalog.subjectsForGrade(code).length) {
    try { await catalog.fetchSubjectsForGrade(code) } catch { /* backend offline */ }
  }
}

// Preserve the active study-option suffix (e.g. "/videos", "/test/objective")
// when switching subject or grade, so students stay on their current option.
function currentSuffix() {
  const m = route.path.match(/^\/app\/preparation\/[^/]+(\/.*)?$/)
  return m && m[1] ? m[1] : ''
}

async function selectGrade(g) {
  selectedGrade.value = g
  await ensureSubjects(g)
  selectedBook.value = filteredSubjects.value[0] ?? null
  if (selectedBook.value) {
    router.replace(`/app/preparation/${selectedBook.value.id}${currentSuffix()}`)
  }
}

// ── Medium filter ──────────────────────────────────────────────
const mediumFilter = ref('All')
const mediumTabs = [
  { key: 'All',     label: 'All' },
  { key: 'english', label: 'En' },
  { key: 'urdu',    label: 'Ur' },
]

const filteredSubjects = computed(() => {
  const list = catalog.subjectsForGrade(selectedGrade.value) || []
  if (mediumFilter.value === 'All') return list
  return list.filter((s) => (s.medium || '').toLowerCase() === mediumFilter.value)
})

// ── Selected book ──────────────────────────────────────────────
const selectedBook = ref(null)

function selectBook(subj) {
  selectedBook.value = subj
  router.push(`/app/preparation/${subj.id}${currentSuffix()}`)
}

onMounted(async () => {
  if (!catalog.grades.length) { try { await catalog.fetchGrades() } catch { /* offline */ } }
  await ensureSubjects(selectedGrade.value)
  const id = route.params.bookId
  selectedBook.value =
    (id && filteredSubjects.value.find((s) => String(s.id) === String(id))) ||
    filteredSubjects.value[0] ||
    null
})

watch(() => route.params.bookId, (id) => {
  if (!id) return
  const found = filteredSubjects.value.find((s) => String(s.id) === String(id))
  if (found) selectedBook.value = found
})

// ── Study options ──────────────────────────────────────────────
const studyOptions = [
  { title: 'Academic Resources',  action: 'resources',   icon: FolderOpen,   bg: 'rgba(59,130,246,0.15)'  },
  { title: 'Video Lectures',      action: 'videos',      icon: Video,        bg: 'rgba(124,58,237,0.15)'  },
  { title: 'Key Notes',           action: 'keynotes',    icon: FileText,     bg: 'rgba(20,184,166,0.15)'  },
  { title: 'Simulations',         action: 'simulations', icon: FlaskConical, bg: 'rgba(16,185,129,0.15)'  },
  { title: 'Virtual Lab',         action: 'vlab',        icon: TestTube2,    bg: 'rgba(244,63,94,0.15)'   },
  { title: 'Objective Paper',     action: 'objective',   icon: CheckSquare,  bg: 'rgba(245,158,11,0.15)'  },
  { title: 'Subjective Paper',    action: 'subjective',  icon: PenLine,      bg: 'rgba(236,72,153,0.15)'  },
  { title: 'Past & Model Papers', action: 'pastpapers',  icon: FileStack,    bg: 'rgba(249,115,22,0.15)'  },
]

const activeOption = computed(() => {
  const p = route.path
  if (p.includes('/test/objective')  || p.endsWith('/objective'))  return 'objective'
  if (p.includes('/test/subjective') || p.endsWith('/subjective')) return 'subjective'
  if (p.endsWith('/resources'))   return 'resources'
  if (p.endsWith('/videos'))      return 'videos'
  if (p.endsWith('/keynotes'))    return 'keynotes'
  if (p.endsWith('/simulations')) return 'simulations'
  if (p.endsWith('/vlab'))        return 'vlab'
  if (p.endsWith('/pastpapers'))  return 'pastpapers'
  return null
})

function handleOption(opt) {
  const id = selectedBook.value?.id
  if (!id) return
  // Every option resolves to a nested route inside this layout — the sidebar
  // never disappears, so students switch options without page-hopping.
  router.push(`/app/preparation/${id}/${opt.action}`)
}
</script>

<style scoped>
/* ─── Shell ─────────────────────────────────────────────── */
.pl-shell {
  display: flex;
  align-items: flex-start;
  margin: -1rem;
  min-height: calc(100vh - 64px);
}
@media (min-width: 768px) { .pl-shell { margin: -1.5rem; } }

/* ─── Sidebar ───────────────────────────────────────────── */
.pl-sidebar {
  width: 236px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  background: var(--t-surface);
  border-right: 1px solid var(--t-border);
  position: sticky;
  top: 0;
  align-self: flex-start;
  max-height: calc(100vh - 64px);
  overflow-y: auto;
  overflow-x: hidden;
  scrollbar-width: thin;
  scrollbar-color: var(--t-border) transparent;
  transition: background 0.25s, border-color 0.25s;
}

/* Section label */
.pl-sec-lbl {
  font-size: 0.65rem;
  font-weight: 800;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--t-text3);
  font-family: 'Syne', sans-serif;
}

/* ── Grade section ── */
.pl-grade-section {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 0.875rem;
  border-bottom: 1px solid var(--t-border);
  flex-shrink: 0;
  gap: 0.5rem;
}
.pl-grade-grid {
  display: flex;
  gap: 3px;
}
.pl-gtab {
  width: 30px;
  height: 26px;
  border-radius: 6px;
  border: 1px solid var(--t-border);
  background: var(--t-hover);
  color: var(--t-text3);
  font-size: 0.75rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.pl-gtab:hover { border-color: var(--t-accent); color: var(--t-accent); }
.pl-gtab.is-on {
  background: var(--t-accent);
  color: white;
  border-color: var(--t-accent);
  box-shadow: 0 2px 6px color-mix(in srgb, var(--t-accent) 35%, transparent);
}

/* ── Library section ── */
.pl-lib-section {
  display: flex;
  flex-direction: column;
  border-bottom: 1px solid var(--t-border);
}
.pl-lib-hd {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.65rem 0.875rem 0.5rem;
  flex-shrink: 0;
}
.pl-medium-tabs {
  display: flex;
  gap: 2px;
  background: var(--t-hover);
  border-radius: 6px;
  padding: 2px;
}
.pl-mtab {
  padding: 0.15rem 0.4rem;
  border-radius: 4px;
  font-size: 0.65rem;
  font-weight: 700;
  color: var(--t-text3);
  background: none;
  border: none;
  cursor: pointer;
  transition: all 0.15s;
  line-height: 1.2;
}
.pl-mtab.is-on { background: var(--t-accent); color: white; }

.pl-book-list {
  padding: 0.3rem 0.4rem 0.5rem;
  display: flex;
  flex-direction: column;
  gap: 1px;
}
.pl-book-row {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 0.55rem;
  padding: 0.5rem 0.6rem;
  border-radius: 8px;
  border: 1px solid transparent;
  background: none;
  cursor: pointer;
  transition: all 0.15s;
  text-align: left;
}
.pl-book-row:hover { background: var(--t-hover); }
.pl-book-row.is-active {
  background: color-mix(in srgb, var(--t-accent) 10%, transparent);
  border-color: color-mix(in srgb, var(--t-accent) 28%, transparent);
}
.pl-bicon {
  width: 28px;
  height: 34px;
  border-radius: 5px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.95rem;
  flex-shrink: 0;
}
.pl-binfo {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 1px;
}
.pl-bname {
  font-size: 0.76rem;
  font-weight: 600;
  color: var(--t-text1);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  display: block;
}
.pl-bsub {
  font-size: 0.62rem;
  color: var(--t-text3);
  display: block;
}
.pl-bchev {
  width: 12px;
  height: 12px;
  color: var(--t-accent);
  flex-shrink: 0;
}

/* ── Study Options section ── */
.pl-opts-section { flex-shrink: 0; padding-bottom: 0.5rem; }
.pl-opts-hd { padding: 0.65rem 0.875rem 0.3rem; }

.pl-opt-row {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.45rem 0.6rem;
  margin: 0 0.4rem;
  width: calc(100% - 0.8rem);
  border-radius: 8px;
  border: 1px solid transparent;
  background: none;
  cursor: pointer;
  transition: all 0.15s;
  text-align: left;
}
.pl-opt-row:hover { background: var(--t-hover); }
.pl-opt-row.is-active {
  background: color-mix(in srgb, var(--t-accent) 10%, transparent);
  border-color: color-mix(in srgb, var(--t-accent) 28%, transparent);
}
.pl-opt-icon {
  width: 26px;
  height: 26px;
  border-radius: 7px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.pl-opt-svg { width: 13px; height: 13px; color: var(--t-text2); }
.pl-opt-row.is-active .pl-opt-svg { color: var(--t-accent); }
.pl-opt-label {
  flex: 1;
  font-size: 0.76rem;
  font-weight: 500;
  color: var(--t-text2);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.pl-opt-row.is-active .pl-opt-label { color: var(--t-accent); font-weight: 600; }
.pl-opt-arr { width: 12px; height: 12px; color: var(--t-accent); flex-shrink: 0; }

/* ─── Content Panel ─────────────────────────────────────── */
.pl-content {
  flex: 1;
  min-width: 0;
  padding: 1.25rem 1.5rem;
  overflow-y: auto;
  min-height: calc(100vh - 64px);
}

/* ─── Desktop study-options bar (top of workspace) ──────── */
.pl-optbar { display: none; }
@media (min-width: 768px) {
  .pl-optbar {
    display: flex;
    flex-wrap: wrap;
    gap: 0.4rem;
    padding-bottom: 1rem;
    margin-bottom: 1.25rem;
    border-bottom: 1px solid var(--t-border);
  }
}
.pl-optbar-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.42rem 0.85rem;
  border-radius: 99px;
  border: 1px solid var(--t-border);
  background: var(--t-surface);
  color: var(--t-text2);
  font-size: 0.78rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
  white-space: nowrap;
}
.pl-optbar-pill:hover { border-color: var(--t-accent); color: var(--t-accent); }
.pl-optbar-pill.is-active {
  background: color-mix(in srgb, var(--t-accent) 12%, transparent);
  border-color: color-mix(in srgb, var(--t-accent) 40%, transparent);
  color: var(--t-accent);
  box-shadow: 0 2px 8px color-mix(in srgb, var(--t-accent) 14%, transparent);
}
.pl-optbar-svg { width: 14px; height: 14px; flex-shrink: 0; }

/* ─── Mobile controls (desktop: hidden) ─────────────────── */
.pl-mobile-controls { display: none; }
.pl-mob-row {
  display: flex;
  gap: 0.4rem;
  margin-bottom: 0.5rem;
  flex-wrap: wrap;
}
.pl-mob-gtab {
  padding: 0.3rem 0.7rem;
  border-radius: 6px;
  border: 1px solid var(--t-border);
  background: var(--t-surface);
  color: var(--t-text3);
  font-size: 0.72rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
}
.pl-mob-gtab.is-on { background: var(--t-accent); color: white; border-color: var(--t-accent); }
.pl-mob-scroll {
  display: flex;
  gap: 0.4rem;
  overflow-x: auto;
  padding-bottom: 0.5rem;
  scrollbar-width: none;
  align-items: center;
  margin-bottom: 0.4rem;
}
.pl-mob-scroll::-webkit-scrollbar { display: none; }
.pl-mob-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  padding: 0.3rem 0.65rem;
  border-radius: 99px;
  border: 1px solid var(--t-border);
  background: var(--t-surface);
  color: var(--t-text2);
  font-size: 0.7rem;
  font-weight: 500;
  white-space: nowrap;
  cursor: pointer;
  flex-shrink: 0;
  transition: all 0.15s;
}
.pl-mob-chip.is-on { background: var(--t-accent); color: white; border-color: var(--t-accent); }
.pl-mob-chip-sm { font-size: 0.64rem; padding: 0.22rem 0.5rem; }
.pl-mob-chip.is-filter {
  background: color-mix(in srgb, var(--t-accent) 14%, transparent);
  color: var(--t-accent);
  border-color: color-mix(in srgb, var(--t-accent) 35%, transparent);
  font-weight: 700;
}
.pl-mob-div { color: var(--t-border); flex-shrink: 0; padding: 0 0.1rem; }
.pl-mob-opts { flex-wrap: nowrap; }
.pl-mob-optchip {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  padding: 0.28rem 0.6rem;
  border-radius: 99px;
  border: 1px solid var(--t-border);
  background: var(--t-surface);
  color: var(--t-text2);
  font-size: 0.68rem;
  font-weight: 500;
  white-space: nowrap;
  cursor: pointer;
  flex-shrink: 0;
  transition: all 0.15s;
}
.pl-mob-optchip.is-on {
  background: color-mix(in srgb, var(--t-accent) 14%, transparent);
  color: var(--t-accent);
  border-color: var(--t-accent);
  font-weight: 700;
}

/* ─── Responsive ────────────────────────────────────────── */
@media (max-width: 767px) {
  .pl-shell { flex-direction: column; margin: -1rem; }
  .pl-sidebar { display: none; }
  .pl-mobile-controls {
    display: block;
    padding: 0.75rem 1rem 0;
    border-bottom: 1px solid var(--t-border);
    margin-bottom: 1rem;
    background: var(--t-surface);
  }
  .pl-content { padding: 1rem; min-height: unset; }
}
@media (min-width: 768px) and (max-width: 1024px) {
  .pl-sidebar { width: 200px; }
}

/* ─── Route transition ──────────────────────────────────── */
.pl-fade-enter-active, .pl-fade-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.pl-fade-enter-from, .pl-fade-leave-to {
  opacity: 0;
  transform: translateY(4px);
}

@media (prefers-reduced-motion: reduce) {
  .pl-fade-enter-active, .pl-fade-leave-active { transition: none; }
  .pl-book-row, .pl-opt-row, .pl-gtab, .pl-mob-chip, .pl-mob-optchip { transition: none; }
}
</style>
