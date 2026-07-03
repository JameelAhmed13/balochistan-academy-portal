<template>
  <div class="animate-fade-in space-y-6">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🔔</div>
      <div class="ag-banner-title">Notifications Center</div>
      <div class="ag-banner-sub">Send announcements and alerts to students</div>
      <div class="ag-banner-actions">
        <div class="ag-banner-stat">{{ totalSent }} Sent</div>
        <div class="ag-banner-stat">{{ templates.length }} Templates</div>
        <RouterLink to="/app/admin/notifications/inbox" class="btn-ghost flex items-center gap-1.5 text-sm">
          <Inbox class="w-4 h-4" /> My Inbox
        </RouterLink>
        <RouterLink to="/app/admin" class="btn-ghost flex items-center gap-1.5 text-sm">
          <ArrowLeft class="w-4 h-4" /> Back
        </RouterLink>
      </div>
    </div>

    <!-- Compose Panel -->
    <div class="ag-card">
      <div class="ag-card-section-label">
        <Megaphone class="w-4 h-4" /> Compose Notification
      </div>
      <div class="ag-card-body space-y-5">

        <!-- Recipient Selector -->
        <div>
          <label class="label">Recipients</label>
          <div class="flex flex-wrap gap-2 mt-1.5">
            <button
              v-for="r in recipientOptions"
              :key="r.value"
              @click="toggleRecipient(r.value)"
              :class="['px-3 py-1.5 rounded-full text-sm font-medium border transition-all',
                selectedRecipients.includes(r.value)
                  ? 'border-transparent text-white' : 'border-[var(--t-border)] text-[var(--t-text2)] bg-transparent hover:bg-[var(--t-hover)]']"
              :style="selectedRecipients.includes(r.value) ? 'background:var(--t-accent);' : ''"
            >
              {{ r.label }}
            </button>
          </div>

          <!-- Role checkboxes for Specific Roles -->
          <div v-if="selectedRecipients.includes('roles')" class="flex flex-wrap gap-4 mt-3 p-3 rounded-xl"
            style="background:var(--t-hover);">
            <label v-for="role in roleOptions" :key="role" class="flex items-center gap-2 cursor-pointer text-sm text-[var(--t-text2)]">
              <input type="checkbox" v-model="selectedRoles" :value="role"
                class="w-4 h-4 rounded accent-[var(--t-accent)]" />
              {{ role }}
            </label>
          </div>
        </div>

        <!-- Title + Type row -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="label">Notification Title</label>
            <input v-model="form.title" class="input mt-1" placeholder="Enter notification title…" />
          </div>
          <div>
            <label class="label">Type</label>
            <select v-model="form.type" class="input mt-1">
              <option value="announcement">Announcement</option>
              <option value="alert">Alert</option>
              <option value="achievement">Achievement</option>
              <option value="reminder">Reminder</option>
            </select>
          </div>
        </div>

        <!-- Message -->
        <div>
          <label class="label">Message</label>
          <textarea v-model="form.message" class="input mt-1 resize-none" rows="4"
            placeholder="Write your notification message here…"></textarea>
        </div>

        <!-- Priority -->
        <div>
          <label class="label">Priority</label>
          <div class="flex gap-2 mt-1.5">
            <button
              v-for="p in priorityOptions"
              :key="p.value"
              @click="form.priority = p.value"
              :class="['px-4 py-2 rounded-lg text-sm font-medium border transition-all',
                form.priority === p.value ? p.activeClass : 'border-[var(--t-border)] text-[var(--t-text2)] hover:bg-[var(--t-hover)]']"
            >
              {{ p.label }}
            </button>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex flex-wrap gap-3 pt-1">
          <button class="btn-primary flex items-center gap-2" @click="sendNotification" :disabled="sending">
            <Send class="w-4 h-4" /> {{ sending ? 'Sending…' : 'Send Now' }}
          </button>
          <button class="btn-secondary flex items-center gap-2" @click="saveTemplate" :disabled="!form.title">
            <Clock class="w-4 h-4" /> Save as Template
          </button>
        </div>

        <!-- Error message -->
        <p v-if="sendError" class="text-sm text-rose-500 mt-1">{{ sendError }}</p>
      </div>
    </div>

    <!-- Templates Library -->
    <div class="ag-card">
      <div class="ag-card-section-label">
        <Clock class="w-4 h-4" /> Templates Library
      </div>
      <div class="ag-card-body">
        <div v-if="templatesLoading" class="py-6 text-center text-sm text-[var(--t-text3)]">Loading templates…</div>
        <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          <div
            v-for="t in templates"
            :key="t.id"
            class="ag-grid-card text-left p-4 transition-all hover:scale-[1.01] relative group"
          >
            <button @click="loadTemplate(t)" class="absolute inset-0 w-full h-full" style="cursor:pointer;" />
            <div class="text-2xl mb-2">{{ t.icon || '📌' }}</div>
            <div class="font-semibold text-sm text-[var(--t-text1)]">{{ t.title }}</div>
            <div class="text-xs text-[var(--t-text3)] mt-1 line-clamp-2">{{ t.body }}</div>
            <div class="mt-2 flex items-center justify-between">
              <span :class="'badge-' + typeToBadge(reverseTypeMap[t.type] || t.type) + ' text-xs'">{{ reverseTypeMap[t.type] || t.type }}</span>
              <button
                v-if="!t.isDefault"
                @click.stop="deleteTemplate(t.id)"
                class="relative z-10 opacity-0 group-hover:opacity-100 transition-opacity text-rose-400 hover:text-rose-600 p-0.5"
                title="Delete template"
              >
                <Trash2 class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Sent Notifications History -->
    <div class="ag-card">
      <div class="ag-card-section-label">
        <CheckCircle class="w-4 h-4" /> Sent Notifications
      </div>
      <div class="ag-card-body">
        <div v-if="historyLoading" class="py-8 text-center text-sm text-[var(--t-text3)]">
          Loading history…
        </div>
        <div v-else class="ag-table">
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Type</th>
                <th>Recipients</th>
                <th>Sent To</th>
                <th>Sent At</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="n in sentNotifications" :key="n.id">
                <td>
                  <div class="font-medium text-[var(--t-text1)]">{{ n.title }}</div>
                  <div class="text-xs text-[var(--t-text3)] mt-0.5 line-clamp-1">{{ n.preview }}</div>
                </td>
                <td><span :class="'badge-' + n.typeBadge + ' text-xs capitalize'">{{ n.type }}</span></td>
                <td>
                  <div class="flex items-center gap-1.5 text-sm text-[var(--t-text2)]">
                    <Users class="w-3.5 h-3.5" /> {{ n.recipients }}
                  </div>
                </td>
                <td class="text-sm text-[var(--t-text2)]">{{ n.sentTo ?? '—' }}</td>
                <td class="text-sm text-[var(--t-text3)]">{{ n.sentAt }}</td>
                <td>
                  <span class="badge-green text-xs flex items-center gap-1 w-fit">
                    <CheckCircle class="w-3 h-3" /> Delivered
                  </span>
                </td>
              </tr>
              <tr v-if="!sentNotifications.length">
                <td colspan="6" class="text-center py-6 text-sm text-[var(--t-text3)]">
                  No notifications sent yet.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ArrowLeft, Send, Users, Clock, CheckCircle, Megaphone, Trash2, Inbox } from '@lucide/vue'
