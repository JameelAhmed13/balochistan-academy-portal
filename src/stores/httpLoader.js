import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useHttpLoaderStore = defineStore('httpLoader', () => {
  const pending = ref(0)
  const active = computed(() => pending.value > 0)

  function start()  { pending.value++ }
  function finish() { pending.value = Math.max(0, pending.value - 1) }

  return { active, start, finish }
})
