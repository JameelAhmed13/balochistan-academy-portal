import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api, { setUnauthorizedHandler } from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(JSON.parse(localStorage.getItem('bap_user') || 'null'))
  const token = ref(localStorage.getItem('bap_token') || null)
  const loginHistory = ref([])

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'admin')
  const hasGrade = computed(() => !!user.value?.gradeCode)

  function persist() {
    if (user.value) localStorage.setItem('bap_user', JSON.stringify(user.value))
    else localStorage.removeItem('bap_user')
    if (token.value) localStorage.setItem('bap_token', token.value)
    else localStorage.removeItem('bap_token')
  }

  function setSession({ token: t, user: u }) {
    token.value = t
    user.value = u
    persist()
    logLocalHistory()
  }

  // username/password against the real backend
  async function login(username, password) {
    const { data } = await api.post('/auth/login', { username, password })
    setSession(data)
    return data.user
  }

  // student self-registration (includes grade)
  async function register(payload) {
    const { data } = await api.post('/auth/register', payload)
    setSession(data)
    return data.user
  }

  // re-hydrate + validate token on app boot
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

  // update own profile (+ optional password change); refreshes the cached user
  async function updateProfile(payload) {
    const { data } = await api.put('/auth/me', payload)
    user.value = data.user
    persist()
    return data.user
  }

  async function logout() {
    try { if (token.value) await api.post('/auth/logout') } catch { /* ignore */ }
    user.value = null
    token.value = null
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

  // optimistic local bump; server is source of truth on next fetch
  function updateCoins(amount) {
    if (user.value) {
      user.value.coins = (user.value.coins || 0) + amount
      persist()
    }
  }

  // wire the 401 handler so any expired token logs out
  setUnauthorizedHandler(() => { logout() })

  return { user, token, isLoggedIn, isAdmin, hasGrade, login, register, fetchMe, updateProfile, logout, loginHistory, loadLoginHistory, updateCoins }
})
