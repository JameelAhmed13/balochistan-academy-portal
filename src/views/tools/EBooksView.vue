<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">E-Books</h2>
    <div class="eb-filters">
      <input v-model="search" type="text" placeholder="Search books..." class="eb-search" />
      <select v-model="filterSubject" class="eb-sel">
        <option value="">All Subjects</option>
        <option v-for="s in SUBJECTS" :key="s.id" :value="s.name">{{ s.name }}</option>
      </select>
    </div>
    <div class="eb-grid">
      <div v-for="book in filtered" :key="book.id" class="eb-card">
        <div class="eb-cover" :style="{ background: book.color }">
          <div class="eb-cover-icon">{{ book.icon }}</div>
        </div>
        <div class="eb-info">
          <div class="eb-title">{{ book.title }}</div>
          <div class="eb-meta">{{ book.subject }} · {{ book.grade }} · {{ book.pages }} pages</div>
          <div class="eb-board">{{ book.board }}</div>
        </div>
        <a :href="book.url || '#'" target="_blank" class="eb-read-btn">Read Online</a>
      </div>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { SUBJECTS } from '@/stores/content'
import PageFooter from '@/components/platform/PageFooter.vue'

const search = ref('')
const filterSubject = ref('')

const books = [
  { id: 1, title: 'Mathematics Class 9', subject: 'Mathematics', grade: 'Class 9', pages: 310, board: 'Federal Board', icon: '📐', color: 'linear-gradient(135deg,#e3f2fd,#bbdefb)', url: '' },
  { id: 2, title: 'Physics Class 9', subject: 'Physics', grade: 'Class 9', pages: 280, board: 'Federal Board', icon: '⚡', color: 'linear-gradient(135deg,#f3e5f5,#e1bee7)', url: '' },
  { id: 3, title: 'Chemistry Class 9', subject: 'Chemistry', grade: 'Class 9', pages: 260, board: 'Federal Board', icon: '🧪', color: 'linear-gradient(135deg,#e8f5e9,#c8e6c9)', url: '' },
  { id: 4, title: 'Biology Class 9', subject: 'Biology', grade: 'Class 9', pages: 295, board: 'Federal Board', icon: '🧬', color: 'linear-gradient(135deg,#fff8e1,#ffe0b2)', url: '' },
  { id: 5, title: 'Urdu Class 9', subject: 'Urdu', grade: 'Class 9', pages: 200, board: 'Federal Board', icon: '📖', color: 'linear-gradient(135deg,#fce4ec,#f8bbd0)', url: '' },
  { id: 6, title: 'English Class 9', subject: 'English', grade: 'Class 9', pages: 180, board: 'Federal Board', icon: '🔤', color: 'linear-gradient(135deg,#e0f7fa,#b2ebf2)', url: '' },
  { id: 7, title: 'Mathematics Class 10', subject: 'Mathematics', grade: 'Class 10', pages: 330, board: 'Federal Board', icon: '📐', color: 'linear-gradient(135deg,#e3f2fd,#bbdefb)', url: '' },
  { id: 8, title: 'Computer Science Class 9', subject: 'Computer Science', grade: 'Class 9', pages: 240, board: 'Federal Board', icon: '💻', color: 'linear-gradient(135deg,#e8eaf6,#c5cae9)', url: '' },
]

const filtered = computed(() => books.filter(b => {
  if (filterSubject.value && b.subject !== filterSubject.value) return false
  if (search.value && !b.title.toLowerCase().includes(search.value.toLowerCase())) return false
  return true
}))
</script>

<style scoped>
.eb-filters { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.eb-search { flex: 1; min-width: 200px; padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-surface); color: var(--t-text1); font-size: 0.875rem; }
.eb-sel { padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-surface); color: var(--t-text1); font-size: 0.82rem; }
.eb-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 1rem; }
.eb-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; display: flex; flex-direction: column; }
.eb-cover { height: 100px; display: flex; align-items: center; justify-content: center; }
.eb-cover-icon { font-size: 2.5rem; }
.eb-info { padding: 0.85rem; flex: 1; }
.eb-title { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); margin-bottom: 0.25rem; }
.eb-meta { font-size: 0.72rem; color: var(--t-text3); }
.eb-board { display: inline-block; margin-top: 0.35rem; padding: 0.15rem 0.5rem; border-radius: 4px; background: #e3f2fd; color: #1565c0; font-size: 0.65rem; font-weight: 700; }
html.dark .eb-board { background: rgba(21,101,192,0.2); color: #90caf9; }
.eb-read-btn { display: block; margin: 0 0.85rem 0.85rem; padding: 0.5rem; border-radius: 7px; background: #4caf50; color: white; text-align: center; font-size: 0.78rem; font-weight: 700; text-decoration: none; }
</style>
