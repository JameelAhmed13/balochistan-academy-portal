<template>
  <div class="adm">
    <header class="adm-head">
      <div>
        <h1>Syllabus</h1>
        <p>Units, topics and learning objectives per grade &amp; subject. Students see this as their course outline.</p>
      </div>
    </header>

    <div class="adm-pickers">
      <label>Grade
        <select v-model="gradeCode" @change="onGradeChange">
          <option value="">Select…</option>
          <option v-for="g in catalog.grades" :key="g.code" :value="g.code">{{ g.label }}</option>
        </select>
      </label>
      <label>Subject
        <select v-model="subjectId" :disabled="!gradeCode" @change="loadSyllabus">
          <option value="">Select…</option>
          <option v-for="s in gradeSubjects" :key="s.id" :value="s.id">{{ s.name }}</option>
        </select>
      </label>
      <button v-if="gradeCode && subjectId" class="adm-btn" @click="addUnit">+ Add unit</button>
    </div>

    <div v-if="loading" class="adm-muted">Loading…</div>
    <div v-else-if="gradeCode && subjectId && !units.length" class="adm-muted">No units yet — add the first one.</div>

    <div v-else class="adm-units">
      <details v-for="u in units" :key="u.id" class="adm-unit" open>
        <summary>
          <b>{{ u.name }}</b>
          <span class="adm-sub">{{ (u.topics || []).length }} topics · {{ objCount(u) }} objectives</span>
          <button class="adm-link danger" @click.prevent="delUnit(u)">delete unit</button>
        </summary>

        <div class="adm-unit-body">
          <!-- unit-level objectives -->
          <div class="adm-objs">
            <div class="adm-objs-head">Learning objectives</div>
            <ul>
              <li v-for="o in u.objectives" :key="o.id">
                {{ o.text }} <button class="adm-x" @click="delObjective(u, null, o)">×</button>
              </li>
            </ul>
            <form class="adm-add" @submit.prevent="addObjective(u, null)">
              <input v-model="newObj[`u${u.id}`]" placeholder="Add objective…" />
              <button class="adm-link">add</button>
            </form>
          </div>

          <!-- topics -->
          <div v-for="t in u.topics" :key="t.id" class="adm-topic">
            <div class="adm-topic-head">
              <b>{{ t.name }}</b>
              <button class="adm-x" @click="delTopic(u, t)">×</button>
            </div>
            <ul>
              <li v-for="o in t.objectives" :key="o.id">
                {{ o.text }} <button class="adm-x" @click="delObjective(u, t, o)">×</button>
              </li>
            </ul>
            <form class="adm-add" @submit.prevent="addObjective(u, t)">
              <input v-model="newObj[`t${t.id}`]" placeholder="Add objective to topic…" />
              <button class="adm-link">add</button>
            </form>
          </div>

          <form class="adm-add" @submit.prevent="addTopic(u)">
            <input v-model="newTopic[`u${u.id}`]" placeholder="Add topic…" />
            <button class="adm-link">+ topic</button>
          </form>
        </div>
      </details>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { useCatalogStore } from '@/stores/catalog'

const catalog = useCatalogStore()
const gradeCode = ref('')
const subjectId = ref('')
const units = ref([])
const loading = ref(false)
const newObj = reactive({})
const newTopic = reactive({})

const gradeSubjects = computed(() => catalog.subjectsForGrade(gradeCode.value))
const objCount = (u) => (u.objectives?.length || 0) + (u.topics || []).reduce((n, t) => n + (t.objectives?.length || 0), 0)

onMounted(() => catalog.fetchGrades())

async function onGradeChange() {
  subjectId.value = ''
  units.value = []
  if (gradeCode.value) await catalog.fetchSubjectsForGrade(gradeCode.value)
}

async function loadSyllabus() {
  if (!gradeCode.value || !subjectId.value) return
  loading.value = true
  try { units.value = await catalog.fetchSyllabus(gradeCode.value, subjectId.value) }
  finally { loading.value = false }
}

async function addUnit() {
  const name = prompt('Unit name?', `Unit ${units.value.length + 1}`)
  if (!name) return
  await catalog.addUnit({ grade_code: gradeCode.value, subject_id: Number(subjectId.value), name, number: units.value.length + 1, sort_order: units.value.length })
  await loadSyllabus()
}
async function delUnit(u) { if (confirm(`Delete ${u.name} and its topics/objectives?`)) { await catalog.deleteUnit(u.id); await loadSyllabus() } }
async function addTopic(u) {
  const name = newTopic[`u${u.id}`]?.trim(); if (!name) return
  await catalog.addTopic({ unit_id: u.id, name }); newTopic[`u${u.id}`] = ''; await loadSyllabus()
}
async function delTopic(u, t) { await catalog.deleteTopic(t.id); await loadSyllabus() }
async function addObjective(u, t) {
  const key = t ? `t${t.id}` : `u${u.id}`
  const text = newObj[key]?.trim(); if (!text) return
  await catalog.addObjective({ unit_id: u.id, topic_id: t?.id ?? undefined, text }); newObj[key] = ''; await loadSyllabus()
}
async function delObjective(u, t, o) { await catalog.deleteObjective(o.id); await loadSyllabus() }
</script>

<style scoped>
.adm { max-width: none; margin: 0; padding: 24px 8px; color: var(--t-text1); }
.adm-head h1 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.9rem; margin: 0 0 4px; }
.adm-head p { margin: 0; color: var(--t-text2); font-size: .9rem; }
.adm-muted { color: var(--t-text2); padding: 14px 0; }
.adm-pickers { display: flex; gap: 14px; align-items: flex-end; margin: 22px 0; flex-wrap: wrap; }
.adm-pickers label { display: flex; flex-direction: column; gap: 5px; font-size: .8rem; font-weight: 600; color: var(--t-text2); }
.adm-pickers select { padding: 10px 12px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); min-width: 180px; }
.adm-btn { padding: 10px 18px; border: 0; border-radius: 11px; cursor: pointer; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.adm-units { display: flex; flex-direction: column; gap: 12px; }
.adm-unit { border: 1px solid var(--t-border); border-radius: 14px; background: var(--t-bg2); overflow: hidden; }
.adm-unit summary { display: flex; align-items: center; gap: 12px; padding: 14px 18px; cursor: pointer; }
.adm-unit summary b { font-family: 'Syne', sans-serif; }
.adm-sub { font-size: .76rem; color: var(--t-text3); }
.adm-unit summary .adm-link { margin-left: auto; }
.adm-unit-body { padding: 0 18px 18px; display: flex; flex-direction: column; gap: 14px; }
.adm-objs-head { font-size: .72rem; text-transform: uppercase; letter-spacing: .07em; color: var(--t-text3); margin-bottom: 6px; }
.adm-topic { border-left: 2px solid var(--t-border); padding-left: 14px; }
.adm-topic-head { display: flex; align-items: center; gap: 8px; }
.adm-unit ul { margin: 6px 0; padding-left: 18px; }
.adm-unit li { font-size: .87rem; color: var(--t-text2); margin-bottom: 4px; }
.adm-add { display: flex; gap: 8px; margin-top: 6px; }
.adm-add input { flex: 1; padding: 8px 11px; border-radius: 9px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text1); font-size: .85rem; }
.adm-link { background: none; border: 0; cursor: pointer; color: var(--t-accent); font-weight: 600; font-size: .82rem; padding: 0; white-space: nowrap; }
.adm-link.danger { color: #f87171; }
.adm-x { background: none; border: 0; cursor: pointer; color: var(--t-text3); font-size: 1rem; line-height: 1; }
.adm-x:hover { color: #f87171; }
</style>
