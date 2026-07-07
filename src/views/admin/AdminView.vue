<template>
  <div class="animate-fade-in space-y-6">

    <!-- Glass Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">🛡️</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">Admin Panel</div>
        <div class="ag-banner-sub">Manage platform content, users, and settings</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">👥 {{ fmt(dashboard.totalUsers) }} Users</span>
        <span class="ag-banner-stat">❓ {{ fmt(dashboard.totalQuestions) }} Questions</span>
        <span class="ag-banner-stat" style="color:var(--t-gold);border-color:color-mix(in srgb,var(--t-gold) 25%,transparent);background:color-mix(in srgb,var(--t-gold) 10%,transparent);">● Live</span>
      </div>
    </div>

    <!-- KPI Row -->
    <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
      <div
        v-for="kpi in kpiCards"
        :key="kpi.label"
        class="ag-grid-card kpi-card"
        :style="{ '--kc': kpi.color }"
      >
        <div class="kpi-icon-wrap">
          <span class="kpi-icon">{{ kpi.icon }}</span>
        </div>
        <div class="kpi-value">{{ kpi.value }}</div>
        <div class="kpi-label">{{ kpi.label }}</div>
        <div class="kpi-glow" />
      </div>
    </div>

    <!-- Two-column layout: Module Grid + Activity Feed -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">

      <!-- Module Grid (spans 2 of 3 cols on xl) -->
      <div class="xl:col-span-2 space-y-3">
        <div class="section-row-label">
          <LayoutGrid class="w-3.5 h-3.5" /> Admin Modules
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <RouterLink
            v-for="mod in adminModules"
            :key="mod.title"
            :to="mod.path"
            class="ag-grid-card module-card"
          >
            <div class="module-icon-wrap" :style="{ background: mod.iconBg }">
              <span style="font-size:1.35rem;">{{ mod.icon }}</span>
            </div>
            <div class="module-body">
              <div class="module-title">{{ mod.title }}</div>
              <div class="module-desc">{{ mod.desc }}</div>
              <span class="module-badge">{{ mod.count }}</span>
            </div>
            <ChevronRight class="module-arrow w-4 h-4" />
          </RouterLink>
        </div>
      </div>

      <!-- Activity Feed -->
      <div class="xl:col-span-1">
        <div class="section-row-label mb-3">
          <Activity class="w-3.5 h-3.5" /> Recent Activity
        </div>
        <div class="ag-card" style="height:calc(100% - 1.75rem);">
          <div v-if="!recentActivity.length" class="activity-empty">No recent activity yet.</div>
          <div
            v-for="(act, i) in recentActivity"
            :key="i"
            class="activity-item"
            :style="{ '--ac': act.color }"
          >
            <div class="activity-dot">{{ act.icon }}</div>
            <div class="activity-content">
              <div class="activity-text">{{ act.text }}</div>
              <div class="activity-time">{{ timeAgo(act.timestamp) }}</div>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- Quick Actions -->
    <div>
      <div class="section-row-label mb-3">
        <Zap class="w-3.5 h-3.5" /> Quick Actions
      </div>
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <button
          v-for="qa in quickActions"
          :key="qa.label"
          type="button"
          class="ag-grid-card quick-action-card"
          :style="{ '--qa': qa.color }"
          @click="qa.action"
        >
          <div class="qa-icon">{{ qa.icon }}</div>
          <div>
            <div class="qa-title">{{ qa.label }}</div>
            <div class="qa-desc">{{ qa.desc }}</div>
          </div>
          <span class="qa-btn">
            <ChevronRight class="w-4 h-4" />
          </span>
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ChevronRight, LayoutGrid, Zap, Activity } from '@lucide/vue'
import api from '@/services/api'

const router = useRouter()

const fmt = (n) => (n == null ? '—' : n >= 1000 ? (n / 1000).toFixed(n >= 10000 ? 0 : 1) + 'K' : String(n))

// Raw aggregated payload from GET /admin/dashboard — everything below derives from this.
const dashboard = ref({
  totalUsers: null, totalStudents: null, totalQuestions: null, totalTests: null,
  totalAttempts: null, totalCoins: null, byGrade: [],
  moduleCounts: {}, recentActivity: [],
})
const loading = ref(false)

async function loadDashboard() {
  loading.value = true
  try {
    const { data } = await api.get('/admin/dashboard')
    dashboard.value = data
  } catch { /* leave dashes if backend offline */ }
  finally { loading.value = false }
}
onMounted(loadDashboard)

