<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">AI Chat Assistant</h2>
    <div class="chat-layout">
      <div class="chat-messages" ref="chatArea">
        <div v-for="(msg, i) in messages" :key="i" class="chat-msg" :class="msg.role === 'user' ? 'chat-user' : 'chat-ai'">
          <div class="chat-msg-avatar">{{ msg.role === 'user' ? '👤' : '🤖' }}</div>
          <div class="chat-msg-bubble">
            <div class="chat-msg-text">{{ msg.content }}</div>
          </div>
        </div>
        <div v-if="loading" class="chat-msg chat-ai">
          <div class="chat-msg-avatar">🤖</div>
          <div class="chat-msg-bubble chat-typing">Thinking...</div>
        </div>
      </div>
      <div class="chat-input-row">
        <div class="chat-suggestions" v-if="messages.length === 1">
          <button v-for="s in suggestions" :key="s" @click="input = s" class="chat-suggest-btn">{{ s }}</button>
        </div>
        <div class="chat-input-area">
          <textarea v-model="input" @keydown.enter.prevent="!loading && input.trim() && send()"
            placeholder="Ask anything about your studies..." rows="2" class="chat-input" />
          <button @click="send" :disabled="!input.trim() || loading" class="chat-send-btn">Send</button>
        </div>
      </div>
    </div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, nextTick } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const input = ref('')
const loading = ref(false)
const chatArea = ref(null)

const suggestions = [
  'Explain photosynthesis simply',
  'Solve x² - 5x + 6 = 0',
  'What is Newton\'s first law?',
  'Explain the periodic table',
]

const messages = ref([
  { role: 'ai', content: 'Assalamualaikum! I\'m your AI Study Assistant. Ask me anything about your subjects — Mathematics, Physics, Chemistry, Biology, Urdu, or English. I\'m here to help you understand concepts and solve problems!' }
])

const responses = {
  photosynthesis: 'Photosynthesis is the process by which plants use sunlight, water, and CO₂ to produce glucose and oxygen:\n6CO₂ + 6H₂O + Light → C₆H₁₂O₆ + 6O₂\nIt happens in the chloroplasts, using chlorophyll to capture light energy.',
  newton: 'Newton\'s Laws of Motion:\n1st Law: Objects stay at rest or in motion unless acted on by a force.\n2nd Law: F = ma (Force = mass × acceleration)\n3rd Law: Every action has an equal and opposite reaction.',
  quadratic: 'For x² - 5x + 6 = 0, use the quadratic formula:\nx = (-b ± √(b²-4ac)) / 2a\na=1, b=-5, c=6\nx = (5 ± √(25-24)) / 2 = (5 ± 1) / 2\nx = 3 or x = 2 ✓',
  periodic: 'The periodic table organizes 118 elements by atomic number. Elements in the same column (group) have similar properties. Periods (rows) represent electron shells. Key groups: alkali metals (Group 1), halogens (Group 17), noble gases (Group 18).',
}

async function send() {
  if (!input.value.trim()) return
  messages.value.push({ role: 'user', content: input.value })
  const q = input.value.toLowerCase(); input.value = ''
  loading.value = true
  await new Promise(r => setTimeout(r, 800))
  let reply = 'That\'s a great question! For detailed explanations, check your textbook chapter on this topic, or try the Conceptual Learning section. I can help with specific formulas, definitions, or worked examples.'
  if (q.includes('photosynthesis')) reply = responses.photosynthesis
  else if (q.includes('newton')) reply = responses.newton
  else if (q.includes('x²') || q.includes('quadratic') || q.includes('5x + 6')) reply = responses.quadratic
  else if (q.includes('periodic')) reply = responses.periodic
  messages.value.push({ role: 'ai', content: reply })
  loading.value = false
  await nextTick(); if (chatArea.value) chatArea.value.scrollTop = chatArea.value.scrollHeight
}
</script>

<style scoped>
.chat-layout { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 14px; overflow: hidden; display: flex; flex-direction: column; height: 70vh; }
.chat-messages { flex: 1; overflow-y: auto; padding: 1rem; display: flex; flex-direction: column; gap: 1rem; background: var(--t-bg); }
.chat-msg { display: flex; gap: 0.75rem; align-items: flex-start; }
.chat-user { flex-direction: row-reverse; }
.chat-msg-avatar { font-size: 1.25rem; flex-shrink: 0; }
.chat-msg-bubble { max-width: 75%; padding: 0.75rem 1rem; border-radius: 12px; font-size: 0.875rem; line-height: 1.5; white-space: pre-wrap; }
.chat-user .chat-msg-bubble { background: #4caf50; color: white; }
.chat-ai .chat-msg-bubble { background: var(--t-surface); color: var(--t-text1); border: 1px solid var(--t-border); }
.chat-typing { font-style: italic; opacity: 0.6; }
.chat-input-row { border-top: 1px solid var(--t-border); padding: 0.75rem; background: var(--t-surface); }
.chat-suggestions { display: flex; gap: 0.4rem; flex-wrap: wrap; margin-bottom: 0.5rem; }
.chat-suggest-btn { padding: 0.3rem 0.65rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-bg); color: var(--t-text2); font-size: 0.72rem; cursor: pointer; }
.chat-suggest-btn:hover { background: var(--t-hover); }
.chat-input-area { display: flex; gap: 0.5rem; }
.chat-input { flex: 1; padding: 0.6rem 0.85rem; border: 1px solid var(--t-border); border-radius: 9px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; resize: none; font-family: inherit; }
.chat-send-btn { padding: 0.6rem 1.25rem; border-radius: 9px; background: #4caf50; color: white; border: none; font-weight: 700; cursor: pointer; font-size: 0.82rem; align-self: flex-end; }
.chat-send-btn:disabled { opacity: 0.4; cursor: not-allowed; }
</style>
