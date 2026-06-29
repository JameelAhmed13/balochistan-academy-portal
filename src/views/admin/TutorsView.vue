<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>AI Tutors</h1>
        <p>Persona tutors students chat with. Assign a subject and optionally a grade, then write the persona prompt.</p>
      </div>
      <button class="adm-btn" @click="openCreate">+ Add Tutor</button>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="adm-loading">Loading tutors…</div>

    <!-- Empty -->
    <div v-else-if="!tutors.length" class="adm-empty">
      No tutors yet — click "Add Tutor" to create the first one.
    </div>

    <!-- Grid -->
    <div v-else class="adm-grid">
      <article v-for="t in tutors" :key="t.id" class="adm-card">
        <div class="adm-card-top">
          <span :class="['adm-ava', t.color || 'grad-blue']">{{ resolveIcon(t.icon) }}</span>
          <div class="adm-card-info">
            <b>{{ t.persona }}</b>
            <span class="adm-sub">
              {{ t.subjectName || 'All subjects' }}
              <template v-if="t.gradeCode"> · {{ gradeLabel(t.gradeCode) }}</template>
            </span>
          </div>
          <span :class="['adm-pill', t.isActive ? 'on' : 'off']">{{ t.isActive ? 'Active' : 'Off' }}</span>
        </div>
        <p class="adm-desc">{{ t.description || '—' }}</p>
        <div class="adm-actions">
          <button class="adm-link" @click="openEdit(t)">Edit</button>
          <button class="adm-link danger" @click="remove(t)">Delete</button>
        </div>
      </article>
    </div>

    <!-- Modal -->
    <div v-if="form" class="adm-modal" @click.self="form = null">
      <div class="adm-dialog">
        <h2>{{ editing ? 'Edit Tutor' : 'Add Tutor' }}</h2>

        <!-- Preview strip -->
        <div class="adm-preview">
          <span :class="['adm-ava', form.color || 'grad-blue']">{{ resolveIcon(form.icon) }}</span>
          <div>
            <div class="adm-preview-name">{{ form.persona || 'Tutor Name' }}</div>
            <div class="adm-preview-sub">
              {{ subjectName || 'All subjects' }}
              <template v-if="form.gradeCode"> · {{ gradeLabel(form.gradeCode) }}</template>
            </div>
          </div>
        </div>

        <div class="adm-row2">
          <label>Persona name <span class="adm-req">*</span>
            <input v-model="form.persona" placeholder="e.g. Ustad Rafi" />
          </label>
          <label>Slug <span class="adm-req">*</span>
            <input v-model="form.slug" :disabled="editing" placeholder="ustad-rafi" />
          </label>
        </div>

        <div class="adm-row2">
          <label>Subject
            <select v-model="form.subjectId">
              <option :value="null">All subjects</option>
              <option v-for="s in catalog.subjects" :key="s.id" :value="s.id">{{ s.name }}</option>
            </select>
          </label>
          <label>Grade <span class="adm-opt">(optional)</span>
            <select v-model="form.gradeCode">
              <option :value="null">All grades</option>
              <option v-for="g in catalog.enabledGrades" :key="g.code" :value="g.code">{{ g.label }}</option>
            </select>
          </label>
        </div>

        <div class="adm-row2">
          <label>Icon
            <select v-model="form.icon">
              <option v-for="o in ICON_OPTIONS" :key="o.emoji" :value="o.emoji">{{ o.emoji }} {{ o.label }}</option>
            </select>
          </label>
          <label>Color
            <select v-model="form.color">
              <option v-for="c in COLOR_OPTIONS" :key="c.value" :value="c.value">{{ c.label }}</option>
            </select>
          </label>
        </div>

        <label>Description
          <input v-model="form.description" placeholder="e.g. Your dedicated Mathematics guide" />
        </label>

        <label>
          <div class="adm-label-row">
            <span>Persona / system prompt</span>
            <button type="button" class="adm-gen-btn" @click="generatePrompt" title="Auto-generate from subject & grade">
              ✨ Generate
            </button>
          </div>
          <textarea v-model="form.systemPrompt" rows="6" placeholder="You are … teach in simple language …" />
        </label>

        <label class="adm-check">
          <input v-model="form.isActive" type="checkbox" /> Active
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
import { ref, computed, onMounted } from 'vue'
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

