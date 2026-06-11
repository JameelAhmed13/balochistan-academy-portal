<template>
  <div class="animate-fade-in space-y-6">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">💬</div>
      <div class="ag-banner-title">Complaints &amp; Feedback</div>
      <div class="ag-banner-sub">Manage student feedback, complaints and suggestions</div>
      <div class="ag-banner-actions">
        <div class="ag-banner-stat">12 Open</div>
        <div class="ag-banner-stat">3 Pending</div>
        <div class="ag-banner-stat">28 Resolved</div>
        <RouterLink to="/app/admin" class="btn-ghost flex items-center gap-1.5 text-sm">
          <ArrowLeft class="w-4 h-4" /> Back
        </RouterLink>
      </div>
    </div>

    <!-- Stats Row -->
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
      <div v-for="s in stats" :key="s.label" class="ag-grid-card p-4 flex items-center gap-3">
        <div class="text-2xl">{{ s.emoji }}</div>
        <div>
          <div class="text-2xl font-bold text-[var(--t-text1)]">{{ s.value }}</div>
          <div class="text-xs text-[var(--t-text3)]">{{ s.label }}</div>
        </div>
      </div>
    </div>

    <!-- Filter Tabs -->
    <div class="ag-tabs">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        @click="activeTab = tab.value"
        :class="['ag-tab', activeTab === tab.value ? 'ag-tab--active' : '']"
      >
        {{ tab.label }}
        <span v-if="tab.count" class="ml-1 text-xs opacity-70">({{ tab.count }})</span>
      </button>
    </div>

    <!-- Complaints List -->
    <div class="space-y-3">
      <div
        v-for="c in filteredComplaints"
        :key="c.id"
        class="ag-grid-card overflow-hidden"
      >
        <!-- Card Header (always visible) -->
        <div class="p-4 flex items-start gap-3">
          <!-- Status dot + Avatar -->
          <div class="flex flex-col items-center gap-1.5 pt-0.5 shrink-0">
            <div class="w-2.5 h-2.5 rounded-full mt-1" :style="'background:' + statusColor(c.status)"></div>
            <div class="w-9 h-9 rounded-full flex items-center justify-center text-sm font-bold text-white"
              :style="'background:' + avatarColor(c.student)">
              {{ initials(c.student) }}
            </div>
          </div>

          <!-- Main Content -->
          <div class="flex-1 min-w-0">
            <div class="flex flex-wrap items-center gap-2">
              <span class="font-semibold text-sm text-[var(--t-text1)]">{{ c.student }}</span>
              <span class="text-xs text-[var(--t-text3)]">{{ c.date }}</span>
              <span :class="'badge-' + categoryBadge(c.category) + ' text-xs'">{{ c.category }}</span>
            </div>
            <div class="font-semibold text-[var(--t-text1)] mt-0.5">{{ c.title }}</div>
            <div class="text-sm text-[var(--t-text2)] mt-0.5 line-clamp-2">{{ c.text }}</div>
          </div>

          <!-- Right side: badges + actions -->
          <div class="flex flex-col items-end gap-2 shrink-0 ml-2">
            <div class="flex items-center gap-1.5">
              <span :class="'badge-' + priorityBadge(c.priority) + ' text-xs'">{{ c.priority }}</span>
              <span :class="'badge-' + statusBadge(c.status) + ' text-xs'">{{ c.status }}</span>
            </div>
            <div class="flex items-center gap-1.5 mt-1">
              <button
                class="btn-primary text-xs px-2.5 py-1 flex items-center gap-1"
                @click.stop="openReplyModal(c)"
              >
                <Send class="w-3 h-3" /> Reply
              </button>
              <button
                v-if="c.status !== 'Resolved'"
                class="btn-secondary text-xs px-2.5 py-1 flex items-center gap-1"
                @click.stop="resolveComplaint(c)"
              >
                <CheckCircle class="w-3 h-3" /> Resolve
              </button>
            </div>
            <button
              class="flex items-center gap-1 text-xs text-[var(--t-text3)] mt-1 hover:text-[var(--t-text1)] transition-colors"
              @click="toggleExpand(c.id)"
            >
              <component :is="expandedId === c.id ? ChevronUp : ChevronDown" class="w-3.5 h-3.5" />
              {{ expandedId === c.id ? 'Collapse' : 'View Details' }}
            </button>
          </div>
        </div>

        <!-- Expanded Section -->
        <Transition name="expand">
          <div v-if="expandedId === c.id" class="border-t px-4 py-4 space-y-3"
            style="border-color:var(--t-border);background:var(--t-hover);">
            <div>
              <div class="text-xs font-semibold text-[var(--t-text3)] uppercase tracking-wide mb-1">Full Message</div>
              <p class="text-sm text-[var(--t-text1)]">{{ c.text }}</p>
            </div>
            <div v-if="c.adminReply">
              <div class="text-xs font-semibold text-[var(--t-text3)] uppercase tracking-wide mb-1">Admin Reply</div>
              <p class="text-sm text-[var(--t-text2)] italic">{{ c.adminReply }}</p>
            </div>
            <div>
              <div class="text-xs font-semibold text-[var(--t-text3)] uppercase tracking-wide mb-1">Quick Reply</div>
              <div class="flex gap-2">
                <input
                  v-model="quickReplies[c.id]"
                  class="input flex-1 text-sm"
                  placeholder="Type a quick reply…"
                  @keyup.enter="submitQuickReply(c)"
                />
                <button class="btn-primary flex items-center gap-1.5 text-sm" @click="submitQuickReply(c)">
                  <Send class="w-3.5 h-3.5" /> Send
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>

      <!-- Empty state -->
      <div v-if="filteredComplaints.length === 0" class="ag-empty">
        <MessageSquare class="w-10 h-10 mx-auto mb-2 opacity-30" />
        <div>No complaints found for this filter.</div>
      </div>
    </div>

    <!-- Reply Modal -->
    <Dialog v-model:visible="replyModal.visible" modal :header="'Reply: ' + (replyModal.complaint?.title || '')"
      :style="{ width: '520px', maxWidth: '95vw' }" :pt="{ mask: { style: 'backdrop-filter:blur(4px)' } }">
      <div v-if="replyModal.complaint" class="space-y-4 pt-2">
        <div class="p-3 rounded-xl text-sm text-[var(--t-text2)]" style="background:var(--t-hover);">
          {{ replyModal.complaint.text }}
        </div>
        <div>
          <label class="label">Your Reply</label>
          <textarea v-model="replyText" class="input mt-1 resize-none" rows="4"
            placeholder="Write your reply to the student…"></textarea>
        </div>
        <label class="flex items-center gap-2.5 cursor-pointer text-sm text-[var(--t-text2)]">
          <input type="checkbox" v-model="resolveOnReply" class="w-4 h-4 rounded accent-[var(--t-accent)]" />
          Mark as Resolved after replying
        </label>
        <div class="flex gap-3 pt-1">
          <button class="btn-primary flex items-center gap-2" @click="submitReply">
            <Send class="w-4 h-4" /> Send Reply
          </button>
          <button class="btn-ghost" @click="replyModal.visible = false">Cancel</button>
        </div>
      </div>
    </Dialog>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { ArrowLeft, MessageSquare, CheckCircle, Clock, AlertCircle, ChevronDown, ChevronUp, Send, Flag } from '@lucide/vue'
