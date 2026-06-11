import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(JSON.parse(localStorage.getItem('bap_user') || 'null'))
  const token = ref(localStorage.getItem('bap_token') || null)
  const loginHistory = ref([])

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'admin')

  async function login(username, phone) {
    // Demo: accept any credentials, mock user
    const mockUser = {
      id: 1,
      name: username || 'Ahmed Khan',
      phone: phone || '03001234567',
      role: username === 'admin' ? 'admin' : 'student',
      institute: 'New Century Educational System',
      grade: 9,
      board: 'Balochistan',
      coins: 1250,
      avatar: null,
    }
    const mockToken = 'demo-token-' + Date.now()
    user.value = mockUser
    token.value = mockToken
    localStorage.setItem('bap_user', JSON.stringify(mockUser))
    localStorage.setItem('bap_token', mockToken)

    // Log history
    const entry = {
      id: Date.now(),
      timestamp: new Date().toISOString(),
      ip: '192.168.1.x',
      device: navigator.userAgent.slice(0, 60),
      status: 'success',
    }
    const hist = JSON.parse(localStorage.getItem('bap_login_history') || '[]')
    hist.unshift(entry)
    localStorage.setItem('bap_login_history', JSON.stringify(hist.slice(0, 50)))
    loginHistory.value = hist
    return mockUser
  }

  function logout() {
    user.value = null
    token.value = null
    localStorage.removeItem('bap_user')
    localStorage.removeItem('bap_token')
  }

  function loadLoginHistory() {
    loginHistory.value = JSON.parse(localStorage.getItem('bap_login_history') || '[]')
  }

  function updateCoins(amount) {
    if (user.value) {
      user.value.coins = (user.value.coins || 0) + amount
      localStorage.setItem('bap_user', JSON.stringify(user.value))
    }
  }

  return { user, token, isLoggedIn, isAdmin, login, logout, loginHistory, loadLoginHistory, updateCoins }
})
