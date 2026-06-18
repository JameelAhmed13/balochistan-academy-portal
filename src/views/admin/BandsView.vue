<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>Grade Bands</h1>
        <p>Manage grouping tiers (Primary, Middle, Secondary…) used to categorise grades.</p>
      </div>
      <button class="adm-btn" @click="openCreate">+ Add band</button>
    </header>

    <div v-if="loading" class="adm-muted">Loading…</div>

    <table v-else class="adm-table">
      <thead>
        <tr><th>Name</th><th>Sort order</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="b in catalog.bands" :key="b.id">
          <td>{{ b.name }}</td>
          <td>{{ b.sortOrder }}</td>
          <td class="adm-actions">
            <button class="adm-link" @click="openEdit(b)">Edit</button>
            <button class="adm-link danger" @click="remove(b)">Delete</button>
          </td>
        </tr>
        <tr v-if="!catalog.bands.length">
          <td colspan="3" class="adm-muted">No bands yet — add one above.</td>
        </tr>
      </tbody>
    </table>

    <!-- create / edit modal -->
    <div v-if="form" class="adm-modal" @click.self="form = null">
      <div class="adm-dialog">
        <h2>{{ editing ? 'Edit band' : 'Add band' }}</h2>
        <label>Name <input v-model="form.name" placeholder="e.g. Primary" /></label>
        <label>Sort order <input v-model.number="form.sortOrder" type="number" min="0" /></label>
        <p v-if="err" class="adm-err">{{ err }}</p>
        <div class="adm-dialog-foot">
          <button class="adm-btn ghost" @click="form = null">Cancel</button>
          <button class="adm-btn" :disabled="saving" @click="save">{{ saving ? 'Saving…' : 'Save' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useCatalogStore } from '@/stores/catalog'

const catalog = useCatalogStore()
const loading = ref(false)
const form = ref(null)
const editing = ref(false)
const saving = ref(false)
const err = ref('')

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
  if (!confirm(`Delete band "${b.name}"? Grades using this band will keep the name as text.`)) return
  try { await catalog.deleteBand(b.id) } catch (e) { alert(e.response?.data?.error || e.message) }
}
</script>

<style scoped>
.adm { max-width: 700px; margin: 0 auto; padding: 24px; color: var(--t-text1); }
.adm-head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; margin-bottom: 24px; }
.adm-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.9rem; margin: 0 0 4px; }
.adm-head p { margin: 0; color: var(--t-text2); font-size: .9rem; }
.adm-muted { color: var(--t-text2); padding: 12px 0; }
.adm-btn { padding: 10px 18px; border: 0; border-radius: 11px; cursor: pointer; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.adm-btn.ghost { background: var(--t-hover); color: var(--t-text1); border: 1px solid var(--t-border); }
.adm-btn:disabled { opacity: .6; }
.adm-table { width: 100%; border-collapse: collapse; }
.adm-table th { text-align: left; font-size: .72rem; letter-spacing: .08em; text-transform: uppercase; color: var(--t-text3); padding: 10px 12px; border-bottom: 1px solid var(--t-border); }
.adm-table td { padding: 12px; border-bottom: 1px solid var(--t-border); font-size: .92rem; }
.adm-actions { display: flex; gap: 12px; }
.adm-link { background: none; border: 0; cursor: pointer; color: var(--t-accent); font-weight: 600; font-size: .88rem; padding: 0; }
.adm-link.danger { color: #f87171; }
.adm-modal { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; background: rgba(0,0,0,.5); padding: 20px; }
.adm-dialog { width: min(420px, 100%); padding: 28px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 14px; }
.adm-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; }
.adm-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.adm-dialog input { padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); }
.adm-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
.adm-err { color: #f87171; font-size: .85rem; margin: 0; }
</style>
