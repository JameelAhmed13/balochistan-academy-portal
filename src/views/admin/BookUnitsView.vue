<template>
  <div class="animate-fade-in space-y-5">

    <!-- Page Header -->
    <div class="ag-page-hd">
      <div class="ag-page-hd-left">
        <button class="btn-ghost" @click="goBack">
          <ArrowLeft class="w-4 h-4" />
        </button>
        <div>
          <h2 class="section-title">Units</h2>
          <p v-if="book" class="bu-sub-label">{{ book.title }}</p>
        </div>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Unit
      </button>
    </div>

    <!-- Glass Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📑</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">{{ book?.title ?? 'Book' }} — Units</div>
        <div class="ag-banner-sub">Manage chapters and units of this book</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">📑 {{ units.length }} Unit{{ units.length !== 1 ? 's' : '' }}</span>
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
              placeholder="Search units…"
              style="padding-left:2rem;"
            />
          </div>
        </div>
      </div>

      <div v-if="loading" class="ag-empty">
        <div class="ag-empty-icon">⏳</div>
        <p>Loading units…</p>
      </div>

      <!-- Units Table -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:48px;">#</th>
              <th style="width:56px;">No.</th>
              <th>Unit Name</th>
              <th>Description</th>
              <th>Source</th>
              <th style="width:80px;">Order</th>
              <th style="width:130px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(u, idx) in filtered" :key="u.id">
              <td class="ag-table-muted">{{ idx + 1 }}</td>
              <td class="ag-table-muted" style="font-weight:600;">
                {{ u.number != null ? u.number : '—' }}
              </td>
              <td>
                <span style="font-weight:600;color:var(--t-text1);font-size:0.875rem;">{{ u.name }}</span>
              </td>
              <td class="ag-table-muted" style="font-size:0.8rem;max-width:220px;">
                {{ u.description || '—' }}
              </td>
              <td class="ag-table-muted" style="font-size:0.8rem;">{{ u.source || '—' }}</td>
              <td class="ag-table-muted">{{ u.sortOrder }}</td>
              <td>
                <div style="display:flex;gap:0.25rem;align-items:center;">
                  <button class="bu-topics-btn" @click="goToTopics(u)" title="Topics & Resources">
                    <BookOpen class="w-3.5 h-3.5" />
                    <span class="bu-topic-badge">{{ u.topicCount ?? 0 }}</span>
                  </button>
                  <button class="btn-ghost" style="padding:0.35rem;" @click="openEdit(u)" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button class="btn-ghost" style="padding:0.35rem;color:#f87171;" @click="remove(u)" title="Delete">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filtered.length === 0">
              <td colspan="7">
                <div class="ag-empty">
                  <div class="ag-empty-icon">📑</div>
                  <p>No units found.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add / Edit Unit Modal -->
    <div v-if="form" class="bu-modal" @click.self="form = null">
      <div class="bu-dialog">
        <h2>{{ editing ? 'Edit unit' : 'Add unit' }}</h2>

        <label>
          Unit Name <span class="bu-required">*</span>
          <input v-model="form.name" type="text" placeholder="e.g. Cell Biology" />
        </label>

        <div class="bu-row">
          <label style="flex:1;">
            Unit Number
            <input v-model.number="form.number" type="number" min="1" placeholder="e.g. 1" />
          </label>
          <label style="flex:1;">
            Sort Order
            <input v-model.number="form.sortOrder" type="number" min="0" placeholder="0" />
          </label>
        </div>

        <label>
          Description
          <textarea v-model="form.description" rows="3" placeholder="Brief description of this unit…" />
        </label>

        <label>
          Source / Reference
          <input v-model="form.source" type="text" placeholder="e.g. Chapter 3, Page 45" />
        </label>

        <p v-if="err" class="bu-err">{{ err }}</p>

        <div class="bu-dialog-foot">
          <button class="btn-secondary" @click="form = null">Cancel</button>
          <button class="btn-primary" :disabled="saving" @click="save">
            {{ saving ? 'Saving…' : (editing ? 'Save changes' : 'Add unit') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, Plus, Pencil, Trash2, Search, BookOpen } from '@lucide/vue'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'

const route   = useRoute()
const router  = useRouter()
const catalog = useCatalogStore()
const confirm = useConfirm()

const bookId    = computed(() => Number(route.params.bookId))
const subjectId = computed(() => Number(route.params.subjectId))

const book    = ref(null)
const units   = ref([])
const loading = ref(false)
const search  = ref('')
const form    = ref(null)
const editing = ref(false)
const saving  = ref(false)
const err     = ref('')

const filtered = computed(() => {
  if (!search.value.trim()) return units.value
  const q = search.value.toLowerCase()
  return units.value.filter(u =>
    u.name?.toLowerCase().includes(q) ||
    u.description?.toLowerCase().includes(q) ||
    u.source?.toLowerCase().includes(q)
  )
})

function goBack() {
  router.push(`/app/admin/content/${subjectId.value}/books`)
}

function goToTopics(u) {
  router.push(`/app/admin/content/${subjectId.value}/books/${bookId.value}/units/${u.id}/topics`)
}

onMounted(async () => {
  loading.value = true
  try {
    const books = await catalog.fetchBooks(subjectId.value)
    book.value  = books.find(b => b.id === bookId.value) ?? null
    units.value = await catalog.fetchBookUnits(bookId.value)
  } finally {
    loading.value = false
  }
})

function blankForm() {
  return { name: '', number: null, sortOrder: 0, description: '', source: '' }
}

function openCreate() {
  editing.value = false
  err.value = ''
  form.value = blankForm()
}

function openEdit(u) {
  editing.value = true
  err.value = ''
  form.value = {
    id:          u.id,
    name:        u.name,
    number:      u.number ?? null,
    sortOrder:   u.sortOrder ?? 0,
    description: u.description ?? '',
    source:      u.source ?? '',
  }
}

async function save() {
  if (!form.value.name.trim()) { err.value = 'Unit name is required'; return }
  saving.value = true; err.value = ''
  try {
    const payload = {
      name:        form.value.name.trim(),
      number:      form.value.number || null,
      sortOrder:   form.value.sortOrder ?? 0,
      description: form.value.description || null,
      source:      form.value.source || null,
    }
    if (editing.value) {
      await catalog.updateBookUnit(bookId.value, form.value.id, payload)
    } else {
      await catalog.createBookUnit(bookId.value, payload)
    }
    units.value = await catalog.fetchBookUnits(bookId.value)
    form.value = null
  } catch (e) {
    err.value = e.response?.data?.error || e.message
  } finally {
    saving.value = false
  }
}

async function remove(u) {
  const ok = await confirm({
    title: `Delete "${u.name}"`,
    message: 'This will permanently remove the unit and all its topics.',
    confirmLabel: 'Delete Unit',
  })
  if (!ok) return
  try {
    await catalog.deleteBookUnit(bookId.value, u.id)
    units.value = units.value.filter(x => x.id !== u.id)
  } catch (e) {
    alert(e.response?.data?.error || e.message)
  }
}
</script>

<style scoped>
.bu-sub-label {
  font-size: 0.78rem; color: var(--t-text3); margin-top: 0.1rem;
}

.bu-topics-btn {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 0.3rem 0.5rem; border-radius: 8px; border: none; cursor: pointer;
  background: var(--t-acc-alpha-sm); color: var(--t-accent);
  font-size: 0.75rem; font-weight: 600; transition: background 0.15s;
}
.bu-topics-btn:hover { background: var(--t-acc-alpha); }
.bu-topic-badge {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 16px; height: 16px; padding: 0 4px; border-radius: 999px;
  background: var(--t-accent); color: #fff; font-size: 0.65rem; font-weight: 700; line-height: 1;
}

/* Modal */
.bu-modal {
  position: fixed; inset: 0; z-index: 100;
  display: grid; place-items: center;
  background: rgba(0,0,0,.5); padding: 20px;
  backdrop-filter: blur(4px);
}
.bu-dialog {
  width: min(480px, 100%); max-height: 92vh; overflow-y: auto;
  padding: 28px; border-radius: 18px;
  background: var(--t-bg2); border: 1px solid var(--t-border);
  display: flex; flex-direction: column; gap: 14px;
  box-shadow: var(--t-shadow-md);
}
.bu-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; color: var(--t-text1); }
.bu-dialog label {
  display: flex; flex-direction: column; gap: 5px;
  font-size: .82rem; font-weight: 600; color: var(--t-text2);
}
.bu-dialog input,
.bu-dialog textarea {
  padding: 10px 12px; border-radius: 10px;
  border: 1px solid var(--t-border);
  background: var(--t-bg); color: var(--t-text1); font-size: .9rem; font: inherit;
  resize: vertical;
}
.bu-dialog input:focus,
.bu-dialog textarea:focus {
  outline: none; border-color: var(--t-accent);
  box-shadow: 0 0 0 3px var(--t-acc-alpha-sm);
}
.bu-row { display: flex; gap: 12px; }
.bu-required { color: #f87171; }
.bu-err { color: #f87171; font-size: .85rem; margin: 0; }
.bu-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
</style>
