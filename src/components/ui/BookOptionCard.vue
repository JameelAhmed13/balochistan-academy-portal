<template>
  <div @click="$emit('click')"
    class="rounded-2xl text-white p-5 cursor-pointer group transition-all duration-300 hover:shadow-card-hover hover:-translate-y-0.5 shadow-card"
    :class="grad"
  >
    <div class="text-3xl mb-3">{{ icon }}</div>
    <div class="font-bold text-base">{{ title }}</div>
    <div class="text-xs text-white/70 mt-1">{{ desc }}</div>
    <div class="mt-3 flex items-center gap-1 text-xs text-white/80 font-medium opacity-0 group-hover:opacity-100 transition-opacity">
      Open <ArrowRight class="w-3 h-3" />
    </div>
  </div>
</template>

<script setup>
import { ArrowRight } from '@lucide/vue'
defineProps({ title: String, icon: String, desc: String, grad: String })
defineEmits(['click'])
</script>

