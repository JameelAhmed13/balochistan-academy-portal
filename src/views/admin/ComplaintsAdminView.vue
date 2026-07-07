<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">💬</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Complaints &amp; Feedback</div>
        <div class="ag-banner-sub">Manage student feedback, complaints and suggestions</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">🔴 {{ stats.open }} Open</span>
        <span class="ag-banner-stat">🟡 {{ stats.inProgress }} In Progress</span>
        <span class="ag-banner-stat">🟢 {{ stats.resolved }} Resolved</span>
        <span class="ag-banner-stat">📋 {{ stats.total }} Total</span>
      </div>
      <RouterLink to="/app/admin" class="btn-ghost" style="cursor:pointer;">
        <ArrowLeft class="w-4 h-4" />
      </RouterLink>
    </div>

    <!-- Filters -->
    <div class="ag-card">
      <div class="ag-card-body" style="display:flex;gap:1rem;flex-wrap:wrap;align-items:center;justify-content:space-between;">
        <div class="ag-tabs" style="border-bottom:none;padding:0;">
          <button v-for="tab in STATUS_TABS" :key="tab.value" type="button"
            :class="['ag-tab', statusFilter === tab.value ? 'active' : '']" @click="setStatus(tab.value)">
            {{ tab.label }}
          </button>
        </div>
        <div>
          <label class="label">Category</label>
          <select v-model="categoryFilter" class="input mt-1" @change="applyFilters">
            <option value="">All categories</option>
            <option v-for="c in CATEGORY_OPTIONS" :key="c" :value="c">{{ c }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Complaints List -->
    <div class="ag-card" style="padding:0;overflow:hidden;">
      <div v-if="loading" class="ag-empty" style="padding:40px;">
        <div class="ag-empty-icon">⏳</div><p>Loading complaints…</p>
      </div>
      <div v-else-if="!complaints.length" class="ag-empty" style="padding:48px 24px;">
        <MessageSquare class="w-10 h-10 mx-auto mb-2 opacity-30" />
        <p>No complaints found for this filter.</p>
      </div>
      <div v-else class="space-y-3" style="padding:1rem;">
        <div v-for="c in complaints" :key="c.id" class="ag-grid-card overflow-hidden">
          <!-- Card Header (always visible) -->
          <div class="p-4 flex items-start gap-3">
            <div class="flex flex-col items-center gap-1.5 pt-0.5 shrink-0">
              <div class="w-2.5 h-2.5 rounded-full mt-1" :style="'background:' + statusColor(c.status)"></div>
              <div class="w-9 h-9 rounded-full flex items-center justify-center text-sm font-bold text-white"
                :style="'background:' + avatarColor(c.userName || '?')">
                {{ initials(c.userName || '?') }}
              </div>
            </div>

            <div class="flex-1 min-w-0">
              <div class="flex flex-wrap items-center gap-2">
                <span class="font-semibold text-sm" style="color:var(--t-text1);">{{ c.userName || 'Unknown' }}</span>
                <span class="text-xs" style="color:var(--t-text3);">{{ formatDate(c.createdAt) }}</span>
                <span :class="[typeClass(c.category), 'text-xs']">{{ c.category }}</span>
                <span v-if="c.messageCount" class="text-xs" style="color:var(--t-text3);">💬 {{ c.messageCount }}</span>
              </div>
              <div class="font-semibold mt-0.5" style="color:var(--t-text1);">{{ c.subject }}</div>
              <div class="text-sm mt-0.5 line-clamp-2" style="color:var(--t-text2);">{{ c.description }}</div>
            </div>

            <div class="flex flex-col items-end gap-2 shrink-0 ml-2">
              <span :class="[statusClass(c.status), 'text-xs']">{{ statusLabel(c.status) }}</span>
              <div class="flex items-center gap-1.5 mt-1">
                <button v-if="!isLocked(c.status)" class="btn-secondary text-xs px-2.5 py-1 flex items-center gap-1"
                  :disabled="statusChangingId === c.id" @click.stop="changeStatus(c, 'resolved')">
                  <CheckCircle class="w-3 h-3" /> Resolve
                </button>
              </div>
              <button class="flex items-center gap-1 text-xs mt-1 transition-colors" style="color:var(--t-text3);" @click="toggleExpand(c)">
                <component :is="expandedId === c.id ? ChevronUp : ChevronDown" class="w-3.5 h-3.5" />
                {{ expandedId === c.id ? 'Collapse' : 'View Conversation' }}
              </button>
            </div>
          </div>

          <!-- Expanded: full chat thread -->
          <Transition name="expand">
            <div v-if="expandedId === c.id" class="border-t" style="border-color:var(--t-border);">
              <div class="cp-thread-messages">
                <div class="cp-msg">
                  <div class="cp-msg-bubble">{{ c.description }}</div>
                  <div class="cp-msg-meta">{{ c.userName || 'Student' }} · {{ formatDateTime(c.createdAt) }}</div>
                </div>

                <div v-if="threadLoading[c.id]" class="ag-empty" style="padding:16px;">
                  <div class="ag-empty-icon">⏳</div><p>Loading conversation…</p>
                </div>
                <template v-else>
                  <div v-for="m in (threads[c.id] || [])" :key="m.id" class="cp-msg" :class="{ 'cp-msg--mine': m.isAdmin }">
                    <div class="cp-msg-bubble">{{ m.message }}</div>
                    <div class="cp-msg-meta">{{ m.isAdmin ? (m.senderName || 'Admin') : (c.userName || 'Student') }} · {{ formatDateTime(m.createdAt) }}</div>
                  </div>
                </template>
              </div>

              <div v-if="isLocked(c.status)" class="cp-locked-note">
                This complaint is {{ statusLabel(c.status).toLowerCase() }} — the conversation and status are locked.
              </div>
              <template v-else>
                <div class="cp-status-row">
                  <label class="text-xs" style="color:var(--t-text3);">Status</label>
                  <select :value="c.status" class="input" @change="changeStatus(c, $event.target.value)" :disabled="statusChangingId === c.id">
                    <option v-if="c.status === 'open'" value="open">Open</option>
                    <option value="in-progress">In Progress</option>
                    <option value="resolved">Resolved</option>
                    <option value="closed">Closed</option>
                  </select>
                </div>

                <form class="cp-thread-composer" @submit.prevent="sendReply(c)">
                  <input v-model="composerText[c.id]" class="input" placeholder="Type a reply…" :disabled="sendingId === c.id" />
                  <button type="submit" class="btn-primary" :disabled="sendingId === c.id || !composerText[c.id]?.trim()">
                    <Send class="w-4 h-4" />
                  </button>
                </form>
              </template>
            </div>
          </Transition>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" style="display:flex;justify-content:center;gap:0.5rem;padding:16px;">
        <button class="btn-ghost text-sm" :disabled="page <= 1" @click="page--; fetchComplaints()">← Prev</button>
        <span style="font-size:0.8rem;color:var(--t-text3);align-self:center;">Page {{ page }} of {{ totalPages }}</span>
        <button class="btn-ghost text-sm" :disabled="page >= totalPages" @click="page++; fetchComplaints()">Next →</button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { ArrowLeft, MessageSquare, CheckCircle, ChevronDown, ChevronUp, Send } from '@lucide/vue'
import { useToast } from 'primevue/usetoast'
import api from '@/services/api'
import { onComplaintEvent } from '@/stores/notifications'

const toast = useToast()

const CATEGORY_OPTIONS = ['Complaint', 'Suggestion', 'Bug Report', 'Feature Request', 'General Feedback']
const STATUS_TABS = [
  { value: '',             label: 'All' },
  { value: 'open',         label: 'Open' },
  { value: 'in-progress',  label: 'In Progress' },
  { value: 'resolved',     label: 'Resolved' },
  { value: 'closed',       label: 'Closed' },
]

const statusFilter   = ref('')
const categoryFilter = ref('')
const complaints  = ref([])
const loading     = ref(false)
const page        = ref(1)
const pageSize    = 20
const total       = ref(0)
const totalPages  = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))