import Dialog from 'primevue/dialog'

const activeTab = ref('all')
const expandedId = ref(null)
const replyText = ref('')
const resolveOnReply = ref(false)
const quickReplies = ref({})

const replyModal = ref({ visible: false, complaint: null })

const tabs = [
  { value: 'all',        label: 'All',         count: 43 },
  { value: 'Open',       label: 'Open',        count: 12 },
  { value: 'Pending',    label: 'Pending',     count: 3 },
  { value: 'Resolved',   label: 'Resolved',    count: 28 },
  { value: 'Suggestion', label: 'Suggestions', count: null },
]

const stats = [
  { emoji: '🔴', value: 12, label: 'Open' },
  { emoji: '🟡', value: 3,  label: 'Pending' },
  { emoji: '🟢', value: 28, label: 'Resolved' },
  { emoji: '📋', value: 43, label: 'Total' },
]

const complaints = ref([
  { id: 1, student: 'Ali Hassan',    date: '10 Jun 2026', category: 'Complaint', title: 'Cannot access Physics MCQs', text: 'I have been trying to access the Physics Chapter 4 MCQs but the page keeps loading indefinitely. I have cleared my browser cache and tried on mobile too but the issue persists.', status: 'Open',     priority: 'High',   adminReply: null },
  { id: 2, student: 'Sara Malik',    date: '9 Jun 2026',  category: 'Bug',       title: 'Timer resets mid-test',     text: 'During my weekly test session the countdown timer reset itself back to the full time. This happened twice and it feels unfair to other students.', status: 'Pending',  priority: 'High',   adminReply: 'We are investigating the timer issue. It may be related to a recent update. We will push a fix within 24 hours.' },
  { id: 3, student: 'Umar Farooq',   date: '9 Jun 2026',  category: 'Suggestion',title: 'Add dark mode option',      text: 'The platform is great but studying at night is hard on the eyes. Please add a proper dark mode toggle. Many students study late and this would be very helpful.', status: 'Open',     priority: 'Normal', adminReply: null },
  { id: 4, student: 'Fatima Zahra',  date: '8 Jun 2026',  category: 'Content',   title: 'Wrong answer in Biology Q', text: 'In Biology Chapter 3, Question 12 about cell division — the marked correct answer is Mitosis but according to my textbook the answer should be Meiosis for that specific scenario.', status: 'Resolved', priority: 'High',   adminReply: 'Thank you for reporting this. We have verified and corrected the answer. Your attention to detail helps everyone.' },
  { id: 5, student: 'Bilal Ahmed',   date: '7 Jun 2026',  category: 'Complaint', title: 'Coins not credited after test', text: 'I scored 95% in the Chemistry test on June 5th but my coin balance did not update. I should have received at least 15 coins based on the coin economy rules.', status: 'Pending',  priority: 'Normal', adminReply: 'We have raised a ticket to the backend team. You will be credited within 1 business day.' },
  { id: 6, student: 'Ayesha Noor',   date: '6 Jun 2026',  category: 'Suggestion',title: 'Offline PDF download for notes', text: 'It would be really useful to download notes as PDFs for offline study, especially for students who have limited internet access in rural areas.', status: 'Open',     priority: 'Low',    adminReply: null },
  { id: 7, student: 'Hamza Qureshi', date: '5 Jun 2026',  category: 'Bug',       title: 'AI Tutor gives wrong answers', text: 'The AI Tutor gave me an incorrect explanation for Newton\'s Third Law and when I asked a follow-up it contradicted itself. The answers seem unreliable for Physics topics.', status: 'Open',     priority: 'High',   adminReply: null },
  { id: 8, student: 'Zara Khan',     date: '4 Jun 2026',  category: 'Complaint', title: 'Subscription payment issue',   text: 'I paid for the annual subscription via EasyPaisa on June 3rd. The payment was deducted but my account still shows as a free user. Transaction ID: EP2026060312345.', status: 'Resolved', priority: 'High',   adminReply: 'We have verified your payment and manually activated your premium subscription. Sorry for the inconvenience.' },
])

