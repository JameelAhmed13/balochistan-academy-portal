<template>
  <div ref="pageEl" class="hl" @mousemove="onMouseMove">
    <!-- atmosphere -->
    <div class="hl-aurora" aria-hidden="true"><i class="a1" /><i class="a2" /><i class="a3" /></div>
    <div class="hl-spot" :style="{ background: `radial-gradient(460px circle at ${mx}px ${my}px, var(--t-acc-alpha-sm), transparent 65%)` }" />

    <!-- ── NAV ── -->
    <nav class="hl-nav glass" :class="{ scrolled }">
      <a href="#top" class="hl-logo">
        <span class="hl-logo-mark">e</span>
        <span class="hl-logo-text">Study<em>Card</em></span>
      </a>
      <div class="hl-nav-links">
        <a href="#about" class="hl-nlink">About</a>
        <a href="#features" class="hl-nlink">Features</a>
        <a href="#system" class="hl-nlink">How it works</a>
        <a href="#contact" class="hl-nlink">Contact</a>
      </div>
      <div class="hl-nav-right">
        <button class="hl-icon-btn" :aria-label="theme.isDark ? 'Switch to light' : 'Switch to dark'" @click="theme.toggle()">
          <Sun v-if="theme.isDark" :size="16" /><Moon v-else :size="16" />
        </button>
        <RouterLink to="/login" class="hl-btn hl-btn-primary sm">Login <ArrowRight :size="14" /></RouterLink>
      </div>
    </nav>

    <!-- ── HERO ── -->
    <header id="top" class="hl-hero">
      <div class="hl-hero-copy">
        <span class="hl-badge reveal"><Sparkles :size="13" /> Pakistan's smart learning companion</span>
        <Transition name="hl-rot" mode="out-in">
          <div :key="slide" class="hl-rot-block">
            <h1 class="hl-h1">{{ slides[slide].title }}<br /><em>{{ slides[slide].em }}</em></h1>
            <p class="hl-sub">{{ slides[slide].sub }}</p>
          </div>
        </Transition>
        <div class="hl-hero-btns reveal">
          <RouterLink to="/login" class="hl-btn hl-btn-primary">Start Learning Free <ArrowRight :size="16" /></RouterLink>
          <a href="#system" class="hl-btn hl-btn-ghost"><ArrowDown :size="16" /> See how it works</a>
        </div>
        <div class="hl-hero-stats reveal">
          <div v-for="s in heroStats" :key="s.label"><b>{{ s.val }}</b><span>{{ s.label }}</span></div>
        </div>
      </div>

      <!-- hero device: iPhone with student self-test UI -->
      <div class="hl-hero-device reveal">
        <Phone>
          <div class="ui-app">
            <div class="ui-appbar"><ChevronLeft :size="13" /> Student Self Test</div>
            <div class="ui-list">
              <div v-for="(r, i) in selfTestRows" :key="r.s" class="ui-row" :class="{ open: i === 1 }">
                <span class="ui-row-av" :style="{ background: r.c }">{{ r.s[0] }}</span>
                <span class="ui-row-info"><b>{{ r.s }}</b><i>CLASS 10 · KPTB</i></span>
                <ChevronDown :size="13" class="ui-row-chev" />
                <div v-if="i === 1" class="ui-row-actions">
                  <span class="ui-pill"><ClipboardList :size="12" /> Objective</span>
                  <span class="ui-pill"><PenLine :size="12" /> Subjective</span>
                </div>
              </div>
            </div>
          </div>
        </Phone>
        <Bot pose="bot-wave" class="hl-hero-bot" />
      </div>
    </header>

    <!-- ── TICKER ── -->
    <div class="hl-ticker" aria-hidden="true">
      <div class="hl-ticker-track">
        <template v-for="n in 2" :key="n">
          <span v-for="s in subjects" :key="`${n}-${s}`" class="hl-tick">{{ s }}<i /></span>
        </template>
      </div>
    </div>

    <!-- ── ABOUT ── -->
    <section id="about" class="hl-sec hl-about">
      <div class="hl-about-card glass reveal">
        <span class="hl-kicker"><BookOpen :size="14" /> About E-Study Card</span>
        <p class="hl-about-text">
          E-Study Card is a modern learning companion built to simplify education for students across Pakistan.
          Whether preparing for <strong>board exams, entry tests</strong>, or self-study, our platform offers smart
          tools, interactive resources, and progress tracking to keep learners on the right path. With expert-designed
          notes, past papers, quizzes, and mock tests, students gain the confidence they need to excel — and parents
          stay connected through dedicated monitoring. <strong>Our goal:</strong> a stress-free, engaging, and
          result-driven learning journey for every student.
        </p>
      </div>
    </section>

    <!-- ── LEARN / PRACTICE / ASK ── -->
    <section class="hl-sec">
      <div class="hl-sec-head reveal">
        <h2 class="hl-h2">Better Learning. <em>Better Results.</em></h2>
        <p class="hl-sec-sub">Three simple moves — learn it, practice it, ask when stuck.</p>
      </div>
      <div class="hl-trio">
        <div v-for="(c, i) in trio" :key="c.title" class="hl-trio-card glass reveal" :style="{ transitionDelay: i * 80 + 'ms' }">
          <span class="hl-trio-ic" :style="{ color: c.color }"><component :is="c.icon" :size="22" /></span>
          <h3>{{ c.title }}</h3>
          <p>{{ c.desc }}</p>
        </div>
      </div>
    </section>

    <!-- ── KEY FEATURES ── -->
    <section id="features" class="hl-sec">
      <div class="hl-sec-head reveal">
        <span class="hl-kicker center"><LayoutGrid :size="14" /> Key Features</span>
        <h2 class="hl-h2">Everything in <em>one platform</em></h2>
        <p class="hl-sec-sub">Comprehensive tools and resources to make learning smarter, faster, and stress-free.</p>
      </div>
      <div class="hl-feat-grid">
        <div v-for="(f, i) in features" :key="f" class="hl-feat reveal" :style="{ transitionDelay: (i % 4) * 60 + 'ms' }">
          <Check :size="15" class="hl-feat-ck" /><span>{{ f }}</span>
        </div>
      </div>
    </section>

    <!-- ── PRACTICE TESTS — desktop + phone mockups ── -->
    <section id="system" class="hl-sec hl-split">
      <div class="hl-split-copy reveal">
        <span class="hl-kicker"><ClipboardCheck :size="14" /> Practice anytime</span>
        <h2 class="hl-h2">Objective &amp; Subjective<br /><em>tests, day or night.</em></h2>
        <p class="hl-sec-sub left">Take real-exam style quizzes and track your scores instantly. AI grades your
          long answers with marks and feedback — practise at your own pace, on phone or desktop.</p>
        <ul class="hl-ticks">
          <li v-for="t in ['Instant auto-grading', 'AI feedback on long answers', 'Scores tracked over time']" :key="t"><Check :size="14" /> {{ t }}</li>
        </ul>
      </div>
      <div class="hl-split-devices reveal">
        <Browser class="hl-browser">
          <div class="ui-dash">
            <div class="ui-dash-side">
              <span class="ui-dash-logo">eStudy</span>
              <i v-for="n in 5" :key="n" :class="{ on: n === 2 }" />
            </div>
            <div class="ui-dash-main">
              <div class="ui-dash-head"><b>Objective Test · Physics</b><span class="ui-score">87%</span></div>
              <div class="ui-q" v-for="q in 3" :key="q">
                <i class="ui-q-bar" :style="{ width: 70 + q * 6 + '%' }" />
                <span class="ui-opt" v-for="o in 4" :key="o" :class="{ pick: o === 2 }">{{ ['A','B','C','D'][o-1] }}</span>
              </div>
            </div>
          </div>
        </Browser>
        <Phone class="hl-split-phone">
          <div class="ui-app">
            <div class="ui-appbar"><ClipboardCheck :size="13" /> Result</div>
            <div class="ui-result">
              <div class="ui-ring"><svg viewBox="0 0 80 80"><circle cx="40" cy="40" r="32" class="rbg" /><circle cx="40" cy="40" r="32" class="rfg" /></svg><b>87%</b></div>
              <span class="ui-result-lbl">Great work!</span>
              <i class="ui-bar" v-for="n in 3" :key="n" :style="{ width: [90,72,60][n-1] + '%' }" />
            </div>
          </div>
        </Phone>
      </div>
    </section>

    <!-- ── FULL SYSTEM STORY ── -->
    <section class="hl-sec hl-split reverse">
      <div class="hl-split-copy reveal">
        <span class="hl-kicker"><GraduationCap :size="14" /> All Boards. All Subjects.</span>
        <h2 class="hl-h2">From your grade to the <em>merit list.</em></h2>
        <p class="hl-sec-sub left">Pick your grade and the whole syllabus opens up — chat with an AI tutor, take
          tests, and climb the leaderboard. Admins manage grades, syllabus and tutors; students just learn.</p>
        <div class="hl-flow">
          <span v-for="(f, i) in flow" :key="f"><b>{{ i + 1 }}</b>{{ f }}</span>
        </div>
      </div>
      <div class="hl-split-devices reveal">
        <Phone class="hl-split-phone">
          <div class="ui-app">
            <div class="ui-appbar"><Bot2 :size="13" /> AI Tutor · Physics</div>
            <div class="ui-chat">
              <div class="ui-msg user">Explain Newton's 2nd law?</div>
              <div class="ui-msg ai">Force = mass × acceleration. So a = F/m — the heavier the object, the smaller the acceleration for the same force.</div>
              <div class="ui-msg user">Give me a question.</div>
            </div>
            <div class="ui-chatbar"><span>Ask anything…</span><Send :size="12" /></div>
          </div>
        </Phone>
        <Browser class="hl-browser">
          <div class="ui-dash">
            <div class="ui-dash-side"><span class="ui-dash-logo">Admin</span><i v-for="n in 5" :key="n" :class="{ on: n === 3 }" /></div>
            <div class="ui-dash-main">
              <div class="ui-dash-head"><b>Leaderboard · Grade 10</b><Trophy :size="14" /></div>
              <div class="ui-lb" v-for="(p, i) in leaders" :key="p"><span class="ui-lb-rank" :class="'r'+(i+1)">{{ i+1 }}</span><span class="ui-lb-name">{{ p }}</span><i class="ui-lb-bar" :style="{ width: [92,85,78][i] + '%' }" /><b>{{ [92,85,78][i] }}%</b></div>
            </div>
          </div>
        </Browser>
      </div>
    </section>

    <!-- ── WHY CHOOSE ── -->
    <section class="hl-sec">
      <div class="hl-sec-head reveal">
        <span class="hl-kicker center"><Star :size="14" /> Why Choose E-Study Card?</span>
        <h2 class="hl-h2">Smart tools. <em>Reliable resources.</em></h2>
      </div>
      <div class="hl-why">
        <div v-for="(w, i) in why" :key="w.title" class="hl-why-card glass reveal" :style="{ transitionDelay: i * 80 + 'ms' }">
          <span class="hl-why-ic"><component :is="w.icon" :size="22" /></span>
          <h3>{{ w.title }}</h3>
          <p>{{ w.desc }}</p>
        </div>
      </div>
    </section>

    <!-- ── STATS ── -->
    <section class="hl-sec hl-stats" ref="statsEl">
      <div v-for="(s, i) in stats" :key="s.label" class="hl-stat glass reveal">
        <b>{{ statOut[i] }}{{ s.suffix }}</b><span>{{ s.label }}</span>
      </div>
    </section>

    <!-- ── CONTACT ── -->
    <section id="contact" class="hl-sec hl-contact">
      <div class="hl-sec-head reveal">
        <span class="hl-kicker center"><Send :size="14" /> Need Assistance? We're Here to Help!</span>
        <h2 class="hl-h2">Get in <em>touch</em></h2>
        <p class="hl-sec-sub">Questions about our courses or packages? Drop your details and our team will reach out.</p>
      </div>
      <div class="hl-contact-grid">
        <form class="hl-form glass reveal" @submit.prevent="submitLead">
          <template v-if="form.status !== 'success'">
            <div class="hl-form-row">
              <input v-model.trim="form.first" type="text" placeholder="First Name" required />
              <input v-model.trim="form.last" type="text" placeholder="Last Name" />
            </div>
            <input v-model.trim="form.email" type="email" placeholder="Your Email" />
            <input v-model.trim="form.phone" type="tel" placeholder="Your Phone" required />
            <textarea v-model.trim="form.msg" rows="4" placeholder="Tell us about your learning needs…" />
            <button type="submit" class="hl-btn hl-btn-primary full" :disabled="form.status === 'sending'">
              <span v-if="form.status === 'sending'" class="hl-spin" />{{ form.status === 'sending' ? 'Sending…' : 'Send Message' }}
            </button>
            <p v-if="form.status === 'error'" class="hl-form-err"><X :size="13" /> Couldn't send right now — please WhatsApp us instead.</p>
          </template>
          <div v-else class="hl-form-done">
            <span class="hl-done-badge"><Check :size="26" /></span>
            <b>Thank you!</b><p>We've received your message and will get back to you shortly.</p>
          </div>
        </form>
        <div class="hl-contact-info glass reveal">
          <a :href="`mailto:${info.email}`" class="hl-ci-row"><Mail :size="16" /><span>{{ info.email }}</span></a>
          <a href="https://www.estudycard.com" target="_blank" rel="noopener" class="hl-ci-row"><Globe :size="16" /><span>www.estudycard.com</span></a>
          <div class="hl-ci-row"><MapPin :size="16" /><span>{{ info.address }}</span></div>
          <a :href="`tel:${info.phone1}`" class="hl-ci-row"><Phone2 :size="16" /><span>{{ info.phone1 }}</span></a>
          <a :href="`tel:${info.phone2}`" class="hl-ci-row"><Phone2 :size="16" /><span>{{ info.phone2 }}</span></a>
          <a href="https://wa.me/923000746850" target="_blank" rel="noopener" class="hl-btn hl-btn-ghost full" style="margin-top:auto">Chat on WhatsApp</a>
        </div>
      </div>
    </section>

    <!-- ── FOOTER ── -->
    <footer class="hl-footer">
      <div class="hl-foot-grid">
        <div class="hl-foot-brand">
          <a href="#top" class="hl-logo"><span class="hl-logo-mark">e</span><span class="hl-logo-text">Study<em>Card</em></span></a>
          <p>High-quality educational resources — digital textbooks, notes, and smart preparation tools. Learning made easy, engaging, and accessible for all students.</p>
        </div>
        <nav class="hl-foot-col" aria-label="Platform">
          <b>Platform</b>
          <a href="#about">About</a><a href="#features">Features</a><a href="#system">How it works</a>
          <RouterLink to="/login">Login / Register</RouterLink>
        </nav>
        <nav class="hl-foot-col" aria-label="Legal">
          <b>Legal</b>
          <a href="#">Privacy Policy</a><a href="#">Refund Policy</a><a href="#">Terms &amp; Conditions</a><a href="#">Cancellation Policy</a>
        </nav>
        <nav class="hl-foot-col" aria-label="Contact">
          <b>Contact Us</b>
          <a :href="`mailto:${info.email}`">{{ info.email }}</a>
          <a :href="`tel:${info.phone1}`">{{ info.phone1 }}</a>
          <a :href="`tel:${info.phone2}`">{{ info.phone2 }}</a>
        </nav>
      </div>
      <div class="hl-foot-bottom">© 2026 E-Study Card · All Rights Reserved · New Century Educational System</div>
    </footer>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onBeforeUnmount, h } from 'vue'
