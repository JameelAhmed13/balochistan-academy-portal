<template>
  <div class="jf-root">
    <AIHelper context="User is in a study group working on board exam preparation" :chips="['Study tips for this subject', 'Create a quiz question', 'Explain a concept', 'Group study schedule advice']" />

    <div class="jf-header">
      <h1 class="jf-title">⚔️ Join Forces</h1>
      <p class="jf-sub">Study together · Group challenges · Beat the exams as a team</p>
    </div>

    <!-- Tabs -->
    <div class="jf-tabs">
      <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id" class="jf-tab" :class="{active: activeTab===tab.id}">
        {{ tab.icon }} {{ tab.label }}
      </button>
    </div>

    <!-- Groups Tab -->
    <div v-if="activeTab === 'groups'">
      <div class="jf-actions">
        <button @click="showCreate = true" class="jf-create-btn">+ Create Group</button>
        <input v-model="searchQuery" placeholder="Search groups..." class="jf-search" />
      </div>

      <div class="jf-groups-list">
        <div v-for="group in filteredGroups" :key="group.id" class="jf-group-card">
          <div class="jf-group-header">
            <div class="jf-group-avatar" :style="{background: group.color}">{{ group.avatar }}</div>
            <div class="jf-group-info">
              <div class="jf-group-name">{{ group.name }}</div>
              <div class="jf-group-meta">
                <span>{{ group.subject }}</span>
                <span>·</span>
                <span>👥 {{ group.members }} members</span>
                <span>·</span>
                <span :class="group.active ? 'jf-active' : 'jf-inactive'">{{ group.active ? '🟢 Active' : '⚫ Quiet' }}</span>
              </div>
            </div>
            <div class="jf-group-btns">
              <button v-if="!joinedGroups.includes(group.id)" @click="joinGroup(group.id)" class="jf-join-btn">Join</button>
              <button v-else @click="openChat(group)" class="jf-chat-btn">💬 Chat</button>
            </div>
          </div>
          <div v-if="joinedGroups.includes(group.id)" class="jf-group-joined-bar">
            <span class="jf-joined-badge">✓ Member</span>
            <button @click="openChallenge(group)" class="jf-challenge-small-btn">⚔️ Group Challenge</button>
            <button @click="leaveGroup(group.id)" class="jf-leave-btn">Leave</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Chat Tab -->
    <div v-if="activeTab === 'chat'" class="jf-chat-view">
      <div v-if="!chatGroup" class="jf-chat-empty">
        <div class="jf-chat-empty-icon">💬</div>
        <div>Join a group first, then tap "Chat" to start chatting</div>
        <button @click="activeTab='groups'" class="jf-btn-secondary">← Back to Groups</button>
      </div>
      <div v-else class="jf-chat-panel">
        <div class="jf-chat-header">
          <button @click="chatGroup=null; activeTab='groups'" class="jf-chat-back">←</button>
          <div class="jf-chat-grp-avatar" :style="{background: chatGroup.color}">{{ chatGroup.avatar }}</div>
          <div>
            <div class="jf-chat-grp-name">{{ chatGroup.name }}</div>
            <div class="jf-chat-grp-meta">{{ chatGroup.members }} members</div>
          </div>
          <button @click="openChallenge(chatGroup)" class="jf-chat-challenge-btn">⚔️ Challenge</button>
        </div>
        <div class="jf-chat-messages" ref="msgArea">
          <div v-for="msg in chatMessages" :key="msg.id" class="jf-msg" :class="{mine: msg.isMine}">
            <div v-if="!msg.isMine" class="jf-msg-avatar">{{ msg.from[0] }}</div>
            <div class="jf-msg-bubble">
              <div v-if="!msg.isMine" class="jf-msg-from">{{ msg.from }}</div>
              <div class="jf-msg-text">{{ msg.text }}</div>
              <div class="jf-msg-time">{{ msg.time }}</div>
            </div>
          </div>
        </div>
        <div class="jf-chat-input-row">
          <input v-model="chatInput" @keyup.enter="sendMessage" placeholder="Type a message..." class="jf-chat-input" />
          <button @click="askAIForGroup" class="jf-ai-chat-btn" title="Ask AI">🤖</button>
          <button @click="sendMessage" class="jf-send-btn">→</button>
        </div>
      </div>
    </div>

    <!-- Challenges Tab -->
    <div v-if="activeTab === 'challenges'" class="jf-challenges-view">
      <div class="jf-challenge-intro">
        <div class="jf-challenge-icon">⚔️</div>
        <div class="jf-challenge-title">Group Challenges</div>
        <div class="jf-challenge-sub">Challenge your group members and see who scores the highest!</div>
      </div>
      <div class="jf-active-challenges">
        <div v-for="ch in activeChallenges" :key="ch.id" class="jf-challenge-card">
          <div class="jf-ch-header">
            <div class="jf-ch-icon">⚔️</div>
            <div>
              <div class="jf-ch-title">{{ ch.title }}</div>
              <div class="jf-ch-meta">{{ ch.group }} · {{ ch.subject }} · {{ ch.deadline }}</div>
            </div>
            <RouterLink to="/app/competition/challenge" class="jf-ch-join-btn">Join →</RouterLink>
          </div>
          <div class="jf-ch-leaderboard">
            <div v-for="(p,i) in ch.standings" :key="i" class="jf-ch-standing">
              <span class="jf-ch-rank">{{ ['🥇','🥈','🥉','4️⃣','5️⃣'][i] }}</span>
              <span class="jf-ch-pname">{{ p.name }}</span>
              <span class="jf-ch-pscore">{{ p.score }} pts</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Members Tab -->
    <div v-if="activeTab === 'members'" class="jf-members-view">
      <div class="jf-members-title">👥 Students Online Now</div>
      <div class="jf-members-list">
        <div v-for="m in onlineMembers" :key="m.id" class="jf-member-card">
          <div class="jf-member-avatar" :style="{background: m.color}">{{ m.initials }}</div>
          <div class="jf-member-info">
            <div class="jf-member-name">{{ m.name }}</div>
            <div class="jf-member-status">{{ m.status }}</div>
          </div>
          <div class="jf-member-online-dot" :class="{active: m.online}"/>
        </div>
      </div>
    </div>

    <!-- Create Group Dialog -->
    <div v-if="showCreate" class="jf-modal-overlay" @click.self="showCreate=false">
      <div class="jf-modal">
        <h3 class="jf-modal-title">Create Study Group</h3>
        <div class="jf-modal-field">
          <label>Group Name</label>
          <input v-model="newGroup.name" placeholder="e.g. Physics Champions" class="jf-modal-input" />
        </div>
        <div class="jf-modal-field">
          <label>Subject</label>
          <select v-model="newGroup.subject" class="jf-modal-input">
            <option value="">Select Subject</option>
            <option v-for="s in subjects" :key="s.id" :value="s.name">{{ s.name }}</option>
          </select>
        </div>
        <div class="jf-modal-field">
          <label>Description</label>
          <textarea v-model="newGroup.desc" placeholder="What will your group study?" class="jf-modal-input" rows="2"/>
        </div>
        <div class="jf-modal-btns">
          <button @click="showCreate=false" class="jf-btn-secondary">Cancel</button>
          <button @click="createGroup" :disabled="!newGroup.name || !newGroup.subject" class="jf-btn-primary">Create Group</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue'
