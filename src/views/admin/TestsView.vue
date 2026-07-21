<template>
  <div class="animate-fade-in space-y-5">

    <!-- Add / Edit Modal -->
    <div v-if="showModal" class="tv-overlay" @click.self="closeModal">
      <div class="tv-modal">
        <div class="tv-modal-header">
          <span>{{ editingId ? 'Edit Test' : 'Create Test' }}</span>
          <button @click="closeModal" class="btn-ghost" style="padding:0.4rem;">
            <X class="w-4 h-4" />
          </button>
        </div>
        <form @submit.prevent="save" class="tv-modal-body">

          <!-- Title -->
          <div>
            <label class="label">Test Title <span style="color:#ef4444;">*</span></label>
            <input v-model="form.title" type="text" class="input"
                   placeholder="e.g. June Monthly Combined Test" required />
          </div>

          <!-- Kind -->
          <div>
            <label class="label">Test Kind</label>
            <select v-model="form.kind" class="input">
              <option value="objective">Objective</option>
              <option value="subjective">Subjective</option>
              <option value="mixed">Mixed</option>
              <option value="daily">Daily Challenge</option>
              <option value="weekly">Weekly Test</option>
              <option value="monthly">Monthly Exam</option>
              <option value="entrance">Entrance Exam</option>
            </select>
          </div>

          <!-- Grade dropdown (before Subject) -->
          <div>
            <label class="label">Grade</label>
            <select v-model="form.gradeCode" class="input">
              <option value="">— All Grades —</option>
              <option v-for="g in catalog.grades" :key="g.code" :value="g.code">{{ g.label }}</option>
            </select>
          </div>

          <!-- Subjects multi-select checkbox panel (dynamic based on grade) -->
          <div>
            <label class="label">Subjects</label>
            <div style="border:1px solid var(--t-input-border); border-radius:12px; padding:0.75rem;
                         background:var(--t-input-bg);">
              <!-- Loading state -->
              <div v-if="subjectLoading"
                   class="flex items-center gap-2"
                   style="min-height:52px; color:var(--t-text3); font-size:0.82rem;">
                <div class="tv-spinner" style="width:14px; height:14px; border-width:2px;"></div>
                Loading subjects…
              </div>
              <!-- Prompt to select grade -->
              <div v-else-if="!form.gradeCode"
                   style="min-height:52px; display:flex; align-items:center;
                          color:var(--t-text3); font-size:0.82rem;">
                ← Select a grade to see subjects
              </div>
              <!-- No subjects found -->
              <div v-else-if="!modalSubjects.length"
                   style="min-height:52px; display:flex; align-items:center;
                          color:var(--t-text3); font-size:0.82rem;">
                No subjects found for this grade
              </div>
              <!-- Checkbox grid -->
              <div v-else
                   style="display:grid; grid-template-columns:1fr 1fr; gap:0.5rem;
                          max-height:180px; overflow-y:auto;">
                <label v-for="s in modalSubjects" :key="s.id"
                       class="flex items-center gap-2"
                       style="font-size:0.82rem; color:var(--t-text2); cursor:pointer;">
                  <input type="checkbox" :value="s.id" v-model="form.subjectIds"
                         style="accent-color:var(--t-accent); width:14px; height:14px;" />
                  {{ s.name }}
                </label>
              </div>
            </div>
          </div>

          <!-- Duration + Total Marks -->
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div>
              <label class="label" style="font-size:0.75rem;">Duration (minutes)</label>
              <input v-model.number="form.durationMin" type="number" min="5" max="300"
                     class="input" style="font-size:0.82rem;" placeholder="e.g. 90" />
            </div>
            <div>
              <label class="label" style="font-size:0.75rem;">Total Marks (optional)</label>
              <input v-model.number="form.totalMarks" type="number" min="1"
                     class="input" style="font-size:0.82rem;" placeholder="e.g. 100" />
            </div>
          </div>

          <!-- Scheduled + End dates (side by side) -->
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div>
              <label class="label" style="font-size:0.75rem;">Scheduled Date &amp; Time</label>
              <DateTimePicker v-model="form.scheduledAt" mode="datetime"
                              placeholder="Set schedule" />
            </div>
            <div>
              <label class="label" style="font-size:0.75rem;">End Date &amp; Time</label>
              <DateTimePicker v-model="form.endsAt" mode="datetime"
                              placeholder="Set end time"
                              :min="form.scheduledAt" />
            </div>
          </div>

          <!-- Error -->
          <div v-if="errMsg"
               style="background:#fef2f2; border:1px solid #fecaca; color:#b91c1c;
                      border-radius:10px; padding:0.75rem; font-size:0.82rem;">
            {{ errMsg }}
          </div>

          <!-- Actions -->
          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button type="submit" class="btn-primary" style="flex:1; justify-content:center;" :disabled="saving">
              <span v-if="saving" class="tv-spinner" style="width:14px; height:14px; border-width:2px;"></span>
              <Plus v-else class="w-4 h-4" />
              {{ editingId ? 'Save Changes' : 'Create Test' }}
            </button>
            <button type="button" @click="closeModal" class="btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Delete Confirm Modal -->
    <div v-if="confirm.show" class="tv-overlay" @click.self="confirm.show = false">
      <div class="tv-modal" style="max-width:400px;">
        <div class="tv-modal-header">
          <span>Delete Test</span>
          <button @click="confirm.show = false" class="btn-ghost" style="padding:0.4rem;">
            <X class="w-4 h-4" />
          </button>
        </div>
        <div style="padding:1.5rem 1.5rem 0;">
          <p style="font-size:0.9rem; color:var(--t-text2);">
            Are you sure you want to delete
            <strong style="color:var(--t-text1);">{{ confirm.title }}</strong>?
          </p>
          <p style="font-size:0.8rem; color:var(--t-text3); margin-top:0.4rem;">This cannot be undone.</p>
        </div>
        <div class="flex gap-2" style="padding:1.25rem 1.5rem;">
          <button @click="doDelete" :disabled="saving"
                  class="btn-primary"
                  style="background:#ef4444; border-color:#ef4444; flex:1; justify-content:center;">
            <Trash2 class="w-4 h-4" /> Delete
          </button>
          <button @click="confirm.show = false" class="btn-secondary">Cancel</button>
        </div>
      </div>
    </div>

    <!-- Question Picker Modal -->
    <div v-if="qpTest" class="tv-overlay" @click.self="closeQP">
      <div class="tv-modal" style="max-width:680px; max-height:92vh; display:flex; flex-direction:column;">

        <!-- Header -->
        <div class="tv-modal-header">
          <span>Manage Questions — <span style="color:var(--t-accent);">{{ qpTest.title }}</span></span>
          <button @click="closeQP" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>

        <!-- "In this test" chip bar -->
        <div style="padding:0.6rem 1.25rem; border-bottom:1px solid var(--t-border);
                    background:var(--t-acc-alpha-sm); min-height:52px;">
          <div style="font-size:0.7rem; font-weight:600; color:var(--t-accent);
                      text-transform:uppercase; letter-spacing:0.06em; margin-bottom:0.4rem;">
            In this test ({{ qpCurrentQuestions.length }})
          </div>
          <div v-if="!qpCurrentQuestions.length"
               style="font-size:0.78rem; color:var(--t-text3);">
            None yet — add questions from the list below.
          </div>
          <div v-else style="display:flex; flex-wrap:wrap; gap:0.35rem; max-height:88px; overflow-y:auto;">
            <span v-for="q in qpCurrentQuestions" :key="q.id" class="qp-chip">
              <span class="qp-chip-text" :title="q.stem">{{ q.stem }}</span>
              <button @click="qpRemove(q.id)" class="qp-chip-x" title="Remove">
                <X class="w-3 h-3" />
              </button>
            </span>
          </div>
        </div>

        <!-- Search + level filter toolbar -->
        <div style="padding:0.6rem 1.25rem; border-bottom:1px solid var(--t-border);
                    display:flex; align-items:center; gap:0.5rem; flex-wrap:wrap;">
          <input v-model="qpSearch" type="text" placeholder="Search questions…"
                 class="input" style="flex:1; min-width:160px; font-size:0.82rem; padding:0.38rem 0.7rem;"
                 @input="onQPSearch" />
          <select v-model="qpDifficulty" class="input"
                  style="width:120px; font-size:0.82rem; padding:0.38rem 0.7rem;"
                  @change="onQPDifficulty">
            <option value="">All levels</option>
            <option value="easy">Easy</option>
            <option value="medium">Medium</option>
            <option value="hard">Hard</option>
          </select>
        </div>

        <!-- Available questions list -->
        <div style="flex:1; overflow-y:auto; padding:0.4rem 1.25rem; position:relative;">
          <!-- Loading overlay -->
          <div v-if="qpLoading"
               style="position:absolute; inset:0; display:flex; align-items:center; justify-content:center; z-index:2;
                      background:rgba(0,0,0,0.04);">
            <div class="tv-spinner" style="width:22px; height:22px; border-width:3px;"></div>
          </div>

          <div v-if="!qpLoading && !qpDisplayList.length"
               style="text-align:center; color:var(--t-text3); font-size:0.85rem; padding:2.5rem 1rem;">
            <span v-if="qpSearch || qpDifficulty">No questions match — try clearing the filters.</span>
            <span v-else-if="qpAvailable.length && !qpDisplayList.length">All questions on this page are already in the test.</span>
            <span v-else-if="qpTest.gradeCode">No questions found for {{ gradeLabel(qpTest.gradeCode) }}.</span>
            <span v-else>No questions found — add some in the Questions tab first.</span>
          </div>

          <div v-for="q in qpDisplayList" :key="q.id" class="qp-row">
            <div style="flex:1; min-width:0;">
              <div style="font-size:0.82rem; color:var(--t-text1); line-height:1.45; word-break:break-word;">
                {{ q.stem }}
              </div>
              <div class="flex items-center gap-1.5" style="margin-top:0.3rem; flex-wrap:wrap;">
                <span class="badge-indigo" style="font-size:0.65rem; text-transform:capitalize;">{{ q.kind }}</span>
                <span v-if="q.difficulty"
                      :class="q.difficulty === 'easy' ? 'badge-green' : q.difficulty === 'hard' ? 'badge-red' : 'badge-amber'"
                      style="font-size:0.65rem; text-transform:capitalize;">{{ q.difficulty }}</span>
                <span v-if="q.marks" style="font-size:0.65rem; color:var(--t-text3);">
                  {{ q.marks }} mark{{ q.marks !== 1 ? 's' : '' }}
                </span>
                <span v-if="q.isAiGenerated" style="font-size:0.65rem; color:var(--t-text3);">🤖 AI</span>
              </div>
            </div>
            <button @click="qpAdd(q)" class="btn-ghost"
                    style="padding:0.3rem 0.5rem; flex-shrink:0; color:var(--t-accent);" title="Add to test">
              <Plus class="w-4 h-4" />
            </button>
          </div>
        </div>

        <!-- Footer: pagination + count + actions -->
        <div style="padding:0.7rem 1.25rem; border-top:1px solid var(--t-border); display:flex; flex-direction:column; gap:0.55rem;">
          <div style="display:flex; align-items:center; gap:0.4rem;">
            <button @click="qpPrev" :disabled="qpPage <= 1 || qpLoading"
                    class="btn-ghost" style="padding:0.25rem 0.6rem; font-size:0.85rem;">‹</button>
            <span style="font-size:0.75rem; color:var(--t-text3); flex:1; text-align:center;">
              Page {{ qpPage }} of {{ qpTotalPages }} · {{ qpTotal }} questions available
            </span>
            <button @click="qpNext" :disabled="qpPage >= qpTotalPages || qpLoading"
                    class="btn-ghost" style="padding:0.25rem 0.6rem; font-size:0.85rem;">›</button>
          </div>
          <div style="display:flex; align-items:center; gap:0.75rem;">
            <span style="flex:1; font-size:0.82rem; color:var(--t-text2);">
              <strong style="color:var(--t-text1);">{{ qpCurrentQuestions.length }}</strong>
              question{{ qpCurrentQuestions.length !== 1 ? 's' : '' }} in this test
            </span>
            <button @click="closeQP" class="btn-secondary">Cancel</button>
            <button @click="saveQP" class="btn-primary" :disabled="qpSaving">
              <span v-if="qpSaving" class="tv-spinner" style="width:14px; height:14px; border-width:2px;"></span>
              <Check v-else class="w-4 h-4" />
              Save
            </button>
          </div>
        </div>

      </div>
    </div>

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
          <Calendar class="w-3.5 h-3.5" /> {{ totalCount }} Total
        </span>
      </div>
      <div class="ag-banner-actions">
        <RouterLink to="/app/admin" class="btn-secondary" style="gap:0.4rem;">
          <ArrowLeft class="w-4 h-4" /> Admin
        </RouterLink>
        <button @click="openAdd" class="btn-primary text-sm">
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
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ draftTests }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Drafts</div>
        </div>
      </div>
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background:var(--t-acc-alpha-sm);">
          <FileText class="w-5 h-5" style="color:var(--t-accent);" />
        </div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ totalCount }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Total Tests</div>
        </div>
      </div>
    </div>

    <!-- Card with view toggle + data -->
    <div class="ag-card">

      <!-- Loading state -->
      <div v-if="loading" style="padding:3rem; text-align:center; color:var(--t-text3);">
        <div class="tv-spinner" style="width:28px; height:28px; border-width:3px; margin:0 auto 0.75rem;"></div>
        Loading tests…
      </div>

      <template v-else>
        <div style="padding:0.85rem 1.25rem; border-bottom:1px solid var(--t-border);
                    display:flex; align-items:center; justify-content:space-between;">
          <span style="font-size:0.82rem; color:var(--t-text2);">
            <strong style="color:var(--t-text1);">{{ totalCount }}</strong> tests total
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

        <!-- Empty state -->
        <div v-if="!tests.length"
             style="padding:3rem; text-align:center; color:var(--t-text3); font-size:0.9rem;">
          No tests yet. Click <strong>Create Test</strong> to add one.
        </div>

        <!-- GRID VIEW -->
        <div v-else-if="view === 'grid'"
             style="padding:1.25rem; display:grid;
                    grid-template-columns:repeat(auto-fill, minmax(300px, 1fr)); gap:1rem;">
          <div v-for="t in tests" :key="t.id" class="ag-grid-card" style="padding:0; overflow:hidden;">
            <div :class="kindGrad(t.kind)" style="height:5px; width:100%;"></div>
            <div style="padding:1rem 1.1rem;">
              <!-- Title + status -->
              <div class="flex items-start justify-between gap-2" style="margin-bottom:0.6rem;">
                <div style="font-weight:600; font-size:0.9rem; color:var(--t-text1); line-height:1.35; flex:1;">
                  {{ t.title }}
                </div>
                <span :class="statusClass(t)" style="flex-shrink:0;">{{ deriveStatus(t) }}</span>
              </div>

              <!-- Kind badge + grade/subject -->
              <div class="flex items-center gap-1.5 flex-wrap" style="margin-bottom:0.75rem;">
                <span class="badge-indigo" style="font-size:0.7rem; text-transform:capitalize;">{{ t.kind }}</span>
                <span v-if="t.gradeCode" style="font-size:0.7rem; color:var(--t-text3);">
                  {{ gradeLabel(t.gradeCode) }}
                </span>
                <span v-if="t.subjectId" style="font-size:0.7rem; color:var(--t-text3);">
                  · {{ subjectMap.get(t.subjectId) ?? `Subject #${t.subjectId}` }}
                </span>
              </div>

              <!-- Duration + questions -->
              <div style="display:flex; flex-direction:column; gap:0.3rem; margin-bottom:0.8rem;">
                <div class="flex items-center gap-1.5" style="font-size:0.75rem; color:var(--t-text3);">
                  <Clock class="w-3 h-3" style="flex-shrink:0;" />
                  <span>{{ t.durationMin }} min · {{ t.questionCount }} questions</span>
                </div>
                <div v-if="t.scheduledAt" class="flex items-center gap-1.5"
                     style="font-size:0.75rem; color:var(--t-text3);">
                  <Calendar class="w-3 h-3" style="flex-shrink:0;" />
                  <span>Opens: <span style="color:var(--t-text2);">{{ fmtDate(t.scheduledAt) }}</span></span>
                </div>
                <div v-if="t.endsAt" class="flex items-center gap-1.5"
                     style="font-size:0.75rem; color:var(--t-text3);">
                  <Calendar class="w-3 h-3" style="flex-shrink:0;" />
                  <span>Closes: <span style="color:var(--t-text2);">{{ fmtDate(t.endsAt) }}</span></span>
                </div>
              </div>

              <!-- Footer: marks + actions -->
              <div class="flex items-center justify-between"
                   style="border-top:1px solid var(--t-border); padding-top:0.65rem;">
                <div class="flex items-center gap-1" style="font-size:0.75rem; color:var(--t-text3);">
                  <FileText class="w-3.5 h-3.5" />
                  <span>{{ t.totalMarks ? t.totalMarks + ' marks' : 'No marks set' }}</span>
                </div>
                <div class="flex gap-1">
                  <button @click="togglePublish(t)" class="btn-ghost" style="padding:0.3rem 0.5rem;"
                          :title="t.isPublished ? 'Unpublish' : 'Publish'">
                    <Eye v-if="!t.isPublished" class="w-3.5 h-3.5" style="color:#10b981;" />
                    <EyeOff v-else class="w-3.5 h-3.5" style="color:#f59e0b;" />
                  </button>
                  <button @click="openQP(t)" class="btn-ghost" style="padding:0.3rem 0.5rem;" title="Manage Questions">
                    <ListChecks class="w-3.5 h-3.5" style="color:#6366f1;" />
                  </button>
                  <button @click="openEdit(t)" class="btn-ghost" style="padding:0.3rem 0.5rem;" title="Edit">
                    <Pencil class="w-3.5 h-3.5" />
                  </button>
                  <button @click="askDelete(t)" class="btn-ghost"
                          style="padding:0.3rem 0.5rem; color:#ef4444;" title="Delete">
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
                <th>Kind</th>
                <th>Grade / Subject</th>
                <th>Duration</th>
                <th>Questions</th>
                <th>Status</th>
                <th>Opens / Closes</th>
                <th style="width:120px;">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="t in tests" :key="t.id">
                <td style="font-weight:500; color:var(--t-text1); max-width:240px;">
                  <div style="overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">{{ t.title }}</div>
                </td>
                <td>
                  <span class="badge-indigo" style="text-transform:capitalize;">{{ t.kind }}</span>
                </td>
                <td style="font-size:0.8rem; color:var(--t-text2);">
                  <span v-if="t.gradeCode">{{ gradeLabel(t.gradeCode) }}</span>
                  <span v-if="t.gradeCode && t.subjectId"> · </span>
                  <span v-if="t.subjectId">
                    {{ subjectMap.get(t.subjectId) ?? `Subject #${t.subjectId}` }}
                  </span>
                  <span v-if="!t.gradeCode && !t.subjectId" style="color:var(--t-text3);">—</span>
                </td>
                <td class="ag-table-muted">{{ t.durationMin }} min</td>
                <td class="ag-table-muted">{{ t.questionCount }}</td>
                <td><span :class="statusClass(t)">{{ deriveStatus(t) }}</span></td>
                <td class="ag-table-muted" style="font-size:0.78rem;">
                  <div v-if="t.scheduledAt">{{ fmtDate(t.scheduledAt) }}</div>
                  <div v-if="t.endsAt" style="color:var(--t-text3);">→ {{ fmtDate(t.endsAt) }}</div>
                  <span v-if="!t.scheduledAt">—</span>
                </td>
                <td>
                  <div class="flex gap-1">
                    <button @click="togglePublish(t)" class="btn-ghost" style="padding:0.3rem;"
                            :title="t.isPublished ? 'Unpublish' : 'Publish'">
                      <Eye v-if="!t.isPublished" class="w-3.5 h-3.5" style="color:#10b981;" />
                      <EyeOff v-else class="w-3.5 h-3.5" style="color:#f59e0b;" />
                    </button>
                    <button @click="openQP(t)" class="btn-ghost" style="padding:0.3rem;" title="Manage Questions">
                      <ListChecks class="w-3.5 h-3.5" style="color:#6366f1;" />
                    </button>
                    <button @click="openEdit(t)" class="btn-ghost" style="padding:0.3rem;" title="Edit">
                      <Pencil class="w-3.5 h-3.5" />
                    </button>
                    <button @click="askDelete(t)" class="btn-ghost"
                            style="padding:0.3rem; color:#ef4444;" title="Delete">
                      <Trash2 class="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { RouterLink } from 'vue-router'