import {
  ArrowRight, ArrowDown, Sparkles, Check, X, Send, Star, BookOpen, LayoutGrid,
  ClipboardCheck, ClipboardList, PenLine, GraduationCap, Trophy, Mail, Globe,
  MapPin, Phone as Phone2, ChevronDown, ChevronLeft, BookMarked, Brain, MessagesSquare,
  Sun, Moon, Library, UserCheck, FileStack, Bot as Bot2,
} from '@lucide/vue'
import { useThemeStore } from '@/stores/theme'

const theme = useThemeStore()

/* ── CSS device frames ── */
function Phone(_p, { slots }) {
  return h('div', { class: 'dv-phone' }, [
    h('span', { class: 'dv-notch' }),
    h('div', { class: 'dv-phone-screen' }, slots.default?.()),
  ])
}
function Browser(_p, { slots }) {
  return h('div', { class: 'dv-browser glass' }, [
    h('div', { class: 'dv-bar' }, [h('i'), h('i'), h('i'), h('span', { class: 'dv-url' }, 'app.estudycard.com')]),
    h('div', { class: 'dv-browser-screen' }, slots.default?.()),
  ])
}
function Bot(props) {
  return h('svg', { viewBox: '0 0 120 150', class: ['hl-bot', props.pose], 'aria-hidden': 'true' }, [
    h('rect', { x: 14, y: 76, width: 12, height: 32, rx: 6, class: 'b-limb b-arm-r' }),
    h('rect', { x: 34, y: 72, width: 52, height: 48, rx: 14, class: 'b-shell' }),
    h('circle', { cx: 60, cy: 96, r: 8, class: 'b-core' }),
    h('rect', { x: 28, y: 22, width: 64, height: 46, rx: 16, class: 'b-shell' }),
    h('rect', { x: 36, y: 32, width: 48, height: 24, rx: 10, class: 'b-visor' }),
    h('circle', { cx: 50, cy: 44, r: 4.5, class: 'b-eye' }),
    h('circle', { cx: 70, cy: 44, r: 4.5, class: 'b-eye' }),
  ])
}
Bot.props = { pose: String }

