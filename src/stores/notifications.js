import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as signalR from '@microsoft/signalr'

const HUB_URL = '/hubs/notifications'

export const useNotificationsStore = defineStore('notifications', () => {
  const notifications = ref([])
  const connected     = ref(false)
  let   _hub          = null

  const unreadCount = computed(() => notifications.value.filter(n => !n.read).length)

  function markRead(id) {
    const n = notifications.value.find(n => n.id === id)
    if (n) n.read = true
  }

  function markAllRead() {
    notifications.value.forEach(n => { n.read = true })
  }

  async function connect() {
    if (_hub) return  // already connected or connecting

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

    _hub.onclose(() => { connected.value = false })
    _hub.onreconnected(() => { connected.value = true })

    try {
      await _hub.start()
      connected.value = true
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
  }

  return { notifications, connected, unreadCount, connect, disconnect, markRead, markAllRead }
})
