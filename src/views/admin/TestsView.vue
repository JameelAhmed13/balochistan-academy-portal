<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📝</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Test Management</div>
        <div class="ag-banner-sub">Create, schedule and monitor tests</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">
          <span style="width:8px; height:8px; border-radius:50%; background:#10b981; display:inline-block;"></span>
          {{ activeTests }} Active
        </span>
        <span class="ag-banner-stat">
          <Calendar class="w-3.5 h-3.5" /> {{ tests.length }} Total
        </span>
      </div>
      <div class="ag-banner-actions">
        <RouterLink to="/app/admin" class="btn-secondary" style="gap:0.4rem;">
          <ArrowLeft class="w-4 h-4" /> Admin
        </RouterLink>
        <button @click="addModal = true" class="btn-primary text-sm">
          <Plus class="w-4 h-4" /> Create Test
        </button>
      </div>
    </div>

    <!-- Stats Row -->
    <div style="display:grid; grid-template-columns:repeat(4,1fr); gap:0.75rem;">
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background:rgba(16,185,129,0.1);">
          <span style="width:10px; height:10px; border-radius:50%; background:#10b981; display:block;"></span>
        </div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ activeTests }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Active Tests</div>
        </div>
      </div>
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background:rgba(245,158,11,0.1);">
          <Calendar class="w-5 h-5" style="color:#f59e0b;" />
        </div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ scheduledTests }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Scheduled</div>
        </div>
      </div>
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background:rgba(99,102,241,0.1);">
          <Clock class="w-5 h-5" style="color:#6366f1;" />
        </div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ completedTests }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Completed</div>
        </div>
      </div>
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background:var(--t-acc-alpha-sm);">
          <Users class="w-5 h-5" style="color:var(--t-accent);" />
        </div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">
            {{ totalStudents.toLocaleString() }}
          </div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Total Students</div>
        </div>
      </div>
    </div>

    <!-- View toggle row -->
    <div class="ag-card">
      <div style="padding:0.85rem 1.25rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; justify-content:space-between;">
        <span style="font-size:0.82rem; color:var(--t-text2);">
          <strong style="color:var(--t-text1);">{{ tests.length }}</strong> tests total
        </span>
        <div class="ag-view-toggle">
          <button class="ag-view-btn" :class="{ active: view === 'grid' }" @click="view = 'grid'" title="Grid view">
            <LayoutGrid class="w-3.5 h-3.5" />
          </button>
          <button class="ag-view-btn" :class="{ active: view === 'list' }" @click="view = 'list'" title="List view">
            <List class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      <!-- GRID VIEW -->
      <div v-if="view === 'grid'"
           style="padding:1.25rem; display:grid; grid-template-columns:repeat(auto-fill, minmax(300px, 1fr)); gap:1rem;">
        <div v-for="t in tests" :key="t.id" class="ag-grid-card" style="padding:0; overflow:hidden;">
          <!-- Colored top stripe -->
          <div :class="typeGrad(t.type)" style="height:5px; width:100%;"></div>
          <div style="padding:1rem 1.1rem;">
            <!-- Title + status -->
            <div class="flex items-start justify-between gap-2" style="margin-bottom:0.6rem;">
              <div style="font-weight:600; font-size:0.9rem; color:var(--t-text1); line-height:1.35; flex:1;">{{ t.title }}</div>
              <span :class="statusClass(t.status)" style="flex-shrink:0;">{{ t.status }}</span>
            </div>

            <!-- Type badge + subjects -->
            <div class="flex items-center gap-1.5 flex-wrap" style="margin-bottom:0.75rem;">
              <span class="badge-indigo" style="font-size:0.7rem;">{{ t.type }}</span>
              <span style="font-size:0.7rem; color:var(--t-text3);">{{ t.subjects }}</span>
            </div>

            <!-- Dates -->
            <div style="display:flex; flex-direction:column; gap:0.3rem; margin-bottom:0.8rem;">
              <div class="flex items-center gap-1.5" style="font-size:0.75rem; color:var(--t-text3);">
                <Calendar class="w-3 h-3" style="flex-shrink:0;" />
                <span>Opens: <span style="color:var(--t-text2);">{{ t.opens }}</span></span>
              </div>
              <div class="flex items-center gap-1.5" style="font-size:0.75rem; color:var(--t-text3);">
                <Clock class="w-3 h-3" style="flex-shrink:0;" />
                <span>Closes: <span style="color:var(--t-text2);">{{ t.closes }}</span></span>
              </div>
            </div>

            <!-- Footer: students + actions -->
            <div class="flex items-center justify-between" style="border-top:1px solid var(--t-border); padding-top:0.65rem;">
              <div class="flex items-center gap-1" style="font-size:0.75rem; color:var(--t-text3);">
                <Users class="w-3.5 h-3.5" />
                <span>{{ t.students }} students</span>
              </div>
              <div class="flex gap-1">
                <button class="btn-ghost" style="padding:0.3rem 0.5rem;" title="Edit">
                  <Pencil class="w-3.5 h-3.5" />
                </button>
                <button class="btn-ghost" style="padding:0.3rem 0.5rem; color:#ef4444;" title="Delete">
                  <Trash2 class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- LIST VIEW -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Type</th>
              <th>Subjects</th>
              <th>Opens</th>
              <th>Closes</th>
              <th>Status</th>
              <th>Students</th>
              <th style="width:90px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in tests" :key="t.id">
              <td style="font-weight:500; color:var(--t-text1); max-width:240px;">
                <div style="overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">{{ t.title }}</div>
              </td>
              <td><span class="badge-indigo">{{ t.type }}</span></td>
              <td style="font-size:0.8rem; color:var(--t-text2);">{{ t.subjects }}</td>
              <td class="ag-table-muted">{{ t.opens }}</td>
              <td class="ag-table-muted">{{ t.closes }}</td>
              <td><span :class="statusClass(t.status)">{{ t.status }}</span></td>
              <td>
                <div class="flex items-center gap-1" style="font-size:0.8rem; color:var(--t-text2);">
                  <Users class="w-3.5 h-3.5" style="color:var(--t-text3);" />
                  {{ t.students }}
                </div>
              </td>
              <td>
                <div class="flex gap-1">
                  <button class="btn-ghost" style="padding:0.3rem;" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create Test Modal -->
    <Dialog v-model:visible="addModal" header="Create Test" :modal="true" :style="{ width: '520px' }">
      <form class="space-y-4" @submit.prevent="submitCreate">

        <!-- Title -->
        <div>
          <label class="label">Test Title</label>
          <input v-model="newTest.title" type="text" class="input" placeholder="e.g. June Monthly Combined Test" required />
        </div>

        <!-- Type -->
        <div>
          <label class="label">Test Type</label>
          <select v-model="newTest.type" class="input">
            <option>Monthly Combined</option>
            <option>Daily Challenge</option>
            <option>Teacher Assigned</option>
            <option>Objective Paper</option>
            <option>Subjective Paper</option>
          </select>
        </div>

        <!-- Subjects multi-select -->
        <div>
          <label class="label">Subjects</label>
          <div style="border:1px solid var(--t-input-border); border-radius:12px; padding:0.75rem;
                       background:var(--t-input-bg); display:grid; grid-template-columns:1fr 1fr; gap:0.5rem; max-height:180px; overflow-y:auto;">
            <label v-for="s in SUBJECTS" :key="s.id"
                   class="flex items-center gap-2"
                   style="font-size:0.82rem; color:var(--t-text2); cursor:pointer;">
              <input type="checkbox" :value="s.name" v-model="newTest.subjects"
                     style="accent-color:var(--t-accent); width:14px; height:14px;" />
              {{ s.icon }} {{ s.name }}
            </label>
          </div>
        </div>

        <!-- Dates -->
        <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
          <div>
            <label class="label" style="font-size:0.75rem;">Opening Date &amp; Time</label>
            <input v-model="newTest.opens" type="datetime-local" class="input" style="font-size:0.82rem;" />
          </div>
          <div>
            <label class="label" style="font-size:0.75rem;">Closing Date &amp; Time</label>
            <input v-model="newTest.closes" type="datetime-local" class="input" style="font-size:0.82rem;" />
          </div>
        </div>

        <!-- Questions + Time Limit -->
        <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
          <div>
            <label class="label" style="font-size:0.75rem;">Number of Questions</label>
            <input v-model.number="newTest.questionCount" type="number" min="1" max="200" class="input" style="font-size:0.82rem;" placeholder="e.g. 40" />
          </div>
          <div>
            <label class="label" style="font-size:0.75rem;">Time Limit (minutes)</label>
            <input v-model.number="newTest.timeLimit" type="number" min="5" max="360" class="input" style="font-size:0.82rem;" placeholder="e.g. 90" />
          </div>
        </div>

        <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
          <button type="submit" class="btn-primary" style="flex:1; justify-content:center;">
            <Plus class="w-4 h-4" /> Create Test
          </button>
          <button type="button" @click="addModal = false" class="btn-secondary">Cancel</button>
        </div>
      </form>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, Plus, Pencil, Trash2, LayoutGrid, List, Calendar, Clock, Users } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import { SUBJECTS } from '@/stores/content'

