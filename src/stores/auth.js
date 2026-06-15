import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api, { setUnauthorizedHandler } from '@/services/api'
import { useNotificationsStore } from '@/stores/notifications'

export const useAuthStore = defineStore('auth', () => {
  const user         = ref(JSON.parse(localStorage.getItem('bap_user')         || 'null'))
  const token        = ref(localStorage.getItem('bap_token')                   || null)
  const refreshToken = ref(localStorage.getItem('bap_refresh_token')           || null)
  const loginHistory = ref([])

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin    = computed(() => user.value?.role === 'admin')
  const hasGrade   = computed(() => !!user.value?.gradeCode)

  function persist() {
    if (user.value)         localStorage.setItem('bap_user',          JSON.stringify(user.value))
    else                    localStorage.removeItem('bap_user')
    if (token.value)        localStorage.setItem('bap_token',         token.value)
    else                    localStorage.removeItem('bap_token')
    if (refreshToken.value) localStorage.setItem('bap_refresh_token', refreshToken.value)
    else                    localStorage.removeItem('bap_refresh_token')
  }

  function setSession({ token: t, refreshToken: rt, user: u }) {
    token.value        = t
    refreshToken.value = rt ?? null
    user.value         = u
    persist()
    logLocalHistory()
  }

  async function login(username, password) {
    const { data } = await api.post('/auth/login', { username, password })
    setSession(data)
    useNotificationsStore().connect().catch(() => {})
    return data.user
  }

  async function register(payload) {
    const { data } = await api.post('/auth/register', payload)
    setSession(data)
    useNotificationsStore().connect().catch(() => {})
    return data.user
  }

  async function fetchMe() {
    if (!token.value) return null
    try {
      const { data } = await api.get('/auth/me')
      user.value = data.user
      persist()
      return data.user
    } catch {
      logout()
      return null
    }
  }

  async function updateGrade(gradeCode) {
    const { data } = await api.put('/auth/me/grade', { gradeCode })
    user.value = data.user
    persist()
    return data.user
  }

  async function logout() {
    useNotificationsStore().disconnect().catch(() => {})
    try { if (token.value) await api.post('/auth/logout') } catch { /* ignore */ }
    token.value        = null
    refreshToken.value = null
    user.value         = null
    persist()
  }

  function logLocalHistory() {
    const entry = { id: Date.now(), timestamp: new Date().toISOString(), device: navigator.userAgent.slice(0, 60), status: 'success' }
    const hist = JSON.parse(localStorage.getItem('bap_login_history') || '[]')
    hist.unshift(entry)
    localStorage.setItem('bap_login_history', JSON.stringify(hist.slice(0, 50)))
    loginHistory.value = hist
  }

  function loadLoginHistory() {
    loginHistory.value = JSON.parse(localStorage.getItem('bap_login_history') || '[]')
  }

  // Optimistic local bump; server is source of truth on next fetch
  function updateCoins(amount) {
    if (user.value) {
      user.value.coins = (user.value.coins || 0) + amount
      persist()
    }
  }

  // Wire the 401 handler — after refresh fails, logout
  setUnauthorizedHandler(() => { logout() })

  return {
    user, token, refreshToken,
    isLoggedIn, isAdmin, hasGrade,
    login, register, fetchMe, updateGrade, logout,
    loginHistory, loadLoginHistory, updateCoins,
  }
})
