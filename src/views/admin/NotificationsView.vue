<template>
  <div class="animate-fade-in space-y-6">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🔔</div>
      <div class="ag-banner-title">Notifications Center</div>
      <div class="ag-banner-sub">Send announcements and alerts to students</div>
      <div class="ag-banner-actions">
        <div class="ag-banner-stat">24 Sent</div>
        <div class="ag-banner-stat">3 Templates</div>
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
          <button class="btn-primary flex items-center gap-2" @click="sendNotification">
            <Send class="w-4 h-4" /> Send Now
          </button>
          <button class="btn-secondary flex items-center gap-2" @click="saveTemplate">
            <Clock class="w-4 h-4" /> Save as Template
          </button>
        </div>
      </div>
    </div>

    <!-- Templates Library -->
    <div class="ag-card">
      <div class="ag-card-section-label">
        <Clock class="w-4 h-4" /> Templates Library
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          <button
            v-for="t in templates"
            :key="t.id"
            @click="loadTemplate(t)"
            class="ag-grid-card text-left p-4 transition-all hover:scale-[1.01]"
            style="cursor:pointer;"
          >
            <div class="text-2xl mb-2">{{ t.emoji }}</div>
            <div class="font-semibold text-sm text-[var(--t-text1)]">{{ t.title }}</div>
            <div class="text-xs text-[var(--t-text3)] mt-1 line-clamp-2">{{ t.message }}</div>
            <div class="mt-2">
              <span :class="'badge-' + t.badgeColor + ' text-xs'">{{ t.type }}</span>
            </div>
          </button>
        </div>
      </div>
    </div>

    <!-- Sent Notifications History -->
    <div class="ag-card">
      <div class="ag-card-section-label">
        <CheckCircle class="w-4 h-4" /> Sent Notifications
      </div>
      <div class="ag-card-body">
        <div class="ag-table">
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Type</th>
                <th>Recipients</th>
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
                <td class="text-sm text-[var(--t-text3)]">{{ n.sentAt }}</td>
                <td>
                  <span v-if="n.status === 'Delivered'" class="badge-green text-xs flex items-center gap-1 w-fit">
                    <CheckCircle class="w-3 h-3" /> Delivered
                  </span>
                  <span v-else-if="n.status === 'Partial'" class="badge-yellow text-xs flex items-center gap-1 w-fit">
                    <AlertCircle class="w-3 h-3" /> Partial
                  </span>
                  <span v-else class="badge-blue text-xs flex items-center gap-1 w-fit">
                    <Clock class="w-3 h-3" /> Scheduled
                  </span>
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
import { ref } from 'vue'
import { ArrowLeft, Bell, Send, Users, Clock, CheckCircle, AlertCircle, Megaphone } from '@lucide/vue'

// Recipient chips
const recipientOptions = [
  { value: 'all', label: 'All Students' },
  { value: 'active', label: 'Active Subscribers' },
  { value: 'roles', label: 'Specific Roles' },
]
const selectedRecipients = ref(['all'])
const roleOptions = ['Student', 'Teacher', 'Parent']
const selectedRoles = ref([])

function toggleRecipient(val) {
  const idx = selectedRecipients.value.indexOf(val)
  if (idx === -1) selectedRecipients.value.push(val)
  else selectedRecipients.value.splice(idx, 1)
}

// Form
const form = ref({ title: '', message: '', type: 'announcement', priority: 'normal' })

const priorityOptions = [
  { value: 'low',    label: 'Low',    activeClass: 'bg-[var(--t-hover2)] border-[var(--t-border)] text-[var(--t-text2)]' },
  { value: 'normal', label: 'Normal', activeClass: 'border-transparent text-white' + ' !bg-[var(--t-accent)]' },
  { value: 'high',   label: 'High',   activeClass: 'bg-rose-500 border-transparent text-white' },
]

function sendNotification() {
  if (!form.value.title || !form.value.message) return
  sentNotifications.value.unshift({
    id: Date.now(),
    title: form.value.title,
    preview: form.value.message.slice(0, 80),
    type: form.value.type,
    typeBadge: typeToBadge(form.value.type),
    recipients: selectedRecipients.value.includes('all') ? 'All Students' : 'Selected',
    sentAt: new Date().toLocaleString('en-PK', { dateStyle: 'medium', timeStyle: 'short' }),
    status: 'Delivered',
  })
  form.value = { title: '', message: '', type: 'announcement', priority: 'normal' }
}

function saveTemplate() {
  alert('Template saved!')
}

function typeToBadge(t) {
  return { announcement: 'blue', alert: 'red', achievement: 'green', reminder: 'yellow' }[t] || 'blue'
}

// Templates
const templates = [
  {
    id: 1, emoji: '⏰', title: 'Test Reminder',
    message: 'Your upcoming test is scheduled for tomorrow. Make sure to prepare well and get enough rest tonight.',
    type: 'Reminder', badgeColor: 'yellow',
  },
  {
    id: 2, emoji: '💳', title: 'Subscription Expiry',
    message: 'Your Balochistan Academy Portal subscription expires in 3 days. Renew now to keep access to all features and content.',
    type: 'Alert', badgeColor: 'red',
  },
  {
    id: 3, emoji: '🏆', title: 'New Achievement',
    message: 'Congratulations! You have unlocked a new achievement. Keep up the excellent work and keep learning!',
    type: 'Achievement', badgeColor: 'green',
  },
  {
    id: 4, emoji: '📊', title: 'Monthly Results',
    message: 'Your monthly performance report is now available. Check your dashboard to see your progress and rankings.',
    type: 'Announcement', badgeColor: 'blue',
  },
]

function loadTemplate(t) {
  form.value.title = t.title
  form.value.message = t.message
  form.value.type = t.type.toLowerCase()
}

// Sent notifications mock
const sentNotifications = ref([
  { id: 1, title: 'Test Reminder: Physics Chapter 5', preview: 'Your upcoming test is scheduled for tomorrow...', type: 'reminder', typeBadge: 'yellow', recipients: 'All Students', sentAt: '10 Jun 2026, 09:00', status: 'Delivered' },
  { id: 2, title: 'Subscription Expiry Alert', preview: 'Your Balochistan Academy Portal subscription expires in 3 days...', type: 'alert', typeBadge: 'red', recipients: 'Active Subscribers', sentAt: '9 Jun 2026, 14:30', status: 'Delivered' },
  { id: 3, title: 'New Achievement Unlocked!', preview: 'Congratulations! You have completed 50 tests...', type: 'achievement', typeBadge: 'green', recipients: '32 Students', sentAt: '8 Jun 2026, 11:15', status: 'Partial' },
  { id: 4, title: 'Monthly Performance Report', preview: 'Your May 2026 results are now available...', type: 'announcement', typeBadge: 'blue', recipients: 'All Students', sentAt: '1 Jun 2026, 08:00', status: 'Delivered' },
  { id: 5, title: 'Holiday Notice', preview: 'Platform will have scheduled maintenance on June 15...', type: 'announcement', typeBadge: 'blue', recipients: 'All Students', sentAt: '28 May 2026, 17:00', status: 'Delivered' },
  { id: 6, title: 'Weekly Test Scheduled', preview: 'A new weekly test for Mathematics has been scheduled...', type: 'reminder', typeBadge: 'yellow', recipients: 'Grade 10 Students', sentAt: '25 May 2026, 10:00', status: 'Scheduled' },
])
</script>
