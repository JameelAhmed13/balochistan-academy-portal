import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useStudentStore = defineStore('student', () => {
  const testRecords = ref(JSON.parse(localStorage.getItem('estudy_tests') || '[]'))
  const coinTransactions = ref(JSON.parse(localStorage.getItem('estudy_coins') || '[]'))
  const withdrawals = ref(JSON.parse(localStorage.getItem('estudy_withdrawals') || '[]'))
  const payoutAccount = ref(JSON.parse(localStorage.getItem('estudy_payout') || 'null'))

  const totalTests    = computed(() => testRecords.value.length)
  const passedTests   = computed(() => testRecords.value.filter(t => (t.score / t.total) >= 0.5).length)
  const avgPercent    = computed(() => {
    if (!testRecords.value.length) return 0
    const sum = testRecords.value.reduce((a, t) => a + (t.score / t.total) * 100, 0)
    return Math.round(sum / testRecords.value.length)
  })
  const totalCoins    = computed(() => coinTransactions.value.reduce((a, c) => a + c.amount, 0))
  const monthlyTests  = computed(() => {
    const now = new Date(); const m = now.getMonth(); const y = now.getFullYear()
    return testRecords.value.filter(t => { const d = new Date(t.date); return d.getMonth() === m && d.getFullYear() === y }).length
  })

  function saveTest(record) {
    testRecords.value.unshift({ ...record, id: Date.now(), date: new Date().toISOString() })
    localStorage.setItem('estudy_tests', JSON.stringify(testRecords.value.slice(0, 200)))
  }

  function addCoinTransaction(tx) {
    coinTransactions.value.unshift({ ...tx, id: Date.now(), date: new Date().toISOString() })
    localStorage.setItem('estudy_coins', JSON.stringify(coinTransactions.value))
  }

  function requestWithdrawal(amount, account) {
    const pending = withdrawals.value.find(w => w.status === 'pending')
    if (pending) throw new Error('A withdrawal request is already pending.')
    if (totalCoins.value < 300) throw new Error('Minimum 300 coins required.')
    const req = { id: Date.now(), amount, account, status: 'pending', date: new Date().toISOString() }
    withdrawals.value.unshift(req)
    addCoinTransaction({ amount: -amount, type: 'withdrawal', label: 'Withdrawal Request' })
    localStorage.setItem('estudy_withdrawals', JSON.stringify(withdrawals.value))
    return req
  }

  function updatePayoutAccount(info) {
    payoutAccount.value = info
    localStorage.setItem('estudy_payout', JSON.stringify(info))
  }

  // Score chart data
  const chartData = computed(() => {
    const sorted = [...testRecords.value].sort((a, b) => new Date(a.date) - new Date(b.date)).slice(-20)
    return {
      labels: sorted.map(t => new Date(t.date).toLocaleDateString('en-PK', { day: 'numeric', month: 'short' })),
      series: [{ name: 'Score %', data: sorted.map(t => Math.round((t.score / t.total) * 100)) }],
    }
  })

  return {
    testRecords, coinTransactions, withdrawals, payoutAccount,
    totalTests, passedTests, avgPercent, totalCoins, monthlyTests, chartData,
    saveTest, addCoinTransaction, requestWithdrawal, updatePayoutAccount,
  }
})
