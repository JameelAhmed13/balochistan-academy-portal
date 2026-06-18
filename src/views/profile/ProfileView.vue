<template>
  <div class="pf">
    <div class="pf-head">
      <div class="pf-avatar">{{ initials }}</div>
      <div class="pf-head-text">
        <h1 class="pf-title">{{ auth.user?.name || 'My Profile' }}</h1>
        <p class="pf-sub">{{ roleLabel }} · @{{ auth.user?.username }}</p>
      </div>
    </div>

    <form class="pf-stack" @submit.prevent="save">
      <!-- Personal information -->
      <section class="card pf-card">
        <h2 class="pf-card-title">Personal Information</h2>
        <div class="pf-fields">
          <div>
            <label class="label">Full Name</label>
            <input v-model.trim="form.name" class="input" type="text" required />
          </div>
          <div class="pf-row">
            <div>
              <label class="label">Phone</label>
              <input v-model.trim="form.phone" class="input" type="tel" placeholder="03xx-xxxxxxx" />
            </div>
            <div>
              <label class="label">Email</label>
              <input v-model.trim="form.email" class="input" type="email" placeholder="you@example.com" />
            </div>
          </div>
          <div class="pf-row">
            <div v-if="isStudent">
              <label class="label">Medium</label>
              <select v-model="form.medium" class="input">
                <option value="English">English</option>
                <option value="Urdu">Urdu</option>
              </select>
            </div>
            <div>
              <label class="label">{{ isStudent ? 'City / Institute' : 'Institute' }}</label>
              <input v-model.trim="form.institute" class="input" type="text" placeholder="e.g. Quetta" />
            </div>
          </div>
          <div class="pf-row">
            <div>
              <label class="label">Username</label>
              <input :value="auth.user?.username" class="input" disabled />
            </div>
            <div v-if="isStudent">
              <label class="label">Grade</label>
              <input :value="auth.user?.gradeCode || '—'" class="input" disabled />
            </div>
          </div>
        </div>
      </section>

      <!-- Password -->
      <section class="card pf-card">
        <h2 class="pf-card-title">Change Password</h2>
        <p class="pf-hint">Leave these blank to keep your current password.</p>
        <div class="pf-fields">
          <div>
            <label class="label">Current Password</label>
            <input v-model="pw.current" class="input" type="password" autocomplete="current-password" />
          </div>
          <div class="pf-row">
            <div>
              <label class="label">New Password</label>
              <input v-model="pw.next" class="input" type="password" autocomplete="new-password" />
            </div>
            <div>
              <label class="label">Confirm New Password</label>
              <input v-model="pw.confirm" class="input" type="password" autocomplete="new-password" />
            </div>
          </div>
        </div>
      </section>

      <div class="pf-actions">
        <span v-if="error" class="pf-error">{{ error }}</span>
        <span v-else-if="saved" class="pf-ok">✓ Changes saved</span>
        <span class="pf-spacer" />
        <button type="submit" class="btn-primary" :disabled="saving">{{ saving ? 'Saving…' : 'Save Changes' }}</button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const toast = useToast()

const isStudent = computed(() => auth.user?.role === 'student')
const roleLabel = computed(() => ({ admin: 'Administrator', student: 'Student', teacher: 'Teacher', parent: 'Parent' }[auth.user?.role] || 'User'))
const initials = computed(() => (auth.user?.name || 'U').split(' ').map((w) => w[0]).slice(0, 2).join('').toUpperCase())

const form = reactive({
  name: auth.user?.name || '',
  phone: auth.user?.phone || '',
  email: auth.user?.email || '',
  medium: auth.user?.medium || 'English',
  institute: auth.user?.institute || '',
})
const pw = reactive({ current: '', next: '', confirm: '' })
const saving = ref(false)
const error = ref('')
const saved = ref(false)

async function save() {
  error.value = ''
  saved.value = false
  if (pw.current || pw.next || pw.confirm) {
    if (pw.next.length < 4) { error.value = 'New password must be at least 4 characters.'; return }
    if (pw.next !== pw.confirm) { error.value = 'New passwords do not match.'; return }
    if (!pw.current) { error.value = 'Enter your current password to set a new one.'; return }
  }
  saving.value = true
  try {
    const payload = { name: form.name, phone: form.phone, email: form.email, institute: form.institute }
    if (isStudent.value) payload.medium = form.medium
    if (pw.next) { payload.currentPassword = pw.current; payload.newPassword = pw.next }
    await auth.updateProfile(payload)
    saved.value = true
    pw.current = pw.next = pw.confirm = ''
    toast.add({ severity: 'success', summary: 'Profile updated', life: 2500 })
  } catch (e) {
    error.value = e.message || 'Could not update profile'
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.pf { max-width: 880px; margin: 0 auto; padding: 8px 4px 40px; }
.pf-head { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
.pf-avatar {
  width: 64px; height: 64px; border-radius: 20px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 1.4rem; color: #fff;
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  box-shadow: 0 0 24px color-mix(in srgb, var(--t-accent) 30%, transparent);
}
.pf-title { font-size: 1.6rem; font-weight: 800; color: var(--t-text1); margin: 0; line-height: 1.1; }
.pf-sub { font-size: 0.85rem; color: var(--t-text3); margin: 0.25rem 0 0; }

.pf-stack { display: flex; flex-direction: column; gap: 1.25rem; }
.pf-card { padding: 1.5rem; }
.pf-card-title { font-size: 1.05rem; font-weight: 700; color: var(--t-text1); margin: 0 0 0.25rem; }
.pf-hint { font-size: 0.8rem; color: var(--t-text3); margin: 0 0 1rem; }
.pf-card-title + .pf-fields { margin-top: 1rem; }
.pf-fields { display: flex; flex-direction: column; gap: 1rem; }
.pf-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
@media (max-width: 560px) { .pf-row { grid-template-columns: 1fr; } }

.pf-actions { display: flex; align-items: center; gap: 0.75rem; }
.pf-spacer { flex: 1; }
.pf-error { font-size: 0.85rem; color: var(--t-danger); font-weight: 600; }
.pf-ok { font-size: 0.85rem; color: var(--t-success); font-weight: 600; }
</style>
