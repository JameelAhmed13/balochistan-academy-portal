<template>
  <div class="pv-home">

    <!-- Book header (when a book is selected) -->
    <div v-if="book" class="pv-book-hd">
      <div class="pv-book-badge" :class="book.color" aria-hidden="true">
        {{ book.icon }}
      </div>
      <div class="pv-book-info">
        <h1 class="pv-book-name">{{ book.name }}</h1>
        <p class="pv-book-desc">Select a study option above to begin</p>
      </div>
    </div>

    <!-- Empty state when no book selected -->
    <div v-else class="pv-empty">
      <div class="pv-empty-icon" aria-hidden="true">📚</div>
      <p class="pv-empty-lbl">Select a subject from the sidebar to get started</p>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { findPrepSubject } from '@/views/preparation/prepSubjects'

const props = defineProps({ bookId: { type: [String, Number], default: null } })

const route = useRoute()

const book = computed(() => {
  const id = props.bookId ?? route.params.bookId ?? null
  if (id == null || id === '') return null
  return findPrepSubject(id)
})
</script>

<style scoped>
.pv-home { width: 100%; }

/* Book header */
.pv-book-hd {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: var(--t-surface);
  border: 1px solid var(--t-border);
  border-radius: 14px;
}
.pv-book-badge {
  width: 52px;
  height: 52px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  flex-shrink: 0;
}
.pv-book-name {
  font-size: 1.2rem;
  font-weight: 800;
  color: var(--t-text1);
  margin: 0 0 0.2rem;
  font-family: 'Syne', sans-serif;
  line-height: 1.2;
}
.pv-book-desc { font-size: 0.8rem; color: var(--t-text3); margin: 0; }

/* Empty state */
.pv-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  text-align: center;
  gap: 0.75rem;
}
.pv-empty-icon { font-size: 2.5rem; }
.pv-empty-lbl { font-size: 0.9rem; color: var(--t-text3); max-width: 260px; }
</style>
