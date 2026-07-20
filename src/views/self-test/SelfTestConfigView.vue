<template>
  <div class="animate-fade-in space-y-5" v-if="subject">
    <!-- Header banner -->
    <div class="config-banner">
      <span class="config-icon">{{ subject?.icon || '📘' }}</span>
      <div>
        <div class="config-title">Self Test — {{ typeName }}</div>
        <div class="config-sub">{{ subject.name }}</div>
      </div>
    </div>

    <!-- Time input (selftest5 style) -->
    <div class="time-row">
      <label class="time-lbl">⏱ Time (min):</label>
      <input v-model.number="timeLimit" type="number" min="5" max="180" class="time-input" />
    </div>

    <!-- Objective section -->
    <div class="config-section">
      <div class="config-section-header">
        <h3>OBJECTIVE PAPER CONFIGURATION</h3>
      </div>
      <div class="config-grid">
        <div class="config-group">
          <div class="config-group-label">🗃 QUESTION TYPE</div>
          <div class="check-row">
            <label v-for="t in ['Exercise','Conceptual']" :key="t" class="check-item">
              <input type="checkbox" v-model="objConfig.types" :value="t" class="check-input" /> {{ t }}
            </label>
          </div>
        </div>
        <div class="config-group">
          <div class="config-group-label">⚡ DIFFICULTY LEVEL</div>
          <div class="check-row">
            <label v-for="d in ['Easy','Medium','Hard']" :key="d" class="check-item">
              <input type="checkbox" v-model="objConfig.difficulty" :value="d" class="check-input" /> {{ d }}
            </label>
          </div>
        </div>
      </div>
    </div>

    <!-- Units/topics -->
    <div class="units-section">
      <div class="units-header">
        Select units and topics
        <button @click="startTest" class="btn-start-test">START TEST</button>
      </div>
      <div class="units-all">
        <label class="unit-row unit-all-row">
          <input type="checkbox" v-model="allUnits" class="check-input" />
          <span class="font-semibold">All Units</span>
        </label>
      </div>
      <div v-if="!units.length" class="unit-group" style="color:var(--t-text3);font-size:0.82rem">
        No units available — all questions will be included.
      </div>
      <div v-for="(unit, i) in units" :key="unit.id" class="unit-group">
        <label class="unit-row unit-header-row">
          <input type="checkbox" v-model="selectedUnits" :value="unit.id" class="check-input" :disabled="allUnits" />
          <span class="unit-name">{{ unit.name }}</span>
          <span class="unit-name-en">Unit {{ i + 1 }}</span>
        </label>
        <div v-if="unit.topics?.length" class="topics-grid">
          <label v-for="topic in unit.topics.slice(0, 7)" :key="topic.id" class="topic-item">
            <input type="checkbox" v-model="selectedTopics" :value="unit.id + '_' + topic.id" class="check-input" :disabled="allUnits" />
            <span class="text-xs">{{ topic.name }}</span>
          </label>
        </div>
      </div>
    </div>

    <button @click="startTest" class="btn-start-big">⚡ START TEST</button>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import PageFooter from '@/components/platform/PageFooter.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const catalog = useCatalogStore()

const bookId = computed(() => +route.params.bookId)
const gradeCode = computed(() => auth.user?.gradeCode || '9')
const testType = computed(() => route.query.type || 'general')

const subject = computed(() => {
  const all = Object.values(catalog.gradeSubjects).flat()
  return all.find(s => String(s.id) === String(bookId.value)) || null
})
const units = ref([])

const typeName = computed(() => ({ general: 'General Self Test', past: 'Past Paper', predicted: 'Predicted Exam' }[testType.value] || 'Self Test'))

const timeLimit = ref(30)
const allUnits = ref(true)
const selectedUnits = ref([])
const selectedTopics = ref([])
const objConfig = ref({ types: ['Exercise', 'Conceptual'], difficulty: ['Easy', 'Medium', 'Hard'] })

watch(allUnits, v => { if (v) selectedUnits.value = [] })

onMounted(async () => {
  try { units.value = await catalog.fetchSyllabus(gradeCode.value, bookId.value) } catch { units.value = [] }
})