const stats = ref({ open: 0, inProgress: 0, resolved: 0, closed: 0, total: 0 })

async function fetchComplaints() {
  loading.value = true
  try {
    const params = { page: page.value, pageSize }
    if (statusFilter.value)   params.status = statusFilter.value
    if (categoryFilter.value) params.category = categoryFilter.value
    const { data } = await api.get('/admin/complaints', { params })
    complaints.value = data.items
    total.value = data.total
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not load complaints', detail: e.message, life: 4000 })
  } finally {
    loading.value = false
  }
}

async function fetchStats() {
  try {
    const { data } = await api.get('/admin/complaints/stats')
    const byStatus = Object.fromEntries((data.byStatus || []).map(s => [s.status, s.count]))
    stats.value = {
      open:       byStatus.open ?? 0,
      inProgress: byStatus['in-progress'] ?? 0,
      resolved:   byStatus.resolved ?? 0,
      closed:     byStatus.closed ?? 0,
      total:      data.total ?? 0,
    }
  } catch { /* leave zeros on failure */ }
}

function setStatus(value) {
  statusFilter.value = value
  applyFilters()
}
function applyFilters() {
  page.value = 1
  fetchComplaints()
}

onMounted(() => {
  fetchComplaints()
  fetchStats()
})

// ── Real-time updates (new complaints / student &amp; admin messages / status) ──
let offComplaintEvent = null
onMounted(() => { offComplaintEvent = onComplaintEvent(handleComplaintEvent) })
onUnmounted(() => { offComplaintEvent?.() })

