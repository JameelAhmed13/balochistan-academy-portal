<template>
  <div class="kpi-card">
    <div class="kpi-icon-wrap" :class="color">{{ icon }}</div>
    <div class="kpi-body">
      <div class="kpi-value">{{ value }}</div>
      <div class="kpi-label">{{ label }}</div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  label: String,
  value: [String, Number],
  icon: String,
  color: { type: String, default: 'blue' },
})
</script>

<style scoped>
.kpi-card {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 1rem; border-radius: 16px;
  background: var(--t-surface);
  border: 1px solid var(--t-border);
  box-shadow: var(--t-shadow);
  transition: background 0.25s, border-color 0.25s, box-shadow 0.25s, transform 0.2s;
  cursor: default;
}
.kpi-card:hover { box-shadow: var(--t-shadow-md); transform: translateY(-2px); }
.kpi-icon-wrap {
  width: 42px; height: 42px; border-radius: 12px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.25rem;
  transition: background 0.25s;
}
/* Icon backgrounds — themed per color */
.kpi-icon-wrap.blue    { background: rgba(59,  130, 246, 0.12); }
.kpi-icon-wrap.green   { background: rgba(16,  185, 129, 0.12); }
.kpi-icon-wrap.purple  { background: rgba(139, 92,  246, 0.12); }
.kpi-icon-wrap.amber   { background: rgba(245, 158, 11,  0.12); }
.kpi-icon-wrap.rose    { background: rgba(239, 68,  68,  0.12); }
.kpi-icon-wrap.teal    { background: rgba(20,  184, 166, 0.12); }

/* In light mode use more opaque fills */
:global(html:not(.dark)) .kpi-icon-wrap.blue   { background: #eff6ff; }
:global(html:not(.dark)) .kpi-icon-wrap.green  { background: #f0fdf4; }
:global(html:not(.dark)) .kpi-icon-wrap.purple { background: #faf5ff; }
:global(html:not(.dark)) .kpi-icon-wrap.amber  { background: #fffbeb; }
:global(html:not(.dark)) .kpi-icon-wrap.rose   { background: #fef2f2; }
:global(html:not(.dark)) .kpi-icon-wrap.teal   { background: #f0fdfa; }

.kpi-body { min-width: 0; }
.kpi-value {
  font-weight: 700; font-size: 1.1rem; line-height: 1.2;
  color: var(--t-text1);
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
  transition: color 0.15s;
}
.kpi-label {
  font-size: 0.72rem; color: var(--t-text3);
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
  transition: color 0.15s;
}
</style>
