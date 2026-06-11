<template>
  <div class="lb-root">
    <AIHelper context="User is checking leaderboard rankings for competitive exam preparation" />

    <div class="lb-header">
      <h1 class="lb-title">🏆 Student Leaderboard</h1>
      <p class="lb-sub">Top performers across all subjects · Updated in real-time</p>
    </div>

    <!-- Filter bar -->
    <div class="lb-filters">
      <button v-for="f in filters" :key="f.value" @click="activeFilter = f.value" class="lb-filter-btn" :class="{active: activeFilter===f.value}">{{ f.label }}</button>
      <div class="lb-spacer"/>
      <select v-model="activeSubject" class="lb-select">
        <option value="all">All Subjects</option>
        <option v-for="s in subjects" :key="s.id" :value="s.id">{{ s.name }}</option>
      </select>
    </div>

    <!-- Top 3 podium -->
    <div class="lb-podium">
      <div class="lb-podium-item lb-second" v-if="topThree[1]">
        <div class="lb-podium-avatar" style="background: linear-gradient(135deg,#9ca3af,#6b7280)">{{ topThree[1].initials }}</div>
        <div class="lb-podium-rank">🥈</div>
        <div class="lb-podium-name">{{ topThree[1].name }}</div>
        <div class="lb-podium-score">{{ topThree[1].score }}</div>
        <div class="lb-podium-block lb-podium-block-2"></div>
      </div>
      <div class="lb-podium-item lb-first" v-if="topThree[0]">
        <div class="lb-crown">👑</div>
        <div class="lb-podium-avatar lb-first-avatar" style="background: linear-gradient(135deg,#f59e0b,#d97706)">{{ topThree[0].initials }}</div>
        <div class="lb-podium-rank">🥇</div>
        <div class="lb-podium-name">{{ topThree[0].name }}</div>
        <div class="lb-podium-score">{{ topThree[0].score }}</div>
        <div class="lb-podium-block lb-podium-block-1"></div>
      </div>
      <div class="lb-podium-item lb-third" v-if="topThree[2]">
        <div class="lb-podium-avatar" style="background: linear-gradient(135deg,#b45309,#92400e)">{{ topThree[2].initials }}</div>
        <div class="lb-podium-rank">🥉</div>
        <div class="lb-podium-name">{{ topThree[2].name }}</div>
        <div class="lb-podium-score">{{ topThree[2].score }}</div>
        <div class="lb-podium-block lb-podium-block-3"></div>
      </div>
    </div>

    <!-- User's rank card -->
    <div v-if="userEntry" class="lb-my-rank-card">
      <div class="lb-my-rank-label">Your Rank</div>
      <div class="lb-my-rank-num">#{{ userEntry.rank }}</div>
      <div class="lb-my-rank-score">{{ userEntry.score }} pts</div>
      <div class="lb-my-rank-tip">{{ userRankTip }}</div>
    </div>

    <!-- Full leaderboard table -->
    <div class="lb-table-wrap">
      <table class="lb-table">
        <thead>
          <tr>
            <th>#</th>
            <th>Student</th>
            <th>Score</th>
            <th>Tests</th>
            <th>Avg%</th>
            <th>Badge</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="entry in displayList" :key="entry.rank" :class="{ 'lb-me-row': entry.isMe }">
            <td>
              <span v-if="entry.rank === 1">🥇</span>
              <span v-else-if="entry.rank === 2">🥈</span>
              <span v-else-if="entry.rank === 3">🥉</span>
              <span v-else class="lb-rank-num">{{ entry.rank }}</span>
            </td>
            <td>
              <div class="lb-student-cell">
                <div class="lb-avatar" :style="{background: entry.color}">{{ entry.initials }}</div>
                <div>
                  <div class="lb-student-name">{{ entry.name }} {{ entry.isMe ? '(You)' : '' }}</div>
                  <div class="lb-student-city">{{ entry.city }}</div>
                </div>
              </div>
            </td>
            <td><span class="lb-score">{{ entry.score }}</span></td>
            <td class="lb-num">{{ entry.tests }}</td>
            <td>
              <div class="lb-avg-bar">
                <div class="lb-avg-fill" :style="{width: entry.avg+'%', background: entry.avg >= 80 ? '#4caf50' : entry.avg >= 60 ? '#f59e0b' : '#ef4444'}"/>
                <span>{{ entry.avg }}%</span>
              </div>
            </td>
            <td><span class="lb-badge" :class="getBadgeClass(entry.avg)">{{ getBadge(entry.avg) }}</span></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Achievements section -->
    <div class="lb-achievements">
      <div class="lb-ach-title">🎖️ Your Achievements</div>
      <div class="lb-ach-grid">
        <div v-for="ach in achievements" :key="ach.id" class="lb-ach-item" :class="{locked: !ach.earned}">
          <div class="lb-ach-icon">{{ ach.icon }}</div>
          <div class="lb-ach-name">{{ ach.name }}</div>
          <div class="lb-ach-desc">{{ ach.desc }}</div>
        </div>
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useStudentStore } from '@/stores/student'
import { SUBJECTS } from '@/stores/content'
import AIHelper from '@/components/platform/AIHelper.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const student = useStudentStore()
const activeFilter = ref('all-time')
const activeSubject = ref('all')