import {
  ArrowLeft, Plus, Pencil, Trash2, LayoutGrid, List,
  Calendar, Clock, FileText, Eye, EyeOff, X, ListChecks, Check,
} from '@lucide/vue'
import api from '@/services/api'
import { useCatalogStore } from '@/stores/catalog'
import DateTimePicker from '@/components/ui/DateTimePicker.vue'

const catalog = useCatalogStore()

// ── View ──────────────────────────────────────────────────────────
const view = ref('grid')

// ── State ─────────────────────────────────────────────────────────
const tests      = ref([])
const totalCount = ref(0)
const loading    = ref(false)
const saving     = ref(false)
const errMsg     = ref('')

// ── Modal ─────────────────────────────────────────────────────────
const showModal      = ref(false)
const editingId      = ref(null)
const subjectLoading = ref(false)

const defaultForm = () => ({
  title:       '',
  kind:        'objective',
  gradeCode:   '',
  subjectIds:  [],
  durationMin: 30,
  totalMarks:  null,
  scheduledAt: '',
  endsAt:      '',
})
const form = ref(defaultForm())

// ── Delete confirm ────────────────────────────────────────────────
const confirm = ref({ show: false, id: null, title: '' })

// ── Subjects for modal (dynamic) ──────────────────────────────────
const modalSubjects = computed(() =>
  form.value.gradeCode ? (catalog.subjectsForGrade(form.value.gradeCode) ?? []) : []
)

