<template>
  <div class="sp-plan">
    <header class="spp-head">
      <div>
        <h1 class="spp-title">Study Plan</h1>
        <p class="spp-sub">An AI-style adaptive schedule that paces your syllabus up to exam day — and re-plans when you fall behind.</p>
      </div>
    </header>

    <!-- Generate / update form -->
    <section class="card spp-form">
      <h2 class="spp-form-title">Build a plan</h2>
      <div class="spp-form-grid">
        <div>
          <label class="label">Subject</label>
          <select v-model="form.subjectId" class="input">
            <option value="" disabled>Select subject</option>
            <option v-for="s in subjects" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div>
          <label class="label">Exam date</label>
          <input v-model="form.examDate" type="date" class="input" :min="tomorrow" />
        </div>
        <div>
          <label class="label">Study hours / day</label>
          <input v-model.number="form.dailyHours" type="number" class="input" min="0.5" max="12" step="0.5" />
        </div>
        <div class="spp-form-action">
          <button class="btn-primary" :disabled="!canGenerate || busy" @click="onGenerate">
            {{ busy ? 'Building…' : 'Generate plan' }}
          </button>
        </div>
      </div>
      <p v-if="error" class="spp-error">{{ error }}</p>
      <p v-else-if="meta && meta.uncovered > 0" class="spp-warn">
        Heads-up: {{ meta.uncovered }} unit(s) couldn't get a dedicated study day before the exam — they're folded into revision. Add more days or hours to cover them.
      </p>
    </section>

    <!-- Empty state -->
    <div v-if="!schedule.plans.length" class="spp-empty card">
      <div class="spp-empty-ic">🗓️</div>
      <p>No study plans yet. Pick a subject and exam date above to generate your first schedule.</p>
    </div>

    <!-- Plans -->
    <section v-for="p in schedule.plans" :key="p.id" class="card spp-plan-card">
      <div class="spp-plan-hd">
        <div class="spp-plan-id">
          <span class="spp-plan-ic" :style="{ background: p.subject?.color || 'var(--t-accent)' }">{{ (p.subject?.name || '?')[0] }}</span>
          <div>
            <div class="spp-plan-name">{{ p.subject?.name }}</div>
            <div class="spp-plan-meta">Exam {{ fmt(p.exam_date) }} · {{ p.daily_hours }}h/day</div>
          </div>
        </div>
        <div class="spp-plan-actions">
          <button class="btn-secondary spp-sm" @click="schedule.recompute(p.subject_id)">Re-plan</button>
          <button class="spp-del" title="Delete plan" @click="schedule.remove(p.subject_id)"><Trash2 :size="15" /></button>
        </div>
      </div>

      <!-- stats -->
      <div class="spp-stats">
        <div class="spp-stat"><b>{{ p.stats.daysToExam }}</b><span>days to exam</span></div>
        <div class="spp-stat"><b>{{ p.stats.coverage }}%</b><span>coverage</span></div>
        <div class="spp-stat"><b>{{ p.stats.compliance }}%</b><span>compliance</span></div>
        <div class="spp-stat spp-stat-accent"><b>{{ p.stats.readiness }}%</b><span>exam readiness</span></div>
      </div>
      <div class="spp-readybar"><i :style="{ width: p.stats.readiness + '%' }" /></div>

      <!-- day list -->
      <div class="spp-days">
        <div v-for="d in p.days" :key="d.id" class="spp-day" :class="{ today: d.day_date === todayIso, done: d.status==='done', missed: d.status==='missed', review: d.status==='needs_review' }">
          <div class="spp-day-date">
            <b>{{ dayNum(d.day_date) }}</b><span>{{ mon(d.day_date) }}</span>
            <em v-if="d.day_date === todayIso">Today</em>
          </div>
          <div class="spp-day-body">
            <div class="spp-day-task">
              <span class="spp-mode" :class="d.mode">{{ d.mode }}</span>
              {{ d.task }}
            </div>
            <div class="spp-day-sub">
              {{ d.est_minutes }} min<template v-if="d.self_test"> · self-test</template>
              <span v-if="d.status !== 'pending'" class="spp-status" :class="d.status">{{ statusLabel(d.status) }}</span>
            </div>
          </div>
          <div class="spp-day-ctl">
            <button class="spp-chip ok" :class="{ on: d.status==='done' }" title="Mark done" @click="toggle(d, 'done')"><Check :size="14" /></button>
            <button class="spp-chip rv" :class="{ on: d.status==='needs_review' }" title="Needs review" @click="toggle(d, 'needs_review')"><RotateCcw :size="14" /></button>
            <input class="spp-move" type="date" :value="d.day_date" :title="'Move to another day'" @change="(e) => schedule.moveDay(d.id, e.target.value)" />
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { Check, RotateCcw, Trash2 } from '@lucide/vue'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '@/stores/auth'
import { useScheduleStore } from '@/stores/schedule'
import api from '@/services/api'