const kpiCards = computed(() => [
  { label: 'Students',      value: fmt(dashboard.value.totalStudents),     icon: '👥', color: '#8b5cf6' },
  { label: 'Tests Taken',   value: fmt(dashboard.value.totalAttempts),     icon: '📝', color: '#3b82f6' },
  { label: 'Coins Issued',  value: fmt(dashboard.value.totalCoins),        icon: '🪙', color: '#10b981' },
  { label: 'Questions',     value: fmt(dashboard.value.totalQuestions),    icon: '❓', color: '#a855f7' },
  { label: 'Tests Created', value: fmt(dashboard.value.totalTests),        icon: '📅', color: '#f59e0b' },
  { label: 'Grades',        value: fmt(dashboard.value.byGrade?.length),   icon: '🎓', color: '#14b8a6' },
])

const adminModules = computed(() => {
  const mc = dashboard.value.moduleCounts || {}
  return [
    { title: 'Content',       desc: 'Subjects, books, units, topics and resources',   icon: '📚', count: `${fmt(mc.subjects)} subjects`,       path: '/app/admin/content',             iconBg: 'rgba(59,130,246,0.12)'  },
    { title: 'Questions',     desc: 'Objective & subjective question bank',            icon: '❓', count: `${fmt(dashboard.value.totalQuestions)} questions`, path: '/app/admin/questions', iconBg: 'rgba(139,92,246,0.12)'  },
    { title: 'Tests',         desc: 'Create and schedule platform tests',              icon: '📝', count: `${fmt(mc.activeTests)} active`,      path: '/app/admin/tests',               iconBg: 'rgba(16,185,129,0.12)'  },
    { title: 'Users',         desc: 'Students, teachers, parents and admins',          icon: '👥', count: `${fmt(mc.users)} users`,             path: '/app/admin/users',               iconBg: 'rgba(236,72,153,0.12)'  },
    { title: 'Coins',         desc: 'Cash subscriptions and coin economy settings',    icon: '🪙', count: `${fmt(mc.pendingCoins)} pending`,    path: '/app/admin/coins',               iconBg: 'rgba(245,158,11,0.12)'  },
    { title: 'Analytics',     desc: 'Platform-wide performance dashboards',            icon: '📊', count: 'Live data',                          path: '/app/admin/analytics',           iconBg: 'rgba(20,184,166,0.12)'  },
    { title: 'Notifications', desc: 'Send announcements and alerts to students',       icon: '🔔', count: `${fmt(mc.notificationsSent)} sent`,  path: '/app/admin/notifications',       iconBg: 'rgba(251,191,36,0.12)'  },
    { title: 'My Inbox',      desc: 'View notifications received by your account',     icon: '📥', count: 'Inbox',                              path: '/app/admin/notifications/inbox', iconBg: 'rgba(99,102,241,0.12)'  },
    { title: 'Settings',      desc: 'Platform configuration and AI API keys',          icon: '⚙️', count: `${fmt(mc.settings)} settings`,       path: '/app/admin/settings',            iconBg: 'rgba(100,116,139,0.12)' },
    { title: 'Complaints',    desc: 'Student feedback and complaint management',       icon: '💬', count: `${fmt(mc.openComplaints)} open`,     path: '/app/admin/complaints',           iconBg: 'rgba(239,68,68,0.12)'   },
    { title: 'Audit Log',     desc: 'Who did what, to what, when, and from where',     icon: '🛡️', count: `${fmt(mc.auditLogCount)} entries`,  path: '/app/admin/audit-log',           iconBg: 'rgba(109,84,232,0.12)'  },
  ]
})

const recentActivity = computed(() => dashboard.value.recentActivity || [])

function timeAgo(iso) {
  if (!iso) return ''
  const diffSec = Math.max(0, (Date.now() - new Date(iso).getTime()) / 1000)
  if (diffSec < 60) return 'Just now'
  if (diffSec < 3600) return `${Math.floor(diffSec / 60)} min ago`
  if (diffSec < 86400) return `${Math.floor(diffSec / 3600)} hr ago`
  if (diffSec < 172800) return 'Yesterday'
  return new Date(iso).toLocaleDateString('en-PK', { timeZone: 'Asia/Karachi', day: 'numeric', month: 'short' })
}

