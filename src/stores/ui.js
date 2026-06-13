import { defineStore } from 'pinia'
import { ref } from 'vue'

// App-wide UI preferences (persisted). navLayout switches the main shell between
// a vertical left sidebar and a horizontal top nav bar.
export const useUiStore = defineStore('ui', () => {
  const navLayout = ref(localStorage.getItem('bap_nav_layout') || 'vertical') // 'vertical' | 'horizontal'

  function setNavLayout(mode) {
    navLayout.value = mode === 'horizontal' ? 'horizontal' : 'vertical'
    localStorage.setItem('bap_nav_layout', navLayout.value)
  }
  function toggleNavLayout() {
    setNavLayout(navLayout.value === 'vertical' ? 'horizontal' : 'vertical')
  }

  return { navLayout, setNavLayout, toggleNavLayout }
})
