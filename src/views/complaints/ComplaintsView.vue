<template>
  <div class="animate-fade-in max-w-2xl space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">💬</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Complaints &amp; Suggestions</div>
        <div class="ag-banner-sub">Share your feedback to help us improve</div>
      </div>
    </div>

    <!-- ── Chat thread (when a complaint is open) ── -->
    <div v-if="activeComplaint" class="ag-card">
      <div class="cp-thread-header">
        <button type="button" class="btn-ghost" style="padding:0.4rem;" @click="closeThread">
          <ArrowLeft class="w-4 h-4" />
        </button>
        <div class="cp-thread-title">
          <span :class="[typeClass(activeComplaint.type), 'text-xs']">{{ activeComplaint.type }}</span>
          <span :class="[statusClass(activeComplaint.status), 'text-xs']">{{ statusLabel(activeComplaint.status) }}</span>
        </div>
        <div class="cp-thread-subject">{{ activeComplaint.title }}</div>
      </div>

      <div class="cp-thread-messages" ref="threadEl">
        <div class="cp-msg cp-msg--mine">
          <div class="cp-msg-bubble">{{ activeComplaint.message }}</div>
          <div class="cp-msg-meta">You · {{ formatDateTime(activeComplaint.date) }}</div>
        </div>

        <div v-if="messagesLoading" class="ag-empty" style="padding:24px;">
          <div class="ag-empty-icon">⏳</div><p>Loading conversation…</p>
        </div>
        <template v-else>
          <div v-for="m in messages" :key="m.id" class="cp-msg" :class="{ 'cp-msg--mine': !m.isAdmin }">
            <div class="cp-msg-bubble">{{ m.message }}</div>
            <div class="cp-msg-meta">{{ m.isAdmin ? (m.senderName || 'Admin') : 'You' }} · {{ formatDateTime(m.createdAt) }}</div>
          </div>
        </template>
      </div>

      <p v-if="isLocked(activeComplaint.status)" class="cp-locked-note">
        This complaint is {{ statusLabel(activeComplaint.status).toLowerCase() }} — the conversation is closed.
      </p>
      <form v-else class="cp-thread-composer" @submit.prevent="sendMessage">
        <input v-model="newMessage" class="input" placeholder="Type a message…" :disabled="sending" />
        <button type="submit" class="btn-primary" :disabled="sending || !newMessage.trim()">
          <Send class="w-4 h-4" />
        </button>
      </form>
    </div>

    <!-- ── Form + list (default view) ── -->
    <template v-else>
      <div class="ag-card">
        <div class="ag-card-body">
          <form @submit.prevent="submit" class="space-y-4">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="label">Type</label>
                <select v-model="form.type" class="input">
                  <option>Complaint</option>
                  <option>Suggestion</option>
                  <option>Bug Report</option>
                  <option>Feature Request</option>
                  <option>General Feedback</option>
                </select>
              </div>
              <div>
                <label class="label">Subject / Module</label>
                <select v-model="form.module" class="input">
                  <option>General</option>
                  <option>AI Tutor</option>
                  <option>Preparation</option>
                  <option>Tests</option>
                  <option>Coins &amp; Rewards</option>
                  <option>Admin Panel</option>
                </select>
              </div>
            </div>
            <div>
              <label class="label">Title</label>
              <input v-model="form.title" type="text" class="input" placeholder="Brief description" required />
            </div>
            <div>
              <label class="label">Details</label>
              <textarea v-model="form.message" class="input min-h-[120px] resize-y" placeholder="Describe your complaint or suggestion in detail…" required />
            </div>
            <button type="submit" :disabled="submitting" class="btn-primary"><Send class="w-4 h-4" /> Submit Feedback</button>
          </form>
        </div>
      </div>

      <div class="ag-card">
        <div class="ag-card-section-label">Previous Submissions</div>
        <div class="ag-card-body">
          <div v-if="loading" class="ag-empty" style="padding:32px;">
            <div class="ag-empty-icon">⏳</div><p>Loading…</p>
          </div>
          <div v-else-if="!submissions.length" class="ag-empty" style="padding:32px;">
            <div class="ag-empty-icon">💬</div><p>No complaints submitted yet.</p>
          </div>
          <div v-else class="space-y-3">
            <button v-for="s in submissions" :key="s.id" type="button" class="cp-item" @click="openThread(s)">
              <div class="flex items-center gap-2 mb-1">
                <span :class="[typeClass(s.type), 'text-xs']">{{ s.type }}</span>
                <span class="cp-item-meta">{{ fmt(s.date) }}</span>
                <span class="ml-auto text-xs" :class="statusClass(s.status)">{{ statusLabel(s.status) }}</span>
              </div>
              <div class="cp-item-title">{{ s.title }}</div>
              <div class="cp-item-desc line-clamp-2">{{ s.message }}</div>
              <div v-if="s.messageCount" class="cp-item-count">💬 {{ s.messageCount }} message{{ s.messageCount !== 1 ? 's' : '' }}</div>
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, nextTick, onMounted, onUnmounted } from 'vue'
import { Send, ArrowLeft } from '@lucide/vue'
import { useToast } from 'primevue/usetoast'
import api from '@/services/api'
import { onComplaintEvent } from '@/stores/notifications'

