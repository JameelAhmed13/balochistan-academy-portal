<template>
  <span class="diff-badge">⚡ Difficulty Level: {{ level }}</span>
</template>
<script setup>
defineProps({ level: String })
</script>
<style scoped>
.diff-badge {
  display: inline-flex; align-items: center; gap: 0.2rem;
  padding: 0.2rem 0.65rem; border-radius: 99px; font-size: 0.72rem; font-weight: 600;
  background: #fff3e0; color: #e65100; border: 1px solid #ffcc80;
  white-space: nowrap;
}
html.dark .diff-badge { background: rgba(230,81,0,0.2); color: #ffb74d; border-color: rgba(230,81,0,0.4); }
</style>
