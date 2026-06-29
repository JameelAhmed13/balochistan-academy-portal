<template>
  <div class="animate-fade-in space-y-5">

    <!-- Page Header -->
    <div class="ag-page-hd">
      <div class="ag-page-hd-left">
        <RouterLink to="/app/admin" class="btn-ghost">
          <ArrowLeft class="w-4 h-4" />
        </RouterLink>
        <h2 class="section-title">Content Management</h2>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Subject
      </button>
    </div>

    <!-- Glass Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📚</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Content Management</div>
        <div class="ag-banner-sub">Subjects, books, units, topics and resources</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">📚 {{ catalog.subjects.length }} Subjects</span>
        <span class="ag-banner-stat ag-medium-ticker">
          🌐
          <span class="ag-ticker-wrap">
            <Transition name="ticker-slide" mode="out-in">
              <span :key="tickerIndex" class="ag-ticker-item">{{ mediumCounts[tickerIndex] }}</span>
            </Transition>
          </span>
        </span>
      </div>
    </div>

    <!-- Controls Card -->
    <div class="ag-card">
      <!-- Medium filter tabs -->
      <div class="ag-tabs">
        <button
          v-for="tab in mediumTabs"
          :key="tab"
          :class="['ag-tab', { active: activeMedium === tab }]"
          @click="activeMedium = tab"
        >{{ tab }}</button>
      </div>

      <!-- Filter Bar -->
      <div class="ag-filter-bar">
        <div class="ag-filter-group" style="flex:1;min-width:200px;">
          <label class="ag-filter-label">Search</label>
          <div style="position:relative;">
            <Search
              class="w-3.5 h-3.5"
              style="position:absolute;left:0.6rem;top:50%;transform:translateY(-50%);color:var(--t-text3);pointer-events:none;"
            />
            <input
              v-model="search"
              type="text"
              class="input"
              placeholder="Search subjects…"
              style="padding-left:2rem;"
            />
          </div>
        </div>
        <div class="ag-filter-group" style="align-self:flex-end;">
          <label class="ag-filter-label">View</label>
          <div class="ag-view-toggle">
            <button
              :class="['ag-view-btn', { active: view === 'grid' }]"
              @click="view = 'grid'"
              title="Grid view"
            >
              <LayoutGrid class="w-3.5 h-3.5" />
            </button>
            <button
              :class="['ag-view-btn', { active: view === 'list' }]"
              @click="view = 'list'"
              title="List view"
            >
              <List class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      <div v-if="catalog.loading" class="ag-empty">
        <div class="ag-empty-icon">⏳</div>
        <p>Loading subjects…</p>
      </div>

      <!-- GRID VIEW -->
      <div v-else-if="view === 'grid'" class="content-grid">
        <div
          v-for="s in filteredSubjects"
          :key="s.id"
          class="ag-grid-card subject-card"
        >
          <div class="subject-icon-wrap" :class="resolveColor(s.color)">
            <span class="subject-icon">{{ resolveIcon(s.icon) }}</span>
          </div>
          <div class="subject-name">{{ s.name }}</div>
          <div class="subject-name-ur urdu">{{ s.nameUr }}</div>
          <span :class="s.medium === 'urdu' ? 'badge-amber' : 'badge-blue'" style="align-self:center;margin-top:0.15rem;">
            {{ s.medium }}
          </span>
          <div class="subject-actions">
            <RouterLink
              :to="`/app/admin/content/${s.id}/books`"
              class="btn-ghost subject-btn"
              style="flex:1;justify-content:center;"
              title="Manage books"
            >
              <BookOpen class="w-3.5 h-3.5" /> Books
              <span v-if="s.bookCount" class="subject-book-count">{{ s.bookCount }}</span>
            </RouterLink>
            <button class="btn-ghost subject-btn" @click="openEdit(s)" title="Edit subject">
              <Pencil class="w-3.5 h-3.5" />
            </button>
            <button class="btn-ghost subject-btn" style="color:#f87171;" @click="remove(s)">
              <Trash2 class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        <div v-if="filteredSubjects.length === 0" class="ag-empty" style="grid-column:1/-1;">
          <div class="ag-empty-icon">📚</div>
          <p>No subjects found matching your filters.</p>
        </div>
      </div>

      <!-- LIST VIEW -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Subject</th>
              <th>Medium</th>
              <th>Color</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(s, idx) in filteredSubjects" :key="s.id">
              <td class="ag-table-muted">{{ idx + 1 }}</td>
              <td>
                <div style="display:flex;align-items:center;gap:0.75rem;">
                  <div
                    :class="resolveColor(s.color)"
                    style="width:32px;height:32px;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:1.1rem;flex-shrink:0;"
                  >{{ resolveIcon(s.icon) }}</div>
                  <div>
                    <div style="font-weight:600;color:var(--t-text1);font-size:0.875rem;">{{ s.name }}</div>
                    <div class="ag-table-muted urdu" style="margin-top:0.1rem;">{{ s.nameUr }}</div>
                  </div>
                </div>
              </td>
              <td>
                <span :class="s.medium === 'urdu' ? 'badge-amber' : 'badge-blue'">{{ s.medium }}</span>
              </td>
              <td class="ag-table-muted" style="font-size:0.8rem;">{{ s.color || '—' }}</td>
              <td>
                <div style="display:flex;gap:0.25rem;">
                  <RouterLink
                    :to="`/app/admin/content/${s.id}/books`"
                    class="btn-ghost"
                    style="padding:0.35rem;gap:0.3rem;"
                    title="Manage books"
                  >
                    <BookOpen class="w-3.5 h-3.5" />
                    <span v-if="s.bookCount" class="subject-book-count">{{ s.bookCount }}</span>
                  </RouterLink>
                  <button class="btn-ghost" style="padding:0.35rem;" @click="openEdit(s)">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button class="btn-ghost" style="padding:0.35rem;color:#f87171;" @click="remove(s)">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredSubjects.length === 0">
              <td colspan="5">
                <div class="ag-empty">
                  <div class="ag-empty-icon">📚</div>
                  <p>No subjects found.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add / Edit Subject Modal -->
    <div v-if="form" class="cn-modal" @click.self="form = null">
      <div class="cn-dialog">
        <h2>{{ editing ? 'Edit subject' : 'Add subject' }}</h2>

        <label>
          Name (English)
          <input v-model="form.name" type="text" placeholder="e.g. Biology" />
        </label>

        <label>
          Name (Urdu)
          <input v-model="form.nameUr" type="text" class="urdu" style="text-align:right;" placeholder="حیاتیات" />
        </label>

        <label>
          Medium
          <div class="cn-medium-row">
            <select v-model="form.medium" class="cn-medium-select">
              <option value="">— select medium —</option>
              <option v-for="m in catalog.mediums" :key="m.id" :value="m.name">{{ m.label }}</option>
            </select>
            <button type="button" class="cn-refresh" @click="catalog.fetchMediums()" title="Refresh mediums">↺</button>
          </div>
        </label>

        <label>
          Color
          <div class="cn-color-row">
            <button
              v-for="c in COLOR_OPTIONS"
              :key="c.value"
              type="button"
              :class="['cn-color-btn', c.value, { selected: form.color === c.value }]"
              :title="c.label"
              @click="form.color = c.value"
            >
              <span v-if="form.color === c.value">✓</span>
            </button>
          </div>
        </label>

        <label>
          Icon (emoji)
          <div class="cn-icon-picker">
            <button
              v-for="em in EMOJI_OPTIONS"
              :key="em"
              type="button"
              :class="['cn-icon-btn', { selected: form.icon === em }]"
              @click="form.icon = em"
            >{{ em }}</button>
          </div>
        </label>

        <p v-if="err" class="cn-err">{{ err }}</p>

        <div class="cn-dialog-foot">
          <button class="btn-secondary" @click="form = null">Cancel</button>
          <button class="btn-primary" :disabled="saving" @click="save">
            {{ saving ? 'Saving…' : (editing ? 'Save changes' : 'Add subject') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { ArrowLeft, Plus, Pencil, Trash2, LayoutGrid, List, Search, BookOpen } from '@lucide/vue'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'

const catalog = useCatalogStore()
const confirm = useConfirm()

const search        = ref('')
const view          = ref('grid')
const activeMedium  = ref('All')
const form          = ref(null)
const editing       = ref(false)
const saving        = ref(false)
const err           = ref('')

const mediumTabs = computed(() =>
  ['All', ...catalog.mediums.map(m => m.name)]
)

const COLOR_OPTIONS = [
  { label: 'Blue',    value: 'grad-blue'    },
  { label: 'Violet',  value: 'grad-violet'  },
  { label: 'Emerald', value: 'grad-emerald' },
  { label: 'Green',   value: 'grad-green'   },
  { label: 'Amber',   value: 'grad-amber'   },
  { label: 'Rose',    value: 'grad-rose'    },
  { label: 'Teal',    value: 'grad-teal'    },
  { label: 'Orange',  value: 'grad-orange'  },
  { label: 'Cyan',    value: 'grad-cyan'    },
  { label: 'Pink',    value: 'grad-pink'    },
]

const EMOJI_OPTIONS = ['📚', '✍️', '🧬', '⚗️', '⚡', '📐', '🏛️', '🌍', '🌙', '💻', '📖', '🔬', '🗺️', '🎓', '🧪', '🌿', '🧮', '⚛️', '🖥️', '✒️']

// ── Icon / color normalizers (handles legacy Lucide names and hex colors from the seeder) ──
const LUCIDE_TO_EMOJI = {
  'calculator':     '🧮',
  'atom':           '⚛️',
  'flask':          '⚗️',
  'leaf':           '🌿',
  'book-open':      '📖',
  'pen':            '✒️',
  'moon':           '🌙',
  'map':            '🗺️',
  'monitor':        '🖥️',
  'microscope':     '🔬',
  'graduation-cap': '🎓',
}

const HEX_TO_GRAD = {
  '#3B82F6': 'grad-blue',
  '#8B5CF6': 'grad-violet',
  '#10B981': 'grad-emerald',
  '#22C55E': 'grad-green',
  '#F59E0B': 'grad-amber',
  '#EF4444': 'grad-rose',
  '#14B8A6': 'grad-teal',
  '#F97316': 'grad-orange',
  '#6366F1': 'grad-violet',
  '#0EA5E9': 'grad-cyan',
}

function resolveIcon(icon) {
  if (!icon) return '📚'
  return LUCIDE_TO_EMOJI[icon] ?? icon
}

function resolveColor(color) {
  if (!color) return 'grad-blue'
  return HEX_TO_GRAD[color] ?? color
}

const mediumCounts = computed(() =>
  catalog.mediums.map(m => `${catalog.subjects.filter(s => s.medium === m.name).length} ${m.label}`)
)

const tickerIndex = ref(0)
let tickerTimer = null
function startTicker() {
  tickerTimer = setInterval(() => {
    if (mediumCounts.value.length > 1)
      tickerIndex.value = (tickerIndex.value + 1) % mediumCounts.value.length
  }, 2500)
}
onUnmounted(() => clearInterval(tickerTimer))

const filteredSubjects = computed(() => {
  let list = catalog.subjects
  if (activeMedium.value !== 'All') {
    list = list.filter(s => s.medium === activeMedium.value)
  }
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(s =>
      s.name?.toLowerCase().includes(q) || s.nameUr?.includes(search.value)
    )
  }
  return list
})

onMounted(() => {
  Promise.all([catalog.fetchAllSubjects(), catalog.fetchMediums()])
  startTicker()
})

function openCreate() {
  editing.value = false
  err.value = ''
  form.value = { name: '', nameUr: '', icon: '📚', color: 'grad-blue', medium: 'english' }
}

function openEdit(s) {
  editing.value = true
  err.value = ''
  form.value = {
    id:     s.id,
    name:   s.name,
    nameUr: s.nameUr ?? '',
    icon:   resolveIcon(s.icon),
    color:  resolveColor(s.color),
    medium: s.medium,
  }
}

async function save() {
  if (!form.value.name.trim()) { err.value = 'Name is required'; return }

  saving.value = true; err.value = ''
  try {
    const payload = {
      name:   form.value.name.trim(),
      nameUr: form.value.nameUr,
      icon:   form.value.icon,
      color:  form.value.color,
      medium: form.value.medium,
    }
    if (editing.value) {
      await catalog.updateSubject(form.value.id, payload)
    } else {
      await catalog.createSubject(payload)
    }
    form.value = null
  } catch (e) {
    err.value = e.response?.data?.error || e.message
  } finally {
    saving.value = false
  }
}

async function remove(s) {
  const ok = await confirm({
    title: `Delete "${s.name}"`,
    message: 'This will permanently remove the subject and all its syllabus content.',
    confirmLabel: 'Delete Subject',
  })
  if (!ok) return
  try {
    await catalog.deleteSubject(s.id)
  } catch (e) {
    alert(e.response?.data?.error || e.message)
  }
}
</script>

<style scoped>
/* ── Grid Layout ── */
.content-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 1rem;
  padding: 1.25rem 1.5rem;
}

