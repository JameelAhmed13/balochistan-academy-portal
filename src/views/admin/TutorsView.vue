<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🤖</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">AI Tutors</div>
        <div class="ag-banner-sub">Persona tutors students chat with — assign a subject, grade, and system prompt</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat"><span>🤖</span> {{ tutors.length }} Tutors</span>
        <span class="ag-banner-stat"><span>✅</span> {{ tutors.filter(t => t.isActive).length }} Active</span>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Tutor
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
            <input v-model="search" type="text" class="input" style="padding-left:2rem;" placeholder="Search by name or subject…" />
          </div>
        </div>
        <div class="ag-filter-group" style="margin-left:auto; justify-content:flex-end;">
          <label class="ag-filter-label" style="opacity:0;">–</label>
          <div class="ag-view-toggle">
            <button class="ag-view-btn" :class="{ active: view === 'grid' }" @click="view = 'grid'" title="Grid view"><LayoutGrid class="w-3.5 h-3.5" /></button>
            <button class="ag-view-btn" :class="{ active: view === 'list' }" @click="view = 'list'" title="List view"><List class="w-3.5 h-3.5" /></button>
          </div>
        </div>
      </div>

      <!-- Count row -->
      <div style="padding:0.5rem 1.5rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; gap:0.5rem;">
        <Filter class="w-3 h-3" style="color:var(--t-text3);" />
        <span style="font-size:0.75rem; color:var(--t-text3);">
          Showing <strong style="color:var(--t-text1);">{{ filtered.length }}</strong>
          of <strong style="color:var(--t-text1);">{{ tutors.length }}</strong> tutors
          <span v-if="loading" style="margin-left:0.5rem; opacity:0.6;">(loading…)</span>
        </span>
      </div>

      <!-- GRID VIEW -->
      <div v-if="view === 'grid'" style="padding:1.25rem; display:grid; grid-template-columns:repeat(auto-fill,minmax(280px,1fr)); gap:1rem;">
        <div v-if="loading" style="grid-column:1/-1;">
          <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading tutors…</p></div>
        </div>
        <div v-for="t in filtered" :key="t.id" class="ag-grid-card" style="display:flex; flex-direction:column; gap:0.6rem;">
          <div style="display:flex; align-items:center; gap:0.75rem;">
            <span :class="['tutor-ava', t.color || 'grad-blue']">{{ resolveIcon(t.icon) }}</span>
            <div style="flex:1; min-width:0;">
              <div style="font-weight:700; font-size:0.95rem; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">{{ t.persona }}</div>
              <div style="font-size:0.75rem; color:var(--t-text3);">
                {{ t.subjectName || 'All subjects' }}
                <template v-if="t.gradeCode"> · {{ gradeLabel(t.gradeCode) }}</template>
              </div>
            </div>
            <span v-if="t.isActive" class="badge-green" style="flex-shrink:0;">Active</span>
            <span v-else style="font-size:0.7rem; font-weight:600; padding:0.2rem 0.45rem; border-radius:99px; background:rgba(148,163,184,0.12); color:var(--t-text3); flex-shrink:0;">Off</span>
          </div>
          <p style="margin:0; font-size:0.84rem; color:var(--t-text2); display:-webkit-box; -webkit-line-clamp:2; line-clamp:2; -webkit-box-orient:vertical; overflow:hidden; min-height:2.4em;">
            {{ t.description || '—' }}
          </p>
          <div style="display:flex; gap:0.5rem; border-top:1px solid var(--t-border); padding-top:0.5rem; margin-top:auto;">
            <button @click="openEdit(t)" class="btn-ghost" style="padding:0.3rem;" title="Edit"><Pencil class="w-3.5 h-3.5" /></button>
            <button @click="remove(t)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete"><Trash2 class="w-3.5 h-3.5" /></button>
          </div>
        </div>
        <div v-if="!loading && !filtered.length" style="grid-column:1/-1;">
          <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No tutors match your search.</p></div>
        </div>
      </div>

      <!-- LIST VIEW -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:48px;">#</th>
              <th>Tutor</th>
              <th>Subject</th>
              <th>Grade</th>
              <th>Description</th>
              <th style="width:80px;">Status</th>
              <th style="width:80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="7">
                <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading tutors…</p></div>
              </td>
            </tr>
            <tr v-for="(t, i) in filtered" :key="t.id">
              <td class="ag-table-muted" style="text-align:center;">{{ i + 1 }}</td>
              <td>
                <div style="display:flex; align-items:center; gap:0.6rem;">
                  <span :class="['tutor-ava-sm', t.color || 'grad-blue']">{{ resolveIcon(t.icon) }}</span>
                  <span style="font-weight:600;">{{ t.persona }}</span>
                </div>
              </td>
              <td class="ag-table-muted">{{ t.subjectName || 'All subjects' }}</td>
              <td class="ag-table-muted">{{ t.gradeCode ? gradeLabel(t.gradeCode) : 'All grades' }}</td>
              <td class="ag-table-muted" style="max-width:200px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">{{ t.description || '—' }}</td>
              <td>
                <span v-if="t.isActive" class="badge-green">Active</span>
                <span v-else style="font-size:0.72rem; font-weight:600; padding:0.2rem 0.45rem; border-radius:99px; background:rgba(148,163,184,0.12); color:var(--t-text3);">Off</span>
              </td>
              <td>
                <div class="flex gap-1">
                  <button @click="openEdit(t)" class="btn-ghost" style="padding:0.3rem;" title="Edit"><Pencil class="w-3.5 h-3.5" /></button>
                  <button @click="remove(t)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete"><Trash2 class="w-3.5 h-3.5" /></button>
                </div>
              </td>
            </tr>
            <tr v-if="!loading && !filtered.length">
              <td colspan="7">
                <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No tutors match your search.</p></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add / Edit Modal -->
    <div v-if="form" class="qb-overlay" @click.self="form = null">
      <div class="qb-modal" style="max-width:580px;">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">{{ editing ? 'Edit Tutor' : 'Add Tutor' }}</h3>
          <button @click="form = null" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>
        <form class="qb-modal-body" @submit.prevent="save">

          <!-- Preview strip -->
          <div style="display:flex; align-items:center; gap:0.75rem; padding:0.75rem 1rem; border-radius:12px; background:var(--t-bg); border:1px solid var(--t-border);">
            <span :class="['tutor-ava', form.color || 'grad-blue']">{{ resolveIcon(form.icon) }}</span>
            <div>
              <div style="font-weight:700; font-size:0.95rem;">{{ form.persona || 'Tutor Name' }}</div>
              <div style="font-size:0.78rem; color:var(--t-text3);">
                {{ subjectName || 'All subjects' }}
                <template v-if="form.gradeCode"> · {{ gradeLabel(form.gradeCode) }}</template>
              </div>
            </div>
          </div>

          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div>
              <label class="label">Persona name <span style="color:#ef4444;">*</span></label>
              <input v-model="form.persona" class="input" placeholder="e.g. Ustad Rafi" />
            </div>
            <div>
              <label class="label">Slug <span style="color:#ef4444;">*</span></label>
              <input v-model="form.slug" :disabled="editing" class="input" placeholder="ustad-rafi" />
            </div>
          </div>

          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div>
              <label class="label">Subject</label>
              <select v-model="form.subjectId" class="input">
                <option :value="null">All subjects</option>
                <option v-for="s in catalog.subjects" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
            </div>
            <div>
              <label class="label">Grade <span style="font-weight:400; color:var(--t-text3);">(optional)</span></label>
              <select v-model="form.gradeCode" class="input">
                <option :value="null">All grades</option>
                <option v-for="g in catalog.enabledGrades" :key="g.code" :value="g.code">{{ g.label }}</option>
              </select>
            </div>
          </div>

          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div>
              <label class="label">Icon</label>
              <select v-model="form.icon" class="input">
                <option v-for="o in ICON_OPTIONS" :key="o.emoji" :value="o.emoji">{{ o.emoji }} {{ o.label }}</option>
              </select>
            </div>
            <div>
              <label class="label">Color</label>
              <select v-model="form.color" class="input">
                <option v-for="c in COLOR_OPTIONS" :key="c.value" :value="c.value">{{ c.label }}</option>
              </select>
            </div>
          </div>

          <div>
            <label class="label">Description</label>
            <input v-model="form.description" class="input" placeholder="e.g. Your dedicated Mathematics guide" />
          </div>

          <div>
            <div style="display:flex; align-items:center; justify-content:space-between; margin-bottom:0.375rem;">
              <label class="label" style="margin:0;">Persona / system prompt</label>
              <button type="button" class="btn-secondary" style="padding:0.25rem 0.6rem; font-size:0.75rem;" @click="generatePrompt">
                ✨ Generate
              </button>
            </div>
            <textarea v-model="form.systemPrompt" rows="5" class="input" style="resize:vertical;" placeholder="You are … teach in simple language …" />
          </div>

          <div style="display:flex; align-items:center; gap:0.5rem;">
            <input v-model="form.isActive" type="checkbox" id="tutor-active" style="width:1rem; height:1rem;" />
            <label for="tutor-active" class="label" style="margin:0; cursor:pointer;">Active</label>
          </div>

          <p v-if="err" class="qb-err">{{ err }}</p>
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button type="submit" class="btn-primary" style="flex:1;" :disabled="saving">
              <span v-if="saving" class="qb-spinner"></span>
              <Plus v-else class="w-4 h-4" />
              {{ saving ? 'Saving…' : (editing ? 'Save Changes' : 'Add Tutor') }}
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
import api from '@/services/api'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'

