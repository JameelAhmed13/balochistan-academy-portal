import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as signalR from '@microsoft/signalr'
import api from '@/services/api'

const HUB_URL = '/hubs/notifications'

// Complaint events ride the same "notification" SignalR message but are routed to whichever
// complaint views are currently mounted instead of the generic notification bell — multiple
// subscribers supported since the admin top-nav badge and a complaints page can both be mounted
// at once, each needing its own independent subscribe/unsubscribe lifecycle.
const _complaintHandlers = new Set()
export function onComplaintEvent(fn) {
  _complaintHandlers.add(fn)
  return () => _complaintHandlers.delete(fn)
}

export const useNotificationsStore = defineStore('notifications', () => {
  const notifications = ref([])
  const connected     = ref(false)
  let   _hub          = null

  const unreadCount = computed(() => notifications.value.filter(n => !n.read).length)

  async function fetchNotifications() {
    try {
      const { data } = await api.get('/notifications')
      notifications.value = data.map(n => ({ ...n, read: n.isRead }))
    } catch { /* silent — hub will push new ones anyway */ }
  }

  async function markRead(id) {
    const n = notifications.value.find(n => n.id === id)
    if (n) n.read = true
    await api.patch(`/notifications/${id}/read`).catch(() => {})
  }

  async function markAllRead() {
    notifications.value.forEach(n => { n.read = true })
    await api.patch('/notifications/read-all').catch(() => {})
  }

  async function connect() {
    if (_hub) return

    _hub = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => localStorage.getItem('bap_token') || '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    _hub.on('ReceiveNotification', (notification) => {
      notifications.value.unshift({ ...notification, read: false, receivedAt: new Date().toISOString() })
    })

    _hub.on('notification', (payload) => {
      if (payload?.type === 'forceLogout') {
        import('@/stores/auth').then(({ useAuthStore }) => useAuthStore().logout())
        return
      }
      if (payload?.type?.startsWith('complaint')) {
        _complaintHandlers.forEach(fn => fn(payload))
        return
      }
      notifications.value.unshift({ ...payload, read: false, receivedAt: new Date().toISOString() })
    })

    _hub.onclose(() => { connected.value = false })
    _hub.onreconnected(() => { connected.value = true })

    try {
      await _hub.start()
      connected.value = true
      await fetchNotifications()
    } catch {
      _hub = null
    }
  }

  async function disconnect() {
    if (_hub) {
      try { await _hub.stop() } catch { /* ignore */ }
      _hub = null
    }
    connected.value = false
    notifications.value = []
  }

  return { notifications, connected, unreadCount, connect, disconnect, markRead, markAllRead, fetchNotifications }
})
