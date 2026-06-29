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
        <span class="ag-banner-stat"><span>📋</span> {{ totalObjective.toLocaleString() }} Objective</span>
        <span class="ag-banner-stat"><span>✍️</span> {{ totalSubjective.toLocaleString() }} Subjective</span>
        <span class="ag-banner-stat"><span>🎯</span> {{ (totalObjective + totalSubjective).toLocaleString() }} Total</span>
      </div>
      <div class="ag-banner-actions">
        <button @click="openAI" class="btn-secondary text-sm">
          <Sparkles class="w-4 h-4" /> AI Generate
        </button>
        <button @click="openAdd" class="btn-primary text-sm">
          <Plus class="w-4 h-4" /> Add Question
        </button>
      </div>
    </div>

    <!-- Stats Row -->
    <div style="display:grid; grid-template-columns:repeat(4,1fr); gap:0.75rem;">
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center text-lg flex-shrink-0"
             style="background:var(--t-acc-alpha-sm); color:var(--t-accent);">📋</div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ totalObjective.toLocaleString() }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">Objective Questions</div>
        </div>
      </div>
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center text-lg flex-shrink-0"
             style="background:rgba(16,185,129,0.1); color:#10b981;">✍️</div>
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
      <div class="ag-card ag-card-body flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center text-lg flex-shrink-0"
             style="background:rgba(139,92,246,0.1); color:#8b5cf6;">✨</div>
        <div>
          <div style="font-size:1.4rem; font-weight:700; color:var(--t-text1); line-height:1.2;">{{ aiCount }}</div>
          <div style="font-size:0.72rem; color:var(--t-text3); font-weight:500;">AI Generated</div>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="ag-card">
      <div class="ag-filter-bar">
        <div class="ag-filter-group">
          <label class="ag-filter-label">Grade</label>
          <select v-model="filterGrade" class="input" style="width:130px;">
            <option :value="null">All Grades</option>
            <option v-for="g in catalog.grades" :key="g.code" :value="g.code">{{ g.label }}</option>
          </select>
        </div>
        <div class="ag-filter-group">
          <label class="ag-filter-label">Subject</label>
          <select v-model="filterSubject" class="input" style="width:150px;" :disabled="!filterGrade">
            <option :value="null">{{ filterGrade ? 'All Subjects' : 'Select grade first' }}</option>
            <option v-for="s in filterGradeSubjects" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div class="ag-filter-group">
          <label class="ag-filter-label">Type</label>
          <select v-model="filterType" class="input" style="width:130px;">
            <option value="">All Types</option>
            <option value="objective">Objective</option>
            <option value="subjective">Subjective</option>
          </select>
        </div>
        <div class="ag-filter-group">
          <label class="ag-filter-label">Difficulty</label>
          <select v-model="filterDiff" class="input" style="width:120px;">
            <option value="">All Levels</option>
            <option>Easy</option>
            <option>Medium</option>
            <option>Hard</option>
          </select>
        </div>
        <div class="ag-filter-group" style="flex:1; min-width:190px;">
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
            <button class="ag-view-btn" :class="{ active: view === 'list' }" @click="view = 'list'" title="List view"><List class="w-3.5 h-3.5" /></button>
            <button class="ag-view-btn" :class="{ active: view === 'grid' }" @click="view = 'grid'" title="Grid view"><LayoutGrid class="w-3.5 h-3.5" /></button>
          </div>
        </div>
      </div>

      <!-- Count row -->
      <div style="padding:0.5rem 1.5rem; border-bottom:1px solid var(--t-border); display:flex; align-items:center; gap:0.5rem;">
        <Filter class="w-3 h-3" style="color:var(--t-text3);" />
        <span style="font-size:0.75rem; color:var(--t-text3);">
          Showing <strong style="color:var(--t-text1);">{{ questions.length }}</strong>
          of <strong style="color:var(--t-text1);">{{ totalCount }}</strong> questions
          <span v-if="loading" style="margin-left:0.5rem; opacity:0.6;">(loading…)</span>
        </span>
      </div>

      <!-- LIST VIEW -->
      <div v-if="view === 'list'" class="overflow-x-auto">
        <table class="ag-table">
          <thead>
            <tr>
              <th style="width:40px;">#</th>
              <th>Question</th>
              <th>Grade · Subject</th>
              <th>Type</th>
              <th>Difficulty</th>
              <th>Cognitive Level</th>
              <th style="width:80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading && questions.length === 0">
              <td colspan="7"><div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading questions…</p></div></td>
            </tr>
            <tr v-for="(q, i) in questions" :key="q.id">
              <td class="ag-table-muted" style="text-align:center;">{{ (page - 1) * pageSize + i + 1 }}</td>
              <td style="max-width:320px;">
                <div style="overflow:hidden; text-overflow:ellipsis; white-space:nowrap; color:var(--t-text1);" :title="q.stem">
                  {{ q.stem.length > 60 ? q.stem.slice(0, 60) + '…' : q.stem }}
                </div>
                <div v-if="q.isAiGenerated" style="font-size:0.68rem; color:#8b5cf6; margin-top:2px;">✨ AI Generated</div>
              </td>
              <td>
                <div style="font-size:0.78rem; color:var(--t-text2);">
                  <span v-if="q.gradeCode">{{ gradeLabel(q.gradeCode) }}</span>
                  <span v-if="q.gradeCode && q.subjectId" style="margin:0 3px; opacity:0.4;">·</span>
                  <span v-if="q.subjectId">{{ subjectName(q.subjectId) }}</span>
                  <span v-if="!q.gradeCode && !q.subjectId" style="opacity:0.4;">—</span>
                </div>
              </td>
              <td>
                <span :class="q.kind === 'objective' ? 'badge-indigo' : 'badge-purple'">
                  {{ q.kind === 'objective' ? 'Objective' : 'Subjective' }}
                </span>
              </td>
              <td><span :class="diffClass(q.difficulty)">{{ q.difficulty || '—' }}</span></td>
              <td><span class="qb-bloom" :class="bloomClass(q.cognitiveLevel)">{{ q.cognitiveLevel || '—' }}</span></td>
              <td>
                <div class="flex gap-1">
                  <button @click="openEdit(q)" class="btn-ghost" style="padding:0.3rem;" title="Edit"><Pencil class="w-3.5 h-3.5" /></button>
                  <button @click="removeQuestion(q)" class="btn-ghost" style="padding:0.3rem; color:#ef4444;" title="Delete"><Trash2 class="w-3.5 h-3.5" /></button>
                </div>
              </td>
            </tr>
            <tr v-if="!loading && questions.length === 0">
              <td colspan="7"><div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No questions match your filters.</p></div></td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- GRID VIEW -->
      <div v-else style="padding:1.25rem; display:grid; grid-template-columns:repeat(2,1fr); gap:1rem;">
        <div v-if="loading && questions.length === 0" style="grid-column:1/-1;">
          <div class="ag-empty"><div class="ag-empty-icon">⏳</div><p>Loading questions…</p></div>
        </div>
        <div v-for="q in questions" :key="q.id" class="ag-grid-card">
          <div class="flex items-start justify-between gap-2" style="margin-bottom:0.75rem;">
            <div class="flex items-center gap-1.5 flex-wrap">
              <span :class="q.kind === 'objective' ? 'badge-indigo' : 'badge-purple'">
                {{ q.kind === 'objective' ? 'Objective' : 'Subjective' }}
              </span>
              <span v-if="q.subjectId" style="font-size:0.7rem; padding:0.2rem 0.55rem; border-radius:99px; background:var(--t-hover2); color:var(--t-text2);">
                {{ subjectName(q.subjectId) }}
              </span>
              <span v-if="q.isAiGenerated" style="font-size:0.68rem; color:#8b5cf6;">✨ AI</span>
            </div>
            <span :class="diffClass(q.difficulty)" style="flex-shrink:0;">{{ q.difficulty || '—' }}</span>
          </div>
          <p style="font-size:0.83rem; color:var(--t-text1); line-height:1.5; margin-bottom:0.7rem;
                     display:-webkit-box; -webkit-box-orient:vertical; -webkit-line-clamp:3; line-clamp:3; overflow:hidden;">
            {{ q.stem }}
          </p>
          <div v-if="q.kind === 'objective' && parseOptions(q.optionsJson).length" style="margin-bottom:0.7rem; display:flex; flex-direction:column; gap:0.3rem;">
            <div v-for="(opt, idx) in parseOptions(q.optionsJson).slice(0, 2)" :key="idx"
                 style="font-size:0.72rem; display:flex; align-items:center; gap:0.4rem;"
                 :style="idx === q.correctIndex ? 'color:#10b981;' : 'color:var(--t-text3);'">
              <span style="width:16px; height:16px; border-radius:4px; border:1px solid var(--t-border); display:flex; align-items:center; justify-content:center; flex-shrink:0; font-size:0.6rem; font-weight:700;"
                    :style="idx === q.correctIndex ? 'background:rgba(16,185,129,0.15); border-color:#10b981; color:#10b981;' : ''">
                {{ 'ABCD'[idx] }}
              </span>
              <span style="overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">{{ opt }}</span>
            </div>
            <div v-if="parseOptions(q.optionsJson).length > 2" style="font-size:0.68rem; color:var(--t-text3);">+ {{ parseOptions(q.optionsJson).length - 2 }} more options</div>
          </div>
          <div class="flex items-center justify-between" style="border-top:1px solid var(--t-border); padding-top:0.6rem; margin-top:auto;">
            <span class="qb-bloom" :class="bloomClass(q.cognitiveLevel)" style="font-size:0.68rem;">{{ q.cognitiveLevel || '—' }}</span>
            <div class="flex gap-1">
              <button @click="openEdit(q)" class="btn-ghost" style="padding:0.25rem 0.5rem;" title="Edit"><Pencil class="w-3.5 h-3.5" /></button>
              <button @click="removeQuestion(q)" class="btn-ghost" style="padding:0.25rem 0.5rem; color:#ef4444;" title="Delete"><Trash2 class="w-3.5 h-3.5" /></button>
            </div>
          </div>
        </div>
        <div v-if="!loading && questions.length === 0" style="grid-column:1/-1;">
          <div class="ag-empty"><div class="ag-empty-icon">🔍</div><p>No questions match your filters.</p></div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" style="padding:0.75rem 1.5rem; border-top:1px solid var(--t-border); display:flex; align-items:center; gap:0.75rem; justify-content:center;">
        <button @click="prevPage" :disabled="page <= 1 || loading" class="btn-secondary" style="padding:0.3rem 0.75rem; font-size:0.78rem;">← Prev</button>
        <span style="font-size:0.78rem; color:var(--t-text2);">Page {{ page }} of {{ totalPages }}</span>
        <button @click="nextPage" :disabled="page >= totalPages || loading" class="btn-secondary" style="padding:0.3rem 0.75rem; font-size:0.78rem;">Next →</button>
      </div>
    </div>

    <!-- ── Add / Edit Modal ──────────────────────────────────────────── -->
    <div v-if="showModal" class="qb-overlay" @click.self="closeModal">
      <div class="qb-modal">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title">{{ editingId ? 'Edit Question' : 'Add Question' }}</h3>
          <button @click="closeModal" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>
        <form class="qb-modal-body" @submit.prevent="saveQuestion">

          <!-- Type (read-only in edit) -->
          <div>
            <label class="label">Question Type</label>
            <div class="flex gap-2">
              <button v-for="t in typeOpts" :key="t.v" type="button"
                :disabled="!!editingId"
                @click="!editingId && (form.kind = t.v)"
                :class="['btn-secondary text-sm flex-1 justify-center', form.kind === t.v ? 'ring-2' : '']"
                :style="form.kind === t.v ? 'background:var(--t-acc-alpha-md); color:var(--t-accent); border-color:var(--t-accent);' : (editingId ? 'opacity:0.55; cursor:default;' : '')">
                {{ t.e }} {{ t.l }}
              </button>
            </div>
          </div>

          <!-- Grade + Subject (read-only in edit) -->
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div>
              <label class="label">Grade</label>
              <select v-if="!editingId" v-model="form.gradeCode" class="input" required>
                <option :value="null" disabled>Select Grade</option>
                <option v-for="g in catalog.grades" :key="g.code" :value="g.code">{{ g.label }}</option>
              </select>
              <div v-else class="qb-readonly">{{ gradeLabel(form.gradeCode) }}</div>
            </div>
            <div>
              <label class="label">Subject</label>
              <select v-if="!editingId" v-model="form.subjectId" class="input" :disabled="!form.gradeCode">
                <option :value="null">{{ form.gradeCode ? 'All / Unspecified' : 'Select grade first' }}</option>
                <option v-for="s in modalSubjects" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
              <div v-else class="qb-readonly">{{ form.subjectId ? subjectName(form.subjectId) : '—' }}</div>
            </div>
          </div>

          <!-- Stem -->
          <div>
            <label class="label">Question Stem</label>
            <textarea v-model="form.stem" class="input" style="min-height:90px; resize:vertical;"
                      placeholder="Enter the full question text…" required></textarea>
          </div>

          <!-- Objective options -->
          <template v-if="form.kind === 'objective'">
            <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
              <div v-for="(_, i) in 4" :key="i">
                <label class="label" style="font-size:0.75rem;">Option {{ 'ABCD'[i] }}</label>
                <input v-model="form.options[i]" type="text" class="input" style="font-size:0.82rem;"
                       :placeholder="'Option ' + 'ABCD'[i]" />
              </div>
            </div>
            <div>
              <label class="label" style="font-size:0.75rem;">Correct Answer</label>
              <select v-model="form.correctIndex" class="input" style="font-size:0.82rem;">
                <option v-for="(_, i) in 4" :key="i" :value="i">{{ 'ABCD'[i] }} — {{ form.options[i] || '(empty)' }}</option>
              </select>
            </div>
          </template>

          <!-- Subjective fields -->
          <template v-if="form.kind === 'subjective'">
            <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
              <div>
                <label class="label" style="font-size:0.75rem;">Question Type</label>
                <select v-model="form.questionType" class="input" style="font-size:0.82rem;">
                  <option>Short</option>
                  <option>Long</option>
                </select>
              </div>
              <div>
                <label class="label" style="font-size:0.75rem;">Marks</label>
                <input v-model.number="form.marks" type="number" min="1" max="100" class="input" style="font-size:0.82rem;" />
              </div>
            </div>
            <div>
              <label class="label" style="font-size:0.75rem;">Model Answer</label>
              <textarea v-model="form.modelAnswer" class="input" style="min-height:80px; resize:vertical; font-size:0.82rem;"
                        placeholder="Provide a model answer…"></textarea>
            </div>
          </template>

          <!-- Difficulty + Cognitive Level (Bloom's) -->
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
            <div>
              <label class="label" style="font-size:0.75rem;">Difficulty</label>
              <select v-model="form.difficulty" class="input" style="font-size:0.82rem;">
                <option>Easy</option>
                <option>Medium</option>
                <option>Hard</option>
              </select>
            </div>
            <div>
              <label class="label" style="font-size:0.75rem;">Cognitive Level (Bloom's)</label>
              <select v-model="form.cognitiveLevel" class="input" style="font-size:0.82rem;">
                <option v-for="b in bloomLevels" :key="b">{{ b }}</option>
              </select>
            </div>
          </div>

          <p v-if="errMsg" class="qb-err">{{ errMsg }}</p>

          <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
            <button type="submit" class="btn-primary" style="flex:1; justify-content:center;" :disabled="saving">
              <span v-if="saving" class="qb-spinner"></span>
              <Plus v-else class="w-4 h-4" />
              {{ saving ? 'Saving…' : (editingId ? 'Save Changes' : 'Add Question') }}
            </button>
            <button type="button" @click="closeModal" class="btn-secondary" :disabled="saving">Cancel</button>
          </div>
        </form>
      </div>
    </div>

    <!-- ── AI Generate Modal ─────────────────────────────────────────── -->
    <div v-if="aiModal" class="qb-overlay" @click.self="closeAI">
      <div class="qb-modal qb-modal-wide">
        <div class="qb-modal-header">
          <h3 class="qb-modal-title"><Sparkles class="w-4 h-4" style="display:inline;vertical-align:middle;margin-right:5px;" />AI Question Generator</h3>
          <button @click="closeAI" class="btn-ghost" style="padding:0.4rem;"><X class="w-4 h-4" /></button>
        </div>

        <div class="qb-modal-body">

          <!-- Config phase -->
          <template v-if="!aiResults.length">
            <div>
              <label class="label">Question Type</label>
              <div class="flex gap-2">
                <button v-for="t in typeOpts" :key="t.v" type="button"
                  @click="aiForm.kind = t.v"
                  :class="['btn-secondary text-sm flex-1 justify-center', aiForm.kind === t.v ? 'ring-2' : '']"
                  :style="aiForm.kind === t.v ? 'background:var(--t-acc-alpha-md); color:var(--t-accent); border-color:var(--t-accent);' : ''">
                  {{ t.e }} {{ t.l }}
                </button>
              </div>
            </div>

            <div style="display:grid; grid-template-columns:1fr 1fr; gap:0.75rem;">
              <div>
                <label class="label">Grade <span style="color:#ef4444;">*</span></label>
                <select v-model="aiForm.gradeCode" class="input">
                  <option :value="null" disabled>Select Grade</option>
                  <option v-for="g in catalog.grades" :key="g.code" :value="g.code">{{ g.label }}</option>
                </select>
              </div>
              <div>
                <label class="label">Subject <span style="color:#ef4444;">*</span></label>
                <select v-model="aiForm.subjectId" class="input" :disabled="!aiForm.gradeCode">
                  <option :value="null" disabled>{{ aiForm.gradeCode ? 'Select Subject' : 'Select grade first' }}</option>
                  <option v-for="s in aiGradeSubjects" :key="s.id" :value="s.id">{{ s.name }}</option>
                </select>
              </div>
            </div>

            <div style="display:grid; grid-template-columns:2fr 1fr; gap:0.75rem;">
              <div>
                <label class="label">Topic / Chapter <span style="font-size:0.72rem; color:var(--t-text3);">(optional)</span></label>
                <input v-model="aiForm.topic" type="text" class="input" placeholder="e.g. Cell Division, Chemical Equations…" />
              </div>
              <div>
                <label class="label">Count</label>
                <input v-model.number="aiForm.count" type="number" min="1" max="20" class="input" />
              </div>
            </div>

            <div>
              <label class="label">Difficulty</label>
              <select v-model="aiForm.difficulty" class="input" style="width:220px;">
                <option value="">Mixed (Easy + Medium + Hard)</option>
                <option>Easy</option>
                <option>Medium</option>
                <option>Hard</option>
              </select>
            </div>

            <div class="qb-ai-note">
              <Sparkles class="w-3.5 h-3.5 flex-shrink-0" style="color:var(--t-accent);margin-top:1px;" />
              <span>Questions are generated by Gemini AI based on the Balochistan Board (BISE) curriculum for the selected grade and subject. Review all questions before saving to the bank.</span>
            </div>

            <p v-if="aiErrMsg" class="qb-err">{{ aiErrMsg }}</p>

            <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
              <button @click="generateQuestions" class="btn-primary" style="flex:1; justify-content:center;"
                      :disabled="aiLoading || !aiForm.gradeCode || !aiForm.subjectId">
                <span v-if="aiLoading" class="qb-spinner"></span>
                <Sparkles v-else class="w-4 h-4" />
                {{ aiLoading ? 'Generating with AI…' : 'Generate ' + aiForm.count + ' Questions' }}
              </button>
              <button @click="closeAI" class="btn-secondary" :disabled="aiLoading">Cancel</button>
            </div>
          </template>

          <!-- Results phase -->
          <template v-else>
            <div style="display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:0.5rem;">
              <div>
                <div style="font-weight:600; color:var(--t-text1); font-size:0.9rem;">{{ aiResults.length }} Questions Generated</div>
                <div style="font-size:0.75rem; color:var(--t-text3);">Select which to save to the question bank</div>
              </div>
              <div class="flex gap-2">
                <button @click="toggleAllAI" class="btn-secondary text-sm">
                  {{ selectedAI.size === aiResults.length ? 'Deselect All' : 'Select All' }}
                </button>
                <button @click="aiResults = []; aiErrMsg = ''" class="btn-secondary text-sm">← Back</button>
              </div>
            </div>

            <div style="display:flex; flex-direction:column; gap:0.5rem; max-height:360px; overflow-y:auto; padding-right:2px;">
              <div v-for="(q, i) in aiResults" :key="i"
                   @click="toggleAI(i)"
                   class="qb-ai-item"
                   :class="{ 'qb-ai-item-sel': selectedAI.has(i) }">
                <div style="display:flex; align-items:flex-start; gap:0.75rem;">
                  <div class="qb-ai-check" :class="{ 'qb-ai-check-on': selectedAI.has(i) }">
                    <Check v-if="selectedAI.has(i)" class="w-3 h-3" />
                  </div>
                  <div style="flex:1; min-width:0;">
                    <div style="font-size:0.83rem; color:var(--t-text1); line-height:1.5; margin-bottom:0.35rem;">{{ q.stem }}</div>
                    <div v-if="q.options" style="display:grid; grid-template-columns:1fr 1fr; gap:0.2rem; margin-bottom:0.35rem;">
                      <div v-for="(opt, oi) in q.options" :key="oi"
                           style="font-size:0.72rem; display:flex; align-items:center; gap:0.35rem;"
                           :style="oi === q.correct ? 'color:#10b981; font-weight:600;' : 'color:var(--t-text3);'">
                        <span style="font-weight:700; flex-shrink:0;">{{ 'ABCD'[oi] }}.</span>
                        <span style="overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">{{ opt }}</span>
                        <Check v-if="oi === q.correct" class="w-3 h-3 flex-shrink-0" />
                      </div>
                    </div>
                    <div v-if="q.modelAnswer" style="font-size:0.72rem; color:var(--t-text2); font-style:italic; margin-bottom:0.35rem;">
                      💡 {{ q.modelAnswer.length > 120 ? q.modelAnswer.slice(0, 120) + '…' : q.modelAnswer }}
                    </div>
                    <div style="display:flex; gap:0.35rem; flex-wrap:wrap;">
                      <span :class="diffClass(q.difficulty)" style="font-size:0.68rem;">{{ q.difficulty }}</span>
                      <span class="qb-bloom" :class="bloomClass(q.cognitiveLevel)" style="font-size:0.68rem;">{{ q.cognitiveLevel }}</span>
                      <span v-if="q.type" style="font-size:0.68rem; padding:0.15rem 0.45rem; border-radius:99px; background:var(--t-hover2); color:var(--t-text2);">{{ q.type }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <p v-if="aiErrMsg" class="qb-err">{{ aiErrMsg }}</p>

            <div class="flex gap-2" style="padding-top:0.5rem; border-top:1px solid var(--t-border);">
              <button @click="saveAIResults" class="btn-primary" style="flex:1; justify-content:center;"
                      :disabled="saving || selectedAI.size === 0">
                <span v-if="saving" class="qb-spinner"></span>
                <Plus v-else class="w-4 h-4" />
                {{ saving ? 'Saving…' : 'Save Selected (' + selectedAI.size + ')' }}
              </button>
              <button @click="aiResults = []; aiErrMsg = ''" class="btn-secondary" :disabled="saving">← Redo</button>
              <button @click="closeAI" class="btn-secondary" :disabled="saving">Cancel</button>
            </div>
          </template>

        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { Plus, Pencil, Trash2, LayoutGrid, List, Search, X, Filter, Sparkles, Check } from '@lucide/vue'
import api from '@/services/api'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'
import { generateObjectiveQuestions, generateSubjectiveQuestions } from '@/services/ollamaService'

const catalog = useCatalogStore()
const confirm = useConfirm()

// ── Constants ─────────────────────────────────────────────────────────
const typeOpts   = [{ v: 'objective', l: 'Objective', e: '📋' }, { v: 'subjective', l: 'Subjective', e: '✍️' }]
const bloomLevels = ['Remember', 'Understand', 'Apply', 'Analyze', 'Evaluate', 'Create']
const BLOOM_NORM  = { knowledge: 'Remember', remembering: 'Remember', understanding: 'Understand', applying: 'Apply', application: 'Apply', analysis: 'Analyze', evaluation: 'Evaluate', synthesis: 'Create', creating: 'Create' }

// ── State ─────────────────────────────────────────────────────────────
const questions       = ref([])
const loading         = ref(false)
const saving          = ref(false)
const aiLoading       = ref(false)
const totalCount      = ref(0)
const totalObjective  = ref(0)
const totalSubjective = ref(0)
const aiCount         = ref(0)
const page            = ref(1)
const pageSize        = ref(30)

// Filters
const filterGrade   = ref(null)
const filterSubject = ref(null)
const filterType    = ref('')
const filterDiff    = ref('')
const search        = ref('')
const view          = ref('list')

// Add/Edit modal
const showModal = ref(false)
const editingId = ref(null)
const errMsg    = ref('')

// AI modal
const aiModal    = ref(false)
const aiResults  = ref([])
const selectedAI = ref(new Set())
const aiErrMsg   = ref('')

// ── Form ──────────────────────────────────────────────────────────────
const mkForm = () => ({
  kind: 'objective', gradeCode: null, subjectId: null,
  stem: '', options: ['', '', '', ''], correctIndex: 0,
  questionType: 'Short', marks: 2, modelAnswer: '',
  difficulty: 'Medium', cognitiveLevel: 'Remember',
})
const form = ref(mkForm())

const mkAiForm = () => ({ kind: 'objective', gradeCode: null, subjectId: null, topic: '', count: 5, difficulty: '' })
const aiForm = ref(mkAiForm())

// ── Computed ──────────────────────────────────────────────────────────
const totalPages       = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))
const hasFilters       = computed(() => filterGrade.value || filterSubject.value || filterType.value || filterDiff.value || search.value)
const filterGradeSubjects = computed(() => filterGrade.value ? (catalog.subjectsForGrade(filterGrade.value) ?? []) : [])
const modalSubjects    = computed(() => form.value.gradeCode ? (catalog.subjectsForGrade(form.value.gradeCode) ?? []) : [])
const aiGradeSubjects  = computed(() => aiForm.value.gradeCode ? (catalog.subjectsForGrade(aiForm.value.gradeCode) ?? []) : [])
const diffStats        = computed(() => {
  const c = { Easy: 0, Medium: 0, Hard: 0 }
  for (const q of questions.value) if (q.difficulty) c[q.difficulty] = (c[q.difficulty] || 0) + 1
  return c
})

// ── Helpers ───────────────────────────────────────────────────────────
function parseOptions(json) { try { return JSON.parse(json) ?? [] } catch { return [] } }
function subjectName(id) { return id ? (catalog.subjects.find(s => s.id === id)?.name ?? `Subject ${id}`) : '—' }
function gradeLabel(code) { return code ? (catalog.grades.find(g => g.code === code)?.label ?? code) : '—' }
function diffClass(d) { return { Easy: 'badge-green', Medium: 'badge-amber', Hard: 'badge-red' }[d] || 'badge-indigo' }
function bloomClass(level) {
  const key = level?.toLowerCase() ?? 'remember'
  const norm = BLOOM_NORM[key] ?? level ?? 'Remember'
  return `bloom-${norm.toLowerCase()}`
}
function inferBloom(difficulty) { return { Easy: 'Remember', Medium: 'Understand', Hard: 'Analyze' }[difficulty] ?? 'Remember' }

// ── Data Loading ──────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: pageSize.value }
    if (filterGrade.value)   params.gradeCode  = filterGrade.value
    if (filterSubject.value) params.subjectId  = filterSubject.value
    if (filterType.value)    params.kind       = filterType.value
    if (filterDiff.value)    params.difficulty = filterDiff.value
    if (search.value)        params.search     = search.value
    const res = await api.get('/questions', { params })
    questions.value  = res.data.items
    totalCount.value = res.data.totalCount
    aiCount.value    = questions.value.filter(q => q.isAiGenerated).length
  } catch (e) {
    console.error('Failed to load questions:', e)
  } finally {
    loading.value = false
  }
}

