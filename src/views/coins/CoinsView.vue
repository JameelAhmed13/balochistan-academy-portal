<template>
  <div class="animate-fade-in space-y-6">
    <!-- Wallet Dashboard -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="card p-5 text-center col-span-1 sm:col-span-2 lg:col-span-1 bg-gradient-to-br from-amber-500 to-orange-500 text-white">
        <div class="text-4xl font-bold">{{ totalCoins }}</div>
        <div class="text-white/80 text-sm mt-1">🪙 Total Coins Earned</div>
      </div>
      <div class="card p-5 text-center">
        <div class="text-2xl font-bold text-emerald-700">Rs. {{ (totalCoins * 0.5).toFixed(0) }}</div>
        <div class="text-xs text-slate-500 mt-1">Converted to PKR (0.50/coin)</div>
      </div>
      <div class="card p-5 text-center">
        <div class="text-2xl font-bold text-slate-800">{{ totalEarned }}</div>
        <div class="text-xs text-slate-500 mt-1">Total Amount Earned</div>
      </div>
      <div class="card p-5 text-center">
        <div class="text-2xl font-bold text-amber-600">{{ pendingAmount }}</div>
        <div class="text-xs text-slate-500 mt-1">Payment Under Clearance</div>
      </div>
    </div>

    <!-- Earning rules -->
    <div class="card p-5">
      <h3 class="font-semibold text-slate-700 mb-3 flex items-center gap-2"><Info class="w-4 h-4 text-blue-500" /> How to Earn Coins</h3>
      <div class="space-y-2 text-sm text-slate-600">
        <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Earn from Objective Self Tests, Parent Tests, and Follow-up Tests</div>
        <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Test must contain at least <strong>35 questions</strong> to be eligible</div>
        <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Each test type per book earns coins for only the <strong>first qualifying attempt</strong></div>
        <div class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Coins awarded scale with your <strong>score percentage</strong></div>
      </div>
    </div>

    <!-- Withdraw & Account -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-5">
      <!-- Payout account -->
      <div class="card p-5 space-y-4">
        <h3 class="font-semibold text-slate-700">Payout Account</h3>
        <div v-if="student.payoutAccount" class="p-3 rounded-xl bg-slate-50 border border-slate-200 text-sm">
          <div class="font-medium text-slate-700">{{ student.payoutAccount.name }}</div>
          <div class="text-slate-500">{{ student.payoutAccount.number }}</div>
          <div class="text-xs text-slate-400 mt-1">{{ student.payoutAccount.service }}</div>
        </div>
        <div v-else class="text-sm text-slate-500 p-3 rounded-xl bg-slate-50 border border-slate-200">No payout account configured.</div>
        <button @click="accountModal = true" class="btn-secondary text-sm"><Settings class="w-4 h-4" /> Update Account</button>
      </div>

      <!-- Withdraw -->
      <div class="card p-5 space-y-4">
        <h3 class="font-semibold text-slate-700">Withdrawal</h3>
        <div class="p-3 rounded-xl bg-amber-50 border border-amber-100 text-sm text-amber-700">
          Minimum <strong>300 coins</strong> required. Current: <strong>{{ totalCoins }} coins</strong>
        </div>
        <div>
          <label class="label">Amount (coins)</label>
          <input v-model.number="withdrawAmount" type="number" class="input" :max="totalCoins" min="300" placeholder="300" />
          <p class="text-xs text-slate-500 mt-1">= Rs. {{ (withdrawAmount * 0.5).toFixed(0) }}</p>
        </div>
        <button @click="requestWithdrawal" :disabled="totalCoins < 300 || !student.payoutAccount" class="btn-primary w-full justify-center">
          <Send class="w-4 h-4" /> Request Withdrawal
        </button>
        <p v-if="!student.payoutAccount" class="text-xs text-red-500">Update your payout account first.</p>
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

  <!-- Account Modal -->
  <Dialog v-model:visible="accountModal" header="Update Payout Account" :modal="true" :style="{ width: '420px' }">
    <form @submit.prevent="saveAccount" class="space-y-4">
      <div>
        <label class="label">Account Name</label>
        <input v-model="accountForm.name" type="text" class="input" placeholder="Full name on account" required />
      </div>
      <div>
        <label class="label">Account Number / CNIC</label>
        <input v-model="accountForm.number" type="text" class="input" placeholder="03xxxxxxxxx or CNIC" required />
      </div>
      <div>
        <label class="label">Service</label>
        <select v-model="accountForm.service" class="input">
          <option>Easypaisa</option>
          <option>JazzCash</option>
          <option>Bank Transfer</option>
        </select>
      </div>
      <div class="flex gap-2">
        <button type="submit" class="btn-primary flex-1 justify-center">Save Account</button>
        <button type="button" @click="accountModal = false" class="btn-secondary">Cancel</button>
      </div>
    </form>
  </Dialog>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Send, Settings, Info, CheckCircle } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import { useToast } from 'primevue/usetoast'
import { useStudentStore } from '@/stores/student'

const student = useStudentStore()
const toast = useToast()

const accountModal   = ref(false)
const withdrawAmount = ref(300)
const accountForm    = ref({ name: '', number: '', service: 'Easypaisa' })
const saving         = ref(false)
const withdrawing    = ref(false)

const totalCoins    = computed(() => student.totalCoins)
const totalEarned   = computed(() => `Rs. ${student.withdrawals.filter(w => w.status === 'approved').reduce((a, w) => a + w.amount * 0.5, 0).toFixed(0)}`)
const pendingAmount = computed(() => `Rs. ${student.withdrawals.filter(w => w.status === 'pending').reduce((a, w) => a + w.amount * 0.5, 0).toFixed(0)}`)

onMounted(() => student.fetchCoinData())

async function requestWithdrawal() {
  withdrawing.value = true
  try {
    await student.requestWithdrawal(withdrawAmount.value)
    toast.add({ severity: 'success', summary: 'Withdrawal Requested', detail: 'Your request has been submitted for review.', life: 4000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Error', detail: e.message, life: 4000 })
  } finally {
    withdrawing.value = false
  }
}

async function saveAccount() {
  saving.value = true
  try {
    await student.updatePayoutAccount({ ...accountForm.value })
    accountModal.value = false
    toast.add({ severity: 'success', summary: 'Account Updated', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Error', detail: e.message, life: 4000 })
  } finally {
    saving.value = false
  }
}

function formatDate(iso) { return new Date(iso).toLocaleDateString('en-PK', { day: 'numeric', month: 'short', year: 'numeric' }) }
</script>

