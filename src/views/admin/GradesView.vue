<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📚</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Grades</div>
        <div class="ag-banner-sub">Define which grades exist (ECD–12) and which subjects each one teaches</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat"><span>📊</span> {{ grades.length }} Grades</span>
        <span class="ag-banner-stat"><span>✅</span> {{ grades.filter(g => g.isEnabled).length }} Enabled</span>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Grade
      </button>
    </div>

    <!-- Main card -->
    <div class="ag-card">
      <!-- Filter bar -->
      <div class="ag-filter-bar">
        <div class="ag-filter-group" style="flex:1; min-width:220px;">
          <label class="ag-filter-label">Search</label>
          <div style="position:relative;">
            <Search class="w-3.5 h-3.5" style="position:absolute; left:0.6rem; top:50%; transform:translateY(-50%); color:var(--t-text3); pointer-events:none;" />
            <input v-model="search" type="text" class="input" style="padding-left:2rem;" placeholder="Search by code or label…" />
          </div>
        </div>
      </div>

      <!-- Count row -->
      <div style="padding:0.5rem 1.5rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; gap:0.5rem;">
        <Filter class="w-3 h-3" style="color:var(--t-text3);" />
        <span style="font-size:0.75rem; color:var(--t-text3);">
          Showing <strong style="color:var(--t-text1);">{{ filtered.length }}</strong>
          of <strong style="color:var(--t-text1);">{{ grades.length }}</strong> grades
        </span>
      </div>

      <!-- Table -->
      <div class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:80px;">Code</th>
              <th>Label</th>
              <th style="width:120px;">Band</th>
              <th style="width:70px; text-align:center;">Sort</th>
              <th style="width:160px;">Subjects</th>
              <th style="width:90px;">Status</th>
              <th style="width:80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="7">
                <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading grades…</p></div>
              </td>
            </tr>
            <tr v-for="g in filtered" :key="g.code">
              <td>
                <code style="font-size:0.8rem; background:var(--t-hover2); padding:0.15rem 0.45rem; border-radius:5px;">{{ g.code }}</code>
              </td>
              <td style="font-weight:600;">{{ g.label }}</td>
              <td class="ag-table-muted">{{ g.band || '—' }}</td>
              <td class="ag-table-muted" style="text-align:center;">{{ g.sortOrder }}</td>
              <td>
                <button class="btn-ghost" style="padding:0.25rem 0.5rem; font-size:0.8rem; gap:0.3rem;" @click="openSubjects(g)">
                  <span class="badge-blue" style="font-size:0.7rem; padding:0.1rem 0.4rem;">{{ g.subjectCount ?? '—' }}</span>
                  Manage
                </button>
              </td>
              <td>
                <span v-if="g.isEnabled" class="badge-green">Enabled</span>
                <span v-else style="font-size:0.72rem; font-weight:600; padding:0.2rem 0.55rem; border-radius:99px; background:rgba(148,163,184,0.12); color:var(--t-text3);">Disabled</span>
              </td>
              <td>
                <div class="flex gap-1">
                  <button @click="openEdit(g)" class="btn-ghost" style="padding:0.3rem;" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button @click="remove(g)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="!loading && !filtered.length">
              <td colspan="7">
                <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No grades match your search.</p></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Grade Create / Edit Modal -->
    <div v-if="form" class="qb-overlay" @click.self="form = null">
      <div class="qb-modal">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">{{ editing ? 'Edit Grade' : 'Add Grade' }}</h3>
          <button @click="form = null" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>
        <form class="qb-modal-body" @submit.prevent="save">
          <div>
            <label class="label">Code</label>
            <input v-model="form.code" :disabled="editing" class="input" placeholder="e.g. 9 or ECD" />
          </div>
          <div>
            <label class="label">Label</label>
            <input v-model="form.label" class="input" placeholder="e.g. Grade 9" />
          </div>
          <div>
            <label class="label">Band</label>
            <div style="display:flex; gap:0.5rem; align-items:center;">
              <select v-model="form.band" class="input" style="flex:1;">
                <option value="">— select band —</option>
                <option v-for="b in catalog.bands" :key="b.id" :value="b.name">{{ b.name }}</option>
              </select>
              <button type="button" class="btn-secondary" style="padding:0.5rem 0.75rem; white-space:nowrap;" @click="catalog.fetchBands()" title="Refresh bands">↺</button>
            </div>
          </div>
          <div>
            <label class="label">Sort Order</label>
            <input v-model.number="form.sortOrder" type="number" min="0" class="input" />
          </div>
          <div style="display:flex; align-items:center; gap:0.5rem; padding:0.25rem 0;">
            <input v-model="form.isEnabled" type="checkbox" id="grade-enabled" style="width:1rem; height:1rem;" />
            <label for="grade-enabled" class="label" style="margin:0; cursor:pointer;">Enabled</label>
          </div>
          <p v-if="err" class="qb-err">{{ err }}</p>
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button type="submit" class="btn-primary" style="flex:1;" :disabled="saving">
              <span v-if="saving" class="qb-spinner"></span>
              <Plus v-else class="w-4 h-4" />
              {{ saving ? 'Saving…' : (editing ? 'Save Changes' : 'Add Grade') }}
            </button>
            <button type="button" @click="form = null" class="btn-secondary" :disabled="saving">Cancel</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Subject Assignment Modal -->
    <div v-if="subjForm" class="qb-overlay" @click.self="subjForm = null">
      <div class="qb-modal" style="max-width:520px;">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">Subjects for {{ subjForm.label }}</h3>
          <button @click="subjForm = null" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>
        <div class="qb-modal-body">
          <p style="margin:0; font-size:0.83rem; color:var(--t-text3);">Tick the subjects taught in this grade.</p>
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.5rem 1rem;">
            <label v-for="s in catalog.subjects" :key="s.id" style="display:flex; align-items:center; gap:0.5rem; cursor:pointer; padding:0.25rem 0; font-size:0.85rem; color:var(--t-text1);">
              <input type="checkbox" :value="s.id" v-model="subjForm.selected" style="width:1rem; height:1rem;" />
              {{ resolveIcon(s.icon) }} {{ s.name }}
            </label>
          </div>
          <p v-if="subjErr" class="qb-err">{{ subjErr }}</p>
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button class="btn-primary" style="flex:1;" :disabled="saving" @click="saveSubjects">
              <span v-if="saving" class="qb-spinner"></span>
              {{ saving ? 'Saving…' : 'Save Subjects' }}
            </button>
            <button class="btn-secondary" @click="subjForm = null" :disabled="saving">Cancel</button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Plus, Search, Filter, Pencil, Trash2, X } from '@lucide/vue'
