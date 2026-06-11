<template>
  <PageLoader />
  <CursorEffect />
  <RouterView />
  <Toast position="top-right" />
  <ConfirmDialog />
</template>

<script setup>
import { RouterView } from 'vue-router'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
import PageLoader from '@/components/effects/PageLoader.vue'
import CursorEffect from '@/components/effects/CursorEffect.vue'
import { useThemeStore } from '@/stores/theme'

// Initialize theme (applies dark/light class to <html>)
useThemeStore()
</script>
