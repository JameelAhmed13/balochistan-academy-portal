<template>
  <header class="cosmic-header">
    <div class="header-glow-line" />

    <!-- Hamburger — mobile only (desktop uses sidebar's own collapse button) -->
    <button @click="$emit('toggle-sidebar')" class="hdr-btn lg:hidden" title="Toggle sidebar">
      <Menu class="w-5 h-5" />
    </button>

    <!-- Page title -->
    <div class="hdr-title-wrap">
      <div class="hdr-breadcrumb">
        <span class="hdr-bc-home">Balochistan Academy Portal</span>
        <span class="hdr-bc-sep">›</span>
        <span class="hdr-bc-cur">{{ pageTitle }}</span>
      </div>
      <p class="hdr-subtitle hidden sm:block">{{ pageSubtitle }}</p>
    </div>

    <div class="flex-1" />

    <!-- Subscription chip -->
    <button @click="subModal = true" class="hdr-sub-chip hidden md:flex">
      <Sparkles class="w-3.5 h-3.5" />
      <span>Rs.999/yr</span>
    </button>

    <!-- ── Theme Toggle ── -->
    <button @click="theme.toggle()" class="hdr-btn hdr-theme-btn" :title="theme.isDark ? 'Switch to Light' : 'Switch to Dark'">
      <Transition name="theme-icon" mode="out-in">
        <Sun v-if="theme.isDark" key="sun" class="w-4.5 h-4.5" />
        <Moon v-else key="moon" class="w-4.5 h-4.5" />
      </Transition>
    </button>

    <!-- Notification bell -->
    <button class="hdr-btn hdr-notif-btn" title="Notifications">
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9M13.73 21a2 2 0 0 1-3.46 0"/>
      </svg>
      <span class="hdr-notif-dot" />
    </button>

    <!-- User dropdown -->
    <div class="relative" ref="userMenu">
      <button @click="dropOpen = !dropOpen" class="hdr-user-btn">
        <div class="hdr-avatar">{{ initials }}</div>
        <div class="hdr-user-info hidden sm:block">
          <div class="hdr-user-name">{{ auth.user?.name }}</div>
          <div class="hdr-user-phone">{{ auth.user?.phone }}</div>
        </div>
        <ChevronDown class="w-4 h-4 hdr-chevron" :class="dropOpen ? 'rotate-180' : ''" />
      </button>

      <Transition name="drop">
        <div v-if="dropOpen" class="hdr-dropdown">
          <div class="hdr-drop-header">
            <div class="hdr-drop-av">{{ initials }}</div>
            <div>
              <div class="hdr-drop-name">{{ auth.user?.name }}</div>
              <div class="hdr-drop-inst">{{ auth.user?.institute }}</div>
            </div>
          </div>
          <div class="hdr-drop-body">
            <router-link to="/app/reports" class="hdr-drop-item" @click="dropOpen=false">
              <BarChart2 class="w-4 h-4" /> My Reports
            </router-link>
            <button class="hdr-drop-item w-full" @click="showHistory">
              <History class="w-4 h-4" /> Login History
            </button>
            <!-- Mobile theme toggle in dropdown -->
            <button class="hdr-drop-item w-full md:hidden" @click="theme.toggle(); dropOpen=false">
              <Sun v-if="theme.isDark" class="w-4 h-4" />
              <Moon v-else class="w-4 h-4" />
              {{ theme.isDark ? 'Light Mode' : 'Dark Mode' }}
            </button>
          </div>
          <div class="hdr-drop-footer">
            <button class="hdr-drop-item danger w-full" @click="logout">
              <LogOut class="w-4 h-4" /> Sign Out
            </button>
          </div>
        </div>
      </Transition>
    </div>
  </header>

  <!-- Modals -->
  <Dialog v-model:visible="historyModal" header="Login History" :modal="true" :style="{ width: '500px' }">
    <div class="space-y-2 max-h-72 overflow-y-auto">
      <div v-for="h in auth.loginHistory" :key="h.id" class="flex items-start gap-3 p-3 rounded-xl text-sm"
        style="background:var(--t-hover);border:1px solid var(--t-border)">
        <div class="w-2 h-2 rounded-full mt-1.5 shrink-0" :class="h.status === 'success' ? 'bg-emerald-400' : 'bg-red-400'" />
        <div class="min-w-0">
          <div class="font-medium" style="color:var(--t-text1)">{{ formatDate(h.timestamp) }}</div>
          <div class="text-xs truncate" style="color:var(--t-text3)">{{ h.device }}</div>
          <div class="text-xs" style="color:var(--t-text3)">IP: {{ h.ip }}</div>
        </div>
        <span class="ml-auto badge" :class="h.status === 'success' ? 'badge-green' : 'badge-red'">{{ h.status }}</span>
      </div>
      <div v-if="!auth.loginHistory.length" class="text-center py-8 text-sm" style="color:var(--t-text3)">No login records found.</div>
    </div>
  </Dialog>

  <Dialog v-model:visible="subModal" header="Full Access Subscription" :modal="true" :style="{ width: '460px' }">
    <div class="space-y-4">
      <div class="p-4 rounded-xl bg-gradient-to-br from-amber-50 to-orange-50 border border-amber-200">
        <div class="text-2xl font-bold text-amber-700">Rs. 999 <span class="text-base font-normal text-amber-600">/ year</span></div>
        <div class="text-sm text-amber-600 mt-1">One-year unlimited access to all features</div>
      </div>
      <ul class="text-sm space-y-2" style="color:var(--t-text2)">
        <li class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Unlimited AI Tutor sessions</li>
        <li class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Full question bank access</li>
        <li class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> AI-graded subjective answers</li>
        <li class="flex gap-2"><CheckCircle class="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" /> Coin rewards &amp; withdrawals</li>
      </ul>
      <div class="p-3 rounded-xl text-sm" style="background:var(--t-hover);border:1px solid var(--t-border);color:var(--t-text2)">
        <strong>Payment:</strong> Transfer to Easypaisa/JazzCash and send screenshot via WhatsApp for activation.
      </div>
      <a href="https://wa.me/923001234567" target="_blank" class="btn-primary w-full justify-center">
        <MessageCircle class="w-4 h-4" /> Activate via WhatsApp
      </a>
    </div>
  </Dialog>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { onClickOutside } from '@vueuse/core'
import { Menu, ChevronDown, LogOut, BarChart2, History, Sparkles, CheckCircle, MessageCircle, Sun, Moon } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'

defineProps({ sidebarCollapsed: Boolean })
defineEmits(['toggle-sidebar'])

const auth = useAuthStore()
const theme = useThemeStore()
const router = useRouter()
const route = useRoute()
const dropOpen = ref(false)
const historyModal = ref(false)
const subModal = ref(false)
const userMenu = ref(null)

onClickOutside(userMenu, () => { dropOpen.value = false })

const initials = computed(() => {
  const n = auth.user?.name || ''
  return n.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase()
})

const PAGE_TITLES = {
  '/app':                      ['Home Dashboard', 'Your learning overview'],
  '/app/hub':                  ['Start Your Learning Journey', 'All features in one place'],
  '/app/ai-tutor':             ['AI Tutor', "Learn from Pakistan's AI Legends"],
  '/app/preparation':          ['Preparation', 'Digital Library & Test Builder'],
  '/app/self-test':            ['Self Test', 'Test yourself on any subject'],
  '/app/objective-tests':      ['Objective Tests', 'MCQ test history & results'],
  '/app/parent-tests':         ['Parent Tests', 'Parent-assigned test tracking'],
  '/app/daily-tests/combined': ['Daily Combined Challenge', 'Multi-subject timed exam'],
  '/app/daily-tests/result':   ['Daily Test Result', 'Your exam performance'],
  '/app/daily-tests':          ['Daily Tests', 'Daily Practice Hub'],
  '/app/competition/monthly':  ['Monthly Exam', 'Mega combined exam'],
  '/app/competition/result':   ['Monthly Test Result', 'Your exam performance'],
  '/app/competition/challenge':['Challenge Quiz', 'Timed competitive challenge'],
  '/app/competition/weekly':   ['Weekly Quiz', 'Subject-wise weekly quiz'],
  '/app/competition/leaderboard':['Leaderboard', 'Top student rankings & badges'],
  '/app/competition':          ['Competition Tests', 'Monthly Exam Powerhouse'],
  '/app/video':                ['Video Lectures', 'Learn from expert teachers'],
  '/app/calculator':           ['Scientific Calculator', 'Solve calculations instantly'],
  '/app/periodic-table':       ['Periodic Table', 'Interactive element reference'],
  '/app/log-table':            ['Logarithm Table', 'Log & antilog reference'],
  '/app/dictionary':           ['Smart Dictionary', 'Words, definitions & AI explanations'],
  '/app/learn-coding':         ['Learn Coding', 'Python, JS, HTML, C++ lessons'],
  '/app/simulations':          ['Simulations', 'Interactive Physics & Chemistry'],
  '/app/join-forces':          ['Study Groups', 'Chat, challenge & study together'],
  '/app/time-table':           ['Study Time Table', 'Plan your study schedule'],
  '/app/smart-notes':          ['Smart Notes', 'Create and organize notes'],
  '/app/full-prep':            ['Full Preparation', 'Complete exam preparation'],
  '/app/global-library':       ['Global Library', 'Access worldwide resources'],
  '/app/learn-coding':         ['Learn Coding', 'Programming for beginners'],
  '/app/puzzle-games':         ['Puzzle Games', 'Educational brain teasers'],
  '/app/online-classes':       ['Online Classes', 'Live and recorded sessions'],
  '/app/career-counselor':     ['Career Counselor', 'Discover your ideal career'],
  '/app/join-forces':          ['Join Forces', 'Study groups & collaboration'],
  '/app/typing-master':        ['Typing Master', 'Improve your typing speed'],
  '/app/messages':             ['Messages', 'Chat with teachers & peers'],
  '/app/slos-mapping':         ['SLOs Mapping', 'Curriculum outcome tracking'],
  '/app/mdcat':                ['MDCAT Preparation', 'Medical entry test prep'],
  '/app/etea':                 ['ETEA Preparation', 'Engineering entry test prep'],
  '/app/conceptual':           ['Conceptual Learning', 'Deep concept explanations'],
  '/app/experiments':          ['Virtual Experiments', 'Interactive lab experiments'],
  '/app/general-knowledge':    ['General Knowledge', 'GK quiz & learning'],
  '/app/simulations':          ['Simulations', 'Interactive physics/chem sims'],
  '/app/study-notebook':      ['Study Notebook', 'Your personal notes'],
  '/app/ebooks':               ['E-Books', 'Digital textbooks'],
  '/app/mcqs-bank':            ['MCQs Bank', 'Browse all MCQ questions'],
  '/app/past-papers':          ['Past Papers', 'Solve board exam papers'],
  '/app/progress-tracking':    ['Progress Tracking', 'Your academic journey'],
  '/app/chatgpt':              ['AI Chat Assistant', 'Ask anything about studies'],
  '/app/coins':                ['Coins & Rewards', 'Your earnings wallet'],
  '/app/reports':              ['Reports', 'Performance analytics'],
  '/app/complaints':           ['Complaints & Suggestions', 'Send us your feedback'],
  '/app/admin':                ['Admin Panel', 'Manage platform content & users'],
}

const pageTitle = computed(() => {
  const p = route.path
  for (const [key, [title]] of Object.entries(PAGE_TITLES)) {
    if (key === '/app' ? (p === '/app' || p === '/app/') : p.startsWith(key)) return title
  }
  return 'Balochistan Academy Portal'
})
const pageSubtitle = computed(() => {
  const p = route.path
  for (const [key, [, sub]] of Object.entries(PAGE_TITLES)) {
    if (key === '/app' ? (p === '/app' || p === '/app/') : p.startsWith(key)) return sub
  }
  return ''
})

function showHistory() {
  auth.loadLoginHistory()
  historyModal.value = true
  dropOpen.value = false
}
function logout() {
  auth.logout()
  router.push('/login')
}
function formatDate(iso) {
  return new Date(iso).toLocaleString('en-PK', { dateStyle: 'medium', timeStyle: 'short' })
}
</script>

<style scoped>
.cosmic-header {
  height: 64px; flex-shrink: 0;
  background: var(--t-header);
  border-bottom: 1px solid var(--t-header-border);
  box-shadow: var(--t-header-shadow);
  backdrop-filter: var(--t-blur);
  display: flex; align-items: center; padding: 0 1rem; gap: 0.75rem;
  position: relative; z-index: 20;
  transition: background 0.25s, border-color 0.25s, box-shadow 0.25s;
}
.header-glow-line {
  position: absolute; bottom: 0; left: 0; right: 0; height: 1px;
  background: linear-gradient(90deg, transparent, var(--t-acc-alpha-md), transparent);
  pointer-events: none;
}

/* Shared icon button */
.hdr-btn {
  width: 38px; height: 38px; border-radius: 11px; flex-shrink: 0;
  background: var(--t-hover); border: 1px solid var(--t-border);
  color: var(--t-text3); display: flex; align-items: center; justify-content: center;
  cursor: none; transition: all 0.15s;
}
.hdr-btn:hover { background: var(--t-hover2); color: var(--t-text1); }

/* Theme toggle gets accent glow on hover */
.hdr-theme-btn:hover {
  border-color: color-mix(in srgb, var(--t-accent) 40%, transparent);
  color: var(--t-accent);
  box-shadow: 0 0 12px color-mix(in srgb, var(--t-accent) 25%, transparent);
}

/* Notification badge */
.hdr-notif-btn { position: relative; }
.hdr-notif-dot {
  position: absolute; top: 8px; right: 8px; width: 6px; height: 6px; border-radius: 50%;
  background: var(--t-accent); box-shadow: 0 0 6px color-mix(in srgb, var(--t-accent) 70%, transparent);
  animation: notifPulse 2s ease-in-out infinite;
}
@keyframes notifPulse { 0%,100%{transform:scale(1)}50%{transform:scale(1.3)} }

.hdr-title-wrap { min-width: 0; }
.hdr-breadcrumb { display: flex; align-items: center; gap: 0.4rem; }
.hdr-bc-home { font-size: 0.72rem; color: var(--t-text3); }
.hdr-bc-sep  { font-size: 0.72rem; color: var(--t-text3); }
.hdr-bc-cur  { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 0.9rem; color: var(--t-text1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 220px; }
.hdr-subtitle { font-size: 0.68rem; color: var(--t-text3); margin: 0; line-height: 1.3; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 260px; }

.hdr-sub-chip {
  align-items: center; gap: 0.4rem;
  padding: 0.4rem 0.9rem; border-radius: 99px;
  background: color-mix(in srgb, var(--t-gold) 15%, transparent);
  border: 1px solid color-mix(in srgb, var(--t-gold) 35%, transparent);
  color: var(--t-gold); font-size: 0.73rem; font-weight: 600;
  cursor: none; transition: all 0.15s; white-space: nowrap;
}
.hdr-sub-chip:hover {
  background: color-mix(in srgb, var(--t-gold) 22%, transparent);
}

.hdr-user-btn {
  display: flex; align-items: center; gap: 0.6rem; padding: 0.35rem 0.65rem; border-radius: 14px;
  background: var(--t-hover); border: 1px solid var(--t-border);
  cursor: none; transition: all 0.15s; flex-shrink: 0;
}
.hdr-user-btn:hover { background: var(--t-hover2); border-color: color-mix(in srgb, var(--t-accent) 30%, transparent); }
.hdr-avatar {
  width: 32px; height: 32px; border-radius: 50%; flex-shrink: 0;
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  color: white; font-weight: 700; font-size: 0.75rem;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 0 12px color-mix(in srgb, var(--t-accent) 30%, transparent);
}
.hdr-user-name  { font-size: 0.82rem; font-weight: 600; color: var(--t-text1); line-height: 1.2; white-space: nowrap; max-width: 120px; overflow: hidden; text-overflow: ellipsis; }
.hdr-user-phone { font-size: 0.68rem; color: var(--t-text3); }
.hdr-chevron { color: var(--t-text3); transition: transform 0.2s; flex-shrink: 0; }

/* Dropdown */
.hdr-dropdown {
  position: absolute; right: 0; top: calc(100% + 8px);
  width: 224px; border-radius: 18px;
  background: var(--t-dropdown-bg);
  border: 1px solid var(--t-border);
  box-shadow: var(--t-shadow-md);
  backdrop-filter: var(--t-blur); overflow: hidden; z-index: 50;
}
.hdr-drop-header {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 1rem; border-bottom: 1px solid var(--t-border);
  background: var(--t-acc-alpha-sm);
}
.hdr-drop-av {
  width: 36px; height: 36px; border-radius: 50%; flex-shrink: 0;
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  color: white; font-weight: 700; font-size: 0.82rem;
  display: flex; align-items: center; justify-content: center;
}
.hdr-drop-name { font-weight: 600; font-size: 0.875rem; color: var(--t-text1); }
.hdr-drop-inst { font-size: 0.7rem; color: var(--t-text3); }
.hdr-drop-body { padding: 0.4rem; }
.hdr-drop-footer { padding: 0.4rem; border-top: 1px solid var(--t-border); }
.hdr-drop-item {
  display: flex; align-items: center; gap: 0.65rem;
  width: 100%; padding: 0.55rem 0.75rem; border-radius: 11px; border: none;
  background: none; color: var(--t-text2); font-size: 0.82rem; font-weight: 500;
  text-decoration: none; transition: all 0.15s; cursor: none; text-align: left;
}
.hdr-drop-item:hover { background: var(--t-dropdown-item); color: var(--t-text1); }
.hdr-drop-item.danger { color: #f87171; }
.hdr-drop-item.danger:hover { background: rgba(248, 113, 113, 0.1); }

/* Theme icon transition */
.theme-icon-enter-active, .theme-icon-leave-active { transition: opacity 0.15s, transform 0.15s; }
.theme-icon-enter-from { opacity: 0; transform: rotate(-90deg) scale(0.5); }
.theme-icon-leave-to   { opacity: 0; transform: rotate(90deg)  scale(0.5); }

/* Dropdown transition */
.drop-enter-active, .drop-leave-active { transition: opacity 0.15s, transform 0.15s; }
.drop-enter-from, .drop-leave-to { opacity: 0; transform: translateY(-8px) scale(0.97); }
</style>
