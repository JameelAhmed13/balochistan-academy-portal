<template>
  <div class="login-page">
    <!-- Animated aurora background -->
    <div class="aurora-bg">
      <div class="aurora a1" /><div class="aurora a2" />
      <div class="aurora a3" /><div class="aurora a4" />
    </div>

    <!-- Neural network canvas -->
    <canvas ref="netCanvas" class="net-canvas" />

    <!-- Grid overlay -->
    <div class="grid-overlay" />

    <!-- Floating orbs -->
    <div class="orb orb1" /><div class="orb orb2" /><div class="orb orb3" />

    <!-- Corner frames -->
    <div class="frame-corner fc-tl" /><div class="frame-corner fc-tr" />
    <div class="frame-corner fc-bl" /><div class="frame-corner fc-br" />

    <!-- Left panel — AI showcase -->
    <div class="login-left">
      <div class="left-content">
        <!-- Brand -->
        <div class="brand-row">
          <div class="brand-icon">
            <img src="@/assets/logo.png" alt="" class="brand-logo-img" />
            <div class="brand-icon-glow" />
          </div>
          <div>
            <div class="brand-name">Balochistan Academy Portal</div>
            <div class="brand-tagline">AI-Powered Learning</div>
          </div>
        </div>

        <!-- Headline -->
        <h1 class="left-headline">
          Your Personal<br />
          <span class="headline-grad">AI Tutor</span><br />
          Awaits.
        </h1>

        <!-- Stats -->
        <div class="left-stats">
          <div v-for="s in showcaseStats" :key="s.label" class="left-stat">
            <div class="stat-num">{{ s.num }}</div>
            <div class="stat-lbl">{{ s.label }}</div>
          </div>
        </div>

        <!-- Floating AI card -->
        <div class="ai-showcase-card">
          <div class="showcase-header">
            <div class="showcase-avatar">⚛</div>
            <div>
              <div class="showcase-name">Einstein AI</div>
              <div class="showcase-subject">Physics · Online</div>
            </div>
            <div class="live-badge"><span class="live-dot" />LIVE</div>
          </div>
          <div class="showcase-msg">
            "Newton's 3rd Law: Every action has an equal and opposite reaction. Let me show you with a rocket example..."
          </div>
          <div class="showcase-chips">
            <span class="chip">Ask anything</span>
            <span class="chip">Urdu support</span>
            <span class="chip">Voice input</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Right panel — Login form -->
    <div class="login-right">
      <div class="glass-card" :class="{ shake: shaking }">
        <!-- Glow border top -->
        <div class="card-glow-bar" />

        <!-- Header -->
        <div class="form-header">
          <div class="form-icon">
            <svg width="28" height="28" viewBox="0 0 28 28" fill="none">
              <path d="M14 2L26 8V14C26 20 20 25.2 14 26C8 25.2 2 20 2 14V8L14 2Z"
                stroke="url(#shieldGrad)" stroke-width="1.5" fill="rgba(124,106,245,0.1)" />
              <path d="M9 14l3.5 3.5L19 10" stroke="#7c6af5" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
              <defs>
                <linearGradient id="shieldGrad" x1="0" y1="0" x2="28" y2="28">
                  <stop stop-color="#7c6af5" /><stop offset="1" stop-color="#06b6d4" />
                </linearGradient>
              </defs>
            </svg>
          </div>
          <h2 class="form-title">{{ mode === 'login' ? 'Welcome Back' : 'Create Account' }}</h2>
          <p class="form-sub">{{ mode === 'login' ? 'Sign in to continue your AI learning journey' : 'Register to start your AI learning journey' }}</p>
        </div>

        <!-- Mode toggle -->
        <div class="mode-toggle" role="tablist">
          <button type="button" role="tab" :aria-selected="mode === 'login'" class="mode-tab" :class="{ active: mode === 'login' }" @click="switchMode('login')">Sign In</button>
          <button type="button" role="tab" :aria-selected="mode === 'register'" class="mode-tab" :class="{ active: mode === 'register' }" @click="switchMode('register')">Register</button>
        </div>

        <!-- Form -->
        <form v-if="mode === 'login'" @submit.prevent="handleLogin" class="login-form">
          <!-- Username field -->
          <div class="field-wrap" :class="{ focused: focusedField === 'user', filled: form.username }">
            <label class="field-label">Username / Student ID</label>
            <div class="field-inner">
              <svg class="field-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
              </svg>
              <input
                v-model="form.username"
                type="text"
                class="glass-input"
                placeholder="Enter your username"
                required
                autocomplete="username"
                @focus="focusedField = 'user'"
                @blur="focusedField = ''"
              />
              <div class="field-glow" />
            </div>
          </div>

          <!-- Phone field -->
          <div class="field-wrap" :class="{ focused: focusedField === 'phone', filled: form.phone }">
            <label class="field-label">Phone Number</label>
            <div class="field-inner">
              <svg class="field-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 014.5 10.1a19.79 19.79 0 01-3.07-8.67A2 2 0 013.4 2h3a2 2 0 012 1.72c.127.96.361 1.903.7 2.81a2 2 0 01-.45 2.11L7.91 9.91a16 16 0 006.09 6.09l1.27-1.27a2 2 0 012.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0122 16.92z"/>
              </svg>
              <input
                v-model="form.phone"
                type="tel"
                class="glass-input"
                placeholder="03xxxxxxxxx"
                required
                autocomplete="tel"
                @focus="focusedField = 'phone'"
                @blur="focusedField = ''"
              />
              <div class="field-glow" />
            </div>
          </div>

          <!-- Error -->
          <Transition name="err">
            <div v-if="errorMsg" class="error-box">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#f87171" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><circle cx="12" cy="16" r="1" fill="#f87171"/></svg>
              {{ errorMsg }}
            </div>
          </Transition>

          <!-- Submit button -->
          <button type="submit" :disabled="loading" class="submit-btn">
            <span v-if="!loading" class="btn-content">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M15 3h4a2 2 0 012 2v14a2 2 0 01-2 2h-4M10 17l5-5-5-5M3 12h12"/>
              </svg>
              Sign In to Balochistan Academy Portal
            </span>
            <span v-else class="btn-loading">
              <span class="spin-dots"><span/><span/><span/></span>
              Authenticating…
            </span>
            <div class="btn-glow" />
          </button>
        </form>

        <!-- Register form (wired to n8n) -->
        <form v-else @submit.prevent="handleRegister" class="login-form">
          <div class="field-wrap">
            <label class="field-label">Full Name</label>
            <div class="field-inner">
              <svg class="field-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
              </svg>
              <input v-model.trim="reg.name" type="text" class="glass-input" placeholder="e.g. Ayesha Khan" required autocomplete="name" />
            </div>
          </div>

          <div class="field-wrap">
            <label class="field-label">Phone / WhatsApp</label>
            <div class="field-inner">
              <svg class="field-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 014.5 10.1a19.79 19.79 0 01-3.07-8.67A2 2 0 013.4 2h3a2 2 0 012 1.72c.127.96.361 1.903.7 2.81a2 2 0 01-.45 2.11L7.91 9.91a16 16 0 006.09 6.09l1.27-1.27a2 2 0 012.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0122 16.92z"/>
              </svg>
              <input v-model.trim="reg.phone" type="tel" class="glass-input" placeholder="03xx-xxxxxxx" required autocomplete="tel" />
            </div>
          </div>

          <div class="field-wrap">
            <label class="field-label">Email</label>
            <div class="field-inner">
              <svg class="field-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="2" y="4" width="20" height="16" rx="2"/><path d="M2 7l10 6 10-6"/>
              </svg>
              <input v-model.trim="reg.email" type="email" class="glass-input" placeholder="you@example.com" required autocomplete="email" />
            </div>
          </div>

          <div class="field-row">
            <div class="field-wrap">
              <label class="field-label">Grade</label>
              <div class="field-inner">
                <select v-model="reg.grade" class="glass-input glass-select">
                  <option value="9">Grade 9</option>
                  <option value="10">Grade 10</option>
                </select>
              </div>
            </div>
            <div class="field-wrap">
              <label class="field-label">Medium</label>
              <div class="field-inner">
                <select v-model="reg.medium" class="glass-input glass-select">
                  <option value="English">English</option>
                  <option value="Urdu">Urdu</option>
                </select>
              </div>
            </div>
          </div>

          <div class="field-wrap">
            <label class="field-label">City</label>
            <div class="field-inner">
              <svg class="field-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 10c0 7-9 12-9 12s-9-5-9-12a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/>
              </svg>
              <input v-model.trim="reg.city" type="text" class="glass-input" placeholder="Quetta" autocomplete="address-level2" />
            </div>
          </div>

          <Transition name="err">
            <div v-if="regError" class="error-box">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#f87171" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><circle cx="12" cy="16" r="1" fill="#f87171"/></svg>
              <span>Couldn’t reach the n8n register webhook (<code>{{ registerUrl }}</code>). Is the workflow <strong>active</strong>? <button type="button" class="link-btn" @click="continueDemo">Continue as demo →</button></span>
            </div>
          </Transition>

          <button type="submit" :disabled="loading" class="submit-btn">
            <span v-if="!loading" class="btn-content">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M16 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="8.5" cy="7" r="4"/><path d="M20 8v6M23 11h-6"/>
              </svg>
              Create Account &amp; Send Welcome
            </span>
            <span v-else class="btn-loading">
              <span class="spin-dots"><span/><span/><span/></span>
              Registering via n8n…
            </span>
            <div class="btn-glow" />
          </button>
        </form>

        <!-- Footer -->
        <div class="form-footer">
          <p class="switch-prompt">
            <template v-if="mode === 'login'">New here? <button type="button" class="link-btn" @click="switchMode('register')">Create an account</button></template>
            <template v-else>Already registered? <button type="button" class="link-btn" @click="switchMode('login')">Sign in</button></template>
          </p>
          <div class="demo-hint">
            <span class="hint-label">DEMO</span>
            Any username & phone · use <code>admin</code> for admin panel
          </div>
          <a href="https://wa.me/923001234567" target="_blank" class="wa-link">
            Need access? <span>WhatsApp us →</span>
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()
const toast = useToast()

