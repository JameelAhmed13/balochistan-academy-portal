<template>
  <div ref="pageEl" class="l2" @mousemove="onMouseMove">

    <!-- ── atmosphere (from classic landing): stars · aurora · cursor spotlight ── -->
    <div class="l2-stars" aria-hidden="true" />
    <div class="l2-stars l2-stars2" aria-hidden="true" />
    <div class="l2-aurora" aria-hidden="true"><i class="a1" /><i class="a2" /><i class="a3" /></div>
    <div class="l2-spotlight" :style="{ background: `radial-gradient(480px circle at ${mx}px ${my}px, rgba(124,106,245,0.09), transparent 65%)` }" />

    <!-- ── Scroll-drawn journey line (spans the whole page) ── -->
    <svg
      class="l2-line-svg"
      aria-hidden="true"
      :width="svgW"
      :height="svgH"
      :viewBox="`0 0 ${svgW} ${svgH}`"
      fill="none"
    >
      <defs>
        <linearGradient id="l2-line-grad" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" :y2="svgH">
          <stop offset="0%"  stop-color="#8a7fff" />
          <stop offset="45%" stop-color="#7c6af5" />
          <stop offset="75%" stop-color="#fbbf24" />
          <stop offset="100%" stop-color="#f59e0b" />
        </linearGradient>
        <filter id="l2-line-glow" x="-50%" y="-50%" width="200%" height="200%">
          <feGaussianBlur stdDeviation="6" result="blur" />
          <feMerge>
            <feMergeNode in="blur" /><feMergeNode in="SourceGraphic" />
          </feMerge>
        </filter>
      </defs>
      <path :d="pathD" class="l2-path-ghost" />
      <path
        ref="lineEl"
        :d="pathD"
        class="l2-path-main"
        filter="url(#l2-line-glow)"
        :stroke-dasharray="pathLen"
        :stroke-dashoffset="dashOffset"
      />
    </svg>

    <!-- glowing tip that rides the line -->
    <div v-show="tipVisible" class="l2-tip" :style="{ transform: `translate(${tip.x}px, ${tip.y}px)` }">
      <span class="l2-tip-core" /><span class="l2-tip-halo" />
    </div>

    <!-- ── NAV ── -->
    <nav class="l2-nav" :class="{ scrolled: scrolled }">
      <RouterLink to="/" class="l2-logo">
        <img src="@/assets/logo.png" alt="Balochistan Academy" class="l2-logo-mark" />
        <span class="l2-logo-text">Balochistan Academy<em>·Path</em></span>
      </RouterLink>
      <div class="l2-nav-right">
        <RouterLink to="/" class="l2-nav-lnk">Classic Landing</RouterLink>
        <a href="#join" class="l2-nav-lnk">Join</a>
        <RouterLink to="/login" class="l2-nav-cta">Start Free <ArrowRight :size="15" /></RouterLink>
      </div>
    </nav>

    <!-- ── HERO ── -->
    <header class="l2-hero">
      <div class="l2-hero-bot reveal">
        <div class="l2-bot-bubble gx">Scroll — I’ll draw your path to the top.</div>
        <Bot pose="bot-wave" class="hero-bot" />
      </div>
      <p class="l2-kicker reveal"><span class="l2-kicker-dot" /> Grade 9 · Balochistan Board · AI-Powered</p>
      <h1 class="l2-h1 reveal">
        Every topper<br />
        follows <em>a line.</em>
      </h1>
      <p class="l2-sub reveal">
        One unbroken path from your first flashcard to the merit list —
        scroll and watch it draw itself. AI robot tutors, board-pattern papers,
        instant marking and province-wide competition, all on a single track.
      </p>
      <div class="l2-hero-btns reveal">
        <RouterLink to="/login" class="l2-btn-primary">Start Learning Free <ArrowRight :size="16" /></RouterLink>
        <a href="#path" class="l2-btn-ghost"><ArrowDown :size="16" /> Trace the path</a>
      </div>
      <div ref="lineStartEl" class="l2-line-start" aria-hidden="true" />
    </header>

    <!-- ── JOURNEY STATIONS — scenes power on when the line arrives ── -->
    <section id="path" class="l2-journey">
      <div
        v-for="(m, i) in milestones"
        :key="m.title"
        class="l2-stop"
        :class="[i % 2 === 0 ? 'is-left' : 'is-right', { active: i < activeStops }]"
      >
        <div :ref="el => setStopNode(el, i)" class="l2-stop-node">
          <span class="l2-stop-ring" /><span class="l2-stop-dot" />
        </div>

        <article class="l2-stop-card reveal">
          <!-- ░ animated scene viewport ░ -->
          <div class="sc" aria-hidden="true">
            <div class="sc-head">
              <span>{{ m.scene }}</span>
              <span class="sc-live"><i />{{ i < activeStops ? 'ONLINE' : 'STANDBY' }}</span>
            </div>
            <div class="sc-standby">— STANDBY —</div>
            <span class="sc-scan" />

            <!-- Scene 1 · AI tutor teaching -->
            <div v-if="i === 0" class="sc-stage">
              <Bot pose="bot-point" class="sc1-bot" />
              <div class="sc1-bubble gx">Concept clear? Try one yourself!</div>
              <div class="sc1-board">
                <span class="sc1-board-tag">PHYSICS · NEWTON'S 2nd LAW</span>
                <i class="sc1-tl sc1-tl1 gx" /><i class="sc1-tl sc1-tl2 gx" />
                <div class="sc1-eq gx">a&nbsp;=&nbsp;F&nbsp;/&nbsp;m&nbsp;=&nbsp;5&nbsp;m/s²</div>
              </div>
            </div>

            <!-- Scene 2 · AI writing the paper -->
            <div v-else-if="i === 1" class="sc-stage">
              <div class="sc2-paper">
                <span class="sc2-ptitle">MODEL PAPER · 9th</span>
                <i class="sc2-pl sc2-pl1 gx" /><i class="sc2-pl sc2-pl2 gx" /><i class="sc2-pl sc2-pl3 gx" />
                <div class="sc2-mcq">
                  <span v-for="(o, oi) in ['A', 'B', 'C', 'D']" :key="o" class="sc2-opt gx" :class="{ pick: oi === 1 }">{{ o }}</span>
                </div>
              </div>
              <svg class="sc2-gear" viewBox="0 0 36 36"><circle cx="18" cy="18" r="10" /></svg>
              <Bot pose="bot-write" class="sc2-bot" />
              <i class="sc-spark s1" /><i class="sc-spark s2" /><i class="sc-spark s3" />
            </div>

            <!-- Scene 3 · AI examiner marking -->
            <div v-else-if="i === 2" class="sc-stage">
              <div class="sc3-rows">
                <div class="sc3-row">
                  <i class="sc3-bar" style="width: 82%" />
                  <span class="sc3-stamp ok gx"><svg viewBox="0 0 24 24"><path d="M5 13l4 4L19 7" /></svg></span>
                </div>
                <div class="sc3-row">
                  <i class="sc3-bar" style="width: 68%" />
                  <span class="sc3-stamp ok gx" style="animation-delay: 1s"><svg viewBox="0 0 24 24"><path d="M5 13l4 4L19 7" /></svg></span>
                </div>
                <div class="sc3-row">
                  <i class="sc3-bar" style="width: 74%" />
                  <span class="sc3-stamp no gx"><svg viewBox="0 0 24 24"><path d="M6 6l12 12M18 6L6 18" /></svg></span>
                </div>
                <i class="sc3-fix gx" />
              </div>
              <div class="sc3-ringwrap">
                <svg class="sc3-ring" viewBox="0 0 84 84">
                  <circle class="ring-bg" cx="42" cy="42" r="32" />
                  <circle class="ring-fg" cx="42" cy="42" r="32" />
                </svg>
                <span class="sc3-score gx">87%</span>
              </div>
              <Bot pose="bot-write" class="sc3-bot" />
            </div>

            <!-- Scene 4 · Province competition -->
            <div v-else class="sc-stage">
              <i v-for="c in 8" :key="c" class="sc4-conf" :class="`c${c}`" />
              <span class="sc4-rank gx">RANK #01 · QUETTA</span>
              <div class="sc4-podium">
                <div class="sc4-col p2 gx"><b>2</b></div>
                <div class="sc4-col p1 gx">
                  <svg class="sc4-cup gx" viewBox="0 0 24 24"><path d="M7 3h10v5a5 5 0 0 1-10 0V3zM7 5H4a3 3 0 0 0 3 4M17 5h3a3 3 0 0 1-3 4M11 13h2v3h-2zM8 16h8v3H8z" /></svg>
                  <b>1</b>
                </div>
                <div class="sc4-col p3 gx"><b>3</b></div>
              </div>
              <Bot pose="bot-cheer" class="sc4-bot" />
            </div>
          </div>

          <span class="l2-stop-step">{{ String(i + 1).padStart(2, '0') }}</span>
          <span class="l2-stop-icon"><component :is="m.icon" :size="22" /></span>
          <h3 class="l2-stop-h">{{ m.title }}</h3>
          <p class="l2-stop-p">{{ m.desc }}</p>
          <ul class="l2-stop-chips">
            <li v-for="c in m.chips" :key="c"><Check :size="12" /> {{ c }}</li>
          </ul>
        </article>
      </div>
    </section>

    <!-- ── STATS — numbers count up when the line passes through ── -->
    <section class="l2-stats" :class="{ lit: statsOn }">
      <div ref="statsAnchorEl" class="l2-center-anchor is-abs" aria-hidden="true" />
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[0].toLocaleString('en-US') }}+</span><span class="l2-stat-l">Students on the path</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[1] }}K+</span><span class="l2-stat-l">AI-graded answers</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[2] }}%</span><span class="l2-stat-l">Improved board scores</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">24/7</span><span class="l2-stat-l">AI tutor on call</span></div>
    </section>

    <!-- ── n8n AUTOMATION — pipeline lights up node by node ── -->
    <section class="l2-n8n">
      <div ref="n8nAnchorEl" class="l2-center-anchor" aria-hidden="true" />
      <p class="l2-kicker reveal"><Workflow :size="14" /> Automation · powered by your local n8n</p>
      <h2 class="l2-h2 reveal">This page is <em>wired</em>, not just pretty.</h2>
      <p class="l2-sub reveal">
        The line ends in a webhook. Every signup below is posted straight into
        <strong>n8n running on your machine</strong> — route it to Sheets, WhatsApp,
        your CRM or anywhere else, without touching this code again.
      </p>

      <div class="l2-pipe reveal" :class="{ lit: n8nOn }" role="img" aria-label="Form submission flows from this page to an n8n webhook, then to any destination you connect">
        <div class="l2-pipe-node is-src">
          <Send :size="18" />
          <span class="l2-pipe-name">Landing2 Form</span>
          <span class="l2-pipe-sub">POST · JSON</span>
        </div>
        <div class="l2-pipe-wire"><span class="l2-pipe-pulse" /></div>
        <div class="l2-pipe-node is-hub">
          <Webhook :size="18" />
          <span class="l2-pipe-name">n8n Webhook</span>
          <span class="l2-pipe-sub">localhost:5678</span>
        </div>
        <div class="l2-pipe-wire"><span class="l2-pipe-pulse" style="animation-delay: .6s" /></div>
        <div class="l2-pipe-fan">
          <div class="l2-pipe-node is-dst"><Database :size="16" /><span class="l2-pipe-name">Sheet / DB</span></div>
          <div class="l2-pipe-node is-dst"><Bell :size="16" /><span class="l2-pipe-name">WhatsApp Alert</span></div>
          <div class="l2-pipe-node is-dst"><Zap :size="16" /><span class="l2-pipe-name">Anything else</span></div>
        </div>
      </div>
    </section>

    <!-- ── LEAD FORM — glows when the line plugs in ── -->
    <section id="join" class="l2-join">
      <div ref="formAnchorEl" class="l2-center-anchor" aria-hidden="true" />
      <div class="l2-form-card reveal" :class="{ lit: formOn }">
        <template v-if="form.status !== 'success'">
          <h2 class="l2-h2 sm">End of the line.<br /><em>Start of yours.</em></h2>
          <p class="l2-form-p">Drop your details — they land directly in your local n8n workflow.</p>

          <form class="l2-form" @submit.prevent="submitLead">
            <div class="l2-field">
              <label for="l2-name">Student name</label>
              <input id="l2-name" v-model.trim="form.name" type="text" required autocomplete="name" placeholder="e.g. Ayesha Khan" />
            </div>
            <div class="l2-field">
              <label for="l2-phone">Phone / WhatsApp</label>
              <input id="l2-phone" v-model.trim="form.phone" type="tel" required autocomplete="tel" placeholder="03xx-xxxxxxx" />
            </div>
            <div class="l2-row">
              <div class="l2-field">
                <label for="l2-grade">Grade</label>
                <select id="l2-grade" v-model="form.grade">
                  <option value="9">Grade 9</option>
                  <option value="10">Grade 10</option>
                </select>
              </div>
              <div class="l2-field">
                <label for="l2-city">City</label>
                <input id="l2-city" v-model.trim="form.city" type="text" placeholder="Quetta" />
              </div>
            </div>

            <button type="submit" class="l2-btn-primary l2-btn-submit" :disabled="form.status === 'sending'">
              <span v-if="form.status === 'sending'" class="l2-spinner" aria-hidden="true" />
              <Send v-else :size="16" />
              {{ form.status === 'sending' ? 'Sending to n8n…' : 'Join via n8n Webhook' }}
            </button>

            <p v-if="form.status === 'error'" class="l2-form-err" role="alert">
              <X :size="14" /> Couldn’t reach the webhook. Is n8n running at
              <code>{{ webhookUrl }}</code> with the workflow <strong>active</strong>?
            </p>
          </form>
        </template>

        <div v-else class="l2-form-done" role="status">
          <span class="l2-done-badge"><Check :size="26" /></span>
          <h2 class="l2-h2 sm">You’re on the line.</h2>
          <p class="l2-form-p">n8n confirmed your spot — we’ll be in touch on WhatsApp.</p>
          <RouterLink to="/login" class="l2-btn-primary">Open the App <ArrowRight :size="16" /></RouterLink>
        </div>
      </div>
    </section>

    <!-- ── FOOTER ── -->
    <footer class="l2-footer">
      <div ref="lineEndEl" class="l2-line-end" aria-hidden="true" />
      <img src="@/assets/logo.png" alt="" class="l2-logo-mark sm" />
      <p>Balochistan Academy Portal — the line from page one to position one.</p>
      <nav class="l2-foot-links">
        <RouterLink to="/">Classic Landing</RouterLink>
        <RouterLink to="/login">Login</RouterLink>
        <a href="#path">The Path</a>
      </nav>
    </footer>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onBeforeUnmount, nextTick, h } from 'vue'
