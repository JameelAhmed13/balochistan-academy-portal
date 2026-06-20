<template>
  <header class="atn">
    <div class="atn-inner">
      <!-- Brand -->
      <RouterLink to="/app/admin" class="atn-brand">
        <img src="@/assets/logo.png" alt="" class="atn-logo" />
        <span class="atn-brand-text">
          <span class="atn-brand-name">Balochistan Academy</span>
          <span class="atn-brand-sub">Admin Panel</span>
        </span>
      </RouterLink>

      <!-- Desktop menus -->
      <nav class="atn-menus" aria-label="Admin sections">
        <div v-for="m in menus" :key="m.key" class="atn-menu"
          @mouseenter="open = m.key" @mouseleave="open = null">
          <button type="button" class="atn-menu-btn" :class="{ 'is-active': isMenuActive(m) }"
            :aria-expanded="open === m.key" @click="open = open === m.key ? null : m.key">
            <component :is="m.icon" class="w-4 h-4" />
            {{ m.label }}
            <ChevronDown class="atn-caret" :class="{ 'is-open': open === m.key }" />
          </button>

          <!-- Mega-menu panel -->
          <Transition name="atn-fade">
            <div v-if="open === m.key" class="atn-panel">
              <div class="atn-panel-title">{{ m.label }}</div>
              <div class="atn-panel-grid">
                <RouterLink v-for="it in m.items" :key="it.path" :to="it.path" class="atn-item"
                  :class="{ 'is-current': isExact(it.path) }" @click="open = null">
                  <span class="atn-item-ic"><component :is="it.icon || m.icon" class="w-4 h-4" /></span>
                  <span class="atn-item-text">
                    <span class="atn-item-label">{{ it.label }}</span>
                    <span class="atn-item-desc">{{ it.desc }}</span>
                  </span>
                </RouterLink>
              </div>
            </div>
          </Transition>
        </div>
      </nav>

      <!-- Right cluster -->
      <div class="atn-right">
        <RouterLink to="/app" class="atn-site" title="Back to student site">
          <ExternalLink class="w-4 h-4" /> <span class="atn-site-lbl">View Site</span>
        </RouterLink>
        <RouterLink to="/app/admin/notifications" class="atn-icon-btn" title="Notifications" aria-label="Notifications">
          <Bell class="w-4 h-4" />
        </RouterLink>
        <button ref="userBtnEl" type="button" class="atn-user" @click="userOpen = !userOpen" aria-label="Account menu">
          <span class="atn-avatar">{{ initial }}</span>
        </button>
        <Transition name="atn-fade">
          <div v-if="userOpen" ref="userMenuEl" class="atn-user-menu">
            <div class="atn-user-hd">
              <div class="atn-user-name">{{ auth.user?.name || 'Admin' }}</div>
              <div class="atn-user-role">Administrator</div>
            </div>
            <button type="button" class="atn-user-item" @click="theme.toggle?.()">
              <component :is="isDark ? Sun : Moon" class="w-4 h-4" /> {{ isDark ? 'Light mode' : 'Dark mode' }}
            </button>
            <RouterLink to="/app" class="atn-user-item" @click="userOpen = false"><LayoutDashboard class="w-4 h-4" /> Student dashboard</RouterLink>
            <button type="button" class="atn-user-item atn-signout" @click="signOut"><LogOut class="w-4 h-4" /> Sign out</button>
          </div>
        </Transition>

        <!-- Mobile hamburger -->
        <button type="button" class="atn-burger" @click="mobileOpen = true" aria-label="Open admin menu">
          <Menu class="w-5 h-5" />
        </button>
      </div>
    </div>

    <!-- Mobile drawer -->
    <Transition name="atn-slide">
      <div v-if="mobileOpen" class="atn-mobile">
        <div class="atn-mobile-hd">
          <span class="atn-brand-name">Admin Panel</span>
          <button type="button" class="atn-icon-btn" @click="mobileOpen = false" aria-label="Close menu"><X class="w-5 h-5" /></button>
        </div>
        <details v-for="m in menus" :key="m.key" class="atn-acc" open>
          <summary class="atn-acc-sum"><component :is="m.icon" class="w-4 h-4" /> {{ m.label }}</summary>
          <RouterLink v-for="it in m.items" :key="it.path" :to="it.path" class="atn-acc-item"
            :class="{ 'is-current': isExact(it.path) }" @click="mobileOpen = false">{{ it.label }}</RouterLink>
        </details>
        <RouterLink to="/app" class="atn-acc-item" @click="mobileOpen = false">↗ View student site</RouterLink>
        <button type="button" class="atn-acc-item atn-signout" @click="signOut">Sign out</button>
      </div>
    </Transition>
    <Transition name="atn-fade">
      <div v-if="mobileOpen" class="atn-mobile-scrim" @click="mobileOpen = false" />
    </Transition>
  </header>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { onClickOutside } from '@vueuse/core'