async function loadStats() {
  try {
    const [o, s] = await Promise.all([
      api.get('/questions', { params: { page: 1, pageSize: 1, kind: 'objective' } }),
      api.get('/questions', { params: { page: 1, pageSize: 1, kind: 'subjective' } }),
    ])
    totalObjective.value  = o.data.totalCount
    totalSubjective.value = s.data.totalCount
  } catch {}
}

// ── Watchers ──────────────────────────────────────────────────────────
let searchDebounce = null
watch(search, () => {
  clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => { page.value = 1; load() }, 350)
})

watch(filterGrade, async (code) => {
  filterSubject.value = null
  page.value = 1
  if (code) await catalog.fetchSubjectsForGrade(code)
  load()
})
watch([filterSubject, filterType, filterDiff], () => { page.value = 1; load() })

watch(() => form.value.gradeCode, async (code) => {
  if (code) await catalog.fetchSubjectsForGrade(code)
  if (!editingId.value) form.value.subjectId = null
})
watch(() => aiForm.value.gradeCode, async (code) => {
  if (code) await catalog.fetchSubjectsForGrade(code)
  aiForm.value.subjectId = null
})

// ── Pagination ────────────────────────────────────────────────────────
function prevPage() { if (page.value > 1) { page.value--; load() } }
function nextPage() { if (page.value < totalPages.value) { page.value++; load() } }