const catalog = useCatalogStore()
const confirm = useConfirm()

const tutors  = ref([])
const form    = ref(null)
const editing = ref(false)
const saving  = ref(false)
const loading = ref(false)
const err     = ref('')
const search  = ref('')
const view    = ref('grid')

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return tutors.value
  return tutors.value.filter(t =>
    t.persona.toLowerCase().includes(q) ||
    (t.subjectName && t.subjectName.toLowerCase().includes(q))
  )
})

const LUCIDE_TO_EMOJI = {
  'calculator': '🧮', 'atom': '⚛️', 'flask': '⚗️', 'leaf': '🌿',
  'book-open': '📖', 'pen': '✒️', 'moon': '🌙', 'map': '🗺️',
  'monitor': '🖥️', 'microscope': '🔬', 'graduation-cap': '🎓', 'robot': '🤖',
}

function resolveIcon(icon) {
  if (!icon) return '🤖'
  return LUCIDE_TO_EMOJI[icon] ?? icon
}

const ICON_OPTIONS = [
  { emoji: '🤖', label: 'Robot (General)' }, { emoji: '⚡', label: 'Physics' },
  { emoji: '📐', label: 'Mathematics' },      { emoji: '⚗️', label: 'Chemistry' },
  { emoji: '🧬', label: 'Biology' },          { emoji: '💻', label: 'Computer Science' },
  { emoji: '📖', label: 'Urdu / Reading' },   { emoji: '✍️', label: 'English' },
  { emoji: '🌍', label: 'Social Studies' },   { emoji: '🏛️', label: 'Pakistan Studies' },
  { emoji: '🔬', label: 'Science' },          { emoji: '🧪', label: 'Lab / Experiments' },
  { emoji: '🕌', label: 'Islamiyat' },        { emoji: '🎓', label: 'Education' },
  { emoji: '🌿', label: 'Nature / Environment' }, { emoji: '🧮', label: 'Calculator' },
  { emoji: '⚛️', label: 'Atomic Physics' },   { emoji: '🗺️', label: 'Geography' },
  { emoji: '🖥️', label: 'Technology' },       { emoji: '✒️', label: 'Writing / Literature' },
]