import {
  LayoutDashboard, BookOpen, ClipboardList, Users, BarChart2, Settings, Bell,
  Menu, X, ChevronDown, ExternalLink, LogOut, Sun, Moon,
  FileText, FolderTree, GraduationCap, Coins, MessageSquareWarning, BellRing, Layers, Globe,
} from '@lucide/vue'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const theme = useThemeStore()

const open = ref(null)
const userOpen = ref(false)
const mobileOpen = ref(false)
const userBtnEl = ref(null)
const userMenuEl = ref(null)
onClickOutside(userMenuEl, () => { userOpen.value = false }, { ignore: [userBtnEl] })

const isDark = computed(() => theme.isDark)
const initial = computed(() => (auth.user?.name || 'A').charAt(0).toUpperCase())

const menus = [
  { key: 'curriculum', label: 'Curriculum', icon: BookOpen, items: [
    { label: 'Dashboard',          path: '/app/admin',          icon: LayoutDashboard, desc: 'Overview & quick stats' },
    { label: 'Grade Bands',        path: '/app/admin/bands',    icon: Layers,          desc: 'Primary, Middle, Secondary…' },
    { label: 'Mediums',            path: '/app/admin/mediums',  icon: Globe,           desc: 'English Medium, اردو میڈیم…' },
    { label: 'Grades',             path: '/app/admin/grades',   icon: GraduationCap,   desc: 'Define grades & their subjects' },
    { label: 'Syllabus',           path: '/app/admin/syllabus', icon: FolderTree,      desc: 'Units, topics & learning objectives' },
    { label: 'AI Tutors',          path: '/app/admin/tutors',   icon: Users,           desc: 'Persona tutors per subject/grade' },
    { label: 'Subjects & Content', path: '/app/admin/content',  icon: FolderTree,      desc: 'Subjects, books, units & topics' },
  ] },
  { key: 'assessments', label: 'Assessments', icon: ClipboardList, items: [
    { label: 'Question Bank',  path: '/app/admin/questions', icon: FileText,      desc: 'Manage MCQ & subjective items' },
    { label: 'Tests & Papers', path: '/app/admin/tests',     icon: ClipboardList, desc: 'Create, assign & track tests' },
  ] },
  { key: 'people', label: 'People', icon: Users, items: [
    { label: 'Users & Roles',   path: '/app/admin/users', icon: GraduationCap, desc: 'Students, teachers & admins' },
    { label: 'Coins & Rewards', path: '/app/admin/coins', icon: Coins,         desc: 'Gamification & coin economy' },
  ] },
  { key: 'insights', label: 'Insights', icon: BarChart2, items: [
    { label: 'Analytics',  path: '/app/admin/analytics',  icon: BarChart2,            desc: 'Platform performance & trends' },
    { label: 'Complaints', path: '/app/admin/complaints', icon: MessageSquareWarning, desc: 'Tickets & resolutions' },
  ] },
  { key: 'system', label: 'System', icon: Settings, items: [
    { label: 'Settings',      path: '/app/admin/settings',      icon: Settings, desc: 'Platform configuration' },
    { label: 'Notifications', path: '/app/admin/notifications', icon: BellRing, desc: 'Broadcasts & alerts' },
  ] },
]