const filteredComplaints = computed(() => {
  if (activeTab.value === 'all') return complaints.value
  if (activeTab.value === 'Suggestion') return complaints.value.filter(c => c.category === 'Suggestion')
  return complaints.value.filter(c => c.status === activeTab.value)
})

function toggleExpand(id) {
  expandedId.value = expandedId.value === id ? null : id
}

function openReplyModal(c) {
  replyModal.value.complaint = c
  replyText.value = c.adminReply || ''
  resolveOnReply.value = false
  replyModal.value.visible = true
}

function submitReply() {
  if (!replyText.value.trim()) return
  const c = complaints.value.find(x => x.id === replyModal.value.complaint.id)
  if (c) {
    c.adminReply = replyText.value
    if (resolveOnReply.value) c.status = 'Resolved'
  }
  replyModal.value.visible = false
  replyText.value = ''
}

function submitQuickReply(c) {
  const text = quickReplies.value[c.id]
  if (!text?.trim()) return
  const target = complaints.value.find(x => x.id === c.id)
  if (target) target.adminReply = text
  quickReplies.value[c.id] = ''
}

function resolveComplaint(c) {
  const target = complaints.value.find(x => x.id === c.id)
  if (target) target.status = 'Resolved'
}

function initials(name) {
  return name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
}

function avatarColor(name) {
  const colors = ['#6366f1','#8b5cf6','#ec4899','#14b8a6','#f59e0b','#10b981','#3b82f6','#ef4444']
  let hash = 0
  for (const ch of name) hash = (hash * 31 + ch.charCodeAt(0)) & 0xffff
  return colors[hash % colors.length]
}

function statusColor(status) {
  return { Open: '#ef4444', Pending: '#f59e0b', Resolved: '#10b981' }[status] || '#6b7280'
}

function statusBadge(status) {
  return { Open: 'red', Pending: 'yellow', Resolved: 'green' }[status] || 'blue'
}

function priorityBadge(p) {
  return { High: 'red', Normal: 'blue', Low: 'green' }[p] || 'blue'
}

function categoryBadge(cat) {
  return { Complaint: 'red', Suggestion: 'blue', Bug: 'yellow', Content: 'purple' }[cat] || 'blue'
}
</script>

<style scoped>
.expand-enter-active, .expand-leave-active {
  transition: all 0.25s ease;
  overflow: hidden;
}
.expand-enter-from, .expand-leave-to {
  opacity: 0;
  max-height: 0;
}
.expand-enter-to, .expand-leave-from {
  opacity: 1;
  max-height: 400px;
}
</style>