import api from '@/services/api'

// ── Type mapping ──────────────────────────────────────────────────────────────
const typeMap        = { announcement: 'announcement', alert: 'error', achievement: 'success', reminder: 'info' }
const reverseTypeMap = { announcement: 'announcement', error: 'alert', success: 'achievement', info: 'reminder', warning: 'reminder' }
const priorityIcon   = { low: '💡', normal: '🔔', high: '🚨' }

// ── Recipient chips ───────────────────────────────────────────────────────────
const recipientOptions = [
  { value: 'all',    label: 'All Students' },
  { value: 'active', label: 'Active Subscribers' },
  { value: 'roles',  label: 'Specific Roles' },
]
const selectedRecipients = ref(['all'])
const roleOptions = ['Student', 'Teacher', 'Parent']
const selectedRoles = ref([])

function toggleRecipient(val) {
  const idx = selectedRecipients.value.indexOf(val)
  if (idx === -1) selectedRecipients.value.push(val)
  else selectedRecipients.value.splice(idx, 1)
}

// ── Form ──────────────────────────────────────────────────────────────────────
const form      = ref({ title: '', message: '', type: 'announcement', priority: 'normal' })
const sending   = ref(false)
const sendError = ref('')

const priorityOptions = [
  { value: 'low',    label: 'Low',    activeClass: 'bg-[var(--t-hover2)] border-[var(--t-border)] text-[var(--t-text2)]' },
  { value: 'normal', label: 'Normal', activeClass: 'border-transparent text-white !bg-[var(--t-accent)]' },
  { value: 'high',   label: 'High',   activeClass: 'bg-rose-500 border-transparent text-white' },
]

