<template>
  <div class="animate-fade-in space-y-5">

    <!-- Page Header -->
    <div class="ag-page-hd">
      <div class="ag-page-hd-left">
        <RouterLink to="/app/admin/content" class="btn-ghost">
          <ArrowLeft class="w-4 h-4" />
        </RouterLink>
        <div>
          <h2 class="section-title">Books</h2>
          <p v-if="subject" class="bk-subject-label">{{ subject.name }}</p>
        </div>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Book
      </button>
    </div>

    <!-- Glass Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📖</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">{{ subject?.name ?? 'Subject' }} — Books</div>
        <div class="ag-banner-sub">Manage textbooks, reference books and study materials</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">📖 {{ books.length }} Book{{ books.length !== 1 ? 's' : '' }}</span>
      </div>
    </div>

    <!-- Controls Card -->
    <div class="ag-card">
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
              placeholder="Search books…"
              style="padding-left:2rem;"
            />
          </div>
        </div>
        <div class="ag-filter-group" style="align-self:flex-end;">
          <label class="ag-filter-label">View</label>
          <div class="ag-view-toggle">
            <button :class="['ag-view-btn', { active: view === 'grid' }]" @click="view = 'grid'" title="Grid view">
              <LayoutGrid class="w-3.5 h-3.5" />
            </button>
            <button :class="['ag-view-btn', { active: view === 'list' }]" @click="view = 'list'" title="List view">
              <List class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      <div v-if="loading" class="ag-empty">
        <div class="ag-empty-icon">⏳</div>
        <p>Loading books…</p>
      </div>

      <!-- GRID VIEW -->
      <div v-else-if="view === 'grid'" class="bk-grid">
        <div v-for="b in filtered" :key="b.id" class="ag-grid-card bk-card">
          <div class="bk-cover" :style="b.coverUrl ? `background-image:url(${b.coverUrl})` : ''">
            <span v-if="!b.coverUrl" class="bk-cover-icon">📖</span>
            <span v-if="!b.isActive" class="bk-inactive-badge">Inactive</span>
          </div>
          <div class="bk-info">
            <div class="bk-title">{{ b.title }}</div>
            <div v-if="b.titleUr" class="bk-title-ur urdu">{{ b.titleUr }}</div>
            <div class="bk-meta">
              <span v-if="b.author" class="bk-meta-item">✍️ {{ b.author }}</span>
              <span v-if="b.edition" class="bk-meta-item">📅 {{ b.edition }}</span>
              <span v-if="b.board" class="bk-meta-item">🏛️ {{ b.board }}</span>
            </div>
            <div style="display:flex;gap:0.35rem;flex-wrap:wrap;margin-top:0.25rem;">
              <span v-if="b.medium" :class="b.medium === 'urdu' ? 'badge-amber' : 'badge-blue'">{{ b.medium }}</span>
              <span v-if="b.gradeCode" class="badge-blue">Grade {{ b.gradeCode }}</span>
            </div>
          </div>
          <div class="bk-actions">
            <RouterLink
              :to="`/app/admin/content/${subjectId}/books/${b.id}/units`"
              class="btn-ghost bk-btn"
              style="flex:1;justify-content:center;"
              title="Manage units"
            >
              <BookMarked class="w-3.5 h-3.5" /> Units
              <span class="bk-unit-count">{{ b.unitCount ?? 0 }}</span>
            </RouterLink>
            <a v-if="b.downloadUrl" :href="b.downloadUrl" target="_blank" class="btn-ghost bk-btn" title="Open book">
              <ExternalLink class="w-3.5 h-3.5" />
            </a>
            <button class="btn-ghost bk-btn" @click="openEdit(b)" title="Edit">
              <Pencil class="w-3.5 h-3.5" />
            </button>
            <button class="btn-ghost bk-btn" style="color:#f87171;" @click="remove(b)">
              <Trash2 class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        <div v-if="filtered.length === 0" class="ag-empty" style="grid-column:1/-1;">
          <div class="ag-empty-icon">📖</div>
          <p>No books found.</p>
        </div>
      </div>

      <!-- LIST VIEW -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Title</th>
              <th>Author</th>
              <th>Edition</th>
              <th>Board</th>
              <th>Medium</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(b, idx) in filtered" :key="b.id">
              <td class="ag-table-muted">{{ idx + 1 }}</td>
              <td>
                <div style="font-weight:600;color:var(--t-text1);font-size:0.875rem;">{{ b.title }}</div>
                <div v-if="b.titleUr" class="ag-table-muted urdu" style="margin-top:0.1rem;">{{ b.titleUr }}</div>
              </td>
              <td class="ag-table-muted">{{ b.author || '—' }}</td>
              <td class="ag-table-muted">{{ b.edition || '—' }}</td>
              <td class="ag-table-muted">{{ b.board || '—' }}</td>
              <td>
                <span v-if="b.medium" :class="b.medium === 'urdu' ? 'badge-amber' : 'badge-blue'">{{ b.medium }}</span>
                <span v-else class="ag-table-muted">—</span>
              </td>
              <td>
                <span :class="b.isActive ? 'badge-blue' : 'badge-amber'">{{ b.isActive ? 'Active' : 'Inactive' }}</span>
              </td>
              <td>
                <div style="display:flex;gap:0.25rem;">
                  <RouterLink
                    :to="`/app/admin/content/${subjectId}/books/${b.id}/units`"
                    class="btn-ghost"
                    style="padding:0.35rem;gap:0.3rem;"
                    title="Manage units"
                  >
                    <BookMarked class="w-3.5 h-3.5" />
                    <span class="bk-unit-count">{{ b.unitCount ?? 0 }}</span>
                  </RouterLink>
                  <a v-if="b.downloadUrl" :href="b.downloadUrl" target="_blank" class="btn-ghost" style="padding:0.35rem;" title="Open book">
                    <ExternalLink class="w-3.5 h-3.5" />
                  </a>
                  <button class="btn-ghost" style="padding:0.35rem;" @click="openEdit(b)">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button class="btn-ghost" style="padding:0.35rem;color:#f87171;" @click="remove(b)">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filtered.length === 0">
              <td colspan="8">
                <div class="ag-empty">
                  <div class="ag-empty-icon">📖</div>
                  <p>No books found.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add / Edit Book Modal -->
    <div v-if="form" class="bk-modal" @click.self="form = null">
      <div class="bk-dialog">
        <h2>{{ editing ? 'Edit book' : 'Add book' }}</h2>

        <label>
          Title (English) <span class="bk-required">*</span>
          <input v-model="form.title" type="text" placeholder="e.g. Biology Part I" />
        </label>

        <label>
          Title (Urdu)
          <input v-model="form.titleUr" type="text" class="urdu" style="text-align:right;" placeholder="حیاتیات حصہ اول" />
        </label>

        <div class="bk-row">
          <label style="flex:1;">
            Author
            <input v-model="form.author" type="text" placeholder="e.g. Prof. Abdul Rauf" />
          </label>
          <label style="flex:1;">
            Publisher
            <input v-model="form.publisher" type="text" placeholder="e.g. Punjab Textbook Board" />
          </label>
        </div>

        <div class="bk-row">
          <label style="flex:1;">
            Edition / Year
            <input v-model="form.edition" type="text" placeholder="e.g. 2024" />
          </label>
          <label style="flex:1;">
            Board
            <input v-model="form.board" type="text" placeholder="e.g. BISE Quetta" />
          </label>
        </div>

        <div class="bk-row">
          <label style="flex:1;">
            Grade
            <select v-model="form.gradeCode">
              <option value="">— all grades —</option>
              <option v-for="g in subjectGrades" :key="g.code" :value="g.code">{{ g.label }}</option>
            </select>
            <span v-if="!subjectGrades.length" class="bk-hint">No grades linked to this subject yet</span>
          </label>
          <label style="flex:1;">
            Medium
            <div class="bk-medium-display">
              <span :class="['bk-medium-badge', form.medium === 'urdu' ? 'badge-amber' : 'badge-blue']">
                {{ form.medium || '—' }}
              </span>
              <span class="bk-medium-hint">inherited from subject</span>
            </div>
          </label>
        </div>

        <label>
          Cover Image URL
          <input v-model="form.coverUrl" type="url" placeholder="https://…" />
        </label>

        <label>
          Download / Read URL
          <input v-model="form.downloadUrl" type="url" placeholder="https://…/book.pdf" />
        </label>

        <label>
          Description
          <textarea v-model="form.description" rows="3" placeholder="Optional short description…" />
        </label>

        <div class="bk-row" style="align-items:center;gap:1rem;">
          <label style="flex:1;">
            Sort Order
            <input v-model.number="form.sortOrder" type="number" min="0" placeholder="0" />
          </label>
          <label class="bk-toggle-label">
            <input type="checkbox" v-model="form.isActive" class="bk-checkbox" />
            Active
          </label>
        </div>

        <p v-if="err" class="bk-err">{{ err }}</p>

        <div class="bk-dialog-foot">
          <button class="btn-secondary" @click="form = null">Cancel</button>
          <button class="btn-primary" :disabled="saving" @click="save">
            {{ saving ? 'Saving…' : (editing ? 'Save changes' : 'Add book') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeft, Plus, Pencil, Trash2, LayoutGrid, List, Search, ExternalLink, BookMarked } from '@lucide/vue'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'

const route   = useRoute()
const catalog = useCatalogStore()
const confirm = useConfirm()

const subjectId = computed(() => Number(route.params.subjectId))
const subject   = computed(() => catalog.findSubject(subjectId.value))

const books         = ref([])
const subjectGrades = ref([])
const loading = ref(false)
const search  = ref('')
const view    = ref('grid')
const form    = ref(null)
const editing = ref(false)
const saving  = ref(false)
const err     = ref('')

const filtered = computed(() => {
  if (!search.value.trim()) return books.value
  const q = search.value.toLowerCase()
  return books.value.filter(b =>
    b.title?.toLowerCase().includes(q) ||
    b.titleUr?.includes(search.value) ||
    b.author?.toLowerCase().includes(q) ||
    b.board?.toLowerCase().includes(q)
  )
})

onMounted(async () => {
  loading.value = true
  try {
    await Promise.all([
      catalog.fetchAllSubjects(),
      catalog.fetchGrades(),
    ])
    ;[books.value, subjectGrades.value] = await Promise.all([
      catalog.fetchBooks(subjectId.value),
      catalog.fetchGradesForSubject(subjectId.value),
    ])
  } finally {
    loading.value = false
  }
})

function blankForm() {
  return {
    title: '', titleUr: '', author: '', publisher: '',
    edition: '', board: '', gradeCode: '', medium: '',
    coverUrl: '', downloadUrl: '', description: '',
    sortOrder: 0, isActive: true,
  }
}

function openCreate() {
  editing.value = false
  err.value = ''
  form.value = { ...blankForm(), medium: subject.value?.medium ?? '' }
}

function openEdit(b) {
  editing.value = true
  err.value = ''
  form.value = {
    id:          b.id,
    title:       b.title,
    titleUr:     b.titleUr ?? '',
    author:      b.author ?? '',
    publisher:   b.publisher ?? '',
    edition:     b.edition ?? '',
    board:       b.board ?? '',
    gradeCode:   b.gradeCode ?? '',
    medium:      b.medium ?? subject.value?.medium ?? '',
    coverUrl:    b.coverUrl ?? '',
    downloadUrl: b.downloadUrl ?? '',
    description: b.description ?? '',
    sortOrder:   b.sortOrder ?? 0,
    isActive:    b.isActive ?? true,
  }
}

async function save() {
  if (!form.value.title.trim()) { err.value = 'Title is required'; return }
  saving.value = true; err.value = ''
  try {
    const payload = {
      title:       form.value.title.trim(),
      titleUr:     form.value.titleUr || null,
      author:      form.value.author || null,
      publisher:   form.value.publisher || null,
      edition:     form.value.edition || null,
      board:       form.value.board || null,
      gradeCode:   form.value.gradeCode || null,
      medium:      form.value.medium || null,
      coverUrl:    form.value.coverUrl || null,
      downloadUrl: form.value.downloadUrl || null,
      description: form.value.description || null,
      sortOrder:   form.value.sortOrder,
      isActive:    form.value.isActive,
    }
    if (editing.value) {
      await catalog.updateBook(subjectId.value, form.value.id, payload)
    } else {
      await catalog.createBook(subjectId.value, payload)
    }
    books.value = await catalog.fetchBooks(subjectId.value)
    form.value = null
  } catch (e) {
    err.value = e.response?.data?.error || e.message
  } finally {
    saving.value = false
  }
}

async function remove(b) {
  const ok = await confirm({
    title: `Delete "${b.title}"`,
    message: 'This will permanently remove the book.',
    confirmLabel: 'Delete Book',
  })
  if (!ok) return
  try {
    await catalog.deleteBook(subjectId.value, b.id)
    books.value = books.value.filter(x => x.id !== b.id)
  } catch (e) {
    alert(e.response?.data?.error || e.message)
  }
}
</script>

<style scoped>
.bk-subject-label {
  font-size: 0.78rem; color: var(--t-text3); margin-top: 0.1rem;
}

/* Grid */
.bk-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  padding: 1.25rem 1.5rem;
}

