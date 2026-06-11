<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Study Time Table</h2>
    <div class="tt-actions">
      <button @click="addSlot" class="tt-add-btn">+ Add Time Slot</button>
    </div>
    <div class="tt-grid">
      <div class="tt-day-header" v-for="day in days" :key="day">{{ day }}</div>
      <template v-for="(row, ri) in slots" :key="ri">
        <div v-for="day in days" :key="day" class="tt-cell"
          :class="{ 'tt-cell-filled': row[day]?.subject }"
          @click="edit(ri, day)">
          <div v-if="row[day]?.subject" class="tt-entry" :style="{ background: subjectColor(row[day].subject) }">
            <div class="tt-entry-time">{{ row[day].time }}</div>
            <div class="tt-entry-sub">{{ row[day].subject }}</div>
          </div>
          <div v-else class="tt-cell-empty">+</div>
        </div>
      </template>
    </div>
    <!-- Edit modal -->
    <div v-if="editCell" class="tt-modal-bg" @click.self="editCell = null">
      <div class="tt-modal">
        <div class="tt-modal-title">{{ editCell.day }}</div>
        <div class="tt-modal-form">
          <label>Time</label>
          <input v-model="editCell.time" type="text" placeholder="e.g. 8:00 AM - 9:00 AM" class="tt-field" />
          <label>Subject</label>
          <select v-model="editCell.subject" class="tt-field">
            <option value="">— Clear —</option>
            <option v-for="s in SUBJECTS" :key="s.id" :value="s.name">{{ s.name }}</option>
          </select>
        </div>
        <div class="tt-modal-actions">
          <button @click="editCell = null" class="tt-cancel-btn">Cancel</button>
          <button @click="saveEdit" class="tt-save-btn">Save</button>
        </div>
      </div>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { SUBJECTS } from '@/stores/content'
import PageFooter from '@/components/platform/PageFooter.vue'

const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
const COLORS = ['#c8e6c9', '#bbdefb', '#ffe0b2', '#e1bee7', '#ffcdd2', '#f0f4c3', '#b2ebf2']

const slots = ref([
  { Mon: { time: '8:00 AM', subject: 'Mathematics' }, Tue: null, Wed: { time: '8:00 AM', subject: 'Physics' }, Thu: null, Fri: { time: '8:00 AM', subject: 'Chemistry' }, Sat: null, Sun: null },
  { Mon: null, Tue: { time: '9:30 AM', subject: 'Biology' }, Wed: null, Thu: { time: '9:30 AM', subject: 'Urdu' }, Fri: null, Sat: null, Sun: null },
  { Mon: { time: '11:00 AM', subject: 'English' }, Tue: null, Wed: null, Thu: null, Fri: { time: '11:00 AM', subject: 'Urdu' }, Sat: null, Sun: null },
])
const editCell = ref(null)

function addSlot() {
  const row = {}; days.forEach(d => row[d] = null); slots.value.push(row)
}
function edit(ri, day) {
  editCell.value = { ri, day, time: slots.value[ri][day]?.time || '', subject: slots.value[ri][day]?.subject || '' }
}
function saveEdit() {
  const { ri, day, time, subject } = editCell.value
  slots.value[ri][day] = subject ? { time, subject } : null
  editCell.value = null
}
function subjectColor(name) {
  const idx = SUBJECTS.findIndex(s => s.name === name)
  return COLORS[idx % COLORS.length] || '#e8f5e9'
}
</script>

<style scoped>
.tt-actions { display: flex; gap: 0.5rem; }
.tt-add-btn { padding: 0.5rem 1.1rem; border-radius: 8px; background: #f57c00; color: white; border: none; font-weight: 700; font-size: 0.82rem; cursor: pointer; }
.tt-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 4px; overflow-x: auto; }
.tt-day-header { background: #4caf50; color: white; text-align: center; padding: 0.5rem; font-size: 0.78rem; font-weight: 700; border-radius: 6px; }
.tt-cell { min-height: 70px; border: 1px dashed var(--t-border); border-radius: 7px; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.tt-cell:hover { background: var(--t-hover); }
.tt-cell-empty { color: var(--t-text3); font-size: 1.2rem; }
.tt-entry { width: 100%; height: 100%; border-radius: 7px; padding: 0.4rem; }
.tt-entry-time { font-size: 0.6rem; color: #555; }
.tt-entry-sub { font-size: 0.72rem; font-weight: 700; color: #333; }
.tt-modal-bg { position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 100; display: flex; align-items: center; justify-content: center; }
.tt-modal { background: var(--t-surface); border-radius: 14px; padding: 1.5rem; width: 320px; }
.tt-modal-title { font-size: 1rem; font-weight: 700; color: var(--t-text1); margin-bottom: 1rem; }
.tt-modal-form { display: flex; flex-direction: column; gap: 0.5rem; }
.tt-modal-form label { font-size: 0.75rem; font-weight: 600; color: var(--t-text3); }
.tt-field { padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 7px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; }
.tt-modal-actions { display: flex; gap: 0.5rem; margin-top: 1rem; }
.tt-cancel-btn { flex: 1; padding: 0.55rem; border-radius: 7px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); cursor: pointer; font-size: 0.82rem; }
.tt-save-btn { flex: 1; padding: 0.55rem; border-radius: 7px; background: #4caf50; color: white; border: none; cursor: pointer; font-weight: 700; font-size: 0.82rem; }
</style>
