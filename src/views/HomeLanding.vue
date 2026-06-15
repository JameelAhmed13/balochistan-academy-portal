<template>
  <div ref="pageEl" class="hl" @mousemove="onMouseMove">
    <!-- atmosphere -->
    <div class="hl-aurora" aria-hidden="true"><i class="a1" /><i class="a2" /><i class="a3" /></div>
    <div class="hl-spot" :style="{ background: `radial-gradient(460px circle at ${mx}px ${my}px, var(--t-acc-alpha-sm), transparent 65%)` }" />

    <!-- ── NAV ── -->
    <nav class="hl-nav glass" :class="{ scrolled }">
      <a href="#top" class="hl-logo">
        <span class="hl-logo-mark">B</span>
        <span class="hl-logo-text">Balochistan Academy<em>&nbsp;Portal</em></span>
      </a>
      <div class="hl-nav-links">
        <a href="#about" class="hl-nlink">About</a>
        <a href="#how" class="hl-nlink">How it works</a>
        <a href="#features" class="hl-nlink">Features</a>
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
          <a href="#how" class="hl-btn hl-btn-ghost"><ArrowDown :size="16" /> See how it works</a>
        </div>
        <div class="hl-hero-stats reveal">
          <div v-for="s in heroStats" :key="s.label"><b>{{ s.val }}</b><span>{{ s.label }}</span></div>
        </div>
      </div>

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

    <!-- ── ABOUT (redesigned, two-column) ── -->
    <section id="about" class="hl-sec hl-about">
      <div class="hl-about-grid">
        <div class="hl-about-copy reveal">
          <span class="hl-kicker"><BookOpen :size="14" /> About Balochistan Academy Portal</span>
          <h2 class="hl-h2">A complete learning companion <em>for every Pakistani student.</em></h2>
          <p class="hl-about-text">
            Balochistan Academy Portal is a modern learning companion built to simplify education for students across
            Pakistan. Whether preparing for <strong>board exams, entry tests</strong>, or self-study, it offers smart
            tools, interactive resources, and progress tracking to keep learners on the right path.
          </p>
          <p class="hl-about-text">
            With expert-designed notes, past papers, quizzes, and mock tests, students gain the confidence to excel —
            and parents stay connected through dedicated monitoring. <strong>Our goal:</strong> a stress-free,
            engaging, result-driven learning journey for every student.
          </p>
          <div class="hl-about-cta">
            <RouterLink to="/login" class="hl-btn hl-btn-primary">Create free account <ArrowRight :size="16" /></RouterLink>
          </div>
        </div>
        <div class="hl-about-side reveal">
          <div v-for="h in aboutHighlights" :key="h.label" class="hl-about-hl glass">
            <span class="hl-about-hl-ic"><component :is="h.icon" :size="20" /></span>
            <div><b>{{ h.val }}</b><span>{{ h.label }}</span></div>
          </div>
          <Bot pose="bot-wave" class="hl-about-bot" />
        </div>
      </div>
    </section>

    <!-- ── HOW IT WORKS — 4 capability cards side by side ── -->
    <section id="how" class="hl-sec">
      <div class="hl-sec-head reveal">
        <span class="hl-kicker center"><Sparkles :size="14" /> How it works</span>
        <h2 class="hl-h2">From first lesson to <em>the merit list.</em></h2>
        <p class="hl-sec-sub">Four AI-powered steps that take every student from learning to topping the board.</p>
      </div>
      <div class="hl-caps">
        <article v-for="(c, i) in capabilities" :key="c.title" class="hl-cap glass reveal" :style="{ transitionDelay: i * 70 + 'ms' }">
          <!-- mini scene -->
          <div class="cap-scene" aria-hidden="true">
            <div class="cap-scene-head"><span>{{ c.scene }}</span><span class="cap-online"><i />ONLINE</span></div>
            <!-- per-type illustration -->
            <div v-if="c.type === 'tutor'" class="cap-tutor">
              <div class="cap-board"><i /><i class="w70" /><b>a = F / m = 5 m/s²</b></div>
            </div>
            <div v-else-if="c.type === 'paper'" class="cap-paper">
              <div class="cap-pcard"><span class="cap-ptag">MODEL PAPER · 9th</span><i /><i class="w80" /><i class="w60" />
                <div class="cap-mcq"><span v-for="o in ['A','B','C','D']" :key="o" :class="{ pick: o === 'B' }">{{ o }}</span></div>
              </div>
            </div>
            <div v-else-if="c.type === 'examiner'" class="cap-exam">
              <div class="cap-rows">
                <div><i class="w80" /><span class="cap-ck ok">✓</span></div>
                <div><i class="w65" /><span class="cap-ck ok">✓</span></div>
                <div><i class="w70" /><span class="cap-ck no">✕</span></div>
              </div>
              <div class="cap-score">87<small>%</small></div>
            </div>
            <div v-else class="cap-arena">
              <div class="cap-podium"><span class="p2" /><span class="p1">🏆</span><span class="p3" /></div>
              <span class="cap-rank">RANK #01</span>
            </div>
            <Bot :pose="c.pose" class="cap-bot" />
          </div>
          <div class="cap-meta">
            <span class="cap-ic"><component :is="c.icon" :size="20" /></span>
            <span class="cap-num">{{ String(i + 1).padStart(2, '0') }}</span>
          </div>
          <h3>{{ c.title }}</h3>
          <p>{{ c.desc }}</p>
          <ul class="cap-chips"><li v-for="ch in c.chips" :key="ch"><Check :size="11" /> {{ ch }}</li></ul>
        </article>
      </div>
    </section>

    <!-- ── ONE PLATFORM — 8 feature cards ── -->
    <section id="features" class="hl-sec">
      <div class="hl-sec-head reveal">
        <span class="hl-kicker center"><LayoutGrid :size="14" /> Everything you need</span>
        <h2 class="hl-h2">One platform. <em>Infinite possibilities.</em></h2>
        <p class="hl-sec-sub">From AI tutoring to competitive tests, coins to reports — built for the Balochistan Board.</p>
      </div>
      <div class="hl-feat-grid">
        <div v-for="(f, i) in features" :key="f.title" class="hl-feat glass reveal" :style="{ transitionDelay: (i % 4) * 60 + 'ms' }">
          <span class="hl-feat-ic"><component :is="f.icon" :size="20" /></span>
          <h3>{{ f.title }}</h3>
          <p>{{ f.desc }}</p>
        </div>
      </div>
    </section>

    <!-- ── WATCH AI STUDY YOUR STUDENTS ── -->
    <section class="hl-sec hl-ai">
      <div class="hl-sec-head reveal">
        <span class="hl-kicker center"><BrainCircuit :size="14" /> AI Intelligence</span>
        <h2 class="hl-h2">Watch AI study <em>your students.</em></h2>
        <p class="hl-sec-sub">Every answer feeds the engine — it tracks, adapts and personalizes each session in real time.</p>
      </div>
      <div class="hl-ai-stage reveal">
        <div v-for="(st, i) in students" :key="st.name" class="hl-scard glass" :class="'pos' + (i + 1)">
          <div class="hl-sc-top">
            <span class="hl-sc-av" :style="{ background: st.av }">{{ st.name[0] }}</span>
            <span class="hl-sc-id"><b>{{ st.name }}</b><i>{{ st.subject }}</i></span>
            <span class="hl-sc-score" :style="{ color: st.color }">{{ st.score }}</span>
          </div>
          <div v-for="bar in st.bars" :key="bar.label" class="hl-bar">
            <span>{{ bar.label }}</span>
            <i class="hl-bar-track"><i class="hl-bar-fill" :style="{ width: aiOn ? bar.pct + '%' : '0%', background: bar.color }" /></i>
            <b>{{ bar.pct }}%</b>
          </div>
        </div>
        <div class="hl-ai-core">
          <i class="hl-ai-ring r1" /><i class="hl-ai-ring r2" />
          <div class="hl-ai-brain"><BrainCircuit :size="30" /><span>AI ENGINE</span></div>
        </div>
        <span class="hl-ai-badge b1 glass"><Target :size="12" /> Weakness detected: Kinematics</span>
        <span class="hl-ai-badge b2 glass"><Lightbulb :size="12" /> Recommended: 5 more MCQs</span>
        <span class="hl-ai-badge b3 glass"><TrendingUp :size="12" /> +18% this week</span>
      </div>
    </section>

    <!-- ── PRACTICE TESTS — desktop + phone ── -->
    <section class="hl-sec hl-split">
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
            <div class="ui-dash-side"><span class="ui-dash-logo">Portal</span><i v-for="n in 5" :key="n" :class="{ on: n === 2 }" /></div>
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

    <!-- ── WHY CHOOSE ── -->
    <section class="hl-sec">
      <div class="hl-sec-head reveal">
        <span class="hl-kicker center"><Star :size="14" /> Why Choose Balochistan Academy Portal?</span>
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
          <a :href="`https://${info.web}`" target="_blank" rel="noopener" class="hl-ci-row"><Globe :size="16" /><span>{{ info.web }}</span></a>
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
          <a href="#top" class="hl-logo"><span class="hl-logo-mark">B</span><span class="hl-logo-text">Balochistan Academy<em>&nbsp;Portal</em></span></a>
          <p>High-quality educational resources — digital textbooks, notes, and smart preparation tools. Learning made easy, engaging, and accessible for all students.</p>
        </div>
        <nav class="hl-foot-col" aria-label="Platform">
          <b>Platform</b>
          <a href="#about">About</a><a href="#how">How it works</a><a href="#features">Features</a>
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
      <div class="hl-foot-bottom">© 2026 Balochistan Academy Portal · All Rights Reserved · New Century Educational System</div>
    </footer>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onBeforeUnmount, h } from 'vue'