import { SUBJECTS } from '@/stores/content'
import AIHelper from '@/components/platform/AIHelper.vue'

const subjects = SUBJECTS || []
const activeTab = ref('groups')
const searchQuery = ref('')
const showCreate = ref(false)
const chatGroup = ref(null)
const chatInput = ref('')
const msgArea = ref(null)

const tabs = [
  { id:'groups', icon:'👥', label:'Groups' },
  { id:'chat', icon:'💬', label:'Chat' },
  { id:'challenges', icon:'⚔️', label:'Challenges' },
  { id:'members', icon:'🌐', label:'Members' },
]

const newGroup = ref({ name:'', subject:'', desc:'' })
const joinedGroups = ref(JSON.parse(localStorage.getItem('bap_joined_groups') || '[]'))
const userGroups = ref(JSON.parse(localStorage.getItem('bap_user_groups') || '[]'))

const allGroups = computed(() => [
  { id:1, name:'Physics Legends', subject:'Physics', avatar:'⚡', color:'linear-gradient(135deg,#6d54e8,#a855f7)', members:28, active:true },
  { id:2, name:'Chemistry Stars', subject:'Chemistry', avatar:'⚗️', color:'linear-gradient(135deg,#4caf50,#00bcd4)', members:19, active:true },
  { id:3, name:'Bio Warriors', subject:'Biology', avatar:'🔬', color:'linear-gradient(135deg,#10b981,#3b82f6)', members:34, active:false },
  { id:4, name:'Math Masters', subject:'Mathematics', avatar:'📐', color:'linear-gradient(135deg,#f59e0b,#ef4444)', members:41, active:true },
  { id:5, name:'Pakistan Studies', subject:'Pakistan Studies', avatar:'🇵🇰', color:'linear-gradient(135deg,#01411c,#2d6a4f)', members:22, active:false },
  { id:6, name:'English Pros', subject:'English', avatar:'📝', color:'linear-gradient(135deg,#ec4899,#8b5cf6)', members:17, active:true },
  ...userGroups.value,
])