// ── Lucide-name → emoji (legacy seeded data used icon names, not emojis) ─────
const LUCIDE_TO_EMOJI = {
  'calculator':     '🧮',
  'atom':           '⚛️',
  'flask':          '⚗️',
  'leaf':           '🌿',
  'book-open':      '📖',
  'pen':            '✒️',
  'moon':           '🌙',
  'map':            '🗺️',
  'monitor':        '🖥️',
  'microscope':     '🔬',
  'graduation-cap': '🎓',
  'robot':          '🤖',
}

function resolveIcon(icon) {
  if (!icon) return '🤖'
  return LUCIDE_TO_EMOJI[icon] ?? icon
}

// ── Static Options ────────────────────────────────────────────
const ICON_OPTIONS = [
  { emoji: '🤖', label: 'Robot (General)' },
  { emoji: '⚡', label: 'Physics' },
  { emoji: '📐', label: 'Mathematics' },
  { emoji: '⚗️', label: 'Chemistry' },
  { emoji: '🧬', label: 'Biology' },
  { emoji: '💻', label: 'Computer Science' },
  { emoji: '📖', label: 'Urdu / Reading' },
  { emoji: '✍️', label: 'English' },
  { emoji: '🌍', label: 'Social Studies' },
  { emoji: '🏛️', label: 'Pakistan Studies' },
  { emoji: '🔬', label: 'Science' },
  { emoji: '🧪', label: 'Lab / Experiments' },
  { emoji: '🕌', label: 'Islamiyat' },
  { emoji: '🎓', label: 'Education' },
  { emoji: '🌿', label: 'Nature / Environment' },
  { emoji: '🧮', label: 'Calculator' },
  { emoji: '⚛️', label: 'Atomic Physics' },
  { emoji: '🗺️', label: 'Geography' },
  { emoji: '🖥️', label: 'Technology' },
  { emoji: '✒️', label: 'Writing / Literature' },
]

const COLOR_OPTIONS = [
  { value: 'grad-blue',    label: '🔵 Blue' },
  { value: 'grad-violet',  label: '🟣 Violet' },
  { value: 'grad-emerald', label: '💚 Emerald' },
  { value: 'grad-rose',    label: '🌹 Rose' },
  { value: 'grad-amber',   label: '🟠 Amber' },
  { value: 'grad-teal',    label: '🩵 Teal' },
  { value: 'grad-pink',    label: '💗 Pink' },
  { value: 'grad-green',   label: '🟢 Green' },
  { value: 'grad-orange',  label: '🟧 Orange' },
  { value: 'grad-cyan',    label: '🩵 Cyan' },
]

// ── Helpers ───────────────────────────────────────────────────
const subjectName = computed(() =>
  form.value?.subjectId
    ? (catalog.subjects.find(s => s.id === form.value.subjectId)?.name ?? '')
    : ''
)

function gradeLabel(code) {
  return catalog.grades.find(g => g.code === code)?.label ?? code
}

// ── Load ──────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try { tutors.value = (await api.get('/admin/tutors')).data }
  finally { loading.value = false }
}

onMounted(async () => {
  await Promise.all([load(), catalog.fetchGrades(), catalog.fetchAllSubjects()])
})

// ── CRUD ──────────────────────────────────────────────────────
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
    id:           t.id,
    persona:      t.persona,
    slug:         t.slug,
    subjectId:    t.subjectId ?? null,
    gradeCode:    t.gradeCode ?? null,
    icon:         resolveIcon(t.icon),
    color:        t.color || 'grad-blue',
    description:  t.description || '',
    systemPrompt: t.systemPrompt || '',
    isActive:     !!t.isActive,
  }
}

async function save() {
  saving.value = true
  err.value = ''
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

// ── System prompt generator ───────────────────────────────────
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
.adm { max-width: none; margin: 0; padding: 24px 8px; color: var(--t-text1); }
.adm-head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; margin-bottom: 24px; }
.adm-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.9rem; margin: 0 0 4px; }
.adm-head p { margin: 0; color: var(--t-text2); font-size: .9rem; max-width: 560px; }

