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
    <button v-if="auth.user?.role === 'student'" @click="subModal = true" class="hdr-sub-chip hidden md:flex" :class="chipClass">
      <component :is="chipIcon" class="w-3.5 h-3.5" />
      <span>{{ chipLabel }}</span>
    </button>

    <!-- ── Theme Toggle ── -->
    <button @click="theme.toggle()" class="hdr-btn hdr-theme-btn" :title="theme.isDark ? 'Switch to Light' : 'Switch to Dark'">
      <Transition name="theme-icon" mode="out-in">
        <Sun v-if="theme.isDark" key="sun" class="w-4.5 h-4.5" />
        <Moon v-else key="moon" class="w-4.5 h-4.5" />
      </Transition>
    </button>

    <!-- Notification bell -->
    <div class="relative" ref="notifWrap">
      <button @click="notifOpen = !notifOpen" class="hdr-btn hdr-notif-btn" title="Notifications">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9M13.73 21a2 2 0 0 1-3.46 0"/>
        </svg>
        <span v-show="notif.unreadCount > 0" class="hdr-notif-dot" />
        <span v-if="notif.unreadCount > 0" class="hdr-notif-count">
          {{ notif.unreadCount > 99 ? '99+' : notif.unreadCount }}
        </span>
      </button>

      <Transition name="drop">
        <div v-if="notifOpen" class="hdr-notif-panel">
          <div class="hdr-notif-head">
            <span class="font-semibold text-sm" style="color:var(--t-text1)">Notifications</span>
            <button v-if="notif.unreadCount > 0" @click="notif.markAllRead()" class="hdr-notif-all-read">
              Mark all read
            </button>
          </div>
          <div class="hdr-notif-list">
            <div v-if="!notif.notifications.length" class="hdr-notif-empty">
              No notifications yet
            </div>
            <div
              v-for="n in notif.notifications.slice(0, 6)"
              :key="n.id"
              @click="notif.markRead(n.id)"
              :class="['hdr-notif-item', { 'hdr-notif-unread': !n.read }]"
            >
              <div class="flex items-start gap-2">
                <span v-if="n.icon" class="text-base leading-none mt-0.5 shrink-0">{{ n.icon }}</span>
                <div class="min-w-0">
                  <div class="hdr-notif-item-title">{{ n.title }}</div>
                  <div v-if="n.body" class="hdr-notif-item-body">{{ n.body }}</div>
                  <div class="hdr-notif-item-time">{{ formatDateTime(n.createdAt || n.receivedAt) }}</div>
                </div>
              </div>
            </div>
          </div>
          <div class="hdr-notif-footer">
            <router-link
              to="/app/notifications"
              @click="notifOpen = false"
              class="hdr-notif-view-all"
            >
              View All Notifications
              <span v-if="notif.unreadCount > 0" class="hdr-notif-footer-badge">{{ notif.unreadCount }} unread</span>
            </router-link>
          </div>
        </div>
      </Transition>
    </div>

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
            <router-link to="/app/profile" class="hdr-drop-item" @click="dropOpen=false">
              <UserCog class="w-4 h-4" /> Edit Profile
            </router-link>
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
          <div class="font-medium" style="color:var(--t-text1)">{{ formatDateTime(h.timestamp) }}</div>
          <div class="text-xs truncate" style="color:var(--t-text3)">{{ h.device }}</div>
          <div class="text-xs" style="color:var(--t-text3)">IP: {{ h.ip }}</div>
        </div>
        <span class="ml-auto badge" :class="h.status === 'success' ? 'badge-green' : 'badge-red'">{{ h.status }}</span>
      </div>
      <div v-if="!auth.loginHistory.length" class="text-center py-8 text-sm" style="color:var(--t-text3)">No login records found.</div>
    </div>
  </Dialog>

  <Dialog v-model:visible="subModal" header="Subscription Plans" :modal="true" :style="{ width: '560px' }" @show="onSubModalOpen">
    <div class="sub-modal">

      <!-- Status banner -->
      <div v-if="student.accessStatus === 'trial'" class="sub-status trial">
        <Clock class="w-4 h-4" />
        <span>{{ student.trialDaysLeft }} day{{ student.trialDaysLeft !== 1 ? 's' : '' }} left in your free trial</span>
      </div>
      <div v-else-if="student.accessStatus === 'expired'" class="sub-status expired">
        <AlertTriangle class="w-4 h-4" />
        <span>Your free trial has ended — subscribe to keep learning</span>
      </div>
      <div v-else-if="student.accessStatus === 'subscribed'" class="sub-status active">
        <CheckCircle class="w-4 h-4" />
        <span>On {{ student.subscription?.planName }} · expires {{ formatDateTime(student.subscription?.endDate) }}</span>
      </div>

      <!-- Plans -->
      <div v-if="loadingPlans" class="sub-loading">Loading plans…</div>
      <div v-else-if="!student.plans.length" class="sub-empty">No subscription plans are available right now.</div>
      <div v-else class="sub-plan-grid">
        <div
          v-for="p in student.plans" :key="p.id" class="sub-plan-card"
          :class="{ current: isCurrentPlan(p.id) }"
        >
          <div v-if="isCurrentPlan(p.id)" class="sub-plan-badge">Current Plan</div>
          <div class="sub-plan-name">{{ p.name }}</div>
          <div class="sub-plan-price">Rs. {{ p.price.toLocaleString() }} <small>/ {{ p.durationDays }} days</small></div>
          <ul class="sub-plan-feats">
            <li><CheckCircle class="w-3.5 h-3.5" /> {{ p.aiTokenQuota.toLocaleString() }} AI tutor tokens</li>
            <li><CheckCircle class="w-3.5 h-3.5" /> Full question bank access</li>
            <li><CheckCircle class="w-3.5 h-3.5" /> AI-graded subjective answers</li>
          </ul>
        </div>
      </div>

      <!-- Actions -->
      <div class="sub-actions">
        <button type="button" class="btn-primary w-full justify-center" @click="goCheckout">
          <Sparkles class="w-4 h-4" /> Activate Now — Pay Securely
        </button>
        <RouterLink to="/app/coins/redeem" class="btn-secondary w-full justify-center" @click="subModal = false">
          <Coins class="w-4 h-4" /> Or Redeem With Coins
        </RouterLink>
        <a href="https://wa.me/923703153540" target="_blank" class="sub-whatsapp">
          <MessageCircle class="w-3.5 h-3.5" /> Need help? Activate via WhatsApp
        </a>
      </div>
    </div>
  </Dialog>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { onClickOutside } from '@vueuse/core'
