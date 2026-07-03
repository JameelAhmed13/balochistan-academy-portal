<template>
  <!-- Floating AI hint button + panel -->
  <div class="aih-wrap">
    <Transition name="aih-panel">
      <div v-if="open" class="aih-panel">
        <div class="aih-panel-header">
          <div class="aih-panel-title">🤖 AI Study Helper</div>
          <button @click="open = false" class="aih-close">✕</button>
        </div>
        <div class="aih-messages" ref="msgArea">
          <div v-for="(msg, i) in messages" :key="i" class="aih-msg" :class="msg.role">
            <div class="aih-bubble">{{ msg.text }}</div>
          </div>
          <div v-if="loading" class="aih-msg ai">
            <div class="aih-bubble aih-typing">
              <span/><span/><span/>
            </div>
          </div>
        </div>
        <div v-if="quickChips.length" class="aih-chips">
          <button v-for="chip in quickChips" :key="chip" @click="ask(chip)" class="aih-chip">
            {{ chip }}
          </button>
        </div>
        <div class="aih-input-row">
          <input v-model="inputText" @keyup.enter="ask()" placeholder="Ask AI anything..." class="aih-input" />
          <button @click="ask()" :disabled="loading || !inputText.trim()" class="aih-send">→</button>
        </div>
      </div>
    </Transition>
    <button @click="open = !open" class="aih-fab" :class="{ 'aih-fab-pulse': !open }" title="AI Study Helper">
      <span class="aih-fab-icon">🤖</span>
    </button>
  </div>
</template>

<script setup>
import { ref, nextTick, watch } from 'vue'
import api from '@/services/api'

const props = defineProps({
  context: { type: String, default: '' },
  chips: { type: Array, default: () => [] },
})

const open = ref(false)
const inputText = ref('')
const loading = ref(false)
const msgArea = ref(null)
const messages = ref([
  { role: 'ai', text: 'Assalamualaikum! I\'m your AI Study Helper. Ask me to explain any concept, solve a problem, or help you understand your answer. How can I help? 🎓' }
])

const quickChips = ref(props.chips.length ? props.chips : ['Explain this concept', 'Give me a hint', 'Why was my answer wrong?', 'Simplify this'])

async function ask(prompt) {
  const text = (prompt || inputText.value).trim()
  if (!text) return
  inputText.value = ''
  messages.value.push({ role: 'user', text })
  loading.value = true
  await nextTick(); scrollBottom()
  try {
    const systemPrompt = `You are an AI study helper for Pakistani board exam students (Grade 9-12). ${props.context ? 'Current context: ' + props.context : ''} Keep answers concise (3-5 sentences), use simple language, and relate to the Pakistan curriculum where possible. Use Urdu words occasionally for warmth.`
    const { data } = await api.post('/ai/gemini', {
      contents: [{ role: 'user', parts: [{ text: systemPrompt + '\n\nStudent: ' + text }] }]
    })
    const reply = data.candidates?.[0]?.content?.parts?.[0]?.text || 'Sorry, I couldn\'t get a response. Try again.'
    messages.value.push({ role: 'ai', text: reply })
  } catch {
    messages.value.push({ role: 'ai', text: 'AI is unavailable right now. Please try again later.' })
  }
  loading.value = false
  await nextTick(); scrollBottom()
}

function scrollBottom() { if (msgArea.value) msgArea.value.scrollTop = msgArea.value.scrollHeight }
</script>

<style scoped>
.aih-wrap { position: fixed; bottom: 6rem; right: 1.5rem; z-index: 60; display: flex; flex-direction: column; align-items: flex-end; gap: 0.75rem; }

.aih-fab {
  width: 52px; height: 52px; border-radius: 50%; border: none; cursor: pointer;
  background: linear-gradient(135deg, #6d54e8, #a855f7);
  color: white; font-size: 1.4rem; display: flex; align-items: center; justify-content: center;
  box-shadow: 0 4px 20px rgba(109,84,232,0.45);
  transition: transform 0.15s, box-shadow 0.2s;
}
.aih-fab:hover { transform: scale(1.08); box-shadow: 0 6px 28px rgba(109,84,232,0.6); }
.aih-fab-pulse { animation: aihPulse 2.5s ease-in-out infinite; }
@keyframes aihPulse { 0%,100%{box-shadow:0 4px 20px rgba(109,84,232,0.45)} 50%{box-shadow:0 4px 32px rgba(109,84,232,0.75)} }

.aih-panel {
  width: 320px; background: var(--t-surface); border: 1px solid var(--t-border);
  border-radius: 18px; box-shadow: 0 8px 40px rgba(0,0,0,0.2);
  display: flex; flex-direction: column; max-height: 480px; overflow: hidden;
}
.aih-panel-header { display: flex; justify-content: space-between; align-items: center; padding: 0.85rem 1rem; border-bottom: 1px solid var(--t-border); background: linear-gradient(135deg, rgba(109,84,232,0.1), rgba(168,85,247,0.08)); }
.aih-panel-title { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); }
.aih-close { background: none; border: none; color: var(--t-text3); cursor: pointer; font-size: 0.9rem; }
.aih-messages { flex: 1; overflow-y: auto; padding: 0.75rem; display: flex; flex-direction: column; gap: 0.6rem; }
.aih-msg { display: flex; }
.aih-msg.user { justify-content: flex-end; }
.aih-bubble { max-width: 85%; padding: 0.6rem 0.85rem; border-radius: 12px; font-size: 0.8rem; line-height: 1.5; white-space: pre-wrap; }
.aih-msg.user .aih-bubble { background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; }
.aih-msg.ai  .aih-bubble { background: var(--t-hover); color: var(--t-text1); border: 1px solid var(--t-border); }
.aih-typing { display: flex; gap: 4px; align-items: center; padding: 0.75rem 1rem; }
.aih-typing span { width: 7px; height: 7px; border-radius: 50%; background: var(--t-text3); animation: typBounce 1.2s infinite; }
.aih-typing span:nth-child(2) { animation-delay: 0.2s; }
.aih-typing span:nth-child(3) { animation-delay: 0.4s; }
@keyframes typBounce { 0%,80%,100%{transform:translateY(0)}40%{transform:translateY(-6px)} }
.aih-chips { display: flex; gap: 0.3rem; flex-wrap: wrap; padding: 0 0.75rem 0.5rem; }
.aih-chip { padding: 0.25rem 0.6rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text2); font-size: 0.68rem; cursor: pointer; white-space: nowrap; }
.aih-chip:hover { background: rgba(109,84,232,0.1); border-color: #6d54e8; color: #6d54e8; }
.aih-input-row { display: flex; gap: 0.4rem; padding: 0.6rem; border-top: 1px solid var(--t-border); }
.aih-input { flex: 1; padding: 0.45rem 0.75rem; border: 1px solid var(--t-border); border-radius: 9px; background: var(--t-bg); color: var(--t-text1); font-size: 0.8rem; }
.aih-send { width: 32px; height: 32px; border-radius: 50%; background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; border: none; cursor: pointer; font-size: 1rem; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.aih-send:disabled { opacity: 0.4; cursor: not-allowed; }
.aih-panel-enter-active, .aih-panel-leave-active { transition: opacity 0.2s, transform 0.2s; }
.aih-panel-enter-from, .aih-panel-leave-to { opacity: 0; transform: translateY(12px) scale(0.96); }
</style>