function exportData() {
  const mc = dashboard.value.moduleCounts || {}
  const rows = [
    ['Metric', 'Value'],
    ['Total Users', dashboard.value.totalUsers],
    ['Total Students', dashboard.value.totalStudents],
    ['Total Questions', dashboard.value.totalQuestions],
    ['Total Tests', dashboard.value.totalTests],
    ['Active Tests', mc.activeTests],
    ['Test Attempts', dashboard.value.totalAttempts],
    ['Coins Issued', dashboard.value.totalCoins],
    ['Subjects', mc.subjects],
    ['Pending Cash Subscriptions', mc.pendingCoins],
    ['Notifications Sent', mc.notificationsSent],
    ['Settings Count', mc.settings],
    ['Open Complaints', mc.openComplaints],
  ]
  const csv = rows.map(r => r.join(',')).join('\n')
  const blob = new Blob([csv], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `admin-dashboard-${new Date().toISOString().slice(0, 10)}.csv`
  a.click()
  URL.revokeObjectURL(url)
}

const quickActions = [
  { label: 'Send Notification', desc: 'Broadcast to all users',    icon: '🔔', color: '#3b82f6', action: () => router.push('/app/admin/notifications') },
  { label: 'Export Data',       desc: 'Download platform reports', icon: '📤', color: '#10b981', action: exportData },
  { label: 'View Settings',     desc: 'Platform configuration',    icon: '⚙️', color: '#8b5cf6', action: () => router.push('/app/admin/settings') },
]
</script>

<style scoped>
.section-row-label {
  display: flex; align-items: center; gap: 0.4rem;
  color: var(--t-text3); font-size: 0.7rem; font-weight: 700;
  letter-spacing: 0.08em; text-transform: uppercase;
}

/* ── KPI Cards ── */
.kpi-card {
  position: relative; overflow: hidden; cursor: default;
  display: flex; flex-direction: column; gap: 0.35rem;
}
.kpi-icon-wrap {
  width: 36px; height: 36px; border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
  background: color-mix(in srgb, var(--kc) 14%, transparent);
  border: 1px solid color-mix(in srgb, var(--kc) 22%, transparent);
  margin-bottom: 0.15rem;
}
.kpi-icon { font-size: 1.05rem; }
.kpi-value {
  font-family: 'Syne', sans-serif;
  font-size: 1.45rem; font-weight: 800; line-height: 1;
  color: var(--t-text1);
}
.kpi-label {
  font-size: 0.68rem; font-weight: 600; color: var(--t-text3);
  text-transform: uppercase; letter-spacing: 0.05em;
}
.kpi-glow {
  position: absolute; bottom: -18px; right: -18px;
  width: 68px; height: 68px; border-radius: 50%;
  background: radial-gradient(circle, color-mix(in srgb, var(--kc) 28%, transparent), transparent 70%);
  pointer-events: none;
}

/* ── Module Cards ── */
.module-card {
  display: flex; align-items: center; gap: 1rem;
  cursor: pointer; text-decoration: none;
}
.module-icon-wrap {
  width: 44px; height: 44px; border-radius: 12px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
}
.module-body { flex: 1; min-width: 0; }
.module-title {
  font-size: 0.875rem; font-weight: 700; color: var(--t-text1); line-height: 1.3;
}
.module-desc {
  font-size: 0.71rem; color: var(--t-text3); margin-top: 0.15rem;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.module-badge {
  display: inline-block; margin-top: 0.35rem;
  padding: 0.15rem 0.55rem; border-radius: 99px;
  font-size: 0.63rem; font-weight: 700;
  background: var(--t-acc-alpha-sm);
  border: 1px solid var(--t-acc-alpha-md);
  color: var(--t-accent);
}
.module-arrow {
  flex-shrink: 0; color: var(--t-text3);
  transition: transform 0.15s, color 0.15s;
}
.module-card:hover .module-arrow {
  transform: translateX(3px); color: var(--t-accent);
}

/* ── Activity Feed ── */
.activity-item {
  display: flex; align-items: flex-start; gap: 0.75rem;
  padding: 0.7rem 1.5rem 0.7rem 1rem;
  border-left: 3px solid var(--ac);
  margin: 0 0 0 0.5rem;
}
.activity-item + .activity-item {
  border-top: 1px solid var(--t-border);
}
.activity-empty { padding: 1.5rem 1rem; text-align: center; font-size: 0.8rem; color: var(--t-text3); }
.activity-dot { font-size: 1rem; flex-shrink: 0; margin-top: 0.05rem; }
.activity-content { flex: 1; min-width: 0; }
.activity-text { font-size: 0.8rem; color: var(--t-text1); line-height: 1.4; }
.activity-time { font-size: 0.68rem; color: var(--t-text3); margin-top: 0.12rem; }

/* ── Quick Actions ── */
.quick-action-card {
  display: flex; align-items: center; gap: 1rem; cursor: pointer;
  width: 100%; text-align: left; font: inherit;
}
.qa-icon {
  font-size: 1.5rem; flex-shrink: 0;
  width: 48px; height: 48px; border-radius: 14px;
  display: flex; align-items: center; justify-content: center;
  background: color-mix(in srgb, var(--qa) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--qa) 20%, transparent);
}
.qa-title { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); }
.qa-desc  { font-size: 0.71rem; color: var(--t-text3); margin-top: 0.15rem; }
.qa-btn {
  margin-left: auto; flex-shrink: 0;
  width: 32px; height: 32px; border-radius: 99px;
  display: flex; align-items: center; justify-content: center;
  background: var(--t-hover2); border: 1px solid var(--t-border);
  color: var(--t-text2); cursor: pointer;
  transition: background 0.15s, color 0.15s, border-color 0.15s;
}
.qa-btn:hover {
  background: var(--t-acc-alpha-md); color: var(--t-accent);
  border-color: color-mix(in srgb, var(--t-accent) 30%, transparent);
}
</style>
