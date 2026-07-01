<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>Role Management</h1>
        <p>Define roles and assign permissions. Permissions are seed-managed — their list cannot be changed here.</p>
      </div>
      <button class="adm-btn" @click="openCreate">+ Add Role</button>
    </header>

    <div v-if="loading" class="adm-muted">Loading…</div>

    <!-- Roles table -->
    <table v-else class="adm-table">
      <thead>
        <tr>
          <th>#</th>
          <th>Role Name</th>
          <th>Description</th>
          <th>Users</th>
          <th>Permissions</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(role, i) in roles" :key="role.id">
          <td class="adm-muted">{{ i + 1 }}</td>
          <td>
            <span class="adm-role-name">{{ role.name }}</span>
          </td>
          <td class="adm-muted">{{ role.description || '—' }}</td>
          <td class="adm-muted">{{ role.userCount }}</td>
          <td>
            <button class="adm-link" @click="openPermissions(role)">
              {{ role.permissionIds.length }}/{{ allPermissions.length }} assigned · manage
            </button>
          </td>
          <td class="adm-actions">
            <button class="adm-link" @click="openEdit(role)">Edit</button>
            <button class="adm-link danger" @click="removeRole(role)">Delete</button>
          </td>
        </tr>
        <tr v-if="!roles.length">
          <td colspan="6" class="adm-muted">No roles yet.</td>
        </tr>
      </tbody>
    </table>

    <!-- Create / Edit Role Modal -->
    <div v-if="roleForm" class="adm-modal" @click.self="roleForm = null">
      <div class="adm-dialog">
        <h2>{{ editingRole ? 'Edit Role' : 'Add Role' }}</h2>

        <label>
          Role Name <span class="adm-req">*</span>
          <input v-model="roleForm.name" placeholder="e.g. moderator" />
          <small class="adm-hint">Lowercase; used as an identifier.</small>
        </label>

        <label>
          Description
          <textarea v-model="roleForm.description" rows="2" placeholder="What can this role do?" />
        </label>

        <div v-if="roleErr" class="adm-err">{{ roleErr }}</div>

        <div class="adm-dialog-foot">
          <button class="adm-btn-ghost" @click="roleForm = null">Cancel</button>
          <button class="adm-btn" :disabled="roleSaving" @click="saveRole">
            {{ roleSaving ? 'Saving…' : (editingRole ? 'Update' : 'Create') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Permission Assignment Panel -->
    <div v-if="permPanel" class="adm-modal" @click.self="permPanel = null">
      <div class="adm-dialog" style="max-width:520px;">
        <div class="adm-perm-head">
          <h2>{{ permPanel.name }} — Permissions</h2>
          <span class="adm-seed-badge">🔒 Seed-managed list</span>
        </div>
        <p class="adm-hint" style="margin-bottom:1rem;">
          The permission list is defined in the system seed and cannot be modified here. You can only assign or revoke permissions for this role.
        </p>

        <!-- Permissions grouped by module -->
        <div v-for="(group, module) in groupedPermissions" :key="module" class="adm-perm-group">
          <div class="adm-perm-module">{{ module }}</div>
          <label v-for="p in group" :key="p.id" class="adm-perm-row">
            <input type="checkbox" :value="p.id" v-model="selectedPermIds" />
            <div>
              <span class="adm-perm-name">{{ p.name }}</span>
              <span class="adm-perm-desc">{{ p.description }}</span>
            </div>
          </label>
        </div>

        <div v-if="permErr" class="adm-err">{{ permErr }}</div>

        <div class="adm-dialog-foot">
          <button class="adm-btn-ghost" @click="permPanel = null">Cancel</button>
          <button class="adm-btn" :disabled="permSaving" @click="savePermissions">
            {{ permSaving ? 'Saving…' : 'Save Permissions' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { api } from '@/services/api'

const roles          = ref([])
const allPermissions = ref([])
const loading        = ref(false)

/* Role CRUD */
const roleForm    = ref(null)
const editingRole = ref(false)
const roleSaving  = ref(false)
const roleErr     = ref('')

/* Permission panel */
const permPanel      = ref(null)
const selectedPermIds = ref([])
const permSaving     = ref(false)
const permErr        = ref('')

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

/* Permissions grouped by module for the panel */
const groupedPermissions = computed(() => {
  return allPermissions.value.reduce((acc, p) => {
    if (!acc[p.module]) acc[p.module] = []
    acc[p.module].push(p)
    return acc
  }, {})
})

/* ── Role CRUD ── */
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

/* ── Permission Assignment ── */
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
    // Update local state
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
.adm { max-width: 1000px; margin: 0 auto; padding: 24px; color: var(--t-text1); }
.adm-head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; margin-bottom: 24px; }
.adm-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.9rem; margin: 0 0 4px; }
.adm-head p { margin: 0; color: var(--t-text2); font-size: .9rem; }
.adm-muted { color: var(--t-text2); padding: 12px 0; }
.adm-hint { color: var(--t-text3); font-size: .78rem; margin: 0; font-weight: 400; }
.adm-btn { padding: 10px 18px; border: 0; border-radius: 11px; cursor: pointer; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.adm-btn:disabled { opacity: .6; }
.adm-btn-ghost { padding: 10px 18px; border-radius: 11px; cursor: pointer; font-weight: 600; color: var(--t-text1); background: var(--t-hover); border: 1px solid var(--t-border); }
.adm-table { width: 100%; border-collapse: collapse; }
.adm-table th { text-align: left; font-size: .72rem; letter-spacing: .08em; text-transform: uppercase; color: var(--t-text3); padding: 10px 12px; border-bottom: 1px solid var(--t-border); }
.adm-table td { padding: 12px; border-bottom: 1px solid var(--t-border); font-size: .92rem; }
.adm-actions { display: flex; gap: 12px; }
.adm-link { background: none; border: 0; cursor: pointer; color: var(--t-accent); font-weight: 600; font-size: .88rem; padding: 0; }
.adm-link.danger { color: #f87171; }
.adm-req { color: #f87171; }
.adm-modal { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; background: rgba(0,0,0,.5); padding: 20px; overflow-y: auto; }
.adm-dialog { width: min(520px, 100%); padding: 28px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 14px; max-height: 90vh; overflow-y: auto; }
.adm-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.3rem; }
.adm-dialog label { display: flex; flex-direction: column; gap: 5px; font-size: .82rem; font-weight: 600; color: var(--t-text2); }
.adm-dialog input, .adm-dialog textarea { padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); font-family: inherit; resize: vertical; }
.adm-dialog-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; }
.adm-err { color: #f87171; font-size: .85rem; }
.adm-role-name {
  font-weight: 600;
  font-size: 0.85rem;
  color: var(--t-text1);
  text-transform: capitalize;
}
.adm-perm-head {
  display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.25rem; flex-wrap: wrap;
}
.adm-seed-badge {
  font-size: 0.72rem; font-weight: 600; color: var(--t-text3);
  background: var(--t-surface2); border: 1px solid var(--t-border);
  border-radius: 6px; padding: 0.2rem 0.5rem; white-space: nowrap;
}
.adm-perm-group { margin-bottom: 1rem; }
.adm-perm-module {
  font-size: 0.72rem; font-weight: 700; text-transform: uppercase;
  letter-spacing: 0.06em; color: var(--t-text3); margin-bottom: 0.4rem;
  padding-bottom: 0.25rem; border-bottom: 1px solid var(--t-border);
}
.adm-perm-row {
  display: flex; align-items: flex-start; gap: 0.6rem;
  padding: 0.35rem 0; cursor: pointer;
}
.adm-perm-row input[type="checkbox"] { margin-top: 0.2rem; flex-shrink: 0; }
.adm-perm-name { display: block; font-size: 0.82rem; font-weight: 600; color: var(--t-text1); font-family: monospace; }
.adm-perm-desc { display: block; font-size: 0.75rem; color: var(--t-text3); }
</style>
