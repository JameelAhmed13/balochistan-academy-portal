<template>
  <div class="ln4" :class="{ 'is-ur': isUr }" :dir="isUr ? 'rtl' : 'ltr'">

    <!-- ── Nav ── -->
    <header class="nav" :class="{ scrolled }">
      <div class="nav-in">
        <RouterLink to="/" class="brand" @click="menuOpen = false">
          <img :src="logoUrl" alt="" class="brand-logo" width="36" height="36" />
          <span class="brand-name">{{ t.brand }}</span>
        </RouterLink>

        <nav class="nav-links" aria-label="Main">
          <a v-for="l in navLinks" :key="l.href" :href="l.href" class="nav-link">{{ l.label }}</a>
        </nav>

        <div class="nav-actions">
          <button type="button" class="lang-btn" @click="toggleLang">{{ t.langToggle }}</button>
          <RouterLink to="/login" class="btn btn-primary btn-nav">{{ t.navCta }}</RouterLink>
          <button
            type="button"
            class="menu-btn"
            :aria-expanded="menuOpen"
            :aria-label="menuOpen ? t.menuClose : t.menuOpen"
            @click="menuOpen = !menuOpen"
          >
            <X v-if="menuOpen" :size="22" />
            <Menu v-else :size="22" />
          </button>
        </div>
      </div>

      <!-- mobile drawer -->
      <nav v-show="menuOpen" class="drawer" aria-label="Mobile">
        <a v-for="l in navLinks" :key="l.href" :href="l.href" class="drawer-link" @click="menuOpen = false">{{ l.label }}</a>
        <div class="drawer-foot">
          <button type="button" class="lang-btn lang-btn-wide" @click="toggleLang">{{ t.langToggle }}</button>
          <RouterLink to="/login" class="btn btn-primary" @click="menuOpen = false">{{ t.navCta }}</RouterLink>
        </div>
      </nav>
    </header>

    <main>
      <!-- ── Hero ── -->
      <section class="hero">
        <div class="wrap hero-grid">
          <div class="hero-copy rv">
            <p class="kicker">{{ t.heroKicker }}</p>
            <h1 class="h1">{{ t.heroH1 }}</h1>
            <p class="lead">{{ t.heroSub }}</p>
            <div class="hero-ctas">
              <RouterLink to="/login" class="btn btn-primary btn-lg">
                {{ t.heroCta1 }} <ArrowRight :size="18" class="flip-rtl" />
              </RouterLink>
              <a href="#how" class="btn btn-ghost btn-lg">{{ t.heroCta2 }}</a>
            </div>
            <p class="hero-note">{{ t.heroNote }}</p>
          </div>

          <div class="hero-visual rv">
            <div class="phone">
              <img :src="imgDash" :alt="t.altDash" width="780" height="1688" fetchpriority="high" />
            </div>
          </div>
        </div>
      </section>

      <!-- ── How students learn ── -->
      <section id="how" class="section">
        <div class="wrap">
          <div class="sec-head rv">
            <p class="kicker">{{ t.howKicker }}</p>
            <h2 class="h2">{{ t.howH2 }}</h2>
            <p class="sub">{{ t.howSub }}</p>
          </div>

          <div v-for="(s, i) in t.steps" :key="s.word" class="step rv" :class="{ alt: i % 2 === 1 }">
            <div class="step-copy">
              <p class="step-tag">
                <span class="step-num">{{ String(i + 1).padStart(2, '0') }}</span>
                <span class="step-word">{{ s.word }}</span>
                <span class="step-word-ur">{{ s.wordUr }}</span>
              </p>
              <h3 class="h3">{{ s.title }}</h3>
              <p class="body">{{ s.desc }}</p>
              <ul class="check-list">
                <li v-for="c in s.points" :key="c"><Check :size="16" aria-hidden="true" /> {{ c }}</li>
              </ul>
            </div>
            <div class="step-visual">
              <div class="phone phone-sm">
                <img :src="stepImages[i]" :alt="s.alt" width="780" height="1688" loading="lazy" />
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ── Study & test system ── -->
      <section id="tests" class="section section-tint">
        <div class="wrap">
          <div class="sec-head rv">
            <p class="kicker">{{ t.testsKicker }}</p>
            <h2 class="h2">{{ t.testsH2 }}</h2>
            <p class="sub">{{ t.testsSub }}</p>
          </div>
          <div class="card-grid">
            <article v-for="(c, i) in t.testCards" :key="c.title" class="card rv">
              <span class="card-icon"><component :is="testIcons[i]" :size="22" aria-hidden="true" /></span>
              <h3 class="card-title">{{ c.title }}</h3>
              <p class="card-desc">{{ c.desc }}</p>
            </article>
          </div>
        </div>
      </section>

      <!-- ── Progress & parents ── -->
      <section id="parents" class="section">
        <div class="wrap parents-grid">
          <div class="rv">
            <p class="kicker">{{ t.parentsKicker }}</p>
            <h2 class="h2">{{ t.parentsH2 }}</h2>
            <p class="body">{{ t.parentsSub }}</p>
            <ul class="check-list check-list-lg">
              <li v-for="b in t.parentsPoints" :key="b"><Check :size="16" aria-hidden="true" /> {{ b }}</li>
            </ul>
          </div>
          <div class="rv">
            <div class="browser">
              <div class="browser-bar" aria-hidden="true"><i /><i /><i /></div>
              <img :src="imgDesk" :alt="t.altDesk" width="1600" height="1000" loading="lazy" />
            </div>
          </div>
        </div>
      </section>

      <!-- ── Stats strip ── -->
      <section class="section section-tint">
        <div class="wrap">
          <div class="stats rv">
            <div v-for="s in t.stats" :key="s.label" class="stat">
              <span class="stat-v">{{ s.value }}</span>
              <span class="stat-l">{{ s.label }}</span>
            </div>
          </div>
          <p class="stats-note rv">{{ t.statsNote }}</p>
        </div>
      </section>

      <!-- ── Final CTA ── -->
      <section class="section cta">
        <div class="wrap rv">
          <h2 class="h2">{{ t.ctaH2 }}</h2>
          <p class="sub">{{ t.ctaSub }}</p>
          <RouterLink to="/login" class="btn btn-primary btn-lg">
            {{ t.ctaBtn }} <ArrowRight :size="18" class="flip-rtl" />
          </RouterLink>
        </div>
      </section>
    </main>

    <!-- ── Footer ── -->
    <footer id="contact" class="footer">
      <div class="wrap footer-grid">
        <div class="footer-brand">
          <div class="brand">
            <img :src="logoUrl" alt="" class="brand-logo" width="36" height="36" />
            <span class="brand-name">{{ t.brand }}</span>
          </div>
          <p class="footer-tag">{{ t.footerTag }}</p>
          <p class="footer-inst">{{ t.footerInst }}</p>
        </div>

        <nav class="footer-col" :aria-label="t.footerLinksH">
          <h3 class="footer-h">{{ t.footerLinksH }}</h3>
          <a v-for="l in navLinks" :key="l.href" :href="l.href" class="footer-link">{{ l.label }}</a>
          <RouterLink to="/login" class="footer-link">{{ t.footerLogin }}</RouterLink>
        </nav>

        <div class="footer-col">
          <h3 class="footer-h">{{ t.contactH }}</h3>
          <p class="footer-contact-p">{{ t.contactSub }}</p>
          <form v-if="form.status !== 'done'" class="contact-form" @submit.prevent="submitContact">
            <label class="sr-only" for="ln4-name">{{ t.formName }}</label>
            <input id="ln4-name" v-model.trim="form.name" type="text" :placeholder="t.formName" required autocomplete="name" />
            <label class="sr-only" for="ln4-phone">{{ t.formPhone }}</label>
            <input id="ln4-phone" v-model.trim="form.phone" type="tel" :placeholder="t.formPhone" required autocomplete="tel" />
            <button type="submit" class="btn btn-primary" :disabled="form.status === 'sending'">
              {{ form.status === 'sending' ? t.formSending : t.formBtn }}
            </button>
            <p v-if="form.status === 'error'" class="form-msg" role="alert">{{ t.formError }}</p>
          </form>
          <p v-else class="form-msg form-ok" role="status">{{ t.formDone }}</p>
        </div>
      </div>

      <div class="wrap footer-bottom">
        <p>{{ t.copyright }}</p>
        <button type="button" class="lang-btn" @click="toggleLang">{{ t.langToggle }}</button>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted, onBeforeUnmount } from 'vue'
