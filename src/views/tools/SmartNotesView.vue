<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Smart Notes</h2>
    <div class="sn-layout">
      <!-- Note list -->
      <div class="sn-sidebar">
        <div class="sn-sidebar-header">
          <button @click="newNote" class="sn-new-btn">+ New Note</button>
        </div>
        <div class="sn-note-list">
          <div v-for="(note, i) in notes" :key="note.id"
            class="sn-note-item" :class="{ 'sn-active': activeIdx === i }"
            @click="activeIdx = i">
            <div class="sn-note-item-title">{{ note.title || 'Untitled' }}</div>
            <div class="sn-note-item-sub">{{ note.subject || 'General' }} · {{ formatDate(note.updated) }}</div>
          </div>
        </div>
      </div>
      <!-- Editor -->
      <div class="sn-editor" v-if="activeNote">
        <div class="sn-editor-toolbar">
          <input v-model="activeNote.title" placeholder="Note title..." class="sn-title-input" />
          <select v-model="activeNote.subject" class="sn-subject-sel">
            <option value="">Select subject</option>
            <option v-for="s in SUBJECTS" :key="s.id" :value="s.name">{{ s.name }}</option>
          </select>
          <button @click="deleteNote" class="sn-del-btn">🗑</button>
        </div>
        <textarea v-model="activeNote.body" placeholder="Start writing your notes here..." class="sn-textarea" />
      </div>
      <div v-else class="sn-empty">Select or create a note to get started.</div>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { SUBJECTS } from '@/stores/content'
import PageFooter from '@/components/platform/PageFooter.vue'

const notes = ref([
  { id: 1, title: 'Photosynthesis Notes', subject: 'Biology', body: 'Photosynthesis is the process by which green plants convert sunlight into food...\n\nEquation: 6CO₂ + 6H₂O → C₆H₁₂O₆ + 6O₂', updated: '2026-06-09' },
  { id: 2, title: 'Newton\'s Laws', subject: 'Physics', body: '1st Law: An object at rest stays at rest...\n2nd Law: F = ma\n3rd Law: For every action, there is an equal and opposite reaction.', updated: '2026-06-08' },
])
const activeIdx = ref(0)
const activeNote = computed(() => notes.value[activeIdx.value] || null)

function newNote() {
  notes.value.unshift({ id: Date.now(), title: '', subject: '', body: '', updated: new Date().toISOString().slice(0, 10) })
  activeIdx.value = 0
}
function deleteNote() {
  notes.value.splice(activeIdx.value, 1)
  activeIdx.value = Math.max(0, activeIdx.value - 1)
}
function formatDate(d) { return d ? d.slice(0, 10) : '' }
</script>

<style scoped>
.sn-layout { display: flex; gap: 0; border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; min-height: 520px; }
.sn-sidebar { width: 240px; flex-shrink: 0; border-right: 1px solid var(--t-border); background: var(--t-surface); display: flex; flex-direction: column; }
.sn-sidebar-header { padding: 0.75rem; border-bottom: 1px solid var(--t-border); }
.sn-new-btn { width: 100%; padding: 0.5rem; border-radius: 7px; background: #4caf50; color: white; border: none; font-weight: 700; font-size: 0.82rem; cursor: pointer; }
.sn-note-list { flex: 1; overflow-y: auto; }
.sn-note-item { padding: 0.75rem 0.85rem; border-bottom: 1px solid var(--t-border); cursor: pointer; }
.sn-note-item:hover { background: var(--t-hover); }
.sn-active { background: rgba(76,175,80,0.1) !important; border-left: 3px solid #4caf50; }
.sn-note-item-title { font-size: 0.82rem; font-weight: 600; color: var(--t-text1); }
.sn-note-item-sub { font-size: 0.68rem; color: var(--t-text3); margin-top: 0.15rem; }
.sn-editor { flex: 1; display: flex; flex-direction: column; background: var(--t-bg); }
.sn-editor-toolbar { display: flex; gap: 0.5rem; padding: 0.75rem; border-bottom: 1px solid var(--t-border); background: var(--t-surface); flex-wrap: wrap; }
.sn-title-input { flex: 1; min-width: 160px; padding: 0.4rem 0.65rem; border: 1px solid var(--t-border); border-radius: 7px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; font-weight: 600; }
.sn-subject-sel { padding: 0.4rem 0.65rem; border: 1px solid var(--t-border); border-radius: 7px; background: var(--t-bg); color: var(--t-text1); font-size: 0.8rem; }
.sn-del-btn { padding: 0.4rem 0.65rem; border: 1px solid var(--t-border); border-radius: 7px; background: var(--t-surface); cursor: pointer; font-size: 0.9rem; }
.sn-textarea { flex: 1; width: 100%; padding: 1rem; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; border: none; resize: none; line-height: 1.6; font-family: inherit; }
.sn-empty { flex: 1; display: flex; align-items: center; justify-content: center; color: var(--t-text3); font-size: 0.875rem; }
@media (max-width: 640px) { .sn-sidebar { width: 100%; border-right: none; border-bottom: 1px solid var(--t-border); } .sn-layout { flex-direction: column; } }
</style>
