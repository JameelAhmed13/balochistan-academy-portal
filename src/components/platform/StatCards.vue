<template>
  <div class="plat-stats">
    <div class="ps-card ps-green">Total Tests: {{ total }}</div>
    <div class="ps-card ps-orange">Pending Tests: {{ pending }}</div>
    <div class="ps-card ps-plain">Taken Tests: {{ taken }}</div>
  </div>
</template>

<script setup>
defineProps({ total: { default: 0 }, pending: { default: 0 }, taken: { default: 0 } })
</script>

<style scoped>
.plat-stats { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1rem; margin-bottom: 1.5rem; }
.ps-card {
  padding: 1.25rem 1rem; border-radius: 12px; text-align: center;
  font-size: 1.2rem; font-weight: 600; font-family: Georgia, serif; letter-spacing: 0.01em;
}
.ps-green  { background: #e8f5e9; color: #2e7d32; border: 1px solid #c8e6c9; }
.ps-orange { background: #fff8e1; color: #e65100; border: 1px solid #ffe0b2; }
.ps-plain  { background: var(--t-surface); color: var(--t-text1); border: 1px solid var(--t-border); }
html.dark .ps-green  { background: rgba(46,125,50,0.15); color: #81c784; border-color: rgba(46,125,50,0.3); }
html.dark .ps-orange { background: rgba(230,81,0,0.15); color: #ffb74d; border-color: rgba(230,81,0,0.3); }
@media (max-width: 600px) { .plat-stats { grid-template-columns: 1fr; } }
</style>