const COLOR_OPTIONS = [
  { value: 'grad-blue',    label: '🔵 Blue' },    { value: 'grad-violet',  label: '🟣 Violet' },
  { value: 'grad-emerald', label: '💚 Emerald' }, { value: 'grad-rose',    label: '🌹 Rose' },
  { value: 'grad-amber',   label: '🟠 Amber' },   { value: 'grad-teal',    label: '🩵 Teal' },
  { value: 'grad-pink',    label: '💗 Pink' },    { value: 'grad-green',   label: '🟢 Green' },
  { value: 'grad-orange',  label: '🟧 Orange' },  { value: 'grad-cyan',    label: '🩵 Cyan' },
]

const subjectName = computed(() =>
  form.value?.subjectId
    ? (catalog.subjects.find(s => s.id === form.value.subjectId)?.name ?? '')
    : ''
)

function gradeLabel(code) {
  return catalog.grades.find(g => g.code === code)?.label ?? code
}

async function load() {
  loading.value = true
  try { tutors.value = (await api.get('/admin/tutors')).data }
  finally { loading.value = false }
}

onMounted(async () => {
  await Promise.all([load(), catalog.fetchGrades(), catalog.fetchAllSubjects()])
})

function blankForm() {
  return { persona: '', slug: '', subjectId: null, gradeCode: null,
           icon: '🤖', color: 'grad-blue', description: '', systemPrompt: '', isActive: true }
}

