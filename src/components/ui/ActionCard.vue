<template>
  <div class="relative rounded-2xl overflow-hidden text-white shadow-card group cursor-pointer transition-all duration-300 hover:shadow-card-hover hover:-translate-y-0.5"
    :class="grad" @click="handleClick">
    <!-- pattern overlay -->
    <div class="absolute inset-0 opacity-10 pointer-events-none"
      style="background-image: radial-gradient(circle at 70% 30%, rgba(255,255,255,0.4) 0%, transparent 60%);" />

    <div class="relative p-4">
      <div class="text-3xl mb-2">{{ icon }}</div>
      <div class="font-bold text-base leading-tight">{{ title }}</div>
      <div class="text-xs text-white/70 mt-0.5">{{ subtitle }}</div>

      <!-- Sub-options (expand on hover) -->
      <Transition name="sub">
        <div v-if="expanded && sub.length" class="mt-3 space-y-1">
          <RouterLink v-for="s in sub" :key="s.label" :to="s.to"
            class="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-white/15 hover:bg-white/25 text-xs font-medium transition-colors"
            @click.stop
          >
            <ChevronRight class="w-3 h-3" />{{ s.label }}
          </RouterLink>
        </div>
      </Transition>
    </div>

    <!-- Arrow -->
    <div class="absolute top-3 right-3 w-7 h-7 rounded-full bg-white/15 flex items-center justify-center transition-transform group-hover:translate-x-0.5">
      <ChevronRight class="w-4 h-4" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ChevronRight } from '@lucide/vue'

const props = defineProps({
  title: String, subtitle: String, grad: String, icon: String, to: String, sub: { type: Array, default: () => [] },
})
const router = useRouter()
const expanded = ref(false)

function handleClick() {
  if (props.sub.length) { expanded.value = !expanded.value }
  else if (props.to) router.push(props.to)
}
</script>

<style scoped>
.sub-enter-active, .sub-leave-active { transition: all 0.25s ease; }
.sub-enter-from, .sub-leave-to { opacity: 0; transform: translateY(-8px); max-height: 0; }
.sub-enter-to, .sub-leave-from { max-height: 200px; }
</style>