const form = ref({ username: '', phone: '' })
const loading = ref(false)
const errorMsg = ref('')
const shaking = ref(false)
const focusedField = ref('')
const netCanvas = ref(null)

// ── Registration (wired to n8n) ──
const mode = ref('login') // 'login' | 'register'
const registerUrl = import.meta.env.VITE_N8N_REGISTER_URL || 'http://localhost:5678/webhook/bap-register'
const reg = ref({ name: '', phone: '', email: '', grade: '9', medium: 'English', city: '' })
const regError = ref('')

function switchMode(m) {
  mode.value = m
  errorMsg.value = ''
  regError.value = ''
}

const showcaseStats = [
  { num: '1,200+', label: 'Questions' },
  { num: '8', label: 'AI Tutors' },
  { num: '10', label: 'Subjects' },
  { num: '99%', label: 'Pass Rate' },
]

async function handleLogin() {
  loading.value = true
  errorMsg.value = ''
  try {
    await auth.login(form.value.username, form.value.phone)
    toast.add({ severity: 'success', summary: 'Welcome back!', detail: `Signed in as ${form.value.username}`, life: 3000 })
    router.push('/app')
  } catch (e) {
    errorMsg.value = e.message
    shaking.value = true
    setTimeout(() => { shaking.value = false }, 500)
  } finally {
    loading.value = false
  }
}

