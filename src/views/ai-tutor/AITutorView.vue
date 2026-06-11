<template>
  <div class="animate-fade-in space-y-6">
    <!-- Header -->
    <div class="text-center py-4">
      <div class="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-brand-50 border border-brand-100 text-brand-700 text-sm font-medium mb-3">
        <Bot class="w-4 h-4" /> Pakistan's First AI Video Tutor Platform
      </div>
      <h2 class="text-2xl font-bold text-slate-800">Learn with AI Legends</h2>
      <p class="text-slate-500 text-sm mt-1">Powered by Google Gemini Â· Choose your subject tutor</p>
    </div>

    <!-- Entry buttons -->
    <div class="flex gap-3 justify-center flex-wrap">
      <button class="btn-primary px-6 py-3 text-base rounded-xl">
        <Video class="w-5 h-5" /> Learn from AI Live
      </button>
      <button class="btn-secondary px-6 py-3 text-base rounded-xl">
        <MessageSquare class="w-5 h-5" /> Try Text Chat
      </button>
    </div>

    <!-- Tutor cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <TutorCard v-for="t in tutors" :key="t.slug" :tutor="t" />
    </div>
  </div>
</template>

<script setup>
import { Bot, Video, MessageSquare } from '@lucide/vue'
import { AI_TUTORS } from '@/stores/content'
import TutorCard from '@/components/tutor/TutorCard.vue'

const tutors = AI_TUTORS
</script>

