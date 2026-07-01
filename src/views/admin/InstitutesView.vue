<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🏫</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Institutes</div>
        <div class="ag-banner-sub">Manage schools, colleges and organisations linked to users</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat"><span>🏛️</span> {{ institutes.length }} Institutes</span>
        <span class="ag-banner-stat"><span>✅</span> {{ institutes.filter(i => i.isActive).length }} Active</span>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Institute
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
            <input v-model="search" type="text" class="input" style="padding-left:2rem;" placeholder="Search by name or code…" />
          </div>
        </div>
        <div class="ag-filter-group" style="margin-left:auto; justify-content:flex-end;">
          <label class="ag-filter-label" style="opacity:0;">–</label>
          <div class="ag-view-toggle">
            <button class="ag-view-btn" :class="{ active: view === 'list' }" @click="view = 'list'" title="List view"><List class="w-3.5 h-3.5" /></button>
            <button class="ag-view-btn" :class="{ active: view === 'grid' }" @click="view = 'grid'" title="Grid view"><LayoutGrid class="w-3.5 h-3.5" /></button>
          </div>
        </div>
      </div>

      <!-- Count row -->
      <div style="padding:0.5rem 1.5rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; gap:0.5rem;">
        <Filter class="w-3 h-3" style="color:var(--t-text3);" />
        <span style="font-size:0.75rem; color:var(--t-text3);">
          Showing <strong style="color:var(--t-text1);">{{ filtered.length }}</strong>
          of <strong style="color:var(--t-text1);">{{ institutes.length }}</strong> institutes
        </span>
      </div>

      <!-- LIST VIEW -->
      <div v-if="view === 'list'" class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:48px;">#</th>
              <th>Name</th>
              <th style="width:100px;">Code</th>
              <th>Address</th>
              <th style="width:80px; text-align:center;">Users</th>
              <th style="width:90px;">Status</th>
              <th style="width:80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="7">
                <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading institutes…</p></div>
              </td>
            </tr>
            <tr v-for="(inst, i) in filtered" :key="inst.id">
              <td class="ag-table-muted" style="text-align:center;">{{ i + 1 }}</td>
              <td style="font-weight:600;">{{ inst.name }}</td>
              <td>
                <code v-if="inst.code" style="font-size:0.78rem; background:var(--t-hover2); padding:0.15rem 0.45rem; border-radius:5px;">{{ inst.code }}</code>
                <span v-else class="ag-table-muted">—</span>
              </td>
              <td class="ag-table-muted">{{ inst.address || '—' }}</td>
              <td class="ag-table-muted" style="text-align:center;">{{ inst.userCount }}</td>
              <td>
                <span v-if="inst.isActive" class="badge-green">Active</span>
                <span v-else style="font-size:0.72rem; font-weight:600; padding:0.2rem 0.55rem; border-radius:99px; background:rgba(148,163,184,0.12); color:var(--t-text3);">Inactive</span>
              </td>
              <td>
                <div class="flex gap-1">
                  <button @click="openEdit(inst)" class="btn-ghost" style="padding:0.3rem;" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button @click="remove(inst)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="!loading && !filtered.length">
              <td colspan="7">
                <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No institutes match your search.</p></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- GRID VIEW -->
      <div v-else style="padding:1.25rem; display:grid; grid-template-columns:repeat(auto-fill,minmax(240px,1fr)); gap:1rem;">
        <div v-if="loading" style="grid-column:1/-1;">
          <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading institutes…</p></div>
        </div>
        <div v-for="inst in filtered" :key="inst.id" class="ag-grid-card">
          <div class="flex items-start justify-between gap-2" style="margin-bottom:0.5rem;">
            <div style="font-weight:700; font-size:0.92rem; color:var(--t-text1);">{{ inst.name }}</div>
            <span v-if="inst.isActive" class="badge-green" style="flex-shrink:0;">Active</span>
            <span v-else style="font-size:0.7rem; font-weight:600; padding:0.2rem 0.55rem; border-radius:99px; background:rgba(148,163,184,0.12); color:var(--t-text3); flex-shrink:0;">Inactive</span>
          </div>
          <div v-if="inst.code" style="font-size:0.75rem; color:var(--t-text3); margin-bottom:0.35rem;">
            Code: <code style="background:var(--t-hover2); padding:0.1rem 0.35rem; border-radius:4px;">{{ inst.code }}</code>
          </div>
          <div v-if="inst.address" style="font-size:0.8rem; color:var(--t-text2); margin-bottom:0.5rem;">{{ inst.address }}</div>
          <div style="font-size:0.75rem; color:var(--t-text3); margin-bottom:0.75rem;">{{ inst.userCount }} users</div>
          <div class="flex items-center justify-between" style="border-top:1px solid var(--t-border); padding-top:0.6rem;">
            <div class="flex gap-1">
              <button @click="openEdit(inst)" class="btn-ghost" style="padding:0.3rem;" title="Edit"><Pencil class="w-3.5 h-3.5" /></button>
              <button @click="remove(inst)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete"><Trash2 class="w-3.5 h-3.5" /></button>
            </div>
          </div>
        </div>
        <div v-if="!loading && !filtered.length" style="grid-column:1/-1;">
          <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No institutes match your search.</p></div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="form" class="qb-overlay" @click.self="form = null">
      <div class="qb-modal">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">{{ editing ? 'Edit Institute' : 'Add Institute' }}</h3>
          <button @click="form = null" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>
        <form class="qb-modal-body" @submit.prevent="save">
          <div>
            <label class="label">Name <span style="color:#ef4444;">*</span></label>
            <input v-model="form.name" class="input" placeholder="e.g. New Century Academy" />
          </div>
          <div>
            <label class="label">Short Code</label>
            <input v-model="form.code" class="input" placeholder="e.g. NCA" style="text-transform:uppercase;" />
          </div>
          <div>
            <label class="label">Address</label>
            <input v-model="form.address" class="input" placeholder="City, District…" />
          </div>
          <div v-if="editing" class="flex items-center gap-2" style="padding:0.5rem 0;">
            <input v-model="form.isActive" type="checkbox" id="inst-active" style="width:1rem; height:1rem;" />
            <label for="inst-active" class="label" style="margin:0; cursor:pointer;">Active</label>
          </div>
          <p v-if="err" class="qb-err">{{ err }}</p>
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button type="submit" class="btn-primary" style="flex:1;" :disabled="saving">
              <span v-if="saving" class="qb-spinner"></span>
              <Plus v-else class="w-4 h-4" />
              {{ saving ? 'Saving…' : (editing ? 'Save Changes' : 'Add Institute') }}
            </button>
            <button type="button" @click="form = null" class="btn-secondary" :disabled="saving">Cancel</button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Plus, Search, Filter, Pencil, Trash2, X, List, LayoutGrid } from '@lucide/vue'
