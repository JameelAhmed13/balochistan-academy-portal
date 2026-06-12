<template>
  <PageLoader />
  <CursorEffect />
  <RouterView />
  <Toast position="top-right" />
  <ConfirmDialog />
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { RouterView } from 'vue-router'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
import PageLoader from '@/components/effects/PageLoader.vue'
import CursorEffect from '@/components/effects/CursorEffect.vue'
import { useThemeStore } from '@/stores/theme'
import { initAutoDirection, stopAutoDirection } from '@/utils/autoDirection'

// Initialize theme (applies dark/light class to <html>)
useThemeStore()

// Auto-align text by language (English → left, Urdu → right) across the whole app.
onMounted(() => initAutoDirection(document.body))
onUnmounted(stopAutoDirection)
</script>
