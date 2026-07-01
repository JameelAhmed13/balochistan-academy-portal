<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🛡️</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Role Management</div>
        <div class="ag-banner-sub">Define roles and assign permissions. The permission list is seed-managed.</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat"><span>🎭</span> {{ roles.length }} Roles</span>
        <span class="ag-banner-stat"><span>🔑</span> {{ allPermissions.length }} Permissions</span>
      </div>
      <button class="btn-primary text-sm" @click="openCreate">
        <Plus class="w-4 h-4" /> Add Role
      </button>
    </div>

    <!-- Main card -->
    <div class="ag-card">
      <!-- Filter bar -->
      <div class="ag-filter-bar">
        <div class="ag-filter-group" style="flex:1; min-width:220px;">
          <label class="ag-filter-label">Search</label>
          <div style="position:relative;">
            <Search class="w-3.5 h-3.5" style="position:absolute; left:0.6rem; top:50%; transform:translateY(-50%); color:var(--t-text3); pointer-events:none;" />
            <input v-model="search" type="text" class="input" style="padding-left:2rem;" placeholder="Search roles…" />
          </div>
        </div>
      </div>

      <!-- Count row -->
      <div style="padding:0.5rem 1.5rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; gap:0.5rem;">
        <Filter class="w-3 h-3" style="color:var(--t-text3);" />
        <span style="font-size:0.75rem; color:var(--t-text3);">
          Showing <strong style="color:var(--t-text1);">{{ filtered.length }}</strong>
          of <strong style="color:var(--t-text1);">{{ roles.length }}</strong> roles
        </span>
      </div>

      <!-- Table -->
      <div class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:48px;">#</th>
              <th>Role Name</th>
              <th>Description</th>
              <th style="width:80px; text-align:center;">Users</th>
              <th style="width:160px;">Permissions</th>
              <th style="width:80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="6">
                <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading roles…</p></div>
              </td>
            </tr>
            <tr v-for="(role, i) in filtered" :key="role.id">
              <td class="ag-table-muted" style="text-align:center;">{{ i + 1 }}</td>
              <td>
                <span style="font-weight:600; font-size:0.85rem; text-transform:capitalize;">{{ role.name }}</span>
              </td>
              <td class="ag-table-muted">{{ role.description || '—' }}</td>
              <td class="ag-table-muted" style="text-align:center;">{{ role.userCount }}</td>
              <td>
                <button class="btn-ghost" style="padding:0.25rem 0.5rem; font-size:0.8rem; gap:0.3rem;" @click="openPermissions(role)">
                  <span class="badge-blue" style="font-size:0.7rem; padding:0.1rem 0.4rem;">{{ role.permissionIds.length }}/{{ allPermissions.length }}</span>
                  Manage
                </button>
              </td>
              <td>
                <div class="flex gap-1">
                  <button @click="openEdit(role)" class="btn-ghost" style="padding:0.3rem;" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button @click="removeRole(role)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="!loading && !filtered.length">
              <td colspan="6">
                <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No roles match your search.</p></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create / Edit Role Modal -->
    <div v-if="roleForm" class="qb-overlay" @click.self="roleForm = null">
      <div class="qb-modal">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">{{ editingRole ? 'Edit Role' : 'Add Role' }}</h3>
          <button @click="roleForm = null" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>
        <form class="qb-modal-body" @submit.prevent="saveRole">
          <div>
            <label class="label">Role Name <span style="color:#ef4444;">*</span></label>
            <input v-model="roleForm.name" class="input" placeholder="e.g. moderator" />
            <p style="font-size:0.75rem; color:var(--t-text3); margin:0.25rem 0 0;">Lowercase; used as an identifier.</p>
          </div>
          <div>
            <label class="label">Description</label>
            <textarea v-model="roleForm.description" rows="2" class="input" style="resize:vertical;" placeholder="What can this role do?" />
          </div>
          <p v-if="roleErr" class="qb-err">{{ roleErr }}</p>
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button type="submit" class="btn-primary" style="flex:1;" :disabled="roleSaving">
              <span v-if="roleSaving" class="qb-spinner"></span>
              <Plus v-else class="w-4 h-4" />
              {{ roleSaving ? 'Saving…' : (editingRole ? 'Save Changes' : 'Create Role') }}
            </button>
            <button type="button" @click="roleForm = null" class="btn-secondary" :disabled="roleSaving">Cancel</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Permission Assignment Modal -->
    <div v-if="permPanel" class="qb-overlay" @click.self="permPanel = null">
      <div class="qb-modal" style="max-width:580px;">
        <div class="qb-modal-header">
          <div>
            <h3 class="qb-modal-title">{{ permPanel.name }} — Permissions</h3>
            <div style="display:flex; align-items:center; gap:0.5rem; margin-top:0.35rem;">
              <span class="perm-seed-badge">🔒 Seed-managed list</span>
            </div>
          </div>
          <button @click="permPanel = null" class="btn-ghost" style="padding:0.4rem; align-self:flex-start;"><X class="w-4 h-4" /></button>
        </div>
        <div class="qb-modal-body">
          <p style="margin:0; font-size:0.82rem; color:var(--t-text3);">
            The permission list is defined in the system seed and cannot be modified here. You can only assign or revoke permissions for this role.
          </p>

          <!-- Permissions grouped by module -->
          <div v-for="(group, module) in groupedPermissions" :key="module" class="perm-group">
            <div class="perm-module-label">{{ module }}</div>
            <label v-for="p in group" :key="p.id" class="perm-row">
              <input type="checkbox" :value="p.id" v-model="selectedPermIds" style="margin-top:0.1rem; flex-shrink:0;" />
              <div>
                <span style="display:block; font-size:0.82rem; font-weight:600; color:var(--t-text1); font-family:monospace;">{{ p.name }}</span>
                <span style="display:block; font-size:0.75rem; color:var(--t-text3);">{{ p.description }}</span>
              </div>
            </label>
          </div>

          <p v-if="permErr" class="qb-err">{{ permErr }}</p>
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button class="btn-primary" style="flex:1;" :disabled="permSaving" @click="savePermissions">
              <span v-if="permSaving" class="qb-spinner"></span>
              {{ permSaving ? 'Saving…' : 'Save Permissions' }}
            </button>
            <button class="btn-secondary" @click="permPanel = null" :disabled="permSaving">Cancel</button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Plus, Search, Filter, Pencil, Trash2, X } from '@lucide/vue'