import {
  ArrowRight, ArrowDown, Check, X, Send, Zap, Bell, Database,
  Webhook, Workflow, Bot as BotIcon, Sparkles, ClipboardCheck, Trophy,
} from '@lucide/vue'

/* ── reusable SVG robot mascot (functional component) ────── */
function Bot(props) {
  return h('svg', { viewBox: '0 0 120 150', class: ['bot', props.pose], 'aria-hidden': 'true' }, [
    h('line', { x1: 60, y1: 10, x2: 60, y2: 22, class: 'bot-neck' }),
    h('circle', { cx: 60, cy: 8, r: 5, class: 'bot-ant' }),
    h('rect', { x: 14, y: 76, width: 12, height: 32, rx: 6, class: 'bot-limb bot-arm-l' }),
    h('rect', { x: 94, y: 76, width: 12, height: 32, rx: 6, class: 'bot-limb bot-arm-r' }),
    h('rect', { x: 44, y: 120, width: 10, height: 18, rx: 5, class: 'bot-limb' }),
    h('rect', { x: 66, y: 120, width: 10, height: 18, rx: 5, class: 'bot-limb' }),
    h('rect', { x: 34, y: 72, width: 52, height: 48, rx: 14, class: 'bot-shell' }),
    h('circle', { cx: 60, cy: 96, r: 8, class: 'bot-core' }),
    h('rect', { x: 28, y: 22, width: 64, height: 46, rx: 16, class: 'bot-shell' }),
    h('rect', { x: 36, y: 32, width: 48, height: 24, rx: 10, class: 'bot-visor' }),
    h('circle', { cx: 50, cy: 44, r: 4.5, class: 'bot-eye' }),
    h('circle', { cx: 70, cy: 44, r: 4.5, class: 'bot-eye bot-eye2' }),
    h('path', { d: 'M52 61 Q60 66 68 61', class: 'bot-mouth' }),
  ])
}
Bot.props = { pose: { type: String, default: '' } }

