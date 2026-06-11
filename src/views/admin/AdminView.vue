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
        <span class="ag-banner-stat">👥 48 Users</span>
        <span class="ag-banner-stat">❓ 1200+ Questions</span>
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
          <div
            v-for="act in recentActivity"
            :key="act.id"
            class="activity-item"
            :style="{ '--ac': act.color }"
          >
            <div class="activity-dot">{{ act.icon }}</div>
            <div class="activity-content">
              <div class="activity-text">{{ act.text }}</div>
              <div class="activity-time">{{ act.time }}</div>
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
        <div
          v-for="qa in quickActions"
          :key="qa.label"
          class="ag-grid-card quick-action-card"
          :style="{ '--qa': qa.color }"
        >
          <div class="qa-icon">{{ qa.icon }}</div>
          <div>
            <div class="qa-title">{{ qa.label }}</div>
            <div class="qa-desc">{{ qa.desc }}</div>
          </div>
          <button class="qa-btn">
            <ChevronRight class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ChevronRight, LayoutGrid, Zap, Activity } from '@lucide/vue'

const kpiCards = [
  { label: 'Total Users',  value: '48',    icon: '👥', color: '#8b5cf6' },
  { label: 'Tests Taken',  value: '312',   icon: '📝', color: '#3b82f6' },
  { label: 'AI Sessions',  value: '128',   icon: '🤖', color: '#10b981' },
  { label: 'Coins Issued', value: '15.2K', icon: '🪙', color: '#f59e0b' },
  { label: 'Questions',    value: '1200',  icon: '❓', color: '#a855f7' },
  { label: 'Active Tests', value: '5',     icon: '📅', color: '#14b8a6' },
]

const adminModules = [
  { title: 'Content',       desc: 'Subjects, books, units, topics and resources',   icon: '📚', count: '10 subjects',    path: '/app/admin/content',       iconBg: 'rgba(59,130,246,0.12)'  },
  { title: 'Questions',     desc: 'Objective & subjective question bank',            icon: '❓', count: '1200 questions', path: '/app/admin/questions',     iconBg: 'rgba(139,92,246,0.12)'  },
  { title: 'Tests',         desc: 'Create and schedule platform tests',              icon: '📝', count: '5 active',       path: '/app/admin/tests',         iconBg: 'rgba(16,185,129,0.12)'  },
  { title: 'Users',         desc: 'Students, teachers, parents and admins',          icon: '👥', count: '48 users',       path: '/app/admin/users',         iconBg: 'rgba(236,72,153,0.12)'  },
  { title: 'Coins',         desc: 'Withdrawal requests and coin settings',           icon: '🪙', count: '3 pending',      path: '/app/admin/coins',         iconBg: 'rgba(245,158,11,0.12)'  },
  { title: 'Analytics',      desc: 'Platform-wide performance dashboards',            icon: '📊', count: 'Live data',      path: '/app/admin/analytics',      iconBg: 'rgba(20,184,166,0.12)'  },
  { title: 'Notifications', desc: 'Send announcements and alerts to students',        icon: '🔔', count: '24 sent',        path: '/app/admin/notifications',  iconBg: 'rgba(251,191,36,0.12)'  },
  { title: 'Settings',      desc: 'Platform configuration and AI API keys',           icon: '⚙️', count: '8 settings',     path: '/app/admin/settings',       iconBg: 'rgba(100,116,139,0.12)' },
  { title: 'Complaints',    desc: 'Student feedback and complaint management',         icon: '💬', count: '12 open',        path: '/app/admin/complaints',     iconBg: 'rgba(239,68,68,0.12)'   },
]

const recentActivity = [
  { id: 1, icon: '👤', text: 'New student registered: Ali Hassan', time: '2 min ago',  color: '#3b82f6' },
  { id: 2, icon: '📝', text: 'Test "Bio Chapter 3" was published',  time: '18 min ago', color: '#10b981' },
  { id: 3, icon: '🪙', text: 'Coin withdrawal request: ₨500',       time: '1 hr ago',   color: '#f59e0b' },
  { id: 4, icon: '❓', text: '40 new questions added to Physics',    time: '3 hr ago',   color: '#8b5cf6' },
  { id: 5, icon: '🛡️', text: 'Admin settings updated by Rizwan',    time: 'Yesterday',  color: '#6d54e8' },
]

const quickActions = [
  { label: 'Send Notification', desc: 'Broadcast to all users',    icon: '🔔', color: '#3b82f6' },
  { label: 'Export Data',       desc: 'Download platform reports', icon: '📤', color: '#10b981' },
  { label: 'View Settings',     desc: 'Platform configuration',    icon: '⚙️', color: '#8b5cf6' },
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
.activity-dot { font-size: 1rem; flex-shrink: 0; margin-top: 0.05rem; }
.activity-content { flex: 1; min-width: 0; }
.activity-text { font-size: 0.8rem; color: var(--t-text1); line-height: 1.4; }
.activity-time { font-size: 0.68rem; color: var(--t-text3); margin-top: 0.12rem; }

/* ── Quick Actions ── */
.quick-action-card {
  display: flex; align-items: center; gap: 1rem; cursor: pointer;
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