import { api } from '@/services/api'

const institutes = ref([])
const loading    = ref(false)
const saving     = ref(false)
const form       = ref(null)
const editing    = ref(false)
const err        = ref('')
const search     = ref('')
const view       = ref('list')

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return institutes.value
  return institutes.value.filter(i =>
    i.name.toLowerCase().includes(q) ||
    (i.code && i.code.toLowerCase().includes(q))
  )
})

async function fetchInstitutes() {
  loading.value = true
  try {
    const res = await api.get('/admin/institutes')
    institutes.value = res.data
  } finally {
    loading.value = false
  }
}

onMounted(fetchInstitutes)

function openCreate() {
  editing.value = false
  err.value     = ''
  form.value    = { name: '', code: '', address: '', isActive: true }
}

function openEdit(inst) {
  editing.value = true
  err.value     = ''
  form.value    = { ...inst }
}

async function save() {
  err.value = ''
  if (!form.value.name.trim()) { err.value = 'Name is required.'; return }
  saving.value = true
  try {
    if (editing.value) {
      await api.put(`/admin/institutes/${form.value.id}`, form.value)
    } else {
      await api.post('/admin/institutes', form.value)
    }
    form.value = null
    await fetchInstitutes()
  } catch (e) {
    err.value = e.data?.message || e.message || 'Failed to save.'
  } finally {
    saving.value = false
  }
}

async function remove(inst) {
  if (!confirm(`Delete "${inst.name}"? This cannot be undone.`)) return
  try {
    await api.delete(`/admin/institutes/${inst.id}`)
    institutes.value = institutes.value.filter(i => i.id !== inst.id)
  } catch (e) {
    alert(e.data?.message || 'Cannot delete this institute.')
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
