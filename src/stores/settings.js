import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'

export const useSettingsStore = defineStore('settings', () => {
  const map = ref({})
  const loaded = ref(false)

  async function load() {
    try {
      const { data } = await api.get('/admin/settings')
      map.value = data || {}
      loaded.value = true
    } catch {
      // non-admins or unauthenticated — silently ignore
    }
  }

  function get(key, fallback = null) {
    const val = map.value[key]
    return val !== undefined && val !== null ? val : fallback
  }

  async function save(key, value) {
    await api.put(`/admin/settings/${key}`, JSON.stringify(value), {
      headers: { 'Content-Type': 'application/json' },
    })
    map.value[key] = String(value)
  }

  async function saveMany(patches) {
    for (const [key, value] of Object.entries(patches)) {
      await save(key, value)
    }
  }

  return { map, loaded, load, get, save, saveMany }
})
