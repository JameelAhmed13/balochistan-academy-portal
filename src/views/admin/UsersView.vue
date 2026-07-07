<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">👥</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">User Management</div>
        <div class="ag-banner-sub">Students, teachers, parents and administrators</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">👨‍🎓 {{ counts.student }} Students</span>
        <span class="ag-banner-stat">👨‍🏫 {{ counts.teacher }} Teachers</span>
        <span class="ag-banner-stat">🛡️ {{ counts.admin }} Admins</span>
      </div>
      <button class="btn-primary text-sm" @click="openAddModal">
        <Plus class="w-4 h-4" /> Add User
      </button>
    </div>

    <!-- KPI Row -->
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
      <KpiCard label="Students" :value="counts.student" icon="👨‍🎓" color="blue" />
      <KpiCard label="Teachers" :value="counts.teacher" icon="👨‍🏫" color="green" />
      <KpiCard label="Parents"  :value="counts.parent"  icon="👨‍👩‍👧" color="amber" />
      <KpiCard label="Admins"   :value="counts.admin"   icon="🛡️" color="purple" />
    </div>

    <!-- Controls Card -->
    <div class="ag-card">
      <!-- Tabs -->
      <div class="ag-tabs">
        <button
          v-for="tab in tabs" :key="tab"
          :class="['ag-tab', { active: activeTab === tab }]"
          @click="setTab(tab)"
          style="cursor: pointer"
        >{{ tab }}</button>
      </div>

      <!-- Filter Bar -->
      <div class="ag-filter-bar">
        <div class="ag-filter-group flex-1" style="min-width: 200px;">
          <label class="ag-filter-label">Search</label>
          <div style="position: relative;">
            <Search class="w-3.5 h-3.5" style="position:absolute;left:0.6rem;top:50%;transform:translateY(-50%);color:var(--t-text3);pointer-events:none;" />
            <input v-model="search" type="text" class="input" placeholder="Search name or username…" style="padding-left: 2rem;" @input="debounceFetch" />
          </div>
        </div>
        <div class="ag-filter-group">
          <label class="ag-filter-label">Institute</label>
          <select v-model="filterInstitute" class="input" style="min-width: 160px;" @change="page = 1; fetchUsers()">
            <option value="">All Institutes</option>
            <option v-for="inst in institutes" :key="inst.id" :value="inst.id">{{ inst.name }}</option>
          </select>
        </div>
        <div class="ag-filter-group" style="align-self: flex-end;">
          <div class="ag-view-toggle">
            <button :class="['ag-view-btn', { active: view === 'grid' }]" @click="view = 'grid'" title="Grid view" style="cursor:pointer;">
              <LayoutGrid class="w-4 h-4" />
            </button>
            <button :class="['ag-view-btn', { active: view === 'list' }]" @click="view = 'list'" title="List view" style="cursor:pointer;">
              <List class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      <!-- Grid View -->
      <div v-if="view === 'grid'" style="padding: 1.25rem 1.5rem;">
        <!-- Loading skeleton -->
        <div v-if="loading" class="users-grid">
          <div v-for="n in 8" :key="n" class="ag-grid-card user-card" style="opacity:0.5;">
            <div style="display:flex;align-items:center;gap:0.75rem;margin-bottom:0.875rem;">
              <div style="width:44px;height:44px;border-radius:14px;background:var(--t-surface2);flex-shrink:0;"></div>
              <div style="flex:1;">
                <div style="height:14px;background:var(--t-surface2);border-radius:4px;margin-bottom:6px;width:70%;"></div>
                <div style="height:11px;background:var(--t-surface2);border-radius:4px;width:50%;"></div>
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="users.length" class="users-grid">
          <div
            v-for="u in users" :key="u.id"
            class="ag-grid-card user-card"
            style="cursor: default;"
          >
            <div style="display:flex;align-items:center;gap:0.75rem;margin-bottom:0.875rem;">
              <div class="user-avatar" :style="avatarGradient(u.name || u.username)">{{ initials(u.name || u.username) }}</div>
              <div style="min-width:0;flex:1;">
                <div style="font-weight:600;font-size:0.9rem;color:var(--t-text1);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">{{ u.name || u.username }}</div>
                <div style="font-size:0.75rem;color:var(--t-text3);">{{ u.phone || u.username }}</div>
              </div>
            </div>
            <div style="display:flex;align-items:center;gap:0.5rem;flex-wrap:wrap;margin-bottom:0.75rem;">
              <span :class="roleClass(u.role)">{{ u.role }}</span>
              <span :class="u.isActive ? 'badge-green' : 'badge-red'">{{ u.isActive ? 'Active' : 'Inactive' }}</span>
            </div>
            <div v-if="u.instituteName" style="font-size:0.75rem;color:var(--t-text2);display:flex;align-items:center;gap:0.35rem;margin-bottom:0.5rem;">
              <Shield class="w-3 h-3" style="flex-shrink:0;color:var(--t-text3);" />
              {{ u.instituteName }}
            </div>
            <div style="font-size:0.75rem;color:var(--t-gold);font-weight:600;margin-bottom:0.875rem;">
              🪙 {{ u.coins }} coins
            </div>
            <div style="display:flex;gap:0.4rem;padding-top:0.75rem;border-top:1px solid var(--t-border);">
              <button
                @click="toggleActive(u)"
                class="btn-ghost"
                style="padding:0.35rem 0.6rem;font-size:0.75rem;cursor:pointer;"
                :style="u.isActive ? 'color:var(--t-gold)' : 'color:#34d399'"
                :title="u.isActive ? 'Deactivate' : 'Activate'"
              >
                <component :is="u.isActive ? UserX : UserCheck" class="w-3.5 h-3.5" />
              </button>
              <button @click="openDevices(u)" class="btn-ghost" style="padding:0.35rem 0.6rem;cursor:pointer;" title="Active sessions">
                <Monitor class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        </div>

        <div v-else class="ag-empty">
          <div class="ag-empty-icon">👥</div>
          <p>No users match your filters.</p>
        </div>
      </div>

      <!-- List View -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Name</th>
              <th>Username / Phone</th>
              <th>Role</th>
              <th>Institute</th>
              <th>Status</th>
              <th>Coins</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="8" style="text-align:center;padding:3rem 1rem;color:var(--t-text3);">Loading…</td>
            </tr>
            <tr v-else-if="!users.length">
              <td colspan="8" style="text-align:center;padding:3rem 1rem;color:var(--t-text3);">No users match your filters.</td>
            </tr>
            <tr v-for="(u, i) in users" :key="u.id">
              <td class="ag-table-muted">{{ (page - 1) * pageSize + i + 1 }}</td>
              <td>
                <div style="display:flex;align-items:center;gap:0.6rem;">
                  <div class="ag-avatar" :style="avatarGradient(u.name || u.username)">{{ initials(u.name || u.username) }}</div>
                  <span style="font-weight:500;color:var(--t-text1);">{{ u.name || '—' }}</span>
                </div>
              </td>
              <td class="ag-table-muted">{{ u.phone || u.username }}</td>
              <td><span :class="roleClass(u.role)">{{ u.role }}</span></td>
              <td class="ag-table-muted">{{ u.instituteName || '—' }}</td>
              <td><span :class="u.isActive ? 'badge-green' : 'badge-red'">{{ u.isActive ? 'Active' : 'Inactive' }}</span></td>
              <td style="color:var(--t-gold);font-weight:600;font-size:0.82rem;">{{ u.coins }} 🪙</td>
              <td>
                <div style="display:flex;gap:0.25rem;">
                  <button
                    @click="toggleActive(u)"
                    class="btn-ghost"
                    style="padding:0.3rem 0.5rem;cursor:pointer;"
                    :style="u.isActive ? 'color:var(--t-gold)' : 'color:#34d399'"
                    :title="u.isActive ? 'Deactivate' : 'Activate'"
                  >
                    <component :is="u.isActive ? UserX : UserCheck" class="w-3.5 h-3.5" />
                  </button>
                  <button @click="openDevices(u)" class="btn-ghost" style="padding:0.3rem 0.5rem;cursor:pointer;" title="Active sessions">
                    <Monitor class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="total > pageSize" style="display:flex;align-items:center;justify-content:center;gap:0.75rem;padding:1rem;border-top:1px solid var(--t-border);">
        <button class="btn-ghost" :disabled="page <= 1" @click="goPage(page - 1)" style="cursor:pointer;">← Prev</button>
        <span style="font-size:0.82rem;color:var(--t-text2);">Page {{ page }} of {{ totalPages }}</span>
        <button class="btn-ghost" :disabled="page >= totalPages" @click="goPage(page + 1)" style="cursor:pointer;">Next →</button>
      </div>
    </div>

    <!-- Devices / Sessions Panel -->
    <div v-if="devicesPanel" class="qb-overlay" @click.self="devicesPanel = false">
      <div class="qb-modal">
        <div class="qb-modal-header">
          <div>
            <h3 class="qb-modal-title">Active Sessions</h3>
            <div style="font-size:0.78rem;color:var(--t-text3);margin-top:0.2rem;">
              {{ devicesUser?.name || devicesUser?.username }}
            </div>
          </div>
          <div style="display:flex;align-items:center;gap:0.5rem;">
            <button v-if="sessions.length" @click="revokeAllSessions"
                    class="btn-secondary"
                    style="font-size:0.78rem;padding:0.35rem 0.75rem;color:#ef4444;border-color:#fca5a5;cursor:pointer;">
              Revoke All
            </button>
            <button @click="devicesPanel = false" class="btn-ghost" style="padding:0.4rem;cursor:pointer;">
              <X class="w-4 h-4" />
            </button>
          </div>
        </div>
        <div class="qb-modal-body">
          <div v-if="sessionsLoading" class="ag-empty">
            <div class="ag-empty-icon">⏳</div><p>Loading sessions…</p>
          </div>
          <div v-else-if="sessionsErr" class="qb-err">{{ sessionsErr }}</div>
          <div v-else-if="!sessions.length" class="ag-empty">
            <div class="ag-empty-icon">✅</div><p>No active sessions found.</p>
          </div>
          <div v-else style="display:flex;flex-direction:column;gap:0.75rem;">
            <div v-for="s in sessions" :key="s.id"
                 style="display:flex;align-items:flex-start;gap:0.75rem;padding:0.75rem;border-radius:12px;background:var(--t-bg);border:1px solid var(--t-border);">
              <div style="font-size:1.5rem;line-height:1;margin-top:0.1rem;">{{ deviceEmoji(s) }}</div>
              <div style="flex:1;min-width:0;">
                <div style="font-weight:600;font-size:0.88rem;color:var(--t-text1);">
                  {{ s.deviceName || 'Unknown device' }}
                </div>
                <div style="font-size:0.75rem;color:var(--t-text3);margin-top:0.2rem;">
                  {{ s.ipAddress || 'IP unknown' }} · signed in {{ relativeDate(s.createdAt) }}
                </div>
                <div style="font-size:0.72rem;color:var(--t-text3);">
                  Expires {{ new Date(s.expiresAt).toLocaleDateString(undefined, { timeZone: 'Asia/Karachi' }) }}
                </div>
              </div>
              <button @click="revokeSession(s.id)" class="btn-ghost"
                      style="padding:0.3rem 0.65rem;font-size:0.75rem;color:#ef4444;flex-shrink:0;cursor:pointer;">
                Revoke
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add User Modal -->
    <Dialog v-model:visible="addModal" modal header="Add New User" :style="{ width: '440px', maxWidth: '95vw' }">
      <div style="display:flex;flex-direction:column;gap:1rem;padding: 0.25rem 0;">
        <div>
          <label class="label">Username <span style="color:var(--t-error);">*</span></label>
          <input v-model="newUser.username" type="text" class="input" placeholder="e.g. ahmed.khan" />
        </div>
        <div>
          <label class="label">Full Name</label>
          <input v-model="newUser.name" type="text" class="input" placeholder="e.g. Ahmed Khan" />
        </div>
        <div>
          <label class="label">Phone Number</label>
          <input v-model="newUser.phone" type="tel" class="input" placeholder="03XX-XXXXXXX" />
        </div>
        <div>
          <label class="label">Password <span style="color:var(--t-error);">*</span></label>
          <input v-model="newUser.password" type="password" class="input" placeholder="Set initial password" />
        </div>
        <div>
          <label class="label">Role</label>
          <select v-model="newUser.roleName" class="input">
            <option value="student">Student</option>
            <option value="teacher">Teacher</option>
            <option value="parent">Parent</option>
            <option value="admin">Admin</option>
          </select>
        </div>
        <div>
          <label class="label">Institute</label>
          <select v-model="newUser.instituteId" class="input">
            <option :value="null">— None —</option>
            <option v-for="inst in institutes" :key="inst.id" :value="inst.id">{{ inst.name }}</option>
          </select>
        </div>
        <div>
          <label class="label">Grant Coins</label>
          <input v-model.number="newUser.coins" type="number" min="0" class="input" placeholder="0" />
        </div>
        <div v-if="addErr" style="color:var(--t-error);font-size:0.82rem;">{{ addErr }}</div>
      </div>
      <template #footer>
        <div style="display:flex;gap:0.5rem;justify-content:flex-end;">
          <button class="btn-secondary" @click="addModal = false" style="cursor:pointer;">Cancel</button>
          <button class="btn-primary" @click="submitAddUser" :disabled="addSaving" style="cursor:pointer;">
            <Plus class="w-4 h-4" /> {{ addSaving ? 'Adding…' : 'Add User' }}
          </button>
        </div>
      </template>
    </Dialog>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Plus, UserX, UserCheck, LayoutGrid, List, Search, Shield, Monitor, X } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import KpiCard from '@/components/ui/KpiCard.vue'