// ── CRUD ──────────────────────────────────────────────────────────────
function openAdd() {
  editingId.value = null
  form.value = mkForm()
  errMsg.value = ''
  showModal.value = true
}

async function openEdit(q) {
  editingId.value = q.id
  form.value = {
    kind:           q.kind,
    gradeCode:      q.gradeCode ?? null,
    subjectId:      q.subjectId ?? null,
    stem:           q.stem,
    options:        [...parseOptions(q.optionsJson), '', '', '', ''].slice(0, 4),
    correctIndex:   q.correctIndex ?? 0,
    questionType:   q.questionType ?? 'Short',
    marks:          q.marks ?? 2,
    modelAnswer:    q.modelAnswer ?? '',
    difficulty:     q.difficulty ?? 'Medium',
    cognitiveLevel: q.cognitiveLevel ?? 'Remember',
  }
  errMsg.value = ''
  showModal.value = true
  if (q.gradeCode) await catalog.fetchSubjectsForGrade(q.gradeCode)
}

function closeModal() {
  if (saving.value) return
  showModal.value = false
  editingId.value = null
}

async function saveQuestion() {
  errMsg.value = ''
  saving.value = true
  try {
    if (editingId.value) {
      const payload = { stem: form.value.stem, difficulty: form.value.difficulty, cognitiveLevel: form.value.cognitiveLevel }
      if (form.value.kind === 'objective') {
        payload.optionsJson  = JSON.stringify(form.value.options)
        payload.correctIndex = form.value.correctIndex
      } else {
        payload.marks       = form.value.marks
        payload.modelAnswer = form.value.modelAnswer
      }
      await api.put(`/questions/${editingId.value}`, payload)
    } else {
      const payload = {
        kind:           form.value.kind,
        gradeCode:      form.value.gradeCode,
        subjectId:      form.value.subjectId,
        stem:           form.value.stem,
        difficulty:     form.value.difficulty,
        cognitiveLevel: form.value.cognitiveLevel,
      }
      if (form.value.kind === 'objective') {
        payload.optionsJson  = JSON.stringify(form.value.options)
        payload.correctIndex = form.value.correctIndex
      } else {
        payload.questionType = form.value.questionType
        payload.marks        = form.value.marks
        payload.modelAnswer  = form.value.modelAnswer
      }
      await api.post('/questions', payload)
    }
    closeModal()
    load()
    loadStats()
  } catch (e) {
    errMsg.value = e?.response?.data?.title ?? e?.response?.data ?? e?.message ?? 'Failed to save question'
  } finally {
    saving.value = false
  }
}

