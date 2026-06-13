<template>
  <div class="syl">
    <header class="syl-head">
      <p class="syl-kicker">{{ gradeLabel }} · Your Syllabus</p>
      <h1>What you'll master this year</h1>
      <p class="syl-sub">Pick a subject to see its units, topics and learning objectives — straight from your grade's curriculum.</p>
    </header>

    <div v-if="!subjects.length && loading" class="syl-muted">Loading your subjects…</div>
    <div v-else-if="!subjects.length" class="syl-muted">No subjects are set for your grade yet. Check back soon.</div>

    <!-- subject chips -->
    <div v-else class="syl-subjects">
      <button
        v-for="s in subjects"
        :key="s.id"
        class="syl-subject"
        :class="{ active: activeSubject?.id === s.id }"
        @click="selectSubject(s)"
      >
        <span class="syl-subject-icon">{{ s.icon || '📘' }}</span>
        <span>{{ s.name }}</span>
      </button>
    </div>

    <!-- syllabus tree -->
    <div v-if="activeSubject" class="syl-body">
      <div v-if="loadingUnits" class="syl-muted">Loading {{ activeSubject.name }} syllabus…</div>
      <div v-else-if="!units.length" class="syl-muted">Syllabus for {{ activeSubject.name }} is being prepared.</div>
      <div v-else class="syl-units">
        <article v-for="(u, i) in units" :key="u.id" class="syl-unit">
          <div class="syl-unit-head">
            <span class="syl-unit-num">{{ String(i + 1).padStart(2, '0') }}</span>
            <div>
              <h3>{{ u.name }}</h3>
              <p v-if="u.description">{{ u.description }}</p>
            </div>
          </div>
          <div v-if="u.objectives?.length" class="syl-objs">
            <span class="syl-objs-label">You'll learn to</span>
            <ul><li v-for="o in u.objectives" :key="o.id">{{ o.text }}</li></ul>
          </div>
          <div v-if="u.topics?.length" class="syl-topics">
            <span v-for="t in u.topics" :key="t.id" class="syl-topic">{{ t.name }}</span>
          </div>
          <RouterLink class="syl-practice" :to="`/app/preparation`">Practice this →</RouterLink>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'

const auth = useAuthStore()
const catalog = useCatalogStore()
const subjects = ref([])
const activeSubject = ref(null)
const units = ref([])
const loading = ref(true)
const loadingUnits = ref(false)

const gradeCode = computed(() => auth.user?.gradeCode)
const gradeLabel = computed(() => catalog.gradeByCode(gradeCode.value)?.label || `Grade ${gradeCode.value}`)

onMounted(async () => {
  if (!catalog.grades.length) await catalog.fetchGrades()
  if (gradeCode.value) {
    subjects.value = await catalog.fetchSubjectsForGrade(gradeCode.value)
    if (subjects.value.length) selectSubject(subjects.value[0])
  }
  loading.value = false
})

async function selectSubject(s) {
  activeSubject.value = s
  loadingUnits.value = true
  try { units.value = await catalog.fetchSyllabus(gradeCode.value, s.id) }
  finally { loadingUnits.value = false }
}
</script>

<style scoped>
.syl { max-width: 1080px; margin: 0 auto; padding: 28px 24px 80px; color: var(--t-text1); }
.syl-head { margin-bottom: 26px; }
.syl-kicker { font-size: .76rem; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; color: var(--t-accent); margin: 0 0 8px; }
.syl-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: clamp(1.7rem, 4vw, 2.4rem); margin: 0 0 8px; }
.syl-sub { margin: 0; color: var(--t-text2); max-width: 620px; }
.syl-muted { color: var(--t-text2); padding: 20px 0; }
.syl-subjects { display: flex; flex-wrap: wrap; gap: 10px; margin-bottom: 28px; }
.syl-subject { display: inline-flex; align-items: center; gap: 8px; padding: 10px 16px; border-radius: 12px; cursor: pointer; font-weight: 600; font-size: .9rem; color: var(--t-text1); background: var(--t-hover); border: 1.5px solid var(--t-border); transition: border-color .2s, transform .2s; }
.syl-subject:hover { transform: translateY(-2px); border-color: var(--t-accent); }
.syl-subject.active { border-color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 12%, transparent); }
.syl-subject-icon { font-size: 1.1rem; }
.syl-units { display: flex; flex-direction: column; gap: 14px; }
.syl-unit { padding: 22px; border-radius: 18px; background: var(--t-bg2); border: 1px solid var(--t-border); }
.syl-unit-head { display: flex; gap: 16px; align-items: flex-start; margin-bottom: 14px; }
.syl-unit-num { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.5rem; color: transparent; -webkit-text-stroke: 1px var(--t-accent); }
.syl-unit-head h3 { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1.15rem; margin: 0 0 4px; }
.syl-unit-head p { margin: 0; font-size: .88rem; color: var(--t-text2); }
.syl-objs-label { font-size: .72rem; font-weight: 700; letter-spacing: .07em; text-transform: uppercase; color: var(--t-text3); }
.syl-objs ul { margin: 6px 0 14px; padding-left: 20px; }
.syl-objs li { font-size: .9rem; color: var(--t-text2); margin-bottom: 5px; }
.syl-topics { display: flex; flex-wrap: wrap; gap: 7px; margin-bottom: 14px; }
.syl-topic { padding: 4px 11px; border-radius: 99px; font-size: .76rem; font-weight: 600; color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 10%, transparent); border: 1px solid var(--t-border); }
.syl-practice { font-size: .85rem; font-weight: 700; color: var(--t-accent); text-decoration: none; }
</style>