import { api } from '@/services/api'

/* ── State ── */
const devicesPanel    = ref(false)
const devicesUser     = ref(null)
const sessions        = ref([])
const sessionsLoading = ref(false)
const sessionsErr     = ref('')

const view          = ref('grid')
const activeTab     = ref('All')
const search        = ref('')
const filterInstitute = ref('')
const addModal      = ref(false)
const addSaving     = ref(false)
const addErr        = ref('')
const loading       = ref(false)
const users         = ref([])
const institutes    = ref([])
const total         = ref(0)
const page          = ref(1)
const pageSize      = 20
const counts        = ref({ student: 0, teacher: 0, parent: 0, admin: 0 })

const tabs = ['All', 'Students', 'Teachers', 'Parents', 'Admins']
const tabMap = { Students: 'student', Teachers: 'teacher', Parents: 'parent', Admins: 'admin' }

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))

const newUser = ref({ username: '', name: '', phone: '', password: '', roleName: 'student', instituteId: null, coins: 0 })

/* ── API Calls ── */
async function fetchUsers() {
  loading.value = true
  try {
    const params = {
      page: page.value,
      pageSize,
    }
    if (activeTab.value !== 'All')   params.role = tabMap[activeTab.value]
    if (search.value.trim())         params.search = search.value.trim()
    if (filterInstitute.value)       params.instituteId = filterInstitute.value
    const res = await api.get('/admin/users', { params })
    users.value = res.data.items
    total.value = res.data.total
  } finally {
    loading.value = false
  }
}