async function removeQuestion(q) {
  const ok = await confirm({
    title:       'Delete Question?',
    message:     `"${q.stem.slice(0, 80)}${q.stem.length > 80 ? '…' : ''}"`,
    confirmText: 'Delete',
    danger:      true,
  })
  if (!ok) return
  try {
    await api.delete(`/questions/${q.id}`)
    load()
    loadStats()
  } catch (e) {
    console.error('Delete failed:', e)
  }
}

// ── AI Generation ─────────────────────────────────────────────────────
function openAI() {
  aiForm.value = mkAiForm()
  aiResults.value = []
  selectedAI.value = new Set()
  aiErrMsg.value = ''
  aiModal.value = true
}
function closeAI() {
  if (saving.value || aiLoading.value) return
  aiModal.value = false
  aiResults.value = []
  selectedAI.value = new Set()
  aiErrMsg.value = ''
}

async function generateQuestions() {
  if (!aiForm.value.gradeCode || !aiForm.value.subjectId) {
    aiErrMsg.value = 'Please select a grade and subject first.'
    return
  }
  aiErrMsg.value = ''
  aiLoading.value = true
  try {
    const ctx = {
      subject:    subjectName(aiForm.value.subjectId),
      grade:      aiForm.value.gradeCode,
      board:      'Balochistan Board (BISE)',
      topic:      aiForm.value.topic || undefined,
      count:      aiForm.value.count,
      difficulty: aiForm.value.difficulty || undefined,
    }
    let results = aiForm.value.kind === 'objective'
      ? await generateObjectiveQuestions(ctx)
      : await generateSubjectiveQuestions(ctx)

    if (!results.length) {
      aiErrMsg.value = 'AI returned no questions. Check your VITE_GEMINI_API_KEY or try a different topic.'
      return
    }
    // Normalize cognitive level to Bloom's taxonomy
    aiResults.value = results.map(q => ({
      ...q,
      cognitiveLevel: (() => {
        const raw = q.cognitiveLevel?.toLowerCase()
        if (!raw) return inferBloom(q.difficulty)
        const norm = BLOOM_NORM[raw]
        if (norm) return norm
        if (bloomLevels.map(b => b.toLowerCase()).includes(raw)) return q.cognitiveLevel
        return inferBloom(q.difficulty)
      })(),
    }))
    selectedAI.value = new Set(aiResults.value.map((_, i) => i))
  } catch (e) {
    aiErrMsg.value = e?.message ?? 'AI generation failed. Ensure VITE_GEMINI_API_KEY is configured.'
  } finally {
    aiLoading.value = false
  }
}

