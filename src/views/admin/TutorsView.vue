<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>AI Tutors</h1>
        <p>Persona tutors students chat with. Assign a subject (and optionally a grade) and write the persona prompt.</p>
      </div>
      <button class="adm-btn" @click="openCreate">+ Add tutor</button>
    </header>

    <div class="adm-grid">
      <article v-for="t in tutors" :key="t.id" class="adm-card">
        <div class="adm-card-top">
          <span class="adm-ava" :style="{ background: 'var(--t-hover)' }">{{ t.icon || '🤖' }}</span>
          <div>
            <b>{{ t.persona }}</b>
            <span class="adm-sub">{{ t.subject_name || 'All subjects' }}<template v-if="t.grade_code"> · Grade {{ t.grade_code }}</template></span>
          </div>
          <span :class="['adm-pill', t.active ? 'on' : 'off']">{{ t.active ? 'Active' : 'Off' }}</span>
        </div>
        <p class="adm-desc">{{ t.description }}</p>
        <div class="adm-actions">
          <button class="adm-link" @click="openEdit(t)">Edit</button>
          <button class="adm-link danger" @click="remove(t)">Delete</button>
        </div>
      </article>
    </div>

    <div v-if="form" class="adm-modal" @click.self="form = null">
      <div class="adm-dialog">
        <h2>{{ editing ? 'Edit tutor' : 'Add tutor' }}</h2>
        <label>Persona name <input v-model="form.persona" placeholder="e.g. Albert Einstein" /></label>
        <label>Slug <input v-model="form.slug" :disabled="editing" placeholder="einstein" /></label>
        <div class="adm-row2">
          <label>Subject
            <select v-model="form.subject_id">
              <option :value="null">All subjects</option>
              <option v-for="s in catalog.subjects" :key="s.id" :value="s.id">{{ s.name }}</option>
            </select>
          </label>
          <label>Grade (optional)
            <select v-model="form.grade_code">
              <option :value="null">All grades</option>
              <option v-for="g in catalog.grades" :key="g.code" :value="g.code">{{ g.label }}</option>
            </select>
          </label>
        </div>
        <div class="adm-row2">
          <label>Icon (emoji) <input v-model="form.icon" placeholder="⚡" /></label>
          <label>Color class <input v-model="form.color" placeholder="grad-amber" /></label>
        </div>
        <label>Description <input v-model="form.description" placeholder="Theoretical Physics Master" /></label>
        <label>Persona / system prompt
          <textarea v-model="form.system_prompt" rows="5" placeholder="You are … teach in simple language …" />
        </label>
        <label class="adm-check"><input v-model="form.active" type="checkbox" /> Active</label>
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
import api from '@/services/api'
import { useCatalogStore } from '@/stores/catalog'

const catalog = useCatalogStore()
const tutors = ref([])
const form = ref(null)
const editing = ref(false)
const saving = ref(false)
const err = ref('')

async function load() { tutors.value = (await api.get('/admin/tutors')).data }
onMounted(async () => {
  await Promise.all([load(), catalog.fetchGrades(), catalog.fetchAllSubjects()])
})

function openCreate() { editing.value = false; err.value = ''; form.value = { persona: '', slug: '', subject_id: null, grade_code: null, icon: '🤖', color: 'grad-blue', description: '', system_prompt: '', active: true } }
function openEdit(t) { editing.value = true; err.value = ''; form.value = { ...t, active: !!t.active } }

async function save() {
  saving.value = true; err.value = ''
  try {
    const payload = { ...form.value, active: form.value.active ? 1 : 0 }
    if (editing.value) await api.put(`/admin/tutors/${form.value.id}`, payload)
    else await api.post('/admin/tutors', payload)
    await load(); await catalog.fetchTutors()
    form.value = null
  } catch (e) { err.value = e.message } finally { saving.value = false }
}

async function remove(t) {
  if (!confirm(`Delete tutor ${t.persona}?`)) return
  try { await api.delete(`/admin/tutors/${t.id}`); await load(); await catalog.fetchTutors() } catch (e) { alert(e.message) }
}
</script>

<style scoped>
.adm { max-width: 1100px; margin: 0 auto; padding: 24px; color: var(--t-text1); }
.adm-head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; margin-bottom: 24px; }
.adm-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.9rem; margin: 0 0 4px; }
.adm-head p { margin: 0; color: var(--t-text2); font-size: .9rem; max-width: 560px; }
.adm-btn { padding: 10px 18px; border: 0; border-radius: 11px; cursor: pointer; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.adm-btn.ghost { background: var(--t-hover); color: var(--t-text1); border: 1px solid var(--t-border); }
.adm-btn:disabled { opacity: .6; }
.adm-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 14px; }
.adm-card { padding: 18px; border-radius: 16px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 10px; }
.adm-card-top { display: flex; align-items: center; gap: 11px; }
.adm-ava { display: grid; place-items: center; width: 40px; height: 40px; border-radius: 12px; font-size: 20px; }
.adm-card-top b { display: block; font-size: .95rem; }
.adm-sub { font-size: .76rem; color: var(--t-text3); }
.adm-card-top .adm-pill { margin-left: auto; }
.adm-desc { margin: 0; font-size: .85rem; color: var(--t-text2); min-height: 34px; }
.adm-actions { display: flex; gap: 14px; }
.adm-link { background: none; border: 0; cursor: pointer; color: var(--t-accent); font-weight: 600; font-size: .85rem; padding: 0; }
.adm-link.danger { color: #f87171; }
.adm-pill { padding: 3px 10px; border-radius: 99px; font-size: .72rem; font-weight: 700; }
.adm-pill.on { background: rgba(52,211,153,.15); color: #34d399; }
.adm-pill.off { background: rgba(148,163,184,.15); color: var(--t-text3); }
.adm-modal { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; background: rgba(0,0,0,.5); padding: 20px; }
.adm-dialog { width: min(540px, 100%); max-height: 92vh; overflow: auto; padding: 28px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 13px; }
.adm-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; }
.adm-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.adm-dialog input:not([type=checkbox]), .adm-dialog select, .adm-dialog textarea { padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); font: inherit; }
.adm-row2 { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.adm-check { flex-direction: row !important; align-items: center; gap: 8px; }
.adm-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
.adm-err { color: #f87171; font-size: .85rem; margin: 0; }
</style>
