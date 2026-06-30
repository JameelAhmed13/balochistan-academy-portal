import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export const useStudentStore = defineStore('student', () => {
  // ── Local test records (preserved for per-test views that need subject/type/bookId) ──
  const testRecords = ref(JSON.parse(localStorage.getItem('bap_tests') || '[]'))

  // ── API-backed stats (override localStorage-computed values when loaded) ──
  const _apiStats = ref(null)   // { attempts, avgPercent, passed, coinsEarned, coins }

  // ── API-backed coin data ──
  const coinTransactions = ref([])  // normalized to { id, label, amount, date, type }
  const withdrawals      = ref([])  // normalized to { id, amount, amountPkr, status, date, processedAt }
  const payoutAccount    = ref(null) // normalized to { id, name, number, service }

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
      labels: sorted.map(t => new Date(t.date).toLocaleDateString('en-PK', { day: 'numeric', month: 'short' })),
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

  // ── Fetch withdrawals ────────────────────────────────────────────────────────
  async function fetchWithdrawals() {
    try {
      const { data } = await api.get('/coins/withdrawals')
      withdrawals.value = data.map(w => ({
        id:          w.id,
        amount:      w.amountCoins,
        amountPkr:   w.amountPkr,
        status:      w.status,
        adminNote:   w.adminNote,
        date:        w.createdAt,
        processedAt: w.processedAt,
      }))
    } catch { /* keep empty */ }
  }

  // ── Fetch payout account ─────────────────────────────────────────────────────
  async function fetchPayoutAccount() {
    try {
      const { data } = await api.get('/coins/payout-account')
      payoutAccount.value = data ? { id: data.id, name: data.accountName, number: data.accountNo, service: data.service } : null
    } catch { /* keep null */ }
  }

  // ── Bootstrap coin data on demand ───────────────────────────────────────────
  async function fetchCoinData() {
    await Promise.allSettled([fetchCoinTransactions(), fetchWithdrawals(), fetchPayoutAccount()])
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
      local.serverId = res.data?.id
      if (res.data?.coinsEarned > 0) fetchStats().catch(() => {})
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

  // ── Withdrawal request ───────────────────────────────────────────────────────
  async function requestWithdrawal(amountCoins) {
    const { data } = await api.post('/coins/withdraw', { amountCoins })
    // Prepend to local list immediately
    withdrawals.value.unshift({
      id:        data.id,
      amount:    data.amountCoins,
      amountPkr: data.amountPkr,
      status:    data.status,
      date:      data.createdAt,
    })
    // Refresh stats to reflect deducted coins
    fetchStats().catch(() => {})
    return data
  }

  // ── Update payout account ────────────────────────────────────────────────────
  async function updatePayoutAccount(info) {
    const { data } = await api.put('/coins/payout-account', {
      accountName: info.name,
      accountNo:   info.number,
      service:     info.service,
    })
    payoutAccount.value = { id: data.id, name: data.accountName, number: data.accountNo, service: data.service }
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

  return {
    testRecords, coinTransactions, withdrawals, payoutAccount,
    totalTests, passedTests, avgPercent, totalCoins, monthlyTests, chartData,
    fetchStats, fetchCoinData, fetchCoinTransactions, fetchWithdrawals, fetchPayoutAccount,
    saveTest, submitAttempt, requestWithdrawal, updatePayoutAccount,
  }
})
