<template>
  <component
    :is="item.external ? 'a' : 'RouterLink'"
    v-bind="item.external ? { href: item.external, target: '_blank', rel: 'noopener noreferrer' } : { to: item.path }"
    :class="['nav-item', isActive ? 'active' : '', collapsed ? 'slim' : '']"
    :title="collapsed ? item.name : undefined"
    :aria-label="item.name + (item.badge ? ' (' + item.badge + ' new)' : '') + (item.external ? ' (opens in new tab)' : '')"
    :aria-current="isActive ? 'page' : undefined"
  >
    <div v-if="isActive" class="nav-active-bar" />
    <div class="nav-icon-wrap" :class="{ active: isActive }">
      <component :is="item.icon" class="nav-icon" />
    </div>
    <span v-if="!collapsed" class="nav-label">{{ item.name }}</span>
    <span v-if="!collapsed && item.badge" class="nav-badge">{{ item.badge }}</span>
    <svg v-if="!collapsed && item.external" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="ml-auto" style="opacity:0.3">
      <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6M15 3h6v6M10 14L21 3"/>
    </svg>
  </component>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const props = defineProps({ item: Object, collapsed: Boolean })
const route = useRoute()
const isActive = computed(() => {
  if (!props.item.path) return false
  if (props.item.path === '/app') return route.path === '/app' || route.path === '/app/'
  if (!route.path.startsWith(props.item.path)) return false
  // Don't highlight this item when a more-specific child nav item owns the route
  if (props.item.exclude && route.path.startsWith(props.item.exclude)) return false
  return true
})
</script>

<style scoped>
.nav-item {
  position: relative; display: flex; align-items: center; gap: 0.65rem;
  padding: 0.6rem 0.75rem; border-radius: 13px; margin-bottom: 2px;
  font-size: 0.85rem; font-weight: 500; text-decoration: none;
  color: var(--t-text3);
  transition: all 0.18s cubic-bezier(0.4,0,0.2,1);
  cursor: pointer; overflow: hidden;
  border: 1px solid transparent;
}
.nav-item:hover {
  background: var(--t-hover);
  color: var(--t-text2);
}
.nav-item.active {
  background: var(--t-acc-alpha-sm);
  color: var(--t-text1);
  border-color: color-mix(in srgb, var(--t-accent) 20%, transparent);
}
.nav-item.slim { justify-content: center; padding: 0.65rem; }

.nav-active-bar {
  position: absolute; left: 0; top: 25%; bottom: 25%; width: 3px;
  border-radius: 0 3px 3px 0;
  background: linear-gradient(180deg, var(--t-accent), color-mix(in srgb, var(--t-accent) 60%, #a78bfa));
  box-shadow: 0 0 8px color-mix(in srgb, var(--t-accent) 50%, transparent);
}
.nav-icon-wrap {
  width: 30px; height: 30px; border-radius: 9px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  background: var(--t-hover);
  transition: all 0.18s;
}
.nav-icon-wrap.active {
  background: var(--t-acc-alpha-md);
  box-shadow: 0 0 12px color-mix(in srgb, var(--t-accent) 25%, transparent);
}
.nav-item:hover .nav-icon-wrap { background: var(--t-acc-alpha-sm); }
.nav-icon { width: 15px; height: 15px; }
.nav-label { flex: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.nav-badge {
  margin-left: auto; padding: 0.1rem 0.45rem; border-radius: 99px;
  background: var(--t-accent); color: white; font-size: 0.65rem; font-weight: 700;
}
</style>