function isExact(path) { return route.path === path }
function isMenuActive(m) { return m.items.some((it) => route.path === it.path || (it.path !== '/app/admin' && route.path.startsWith(it.path))) }

function signOut() { auth.logout(); router.push('/login') }
</script>

<style scoped>
.atn {
  position: sticky; top: 0; z-index: 50;
  background: var(--t-header); border-bottom: 1px solid var(--t-header-border);
  box-shadow: var(--t-header-shadow); backdrop-filter: var(--t-blur);
}
.atn-inner { display: flex; align-items: center; gap: 0.5rem; height: 64px; padding: 0 1rem; max-width: 1400px; margin: 0 auto; }

.atn-brand { display: flex; align-items: center; gap: 0.6rem; text-decoration: none; flex-shrink: 0; margin-right: 0.5rem; }
.atn-logo { width: 36px; height: 36px; object-fit: contain; border-radius: 8px; }
.atn-brand-text { display: flex; flex-direction: column; line-height: 1.1; }
.atn-brand-name { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 0.9rem; color: var(--t-text1); }
.atn-brand-sub { font-size: 0.62rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: var(--t-accent); }

.atn-menus { display: flex; align-items: center; gap: 0.15rem; flex: 1; }
.atn-menu { position: relative; }
.atn-menu-btn {
  display: inline-flex; align-items: center; gap: 0.4rem; padding: 0.5rem 0.7rem; border-radius: 10px;
  background: none; border: none; cursor: pointer; font-size: 0.82rem; font-weight: 600; color: var(--t-text2);
  transition: all 0.15s; white-space: nowrap;
}
.atn-menu-btn:hover { background: var(--t-hover); color: var(--t-text1); }
.atn-menu-btn.is-active { color: var(--t-accent); }
.atn-menu-btn.is-active::after { content: ''; position: absolute; left: 0.7rem; right: 0.7rem; bottom: -1px; height: 2px; background: var(--t-accent); border-radius: 2px; }
.atn-caret { width: 13px; height: 13px; opacity: 0.6; transition: transform 0.18s; }
.atn-caret.is-open { transform: rotate(180deg); }

.atn-panel {
  position: absolute; top: calc(100% + 6px); left: 0; min-width: 300px;
  background: var(--t-dropdown-bg); border: 1px solid var(--t-border); border-radius: 14px;
  box-shadow: var(--t-shadow-md); padding: 0.85rem; z-index: 60;
}
.atn-panel-title { font-size: 0.66rem; font-weight: 800; letter-spacing: 0.1em; text-transform: uppercase; color: var(--t-text3); padding: 0 0.4rem 0.5rem; }
.atn-panel-grid { display: grid; grid-template-columns: 1fr; gap: 0.2rem; }
.atn-item { display: flex; align-items: flex-start; gap: 0.6rem; padding: 0.55rem 0.5rem; border-radius: 10px; text-decoration: none; transition: background 0.12s; }
.atn-item:hover { background: var(--t-dropdown-item); }
.atn-item.is-current { background: var(--t-acc-alpha-sm); }
.atn-item-ic { width: 30px; height: 30px; border-radius: 8px; flex-shrink: 0; display: flex; align-items: center; justify-content: center; background: color-mix(in srgb, var(--t-accent) 12%, transparent); color: var(--t-accent); }
.atn-item-text { display: flex; flex-direction: column; min-width: 0; }
.atn-item-label { font-size: 0.84rem; font-weight: 600; color: var(--t-text1); }
.atn-item.is-current .atn-item-label { color: var(--t-accent); }
.atn-item-desc { font-size: 0.7rem; color: var(--t-text3); }

