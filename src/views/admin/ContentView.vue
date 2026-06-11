<template>
  <div class="animate-fade-in space-y-5">

    <!-- Page Header -->
    <div class="ag-page-hd">
      <div class="ag-page-hd-left">
        <RouterLink to="/app/admin" class="btn-ghost">
          <ArrowLeft class="w-4 h-4" />
        </RouterLink>
        <h2 class="section-title">Content Management</h2>
      </div>
      <button class="btn-primary text-sm" @click="addModal = true">
        <Plus class="w-4 h-4" /> Add Subject
      </button>
    </div>

    <!-- Glass Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📚</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Content Management</div>
        <div class="ag-banner-sub">Subjects, books, units, topics and resources</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">📚 10 Subjects</span>
        <span class="ag-banner-stat">📑 240 Units</span>
      </div>
    </div>

    <!-- Controls Card -->
    <div class="ag-card">
      <!-- Medium filter tabs -->
      <div class="ag-tabs">
        <button
          v-for="tab in mediumTabs"
          :key="tab"
          :class="['ag-tab', { active: activeMedium === tab }]"
          style="cursor:pointer;"
          @click="activeMedium = tab"
        >{{ tab }}</button>
      </div>

      <!-- Filter Bar -->
      <div class="ag-filter-bar">
        <div class="ag-filter-group" style="flex:1;min-width:200px;">
          <label class="ag-filter-label">Search</label>
          <div style="position:relative;">
            <Search
              class="w-3.5 h-3.5"
              style="position:absolute;left:0.6rem;top:50%;transform:translateY(-50%);color:var(--t-text3);pointer-events:none;"
            />
            <input
              v-model="search"
              type="text"
              class="input"
              placeholder="Search subjects…"
              style="padding-left:2rem;"
            />
          </div>
        </div>
        <div class="ag-filter-group" style="align-self:flex-end;">
          <label class="ag-filter-label">View</label>
          <div class="ag-view-toggle">
            <button
              :class="['ag-view-btn', { active: view === 'grid' }]"
              style="cursor:pointer;"
              @click="view = 'grid'"
              title="Grid view"
            >
              <LayoutGrid class="w-3.5 h-3.5" />
            </button>
            <button
              :class="['ag-view-btn', { active: view === 'list' }]"
              style="cursor:pointer;"
              @click="view = 'list'"
              title="List view"
            >
              <List class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      <!-- GRID VIEW -->
      <div v-if="view === 'grid'" class="content-grid">
        <div
          v-for="s in filteredSubjects"
          :key="s.id"
          class="ag-grid-card subject-card"
        >
          <!-- Big icon -->
          <div class="subject-icon-wrap" :class="s.color">
            <span class="subject-icon">{{ s.icon }}</span>
          </div>
          <!-- Name -->
          <div class="subject-name">{{ s.name }}</div>
          <div class="subject-name-ur urdu">{{ s.nameUr }}</div>
          <!-- Medium badge -->
          <span :class="s.medium === 'urdu' ? 'badge-amber' : 'badge-blue'" style="align-self:center;margin-top:0.15rem;">
            {{ s.medium }}
          </span>
          <!-- Stats row -->
          <div class="subject-stats">
            <div class="subject-stat">
              <span class="subject-stat-val">{{ (content.units[s.id] || []).length }}</span>
              <span class="subject-stat-lbl">Units</span>
            </div>
            <div class="subject-stat-divider" />
            <div class="subject-stat">
              <span class="subject-stat-val">
                {{ (content.objectiveBank[s.id] || []).length + (content.subjectiveBank[s.id] || []).length }}
              </span>
              <span class="subject-stat-lbl">Questions</span>
            </div>
          </div>
          <!-- Action buttons -->
          <div class="subject-actions">
            <button class="btn-ghost subject-btn" style="flex:1;justify-content:center;">
              <Pencil class="w-3.5 h-3.5" /> Edit
            </button>
            <button class="btn-ghost subject-btn" style="color:#f87171;">
              <Trash2 class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        <!-- Empty state -->
        <div v-if="filteredSubjects.length === 0" class="ag-empty" style="grid-column:1/-1;">
          <div class="ag-empty-icon">📚</div>
          <p>No subjects found matching your filters.</p>
        </div>
      </div>

      <!-- LIST VIEW -->
      <div v-else class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Subject</th>
              <th>Medium</th>
              <th>Questions</th>
              <th>Units</th>
              <th>Resources</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(s, idx) in filteredSubjects" :key="s.id">
              <td class="ag-table-muted">{{ idx + 1 }}</td>
              <td>
                <div style="display:flex;align-items:center;gap:0.75rem;">
                  <div
                    :class="s.color"
                    style="width:32px;height:32px;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:1.1rem;flex-shrink:0;"
                  >{{ s.icon }}</div>
                  <div>
                    <div style="font-weight:600;color:var(--t-text1);font-size:0.875rem;">{{ s.name }}</div>
                    <div class="ag-table-muted urdu" style="margin-top:0.1rem;">{{ s.nameUr }}</div>
                  </div>
                </div>
              </td>
              <td>
                <span :class="s.medium === 'urdu' ? 'badge-amber' : 'badge-blue'">{{ s.medium }}</span>
              </td>
              <td style="color:var(--t-text2);">
                {{ (content.objectiveBank[s.id] || []).length + (content.subjectiveBank[s.id] || []).length }}
              </td>
              <td style="color:var(--t-text2);">{{ (content.units[s.id] || []).length }}</td>
              <td><span class="badge-green">8 resources</span></td>
              <td>
                <div style="display:flex;gap:0.25rem;">
                  <button class="btn-ghost" style="padding:0.35rem;">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button class="btn-ghost" style="padding:0.35rem;color:#f87171;">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredSubjects.length === 0">
              <td colspan="7">
                <div class="ag-empty">
                  <div class="ag-empty-icon">📚</div>
                  <p>No subjects found.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add Subject Modal -->
    <Dialog
      v-model:visible="addModal"
      header="Add Subject"
      :modal="true"
      :style="{ width: '460px' }"
    >
      <form class="space-y-4" @submit.prevent="submitAdd">
        <div>
          <label class="label">Subject Name (English)</label>
          <input v-model="form.nameEn" type="text" class="input" placeholder="e.g. Biology" required />
        </div>
        <div>
          <label class="label">Subject Name (Urdu)</label>
          <input
            v-model="form.nameUr"
            type="text"
            class="input urdu"
            style="text-align:right;"
            placeholder="حیاتیات"
          />
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="label">Grade</label>
            <input v-model.number="form.grade" type="number" class="input" value="9" min="1" max="12" />
          </div>
          <div>
            <label class="label">Medium</label>
            <select v-model="form.medium" class="input">
              <option value="english">English</option>
              <option value="urdu">Urdu</option>
            </select>
          </div>
        </div>
        <div>
          <label class="label">Icon (emoji)</label>
          <div class="icon-picker">
            <button
              v-for="em in emojiOptions"
              :key="em"
              type="button"
              :class="['icon-option', { selected: form.icon === em }]"
              @click="form.icon = em"
            >{{ em }}</button>
          </div>
        </div>
        <div style="display:flex;gap:0.5rem;padding-top:0.5rem;">
          <button type="submit" class="btn-primary" style="flex:1;justify-content:center;">
            <Plus class="w-4 h-4" /> Add Subject
          </button>
          <button type="button" class="btn-secondary" @click="addModal = false">Cancel</button>
        </div>
      </form>
    </Dialog>

  </div>
