<template>
  <Transition name="loader-fade">
    <div v-if="visible" class="page-loader">
      <!-- Neural network canvas -->
      <canvas ref="canvas" class="loader-canvas" />

      <!-- Scan line -->
      <div class="scan-line" />

      <!-- Center content -->
      <div class="loader-center">
        <!-- AI brain icon -->
        <div class="brain-wrap">
          <svg class="brain-svg" viewBox="0 0 120 120" fill="none">
            <!-- Outer ring -->
            <circle cx="60" cy="60" r="56" stroke="rgba(124,106,245,0.2)" stroke-width="1" />
            <circle cx="60" cy="60" r="56" stroke="url(#ringGrad)" stroke-width="2"
              stroke-dasharray="352" :stroke-dashoffset="ringOffset" stroke-linecap="round" />
            <!-- Inner hex -->
            <polygon points="60,20 94,40 94,80 60,100 26,80 26,40"
              stroke="rgba(124,106,245,0.3)" stroke-width="1" fill="none" />
            <polygon points="60,20 94,40 94,80 60,100 26,80 26,40"
              stroke="url(#hexGrad)" stroke-width="1.5" fill="rgba(124,106,245,0.04)"
              stroke-dasharray="240" :stroke-dashoffset="hexOffset" />
            <!-- Core -->
            <circle cx="60" cy="60" r="22" fill="rgba(124,106,245,0.08)"
              stroke="rgba(124,106,245,0.4)" stroke-width="1.5" />
            <circle cx="60" cy="60" r="22" stroke="url(#coreGrad)" stroke-width="2"
              stroke-dasharray="138" :stroke-dashoffset="coreOffset" />
            <!-- Dots -->
            <circle v-for="d in nodes" :key="d.id"
              :cx="d.x" :cy="d.y" :r="d.r"
              :fill="d.active ? '#fbbf24' : 'rgba(124,106,245,0.5)'"
              :opacity="d.opacity"
            />
            <!-- Gradient defs -->
            <defs>
              <linearGradient id="ringGrad" x1="0" y1="0" x2="120" y2="120" gradientUnits="userSpaceOnUse">
                <stop offset="0%" stop-color="#7c6af5" />
                <stop offset="50%" stop-color="#fbbf24" />
                <stop offset="100%" stop-color="#06b6d4" />
              </linearGradient>
              <linearGradient id="hexGrad" x1="0" y1="0" x2="120" y2="120" gradientUnits="userSpaceOnUse">
                <stop offset="0%" stop-color="#06b6d4" />
                <stop offset="100%" stop-color="#7c6af5" />
              </linearGradient>
              <linearGradient id="coreGrad" x1="0" y1="0" x2="120" y2="120" gradientUnits="userSpaceOnUse">
                <stop offset="0%" stop-color="#fbbf24" />
                <stop offset="100%" stop-color="#7c6af5" />
              </linearGradient>
            </defs>
          </svg>
          <!-- Pulse rings -->
          <div class="pulse-ring r1" />
          <div class="pulse-ring r2" />
          <div class="pulse-ring r3" />
        </div>

        <!-- Logo text -->
        <div class="loader-brand">
          <span class="brand-e">e</span><span class="brand-study">Study</span><span class="brand-card">Card</span>
        </div>

        <!-- AI status text -->
        <div class="loader-status">
          <span class="status-dot" /><span>{{ statusText }}</span>
        </div>

        <!-- Progress bar -->
        <div class="loader-progress-wrap">
          <div class="loader-progress-track">
            <div class="loader-progress-fill" :style="{ width: progress + '%' }" />
            <div class="loader-progress-glow" :style="{ left: progress + '%' }" />
          </div>
          <div class="loader-progress-label">{{ Math.round(progress) }}%</div>
        </div>
      </div>

      <!-- Corner decorations -->
      <div class="corner tl" /><div class="corner tr" />
      <div class="corner bl" /><div class="corner br" />
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const visible = ref(true)
const progress = ref(0)
const statusIdx = ref(0)
const canvas = ref(null)

const STATUSES = [
  'Initializing AI Engine…',
  'Loading Question Bank…',
  'Connecting AI Tutors…',
  'Calibrating Neural Network…',
  'Almost Ready…',
]
const statusText = computed(() => STATUSES[statusIdx.value] || STATUSES[STATUSES.length - 1])

// SVG animation values
const ringOffset = ref(352)
const hexOffset = ref(240)
const coreOffset = ref(138)

