<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🛡️</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Audit Log</div>
        <div class="ag-banner-sub">Who did what, to what, when, and from where</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">📄 {{ total.toLocaleString() }} Entries</span>
      </div>
      <RouterLink to="/app/admin" class="btn-ghost" style="cursor:pointer;">
        <ArrowLeft class="w-4 h-4" />
      </RouterLink>
    </div>

    <!-- Filters -->
    <div class="ag-card">
      <div class="ag-card-body" style="display:flex;gap:0.75rem;flex-wrap:wrap;align-items:flex-end;">
        <div>
          <label class="label">Action</label>
          <select v-model="filters.action" class="input mt-1" @change="applyFilters">
            <option value="">All actions</option>
            <option v-for="a in filterOptions.actions" :key="a" :value="a">{{ a }}</option>
          </select>
        </div>
        <div>
          <label class="label">Entity Type</label>
          <select v-model="filters.entityType" class="input mt-1" @change="applyFilters">
            <option value="">All entities</option>
            <option v-for="e in filterOptions.entityTypes" :key="e" :value="e">{{ e }}</option>
          </select>
        </div>
        <div>
          <label class="label">From</label>
          <input v-model="filters.from" type="date" class="input mt-1" @change="applyFilters" />
        </div>
        <div>
          <label class="label">To</label>
          <input v-model="filters.to" type="date" class="input mt-1" @change="applyFilters" />
        </div>
        <button class="btn-ghost text-sm" @click="resetFilters">Clear filters</button>
      </div>
    </div>

    <!-- Table -->
    <div class="ag-card" style="padding:0;overflow:hidden;">
      <div v-if="loading" class="ag-empty" style="padding:40px;">
        <div class="ag-empty-icon">⏳</div><p>Loading audit log…</p>
      </div>
      <div v-else-if="!entries.length" class="ag-empty" style="padding:48px 24px;">
        <div class="ag-empty-icon">🗒️</div>
        <p>No audit entries match these filters.</p>
      </div>
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Action</th>
              <th>Entity</th>
              <th>Details</th>
              <th>Actor</th>
              <th>IP Address</th>
              <th>When</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(e, i) in entries" :key="e.id">
              <td class="ag-table-muted">{{ (page - 1) * pageSize + i + 1 }}</td>
              <td><span :class="actionClass(e.action)">{{ e.action }}</span></td>
              <td class="ag-table-muted">{{ e.entityType || '—' }}{{ e.entityId ? ` #${e.entityId}` : '' }}</td>
              <td class="ag-table-muted" style="max-width:280px;">{{ summarize(e) }}</td>
              <td style="font-weight:500;color:var(--t-text1);">{{ e.userName || 'System' }}</td>
              <td class="ag-table-muted">{{ e.ipAddress || '—' }}</td>
              <td class="ag-table-muted" style="white-space:nowrap;">{{ formatDate(e.timestamp) }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" style="display:flex;justify-content:center;gap:0.5rem;padding:16px;">
        <button class="btn-ghost text-sm" :disabled="page <= 1" @click="page--; fetchEntries()">← Prev</button>
        <span style="font-size:0.8rem;color:var(--t-text3);align-self:center;">Page {{ page }} of {{ totalPages }}</span>
        <button class="btn-ghost text-sm" :disabled="page >= totalPages" @click="page++; fetchEntries()">Next →</button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ArrowLeft } from '@lucide/vue'
import api from '@/services/api'

const entries    = ref([])
const loading     = ref(false)
const total       = ref(0)
const page        = ref(1)
const pageSize    = 30
const totalPages  = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))

const filters = ref({ action: '', entityType: '', from: '', to: '' })
const filterOptions = ref({ actions: [], entityTypes: [] })

async function fetchEntries() {
  loading.value = true
  try {
    const params = { page: page.value, pageSize }
    if (filters.value.action)     params.action = filters.value.action
    if (filters.value.entityType) params.entityType = filters.value.entityType
    if (filters.value.from)       params.from = filters.value.from
    if (filters.value.to)         params.to = filters.value.to
    const { data } = await api.get('/admin/audit-log', { params })
    entries.value = data.items
    total.value   = data.total
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  fetchEntries()
}

function resetFilters() {
  filters.value = { action: '', entityType: '', from: '', to: '' }
  applyFilters()
}

async function fetchFilterOptions() {
  try {
    const { data } = await api.get('/admin/audit-log/filters')
    filterOptions.value = data
  } catch { /* leave empty */ }
}

onMounted(() => {
  fetchEntries()
  fetchFilterOptions()
})

const ACTION_CLASSES = {
  Create: 'badge-blue', Register: 'badge-blue', Assign: 'badge-blue',
  Update: 'badge-amber', Reply: 'badge-amber',
  Delete: 'badge-red', Reject: 'badge-red', Deactivate: 'badge-red',
  Publish: 'badge-emerald', Activate: 'badge-emerald', Approve: 'badge-emerald', Send: 'badge-emerald',
}
function actionClass(action) { return ACTION_CLASSES[action] ?? 'badge-indigo' }

function summarize(entry) {
  const raw = entry.newValues || entry.oldValues
  if (!raw) return '—'
  try {
    const obj = JSON.parse(raw)
    return Object.entries(obj).map(([k, v]) => `${k}: ${v}`).join(', ')
  } catch {
    return raw
  }
}

function formatDate(iso) {
  return new Date(iso).toLocaleString('en-PK', { timeZone: 'Asia/Karachi', dateStyle: 'medium', timeStyle: 'short' })
}
</script>