function toggleAI(i) {
  const s = new Set(selectedAI.value)
  if (s.has(i)) s.delete(i); else s.add(i)
  selectedAI.value = s
}
function toggleAllAI() {
  selectedAI.value = selectedAI.value.size === aiResults.value.length
    ? new Set()
    : new Set(aiResults.value.map((_, i) => i))
}

async function saveAIResults() {
  if (!selectedAI.value.size) return
  aiErrMsg.value = ''
  saving.value = true
  try {
    const toSave = [...selectedAI.value].map(i => aiResults.value[i])
    await Promise.all(toSave.map(q => {
      const payload = {
        kind:           aiForm.value.kind,
        gradeCode:      aiForm.value.gradeCode,
        subjectId:      aiForm.value.subjectId,
        stem:           q.stem,
        difficulty:     q.difficulty,
        cognitiveLevel: q.cognitiveLevel,
        isAiGenerated:  true,
      }
      if (aiForm.value.kind === 'objective') {
        payload.optionsJson  = JSON.stringify(q.options)
        payload.correctIndex = q.correct
      } else {
        payload.questionType = q.type ?? 'Short'
        payload.marks        = q.marks ?? 2
        payload.modelAnswer  = q.modelAnswer ?? ''
      }
      return api.post('/questions', payload)
    }))
    closeAI()
    load()
    loadStats()
  } catch (e) {
    aiErrMsg.value = e?.response?.data?.title ?? e?.message ?? 'Failed to save questions'
  } finally {
    saving.value = false
  }
}