async function fetchCounts() {
  const roles = ['student', 'teacher', 'parent', 'admin']
  const results = await Promise.allSettled(
    roles.map(r => api.get('/admin/users', { params: { role: r, pageSize: 1 } }))
  )
  results.forEach((r, i) => {
    if (r.status === 'fulfilled') counts.value[roles[i]] = r.value.data.total
  })
}

async function fetchInstitutes() {
  try {
    const res = await api.get('/admin/institutes')
    institutes.value = res.data
  } catch { /* non-critical */ }
}

onMounted(() => {
  fetchUsers()
  fetchCounts()
  fetchInstitutes()
})

function setTab(tab) {
  activeTab.value = tab
  page.value = 1
  fetchUsers()
}

function goPage(p) {
  page.value = p
  fetchUsers()
}

let debounceTimer = null
function debounceFetch() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => { page.value = 1; fetchUsers() }, 350)
}

/* ── Actions ── */
async function toggleActive(u) {
  try {
    const res = await api.patch(`/admin/users/${u.id}/toggle`)
    u.isActive = res.data.isActive
    fetchCounts()
  } catch { /* ignore */ }
}

function openAddModal() {
  addErr.value  = ''
  addModal.value = true
  newUser.value  = { username: '', name: '', phone: '', password: '', roleName: 'student', instituteId: null, coins: 0 }
}