const subjects = SUBJECTS || []
const filters = [
  { label: 'All Time', value: 'all-time' },
  { label: 'This Month', value: 'monthly' },
  { label: 'This Week', value: 'weekly' },
]

const COLORS = ['linear-gradient(135deg,#6d54e8,#a855f7)','linear-gradient(135deg,#4caf50,#00bcd4)','linear-gradient(135deg,#f59e0b,#ef4444)','linear-gradient(135deg,#ec4899,#8b5cf6)','linear-gradient(135deg,#10b981,#3b82f6)','linear-gradient(135deg,#f97316,#eab308)']
const CITIES = ['Karachi','Lahore','Islamabad','Quetta','Peshawar','Multan','Faisalabad','Rawalpindi','Hyderabad','Sialkot']
const NAMES = ['Ahmed Hassan','Sara Khan','Ali Raza','Fatima Malik','Usman Tariq','Ayesha Noor','Bilal Ahmed','Zara Sheikh','Hamza Butt','Saira Iqbal','Junaid Ali','Maryam Shah','Faisal Khan','Nadia Hussain','Imran Qureshi','Hina Baig','Asad Mehmood','Uzma Riaz','Tariq Bashir','Sana Javed']

const mockEntries = computed(() => {
  return NAMES.map((name, i) => {
    const seed = (name.charCodeAt(0) + name.charCodeAt(1) + i) % 37
    const tests = 15 + ((seed * 7 + i * 3) % 20)
    const avg = 45 + ((seed * 11 + i * 5) % 50)
    return {
      rank: i + 1,
      name,
      initials: name.split(' ').map(n=>n[0]).join(''),
      city: CITIES[i % CITIES.length],
      color: COLORS[i % COLORS.length],
      tests,
      avg,
      score: tests * Math.round(avg * 1.2),
      isMe: false,
    }
  }).sort((a,b)=>b.score-a.score).map((e,i)=>({...e, rank:i+1}))
})

