<template>
  <div class="animate-fade-in space-y-6">
    <!-- Header -->
    <div class="text-center py-4">
      <div class="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-brand-50 border border-brand-100 text-brand-700 text-sm font-medium mb-3">
        <Bot class="w-4 h-4" /> Pakistan's First AI Video Tutor Platform
      </div>
      <h2 class="text-2xl font-bold text-slate-800">Learn with AI Legends</h2>
      <p class="text-slate-500 text-sm mt-1">Powered by Google Gemini · Choose your subject tutor</p>
    </div>

    <!-- Entry buttons -->
    <div class="flex gap-3 justify-center flex-wrap">
      <!-- <RouterLink :to="`/app/ai-tutor/${defaultSlug}/video`" class="btn-primary px-6 py-3 text-base rounded-xl flex items-center gap-2">
        <Video class="w-5 h-5" /> Learn from AI Live
      </RouterLink> -->
      <RouterLink :to="`/app/ai-tutor/${defaultSlug}/chat`" class="btn-primary px-6 py-3 text-base rounded-xl flex items-center gap-2">
        <MessageSquare class="w-5 h-5" /> Try Text Chat
      </RouterLink>
    </div>

    <!-- Tutor cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <TutorCard v-for="t in tutors" :key="t.slug" :tutor="t" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Bot, Video, MessageSquare } from '@lucide/vue'
import { AI_TUTORS } from '@/stores/content'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import TutorCard from '@/components/tutor/TutorCard.vue'

const auth = useAuthStore()
const catalog = useCatalogStore()
// fall back to the static personas if the catalog hasn't loaded / backend is offline
const tutors = ref(AI_TUTORS)
const defaultSlug = computed(() => tutors.value[0]?.slug || 'einstein')

onMounted(async () => {
  try {
    if (!catalog.tutors.length) await catalog.fetchTutors()
    const code = auth.user?.gradeCode
    const list = code ? catalog.tutorsForGrade(code) : catalog.tutors
    if (list.length) {
      tutors.value = list.map((t) => ({
        slug: t.slug, persona: t.persona,
        subject: t.subject_name || (typeof t.subject === 'object' ? t.subject?.name : t.subject) || 'General',
        icon: t.icon || '🤖', color: t.color || 'grad-blue', desc: t.description || t.desc || '',
        systemPrompt: t.system_prompt,
      }))
    }
  } catch { /* keep static fallback */ }
})
</script>