watch(() => form.value.gradeCode, async (code) => {
  if (!editingId.value) form.value.subjectIds = []
  if (code) {
    subjectLoading.value = true
    await catalog.fetchSubjectsForGrade(code)
    subjectLoading.value = false
  }
})

// Auto-clear endsAt if scheduledAt moves to be equal or after it
watch(() => form.value.scheduledAt, (newSched) => {
  if (newSched && form.value.endsAt && form.value.endsAt <= newSched) {
    form.value.endsAt = ''
  }
})

// ── Subject lookup map (all cached grades) ────────────────────────
const subjectMap = computed(() => {
  const map = new Map()
  for (const g of catalog.grades) {
    const subs = catalog.subjectsForGrade(g.code)
    if (subs) subs.forEach(s => map.set(s.id, s.name))
  }
  return map
})

// ── Question Picker ───────────────────────────────────────────────
const qpTest             = ref(null)
const qpCurrentQuestions = ref([])   // full question objects already in the test
const qpAvailable        = ref([])   // current page from backend (unselected only shown)
const qpTotal            = ref(0)
const qpPage             = ref(1)
const qpPageSize         = 15
const qpLoading          = ref(false)
const qpSaving           = ref(false)
const qpSearch           = ref('')
const qpDifficulty       = ref('')
let   qpDebounce         = null