function startTest() {
  const unitParam = allUnits.value ? '' : selectedUnits.value.join(',')
  router.push({
    path: `/app/self-test/${bookId.value}/take`,
    query: {
      type: testType.value,
      time: timeLimit.value,
      types: objConfig.value.types.join(','),
      difficulty: objConfig.value.difficulty.join(','),
      ...(unitParam ? { units: unitParam } : {}),
    }
  })
}
</script>

<style scoped>
.config-banner {
  display: flex; align-items: center; gap: 1rem;
  padding: 1.25rem 1.5rem; border-radius: 14px; color: white;
  box-shadow: 0 4px 20px rgba(0,0,0,0.2);
}
.config-icon { font-size: 2rem; }
.config-title { font-size: 1.1rem; font-weight: 800; }
.config-sub { font-size: 0.82rem; opacity: 0.8; }

.time-row {
  display: flex; align-items: center; gap: 1rem;
  padding: 0.85rem 1.25rem; border-radius: 10px;
  background: #fffde7; border: 1px solid #ffe0b2;
}
.time-lbl { font-size: 0.9rem; font-weight: 600; color: #e65100; }
.time-input {
  width: 80px; padding: 0.35rem 0.75rem; border-radius: 8px;
  border: 1px solid #ccc; font-size: 0.95rem; font-weight: 600;
  background: white; color: #333;
}
html.dark .time-row { background: rgba(255,220,0,0.08); border-color: rgba(255,152,0,0.3); }
html.dark .time-lbl { color: #ffb74d; }
html.dark .time-input { background: #1a2235; color: white; border-color: rgba(255,255,255,0.2); }

.config-section { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.config-section-header { background: var(--t-hover2); padding: 0.65rem 1.25rem; }
.config-section-header h3 { font-size: 0.8rem; font-weight: 700; color: var(--t-text2); letter-spacing: 0.05em; margin: 0; }
.config-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0; }
.config-group { padding: 1rem 1.25rem; }
.config-group + .config-group { border-left: 1px solid var(--t-border); }
.config-group-label { font-size: 0.72rem; font-weight: 800; color: #3b82f6; letter-spacing: 0.06em; margin-bottom: 0.65rem; }
.check-row { display: flex; flex-wrap: wrap; gap: 0.65rem; }
.check-item { display: flex; align-items: center; gap: 0.4rem; font-size: 0.875rem; color: var(--t-text1); cursor: pointer; }
.check-input { accent-color: #3b82f6; width: 15px; height: 15px; cursor: pointer; }

.units-section { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.units-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 0.9rem 1.25rem; background: var(--t-hover2);
  font-size: 0.875rem; color: var(--t-text2); font-weight: 500;
  border-bottom: 1px solid var(--t-border);
}
.btn-start-test {
  padding: 0.5rem 1.5rem; border-radius: 8px; font-weight: 700; font-size: 0.82rem;
  background: #3b82f6; color: white; border: none; cursor: pointer;
}
.units-all { padding: 0.65rem 1.25rem; border-bottom: 1px solid var(--t-border); }
.unit-row { display: flex; align-items: center; gap: 0.6rem; cursor: pointer; }
.unit-all-row { font-size: 0.9rem; font-weight: 600; color: var(--t-text1); }
.unit-group { border-bottom: 1px solid var(--t-border); padding: 0.65rem 1.25rem; }
.unit-header-row { margin-bottom: 0.5rem; }
.unit-name { font-size: 1rem; color: var(--t-text1); }
.unit-name-en { font-size: 0.7rem; color: var(--t-text3); margin-left: auto; }
.topics-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.35rem 1rem; padding-left: 1.75rem; }
.topic-item { display: flex; align-items: center; gap: 0.35rem; cursor: pointer; color: var(--t-text2); font-size: 0.82rem; }

.btn-start-big {
  width: 100%; padding: 1rem; border-radius: 12px; font-size: 1rem; font-weight: 700;
  background: linear-gradient(135deg, #3b82f6, #6c5ce7); color: white; border: none; cursor: pointer;
  box-shadow: 0 4px 20px rgba(59,130,246,0.3);
}
</style>
