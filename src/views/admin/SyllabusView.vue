<template>
  <div class="sv-root">

    <!-- ── Page Header ─────────────────────────────────────────── -->
    <header class="sv-page-hd">
      <div class="sv-page-hd-icon">
        <BookOpenText class="w-5 h-5" />
      </div>
      <div>
        <h1 class="sv-page-hd-title">Syllabus</h1>
        <p class="sv-page-hd-sub">Units, topics and learning objectives per grade &amp; subject</p>
      </div>
    </header>

    <!-- ── Filter Bar ──────────────────────────────────────────── -->
    <div class="sv-filter-bar">
      <div class="sv-select-group">
        <label class="sv-label">Grade</label>
        <div class="sv-select-wrap">
          <GraduationCap class="sv-sel-icon" />
          <select class="sv-select" v-model="gradeCode" @change="onGradeChange">
            <option value="">Select grade…</option>
            <option v-for="g in catalog.enabledGrades" :key="g.code" :value="g.code">{{ g.label }}</option>
          </select>
        </div>
      </div>

      <div class="sv-select-group">
        <label class="sv-label">Subject</label>
        <div class="sv-select-wrap">
          <Loader2 v-if="subjectsLoading" class="sv-sel-icon sv-spin" />
          <BookOpen v-else class="sv-sel-icon" />
          <select class="sv-select" v-model="subjectId" :disabled="!gradeCode || subjectsLoading" @change="onSubjectChange">
            <option value="">{{ subjectsLoading ? 'Loading subjects…' : gradeSubjects.length ? 'Select subject…' : gradeCode ? 'No subjects for this grade' : 'Select grade first' }}</option>
            <option v-for="s in gradeSubjects" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
      </div>

      <div v-if="subjectId" class="sv-select-group">
        <label class="sv-label">Book</label>
        <div class="sv-select-wrap sv-select-wrap-book">
          <BookCopy class="sv-sel-icon" />
          <select class="sv-select" v-model="selectedBookId" @change="onBookChange">
            <option value="">All units (no book)</option>
            <option v-for="b in books" :key="b.id" :value="b.id">{{ b.title }}</option>
          </select>
        </div>
      </div>

      <div class="sv-filter-spacer" />

      <button
        v-if="gradeCode && subjectId && !isBookMode"
        class="sv-btn-primary"
        @click="openUnitModal(null)"
      >
        <Plus class="w-4 h-4" /> Add Unit
      </button>
    </div>

    <!-- ── Loading ─────────────────────────────────────────────── -->
    <div v-if="loading" class="sv-loading">
      <Loader2 class="sv-spin" />
      <span>Loading syllabus…</span>
    </div>

    <!-- ── Empty prompt (no selection) ───────────────────────── -->
    <div v-else-if="!gradeCode || !subjectId" class="sv-empty">
      <div class="sv-empty-icon-wrap">
        <BookOpenText class="sv-empty-icon" />
      </div>
      <p class="sv-empty-title">Select a grade and subject</p>
      <p class="sv-empty-sub">Use the filters above to load the syllabus for a specific class</p>
    </div>

    <!-- ── Empty (selection made, no units) ──────────────────── -->
    <div v-else-if="!units.length" class="sv-empty">
      <div class="sv-empty-icon-wrap">
        <LayoutList class="sv-empty-icon" />
      </div>
      <p class="sv-empty-title">{{ isBookMode ? 'No units in this book yet' : 'No units yet' }}</p>
      <p class="sv-empty-sub">
        <template v-if="isBookMode">Add units to this book from the <router-link class="sv-link" :to="{ name: 'AdminBookUnits', params: { subjectId, bookId: selectedBookId } }">Book Units</router-link> section</template>
        <template v-else>Click "Add Unit" above to build this syllabus</template>
      </p>
    </div>

    <template v-else>
      <!-- ── Stats Banner ─────────────────────────────────────── -->
      <div class="sv-banner">
        <div class="sv-stat">
          <span class="sv-stat-n">{{ units.length }}</span>
          <span class="sv-stat-l">Units</span>
        </div>
        <div class="sv-banner-div" />
        <div class="sv-stat">
          <span class="sv-stat-n">{{ totalTopics }}</span>
          <span class="sv-stat-l">Topics</span>
        </div>
        <div class="sv-banner-div" />
        <div class="sv-stat">
          <span class="sv-stat-n">{{ totalObjectives }}</span>
          <span class="sv-stat-l">Objectives</span>
        </div>
      </div>

      <!-- ── Book-mode info note ──────────────────────────────── -->
      <div v-if="isBookMode" class="sv-book-note">
        <Info class="w-4 h-4 sv-book-note-icon" />
        <span>
          Units and topics come from the selected book.
          <router-link class="sv-link" :to="{ name: 'AdminBookUnits', params: { subjectId, bookId: selectedBookId } }">Manage units &amp; topics →</router-link>
        </span>
      </div>

      <!-- ── Units Accordion ──────────────────────────────────── -->
      <div class="sv-units">
        <div v-for="u in units" :key="u.id" class="sv-unit-card">

          <!-- Unit Header Row -->
          <div class="sv-unit-hd" :class="{ open: expandedUnits[u.id] }" @click="toggleUnit(u.id)">
            <button class="sv-chevron" :class="{ open: expandedUnits[u.id] }">
              <ChevronDown class="w-4 h-4" />
            </button>
            <div class="sv-unit-hd-main">
              <div class="sv-unit-hd-top">
                <span v-if="u.number" class="sv-unit-num">Unit {{ u.number }}</span>
                <span class="sv-unit-name">{{ u.name }}</span>
              </div>
              <div v-if="u.description" class="sv-unit-desc-preview">{{ u.description }}</div>
            </div>
            <div class="sv-unit-badges">
              <span class="sv-badge topics">{{ (u.topics || []).length }} topics</span>
              <span class="sv-badge objs">{{ totalUnitObjectives(u) }} obj.</span>
            </div>
            <div v-if="!u.bookId" class="sv-unit-actions" @click.stop>
              <button class="sv-icon-btn" title="Edit unit" @click="openUnitModal(u)">
                <Pencil class="w-3.5 h-3.5" />
              </button>
              <button class="sv-icon-btn danger" title="Delete unit" @click="confirmDeleteUnit(u)">
                <Trash2 class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          <!-- Unit Body -->
          <Transition name="sv-slide">
            <div v-show="expandedUnits[u.id]" class="sv-unit-body">

              <!-- ── Topics ─────────────────────────────────── -->
              <div class="sv-section">
                <div class="sv-section-hd">
                  <div class="sv-section-hd-left">
                    <ListTree class="sv-section-icon" />
                    <span class="sv-section-title">Topics</span>
                    <span class="sv-count-chip">{{ (u.topics || []).length }}</span>
                  </div>
                  <button v-if="!u.bookId" class="sv-btn-secondary" @click="openTopicModal(u, null)">
                    <Plus class="w-3.5 h-3.5" /> Add Topic
                  </button>
                </div>

                <div v-if="!(u.topics || []).length" class="sv-sub-empty">
                  No topics yet — add the first one above.
                </div>

                <div v-else class="sv-topics-list">
                  <div v-for="t in u.topics" :key="t.id" class="sv-topic-row">
                    <!-- Topic Row Header -->
                    <div class="sv-topic-hd">
                      <button class="sv-topic-toggle" :class="{ open: expandedTopics[t.id] }" @click="toggleTopic(t.id)">
                        <ChevronRight class="w-3.5 h-3.5" />
                      </button>
                      <span class="sv-topic-dot" />
                      <span class="sv-topic-name">{{ t.name }}</span>
                      <span class="sv-obj-pill">{{ (t.objectives || []).length }}</span>
                      <div v-if="!u.bookId" class="sv-row-actions">
                        <button class="sv-icon-btn sm" title="Edit topic" @click="openTopicModal(u, t)">
                          <Pencil class="w-3 h-3" />
                        </button>
                        <button class="sv-icon-btn sm danger" title="Delete topic" @click="confirmDeleteTopic(u, t)">
                          <Trash2 class="w-3 h-3" />
                        </button>
                      </div>
                    </div>

                    <!-- Topic Objectives (expanded) -->
                    <Transition name="sv-slide">
                      <div v-show="expandedTopics[t.id]" class="sv-topic-obj-panel">
                        <div v-if="!(t.objectives || []).length" class="sv-obj-empty">
                          No objectives for this topic yet.
                        </div>
                        <div v-else class="sv-obj-list">
                          <div v-for="o in t.objectives" :key="o.id" class="sv-obj-item">
                            <span class="sv-obj-bullet" />
                            <span v-if="o.code" class="sv-obj-code">{{ o.code }}</span>
                            <span v-if="o.cognitiveLevel" :class="['sv-cog-badge', o.cognitiveLevel.toLowerCase()]">{{ o.cognitiveLevel }}</span>
                            <span class="sv-obj-text">{{ o.objectiveText }}</span>
                            <div class="sv-row-actions">
                              <button class="sv-icon-btn sm" title="Edit" @click="openObjModal(u, t, o)">
                                <Pencil class="w-3 h-3" />
                              </button>
                              <button class="sv-icon-btn sm danger" title="Delete" @click="confirmDeleteObj(u, t, o)">
                                <Trash2 class="w-3 h-3" />
                              </button>
                            </div>
                          </div>
                        </div>
                        <button class="sv-add-obj-link" @click="openObjModal(u, t, null)">
                          <Plus class="w-3.5 h-3.5" /> Add objective to this topic
                        </button>
                      </div>
                    </Transition>
                  </div>
                </div>
              </div>

              <!-- ── Unit-Level Objectives ───────────────────── -->
              <div class="sv-section sv-section-last">
                <div class="sv-section-hd">
                  <div class="sv-section-hd-left">
                    <Target class="sv-section-icon" />
                    <span class="sv-section-title">Unit Objectives</span>
                    <span class="sv-count-chip">{{ (u.objectives || []).length }}</span>
                  </div>
                  <button class="sv-btn-secondary" @click="openObjModal(u, null, null)">
                    <Plus class="w-3.5 h-3.5" /> Add Objective
                  </button>
                </div>

                <div v-if="!(u.objectives || []).length" class="sv-sub-empty">
                  No unit-level objectives yet.
                </div>
                <div v-else class="sv-obj-list">
                  <div v-for="o in u.objectives" :key="o.id" class="sv-obj-item">
                    <span class="sv-obj-bullet" />
                    <span v-if="o.code" class="sv-obj-code">{{ o.code }}</span>
                    <span v-if="o.cognitiveLevel" :class="['sv-cog-badge', o.cognitiveLevel.toLowerCase()]">{{ o.cognitiveLevel }}</span>
                    <span class="sv-obj-text">{{ o.objectiveText }}</span>
                    <div class="sv-row-actions">
                      <button class="sv-icon-btn sm" title="Edit" @click="openObjModal(u, null, o)">
                        <Pencil class="w-3 h-3" />
                      </button>
                      <button class="sv-icon-btn sm danger" title="Delete" @click="confirmDeleteObj(u, null, o)">
                        <Trash2 class="w-3 h-3" />
                      </button>
                    </div>
                  </div>
                </div>
              </div>

            </div>
          </Transition>
        </div>
      </div>
    </template>

    <!-- ════════════════ MODALS ════════════════════════════════ -->

    <!-- Unit Modal -->
    <Teleport to="body">
      <div v-if="unitModal.open" class="sv-overlay" @click.self="unitModal.open = false">
        <div class="sv-modal" role="dialog" aria-modal="true">
          <div class="sv-modal-hd">
            <div class="sv-modal-hd-icon">
              <LayoutList class="w-4 h-4" />
            </div>
            <h2 class="sv-modal-title">{{ unitModal.isEdit ? 'Edit Unit' : 'Add Unit' }}</h2>
            <button class="sv-modal-close" @click="unitModal.open = false" aria-label="Close">
              <X class="w-4 h-4" />
            </button>
          </div>

          <form @submit.prevent="saveUnit" class="sv-modal-body">
            <div class="sv-field">
              <label class="sv-field-label">Unit Name <span class="sv-req">*</span></label>
              <input
                class="sv-input"
                v-model="unitModal.form.name"
                placeholder="e.g. Cells and Cell Biology"
                required
                autofocus
              />
            </div>

            <div class="sv-field-row">
              <div class="sv-field">
                <label class="sv-field-label">Unit Number</label>
                <input class="sv-input" type="number" v-model.number="unitModal.form.number" placeholder="1" min="1" />
              </div>
              <div class="sv-field">
                <label class="sv-field-label">Sort Order</label>
                <input class="sv-input" type="number" v-model.number="unitModal.form.sortOrder" placeholder="0" min="0" />
              </div>
            </div>

            <div class="sv-field">
              <label class="sv-field-label">Description <span class="sv-opt">(optional)</span></label>
              <textarea
                class="sv-textarea"
                v-model="unitModal.form.description"
                rows="3"
                placeholder="Brief overview of what students will learn in this unit…"
              />
            </div>

            <div class="sv-modal-ft">
              <button type="button" class="sv-btn-ghost" @click="unitModal.open = false">Cancel</button>
              <button type="submit" class="sv-btn-primary" :disabled="unitModal.saving">
                <Loader2 v-if="unitModal.saving" class="sv-spin w-4 h-4" />
                {{ unitModal.isEdit ? 'Save Changes' : 'Add Unit' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>

    <!-- Topic Modal -->
    <Teleport to="body">
      <div v-if="topicModal.open" class="sv-overlay" @click.self="topicModal.open = false">
        <div class="sv-modal" role="dialog" aria-modal="true">
          <div class="sv-modal-hd">
            <div class="sv-modal-hd-icon">
              <ListTree class="w-4 h-4" />
            </div>
            <h2 class="sv-modal-title">{{ topicModal.isEdit ? 'Edit Topic' : 'Add Topic' }}</h2>
            <button class="sv-modal-close" @click="topicModal.open = false" aria-label="Close">
              <X class="w-4 h-4" />
            </button>
          </div>

          <form @submit.prevent="saveTopic" class="sv-modal-body">
            <p class="sv-modal-ctx">
              <span class="sv-ctx-label">Unit:</span> {{ topicModal.unit?.name }}
            </p>
            <div class="sv-field">
              <label class="sv-field-label">Topic Name <span class="sv-req">*</span></label>
              <input
                class="sv-input"
                v-model="topicModal.form.name"
                placeholder="e.g. Cell Structure and Function"
                required
                autofocus
              />
            </div>
            <div class="sv-field sv-field-sm">
              <label class="sv-field-label">Sort Order</label>
              <input class="sv-input" type="number" v-model.number="topicModal.form.sortOrder" placeholder="0" min="0" />
            </div>

            <div class="sv-modal-ft">
              <button type="button" class="sv-btn-ghost" @click="topicModal.open = false">Cancel</button>
              <button type="submit" class="sv-btn-primary" :disabled="topicModal.saving">
                <Loader2 v-if="topicModal.saving" class="sv-spin w-4 h-4" />
                {{ topicModal.isEdit ? 'Save Changes' : 'Add Topic' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>

    <!-- Objective Modal -->
    <Teleport to="body">
      <div v-if="objModal.open" class="sv-overlay" @click.self="objModal.open = false">
        <div class="sv-modal" role="dialog" aria-modal="true">
          <div class="sv-modal-hd">
            <div class="sv-modal-hd-icon">
              <Target class="w-4 h-4" />
            </div>
            <h2 class="sv-modal-title">{{ objModal.isEdit ? 'Edit Objective' : 'Add Objective' }}</h2>
            <button class="sv-modal-close" @click="objModal.open = false" aria-label="Close">
              <X class="w-4 h-4" />
            </button>
          </div>

          <form @submit.prevent="saveObjective" class="sv-modal-body">
            <p class="sv-modal-ctx">
              <span class="sv-ctx-label">{{ objModal.topic ? 'Topic:' : 'Unit:' }}</span>
              {{ objModal.topic ? objModal.topic.name : objModal.unit?.name }}
            </p>

            <div class="sv-field">
              <label class="sv-field-label">Objective Text <span class="sv-req">*</span></label>
              <textarea
                class="sv-textarea"
                v-model="objModal.form.objectiveText"
                rows="3"
                placeholder="Students will be able to…"
                required
                autofocus
              />
            </div>

            <div class="sv-field-row">
              <div class="sv-field">
                <label class="sv-field-label">Code <span class="sv-opt">(optional)</span></label>
                <input class="sv-input" v-model="objModal.form.code" placeholder="e.g. K1, A2, C3" />
              </div>
              <div class="sv-field">
                <label class="sv-field-label">Cognitive Level</label>
                <select class="sv-select sv-select-inline" v-model="objModal.form.cognitiveLevel">
                  <option value="">— None —</option>
                  <option v-for="lvl in cognitiveLevels" :key="lvl" :value="lvl">{{ lvl }}</option>
                </select>
              </div>
            </div>

            <!-- Bloom's taxonomy hint -->
            <div class="sv-bloom-hint">
              <div v-for="lvl in cognitiveLevels" :key="lvl" :class="['sv-bloom-chip', lvl.toLowerCase(), { active: objModal.form.cognitiveLevel === lvl }]" @click="objModal.form.cognitiveLevel = objModal.form.cognitiveLevel === lvl ? '' : lvl">
                {{ lvl }}
              </div>
            </div>

            <p v-if="objModal.errMsg" class="sv-modal-err">{{ objModal.errMsg }}</p>

            <div class="sv-modal-ft">
              <button type="button" class="sv-btn-ghost" @click="objModal.open = false">Cancel</button>
              <button type="submit" class="sv-btn-primary" :disabled="objModal.saving">
                <Loader2 v-if="objModal.saving" class="sv-spin w-4 h-4" />
                {{ objModal.isEdit ? 'Save Changes' : 'Add Objective' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'
import {
  BookOpenText, BookOpen, BookCopy, GraduationCap, Plus, Pencil, Trash2,
  ChevronDown, ChevronRight, Loader2, X, ListTree, Target, LayoutList, Info,
} from '@lucide/vue'

const catalog = useCatalogStore()
const confirm = useConfirm()

// ── State ────────────────────────────────────────────────────
const gradeCode        = ref('')
const subjectId        = ref('')
const selectedBookId   = ref('')
const books            = ref([])
const units            = ref([])
const loading          = ref(false)
const subjectsLoading  = ref(false)
const expandedUnits  = ref({})
const expandedTopics = ref({})

const cognitiveLevels = ['Remember', 'Understand', 'Apply', 'Analyze', 'Evaluate', 'Create']

// ── Computed ─────────────────────────────────────────────────
const gradeSubjects   = computed(() => catalog.subjectsForGrade(gradeCode.value))
const isBookMode      = computed(() => !!selectedBookId.value)
const totalTopics     = computed(() => units.value.reduce((n, u) => n + (u.topics?.length ?? 0), 0))
const totalObjectives = computed(() => units.value.reduce((n, u) => {
  const unit  = u.objectives?.length ?? 0
  const topic = (u.topics ?? []).reduce((m, t) => m + (t.objectives?.length ?? 0), 0)
  return n + unit + topic
}, 0))

function totalUnitObjectives(u) {
  return (u.objectives?.length ?? 0) + (u.topics ?? []).reduce((n, t) => n + (t.objectives?.length ?? 0), 0)
}

// ── Lifecycle ────────────────────────────────────────────────
onMounted(() => catalog.fetchGrades())

// ── Grade / Subject / Book ────────────────────────────────────
async function onGradeChange() {
  subjectId.value    = ''
  selectedBookId.value = ''
  books.value        = []
  units.value        = []
  expandedUnits.value  = {}
  expandedTopics.value = {}
  if (gradeCode.value) {
    subjectsLoading.value = true
    try { await catalog.fetchSubjectsForGrade(gradeCode.value) }
    finally { subjectsLoading.value = false }
  }
}

async function onSubjectChange() {
  selectedBookId.value = ''
  units.value          = []
  expandedUnits.value  = {}
  expandedTopics.value = {}
  if (subjectId.value) {
    const allBooks = await catalog.fetchBooks(subjectId.value)
    // Show books tied to this grade, plus books with no grade restriction
    books.value = allBooks.filter(b => !b.gradeCode || b.gradeCode === gradeCode.value)
    await loadSyllabus()
  } else {
    books.value = []
  }
}

async function onBookChange() {
  units.value          = []
  expandedUnits.value  = {}
  expandedTopics.value = {}
  await loadSyllabus()
}

async function loadSyllabus() {
  if (!gradeCode.value || !subjectId.value) return
  loading.value = true
  try {
    if (selectedBookId.value) {
      units.value = await catalog.fetchBookSyllabus(Number(selectedBookId.value))
    } else {
      units.value = await catalog.fetchSyllabus(gradeCode.value, subjectId.value)
    }
    if (units.value.length) expandedUnits.value[units.value[0].id] = true
  } finally {
    loading.value = false
  }
}

// ── Accordion toggles ────────────────────────────────────────
function toggleUnit(id)  { expandedUnits.value[id]  = !expandedUnits.value[id] }
function toggleTopic(id) { expandedTopics.value[id] = !expandedTopics.value[id] }

// ════════════════════════════════════════════════════════════
// ── Unit CRUD ────────────────────────────────────────────────
// ════════════════════════════════════════════════════════════
const unitModal = reactive({
  open: false, isEdit: false, editId: null, saving: false,
  form: { name: '', number: null, sortOrder: 0, description: '' },
})

function openUnitModal(u) {
  unitModal.isEdit = !!u
  unitModal.editId = u?.id ?? null
  unitModal.form.name        = u?.name ?? ''
  unitModal.form.number      = u?.number ?? (units.value.length + 1)
  unitModal.form.sortOrder   = u?.sortOrder ?? units.value.length
  unitModal.form.description = u?.description ?? ''
  unitModal.open = true
}

async function saveUnit() {
  unitModal.saving = true
  try {
    const payload = {
      gradeCode: gradeCode.value,
      subjectId: Number(subjectId.value),
      name:        unitModal.form.name,
      number:      unitModal.form.number || null,
      sortOrder:   unitModal.form.sortOrder ?? 0,
      description: unitModal.form.description || null,
    }
    if (unitModal.isEdit) {
      await catalog.updateUnit(unitModal.editId, payload)
    } else {
      const created = await catalog.addUnit(payload)
      if (created?.id) expandedUnits.value[created.id] = true
    }
    unitModal.open = false
    await loadSyllabus()
  } finally {
    unitModal.saving = false
  }
}

async function confirmDeleteUnit(u) {
  const ok = await confirm({
    title: `Delete "${u.name}"?`,
    message: 'All topics and learning objectives inside this unit will also be deleted. This cannot be undone.',
    confirmLabel: 'Delete Unit',
  })
  if (ok) { await catalog.deleteUnit(u.id); await loadSyllabus() }
}

// ════════════════════════════════════════════════════════════
// ── Topic CRUD ───────────────────────────────────────────────
// ════════════════════════════════════════════════════════════
const topicModal = reactive({
  open: false, isEdit: false, editId: null, unit: null, saving: false,
  form: { name: '', sortOrder: 0 },
})

function openTopicModal(u, t) {
  topicModal.unit      = u
  topicModal.isEdit    = !!t
  topicModal.editId    = t?.id ?? null
  topicModal.form.name      = t?.name ?? ''
  topicModal.form.sortOrder = t?.sortOrder ?? (u.topics?.length ?? 0)
  topicModal.open = true
}

async function saveTopic() {
  topicModal.saving = true
  try {
    const payload = {
      unitId:    topicModal.unit.id,
      name:      topicModal.form.name,
      sortOrder: topicModal.form.sortOrder ?? 0,
    }
    if (topicModal.isEdit) {
      await catalog.updateSyllabusTopic(topicModal.editId, payload)
    } else {
      const created = await catalog.addTopic(payload)
      if (created?.id) expandedTopics.value[created.id] = true
    }
    topicModal.open = false
    await loadSyllabus()
  } finally {
    topicModal.saving = false
  }
}

async function confirmDeleteTopic(u, t) {
  const ok = await confirm({
    title: `Delete topic "${t.name}"?`,
    message: 'All learning objectives for this topic will also be deleted.',
    confirmLabel: 'Delete Topic',
  })
  if (ok) { await catalog.deleteTopic(t.id); await loadSyllabus() }
}

// ════════════════════════════════════════════════════════════
// ── Objective CRUD ───────────────────────────────────────────
// ════════════════════════════════════════════════════════════
const objModal = reactive({
  open: false, isEdit: false, editId: null, unit: null, topic: null, saving: false, errMsg: '',
  form: { objectiveText: '', code: '', cognitiveLevel: '' },
})

function openObjModal(u, t, o) {
  objModal.unit    = u
  objModal.topic   = t
  objModal.isEdit  = !!o
  objModal.editId  = o?.id ?? null
  objModal.errMsg  = ''
  objModal.form.objectiveText  = o?.objectiveText ?? ''
  objModal.form.code           = o?.code ?? ''
  objModal.form.cognitiveLevel = o?.cognitiveLevel ?? ''
  objModal.open = true
}

async function saveObjective() {
  objModal.saving = true
  objModal.errMsg = ''
  try {
    const payload = {
      unitId:         objModal.unit.id,
      topicId:        objModal.topic?.id ?? null,
      objectiveText:  objModal.form.objectiveText,
      code:           objModal.form.code || null,
      cognitiveLevel: objModal.form.cognitiveLevel || null,
    }
    if (objModal.isEdit) {
      await catalog.updateObjective(objModal.editId, payload)
    } else {
      await catalog.addObjective(payload)
    }
    objModal.open = false
    await loadSyllabus()
  } catch (e) {
    objModal.errMsg = e?.response?.data?.title ?? e?.response?.data ?? e?.message ?? 'Failed to save objective'
  } finally {
    objModal.saving = false
  }
}

async function confirmDeleteObj(u, t, o) {
  const preview = o.objectiveText.length > 80 ? o.objectiveText.slice(0, 80) + '…' : o.objectiveText
  const ok = await confirm({
    title: 'Delete objective?',
    message: `"${preview}"`,
    confirmLabel: 'Delete',
  })
  if (ok) { await catalog.deleteObjective(o.id); await loadSyllabus() }
}
</script>

<style scoped>
/* ── Root ──────────────────────────────────────────────────── */
.sv-root {
  padding: 28px 32px;
  color: var(--t-text1);
  max-width: 960px;
}
@media (max-width: 700px) { .sv-root { padding: 16px; } }

/* ── Page Header ─────────────────────────────────────────── */
.sv-page-hd {
  display: flex; align-items: center; gap: 16px; margin-bottom: 28px;
}
.sv-page-hd-icon {
  width: 44px; height: 44px; border-radius: 14px; flex-shrink: 0;
  background: var(--t-acc-alpha-sm);
  display: flex; align-items: center; justify-content: center;
  color: var(--t-accent);
}
.sv-page-hd-title {
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.75rem;
  margin: 0 0 2px; color: var(--t-text1);
}
.sv-page-hd-sub { margin: 0; color: var(--t-text3); font-size: 0.875rem; }

/* ── Filter Bar ──────────────────────────────────────────── */
.sv-filter-bar {
  display: flex; align-items: flex-end; gap: 16px; flex-wrap: wrap;
  padding: 16px 20px; border-radius: 14px;
  background: var(--t-bg2); border: 1px solid var(--t-border);
  margin-bottom: 20px;
}
.sv-filter-spacer { flex: 1; }
.sv-select-group { display: flex; flex-direction: column; gap: 6px; }
.sv-label { font-size: 0.75rem; font-weight: 600; color: var(--t-text3); letter-spacing: 0.04em; text-transform: uppercase; }
.sv-select-wrap {
  position: relative; display: flex; align-items: center;
  border: 1px solid var(--t-border); border-radius: 10px;
  background: var(--t-bg); transition: border-color 0.15s;
  min-width: 180px;
}
.sv-select-wrap:focus-within { border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-acc-alpha-sm); }
.sv-sel-icon { position: absolute; left: 11px; width: 14px; height: 14px; color: var(--t-text3); pointer-events: none; }
.sv-select {
  width: 100%; padding: 10px 12px 10px 34px; border: none; background: transparent;
  color: var(--t-text1); font-size: 0.875rem; font-family: inherit; cursor: pointer; outline: none;
  appearance: none;
}
.sv-select:disabled { opacity: 0.5; cursor: not-allowed; }
.sv-select-inline {
  padding: 10px 12px; border: 1px solid var(--t-border); border-radius: 10px;
  background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem;
  font-family: inherit; width: 100%; outline: none; appearance: none;
  transition: border-color 0.15s;
}
.sv-select-inline:focus { border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-acc-alpha-sm); }

/* ── Buttons ─────────────────────────────────────────────── */
.sv-btn-primary {
  display: inline-flex; align-items: center; gap: 7px;
  padding: 10px 18px; border: none; border-radius: 11px; cursor: pointer;
  font-weight: 700; font-size: 0.875rem; font-family: inherit;
  color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  box-shadow: 0 2px 12px var(--t-acc-alpha-sm); transition: all 0.15s;
}
.sv-btn-primary:hover { transform: translateY(-1px); box-shadow: 0 4px 16px var(--t-acc-alpha-sm); }
.sv-btn-primary:disabled { opacity: 0.6; cursor: not-allowed; transform: none; }

.sv-btn-secondary {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 7px 13px; border-radius: 9px; font-weight: 600;
  font-size: 0.8rem; font-family: inherit; cursor: pointer;
  color: var(--t-accent); background: var(--t-acc-alpha-sm);
  border: 1px solid color-mix(in srgb, var(--t-accent) 25%, transparent);
  transition: all 0.15s;
}
.sv-btn-secondary:hover { background: var(--t-acc-alpha-sm); filter: brightness(1.07); }

.sv-btn-ghost {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 10px 16px; border-radius: 10px; border: 1px solid var(--t-border);
  cursor: pointer; font-weight: 600; font-size: 0.875rem; font-family: inherit;
  color: var(--t-text2); background: var(--t-bg); transition: all 0.15s;
}
.sv-btn-ghost:hover { color: var(--t-text1); border-color: var(--t-text3); }

.sv-icon-btn {
  display: inline-flex; align-items: center; justify-content: center;
  width: 30px; height: 30px; border-radius: 8px; border: none; cursor: pointer;
  background: transparent; color: var(--t-text3); transition: all 0.15s; flex-shrink: 0;
}
.sv-icon-btn:hover { background: var(--t-bg); color: var(--t-text1); }
.sv-icon-btn.danger:hover { background: #f8717118; color: #f87171; }
.sv-icon-btn.sm { width: 24px; height: 24px; border-radius: 6px; }

/* ── Book select wider ───────────────────────────────────── */
.sv-select-wrap-book { min-width: 220px; }

/* ── Book-mode info note ─────────────────────────────────── */
.sv-book-note {
  display: flex; align-items: flex-start; gap: 10px;
  padding: 12px 16px; border-radius: 12px; margin-bottom: 16px;
  background: color-mix(in srgb, var(--t-accent) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-accent) 22%, transparent);
  font-size: 0.85rem; color: var(--t-text2);
}
.sv-book-note-icon { color: var(--t-accent); flex-shrink: 0; margin-top: 1px; }
.sv-link { color: var(--t-accent); font-weight: 600; text-decoration: none; }
.sv-link:hover { text-decoration: underline; }

/* ── Stats Banner ────────────────────────────────────────── */
.sv-banner {
  display: flex; align-items: center; gap: 0;
  padding: 14px 24px; border-radius: 14px; margin-bottom: 20px;
  background: var(--t-bg2); border: 1px solid var(--t-border);
}
.sv-stat { display: flex; flex-direction: column; align-items: center; gap: 2px; padding: 0 28px; }
.sv-stat-n { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.5rem; color: var(--t-accent); line-height: 1; }
.sv-stat-l { font-size: 0.73rem; color: var(--t-text3); font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
.sv-banner-div { width: 1px; height: 36px; background: var(--t-border); }

/* ── Empty States ────────────────────────────────────────── */
.sv-empty {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 10px; padding: 64px 24px; text-align: center;
}
.sv-empty-icon-wrap {
  width: 56px; height: 56px; border-radius: 18px;
  background: var(--t-acc-alpha-sm);
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 4px;
}
.sv-empty-icon { width: 26px; height: 26px; color: var(--t-accent); }
.sv-empty-title { margin: 0; font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1.1rem; color: var(--t-text1); }
.sv-empty-sub   { margin: 0; font-size: 0.875rem; color: var(--t-text3); }

/* ── Loading ─────────────────────────────────────────────── */
.sv-loading {
  display: flex; align-items: center; gap: 10px;
  padding: 32px 24px; color: var(--t-text3); font-size: 0.875rem;
}

/* ── Units List ──────────────────────────────────────────── */
.sv-units { display: flex; flex-direction: column; gap: 10px; }

/* ── Unit Card ───────────────────────────────────────────── */
.sv-unit-card {
  border: 1px solid var(--t-border); border-radius: 16px;
  background: var(--t-bg2); overflow: hidden;
  transition: box-shadow 0.2s;
}
.sv-unit-card:hover { box-shadow: 0 4px 20px var(--t-shadow); }

.sv-unit-hd {
  display: flex; align-items: center; gap: 12px;
  padding: 16px 18px; cursor: pointer; user-select: none;
  transition: background 0.15s;
}
.sv-unit-hd:hover { background: var(--t-bg); }

.sv-chevron {
  display: inline-flex; align-items: center; justify-content: center;
  width: 28px; height: 28px; flex-shrink: 0; border-radius: 8px;
  border: none; cursor: pointer; background: transparent; color: var(--t-text3);
  transition: transform 0.2s, color 0.15s;
}
.sv-chevron.open { transform: rotate(0deg); color: var(--t-accent); }
.sv-chevron:not(.open) { transform: rotate(-90deg); }

.sv-unit-hd-main { flex: 1; min-width: 0; }
.sv-unit-hd-top  { display: flex; align-items: baseline; gap: 8px; }
.sv-unit-num  { font-size: 0.72rem; font-weight: 700; color: var(--t-accent); text-transform: uppercase; letter-spacing: 0.06em; }
.sv-unit-name { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1.05rem; color: var(--t-text1); }
.sv-unit-desc-preview { font-size: 0.8rem; color: var(--t-text3); margin-top: 3px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 520px; }

.sv-unit-badges { display: flex; gap: 6px; flex-shrink: 0; }
.sv-badge {
  padding: 3px 10px; border-radius: 99px; font-size: 0.72rem; font-weight: 600;
  background: var(--t-bg); border: 1px solid var(--t-border); color: var(--t-text3);
}
.sv-badge.topics { background: var(--t-acc-alpha-sm); color: var(--t-accent); border-color: color-mix(in srgb, var(--t-accent) 20%, transparent); }

.sv-unit-actions { display: flex; gap: 4px; flex-shrink: 0; }

/* ── Unit Body ───────────────────────────────────────────── */
.sv-unit-body {
  border-top: 1px solid var(--t-border);
  padding: 18px 20px;
  display: flex; flex-direction: column; gap: 20px;
}

/* ── Sections (Topics / Objectives) ─────────────────────── */
.sv-section {
  border: 1px solid var(--t-border); border-radius: 12px;
  background: var(--t-bg); overflow: hidden;
}

.sv-section-hd {
  display: flex; align-items: center; justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid var(--t-border);
  background: var(--t-bg2);
}
.sv-section-hd-left { display: flex; align-items: center; gap: 8px; }
.sv-section-icon { width: 15px; height: 15px; color: var(--t-accent); }
.sv-section-title { font-weight: 700; font-size: 0.85rem; color: var(--t-text1); }
.sv-count-chip {
  padding: 2px 8px; border-radius: 99px; font-size: 0.72rem; font-weight: 700;
  background: var(--t-acc-alpha-sm); color: var(--t-accent);
}

.sv-sub-empty { padding: 16px; font-size: 0.83rem; color: var(--t-text3); }

/* ── Topics List ─────────────────────────────────────────── */
.sv-topics-list { display: flex; flex-direction: column; }

.sv-topic-row {
  border-bottom: 1px solid var(--t-border);
}
.sv-topic-row:last-child { border-bottom: none; }

.sv-topic-hd {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 16px;
  transition: background 0.12s;
}
.sv-topic-hd:hover { background: color-mix(in srgb, var(--t-accent) 4%, transparent); }

.sv-topic-toggle {
  display: inline-flex; align-items: center; justify-content: center;
  width: 22px; height: 22px; border-radius: 6px; flex-shrink: 0;
  border: none; cursor: pointer; background: transparent; color: var(--t-text3);
  transition: transform 0.2s, color 0.15s;
}
.sv-topic-toggle.open { transform: rotate(90deg); color: var(--t-accent); }

.sv-topic-dot {
  width: 6px; height: 6px; border-radius: 50%; flex-shrink: 0;
  background: var(--t-accent); opacity: 0.6;
}
.sv-topic-name { flex: 1; font-weight: 600; font-size: 0.9rem; color: var(--t-text1); }
.sv-obj-pill {
  font-size: 0.7rem; font-weight: 700; padding: 2px 7px; border-radius: 99px;
  background: var(--t-border); color: var(--t-text3);
}
.sv-row-actions { display: flex; gap: 2px; flex-shrink: 0; opacity: 0; transition: opacity 0.15s; }
.sv-topic-hd:hover .sv-row-actions,
.sv-obj-item:hover .sv-row-actions { opacity: 1; }

/* ── Topic Objectives Panel ──────────────────────────────── */
.sv-topic-obj-panel {
  border-top: 1px dashed var(--t-border);
  background: color-mix(in srgb, var(--t-bg) 60%, transparent);
  padding: 10px 16px 12px 48px;
}

.sv-obj-empty { font-size: 0.8rem; color: var(--t-text3); padding: 4px 0 8px; }

.sv-add-obj-link {
  display: inline-flex; align-items: center; gap: 5px; margin-top: 8px;
  font-size: 0.78rem; font-weight: 600; color: var(--t-accent);
  background: none; border: none; cursor: pointer; padding: 0;
  font-family: inherit;
}
.sv-add-obj-link:hover { text-decoration: underline; }

/* ── Objective Items ─────────────────────────────────────── */
.sv-obj-list { display: flex; flex-direction: column; gap: 1px; padding: 8px 16px; }

.sv-obj-item {
  display: flex; align-items: flex-start; gap: 8px;
  padding: 8px 10px; border-radius: 8px;
  transition: background 0.12s;
}
.sv-obj-item:hover { background: var(--t-bg); }

.sv-obj-bullet {
  width: 5px; height: 5px; border-radius: 50%; margin-top: 7px; flex-shrink: 0;
  background: var(--t-text3);
}
.sv-obj-code {
  font-size: 0.7rem; font-weight: 700; padding: 2px 6px; border-radius: 5px;
  background: var(--t-border); color: var(--t-text2); flex-shrink: 0; margin-top: 1px;
  font-family: monospace;
}
.sv-obj-text { flex: 1; font-size: 0.875rem; color: var(--t-text2); line-height: 1.5; }

/* Cognitive Level Badges */
.sv-cog-badge {
  font-size: 0.65rem; font-weight: 700; padding: 2px 7px; border-radius: 5px;
  flex-shrink: 0; text-transform: uppercase; letter-spacing: 0.04em;
  margin-top: 1px;
}
.sv-cog-badge.remember  { background: #dbeafe; color: #1d4ed8; }
.sv-cog-badge.understand { background: #ccfbf1; color: #0f766e; }
.sv-cog-badge.apply     { background: #dcfce7; color: #15803d; }
.sv-cog-badge.analyze   { background: #fef3c7; color: #b45309; }
.sv-cog-badge.evaluate  { background: #ede9fe; color: #7c3aed; }
.sv-cog-badge.create    { background: #ffe4e6; color: #be123c; }

/* ── Overlay / Modal ─────────────────────────────────────── */
.sv-overlay {
  position: fixed; inset: 0; z-index: 9000;
  background: rgba(0, 0, 0, 0.45); backdrop-filter: blur(3px);
  display: flex; align-items: center; justify-content: center; padding: 20px;
}
.sv-modal {
  background: var(--t-bg2); border: 1px solid var(--t-border);
  border-radius: 20px; width: 100%; max-width: 520px;
  box-shadow: 0 24px 64px rgba(0,0,0,0.22);
  overflow: hidden;
}
.sv-modal-hd {
  display: flex; align-items: center; gap: 12px;
  padding: 18px 22px;
  border-bottom: 1px solid var(--t-border);
  background: var(--t-bg);
}
.sv-modal-hd-icon {
  width: 34px; height: 34px; border-radius: 10px; flex-shrink: 0;
  background: var(--t-acc-alpha-sm); color: var(--t-accent);
  display: flex; align-items: center; justify-content: center;
}
.sv-modal-title { flex: 1; margin: 0; font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.1rem; color: var(--t-text1); }
.sv-modal-close {
  display: inline-flex; align-items: center; justify-content: center;
  width: 32px; height: 32px; border-radius: 8px; border: none; cursor: pointer;
  background: transparent; color: var(--t-text3); transition: all 0.15s;
}
.sv-modal-close:hover { background: var(--t-border); color: var(--t-text1); }

.sv-modal-body { padding: 22px; display: flex; flex-direction: column; gap: 16px; }
.sv-modal-ctx { margin: 0; font-size: 0.83rem; color: var(--t-text3); }
.sv-ctx-label { font-weight: 700; color: var(--t-text2); }
.sv-modal-err {
  margin: 0; padding: 10px 14px; border-radius: 9px; font-size: 0.83rem;
  background: #fef2f2; border: 1px solid #fecaca; color: #b91c1c;
}

.sv-modal-ft {
  display: flex; justify-content: flex-end; gap: 10px;
  padding-top: 4px; border-top: 1px solid var(--t-border); margin-top: 4px;
}

/* ── Form Fields ─────────────────────────────────────────── */
.sv-field { display: flex; flex-direction: column; gap: 6px; }
.sv-field-sm { max-width: 160px; }
.sv-field-row { display: flex; gap: 14px; }
.sv-field-row .sv-field { flex: 1; }

.sv-field-label { font-size: 0.78rem; font-weight: 600; color: var(--t-text2); }
.sv-req { color: #f87171; }
.sv-opt { font-weight: 400; color: var(--t-text3); }

.sv-input, .sv-textarea {
  padding: 10px 13px; border: 1px solid var(--t-border); border-radius: 10px;
  background: var(--t-bg); color: var(--t-text1);
  font-size: 0.875rem; font-family: inherit; outline: none;
  transition: border-color 0.15s, box-shadow 0.15s;
  width: 100%; box-sizing: border-box;
}
.sv-input:focus, .sv-textarea:focus {
  border-color: var(--t-accent);
  box-shadow: 0 0 0 3px var(--t-acc-alpha-sm);
}
.sv-textarea { resize: vertical; min-height: 80px; }

/* ── Bloom's Taxonomy Hint ───────────────────────────────── */
.sv-bloom-hint { display: flex; flex-wrap: wrap; gap: 6px; }
.sv-bloom-chip {
  padding: 4px 10px; border-radius: 7px; font-size: 0.72rem; font-weight: 700;
  cursor: pointer; user-select: none; transition: all 0.15s; opacity: 0.5;
  text-transform: uppercase; letter-spacing: 0.03em;
}
.sv-bloom-chip:hover { opacity: 0.85; }
.sv-bloom-chip.active { opacity: 1; transform: scale(1.05); }
.sv-bloom-chip.remember  { background: #dbeafe; color: #1d4ed8; }
.sv-bloom-chip.understand { background: #ccfbf1; color: #0f766e; }
.sv-bloom-chip.apply     { background: #dcfce7; color: #15803d; }
.sv-bloom-chip.analyze   { background: #fef3c7; color: #b45309; }
.sv-bloom-chip.evaluate  { background: #ede9fe; color: #7c3aed; }
.sv-bloom-chip.create    { background: #ffe4e6; color: #be123c; }

/* ── Transition ──────────────────────────────────────────── */
.sv-slide-enter-active,
.sv-slide-leave-active { transition: none; }

/* ── Spin ────────────────────────────────────────────────── */
.sv-spin {
  animation: sv-spin 0.7s linear infinite;
}
@keyframes sv-spin {
  from { transform: rotate(0deg); }
  to   { transform: rotate(360deg); }
}
</style>
