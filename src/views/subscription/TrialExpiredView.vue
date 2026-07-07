<template>
  <div class="te-root">
    <div class="te-card">
      <div class="te-icon">⏳</div>
      <h1 class="te-title">Your Free Trial Has Ended</h1>
      <p class="te-sub">
        Your 7-day free trial finished{{ trialEndDate ? ` on ${trialEndDate}` : '' }}. Subscribe to keep using
        AI Tutor, tests, and all learning tools.
      </p>

      <div v-if="loading" class="te-loading">Loading plans…</div>
      <div v-else-if="student.plans.length" class="te-plans">
        <div v-for="p in student.plans" :key="p.id" class="te-plan">
          <div class="te-plan-name">{{ p.name }}</div>
          <div class="te-plan-price">Rs. {{ p.price.toLocaleString() }} <small>/ {{ p.durationDays }} days</small></div>
          <div class="te-plan-tokens">🪙 {{ p.aiTokenQuota.toLocaleString() }} AI tokens</div>
        </div>
      </div>

      <div class="te-actions">
        <RouterLink to="/app/coins/redeem" class="btn-primary">🪙 Redeem with Coins</RouterLink>
        <RouterLink to="/app/checkout" class="btn-secondary">💳 Pay via Easypaisa / JazzCash / Bank</RouterLink>
      </div>

      <button type="button" class="te-logout" @click="auth.logout()">Log out</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useStudentStore } from '@/stores/student'
import { useAuthStore } from '@/stores/auth'

const student = useStudentStore()
const auth = useAuthStore()
const loading = ref(false)

const trialEndDate = computed(() =>
  student.trialEndsAt ? new Date(student.trialEndsAt).toLocaleDateString('en-PK', { day: 'numeric', month: 'short', year: 'numeric', timeZone: 'Asia/Karachi' }) : null,
)

onMounted(async () => {
  loading.value = true
  try {
    await Promise.allSettled([student.fetchPlans(), student.fetchSubscription(), student.fetchBalance()])
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.te-root { min-height: 70vh; display: flex; align-items: center; justify-content: center; padding: 24px; }
.te-card {
  width: min(560px, 100%); text-align: center; padding: 2.5rem 2rem;
  border-radius: 20px; background: var(--t-bg2); border: 1px solid var(--t-border);
  box-shadow: var(--t-shadow-md);
}
.te-icon { font-size: 2.75rem; margin-bottom: 0.5rem; }
.te-title { font-size: 1.4rem; font-weight: 800; color: var(--t-text1); margin: 0; }
.te-sub { font-size: 0.88rem; color: var(--t-text2); line-height: 1.6; margin: 0.5rem 0 1.5rem; }

.te-loading { font-size: 0.85rem; color: var(--t-text3); padding: 1rem 0; }
.te-plans { display: flex; gap: 0.75rem; justify-content: center; flex-wrap: wrap; margin-bottom: 1.5rem; }
.te-plan {
  min-width: 140px; padding: 0.85rem 1rem; border-radius: 12px;
  border: 1px solid var(--t-border); background: var(--t-bg);
}
.te-plan-name { font-weight: 700; font-size: 0.85rem; color: var(--t-text1); }
.te-plan-price { font-size: 0.95rem; font-weight: 800; color: var(--t-text1); margin: 0.2rem 0; }
.te-plan-price small { font-size: 0.7rem; font-weight: 600; color: var(--t-text3); }
.te-plan-tokens { font-size: 0.75rem; color: var(--t-gold, #d97706); font-weight: 600; }

.te-actions { display: flex; flex-direction: column; gap: 0.6rem; align-items: stretch; }
.te-actions > * { justify-content: center; }
.te-logout {
  margin-top: 1.25rem; background: none; border: none; cursor: pointer;
  font-size: 0.8rem; color: var(--t-text3); text-decoration: underline;
}
.te-logout:hover { color: var(--t-text2); }
</style>