// ── Misc ──────────────────────────────────────────────────────────────
function clearFilters() {
  filterGrade.value   = null
  filterSubject.value = null
  filterType.value    = ''
  filterDiff.value    = ''
  search.value        = ''
}

onMounted(() => {
  Promise.all([load(), loadStats()])
})
</script>

<style scoped>
.qb-overlay {
  position: fixed; inset: 0; z-index: 1000;
  background: rgba(0,0,0,0.45); backdrop-filter: blur(4px);
  display: flex; align-items: center; justify-content: center;
  padding: 1.25rem;
}
.qb-modal {
  background: var(--t-bg2); border: 1px solid var(--t-border);
  border-radius: 16px; width: 100%; max-width: 560px;
  max-height: calc(100vh - 2.5rem); overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}
.qb-modal-wide { max-width: 660px; }
.qb-modal-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 1.25rem 1.5rem 0;
}
.qb-modal-title { font-size:1rem; font-weight:700; color:var(--t-text1); margin:0; }
.qb-modal-body  { padding: 1.25rem 1.5rem 1.5rem; display:flex; flex-direction:column; gap:1rem; }

.qb-readonly {
  padding: 7px 10px; border: 1px solid var(--t-border); border-radius: 8px;
  background: var(--t-bg2); font-size:0.875rem; color:var(--t-text2);
  min-height: 36px; display:flex; align-items:center;
}
.qb-err {
  margin:0; padding: 10px 14px; border-radius: 9px;
  font-size:0.83rem; background:#fef2f2; border:1px solid #fecaca; color:#b91c1c;
}
.qb-spinner {
  display:inline-block; width:12px; height:12px;
  border: 2px solid rgba(255,255,255,0.4); border-top-color:white;
  border-radius:50%; animation: qbspin 0.7s linear infinite;
}
@keyframes qbspin { to { transform: rotate(360deg); } }

