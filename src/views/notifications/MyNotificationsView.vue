<template>
  <div class="animate-fade-in space-y-5">

    <!-- Header row -->
    <div class="flex items-center justify-between gap-4 flex-wrap">
      <div>
        <h1 class="text-xl font-bold" style="color:var(--t-text1)">My Notifications</h1>
        <p class="text-sm mt-0.5" style="color:var(--t-text3)">
          {{ notif.unreadCount > 0 ? `${notif.unreadCount} unread` : 'All caught up!' }}
        </p>
      </div>
      <div class="flex items-center gap-2">
        <button
          v-if="notif.unreadCount > 0"
          @click="notif.markAllRead()"
          class="btn-secondary flex items-center gap-2 text-sm"
        >
          <CheckCheck class="w-4 h-4" /> Mark All Read
        </button>
        <button @click="notif.fetchNotifications()" class="btn-ghost flex items-center gap-2 text-sm">
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" /> Refresh
        </button>
      </div>
    </div>

    <!-- Filter tabs -->
    <div class="flex gap-2 flex-wrap">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        @click="activeTab = tab.value"
        :class="['px-4 py-1.5 rounded-full text-sm font-medium border transition-all',
          activeTab === tab.value
            ? 'border-transparent text-white'
            : 'border-[var(--t-border)] text-[var(--t-text2)] hover:bg-[var(--t-hover)]']"
        :style="activeTab === tab.value ? 'background:var(--t-accent)' : ''"
      >
        {{ tab.label }}
        <span v-if="tab.count > 0" class="ml-1.5 px-1.5 py-0.5 rounded-full text-xs font-bold"
          :class="activeTab === tab.value ? 'bg-white/20 text-white' : 'bg-[var(--t-hover2)] text-[var(--t-text3)]'">
          {{ tab.count }}
        </span>
      </button>
    </div>

    <!-- Notifications list -->
    <div class="ag-card">
      <div class="ag-card-body p-0">

        <!-- Empty state -->
        <div v-if="!filtered.length" class="flex flex-col items-center justify-center py-16 gap-3">
          <div class="text-5xl">🔔</div>
          <div class="text-base font-semibold" style="color:var(--t-text2)">
            {{ activeTab === 'unread' ? 'No unread notifications' : 'No notifications yet' }}
          </div>
          <div class="text-sm" style="color:var(--t-text3)">
            {{ activeTab === 'unread' ? 'You\'re all caught up!' : 'Notifications from your admin will appear here.' }}
          </div>
        </div>

        <!-- Notification rows -->
        <div v-else>
          <div
            v-for="(n, idx) in filtered"
            :key="n.id"
            @click="handleClick(n)"
            :class="[
              'flex items-start gap-4 px-5 py-4 cursor-pointer transition-colors',
              idx < filtered.length - 1 ? 'border-b border-[var(--t-border)]' : '',
              !n.read ? 'bg-[var(--t-acc-alpha-sm)]' : 'hover:bg-[var(--t-hover)]'
            ]"
          >
            <!-- Icon / unread dot -->
            <div class="relative shrink-0 mt-0.5">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center text-xl"
                :style="`background:${typeBg[n.type] || typeBg.info}`">
                {{ n.icon || typeEmoji[n.type] || '🔔' }}
              </div>
              <span v-if="!n.read"
                class="absolute -top-1 -right-1 w-3 h-3 rounded-full border-2"
                style="background:var(--t-accent);border-color:var(--t-surface);"
              />
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between gap-3">
                <div class="font-semibold text-sm" :class="n.read ? 'text-[var(--t-text2)]' : ''" style="color:var(--t-text1)">
                  {{ n.title }}
                </div>
                <span :class="['badge-' + typeBadge[n.type] + ' text-xs shrink-0 capitalize']">
                  {{ n.type }}
                </span>
              </div>
              <div v-if="n.body" class="text-sm mt-1 line-clamp-2" style="color:var(--t-text3)">{{ n.body }}</div>
              <div class="flex items-center gap-3 mt-1.5">
                <span class="text-xs" style="color:var(--t-text3)">
                  {{ formatDate(n.createdAt || n.receivedAt) }}
                </span>
                <span v-if="!n.read" class="text-xs font-semibold" style="color:var(--t-accent)">New</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { CheckCheck, RefreshCw } from '@lucide/vue'
import { useNotificationsStore } from '@/stores/notifications'

const notif   = useNotificationsStore()
const loading = ref(false)
const activeTab = ref('all')

const typeBg = {
  announcement: 'rgba(99,102,241,0.12)',
  info:         'rgba(59,130,246,0.12)',
  success:      'rgba(16,185,129,0.12)',
  warning:      'rgba(245,158,11,0.12)',
  error:        'rgba(239,68,68,0.12)',
}
const typeEmoji = { announcement: '📢', info: 'ℹ️', success: '✅', warning: '⚠️', error: '🚨' }
const typeBadge = { announcement: 'blue', info: 'blue', success: 'green', warning: 'yellow', error: 'red' }

const tabs = computed(() => [
  { value: 'all',    label: 'All',        count: notif.notifications.length },
  { value: 'unread', label: 'Unread',     count: notif.unreadCount },
  { value: 'read',   label: 'Read',       count: notif.notifications.filter(n => n.read).length },
])

const filtered = computed(() => {
  if (activeTab.value === 'unread') return notif.notifications.filter(n => !n.read)
  if (activeTab.value === 'read')   return notif.notifications.filter(n =>  n.read)
  return notif.notifications
})

function handleClick(n) {
  if (!n.read) notif.markRead(n.id)
}

function formatDate(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  const now = new Date()
  const diff = now - d
  if (diff < 60000)       return 'Just now'
  if (diff < 3600000)     return `${Math.floor(diff / 60000)}m ago`
  if (diff < 86400000)    return `${Math.floor(diff / 3600000)}h ago`
  if (diff < 604800000)   return `${Math.floor(diff / 86400000)}d ago`
  return d.toLocaleString('en-PK', { dateStyle: 'medium', timeStyle: 'short' })
}

onMounted(async () => {
  loading.value = true
  await notif.fetchNotifications()
  loading.value = false
})
</script>
