<template>
  <div class="animate-fade-in space-y-5 au-wrap">
    <div>
      <h2 class="section-title">Upload Assessments</h2>
      <p class="text-sm" style="color:var(--t-text3)">
        Upload papers, question banks, past papers, syllabus or notes. Tag each batch by subject, grade and type.
      </p>
    </div>

    <!-- Tagging + dropzone -->
    <div class="card p-5 space-y-4">
      <div class="au-tags">
        <div class="au-field">
          <label class="label">Subject</label>
          <select v-model="form.subject" class="input">
            <option v-for="s in SUBJECTS" :key="s.id" :value="s.name">{{ s.name }}</option>
          </select>
        </div>
        <div class="au-field">
          <label class="label">Grade</label>
          <select v-model="form.grade" class="input">
            <option v-for="g in grades" :key="g" :value="g">{{ g === 'ECD' ? 'ECD' : 'Grade ' + g }}</option>
          </select>
        </div>
        <div class="au-field">
          <label class="label">Type</label>
          <select v-model="form.kind" class="input">
            <option v-for="k in kinds" :key="k" :value="k">{{ k }}</option>
          </select>
        </div>
      </div>

      <div
        class="au-drop" :class="{ 'is-drag': dragging }"
        @click="picker?.click()"
        @dragover.prevent="dragging = true"
        @dragleave.prevent="dragging = false"
        @drop.prevent="onDrop">
        <UploadCloud class="w-9 h-9" style="color:var(--t-accent)" />
        <p class="au-drop-title">Drop files here or <span class="au-drop-link">browse</span></p>
        <p class="au-drop-sub">PDF, Word, images — multiple files allowed</p>
        <input ref="picker" type="file" multiple class="hidden"
          accept=".pdf,.doc,.docx,.png,.jpg,.jpeg,.txt"
          @change="onPick" />
      </div>

      <p v-if="justAdded" class="au-toast"><CheckCircle2 class="w-4 h-4" /> Added {{ justAdded }} file{{ justAdded === 1 ? '' : 's' }}.</p>
    </div>

    <!-- Uploaded list -->
    <div class="card overflow-hidden">
      <div class="au-list-hd">
        <span class="au-list-title">Uploaded Assessments <span class="au-count">{{ items.length }}</span></span>
        <button v-if="items.length" type="button" class="btn-ghost text-xs" @click="clearAll"><Trash2 class="w-3.5 h-3.5" /> Clear all</button>
      </div>

      <div v-if="!items.length" class="au-empty">
        <FileStack class="w-12 h-12 mb-2 opacity-40" />
        <p class="text-sm">No assessments uploaded yet.</p>
      </div>

      <ul v-else class="au-items">
        <li v-for="it in items" :key="it.id" class="au-item">
          <div class="au-item-icon" :title="it.mime">{{ iconFor(it) }}</div>
          <div class="au-item-main">
            <div class="au-item-name">{{ it.name }}</div>
            <div class="au-item-meta">
              <span class="badge-purple">{{ it.kind }}</span>
              <span>{{ it.subject }}</span>
              <span>·</span>
              <span>{{ it.grade === 'ECD' ? 'ECD' : 'Grade ' + it.grade }}</span>
              <span>·</span>
              <span>{{ formatSize(it.size) }}</span>
              <span>·</span>
              <span>{{ it.date }}</span>
            </div>
          </div>
          <div class="au-item-actions">
            <a v-if="it.url" :href="it.url" target="_blank" rel="noopener" class="au-act" title="Open (this session)"><ExternalLink class="w-4 h-4" /></a>
            <span v-else class="au-act-note" title="File not retained after reload — re-upload to view">stored</span>
            <button type="button" class="au-act au-del" @click="remove(it.id)" title="Remove"><Trash2 class="w-4 h-4" /></button>
          </div>
        </li>
      </ul>

      <div class="au-note">
        <Info class="w-3.5 h-3.5 shrink-0" />
        <span>Front-end demo: file details are saved in this browser and files open during this session. Connect a backend (or Netlify storage) to retain uploads permanently and feed them to Saathi's curriculum search.</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { UploadCloud, Trash2, FileStack, ExternalLink, CheckCircle2, Info } from '@lucide/vue'
import { SUBJECTS } from '@/stores/content'

const STORAGE_KEY = 'bap_assessments'
const grades = ['ECD', '1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12']
const kinds = ['Past Paper', 'Model Paper', 'Question Bank', 'Notes', 'Syllabus', 'Marking Scheme', 'Worksheet']

const form = ref({ subject: SUBJECTS[0]?.name || 'Physics', grade: '9', kind: 'Past Paper' })
const items = ref([])
const picker = ref(null)
const dragging = ref(false)
const justAdded = ref(0)

onMounted(() => {
  try { items.value = JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]') } catch { items.value = [] }
})

