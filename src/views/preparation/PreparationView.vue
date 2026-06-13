<template>
  <div class="pv-home">

    <!-- Book header (when a book is selected) -->
    <div v-if="book" class="pv-book-hd">
      <div class="pv-book-badge" :class="book.color" aria-hidden="true">
        {{ book.icon }}
      </div>
      <div class="pv-book-info">
        <h1 class="pv-book-name">{{ book.name }}</h1>
        <p class="pv-book-desc">Select a study option below to begin</p>
      </div>
    </div>

    <!-- Empty state when no book selected -->
    <div v-else class="pv-empty">
      <div class="pv-empty-icon" aria-hidden="true">📚</div>
      <p class="pv-empty-lbl">Select a subject from the sidebar to get started</p>
    </div>

    <!-- Study option cards grid -->
    <div v-if="book" class="pv-opts-grid" role="list" aria-label="Study options">
      <button
        v-for="opt in studyOptions"
        :key="opt.action"
        type="button"
        role="listitem"
        @click="handleOption(opt)"
        class="pv-opt-card"
        :aria-label="opt.title + ': ' + opt.desc">
        <span class="pv-ocard-icon-wrap" :class="opt.grad" aria-hidden="true">
          <component :is="opt.icon" class="pv-ocard-svg" />
        </span>
        <div class="pv-ocard-body">
          <span class="pv-ocard-title">{{ opt.title }}</span>
          <span class="pv-ocard-desc">{{ opt.desc }}</span>
        </div>
        <ArrowRight class="pv-ocard-arr" aria-hidden="true" />
      </button>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  ArrowRight,
  FolderOpen, Video, FileText, FlaskConical,
  TestTube2, CheckSquare, PenLine, FileStack,
} from '@lucide/vue'
import { findPrepSubject } from '@/views/preparation/prepSubjects'

const props = defineProps({ bookId: { type: [String, Number], default: null } })

const router = useRouter()
const route  = useRoute()

const book = computed(() => {
  const id = props.bookId ?? route.params.bookId ?? null
  if (id == null || id === '') return null
  return findPrepSubject(id)
})

const studyOptions = [
  { title: 'Academic Resources',  desc: 'eBooks, slides, and study materials',    grad: 'grad-blue',    icon: FolderOpen,   action: 'resources'   },
  { title: 'Video Lectures',      desc: 'Expert teacher explanations',             grad: 'grad-violet',  icon: Video,        action: 'videos'      },
  { title: 'Key Notes',           desc: 'Smart summaries of each chapter',         grad: 'grad-teal',    icon: FileText,     action: 'keynotes'    },
  { title: 'Simulations',         desc: 'Interactive 3D concept demos',            grad: 'grad-emerald', icon: FlaskConical, action: 'simulations' },
  { title: 'Virtual Lab',         desc: 'Safe, guided lab experiments',            grad: 'grad-rose',    icon: TestTube2,    action: 'vlab'        },
  { title: 'Objective Paper',     desc: 'MCQ practice with instant grading',       grad: 'grad-amber',   icon: CheckSquare,  action: 'objective'   },
  { title: 'Subjective Paper',    desc: 'Long-answer questions with AI feedback',  grad: 'grad-pink',    icon: PenLine,      action: 'subjective'  },
  { title: 'Past & Model Papers', desc: 'Board exam papers from previous years',   grad: 'grad-orange',  icon: FileStack,    action: 'pastpapers'  },
]

function handleOption(opt) {
  const id = book.value?.id
  if (!id) return
  // All options open as nested panels inside the Preparation layout
  router.push(`/app/preparation/${id}/${opt.action}`)
}
</script>

<style scoped>
.pv-home { width: 100%; }

/* Book header */
.pv-book-hd {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
  padding: 1rem 1.25rem;
  background: var(--t-surface);
  border: 1px solid var(--t-border);
  border-radius: 14px;
}
.pv-book-badge {
  width: 52px;
  height: 52px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  flex-shrink: 0;
}
.pv-book-name {
  font-size: 1.2rem;
  font-weight: 800;
  color: var(--t-text1);
  margin: 0 0 0.2rem;
  font-family: 'Syne', sans-serif;
  line-height: 1.2;
}
.pv-book-desc { font-size: 0.8rem; color: var(--t-text3); margin: 0; }

/* Empty state */
.pv-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  text-align: center;
  gap: 0.75rem;
}
.pv-empty-icon { font-size: 2.5rem; }
.pv-empty-lbl { font-size: 0.9rem; color: var(--t-text3); max-width: 260px; }

/* Options grid — fills the workspace, scaling columns to the available width */
.pv-opts-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.85rem;
}
@media (min-width: 900px)  { .pv-opts-grid { grid-template-columns: repeat(4, 1fr); } }
@media (min-width: 1500px) { .pv-opts-grid { grid-template-columns: repeat(5, 1fr); } }
@media (min-width: 1900px) { .pv-opts-grid { grid-template-columns: repeat(6, 1fr); } }

.pv-opt-card {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.65rem;
  padding: 1rem;
  background: var(--t-surface);
  border: 1px solid var(--t-border);
  border-radius: 14px;
  cursor: pointer;
  text-align: left;
  transition: border-color 0.15s, box-shadow 0.15s, transform 0.15s;
}
.pv-opt-card:hover {
  border-color: var(--t-accent);
  box-shadow: 0 4px 16px color-mix(in srgb, var(--t-accent) 14%, transparent);
  transform: translateY(-2px);
}
.pv-ocard-icon-wrap {
  width: 40px;
  height: 40px;
  border-radius: 11px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.pv-ocard-svg { width: 18px; height: 18px; color: white; }
.pv-ocard-body { flex: 1; min-width: 0; }
.pv-ocard-title {
  display: block;
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--t-text1);
  margin-bottom: 0.2rem;
  line-height: 1.25;
}
.pv-ocard-desc { display: block; font-size: 0.7rem; color: var(--t-text3); line-height: 1.35; }
.pv-ocard-arr {
  width: 14px;
  height: 14px;
  color: var(--t-text3);
  align-self: flex-end;
  transition: color 0.15s;
}
.pv-opt-card:hover .pv-ocard-arr { color: var(--t-accent); }

/* Icon gradient backgrounds */
.grad-blue    { background: linear-gradient(135deg, #3b82f6, #1d4ed8); }
.grad-violet  { background: linear-gradient(135deg, #8b5cf6, #6d28d9); }
.grad-teal    { background: linear-gradient(135deg, #14b8a6, #0f766e); }
.grad-emerald { background: linear-gradient(135deg, #10b981, #059669); }
.grad-rose    { background: linear-gradient(135deg, #f43f5e, #be123c); }
.grad-amber   { background: linear-gradient(135deg, #f59e0b, #d97706); }
.grad-pink    { background: linear-gradient(135deg, #ec4899, #be185d); }
.grad-orange  { background: linear-gradient(135deg, #f97316, #ea580c); }

@media (prefers-reduced-motion: reduce) {
  .pv-opt-card { transition: none; }
  .pv-opt-card:hover { transform: none; }
}
</style>
