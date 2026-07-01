<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🎓</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Grade Bands</div>
        <div class="ag-banner-sub">Manage grouping tiers (Primary, Middle, Secondary…) used to categorise grades</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat"><span>📊</span> {{ catalog.bands.length }} Bands</span>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Band
      </button>
    </div>

    <!-- Main card -->
    <div class="ag-card">
      <!-- Filter bar -->
      <div class="ag-filter-bar">
        <div class="ag-filter-group" style="flex:1; min-width:200px;">
          <label class="ag-filter-label">Search</label>
          <div style="position:relative;">
            <Search class="w-3.5 h-3.5" style="position:absolute; left:0.6rem; top:50%; transform:translateY(-50%); color:var(--t-text3); pointer-events:none;" />
            <input v-model="search" type="text" class="input" style="padding-left:2rem;" placeholder="Search bands…" />
          </div>
        </div>
      </div>

      <!-- Count row -->
      <div style="padding:0.5rem 1.5rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; gap:0.5rem;">
        <Filter class="w-3 h-3" style="color:var(--t-text3);" />
        <span style="font-size:0.75rem; color:var(--t-text3);">
          Showing <strong style="color:var(--t-text1);">{{ filtered.length }}</strong>
          of <strong style="color:var(--t-text1);">{{ catalog.bands.length }}</strong> bands
        </span>
      </div>

      <!-- Table -->
      <div class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:48px;">#</th>
              <th>Name</th>
              <th style="width:120px; text-align:center;">Sort Order</th>
              <th style="width:80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="4">
                <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading bands…</p></div>
              </td>
            </tr>
            <tr v-for="(b, i) in filtered" :key="b.id">
              <td class="ag-table-muted" style="text-align:center;">{{ i + 1 }}</td>
              <td style="font-weight:600;">{{ b.name }}</td>
              <td class="ag-table-muted" style="text-align:center;">{{ b.sortOrder }}</td>
              <td>
                <div class="flex gap-1">
                  <button @click="openEdit(b)" class="btn-ghost" style="padding:0.3rem;" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button @click="remove(b)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="!loading && !filtered.length">
              <td colspan="4">
                <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No bands match your search.</p></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="form" class="qb-overlay" @click.self="form = null">
      <div class="qb-modal">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">{{ editing ? 'Edit Band' : 'Add Band' }}</h3>
          <button @click="form = null" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>
        <form class="qb-modal-body" @submit.prevent="save">
          <div>
            <label class="label">Name <span style="color:#ef4444;">*</span></label>
            <input v-model="form.name" class="input" placeholder="e.g. Primary" />
          </div>
          <div>
            <label class="label">Sort Order</label>
            <input v-model.number="form.sortOrder" type="number" min="0" class="input" />
          </div>
          <p v-if="err" class="qb-err">{{ err }}</p>
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button type="submit" class="btn-primary" style="flex:1;" :disabled="saving">
              <span v-if="saving" class="qb-spinner"></span>
              <Plus v-else class="w-4 h-4" />
              {{ saving ? 'Saving…' : (editing ? 'Save Changes' : 'Add Band') }}
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
import { Plus, Search, Filter, Pencil, Trash2, X } from '@lucide/vue'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'

const catalog = useCatalogStore()
const confirm = useConfirm()
const loading = ref(false)
const form    = ref(null)
const editing = ref(false)
const saving  = ref(false)
const err     = ref('')
const search  = ref('')

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return catalog.bands
  return catalog.bands.filter(b => b.name.toLowerCase().includes(q))
})

onMounted(async () => {
  loading.value = true
  try { await catalog.fetchBands() } finally { loading.value = false }
})

function openCreate() {
  editing.value = false
  err.value = ''
  form.value = { name: '', sortOrder: catalog.bands.length + 1 }
}

function openEdit(b) {
  editing.value = true
  err.value = ''
  form.value = { ...b }
}

async function save() {
  if (!form.value.name.trim()) { err.value = 'Name is required'; return }
  saving.value = true; err.value = ''
  try {
    if (editing.value) await catalog.updateBand(form.value.id, form.value)
    else               await catalog.createBand(form.value)
    form.value = null
  } catch (e) {
    err.value = e.response?.data?.error || e.message
  } finally {
    saving.value = false
  }
}

async function remove(b) {
  const ok = await confirm({
    title: `Delete "${b.name}"`,
    message: 'Grades already using this band will keep its name as stored text.',
    confirmLabel: 'Delete Band',
  })
  if (!ok) return
  try { await catalog.deleteBand(b.id) } catch (e) { alert(e.response?.data?.error || e.message) }
}
</script>

<style scoped>
.qb-overlay { position:fixed; inset:0; z-index:1000; background:rgba(0,0,0,0.45); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; padding:1.25rem; }
.qb-modal { background:var(--t-bg2); border:1px solid var(--t-border); border-radius:16px; width:100%; max-width:440px; max-height:calc(100vh - 2.5rem); overflow-y:auto; box-shadow:0 20px 60px rgba(0,0,0,0.3); }
.qb-modal-header { display:flex; align-items:center; justify-content:space-between; padding:1.25rem 1.5rem 0; }
.qb-modal-title { font-size:1rem; font-weight:700; color:var(--t-text1); margin:0; }
.qb-modal-body { padding:1.25rem 1.5rem 1.5rem; display:flex; flex-direction:column; gap:1rem; }
.qb-spinner { display:inline-block; width:12px; height:12px; border:2px solid rgba(255,255,255,0.4); border-top-color:white; border-radius:50%; animation:qbspin 0.7s linear infinite; }
@keyframes qbspin { to { transform:rotate(360deg); } }
.qb-err { margin:0; padding:10px 14px; border-radius:9px; font-size:0.83rem; background:#fef2f2; border:1px solid #fecaca; color:#b91c1c; }
</style>