const filteredGroups = computed(()=> searchQuery.value ? allGroups.value.filter(g=>g.name.toLowerCase().includes(searchQuery.value.toLowerCase()) || g.subject.toLowerCase().includes(searchQuery.value.toLowerCase())) : allGroups.value)

function joinGroup(id) {
  if (!joinedGroups.value.includes(id)) joinedGroups.value.push(id)
  localStorage.setItem('bap_joined_groups', JSON.stringify(joinedGroups.value))
}
function leaveGroup(id) {
  joinedGroups.value = joinedGroups.value.filter(i=>i!==id)
  localStorage.setItem('bap_joined_groups', JSON.stringify(joinedGroups.value))
}
function createGroup() {
  const id = Date.now()
  const colors = ['linear-gradient(135deg,#6d54e8,#a855f7)','linear-gradient(135deg,#4caf50,#00bcd4)','linear-gradient(135deg,#f59e0b,#ef4444)']
  const grp = { id, name: newGroup.value.name, subject: newGroup.value.subject, avatar: '⭐', color: colors[Math.floor(Math.random()*colors.length)], members: 1, active: true, isOwner: true }
  userGroups.value.push(grp)
  localStorage.setItem('bap_user_groups', JSON.stringify(userGroups.value))
  joinedGroups.value.push(id)
  localStorage.setItem('bap_joined_groups', JSON.stringify(joinedGroups.value))
  newGroup.value = { name:'', subject:'', desc:'' }
  showCreate.value = false
}

const CHAT_KEY_PREFIX = 'bap_chat_'
function openChat(group) { chatGroup.value = group; activeTab.value = 'chat' }
function openChallenge(group) { activeTab.value = 'challenges' }

const chatMessages = computed(() => {
  if (!chatGroup.value) return []
  const key = CHAT_KEY_PREFIX + chatGroup.value.id
  return JSON.parse(localStorage.getItem(key) || '[]')
})

function sendMessage() {
  if (!chatInput.value.trim() || !chatGroup.value) return
  const key = CHAT_KEY_PREFIX + chatGroup.value.id
  const msgs = JSON.parse(localStorage.getItem(key) || '[]')
  msgs.push({ id: Date.now(), from: 'You', text: chatInput.value.trim(), time: new Date().toLocaleTimeString('en-PK',{hour:'2-digit',minute:'2-digit'}), isMine: true })
  localStorage.setItem(key, JSON.stringify(msgs.slice(-50)))
  chatInput.value = ''
  nextTick(()=>{ if(msgArea.value) msgArea.value.scrollTop = msgArea.value.scrollHeight })
}

