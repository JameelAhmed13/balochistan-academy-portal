<template>
  <div class="animate-fade-in space-y-6">
    <!-- Wallet Dashboard -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="card p-5 text-center bg-gradient-to-br from-amber-500 to-orange-500 text-white">
        <div class="text-4xl font-bold">{{ totalCoins }}</div>
        <div class="text-white/80 text-sm mt-1">🪙 Total Coins Earned</div>
      </div>
      <div class="card p-5 text-center">
        <div class="text-2xl font-bold text-emerald-700">Rs. {{ (student.balance?.pkr ?? 0).toFixed(0) }}</div>
        <div class="text-xs text-slate-500 mt-1">Subscription value equivalent</div>
      </div>
      <div class="card p-5 text-center">
        <div class="text-2xl font-bold text-slate-800">{{ student.balance?.earnedToday ?? 0 }}</div>
        <div class="text-xs text-slate-500 mt-1">Earned Today</div>
      </div>
    </div>

    <!-- Earning rules + Redeem CTA -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-5">
      <div class="card p-5">
        <h3 class="font-semibold text-slate-700 mb-3 flex items-center gap-2"><Info class="w-4 h-4 text-blue-500" /> How to Earn Coins</h3>
        <div class="space-y-2 text-sm text-slate-600">
          <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Earn from Objective Self Tests, Parent Tests, and Follow-up Tests</div>
          <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Test must contain at least <strong>35 questions</strong> to be eligible</div>
          <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Each test type per book earns coins for only the <strong>first qualifying attempt</strong></div>
          <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Coins awarded scale with your <strong>score percentage</strong></div>
        </div>
      </div>

      <div class="card p-5 flex flex-col justify-between">
        <div>
          <h3 class="font-semibold text-slate-700 mb-2 flex items-center gap-2"><Gift class="w-4 h-4 text-violet-500" /> Redeem Your Coins</h3>
          <p class="text-sm text-slate-600">
            Coins buy a new subscription, renew your current one, or extra AI tokens — spend them from the Redeem page.
          </p>
        </div>
        <RouterLink to="/app/coins/redeem" class="btn-primary w-full justify-center mt-4">
          <Sparkles class="w-4 h-4" /> Redeem Coins
        </RouterLink>
      </div>
    </div>

    <!-- Transaction History -->
    <div class="card">
      <div class="p-4 border-b border-slate-100 flex items-center justify-between">
        <h3 class="font-semibold text-slate-700">Transaction History</h3>
        <span class="badge-blue">{{ student.coinTransactions.length }} records</span>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-xs text-slate-500 uppercase tracking-wider border-b border-slate-100">
              <th class="px-4 py-3 text-left">Sr</th>
              <th class="px-4 py-3 text-left">Type</th>
              <th class="px-4 py-3 text-left">Amount</th>
              <th class="px-4 py-3 text-left">Date</th>
              <th class="px-4 py-3 text-left">Status</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="(tx, i) in student.coinTransactions" :key="tx.id" class="hover:bg-slate-50 transition-colors">
              <td class="px-4 py-3 text-slate-500">{{ i + 1 }}</td>
              <td class="px-4 py-3 text-slate-700">{{ tx.label }}</td>
              <td class="px-4 py-3 font-semibold" :class="tx.amount > 0 ? 'text-emerald-600' : 'text-red-600'">
                {{ tx.amount > 0 ? '+' : '' }}{{ tx.amount }} 🪙
              </td>
              <td class="px-4 py-3 text-xs text-slate-500">{{ formatDate(tx.date) }}</td>
              <td class="px-4 py-3"><span :class="tx.amount > 0 ? 'badge-green' : 'badge-amber'">{{ tx.type }}</span></td>
            </tr>
            <tr v-if="!student.coinTransactions.length">
              <td colspan="5" class="px-4 py-12 text-center text-slate-400 text-sm">No transactions yet.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { Info, CheckCircle, Gift, Sparkles } from '@lucide/vue'
import { useStudentStore } from '@/stores/student'

const student = useStudentStore()

const totalCoins = computed(() => student.totalCoins)

onMounted(() => Promise.allSettled([student.fetchCoinData(), student.fetchStats()]))

function formatDate(iso) { return new Date(iso).toLocaleDateString('en-PK', { timeZone: 'Asia/Karachi', day: 'numeric', month: 'short', year: 'numeric' }) }
</script>