async function submitAddUser() {
  addErr.value = ''
  if (!newUser.value.username.trim()) { addErr.value = 'Username is required.'; return }
  if (!newUser.value.password)        { addErr.value = 'Password is required.';  return }
  addSaving.value = true
  try {
    await api.post('/admin/users', newUser.value)
    addModal.value = false
    await fetchUsers()
    await fetchCounts()
  } catch (e) {
    addErr.value = e.data?.message || 'Failed to create user.'
  } finally {
    addSaving.value = false
  }
}

/* ── Session Management ── */
async function openDevices(u) {
  devicesUser.value     = u
  devicesPanel.value    = true
  sessionsErr.value     = ''
  sessions.value        = []
  sessionsLoading.value = true
  try {
    const res = await api.get(`/admin/users/${u.id}/sessions`)
    sessions.value = res.data
  } catch { sessionsErr.value = 'Failed to load sessions.' }
  finally  { sessionsLoading.value = false }
}

async function revokeSession(tokenId) {
  try {
    await api.delete(`/admin/users/${devicesUser.value.id}/sessions/${tokenId}`)
    sessions.value = sessions.value.filter(s => s.id !== tokenId)
  } catch { sessionsErr.value = 'Failed to revoke session.' }
}

async function revokeAllSessions() {
  try {
    await api.delete(`/admin/users/${devicesUser.value.id}/sessions`)
    sessions.value = []
  } catch { sessionsErr.value = 'Failed to revoke all sessions.' }
}

