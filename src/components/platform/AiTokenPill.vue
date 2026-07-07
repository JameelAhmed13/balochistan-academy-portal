<template>
  <RouterLink to="/app/coins/redeem" class="tp-pill" :class="{ empty: student.totalAiTokens <= 0 }">
    🪙 {{ student.totalAiTokens }} AI token{{ student.totalAiTokens !== 1 ? 's' : '' }} left
  </RouterLink>
</template>

<script setup>
import { onMounted } from 'vue'
import { useStudentStore } from '@/stores/student'

const student = useStudentStore()
onMounted(() => { if (!student.subscription) student.fetchSubscription() })
</script>

<style scoped>
.tp-pill {
  display: inline-flex; align-items: center; gap: 0.3rem;
  font-size: 0.72rem; font-weight: 700; text-decoration: none;
  padding: 0.25rem 0.65rem; border-radius: 999px;
  background: var(--t-hover); border: 1px solid var(--t-border); color: var(--t-text2);
  transition: all 0.15s;
}
.tp-pill:hover { border-color: var(--t-accent); color: var(--t-accent); }
.tp-pill.empty {
  background: color-mix(in srgb, #ef4444 10%, transparent);
  border-color: color-mix(in srgb, #ef4444 30%, transparent);
  color: #ef4444;
}
</style>