async function handleRegister() {
  loading.value = true
  regError.value = ''
  try {
    const res = await fetch(registerUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...reg.value, source: 'app-register', submittedAt: new Date().toISOString() }),
    })
    if (!res.ok) throw new Error(`Register webhook responded ${res.status}`)
    toast.add({ severity: 'success', summary: 'Account created!', detail: 'Welcome message sent via n8n', life: 3000 })
    await auth.login(reg.value.name, reg.value.phone)
    router.push('/app')
  } catch (e) {
    console.error('[register] n8n webhook failed:', e)
    regError.value = e.message
    shaking.value = true
    setTimeout(() => { shaking.value = false }, 500)
  } finally {
    loading.value = false
  }
}

// Skip the n8n round-trip (demo) when the webhook is unreachable
function continueDemo() {
  auth.login(reg.value.name || 'Demo Student', reg.value.phone || '03001234567')
  router.push('/app')
}

// Neural net canvas
let animHandle = null
const pts = []
function initNet() {
  const c = netCanvas.value
  if (!c) return
  c.width = c.offsetWidth
  c.height = c.offsetHeight
  for (let i = 0; i < 40; i++) {
    pts.push({ x: Math.random() * c.width, y: Math.random() * c.height,
      vx: (Math.random()-.5)*.3, vy: (Math.random()-.5)*.3 })
  }
  function draw() {
    const ctx = c.getContext('2d')
    ctx.clearRect(0, 0, c.width, c.height)
    pts.forEach(p => {
      p.x += p.vx; p.y += p.vy
      if (p.x < 0 || p.x > c.width) p.vx *= -1
      if (p.y < 0 || p.y > c.height) p.vy *= -1
      ctx.beginPath(); ctx.arc(p.x, p.y, 1.5, 0, Math.PI*2)
      ctx.fillStyle = 'rgba(124,106,245,0.3)'; ctx.fill()
    })
    for (let i = 0; i < pts.length; i++) for (let j = i+1; j < pts.length; j++) {
      const d = Math.hypot(pts[i].x - pts[j].x, pts[i].y - pts[j].y)
      if (d < 100) {
        ctx.beginPath(); ctx.moveTo(pts[i].x, pts[i].y); ctx.lineTo(pts[j].x, pts[j].y)
        ctx.strokeStyle = `rgba(124,106,245,${0.12*(1-d/100)})`; ctx.lineWidth=.5; ctx.stroke()
      }
    }
    animHandle = requestAnimationFrame(draw)
  }
  draw()
}
onMounted(initNet)
onUnmounted(() => cancelAnimationFrame(animHandle))
</script>

