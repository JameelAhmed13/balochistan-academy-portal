<template>
  <div class="animate-fade-in msg-layout">
    <div class="msg-sidebar">
      <div class="msg-sidebar-header">Messages</div>
      <div class="msg-contact-list">
        <div v-for="c in contacts" :key="c.id" class="msg-contact"
          :class="{ 'msg-contact-active': activeContact?.id === c.id }" @click="activeContact = c">
          <div class="msg-avatar">{{ c.name[0] }}</div>
          <div class="msg-contact-info">
            <div class="msg-contact-name">{{ c.name }}</div>
            <div class="msg-contact-last">{{ c.lastMsg }}</div>
          </div>
          <div v-if="c.unread" class="msg-unread-badge">{{ c.unread }}</div>
        </div>
      </div>
    </div>
    <div class="msg-main" v-if="activeContact">
      <div class="msg-main-header">
        <div class="msg-main-avatar">{{ activeContact.name[0] }}</div>
        <div class="msg-main-name">{{ activeContact.name }}</div>
        <div class="msg-main-role">{{ activeContact.role }}</div>
      </div>
      <div class="msg-chat-area">
        <div v-for="(msg, i) in activeContact.messages" :key="i"
          class="msg-bubble-wrap" :class="msg.from === 'me' ? 'msg-mine' : 'msg-theirs'">
          <div class="msg-bubble">{{ msg.text }}</div>
          <div class="msg-time">{{ msg.time }}</div>
        </div>
      </div>
      <div class="msg-input-row">
        <input v-model="newMsg" @keyup.enter="sendMsg" type="text" placeholder="Type a message..." class="msg-input" />
        <button @click="sendMsg" class="msg-send-btn">Send</button>
      </div>
    </div>
    <div v-else class="msg-empty">Select a conversation to start messaging.</div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const newMsg = ref('')
const activeContact = ref(null)

const contacts = ref([
  { id: 1, name: 'Sir Ahmed', role: 'Mathematics Teacher', lastMsg: 'Great work on the test!', unread: 2, messages: [
    { from: 'them', text: 'Assalamualaikum! How are your preparations going?', time: '10:30 AM' },
    { from: 'me', text: 'Walaikum Assalam Sir, going well. Struggling with factorization.', time: '10:32 AM' },
    { from: 'them', text: 'Great work on the test! Keep it up.', time: '10:35 AM' },
  ]},
  { id: 2, name: 'Ma\'am Farah', role: 'Chemistry Teacher', lastMsg: 'Submit assignment by Friday', unread: 0, messages: [
    { from: 'them', text: 'Please submit your chemistry assignment by Friday.', time: 'Yesterday' },
  ]},
])

function sendMsg() {
  if (!newMsg.value.trim() || !activeContact.value) return
  activeContact.value.messages.push({ from: 'me', text: newMsg.value, time: 'Now' })
  activeContact.value.lastMsg = newMsg.value
  newMsg.value = ''
}
</script>

<style scoped>
.msg-layout { display: flex; height: 70vh; border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.msg-sidebar { width: 260px; flex-shrink: 0; border-right: 1px solid var(--t-border); background: var(--t-surface); display: flex; flex-direction: column; }
.msg-sidebar-header { padding: 0.85rem 1rem; font-size: 0.875rem; font-weight: 700; color: var(--t-text1); border-bottom: 1px solid var(--t-border); }
.msg-contact-list { flex: 1; overflow-y: auto; }
.msg-contact { display: flex; align-items: center; gap: 0.75rem; padding: 0.75rem 1rem; cursor: pointer; border-bottom: 1px solid var(--t-border); }
.msg-contact:hover { background: var(--t-hover); }
.msg-contact-active { background: rgba(76,175,80,0.1) !important; }
.msg-avatar { width: 36px; height: 36px; border-radius: 50%; background: #4caf50; color: white; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 0.875rem; flex-shrink: 0; }
.msg-contact-info { flex: 1; min-width: 0; }
.msg-contact-name { font-size: 0.82rem; font-weight: 700; color: var(--t-text1); }
.msg-contact-last { font-size: 0.7rem; color: var(--t-text3); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.msg-unread-badge { background: #4caf50; color: white; font-size: 0.65rem; font-weight: 700; border-radius: 99px; padding: 0.1rem 0.4rem; }
.msg-main { flex: 1; display: flex; flex-direction: column; min-width: 0; }
.msg-main-header { display: flex; align-items: center; gap: 0.75rem; padding: 0.75rem 1rem; border-bottom: 1px solid var(--t-border); background: var(--t-surface); }
.msg-main-avatar { width: 36px; height: 36px; border-radius: 50%; background: #00bcd4; color: white; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 0.875rem; }
.msg-main-name { font-weight: 700; font-size: 0.875rem; color: var(--t-text1); }
.msg-main-role { font-size: 0.7rem; color: var(--t-text3); margin-left: 0.25rem; }
.msg-chat-area { flex: 1; overflow-y: auto; padding: 1rem; display: flex; flex-direction: column; gap: 0.75rem; background: var(--t-bg); }
.msg-bubble-wrap { display: flex; flex-direction: column; }
.msg-mine { align-items: flex-end; }
.msg-theirs { align-items: flex-start; }
.msg-bubble { max-width: 70%; padding: 0.6rem 0.85rem; border-radius: 10px; font-size: 0.82rem; }
.msg-mine .msg-bubble { background: #4caf50; color: white; }
.msg-theirs .msg-bubble { background: var(--t-surface); color: var(--t-text1); border: 1px solid var(--t-border); }
.msg-time { font-size: 0.65rem; color: var(--t-text3); margin-top: 0.2rem; }
.msg-input-row { display: flex; gap: 0.5rem; padding: 0.75rem; border-top: 1px solid var(--t-border); background: var(--t-surface); }
.msg-input { flex: 1; padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 9px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; }
.msg-send-btn { padding: 0.5rem 1.1rem; border-radius: 9px; background: #4caf50; color: white; border: none; font-weight: 700; cursor: pointer; }
.msg-empty { flex: 1; display: flex; align-items: center; justify-content: center; color: var(--t-text3); font-size: 0.875rem; }
@media (max-width: 640px) { .msg-sidebar { width: 100%; } .msg-layout { flex-direction: column; } }
</style>
