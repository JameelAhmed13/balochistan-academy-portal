<template>
  <div class="card overflow-hidden group">
    <!-- Gradient header -->
    <div class="h-28 relative flex items-center justify-center" :class="tutor.color">
      <div class="absolute inset-0 opacity-20" style="background-image: radial-gradient(circle at 60% 40%, rgba(255,255,255,0.5) 0%, transparent 60%);" />
      <div class="relative text-center text-white">
        <div class="text-4xl mb-1">{{ tutor.icon }}</div>
        <div class="text-xs font-medium opacity-80">{{ tutor.subject }}</div>
      </div>
    </div>

    <div class="p-4 space-y-3">
      <div>
        <div class="font-bold text-slate-800 text-sm">{{ tutor.persona }}</div>
        <div class="text-xs text-slate-500">{{ tutor.desc }}</div>
      </div>
      <div class="flex gap-2">
        <RouterLink :to="`/app/ai-tutor/${tutor.slug}/chat`"
          class="flex-1 flex items-center justify-center gap-1.5 py-2 rounded-xl bg-slate-100 hover:bg-brand-600 hover:text-white text-slate-700 text-xs font-medium transition-all"
        >
          <MessageSquare class="w-3.5 h-3.5" /> Chat
        </RouterLink>
        <!-- <RouterLink :to="`/app/ai-tutor/${tutor.slug}/video`"
          class="flex-1 flex items-center justify-center gap-1.5 py-2 rounded-xl bg-slate-100 hover:bg-purple-600 hover:text-white text-slate-700 text-xs font-medium transition-all"
        >
          <Video class="w-3.5 h-3.5" /> Video
        </RouterLink> -->
      </div>
    </div>
  </div>
</template>

<script setup>
import { MessageSquare, Video } from '@lucide/vue'
defineProps({ tutor: Object })
</script>

