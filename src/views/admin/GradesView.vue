<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>Grades</h1>
        <p>Define which grades exist (ECD–12) and which subjects each one teaches.</p>
      </div>
      <button class="adm-btn" @click="openCreate">+ Add grade</button>
    </header>

    <div v-if="catalog.loading" class="adm-muted">Loading…</div>

    <table v-else class="adm-table">
      <thead>
        <tr><th>Code</th><th>Label</th><th>Band</th><th>Subjects</th><th>Status</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="g in catalog.grades" :key="g.code">
          <td><code>{{ g.code }}</code></td>
          <td>{{ g.label }}</td>
          <td>{{ g.band }}</td>
          <td>
            <button class="adm-link" @click="openSubjects(g)">{{ g.subjectCount ?? '—' }} subjects · edit</button>
          </td>
          <td><span :class="['adm-pill', g.enabled ? 'on' : 'off']">{{ g.enabled ? 'Enabled' : 'Disabled' }}</span></td>
          <td class="adm-actions">
            <button class="adm-link" @click="openEdit(g)">Edit</button>
            <button class="adm-link danger" @click="remove(g)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- grade create/edit modal -->
    <div v-if="form" class="adm-modal" @click.self="form = null">
      <div class="adm-dialog">
        <h2>{{ editing ? 'Edit grade' : 'Add grade' }}</h2>
        <label>Code <input v-model="form.code" :disabled="editing" placeholder="e.g. 9 or ECD" /></label>
        <label>Label <input v-model="form.label" placeholder="e.g. Grade 9" /></label>
        <label>Band
          <div class="adm-band-row">
            <select v-model="form.band" class="adm-select">
              <option value="">— select band —</option>
              <option v-for="b in catalog.bands" :key="b.id" :value="b.name">{{ b.name }}</option>
            </select>
            <button type="button" class="adm-refresh" @click="catalog.fetchBands()" title="Refresh bands">↺</button>
          </div>
        </label>
        <label>Sort order <input v-model.number="form.sort_order" type="number" /></label>
        <label class="adm-check"><input v-model="form.enabled" type="checkbox" /> Enabled</label>
        <p v-if="err" class="adm-err">{{ err }}</p>
        <div class="adm-dialog-foot">
          <button class="adm-btn ghost" @click="form = null">Cancel</button>
          <button class="adm-btn" :disabled="saving" @click="save">{{ saving ? 'Saving…' : 'Save' }}</button>
        </div>
      </div>
    </div>

    <!-- grade-subjects modal -->
    <div v-if="subjForm" class="adm-modal" @click.self="subjForm = null">
      <div class="adm-dialog">
        <h2>Subjects for {{ subjForm.label }}</h2>
        <p class="adm-muted">Tick the subjects taught in this grade.</p>
        <div class="adm-subgrid">
          <label v-for="s in catalog.subjects" :key="s.id" class="adm-check">
            <input type="checkbox" :value="s.id" v-model="subjForm.selected" /> {{ s.icon }} {{ s.name }}
          </label>
        </div>
        <p v-if="err" class="adm-err">{{ err }}</p>
        <div class="adm-dialog-foot">
          <button class="adm-btn ghost" @click="subjForm = null">Cancel</button>
          <button class="adm-btn" :disabled="saving" @click="saveSubjects">{{ saving ? 'Saving…' : 'Save subjects' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useCatalogStore } from '@/stores/catalog'

const catalog = useCatalogStore()
const form = ref(null)
const editing = ref(false)
const subjForm = ref(null)
const saving = ref(false)
const err = ref('')

onMounted(async () => {
  await Promise.all([catalog.fetchGrades(), catalog.fetchAllSubjects(), catalog.fetchBands()])
})

function openCreate() { editing.value = false; err.value = ''; form.value = { code: '', label: '', band: '', sort_order: 99, enabled: true } }
function openEdit(g) { editing.value = true; err.value = ''; form.value = { ...g, enabled: !!g.enabled } }

async function save() {
  saving.value = true; err.value = ''
  try {
    const payload = { ...form.value, enabled: form.value.enabled ? 1 : 0 }
    if (editing.value) await catalog.updateGrade(form.value.code, payload)
    else await catalog.createGrade(payload)
    form.value = null
  } catch (e) { err.value = e.message } finally { saving.value = false }
}

async function remove(g) {
  if (!confirm(`Delete grade ${g.label}? This removes its subject links.`)) return
  try { await catalog.deleteGrade(g.code) } catch (e) { alert(e.message) }
}

async function openSubjects(g) {
  err.value = ''
  const subs = await catalog.fetchSubjectsForGrade(g.code)
  subjForm.value = { code: g.code, label: g.label, selected: subs.map((s) => s.id) }
}

async function saveSubjects() {
  saving.value = true; err.value = ''
  try {
    await catalog.setGradeSubjects(subjForm.value.code, subjForm.value.selected)
    await catalog.fetchGrades()
    subjForm.value = null
  } catch (e) { err.value = e.message } finally { saving.value = false }
}
</script>

<style scoped>
.adm { max-width: 1100px; margin: 0 auto; padding: 24px; color: var(--t-text1); }
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
.adm-pill { padding: 3px 10px; border-radius: 99px; font-size: .72rem; font-weight: 700; }
.adm-pill.on { background: rgba(52,211,153,.15); color: #34d399; }
.adm-pill.off { background: rgba(148,163,184,.15); color: var(--t-text3); }
.adm-modal { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; background: rgba(0,0,0,.5); padding: 20px; }
.adm-dialog { width: min(480px, 100%); max-height: 90vh; overflow: auto; padding: 28px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 14px; }
.adm-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; }
.adm-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.adm-dialog input:not([type=checkbox]) { padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); }
.adm-check { flex-direction: row !important; align-items: center; gap: 8px; }
.adm-subgrid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.adm-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
.adm-err { color: #f87171; font-size: .85rem; margin: 0; }
.adm-band-row { display: flex; gap: 8px; align-items: center; }
.adm-select { flex: 1; padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); font-size: .9rem; }
.adm-refresh { flex-shrink: 0; width: 36px; height: 36px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); cursor: pointer; font-size: 1.1rem; display: flex; align-items: center; justify-content: center; transition: all .15s; }
.adm-refresh:hover { border-color: var(--t-accent); color: var(--t-accent); }
</style>
