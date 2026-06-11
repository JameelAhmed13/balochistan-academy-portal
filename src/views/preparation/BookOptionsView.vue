<template>
  <div class="animate-fade-in space-y-6" v-if="subject">
    <!-- Header -->
    <div class="flex items-center gap-4">
      <button @click="$router.push('/app/preparation')" class="btn-ghost">
        <ArrowLeft class="w-4 h-4" /> Back
      </button>
      <div class="w-12 h-14 rounded-xl flex items-center justify-center text-3xl" :class="subject.color">{{ subject.icon }}</div>
      <div>
        <h2 class="section-title">{{ subject.name }}</h2>
        <p class="text-sm" style="color:var(--t-text2)">{{ subject.nameUr }} · Grade 9 · Balochistan Board</p>
      </div>
    </div>

    <!-- Options grid -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <BookOptionCard v-for="opt in options" :key="opt.title" v-bind="opt" @click="handleOption(opt)" />
    </div>
  </div>
  <div v-else class="text-center py-20" style="color:var(--t-text3)">Subject not found.</div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { ArrowLeft } from '@lucide/vue'
import { SUBJECTS } from '@/stores/content'
import BookOptionCard from '@/components/ui/BookOptionCard.vue'

const props = defineProps({ bookId: String })
const router = useRouter()

const subject = computed(() => SUBJECTS.find(s => s.id === +props.bookId))

const options = [
  { title: 'Academic Resources',   icon: '📁', desc: 'Downloadable files & notes',         grad: 'grad-blue',    action: 'resources' },
  { title: 'Video Lectures',        icon: '🎬', desc: 'Chapter-wise video lessons',          grad: 'grad-violet',  action: 'videos' },
  { title: 'Key Notes',             icon: '📝', desc: 'Summarized chapter key points',       grad: 'grad-teal',    action: 'keynotes' },
  { title: 'Simulations',           icon: '🔬', desc: 'Interactive subject simulations',     grad: 'grad-emerald', action: 'simulations' },
  { title: 'Virtual Lab',           icon: '⚗️', desc: 'Hands-on virtual experiments',        grad: 'grad-rose',    action: 'vlab' },
  { title: 'Objective Paper',       icon: '✅', desc: 'MCQ preparation & practice',          grad: 'grad-amber',   action: 'objective' },
  { title: 'Subjective Paper',      icon: '✍️', desc: 'Short & long answer practice',        grad: 'grad-pink',    action: 'subjective' },
  { title: 'Past & Model Papers',   icon: '📄', desc: 'Previous year & model papers',        grad: 'grad-orange',  action: 'pastpapers' },
]

function handleOption(opt) {
  const id = props.bookId
  const dest = {
    objective:   `/app/preparation/${id}/objective`,
    subjective:  `/app/preparation/${id}/subjective`,
    resources:   '/app/ebooks',
    videos:      '/app/video',
    keynotes:    '/app/smart-notes',
    simulations: '/app/simulations',
    vlab:        '/app/experiments',
    pastpapers:  '/app/past-papers',
  }[opt.action]
  if (dest) router.push(dest)
}
</script>