<style scoped>
/* ─── Base ─── */
.login-page {
  min-height: 100vh; display: flex; position: relative; overflow: hidden;
  background: #030712;
  cursor: none;
}

/* ─── Aurora background ─── */
.aurora-bg { position: fixed; inset: 0; pointer-events: none; z-index: 0; }
.aurora {
  position: absolute; border-radius: 50%; filter: blur(80px);
  animation: auroraMove 12s ease-in-out infinite alternate;
}
.a1 { width: 600px; height: 600px; top: -200px; left: -100px; background: radial-gradient(circle, rgba(124,106,245,0.25), transparent 70%); animation-delay: 0s; }
.a2 { width: 500px; height: 500px; bottom: -150px; right: -80px; background: radial-gradient(circle, rgba(6,182,212,0.2), transparent 70%); animation-delay: 3s; }
.a3 { width: 400px; height: 400px; top: 40%; left: 40%; background: radial-gradient(circle, rgba(251,191,36,0.12), transparent 70%); animation-delay: 6s; }
.a4 { width: 350px; height: 350px; bottom: 20%; left: 10%; background: radial-gradient(circle, rgba(167,139,250,0.15), transparent 70%); animation-delay: 9s; }
@keyframes auroraMove { from { transform: translate(0,0) scale(1) } to { transform: translate(30px,20px) scale(1.1) } }