import api from '@/services/api'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'

const catalog = useCatalogStore()
const confirm = useConfirm()

const LUCIDE_TO_EMOJI = {
  'calculator': '🧮', 'atom': '⚛️', 'flask': '⚗️', 'leaf': '🌿',
  'book-open': '📖', 'pen': '✒️', 'moon': '🌙', 'map': '🗺️',
  'monitor': '🖥️', 'microscope': '🔬', 'graduation-cap': '🎓',
}
function resolveIcon(icon) { return LUCIDE_TO_EMOJI[icon] ?? icon ?? '📚' }

const grades   = ref([])
const loading  = ref(false)
const saving   = ref(false)
const err      = ref('')
const subjErr  = ref('')
const form     = ref(null)
const editing  = ref(false)
const subjForm = ref(null)
const search   = ref('')

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return grades.value
  return grades.value.filter(g =>
    g.code.toLowerCase().includes(q) || g.label.toLowerCase().includes(q)
  )
})

async function fetchGrades() {
  loading.value = true
  try {
    grades.value = (await api.get('/admin/grades')).data
  } catch (e) {
    err.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  Promise.all([fetchGrades(), catalog.fetchAllSubjects(), catalog.fetchBands()])
})

function openCreate() {
  editing.value = false
  err.value = ''
  form.value = { code: '', label: '', band: '', sortOrder: grades.value.length + 1, isEnabled: true }
}

function openEdit(g) {
  editing.value = true
  err.value = ''
  form.value = {
    code:      g.code,
    label:     g.label,
    band:      g.band ?? '',
    sortOrder: g.sortOrder,
    isEnabled: !!g.isEnabled,
  }
}

async function save() {
  if (!form.value.code.trim())  { err.value = 'Code is required';  return }
  if (!form.value.label.trim()) { err.value = 'Label is required'; return }

  saving.value = true; err.value = ''
  try {
    if (editing.value) {
      await api.put(`/admin/grades/${form.value.code}`, {
        label:     form.value.label,
        band:      form.value.band,
        sortOrder: form.value.sortOrder,
        isEnabled: form.value.isEnabled,
      })
    } else {
      await api.post('/admin/grades', {
        code:      form.value.code.trim(),
        label:     form.value.label.trim(),
        band:      form.value.band,
        sortOrder: form.value.sortOrder,
        isEnabled: form.value.isEnabled,
      })
    }
    form.value = null
    await fetchGrades()
    catalog.fetchGrades().catch(() => {})
  } catch (e) {
    err.value = e.message
  } finally {
    saving.value = false
  }
}

async function remove(g) {
  const ok = await confirm({
    title: `Delete ${g.label}`,
    message: 'This will permanently remove the grade and all its subject links.',
    confirmLabel: 'Delete Grade',
  })
  if (!ok) return
  try {
    await api.delete(`/admin/grades/${g.code}`)
    await fetchGrades()
    catalog.fetchGrades().catch(() => {})
  } catch (e) {
    alert(e.message)
  }
}

async function openSubjects(g) {
  subjErr.value = ''
  const [subs] = await Promise.all([
    catalog.fetchSubjectsForGrade(g.code),
    catalog.fetchAllSubjects(),
  ])
  subjForm.value = { code: g.code, label: g.label, selected: subs.map((s) => s.id) }
}

async function saveSubjects() {
  saving.value = true; subjErr.value = ''
  try {
    await api.put(`/admin/grades/${subjForm.value.code}/subjects`, {
      subjectIds: subjForm.value.selected,
    })
    await fetchGrades()
    subjForm.value = null
  } catch (e) {
    subjErr.value = e.message
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.qb-overlay { position:fixed; inset:0; z-index:1000; background:rgba(0,0,0,0.45); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; padding:1.25rem; }
.qb-modal { background:var(--t-bg2); border:1px solid var(--t-border); border-radius:16px; width:100%; max-width:480px; max-height:calc(100vh - 2.5rem); overflow-y:auto; box-shadow:0 20px 60px rgba(0,0,0,0.3); }
.qb-modal-header { display:flex; align-items:center; justify-content:space-between; padding:1.25rem 1.5rem 0; }
.qb-modal-title { font-size:1rem; font-weight:700; color:var(--t-text1); margin:0; }
.qb-modal-body { padding:1.25rem 1.5rem 1.5rem; display:flex; flex-direction:column; gap:1rem; }
.qb-spinner { display:inline-block; width:12px; height:12px; border:2px solid rgba(255,255,255,0.4); border-top-color:white; border-radius:50%; animation:qbspin 0.7s linear infinite; }
@keyframes qbspin { to { transform:rotate(360deg); } }
.qb-err { margin:0; padding:10px 14px; border-radius:9px; font-size:0.83rem; background:#fef2f2; border:1px solid #fecaca; color:#b91c1c; }
</style>