async function sendNotification() {
  if (!form.value.title || !form.value.message) return
  sendError.value = ''
  sending.value = true
  try {
    const targets = selectedRecipients.value.includes('roles') && selectedRoles.value.length
      ? selectedRoles.value.map(r => ({ TargetRole: r }))
      : [{ TargetRole: null }]

    for (const t of targets) {
      const { data } = await api.post('/admin/notifications', {
        Title:      form.value.title,
        Body:       form.value.message,
        Type:       typeMap[form.value.type] || 'info',
        Icon:       priorityIcon[form.value.priority],
        TargetRole: t.TargetRole ?? undefined,
      })
      sentNotifications.value.unshift({
        id:         data.id,
        title:      form.value.title,
        preview:    form.value.message.slice(0, 80),
        type:       form.value.type,
        typeBadge:  typeToBadge(form.value.type),
        recipients: t.TargetRole || 'All Students',
        sentTo:     data.sentTo,
        sentAt:     new Date().toLocaleString('en-PK', { dateStyle: 'medium', timeStyle: 'short' }),
        status:     'Delivered',
      })
      totalSent.value++
    }
    form.value = { title: '', message: '', type: 'announcement', priority: 'normal' }
  } catch (e) {
    sendError.value = e.response?.data?.error || e.response?.data?.title || 'Failed to send notification.'
  } finally {
    sending.value = false
  }
}

function typeToBadge(t) {
  return { announcement: 'blue', alert: 'red', achievement: 'green', reminder: 'yellow' }[t] || 'blue'
}

// ── Templates ─────────────────────────────────────────────────────────────────
const templates        = ref([])
const templatesLoading = ref(false)

async function loadTemplates() {
  templatesLoading.value = true
  try {
    const { data } = await api.get('/admin/notification-templates')
    templates.value = data
  } catch { /* leave empty */ }
  templatesLoading.value = false
}

function loadTemplate(t) {
  form.value.title   = t.title
  form.value.message = t.body || ''
  form.value.type    = reverseTypeMap[t.type] || 'announcement'
}

async function saveTemplate() {
  if (!form.value.title) return
  try {
    const { data } = await api.post('/admin/notification-templates', {
      Title: form.value.title,
      Body:  form.value.message,
      Type:  typeMap[form.value.type] || 'info',
      Icon:  '📌',
    })
    templates.value.push(data)
  } catch (e) {
    alert(e.response?.data?.error || 'Failed to save template.')
  }
}

async function deleteTemplate(id) {
  try {
    await api.delete(`/admin/notification-templates/${id}`)
    templates.value = templates.value.filter(t => t.id !== id)
  } catch (e) {
    alert(e.response?.data?.error || 'Failed to delete template.')
  }
}

// ── History ───────────────────────────────────────────────────────────────────
const sentNotifications = ref([])
const totalSent         = ref(0)
const historyLoading    = ref(false)

async function loadHistory() {
  historyLoading.value = true
  try {
    const { data } = await api.get('/admin/notifications')
    totalSent.value = data.total
    sentNotifications.value = data.items.map(n => ({
      id:         n.id,
      title:      n.title,
      preview:    (n.body || '').slice(0, 80),
      type:       reverseTypeMap[n.type] || n.type,
      typeBadge:  { announcement: 'blue', error: 'red', success: 'green', info: 'yellow', warning: 'yellow' }[n.type] || 'blue',
      recipients: n.targetRole || n.targetGrade
        ? [n.targetRole, n.targetGrade].filter(Boolean).join(' / ')
        : 'All Students',
      sentTo:     n.sentTo,
      sentAt:     new Date(n.createdAt).toLocaleString('en-PK', { dateStyle: 'medium', timeStyle: 'short' }),
      status:     'Delivered',
    }))
  } catch { /* leave empty */ }
  historyLoading.value = false
}

onMounted(() => {
  loadTemplates()
  loadHistory()
})
</script>