/* ── content ─────────────────────────────────────────────── */
const milestones = [
  {
    icon: BotIcon,
    scene: 'AI TUTOR · CH 3',
    title: 'Learn live with your AI tutor',
    desc: 'Ask anything, anytime — your robot teacher explains every concept in Urdu or English until it clicks, with chat and video lessons.',
    chips: ['24/7 doubt solving', 'Chat + video tutor', 'Urdu + English'],
  },
  {
    icon: Sparkles,
    scene: 'PAPER ENGINE',
    title: 'AI writes your paper',
    desc: 'Gemini drafts fresh objective and subjective papers in the exact Balochistan Board pattern — never the same test twice.',
    chips: ['Board pattern', 'Unlimited papers', 'Difficulty dial'],
  },
  {
    icon: ClipboardCheck,
    scene: 'AI EXAMINER',
    title: 'Marked by AI in seconds',
    desc: 'Submit your answers and watch the robot examiner stamp every line — marks, corrections and a model answer, instantly.',
    chips: ['Instant marks', 'Per-line feedback', 'Weak-topic alerts'],
  },
  {
    icon: Trophy,
    scene: 'ARENA · LIVE',
    title: 'Compete across the province',
    desc: 'Daily streaks, weekly quizzes and monthly competitions — earn coins and race the whole province up the merit list.',
    chips: ['Daily streaks', 'Coins & rewards', 'Province ranks'],
  },
]

/* ── n8n lead form ───────────────────────────────────────── */
const webhookUrl = import.meta.env.VITE_N8N_WEBHOOK_URL || 'http://localhost:5678/webhook/estudy-lead'
const form = reactive({ name: '', phone: '', grade: '9', city: '', status: 'idle' })

async function submitLead() {
  form.status = 'sending'
  try {
    const res = await fetch(webhookUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: form.name,
        phone: form.phone,
        grade: form.grade,
        city: form.city,
        source: 'landing2',
        submittedAt: new Date().toISOString(),
      }),
    })
    if (!res.ok) throw new Error(`Webhook responded ${res.status}`)
    form.status = 'success'
  } catch (err) {
    console.error('[landing2] n8n webhook failed:', err)
    form.status = 'error'
  }
}

/* ── scroll-drawn SVG line ───────────────────────────────── */
const pageEl = ref(null)
const lineEl = ref(null)
const lineStartEl = ref(null)
const lineEndEl = ref(null)
const statsAnchorEl = ref(null)
const n8nAnchorEl = ref(null)
const formAnchorEl = ref(null)
const stopNodes = []
function setStopNode(el, i) { if (el) stopNodes[i] = el }

const svgW = ref(0)
const svgH = ref(0)
const pathD = ref('')
const pathLen = ref(0)
const dashOffset = ref(0)
const tip = reactive({ x: -100, y: -100 })
const tipVisible = ref(false)
const activeStops = ref(0)
const scrolled = ref(false)
const statsOn = ref(false)
const n8nOn = ref(false)
const formOn = ref(false)
const statCounters = reactive([0, 0, 0])
const mx = ref(-500)
const my = ref(-500)

const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches

function onMouseMove(e) { mx.value = e.clientX; my.value = e.clientY }

function relY(el) {
  const pr = pageEl.value.getBoundingClientRect()
  const r = el.getBoundingClientRect()
  return r.top - pr.top + r.height / 2
}
function relX(el) {
  const pr = pageEl.value.getBoundingClientRect()
  const r = el.getBoundingClientRect()
  return r.left - pr.left + r.width / 2
}

let stopYs = []
let secY = { stats: Infinity, n8n: Infinity, form: Infinity }
let countersDone = false

function runCounters() {
  if (countersDone) return
  countersDone = true
  const targets = [12000, 350, 92]
  if (reduceMotion) { targets.forEach((t, i) => { statCounters[i] = t }); return }
  const t0 = performance.now()
  const dur = 1500
  const step = (t) => {
    const p = Math.min(1, (t - t0) / dur)
    const e = 1 - Math.pow(1 - p, 3)
    targets.forEach((tv, i) => { statCounters[i] = Math.round(tv * e) })
    if (p < 1) requestAnimationFrame(step)
  }
  requestAnimationFrame(step)
}

function buildPath() {
  const page = pageEl.value
  if (!page || !lineStartEl.value || !lineEndEl.value) return
  const w = page.clientWidth
  const h = page.scrollHeight
  svgW.value = w
  svgH.value = h

  const pts = [{ x: w / 2, y: relY(lineStartEl.value) }]
  stopNodes.forEach((el) => { if (el) pts.push({ x: relX(el), y: relY(el) }) })
  if (statsAnchorEl.value) pts.push({ x: w / 2, y: relY(statsAnchorEl.value) })
  if (n8nAnchorEl.value) pts.push({ x: w / 2, y: relY(n8nAnchorEl.value) })
  if (formAnchorEl.value) pts.push({ x: w / 2, y: relY(formAnchorEl.value) })
  pts.push({ x: w / 2, y: relY(lineEndEl.value) })

  stopYs = stopNodes.map((el) => (el ? relY(el) : Infinity))
  secY = {
    stats: statsAnchorEl.value ? relY(statsAnchorEl.value) : Infinity,
    n8n: n8nAnchorEl.value ? relY(n8nAnchorEl.value) : Infinity,
    form: formAnchorEl.value ? relY(formAnchorEl.value) : Infinity,
  }

  // smooth S-curves with vertical tangents between consecutive points
  let d = `M ${pts[0].x.toFixed(1)} ${pts[0].y.toFixed(1)}`
  for (let i = 1; i < pts.length; i++) {
    const a = pts[i - 1], b = pts[i]
    const cy = (b.y - a.y) * 0.55
    d += ` C ${a.x.toFixed(1)} ${(a.y + cy).toFixed(1)}, ${b.x.toFixed(1)} ${(b.y - cy).toFixed(1)}, ${b.x.toFixed(1)} ${b.y.toFixed(1)}`
  }
  pathD.value = d

  nextTick(() => {
    if (!lineEl.value) return
    pathLen.value = lineEl.value.getTotalLength()
    if (reduceMotion) {
      dashOffset.value = 0
      activeStops.value = milestones.length
      statsOn.value = n8nOn.value = formOn.value = true
      runCounters()
      tipVisible.value = false
    } else {
      drawn = -1
      updateLine(true)
    }
  })
}

let drawn = 0
let rafId = 0
let animating = false

function targetLen() {
  const page = pageEl.value
  if (!page || !pathLen.value) return 0
  const pr = page.getBoundingClientRect()
  const lead = window.innerHeight * 0.82
  const scrolledPx = -pr.top + lead
  // denominator shrinks by the lead overshoot so the line completes exactly at page bottom
  const progress = Math.min(1, Math.max(0, scrolledPx / (page.scrollHeight - (window.innerHeight - lead))))
  return pathLen.value * progress
}

function updateLine(instant = false) {
  const t = targetLen()
  drawn = instant ? t : drawn + (t - drawn) * 0.18
  if (Math.abs(t - drawn) < 0.5) drawn = t
  dashOffset.value = Math.max(0, pathLen.value - drawn)

  if (lineEl.value && drawn > 1) {
    const pt = lineEl.value.getPointAtLength(drawn)
    tip.x = pt.x
    tip.y = pt.y
    tipVisible.value = drawn < pathLen.value - 2
    activeStops.value = stopYs.filter((y) => pt.y >= y - 4).length
    if (!statsOn.value && pt.y >= secY.stats - 10) { statsOn.value = true; runCounters() }
    if (!n8nOn.value && pt.y >= secY.n8n - 10) n8nOn.value = true
    if (!formOn.value && pt.y >= secY.form - 10) formOn.value = true
  }
  if (!instant && Math.abs(t - drawn) >= 0.5) {
    rafId = requestAnimationFrame(() => updateLine())
  } else {
    animating = false
  }
}