const nodes = ref([
  { id:1, x:60, y:20, r:3, active:false, opacity:0.6 },
  { id:2, x:94, y:40, r:2.5, active:false, opacity:0.5 },
  { id:3, x:94, y:80, r:3, active:false, opacity:0.7 },
  { id:4, x:60, y:100, r:2.5, active:false, opacity:0.5 },
  { id:5, x:26, y:80, r:3, active:false, opacity:0.6 },
  { id:6, x:26, y:40, r:2.5, active:false, opacity:0.5 },
  { id:7, x:60, y:60, r:4, active:true, opacity:1 },
])

let animHandle = null
let canvasCtx = null
const cPoints = []

function initCanvas() {
  const c = canvas.value
  if (!c) return
  c.width = window.innerWidth
  c.height = window.innerHeight
  canvasCtx = c.getContext('2d')
  for (let i = 0; i < 60; i++) {
    cPoints.push({
      x: Math.random() * c.width,
      y: Math.random() * c.height,
      vx: (Math.random() - 0.5) * 0.4,
      vy: (Math.random() - 0.5) * 0.4,
      r: Math.random() * 2 + 0.5,
    })
  }
}

function drawCanvas() {
  if (!canvasCtx || !canvas.value) return
  const c = canvas.value
  canvasCtx.clearRect(0, 0, c.width, c.height)
  cPoints.forEach(p => {
    p.x += p.vx; p.y += p.vy
    if (p.x < 0 || p.x > c.width) p.vx *= -1
    if (p.y < 0 || p.y > c.height) p.vy *= -1
    canvasCtx.beginPath()
    canvasCtx.arc(p.x, p.y, p.r, 0, Math.PI * 2)
    canvasCtx.fillStyle = 'rgba(124,106,245,0.4)'
    canvasCtx.fill()
  })
  for (let i = 0; i < cPoints.length; i++) {
    for (let j = i + 1; j < cPoints.length; j++) {
      const dx = cPoints[i].x - cPoints[j].x
      const dy = cPoints[i].y - cPoints[j].y
      const dist = Math.sqrt(dx * dx + dy * dy)
      if (dist < 120) {
        canvasCtx.beginPath()
        canvasCtx.moveTo(cPoints[i].x, cPoints[i].y)
        canvasCtx.lineTo(cPoints[j].x, cPoints[j].y)
        canvasCtx.strokeStyle = `rgba(124,106,245,${0.15 * (1 - dist/120)})`
        canvasCtx.lineWidth = 0.5
        canvasCtx.stroke()
      }
    }
  }
}

let t = 0
function animateLoader() {
  t += 0.02
  // Spinning ring
  ringOffset.value = 352 - (352 * (t % (Math.PI * 2)) / (Math.PI * 2))
  hexOffset.value = 240 - (240 * ((t * 0.7) % (Math.PI * 2)) / (Math.PI * 2))
  coreOffset.value = 138 - (138 * ((t * 1.3) % (Math.PI * 2)) / (Math.PI * 2))
  // Node flicker
  nodes.value = nodes.value.map((n, i) => ({
    ...n,
    active: Math.sin(t * 2 + i * 0.8) > 0.5,
    opacity: 0.3 + 0.7 * Math.abs(Math.sin(t * 1.5 + i * 0.5)),
  }))
  drawCanvas()
  animHandle = requestAnimationFrame(animateLoader)
}

let progTimer = null
onMounted(() => {
  initCanvas()
  animHandle = requestAnimationFrame(animateLoader)

  let step = 0
  progTimer = setInterval(() => {
    step++
    progress.value = Math.min(100, progress.value + (Math.random() * 18 + 6))
    statusIdx.value = Math.min(STATUSES.length - 1, Math.floor(progress.value / 22))
    if (progress.value >= 100) {
      clearInterval(progTimer)
      setTimeout(() => { visible.value = false }, 600)
    }
  }, 260)
})

onUnmounted(() => {
  cancelAnimationFrame(animHandle)
  clearInterval(progTimer)
})
</script>

<style scoped>
.page-loader {
  position: fixed; inset: 0; z-index: 99999;
  background: #030712;
  display: flex; align-items: center; justify-content: center;
  overflow: hidden;
}
.loader-canvas {
  position: absolute; inset: 0; pointer-events: none;
}
.scan-line {
  position: absolute; left: 0; right: 0; height: 2px;
  background: linear-gradient(90deg, transparent, rgba(124,106,245,0.6), rgba(6,182,212,0.4), transparent);
  animation: scanMove 2.5s linear infinite;
  box-shadow: 0 0 20px rgba(124,106,245,0.4);
  pointer-events: none;
}
@keyframes scanMove {
  from { top: -2px } to { top: 100vh }
}