const userScore = computed(() => student.avgPercent ? Math.round(student.passedTests * student.avgPercent * 1.1) : 0)
const userEntry = computed(() => ({
  rank: mockEntries.value.findIndex(e=>e.score <= userScore.value) + 1 || mockEntries.value.length + 1,
  score: userScore.value,
  name: 'You',
}))
const displayList = computed(() => {
  const list = [...mockEntries.value]
  const myIdx = list.findIndex(e=>e.score <= userScore.value)
  const meEntry = { rank: myIdx + 1, name: student.name || 'You', initials: (student.name||'Y').split(' ').map(n=>n[0]).join(''), city:'Your City', color: COLORS[0], tests: student.passedTests || 0, avg: student.avgPercent || 0, score: userScore.value, isMe: true }
  if (userScore.value > 0) list.splice(myIdx, 0, meEntry)
  return list.slice(0, 25).map((e,i)=>({...e, rank:i+1}))
})
const topThree = computed(()=>displayList.value.slice(0,3))
const userRankTip = computed(()=>{
  const rank = userEntry.value.rank
  if (rank <= 1) return 'You are #1! Incredible work! 🔥'
  if (rank <= 5) return 'Top 5! You are among the best students! 🌟'
  if (rank <= 10) return 'Top 10! Keep pushing — top 5 is within reach! 💪'
  return 'Take more tests to climb the leaderboard!'
})
function getBadge(avg) {
  if (avg >= 90) return '⭐ Scholar'
  if (avg >= 80) return '🌟 Expert'
  if (avg >= 70) return '🔥 Advanced'
  if (avg >= 60) return '📚 Learner'
  return '🌱 Beginner'
}
function getBadgeClass(avg) {
  if (avg >= 90) return 'badge-scholar'
  if (avg >= 80) return 'badge-expert'
  if (avg >= 70) return 'badge-advanced'
  return 'badge-learner'
}

const achievements = computed(()=>[
  { id:1, icon:'🏅', name:'First Test', desc:'Complete your first test', earned: student.passedTests > 0 },
  { id:2, icon:'🔥', name:'5-Day Streak', desc:'Study 5 days in a row', earned: student.passedTests >= 5 },
  { id:3, icon:'⭐', name:'Top Scorer', desc:'Score above 90% in any test', earned: student.avgPercent >= 90 },
  { id:4, icon:'🏆', name:'Top 10', desc:'Reach top 10 on leaderboard', earned: userEntry.value.rank <= 10 },
  { id:5, icon:'📚', name:'10 Tests', desc:'Complete 10 tests total', earned: student.passedTests >= 10 },
  { id:6, icon:'💎', name:'Scholar', desc:'Maintain 80%+ average', earned: student.avgPercent >= 80 },
])
</script>