import {
  Menu, X, ArrowRight, Check,
  SlidersHorizontal, CalendarCheck, Trophy, ListChecks, PenLine, BarChart3,
} from '@lucide/vue'

import logoUrl from '@/assets/logo.png'
import imgDash from '@/assets/landing4/dash-mobile.png'
import imgVideo from '@/assets/landing4/video-mobile.png'
import imgTutor from '@/assets/landing4/tutor-mobile.png'
import imgNotes from '@/assets/landing4/notes-mobile.png'
import imgDesk from '@/assets/landing4/dash-desktop.png'

const stepImages = [imgVideo, imgTutor, imgNotes]
const testIcons = [SlidersHorizontal, CalendarCheck, Trophy, ListChecks, PenLine, BarChart3]

/* ── language ── */
const lang = ref(localStorage.getItem('ln4-lang') || 'en')
const isUr = computed(() => lang.value === 'ur')
function toggleLang() {
  lang.value = isUr.value ? 'en' : 'ur'
  localStorage.setItem('ln4-lang', lang.value)
}

const STRINGS = {
  en: {
    brand: 'Balochistan Academy Portal',
    langToggle: 'اردو',
    menuOpen: 'Open menu',
    menuClose: 'Close menu',
    navCta: 'Start Learning',
    nav: { how: 'How it Works', tests: 'Tests', parents: 'For Parents', contact: 'Contact' },

    heroKicker: 'Balochistan Board · Classes 1–10',
    heroH1: 'Watch, listen and read your way to board-exam success.',
    heroSub:
      'Balochistan Academy Portal turns the official textbooks into video lectures, spoken explanations and revision notes — then tests you on every chapter until you are exam-ready.',
    heroCta1: 'Start Learning Free',
    heroCta2: 'See how it works',
    heroNote: 'Works on any phone · English & Urdu medium',
    altDash: 'Student dashboard of the Balochistan Academy Portal app on a phone',

    howKicker: 'How students learn',
    howH2: 'Three steps to master every chapter',
    howSub: 'Daikho · Suno · Parho — every chapter is watched, heard and read before it is tested.',
    steps: [
      {
        word: 'Daikho', wordUr: 'دیکھو',
        title: 'Watch — video lectures for every chapter',
        desc: 'Recorded lessons by expert teachers, mapped chapter-by-chapter to the textbooks — from Class 1 to Matric, with a built-in player and lesson playlists.',
        points: ['1,570+ video lessons in 23 courses', '231 textbook chapters covered', 'Search any course, play right inside the app'],
        alt: 'Video Lectures screen listing courses with chapters and lesson counts',
      },
      {
        word: 'Suno', wordUr: 'سنو',
        title: 'Listen — explanations you can hear, anywhere',
        desc: 'AI subject tutors explain every concept aloud in English or Urdu. Ask by typing or by voice, and hear the answer spoken back — ideal for revision on the go.',
        points: ['Live AI video tutor with spoken answers', 'Voice input for questions and written tests', 'English and Urdu medium support'],
        alt: 'AI Tutor screen with live video tutor and text chat options',
      },
      {
        word: 'Parho', wordUr: 'پڑھو',
        title: 'Read — notes, key points and past papers',
        desc: 'Chapter-wise key notes and revision checklists, plus e-books and past papers — textbook-aligned reading material for every subject.',
        points: ['Chapter-wise revision checklists', 'Key notes for all 10 subjects', 'E-books and past papers in one place'],
        alt: 'Key Notes screen showing a chapter-wise revision checklist for Physics',
      },
    ],

    testsKicker: 'Study & test system',
    testsH2: 'Practice the way the board examines',
    testsSub: 'Board-pattern objective and subjective practice with instant marking — every result recorded.',
    testCards: [
      { title: 'Self Tests', desc: 'Build your own test from any units and topics — objective or subjective, easy to hard.' },
      { title: 'Daily Tests', desc: 'A combined multi-subject challenge every day that keeps your study streak alive.' },
      { title: 'Monthly Tests', desc: 'Full-pattern monthly exams and weekly quizzes, with leaderboards across the province.' },
      { title: 'Objective Practice', desc: '1,200+ board-pattern MCQs with instant checking and an explanation for every answer.' },
      { title: 'Subjective Practice', desc: 'Write full answers in the app — AI marks them against model answers with feedback.' },
      { title: 'Instant Results', desc: 'Scores, solutions and weak-topic breakdowns appear the moment you submit.' },
    ],

    parentsKicker: 'Progress & parents',
    parentsH2: 'Parents see the progress, not just the promise',
    parentsSub:
      'Every test a student takes is recorded. Reports, graphs and follow-up tests are one tap away — so parents and teachers always know exactly where a child stands.',
    parentsPoints: [
      'Parent/Teacher tests — assign a test at home, see the result instantly',
      'Result reports with charts for every test taken',
      'Progress tracking, coins and streaks that reward daily study',
    ],
    altDesk: 'Home dashboard showing test records, result reports and parent/teacher test modules',

    stats: [
      { value: '1,570+', label: 'Video lessons' },
      { value: '231', label: 'Chapters covered' },
      { value: '1,800+', label: 'Practice questions' },
      { value: '10', label: 'Subjects' },
    ],
    statsNote: 'Aligned with the Balochistan Board syllabus and textbook chapters — in English and Urdu medium.',

    ctaH2: 'Start preparing today',
    ctaSub: 'Full access for Rs. 999 a year — every subject, every test, English and Urdu medium.',
    ctaBtn: 'Start Learning',

    footerTag: 'Board-exam preparation for Balochistan — watch, listen, read and test, in English and Urdu.',
    footerInst: 'New Century Educational System (bbisedemo) · Balochistan Board',
    footerLinksH: 'Explore',
    footerLogin: 'Student Login',
    contactH: 'Contact',
    contactSub: 'Leave your number and we will call you back about admission.',
    formName: 'Your name',
    formPhone: 'Phone / WhatsApp',
    formBtn: 'Request a Call',
    formSending: 'Sending…',
    formDone: 'Thank you — we will call you back soon.',
    formError: 'Could not send right now. Please try again later.',
    copyright: '© 2026 Balochistan Academy Portal',
  },

  ur: {
    brand: 'بلوچستان اکیڈمی پورٹل',
    langToggle: 'English',
    menuOpen: 'مینیو کھولیں',
    menuClose: 'مینیو بند کریں',
    navCta: 'سیکھنا شروع کریں',
    nav: { how: 'یہ کیسے کام کرتا ہے', tests: 'ٹیسٹ', parents: 'والدین کے لیے', contact: 'رابطہ' },

    heroKicker: 'بلوچستان بورڈ · کلاس ۱ تا ۱۰',
    heroH1: 'دیکھو، سنو، پڑھو — اور بورڈ امتحان میں کامیابی پاؤ۔',
    heroSub:
      'بلوچستان اکیڈمی پورٹل نصابی کتابوں کو ویڈیو لیکچرز، سنائی جانے والی وضاحتوں اور ریویژن نوٹس میں بدل دیتا ہے — پھر ہر باب پر آپ کا ٹیسٹ لیتا ہے، جب تک آپ امتحان کے لیے پوری طرح تیار نہ ہو جائیں۔',
    heroCta1: 'مفت سیکھنا شروع کریں',
    heroCta2: 'طریقہ دیکھیں',
    heroNote: 'ہر فون پر چلتا ہے · انگریزی اور اردو میڈیم',
    altDash: 'بلوچستان اکیڈمی پورٹل ایپ کا طالب علم ڈیش بورڈ',

    howKicker: 'طلبہ کیسے سیکھتے ہیں',
    howH2: 'ہر باب پر عبور کے تین قدم',
    howSub: 'دیکھو · سنو · پڑھو — ہر باب پہلے دیکھا، سنا اور پڑھا جاتا ہے، پھر اس کا ٹیسٹ ہوتا ہے۔',
    steps: [
      {
        word: 'Daikho', wordUr: 'دیکھو',
        title: 'دیکھیں — ہر باب کے ویڈیو لیکچرز',
        desc: 'ماہر اساتذہ کے ریکارڈ شدہ اسباق، نصابی کتابوں کے ہر باب کے مطابق ترتیب دیے گئے — کلاس اوّل سے میٹرک تک، ایپ کے اندر مکمل پلیئر اور پلے لسٹ کے ساتھ۔',
        points: ['۲۳ کورسز میں 1,570+ ویڈیو اسباق', 'نصاب کے 231 ابواب کا احاطہ', 'کوئی بھی کورس تلاش کریں، ایپ ہی میں چلائیں'],
        alt: 'ویڈیو لیکچرز کی اسکرین، کورسز اور اسباق کی فہرست کے ساتھ',
      },
      {
        word: 'Suno', wordUr: 'سنو',
        title: 'سنیں — وضاحتیں جو آپ کہیں بھی سن سکتے ہیں',
        desc: 'AI ٹیوٹر ہر تصور انگریزی یا اردو میں بول کر سمجھاتے ہیں۔ سوال لکھ کر یا بول کر پوچھیں اور جواب سنیں — سفر میں ریویژن کے لیے بہترین۔',
        points: ['بولنے والا لائیو AI ویڈیو ٹیوٹر', 'سوالوں اور تحریری ٹیسٹ کے لیے آواز سے اِن پٹ', 'انگریزی اور اردو میڈیم کی سہولت'],
        alt: 'AI ٹیوٹر کی اسکرین، لائیو ویڈیو ٹیوٹر اور چیٹ کے اختیارات کے ساتھ',
      },
      {
        word: 'Parho', wordUr: 'پڑھو',
        title: 'پڑھیں — نوٹس، اہم نکات اور پرانے پرچے',
        desc: 'باب وار اہم نوٹس اور ریویژن چیک لسٹ، ساتھ ہی ای بکس اور پرانے پرچے — ہر مضمون کے لیے نصاب کے مطابق مواد۔',
        points: ['باب وار ریویژن چیک لسٹ', 'تمام ۱۰ مضامین کے اہم نوٹس', 'ای بکس اور پرانے پرچے ایک جگہ'],
        alt: 'فزکس کی باب وار ریویژن چیک لسٹ دکھاتی کی نوٹس اسکرین',
      },
    ],

    testsKicker: 'مطالعہ اور ٹیسٹ کا نظام',
    testsH2: 'اسی انداز میں مشق کریں جس میں بورڈ امتحان لیتا ہے',
    testsSub: 'بورڈ پیٹرن کے معروضی اور انشائی سوالات، فوری مارکنگ کے ساتھ — ہر نتیجہ محفوظ رہتا ہے۔',
    testCards: [
      { title: 'سیلف ٹیسٹ', desc: 'کسی بھی یونٹ اور ٹاپک سے اپنا ٹیسٹ خود بنائیں — معروضی یا انشائی، آسان سے مشکل تک۔' },
      { title: 'روزانہ ٹیسٹ', desc: 'ہر روز کئی مضامین پر مشتمل مشترکہ چیلنج، جو آپ کی پڑھائی کا تسلسل قائم رکھتا ہے۔' },
      { title: 'ماہانہ ٹیسٹ', desc: 'مکمل پیٹرن کے ماہانہ امتحانات اور ہفتہ وار کوئز، صوبہ بھر کی درجہ بندی کے ساتھ۔' },
      { title: 'معروضی مشق', desc: 'بورڈ پیٹرن کے 1,200+ MCQs، فوری جانچ اور ہر جواب کی وضاحت کے ساتھ۔' },
      { title: 'انشائی مشق', desc: 'ایپ میں مکمل جواب لکھیں — AI نمونہ جوابات سے موازنہ کر کے نمبر اور رائے دیتا ہے۔' },
      { title: 'فوری نتائج', desc: 'جمع کرتے ہی نمبر، حل اور کمزور موضوعات کی تفصیل سامنے آ جاتی ہے۔' },
    ],

    parentsKicker: 'پیش رفت اور والدین',
    parentsH2: 'والدین صرف وعدہ نہیں، پیش رفت دیکھتے ہیں',
    parentsSub:
      'طالب علم کا ہر ٹیسٹ ریکارڈ ہوتا ہے۔ رپورٹس، گراف اور فالو اَپ ٹیسٹ ایک ٹچ پر موجود ہیں — تاکہ والدین اور اساتذہ کو ہر وقت معلوم رہے کہ بچہ کہاں کھڑا ہے۔',
    parentsPoints: [
      'والدین/اساتذہ کے ٹیسٹ — گھر بیٹھے ٹیسٹ دیں اور فوراً نتیجہ دیکھیں',
      'ہر ٹیسٹ کی چارٹ کے ساتھ رزلٹ رپورٹ',
      'پیش رفت کا ریکارڈ، کوائنز اور اسٹریکس جو روزانہ پڑھائی پر انعام دیتے ہیں',
    ],
    altDesk: 'ہوم ڈیش بورڈ، ٹیسٹ ریکارڈ، رزلٹ رپورٹس اور والدین/اساتذہ ٹیسٹ کے ماڈیولز کے ساتھ',

    stats: [
      { value: '1,570+', label: 'ویڈیو اسباق' },
      { value: '231', label: 'ابواب کا احاطہ' },
      { value: '1,800+', label: 'مشقی سوالات' },
      { value: '10', label: 'مضامین' },
    ],
    statsNote: 'بلوچستان بورڈ کے نصاب اور کتابی ابواب کے عین مطابق — انگریزی اور اردو میڈیم میں۔',

    ctaH2: 'آج ہی تیاری شروع کریں',
    ctaSub: 'سال بھر کی مکمل رسائی صرف 999 روپے میں — ہر مضمون، ہر ٹیسٹ، انگریزی اور اردو میڈیم۔',
    ctaBtn: 'سیکھنا شروع کریں',

    footerTag: 'بلوچستان کے لیے بورڈ امتحان کی تیاری — دیکھو، سنو، پڑھو اور ٹیسٹ دو، انگریزی اور اردو میں۔',
    footerInst: 'نیو سنچری ایجوکیشنل سسٹم (bbisedemo) · بلوچستان بورڈ',
    footerLinksH: 'صفحات',
    footerLogin: 'طالب علم لاگ اِن',
    contactH: 'رابطہ',
    contactSub: 'اپنا نمبر چھوڑیں، ہم داخلے کے بارے میں آپ کو کال کریں گے۔',
    formName: 'آپ کا نام',
    formPhone: 'فون / واٹس ایپ',
    formBtn: 'کال کی درخواست',
    formSending: 'بھیجا جا رہا ہے…',
    formDone: 'شکریہ — ہم جلد آپ کو کال کریں گے۔',
    formError: 'ابھی نہیں بھیجا جا سکا۔ براہِ کرم بعد میں دوبارہ کوشش کریں۔',
    copyright: '© 2026 بلوچستان اکیڈمی پورٹل',
  },
}