</template>

<script setup>
import { ref, computed, reactive } from 'vue'
import { ArrowLeft, Plus, Pencil, Trash2, LayoutGrid, List, Search } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import { SUBJECTS, useContentStore } from '@/stores/content'

const content = useContentStore()

const search      = ref('')
const view        = ref('grid')
const activeMedium = ref('All')
const addModal    = ref(false)

const mediumTabs = ['All', 'English', 'Urdu']

const emojiOptions = ['📚', '✍️', '🧬', '⚗️', '⚡', '📐', '🏛️', '🌍', '☪️', '💻', '📖', '🔬', '🗺️', '🎓']

const form = reactive({
  nameEn: '',
  nameUr: '',
  grade: 9,
  medium: 'english',
  icon: '📚',
})

const filteredSubjects = computed(() => {
  let list = SUBJECTS
  if (activeMedium.value !== 'All') {
    list = list.filter(s => s.medium === activeMedium.value.toLowerCase())
  }
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(s =>
      s.name.toLowerCase().includes(q) || s.nameUr.includes(search.value)
    )
  }
  return list
})

function submitAdd() {
  // In a real app: dispatch to store / API
  addModal.value = false
  form.nameEn = ''
  form.nameUr = ''
  form.grade  = 9
  form.medium = 'english'
  form.icon   = '📚'
}
</script>

<style scoped>
/* ── Grid Layout ── */
.content-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 1rem;
  padding: 1.25rem 1.5rem;
}

/* ── Subject Card ── */
.subject-card {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  cursor: default;
}
.subject-icon-wrap {
  width: 56px; height: 56px; border-radius: 16px;
  display: flex; align-items: center; justify-content: center;
  align-self: center;
}
.subject-icon { font-size: 1.75rem; }

.subject-name {
  font-size: 0.875rem; font-weight: 700; color: var(--t-text1);
  text-align: center; line-height: 1.3;
}
.subject-name-ur {
  font-size: 0.78rem; color: var(--t-text3);
  text-align: center; margin-top: -0.25rem;
}

.subject-stats {
  display: flex; align-items: center; justify-content: center; gap: 0.5rem;
  background: var(--t-hover);
  border-radius: 10px; padding: 0.45rem 0.5rem;
  margin-top: 0.15rem;
}
.subject-stat {
  display: flex; flex-direction: column; align-items: center; gap: 0.05rem;
}
.subject-stat-val {
  font-size: 0.875rem; font-weight: 700; color: var(--t-text1);
}
.subject-stat-lbl {
  font-size: 0.62rem; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.04em;
}
.subject-stat-divider {
  width: 1px; height: 24px; background: var(--t-border);
}

.subject-actions {
  display: flex; gap: 0.35rem; margin-top: 0.1rem;
}
.subject-btn {
  font-size: 0.75rem; padding: 0.35rem 0.5rem;
}

/* ── Icon Picker ── */
.icon-picker {
  display: flex; flex-wrap: wrap; gap: 0.35rem; padding: 0.5rem 0;
}
.icon-option {
  width: 38px; height: 38px; border-radius: 10px; font-size: 1.1rem;
  display: flex; align-items: center; justify-content: center;
  background: var(--t-hover); border: 1.5px solid var(--t-border);
  cursor: pointer; transition: all 0.15s;
}
.icon-option:hover {
  background: var(--t-hover2); border-color: var(--t-accent);
}
.icon-option.selected {
  background: var(--t-acc-alpha-md);
  border-color: var(--t-accent);
  box-shadow: 0 0 0 2px var(--t-acc-alpha-sm);
}
</style>