.loader-center {
  position: relative; z-index: 2;
  display: flex; flex-direction: column; align-items: center; gap: 1.5rem;
}

/* Brain animation */
.brain-wrap {
  position: relative; width: 140px; height: 140px;
  display: flex; align-items: center; justify-content: center;
}
.brain-svg { width: 120px; height: 120px; animation: rotateSlow 8s linear infinite; }
@keyframes rotateSlow { from { transform: rotate(0deg) } to { transform: rotate(360deg) } }

.pulse-ring {
  position: absolute; border-radius: 50%; border: 1px solid rgba(124,106,245,0.4);
  animation: pulseRing 2s ease-out infinite;
}
.r1 { width: 110px; height: 110px; animation-delay: 0s; }
.r2 { width: 130px; height: 130px; animation-delay: 0.5s; border-color: rgba(251,191,36,0.3); }
.r3 { width: 150px; height: 150px; animation-delay: 1s; border-color: rgba(6,182,212,0.2); }
@keyframes pulseRing {
  0% { transform: scale(0.8); opacity: 1 }
  100% { transform: scale(1.3); opacity: 0 }
}

/* Brand text */
.loader-brand {
  font-family: 'Orbitron', sans-serif; font-weight: 700; font-size: 2rem;
  letter-spacing: 0.1em; text-transform: uppercase;
}
.brand-e { color: #fbbf24; text-shadow: 0 0 20px rgba(251,191,36,0.8); }
.brand-study { color: #fff; }
.brand-card { color: #7c6af5; text-shadow: 0 0 20px rgba(124,106,245,0.8); }

/* Status */
.loader-status {
  display: flex; align-items: center; gap: 0.6rem;
  color: rgba(148,163,184,0.8); font-size: 0.82rem; letter-spacing: 0.08em;
  font-family: 'Plus Jakarta Sans', sans-serif;
}
.status-dot {
  width: 6px; height: 6px; border-radius: 50%;
  background: #06b6d4; box-shadow: 0 0 8px #06b6d4;
  animation: dotBlink 1s ease-in-out infinite;
}
@keyframes dotBlink { 0%,100% { opacity: 1 } 50% { opacity: 0.2 } }

/* Progress */
.loader-progress-wrap {
  display: flex; flex-direction: column; align-items: center; gap: 0.5rem;
  width: 280px;
}
.loader-progress-track {
  position: relative; width: 100%; height: 3px;
  background: rgba(255,255,255,0.06); border-radius: 99px; overflow: visible;
}
.loader-progress-fill {
  height: 100%; border-radius: 99px;
  background: linear-gradient(90deg, #7c6af5, #fbbf24, #06b6d4);
  background-size: 200% 100%;
  animation: gradShift 2s linear infinite;
  transition: width 0.3s ease-out;
  box-shadow: 0 0 10px rgba(124,106,245,0.6);
}
@keyframes gradShift { 0% { background-position: 0 } 100% { background-position: 200% 0 } }
.loader-progress-glow {
  position: absolute; top: 50%; transform: translate(-50%, -50%);
  width: 12px; height: 12px; border-radius: 50%;
  background: #fbbf24; box-shadow: 0 0 16px #fbbf24;
  transition: left 0.3s ease-out;
}
.loader-progress-label {
  color: rgba(148,163,184,0.6); font-size: 0.7rem; font-family: 'Orbitron', sans-serif;
  letter-spacing: 0.1em;
}

/* Corners */
.corner {
  position: absolute; width: 24px; height: 24px;
  border-color: rgba(124,106,245,0.5); border-style: solid;
}
.tl { top: 24px; left: 24px; border-width: 2px 0 0 2px; }
.tr { top: 24px; right: 24px; border-width: 2px 2px 0 0; }
.bl { bottom: 24px; left: 24px; border-width: 0 0 2px 2px; }
.br { bottom: 24px; right: 24px; border-width: 0 2px 2px 0; }

/* Transition */
.loader-fade-leave-active { transition: opacity 0.6s ease, transform 0.6s ease; }
.loader-fade-leave-to { opacity: 0; transform: scale(1.04); }
</style>