const qpSelectedSet = computed(() => new Set(qpCurrentQuestions.value.map(q => q.id)))
const qpDisplayList = computed(() => qpAvailable.value.filter(q => !qpSelectedSet.value.has(q.id)))
const qpTotalPages  = computed(() => Math.max(1, Math.ceil(qpTotal.value / qpPageSize)))

async function loadQPPage() {
  if (!qpTest.value) return
  qpLoading.value = true
  try {
    const res = await api.get('/questions', {
      params: {
        gradeCode:  qpTest.value.gradeCode || undefined,
        subjectId:  qpTest.value.subjectId || undefined,
        search:     qpSearch.value.trim()  || undefined,
        difficulty: qpDifficulty.value     || undefined,
        page:       qpPage.value,
        pageSize:   qpPageSize,
      },
    })
    qpAvailable.value = res.data.items ?? []
    qpTotal.value     = res.data.totalCount ?? qpAvailable.value.length
  } catch (e) {
    console.error('QP load failed', e)
  } finally {
    qpLoading.value = false
  }
}

async function openQP(t) {
  qpTest.value             = t
  qpSearch.value           = ''
  qpDifficulty.value       = ''
  qpPage.value             = 1
  qpCurrentQuestions.value = []
  qpLoading.value          = true
  try {
    const [testRes, qRes] = await Promise.all([
      api.get(`/tests/${t.id}`),
      api.get('/questions', {
        params: {
          gradeCode: t.gradeCode || undefined,
          subjectId: t.subjectId || undefined,
          page:      1,
          pageSize:  qpPageSize,
        },
      }),
    ])
    qpCurrentQuestions.value = testRes.data.questions ?? []
    qpAvailable.value        = qRes.data.items ?? []
    qpTotal.value            = qRes.data.totalCount ?? qpAvailable.value.length
  } catch (e) {
    console.error('QP open failed', e)
  } finally {
    qpLoading.value = false
  }
}