function deviceEmoji(s) {
  const txt = ((s.deviceName || '') + (s.userAgent || '')).toLowerCase()
  return (txt.includes('android') || txt.includes('iphone') || txt.includes('ipad')) ? '📱' : '💻'
}

function relativeDate(iso) {
  const mins = Math.floor((Date.now() - new Date(iso)) / 60000)
  if (mins < 1)  return 'just now'
  if (mins < 60) return `${mins}m ago`
  const hrs = Math.floor(mins / 60)
  if (hrs < 24)  return `${hrs}h ago`
  return `${Math.floor(hrs / 24)}d ago`
}

/* ── Helpers ── */
function initials(name) {
  if (!name) return '?'
  return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase()
}

const gradients = [
  'linear-gradient(135deg,#7c6af5,#5b43cc)',
  'linear-gradient(135deg,#10b981,#0891b2)',
  'linear-gradient(135deg,#f59e0b,#ea580c)',
  'linear-gradient(135deg,#e11d48,#db2777)',
  'linear-gradient(135deg,#0891b2,#6366f1)',
]
function avatarGradient(name) {
  const idx = (name?.charCodeAt(0) ?? 0) % gradients.length
  return `background: ${gradients[idx]};`
}

function roleClass(r) {
  return { student: 'badge-blue', teacher: 'badge-green', admin: 'badge-purple', parent: 'badge-amber' }[r] ?? 'badge-indigo'
}
</script>

<style scoped>
.users-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}
@media (min-width: 640px)  { .users-grid { grid-template-columns: repeat(3, 1fr); } }
@media (min-width: 1024px) { .users-grid { grid-template-columns: repeat(4, 1fr); } }

.user-card { display: flex; flex-direction: column; }

.user-avatar {
  width: 44px; height: 44px; border-radius: 14px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  font-weight: 700; font-size: 1rem; color: #fff;
}

.qb-overlay { position:fixed; inset:0; z-index:1000; background:rgba(0,0,0,0.45); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; padding:1.25rem; }
.qb-modal { background:var(--t-bg2); border:1px solid var(--t-border); border-radius:16px; width:100%; max-width:520px; max-height:calc(100vh - 2.5rem); overflow-y:auto; box-shadow:0 20px 60px rgba(0,0,0,0.3); }
.qb-modal-header { display:flex; align-items:center; justify-content:space-between; padding:1.25rem 1.5rem 0; gap:0.5rem; }
.qb-modal-title { font-size:1rem; font-weight:700; color:var(--t-text1); margin:0; }
.qb-modal-body { padding:1.25rem 1.5rem 1.5rem; display:flex; flex-direction:column; gap:1rem; }
.qb-err { padding:10px 14px; border-radius:9px; font-size:0.83rem; background:#fef2f2; border:1px solid #fecaca; color:#b91c1c; margin:0; }
</style>