const t = computed(() => STRINGS[lang.value])
const navLinks = computed(() => [
  { href: '#how', label: t.value.nav.how },
  { href: '#tests', label: t.value.nav.tests },
  { href: '#parents', label: t.value.nav.parents },
  { href: '#contact', label: t.value.nav.contact },
])

/* ── nav state ── */
const menuOpen = ref(false)
const scrolled = ref(false)
function onScroll() { scrolled.value = window.scrollY > 8 }

/* ── contact form ── */
const webhookUrl = import.meta.env.VITE_N8N_WEBHOOK_URL || 'http://localhost:5678/webhook/bap-lead'
const form = reactive({ name: '', phone: '', status: 'idle' })
async function submitContact() {
  form.status = 'sending'
  try {
    const res = await fetch(webhookUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: form.name, phone: form.phone, source: 'landing4', submittedAt: new Date().toISOString() }),
    })
    if (!res.ok) throw new Error(`Webhook responded ${res.status}`)
    form.status = 'done'
  } catch {
    form.status = 'error'
  }
}

/* ── scroll reveal (fade/slide only, respects reduced motion) ──
   Content is visible by default; the hidden state is applied by JS only
   to below-fold elements, so the page is never blank without JS. */
let observer = null
onMounted(() => {
  const els = document.querySelectorAll('.ln4 .rv')
  const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches
  if (!reduce && 'IntersectionObserver' in window) {
    observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((e) => {
          if (e.isIntersecting) {
            e.target.classList.add('in')
            observer.unobserve(e.target)
          }
        })
      },
      { rootMargin: '0px 0px -10% 0px' },
    )
    els.forEach((el) => {
      if (el.getBoundingClientRect().top > window.innerHeight) {
        el.classList.add('rv-h')
        observer.observe(el)
      }
    })
  }
  window.addEventListener('scroll', onScroll, { passive: true })
  onScroll()
})
onBeforeUnmount(() => {
  observer?.disconnect()
  window.removeEventListener('scroll', onScroll)
})
</script>