function closeQP() { qpTest.value = null }

function qpAdd(q) {
  if (!qpSelectedSet.value.has(q.id)) qpCurrentQuestions.value.push(q)
}
function qpRemove(id) {
  qpCurrentQuestions.value = qpCurrentQuestions.value.filter(q => q.id !== id)
}

function onQPSearch() {
  clearTimeout(qpDebounce)
  qpDebounce = setTimeout(() => { qpPage.value = 1; loadQPPage() }, 350)
}
function onQPDifficulty() { qpPage.value = 1; loadQPPage() }
function qpPrev() { if (qpPage.value > 1)                  { qpPage.value--; loadQPPage() } }
function qpNext() { if (qpPage.value < qpTotalPages.value)  { qpPage.value++; loadQPPage() } }

async function saveQP() {
  if (!qpTest.value) return
  qpSaving.value = true
  try {
    const ids = qpCurrentQuestions.value.map(q => q.id)
    await api.put(`/tests/${qpTest.value.id}/questions`, ids)
    const t = tests.value.find(x => x.id === qpTest.value.id)
    if (t) t.questionCount = ids.length
    closeQP()
  } catch (e) {
    console.error('Failed to save questions', e)
  } finally {
    qpSaving.value = false
  }
}

// ── Stats ─────────────────────────────────────────────────────────
const activeTests    = computed(() => tests.value.filter(t => t.isPublished && !t.isScheduled).length)
const scheduledTests = computed(() => tests.value.filter(t => t.isPublished && t.isScheduled).length)
const draftTests     = computed(() => tests.value.filter(t => !t.isPublished).length)