/* ── hero rotation ── */
const slides = [
  { title: 'Knowledge at your', em: 'Fingertips.', sub: 'Grow. Learn. Succeed — your complete study companion for every grade and board.' },
  { title: 'Better Learning.', em: 'Better Results.', sub: 'Learn it, practise it, ask when stuck. Smart tools that turn effort into top marks.' },
  { title: 'All Boards. All Subjects.', em: 'Zero Confusion.', sub: 'Syllabus-aligned learning across Pakistan — anytime, anywhere, on any device.' },
]
const slide = ref(0)
let slideTimer = null

const heroStats = [
  { val: '10K+', label: 'Students' }, { val: '300K+', label: 'Questions' },
  { val: '13', label: 'Grades' }, { val: '24/7', label: 'AI Tutor' },
]
const subjects = ['Physics', 'Chemistry', 'Biology', 'Mathematics', 'English', 'Urdu', 'Pak Studies', 'Computer Science', 'General Knowledge', 'Islamiyat']
const selfTestRows = [
  { s: 'Urdu', c: 'linear-gradient(135deg,#6366f1,#8b5cf6)' },
  { s: 'Biology', c: 'linear-gradient(135deg,#10b981,#059669)' },
  { s: 'Physics', c: 'linear-gradient(135deg,#f59e0b,#d97706)' },
  { s: 'Pak Studies', c: 'linear-gradient(135deg,#ec4899,#be185d)' },
]