<style scoped>
/* ════ Design tokens ════ */
.ln4 {
  /* color — neutrals + 1 primary (violet) + 1 accent (emerald) */
  --bg: #0b0f1d;
  --surface: #131830;
  --surface-2: #181e3a;
  --border: rgba(160, 170, 210, 0.14);
  --border-strong: rgba(160, 170, 210, 0.26);
  --text-1: #eceef8;
  --text-2: #a9b0cc;
  --text-3: #7c84a8;
  --primary: #6d5ef0;
  --primary-hover: #7e71f4;
  --primary-soft: rgba(109, 94, 240, 0.14);
  --primary-text: #b3a9ff;
  --accent: #3dd6a3;
  --on-primary: #ffffff;

  /* spacing — 8px scale */
  --sp-1: 8px; --sp-2: 16px; --sp-3: 24px; --sp-4: 32px;
  --sp-5: 40px; --sp-6: 48px; --sp-8: 64px; --sp-12: 96px;

  /* radii */
  --r-sm: 8px; --r-md: 12px; --r-lg: 16px; --r-xl: 24px;

  /* type scale: 14 / 16 / 18 / 24 / 32 / 48 */
  --fs-sm: 0.875rem;
  --fs-base: 1rem;
  --fs-lg: 1.125rem;
  --fs-xl: 1.5rem;
  --fs-2xl: 2rem;
  --fs-3xl: 3rem;

  --shadow-card: 0 1px 2px rgba(3, 5, 16, 0.4), 0 16px 40px rgba(3, 5, 16, 0.35);
  --nav-h: 64px;

  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
  font-size: var(--fs-base);
  line-height: 1.6;
  color: var(--text-1);
  background: var(--bg);
  min-height: 100vh;
  overflow-x: clip;
}

