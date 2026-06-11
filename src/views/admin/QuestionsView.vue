<template>
  <div class="animate-fade-in space-y-5">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">❓</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Question Bank</div>
        <div class="ag-banner-sub">Manage objective &amp; subjective questions</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">
          <span>📋</span> {{ totalObjective.toLocaleString() }} Objective
        </span>
        <span class="ag-banner-stat">
          <span>✍️</span> {{ totalSubjective.toLocaleString() }} Subjective
        </span>
        <span class="ag-banner-stat">
          <span>🎯</span> {{ (totalObjective + totalSubjective).toLocaleString() }} Total
        </span>
      </div>
      <div class="ag-banner-actions">
        <button @click="addModal = true" class="btn-primary text-sm">
          <Plus class="w-4 h-4" /> Add Question
        </button>
      </div>
    </div>

    <!-- Stats Row -->
    <div class="grid grid-cols-2 gap-3" style="grid-template-columns: repeat(4, 1fr);">
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center text-lg flex-shrink-0"
             style="background: var(--t-acc-alpha-sm); color: var(--t-accent);">📋</div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ totalObjective.toLocaleString() }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Objective Questions</div>
        </div>
      </div>
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center text-lg flex-shrink-0"
             style="background: rgba(16,185,129,0.1); color: #10b981;">✍️</div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ totalSubjective.toLocaleString() }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Subjective Questions</div>
        </div>
      </div>
      <div class="ag-card ag-card-body">
        <div style="font-size:0.68rem; font-weight:700; text-transform:uppercase; letter-spacing:0.06em; color:var(--t-text3); margin-bottom:0.6rem;">By Difficulty</div>
        <div class="flex gap-2 flex-wrap">
          <span class="badge-green" style="font-size:0.75rem;">Easy {{ diffStats.Easy }}</span>
          <span class="badge-amber" style="font-size:0.75rem;">Medium {{ diffStats.Medium }}</span>
          <span class="badge-red" style="font-size:0.75rem;">Hard {{ diffStats.Hard }}</span>
        </div>
      </div>
      <div class="ag-card ag-card-body">
        <div style="font-size:0.68rem; font-weight:700; text-transform:uppercase; letter-spacing:0.06em; color:var(--t-text3); margin-bottom:0.6rem;">Subjects Covered</div>
        <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ SUBJECTS.length }}</div>
        <div style="font-size:0.72rem; color:var(--t-text3);">all subjects loaded</div>
      </div>
    </div>

    <!-- Filter Bar + View Toggle -->
    <div class="ag-card">
      <div class="ag-filter-bar">
        <div class="ag-filter-group">
          <label class="ag-filter-label">Subject</label>
          <select v-model="filterSubject" class="input" style="width:160px;">
            <option :value="null">All Subjects</option>
            <option v-for="s in SUBJECTS" :key="s.id" :value="s.id">{{ s.icon }} {{ s.name }}</option>
          </select>
        </div>
        <div class="ag-filter-group">
          <label class="ag-filter-label">Type</label>
          <select v-model="filterType" class="input" style="width:140px;">
            <option value="">All Types</option>
            <option>Objective</option>
            <option>Subjective</option>
          </select>
        </div>
        <div class="ag-filter-group">
          <label class="ag-filter-label">Difficulty</label>
          <select v-model="filterDiff" class="input" style="width:130px;">
            <option value="">All Levels</option>
            <option>Easy</option>
            <option>Medium</option>
            <option>Hard</option>
          </select>
        </div>
        <div class="ag-filter-group" style="flex:1; min-width:200px;">
          <label class="ag-filter-label">Search</label>
          <div style="position:relative;">
            <Search class="w-3.5 h-3.5" style="position:absolute; left:0.6rem; top:50%; transform:translateY(-50%); color:var(--t-text3); pointer-events:none;" />
            <input v-model="search" type="text" class="input" style="padding-left:2rem;" placeholder="Search question text…" />
          </div>
        </div>
        <div class="ag-filter-group" style="justify-content:flex-end;">
          <label class="ag-filter-label" style="opacity:0;">–</label>
          <button v-if="hasFilters" @click="clearFilters" class="btn-secondary" style="gap:0.4rem; white-space:nowrap;">
            <X class="w-3.5 h-3.5" /> Clear
          </button>
        </div>
        <div class="ag-filter-group" style="margin-left:auto; justify-content:flex-end;">
          <label class="ag-filter-label" style="opacity:0;">–</label>
          <div class="ag-view-toggle">
            <button class="ag-view-btn" :class="{ active: view === 'list' }" @click="view = 'list'" title="List view">
              <List class="w-3.5 h-3.5" />
            </button>
            <button class="ag-view-btn" :class="{ active: view === 'grid' }" @click="view = 'grid'" title="Grid view">
              <LayoutGrid class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      <!-- Result count row -->
      <div style="padding:0.5rem 1.5rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; gap:0.5rem;">
        <Filter class="w-3 h-3" style="color:var(--t-text3);" />
        <span style="font-size:0.75rem; color:var(--t-text3);">
          Showing <strong style="color:var(--t-text1);">{{ Math.min(displayedQuestions.length, 50) }}</strong>
          of <strong style="color:var(--t-text1);">{{ displayedQuestions.length }}</strong> questions
        </span>
      </div>

      <!-- LIST VIEW -->
      <div v-if="view === 'list'" class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:40px;">#</th>
              <th>Question</th>
              <th>Subject</th>
              <th>Type</th>
              <th>Difficulty</th>
              <th>Cognitive Level</th>
              <th style="width:80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(q, i) in displayedQuestions.slice(0, 50)" :key="q.id">
              <td class="ag-table-muted" style="text-align:center;">{{ i + 1 }}</td>
              <td style="max-width:320px;">
                <div style="overflow:hidden; text-overflow:ellipsis; white-space:nowrap; color:var(--t-text1);"
                     :title="q.stem">
                  {{ q.stem.length > 60 ? q.stem.slice(0, 60) + '…' : q.stem }}
                </div>
              </td>
              <td>
                <span style="font-size:0.75rem; color:var(--t-text2);">{{ q.subject }}</span>
              </td>
              <td>
                <span :class="q.correct !== undefined ? 'badge-indigo' : 'badge-purple'">
                  {{ q.correct !== undefined ? 'Objective' : 'Subjective' }}
                </span>
              </td>
              <td>
                <span :class="diffClass(q.difficulty)">{{ q.difficulty }}</span>
              </td>
              <td>
                <span style="font-size:0.78rem; color:var(--t-text3);">{{ q.cognitiveLevel }}</span>
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
            <tr v-if="displayedQuestions.length === 0">
              <td colspan="7">
                <div class="ag-empty">
                  <div class="ag-empty-icon">🔍</div>
                  <p>No questions match your filters.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="displayedQuestions.length > 50"
             style="padding:0.75rem; text-align:center; font-size:0.75rem; color:var(--t-text3); border-top:1px solid var(--t-border);">
          Showing first 50 of {{ displayedQuestions.length }} questions — refine filters to narrow results
        </div>
      </div>

      <!-- GRID VIEW -->
      <div v-else style="padding:1.25rem; display:grid; grid-template-columns:repeat(2, 1fr); gap:1rem;">
        <div v-for="q in displayedQuestions.slice(0, 50)" :key="q.id" class="ag-grid-card">
          <!-- Card Header -->
          <div class="flex items-start justify-between gap-2" style="margin-bottom:0.75rem;">
            <div class="flex items-center gap-1.5 flex-wrap">
              <span :class="q.correct !== undefined ? 'badge-indigo' : 'badge-purple'">
                {{ q.correct !== undefined ? 'Objective' : 'Subjective' }}
              </span>
              <span style="font-size:0.7rem; padding:0.2rem 0.55rem; border-radius:99px; background:var(--t-hover2); color:var(--t-text2);">
                {{ q.subject }}
              </span>
            </div>
            <span :class="diffClass(q.difficulty)" style="flex-shrink:0;">{{ q.difficulty }}</span>
          </div>

          <!-- Stem -->
          <p style="font-size:0.83rem; color:var(--t-text1); line-height:1.5; margin-bottom:0.7rem;
                     display:-webkit-box; -webkit-box-orient:vertical; -webkit-line-clamp:3; overflow:hidden;">
            {{ q.stem }}
          </p>

          <!-- Options Preview (Objective only) -->
          <div v-if="q.options" style="margin-bottom:0.7rem; display:flex; flex-direction:column; gap:0.3rem;">
            <div v-for="(opt, idx) in q.options.slice(0, 2)" :key="idx"
                 style="font-size:0.72rem; color:var(--t-text3); display:flex; align-items:center; gap:0.4rem;">
              <span style="width:16px; height:16px; border-radius:4px; border:1px solid var(--t-border);
                           display:flex; align-items:center; justify-content:center; flex-shrink:0; font-size:0.6rem; font-weight:700;"
                    :style="idx === q.correct ? 'background:rgba(16,185,129,0.15); border-color:#10b981; color:#10b981;' : ''">
                {{ 'ABCD'[idx] }}
              </span>
              <span style="overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">{{ opt }}</span>
            </div>
            <div v-if="q.options.length > 2"
                 style="font-size:0.68rem; color:var(--t-text3);">+ {{ q.options.length - 2 }} more options</div>
          </div>

          <!-- Footer -->
          <div class="flex items-center justify-between" style="border-top:1px solid var(--t-border); padding-top:0.6rem; margin-top:auto;">
            <span style="font-size:0.7rem; color:var(--t-text3);">{{ q.cognitiveLevel }}</span>
            <div class="flex gap-1">
              <button class="btn-ghost" style="padding:0.25rem 0.5rem;" title="Edit">
                <Pencil class="w-3.5 h-3.5" />
              </button>
              <button class="btn-ghost" style="padding:0.25rem 0.5rem; color:#ef4444;" title="Delete">
                <Trash2 class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        </div>

        <!-- Empty state (grid) -->
        <div v-if="displayedQuestions.length === 0"
             style="grid-column:1/-1;">
          <div class="ag-empty">
            <div class="ag-empty-icon">🔍</div>
            <p>No questions match your filters.</p>
          </div>
        </div>
      </div>

      <div v-if="view === 'grid' && displayedQuestions.length > 50"
           style="padding:0.75rem; text-align:center; font-size:0.75rem; color:var(--t-text3); border-top:1px solid var(--t-border);">
        Showing first 50 of {{ displayedQuestions.length }} questions — refine filters to narrow results
      </div>
    </div>

    <!-- Add Question Modal -->
    <Dialog v-model:visible="addModal" header="Add Question" :modal="true" :style="{ width: '560px' }">
      <form class="space-y-4" @submit.prevent="submitAdd">

        <!-- Type selector -->
        <div>
          <label class="label">Question Type</label>
          <div class="flex gap-2">
            <button v-for="t in ['Objective', 'Subjective']" :key="t" type="button"
              @click="newQ.qtype = t"
              :class="['btn-secondary text-sm flex-1 justify-center', newQ.qtype === t ? 'ring-2' : '']"
              :style="newQ.qtype === t ? 'background:var(--t-acc-alpha-md); color:var(--t-accent); border-color:var(--t-accent);' : ''">
              {{ t === 'Objective' ? '📋' : '✍️' }} {{ t }}
            </button>
          </div>
        </div>

        <!-- Subject -->
        <div>
          <label class="label">Subject</label>
          <select v-model="newQ.subject" class="input" required>
            <option value="" disabled>Select a subject</option>
            <option v-for="s in SUBJECTS" :key="s.id" :value="s.name">{{ s.icon }} {{ s.name }}</option>
          </select>
        </div>

        <!-- Stem -->
        <div>
          <label class="label">Question Stem</label>
          <textarea v-model="newQ.stem" class="input" style="min-height:90px; resize:vertical;"
                    placeholder="Enter the full question text…" required></textarea>
        </div>

        <!-- Objective options -->
        <template v-if="newQ.qtype === 'Objective'">
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div v-for="(_, i) in 4" :key="i">
              <label class="label" style="font-size:0.75rem;">Option {{ 'ABCD'[i] }}</label>
              <input v-model="newQ.options[i]" type="text" class="input" style="font-size:0.82rem;"
                     :placeholder="'Option ' + 'ABCD'[i]" />
            </div>
          </div>
          <div>
            <label class="label" style="font-size:0.75rem;">Correct Answer</label>
            <select v-model="newQ.correct" class="input" style="font-size:0.82rem;">
              <option v-for="(_, i) in 4" :key="i" :value="i">{{ 'ABCD'[i] }} — {{ newQ.options[i] || '(empty)' }}</option>
            </select>
          </div>
        </template>

        <!-- Difficulty + Cognitive -->
        <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
          <div>
            <label class="label" style="font-size:0.75rem;">Difficulty</label>
            <select v-model="newQ.difficulty" class="input" style="font-size:0.82rem;">
              <option>Easy</option>
              <option>Medium</option>
              <option>Hard</option>
            </select>
          </div>
          <div>
            <label class="label" style="font-size:0.75rem;">Cognitive Level</label>
            <select v-model="newQ.cognitiveLevel" class="input" style="font-size:0.82rem;">
              <option>Knowledge</option>
              <option>Understanding</option>
              <option>Applying</option>
            </select>
          </div>
        </div>

        <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
          <button type="submit" class="btn-primary" style="flex:1; justify-content:center;">
            <Plus class="w-4 h-4" /> Add Question
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
import { ArrowLeft, Plus, Pencil, Trash2, LayoutGrid, List, Search, X, Filter } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import { SUBJECTS, useContentStore } from '@/stores/content'