const trio = [
  { title: 'Learn', desc: 'E-books, smart key notes and video lectures crafted by subject experts — aligned to your curriculum.', icon: BookMarked, color: '#6366f1' },
  { title: 'Practice', desc: 'Unit & topic-wise MCQs, objective and subjective self-tests with instant, real-exam-style grading.', icon: ClipboardCheck, color: '#10b981' },
  { title: 'Ask', desc: 'Stuck on a concept? The AI Assistant and Ask-a-Question button answer in seconds, day or night.', icon: MessagesSquare, color: '#f59e0b' },
]

const features = [
  'SLOs Level Mapping', 'Difficulty Level Categorization', 'Exercise & Conceptual Questions',
  'E-Books & Key Notes', 'MCQs with Explanations', 'Video Lectures & Lecture Notes',
  'Past Papers & Model Papers', 'Unit & Topic-wise MCQs', 'Self-Test (Objective & Subjective)',
  'AI Assistant & Ask-a-Question', 'Scientific Calculator · Dictionary · Log Table', 'Quick-Response Interface',
]

const flow = ['Pick your grade', 'Open the syllabus', 'Learn with AI tutor', 'Take tests', 'Climb the leaderboard']
const leaders = ['Ayesha K.', 'Bilal M.', 'Sara A.']

const why = [
  { title: 'Interactive Textbooks & Notes', desc: 'Downloadable e-books and concise smart notes crafted by subject experts — always available, always aligned with your curriculum.', icon: Library },
  { title: 'Personalized Learning', desc: 'From Grade 1 to 12, choose your subjects and explore notes, quizzes, solved questions and practice tests designed for your level.', icon: UserCheck },
  { title: 'Past Papers for Practice', desc: 'Work through real past exam papers to master time management, identify key patterns, and prepare effectively for your exams.', icon: FileStack },
]

const stats = [
  { target: 300000, suffix: '+', label: 'Practice Questions' },
  { target: 3000, suffix: '+', label: 'E-Books & Notes' },
  { target: 5000, suffix: '+', label: 'Students Enrolled' },
  { target: 24, suffix: '/7', label: 'Hours Support' },
]
const statOut = reactive(stats.map(() => '0'))
let statsDone = false

const info = {
  email: 'info@estudycard.com',
  address: 'Office No. 1, Ground Floor, Waseem Kareem Plaza, Main Swabi-Jehangira Road, Punjpir, Swabi, KPK',
  phone1: '0938-280154', phone2: '0300-0746850',
}

/* ── contact form → n8n lead webhook ── */
const webhookUrl = import.meta.env.VITE_N8N_WEBHOOK_URL || 'http://localhost:5678/webhook/bap-lead'
const form = reactive({ first: '', last: '', email: '', phone: '', msg: '', status: 'idle' })
async function submitLead() {
  form.status = 'sending'
  try {
    const res = await fetch(webhookUrl, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: `${form.first} ${form.last}`.trim(), phone: form.phone, email: form.email,
        message: form.msg, source: 'home-landing', submittedAt: new Date().toISOString() }),
    })
    if (!res.ok) throw new Error(String(res.status))
    form.status = 'success'
  } catch (e) { console.error('[home-landing] lead failed:', e); form.status = 'error' }
}

/* ── interactions ── */
const pageEl = ref(null)
const statsEl = ref(null)
const scrolled = ref(false)
const mx = ref(-500); const my = ref(-500)
const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches
let revealObs = null
function onMouseMove(e) { mx.value = e.clientX; my.value = e.clientY }
function onScroll() { scrolled.value = window.scrollY > 30 }

function runStats() {
  if (statsDone) return
  statsDone = true
  if (reduce) { stats.forEach((s, i) => { statOut[i] = s.target.toLocaleString('en-US') }); return }
  const t0 = performance.now(); const dur = 1600
  const step = (t) => {
    const p = Math.min(1, (t - t0) / dur); const e = 1 - Math.pow(1 - p, 3)
    stats.forEach((s, i) => { statOut[i] = Math.round(s.target * e).toLocaleString('en-US') })
    if (p < 1) requestAnimationFrame(step)
  }
  requestAnimationFrame(step)
}

onMounted(() => {
  if (!reduce) slideTimer = setInterval(() => { slide.value = (slide.value + 1) % slides.length }, 4500)
  window.addEventListener('scroll', onScroll, { passive: true }); onScroll()

  revealObs = new IntersectionObserver((entries) => {
    entries.forEach((en) => {
      if (en.isIntersecting) {
        en.target.dataset.in = '1'
        if (en.target === statsEl.value) runStats()
        if (!en.target.classList.contains('hl-stats')) revealObs.unobserve(en.target)
      }
    })
  }, { threshold: 0.12 })
  pageEl.value.querySelectorAll('.reveal').forEach((el) => { reduce ? (el.dataset.in = '1') : revealObs.observe(el) })
  if (statsEl.value) revealObs.observe(statsEl.value)
})
onBeforeUnmount(() => { clearInterval(slideTimer); window.removeEventListener('scroll', onScroll); revealObs?.disconnect() })
</script>

<style scoped>
.hl {
  position: relative; min-height: 100vh; overflow-x: clip;
  background: var(--t-bg); color: var(--t-text1);
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
}
.hl-aurora { position: fixed; inset: 0; z-index: 0; overflow: hidden; pointer-events: none; }
.hl-aurora i { position: absolute; width: 520px; height: 520px; border-radius: 50%; filter: blur(100px); }
.hl-aurora .a1 { top: -140px; left: -120px; background: var(--t-aurora1); }
.hl-aurora .a2 { top: 36%; right: -180px; background: var(--t-aurora2); }
.hl-aurora .a3 { bottom: -160px; left: 30%; width: 440px; background: var(--t-aurora3); }
.hl-spot { position: fixed; inset: 0; z-index: 0; pointer-events: none; }