function openCreate() {
  editing.value = false
  err.value = ''
  form.value = blankForm()
}

function openEdit(t) {
  editing.value = true
  err.value = ''
  form.value = {
    id: t.id, persona: t.persona, slug: t.slug,
    subjectId: t.subjectId ?? null, gradeCode: t.gradeCode ?? null,
    icon: resolveIcon(t.icon), color: t.color || 'grad-blue',
    description: t.description || '', systemPrompt: t.systemPrompt || '',
    isActive: !!t.isActive,
  }
}

async function save() {
  saving.value = true; err.value = ''
  try {
    const payload = { ...form.value }
    if (editing.value) await api.put(`/admin/tutors/${form.value.id}`, payload)
    else               await api.post('/admin/tutors', payload)
    await load()
    await catalog.fetchTutors()
    form.value = null
  } catch (e) {
    err.value = e?.response?.data?.title ?? e?.response?.data ?? e?.message ?? 'Failed to save'
  } finally {
    saving.value = false
  }
}

async function remove(t) {
  const ok = await confirm({
    title: `Delete "${t.persona}"?`,
    message: 'This AI tutor will be permanently removed and unavailable to students.',
    confirmLabel: 'Delete Tutor',
  })
  if (!ok) return
  try {
    await api.delete(`/admin/tutors/${t.id}`)
    await load()
    await catalog.fetchTutors()
  } catch (e) { err.value = e.message }
}

function generatePrompt() {
  const subject  = catalog.subjects.find(s => s.id === form.value.subjectId)
  const grade    = catalog.grades.find(g => g.code === form.value.gradeCode)
  const name     = form.value.persona || 'an expert teacher'
  const subName  = subject?.name ?? 'the subject'
  const gradeLbl = grade ? ` for ${grade.label} students` : ''

  form.value.systemPrompt =
`You are ${name}, a dedicated ${subName} teacher${gradeLbl} at Balochistan Academy.

Your role is to help students understand ${subName} concepts clearly and engagingly.

Guidelines:
- Use simple, clear language appropriate for ${grade?.label ?? "the student's level"}
- Break complex topics into easy steps with examples
- Use real-world examples and local context where possible
- Encourage curiosity and reward questions
- Be patient, supportive, and motivating
- If a student is confused, explain from a different angle

Always respond in the language the student uses (Urdu or English).`
}
</script>

<style scoped>
.tutor-ava    { display:grid; place-items:center; width:42px; height:42px; border-radius:12px; font-size:21px; flex-shrink:0; color:#fff; }
.tutor-ava-sm { display:grid; place-items:center; width:28px; height:28px; border-radius:8px; font-size:14px; flex-shrink:0; color:#fff; }
.qb-overlay { position:fixed; inset:0; z-index:1000; background:rgba(0,0,0,0.45); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; padding:1.25rem; }
.qb-modal { background:var(--t-bg2); border:1px solid var(--t-border); border-radius:16px; width:100%; max-width:520px; max-height:calc(100vh - 2.5rem); overflow-y:auto; box-shadow:0 20px 60px rgba(0,0,0,0.3); }
.qb-modal-header { display:flex; align-items:center; justify-content:space-between; padding:1.25rem 1.5rem 0; }
.qb-modal-title { font-size:1rem; font-weight:700; color:var(--t-text1); margin:0; }
.qb-modal-body { padding:1.25rem 1.5rem 1.5rem; display:flex; flex-direction:column; gap:1rem; }
.qb-spinner { display:inline-block; width:12px; height:12px; border:2px solid rgba(255,255,255,0.4); border-top-color:white; border-radius:50%; animation:qbspin 0.7s linear infinite; }
@keyframes qbspin { to { transform:rotate(360deg); } }
.qb-err { margin:0; padding:10px 14px; border-radius:9px; font-size:0.83rem; background:#fef2f2; border:1px solid #fecaca; color:#b91c1c; }
</style>