/* Card */
.bk-card {
  display: flex; flex-direction: column; gap: 0.5rem; cursor: default;
}
.bk-cover {
  height: 120px; border-radius: 10px;
  background: var(--t-hover2) center/cover no-repeat;
  display: flex; align-items: center; justify-content: center;
  position: relative; flex-shrink: 0;
}
.bk-cover-icon { font-size: 2.5rem; }
.bk-inactive-badge {
  position: absolute; top: 6px; right: 6px;
  background: rgba(0,0,0,.55); color: #fbbf24;
  font-size: 0.65rem; font-weight: 700; padding: 2px 6px; border-radius: 6px;
}
.bk-info { display: flex; flex-direction: column; gap: 0.25rem; flex: 1; }
.bk-title { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); line-height: 1.3; }
.bk-title-ur { font-size: 0.78rem; color: var(--t-text3); text-align: right; }
.bk-meta { display: flex; flex-wrap: wrap; gap: 0.2rem; }
.bk-meta-item { font-size: 0.72rem; color: var(--t-text3); }
.bk-actions { display: flex; gap: 0.35rem; margin-top: 0.15rem; }
.bk-btn { font-size: 0.75rem; padding: 0.35rem 0.5rem; }
.bk-unit-count {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 18px; height: 18px; padding: 0 5px;
  border-radius: 999px; font-size: 0.65rem; font-weight: 700; line-height: 1;
  background: var(--t-accent); color: #fff;
}

