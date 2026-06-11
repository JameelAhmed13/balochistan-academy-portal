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
        <span class="ag-banner-stat">👨‍🎓 42 Students</span>
        <span class="ag-banner-stat">👨‍🏫 4 Teachers</span>
        <span class="ag-banner-stat">🛡️ 2 Admins</span>
      </div>
      <button class="btn-primary text-sm" @click="addModal = true">
        <Plus class="w-4 h-4" /> Add User
      </button>
    </div>

    <!-- KPI Row -->
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
      <KpiCard label="Students" :value="42" icon="👨‍🎓" color="blue" />
      <KpiCard label="Teachers" :value="4"  icon="👨‍🏫" color="green" />
      <KpiCard label="Parents"  :value="12" icon="👨‍👩‍👧" color="amber" />
      <KpiCard label="Admins"   :value="2"  icon="🛡️" color="purple" />
    </div>

    <!-- Controls Card -->
    <div class="ag-card">
      <!-- Tabs -->
      <div class="ag-tabs">
        <button
          v-for="tab in tabs" :key="tab"
          :class="['ag-tab', { active: activeTab === tab }]"
          @click="activeTab = tab"
          style="cursor: pointer"
        >{{ tab }}</button>
      </div>

      <!-- Filter Bar -->
      <div class="ag-filter-bar">
        <div class="ag-filter-group flex-1" style="min-width: 200px;">
          <label class="ag-filter-label">Search</label>
          <div style="position: relative;">
            <Search class="w-3.5 h-3.5" style="position:absolute;left:0.6rem;top:50%;transform:translateY(-50%);color:var(--t-text3);pointer-events:none;" />
            <input v-model="search" type="text" class="input" placeholder="Search name or phone…" style="padding-left: 2rem;" />
          </div>
        </div>
        <div class="ag-filter-group">
          <label class="ag-filter-label">Institute</label>
          <select v-model="filterInstitute" class="input" style="min-width: 160px;">
            <option value="">All Institutes</option>
            <option>New Century</option>
            <option>City College</option>
            <option>Platform</option>
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
        <div v-if="filteredUsers.length" class="users-grid">
          <div
            v-for="u in filteredUsers" :key="u.id"
            class="ag-grid-card user-card"
            style="cursor: default;"
          >
            <!-- Avatar -->
            <div style="display:flex;align-items:center;gap:0.75rem;margin-bottom:0.875rem;">
              <div class="user-avatar" :style="avatarGradient(u.name)">{{ initials(u.name) }}</div>
              <div style="min-width:0;flex:1;">
                <div style="font-weight:600;font-size:0.9rem;color:var(--t-text1);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">{{ u.name }}</div>
                <div style="font-size:0.75rem;color:var(--t-text3);">{{ u.phone }}</div>
              </div>
            </div>
            <!-- Role + Status -->
            <div style="display:flex;align-items:center;gap:0.5rem;flex-wrap:wrap;margin-bottom:0.75rem;">
              <span :class="roleClass(u.role)">{{ u.role }}</span>
              <span :class="u.active ? 'badge-green' : 'badge-red'">{{ u.active ? 'Active' : 'Inactive' }}</span>
            </div>
            <!-- Institute -->
            <div style="font-size:0.75rem;color:var(--t-text2);display:flex;align-items:center;gap:0.35rem;margin-bottom:0.5rem;">
              <Shield class="w-3 h-3" style="flex-shrink:0;color:var(--t-text3);" />
              {{ u.institute }}
            </div>
            <!-- Coins -->
            <div style="font-size:0.75rem;color:var(--t-gold);font-weight:600;margin-bottom:0.875rem;">
              🪙 {{ u.coins }} coins
            </div>
            <!-- Actions -->
            <div style="display:flex;gap:0.4rem;padding-top:0.75rem;border-top:1px solid var(--t-border);">
              <button class="btn-ghost" style="padding:0.35rem 0.6rem;font-size:0.75rem;cursor:pointer;" title="Edit user">
                <Pencil class="w-3.5 h-3.5" />
              </button>
              <button
                @click="toggleActive(u)"
                class="btn-ghost"
                style="padding:0.35rem 0.6rem;font-size:0.75rem;cursor:pointer;"
                :style="u.active ? 'color:var(--t-gold)' : 'color:#34d399'"
                :title="u.active ? 'Deactivate' : 'Activate'"
              >
                <component :is="u.active ? UserX : UserCheck" class="w-3.5 h-3.5" />
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
              <th>Phone</th>
              <th>Role</th>
              <th>Institute</th>
              <th>Subscription</th>
              <th>Coins</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="!filteredUsers.length">
              <td colspan="8" style="text-align:center;padding:3rem 1rem;color:var(--t-text3);">No users match your filters.</td>
            </tr>
            <tr v-for="(u, i) in filteredUsers" :key="u.id">
              <td class="ag-table-muted">{{ i + 1 }}</td>
              <td>
                <div style="display:flex;align-items:center;gap:0.6rem;">
                  <div class="ag-avatar" :style="avatarGradient(u.name)">{{ initials(u.name) }}</div>
                  <span style="font-weight:500;color:var(--t-text1);">{{ u.name }}</span>
                </div>
              </td>
              <td class="ag-table-muted">{{ u.phone }}</td>
              <td><span :class="roleClass(u.role)">{{ u.role }}</span></td>
              <td class="ag-table-muted">{{ u.institute }}</td>
              <td><span :class="u.active ? 'badge-green' : 'badge-red'">{{ u.active ? 'Active' : 'Inactive' }}</span></td>
              <td style="color:var(--t-gold);font-weight:600;font-size:0.82rem;">{{ u.coins }} 🪙</td>
              <td>
                <div style="display:flex;gap:0.25rem;">
                  <button class="btn-ghost" style="padding:0.3rem 0.5rem;cursor:pointer;" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button
                    @click="toggleActive(u)"
                    class="btn-ghost"
                    style="padding:0.3rem 0.5rem;cursor:pointer;"
                    :style="u.active ? 'color:var(--t-gold)' : 'color:#34d399'"
                    :title="u.active ? 'Deactivate' : 'Activate'"
                  >
                    <component :is="u.active ? UserX : UserCheck" class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add User Modal -->
    <Dialog v-model:visible="addModal" modal header="Add New User" :style="{ width: '440px', maxWidth: '95vw' }">
      <div style="display:flex;flex-direction:column;gap:1rem;padding: 0.25rem 0;">
        <div>
          <label class="label">Full Name</label>
          <input v-model="newUser.name" type="text" class="input" placeholder="e.g. Ahmed Khan" />
        </div>
        <div>
          <label class="label">Phone Number</label>
          <input v-model="newUser.phone" type="tel" class="input" placeholder="03XX-XXXXXXX" />
        </div>
        <div>
          <label class="label">Password</label>
          <input v-model="newUser.password" type="password" class="input" placeholder="Set initial password" />
        </div>
        <div>
          <label class="label">Role</label>
          <select v-model="newUser.role" class="input">
            <option value="">Select role…</option>
            <option value="student">Student</option>
            <option value="teacher">Teacher</option>
            <option value="parent">Parent</option>
            <option value="admin">Admin</option>
          </select>
        </div>
        <div>
          <label class="label">Institute</label>
          <select v-model="newUser.institute" class="input">
            <option value="">Select institute…</option>
            <option>New Century</option>
            <option>City College</option>
            <option>Platform</option>
          </select>
        </div>
        <div>
          <label class="label">Grant Coins</label>
          <input v-model.number="newUser.coins" type="number" min="0" class="input" placeholder="0" />
        </div>
      </div>
      <template #footer>
        <div style="display:flex;gap:0.5rem;justify-content:flex-end;">
          <button class="btn-secondary" @click="addModal = false" style="cursor:pointer;">Cancel</button>
          <button class="btn-primary" @click="submitAddUser" style="cursor:pointer;">
            <Plus class="w-4 h-4" /> Add User
          </button>
        </div>
      </template>
    </Dialog>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { ArrowLeft, Plus, Pencil, UserX, UserCheck, LayoutGrid, List, Search, Shield } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import KpiCard from '@/components/ui/KpiCard.vue'