.net-canvas { position: absolute; inset: 0; width: 100%; height: 100%; z-index: 1; pointer-events: none; }
.grid-overlay {
  position: fixed; inset: 0; z-index: 1; pointer-events: none;
  background-image: linear-gradient(rgba(124,106,245,0.04) 1px, transparent 1px),
                    linear-gradient(90deg, rgba(124,106,245,0.04) 1px, transparent 1px);
  background-size: 40px 40px;
}
.orb { position: absolute; border-radius: 50%; pointer-events: none; z-index: 1; }
.orb1 { width: 300px; height: 300px; top: 10%; right: 5%; background: radial-gradient(circle, rgba(124,106,245,0.08), transparent 70%); animation: float 8s ease-in-out infinite; }
.orb2 { width: 200px; height: 200px; bottom: 15%; left: 8%; background: radial-gradient(circle, rgba(251,191,36,0.07), transparent 70%); animation: float 10s ease-in-out infinite reverse; }
.orb3 { width: 150px; height: 150px; top: 60%; right: 40%; background: radial-gradient(circle, rgba(6,182,212,0.06), transparent 70%); animation: float 6s ease-in-out infinite; animation-delay: 2s; }
@keyframes float { 0%,100% { transform: translateY(0) } 50% { transform: translateY(-20px) } }

.frame-corner { position: fixed; width: 20px; height: 20px; border-color: rgba(124,106,245,0.4); border-style: solid; z-index: 2; }
.fc-tl { top: 16px; left: 16px; border-width: 2px 0 0 2px; }
.fc-tr { top: 16px; right: 16px; border-width: 2px 2px 0 0; }
.fc-bl { bottom: 16px; left: 16px; border-width: 0 0 2px 2px; }
.fc-br { bottom: 16px; right: 16px; border-width: 0 2px 2px 0; }

/* ─── Left panel ─── */
.login-left {
  flex: 1; display: none; align-items: center; justify-content: center;
  padding: 3rem; position: relative; z-index: 3;
}
@media (min-width: 1024px) { .login-left { display: flex; } }
.left-content { max-width: 440px; }