/* Urdu mode */
.ln4.is-ur { font-family: 'Noto Nastaliq Urdu', 'Plus Jakarta Sans', serif; line-height: 2; }
.ln4.is-ur .h1, .ln4.is-ur .h2, .ln4.is-ur .h3 { line-height: 1.9; letter-spacing: 0; }
.ln4.is-ur .kicker { letter-spacing: 0; }
.ln4[dir='rtl'] .flip-rtl { transform: scaleX(-1); }

.sr-only {
  position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px;
  overflow: hidden; clip: rect(0 0 0 0); white-space: nowrap; border: 0;
}

.wrap { max-width: 1140px; margin-inline: auto; padding-inline: var(--sp-3); }

/* ════ Type ════ */
.h1 {
  font-size: clamp(2.25rem, 5.5vw, var(--fs-3xl));
  font-weight: 800; line-height: 1.12; letter-spacing: -0.02em;
  margin: 0 0 var(--sp-3); color: var(--text-1);
}
.h2 {
  font-size: clamp(1.75rem, 4vw, var(--fs-2xl));
  font-weight: 800; line-height: 1.2; letter-spacing: -0.015em;
  margin: 0 0 var(--sp-2); color: var(--text-1);
}
.h3 { font-size: var(--fs-xl); font-weight: 700; line-height: 1.3; margin: 0 0 var(--sp-2); color: var(--text-1); }
.lead { font-size: var(--fs-lg); color: var(--text-2); margin: 0 0 var(--sp-4); max-width: 34em; }
.body { font-size: var(--fs-base); color: var(--text-2); margin: 0 0 var(--sp-3); max-width: 36em; }
.sub { font-size: var(--fs-lg); color: var(--text-2); margin: 0 auto; max-width: 38em; }
.kicker {
  font-size: var(--fs-sm); font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--primary-text); margin: 0 0 var(--sp-2);
}
.ln4.is-ur .kicker { text-transform: none; }

