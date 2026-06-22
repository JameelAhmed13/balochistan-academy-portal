<template>
  <!-- Mobile overlay -->
  <Transition name="overlay">
    <div v-if="!collapsed && isMobile" class="mob-overlay" @click="$emit('toggle')" />
  </Transition>

  <aside :class="['cosmic-sidebar', isMobile ? 'is-fixed' : 'is-relative',
    collapsed && isMobile ? 'sidebar-hidden' : '',
    collapsed && !isMobile ? 'sidebar-slim' : 'sidebar-full']">

    <!-- Aurora glow layers (dark-only via CSS vars) -->
    <div class="sb-aurora sb-au1" />
    <div class="sb-aurora sb-au2" />

    <!-- Logo -->
    <div class="sb-logo" :class="{ slim: collapsed && !isMobile }">
      <div class="sb-logo-mark">
        <img src="@/assets/logo.png" alt="Balochistan Academy Portal logo" class="sb-logo-img" />
      </div>
      <Transition name="fade-label">
        <div v-if="!collapsed || isMobile" class="sb-logo-text">
          <div class="sb-logo-name">Balochistan Academy Portal</div>
          <div class="sb-logo-sub">AI Exam Prep</div>
        </div>
      </Transition>
    </div>

    <!-- Institute badge -->
    <Transition name="fade-label">
      <div v-if="!collapsed || isMobile" class="sb-inst">
        <div class="sb-inst-icon">🏫</div>
        <div class="sb-inst-text">
          <div class="sb-inst-name">New Century Educational System</div>
          <div class="sb-inst-grade">Grade 9 · Balochistan Board</div>
        </div>
      </div>
    </Transition>

    <!-- Nav section label -->
    <Transition name="fade-label">
      <div v-if="!collapsed || isMobile" class="sb-nav-label">NAVIGATION</div>
    </Transition>

    <!-- Nav items -->
    <nav class="sb-nav">
      <NavItem v-for="item in navItems" :key="item.name" :item="item" :collapsed="collapsed && !isMobile" />
    </nav>

    <!-- Bottom -->
    <div class="sb-bottom">
      <button v-if="!isMobile" type="button" @click="$emit('toggle')" class="sb-collapse-btn" :title="collapsed ? 'Expand sidebar' : 'Collapse sidebar'" :aria-label="collapsed ? 'Expand sidebar' : 'Collapse sidebar'" :aria-expanded="!collapsed">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"
          :style="{ transform: collapsed ? 'rotate(180deg)' : 'none', transition: 'transform 0.3s' }">
          <path d="M15 18l-6-6 6-6" />
        </svg>
        <Transition name="fade-label">
          <span v-if="!collapsed">Collapse</span>
        </Transition>
      </button>
      <div class="sb-divider" />
      <button type="button" @click="logout" class="sb-signout" :class="{ 'justify-center': collapsed && !isMobile }" aria-label="Sign out of Balochistan Academy Portal">
        <LogOut class="sb-signout-icon" />
        <Transition name="fade-label">
          <span v-if="!collapsed || isMobile">Sign Out</span>
        </Transition>
      </button>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useWindowSize } from '@vueuse/core'
import { LogOut, Home, Bot, BookOpen, Calendar, Trophy, Coins, HelpCircle, Layers, Medal, Sparkles, FileBarChart } from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'
import NavItem from './NavItem.vue'

defineProps({ collapsed: Boolean })
defineEmits(['toggle'])

const { width } = useWindowSize()
const isMobile = computed(() => width.value < 1024)
const auth = useAuthStore()
const router = useRouter()

const navItems = [
  { name: 'Home',              path: '/app',               icon: Home },
  { name: 'AI Tutor',          path: '/app/ai-tutor',      icon: Bot },
  { name: 'Saathi AI',         path: '/app/saathi',        icon: Sparkles },
  { name: 'Preparation',       path: '/app/preparation',   icon: BookOpen },
  { name: 'My Syllabus',       path: '/app/preparation/syllabus', icon: BookOpen },
  { name: 'Self Test',         path: '/app/self-test',     icon: Layers },
  { name: 'Daily Tests',       path: '/app/daily-tests',   icon: Calendar },
  { name: 'Competition Tests', path: '/app/competition',   icon: Trophy },
  { name: 'Leaderboard',       path: '/app/competition/leaderboard', icon: Medal },
  { name: 'My Report',         path: '/app/report',        icon: FileBarChart },
  { name: 'Coins Earned',      path: '/app/coins',         icon: Coins },
  { name: 'How It Works',      path: null,                 icon: HelpCircle, external: 'https://youtube.com' },
]

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<style scoped>
.mob-overlay {
  position: fixed; inset: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(4px);
  z-index: 30;
}
.cosmic-sidebar {
  display: flex; flex-direction: column;
  background: var(--t-sidebar);
  border-right: 1px solid var(--t-sidebar-border);
  box-shadow: var(--t-sidebar-shadow);
  backdrop-filter: var(--t-blur);
  z-index: 40;
  overflow: hidden;
  transition: width 0.3s cubic-bezier(.4,0,.2,1), background 0.25s, border-color 0.25s, box-shadow 0.25s;
  height: 100%;
  flex-shrink: 0;
}
.cosmic-sidebar.is-fixed { position: fixed; inset-y: 0; left: 0; }
.cosmic-sidebar.is-relative { position: relative; }
.sidebar-hidden { transform: translateX(-100%); }
.sidebar-full { width: 256px; }
.sidebar-slim { width: 64px; }