// ── View ──────────────────────────────────────────────────────────
const view = ref('grid')

// ── Mock data ─────────────────────────────────────────────────────
const tests = ref([
  {
    id: 1,
    title: 'June Monthly Combined Test',
    type: 'Monthly Combined',
    subjects: 'All Subjects',
    opens: '2026-06-01',
    closes: '2026-06-30',
    status: 'Active',
    students: 312,
  },
  {
    id: 2,
    title: 'Physics Chapter 2 — Motion',
    type: 'Teacher Assigned',
    subjects: 'Physics',
    opens: '2026-06-12',
    closes: '2026-06-14',
    status: 'Active',
    students: 87,
  },
  {
    id: 3,
    title: 'Biology Mid-Term Paper',
    type: 'Subjective Paper',
    subjects: 'Biology',
    opens: '2026-06-20',
    closes: '2026-06-20',
    status: 'Scheduled',
    students: 0,
  },
  {
    id: 4,
    title: 'Daily Challenge — Chemistry',
    type: 'Daily Challenge',
    subjects: 'Chemistry',
    opens: '2026-05-28',
    closes: '2026-05-28',
    status: 'Completed',
    students: 145,
  },
  {
    id: 5,
    title: 'Mathematics Objective Paper',
    type: 'Objective Paper',
    subjects: 'Mathematics',
    opens: '2026-07-01',
    closes: '2026-07-01',
    status: 'Scheduled',
    students: 0,
  },
])