/* ════ Buttons ════ */
.btn {
  display: inline-flex; align-items: center; justify-content: center; gap: 8px;
  min-height: 44px; padding: 0 var(--sp-3);
  border: 1px solid transparent; border-radius: var(--r-md); cursor: pointer;
  font-family: inherit; font-size: var(--fs-base); font-weight: 700; text-decoration: none;
  transition: background-color 0.2s ease, border-color 0.2s ease, transform 0.2s ease;
}
.btn-primary { background: var(--primary); color: var(--on-primary); }
.btn-primary:hover { background: var(--primary-hover); }
.btn-primary:disabled { opacity: 0.65; cursor: wait; }
.btn-ghost { border-color: var(--border-strong); color: var(--text-1); background: transparent; }
.btn-ghost:hover { border-color: var(--primary-text); background: var(--primary-soft); }
.btn-lg { min-height: 52px; padding: 0 var(--sp-4); }
.btn:focus-visible, .lang-btn:focus-visible, .menu-btn:focus-visible,
.nav-link:focus-visible, .drawer-link:focus-visible, .footer-link:focus-visible {
  outline: 2px solid var(--primary-text); outline-offset: 2px;
}

/* ════ Nav ════ */
.nav {
  position: fixed; top: 0; left: 0; right: 0; z-index: 50;
  background: transparent; border-bottom: 1px solid transparent;
  transition: background-color 0.25s ease, border-color 0.25s ease;
}
.nav.scrolled { background: rgba(11, 15, 29, 0.88); backdrop-filter: blur(12px); border-bottom-color: var(--border); }
.nav-in {
  max-width: 1140px; margin-inline: auto; padding-inline: var(--sp-3);
  height: var(--nav-h); display: flex; align-items: center; justify-content: space-between; gap: var(--sp-2);
}
.brand { display: inline-flex; align-items: center; gap: 12px; text-decoration: none; min-width: 0; }
.brand-logo { width: 36px; height: 36px; border-radius: var(--r-sm); object-fit: contain; flex-shrink: 0; }
.brand-name { font-size: var(--fs-base); font-weight: 800; color: var(--text-1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.nav-links { display: none; }
.nav-link {
  display: inline-flex; align-items: center; min-height: 44px; padding: 0 14px;
  border-radius: var(--r-sm); font-size: var(--fs-sm); font-weight: 600;
  color: var(--text-2); text-decoration: none; transition: color 0.2s ease, background-color 0.2s ease;
}
.nav-link:hover { color: var(--text-1); background: var(--primary-soft); }
.nav-actions { display: flex; align-items: center; gap: var(--sp-1); }
.lang-btn {
  display: inline-flex; align-items: center; justify-content: center;
  min-height: 44px; min-width: 44px; padding: 0 14px;
  border: 1px solid var(--border-strong); border-radius: var(--r-sm);
  background: transparent; color: var(--text-2); cursor: pointer;
  font-size: var(--fs-sm); font-weight: 700;
  font-family: 'Noto Nastaliq Urdu', 'Plus Jakarta Sans', sans-serif;
  transition: color 0.2s ease, border-color 0.2s ease;
}
.ln4.is-ur .lang-btn { font-family: 'Plus Jakarta Sans', sans-serif; }
.lang-btn:hover { color: var(--text-1); border-color: var(--primary-text); }
.btn-nav { display: none; }
.menu-btn {
  display: inline-flex; align-items: center; justify-content: center;
  width: 44px; height: 44px; border: 1px solid var(--border-strong); border-radius: var(--r-sm);
  background: transparent; color: var(--text-1); cursor: pointer;
}
.drawer {
  display: flex; flex-direction: column; gap: 4px;
  background: rgba(11, 15, 29, 0.97); backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--border);
  padding: var(--sp-2) var(--sp-3) var(--sp-3);
}
.drawer-link {
  display: flex; align-items: center; min-height: 48px; padding-inline: var(--sp-2);
  border-radius: var(--r-sm); font-size: var(--fs-base); font-weight: 600;
  color: var(--text-1); text-decoration: none;
}
.drawer-link:hover { background: var(--primary-soft); }
.drawer-foot { display: grid; grid-template-columns: auto 1fr; gap: var(--sp-1); margin-top: var(--sp-2); }
.lang-btn-wide { width: 100%; }

/* ════ Hero ════ */
.hero {
  position: relative;
  padding: calc(var(--nav-h) + var(--sp-6)) 0 var(--sp-8);
  /* the page's single, subtle gradient */
  background: radial-gradient(900px 480px at 50% -10%, rgba(109, 94, 240, 0.16), transparent 70%);
}
.hero-grid { display: grid; grid-template-columns: 1fr; gap: var(--sp-6); align-items: center; }
.hero-copy { text-align: center; }
.hero-ctas { display: flex; flex-wrap: wrap; gap: var(--sp-2); justify-content: center; }
.hero-note { font-size: var(--fs-sm); color: var(--text-3); margin: var(--sp-2) 0 0; }
.hero-copy .lead { margin-inline: auto; }
.hero-visual { display: flex; justify-content: center; }

/* device frame — minimal, no fake UI chrome */
.phone {
  width: min(300px, 78vw);
  padding: 10px; border-radius: 40px;
  background: var(--surface); border: 1px solid var(--border-strong);
  box-shadow: var(--shadow-card);
}
.phone img { display: block; width: 100%; height: auto; border-radius: 30px; }
.phone-sm { width: min(260px, 72vw); }

/* ════ Sections ════ */
.section { padding: var(--sp-12) 0; }
.section-tint { background: rgba(19, 24, 48, 0.45); border-block: 1px solid var(--border); }
.sec-head { text-align: center; max-width: 44em; margin: 0 auto var(--sp-8); }

/* ════ How — steps ════ */
.step {
  display: grid; grid-template-columns: 1fr; gap: var(--sp-4);
  align-items: center; padding: var(--sp-6) 0;
  border-top: 1px solid var(--border);
}
.step:first-of-type { border-top: 0; }
.step-visual { display: flex; justify-content: center; order: -1; }
.step-tag { display: flex; align-items: baseline; gap: var(--sp-2); margin: 0 0 var(--sp-2); }
.step-num { font-size: var(--fs-sm); font-weight: 800; color: var(--text-3); letter-spacing: 0.08em; }
.step-word { font-size: var(--fs-lg); font-weight: 800; color: var(--primary-text); }
.step-word-ur { font-family: 'Noto Nastaliq Urdu', serif; font-size: var(--fs-lg); color: var(--text-2); }
.check-list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: var(--sp-1); }
.check-list li { display: flex; align-items: flex-start; gap: 10px; font-size: var(--fs-base); color: var(--text-2); }
.check-list svg { flex-shrink: 0; margin-top: 5px; color: var(--accent); }
.ln4.is-ur .check-list svg { margin-top: 12px; }
.check-list-lg { gap: var(--sp-2); margin-top: var(--sp-3); }

