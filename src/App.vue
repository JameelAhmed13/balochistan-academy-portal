<template>
  <HttpLoader />
  <PageLoader />
  <CursorEffect />
  <RouterView />
  <Toast position="top-right" />
  <ConfirmModal />
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { RouterView } from 'vue-router'
import Toast from 'primevue/toast'
import PageLoader from '@/components/effects/PageLoader.vue'
import HttpLoader from '@/components/effects/HttpLoader.vue'
import CursorEffect from '@/components/effects/CursorEffect.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'
import { useThemeStore } from '@/stores/theme'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import { useSettingsStore } from '@/stores/settings'
import { initAutoDirection, stopAutoDirection } from '@/utils/autoDirection'

// Initialize theme (applies dark/light class to <html>)
useThemeStore()

// Re-validate the stored JWT + refresh the user (incl. their grade) on boot, then
// preload the catalog (grades/tutors) and the student's grade subjects so subject
// resolution (findPrepSubject) is correct from the first render — avoids the
// hardcoded-fallback id collision (e.g. catalog English id 1 vs legacy Urdu id 1).
const auth = useAuthStore()
const catalog = useCatalogStore()
const settingsStore = useSettingsStore()
async function boot() {
  if (auth.token) await auth.fetchMe()
  catalog.bootstrap().catch(() => {})
  const code = auth.user?.gradeCode
  if (code) catalog.fetchSubjectsForGrade(code).catch(() => {})
  settingsStore.load().catch(() => {})
}
boot()

// Auto-align text by language (English → left, Urdu → right) across the whole app.
onMounted(() => initAutoDirection(document.body))
onUnmounted(stopAutoDirection)
</script>