// ── Computed stats ────────────────────────────────────────────────
const activeTests    = computed(() => tests.value.filter(t => t.status === 'Active').length)
const scheduledTests = computed(() => tests.value.filter(t => t.status === 'Scheduled').length)
const completedTests = computed(() => tests.value.filter(t => t.status === 'Completed').length)
const totalStudents  = computed(() => tests.value.reduce((a, t) => a + t.students, 0))

// ── Helpers ───────────────────────────────────────────────────────
function statusClass(s) {
  return { Active: 'badge-green', Scheduled: 'badge-amber', Completed: 'badge-indigo' }[s] || 'badge-indigo'
}

function typeGrad(type) {
  const map = {
    'Monthly Combined': 'grad-violet',
    'Daily Challenge':  'grad-amber',
    'Teacher Assigned': 'grad-blue',
    'Objective Paper':  'grad-teal',
    'Subjective Paper': 'grad-rose',
  }
  return map[type] || 'grad-violet'
}

// ── Create modal ──────────────────────────────────────────────────
const addModal = ref(false)
const newTest = ref({
  title: '',
  type: 'Monthly Combined',
  subjects: [],
  opens: '',
  closes: '',
  questionCount: null,
  timeLimit: null,
})

function submitCreate() {
  addModal.value = false
  newTest.value = { title: '', type: 'Monthly Combined', subjects: [], opens: '', closes: '', questionCount: null, timeLimit: null }
}
</script>