import { Menu, ChevronDown, LogOut, BarChart2, History, Sparkles, CheckCircle, MessageCircle, Sun, Moon, UserCog, Clock, AlertTriangle, Coins } from '@lucide/vue'
import Dialog from 'primevue/dialog'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'
import { useNotificationsStore } from '@/stores/notifications'
import { useStudentStore } from '@/stores/student'
import { formatDateTime } from '@/utils/datetime'

defineProps({ sidebarCollapsed: Boolean })
defineEmits(['toggle-sidebar'])

const auth = useAuthStore()
const theme = useThemeStore()
const notif = useNotificationsStore()
const student = useStudentStore()
const router = useRouter()
const route = useRoute()
const dropOpen = ref(false)
const historyModal = ref(false)
const subModal = ref(false)
const loadingPlans = ref(false)
const userMenu = ref(null)
const notifOpen = ref(false)
const notifWrap = ref(null)

onMounted(() => {
  if (auth.user?.role === 'student' && student.accessStatus === null) student.fetchSubscription()
})

onClickOutside(userMenu,  () => { dropOpen.value  = false })
onClickOutside(notifWrap, () => { notifOpen.value = false })

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
  '/app/notifications':        ['My Notifications', 'All your alerts and announcements'],
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

const chipLabel = computed(() => {
  if (student.accessStatus === 'subscribed') return student.subscription?.planName || 'Subscribed'
  if (student.accessStatus === 'expired') return 'Subscribe Now'
  if (student.accessStatus === 'trial') return `Trial: ${student.trialDaysLeft}d left`
  return 'Subscribe'
})
const chipIcon = computed(() => {
  if (student.accessStatus === 'subscribed') return CheckCircle
  if (student.accessStatus === 'expired') return AlertTriangle
  return Sparkles
})
const chipClass = computed(() => ({
  'hdr-sub-chip--active': student.accessStatus === 'subscribed',
  'hdr-sub-chip--expired': student.accessStatus === 'expired',
}))

function isCurrentPlan(planId) {
  return student.accessStatus === 'subscribed' && student.subscription?.planId === planId
}

async function onSubModalOpen() {
  if (student.plans.length) return
  loadingPlans.value = true
  try {
    await Promise.allSettled([student.fetchPlans(), student.fetchSubscription()])
  } finally {
    loadingPlans.value = false
  }
}