const auth = useAuthStore()
const schedule = useScheduleStore()
const toast = useToast()

const subjects = ref([])
const form = reactive({ subjectId: '', examDate: '', dailyHours: 2 })
const busy = ref(false)
const error = ref('')
const meta = ref(null)

const todayIso = new Date().toISOString().slice(0, 10)
const tomorrow = new Date(Date.now() + 86400000).toISOString().slice(0, 10)
const canGenerate = computed(() => form.subjectId && form.examDate)

async function loadSubjects() {
  const grade = auth.user?.gradeCode
  if (!grade) return
  try { subjects.value = (await api.get(`/grades/${grade}/subjects`)).data } catch { subjects.value = [] }
}

async function onGenerate() {
  error.value = ''
  meta.value = null
  busy.value = true
  try {
    const res = await schedule.generate({ subjectId: form.subjectId, examDate: form.examDate, dailyHours: form.dailyHours })
    meta.value = res.meta
    toast.add({ severity: 'success', summary: 'Plan ready', detail: `${res.meta.studyDays} study days + ${res.meta.revisionDays} revision days`, life: 3000 })
  } catch (e) {
    error.value = e.message || 'Could not build the plan'
  } finally {
    busy.value = false
  }
}

function toggle(day, status) {
  schedule.setDayStatus(day.id, day.status === status ? 'pending' : status)
}

const statusLabel = (s) => ({ done: 'Done', missed: 'Missed', needs_review: 'Needs review' }[s] || s)
const fmt = (iso) => new Date(iso).toLocaleDateString('en-PK', { dateStyle: 'medium' })
const dayNum = (iso) => new Date(iso).getUTCDate()
const mon = (iso) => new Date(iso).toLocaleDateString('en-US', { month: 'short' })

onMounted(() => { loadSubjects(); schedule.fetchAll() })
</script>

<style scoped>
.sp-plan { max-width: 1100px; margin: 0 auto; padding: 8px 4px 40px; }
.spp-head { margin-bottom: 1.25rem; }
.spp-title { font-size: 1.7rem; font-weight: 800; color: var(--t-text1); margin: 0; }
.spp-sub { font-size: 0.88rem; color: var(--t-text3); margin: 0.3rem 0 0; max-width: 680px; line-height: 1.5; }

.spp-form { padding: 1.25rem 1.5rem; margin-bottom: 1.25rem; }
.spp-form-title { font-size: 1rem; font-weight: 700; color: var(--t-text1); margin: 0 0 1rem; }
.spp-form-grid { display: grid; grid-template-columns: 1.4fr 1fr 0.9fr auto; gap: 0.85rem; align-items: end; }
@media (max-width: 760px) { .spp-form-grid { grid-template-columns: 1fr 1fr; } }
.spp-form-action { display: flex; }
.spp-form-action .btn-primary { width: 100%; justify-content: center; }
.spp-error { color: var(--t-danger); font-size: 0.85rem; font-weight: 600; margin: 0.85rem 0 0; }
.spp-warn { color: var(--t-warn); font-size: 0.82rem; margin: 0.85rem 0 0; line-height: 1.45; }

.spp-empty { padding: 3rem 1rem; text-align: center; color: var(--t-text3); }
.spp-empty-ic { font-size: 2.5rem; margin-bottom: 0.5rem; }