.brand-row { display: flex; align-items: center; gap: 1rem; margin-bottom: 2.5rem; }
.brand-icon {
  position: relative; width: 52px; height: 52px; border-radius: 16px;
  display: flex; align-items: center; justify-content: center;
}
.brand-logo-img { width: 64px; height: 64px; object-fit: contain; border-radius: 14px; position: relative; z-index: 1; }
.brand-icon-glow {
  position: absolute; inset: -4px; border-radius: 20px;
  background: linear-gradient(135deg, rgba(124,106,245,0.3), rgba(6,182,212,0.2));
  filter: blur(8px); z-index: -1;
}
.brand-name { font-family: 'Orbitron', sans-serif; font-weight: 700; font-size: 1.2rem; color: #f1f5f9; }
.brand-tagline { font-size: 0.75rem; color: rgba(148,163,184,0.7); margin-top: 0.1rem; }

.left-headline {
  font-family: 'Syne', sans-serif; font-weight: 800;
  font-size: clamp(2.4rem, 4vw, 3.2rem); line-height: 1.05;
  letter-spacing: -0.03em; color: #f1f5f9; margin: 0 0 2rem;
}
.headline-grad {
  background: linear-gradient(90deg, #fbbf24, #f97316, #7c6af5);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
  background-size: 200% 100%; animation: gradShift 3s linear infinite;
}
@keyframes gradShift { 0% { background-position: 0 } 100% { background-position: 200% 0 } }

.left-stats { display: grid; grid-template-columns: repeat(2,1fr); gap: 1rem; margin-bottom: 2rem; }
.left-stat {
  padding: 1rem; border-radius: 14px;
  background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.07);
  backdrop-filter: blur(10px);
}
.stat-num { font-family: 'Orbitron', sans-serif; font-weight: 700; font-size: 1.4rem; color: #fbbf24; }
.stat-lbl { color: rgba(148,163,184,0.7); font-size: 0.78rem; margin-top: 0.15rem; }

.ai-showcase-card {
  padding: 1.25rem; border-radius: 18px;
  background: rgba(255,255,255,0.04); border: 1px solid rgba(124,106,245,0.25);
  backdrop-filter: blur(20px); box-shadow: 0 8px 40px rgba(0,0,0,0.4);
}
.showcase-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; }
.showcase-avatar {
  width: 40px; height: 40px; border-radius: 12px;
  background: linear-gradient(135deg, #1e1a40, #4a35a6);
  display: flex; align-items: center; justify-content: center; font-size: 20px;
}
.showcase-name { font-weight: 700; font-size: 0.9rem; color: #f1f5f9; }
.showcase-subject { font-size: 0.75rem; color: rgba(148,163,184,0.6); }
.live-badge {
  margin-left: auto; display: flex; align-items: center; gap: 0.35rem;
  padding: 0.2rem 0.65rem; border-radius: 99px;
  background: rgba(34,197,94,0.15); border: 1px solid rgba(34,197,94,0.3);
  color: #4ade80; font-size: 0.68rem; font-weight: 700; letter-spacing: 0.08em;
}
.live-dot { width: 5px; height: 5px; border-radius: 50%; background: #4ade80; animation: dotBlink 1s infinite; }
@keyframes dotBlink { 0%,100% { opacity: 1 } 50% { opacity: 0.3 } }
.showcase-msg { color: rgba(148,163,184,0.8); font-size: 0.82rem; line-height: 1.55; margin-bottom: 0.75rem; font-style: italic; }
.showcase-chips { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.chip {
  padding: 0.25rem 0.65rem; border-radius: 99px; font-size: 0.7rem; font-weight: 500;
  background: rgba(124,106,245,0.12); border: 1px solid rgba(124,106,245,0.2);
  color: rgba(167,139,250,0.9);
}

/* ─── Right panel ─── */
.login-right {
  flex: 1; max-width: 520px; display: flex; align-items: center; justify-content: center;
  padding: 2rem 2rem; position: relative; z-index: 10;
  margin: 0 auto;
}
@media (min-width: 1024px) { .login-right { padding: 3rem 4rem; } }

.glass-card {
  width: 100%; max-width: 420px;
  /* layered frosted glass: bright top-left fall-off to a faint violet tint */
  background:
    linear-gradient(135deg, rgba(255,255,255,0.12), rgba(255,255,255,0.03) 42%, rgba(124,106,245,0.06));
  backdrop-filter: blur(28px) saturate(190%);
  -webkit-backdrop-filter: blur(28px) saturate(190%);
  border: 1px solid rgba(255,255,255,0.14);
  border-radius: 28px;
  /* outer depth + crisp top edge highlight + soft bottom shade = real glass thickness */
  box-shadow:
    0 24px 80px rgba(0,0,0,0.6),
    0 0 0 1px rgba(124,106,245,0.1),
    inset 0 1px 0 rgba(255,255,255,0.22),
    inset 0 -1px 0 rgba(0,0,0,0.25);
  overflow: hidden; position: relative; isolation: isolate;
}
/* specular light-reflection sweep across the pane */
.glass-card::before {
  content: ''; position: absolute; inset: 0; z-index: 0; pointer-events: none;
  background:
    radial-gradient(130% 80% at 0% 0%, rgba(255,255,255,0.16), transparent 55%),
    linear-gradient(115deg, transparent 30%, rgba(255,255,255,0.06) 47%, transparent 64%);
  background-size: 100% 100%, 250% 100%;
  background-position: 0 0, -40% 0;
  animation: glassSheen 9s ease-in-out infinite;
}
/* keep all card content above the sheen layer */
.glass-card > * { position: relative; z-index: 1; }
@keyframes glassSheen {
  0%, 100% { background-position: 0 0, -40% 0; }
  50%      { background-position: 0 0, 140% 0; }
}
@media (prefers-reduced-motion: reduce) {
  .glass-card::before { animation: none; }
}
.glass-card.shake { animation: shake 0.4s ease; }
@keyframes shake {
  0%,100% { transform: translateX(0) }
  20% { transform: translateX(-8px) }
  40% { transform: translateX(8px) }
  60% { transform: translateX(-6px) }
  80% { transform: translateX(6px) }
}

.card-glow-bar {
  height: 3px; width: 100%;
  background: linear-gradient(90deg, #7c6af5, #fbbf24, #06b6d4, #7c6af5);
  background-size: 300% 100%;
  animation: gradShift 3s linear infinite;
}

.form-header { padding: 2rem 2rem 1.25rem; text-align: center; }
.form-icon {
  width: 52px; height: 52px; border-radius: 16px; margin: 0 auto 1rem;
  background: rgba(124,106,245,0.12); border: 1px solid rgba(124,106,245,0.25);
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 0 24px rgba(124,106,245,0.2);
}
.form-title {
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 1.75rem;
  color: #f1f5f9; letter-spacing: -0.02em; margin: 0 0 0.35rem;
}
.form-sub { color: rgba(148,163,184,0.7); font-size: 0.875rem; margin: 0; }

/* ─── Mode toggle (Sign In / Register) ─── */
.mode-toggle {
  display: flex; gap: 0.35rem; margin: 0 2rem 0.25rem;
  padding: 0.3rem; border-radius: 14px;
  background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.08);
}
.mode-tab {
  flex: 1; padding: 0.55rem 0; border: none; border-radius: 11px;
  background: transparent; color: rgba(148,163,184,0.7);
  font-family: 'Plus Jakarta Sans', sans-serif; font-weight: 600; font-size: 0.85rem;
  cursor: none; transition: color 0.2s, background 0.2s, box-shadow 0.2s;
}
.mode-tab:hover { color: #e2e8f0; }
.mode-tab.active {
  color: #f1f5f9;
  background: linear-gradient(135deg, rgba(124,106,245,0.9), rgba(91,67,204,0.9));
  box-shadow: 0 4px 16px rgba(124,106,245,0.35);
}

.login-form { padding: 0.5rem 2rem 0; display: flex; flex-direction: column; gap: 1rem; }
.field-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
.glass-select {
  padding-left: 1rem; appearance: none; -webkit-appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23a78bfa' stroke-width='2'%3E%3Cpath d='M6 9l6 6 6-6'/%3E%3C/svg%3E");
  background-repeat: no-repeat; background-position: right 0.9rem center;
}
.glass-select option { background: #0f1424; color: #f1f5f9; }

.field-wrap { display: flex; flex-direction: column; gap: 0.4rem; }
.field-label {
  font-size: 0.78rem; font-weight: 600; letter-spacing: 0.04em; text-transform: uppercase;
  color: rgba(148,163,184,0.6); transition: color 0.2s;
}
.field-wrap.focused .field-label { color: #a78bfa; }
.field-inner { position: relative; display: flex; align-items: center; }
.field-icon {
  position: absolute; left: 14px; color: rgba(148,163,184,0.4); z-index: 1;
  transition: color 0.2s; pointer-events: none; width: 16px; height: 16px;
}
.field-wrap.focused .field-icon { color: #a78bfa; }
.glass-input {
  width: 100%; padding: 0.85rem 1rem 0.85rem 2.75rem;
  background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.1);
  border-radius: 14px; color: #f1f5f9; font-size: 0.95rem;
  font-family: 'Plus Jakarta Sans', sans-serif;
  backdrop-filter: blur(8px); -webkit-backdrop-filter: blur(8px);
  box-shadow: inset 0 1px 0 rgba(255,255,255,0.08);
  outline: none; transition: all 0.25s;
  cursor: none;
}
.glass-input::placeholder { color: rgba(148,163,184,0.3); }
.glass-input:focus {
  background: rgba(124,106,245,0.08);
  border-color: rgba(124,106,245,0.4);
  box-shadow: 0 0 0 3px rgba(124,106,245,0.12), 0 0 20px rgba(124,106,245,0.1);
}
.field-glow {
  position: absolute; inset: 0; border-radius: 14px; pointer-events: none; opacity: 0;
  background: linear-gradient(135deg, rgba(124,106,245,0.05), transparent);
  transition: opacity 0.3s;
}
.field-wrap.focused .field-glow { opacity: 1; }

.error-box {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.65rem 1rem; border-radius: 12px;
  background: rgba(239,68,68,0.08); border: 1px solid rgba(239,68,68,0.2);
  color: #fca5a5; font-size: 0.82rem;
}
.err-enter-active { transition: all 0.25s; }
.err-enter-from { opacity: 0; transform: translateY(-6px); }

.submit-btn {
  position: relative; overflow: hidden; width: 100%;
  padding: 1rem; border-radius: 14px; border: none;
  background: linear-gradient(135deg, #7c6af5, #5b43cc);
  color: white; font-weight: 700; font-size: 0.95rem;
  font-family: 'Plus Jakarta Sans', sans-serif;
  cursor: none; transition: all 0.2s;
  box-shadow: 0 4px 20px rgba(124,106,245,0.4), 0 0 0 1px rgba(124,106,245,0.3);
  margin-top: 0.25rem;
}
.submit-btn:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 32px rgba(124,106,245,0.55), 0 0 0 1px rgba(124,106,245,0.4);
}
.submit-btn:active { transform: translateY(0); }
.submit-btn:disabled { opacity: 0.7; }
.btn-content, .btn-loading {
  display: flex; align-items: center; justify-content: center; gap: 0.5rem; position: relative; z-index: 1;
}
.btn-glow {
  position: absolute; inset: 0;
  background: linear-gradient(135deg, rgba(255,255,255,0.1), transparent);
  pointer-events: none;
}
.spin-dots { display: flex; gap: 4px; }
.spin-dots span {
  width: 5px; height: 5px; border-radius: 50%; background: white;
  animation: dotJump 0.6s infinite;
}
.spin-dots span:nth-child(2) { animation-delay: 0.15s; }
.spin-dots span:nth-child(3) { animation-delay: 0.3s; }
@keyframes dotJump { 0%,80%,100% { transform: translateY(0) } 40% { transform: translateY(-6px) } }

.form-footer { padding: 1.25rem 2rem 2rem; display: flex; flex-direction: column; gap: 0.6rem; }
.switch-prompt { text-align: center; font-size: 0.82rem; color: rgba(148,163,184,0.7); margin: 0; }
.link-btn {
  background: none; border: none; padding: 0; cursor: none;
  color: #a78bfa; font-family: inherit; font-size: inherit; font-weight: 700;
  text-decoration: underline; text-underline-offset: 2px; transition: color 0.2s;
}
.link-btn:hover { color: #c4b5fd; }
.demo-hint {
  display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap;
  padding: 0.6rem 0.85rem; border-radius: 10px;
  background: rgba(251,191,36,0.06); border: 1px solid rgba(251,191,36,0.15);
  color: rgba(148,163,184,0.7); font-size: 0.78rem;
}
.hint-label {
  padding: 0.1rem 0.45rem; border-radius: 4px;
  background: rgba(251,191,36,0.15); color: #fbbf24;
  font-weight: 700; font-size: 0.65rem; letter-spacing: 0.06em;
}
code { background: rgba(255,255,255,0.08); padding: 0.1rem 0.35rem; border-radius: 4px; color: #a78bfa; font-size: 0.82rem; }
.wa-link { color: rgba(148,163,184,0.5); font-size: 0.78rem; text-decoration: none; text-align: center; transition: color 0.2s; }
.wa-link span { color: #4ade80; }
.wa-link:hover { color: rgba(148,163,184,0.9); }
</style>