function onScroll() {
  scrolled.value = window.scrollY > 40
  if (reduceMotion || animating) return
  animating = true
  rafId = requestAnimationFrame(() => updateLine())
}

/* ── reveal-on-scroll ────────────────────────────────────── */
let revealObs = null
let resizeObs = null

onMounted(async () => {
  await nextTick()
  buildPath()
  if (document.fonts?.ready) document.fonts.ready.then(buildPath)
  setTimeout(buildPath, 600)

  resizeObs = new ResizeObserver(() => buildPath())
  resizeObs.observe(pageEl.value)

  window.addEventListener('scroll', onScroll, { passive: true })
  onScroll()

  // data attribute, not a class — Vue's :class patching on elements like the
  // form card (:class="{ lit }") would wipe a manually-added class
  revealObs = new IntersectionObserver(
    (entries) => entries.forEach((e) => { if (e.isIntersecting) { e.target.dataset.in = '1'; revealObs.unobserve(e.target) } }),
    { threshold: 0.15 },
  )
  pageEl.value.querySelectorAll('.reveal').forEach((el) => {
    if (reduceMotion) el.dataset.in = '1'
    else revealObs.observe(el)
  })
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', onScroll)
  cancelAnimationFrame(rafId)
  revealObs?.disconnect()
  resizeObs?.disconnect()
})
</script>

<style scoped>
/* ════ base canvas ════ */
.l2 {
  position: relative;
  min-height: 100vh;
  overflow-x: clip;
  background:
    radial-gradient(1100px 600px at 85% -5%, rgba(124, 106, 245, 0.16), transparent 60%),
    radial-gradient(900px 500px at 10% 30%, rgba(251, 191, 36, 0.05), transparent 60%),
    radial-gradient(1000px 700px at 50% 105%, rgba(124, 106, 245, 0.12), transparent 60%),
    #05030d;
  color: #eceaf6;
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
}

/* ════ atmosphere ════ */
.l2-stars {
  position: absolute; inset: 0; z-index: 0; pointer-events: none;
  background-image:
    radial-gradient(1.5px 1.5px at 12% 22%, rgba(255, 255, 255, 0.5), transparent),
    radial-gradient(1px 1px at 64% 8%, rgba(255, 255, 255, 0.4), transparent),
    radial-gradient(1.5px 1.5px at 86% 38%, rgba(168, 164, 255, 0.5), transparent),
    radial-gradient(1px 1px at 38% 58%, rgba(255, 255, 255, 0.35), transparent),
    radial-gradient(1.5px 1.5px at 8% 82%, rgba(251, 191, 36, 0.4), transparent),
    radial-gradient(1px 1px at 92% 74%, rgba(255, 255, 255, 0.35), transparent);
  background-size: 620px 620px;
  animation: l2-twinkle 5s ease-in-out infinite;
}
.l2-stars2 { background-size: 410px 410px; animation-delay: 2.5s; opacity: 0.6; }
@keyframes l2-twinkle { 0%, 100% { opacity: 0.45; } 50% { opacity: 1; } }

.l2-aurora { position: absolute; inset: 0; z-index: 0; overflow: hidden; pointer-events: none; }
.l2-aurora i { position: absolute; width: 560px; height: 560px; border-radius: 50%; filter: blur(95px); }
.l2-aurora .a1 { top: -160px; left: -120px; background: rgba(59, 42, 132, 0.55); animation: l2-drift1 19s ease-in-out infinite alternate; }
.l2-aurora .a2 { top: 32%; right: -200px; background: rgba(35, 25, 103, 0.6); animation: l2-drift2 24s ease-in-out infinite alternate; }
.l2-aurora .a3 { bottom: -220px; left: 28%; width: 480px; background: rgba(245, 158, 11, 0.22); animation: l2-drift1 28s ease-in-out infinite alternate-reverse; }
@keyframes l2-drift1 { to { transform: translate(70px, 50px) scale(1.12); } }
@keyframes l2-drift2 { to { transform: translate(-80px, -40px) scale(0.92); } }

.l2-spotlight { position: fixed; inset: 0; z-index: 1; pointer-events: none; }

