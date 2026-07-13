import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api, { setSubscriptionRequiredHandler } from '@/services/api'
import router from '@/router'

export const useStudentStore = defineStore('student', () => {
  // ── Local test records (preserved for per-test views that need subject/type/bookId) ──
  const testRecords = ref(JSON.parse(localStorage.getItem('bap_tests') || '[]'))

  // ── API-backed stats (override localStorage-computed values when loaded) ──
  const _apiStats = ref(null)   // { attempts, avgPercent, passed, coinsEarned, coins }

  // ── API-backed coin data ──
  const coinTransactions = ref([])  // normalized to { id, label, amount, date, type }
  const balance          = ref(null) // { coins, pkr, earnedToday } from /coins/balance

  // ── API-backed subscription / AI token data ──
  const subscription      = ref(null) // { id, planId, planName, startDate, endDate, status, aiTokenQuota, aiTokensUsedThisPeriod, tokensRemainingThisPeriod } | null
  const pendingCashPurchase = ref(null) // { id, planId, planName, paymentMethod, referenceNumber } | null — awaiting admin verification
  const bonusAiTokens     = ref(0)
  const coinsPerAiToken   = ref(20) // coin cost of one extra AI token — refreshed from /subscriptions/me
  const totalAiTokens     = computed(() => (subscription.value?.tokensRemainingThisPeriod ?? 0) + bonusAiTokens.value)
  const hasActiveSubscription = computed(() => subscription.value?.status === 'active')

  // ── Trial / access status: 'trial' | 'subscribed' | 'expired' | null (not yet loaded) ──
  const accessStatus  = ref(null)
  const trialDaysLeft = ref(0)
  const trialEndsAt   = ref(null)

  // ── Computed stats (prefer API, fallback to local) ──
  const totalTests  = computed(() => _apiStats.value?.attempts  ?? testRecords.value.length)
  const passedTests = computed(() => _apiStats.value?.passed    ?? testRecords.value.filter(t => (t.score / t.total) >= 0.5).length)
  const avgPercent  = computed(() => {
    if (_apiStats.value) return Math.round(_apiStats.value.avgPercent || 0)
    if (!testRecords.value.length) return 0
    return Math.round(testRecords.value.reduce((a, t) => a + (t.score / t.total) * 100, 0) / testRecords.value.length)
  })
  const monthlyTests = computed(() => {
    const now = new Date(); const m = now.getMonth(); const y = now.getFullYear()
    return testRecords.value.filter(t => { const d = new Date(t.date); return d.getMonth() === m && d.getFullYear() === y }).length
  })
  const totalCoins = computed(() => _apiStats.value?.coins ?? coinTransactions.value.reduce((a, c) => a + c.amount, 0))

  const chartData = computed(() => {
    const sorted = [...testRecords.value].sort((a, b) => new Date(a.date) - new Date(b.date)).slice(-20)
    return {
      labels: sorted.map(t => new Date(t.date).toLocaleDateString('en-PK', { timeZone: 'Asia/Karachi', day: 'numeric', month: 'short' })),
      series: [{ name: 'Score %', data: sorted.map(t => Math.round((t.score / t.total) * 100)) }],
    }
  })

  // ── Fetch API stats ──────────────────────────────────────────────────────────
  async function fetchStats() {
    try {
      const { data } = await api.get('/tests/me/stats')
      _apiStats.value = data
    } catch { /* keep local fallback */ }
  }

  // ── Fetch coin ledger ────────────────────────────────────────────────────────
  async function fetchCoinTransactions() {
    try {
      const { data } = await api.get('/coins/transactions', { params: { pageSize: 100 } })
      coinTransactions.value = data.items.map(c => ({
        id:     c.id,
        label:  c.reason || 'Coin transaction',
        amount: c.amount,
        date:   c.timestamp,
        type:   c.amount > 0 ? 'Earned' : 'Deducted',
      }))
    } catch { /* keep empty */ }
  }

  // ── Fetch coin balance (real PKR-equivalent rate, not hardcoded client-side) ──
  async function fetchBalance() {
    try {
      const { data } = await api.get('/coins/balance')
      balance.value = data
    } catch { /* keep null */ }
  }

  // ── Fetch current subscription + AI token balance ────────────────────────────
  async function fetchSubscription() {
    try {
      const { data } = await api.get('/subscriptions/me')
      subscription.value = data.subscription
      pendingCashPurchase.value = data.pendingCashPurchase
      bonusAiTokens.value = data.bonusAiTokens
      coinsPerAiToken.value = data.coinsPerAiToken
      accessStatus.value = data.accessStatus
      trialDaysLeft.value = data.trialDaysLeft
      trialEndsAt.value = data.trialEndsAt
    } catch { /* keep defaults */ }
  }

  // ── Fetch redeemable subscription plans (with coin cost) ──────────────────────
  const plans = ref([])
  async function fetchPlans() {
    try {
      const { data } = await api.get('/subscriptions/plans')
      plans.value = data
    } catch { /* keep empty */ }
  }

  // ── Bootstrap coin data on demand ───────────────────────────────────────────
  async function fetchCoinData() {
    await Promise.allSettled([fetchCoinTransactions(), fetchBalance(), fetchSubscription()])
  }

  // ── Save test record locally + fire attempt to backend ──────────────────────
  function saveTest(record) {
    const local = { ...record, id: Date.now(), date: new Date().toISOString() }
    testRecords.value.unshift(local)
    localStorage.setItem('bap_tests', JSON.stringify(testRecords.value.slice(0, 200)))

    // Fire-and-forget backend attempt for coins/leaderboard
    api.post('/tests/attempts', {
      testId:      record.testId || null,
      score:       record.score       || 0,
      total:       record.total       || 0,
      durationSec: record.durationSec || 0,
      answersJson: record.answers ? JSON.stringify(record.answers) : null,
      attemptType: mapAttemptType(record.type),
    }).then(res => {
      // Replace the array item with a new object so Vue's reactivity picks up new fields
      const idx = testRecords.value.findIndex(r => r.id === local.id)
      if (idx !== -1) {
        testRecords.value[idx] = { ...testRecords.value[idx], serverId: res.data?.id, coins: res.data?.coinsEarned ?? 0 }
        localStorage.setItem('bap_tests', JSON.stringify(testRecords.value.slice(0, 200)))
      }
      if (res.data?.coinsEarned > 0) {
        fetchStats().catch(() => {})
        fetchCoinData().catch(() => {})
      }
    }).catch(() => {})

    return local.id
  }

  // ── Submit attempt directly (for views that have a backend test ID) ──────────
  async function submitAttempt(payload) {
    const { data } = await api.post('/tests/attempts', payload)
    // Also keep a local record for report views
    testRecords.value.unshift({
      id:      data.id,
      serverId: data.id,
      score:   data.score,
      total:   data.total,
      date:    data.submittedAt,
      type:    payload.attemptType || 'test',
    })
    localStorage.setItem('bap_tests', JSON.stringify(testRecords.value.slice(0, 200)))
    return data
  }

  // ── Redeem coins for a new (or switched) subscription ─────────────────────────
  async function purchaseSubscription(planId) {
    const { data } = await api.post('/subscriptions/me/purchase', { planId })
    await Promise.allSettled([fetchSubscription(), fetchBalance(), fetchStats()])
    return data
  }

  // ── Redeem coins to renew the current subscription ────────────────────────────
  async function renewSubscription() {
    const { data } = await api.post('/subscriptions/me/renew')
    await Promise.allSettled([fetchSubscription(), fetchBalance(), fetchStats()])
    return data
  }

  // ── Redeem coins for extra AI tokens ──────────────────────────────────────────
  async function buyTokens(tokenAmount) {
    const { data } = await api.post('/subscriptions/me/buy-tokens', { tokenAmount })
    bonusAiTokens.value = data.bonusAiTokens
    await Promise.allSettled([fetchBalance(), fetchStats()])
    return data
  }

  // ── Submit a real-money payment for a plan, pending admin verification ────────
  async function purchaseCash(planId, paymentMethod, referenceNumber) {
    const { data } = await api.post('/subscriptions/me/purchase-cash', { planId, paymentMethod, referenceNumber })
    await fetchSubscription()
    return data
  }

  // ── Single-shot AI prompt (Dictionary, Join Forces, Learn Coding, Simulations) ─
  // Spends one AI token via the shared backend gate — throws on 402 when out of tokens
  // (caller should catch and check err.response?.data?.outOfTokens).
  async function generateAi(prompt, system) {
    const { data } = await api.post('/ai/generate', { prompt, system })
    if (subscription.value) subscription.value.tokensRemainingThisPeriod = data.tokensRemainingThisPeriod
    bonusAiTokens.value = data.bonusAiTokens
    return data.reply
  }

  // ── Sync server-side attempt history into testRecords (once per session) ────
  const _attemptsFetched = ref(false)
  async function fetchAttempts() {
    if (_attemptsFetched.value) return
    _attemptsFetched.value = true
    try {
      const { data } = await api.get('/tests/me/attempts', { params: { pageSize: 200 } })
      const items = data?.items || data || []
      const existingIds = new Set(testRecords.value.filter(r => r.serverId).map(r => String(r.serverId)))
      const newRecords = items
        .filter(r => !existingIds.has(String(r.id)))
        .map(r => ({
          id: r.id,
          serverId: r.id,
          subject: r.subject || r.subjectName || '',
          type: r.attemptType || r.type || 'self-test',
          score: r.score ?? 0,
          total: r.total ?? 0,
          date: r.createdAt || r.submittedAt || r.date || new Date().toISOString(),
        }))
      if (newRecords.length) {
        testRecords.value.push(...newRecords)
        localStorage.setItem('bap_tests', JSON.stringify(testRecords.value.slice(0, 200)))
      }
    } catch { /* keep local-only data */ }
  }

  // ── Map UI type strings to backend attemptType values ───────────────────────
  function mapAttemptType(type) {
    if (!type) return 'self-test'
    const t = type.toLowerCase()
    if (t.includes('self'))      return 'self-test'
    if (t.includes('parent'))    return 'parent-test'
    if (t.includes('challenge')) return 'challenge'
    if (t.includes('weekly'))    return 'weekly'
    if (t.includes('monthly'))   return 'monthly'
    if (t.includes('daily'))     return 'daily'
    if (t.includes('objective')) return 'objective'
    if (t.includes('subject'))   return 'subjective'
    return 'self-test'
  }

  // Backend blocks any gated call once the trial has ended with no active subscription —
  // bounce straight to the paywall regardless of which screen triggered the call.
  setSubscriptionRequiredHandler(() => {
    accessStatus.value = 'expired'
    if (router.currentRoute.value.name !== 'Subscribe') router.push({ name: 'Subscribe' })
  })

  return {
    testRecords, coinTransactions, balance, subscription, pendingCashPurchase, hasActiveSubscription,
    bonusAiTokens, coinsPerAiToken, totalAiTokens, plans, accessStatus, trialDaysLeft, trialEndsAt,
    totalTests, passedTests, avgPercent, totalCoins, monthlyTests, chartData,
    fetchStats, fetchCoinData, fetchCoinTransactions, fetchBalance, fetchSubscription, fetchPlans,
    saveTest, submitAttempt, purchaseSubscription, renewSubscription, buyTokens, purchaseCash, generateAi, fetchAttempts,
  }
})