// ── Helpers ───────────────────────────────────────────────────────
function deriveStatus(t) {
  if (!t.isPublished) return 'Draft'
  if (t.isScheduled)  return 'Scheduled'
  return 'Active'
}

function statusClass(t) {
  const s = deriveStatus(t)
  return { Active: 'badge-green', Scheduled: 'badge-amber', Draft: 'badge-indigo' }[s] ?? 'badge-indigo'
}

function kindGrad(kind) {
  const map = {
    objective: 'grad-teal',
    subjective: 'grad-rose',
    mixed: 'grad-violet',
    daily: 'grad-amber',
    weekly: 'grad-blue',
    monthly: 'grad-rose',
    entrance: 'grad-green',
  }
  return map[kind] ?? 'grad-violet'
}

function gradeLabel(code) {
  return catalog.grades.find(g => g.code === code)?.label ?? code
}

function fmtDate(dt) {
  if (!dt) return '—'
  return new Date(dt).toLocaleDateString('en-PK', { day: '2-digit', month: 'short', year: 'numeric', timeZone: 'Asia/Karachi' })
}

// ── Load ──────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const res = await api.get('/tests', { params: { published: false, pageSize: 200 } })
    tests.value  = res.data.items
    totalCount.value = res.data.totalCount ?? res.data.total ?? tests.value.length
    // Pre-fetch subjects for all grades that appear in the test list
    const uniqueGrades = [...new Set(tests.value.map(t => t.gradeCode).filter(Boolean))]
    await Promise.all(uniqueGrades.map(g => catalog.fetchSubjectsForGrade(g)))
  } catch (e) {
    console.error('Failed to load tests', e)
  } finally {
    loading.value = false
  }
}