/* ════ Test cards ════ */
.card-grid { display: grid; grid-template-columns: 1fr; gap: var(--sp-2); }
.card {
  padding: var(--sp-3); border-radius: var(--r-lg);
  background: var(--surface); border: 1px solid var(--border);
  transition: border-color 0.2s ease, transform 0.2s ease;
}
.card:hover { border-color: var(--border-strong); transform: translateY(-2px); }
.card-icon {
  display: inline-flex; align-items: center; justify-content: center;
  width: 44px; height: 44px; border-radius: var(--r-md); margin-bottom: var(--sp-2);
  background: var(--primary-soft); color: var(--primary-text);
}
.card-title { font-size: var(--fs-lg); font-weight: 700; margin: 0 0 var(--sp-1); color: var(--text-1); }
.card-desc { font-size: var(--fs-base); color: var(--text-2); margin: 0; }

/* ════ Parents ════ */
.parents-grid { display: grid; grid-template-columns: 1fr; gap: var(--sp-6); align-items: center; }
.browser {
  border-radius: var(--r-lg); overflow: hidden;
  background: var(--surface); border: 1px solid var(--border-strong);
  box-shadow: var(--shadow-card);
}
.browser-bar { display: flex; gap: 6px; padding: 12px 14px; border-bottom: 1px solid var(--border); }
.browser-bar i { width: 10px; height: 10px; border-radius: 50%; background: var(--border-strong); }
.browser img { display: block; width: 100%; height: auto; }