/* ════ the line ════ */
.l2-line-svg { position: absolute; inset: 0; z-index: 0; pointer-events: none; }
.l2-path-ghost { stroke: rgba(138, 127, 255, 0.10); stroke-width: 2; stroke-dasharray: 3 9; }
.l2-path-main { stroke: url(#l2-line-grad); stroke-width: 3; stroke-linecap: round; }

.l2-tip { position: absolute; top: 0; left: 0; z-index: 1; pointer-events: none; will-change: transform; }
.l2-tip-core, .l2-tip-halo { position: absolute; border-radius: 9999px; transform: translate(-50%, -50%); }
.l2-tip-core { width: 10px; height: 10px; background: #fff; box-shadow: 0 0 12px 3px rgba(251, 191, 36, 0.9); }
.l2-tip-halo { width: 34px; height: 34px; background: radial-gradient(circle, rgba(251,191,36,0.35), transparent 70%); animation: l2-halo 1.6s ease-in-out infinite; }
@keyframes l2-halo { 0%, 100% { opacity: 0.6; } 50% { opacity: 1; } }

.l2-line-start, .l2-line-end, .l2-center-anchor { width: 2px; height: 2px; margin: 0 auto; }
.l2-center-anchor.is-abs { position: absolute; left: 50%; top: 50%; margin: 0; }

/* ════ robot mascot ════ */
:deep(.bot) { overflow: visible; display: block; }
:deep(.bot-shell) { fill: #161038; stroke: rgba(138, 127, 255, 0.55); stroke-width: 2; }
:deep(.bot-visor) { fill: #07041c; }
:deep(.bot-eye) { fill: #a99cff; transform-box: fill-box; transform-origin: center; animation: l2-blink 4.2s infinite; }
:deep(.bot-eye2) { animation-delay: 0.15s; }
:deep(.bot-mouth) { fill: none; stroke: #a99cff; stroke-width: 2.5; stroke-linecap: round; }
:deep(.bot-neck) { stroke: rgba(138, 127, 255, 0.55); stroke-width: 2.5; }
:deep(.bot-ant) { fill: #fbbf24; animation: l2-antpulse 2.2s ease-in-out infinite; }
:deep(.bot-limb) { fill: #161038; stroke: rgba(138, 127, 255, 0.45); stroke-width: 2; }
:deep(.bot-core) { fill: #fbbf24; animation: l2-antpulse 2.6s ease-in-out infinite; }
:deep(.bot-arm-l), :deep(.bot-arm-r) { transform-box: fill-box; transform-origin: 50% 12%; }
:deep(.bot-wave .bot-arm-r) { animation: l2-wave 1.7s ease-in-out infinite; }
:deep(.bot-point .bot-arm-r) { transform: rotate(-115deg); }
:deep(.bot-write .bot-arm-l) { animation: l2-scribble 0.7s ease-in-out infinite; }
:deep(.bot-cheer .bot-arm-l) { animation: l2-cheerL 1s ease-in-out infinite; }
:deep(.bot-cheer .bot-arm-r) { animation: l2-cheerR 1s ease-in-out infinite; }
@keyframes l2-blink { 0%, 91%, 100% { transform: scaleY(1); } 94% { transform: scaleY(0.12); } }
@keyframes l2-antpulse { 0%, 100% { opacity: 0.55; } 50% { opacity: 1; } }
@keyframes l2-wave { 0%, 100% { transform: rotate(10deg); } 50% { transform: rotate(-110deg); } }
@keyframes l2-scribble { 0%, 100% { transform: rotate(35deg); } 50% { transform: rotate(55deg); } }
@keyframes l2-cheerL { 0%, 100% { transform: rotate(140deg); } 50% { transform: rotate(165deg); } }
@keyframes l2-cheerR { 0%, 100% { transform: rotate(-140deg); } 50% { transform: rotate(-165deg); } }

/* ════ nav ════ */
.l2-nav {
  position: fixed; top: 14px; left: 16px; right: 16px; z-index: 50;
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 18px; border-radius: 16px;
  border: 1px solid transparent; transition: background 0.25s, border-color 0.25s, backdrop-filter 0.25s;
}
.l2-nav.scrolled { background: rgba(7, 5, 18, 0.72); border-color: rgba(138, 127, 255, 0.18); backdrop-filter: blur(14px); }
.l2-logo { display: inline-flex; align-items: center; gap: 10px; text-decoration: none; }
.l2-logo-mark {
  width: 40px; height: 40px; border-radius: 11px; flex-shrink: 0;
  object-fit: contain; padding: 3px; background: #fff;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.22);
}
.l2-logo-mark.sm { width: 32px; height: 32px; border-radius: 9px; padding: 2px; }
.l2-logo-text { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 14.5px; color: #eceaf6; letter-spacing: 0.01em; }
.l2-logo-text em { font-style: normal; color: #fbbf24; }
.l2-nav-right { display: flex; align-items: center; gap: 6px; }
.l2-nav-lnk {
  padding: 9px 13px; border-radius: 10px; font-size: 13.5px; font-weight: 600;
  color: #b9b3d9; text-decoration: none; transition: color 0.2s, background 0.2s;
}
.l2-nav-lnk:hover { color: #fff; background: rgba(138, 127, 255, 0.12); }
.l2-nav-cta {
  display: inline-flex; align-items: center; gap: 6px; padding: 10px 16px; border-radius: 11px;
  font-size: 13.5px; font-weight: 700; color: #05030d; text-decoration: none;
  background: linear-gradient(135deg, #a8a4ff, #fbbf24); transition: opacity 0.2s, box-shadow 0.2s;
}
.l2-nav-cta:hover { opacity: 0.92; box-shadow: 0 0 24px rgba(251, 191, 36, 0.35); }
@media (max-width: 640px) { .l2-nav-lnk { display: none; } }

/* ════ hero ════ */
.l2-hero {
  position: relative; z-index: 2; max-width: 1080px; margin: 0 auto;
  padding: 130px 24px 96px; text-align: center;
}
.l2-hero-bot { position: relative; display: inline-block; margin-bottom: 12px; animation: l2-float 5s ease-in-out infinite; }
.hero-bot { width: 92px; }
.l2-bot-bubble {
  position: absolute; left: 100%; top: 4px; width: max-content; max-width: 220px;
  padding: 9px 14px; border-radius: 14px 14px 14px 3px; font-size: 12.5px; font-weight: 600; text-align: left;
  color: #f5f3ff; background: rgba(124, 106, 245, 0.18); border: 1px solid rgba(138, 127, 255, 0.4);
  backdrop-filter: blur(6px); opacity: 0; transform: scale(0.6); transform-origin: bottom left;
  animation: l2-pop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1) 1s forwards;
}
@keyframes l2-float { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-10px); } }
@keyframes l2-pop { to { opacity: 1; transform: scale(1); } }
@media (max-width: 640px) { .l2-bot-bubble { display: none; } }

.l2-kicker {
  display: inline-flex; align-items: center; gap: 8px;
  font-size: 12.5px; font-weight: 700; letter-spacing: 0.14em; text-transform: uppercase;
  color: #a8a4ff; margin-bottom: 28px;
}
.l2-kicker-dot { width: 7px; height: 7px; border-radius: 99px; background: #fbbf24; box-shadow: 0 0 10px rgba(251,191,36,0.9); animation: l2-halo 2s infinite; }
.l2-h1 {
  font-family: 'Syne', sans-serif; font-weight: 800;
  font-size: clamp(3rem, 9.5vw, 7.5rem); line-height: 0.98; letter-spacing: -0.04em;
  margin: 0 0 28px; color: #f5f3ff;
}
.l2-h1 em {
  font-style: italic;
  background: linear-gradient(100deg, #fbbf24, #f59e0b 60%, #8a7fff);
  -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l2-sub { max-width: 620px; margin: 0 auto 40px; font-size: clamp(1rem, 1.6vw, 1.2rem); line-height: 1.7; color: #b9b3d9; }
.l2-sub strong { color: #eceaf6; }
.l2-hero-btns { display: flex; flex-wrap: wrap; justify-content: center; gap: 14px; margin-bottom: 90px; }

.l2-btn-primary {
  display: inline-flex; align-items: center; gap: 9px; padding: 15px 28px; border: 0; border-radius: 14px;
  font-family: inherit; font-size: 15px; font-weight: 800; color: #05030d; text-decoration: none; cursor: pointer;
  background: linear-gradient(135deg, #a8a4ff, #fbbf24);
  transition: transform 0.2s, box-shadow 0.2s; box-shadow: 0 10px 36px rgba(124, 106, 245, 0.3);
}
.l2-btn-primary:hover { box-shadow: 0 14px 44px rgba(251, 191, 36, 0.4); }
.l2-btn-primary:disabled { opacity: 0.7; cursor: wait; }
.l2-btn-ghost {
  display: inline-flex; align-items: center; gap: 9px; padding: 15px 24px; border-radius: 14px;
  font-size: 15px; font-weight: 700; color: #d9d5f2; text-decoration: none; cursor: pointer;
  border: 1px solid rgba(138, 127, 255, 0.3); transition: border-color 0.2s, background 0.2s;
}
.l2-btn-ghost:hover { border-color: rgba(251, 191, 36, 0.5); background: rgba(138, 127, 255, 0.08); }

/* ════ journey stations ════ */
.l2-journey { position: relative; z-index: 2; max-width: 1180px; margin: 0 auto; padding: 40px 24px 80px; }
.l2-stop { position: relative; min-height: 300px; display: flex; align-items: center; padding: 56px 0; }
.l2-stop-node { position: absolute; top: 50%; width: 2px; height: 2px; }
.l2-stop.is-left  .l2-stop-node { left: 21%; }
.l2-stop.is-right .l2-stop-node { left: 79%; }
.l2-stop-dot, .l2-stop-ring { position: absolute; border-radius: 9999px; transform: translate(-50%, -50%); }
.l2-stop-dot { width: 14px; height: 14px; background: #1a1430; border: 2px solid rgba(138, 127, 255, 0.5); transition: background 0.4s, border-color 0.4s, box-shadow 0.4s; }
.l2-stop-ring { width: 40px; height: 40px; border: 1px solid rgba(138, 127, 255, 0.25); transition: border-color 0.4s; }
.l2-stop.active .l2-stop-dot { background: #fbbf24; border-color: #fde68a; box-shadow: 0 0 18px rgba(251, 191, 36, 0.8); }
.l2-stop.active .l2-stop-ring { border-color: rgba(251, 191, 36, 0.5); }
/* one-time ping burst when the line arrives */
.l2-stop.active .l2-stop-node::after {
  content: ''; position: absolute; width: 14px; height: 14px; border-radius: 9999px;
  transform: translate(-50%, -50%); border: 2px solid #fbbf24;
  animation: l2-ping 0.9s cubic-bezier(0, 0, 0.2, 1) 1 forwards;
}
@keyframes l2-ping { from { opacity: 0.9; } to { transform: translate(-50%, -50%) scale(5); opacity: 0; } }

.l2-stop-card {
  position: relative; width: min(480px, 100%); padding: 26px 28px 30px; border-radius: 22px;
  background: rgba(15, 11, 34, 0.78); border: 1px solid rgba(138, 127, 255, 0.16);
  backdrop-filter: blur(8px); transition: border-color 0.3s, box-shadow 0.4s;
}
.l2-stop-card:hover { border-color: rgba(251, 191, 36, 0.35); }
.l2-stop.active .l2-stop-card { box-shadow: 0 24px 70px rgba(124, 106, 245, 0.12); }
.l2-stop.is-left  .l2-stop-card { margin-left: auto;  margin-right: 4%; }
.l2-stop.is-right .l2-stop-card { margin-right: auto; margin-left: 4%; }
.l2-stop-step {
  position: absolute; top: 240px; right: 26px;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 30px;
  color: transparent; -webkit-text-stroke: 1px rgba(138, 127, 255, 0.4);
}
.l2-stop-icon {
  display: grid; place-items: center; width: 46px; height: 46px; border-radius: 13px; margin-bottom: 16px;
  color: #fbbf24; background: rgba(251, 191, 36, 0.1); border: 1px solid rgba(251, 191, 36, 0.25);
}
.l2-stop-h { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 24px; letter-spacing: -0.02em; margin: 0 0 10px; color: #f5f3ff; }
.l2-stop-p { margin: 0 0 18px; font-size: 15px; line-height: 1.65; color: #b9b3d9; }
.l2-stop-chips { display: flex; flex-wrap: wrap; gap: 8px; margin: 0; padding: 0; list-style: none; }
.l2-stop-chips li {
  display: inline-flex; align-items: center; gap: 5px; padding: 5px 11px; border-radius: 99px;
  font-size: 12px; font-weight: 600; color: #a8a4ff;
  background: rgba(138, 127, 255, 0.1); border: 1px solid rgba(138, 127, 255, 0.2);
}
@media (max-width: 860px) {
  .l2-stop { min-height: 0; padding: 36px 0; }
  .l2-stop.is-left .l2-stop-card, .l2-stop.is-right .l2-stop-card { margin: 0 auto; }
  .l2-stop.is-left .l2-stop-node { left: 8%; }
  .l2-stop.is-right .l2-stop-node { left: 92%; }
}

/* ════ scene viewports — power on when the line arrives ════ */
.sc {
  position: relative; height: 200px; margin-bottom: 22px; border-radius: 16px; overflow: hidden;
  background:
    radial-gradient(120% 120% at 80% 0%, rgba(124, 106, 245, 0.14), transparent 50%),
    linear-gradient(160deg, #0a0620, #100a28);
  border: 1px solid rgba(138, 127, 255, 0.2);
  filter: saturate(0.15) brightness(0.6);
  transition: filter 0.8s ease;
}
.l2-stop.active .sc { filter: none; }
.sc-stage { position: absolute; inset: 0; }
.sc-head {
  position: absolute; top: 10px; left: 14px; right: 14px; z-index: 2;
  display: flex; justify-content: space-between; align-items: center;
  font-size: 10px; font-weight: 700; letter-spacing: 0.14em; color: #8d86b8; text-transform: uppercase;
}
.sc-live { display: inline-flex; align-items: center; gap: 5px; color: #fbbf24; }
.sc-live i { width: 6px; height: 6px; border-radius: 99px; background: #5d5687; }
.l2-stop.active .sc-live i { background: #fbbf24; animation: l2-halo 1.4s infinite; }
.sc-standby {
  position: absolute; inset: 0; z-index: 3; display: grid; place-items: center;
  font-size: 11px; font-weight: 700; letter-spacing: 0.3em; color: #5d5687;
  background: rgba(5, 3, 13, 0.45); transition: opacity 0.6s; pointer-events: none;
}
.l2-stop.active .sc-standby { opacity: 0; }
.sc-scan {
  position: absolute; top: 0; bottom: 0; width: 70px; z-index: 1; opacity: 0;
  background: linear-gradient(90deg, transparent, rgba(138, 127, 255, 0.07), transparent);
}
.l2-stop.active .sc-scan { opacity: 1; animation: l2-scan 3.4s linear infinite; }
@keyframes l2-scan { from { transform: translateX(-90px); } to { transform: translateX(540px); } }

/* — scene 1 · AI tutor — */
.sc1-bot { position: absolute; left: 18px; bottom: 8px; width: 76px; animation: l2-float 4.5s ease-in-out infinite; }
.sc1-bubble {
  position: absolute; left: 24px; top: 30px; max-width: 150px; z-index: 2;
  padding: 7px 11px; border-radius: 12px 12px 12px 3px; font-size: 11px; font-weight: 600; color: #f5f3ff;
  background: rgba(124, 106, 245, 0.22); border: 1px solid rgba(138, 127, 255, 0.45);
  opacity: 0; transform: scale(0.5); transform-origin: bottom left;
}
.l2-stop.active .sc1-bubble { animation: l2-pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) 2.6s forwards; }
.sc1-board {
  position: absolute; right: 14px; top: 38px; width: 54%; padding: 14px 16px; border-radius: 12px;
  background: rgba(5, 3, 13, 0.65); border: 1px solid rgba(138, 127, 255, 0.25); text-align: left;
}
.sc1-board-tag { display: block; font-size: 9px; font-weight: 700; letter-spacing: 0.12em; color: #8d86b8; margin-bottom: 10px; }
.sc1-tl { display: block; height: 8px; border-radius: 99px; background: rgba(138, 127, 255, 0.35); margin-bottom: 8px; transform: scaleX(0); transform-origin: left; }
.sc1-tl1 { width: 92%; } .sc1-tl2 { width: 64%; }
.l2-stop.active .sc1-tl1 { animation: l2-grow 0.5s ease 0.3s forwards; }
.l2-stop.active .sc1-tl2 { animation: l2-grow 0.5s ease 0.8s forwards; }
.sc1-eq {
  margin-top: 12px; font-family: 'Syne', sans-serif; font-weight: 700; font-size: 15px; color: #fbbf24;
  white-space: nowrap; overflow: hidden; width: 0; border-right: 2px solid rgba(251, 191, 36, 0);
}
.l2-stop.active .sc1-eq { border-right-color: #fbbf24; animation: l2-type 1.1s steps(16) 1.3s forwards, l2-caret 0.8s step-end 1.3s 4; }
@keyframes l2-grow { to { transform: scaleX(1); } }
@keyframes l2-type { to { width: 100%; } }
@keyframes l2-caret { 50% { border-right-color: transparent; } }

/* — scene 2 · paper engine — */
.sc2-paper {
  position: absolute; left: 18px; top: 38px; width: 52%; padding: 13px 15px; border-radius: 10px;
  background: #f3f0ff; transform: rotate(-2deg); text-align: left;
  box-shadow: 0 14px 34px rgba(0, 0, 0, 0.45);
}
.sc2-ptitle { display: block; font-size: 9px; font-weight: 800; letter-spacing: 0.12em; color: #4a35a6; margin-bottom: 9px; }
.sc2-pl { display: block; height: 7px; border-radius: 99px; background: #c9c2ec; margin-bottom: 7px; transform: scaleX(0); transform-origin: left; }
.sc2-pl1 { width: 95%; } .sc2-pl2 { width: 78%; } .sc2-pl3 { width: 86%; }
.l2-stop.active .sc2-pl1 { animation: l2-grow 0.45s ease 0.3s forwards; }
.l2-stop.active .sc2-pl2 { animation: l2-grow 0.45s ease 0.75s forwards; }
.l2-stop.active .sc2-pl3 { animation: l2-grow 0.45s ease 1.2s forwards; }
.sc2-mcq { display: flex; gap: 7px; margin-top: 11px; }
.sc2-opt {
  display: grid; place-items: center; width: 24px; height: 24px; border-radius: 8px;
  font-size: 11px; font-weight: 800; color: #4a35a6; border: 1.5px solid #c9c2ec;
  opacity: 0; transform: scale(0.4);
}
.l2-stop.active .sc2-opt { animation: l2-pop 0.35s cubic-bezier(0.34, 1.56, 0.64, 1) forwards; }
.l2-stop.active .sc2-opt:nth-child(1) { animation-delay: 1.6s; }
.l2-stop.active .sc2-opt:nth-child(2) { animation-delay: 1.75s; }
.l2-stop.active .sc2-opt:nth-child(3) { animation-delay: 1.9s; }
.l2-stop.active .sc2-opt:nth-child(4) { animation-delay: 2.05s; }
.l2-stop.active .sc2-opt.pick { background: #fbbf24; border-color: #f59e0b; box-shadow: 0 0 14px rgba(251, 191, 36, 0.7); }
.sc2-bot { position: absolute; right: 16px; bottom: 8px; width: 72px; animation: l2-float 5s ease-in-out infinite; }
.sc2-gear { position: absolute; right: 30px; top: 26px; width: 30px; height: 30px; }
.sc2-gear circle { fill: none; stroke: #8a7fff; stroke-width: 6; stroke-dasharray: 4 3.5; }
.l2-stop.active .sc2-gear { animation: l2-spin 3s linear infinite; }
@keyframes l2-spin { to { transform: rotate(360deg); } }
.sc-spark { position: absolute; width: 5px; height: 5px; border-radius: 99px; background: #fbbf24; opacity: 0; }
.sc-spark.s1 { right: 78px; top: 50px; } .sc-spark.s2 { right: 110px; top: 92px; } .sc-spark.s3 { right: 60px; top: 120px; }
.l2-stop.active .sc-spark { animation: l2-twinkle 1.8s ease-in-out infinite; }
.l2-stop.active .sc-spark.s2 { animation-delay: 0.5s; }
.l2-stop.active .sc-spark.s3 { animation-delay: 1.1s; }

/* — scene 3 · AI examiner — */
.sc3-rows { position: absolute; left: 18px; top: 44px; width: 52%; }
.sc3-row { position: relative; display: flex; align-items: center; margin-bottom: 15px; }
.sc3-bar { display: block; height: 9px; border-radius: 99px; background: rgba(138, 127, 255, 0.3); }
.sc3-stamp {
  position: absolute; right: -32px; top: 50%; width: 23px; height: 23px; margin-top: -12px;
  display: grid; place-items: center; border-radius: 99px;
  opacity: 0; transform: scale(0.3);
}
.sc3-stamp svg { width: 13px; height: 13px; fill: none; stroke: currentColor; stroke-width: 3.4; stroke-linecap: round; }
.sc3-stamp.ok { color: #34d399; background: rgba(52, 211, 153, 0.14); border: 1.5px solid rgba(52, 211, 153, 0.5); }
.sc3-stamp.no { color: #fb7185; background: rgba(251, 113, 133, 0.14); border: 1.5px solid rgba(251, 113, 133, 0.5); }
.l2-stop.active .sc3-stamp { animation: l2-stamp 0.45s cubic-bezier(0.34, 1.8, 0.64, 1) 0.5s forwards; }
.l2-stop.active .sc3-stamp.no { animation-delay: 1.5s; }
@keyframes l2-stamp { 60% { transform: scale(1.25); opacity: 1; } 100% { transform: scale(1); opacity: 1; } }
.sc3-fix { display: block; width: 58%; height: 6px; border-radius: 99px; background: rgba(251, 191, 36, 0.75); transform: scaleX(0); transform-origin: left; }
.l2-stop.active .sc3-fix { animation: l2-grow 0.5s ease 2.1s forwards; }
.sc3-ringwrap { position: absolute; right: 18px; top: 38px; width: 84px; height: 84px; }
.sc3-ring { width: 84px; height: 84px; transform: rotate(-90deg); }
.ring-bg { fill: none; stroke: rgba(138, 127, 255, 0.18); stroke-width: 8; }
.ring-fg { fill: none; stroke: #fbbf24; stroke-width: 8; stroke-linecap: round; stroke-dasharray: 201; stroke-dashoffset: 201; }
.l2-stop.active .ring-fg { animation: l2-ring 1.5s cubic-bezier(0.22, 1, 0.36, 1) 0.7s forwards; }
@keyframes l2-ring { to { stroke-dashoffset: 26; } }
.sc3-score {
  position: absolute; inset: 0; display: grid; place-items: center;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 19px; color: #fbbf24; opacity: 0;
}
.l2-stop.active .sc3-score { animation: l2-fadein 0.5s ease 1.9s forwards; }
@keyframes l2-fadein { to { opacity: 1; } }
.sc3-bot { position: absolute; right: 26px; bottom: -6px; width: 58px; }

/* — scene 4 · competition arena — */
.sc4-podium { position: absolute; left: 26px; bottom: 14px; display: flex; align-items: flex-end; gap: 9px; }
.sc4-col {
  position: relative; width: 38px; border-radius: 8px 8px 0 0; display: flex; justify-content: center;
  background: linear-gradient(180deg, rgba(138, 127, 255, 0.4), rgba(138, 127, 255, 0.12));
  border: 1px solid rgba(138, 127, 255, 0.35); border-bottom: 0;
  transform: scaleY(0); transform-origin: bottom;
}
.sc4-col b { margin-top: 7px; font-family: 'Syne', sans-serif; font-size: 14px; color: #d9d5f2; }
.sc4-col.p1 { height: 86px; background: linear-gradient(180deg, rgba(251, 191, 36, 0.5), rgba(251, 191, 36, 0.12)); border-color: rgba(251, 191, 36, 0.5); }
.sc4-col.p2 { height: 58px; } .sc4-col.p3 { height: 42px; }
.l2-stop.active .sc4-col.p2 { animation: l2-grow-y 0.6s cubic-bezier(0.22, 1, 0.36, 1) 0.3s forwards; }
.l2-stop.active .sc4-col.p1 { animation: l2-grow-y 0.6s cubic-bezier(0.22, 1, 0.36, 1) 0.65s forwards; }
.l2-stop.active .sc4-col.p3 { animation: l2-grow-y 0.6s cubic-bezier(0.22, 1, 0.36, 1) 1s forwards; }
@keyframes l2-grow-y { to { transform: scaleY(1); } }
.sc4-cup {
  position: absolute; top: -34px; left: 50%; width: 26px; height: 26px; margin-left: -13px;
  fill: none; stroke: #fbbf24; stroke-width: 1.8; stroke-linejoin: round;
  opacity: 0; transform: translateY(-26px);
}
.l2-stop.active .sc4-cup { animation: l2-drop 0.55s cubic-bezier(0.34, 1.56, 0.64, 1) 1.5s forwards; }
@keyframes l2-drop { to { opacity: 1; transform: translateY(0); } }
.sc4-rank {
  position: absolute; right: 16px; top: 40px; padding: 6px 12px; border-radius: 99px;
  font-size: 10.5px; font-weight: 800; letter-spacing: 0.1em; color: #05030d;
  background: linear-gradient(135deg, #a8a4ff, #fbbf24);
  opacity: 0; transform: scale(0.5);
}
.l2-stop.active .sc4-rank { animation: l2-pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) 2s forwards; }
.sc4-bot { position: absolute; right: 24px; bottom: 4px; width: 70px; }
.sc4-conf { position: absolute; top: -12px; width: 5px; height: 10px; border-radius: 2px; opacity: 0; }
.sc4-conf.c1 { left: 12%; background: #fbbf24; } .sc4-conf.c2 { left: 26%; background: #8a7fff; }
.sc4-conf.c3 { left: 40%; background: #34d399; } .sc4-conf.c4 { left: 54%; background: #fbbf24; }
.sc4-conf.c5 { left: 66%; background: #fb7185; } .sc4-conf.c6 { left: 78%; background: #8a7fff; }
.sc4-conf.c7 { left: 88%; background: #34d399; } .sc4-conf.c8 { left: 33%; background: #fb7185; }
.l2-stop.active .sc4-conf { animation: l2-fall 2.4s linear infinite; }
.l2-stop.active .sc4-conf.c2 { animation-delay: 0.4s; } .l2-stop.active .sc4-conf.c3 { animation-delay: 0.9s; }
.l2-stop.active .sc4-conf.c4 { animation-delay: 1.3s; } .l2-stop.active .sc4-conf.c5 { animation-delay: 0.2s; }
.l2-stop.active .sc4-conf.c6 { animation-delay: 1.7s; } .l2-stop.active .sc4-conf.c7 { animation-delay: 0.7s; }
.l2-stop.active .sc4-conf.c8 { animation-delay: 2s; }
@keyframes l2-fall {
  0% { opacity: 0; transform: translateY(0) rotate(0deg); }
  8% { opacity: 1; }
  100% { opacity: 0.4; transform: translateY(230px) rotate(480deg); }
}

/* ════ stats ════ */
.l2-stats {
  position: relative; z-index: 2; max-width: 1080px; margin: 0 auto; padding: 30px 24px 110px;
  display: grid; grid-template-columns: repeat(4, 1fr); gap: 18px; text-align: center;
}
.l2-stat {
  padding: 26px 10px; border-radius: 18px; background: rgba(15, 11, 34, 0.6);
  border: 1px solid rgba(138, 127, 255, 0.14); transition: border-color 0.6s, box-shadow 0.6s;
}
.l2-stats.lit .l2-stat { border-color: rgba(251, 191, 36, 0.3); box-shadow: 0 0 34px rgba(251, 191, 36, 0.07); }
.l2-stat-v {
  display: block; font-family: 'Syne', sans-serif; font-weight: 800; font-size: clamp(1.6rem, 3.2vw, 2.4rem);
  font-variant-numeric: tabular-nums;
  background: linear-gradient(120deg, #a8a4ff, #fbbf24); -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l2-stat-l { display: block; margin-top: 6px; font-size: 12.5px; font-weight: 600; letter-spacing: 0.06em; text-transform: uppercase; color: #8d86b8; }
@media (max-width: 760px) { .l2-stats { grid-template-columns: repeat(2, 1fr); } }

/* ════ n8n section ════ */
.l2-n8n { position: relative; z-index: 2; max-width: 980px; margin: 0 auto; padding: 60px 24px 130px; text-align: center; }
.l2-h2 {
  font-family: 'Syne', sans-serif; font-weight: 800; letter-spacing: -0.03em;
  font-size: clamp(2.1rem, 5.4vw, 3.8rem); line-height: 1.06; margin: 14px 0 22px; color: #f5f3ff;
}
.l2-h2.sm { font-size: clamp(1.7rem, 4vw, 2.6rem); }
.l2-h2 em { font-style: italic; background: linear-gradient(100deg, #fbbf24, #8a7fff); -webkit-background-clip: text; background-clip: text; color: transparent; }

.l2-pipe { display: flex; align-items: center; justify-content: center; gap: 0; margin-top: 54px; flex-wrap: wrap; }
.l2-pipe-node {
  display: flex; flex-direction: column; align-items: center; gap: 5px;
  padding: 18px 22px; border-radius: 16px; color: #d9d5f2;
  background: rgba(15, 11, 34, 0.85); border: 1px solid rgba(138, 127, 255, 0.22);
  opacity: 0.4; transition: opacity 0.5s, border-color 0.5s, box-shadow 0.5s;
}
.l2-pipe.lit .l2-pipe-node { opacity: 1; }
.l2-pipe.lit .l2-pipe-node.is-src { transition-delay: 0s; }
.l2-pipe.lit .l2-pipe-node.is-hub { transition-delay: 0.35s; border-color: rgba(251, 191, 36, 0.45); color: #fbbf24; box-shadow: 0 0 32px rgba(251, 191, 36, 0.14); }
.l2-pipe.lit .l2-pipe-fan .l2-pipe-node:nth-child(1) { transition-delay: 0.7s; }
.l2-pipe.lit .l2-pipe-fan .l2-pipe-node:nth-child(2) { transition-delay: 0.9s; }
.l2-pipe.lit .l2-pipe-fan .l2-pipe-node:nth-child(3) { transition-delay: 1.1s; }
.l2-pipe-name { font-size: 13px; font-weight: 700; color: #eceaf6; }
.l2-pipe-sub { font-size: 11px; font-weight: 600; color: #8d86b8; font-variant-numeric: tabular-nums; }
.l2-pipe-wire { position: relative; width: 64px; height: 2px; background: rgba(138, 127, 255, 0.3); overflow: hidden; }
.l2-pipe-pulse {
  position: absolute; top: 0; left: 0; width: 22px; height: 2px;
  background: linear-gradient(90deg, transparent, #fbbf24, transparent);
  animation: l2-pulse 1.8s linear infinite; animation-play-state: paused;
}
.l2-pipe.lit .l2-pipe-pulse { animation-play-state: running; }
@keyframes l2-pulse { from { transform: translateX(-24px); } to { transform: translateX(70px); } }
.l2-pipe-fan { display: flex; flex-direction: column; gap: 10px; }
.l2-pipe-fan .l2-pipe-node { flex-direction: row; padding: 11px 16px; gap: 9px; }
@media (max-width: 760px) {
  .l2-pipe { flex-direction: column; gap: 12px; }
  .l2-pipe-wire { width: 2px; height: 40px; }
  .l2-pipe-pulse { width: 2px; height: 18px; background: linear-gradient(180deg, transparent, #fbbf24, transparent); animation-name: l2-pulse-v; }
  @keyframes l2-pulse-v { from { transform: translateY(-20px); } to { transform: translateY(46px); } }
}

/* ════ lead form ════ */
.l2-join { position: relative; z-index: 2; max-width: 640px; margin: 0 auto; padding: 30px 24px 140px; }
.l2-form-card {
  padding: 44px 42px; border-radius: 26px; text-align: center;
  background: rgba(15, 11, 34, 0.88); border: 1px solid rgba(138, 127, 255, 0.22);
  backdrop-filter: blur(10px); box-shadow: 0 30px 80px rgba(0, 0, 0, 0.45);
  transition: border-color 0.7s, box-shadow 0.7s;
}
.l2-form-card.lit { border-color: rgba(251, 191, 36, 0.45); box-shadow: 0 0 70px rgba(251, 191, 36, 0.1), 0 30px 80px rgba(0, 0, 0, 0.45); }
.l2-form-p { margin: 0 0 30px; font-size: 15px; line-height: 1.6; color: #b9b3d9; }
.l2-form { display: flex; flex-direction: column; gap: 16px; text-align: left; }
.l2-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.l2-field { display: flex; flex-direction: column; gap: 7px; }
.l2-field label { font-size: 12.5px; font-weight: 700; letter-spacing: 0.07em; text-transform: uppercase; color: #a8a4ff; }
.l2-field input, .l2-field select {
  padding: 13px 15px; border-radius: 12px; font: inherit; font-size: 15px; color: #f5f3ff;
  background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.25);
  transition: border-color 0.2s, box-shadow 0.2s;
}
.l2-field input::placeholder { color: #5d5687; }
.l2-field input:focus-visible, .l2-field select:focus-visible {
  outline: none; border-color: #fbbf24; box-shadow: 0 0 0 3px rgba(251, 191, 36, 0.18);
}
.l2-btn-submit { justify-content: center; margin-top: 8px; }
.l2-spinner {
  width: 16px; height: 16px; border-radius: 99px;
  border: 2px solid rgba(5, 3, 13, 0.3); border-top-color: #05030d;
  animation: l2-spin 0.7s linear infinite;
}
.l2-form-err {
  display: flex; align-items: center; flex-wrap: wrap; gap: 6px; margin: 4px 0 0;
  padding: 11px 14px; border-radius: 11px; font-size: 13px; line-height: 1.5; color: #fda4af;
  background: rgba(244, 63, 94, 0.08); border: 1px solid rgba(244, 63, 94, 0.25);
}
.l2-form-err code { font-size: 12px; color: #fecdd3; word-break: break-all; }
.l2-form-done { display: flex; flex-direction: column; align-items: center; gap: 6px; padding: 12px 0; }
.l2-done-badge {
  display: grid; place-items: center; width: 64px; height: 64px; border-radius: 9999px; margin-bottom: 14px;
  color: #05030d; background: linear-gradient(135deg, #a8a4ff, #fbbf24);
  box-shadow: 0 0 40px rgba(251, 191, 36, 0.4);
}
@media (max-width: 560px) { .l2-form-card { padding: 32px 22px; } .l2-row { grid-template-columns: 1fr; } }

/* ════ footer ════ */
.l2-footer {
  position: relative; z-index: 2; padding: 20px 24px 56px; text-align: center;
  display: flex; flex-direction: column; align-items: center; gap: 14px;
  border-top: 1px solid rgba(138, 127, 255, 0.12);
}
.l2-footer p { margin: 0; font-size: 13.5px; color: #8d86b8; }
.l2-foot-links { display: flex; gap: 20px; }
.l2-foot-links a { font-size: 13px; font-weight: 600; color: #a8a4ff; text-decoration: none; transition: color 0.2s; }
.l2-foot-links a:hover { color: #fbbf24; }
.l2-line-end { margin-bottom: 18px; }

/* ════ reveal ════ */
.reveal { opacity: 0; transform: translateY(26px); transition: opacity 0.7s ease, transform 0.7s ease; }
.reveal[data-in] { opacity: 1; transform: none; }

/* ════ reduced motion: everything at final state, no animation ════ */
@media (prefers-reduced-motion: reduce) {
  .l2 *, .l2 *::before, .l2 *::after, :deep(.bot) * { animation: none !important; transition: none !important; }
  .reveal { opacity: 1; transform: none; }
  .gx { opacity: 1 !important; transform: none !important; }
  .sc { filter: none; }
  .sc-standby { opacity: 0; }
  .sc1-eq { width: 100% !important; border-right-color: transparent !important; }
  .ring-fg { stroke-dashoffset: 26 !important; }
  .sc4-conf { opacity: 0 !important; }
  .l2-pipe-node { opacity: 1; }
}
</style>