import {
  ArrowRight, ArrowDown, Sparkles, Check, X, Send, Star, BookOpen, LayoutGrid,
  ClipboardCheck, ClipboardList, PenLine, GraduationCap, Trophy, Mail, Globe,
  MapPin, Phone as Phone2, ChevronDown, ChevronLeft, Sun, Moon,
  BrainCircuit, Target, Lightbulb, TrendingUp, Bot as Bot2, FileText, Video,
  Coins, Library, Users, BookMarked,
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
    h('div', { class: 'dv-bar' }, [h('i'), h('i'), h('i'), h('span', { class: 'dv-url' }, 'portal.estudycard.com')]),
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

const aboutHighlights = [
  { icon: BookMarked, val: '10 Subjects', label: 'ECD to Grade 12' },
  { icon: ClipboardCheck, val: 'Self-Tests', label: 'Objective + Subjective' },
  { icon: BrainCircuit, val: 'AI Tutors', label: 'Available 24/7' },
  { icon: Trophy, val: 'Competitions', label: 'Province leaderboards' },
]

/* ── 4 capability cards ── */
const capabilities = [
  { type: 'tutor', scene: 'AI TUTOR', pose: 'bot-wave', icon: Bot2, title: 'Learn live with your AI tutor',
    desc: 'Ask anything, anytime — your AI teacher explains every concept in Urdu or English until it clicks, with chat and video.',
    chips: ['24/7 doubt solving', 'Chat + video', 'Urdu + English'] },
  { type: 'paper', scene: 'PAPER ENGINE', pose: '', icon: Sparkles, title: 'AI writes your paper',
    desc: 'Gemini drafts fresh objective and subjective papers in the exact Balochistan Board pattern — never the same test twice.',
    chips: ['Board pattern', 'Unlimited papers', 'Difficulty dial'] },
  { type: 'examiner', scene: 'AI EXAMINER', pose: '', icon: ClipboardCheck, title: 'Marked by AI in seconds',
    desc: 'Submit your answers and watch the AI examiner stamp every line — marks, corrections and a model answer, instantly.',
    chips: ['Instant marks', 'Per-line feedback', 'Weak-topic alerts'] },
  { type: 'arena', scene: 'ARENA · LIVE', pose: 'bot-wave', icon: Trophy, title: 'Compete across the province',
    desc: 'Daily streaks, weekly quizzes and monthly competitions — earn coins and race the whole province up the merit list.',
    chips: ['Daily streaks', 'Coins & rewards', 'Province ranks'] },
]

/* ── 8 platform features ── */
const features = [
  { icon: Bot2, title: '8 AI Tutors', desc: 'Einstein, Ghalib, Curie & more — each subject gets a legendary AI mentor.' },
  { icon: FileText, title: 'Daily Tests', desc: 'Auto-generated MCQ and subjective tests with instant AI grading.' },
  { icon: Trophy, title: 'Competition', desc: 'Monthly institute-wide exams with leaderboards and mega challenges.' },
  { icon: Coins, title: 'Coins & Rewards', desc: 'Earn real coins for test performance. Withdraw via Easypaisa.' },
  { icon: Video, title: 'Video Lessons', desc: 'AI-voiced video lessons with voice Q&A powered by Google TTS.' },
  { icon: TrendingUp, title: 'Reports', desc: 'Detailed charts for students, parents and teachers in one dashboard.' },
  { icon: PenLine, title: 'Subjective AI', desc: 'Write long answers with voice input. Gemini grades & gives feedback.' },
  { icon: Library, title: 'Digital Library', desc: '10 subjects, key notes, past papers, simulations and virtual labs.' },
]

/* ── AI intelligence students ── */
const students = [
  { name: 'Ahmed K.', subject: 'Physics', score: '92%', color: '#22c55e', av: 'linear-gradient(135deg,#6366f1,#8b5cf6)',
    bars: [{ label: 'Accuracy', pct: 92, color: '#6366f1' }, { label: 'Speed', pct: 78, color: '#06b6d4' }, { label: 'Retention', pct: 85, color: '#f59e0b' }] },
  { name: 'Bilal M.', subject: 'Math', score: '95%', color: '#f59e0b', av: 'linear-gradient(135deg,#0ea5e9,#6366f1)',
    bars: [{ label: 'Accuracy', pct: 95, color: '#22c55e' }, { label: 'Speed', pct: 88, color: '#6366f1' }, { label: 'Retention', pct: 90, color: '#f59e0b' }] },
  { name: 'Fatima R.', subject: 'Chemistry', score: '87%', color: '#06b6d4', av: 'linear-gradient(135deg,#ec4899,#8b5cf6)',
    bars: [{ label: 'Accuracy', pct: 87, color: '#a78bfa' }, { label: 'Speed', pct: 91, color: '#06b6d4' }, { label: 'Retention', pct: 82, color: '#f59e0b' }] },
  { name: 'Sara A.', subject: 'Biology', score: '89%', color: '#f87171', av: 'linear-gradient(135deg,#10b981,#059669)',
    bars: [{ label: 'Accuracy', pct: 89, color: '#f87171' }, { label: 'Speed', pct: 74, color: '#06b6d4' }, { label: 'Retention', pct: 93, color: '#a78bfa' }] },
]

const why = [
  { title: 'Interactive Textbooks & Notes', desc: 'Downloadable e-books and concise smart notes crafted by subject experts — always available, always aligned with your curriculum.', icon: Library },
  { title: 'Personalized Learning', desc: 'From Grade 1 to 12, choose your subjects and explore notes, quizzes, solved questions and practice tests designed for your level.', icon: Users },
  { title: 'Past Papers for Practice', desc: 'Work through real past exam papers to master time management, identify key patterns, and prepare effectively for your exams.', icon: FileText },
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
  email: 'info@estudycard.com', web: 'www.estudycard.com',
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
const aiOn = ref(false)
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
        if (en.target.classList.contains('hl-ai-stage')) aiOn.value = true
        if (!en.target.classList.contains('hl-stats')) revealObs.unobserve(en.target)
      }
    })
  }, { threshold: 0.12 })
  pageEl.value.querySelectorAll('.reveal').forEach((el) => { reduce ? (el.dataset.in = '1') : revealObs.observe(el) })
  if (statsEl.value) revealObs.observe(statsEl.value)
  if (reduce) aiOn.value = true
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