const toast = useToast()
const form        = ref({ type: 'Complaint', module: 'General', title: '', message: '' })
const submissions = ref([])
const submitting  = ref(false)
const loading     = ref(false)

function normalise(c) {
  return {
    id:           c.id,
    type:         c.category || 'Complaint',
    title:        c.subject,
    message:      c.description,
    date:         c.createdAt,
    status:       c.status || 'open',
    messageCount: c.messageCount || 0,
  }
}

onMounted(async () => {
  loading.value = true
  try {
    const { data } = await api.get('/complaints')
    submissions.value = data.map(normalise)
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not load your complaints', detail: e.message, life: 4000 })
  } finally {
    loading.value = false
  }
})

// ── Real-time updates (admin replies / status changes) ────────────────────────
let offComplaintEvent = null
onMounted(() => { offComplaintEvent = onComplaintEvent(handleComplaintEvent) })
onUnmounted(() => { offComplaintEvent?.() })

function handleComplaintEvent(payload) {
  if (payload.type === 'complaintMessage') {
    const { complaintId, message } = payload
    if (activeComplaint.value?.id === complaintId && !messages.value.some(m => m.id === message.id)) {
      messages.value.push(message)
      scrollToBottom()
    }
    const s = submissions.value.find(x => x.id === complaintId)
    if (s) s.messageCount = (s.messageCount || 0) + 1
  } else if (payload.type === 'complaintStatus') {
    const { complaintId, status } = payload
    const s = submissions.value.find(x => x.id === complaintId)
    if (s) s.status = status
    if (activeComplaint.value?.id === complaintId) activeComplaint.value.status = status
  }
}