/* ── Subject Card ── */
.subject-card {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  cursor: default;
}
.subject-icon-wrap {
  width: 56px; height: 56px; border-radius: 16px;
  display: flex; align-items: center; justify-content: center;
  align-self: center;
}
.subject-icon { font-size: 1.75rem; }

.subject-name {
  font-size: 0.875rem; font-weight: 700; color: var(--t-text1);
  text-align: center; line-height: 1.3;
}
.subject-name-ur {
  font-size: 0.78rem; color: var(--t-text3);
  text-align: center; margin-top: -0.25rem;
}

.subject-actions {
  display: flex; gap: 0.35rem; margin-top: 0.25rem;
}
.subject-btn {
  font-size: 0.75rem; padding: 0.35rem 0.5rem;
}
.subject-book-count {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 18px; height: 18px; padding: 0 5px;
  border-radius: 999px; font-size: 0.65rem; font-weight: 700; line-height: 1;
  background: var(--t-accent); color: #fff;
}

/* ── Modal ── */
.cn-modal {
  position: fixed; inset: 0; z-index: 100;
  display: grid; place-items: center;
  background: rgba(0,0,0,.5);
  padding: 20px;
  backdrop-filter: blur(4px);
}
.cn-dialog {
  width: min(500px, 100%); max-height: 92vh; overflow-y: auto;
  padding: 28px; border-radius: 18px;
  background: var(--t-bg2); border: 1px solid var(--t-border);
  display: flex; flex-direction: column; gap: 14px;
  box-shadow: var(--t-shadow-md);
}
.cn-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; color: var(--t-text1); }
.cn-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.cn-dialog input, .cn-dialog select {
  padding: 10px 12px; border-radius: 10px;
  border: 1px solid var(--t-border);
  background: var(--t-bg); color: var(--t-text1); font-size: .9rem; font: inherit;
}
.cn-dialog input:focus, .cn-dialog select:focus {
  outline: none; border-color: var(--t-accent);
  box-shadow: 0 0 0 3px var(--t-acc-alpha-sm);
}
.cn-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
.cn-err { color: #f87171; font-size: .85rem; margin: 0; }

/* ── Color picker ── */
.cn-color-row { display: flex; flex-wrap: wrap; gap: 8px; padding: 4px 0; }
.cn-color-btn {
  width: 36px; height: 36px; border-radius: 10px;
  border: 2px solid transparent;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; font-size: 1rem; color: #fff; font-weight: 700;
  transition: border-color .15s, transform .1s;
}
.cn-color-btn:hover { transform: scale(1.08); }
.cn-color-btn.selected { border-color: var(--t-text1); }

/* ── Medium ticker ── */
.ag-medium-ticker { display: flex; align-items: center; gap: 5px; }
.ag-ticker-wrap { display: inline-block; overflow: hidden; height: 1.25em; min-width: 140px; position: relative; }
.ag-ticker-item { display: inline-block; white-space: nowrap; }

.ticker-slide-enter-active { transition: transform 0.35s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.35s ease; }
.ticker-slide-leave-active { transition: transform 0.28s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.28s ease; position: absolute; }
.ticker-slide-enter-from   { transform: translateY(100%); opacity: 0; }
.ticker-slide-enter-to     { transform: translateY(0);    opacity: 1; }
.ticker-slide-leave-from   { transform: translateY(0);    opacity: 1; }
.ticker-slide-leave-to     { transform: translateY(-100%); opacity: 0; }

/* ── Medium row ── */
.cn-medium-row { display: flex; gap: 8px; align-items: center; }
.cn-medium-select { flex: 1; padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); font-size: .9rem; }
.cn-medium-select:focus { outline: none; border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-acc-alpha-sm); }
.cn-refresh { flex-shrink: 0; width: 36px; height: 36px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); cursor: pointer; font-size: 1.1rem; display: flex; align-items: center; justify-content: center; transition: all .15s; }
.cn-refresh:hover { border-color: var(--t-accent); color: var(--t-accent); }

/* ── Icon picker ── */
.cn-icon-picker { display: flex; flex-wrap: wrap; gap: 0.35rem; padding: 0.5rem 0; }
.cn-icon-btn {
  width: 38px; height: 38px; border-radius: 10px; font-size: 1.1rem;
  display: flex; align-items: center; justify-content: center;
  background: var(--t-hover); border: 1.5px solid var(--t-border);
  cursor: pointer; transition: all 0.15s;
}
.cn-icon-btn:hover { background: var(--t-hover2); border-color: var(--t-accent); }
.cn-icon-btn.selected {
  background: var(--t-acc-alpha-md);
  border-color: var(--t-accent);
  box-shadow: 0 0 0 2px var(--t-acc-alpha-sm);
}
</style>