async function askAIForGroup() {
  if (!chatGroup.value) return
  const key = CHAT_KEY_PREFIX + chatGroup.value.id
  const msgs = JSON.parse(localStorage.getItem(key) || '[]')
  msgs.push({ id: Date.now()-1, from: 'You', text: `@AI Give a study tip for ${chatGroup.value.subject}`, time: new Date().toLocaleTimeString('en-PK',{hour:'2-digit',minute:'2-digit'}), isMine: true })
  try {
    const k = import.meta.env.VITE_GEMINI_API_KEY
    if (!k) throw new Error()
    const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=${k}`, {
      method:'POST', headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ contents:[{ parts:[{ text:`Give one concise study tip for ${chatGroup.value.subject} for Pakistani board exams. Make it practical and actionable. Under 3 sentences.` }] }] })
    })
    const data = await res.json()
    const reply = data.candidates?.[0]?.content?.parts?.[0]?.text || 'AI unavailable.'
    msgs.push({ id: Date.now(), from: '🤖 AI Tutor', text: reply, time: new Date().toLocaleTimeString('en-PK',{hour:'2-digit',minute:'2-digit'}), isMine: false })
  } catch {
    msgs.push({ id: Date.now(), from: '🤖 AI Tutor', text: 'AI unavailable. Check your API key.', time: new Date().toLocaleTimeString('en-PK',{hour:'2-digit',minute:'2-digit'}), isMine: false })
  }
  localStorage.setItem(key, JSON.stringify(msgs.slice(-50)))
  nextTick(()=>{ if(msgArea.value) msgArea.value.scrollTop = msgArea.value.scrollHeight })
}

const activeChallenges = [
  { id:1, title:'Physics Speed Round', group:'Physics Legends', subject:'Physics', deadline:'Ends today', standings:[{name:'Ahmed',score:95},{name:'Sara',score:88},{name:'Ali',score:76},{name:'Fatima',score:70},{name:'You',score:0}] },
  { id:2, title:'Chemistry Quick Fire', group:'Chemistry Stars', subject:'Chemistry', deadline:'Ends tomorrow', standings:[{name:'Usman',score:90},{name:'Ayesha',score:82},{name:'Bilal',score:75},{name:'You',score:0}] },
]

const onlineMembers = [
  { id:1, name:'Ahmed Hassan', initials:'AH', color:'linear-gradient(135deg,#6d54e8,#a855f7)', status:'Studying Physics', online:true },
  { id:2, name:'Sara Khan', initials:'SK', color:'linear-gradient(135deg,#4caf50,#00bcd4)', status:'Taking a test', online:true },
  { id:3, name:'Ali Raza', initials:'AR', color:'linear-gradient(135deg,#f59e0b,#ef4444)', status:'Chemistry Chapter 3', online:true },
  { id:4, name:'Fatima Malik', initials:'FM', color:'linear-gradient(135deg,#ec4899,#8b5cf6)', status:'Last seen 20 min ago', online:false },
  { id:5, name:'Usman Tariq', initials:'UT', color:'linear-gradient(135deg,#10b981,#3b82f6)', status:'Reviewing notes', online:true },
  { id:6, name:'Ayesha Noor', initials:'AN', color:'linear-gradient(135deg,#f97316,#eab308)', status:'Last seen 1 hour ago', online:false },
]
</script>

<style scoped>
.jf-root { max-width: 800px; margin: 0 auto; padding: 1.5rem; }
.jf-header { text-align: center; padding: 1.5rem 0; }
.jf-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.jf-sub { color: var(--t-text3); font-size: 0.875rem; }
.jf-tabs { display: flex; gap: 0.25rem; background: var(--t-hover); border-radius: 14px; padding: 0.25rem; margin-bottom: 1.25rem; overflow-x: auto; }
.jf-tab { flex: 1; padding: 0.55rem 0.75rem; border-radius: 10px; border: none; background: none; color: var(--t-text2); font-size: 0.82rem; font-weight: 600; cursor: pointer; white-space: nowrap; }
.jf-tab.active { background: var(--t-surface); color: var(--t-text1); box-shadow: 0 1px 4px rgba(0,0,0,0.1); }
.jf-actions { display: flex; gap: 0.75rem; margin-bottom: 1rem; }
.jf-create-btn { padding: 0.55rem 1.1rem; background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; border: none; border-radius: 12px; font-weight: 700; cursor: pointer; white-space: nowrap; font-size: 0.875rem; }
.jf-search { flex: 1; padding: 0.55rem 1rem; border: 1px solid var(--t-border); border-radius: 12px; background: var(--t-surface); color: var(--t-text1); font-size: 0.875rem; }
.jf-groups-list { display: flex; flex-direction: column; gap: 0.75rem; }
.jf-group-card { border: 1px solid var(--t-border); border-radius: 16px; padding: 1rem; background: var(--t-surface); }
.jf-group-header { display: flex; align-items: center; gap: 0.85rem; }
.jf-group-avatar { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; flex-shrink: 0; }
.jf-group-info { flex: 1; min-width: 0; }
.jf-group-name { font-weight: 700; color: var(--t-text1); font-size: 0.95rem; }
.jf-group-meta { display: flex; gap: 0.4rem; font-size: 0.72rem; color: var(--t-text3); flex-wrap: wrap; }
.jf-active { color: #4caf50; }
.jf-inactive { color: var(--t-text3); }
.jf-group-btns { display: flex; gap: 0.4rem; flex-shrink: 0; }
.jf-join-btn { padding: 0.4rem 1rem; background: rgba(76,175,80,0.1); color: #4caf50; border: 1px solid rgba(76,175,80,0.3); border-radius: 99px; font-weight: 700; font-size: 0.8rem; cursor: pointer; }
.jf-chat-btn { padding: 0.4rem 0.85rem; background: rgba(109,84,232,0.1); color: #6d54e8; border: 1px solid rgba(109,84,232,0.3); border-radius: 99px; font-weight: 700; font-size: 0.8rem; cursor: pointer; }
.jf-group-joined-bar { display: flex; align-items: center; gap: 0.5rem; margin-top: 0.75rem; padding-top: 0.75rem; border-top: 1px solid var(--t-border); flex-wrap: wrap; }
.jf-joined-badge { font-size: 0.72rem; font-weight: 700; color: #4caf50; padding: 0.2rem 0.5rem; background: rgba(76,175,80,0.1); border-radius: 99px; }
.jf-challenge-small-btn { padding: 0.35rem 0.75rem; background: rgba(245,158,11,0.1); color: #f59e0b; border: 1px solid rgba(245,158,11,0.3); border-radius: 99px; font-size: 0.78rem; font-weight: 700; cursor: pointer; }
.jf-leave-btn { margin-left: auto; padding: 0.35rem 0.75rem; background: none; border: 1px solid var(--t-border); color: var(--t-text3); border-radius: 99px; font-size: 0.78rem; cursor: pointer; }
.jf-chat-view { height: 500px; display: flex; flex-direction: column; }
.jf-chat-empty { display: flex; flex-direction: column; align-items: center; gap: 0.75rem; padding: 3rem; text-align: center; color: var(--t-text2); }
.jf-chat-empty-icon { font-size: 3rem; }
.jf-chat-panel { display: flex; flex-direction: column; height: 100%; border: 1px solid var(--t-border); border-radius: 18px; overflow: hidden; }
.jf-chat-header { display: flex; align-items: center; gap: 0.75rem; padding: 0.85rem 1rem; border-bottom: 1px solid var(--t-border); background: var(--t-hover); }
.jf-chat-back { background: none; border: none; color: var(--t-text2); font-size: 1.25rem; cursor: pointer; }
.jf-chat-grp-avatar { width: 32px; height: 32px; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 1rem; flex-shrink: 0; }
.jf-chat-grp-name { font-weight: 700; font-size: 0.9rem; color: var(--t-text1); }
.jf-chat-grp-meta { font-size: 0.7rem; color: var(--t-text3); }
.jf-chat-challenge-btn { margin-left: auto; padding: 0.35rem 0.75rem; background: rgba(245,158,11,0.1); color: #f59e0b; border: 1px solid rgba(245,158,11,0.3); border-radius: 99px; font-size: 0.78rem; font-weight: 700; cursor: pointer; }
.jf-chat-messages { flex: 1; overflow-y: auto; padding: 1rem; display: flex; flex-direction: column; gap: 0.75rem; background: var(--t-bg); }
.jf-msg { display: flex; gap: 0.5rem; align-items: flex-end; }
.jf-msg.mine { flex-direction: row-reverse; }
.jf-msg-avatar { width: 28px; height: 28px; border-radius: 50%; background: var(--t-hover); color: var(--t-text2); font-size: 0.78rem; font-weight: 700; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.jf-msg-bubble { max-width: 75%; }
.jf-msg-from { font-size: 0.7rem; color: var(--t-text3); margin-bottom: 0.2rem; font-weight: 600; }
.jf-msg-text { padding: 0.6rem 0.85rem; border-radius: 14px; font-size: 0.85rem; line-height: 1.5; background: var(--t-surface); color: var(--t-text1); border: 1px solid var(--t-border); }
.jf-msg.mine .jf-msg-text { background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; border-color: transparent; }
.jf-msg-time { font-size: 0.65rem; color: var(--t-text3); margin-top: 0.2rem; text-align: right; }
.jf-chat-input-row { display: flex; gap: 0.4rem; padding: 0.75rem; border-top: 1px solid var(--t-border); }
.jf-chat-input { flex: 1; padding: 0.55rem 0.85rem; border: 1px solid var(--t-border); border-radius: 12px; background: var(--t-bg); color: var(--t-text1); font-size: 0.875rem; }
.jf-ai-chat-btn { width: 36px; height: 36px; border-radius: 50%; background: linear-gradient(135deg, rgba(109,84,232,0.15), rgba(168,85,247,0.15)); border: 1px solid rgba(109,84,232,0.3); cursor: pointer; font-size: 1rem; display: flex; align-items: center; justify-content: center; }
.jf-send-btn { width: 36px; height: 36px; border-radius: 50%; background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; border: none; cursor: pointer; font-size: 1rem; display: flex; align-items: center; justify-content: center; }
.jf-challenge-intro { text-align: center; padding: 1.5rem 0; }
.jf-challenge-icon { font-size: 3rem; margin-bottom: 0.5rem; }
.jf-challenge-title { font-size: 1.25rem; font-weight: 800; color: var(--t-text1); }
.jf-challenge-sub { color: var(--t-text3); font-size: 0.875rem; margin-top: 0.3rem; }
.jf-active-challenges { display: flex; flex-direction: column; gap: 1rem; }
.jf-challenge-card { border: 1px solid rgba(245,158,11,0.2); border-radius: 16px; padding: 1.25rem; background: rgba(245,158,11,0.03); }
.jf-ch-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; }
.jf-ch-icon { font-size: 1.75rem; flex-shrink: 0; }
.jf-ch-title { font-weight: 800; color: var(--t-text1); font-size: 0.95rem; }
.jf-ch-meta { font-size: 0.72rem; color: var(--t-text3); }
.jf-ch-join-btn { margin-left: auto; padding: 0.45rem 1rem; background: linear-gradient(135deg, #f59e0b, #ef4444); color: white; border: none; border-radius: 99px; font-weight: 700; font-size: 0.8rem; cursor: pointer; text-decoration: none; display: inline-flex; }
.jf-ch-leaderboard { display: flex; flex-direction: column; gap: 0.3rem; }
.jf-ch-standing { display: flex; align-items: center; gap: 0.6rem; padding: 0.3rem 0; }
.jf-ch-rank { width: 1.5rem; text-align: center; }
.jf-ch-pname { flex: 1; font-size: 0.85rem; color: var(--t-text1); }
.jf-ch-pscore { font-size: 0.82rem; font-weight: 700; color: var(--t-text2); }
.jf-members-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.75rem; }
.jf-members-list { display: flex; flex-direction: column; gap: 0.5rem; }
.jf-member-card { display: flex; align-items: center; gap: 0.85rem; padding: 0.75rem 1rem; border: 1px solid var(--t-border); border-radius: 14px; background: var(--t-surface); }
.jf-member-avatar { width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-weight: 800; font-size: 0.85rem; flex-shrink: 0; }
.jf-member-name { font-weight: 600; font-size: 0.9rem; color: var(--t-text1); }
.jf-member-status { font-size: 0.72rem; color: var(--t-text3); }
.jf-member-online-dot { width: 10px; height: 10px; border-radius: 50%; background: var(--t-border); margin-left: auto; flex-shrink: 0; }
.jf-member-online-dot.active { background: #4caf50; box-shadow: 0 0 0 3px rgba(76,175,80,0.2); }
.jf-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); backdrop-filter: blur(4px); z-index: 50; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.jf-modal { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 20px; padding: 1.5rem; width: 100%; max-width: 420px; }
.jf-modal-title { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); margin-bottom: 1.25rem; }
.jf-modal-field { margin-bottom: 1rem; }
.jf-modal-field label { display: block; font-size: 0.75rem; font-weight: 700; color: var(--t-text3); margin-bottom: 0.4rem; }
.jf-modal-input { width: 100%; padding: 0.55rem 0.85rem; border: 1px solid var(--t-border); border-radius: 10px; background: var(--t-bg); color: var(--t-text1); font-size: 0.9rem; resize: vertical; box-sizing: border-box; }
.jf-modal-btns { display: flex; gap: 0.6rem; justify-content: flex-end; margin-top: 1.25rem; }
.jf-btn-secondary { padding: 0.6rem 1.1rem; background: var(--t-hover); border: 1px solid var(--t-border); color: var(--t-text1); border-radius: 10px; font-weight: 700; cursor: pointer; font-size: 0.875rem; }
.jf-btn-primary { padding: 0.6rem 1.1rem; background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; border: none; border-radius: 10px; font-weight: 700; cursor: pointer; font-size: 0.875rem; }
.jf-btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }
</style>
