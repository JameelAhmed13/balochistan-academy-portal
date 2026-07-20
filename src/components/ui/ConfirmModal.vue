<template>
  <Teleport to="body">
    <Transition name="cm-fade">
      <div v-if="store.visible" class="cm-backdrop" @click.self="store.reject()" role="dialog"
        aria-modal="true" :aria-labelledby="titleId" :aria-describedby="msgId">
        <Transition name="cm-pop" appear>
          <div v-if="store.visible" class="cm-dialog">

            <!-- Icon badge -->
            <div class="cm-icon-wrap" :class="`cm-icon--${store.type}`">
              <component :is="icon" class="cm-icon-svg" stroke-width="2" />
              <div class="cm-icon-ring" />
            </div>

            <!-- Text -->
            <div class="cm-body">
              <h3 :id="titleId" class="cm-title">{{ store.title }}</h3>
              <p  :id="msgId"   class="cm-msg" v-if="store.message">{{ store.message }}</p>
            </div>

            <!-- Actions -->
            <div class="cm-footer">
              <button class="cm-btn cm-btn--ghost"   @click="store.reject()">{{ store.cancelLabel }}</button>
              <button class="cm-btn" :class="`cm-btn--${store.type}`" @click="store.accept()">
                <component :is="icon" class="cm-btn-icon" stroke-width="2.5" />
                {{ store.confirmLabel }}
              </button>
            </div>

          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'
import { Trash2, AlertTriangle, Info } from '@lucide/vue'
import { useConfirmModalStore } from '@/stores/confirmModal'

const store = useConfirmModalStore()

const titleId = 'cm-title-' + Math.random().toString(36).slice(2)
const msgId   = 'cm-msg-'   + Math.random().toString(36).slice(2)

const icon = computed(() => ({
  danger:  Trash2,
  warning: AlertTriangle,
  info:    Info,
}[store.type] ?? AlertTriangle))
</script>

<style scoped>
/* ── Backdrop ─────────────────────────────────────────────────────── */
.cm-backdrop {
  position: fixed; inset: 0; z-index: 99990;
  display: grid; place-items: center; padding: 20px;
  background: rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(6px) saturate(160%);
  -webkit-backdrop-filter: blur(6px) saturate(160%);
}

/* ── Dialog card ──────────────────────────────────────────────────── */
.cm-dialog {
  width: min(400px, 100%);
  background: var(--t-bg2);
  border: 1px solid var(--t-border);
  border-radius: 22px;
  padding: 32px 28px 24px;
  display: flex; flex-direction: column; align-items: center; gap: 20px;
  box-shadow: var(--t-shadow-md),
              0 0 0 1px color-mix(in srgb, var(--t-accent) 6%, transparent) inset;
  position: relative; overflow: hidden;
}

/* subtle top-edge highlight */
.cm-dialog::before {
  content: '';
  position: absolute; top: 0; left: 10%; right: 10%; height: 1px;
  background: linear-gradient(90deg, transparent, color-mix(in srgb, var(--t-accent) 30%, transparent), transparent);
}

/* ── Icon badge ───────────────────────────────────────────────────── */
.cm-icon-wrap {
  position: relative;
  width: 64px; height: 64px;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}

.cm-icon--danger  { background: rgba(239, 68,  68,  0.12); }
.cm-icon--warning { background: rgba(245, 158, 11,  0.12); }
.cm-icon--info    { background: color-mix(in srgb, var(--t-accent) 12%, transparent); }

.cm-icon-svg {
  width: 28px; height: 28px;
  position: relative; z-index: 1;
}
.cm-icon--danger  .cm-icon-svg { color: #f87171; }
.cm-icon--warning .cm-icon-svg { color: #fbbf24; }
.cm-icon--info    .cm-icon-svg { color: var(--t-accent); }

/* pulsing outer ring */
.cm-icon-ring {
  position: absolute; inset: -4px;
  border-radius: 50%;
  animation: cm-pulse 2s ease-in-out infinite;
}
.cm-icon--danger  .cm-icon-ring { border: 1.5px solid rgba(239, 68,  68,  0.35); }
.cm-icon--warning .cm-icon-ring { border: 1.5px solid rgba(245, 158, 11,  0.35); }
.cm-icon--info    .cm-icon-ring { border: 1.5px solid color-mix(in srgb, var(--t-accent) 35%, transparent); }

@keyframes cm-pulse {
  0%, 100% { transform: scale(1);    opacity: 1;   }
  50%       { transform: scale(1.12); opacity: 0.5; }
}

/* ── Text ─────────────────────────────────────────────────────────── */
.cm-body { text-align: center; display: flex; flex-direction: column; gap: 8px; }

.cm-title {
  font-family: 'Syne', sans-serif;
  font-weight: 800; font-size: 1.15rem;
  color: var(--t-text1); margin: 0;
}

.cm-msg {
  font-size: 0.875rem; line-height: 1.55;
  color: var(--t-text2); margin: 0;
}

/* ── Footer buttons ───────────────────────────────────────────────── */
.cm-footer {
  display: flex; gap: 10px; width: 100%; margin-top: 4px;
}

.cm-btn {
  flex: 1; display: inline-flex; align-items: center; justify-content: center;
  gap: 7px; padding: 11px 18px;
  border: none; border-radius: 12px;
  font-size: 0.88rem; font-weight: 700; cursor: pointer;
  transition: all 0.15s ease; white-space: nowrap;
}

.cm-btn-icon { width: 14px; height: 14px; flex-shrink: 0; }

.cm-btn--ghost {
  background: var(--t-hover);
  color: var(--t-text2);
  border: 1px solid var(--t-border);
}
.cm-btn--ghost:hover {
  background: var(--t-hover2);
  color: var(--t-text1);
}

/* danger */
.cm-btn--danger {
  background: linear-gradient(135deg, #ef4444, #dc2626);
  color: #fff;
  box-shadow: 0 4px 14px rgba(239, 68, 68, 0.35);
}
.cm-btn--danger:hover {
  background: linear-gradient(135deg, #dc2626, #b91c1c);
  box-shadow: 0 4px 18px rgba(239, 68, 68, 0.5);
  transform: translateY(-1px);
}
.cm-btn--danger:active { transform: translateY(0); }

/* warning */
.cm-btn--warning {
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: #fff;
  box-shadow: 0 4px 14px rgba(245, 158, 11, 0.35);
}
.cm-btn--warning:hover {
  background: linear-gradient(135deg, #d97706, #b45309);
  box-shadow: 0 4px 18px rgba(245, 158, 11, 0.5);
  transform: translateY(-1px);
}

/* info */
.cm-btn--info {
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  color: #fff;
  box-shadow: 0 4px 14px color-mix(in srgb, var(--t-accent) 40%, transparent);
}
.cm-btn--info:hover {
  filter: brightness(1.1);
  transform: translateY(-1px);
}

/* ── Transitions ──────────────────────────────────────────────────── */
.cm-fade-enter-active, .cm-fade-leave-active { transition: opacity 0.2s ease; }
.cm-fade-enter-from,  .cm-fade-leave-to      { opacity: 0; }

.cm-pop-enter-active  { transition: opacity 0.22s ease, transform 0.22s cubic-bezier(0.34, 1.56, 0.64, 1); }
.cm-pop-leave-active  { transition: opacity 0.18s ease, transform 0.18s ease; }
.cm-pop-enter-from    { opacity: 0; transform: scale(0.88) translateY(8px); }
.cm-pop-leave-to      { opacity: 0; transform: scale(0.95) translateY(4px); }
</style>