async function submit() {
  submitting.value = true
  try {
    const { data } = await api.post('/complaints', {
      category:    form.value.type,
      subject:     form.value.title,
      description: `[${form.value.module}] ${form.value.message}`,
    })
    submissions.value.unshift(normalise(data))
    form.value = { type: 'Complaint', module: 'General', title: '', message: '' }
    toast.add({ severity: 'success', summary: 'Submitted', detail: 'Thank you for your feedback!', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Error', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    submitting.value = false
  }
}

// ── Chat thread ──────────────────────────────────────────────────────────────
const activeComplaint  = ref(null)
const messages          = ref([])
const messagesLoading   = ref(false)
const newMessage        = ref('')
const sending           = ref(false)
const threadEl          = ref(null)

function scrollToBottom() {
  nextTick(() => { if (threadEl.value) threadEl.value.scrollTop = threadEl.value.scrollHeight })
}

async function openThread(s) {
  activeComplaint.value = s
  messages.value = []
  messagesLoading.value = true
  try {
    const { data } = await api.get(`/complaints/${s.id}/messages`)
    messages.value = data
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not load conversation', detail: e.message, life: 4000 })
  } finally {
    messagesLoading.value = false
    scrollToBottom()
  }
}

function closeThread() {
  activeComplaint.value = null
  messages.value = []
}

async function sendMessage() {
  if (!newMessage.value.trim() || !activeComplaint.value) return
  sending.value = true
  try {
    const { data } = await api.post(`/complaints/${activeComplaint.value.id}/messages`, { message: newMessage.value.trim() })
    messages.value.push(data)
    newMessage.value = ''
    scrollToBottom()

    const s = submissions.value.find(x => x.id === activeComplaint.value.id)
    if (s) s.messageCount = (s.messageCount || 0) + 1
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not send message', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    sending.value = false
  }
}

// A resolved/closed complaint is a locked, read-only thread — no further messages either side.
function isLocked(status) { return status === 'resolved' || status === 'closed' }

// ── Presentation helpers ─────────────────────────────────────────────────────
const STATUS_CLASSES = { open: 'badge-amber', 'in-progress': 'badge-blue', resolved: 'badge-green', closed: 'badge-indigo' }
function statusClass(status) { return STATUS_CLASSES[status] ?? 'badge-amber' }
function statusLabel(status) {
  return { open: 'Open', 'in-progress': 'In Progress', resolved: 'Resolved', closed: 'Closed' }[status] ?? status
}

const TYPE_CLASSES = {
  Complaint: 'badge-red', Suggestion: 'badge-blue', 'Bug Report': 'badge-amber',
  'Feature Request': 'badge-purple', 'General Feedback': 'badge-indigo',
}
function typeClass(type) { return TYPE_CLASSES[type] ?? 'badge-indigo' }

function fmt(iso) { return new Date(iso).toLocaleDateString('en-PK', { dateStyle: 'medium', timeZone: 'Asia/Karachi' }) }
function formatDateTime(iso) { return new Date(iso).toLocaleString('en-PK', { dateStyle: 'medium', timeStyle: 'short', timeZone: 'Asia/Karachi' }) }
</script>

<style scoped>
.cp-item {
  display: block; width: 100%; text-align: left; font: inherit; cursor: pointer;
  padding: 0.75rem; border-radius: 12px;
  background: var(--t-hover); border: 1px solid var(--t-border);
  transition: background 0.15s;
}
.cp-item:hover { background: var(--t-hover2); }
.cp-item-meta { font-size: 0.75rem; color: var(--t-text3); }
.cp-item-title { font-weight: 500; font-size: 0.875rem; color: var(--t-text1); }
.cp-item-desc { font-size: 0.75rem; color: var(--t-text3); margin-top: 0.125rem; }
.cp-item-count { font-size: 0.7rem; color: var(--t-accent); margin-top: 0.35rem; font-weight: 600; }

/* Chat thread */
.cp-thread-header { padding: 1rem 1.25rem; border-bottom: 1px solid var(--t-border); display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.cp-thread-title { display: flex; gap: 0.4rem; align-items: center; }
.cp-thread-subject { font-weight: 600; font-size: 0.9rem; color: var(--t-text1); width: 100%; }
.cp-thread-messages {
  display: flex; flex-direction: column; gap: 0.65rem;
  padding: 1.25rem; max-height: 50vh; min-height: 200px; overflow-y: auto;
}
.cp-msg { display: flex; flex-direction: column; max-width: 78%; align-self: flex-start; }
.cp-msg--mine { align-self: flex-end; align-items: flex-end; }
.cp-msg-bubble {
  padding: 0.6rem 0.85rem; border-radius: 14px; font-size: 0.85rem; line-height: 1.5;
  background: var(--t-hover); color: var(--t-text1); border: 1px solid var(--t-border);
}
.cp-msg--mine .cp-msg-bubble {
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); color: #fff; border-color: transparent;
}
.cp-msg-meta { font-size: 0.68rem; color: var(--t-text3); margin-top: 0.25rem; padding: 0 0.2rem; }
.cp-thread-composer { display: flex; gap: 0.5rem; padding: 0.85rem 1.25rem; border-top: 1px solid var(--t-border); }
.cp-thread-composer .input { flex: 1; }
.cp-locked-note { padding: 0.85rem 1.25rem; border-top: 1px solid var(--t-border); font-size: 0.8rem; color: var(--t-text3); text-align: center; margin: 0; }
</style>