.hl-h1 { font-weight: 700; font-size: clamp(2.4rem, 5.2vw, 4.2rem); line-height: 1.02; letter-spacing: -0.03em; margin: 0 0 18px; }
.hl-h1 em { font-style: normal; background: linear-gradient(100deg, var(--t-accent), var(--t-accent2) 52%, var(--t-yellow)); -webkit-background-clip: text; background-clip: text; color: transparent; }
.hl-h2 { font-weight: 700; font-size: clamp(1.8rem, 3.6vw, 2.9rem); line-height: 1.08; letter-spacing: -0.025em; margin: 0 0 14px; }
.hl-h2 em { font-style: normal; background: linear-gradient(100deg, var(--t-accent), var(--t-accent2) 52%, var(--t-yellow)); -webkit-background-clip: text; background-clip: text; color: transparent; }
.hl-kicker { display: inline-flex; align-items: center; gap: 7px; font-size: 12.5px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: var(--t-accent); margin-bottom: 14px; }
.hl-kicker.center { justify-content: center; }
.hl-sec-sub { font-size: clamp(1rem, 1.4vw, 1.12rem); line-height: 1.7; color: var(--t-text2); max-width: 620px; margin: 0 auto; }
.hl-sec-sub.left { margin: 0; }

/* nav */
.hl-nav { position: fixed; top: 12px; left: 16px; right: 16px; z-index: 50; border-radius: 16px; display: flex; align-items: center; justify-content: space-between; padding: 9px 16px; border: 1px solid transparent; background: transparent; box-shadow: none; transition: padding 0.25s; }
.hl-nav.scrolled { background: var(--t-glass-bg); border-color: var(--t-glass-border); box-shadow: var(--t-glass-shadow); -webkit-backdrop-filter: var(--t-glass-blur); backdrop-filter: var(--t-glass-blur); }
.hl-logo { display: inline-flex; align-items: center; gap: 9px; text-decoration: none; }
.hl-logo-mark { display: grid; place-items: center; width: 34px; height: 34px; border-radius: 10px; font-family: 'Space Grotesk'; font-weight: 700; font-size: 18px; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2) 55%, var(--t-yellow)); }
.hl-logo-text { font-weight: 700; font-size: 15.5px; color: var(--t-text1); white-space: nowrap; }
.hl-logo-text em { font-style: normal; color: var(--t-accent); }
.hl-nav-links { display: flex; gap: 4px; }
.hl-nlink { padding: 8px 13px; border-radius: 9px; font-size: 13.5px; font-weight: 600; color: var(--t-text2); text-decoration: none; transition: color 0.2s, background 0.2s; }
.hl-nlink:hover { color: var(--t-text1); background: var(--t-hover); }
.hl-nav-right { display: flex; align-items: center; gap: 8px; }
.hl-icon-btn { display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); cursor: pointer; transition: color 0.2s, border-color 0.2s; }
.hl-icon-btn:hover { color: var(--t-accent); border-color: var(--t-accent); }
@media (max-width: 820px) { .hl-nav-links { display: none; } .hl-logo-text { font-size: 13.5px; } }

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
.hl-hero-stats b { font-family: 'Space Grotesk'; font-weight: 700; font-size: 22px; background: linear-gradient(120deg, var(--t-accent), var(--t-accent2) 55%, var(--t-yellow)); -webkit-background-clip: text; background-clip: text; color: transparent; }
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
.hl-about-grid { display: grid; grid-template-columns: 1.15fr 0.85fr; gap: 44px; align-items: center; }
@media (max-width: 880px) { .hl-about-grid { grid-template-columns: 1fr; gap: 32px; } }
.hl-about-text { font-size: 15.5px; line-height: 1.8; color: var(--t-text2); margin: 0 0 14px; }
.hl-about-text strong { color: var(--t-text1); }
.hl-about-cta { margin-top: 10px; }
.hl-about-side { position: relative; display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }
.hl-about-hl { padding: 18px; border-radius: 18px; display: flex; align-items: center; gap: 12px; }
.hl-about-hl-ic { display: grid; place-items: center; width: 40px; height: 40px; border-radius: 11px; flex-shrink: 0; color: var(--t-accent); background: var(--t-acc-alpha-sm); border: 1px solid var(--t-glass-border); }
.hl-about-hl b { display: block; font-family: 'Space Grotesk'; font-size: 15px; color: var(--t-text1); }
.hl-about-hl span { font-size: 11.5px; color: var(--t-text3); }
.hl-about-bot { position: absolute; right: -10px; bottom: -40px; width: 72px; opacity: 0.9; }
@media (max-width: 880px) { .hl-about-bot { display: none; } }