.qb-bloom { padding:0.15rem 0.5rem; border-radius:99px; font-size:0.72rem; font-weight:500; }
.bloom-remember   { background:#fce7f3; color:#9d174d; }
.bloom-understand { background:#dbeafe; color:#1e40af; }
.bloom-apply      { background:#d1fae5; color:#065f46; }
.bloom-analyze    { background:#fef3c7; color:#92400e; }
.bloom-evaluate   { background:#ede9fe; color:#4c1d95; }
.bloom-create     { background:#fee2e2; color:#991b1b; }

.qb-ai-note {
  display:flex; align-items:flex-start; gap:0.5rem;
  padding:0.75rem 1rem; border-radius:10px;
  background:var(--t-acc-alpha-sm); border:1px solid var(--t-acc-alpha-md);
  font-size:0.78rem; color:var(--t-text2); line-height:1.5;
}
.qb-ai-item {
  border: 1.5px solid var(--t-border); border-radius:10px; padding:0.75rem 1rem;
  cursor:pointer; transition: border-color 0.15s, background 0.15s;
}
.qb-ai-item:hover { border-color:var(--t-accent); background:var(--t-hover1); }
.qb-ai-item-sel   { border-color:var(--t-accent) !important; background:var(--t-acc-alpha-sm) !important; }
.qb-ai-check {
  width:18px; height:18px; border:1.5px solid var(--t-border);
  border-radius:5px; flex-shrink:0; display:flex; align-items:center; justify-content:center;
  transition: all 0.15s; margin-top:2px;
}
.qb-ai-check-on { background:var(--t-accent); border-color:var(--t-accent); color:white; }
</style>