/* ════ Stats ════ */
.stats { display: grid; grid-template-columns: repeat(2, 1fr); gap: var(--sp-3); text-align: center; }
.stat { display: flex; flex-direction: column; gap: 4px; }
.stat-v { font-size: var(--fs-2xl); font-weight: 800; color: var(--text-1); font-variant-numeric: tabular-nums; }
.stat-l { font-size: var(--fs-sm); font-weight: 600; color: var(--text-3); }
.stats-note { text-align: center; font-size: var(--fs-base); color: var(--text-2); margin: var(--sp-5) auto 0; max-width: 38em; }

/* ════ CTA ════ */
.cta .wrap { text-align: center; }
.cta .sub { margin-bottom: var(--sp-4); }

/* ════ Footer ════ */
.footer { border-top: 1px solid var(--border); padding: var(--sp-8) 0 var(--sp-3); background: rgba(19, 24, 48, 0.3); }
.footer-grid { display: grid; grid-template-columns: 1fr; gap: var(--sp-6); }
.footer-tag { font-size: var(--fs-base); color: var(--text-2); margin: var(--sp-2) 0 var(--sp-1); max-width: 30em; }
.footer-inst { font-size: var(--fs-sm); color: var(--text-3); margin: 0; }
.footer-col { display: flex; flex-direction: column; align-items: flex-start; gap: 2px; }
.footer-h { font-size: var(--fs-sm); font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text-3); margin: 0 0 var(--sp-1); }
.ln4.is-ur .footer-h { text-transform: none; letter-spacing: 0; }
.footer-link {
  display: inline-flex; align-items: center; min-height: 40px;
  font-size: var(--fs-base); color: var(--text-2); text-decoration: none;
}
.footer-link:hover { color: var(--text-1); }
.footer-contact-p { font-size: var(--fs-base); color: var(--text-2); margin: 0 0 var(--sp-2); }
.contact-form { display: flex; flex-direction: column; gap: var(--sp-1); width: 100%; max-width: 320px; }
.contact-form input {
  min-height: 44px; padding: 0 var(--sp-2);
  border: 1px solid var(--border-strong); border-radius: var(--r-md);
  background: var(--bg); color: var(--text-1);
  font-family: inherit; font-size: var(--fs-base);
}
.contact-form input::placeholder { color: var(--text-3); }
.contact-form input:focus { outline: 2px solid var(--primary-text); outline-offset: 1px; border-color: transparent; }
.form-msg { font-size: var(--fs-sm); color: var(--text-2); margin: var(--sp-1) 0 0; }
.form-ok { color: var(--accent); }
.footer-bottom {
  display: flex; align-items: center; justify-content: space-between; gap: var(--sp-2);
  margin-top: var(--sp-6); padding-top: var(--sp-3); border-top: 1px solid var(--border);
}
.footer-bottom p { font-size: var(--fs-sm); color: var(--text-3); margin: 0; }

/* ════ Reveal motion (fade/slide only; .rv-h is added by JS) ════ */
.rv-h { opacity: 0; transform: translateY(16px); transition: opacity 0.5s ease, transform 0.5s ease; }
.rv-h.in { opacity: 1; transform: none; }
@media (prefers-reduced-motion: reduce) {
  .rv-h { opacity: 1; transform: none; transition: none; }
  .btn, .card { transition: none; }
}

/* ════ ≥768px ════ */
@media (min-width: 768px) {
  .card-grid { grid-template-columns: repeat(2, 1fr); gap: var(--sp-3); }
  .stats { grid-template-columns: repeat(4, 1fr); }
  .step { grid-template-columns: 1fr 1fr; gap: var(--sp-8); }
  .step-visual { order: 0; }
  .step.alt .step-visual { order: -1; }
  .footer-grid { grid-template-columns: 1.4fr 0.8fr 1fr; }
}

/* ════ ≥1024px ════ */
@media (min-width: 1024px) {
  .nav-links { display: flex; gap: 4px; }
  .btn-nav { display: inline-flex; }
  .menu-btn, .drawer { display: none; }
  .hero { padding-top: calc(var(--nav-h) + var(--sp-12)); }
  .hero-grid { grid-template-columns: 1.1fr 0.9fr; gap: var(--sp-8); }
  .hero-copy { text-align: start; }
  .hero-copy .lead { margin-inline: 0; }
  .hero-ctas { justify-content: flex-start; }
  .card-grid { grid-template-columns: repeat(3, 1fr); }
  .parents-grid { grid-template-columns: 0.9fr 1.1fr; gap: var(--sp-8); }
}
</style>