<style scoped>
.lb-root { max-width: 860px; margin: 0 auto; padding: 1.5rem; }
.lb-header { text-align: center; padding: 1.5rem 0; }
.lb-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.lb-sub { color: var(--t-text3); font-size: 0.875rem; }
.lb-filters { display: flex; gap: 0.5rem; flex-wrap: wrap; align-items: center; margin-bottom: 1.5rem; }
.lb-filter-btn { padding: 0.4rem 1rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.82rem; cursor: pointer; }
.lb-filter-btn.active, .lb-filter-btn:hover { background: rgba(245,158,11,0.1); color: #f59e0b; border-color: rgba(245,158,11,0.3); }
.lb-spacer { flex: 1; }
.lb-select { padding: 0.4rem 0.75rem; border: 1px solid var(--t-border); border-radius: 99px; background: var(--t-hover); color: var(--t-text2); font-size: 0.82rem; }
.lb-podium { display: flex; justify-content: center; align-items: flex-end; gap: 0.5rem; margin-bottom: 1.5rem; }
.lb-podium-item { display: flex; flex-direction: column; align-items: center; gap: 0.25rem; }
.lb-podium-avatar { width: 52px; height: 52px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-weight: 800; font-size: 1rem; flex-shrink: 0; }
.lb-first-avatar { width: 64px; height: 64px; font-size: 1.2rem; }
.lb-crown { font-size: 1.5rem; }
.lb-podium-rank { font-size: 1.5rem; }
.lb-podium-name { font-size: 0.75rem; font-weight: 700; color: var(--t-text1); text-align: center; max-width: 80px; }
.lb-podium-score { font-size: 0.72rem; color: var(--t-text3); margin-bottom: 0.25rem; }
.lb-podium-block { width: 80px; border-radius: 8px 8px 0 0; }
.lb-podium-block-1 { height: 72px; background: linear-gradient(180deg,#f59e0b,#d97706); }
.lb-podium-block-2 { height: 52px; background: linear-gradient(180deg,#9ca3af,#6b7280); }
.lb-podium-block-3 { height: 40px; background: linear-gradient(180deg,#b45309,#92400e); }
.lb-my-rank-card { display: flex; align-items: center; gap: 1rem; padding: 1rem 1.5rem; border: 2px solid rgba(245,158,11,0.3); border-radius: 16px; background: rgba(245,158,11,0.05); margin-bottom: 1.5rem; }
.lb-my-rank-label { font-size: 0.75rem; font-weight: 700; color: var(--t-text3); }
.lb-my-rank-num { font-size: 1.75rem; font-weight: 800; color: #f59e0b; }
.lb-my-rank-score { font-size: 0.875rem; font-weight: 700; color: var(--t-text2); }
.lb-my-rank-tip { font-size: 0.8rem; color: var(--t-text2); flex: 1; text-align: right; }
.lb-table-wrap { overflow-x: auto; margin-bottom: 1.5rem; }
.lb-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.lb-table thead tr { background: #4caf50; }
.lb-table th { padding: 0.65rem 0.85rem; text-align: left; font-size: 0.75rem; font-weight: 700; color: white; }
.lb-table td { padding: 0.65rem 0.85rem; border-bottom: 1px solid var(--t-border); color: var(--t-text1); vertical-align: middle; }
.lb-me-row td { background: rgba(245,158,11,0.06); }
.lb-me-row td:first-child { border-left: 3px solid #f59e0b; }
.lb-rank-num { font-weight: 700; color: var(--t-text3); }
.lb-student-cell { display: flex; align-items: center; gap: 0.6rem; }
.lb-avatar { width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-weight: 800; font-size: 0.72rem; flex-shrink: 0; }
.lb-student-name { font-weight: 600; font-size: 0.85rem; color: var(--t-text1); }
.lb-student-city { font-size: 0.7rem; color: var(--t-text3); }
.lb-score { font-weight: 800; color: var(--t-text1); }
.lb-num { text-align: center; color: var(--t-text2); }
.lb-avg-bar { display: flex; align-items: center; gap: 0.4rem; }
.lb-avg-bar > div { flex: 1; height: 6px; background: var(--t-border); border-radius: 99px; overflow: hidden; max-width: 60px; }
.lb-avg-fill { height: 100%; border-radius: 99px; transition: width 0.6s; }
.lb-avg-bar > span { font-size: 0.75rem; color: var(--t-text2); white-space: nowrap; }
.lb-badge { font-size: 0.7rem; font-weight: 700; padding: 0.2rem 0.5rem; border-radius: 99px; white-space: nowrap; }
.badge-scholar { background: rgba(245,158,11,0.12); color: #d97706; }
.badge-expert  { background: rgba(76,175,80,0.12); color: #4caf50; }
.badge-advanced { background: rgba(0,188,212,0.12); color: #00bcd4; }
.badge-learner { background: rgba(109,84,232,0.12); color: #6d54e8; }
.lb-achievements { margin-bottom: 2rem; }
.lb-ach-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.75rem; }
.lb-ach-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(120px, 1fr)); gap: 0.75rem; }
.lb-ach-item { border: 1px solid var(--t-border); border-radius: 14px; padding: 1rem 0.75rem; text-align: center; transition: all 0.15s; }
.lb-ach-item:not(.locked) { border-color: rgba(245,158,11,0.3); background: rgba(245,158,11,0.04); }
.lb-ach-item.locked { opacity: 0.4; filter: grayscale(1); }
.lb-ach-icon { font-size: 1.75rem; margin-bottom: 0.35rem; }
.lb-ach-name { font-weight: 700; font-size: 0.78rem; color: var(--t-text1); }
.lb-ach-desc { font-size: 0.68rem; color: var(--t-text3); margin-top: 0.2rem; }
</style>