const content = useContentStore()

// ── Filters ──────────────────────────────────────────────────────
const search        = ref('')
const filterSubject = ref(null)
const filterType    = ref('')
const filterDiff    = ref('')
const view          = ref('list')

const hasFilters = computed(() => filterSubject.value || filterType.value || filterDiff.value || search.value)

function clearFilters() {
  search.value        = ''
  filterSubject.value = null
  filterType.value    = ''
  filterDiff.value    = ''
}

// ── Stats ─────────────────────────────────────────────────────────
const totalObjective  = computed(() => Object.values(content.objectiveBank).reduce((a, b) => a + b.length, 0))
const totalSubjective = computed(() => Object.values(content.subjectiveBank).reduce((a, b) => a + b.length, 0))

const diffStats = computed(() => {
  const all = [
    ...Object.values(content.objectiveBank).flat(),
    ...Object.values(content.subjectiveBank).flat(),
  ]
  return {
    Easy:   all.filter(q => q.difficulty === 'Easy').length,
    Medium: all.filter(q => q.difficulty === 'Medium').length,
    Hard:   all.filter(q => q.difficulty === 'Hard').length,
  }
})

// ── Filtered questions ────────────────────────────────────────────
const displayedQuestions = computed(() => {
  const sid = filterSubject.value

  const obj  = sid ? (content.objectiveBank[sid]  || []) : Object.values(content.objectiveBank).flat()
  const subj = sid ? (content.subjectiveBank[sid]  || []) : Object.values(content.subjectiveBank).flat()

  let combined = []
  const t = filterType.value
  if (!t || t === 'Objective')  combined.push(...obj)
  if (!t || t === 'Subjective') combined.push(...subj)

  if (filterDiff.value) combined = combined.filter(q => q.difficulty === filterDiff.value)
  if (search.value)     combined = combined.filter(q => q.stem.toLowerCase().includes(search.value.toLowerCase()))

  return combined
})

// ── Helpers ───────────────────────────────────────────────────────
function diffClass(d) {
  return { Easy: 'badge-green', Medium: 'badge-amber', Hard: 'badge-red' }[d] || 'badge-indigo'
}

// ── Add modal ─────────────────────────────────────────────────────
const addModal = ref(false)
const newQ = ref({
  qtype: 'Objective',
  subject: '',
  stem: '',
  options: ['', '', '', ''],
  correct: 0,
  difficulty: 'Medium',
  cognitiveLevel: 'Knowledge',
})

function submitAdd() {
  addModal.value = false
  newQ.value = { qtype: 'Objective', subject: '', stem: '', options: ['', '', '', ''], correct: 0, difficulty: 'Medium', cognitiveLevel: 'Knowledge' }
}
</script>
