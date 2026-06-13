<template>
  <div class="sg-page">
    <div class="sg-card">
      <h1 class="sg-title">Choose your class</h1>
      <p class="sg-sub">Pick your grade — your syllabus, tests and leaderboard are all tuned to it.</p>

      <div v-if="loading" class="sg-loading">Loading grades…</div>
      <div v-else class="sg-grid">
        <button
          v-for="g in grades"
          :key="g.code"
          class="sg-grade"
          :class="{ active: selected === g.code }"
          @click="selected = g.code"
        >
          <span class="sg-grade-label">{{ g.label }}</span>
          <span class="sg-grade-band">{{ g.band }}</span>
        </button>
      </div>

      <p v-if="error" class="sg-error">{{ error }}</p>

      <button class="sg-confirm" :disabled="!selected || saving" @click="confirm">
        {{ saving ? 'Saving…' : 'Start learning →' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()
const grades = ref([])
const selected = ref(auth.user?.gradeCode || '')
const loading = ref(true)
const saving = ref(false)
const error = ref('')

onMounted(async () => {
  try {
    grades.value = (await api.get('/grades')).data
  } catch (e) {
    error.value = e.message || 'Could not load grades'
  } finally {
    loading.value = false
  }
})

async function confirm() {
  if (!selected.value) return
  saving.value = true
  error.value = ''
  try {
    const { data } = await api.put('/auth/me/grade', { gradeCode: selected.value })
    auth.user = data.user
    localStorage.setItem('bap_user', JSON.stringify(data.user))
    router.push('/app')
  } catch (e) {
    error.value = e.message || 'Could not save your grade'
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.sg-page { min-height: 100vh; display: grid; place-items: center; padding: 24px; background: var(--t-bg); color: var(--t-text1); }
.sg-card { width: min(720px, 100%); padding: 40px; border-radius: 24px; background: var(--t-bg2); border: 1px solid var(--t-border); }
.sg-title { font-family: 'Syne', sans-serif; font-weight: 800; font-size: clamp(1.8rem, 4vw, 2.6rem); margin: 0 0 8px; }
.sg-sub { margin: 0 0 28px; color: var(--t-text2); }
.sg-loading { color: var(--t-text2); padding: 30px 0; }
.sg-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(120px, 1fr)); gap: 12px; margin-bottom: 28px; }
.sg-grade {
  display: flex; flex-direction: column; gap: 4px; align-items: flex-start; padding: 16px;
  border-radius: 14px; cursor: pointer; text-align: left;
  background: var(--t-hover); border: 1.5px solid var(--t-border); transition: border-color .2s, transform .2s, background .2s;
}
.sg-grade:hover { transform: translateY(-2px); border-color: var(--t-accent); }
.sg-grade.active { border-color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 12%, transparent); }
.sg-grade-label { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1.05rem; }
.sg-grade-band { font-size: .72rem; font-weight: 600; letter-spacing: .06em; text-transform: uppercase; color: var(--t-text3); }
.sg-error { color: #f87171; margin: 0 0 16px; font-size: .9rem; }
.sg-confirm {
  width: 100%; padding: 15px; border: 0; border-radius: 14px; cursor: pointer;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1rem; color: #fff;
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  transition: opacity .2s, transform .2s;
}
.sg-confirm:disabled { opacity: .5; cursor: not-allowed; }
.sg-confirm:not(:disabled):hover { transform: translateY(-2px); }
</style>
