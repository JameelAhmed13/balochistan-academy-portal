<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🪙</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Coins &amp; Finance</div>
        <div class="ag-banner-sub">Manage withdrawal requests and coin economy settings</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat" style="background:rgba(245,158,11,0.12);border-color:rgba(245,158,11,0.25);color:#fbbf24;">
          ⏳ {{ pendingCount }} Pending
        </span>
        <span class="ag-banner-stat">
          🪙 {{ totalCoinsIssued.toLocaleString() }} Issued
        </span>
      </div>
      <RouterLink to="/app/admin" class="btn-ghost" style="cursor:pointer;">
        <ArrowLeft class="w-4 h-4" />
      </RouterLink>
    </div>

    <!-- Settings Glass Card -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <Settings class="w-3.5 h-3.5" style="color:var(--t-accent);" />
        Coin Economy Settings
      </div>
      <div class="ag-card-body">
        <div class="settings-grid">
          <div>
            <label class="label">Coin → PKR Rate</label>
            <div style="display:flex;align-items:center;gap:0.5rem;">
              <input v-model.number="coinRate" type="number" step="0.1" min="0.1" class="input" />
              <span style="font-size:0.8rem;color:var(--t-text3);white-space:nowrap;">PKR / coin</span>
            </div>
            <p style="font-size:0.72rem;color:var(--t-text3);margin-top:0.35rem;">
              Current: 1000 coins = Rs. {{ (1000 * coinRate).toLocaleString() }}
            </p>
          </div>
          <div>
            <label class="label">Min Questions for Eligibility</label>
            <input v-model.number="minQs" type="number" min="1" class="input" />
            <p style="font-size:0.72rem;color:var(--t-text3);margin-top:0.35rem;">
              Students must answer ≥{{ minQs }} questions
            </p>
          </div>
          <div>
            <label class="label">Min Coins for Withdrawal</label>
            <input v-model.number="minWithdrawal" type="number" min="100" class="input" />
            <p style="font-size:0.72rem;color:var(--t-text3);margin-top:0.35rem;">
              Min Rs. {{ (minWithdrawal * coinRate).toFixed(0) }} per request
            </p>
          </div>
        </div>
        <button class="btn-primary mt-4" style="cursor:pointer;">
          <TrendingUp class="w-4 h-4" /> Save Settings
        </button>
      </div>
    </div>

    <!-- Pending Withdrawal Requests -->
    <div>
      <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:0.875rem;">
        <h3 style="font-size:0.9rem;font-weight:700;color:var(--t-text1);display:flex;align-items:center;gap:0.5rem;">
          <Award class="w-4 h-4" style="color:var(--t-accent);" />
          Pending Withdrawal Requests
          <span class="badge-amber" style="margin-left:0.25rem;">{{ pendingCount }}</span>
        </h3>
      </div>

      <div v-if="pendingRequests.length" style="display:flex;flex-direction:column;gap:0.875rem;">
        <div
          v-for="r in pendingRequests" :key="r.id"
          class="ag-grid-card"
          style="cursor:default;display:flex;align-items:center;flex-wrap:wrap;gap:1rem;"
        >
          <!-- Left: Avatar + Info -->
          <div style="display:flex;align-items:center;gap:0.875rem;flex:1;min-width:200px;">
            <div class="ag-avatar-lg" :style="avatarGrad(r.name)">{{ initials(r.name) }}</div>
            <div>
              <div style="font-weight:600;font-size:0.9rem;color:var(--t-text1);">{{ r.name }}</div>
              <div style="font-size:0.75rem;color:var(--t-text3);margin-top:0.15rem;">{{ r.account }}</div>
              <div style="font-size:0.72rem;color:var(--t-text3);margin-top:0.1rem;">{{ r.date }}</div>
            </div>
          </div>

          <!-- Center: Amounts -->
          <div style="display:flex;gap:1.5rem;align-items:center;flex-shrink:0;">
            <div style="text-align:center;">
              <div style="font-size:1.1rem;font-weight:800;color:var(--t-gold);">{{ r.coins.toLocaleString() }}</div>
              <div style="font-size:0.68rem;color:var(--t-text3);text-transform:uppercase;letter-spacing:0.05em;">Coins</div>
            </div>
            <div style="width:1px;height:32px;background:var(--t-border);flex-shrink:0;"></div>
            <div style="text-align:center;">
              <div style="font-size:1.1rem;font-weight:800;color:#34d399;">Rs.{{ r.pkr.toLocaleString() }}</div>
              <div style="font-size:0.68rem;color:var(--t-text3);text-transform:uppercase;letter-spacing:0.05em;">PKR</div>
            </div>
          </div>

          <!-- Right: Status + Actions -->
          <div style="display:flex;align-items:center;gap:0.6rem;flex-shrink:0;">
            <span class="badge-amber">Pending</span>
            <button
              @click="r.status = 'approved'"
              class="btn-primary"
              style="padding:0.35rem 0.75rem;font-size:0.78rem;cursor:pointer;"
            >
              <Check class="w-3.5 h-3.5" /> Approve
            </button>
            <button
              @click="r.status = 'rejected'"
              class="btn-secondary"
              style="padding:0.35rem 0.75rem;font-size:0.78rem;cursor:pointer;color:#f87171;border-color:rgba(239,68,68,0.25);"
            >
              <X class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      <div v-else class="ag-card">
        <div class="ag-empty">
          <div class="ag-empty-icon">🎉</div>
          <p>No pending withdrawal requests.</p>
        </div>
      </div>
    </div>

    <!-- Coin Distribution Chart -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <Award class="w-3.5 h-3.5" style="color:var(--t-gold);" />
        Coin Distribution — Top 5 Students
      </div>
      <div class="ag-card-body">
        <apexchart type="bar" height="220" :options="coinChartOptions" :series="coinChartSeries" />
      </div>
    </div>

    <!-- Completed Transactions Table -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="display:flex;align-items:center;gap:0.5rem;">
        <TrendingUp class="w-3.5 h-3.5" style="color:var(--t-accent);" />
        Completed Transactions
      </div>
      <div class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Student</th>
              <th>Coins</th>
              <th>PKR</th>
              <th>Account</th>
              <th>Date</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="!completedRequests.length">
              <td colspan="7" style="text-align:center;padding:3rem 1rem;color:var(--t-text3);">No completed transactions yet.</td>
            </tr>
            <tr v-for="(r, i) in completedRequests" :key="r.id">
              <td class="ag-table-muted">{{ i + 1 }}</td>
              <td>
                <div style="display:flex;align-items:center;gap:0.6rem;">
                  <div class="ag-avatar" :style="avatarGrad(r.name)">{{ initials(r.name) }}</div>
                  <span style="font-weight:500;color:var(--t-text1);">{{ r.name }}</span>
                </div>
              </td>
              <td style="color:var(--t-gold);font-weight:700;">{{ r.coins.toLocaleString() }} 🪙</td>
              <td style="color:#34d399;font-weight:600;">Rs. {{ r.pkr.toLocaleString() }}</td>
              <td class="ag-table-muted">{{ r.account }}</td>
              <td class="ag-table-muted">{{ r.date }}</td>
              <td><span :class="statusClass(r.status)">{{ r.status }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { ArrowLeft, Settings, Check, X, TrendingUp, Award } from '@lucide/vue'
import { useThemeStore } from '@/stores/theme'

const theme = useThemeStore()

/* ── Settings ── */
const coinRate      = ref(0.5)
const minQs         = ref(35)
const minWithdrawal = ref(300)

/* ── Mock Data ── */
const mockRequests = ref([
  { id: 1, name: 'Ahmed Khan',    coins: 500, pkr: 250, account: 'Easypaisa: 0300-1234567', date: '2026-06-09', status: 'pending'  },
  { id: 2, name: 'Fatima Ali',    coins: 350, pkr: 175, account: 'JazzCash: 0311-2345678',  date: '2026-06-08', status: 'pending'  },
  { id: 3, name: 'Bilal Hussain', coins: 400, pkr: 200, account: 'Easypaisa: 0344-1234567', date: '2026-06-01', status: 'approved' },
  { id: 4, name: 'Zainab Malik',  coins: 600, pkr: 300, account: 'JazzCash: 0322-9876543',  date: '2026-05-28', status: 'approved' },
  { id: 5, name: 'Sara Qureshi',  coins: 320, pkr: 160, account: 'Easypaisa: 0355-5678901', date: '2026-05-20', status: 'rejected' },
])

/* ── Filtered ── */
const pendingRequests   = computed(() => mockRequests.value.filter(r => r.status === 'pending'))
const completedRequests = computed(() => mockRequests.value.filter(r => r.status !== 'pending'))
const pendingCount      = computed(() => pendingRequests.value.length)
const totalCoinsIssued  = computed(() => mockRequests.value.filter(r => r.status === 'approved').reduce((s, r) => s + r.coins, 0))

/* ── Coin Distribution Chart ── */
const coinChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', theme: { mode: theme.isDark ? 'dark' : 'light' } },
  xaxis: {
    categories: ['Ahmed Khan', 'Fatima Ali', 'Zainab Malik', 'Bilal Hussain', 'Sara Qureshi'],
    labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b', fontSize: '0.72rem' } },
  },
  yaxis: { labels: { style: { colors: theme.isDark ? '#94a3b8' : '#64748b' } } },
  colors: ['#f59e0b'],
  dataLabels: { enabled: true, style: { fontSize: '0.7rem', colors: [theme.isDark ? '#fff' : '#1e293b'] } },
  grid:        { borderColor: theme.isDark ? 'rgba(255,255,255,0.06)' : '#e2e8f0', strokeDashArray: 3 },
  plotOptions: { bar: { borderRadius: 6, columnWidth: '52%' } },
  tooltip:     { theme: theme.isDark ? 'dark' : 'light' },
}))
const coinChartSeries = [{ name: 'Coins Earned', data: [1250, 980, 850, 340, 560] }]

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
function statusClass(s) {
  return { pending: 'badge-amber', approved: 'badge-green', rejected: 'badge-red' }[s] ?? 'badge-indigo'
}
</script>

<style scoped>
.settings-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.25rem;
}
@media (max-width: 640px) {
  .settings-grid { grid-template-columns: 1fr; }
}

/* ag-avatar-lg is global but redeclare for scoped safety */
.ag-avatar-lg {
  width: 44px; height: 44px; border-radius: 14px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  font-weight: 700; font-size: 1rem; color: #fff;
}
</style>
