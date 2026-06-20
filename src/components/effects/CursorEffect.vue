<template>
  <div>
    <!-- Spotlight radial gradient that follows cursor -->
    <div class="cursor-spotlight" :style="spotlightStyle" />
    <!-- Main cursor dot -->
    <div class="cursor-dot" :style="dotStyle" />
    <!-- Trailing ring -->
    <div class="cursor-ring" :style="ringStyle" :class="{ clicking: isClicking, hovering: isHovering }" />
    <!-- Particle trail -->
    <div
      v-for="p in particles"
      :key="p.id"
      class="cursor-particle"
      :style="{ left: p.x + 'px', top: p.y + 'px', opacity: p.opacity, transform: `scale(${p.scale})`, background: p.color }"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue'

const mouseX = ref(-200)
const mouseY = ref(-200)
const ringX = ref(-200)
const ringY = ref(-200)
const isClicking = ref(false)
const isHovering = ref(false)
const particles = ref([])
let particleId = 0
let animFrame = null
let lastParticleTime = 0

const PARTICLE_COLORS = ['#7c6af5', '#fbbf24', '#06b6d4', '#a78bfa', '#f59e0b']

const spotlightStyle = ref({})
const dotStyle = ref({})
const ringStyle = ref({})

function lerp(a, b, t) { return a + (b - a) * t }

function animate() {
  ringX.value = lerp(ringX.value, mouseX.value, 0.12)
  ringY.value = lerp(ringY.value, mouseY.value, 0.12)

  dotStyle.value = {
    left: mouseX.value + 'px',
    top: mouseY.value + 'px',
  }
  ringStyle.value = {
    left: ringX.value + 'px',
    top: ringY.value + 'px',
  }
  spotlightStyle.value = {
    background: `radial-gradient(400px circle at ${mouseX.value}px ${mouseY.value}px, rgba(124,106,245,0.08) 0%, transparent 70%)`,
  }

  // Fade + shrink particles
  particles.value = particles.value
    .map(p => ({ ...p, opacity: p.opacity - 0.035, scale: p.scale - 0.025 }))
    .filter(p => p.opacity > 0)

  animFrame = requestAnimationFrame(animate)
}

function onMouseMove(e) {
  mouseX.value = e.clientX
  mouseY.value = e.clientY

  const now = performance.now()
  if (now - lastParticleTime > 30) {
    lastParticleTime = now
    particles.value.push({
      id: ++particleId,
      x: e.clientX,
      y: e.clientY,
      opacity: 0.7,
      scale: 1,
      color: PARTICLE_COLORS[particleId % PARTICLE_COLORS.length],
    })
    if (particles.value.length > 20) particles.value.shift()
  }
}

function onMouseDown() { isClicking.value = true }
function onMouseUp() { isClicking.value = false }

function onMouseOver(e) {
  if (e.target.matches('a, button, [role="button"], input, select, textarea, label, [tabindex]')) {
    isHovering.value = true
  }
}
function onMouseOut(e) {
  if (e.target.matches('a, button, [role="button"], input, select, textarea, label, [tabindex]')) {
    isHovering.value = false
  }
}

onMounted(() => {
  document.addEventListener('mousemove', onMouseMove, { passive: true })
  document.addEventListener('mousedown', onMouseDown)
  document.addEventListener('mouseup', onMouseUp)
  document.addEventListener('mouseover', onMouseOver)
  document.addEventListener('mouseout', onMouseOut)
  animFrame = requestAnimationFrame(animate)
  // Hide default cursor globally
  document.documentElement.style.cursor = 'none'
})

onUnmounted(() => {
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mousedown', onMouseDown)
  document.removeEventListener('mouseup', onMouseUp)
  document.removeEventListener('mouseover', onMouseOver)
  document.removeEventListener('mouseout', onMouseOut)
  cancelAnimationFrame(animFrame)
  document.documentElement.style.cursor = ''
})
</script>

<style scoped>
.cursor-spotlight {
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 999990;
  transition: none;
}
.cursor-dot {
  position: fixed;
  width: 6px;
  height: 6px;
  background: #fbbf24;
  border-radius: 50%;
  transform: translate(-50%, -50%);
  pointer-events: none;
  z-index: 999999;
  box-shadow: 0 0 8px #fbbf24, 0 0 16px rgba(251,191,36,0.5);
  transition: transform 0.05s;
}
.cursor-ring {
  position: fixed;
  width: 32px;
  height: 32px;
  border: 1.5px solid rgba(124,106,245,0.7);
  border-radius: 50%;
  transform: translate(-50%, -50%);
  pointer-events: none;
  z-index: 999998;
  transition: width 0.2s, height 0.2s, border-color 0.2s, background 0.2s;
  box-shadow: 0 0 12px rgba(124,106,245,0.3), inset 0 0 8px rgba(124,106,245,0.1);
}
.cursor-ring.hovering {
  width: 48px;
  height: 48px;
  border-color: rgba(251,191,36,0.8);
  background: rgba(251,191,36,0.05);
  box-shadow: 0 0 20px rgba(251,191,36,0.4);
}
.cursor-ring.clicking {
  width: 24px;
  height: 24px;
  background: rgba(124,106,245,0.15);
  border-color: #7c6af5;
}
.cursor-particle {
  position: fixed;
  width: 5px;
  height: 5px;
  border-radius: 50%;
  pointer-events: none;
  z-index: 999997;
  transform: translate(-50%, -50%) scale(1);
  transition: none;
}
</style>