.atn-right { display: flex; align-items: center; gap: 0.4rem; flex-shrink: 0; }
.atn-site { display: inline-flex; align-items: center; gap: 0.35rem; padding: 0.4rem 0.7rem; border-radius: 10px; border: 1px solid var(--t-border); color: var(--t-text2); font-size: 0.76rem; font-weight: 600; text-decoration: none; transition: all 0.15s; }
.atn-site:hover { border-color: var(--t-accent); color: var(--t-accent); }
.atn-icon-btn { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: var(--t-text2); background: none; border: 1px solid transparent; cursor: pointer; text-decoration: none; transition: all 0.15s; }
.atn-icon-btn:hover { background: var(--t-hover); color: var(--t-text1); }
.atn-user { background: none; border: none; cursor: pointer; padding: 0; }
.atn-avatar { width: 34px; height: 34px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 0.8rem; color: #fff; background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); }
.atn-user-menu { position: absolute; top: 58px; right: 1rem; width: 220px; background: var(--t-dropdown-bg); border: 1px solid var(--t-border); border-radius: 14px; box-shadow: var(--t-shadow-md); padding: 0.4rem; z-index: 60; }
.atn-user-hd { padding: 0.6rem 0.7rem; border-bottom: 1px solid var(--t-border); margin-bottom: 0.3rem; }
.atn-user-name { font-size: 0.85rem; font-weight: 700; color: var(--t-text1); }
.atn-user-role { font-size: 0.7rem; color: var(--t-accent); }
.atn-user-item { display: flex; align-items: center; gap: 0.55rem; width: 100%; padding: 0.55rem 0.7rem; border-radius: 9px; background: none; border: none; cursor: pointer; font-size: 0.82rem; color: var(--t-text2); text-decoration: none; text-align: left; }
.atn-user-item:hover { background: var(--t-dropdown-item); color: var(--t-text1); }
.atn-signout { color: #f87171; }
.atn-signout:hover { background: rgba(248,113,113,0.1); color: #f87171; }

.atn-burger { display: none; width: 38px; height: 38px; border-radius: 10px; align-items: center; justify-content: center; background: none; border: 1px solid var(--t-border); color: var(--t-text2); cursor: pointer; }

/* Mobile drawer */
.atn-mobile { position: fixed; top: 0; right: 0; bottom: 0; width: 280px; max-width: 85vw; z-index: 70; background: var(--t-sidebar); border-left: 1px solid var(--t-sidebar-border); backdrop-filter: var(--t-blur); overflow-y: auto; padding: 0.75rem; }
.atn-mobile-hd { display: flex; align-items: center; justify-content: space-between; padding: 0.5rem 0.5rem 0.85rem; border-bottom: 1px solid var(--t-border); margin-bottom: 0.5rem; }
.atn-mobile-scrim { position: fixed; inset: 0; z-index: 65; background: rgba(0,0,0,0.5); backdrop-filter: blur(2px); }
.atn-acc { border-bottom: 1px solid var(--t-border); }
.atn-acc-sum { display: flex; align-items: center; gap: 0.5rem; padding: 0.7rem 0.5rem; font-size: 0.82rem; font-weight: 700; color: var(--t-text1); cursor: pointer; list-style: none; }
.atn-acc-sum::-webkit-details-marker { display: none; }
.atn-acc-item { display: block; padding: 0.55rem 0.5rem 0.55rem 2rem; font-size: 0.8rem; color: var(--t-text2); text-decoration: none; border-radius: 8px; background: none; border: none; width: 100%; text-align: left; cursor: pointer; }
.atn-acc-item:hover { background: var(--t-hover); color: var(--t-text1); }
.atn-acc-item.is-current { color: var(--t-accent); }

.atn-fade-enter-active, .atn-fade-leave-active { transition: opacity 0.15s, transform 0.15s; }
.atn-fade-enter-from, .atn-fade-leave-to { opacity: 0; transform: translateY(-4px); }
.atn-slide-enter-active, .atn-slide-leave-active { transition: transform 0.25s ease; }
.atn-slide-enter-from, .atn-slide-leave-to { transform: translateX(100%); }

@media (max-width: 1023px) {
  .atn-menus, .atn-site { display: none; }
  .atn-burger { display: flex; }
}
</style>