/* capability cards */
.hl-caps { display: grid; grid-template-columns: repeat(4, 1fr); gap: 18px; }
@media (max-width: 1080px) { .hl-caps { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 560px) { .hl-caps { grid-template-columns: 1fr; } }
.hl-cap { padding: 16px; border-radius: 22px; display: flex; flex-direction: column; }
.cap-scene { position: relative; height: 130px; border-radius: 14px; overflow: hidden; padding: 11px 13px; background: linear-gradient(160deg, #0c0a1f, #16122e); border: 1px solid rgba(129,140,248,0.2); margin-bottom: 16px; }
.cap-scene-head { display: flex; justify-content: space-between; align-items: center; font-size: 8.5px; font-weight: 700; letter-spacing: 0.12em; color: #8d86b8; }
.cap-online { display: inline-flex; align-items: center; gap: 4px; color: #fbbf24; }
.cap-online i { width: 5px; height: 5px; border-radius: 99px; background: #fbbf24; }
.cap-bot { position: absolute; right: 8px; bottom: 4px; width: 46px; }
/* tutor scene */
.cap-board { margin-top: 12px; width: 64%; padding: 9px 10px; border-radius: 8px; background: rgba(5,3,13,0.6); border: 1px solid rgba(129,140,248,0.25); }
.cap-board i { display: block; height: 5px; border-radius: 9px; background: rgba(129,140,248,0.35); margin-bottom: 5px; }
.cap-board i.w70 { width: 70%; }
.cap-board b { font-family: 'Space Grotesk'; font-size: 11px; color: #fbbf24; }
/* paper scene */
.cap-pcard { margin-top: 8px; width: 66%; padding: 9px 10px; border-radius: 8px; background: #ece9fb; transform: rotate(-2deg); }
.cap-ptag { font-size: 7px; font-weight: 800; letter-spacing: 0.1em; color: #4a35a6; }
.cap-pcard > i { display: block; height: 4px; border-radius: 9px; background: #c9c2ec; margin: 5px 0; }
.cap-pcard > i.w80 { width: 80%; } .cap-pcard > i.w60 { width: 60%; }
.cap-mcq { display: flex; gap: 4px; margin-top: 6px; }
.cap-mcq span { width: 16px; height: 16px; border-radius: 5px; display: grid; place-items: center; font-size: 8px; font-weight: 800; color: #4a35a6; border: 1px solid #c9c2ec; }
.cap-mcq span.pick { color: #fff; background: #fbbf24; border-color: #fbbf24; }
/* examiner scene */
.cap-rows { margin-top: 12px; width: 58%; display: flex; flex-direction: column; gap: 9px; }
.cap-rows > div { position: relative; display: flex; align-items: center; }
.cap-rows i { display: block; height: 6px; border-radius: 9px; background: rgba(129,140,248,0.3); }
.cap-rows i.w80 { width: 80%; } .cap-rows i.w65 { width: 65%; } .cap-rows i.w70 { width: 70%; }
.cap-ck { position: absolute; right: -18px; width: 14px; height: 14px; border-radius: 99px; display: grid; place-items: center; font-size: 8px; }
.cap-ck.ok { color: #34d399; background: rgba(52,211,153,0.15); } .cap-ck.no { color: #fb7185; background: rgba(251,113,133,0.15); }
.cap-score { position: absolute; right: 42px; top: 38px; font-family: 'Space Grotesk'; font-weight: 700; font-size: 26px; color: #fbbf24; }
.cap-score small { font-size: 12px; }
/* arena scene */
.cap-podium { margin-top: 26px; display: flex; align-items: flex-end; gap: 5px; padding-left: 8px; }
.cap-podium span { width: 22px; border-radius: 4px 4px 0 0; background: rgba(129,140,248,0.35); display: grid; place-items: end center; font-size: 12px; }
.cap-podium .p1 { height: 44px; background: rgba(251,191,36,0.4); } .cap-podium .p2 { height: 30px; } .cap-podium .p3 { height: 22px; }
.cap-rank { position: absolute; right: 10px; top: 36px; padding: 3px 8px; border-radius: 99px; font-size: 8px; font-weight: 800; color: #05030d; background: linear-gradient(135deg,#a8a4ff,#fbbf24); }
.cap-meta { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.cap-ic { display: grid; place-items: center; width: 40px; height: 40px; border-radius: 12px; color: var(--t-accent); background: var(--t-acc-alpha-sm); border: 1px solid var(--t-glass-border); }
.cap-num { font-family: 'Space Grotesk'; font-weight: 700; font-size: 26px; color: transparent; -webkit-text-stroke: 1px var(--t-border); }
.hl-cap h3 { font-size: 17px; font-weight: 700; margin: 0 0 8px; color: var(--t-text1); }
.hl-cap p { font-size: 13px; line-height: 1.6; color: var(--t-text2); margin: 0 0 14px; flex: 1; }
.cap-chips { list-style: none; padding: 0; margin: 0; display: flex; flex-wrap: wrap; gap: 6px; }
.cap-chips li { display: inline-flex; align-items: center; gap: 4px; padding: 4px 9px; border-radius: 99px; font-size: 11px; font-weight: 600; color: var(--t-accent); background: var(--t-acc-alpha-sm); border: 1px solid var(--t-glass-border); }

/* features */
.hl-feat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
@media (max-width: 980px) { .hl-feat-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 560px) { .hl-feat-grid { grid-template-columns: 1fr; } }
.hl-feat { padding: 24px 22px; border-radius: 18px; transition: transform 0.2s, border-color 0.2s; }
.hl-feat:hover { transform: translateY(-3px); }
.hl-feat-ic { display: inline-grid; place-items: center; width: 44px; height: 44px; border-radius: 13px; margin-bottom: 14px; color: var(--t-accent); background: var(--t-acc-alpha-sm); border: 1px solid var(--t-glass-border); }
.hl-feat h3 { font-size: 16.5px; font-weight: 700; margin: 0 0 7px; color: var(--t-text1); }
.hl-feat p { font-size: 13.5px; line-height: 1.6; color: var(--t-text2); margin: 0; }

/* AI intelligence */
.hl-ai-stage { position: relative; display: grid; grid-template-columns: 1fr auto 1fr; grid-template-rows: auto auto; gap: 20px 40px; align-items: center; max-width: 1000px; margin: 0 auto; }
.hl-scard { padding: 16px 18px; border-radius: 18px; }
.hl-scard.pos1 { grid-column: 1; grid-row: 1; } .hl-scard.pos2 { grid-column: 3; grid-row: 1; }
.hl-scard.pos3 { grid-column: 1; grid-row: 2; } .hl-scard.pos4 { grid-column: 3; grid-row: 2; }
.hl-ai-core { grid-column: 2; grid-row: 1 / 3; position: relative; width: 150px; height: 150px; display: grid; place-items: center; }
.hl-ai-ring { position: absolute; border-radius: 50%; border: 1px dashed rgba(129,140,248,0.4); }
.hl-ai-ring.r1 { inset: 0; animation: hl-spin 22s linear infinite; } .hl-ai-ring.r2 { inset: 22px; border-color: rgba(251,191,36,0.3); animation: hl-spin 16s linear infinite reverse; }
.hl-ai-brain { width: 92px; height: 92px; border-radius: 50%; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 4px; color: var(--t-accent); background: var(--t-surface); border: 1px solid var(--t-accent); box-shadow: 0 0 36px var(--t-acc-alpha-md); }
.hl-ai-brain span { font-size: 8px; font-weight: 800; letter-spacing: 0.16em; }
@keyframes hl-spin { to { transform: rotate(360deg); } }
.hl-sc-top { display: flex; align-items: center; gap: 10px; margin-bottom: 12px; }
.hl-sc-av { display: grid; place-items: center; width: 34px; height: 34px; border-radius: 11px; color: #fff; font-weight: 700; font-size: 14px; }
.hl-sc-id { flex: 1; } .hl-sc-id b { display: block; font-size: 13.5px; color: var(--t-text1); } .hl-sc-id i { font-style: normal; font-size: 11px; color: var(--t-text3); }
.hl-sc-score { font-family: 'Space Grotesk'; font-weight: 700; font-size: 17px; }
.hl-bar { display: grid; grid-template-columns: 60px 1fr 32px; align-items: center; gap: 8px; margin-bottom: 6px; }
.hl-bar span { font-size: 10.5px; font-weight: 600; color: var(--t-text3); }
.hl-bar b { font-size: 10.5px; font-weight: 700; color: var(--t-text2); text-align: right; }
.hl-bar-track { display: block; height: 5px; border-radius: 99px; background: var(--t-hover); overflow: hidden; }
.hl-bar-fill { display: block; height: 100%; border-radius: 99px; width: 0; transition: width 1.1s cubic-bezier(0.22,1,0.36,1) 0.3s; }
.hl-ai-badge { position: absolute; display: inline-flex; align-items: center; gap: 6px; padding: 7px 12px; border-radius: 99px; font-size: 11px; font-weight: 600; color: var(--t-text1); z-index: 3; }
.hl-ai-badge svg { color: var(--t-gold); }
.hl-ai-badge.b1 { top: -14px; left: 50%; transform: translateX(-50%); }
.hl-ai-badge.b2 { bottom: -14px; left: 50%; transform: translateX(-50%); }
.hl-ai-badge.b3 { top: 46%; right: -10px; }
@media (max-width: 880px) {
  .hl-ai-stage { grid-template-columns: 1fr 1fr; grid-template-rows: auto; }
  .hl-scard.pos1, .hl-scard.pos2, .hl-scard.pos3, .hl-scard.pos4 { grid-column: auto; grid-row: auto; }
  .hl-ai-core { display: none; } .hl-ai-badge { display: none; }
}
@media (max-width: 520px) { .hl-ai-stage { grid-template-columns: 1fr; } }

/* split */
.hl-split { display: grid; grid-template-columns: 1fr 1.05fr; gap: 54px; align-items: center; }
@media (max-width: 940px) { .hl-split { grid-template-columns: 1fr; } }
.hl-ticks { list-style: none; padding: 0; margin: 20px 0 0; display: flex; flex-direction: column; gap: 11px; }
.hl-ticks li { display: flex; align-items: center; gap: 9px; font-size: 14.5px; font-weight: 600; color: var(--t-text1); }
.hl-ticks svg { color: var(--t-accent); }
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
.hl-stat b { display: block; font-family: 'Space Grotesk'; font-weight: 700; font-size: clamp(1.7rem, 3.4vw, 2.5rem); background: linear-gradient(120deg, var(--t-accent), var(--t-accent2) 55%, var(--t-yellow)); -webkit-background-clip: text; background-clip: text; color: transparent; font-variant-numeric: tabular-nums; }
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
.hl-spin { width: 15px; height: 15px; border-radius: 99px; border: 2px solid rgba(255,255,255,0.4); border-top-color: #fff; animation: hl-spinner 0.7s linear infinite; }
@keyframes hl-spinner { to { transform: rotate(360deg); } }
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

/* device frames */
:deep(.dv-phone) { position: relative; width: 250px; flex-shrink: 0; padding: 9px; border-radius: 40px; background: linear-gradient(160deg, var(--t-accent2), var(--t-bg2) 55%, var(--t-accent)); box-shadow: var(--t-glass-shadow), 0 0 0 1px var(--t-glass-border); }
:deep(.dv-notch) { position: absolute; top: 16px; left: 50%; transform: translateX(-50%); z-index: 3; width: 64px; height: 18px; border-radius: 99px; background: #05060c; }
:deep(.dv-phone-screen) { position: relative; height: 480px; border-radius: 32px; overflow: hidden; background: var(--t-surface); border: 1px solid var(--t-border); }
:deep(.dv-browser) { width: min(440px, 100%); border-radius: 16px; overflow: hidden; }
:deep(.dv-bar) { display: flex; align-items: center; gap: 6px; padding: 11px 14px; border-bottom: 1px solid var(--t-border); }
:deep(.dv-bar i) { width: 10px; height: 10px; border-radius: 99px; background: var(--t-border); }
:deep(.dv-bar i:nth-child(1)) { background: #f87171; } :deep(.dv-bar i:nth-child(2)) { background: #fbbf24; } :deep(.dv-bar i:nth-child(3)) { background: #34d399; }
:deep(.dv-url) { margin-left: 10px; padding: 3px 12px; border-radius: 7px; font-size: 11px; color: var(--t-text3); background: var(--t-hover); flex: 1; }
:deep(.dv-browser-screen) { height: 300px; overflow: hidden; background: var(--t-bg); }

/* mini app UIs */
:deep(.ui-app) { height: 100%; display: flex; flex-direction: column; padding-top: 40px; }
:deep(.ui-appbar) { display: flex; align-items: center; gap: 6px; padding: 10px 14px; font-size: 12px; font-weight: 700; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
:deep(.ui-list) { padding: 12px; display: flex; flex-direction: column; gap: 9px; }
:deep(.ui-row) { display: grid; grid-template-columns: auto 1fr auto; align-items: center; gap: 10px; padding: 10px; border-radius: 12px; background: var(--t-hover); border: 1px solid var(--t-border); }
:deep(.ui-row.open) { background: var(--t-acc-alpha-sm); border-color: var(--t-accent); }
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
:deep(.ui-dash) { display: grid; grid-template-columns: 78px 1fr; height: 100%; }
:deep(.ui-dash-side) { padding: 12px 10px; background: var(--t-bg2); border-right: 1px solid var(--t-border); display: flex; flex-direction: column; gap: 8px; }
:deep(.ui-dash-logo) { font-family: 'Space Grotesk'; font-weight: 700; font-size: 13px; color: var(--t-accent); margin-bottom: 6px; }
:deep(.ui-dash-side i) { height: 8px; border-radius: 99px; background: var(--t-hover); }
:deep(.ui-dash-side i.on) { background: linear-gradient(90deg, var(--t-accent), var(--t-accent2)); }
:deep(.ui-dash-main) { padding: 14px; }
:deep(.ui-dash-head) { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; font-size: 13px; color: var(--t-text1); }
:deep(.ui-score) { font-family: 'Space Grotesk'; font-weight: 700; color: var(--t-accent); }
:deep(.ui-q) { display: flex; align-items: center; gap: 6px; margin-bottom: 12px; }
:deep(.ui-q-bar) { height: 8px; border-radius: 99px; background: var(--t-hover); }
:deep(.ui-opt) { width: 20px; height: 20px; border-radius: 6px; display: grid; place-items: center; font-size: 10px; font-weight: 700; color: var(--t-text3); border: 1px solid var(--t-border); }
:deep(.ui-opt.pick) { color: #fff; background: var(--t-accent); border-color: var(--t-accent); }

/* robot */
:deep(.hl-bot) { overflow: visible; }
:deep(.hl-bot .b-shell) { fill: var(--t-surface); stroke: var(--t-accent); stroke-width: 2; }
:deep(.hl-bot .b-visor) { fill: var(--t-bg); }
:deep(.hl-bot .b-eye) { fill: var(--t-accent); }
:deep(.hl-bot .b-core) { fill: var(--t-gold); }
:deep(.hl-bot .b-limb) { fill: var(--t-surface); stroke: var(--t-accent); stroke-width: 2; }
:deep(.cap-scene .hl-bot .b-shell), :deep(.cap-scene .hl-bot .b-limb) { fill: #16122e; stroke: #818cf8; }
:deep(.cap-scene .hl-bot .b-visor) { fill: #0c0a1f; }
:deep(.cap-scene .hl-bot .b-eye) { fill: #a99cff; }
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