h1, h2, h3, .hl-logo-text, .hl-h1, .hl-h2 { font-family: 'Space Grotesk', system-ui, sans-serif; }

/* headings */
.hl-h1 { font-weight: 700; font-size: clamp(2.4rem, 5.2vw, 4.2rem); line-height: 1.02; letter-spacing: -0.03em; margin: 0 0 18px; }
.hl-h1 em { font-style: normal; background: linear-gradient(100deg, var(--t-accent), var(--t-accent2)); -webkit-background-clip: text; background-clip: text; color: transparent; }
.hl-h2 { font-weight: 700; font-size: clamp(1.8rem, 3.6vw, 2.9rem); line-height: 1.08; letter-spacing: -0.025em; margin: 0 0 14px; }
.hl-h2 em { font-style: normal; background: linear-gradient(100deg, var(--t-accent), var(--t-accent2)); -webkit-background-clip: text; background-clip: text; color: transparent; }
.hl-kicker { display: inline-flex; align-items: center; gap: 7px; font-size: 12.5px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: var(--t-accent); margin-bottom: 14px; }
.hl-kicker.center { justify-content: center; }
.hl-sec-sub { font-size: clamp(1rem, 1.4vw, 1.12rem); line-height: 1.7; color: var(--t-text2); max-width: 620px; margin: 0 auto; }
.hl-sec-sub.left { margin: 0; }

/* nav */
.hl-nav {
  position: fixed; top: 12px; left: 16px; right: 16px; z-index: 50; border-radius: 16px;
  display: flex; align-items: center; justify-content: space-between; padding: 9px 16px;
  border: 1px solid transparent; background: transparent; box-shadow: none;
  transition: padding 0.25s;
}
.hl-nav.scrolled { background: var(--t-glass-bg); border-color: var(--t-glass-border); box-shadow: var(--t-glass-shadow); -webkit-backdrop-filter: var(--t-glass-blur); backdrop-filter: var(--t-glass-blur); }
.hl-logo { display: inline-flex; align-items: center; gap: 9px; text-decoration: none; }
.hl-logo-mark { display: grid; place-items: center; width: 34px; height: 34px; border-radius: 10px; font-family: 'Space Grotesk'; font-weight: 700; font-size: 18px; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.hl-logo-text { font-weight: 700; font-size: 17px; color: var(--t-text1); }
.hl-logo-text em { font-style: normal; color: var(--t-accent); }
.hl-nav-links { display: flex; gap: 4px; }
.hl-nlink { padding: 8px 13px; border-radius: 9px; font-size: 13.5px; font-weight: 600; color: var(--t-text2); text-decoration: none; transition: color 0.2s, background 0.2s; }
.hl-nlink:hover { color: var(--t-text1); background: var(--t-hover); }
.hl-nav-right { display: flex; align-items: center; gap: 8px; }
.hl-icon-btn { display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); cursor: pointer; transition: color 0.2s, border-color 0.2s; }
.hl-icon-btn:hover { color: var(--t-accent); border-color: var(--t-accent); }
@media (max-width: 720px) { .hl-nav-links { display: none; } }

/* buttons */
.hl-btn { display: inline-flex; align-items: center; justify-content: center; gap: 8px; padding: 13px 24px; border-radius: 13px; font-size: 14.5px; font-weight: 700; text-decoration: none; cursor: pointer; border: 1px solid transparent; transition: transform 0.2s, box-shadow 0.2s, border-color 0.2s, background 0.2s; }
.hl-btn.sm { padding: 9px 16px; font-size: 13.5px; }
.hl-btn.full { width: 100%; }
.hl-btn-primary { color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); box-shadow: 0 10px 30px var(--t-acc-alpha-md); }
.hl-btn-primary:hover { transform: translateY(-2px); box-shadow: 0 14px 38px var(--t-acc-alpha-md); }
.hl-btn-primary:disabled { opacity: 0.7; cursor: wait; transform: none; }
.hl-btn-ghost { color: var(--t-text1); border-color: var(--t-border); background: var(--t-surface); }
.hl-btn-ghost:hover { border-color: var(--t-accent); color: var(--t-accent); }

/* hero */
.hl-hero { position: relative; z-index: 2; max-width: 1200px; margin: 0 auto; padding: 130px 24px 60px; display: grid; grid-template-columns: 1.05fr 0.95fr; gap: 50px; align-items: center; }
@media (max-width: 940px) { .hl-hero { grid-template-columns: 1fr; gap: 36px; text-align: center; } }
.hl-badge { display: inline-flex; align-items: center; gap: 7px; padding: 6px 13px; border-radius: 99px; font-size: 12.5px; font-weight: 600; color: var(--t-accent); background: var(--t-acc-alpha-sm); border: 1px solid var(--t-glass-border); margin-bottom: 20px; }
.hl-rot-block { min-height: 200px; }
.hl-sub { font-size: clamp(1rem, 1.5vw, 1.18rem); line-height: 1.7; color: var(--t-text2); max-width: 520px; margin: 0 0 28px; }
@media (max-width: 940px) { .hl-sub { margin-inline: auto; } .hl-rot-block { min-height: 220px; } }
.hl-hero-btns { display: flex; gap: 13px; flex-wrap: wrap; margin-bottom: 34px; }
@media (max-width: 940px) { .hl-hero-btns { justify-content: center; } }
.hl-hero-stats { display: flex; gap: 30px; flex-wrap: wrap; }
@media (max-width: 940px) { .hl-hero-stats { justify-content: center; } }
.hl-hero-stats div { display: flex; flex-direction: column; }
.hl-hero-stats b { font-family: 'Space Grotesk'; font-weight: 700; font-size: 22px; background: linear-gradient(120deg, var(--t-accent), var(--t-accent2)); -webkit-background-clip: text; background-clip: text; color: transparent; }
.hl-hero-stats span { font-size: 11.5px; font-weight: 600; letter-spacing: 0.06em; text-transform: uppercase; color: var(--t-text3); }
.hl-hero-device { position: relative; display: flex; justify-content: center; }
.hl-hero-bot { position: absolute; right: 4%; bottom: -10px; width: 84px; }

