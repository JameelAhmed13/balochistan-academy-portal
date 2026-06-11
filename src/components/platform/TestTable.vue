<template>
  <div class="pt-wrap">
    <div class="pt-header-row">
      <h3 class="pt-title">Tests Details</h3>
      <slot name="actions" />
    </div>
    <div class="pt-table-wrap">
      <table class="pt-table">
        <thead>
          <tr class="pt-thead">
            <th>Test No</th>
            <th>Book</th>
            <th>Test Date</th>
            <th>Result</th>
            <th>Coins</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          <slot name="rows" />
          <tr v-if="empty">
            <td colspan="6" class="pt-empty">
              <slot name="empty">
                <div class="pt-empty-state">
                  <div class="pt-empty-icon">📋</div>
                  <div class="pt-empty-title">No tests found</div>
                  <div class="pt-empty-sub">No follow-up tests have been generated yet.</div>
                </div>
              </slot>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
defineProps({ empty: Boolean })
</script>

<style scoped>
.pt-wrap { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.pt-header-row { display: flex; align-items: center; justify-content: space-between; padding: 1rem 1.25rem; border-bottom: 1px solid var(--t-border); }
.pt-title { font-size: 0.95rem; font-weight: 600; color: var(--t-text1); margin: 0; }
.pt-table-wrap { overflow-x: auto; }
.pt-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.pt-thead th {
  background: #4caf50; color: white; padding: 0.75rem 1rem;
  text-align: left; font-weight: 600; font-size: 0.8rem; white-space: nowrap;
}
.pt-table :deep(td) { padding: 0.75rem 1rem; color: var(--t-text1); border-bottom: 1px solid var(--t-border); }
.pt-table :deep(tr:last-child td) { border-bottom: none; }
.pt-table :deep(tr:hover td) { background: var(--t-hover); }
.pt-table :deep(tr:nth-child(even) td) { background: var(--t-hover); }

/* Buttons */
.pt-table :deep(.btn-view) {
  display: inline-block; padding: 0.4rem 1rem; border-radius: 6px; font-size: 0.78rem; font-weight: 700;
  background: #00bcd4; color: white; border: none; cursor: pointer; white-space: nowrap;
  text-decoration: none; transition: opacity 0.15s;
}
.pt-table :deep(.btn-view:hover) { opacity: 0.85; }
.pt-table :deep(.btn-attempt) {
  display: inline-block; padding: 0.4rem 1rem; border-radius: 6px; font-size: 0.78rem; font-weight: 700;
  background: #4caf50; color: white; border: none; cursor: pointer; white-space: nowrap;
  text-decoration: none; transition: opacity 0.15s;
}
.pt-table :deep(.btn-attempt:hover) { opacity: 0.85; }
.pt-table :deep(.btn-create) {
  display: inline-flex; align-items: center; gap: 0.3rem;
  padding: 0.45rem 1.1rem; border-radius: 8px; font-size: 0.82rem; font-weight: 700;
  background: #f57c00; color: white; border: none; cursor: pointer; white-space: nowrap;
  transition: opacity 0.15s;
}
.pt-table :deep(.btn-create:hover) { opacity: 0.85; }
.pt-table :deep(.status-pending)  { color: #f57c00; font-size: 0.8rem; }
.pt-table :deep(.status-expired)  { color: #e53935; font-size: 0.8rem; }
.pt-table :deep(.status-notq)     { color: #757575; font-size: 0.8rem; }
.pt-empty { padding: 2.5rem 1rem; }
.pt-empty-state { display: flex; flex-direction: column; align-items: center; gap: 0.5rem; text-align: center; }
.pt-empty-icon { font-size: 2.5rem; }
.pt-empty-title { font-size: 1rem; font-weight: 600; color: var(--t-text2); }
.pt-empty-sub { font-size: 0.8rem; color: var(--t-text3); }
</style>