.spp-plan-card { padding: 1.25rem 1.5rem; margin-bottom: 1.1rem; }
.spp-plan-hd { display: flex; align-items: center; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
.spp-plan-id { display: flex; align-items: center; gap: 0.75rem; }
.spp-plan-ic { width: 42px; height: 42px; border-radius: 13px; display: grid; place-items: center; color: #fff; font-weight: 800; font-size: 1.05rem; }
.spp-plan-name { font-size: 1.05rem; font-weight: 700; color: var(--t-text1); }
.spp-plan-meta { font-size: 0.76rem; color: var(--t-text3); }
.spp-plan-actions { display: flex; align-items: center; gap: 0.5rem; }
.spp-sm { padding: 0.45rem 0.9rem; font-size: 0.8rem; }
.spp-del { width: 34px; height: 34px; border-radius: 9px; display: grid; place-items: center; background: var(--t-hover); border: 1px solid var(--t-border); color: var(--t-danger); cursor: pointer; }
.spp-del:hover { background: var(--t-danger-bg); }

.spp-stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.6rem; margin: 1.1rem 0 0.6rem; }
.spp-stat { background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 12px; padding: 0.6rem 0.8rem; text-align: center; }
.spp-stat b { display: block; font-size: 1.25rem; font-weight: 800; color: var(--t-text1); }
.spp-stat span { font-size: 0.68rem; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.04em; }
.spp-stat-accent b { color: var(--t-accent); }
.spp-readybar { height: 6px; border-radius: 99px; background: var(--t-hover); overflow: hidden; margin-bottom: 1rem; }
.spp-readybar i { display: block; height: 100%; border-radius: 99px; background: linear-gradient(90deg, var(--t-accent), var(--t-accent2)); transition: width 0.4s; }

.spp-days { display: flex; flex-direction: column; gap: 0.4rem; max-height: 460px; overflow-y: auto; padding-right: 0.25rem; }
.spp-day { display: flex; align-items: center; gap: 0.85rem; padding: 0.6rem 0.75rem; border-radius: 12px; border: 1px solid var(--t-border); background: var(--t-surface); }
.spp-day.today { border-color: var(--t-accent); box-shadow: 0 0 0 1px var(--t-accent) inset; }
.spp-day.done { opacity: 0.7; }
.spp-day.missed { border-color: color-mix(in srgb, var(--t-danger) 40%, transparent); }
.spp-day.review { border-color: color-mix(in srgb, var(--t-warn) 45%, transparent); }
.spp-day-date { width: 48px; flex-shrink: 0; text-align: center; line-height: 1.1; position: relative; }
.spp-day-date b { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); }
.spp-day-date span { display: block; font-size: 0.66rem; text-transform: uppercase; color: var(--t-text3); }
.spp-day-date em { display: block; font-style: normal; font-size: 0.58rem; font-weight: 800; color: var(--t-accent); text-transform: uppercase; letter-spacing: 0.04em; }
.spp-day-body { flex: 1; min-width: 0; }
.spp-day-task { font-size: 0.88rem; font-weight: 600; color: var(--t-text1); display: flex; align-items: center; gap: 0.5rem; }
.spp-mode { font-size: 0.6rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.05em; padding: 0.1rem 0.45rem; border-radius: 99px; }
.spp-mode.study { background: color-mix(in srgb, var(--t-accent) 14%, transparent); color: var(--t-accent); }
.spp-mode.revision { background: color-mix(in srgb, var(--t-gold) 18%, transparent); color: var(--t-gold); }
.spp-day-sub { font-size: 0.72rem; color: var(--t-text3); margin-top: 0.15rem; display: flex; align-items: center; gap: 0.5rem; }
.spp-status { font-weight: 700; }
.spp-status.done { color: var(--t-success); }
.spp-status.missed { color: var(--t-danger); }
.spp-status.needs_review { color: var(--t-warn); }
.spp-day-ctl { display: flex; align-items: center; gap: 0.35rem; flex-shrink: 0; }
.spp-chip { width: 30px; height: 30px; border-radius: 8px; display: grid; place-items: center; background: var(--t-hover); border: 1px solid var(--t-border); color: var(--t-text3); cursor: pointer; transition: all 0.15s; }
.spp-chip.ok.on { background: var(--t-success-bg); color: var(--t-success); border-color: color-mix(in srgb, var(--t-success) 40%, transparent); }
.spp-chip.rv.on { background: rgba(180,131,9,0.14); color: var(--t-warn); border-color: color-mix(in srgb, var(--t-warn) 40%, transparent); }
.spp-move { width: 34px; height: 30px; border-radius: 8px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text3); cursor: pointer; padding: 0 2px; font-size: 0; }
.spp-move::-webkit-calendar-picker-indicator { opacity: 0.6; }
</style>