.hl-rot-enter-active, .hl-rot-leave-active { transition: opacity 0.4s ease, transform 0.4s ease; }
.hl-rot-enter-from { opacity: 0; transform: translateY(14px); }
.hl-rot-leave-to { opacity: 0; transform: translateY(-12px); }

/* ticker */
.hl-ticker { position: relative; z-index: 2; overflow: hidden; padding: 14px 0; border-top: 1px solid var(--t-border); border-bottom: 1px solid var(--t-border); }
.hl-ticker-track { display: flex; width: max-content; animation: hl-marq 28s linear infinite; }
.hl-ticker:hover .hl-ticker-track { animation-play-state: paused; }
.hl-tick { display: inline-flex; align-items: center; gap: 16px; padding-right: 16px; font-family: 'Space Grotesk'; font-weight: 600; font-size: 13.5px; letter-spacing: 0.05em; text-transform: uppercase; color: var(--t-text3); white-space: nowrap; }
.hl-tick i { width: 5px; height: 5px; border-radius: 99px; background: var(--t-accent); opacity: 0.6; }
@keyframes hl-marq { to { transform: translateX(-50%); } }

/* sections */
.hl-sec { position: relative; z-index: 2; max-width: 1180px; margin: 0 auto; padding: 80px 24px; }
.hl-sec-head { text-align: center; margin-bottom: 48px; }

/* about */
.hl-about-card { padding: 40px 44px; border-radius: 24px; text-align: center; }
.hl-about-text { font-size: clamp(1rem, 1.5vw, 1.18rem); line-height: 1.8; color: var(--t-text2); margin: 0; }
.hl-about-text strong { color: var(--t-text1); }

/* trio */
.hl-trio { display: grid; grid-template-columns: repeat(3, 1fr); gap: 18px; }
@media (max-width: 820px) { .hl-trio { grid-template-columns: 1fr; } }
.hl-trio-card { padding: 28px 26px; border-radius: 20px; }
.hl-trio-ic { display: grid; place-items: center; width: 50px; height: 50px; border-radius: 14px; margin-bottom: 16px; background: var(--t-acc-alpha-sm); border: 1px solid var(--t-glass-border); }
.hl-trio-card h3 { font-size: 19px; font-weight: 700; margin: 0 0 9px; color: var(--t-text1); }
.hl-trio-card p { font-size: 14px; line-height: 1.65; color: var(--t-text2); margin: 0; }

/* features */
.hl-feat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }
@media (max-width: 980px) { .hl-feat-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 560px) { .hl-feat-grid { grid-template-columns: 1fr; } }
.hl-feat { display: flex; align-items: center; gap: 11px; padding: 15px 17px; border-radius: 14px; background: var(--t-surface); border: 1px solid var(--t-border); font-size: 14px; font-weight: 600; color: var(--t-text1); transition: border-color 0.2s, transform 0.2s; }
.hl-feat:hover { border-color: var(--t-accent); transform: translateY(-2px); }
.hl-feat-ck { flex-shrink: 0; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); border-radius: 99px; padding: 3px; width: 20px; height: 20px; }

/* split sections */
.hl-split { display: grid; grid-template-columns: 1fr 1.05fr; gap: 54px; align-items: center; }
.hl-split.reverse .hl-split-copy { order: 2; }
@media (max-width: 940px) { .hl-split, .hl-split.reverse .hl-split-copy { grid-template-columns: 1fr; order: 0; } }
.hl-ticks { list-style: none; padding: 0; margin: 20px 0 0; display: flex; flex-direction: column; gap: 11px; }
.hl-ticks li { display: flex; align-items: center; gap: 9px; font-size: 14.5px; font-weight: 600; color: var(--t-text1); }
.hl-ticks svg { color: var(--t-accent); }
.hl-flow { display: flex; flex-wrap: wrap; gap: 9px; margin-top: 22px; }
.hl-flow span { display: inline-flex; align-items: center; gap: 7px; padding: 7px 13px 7px 7px; border-radius: 99px; font-size: 13px; font-weight: 600; color: var(--t-text1); background: var(--t-surface); border: 1px solid var(--t-border); }
.hl-flow b { display: grid; place-items: center; width: 20px; height: 20px; border-radius: 99px; font-size: 11px; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.hl-split-devices { position: relative; display: flex; align-items: center; justify-content: center; gap: 18px; min-height: 360px; }
.hl-split-phone { position: relative; z-index: 2; }
@media (max-width: 560px) { .hl-split-devices { flex-direction: column; } }

/* why */
.hl-why { display: grid; grid-template-columns: repeat(3, 1fr); gap: 18px; }
@media (max-width: 820px) { .hl-why { grid-template-columns: 1fr; } }
.hl-why-card { padding: 30px 28px; border-radius: 20px; text-align: center; }
.hl-why-ic { display: inline-grid; place-items: center; width: 56px; height: 56px; border-radius: 16px; margin-bottom: 18px; color: var(--t-accent); background: var(--t-acc-alpha-sm); border: 1px solid var(--t-glass-border); }
.hl-why-card h3 { font-size: 18px; font-weight: 700; margin: 0 0 10px; color: var(--t-text1); }
.hl-why-card p { font-size: 14px; line-height: 1.65; color: var(--t-text2); margin: 0; }

/* stats */
.hl-stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 18px; }
@media (max-width: 720px) { .hl-stats { grid-template-columns: repeat(2, 1fr); } }
.hl-stat { padding: 30px 18px; border-radius: 20px; text-align: center; }
.hl-stat b { display: block; font-family: 'Space Grotesk'; font-weight: 700; font-size: clamp(1.7rem, 3.4vw, 2.5rem); background: linear-gradient(120deg, var(--t-accent), var(--t-accent2)); -webkit-background-clip: text; background-clip: text; color: transparent; font-variant-numeric: tabular-nums; }
.hl-stat span { display: block; margin-top: 6px; font-size: 12.5px; font-weight: 600; letter-spacing: 0.04em; text-transform: uppercase; color: var(--t-text3); }

