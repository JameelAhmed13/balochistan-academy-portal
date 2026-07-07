<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🪙</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Coins &amp; Finance</div>
        <div class="ag-banner-sub">Coin economy settings and how students are redeeming coins</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">
          📄 {{ total.toLocaleString() }} Redemption{{ total !== 1 ? 's' : '' }}
        </span>
      </div>
      <RouterLink to="/app/admin" class="btn-ghost" style="cursor:pointer;">
        <ArrowLeft class="w-4 h-4" />
      </RouterLink>
    </div>

    <!-- Pending Cash Subscriptions -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <Wallet class="w-3.5 h-3.5" style="color:#f59e0b;" />
        Pending Cash Subscriptions
        <span v-if="pending.length" class="badge-amber" style="margin-left:0.25rem;">{{ pending.length }}</span>
      </div>

      <div v-if="pendingLoading" class="ag-empty" style="padding:32px;">
        <div class="ag-empty-icon">⏳</div><p>Loading…</p>
      </div>
      <div v-else-if="!pending.length" class="ag-empty" style="padding:32px 24px;">
        <div class="ag-empty-icon">✅</div>
        <p>No payments awaiting verification.</p>
      </div>
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>Student</th><th>Plan</th><th>Price</th><th>Method</th><th>Reference</th><th>Submitted</th><th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in pending" :key="p.id">
              <td style="font-weight:500;color:var(--t-text1);">{{ p.userName }}</td>
              <td class="ag-table-muted">{{ p.planName }}</td>
              <td class="ag-table-muted">Rs. {{ p.price.toLocaleString() }}</td>
              <td class="ag-table-muted">{{ p.paymentMethod }}</td>
              <td class="ag-table-muted">{{ p.referenceNumber }}</td>
              <td class="ag-table-muted">{{ formatDate(p.submittedAt) }}</td>
              <td>
                <div style="display:flex;gap:0.4rem;">
                  <button class="btn-primary text-sm" style="padding:0.3rem 0.75rem;" :disabled="resolving === p.id" @click="resolvePending(p.id, true)">Approve</button>
                  <button class="btn-secondary text-sm" style="padding:0.3rem 0.75rem;color:#f87171;" :disabled="resolving === p.id" @click="resolvePending(p.id, false)">Reject</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Coin Distribution Chart -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <Award class="w-3.5 h-3.5" style="color:var(--t-gold);" />
        Coin Distribution — Top Students
      </div>
      <div class="ag-card-body">
        <div v-if="!coinChartSeries[0].data.length" class="ag-empty" style="padding:24px;">
          <p>No coins earned yet.</p>
        </div>
        <apexchart v-else type="bar" height="220" :options="coinChartOptions" :series="coinChartSeries" />
      </div>
    </div>

    <!-- Recent Redemptions -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <TrendingUp class="w-3.5 h-3.5" style="color:var(--t-accent);" />
        Recent Redemptions
      </div>

      <!-- Loading -->
      <div v-if="loading" class="ag-empty" style="padding:40px;">
        <div class="ag-empty-icon">⏳</div><p>Loading redemptions…</p>
      </div>

      <!-- Empty -->
      <div v-else-if="!redemptions.length" class="ag-empty" style="padding:48px 24px;">
        <div class="ag-empty-icon">🎉</div>
        <p>No redemptions yet — coins are only spent when a student buys or renews a subscription, or buys AI tokens.</p>
      </div>

      <!-- Table -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Student</th>
              <th>Type</th>
              <th>Detail</th>
              <th>Coins Spent</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(r, i) in redemptions" :key="r.id">
              <td class="ag-table-muted">{{ i + 1 }}</td>
              <td>
                <div style="display:flex;align-items:center;gap:0.6rem;">
                  <div class="ag-avatar" :style="avatarGrad(r.userName || '?')">{{ initials(r.userName || '?') }}</div>
                  <span style="font-weight:500;color:var(--t-text1);">{{ r.userName || 'Unknown' }}</span>
                </div>
              </td>
              <td><span :class="typeClass(r.type)">{{ typeLabel(r.type) }}</span></td>
              <td class="ag-table-muted">
                <template v-if="r.type === 'token_topup'">+{{ r.tokensGranted }} AI tokens</template>
                <template v-else>{{ r.planName || '—' }}</template>
              </td>
              <td style="color:#f87171;font-weight:700;">−{{ r.coinsSpent.toLocaleString() }} 🪙</td>
              <td class="ag-table-muted">{{ formatDate(r.createdAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" style="display:flex;justify-content:center;gap:0.5rem;padding:16px;">
        <button class="btn-ghost text-sm" :disabled="page <= 1" @click="page--">← Prev</button>
        <span style="font-size:0.8rem;color:var(--t-text3);align-self:center;">Page {{ page }} of {{ totalPages }}</span>
        <button class="btn-ghost text-sm" :disabled="page >= totalPages" @click="page++">Next →</button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { ArrowLeft, TrendingUp, Award, Wallet } from '@lucide/vue'
import { useThemeStore } from '@/stores/theme'
import { useToast } from 'primevue/usetoast'
import api from '@/services/api'
import { formatDate } from '@/utils/datetime'

const theme = useThemeStore()
const toast = useToast()

/* ── Recent Redemptions (real data) ── */
const redemptions = ref([])
const loading      = ref(false)
const total        = ref(0)
const page         = ref(1)
const pageSize     = 20
const totalPages   = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))

async function fetchRedemptions() {
  loading.value = true
  try {
    const { data } = await api.get('/admin/coins/redemptions', { params: { page: page.value, pageSize } })
    redemptions.value = data.items
    total.value       = data.total
  } finally {
    loading.value = false
  }
}
watch(page, fetchRedemptions)

/* ── Pending Cash Subscriptions ── */
const pending        = ref([])
const pendingLoading  = ref(false)
const resolving       = ref(null)

async function fetchPending() {
  pendingLoading.value = true
  try {
    const { data } = await api.get('/admin/subscriptions/pending')
    pending.value = data
  } finally {
    pendingLoading.value = false
  }
}

async function resolvePending(id, approve) {
  resolving.value = id
  try {
    await api.put(`/admin/subscriptions/pending/${id}`, { approve })
    pending.value = pending.value.filter(p => p.id !== id)
    toast.add({ severity: 'success', summary: approve ? 'Subscription activated' : 'Payment rejected', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not resolve', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    resolving.value = null
  }
}

/* ── Coin Distribution Chart (real top earners) ── */
const topEarners = ref([])
async function fetchTopEarners() {
  try {
    const { data } = await api.get('/admin/coins/top-earners', { params: { limit: 5 } })
    topEarners.value = data
  } catch { /* keep empty */ }
}

const coinChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', theme: { mode: theme.isDark ? 'dark' : 'light' } },
  xaxis: {
    categories: topEarners.value.map(e => e.userName),
    labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b', fontSize: '0.72rem' } },
  },
  yaxis: { labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  colors: ['#f59e0b'],
  dataLabels: { enabled: true, style: { fontSize: '0.7rem', colors: [theme.isDark ? '#fff' : '#1e293b'] } },
  grid:        { borderColor: theme.isDark ? 'rgba(255,255,255,0.06)' : '#e2e8f0', strokeDashArray: 3 },
  plotOptions: { bar: { borderRadius: 6, columnWidth: '52%' } },
  tooltip:     { theme: theme.isDark ? 'dark' : 'light' },
}))
const coinChartSeries = computed(() => [{ name: 'Coins Earned', data: topEarners.value.map(e => e.totalCoins) }])

onMounted(() => {
  fetchRedemptions()
  fetchPending()
  fetchTopEarners()
})

/* ── Helpers ── */
function initials(name) {
  return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase()
}
const gradients = [
  'linear-gradient(135deg,#7c6af5,#5b43cc)',
  'linear-gradient(135deg,#10b981,#0891b2)',
  'linear-gradient(135deg,#f59e0b,#ea580c)',
  'linear-gradient(135deg,#e11d48,#db2777)',
  'linear-gradient(135deg,#0891b2,#6366f1)',
]
function avatarGrad(name) {
  return `background: ${gradients[name.charCodeAt(0) % gradients.length]};`
}
const TYPE_LABELS = {
  subscription_purchase: 'Subscription Purchase',
  subscription_renewal:  'Renewal',
  token_topup:            'Token Top-up',
}
const TYPE_CLASSES = {
  subscription_purchase: 'badge-blue',
  subscription_renewal:  'badge-emerald',
  token_topup:            'badge-amber',
}
function typeLabel(t) { return TYPE_LABELS[t] ?? t }
function typeClass(t) { return TYPE_CLASSES[t] ?? 'badge-indigo' }
</script>

<style scoped>
/* ag-avatar-lg is global but redeclare for scoped safety */
.ag-avatar-lg {
  width: 44px; height: 44px; border-radius: 14px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  font-weight: 700; font-size: 1rem; color: #fff;
}
</style>
