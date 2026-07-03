<template>
  <div class="animate-fade-in space-y-6">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">⚙️</div>
      <div class="ag-banner-title">Platform Settings</div>
      <div class="ag-banner-sub">Configure the platform, AI, subscriptions, and branding</div>
      <div class="ag-banner-actions">
        <RouterLink to="/app/admin" class="btn-ghost flex items-center gap-1.5 text-sm">
          <ArrowLeft class="w-4 h-4" /> Back
        </RouterLink>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="ag-card">
      <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading settings…</p></div>
    </div>

    <template v-else>

    <!-- ─── Panel 1: General Platform ──────────────────────────────────────── -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:var(--t-accent);">
        <Globe class="w-4 h-4" /> General Platform
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="label">Platform Name</label>
            <input v-model="g.site_name" class="input mt-1" placeholder="Balochistan Academy Portal" />
          </div>
          <div>
            <label class="label">Institute Name</label>
            <input v-model="g.institute_name" class="input mt-1" placeholder="UltraSoft Systems" />
          </div>
          <div>
            <label class="label">Support WhatsApp</label>
            <input v-model="g.support_whatsapp" class="input mt-1" placeholder="+923703153540" />
          </div>
        </div>

        <!-- Grade multi-select -->
        <div class="mt-4">
          <label class="label">Visible Grades (students see only these on registration)</label>
          <p class="text-xs" style="color:var(--t-text3); margin-bottom:0.5rem;">
            Leave all unchecked to show every enabled grade.
          </p>
          <div v-if="allGrades.length" class="grade-checkboxes">
            <label v-for="gr in allGrades" :key="gr.code" class="grade-chip"
                   :class="{ active: g.allowed_grades_set.has(gr.code) }">
              <input type="checkbox" :value="gr.code"
                     :checked="g.allowed_grades_set.has(gr.code)"
                     @change="toggleGrade(gr.code)" />
              {{ gr.label || gr.code }}
            </label>
          </div>
          <div v-else class="ag-empty" style="padding:0.75rem;">
            <p>No grades found</p>
          </div>
        </div>

        <!-- Registration toggle -->
        <div class="flex items-center gap-3 mt-4">
          <span class="label mb-0">Allow New Registrations</span>
          <div class="toggle-wrap">
            <input type="checkbox" :checked="g.registration_open === 'true'"
                   @change="g.registration_open = $event.target.checked ? 'true' : 'false'"
                   id="regOpen" />
            <label for="regOpen" class="toggle-pill"></label>
          </div>
          <span class="text-xs" style="color:var(--t-text3);">
            {{ g.registration_open === 'true' ? 'Open' : 'Closed' }}
          </span>
        </div>

        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" :disabled="saving.general" @click="saveGeneral">
            <Save class="w-4 h-4" />
            {{ saving.general ? 'Saving…' : 'Save General Settings' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ─── Panel 2: Subscription & Pricing ────────────────────────────────── -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:#10b981;">
        <Shield class="w-4 h-4" /> Subscription &amp; Pricing
      </div>
      <div class="ag-card-body">

        <!-- Plans table -->
        <div class="flex items-center justify-between mb-3">
          <p class="text-sm" style="color:var(--t-text2);">
            Subscription plans ({{ plans.filter(p => p.isActive).length }} active)
          </p>
          <button class="btn-primary text-sm" style="padding:0.4rem 0.9rem;" @click="openPlanModal(null)">
            + Add Plan
          </button>
        </div>

        <div v-if="plans.length" class="ag-table-wrap">
          <table class="ag-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Price (PKR)</th>
                <th>Duration</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in plans" :key="p.id">
                <td style="font-weight:600;">{{ p.name }}</td>
                <td>{{ p.price.toLocaleString() }}</td>
                <td>{{ p.durationDays }} days</td>
                <td>
                  <span :class="p.isActive ? 'badge-green' : 'badge-gray'">
                    {{ p.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td>
                  <div class="flex gap-1.5">
                    <button class="btn-ghost text-xs" style="padding:0.25rem 0.6rem;" @click="openPlanModal(p)">Edit</button>
                    <button class="btn-ghost text-xs" style="padding:0.25rem 0.6rem; color:#ef4444;" @click="deletePlan(p.id)">
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-else class="ag-empty" style="padding:1.5rem;">
          <div class="ag-empty-icon">📋</div><p>No subscription plans yet</p>
        </div>

        <!-- Global subscription settings -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-5 pt-5" style="border-top:1px solid var(--t-border);">
          <div>
            <label class="label">Trial Days</label>
            <input v-model.number="sub.trial_days" type="number" min="0" class="input mt-1" />
          </div>
          <div class="flex items-center gap-3 pt-6">
            <input type="checkbox" :checked="sub.subscription_auto_expire === 'true'"
                   @change="sub.subscription_auto_expire = $event.target.checked ? 'true' : 'false'"
                   id="autoExpire" class="w-4 h-4" />
            <label for="autoExpire" class="label mb-0" style="cursor:pointer;">Auto-Expire Subscriptions</label>
          </div>
          <div class="flex items-center gap-3 pt-6">
            <input type="checkbox" :checked="sub.subscription_activate_on_payment === 'true'"
                   @change="sub.subscription_activate_on_payment = $event.target.checked ? 'true' : 'false'"
                   id="activatePayment" class="w-4 h-4" />
            <label for="activatePayment" class="label mb-0" style="cursor:pointer;">Activate on Payment</label>
          </div>
        </div>
        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" :disabled="saving.subscription" @click="saveSubscription">
            <Save class="w-4 h-4" />
            {{ saving.subscription ? 'Saving…' : 'Save Subscription Settings' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ─── Panel 3: Coin Economy ───────────────────────────────────────────── -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:var(--t-gold);">
        <Coins class="w-4 h-4" /> Coin Economy
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div>
            <label class="label">Coin Rate (PKR per coin)</label>
            <input v-model="coin.coin_rate_pkr" type="number" step="0.01" class="input mt-1" />
          </div>
          <div>
            <label class="label">Min Withdrawal Coins</label>
            <input v-model.number="coin.min_withdrawal" type="number" class="input mt-1" />
          </div>
          <div>
            <label class="label">Min Questions for Eligibility</label>
            <input v-model.number="coin.min_questions_for_eligibility" type="number" class="input mt-1" />
          </div>
          <div>
            <label class="label">Daily Login Bonus</label>
            <input v-model.number="coin.daily_login_bonus" type="number" class="input mt-1" />
          </div>
          <div>
            <label class="label">Coins for ≥90% score</label>
            <input v-model.number="coin.coins_per_90pct" type="number" class="input mt-1" />
          </div>
          <div>
            <label class="label">Coins for 70–89% score</label>
            <input v-model.number="coin.coins_per_70pct" type="number" class="input mt-1" />
          </div>
          <div>
            <label class="label">Coins for 50–69% score</label>
            <input v-model.number="coin.coins_per_50pct" type="number" class="input mt-1" />
          </div>
          <div>
            <label class="label">Coins for attempted (&lt;50%)</label>
            <input v-model.number="coin.coins_per_pass" type="number" class="input mt-1" />
          </div>
        </div>
        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" :disabled="saving.coins" @click="saveCoins">
            <Save class="w-4 h-4" />
            {{ saving.coins ? 'Saving…' : 'Save Coin Settings' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ─── Panel 4: AI & Features ──────────────────────────────────────────── -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:#8b5cf6;">
        <Bot class="w-4 h-4" /> AI &amp; Features
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="label">Gemini API Key</label>
            <div class="input-secret-wrap mt-1">
              <input v-model="ai.gemini_api_key" :type="show.gemini ? 'text' : 'password'"
                     class="input font-mono" style="padding-right:2.5rem;" placeholder="AIza…" />
              <button @click="show.gemini = !show.gemini" class="input-secret-eye" type="button">
                {{ show.gemini ? '🙈' : '👁️' }}
              </button>
            </div>
          </div>
          <div>
            <label class="label">Gemini Model</label>
            <input v-model="ai.gemini_model" class="input mt-1" placeholder="gemini-2.5-flash-lite" />
          </div>
          <div>
            <label class="label">TTS API Key</label>
            <div class="input-secret-wrap mt-1">
              <input v-model="ai.tts_api_key" :type="show.tts ? 'text' : 'password'"
                     class="input font-mono" style="padding-right:2.5rem;" placeholder="AIza…" />
              <button @click="show.tts = !show.tts" class="input-secret-eye" type="button">
                {{ show.tts ? '🙈' : '👁️' }}
              </button>
            </div>
          </div>
          <div>
            <label class="label">Default AI Language</label>
            <select v-model="ai.ai_default_language" class="input mt-1">
              <option value="english">English</option>
              <option value="urdu">Urdu</option>
              <option value="both">Both</option>
            </select>
          </div>
          <div>
            <label class="label">Ollama URL</label>
            <input v-model="ai.ollama_url" class="input mt-1" placeholder="http://localhost:11434" />
          </div>
          <div>
            <label class="label">Ollama Model</label>
            <input v-model="ai.ollama_model" class="input mt-1" placeholder="llama3" />
          </div>
          <div>
            <label class="label">Ollama Timeout (seconds)</label>
            <input v-model.number="ai.ollama_timeout_sec" type="number" class="input mt-1" />
          </div>
        </div>

        <div class="flex flex-wrap gap-6 mt-4">
          <label class="toggle-row">
            <span class="label mb-0">Enable AI Tutor</span>
            <div class="toggle-wrap">
              <input type="checkbox" :checked="ai.ai_tutor_enabled === 'true'"
                     @change="ai.ai_tutor_enabled = $event.target.checked ? 'true' : 'false'"
                     id="enableTutor" />
              <label for="enableTutor" class="toggle-pill"></label>
            </div>
          </label>
          <label class="toggle-row">
            <span class="label mb-0">Enable Video Lessons</span>
            <div class="toggle-wrap">
              <input type="checkbox" :checked="ai.video_lessons_enabled === 'true'"
                     @change="ai.video_lessons_enabled = $event.target.checked ? 'true' : 'false'"
                     id="enableVideo" />
              <label for="enableVideo" class="toggle-pill"></label>
            </div>
          </label>
          <label class="toggle-row">
            <span class="label mb-0">Enable AI Grading</span>
            <div class="toggle-wrap">
              <input type="checkbox" :checked="ai.ai_grading_enabled === 'true'"
                     @change="ai.ai_grading_enabled = $event.target.checked ? 'true' : 'false'"
                     id="enableGrading" />
              <label for="enableGrading" class="toggle-pill"></label>
            </div>
          </label>
        </div>

        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" :disabled="saving.ai" @click="saveAi">
            <Save class="w-4 h-4" />
            {{ saving.ai ? 'Saving…' : 'Save AI Settings' }}
          </button>
        </div>
      </div>
    </div>

    </template><!-- end v-else -->

    <!-- ─── Plan modal ──────────────────────────────────────────────────────── -->
    <div v-if="planModal.open" class="qb-overlay" @click.self="planModal.open = false">
      <div class="qb-modal">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">{{ planModal.id ? 'Edit Plan' : 'New Subscription Plan' }}</h3>
          <button @click="planModal.open = false" class="btn-ghost" style="padding:0.4rem;">
            <X class="w-4 h-4" />
          </button>
        </div>
        <div class="qb-modal-body">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="sm:col-span-2">
              <label class="label">Plan Name</label>
              <input v-model="planModal.name" class="input mt-1" placeholder="Monthly" />
            </div>
            <div class="sm:col-span-2">
              <label class="label">Description</label>
              <div class="desc-editor mt-1">
                <!-- Toolbar -->
                <div class="desc-editor-toolbar">
                  <button type="button" class="desc-tb-btn"
                    :class="{ active: descEditor?.isActive('bold') }"
                    @click="descEditor?.chain().focus().toggleBold().run()"
                    title="Bold">
                    <Bold class="w-3.5 h-3.5" />
                  </button>
                  <button type="button" class="desc-tb-btn"
                    :class="{ active: descEditor?.isActive('italic') }"
                    @click="descEditor?.chain().focus().toggleItalic().run()"
                    title="Italic">
                    <Italic class="w-3.5 h-3.5" />
                  </button>
                  <button type="button" class="desc-tb-btn"
                    :class="{ active: descEditor?.isActive('underline') }"
                    @click="descEditor?.chain().focus().toggleUnderline().run()"
                    title="Underline">
                    <UnderlineIcon class="w-3.5 h-3.5" />
                  </button>
                  <div class="desc-tb-sep" />
                  <button type="button" class="desc-tb-btn"
                    :class="{ active: descEditor?.isActive('bulletList') }"
                    @click="descEditor?.chain().focus().toggleBulletList().run()"
                    title="Bullet list">
                    <List class="w-3.5 h-3.5" />
                  </button>
                  <button type="button" class="desc-tb-btn"
                    :class="{ active: descEditor?.isActive('heading', { level: 3 }) }"
                    @click="descEditor?.chain().focus().toggleHeading({ level: 3 }).run()"
                    title="Heading">
                    <span class="desc-tb-label">H</span>
                  </button>
                  <div class="desc-tb-sep" />
                  <!-- Text color -->
                  <label class="desc-tb-btn desc-color-btn" title="Text color">
                    <span class="desc-color-indicator"
                          :style="{ background: descEditor?.getAttributes('textStyle').color || '#374151' }"></span>
                    <input type="color" class="desc-color-input"
                           :value="descEditor?.getAttributes('textStyle').color || '#374151'"
                           @input="descEditor?.chain().focus().setColor($event.target.value).run()" />
                  </label>
                  <!-- Highlight -->
                  <div class="desc-highlight-wrap">
                    <button type="button" class="desc-tb-btn desc-hl-trigger"
                      :class="{ active: descEditor?.isActive('highlight') }"
                      title="Highlight color"
                      @click.stop="showHlPicker = !showHlPicker">
                      <span class="desc-hl-icon">🖊</span>
                    </button>
                    <div v-if="showHlPicker" class="desc-hl-picker" @click.stop>
                      <button v-for="c in HIGHLIGHT_COLORS" :key="c.val" type="button"
                        class="desc-hl-swatch"
                        :style="{ background: c.val }"
                        :title="c.label"
                        @click="descEditor?.chain().focus().setHighlight({ color: c.val }).run(); showHlPicker = false">
                      </button>
                      <button type="button" class="desc-hl-clear"
                        @click="descEditor?.chain().focus().unsetHighlight().run(); showHlPicker = false">
                        ✕
                      </button>
                    </div>
                  </div>
                  <div class="desc-tb-sep" />
                  <button type="button" class="desc-tb-btn"
                    @click="descEditor?.chain().focus().clearNodes().unsetAllMarks().run()"
                    title="Clear formatting">
                    <span class="desc-tb-label" style="font-size:0.7rem;">T×</span>
                  </button>
                </div>
                <!-- Editor area -->
                <EditorContent :editor="descEditor" />
              </div>
            </div>
            <div>
              <label class="label">Price (PKR)</label>
              <input v-model.number="planModal.price" type="number" min="0" class="input mt-1" />
            </div>
            <div>
              <label class="label">Duration (Days)</label>
              <input v-model.number="planModal.durationDays" type="number" min="1" class="input mt-1" />
            </div>
            <div>
              <label class="label">Sort Order</label>
              <input v-model.number="planModal.sortOrder" type="number" class="input mt-1" />
            </div>
            <div class="flex items-center gap-3 pt-5">
              <input type="checkbox" v-model="planModal.isActive" id="planActive" class="w-4 h-4" />
              <label for="planActive" class="label mb-0" style="cursor:pointer;">Active</label>
            </div>
          </div>
          <div v-if="planModal.err" class="qb-err mt-3">{{ planModal.err }}</div>
          <div class="flex gap-3 mt-4">
            <button class="btn-primary flex-1" :disabled="planModal.saving" @click="savePlan">
              {{ planModal.saving ? 'Saving…' : (planModal.id ? 'Update Plan' : 'Create Plan') }}
            </button>
            <button class="btn-ghost" @click="planModal.open = false">Cancel</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Toast -->
    <Transition name="toast">
      <div v-if="toast" class="fixed bottom-6 right-6 z-50 px-5 py-3 rounded-xl shadow-lg text-sm font-medium flex items-center gap-2"
        style="background:var(--t-accent);color:#fff;">
        <Save class="w-4 h-4" /> {{ toast }}
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch, nextTick } from 'vue'
import { ArrowLeft, Globe, Coins, Bot, Shield, Save, X, Bold, Italic, Underline as UnderlineIcon, List } from '@lucide/vue'
import { useEditor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Underline from '@tiptap/extension-underline'
import Placeholder from '@tiptap/extension-placeholder'
import { TextStyle, Color } from '@tiptap/extension-text-style'
import Highlight from '@tiptap/extension-highlight'
import api from '@/services/api'
import { useSettingsStore } from '@/stores/settings'

const settingsStore = useSettingsStore()
const loading = ref(true)
const toast = ref('')
let toastTimer = null

function showToast(msg = 'Saved!') {
  clearTimeout(toastTimer)
  toast.value = msg
  toastTimer = setTimeout(() => { toast.value = '' }, 2500)
}

// ── Panel 1 state ────────────────────────────────────────────────────────────
const g = reactive({
  site_name: '',
  institute_name: '',
  support_whatsapp: '',
  registration_open: 'true',
  allowed_grades: '',
  allowed_grades_set: new Set(),
})
const allGrades = ref([])

function toggleGrade(code) {
  if (g.allowed_grades_set.has(code)) g.allowed_grades_set.delete(code)
  else g.allowed_grades_set.add(code)
  g.allowed_grades = [...g.allowed_grades_set].join(',')
}

// ── Panel 2 state ────────────────────────────────────────────────────────────
const sub = reactive({
  trial_days: '7',
  subscription_auto_expire: 'true',
  subscription_activate_on_payment: 'false',
})
const plans = ref([])
const planModal = reactive({
  open: false, id: null, name: '', description: '', price: 0,
  durationDays: 30, sortOrder: 0, isActive: true, saving: false, err: '',
})

const showHlPicker = ref(false)
const HIGHLIGHT_COLORS = [
  { val: '#fef08a', label: 'Yellow' },
  { val: '#bbf7d0', label: 'Green' },
  { val: '#bfdbfe', label: 'Blue' },
  { val: '#fecaca', label: 'Red' },
  { val: '#e9d5ff', label: 'Purple' },
  { val: '#fed7aa', label: 'Orange' },
  { val: '#f9a8d4', label: 'Pink' },
]

// TipTap editor for plan description
const descEditor = useEditor({
  extensions: [
    StarterKit,
    Underline,
    TextStyle,
    Color,
    Highlight.configure({ multicolor: true }),
    Placeholder.configure({ placeholder: 'Write a description…' }),
  ],
  content: '',
  editorProps: {
    attributes: { class: 'desc-editor-content' },
  },
})

watch(() => planModal.open, (isOpen) => {
  showHlPicker.value = false
  if (isOpen) {
    nextTick(() => descEditor.value?.commands.setContent(planModal.description || '', false))
  }
})

function openPlanModal(plan) {
  planModal.err = ''
  if (plan) {
    Object.assign(planModal, {
      open: true, id: plan.id, name: plan.name, description: plan.description || '',
      price: plan.price, durationDays: plan.durationDays, sortOrder: plan.sortOrder,
      isActive: plan.isActive, saving: false,
    })
  } else {
    Object.assign(planModal, {
      open: true, id: null, name: '', description: '', price: 0,
      durationDays: 30, sortOrder: 0, isActive: true, saving: false,
    })
  }
}

async function savePlan() {
  if (!planModal.name.trim()) { planModal.err = 'Plan name is required.'; return }
  planModal.saving = true; planModal.err = ''
  try {
    const descHtml = descEditor.value?.getHTML() ?? ''
    const payload = {
      name: planModal.name,
      description: descHtml === '<p></p>' ? null : descHtml,
      price: planModal.price, durationDays: planModal.durationDays,
      sortOrder: planModal.sortOrder, isActive: planModal.isActive,
    }
    if (planModal.id) {
      const { data } = await api.put(`/admin/subscriptions/${planModal.id}`, payload)
      const idx = plans.value.findIndex(p => p.id === planModal.id)
      if (idx !== -1) plans.value[idx] = data
    } else {
      const { data } = await api.post('/admin/subscriptions', payload)
      plans.value.push(data)
    }
    planModal.open = false
    showToast('Plan saved!')
  } catch (e) {
    planModal.err = e?.response?.data?.error || 'Failed to save plan.'
  } finally { planModal.saving = false }
}

async function deletePlan(id) {
  if (!confirm('Deactivate this subscription plan?')) return
  await api.delete(`/admin/subscriptions/${id}`)
  const p = plans.value.find(x => x.id === id)
  if (p) p.isActive = false
  showToast('Plan deactivated')
}

// ── Panel 3 state ────────────────────────────────────────────────────────────
const coin = reactive({
  coin_rate_pkr: '0.50',
  min_withdrawal: '300',
  min_questions_for_eligibility: '50',
  daily_login_bonus: '2',
  coins_per_90pct: '50',
  coins_per_70pct: '30',
  coins_per_50pct: '15',
  coins_per_pass: '5',
})

// ── Panel 4 state ────────────────────────────────────────────────────────────
const ai = reactive({
  gemini_api_key: '',
  gemini_model: 'gemini-2.5-flash-lite',
  tts_api_key: '',
  ollama_url: 'http://localhost:11434',
  ollama_model: 'llama3',
  ollama_timeout_sec: '30',
  ai_default_language: 'both',
  ai_tutor_enabled: 'true',
  video_lessons_enabled: 'true',
  ai_grading_enabled: 'true',
})
const show = reactive({ gemini: false, tts: false })

const saving = reactive({ general: false, subscription: false, coins: false, ai: false })

// ── Load ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  try {
    const [settRes, gradesRes, plansRes] = await Promise.all([
      api.get('/admin/settings'),
      api.get('/admin/grades'),
      api.get('/admin/subscriptions'),
    ])

    const s = settRes.data || {}
    settingsStore.map = s

    // General
    g.site_name         = s['site_name']         ?? 'Balochistan Academy Portal'
    g.institute_name    = s['institute_name']     ?? ''
    g.support_whatsapp  = s['support_whatsapp']   ?? ''
    g.registration_open = s['registration_open']  ?? 'true'
    g.allowed_grades    = s['allowed_grades']      ?? ''
    g.allowed_grades_set = new Set(
      g.allowed_grades ? g.allowed_grades.split(',').map(x => x.trim()).filter(Boolean) : []
    )

    // Grades
    allGrades.value = gradesRes.data || []

    // Plans
    plans.value = plansRes.data || []

    // Subscription globals
    sub.trial_days                      = s['trial_days']                       ?? '7'
    sub.subscription_auto_expire        = s['subscription_auto_expire']         ?? 'true'
    sub.subscription_activate_on_payment = s['subscription_activate_on_payment'] ?? 'false'

    // Coins
    coin.coin_rate_pkr                  = s['coin_rate_pkr']                  ?? '0.50'
    coin.min_withdrawal                 = s['min_withdrawal']                  ?? '300'
    coin.min_questions_for_eligibility  = s['min_questions_for_eligibility']   ?? '50'
    coin.daily_login_bonus              = s['daily_login_bonus']               ?? '2'
    coin.coins_per_90pct                = s['coins_per_90pct']                 ?? '50'
    coin.coins_per_70pct                = s['coins_per_70pct']                 ?? '30'
    coin.coins_per_50pct                = s['coins_per_50pct']                 ?? '15'
    coin.coins_per_pass                 = s['coins_per_pass']                  ?? '5'

    // AI
    ai.gemini_api_key       = s['gemini_api_key']       ?? ''
    ai.gemini_model         = s['gemini_model']         ?? 'gemini-2.5-flash-lite'
    ai.tts_api_key          = s['tts_api_key']          ?? ''
    ai.ollama_url           = s['ollama_url']           ?? 'http://localhost:11434'
    ai.ollama_model         = s['ollama_model']         ?? 'llama3'
    ai.ollama_timeout_sec   = s['ollama_timeout_sec']   ?? '30'
    ai.ai_default_language  = s['ai_default_language']  ?? 'both'
    ai.ai_tutor_enabled     = s['ai_tutor_enabled']     ?? 'true'
    ai.video_lessons_enabled = s['video_lessons_enabled'] ?? 'true'
    ai.ai_grading_enabled   = s['ai_grading_enabled']   ?? 'true'
  } catch (e) {
    console.error('Settings load failed', e)
  } finally {
    loading.value = false
  }
})

// ── Save helpers ──────────────────────────────────────────────────────────────
async function saveGeneral() {
  saving.general = true
  try {
    await settingsStore.saveMany({
      site_name:        g.site_name,
      institute_name:   g.institute_name,
      support_whatsapp: g.support_whatsapp,
      registration_open: g.registration_open,
      allowed_grades:   g.allowed_grades,
    })
    showToast('General settings saved!')
  } catch { showToast('Save failed') }
  finally { saving.general = false }
}

async function saveSubscription() {
  saving.subscription = true
  try {
    await settingsStore.saveMany({
      trial_days:                       String(sub.trial_days),
      subscription_auto_expire:         sub.subscription_auto_expire,
      subscription_activate_on_payment: sub.subscription_activate_on_payment,
    })
    showToast('Subscription settings saved!')
  } catch { showToast('Save failed') }
  finally { saving.subscription = false }
}

async function saveCoins() {
  saving.coins = true
  try {
    await settingsStore.saveMany({
      coin_rate_pkr:                 String(coin.coin_rate_pkr),
      min_withdrawal:                String(coin.min_withdrawal),
      min_questions_for_eligibility: String(coin.min_questions_for_eligibility),
      daily_login_bonus:             String(coin.daily_login_bonus),
      coins_per_90pct:               String(coin.coins_per_90pct),
      coins_per_70pct:               String(coin.coins_per_70pct),
      coins_per_50pct:               String(coin.coins_per_50pct),
      coins_per_pass:                String(coin.coins_per_pass),
    })
    showToast('Coin settings saved!')
  } catch { showToast('Save failed') }
  finally { saving.coins = false }
}

async function saveAi() {
  saving.ai = true
  try {
    await settingsStore.saveMany({
      gemini_api_key:       ai.gemini_api_key,
      gemini_model:         ai.gemini_model,
      tts_api_key:          ai.tts_api_key,
      ollama_url:           ai.ollama_url,
      ollama_model:         ai.ollama_model,
      ollama_timeout_sec:   String(ai.ollama_timeout_sec),
      ai_default_language:  ai.ai_default_language,
      ai_tutor_enabled:     ai.ai_tutor_enabled,
      video_lessons_enabled: ai.video_lessons_enabled,
      ai_grading_enabled:   ai.ai_grading_enabled,
    })
    showToast('AI settings saved!')
  } catch { showToast('Save failed') }
  finally { saving.ai = false }
}
</script>

<style scoped>
.grade-checkboxes {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.grade-chip {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  border: 1.5px solid var(--t-border);
  background: var(--t-bg);
  cursor: pointer;
  font-size: 0.83rem;
  font-weight: 500;
  color: var(--t-text2);
  transition: all 0.15s;
}
.grade-chip:hover { border-color: var(--t-accent); color: var(--t-accent); }
.grade-chip.active { border-color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 12%, transparent); color: var(--t-accent); }
.grade-chip input { display: none; }

.input-secret-wrap { position: relative; display: flex; }
.input-secret-wrap .input { flex: 1; }
.input-secret-eye {
  position: absolute; right: 0.5rem; top: 50%; transform: translateY(-50%);
  background: none; border: none; cursor: pointer; font-size: 1rem; line-height: 1;
}

.qb-overlay { position:fixed; inset:0; z-index:1000; background:rgba(0,0,0,0.45); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; padding:1.25rem; }
.qb-modal { background:var(--t-bg2); border:1px solid var(--t-border); border-radius:16px; width:100%; max-width:520px; max-height:calc(100vh - 2.5rem); overflow-y:auto; box-shadow:0 20px 60px rgba(0,0,0,0.3); }
.qb-modal-header { display:flex; align-items:center; justify-content:space-between; padding:1.25rem 1.5rem 0; }
.qb-modal-title { font-size:1rem; font-weight:700; color:var(--t-text1); margin:0; }
.qb-modal-body { padding:1.25rem 1.5rem 1.5rem; display:flex; flex-direction:column; gap:0.5rem; }
.qb-err { padding:10px 14px; border-radius:9px; font-size:0.83rem; background:#fef2f2; border:1px solid #fecaca; color:#b91c1c; }

.badge-gray { display:inline-flex; align-items:center; padding:2px 8px; border-radius:99px; font-size:0.72rem; font-weight:600; background:#f3f4f6; color:#6b7280; }

.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from, .toast-leave-to { opacity: 0; transform: translateY(10px); }

/* ── TipTap description editor ── */
.desc-editor {
  border: 1.5px solid var(--t-border);
  border-radius: 10px;
  overflow: hidden;
  background: var(--t-bg);
  transition: border-color 0.15s;
}
.desc-editor:focus-within { border-color: var(--t-accent); }

.desc-editor-toolbar {
  display: flex;
  align-items: center;
  gap: 2px;
  padding: 0.35rem 0.5rem;
  background: var(--t-bg2);
  border-bottom: 1px solid var(--t-border);
  flex-wrap: wrap;
}
.desc-tb-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 6px;
  background: transparent;
  color: var(--t-text2);
  cursor: pointer;
  transition: background 0.12s, color 0.12s;
}
.desc-tb-btn:hover { background: var(--t-border); color: var(--t-text1); }
.desc-tb-btn.active { background: color-mix(in srgb, var(--t-accent) 15%, transparent); color: var(--t-accent); }
.desc-tb-sep { width: 1px; height: 18px; background: var(--t-border); margin: 0 2px; }
.desc-tb-label { font-size: 0.78rem; font-weight: 700; line-height: 1; }

:deep(.desc-editor-content) {
  padding: 0.65rem 0.85rem;
  min-height: 100px;
  font-size: 0.875rem;
  color: var(--t-text1);
  line-height: 1.6;
  outline: none;
}
:deep(.desc-editor-content p) { margin: 0 0 0.4em; }
:deep(.desc-editor-content ul) { margin: 0.25em 0 0.4em 1.2em; }
:deep(.desc-editor-content h3) { font-size: 0.95rem; font-weight: 700; margin: 0.5em 0 0.25em; }
:deep(.desc-editor-content strong) { font-weight: 700; }
:deep(.desc-editor-content em) { font-style: italic; }
:deep(.desc-editor-content u) { text-decoration: underline; }
:deep(.desc-editor-content p.is-editor-empty:first-child::before) {
  content: attr(data-placeholder);
  color: var(--t-text3);
  pointer-events: none;
  float: left;
  height: 0;
}

/* Color picker button */
.desc-color-btn { position: relative; cursor: pointer; }
.desc-color-indicator {
  width: 16px; height: 16px; border-radius: 3px;
  border: 1.5px solid rgba(0,0,0,0.15); display: block;
}
.desc-color-input {
  position: absolute; inset: 0; opacity: 0; width: 100%; height: 100%;
  cursor: pointer; border: none; padding: 0;
}

/* Highlight picker */
.desc-highlight-wrap { position: relative; }
.desc-hl-trigger .desc-hl-icon { font-size: 0.9rem; line-height: 1; }
.desc-hl-picker {
  position: absolute; top: calc(100% + 4px); left: 50%; transform: translateX(-50%);
  background: var(--t-bg2); border: 1px solid var(--t-border);
  border-radius: 10px; padding: 6px; z-index: 100;
  display: flex; gap: 4px; align-items: center;
  box-shadow: 0 4px 20px rgba(0,0,0,0.18);
}
.desc-hl-swatch {
  width: 22px; height: 22px; border-radius: 5px; border: 1.5px solid rgba(0,0,0,0.1);
  cursor: pointer; transition: transform 0.1s;
}
.desc-hl-swatch:hover { transform: scale(1.2); }
.desc-hl-clear {
  width: 22px; height: 22px; border-radius: 5px; border: 1px solid var(--t-border);
  background: transparent; cursor: pointer; font-size: 0.7rem;
  color: var(--t-text3); display: flex; align-items: center; justify-content: center;
}
.desc-hl-clear:hover { background: var(--t-border); }

.toggle-row { display:flex; align-items:center; gap:0.75rem; cursor:pointer; }
.toggle-wrap { position:relative; display:inline-block; width:44px; height:24px; }
.toggle-wrap input { opacity:0; width:0; height:0; }
.toggle-pill { position:absolute; cursor:pointer; top:0; left:0; right:0; bottom:0; background:#d1d5db; border-radius:12px; transition:.3s; }
.toggle-pill:before { position:absolute; content:""; height:18px; width:18px; left:3px; bottom:3px; background:#fff; border-radius:50%; transition:.3s; }
.toggle-wrap input:checked + .toggle-pill { background:var(--t-accent); }
.toggle-wrap input:checked + .toggle-pill:before { transform:translateX(20px); }
</style>