function persist() {
  // Persist metadata only (not the file blobs / object URLs).
  const meta = items.value.map(({ url, ...rest }) => rest)
  try { localStorage.setItem(STORAGE_KEY, JSON.stringify(meta)) } catch { /* quota */ }
}

function addFiles(fileList) {
  const files = Array.from(fileList || [])
  if (!files.length) return
  let n = 0
  files.forEach((f, i) => {
    items.value.unshift({
      id: `${Date.now()}-${i}-${Math.round(performance.now())}`,
      name: f.name,
      size: f.size,
      mime: f.type || 'file',
      subject: form.value.subject,
      grade: form.value.grade,
      kind: form.value.kind,
      date: new Date().toLocaleDateString('en-PK', { year: 'numeric', month: 'short', day: 'numeric' }),
      url: URL.createObjectURL(f), // valid for this session only
    })
    n++
  })
  persist()
  justAdded.value = n
  setTimeout(() => { justAdded.value = 0 }, 3000)
}

function onPick(e) { addFiles(e.target.files); e.target.value = '' }
function onDrop(e) { dragging.value = false; addFiles(e.dataTransfer?.files) }
function remove(id) { items.value = items.value.filter((it) => it.id !== id); persist() }
function clearAll() { items.value = []; persist() }

function formatSize(bytes) {
  if (!bytes) return '—'
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(0) + ' KB'
  return (bytes / 1024 / 1024).toFixed(1) + ' MB'
}
function iconFor(it) {
  const m = (it.mime || '').toLowerCase()
  if (m.includes('pdf')) return '📕'
  if (m.includes('image')) return '🖼️'
  if (m.includes('word') || it.name.match(/\.docx?$/i)) return '📘'
  return '📄'
}
</script>

<style scoped>
.au-wrap { max-width: 860px; }
.au-tags { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem; }
@media (max-width: 560px) { .au-tags { grid-template-columns: 1fr; } }
.au-field { display: flex; flex-direction: column; }

.au-drop {
  border: 2px dashed var(--t-border); border-radius: 16px; padding: 1.75rem 1rem;
  display: flex; flex-direction: column; align-items: center; gap: 0.3rem; text-align: center;
  cursor: pointer; transition: all 0.15s; background: var(--t-hover);
}
.au-drop:hover, .au-drop.is-drag { border-color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 7%, transparent); }
.au-drop-title { font-size: 0.9rem; font-weight: 600; color: var(--t-text1); }
.au-drop-link { color: var(--t-accent); text-decoration: underline; }
.au-drop-sub { font-size: 0.72rem; color: var(--t-text3); }
.hidden { display: none; }
.au-toast { display: inline-flex; align-items: center; gap: 0.4rem; font-size: 0.78rem; color: var(--t-success); }

.au-list-hd { display: flex; align-items: center; justify-content: space-between; padding: 0.85rem 1.1rem; border-bottom: 1px solid var(--t-border); }
.au-list-title { font-size: 0.88rem; font-weight: 700; color: var(--t-text1); }
.au-count { font-size: 0.7rem; font-weight: 700; color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 12%, transparent); border-radius: 99px; padding: 0.1rem 0.5rem; margin-left: 0.35rem; }
.au-empty { display: flex; flex-direction: column; align-items: center; padding: 2.5rem 1rem; color: var(--t-text3); }

.au-items { list-style: none; margin: 0; padding: 0; }
.au-item { display: flex; align-items: center; gap: 0.85rem; padding: 0.75rem 1.1rem; border-bottom: 1px solid var(--t-border); }
.au-item:last-child { border-bottom: none; }
.au-item-icon { font-size: 1.4rem; width: 36px; text-align: center; flex-shrink: 0; }
.au-item-main { flex: 1; min-width: 0; }
.au-item-name { font-size: 0.85rem; font-weight: 600; color: var(--t-text1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.au-item-meta { display: flex; align-items: center; gap: 0.4rem; flex-wrap: wrap; font-size: 0.72rem; color: var(--t-text3); margin-top: 0.15rem; }
.au-item-actions { display: flex; align-items: center; gap: 0.3rem; flex-shrink: 0; }
.au-act { width: 32px; height: 32px; border-radius: 8px; display: flex; align-items: center; justify-content: center; color: var(--t-text2); background: var(--t-hover); border: 1px solid var(--t-border); cursor: pointer; text-decoration: none; }
.au-act:hover { color: var(--t-accent); border-color: var(--t-accent); }
.au-del:hover { color: #f87171; border-color: rgba(248,113,113,0.4); }
.au-act-note { font-size: 0.62rem; color: var(--t-text3); padding: 0 0.3rem; }
.au-note { display: flex; gap: 0.5rem; padding: 0.7rem 1.1rem; font-size: 0.7rem; color: var(--t-text3); background: var(--t-hover); border-top: 1px solid var(--t-border); line-height: 1.5; }
</style>