/* contact */
.hl-contact-grid { display: grid; grid-template-columns: 1.2fr 1fr; gap: 20px; }
@media (max-width: 820px) { .hl-contact-grid { grid-template-columns: 1fr; } }
.hl-form { padding: 30px; border-radius: 22px; display: flex; flex-direction: column; gap: 13px; }
.hl-form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 13px; }
.hl-form input, .hl-form textarea { width: 100%; padding: 13px 15px; border-radius: 12px; font: inherit; font-size: 14.5px; color: var(--t-text1); background: var(--t-input-bg); border: 1px solid var(--t-input-border); transition: border-color 0.2s, box-shadow 0.2s; resize: vertical; }
.hl-form input::placeholder, .hl-form textarea::placeholder { color: var(--t-text3); }
.hl-form input:focus-visible, .hl-form textarea:focus-visible { outline: none; border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-input-focus); }
.hl-form-err { display: flex; align-items: center; gap: 6px; font-size: 13px; color: var(--t-danger); margin: 0; }
.hl-form-done { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 6px; padding: 20px 0; }
.hl-form-done b { font-family: 'Space Grotesk'; font-size: 20px; color: var(--t-text1); }
.hl-form-done p { font-size: 14px; color: var(--t-text2); margin: 0; }
.hl-done-badge { display: grid; place-items: center; width: 60px; height: 60px; border-radius: 99px; margin-bottom: 10px; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.hl-spin { width: 15px; height: 15px; border-radius: 99px; border: 2px solid rgba(255,255,255,0.4); border-top-color: #fff; animation: hl-spin 0.7s linear infinite; }
@keyframes hl-spin { to { transform: rotate(360deg); } }
.hl-contact-info { padding: 26px; border-radius: 22px; display: flex; flex-direction: column; gap: 14px; }
.hl-ci-row { display: flex; align-items: flex-start; gap: 12px; font-size: 13.5px; color: var(--t-text2); text-decoration: none; transition: color 0.2s; }
.hl-ci-row svg { flex-shrink: 0; color: var(--t-accent); margin-top: 2px; }
a.hl-ci-row:hover { color: var(--t-accent); }

/* footer */
.hl-footer { position: relative; z-index: 2; border-top: 1px solid var(--t-border); padding: 50px 24px 30px; background: var(--t-bg2); }
.hl-foot-grid { max-width: 1180px; margin: 0 auto; display: grid; grid-template-columns: 1.6fr 1fr 1fr 1fr; gap: 30px; }
@media (max-width: 820px) { .hl-foot-grid { grid-template-columns: 1fr 1fr; } }
.hl-foot-brand p { margin: 14px 0 0; font-size: 13.5px; line-height: 1.7; color: var(--t-text3); max-width: 320px; }
.hl-foot-col { display: flex; flex-direction: column; gap: 9px; }
.hl-foot-col b { font-family: 'Space Grotesk'; font-size: 14px; color: var(--t-text1); margin-bottom: 4px; }
.hl-foot-col a { font-size: 13px; font-weight: 500; color: var(--t-text3); text-decoration: none; transition: color 0.2s; }
.hl-foot-col a:hover { color: var(--t-accent); }
.hl-foot-bottom { max-width: 1180px; margin: 36px auto 0; padding-top: 20px; border-top: 1px solid var(--t-border); font-size: 12.5px; color: var(--t-text3); text-align: center; }

/* ── device frames ── */
:deep(.dv-phone) { position: relative; width: 250px; flex-shrink: 0; padding: 9px; border-radius: 40px; background: linear-gradient(160deg, var(--t-accent2), var(--t-bg2) 55%, var(--t-accent)); box-shadow: var(--t-glass-shadow), 0 0 0 1px var(--t-glass-border); }
:deep(.dv-notch) { position: absolute; top: 16px; left: 50%; transform: translateX(-50%); z-index: 3; width: 64px; height: 18px; border-radius: 99px; background: #05060c; }
:deep(.dv-phone-screen) { position: relative; height: 480px; border-radius: 32px; overflow: hidden; background: var(--t-surface); border: 1px solid var(--t-border); }
:deep(.dv-browser) { width: min(440px, 100%); border-radius: 16px; overflow: hidden; }
:deep(.dv-bar) { display: flex; align-items: center; gap: 6px; padding: 11px 14px; border-bottom: 1px solid var(--t-border); }
:deep(.dv-bar i) { width: 10px; height: 10px; border-radius: 99px; background: var(--t-border); }
:deep(.dv-bar i:nth-child(1)) { background: #f87171; } :deep(.dv-bar i:nth-child(2)) { background: #fbbf24; } :deep(.dv-bar i:nth-child(3)) { background: #34d399; }
:deep(.dv-url) { margin-left: 10px; padding: 3px 12px; border-radius: 7px; font-size: 11px; color: var(--t-text3); background: var(--t-hover); flex: 1; }
:deep(.dv-browser-screen) { height: 300px; overflow: hidden; background: var(--t-bg); }

/* ── mini app UIs ── */
:deep(.ui-app) { height: 100%; display: flex; flex-direction: column; padding-top: 40px; }
:deep(.ui-appbar) { display: flex; align-items: center; gap: 6px; padding: 10px 14px; font-size: 12px; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
:deep(.ui-list) { padding: 12px; display: flex; flex-direction: column; gap: 9px; }
:deep(.ui-row) { display: grid; grid-template-columns: auto 1fr auto; align-items: center; gap: 10px; padding: 10px; border-radius: 12px; background: var(--t-hover); border: 1px solid var(--t-border); }
:deep(.ui-row.open) { background: var(--t-acc-alpha-sm); border-color: var(--t-accent); grid-template-columns: auto 1fr auto; }
:deep(.ui-row-av) { width: 30px; height: 30px; border-radius: 9px; display: grid; place-items: center; color: #fff; font-weight: 700; font-size: 12px; }
:deep(.ui-row-info) { display: flex; flex-direction: column; }
:deep(.ui-row-info b) { font-size: 12.5px; color: var(--t-text1); }
:deep(.ui-row-info i) { font-style: normal; font-size: 9.5px; color: var(--t-text3); }
:deep(.ui-row-chev) { color: var(--t-text3); }
:deep(.ui-row-actions) { grid-column: 1 / -1; display: flex; gap: 8px; margin-top: 4px; }
:deep(.ui-pill) { flex: 1; display: inline-flex; align-items: center; justify-content: center; gap: 5px; padding: 8px; border-radius: 9px; font-size: 11.5px; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
:deep(.ui-pill:nth-child(2)) { color: var(--t-accent); background: var(--t-surface); border: 1px solid var(--t-accent); }

:deep(.ui-result) { flex: 1; display: flex; flex-direction: column; align-items: center; gap: 10px; padding: 24px 18px; }
:deep(.ui-ring) { position: relative; width: 96px; height: 96px; }
:deep(.ui-ring svg) { width: 96px; height: 96px; transform: rotate(-90deg); }
:deep(.ui-ring .rbg) { fill: none; stroke: var(--t-hover); stroke-width: 8; }
:deep(.ui-ring .rfg) { fill: none; stroke: var(--t-accent); stroke-width: 8; stroke-linecap: round; stroke-dasharray: 201; stroke-dashoffset: 26; }
:deep(.ui-ring b) { position: absolute; inset: 0; display: grid; place-items: center; font-family: 'Space Grotesk'; font-weight: 700; font-size: 22px; color: var(--t-accent); }
:deep(.ui-result-lbl) { font-size: 13px; font-weight: 700; color: var(--t-text1); }
:deep(.ui-bar) { width: 80%; height: 9px; border-radius: 99px; background: var(--t-acc-alpha-md); }

:deep(.ui-chat) { flex: 1; display: flex; flex-direction: column; gap: 8px; padding: 12px; overflow: hidden; }
:deep(.ui-msg) { max-width: 85%; padding: 9px 12px; border-radius: 13px; font-size: 12px; line-height: 1.5; }
:deep(.ui-msg.user) { align-self: flex-end; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); border-bottom-right-radius: 4px; }
:deep(.ui-msg.ai) { align-self: flex-start; color: var(--t-text1); background: var(--t-hover); border: 1px solid var(--t-border); border-bottom-left-radius: 4px; }
:deep(.ui-chatbar) { display: flex; align-items: center; justify-content: space-between; margin: 0 12px 12px; padding: 10px 13px; border-radius: 12px; font-size: 11.5px; color: var(--t-text3); background: var(--t-hover); border: 1px solid var(--t-border); }
:deep(.ui-chatbar svg) { color: var(--t-accent); }

:deep(.ui-dash) { display: grid; grid-template-columns: 78px 1fr; height: 100%; }
:deep(.ui-dash-side) { padding: 12px 10px; background: var(--t-bg2); border-right: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 8px; }
:deep(.ui-dash-logo) { font-family: 'Space Grotesk'; font-weight: 700; font-size: 13px; color: var(--t-accent); margin-bottom: 6px; }
:deep(.ui-dash-side i) { height: 8px; border-radius: 99px; background: var(--t-hover); }
:deep(.ui-dash-side i.on) { background: linear-gradient(90deg, var(--t-accent), var(--t-accent2)); }
:deep(.ui-dash-main) { padding: 14px; }
:deep(.ui-dash-head) { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; font-size: 13px; color: var(--t-text1); }
:deep(.ui-dash-head svg) { color: var(--t-gold); }
:deep(.ui-score) { font-family: 'Space Grotesk'; font-weight: 700; color: var(--t-accent); }
:deep(.ui-q) { display: flex; align-items: center; gap: 6px; margin-bottom: 12px; }
:deep(.ui-q-bar) { height: 8px; border-radius: 99px; background: var(--t-hover); }
:deep(.ui-opt) { width: 20px; height: 20px; border-radius: 6px; display: grid; place-items: center; font-size: 10px; font-weight: 700; color: var(--t-text3); border: 1px solid var(--t-border); }
:deep(.ui-opt.pick) { color: #fff; background: var(--t-accent); border-color: var(--t-accent); }
:deep(.ui-lb) { display: grid; grid-template-columns: auto 1fr auto auto; align-items: center; gap: 8px; margin-bottom: 11px; }
:deep(.ui-lb-rank) { width: 22px; height: 22px; border-radius: 7px; display: grid; place-items: center; font-size: 11px; font-weight: 700; color: #fff; }
:deep(.ui-lb-rank.r1) { background: #f59e0b; } :deep(.ui-lb-rank.r2) { background: #94a3b8; } :deep(.ui-lb-rank.r3) { background: #b45309; }
:deep(.ui-lb-name) { font-size: 12px; color: var(--t-text1); }
:deep(.ui-lb-bar) { width: 70px; height: 7px; border-radius: 99px; background: linear-gradient(90deg, var(--t-accent), var(--t-accent2)); }
:deep(.ui-lb b) { font-size: 11px; color: var(--t-text2); font-family: 'Space Grotesk'; }

/* robot */
:deep(.hl-bot) { overflow: visible; }
:deep(.hl-bot .b-shell) { fill: var(--t-surface); stroke: var(--t-accent); stroke-width: 2; }
:deep(.hl-bot .b-visor) { fill: var(--t-bg); }
:deep(.hl-bot .b-eye) { fill: var(--t-accent); }
:deep(.hl-bot .b-core) { fill: var(--t-gold); }
:deep(.hl-bot .b-limb) { fill: var(--t-surface); stroke: var(--t-accent); stroke-width: 2; }
:deep(.hl-bot.bot-wave .b-arm-r) { transform-box: fill-box; transform-origin: 50% 12%; animation: hl-wave 1.7s ease-in-out infinite; }
@keyframes hl-wave { 0%, 100% { transform: rotate(8deg); } 50% { transform: rotate(-100deg); } }

/* reveal */
.reveal { opacity: 0; transform: translateY(24px) scale(0.99); filter: blur(4px); transition: opacity 0.6s ease, transform 0.6s ease, filter 0.6s ease; }
.reveal[data-in] { opacity: 1; transform: none; filter: none; }

@media (prefers-reduced-motion: reduce) {
  .hl * , .hl *::before, .hl *::after { animation: none !important; transition: none !important; }
  .reveal { opacity: 1; transform: none; filter: none; }
}
</style>
