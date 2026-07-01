<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>Institutes</h1>
        <p>Manage schools, colleges and organisations linked to users.</p>
      </div>
      <button class="adm-btn" @click="openCreate">+ Add Institute</button>
    </header>

    <div v-if="loading" class="adm-muted">Loading…</div>

    <table v-else class="adm-table">
      <thead>
        <tr>
          <th>#</th>
          <th>Name</th>
          <th>Code</th>
          <th>Address</th>
          <th>Users</th>
          <th>Status</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(inst, i) in institutes" :key="inst.id">
          <td class="adm-muted">{{ i + 1 }}</td>
          <td>{{ inst.name }}</td>
          <td><code v-if="inst.code">{{ inst.code }}</code><span v-else class="adm-muted">—</span></td>
          <td class="adm-muted">{{ inst.address || '—' }}</td>
          <td class="adm-muted">{{ inst.userCount }}</td>
          <td>
            <span :class="['adm-pill', inst.isActive ? 'on' : 'off']">
              {{ inst.isActive ? 'Active' : 'Inactive' }}
            </span>
          </td>
          <td class="adm-actions">
            <button class="adm-link" @click="openEdit(inst)">Edit</button>
            <button class="adm-link danger" @click="remove(inst)">Delete</button>
          </td>
        </tr>
        <tr v-if="!institutes.length">
          <td colspan="7" class="adm-muted">No institutes yet. Click "+ Add Institute" to create one.</td>
        </tr>
      </tbody>
    </table>

    <!-- Create / Edit Modal -->
    <div v-if="form" class="adm-modal" @click.self="form = null">
      <div class="adm-dialog">
        <h2>{{ editing ? 'Edit Institute' : 'Add Institute' }}</h2>

        <label>
          Name <span class="adm-req">*</span>
          <input v-model="form.name" placeholder="e.g. New Century Academy" />
        </label>

        <label>
          Short Code
          <input v-model="form.code" placeholder="e.g. NCA" style="text-transform:uppercase;" />
        </label>

        <label>
          Address
          <input v-model="form.address" placeholder="City, District…" />
        </label>

        <label v-if="editing" class="adm-check">
          <input v-model="form.isActive" type="checkbox" />
          Active
        </label>

        <div v-if="err" class="adm-err">{{ err }}</div>

        <div class="adm-dialog-foot">
          <button class="adm-btn-ghost" @click="form = null">Cancel</button>
          <button class="adm-btn" :disabled="saving" @click="save">
            {{ saving ? 'Saving…' : (editing ? 'Update' : 'Create') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { api } from '@/services/api'

const institutes = ref([])
const loading    = ref(false)
const saving     = ref(false)
const form       = ref(null)
const editing    = ref(false)
const err        = ref('')

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
.adm { max-width: 900px; margin: 0 auto; padding: 24px; color: var(--t-text1); }
.adm-head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; margin-bottom: 24px; }
.adm-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.9rem; margin: 0 0 4px; }
.adm-head p { margin: 0; color: var(--t-text2); font-size: .9rem; }
.adm-muted { color: var(--t-text2); padding: 12px 0; }
.adm-btn { padding: 10px 18px; border: 0; border-radius: 11px; cursor: pointer; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.adm-btn:disabled { opacity: .6; }
.adm-btn-ghost { padding: 10px 18px; border-radius: 11px; cursor: pointer; font-weight: 600; color: var(--t-text1); background: var(--t-hover); border: 1px solid var(--t-border); }
.adm-table { width: 100%; border-collapse: collapse; }
.adm-table th { text-align: left; font-size: .72rem; letter-spacing: .08em; text-transform: uppercase; color: var(--t-text3); padding: 10px 12px; border-bottom: 1px solid var(--t-border); }
.adm-table td { padding: 12px; border-bottom: 1px solid var(--t-border); font-size: .92rem; }
.adm-actions { display: flex; gap: 12px; }
.adm-link { background: none; border: 0; cursor: pointer; color: var(--t-accent); font-weight: 600; font-size: .88rem; padding: 0; }
.adm-link.danger { color: #f87171; }
.adm-modal { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; background: rgba(0,0,0,.5); padding: 20px; }
.adm-dialog { width: min(440px, 100%); padding: 28px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 14px; }
.adm-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; }
.adm-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.adm-dialog input { padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); }
.adm-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
.adm-err { color: #f87171; font-size: .85rem; }
.adm-req { color: #f87171; }
.adm-check { flex-direction: row !important; align-items: center; gap: 8px !important; cursor: pointer; }
.adm-pill { font-size: .75rem; font-weight: 700; padding: 2px 10px; border-radius: 99px; }
.adm-pill.on  { background: #d1fae5; color: #065f46; }
.adm-pill.off { background: #fee2e2; color: #991b1b; }
</style>
