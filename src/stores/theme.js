import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useThemeStore = defineStore('theme', () => {
  const saved = localStorage.getItem('bap-theme')
  const isDark = ref(saved ? saved === 'dark' : true)

  function apply() {
    if (isDark.value) {
      document.documentElement.classList.add('dark')
      document.documentElement.classList.remove('light')
    } else {
      document.documentElement.classList.add('light')
      document.documentElement.classList.remove('dark')
    }
  }

  function toggle() {
    isDark.value = !isDark.value
    localStorage.setItem('bap-theme', isDark.value ? 'dark' : 'light')
    apply()
  }

  apply()
  return { isDark, toggle }
})