/* Modal */
.bk-modal {
  position: fixed; inset: 0; z-index: 100;
  display: grid; place-items: center;
  background: rgba(0,0,0,.5); padding: 20px;
  backdrop-filter: blur(4px);
}
.bk-dialog {
  width: min(540px, 100%); max-height: 92vh; overflow-y: auto;
  padding: 28px; border-radius: 18px;
  background: var(--t-bg2); border: 1px solid var(--t-border);
  display: flex; flex-direction: column; gap: 14px;
  box-shadow: var(--t-shadow-md);
}
.bk-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; color: var(--t-text1); }
.bk-dialog label {
  display: flex; flex-direction: column; gap: 5px;
  font-size: .82rem; font-weight: 600; color: var(--t-text2);
}
.bk-dialog input,
.bk-dialog select,
.bk-dialog textarea {
  padding: 10px 12px; border-radius: 10px;
  border: 1px solid var(--t-border);
  background: var(--t-bg); color: var(--t-text1); font-size: .9rem; font: inherit;
  resize: vertical;
}
.bk-dialog input:focus,
.bk-dialog select:focus,
.bk-dialog textarea:focus {
  outline: none; border-color: var(--t-accent);
  box-shadow: 0 0 0 3px var(--t-acc-alpha-sm);
}
.bk-row { display: flex; gap: 12px; }
.bk-hint { display: block; font-size: .78rem; color: var(--t-text3); margin-top: 4px; }
.bk-medium-display {
  display: flex; align-items: center; gap: 8px;
  padding: 7px 10px; border: 1px solid var(--t-border);
  border-radius: 8px; background: var(--t-bg2); min-height: 36px;
}
.bk-medium-badge {
  font-size: .78rem; font-weight: 600; letter-spacing: .04em;
  text-transform: capitalize; padding: 2px 10px; border-radius: 999px;
}
.badge-amber { background: #fef3c7; color: #92400e; }
.badge-blue  { background: #dbeafe; color: #1e40af; }
.bk-medium-hint { font-size: .75rem; color: var(--t-text3); }
.bk-toggle-label {
  flex-direction: row !important; align-items: center; gap: 8px !important;
  font-size: .85rem !important; padding-top: 1.4rem;
}
.bk-checkbox { width: 16px; height: 16px; cursor: pointer; accent-color: var(--t-accent); }
.bk-required { color: #f87171; }
.bk-err { color: #f87171; font-size: .85rem; margin: 0; }
.bk-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
</style>
