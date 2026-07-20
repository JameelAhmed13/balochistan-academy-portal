<template>
  <div class="admin-shell">
    <a href="#admin-main" class="skip-link">Skip to main content</a>
    <AdminTopNav />
    <main id="admin-main" tabindex="-1" class="admin-main" aria-label="Admin content">
      <div class="admin-main-inner">
        <RouterView v-slot="{ Component, route }">
          <Transition name="fade" mode="out-in">
            <component :is="Component" :key="route.path" />
          </Transition>
        </RouterView>
      </div>
    </main>
  </div>
</template>

<script setup>
import { RouterView } from 'vue-router'
import AdminTopNav from './AdminTopNav.vue'
</script>

<style scoped>
.admin-shell { min-height: 100vh; background: var(--t-bg); color: var(--t-text1); }
.admin-main { min-height: calc(100vh - 64px); overflow-y: auto; }
.admin-main-inner { max-width: none; width: 100%; margin: 0; padding: 1.5rem 1rem; }
@media (min-width: 768px) { .admin-main-inner { padding: 1.75rem 2rem; } }
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; transform: translateY(6px); }
</style>
