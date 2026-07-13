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

    <!-- Loading -->
    <div v-if="loading" class="pp-state">
      <div class="pp-spinner" />
      <span>Loading papers…</span>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="pp-state pp-state--error">
      <span>{{ error }}</span>
      <button class="btn-ghost pp-retry" @click="load">Retry</button>
    </div>

    <!-- Empty -->
    <div v-else-if="!papers.length" class="pp-state">
      <span>No past papers available for this subject yet.</span>
    </div>

    <template v-else>
      <!-- Year filter -->
      <div class="sp-toolbar">
        <button type="button" class="sp-pill" :class="{ 'is-on': year === 'all' }" @click="year = 'all'">All years</button>
        <button
          v-for="y in availableYears"
          :key="y"
          type="button"
          class="sp-pill"
          :class="{ 'is-on': year === y }"
          @click="year = y"
        >{{ y }}</button>
      </div>

      <!-- Papers -->
      <div class="pp-list">
        <div v-for="p in filtered" :key="p.id" class="pp-row">
          <div class="pp-row-icon">{{ subject?.icon || '📄' }}</div>
          <div class="pp-row-main">
            <div class="pp-row-title">{{ p.subjectName }} — {{ p.paperType }} {{ p.year }}</div>
            <div class="pp-row-meta">{{ gradeLabel }} · {{ p.board }} · {{ p.totalMarks }} marks · {{ p.timeLimitMinutes }} min</div>
          </div>
          <span class="pp-badge">{{ p.board.split(' ')[0] }}</span>
          <RouterLink
            :to="`/app/preparation/${bid}/test/objective?type=past&year=${p.year}&paperId=${p.id}`"
            class="btn-primary pp-attempt"
          >
            Attempt <ArrowRight class="w-3.5 h-3.5" />
          </RouterLink>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowRight } from '@lucide/vue'
import api from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { findPrepSubject, gradeLabelFor } from '@/views/preparation/prepSubjects'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()
const auth = useAuthStore()

const bid = computed(() => props.bookId ?? route.params.bookId)
const subject = computed(() => findPrepSubject(bid.value))
const gradeLabel = computed(() => gradeLabelFor(bid.value))

const papers = ref([])
const loading = ref(false)
const error = ref(null)
const year = ref('all')

const availableYears = computed(() =>
  [...new Set(papers.value.map((p) => p.year))].sort((a, b) => b - a),
)
const filtered = computed(() =>
  year.value === 'all' ? papers.value : papers.value.filter((p) => p.year === year.value),
)

async function load() {
  if (!bid.value) return
  loading.value = true
  error.value = null
  year.value = 'all'
  try {
    const params = { subjectId: bid.value }
    const gradeCode = auth.user?.gradeCode
    if (gradeCode) params.gradeCode = gradeCode
    const { data } = await api.get('/past-papers', { params })
    papers.value = data
  } catch (e) {
    error.value = e.message || 'Failed to load past papers'
  } finally {
    loading.value = false
  }
}

onMounted(load)
watch(bid, load)
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
.pp-state {
  display: flex; flex-direction: column; align-items: center; gap: 0.75rem;
  padding: 3rem 1rem; color: var(--t-text3); font-size: 0.85rem; text-align: center;
}
.pp-state--error { color: #ef4444; }
.pp-retry { margin-top: 0.25rem; }
.pp-spinner {
  width: 24px; height: 24px; border-radius: 50%;
  border: 3px solid var(--t-border); border-top-color: var(--t-accent);
  animation: spin 0.7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 560px) {
  .pp-badge { display: none; }
  .pp-row { flex-wrap: wrap; }
}
</style>
