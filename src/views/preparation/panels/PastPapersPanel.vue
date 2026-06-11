<template>
  <div class="sp">
    <!-- Header -->
    <div class="sp-head">
      <span class="sp-head-icon" :class="subject?.color || 'grad-orange'">{{ subject?.icon || '📄' }}</span>
      <div class="sp-head-info">
        <div class="sp-eyebrow">Board Exams · Practice</div>
        <h1 class="sp-title">{{ subject?.name || 'Past & Model Papers' }}</h1>
        <p class="sp-sub">{{ gradeLabel }} board papers — attempt them here as a timed objective test.</p>
      </div>
    </div>

    <!-- Year filter -->
    <div class="sp-toolbar">
      <button type="button" class="sp-pill" :class="{ 'is-on': year === 'all' }" @click="year = 'all'">All years</button>
      <button v-for="y in years" :key="y" type="button" class="sp-pill" :class="{ 'is-on': year === y }" @click="year = y">{{ y }}</button>
    </div>

    <!-- Papers -->
    <div class="pp-list">
      <div v-for="p in filtered" :key="p.id" class="pp-row">
        <div class="pp-row-icon">{{ subject?.icon || '📄' }}</div>
        <div class="pp-row-main">
          <div class="pp-row-title">{{ subject?.name }} — Annual {{ p.year }}</div>
          <div class="pp-row-meta">{{ gradeLabel }} · {{ p.board }} · {{ p.type }}</div>
        </div>
        <span class="pp-badge">{{ p.board.split(' ')[0] }}</span>
        <RouterLink :to="`/app/preparation/${bid}/test/objective?type=past&year=${p.year}`" class="btn-primary pp-attempt">
          Attempt <ArrowRight class="w-3.5 h-3.5" />
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowRight } from '@lucide/vue'
import { findPrepSubject, gradeLabelFor } from '@/views/preparation/prepSubjects'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()

const bid = computed(() => props.bookId ?? route.params.bookId)
const subject = computed(() => findPrepSubject(bid.value))
const gradeLabel = computed(() => gradeLabelFor(bid.value))

const years = [2024, 2023, 2022, 2021, 2020, 2019]
const year = ref('all')

// Subject-scoped board papers across years (Federal Board on even years, Punjab Board on odd).
const papers = computed(() =>
  years.map((y, i) => ({
    id: `${bid.value}-${y}`,
    year: y,
    board: i % 2 === 0 ? 'Federal Board' : 'Punjab Board',
    type: y >= 2023 ? 'Model Paper' : 'Past Paper',
  })),
)
const filtered = computed(() => (year.value === 'all' ? papers.value : papers.value.filter((p) => p.year === year.value)))
</script>

<style scoped>
.pp-list { display: flex; flex-direction: column; gap: 0.5rem; }
.pp-row {
  display: flex; align-items: center; gap: 0.85rem; padding: 0.7rem 0.9rem;
  border: 1px solid var(--t-border); border-radius: 12px; background: var(--t-surface);
}
.pp-row-icon {
  width: 38px; height: 38px; border-radius: 9px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center; font-size: 1.2rem;
  background: var(--t-hover);
}
.pp-row-main { flex: 1; min-width: 0; }
.pp-row-title { font-size: 0.86rem; font-weight: 700; color: var(--t-text1); }
.pp-row-meta { font-size: 0.72rem; color: var(--t-text3); margin-top: 0.1rem; }
.pp-badge {
  font-size: 0.64rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.04em;
  color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-accent) 22%, transparent);
  border-radius: 99px; padding: 0.2rem 0.6rem; flex-shrink: 0;
}
.pp-attempt { font-size: 0.78rem; padding: 0.45rem 0.9rem; flex-shrink: 0; }
@media (max-width: 560px) {
  .pp-badge { display: none; }
  .pp-row { flex-wrap: wrap; }
}
</style>