.adm-btn { padding: 10px 18px; border: 0; border-radius: 11px; cursor: pointer; font-weight: 700; font-size: .875rem; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.adm-btn.ghost { background: var(--t-hover); color: var(--t-text1); border: 1px solid var(--t-border); }
.adm-btn:disabled { opacity: .6; cursor: not-allowed; }

.adm-loading { padding: 32px 0; color: var(--t-text3); font-size: .9rem; }
.adm-empty   { padding: 48px 0; text-align: center; color: var(--t-text3); font-size: .9rem; }

.adm-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(290px, 1fr)); gap: 14px; }
.adm-card { padding: 18px; border-radius: 16px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 10px; }
.adm-card-top { display: flex; align-items: center; gap: 11px; }
.adm-ava { display: grid; place-items: center; width: 42px; height: 42px; border-radius: 12px; font-size: 21px; flex-shrink: 0; color: #fff; }
.adm-card-info { flex: 1; min-width: 0; }
.adm-card-info b { display: block; font-size: .95rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.adm-sub { font-size: .76rem; color: var(--t-text3); }
.adm-card-top .adm-pill { margin-left: auto; flex-shrink: 0; }
.adm-desc { margin: 0; font-size: .85rem; color: var(--t-text2); min-height: 34px; display: -webkit-box; -webkit-line-clamp: 2; line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.adm-actions { display: flex; gap: 14px; }
.adm-link { background: none; border: 0; cursor: pointer; color: var(--t-accent); font-weight: 600; font-size: .85rem; padding: 0; }
.adm-link.danger { color: #f87171; }

.adm-pill { padding: 3px 10px; border-radius: 99px; font-size: .72rem; font-weight: 700; }
.adm-pill.on  { background: rgba(52,211,153,.15); color: #34d399; }
.adm-pill.off { background: rgba(148,163,184,.15); color: var(--t-text3); }

/* ── Modal ───────────────────────────────────────────────────── */
.adm-modal { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; background: rgba(0,0,0,.5); backdrop-filter: blur(4px); padding: 20px; }
.adm-dialog { width: min(560px, 100%); max-height: 92vh; overflow-y: auto; padding: 28px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 14px; }
.adm-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; }

/* Preview strip */
.adm-preview { display: flex; align-items: center; gap: 12px; padding: 12px 14px; border-radius: 12px; background: var(--t-bg); border: 1px solid var(--t-border); }
.adm-preview-name { font-weight: 700; font-size: .95rem; }
.adm-preview-sub  { font-size: .78rem; color: var(--t-text3); }

.adm-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.adm-dialog input:not([type=checkbox]),
.adm-dialog select,
.adm-dialog textarea {
  padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border);
  background: var(--t-bg); color: var(--t-text1); font: inherit;
  transition: border-color .15s;
}
.adm-dialog input:not([type=checkbox]):focus,
.adm-dialog select:focus,
.adm-dialog textarea:focus { outline: none; border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-acc-alpha-sm); }
.adm-dialog input:disabled { opacity: .5; cursor: not-allowed; }

.adm-row2 { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.adm-check { flex-direction: row !important; align-items: center; gap: 8px; cursor: pointer; }
.adm-req { color: #f87171; font-weight: 400; }
.adm-opt { font-weight: 400; color: var(--t-text3); }

/* Generate button row */
.adm-label-row { display: flex; align-items: center; justify-content: space-between; }
.adm-gen-btn {
  padding: 3px 10px; border-radius: 7px; border: 1px solid var(--t-border);
  background: var(--t-bg); color: var(--t-accent); font: inherit; font-size: .75rem;
  font-weight: 700; cursor: pointer; transition: all .15s;
}
.adm-gen-btn:hover { background: var(--t-acc-alpha-sm); border-color: var(--t-accent); }

.adm-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 4px; }
.adm-err { color: #f87171; font-size: .85rem; margin: 0; padding: 8px 12px; background: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; }
</style>
