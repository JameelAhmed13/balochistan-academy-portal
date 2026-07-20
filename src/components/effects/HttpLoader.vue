<template>
  <div class="hl" :class="cls" aria-hidden="true">
    <div class="hl-track">
      <div class="hl-fill" :style="fillStyle" />
      <div class="hl-tip" :style="tipStyle" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onUnmounted } from 'vue'
import { useHttpLoaderStore } from '@/stores/httpLoader'

const store = useHttpLoaderStore()

// 'idle' | 'loading' | 'done'
const phase = ref('idle')
const width = ref(0)
let doneTimer = null

watch(() => store.active, (isActive) => {
  clearTimeout(doneTimer)

  if (isActive) {
    phase.value = 'loading'
    width.value = 0
    // next tick lets Vue paint width=0 first so the CSS transition fires
    nextTick(() => { width.value = 82 })
  } else {
    if (phase.value === 'loading') {
      phase.value = 'done'
      width.value = 100
      doneTimer = setTimeout(() => { phase.value = 'idle'; width.value = 0 }, 450)
    }
  }
})

onUnmounted(() => clearTimeout(doneTimer))

const cls = computed(() => ({
  'hl--loading': phase.value === 'loading',
  'hl--done':    phase.value === 'done',
}))

const fillStyle = computed(() => {
  if (phase.value === 'loading') {
    return { width: width.value + '%', transition: 'width 12s cubic-bezier(0.15, 0.05, 0, 0.9)' }
  }
  return { width: width.value + '%', transition: 'width 0.22s ease-in' }
})

const tipStyle = computed(() => ({
  left: `calc(${width.value}% - 2px)`,
  transition: phase.value === 'loading'
    ? 'left 12s cubic-bezier(0.15, 0.05, 0, 0.9)'
    : 'left 0.22s ease-in',
}))
</script>

<style scoped>
.hl {
  position: fixed;
  top: 0; left: 0; right: 0;
  height: 3px;
  z-index: 99998;
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.15s ease;
}
.hl--loading { opacity: 1; }
.hl--done    { opacity: 1; }

/* hide after done transition completes */
.hl:not(.hl--loading):not(.hl--done) { opacity: 0; }

.hl-track {
  position: relative;
  width: 100%;
  height: 100%;
}

.hl-fill {
  position: absolute;
  top: 0; left: 0; bottom: 0;
  border-radius: 0 2px 2px 0;
  background: linear-gradient(90deg, var(--t-accent), var(--t-accent2), var(--t-accent));
  background-size: 200% 100%;
  box-shadow: 0 0 10px color-mix(in srgb, var(--t-accent) 70%, transparent),
              0 0 4px  color-mix(in srgb, var(--t-accent2) 50%, transparent);
}

/* animated shimmer on the fill */
.hl--loading .hl-fill {
  animation: hl-shimmer 1.6s linear infinite;
}
@keyframes hl-shimmer {
  0%   { background-position: 100% 0 }
  100% { background-position: -100% 0 }
}

/* glowing dot at the leading edge */
.hl-tip {
  position: absolute;
  top: 50%;
  transform: translate(-50%, -50%);
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--t-accent2);
  box-shadow: 0 0 10px 3px color-mix(in srgb, var(--t-accent2) 80%, transparent),
              0 0 4px  2px color-mix(in srgb, var(--t-accent) 60%, transparent);
  opacity: 0;
  transition: opacity 0.2s;
}
.hl--loading .hl-tip { opacity: 1; }
.hl--done    .hl-tip { opacity: 0; }
</style>
