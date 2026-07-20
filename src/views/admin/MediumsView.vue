<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>Teaching Mediums</h1>
        <p>Define the languages of instruction (e.g. English Medium, اردو میڈیم) used to classify subjects.</p>
      </div>
      <button class="adm-btn" @click="openCreate">+ Add medium</button>
    </header>

    <div v-if="loading" class="adm-muted">Loading…</div>

    <table v-else class="adm-table">
      <thead>
        <tr><th>Name (value)</th><th>Label (display)</th><th>Sort order</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="m in catalog.mediums" :key="m.id">
          <td><code>{{ m.name }}</code></td>
          <td>{{ m.label }}</td>
          <td class="adm-sort">{{ m.sortOrder }}</td>
          <td class="adm-actions">
            <button class="adm-link" @click="openEdit(m)">Edit</button>
            <button class="adm-link danger" @click="remove(m)">Delete</button>
          </td>
        </tr>
        <tr v-if="!catalog.mediums.length">
          <td colspan="4" class="adm-muted">No mediums yet — add one above.</td>
        </tr>
      </tbody>
    </table>

    <!-- create / edit modal -->
    <div v-if="form" class="adm-modal" @click.self="form = null">
      <div class="adm-dialog">
        <h2>{{ editing ? 'Edit medium' : 'Add medium' }}</h2>

        <label>
          Name (stored value)
          <input v-model="form.name" :disabled="editing" placeholder="e.g. english" />
          <span class="adm-hint">Lowercase key stored on subjects — cannot be changed after creation.</span>
        </label>

        <label>
          Label (display text)
          <input v-model="form.label" placeholder="e.g. English Medium" />
        </label>

        <label>
          Sort order
          <input v-model.number="form.sortOrder" type="number" min="0" />
        </label>

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
import { useConfirm } from '@/composables/useConfirm'

const catalog = useCatalogStore()
const confirm = useConfirm()
const loading = ref(false)
const form    = ref(null)
const editing = ref(false)
const saving  = ref(false)
const err     = ref('')

onMounted(async () => {
  loading.value = true
  try { await catalog.fetchMediums() } finally { loading.value = false }
})

function openCreate() {
  editing.value = false; err.value = ''
  form.value = { name: '', label: '', sortOrder: catalog.mediums.length + 1 }
}

function openEdit(m) {
  editing.value = true; err.value = ''
  form.value = { ...m }
}

async function save() {
  if (!form.value.name.trim())  { err.value = 'Name is required';  return }
  if (!form.value.label.trim()) { err.value = 'Label is required'; return }
  saving.value = true; err.value = ''
  try {
    if (editing.value) await catalog.updateMedium(form.value.id, form.value)
    else               await catalog.createMedium(form.value)
    form.value = null
  } catch (e) {
    err.value = e.response?.data?.error || e.message
  } finally {
    saving.value = false
  }
}

async function remove(m) {
  const ok = await confirm({
    title: `Delete "${m.label}"`,
    message: 'Subjects already using this medium will keep the stored name as text.',
    confirmLabel: 'Delete Medium',
  })
  if (!ok) return
  try { await catalog.deleteMedium(m.id) } catch (e) { alert(e.response?.data?.error || e.message) }
}
</script>

<style scoped>
.adm { max-width: 720px; margin: 0 auto; padding: 24px; color: var(--t-text1); }
.adm-head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; margin-bottom: 24px; }
.adm-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.9rem; margin: 0 0 4px; }
.adm-head p { margin: 0; color: var(--t-text2); font-size: .9rem; max-width: 480px; }
.adm-muted { color: var(--t-text2); padding: 12px 0; }
.adm-btn { padding: 10px 18px; border: 0; border-radius: 11px; cursor: pointer; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.adm-btn.ghost { background: var(--t-hover); color: var(--t-text1); border: 1px solid var(--t-border); }
.adm-btn:disabled { opacity: .6; }
.adm-table { width: 100%; border-collapse: collapse; }
.adm-table th { text-align: left; font-size: .72rem; letter-spacing: .08em; text-transform: uppercase; color: var(--t-text3); padding: 10px 12px; border-bottom: 1px solid var(--t-border); }
.adm-table td { padding: 12px; border-bottom: 1px solid var(--t-border); font-size: .92rem; }
.adm-sort { color: var(--t-text3); font-size: .82rem; text-align: center; }
.adm-actions { display: flex; gap: 12px; }
.adm-link { background: none; border: 0; cursor: pointer; color: var(--t-accent); font-weight: 600; font-size: .88rem; padding: 0; }
.adm-link.danger { color: #f87171; }
.adm-modal { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; background: rgba(0,0,0,.5); padding: 20px; backdrop-filter: blur(4px); }
.adm-dialog { width: min(440px, 100%); padding: 28px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 14px; box-shadow: var(--t-shadow-md); }
.adm-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; }
.adm-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.adm-dialog input { padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); font-size: .9rem; }
.adm-dialog input:focus { outline: none; border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-acc-alpha-sm); }
.adm-dialog input:disabled { opacity: .5; cursor: not-allowed; }
.adm-hint { font-size: .74rem; font-weight: 400; color: var(--t-text3); margin-top: 1px; }
.adm-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
.adm-err { color: #f87171; font-size: .85rem; margin: 0; }
</style>