import { api } from '@/services/api'

const roles          = ref([])
const allPermissions = ref([])
const loading        = ref(false)

const roleForm    = ref(null)
const editingRole = ref(false)
const roleSaving  = ref(false)
const roleErr     = ref('')

const permPanel      = ref(null)
const selectedPermIds = ref([])
const permSaving     = ref(false)
const permErr        = ref('')

const search = ref('')

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return roles.value
  return roles.value.filter(r =>
    r.name.toLowerCase().includes(q) ||
    (r.description && r.description.toLowerCase().includes(q))
  )
})

const groupedPermissions = computed(() => {
  return allPermissions.value.reduce((acc, p) => {
    if (!acc[p.module]) acc[p.module] = []
    acc[p.module].push(p)
    return acc
  }, {})
})

async function fetchAll() {
  loading.value = true
  try {
    const [rolesRes, permsRes] = await Promise.all([
      api.get('/admin/roles'),
      api.get('/admin/permissions'),
    ])
    roles.value          = rolesRes.data
    allPermissions.value = permsRes.data
  } finally {
    loading.value = false
  }
}

onMounted(fetchAll)

function openCreate() {
  editingRole.value = false
  roleErr.value     = ''
  roleForm.value    = { name: '', description: '' }
}

function openEdit(role) {
  editingRole.value = true
  roleErr.value     = ''
  roleForm.value    = { id: role.id, name: role.name, description: role.description || '' }
}

async function saveRole() {
  roleErr.value = ''
  if (!roleForm.value.name.trim()) { roleErr.value = 'Role name is required.'; return }
  roleSaving.value = true
  try {
    if (editingRole.value) {
      await api.put(`/admin/roles/${roleForm.value.id}`, roleForm.value)
    } else {
      await api.post('/admin/roles', roleForm.value)
    }
    roleForm.value = null
    await fetchAll()
  } catch (e) {
    roleErr.value = e.data?.message || e.message || 'Failed to save role.'
  } finally {
    roleSaving.value = false
  }
}

async function removeRole(role) {
  if (!confirm(`Delete role "${role.name}"?`)) return
  try {
    await api.delete(`/admin/roles/${role.id}`)
    roles.value = roles.value.filter(r => r.id !== role.id)
  } catch (e) {
    alert(e.data?.message || 'Cannot delete this role.')
  }
}

function openPermissions(role) {
  permErr.value        = ''
  permPanel.value      = role
  selectedPermIds.value = [...role.permissionIds]
}

async function savePermissions() {
  permErr.value    = ''
  permSaving.value = true
  try {
    await api.put(`/admin/roles/${permPanel.value.id}/permissions`, selectedPermIds.value)
    const target = roles.value.find(r => r.id === permPanel.value.id)
    if (target) target.permissionIds = [...selectedPermIds.value]
    permPanel.value = null
  } catch (e) {
    permErr.value = e.data?.message || 'Failed to save permissions.'
  } finally {
    permSaving.value = false
  }
}
</script>

<style scoped>
.qb-overlay { position:fixed; inset:0; z-index:1000; background:rgba(0,0,0,0.45); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; padding:1.25rem; }
.qb-modal { background:var(--t-bg2); border:1px solid var(--t-border); border-radius:16px; width:100%; max-width:520px; max-height:calc(100vh - 2.5rem); overflow-y:auto; box-shadow:0 20px 60px rgba(0,0,0,0.3); }
.qb-modal-header { display:flex; align-items:center; justify-content:space-between; padding:1.25rem 1.5rem 0; gap:0.5rem; }
.qb-modal-title { font-size:1rem; font-weight:700; color:var(--t-text1); margin:0; }
.qb-modal-body { padding:1.25rem 1.5rem 1.5rem; display:flex; flex-direction:column; gap:1rem; }
.qb-spinner { display:inline-block; width:12px; height:12px; border:2px solid rgba(255,255,255,0.4); border-top-color:white; border-radius:50%; animation:qbspin 0.7s linear infinite; }
@keyframes qbspin { to { transform:rotate(360deg); } }
.qb-err { margin:0; padding:10px 14px; border-radius:9px; font-size:0.83rem; background:#fef2f2; border:1px solid #fecaca; color:#b91c1c; }
.perm-seed-badge { font-size:0.72rem; font-weight:600; color:var(--t-text3); background:var(--t-surface2,var(--t-hover2)); border:1px solid var(--t-border); border-radius:6px; padding:0.2rem 0.5rem; white-space:nowrap; }
.perm-group { margin-bottom:0.5rem; }
.perm-module-label { font-size:0.72rem; font-weight:700; text-transform:uppercase; letter-spacing:0.06em; color:var(--t-text3); margin-bottom:0.4rem; padding-bottom:0.25rem; border-bottom:1px solid var(--t-border); }
.perm-row { display:flex; align-items:flex-start; gap:0.6rem; padding:0.35rem 0; cursor:pointer; }
</style>
