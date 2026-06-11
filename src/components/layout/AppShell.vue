<template>
  <div class="flex h-screen overflow-hidden" style="background:var(--t-bg)">
    <!-- Skip navigation link for keyboard/screen-reader users -->
    <a href="#main-content" class="skip-link">Skip to main content</a>

    <!-- Sidebar -->
    <AppSidebar :collapsed="sidebarCollapsed" @toggle="sidebarCollapsed = !sidebarCollapsed" />

    <!-- Main area -->
    <div class="flex flex-col flex-1 min-w-0 overflow-hidden">
      <AppHeader :sidebar-collapsed="sidebarCollapsed" @toggle-sidebar="sidebarCollapsed = !sidebarCollapsed" />

      <!-- Page content -->
      <main id="main-content" tabindex="-1" class="flex-1 overflow-y-auto p-4 md:p-6" style="background:var(--t-bg);color:var(--t-text1)" aria-label="Main content">
        <RouterView v-slot="{ Component, route }">
          <Transition name="fade" mode="out-in">
            <component :is="Component" :key="route.path" />
          </Transition>
        </RouterView>
      </main>
    </div>

    <!-- Floating WhatsApp + Quick Services -->
    <FloatingActions />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { RouterView } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppHeader from './AppHeader.vue'
import FloatingActions from './FloatingActions.vue'

const sidebarCollapsed = ref(false)
</script>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; transform: translateY(6px); }
</style>
