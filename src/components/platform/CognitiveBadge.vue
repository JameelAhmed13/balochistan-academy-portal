<template>
  <span class="cog-badge">🧠 Cognitive Level: {{ level }}</span>
</template>
<script setup>
defineProps({ level: String })
</script>
<style scoped>
.cog-badge {
  display: inline-flex; align-items: center; gap: 0.2rem;
  padding: 0.2rem 0.65rem; border-radius: 99px; font-size: 0.72rem; font-weight: 600;
  background: #e3f2fd; color: #1565c0; border: 1px solid #90caf9;
  white-space: nowrap;
}
html.dark .cog-badge { background: rgba(21,101,192,0.2); color: #90caf9; border-color: rgba(21,101,192,0.4); }
</style>
