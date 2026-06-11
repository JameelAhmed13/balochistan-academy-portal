<template>
  <div class="fixed bottom-6 right-6 flex flex-col items-end gap-3 z-50">
    <!-- Quick Services toggle -->
    <Transition name="quick">
      <div v-if="quickOpen" class="qs-panel animate-fade-in">
        <div class="flex items-center justify-between mb-3">
          <span class="qs-heading">Quick Services</span>
          <button @click="quickOpen=false" class="qs-close"><X class="w-4 h-4" /></button>
        </div>
        <div class="grid grid-cols-2 gap-2">
          <component :is="s.url.startsWith('/') ? 'RouterLink' : 'a'"
            v-for="s in quickServices" :key="s.name"
            :to="s.url.startsWith('/') ? s.url : undefined"
            :href="s.url.startsWith('/') ? undefined : s.url"
            :target="s.url.startsWith('/') ? undefined : '_blank'"
            @click="quickOpen = false"
            class="qs-tile"
          >
            <span class="text-base">{{ s.icon }}</span>{{ s.name }}
          </component>
        </div>
      </div>
    </Transition>

    <div class="flex items-center gap-2">
      <!-- WhatsApp -->
      <a href="https://wa.me/923001234567" target="_blank"
        class="w-12 h-12 rounded-full bg-[#25D366] hover:bg-[#20bb5a] text-white flex items-center justify-center shadow-lg hover:shadow-xl transition-all hover:scale-110 active:scale-95"
        title="Contact via WhatsApp"
      >
        <MessageCircle class="w-5 h-5" />
      </a>
      <!-- Quick Services -->
      <button
        @click="quickOpen = !quickOpen"
        class="qs-fab"
        :class="{ 'qs-fab-open': quickOpen }"
        title="Quick Services"
      >
        <Zap class="w-5 h-5" />
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { X, MessageCircle, Zap } from '@lucide/vue'

const quickOpen = ref(false)

const quickServices = [
  { name: 'E-Books',           icon: '📚', url: '/app/ebooks' },
  { name: 'Puzzle Games',      icon: '🧩', url: '/app/puzzle-games' },
  { name: 'Typing Master',     icon: '⌨️', url: '/app/typing-master' },
  { name: 'Learn Coding',      icon: '💻', url: '/app/learn-coding' },
  { name: 'Smart Notes',       icon: '📓', url: '/app/smart-notes' },
  { name: 'Calculator',        icon: '🧮', url: '/app/calculator' },
  { name: 'Log Table',         icon: '📊', url: '/app/log-table' },
  { name: 'Periodic Table',    icon: '⚗️', url: '/app/periodic-table' },
  { name: 'Dictionary',        icon: '📖', url: '/app/dictionary' },
  { name: 'AI Chat',           icon: '🤖', url: '/app/chatgpt' },
]
</script>

<style scoped>
/* Quick Services panel */
.qs-panel {
  width: 18rem; padding: 0.85rem; border-radius: 18px;
  background: var(--t-surface);
  border: 1px solid var(--t-border);
  box-shadow: var(--t-shadow-md);
  backdrop-filter: var(--t-blur);
}
.qs-heading { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); }
.qs-close {
  display: flex; align-items: center; justify-content: center;
  padding: 0.25rem; border-radius: 8px; border: none; background: none;
  color: var(--t-text3); cursor: pointer; transition: all 0.15s;
}
.qs-close:hover { background: var(--t-hover); color: var(--t-text1); }

.qs-tile {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.55rem 0.75rem; border-radius: 12px;
  background: var(--t-hover); border: 1px solid transparent;
  color: var(--t-text2); font-size: 0.75rem; font-weight: 600;
  text-decoration: none; transition: all 0.15s;
}
.qs-tile:hover {
  background: var(--t-acc-alpha-sm);
  border-color: color-mix(in srgb, var(--t-accent) 35%, transparent);
  color: var(--t-accent);
}

/* Quick Services FAB */
.qs-fab {
  width: 3rem; height: 3rem; border-radius: 50%; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  color: #fff; border: none; cursor: pointer;
  box-shadow: 0 6px 18px color-mix(in srgb, var(--t-accent) 35%, transparent);
  transition: transform 0.15s, box-shadow 0.2s;
}
.qs-fab:hover { transform: scale(1.1); box-shadow: 0 0 26px color-mix(in srgb, var(--t-accent) 60%, transparent); }
.qs-fab:active { transform: scale(0.95); }
.qs-fab-open { transform: rotate(90deg); }
.qs-fab-open:hover { transform: rotate(90deg) scale(1.1); }

.quick-enter-active, .quick-leave-active { transition: opacity 0.2s, transform 0.2s; }
.quick-enter-from, .quick-leave-to { opacity: 0; transform: translateY(10px) scale(0.97); }
</style>