/* ── State ── */
const view        = ref('grid')
const activeTab   = ref('All')
const search      = ref('')
const filterInstitute = ref('')
const addModal    = ref(false)

const tabs = ['All', 'Students', 'Teachers', 'Parents', 'Admins']

const newUser = ref({ name: '', phone: '', password: '', role: '', institute: '', coins: 0 })

/* ── Mock Data ── */
const mockUsers = ref([
  { id: 1, name: 'Ahmed Khan',      phone: '03001234567', role: 'student', institute: 'New Century', active: true,  coins: 1250 },
  { id: 2, name: 'Fatima Ali',      phone: '03112345678', role: 'student', institute: 'New Century', active: true,  coins: 980  },
  { id: 3, name: 'Zainab Malik',    phone: '03221234567', role: 'student', institute: 'City College', active: true, coins: 850  },
  { id: 4, name: 'Bilal Hussain',   phone: '03331234567', role: 'student', institute: 'New Century', active: false, coins: 340  },
  { id: 5, name: 'Sir Raza',        phone: '03441234567', role: 'teacher', institute: 'New Century', active: true,  coins: 0    },
  { id: 6, name: 'Mrs. Nadia',      phone: '03551234567', role: 'teacher', institute: 'City College', active: true, coins: 0    },
  { id: 7, name: 'Parent of Ahmed', phone: '03661234567', role: 'parent',  institute: 'New Century', active: true,  coins: 0    },
  { id: 8, name: 'Admin User',      phone: '03771234567', role: 'admin',   institute: 'Platform',    active: true,  coins: 0    },
])

/* ── Computed ── */
const filteredUsers = computed(() => {
  const tabMap = { Students: 'student', Teachers: 'teacher', Parents: 'parent', Admins: 'admin' }
  return mockUsers.value.filter(u => {
    const matchTab  = activeTab.value === 'All' || u.role === tabMap[activeTab.value]
    const q         = search.value.trim().toLowerCase()
    const matchSearch = !q || u.name.toLowerCase().includes(q) || u.phone.includes(q)
    const matchInst = !filterInstitute.value || u.institute === filterInstitute.value
    return matchTab && matchSearch && matchInst
  })
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
function avatarGradient(name) {
  const idx = name.charCodeAt(0) % gradients.length
  return `background: ${gradients[idx]};`
}

function roleClass(r) {
  return { student: 'badge-blue', teacher: 'badge-green', admin: 'badge-purple', parent: 'badge-amber' }[r] ?? 'badge-indigo'
}

function toggleActive(u) { u.active = !u.active }

function submitAddUser() {
  if (!newUser.value.name || !newUser.value.role) return
  mockUsers.value.push({
    id: Date.now(),
    ...newUser.value,
    active: true,
  })
  newUser.value = { name: '', phone: '', password: '', role: '', institute: '', coins: 0 }
  addModal.value = false
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
</style>