function showHistory() {
  auth.loadLoginHistory()
  historyModal.value = true
  dropOpen.value = false
}
function goCheckout() {
  subModal.value = false
  router.push('/app/checkout')
}
function logout() {
  auth.logout()
  router.push('/login')
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

.hdr-notif-count {
  position: absolute; top: 4px; right: 4px;
  min-width: 16px; height: 16px; border-radius: 99px; padding: 0 4px;
  background: #ef4444; color: white; font-size: 0.6rem; font-weight: 700;
  display: flex; align-items: center; justify-content: center;
  line-height: 1; pointer-events: none;
}

/* Notification panel */
.hdr-notif-panel {
  position: absolute; right: 0; top: calc(100% + 8px);
  width: 320px; border-radius: 18px;
  background: var(--t-dropdown-bg);
  border: 1px solid var(--t-border);
  box-shadow: var(--t-shadow-md);
  backdrop-filter: var(--t-blur);
  overflow: hidden; z-index: 50;
}
.hdr-notif-head {
  display: flex; justify-content: space-between; align-items: center;
  padding: 0.85rem 1rem; border-bottom: 1px solid var(--t-border);
  background: var(--t-acc-alpha-sm);
}
.hdr-notif-all-read {
  font-size: 0.7rem; font-weight: 600; color: var(--t-accent);
  background: none; border: none; cursor: pointer; padding: 0;
}
.hdr-notif-all-read:hover { text-decoration: underline; }
.hdr-notif-list { max-height: 320px; overflow-y: auto; }
.hdr-notif-empty {
  padding: 2rem 1rem; text-align: center; font-size: 0.82rem; color: var(--t-text3);
}
.hdr-notif-item {
  padding: 0.7rem 1rem; border-bottom: 1px solid var(--t-border);
  cursor: pointer; transition: background 0.12s;
}
.hdr-notif-item:last-child { border-bottom: none; }
.hdr-notif-item:hover { background: var(--t-hover); }
.hdr-notif-unread { background: var(--t-acc-alpha-sm); }
.hdr-notif-unread:hover { background: var(--t-acc-alpha-md); }
.hdr-notif-item-title {
  font-size: 0.8rem; font-weight: 600; color: var(--t-text1);
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.hdr-notif-item-body {
  font-size: 0.72rem; color: var(--t-text2); margin-top: 2px;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
}
.hdr-notif-item-time { font-size: 0.66rem; color: var(--t-text3); margin-top: 4px; }

.hdr-notif-footer {
  border-top: 1px solid var(--t-border);
  padding: 0.6rem 1rem;
  background: var(--t-acc-alpha-sm);
}
.hdr-notif-view-all {
  display: flex; align-items: center; justify-content: space-between;
  font-size: 0.75rem; font-weight: 600;
  color: var(--t-accent); text-decoration: none; transition: opacity 0.15s;
}
.hdr-notif-view-all:hover { opacity: 0.8; }
.hdr-notif-footer-badge {
  font-size: 0.65rem; font-weight: 700; padding: 2px 7px; border-radius: 99px;
  background: var(--t-accent); color: white;
}

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
.hdr-sub-chip--active {
  background: color-mix(in srgb, #10b981 15%, transparent);
  border-color: color-mix(in srgb, #10b981 35%, transparent);
  color: #10b981;
}
.hdr-sub-chip--active:hover { background: color-mix(in srgb, #10b981 22%, transparent); }
.hdr-sub-chip--expired {
  background: color-mix(in srgb, #ef4444 15%, transparent);
  border-color: color-mix(in srgb, #ef4444 35%, transparent);
  color: #ef4444;
  animation: chipPulse 2s ease-in-out infinite;
}
.hdr-sub-chip--expired:hover { background: color-mix(in srgb, #ef4444 22%, transparent); }
@keyframes chipPulse { 0%,100%{opacity:1} 50%{opacity:0.7} }

/* Subscription modal */
.sub-modal { display: flex; flex-direction: column; gap: 1rem; }
.sub-status {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.65rem 0.9rem; border-radius: 12px;
  font-size: 0.82rem; font-weight: 600;
}
.sub-status.trial { background: color-mix(in srgb, var(--t-gold) 12%, transparent); color: var(--t-gold); }
.sub-status.expired { background: color-mix(in srgb, #ef4444 10%, transparent); color: #ef4444; }
.sub-status.active { background: color-mix(in srgb, #10b981 10%, transparent); color: #10b981; }

.sub-loading, .sub-empty { text-align: center; padding: 1.5rem; font-size: 0.85rem; color: var(--t-text3); }

.sub-plan-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 0.75rem; }
.sub-plan-card {
  position: relative; padding: 0.9rem; border-radius: 14px;
  border: 1.5px solid var(--t-border); background: var(--t-bg);
  display: flex; flex-direction: column; gap: 0.5rem;
}
.sub-plan-card.current { border-color: var(--t-accent); background: var(--t-acc-alpha-sm); }
.sub-plan-badge {
  position: absolute; top: -9px; right: 10px;
  font-size: 0.6rem; font-weight: 700; padding: 2px 8px; border-radius: 99px;
  background: var(--t-accent); color: #fff; text-transform: uppercase; letter-spacing: 0.03em;
}
.sub-plan-name { font-weight: 700; font-size: 0.88rem; color: var(--t-text1); }
.sub-plan-price { font-size: 1.05rem; font-weight: 800; color: var(--t-text1); }
.sub-plan-price small { font-size: 0.68rem; font-weight: 600; color: var(--t-text3); }
.sub-plan-feats { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 0.3rem; }
.sub-plan-feats li { display: flex; align-items: flex-start; gap: 0.35rem; font-size: 0.72rem; color: var(--t-text2); line-height: 1.4; }
.sub-plan-feats li :deep(svg) { color: #10b981; flex-shrink: 0; margin-top: 0.1rem; }

.sub-actions { display: flex; flex-direction: column; gap: 0.6rem; }
.sub-whatsapp {
  display: flex; align-items: center; justify-content: center; gap: 0.4rem;
  font-size: 0.78rem; font-weight: 600; color: var(--t-text3); text-decoration: none; padding: 0.3rem;
}
.sub-whatsapp:hover { color: var(--t-accent); }

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