/* Aurora — visible only in dark (values are 'transparent' in light) */
.sb-aurora {
  position: absolute; border-radius: 50%; filter: blur(60px); pointer-events: none; z-index: 0;
}
.sb-au1 {
  width: 220px; height: 220px; top: -60px; left: -60px;
  background: radial-gradient(circle, var(--t-aurora1), transparent 70%);
}
.sb-au2 {
  width: 180px; height: 180px; bottom: 60px; right: -60px;
  background: radial-gradient(circle, var(--t-aurora3), transparent 70%);
}

/* Logo */
.sb-logo {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 1.25rem 1rem;
  border-bottom: 1px solid var(--t-sidebar-border);
  position: relative; z-index: 1; flex-shrink: 0;
  transition: border-color 0.25s;
}
.sb-logo.slim { justify-content: center; }
.sb-logo-mark {
  width: 46px; height: 46px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
}
.sb-logo-img {
  width: 46px; height: 46px;
  object-fit: contain;
  border-radius: 10px;
}
.sb-logo-name { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 0.9rem; color: var(--t-text1); line-height: 1.2; }
.sb-logo-sub { font-size: 0.68rem; color: var(--t-text3); line-height: 1.2; }

/* Institute badge */
.sb-inst {
  display: flex; align-items: center; gap: 0.6rem;
  margin: 0.75rem 0.75rem 0;
  padding: 0.6rem 0.75rem; border-radius: 14px;
  background: var(--t-hover); border: 1px solid var(--t-border);
  position: relative; z-index: 1; flex-shrink: 0;
  transition: background 0.25s, border-color 0.25s;
}
.sb-inst-icon { font-size: 1.1rem; flex-shrink: 0; }
.sb-inst-name { font-size: 0.68rem; color: var(--t-text3); line-height: 1.3; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.sb-inst-grade { font-size: 0.68rem; color: var(--t-accent); font-weight: 500; }

/* Nav label */
.sb-nav-label {
  font-size: 0.6rem; font-weight: 700; letter-spacing: 0.12em;
  color: var(--t-text3); padding: 1.25rem 1rem 0.4rem;
  position: relative; z-index: 1; flex-shrink: 0;
}

/* Nav */
.sb-nav {
  flex: 1; padding: 0.25rem 0.5rem; overflow-y: auto;
  position: relative; z-index: 1;
  scrollbar-width: thin; scrollbar-color: var(--t-border) transparent;
}

/* Bottom */
.sb-bottom {
  padding: 0.75rem; border-top: 1px solid var(--t-border);
  position: relative; z-index: 1; flex-shrink: 0;
  display: flex; flex-direction: column; gap: 0.4rem;
  transition: border-color 0.25s;
}
.sb-collapse-btn {
  display: flex; align-items: center; gap: 0.65rem; width: 100%;
  padding: 0.55rem 0.75rem; border-radius: 12px;
  background: none; border: none; color: var(--t-text3); font-size: 0.78rem; font-weight: 500;
  cursor: pointer; transition: all 0.15s; white-space: nowrap;
}
.sb-collapse-btn:hover { background: var(--t-hover); color: var(--t-text2); }
.sb-divider { height: 1px; background: var(--t-border); }
.sb-signout {
  display: flex; align-items: center; gap: 0.65rem; width: 100%;
  padding: 0.6rem 0.75rem; border-radius: 12px;
  background: none; border: none; color: #f87171; font-size: 0.82rem; font-weight: 500;
  cursor: pointer; transition: all 0.15s; white-space: nowrap;
}
.sb-signout:hover { background: rgba(248, 113, 113, 0.1); }
.sb-signout-icon { width: 16px; height: 16px; flex-shrink: 0; }

/* Transitions */
.fade-label-enter-active, .fade-label-leave-active { transition: opacity 0.2s, max-width 0.3s; }
.fade-label-enter-from, .fade-label-leave-to { opacity: 0; max-width: 0; overflow: hidden; }
.fade-label-enter-to, .fade-label-leave-from { max-width: 200px; }
.overlay-enter-active, .overlay-leave-active { transition: opacity 0.3s; }
.overlay-enter-from, .overlay-leave-to { opacity: 0; }
</style>