load()

// ── CRUD ──────────────────────────────────────────────────────────
function openAdd() {
  editingId.value = null
  form.value = defaultForm()
  errMsg.value = ''
  showModal.value = true
}

function openEdit(t) {
  editingId.value = t.id
  form.value = {
    title:       t.title,
    kind:        t.kind,
    gradeCode:   t.gradeCode ?? '',
    subjectIds:  t.subjectId ? [t.subjectId] : [],
    durationMin: t.durationMin,
    totalMarks:  t.totalMarks ?? null,
    scheduledAt: t.scheduledAt ? t.scheduledAt.substring(0, 16) : '',
    endsAt:      t.endsAt      ? t.endsAt.substring(0, 16)      : '',
  }
  if (t.gradeCode) catalog.fetchSubjectsForGrade(t.gradeCode)
  errMsg.value = ''
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  editingId.value = null
}

async function save() {
  errMsg.value = ''
  if (form.value.scheduledAt && form.value.endsAt && form.value.endsAt <= form.value.scheduledAt) {
    errMsg.value = 'End date & time must be after the scheduled date & time.'
    return
  }
  saving.value = true
  try {
    const payload = {
      title:       form.value.title,
      kind:        form.value.kind,
      gradeCode:   form.value.gradeCode || null,
      subjectId:   form.value.subjectIds[0] ?? null,
      durationMin: form.value.durationMin || 30,
      totalMarks:  form.value.totalMarks  || null,
      isScheduled: !!form.value.scheduledAt,
      scheduledAt: form.value.scheduledAt || null,
      endsAt:      form.value.endsAt      || null,
    }
    if (editingId.value) {
      await api.put(`/tests/${editingId.value}`, payload)
    } else {
      await api.post('/tests', payload)
    }
    closeModal()
    load()
  } catch (e) {
    errMsg.value = e.message ?? 'Failed to save test.'
  } finally {
    saving.value = false
  }
}