function handleComplaintEvent(payload) {
  if (payload.type === 'complaintCreated') {
    fetchStats()
    if (page.value === 1) fetchComplaints()
  } else if (payload.type === 'complaintMessage') {
    const { complaintId, message } = payload
    if (threads.value[complaintId] && !threads.value[complaintId].some(m => m.id === message.id)) {
      threads.value = { ...threads.value, [complaintId]: [...threads.value[complaintId], message] }
    }
    const c = complaints.value.find(x => x.id === complaintId)
    if (c) c.messageCount = (c.messageCount || 0) + 1
  } else if (payload.type === 'complaintStatus') {
    const { complaintId, status } = payload
    const c = complaints.value.find(x => x.id === complaintId)
    if (c) c.status = status
    fetchStats()
  }
}

// ── Conversation thread (lazy-loaded on expand) ──────────────────────────────
const expandedId    = ref(null)
const threads       = ref({})   // complaintId -> ComplaintMessageDto[]
const threadLoading = ref({})   // complaintId -> boolean
const composerText  = ref({})   // complaintId -> string

async function toggleExpand(c) {
  expandedId.value = expandedId.value === c.id ? null : c.id
  if (expandedId.value === c.id && threads.value[c.id] === undefined) {
    threadLoading.value = { ...threadLoading.value, [c.id]: true }
    try {
      const { data } = await api.get(`/complaints/${c.id}/messages`)
      threads.value = { ...threads.value, [c.id]: data }
    } catch (e) {
      toast.add({ severity: 'error', summary: 'Could not load conversation', detail: e.message, life: 4000 })
    } finally {
      threadLoading.value = { ...threadLoading.value, [c.id]: false }
    }
  }
}

const sendingId = ref(null)
async function sendReply(c) {
  const text = composerText.value[c.id]
  if (!text?.trim()) return
  sendingId.value = c.id
  try {
    const { data } = await api.post(`/complaints/${c.id}/messages`, { message: text.trim() })
    threads.value = { ...threads.value, [c.id]: [...(threads.value[c.id] || []), data] }
    composerText.value[c.id] = ''
    c.messageCount = (c.messageCount || 0) + 1
    toast.add({ severity: 'success', summary: 'Reply sent', life: 2500 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not send reply', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    sendingId.value = null
  }
}

// ── Status change (independent of messaging) ─────────────────────────────────
const statusChangingId = ref(null)
async function changeStatus(c, status) {
  statusChangingId.value = c.id
  try {
    const { data } = await api.put(`/admin/complaints/${c.id}`, { status })
    Object.assign(c, data)
    fetchStats()
    toast.add({ severity: 'success', summary: `Marked as ${statusLabel(status)}`, life: 2500 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not update status', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    statusChangingId.value = null
  }
}

// ── Presentation helpers ─────────────────────────────────────────────────────
function initials(name) {
  return name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
}
function avatarColor(name) {
  const colors = ['#6366f1', '#8b5cf6', '#ec4899', '#14b8a6', '#f59e0b', '#10b981', '#3b82f6', '#ef4444']
  let hash = 0
  for (const ch of name) hash = (hash * 31 + ch.charCodeAt(0)) & 0xffff
  return colors[hash % colors.length]
}
function statusColor(status) {
  return { open: '#f59e0b', 'in-progress': '#3b82f6', resolved: '#10b981', closed: '#6366f1' }[status] || '#6b7280'
}
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
// A resolved/closed complaint is a locked, read-only thread — no further messages or status changes.
function isLocked(status) { return status === 'resolved' || status === 'closed' }
function formatDate(iso) { return new Date(iso).toLocaleDateString('en-PK', { dateStyle: 'medium', timeZone: 'Asia/Karachi' }) }
function formatDateTime(iso) { return new Date(iso).toLocaleString('en-PK', { dateStyle: 'medium', timeStyle: 'short', timeZone: 'Asia/Karachi' }) }
</script>

<style scoped>
.expand-enter-active, .expand-leave-active {
  transition: all 0.25s ease;
  overflow: hidden;
}
.expand-enter-from, .expand-leave-to {
  opacity: 0;
  max-height: 0;
}
.expand-enter-to, .expand-leave-from {
  opacity: 1;
  max-height: 600px;
}

/* Chat thread */
.cp-thread-messages {
  display: flex; flex-direction: column; gap: 0.65rem;
  padding: 1rem 1.25rem; max-height: 360px; min-height: 100px; overflow-y: auto;
  background: var(--t-bg);
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
.cp-status-row { display: flex; align-items: center; gap: 0.6rem; padding: 0.65rem 1.25rem; border-top: 1px solid var(--t-border); }
.cp-status-row select.input { width: auto; padding: 0.4rem 0.7rem; font-size: 0.8rem; }
.cp-thread-composer { display: flex; gap: 0.5rem; padding: 0.85rem 1.25rem; border-top: 1px solid var(--t-border); }
.cp-thread-composer .input { flex: 1; }
.cp-locked-note { padding: 0.85rem 1.25rem; border-top: 1px solid var(--t-border); font-size: 0.8rem; color: var(--t-text3); text-align: center; }
</style>