function askDelete(t) {
  confirm.value = { show: true, id: t.id, title: t.title }
}

async function doDelete() {
  saving.value = true
  try {
    await api.delete(`/tests/${confirm.value.id}`)
    confirm.value.show = false
    load()
  } catch (e) {
    console.error('Delete failed', e)
  } finally {
    saving.value = false
  }
}

async function togglePublish(t) {
  try {
    const res = await api.patch(`/tests/${t.id}/publish`)
    t.isPublished = res.data.isPublished
  } catch (e) {
    console.error('Toggle publish failed', e)
  }
}
</script>

<style scoped>
.tv-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 1rem;
}

.tv-modal {
  background: var(--t-bg2);
  border: 1px solid var(--t-border);
  border-radius: 16px;
  width: 100%;
  max-width: 520px;
  max-height: 90vh;
  overflow-y: auto;
}

.tv-modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.1rem 1.5rem;
  border-bottom: 1px solid var(--t-border);
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--t-text1);
  position: sticky;
  top: 0;
  background: var(--t-bg2);
  z-index: 1;
}

.tv-modal-body {
  padding: 1.25rem 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.tv-spinner {
  width: 18px;
  height: 18px;
  border: 2px solid var(--t-border);
  border-top-color: var(--t-accent);
  border-radius: 50%;
  animation: tv-spin 0.6s linear infinite;
  display: inline-block;
  vertical-align: middle;
}

@keyframes tv-spin {
  to { transform: rotate(360deg); }
}

.qp-row {
  display: flex;
  align-items: flex-start;
  gap: 0.65rem;
  padding: 0.55rem 0.65rem;
  border-radius: 10px;
  border: 1px solid transparent;
  margin-bottom: 0.25rem;
  transition: background 0.12s;
}
.qp-row:hover { background: var(--t-bg3); }

.qp-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  background: var(--t-bg2);
  border: 1px solid var(--t-border);
  border-radius: 999px;
  padding: 0.18rem 0.35rem 0.18rem 0.65rem;
  font-size: 0.72rem;
  color: var(--t-text2);
  max-width: 260px;
}
.qp-chip-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.qp-chip-x {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--t-text3);
  padding: 0;
  display: flex;
  align-items: center;
  flex-shrink: 0;
  border-radius: 50%;
  transition: color 0.12s;
}
.qp-chip-x:hover { color: #ef4444; }

.badge-red {
  display: inline-flex;
  align-items: center;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  background: rgba(239,68,68,0.12);
  color: #ef4444;
}
</style>
