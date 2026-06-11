<template>
  <div class="wq-root">
    <AIHelper context="User is doing weekly quiz for board exam preparation" :chips="['Explain the correct answer', 'Which chapter is this from?', 'Give similar practice questions', 'What should I study more?']" />

    <div class="wq-hero">
      <div class="wq-hero-badge">📅 Week {{ weekNumber }}</div>
      <h1 class="wq-hero-title">Weekly Quiz</h1>
      <p class="wq-hero-sub">New quizzes every Monday · Subject-wise · Track your progress week by week</p>
    </div>

    <!-- Setup -->
    <div v-if="phase === 'setup'">
      <div class="wq-week-info">
        <div class="wq-week-card">
          <div class="wq-week-dates">📅 Week {{ weekNumber }} · {{ weekRange }}</div>
          <div class="wq-week-streak">🔥 {{ streak }}-week streak</div>
        </div>
      </div>

      <div class="wq-subjects-grid">
        <div v-for="subj in quizSubjects" :key="subj.id" class="wq-subj-card">
          <div class="wq-subj-header">
            <div class="wq-subj-icon">{{ subj.icon }}</div>
            <div>
              <div class="wq-subj-name">{{ subj.name }}</div>
              <div class="wq-subj-meta">{{ subj.questions }} questions · {{ subj.duration }} min</div>
            </div>
            <div class="wq-subj-status" :class="getStatusClass(subj.id)">{{ getStatusText(subj.id) }}</div>
          </div>
          <div v-if="weeklyScores[subj.id]" class="wq-subj-score">
            <div class="wq-score-bar">
              <div class="wq-score-fill" :style="{width: weeklyScores[subj.id].pct + '%', background: weeklyScores[subj.id].pct >= 70 ? '#4caf50' : '#f59e0b'}"/>
            </div>
            <span class="wq-score-label">{{ weeklyScores[subj.id].pct }}%</span>
          </div>
          <button @click="startQuiz(subj)" class="wq-start-btn" :class="{ done: weeklyScores[subj.id] }">
            {{ weeklyScores[subj.id] ? '🔄 Retake' : '▶ Start Quiz' }}
          </button>
        </div>
      </div>

      <!-- Weekly summary -->
      <div class="wq-summary">
        <div class="wq-summary-title">📊 This Week's Progress</div>
        <div class="wq-summary-stats">
          <div class="wq-stat">
            <div class="wq-stat-val">{{ completedCount }}/{{ quizSubjects.length }}</div>
            <div class="wq-stat-label">Completed</div>
          </div>
          <div class="wq-stat">
            <div class="wq-stat-val">{{ weeklyAvg }}%</div>
            <div class="wq-stat-label">Average</div>
          </div>
          <div class="wq-stat">
            <div class="wq-stat-val">{{ streak }}</div>
            <div class="wq-stat-label">Week Streak</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Taking -->
    <div v-if="phase === 'taking'" class="wq-taking">
      <div class="wq-take-header">
        <button @click="phase='setup'" class="wq-back-btn">← Back</button>
        <div class="wq-take-title">{{ activeSubject?.name }}</div>
        <div class="wq-take-timer" :class="{ warn: timeLeft <= 10 }">⏱ {{ Math.floor(timeLeft/60) }}:{{ String(timeLeft%60).padStart(2,'0') }}</div>
      </div>
      <div class="wq-take-progress">
        <div class="wq-take-bar"><div class="wq-take-fill" :style="{width: ((currentIdx+1)/questions.length*100)+'%'}"/></div>
        <span>{{ currentIdx+1 }}/{{ questions.length }}</span>
      </div>
      <div v-if="currentQ" class="wq-question">
        <div class="wq-q-num">Question {{ currentIdx + 1 }}</div>
        <div class="wq-q-text">{{ currentQ.question }}</div>
        <div class="wq-q-opts">
          <button v-for="(opt, i) in currentQ.options" :key="i" @click="answer(i)" class="wq-opt"
            :class="{
              selected: selectedAns === i && !showAns,
              correct: showAns && i === currentQ.correct,
              wrong: showAns && selectedAns === i && i !== currentQ.correct,
            }" :disabled="showAns">
            <span class="wq-opt-letter">{{ 'ABCD'[i] }}</span> {{ opt }}
          </button>
        </div>
        <div v-if="showAns" class="wq-ans-feedback">
          <div :class="selectedAns === currentQ.correct ? 'wq-correct' : 'wq-wrong'">
            {{ selectedAns === currentQ.correct ? '✅ Correct!' : '❌ Wrong — Correct: ' + currentQ.options[currentQ.correct] }}
          </div>
          <button @click="next" class="wq-next-btn">{{ currentIdx < questions.length - 1 ? 'Next →' : 'Finish Quiz →' }}</button>
        </div>
      </div>
    </div>

    <!-- Result -->
    <div v-if="phase === 'result'" class="wq-result">
      <div class="wq-result-emoji">{{ resultData.pct >= 80 ? '🏆' : resultData.pct >= 60 ? '🌟' : '📚' }}</div>
      <div class="wq-result-title">{{ activeSubject?.name }} — Week {{ weekNumber }}</div>
      <div class="wq-result-big">{{ resultData.pct }}%</div>
      <div class="wq-result-detail">{{ resultData.correct }} out of {{ resultData.total }} correct</div>
      <div class="wq-result-message">{{ resultData.pct >= 80 ? 'Excellent work! You\'re well prepared.' : resultData.pct >= 60 ? 'Good effort! Review the topics you missed.' : 'Keep practicing! Try again to improve.' }}</div>
      <div class="wq-result-btns">
        <button @click="phase='setup'" class="wq-result-btn secondary">← All Subjects</button>
        <RouterLink to="/app/competition/leaderboard" class="wq-result-btn primary">🏆 Leaderboard</RouterLink>
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed, onUnmounted } from 'vue'
import { SUBJECTS } from '@/stores/content'
import { useStudentStore } from '@/stores/student'
import AIHelper from '@/components/platform/AIHelper.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const student = useStudentStore()
const phase = ref('setup')
const activeSubject = ref(null)
const questions = ref([])
const currentIdx = ref(0)
const selectedAns = ref(null)
const showAns = ref(false)
const answers = ref([])
const timeLeft = ref(600)
let timer = null

const subjects = SUBJECTS || []
const now = new Date()
const weekNumber = computed(() => {
  const start = new Date(now.getFullYear(), 0, 1)
  return Math.ceil((((now - start) / 86400000) + start.getDay() + 1) / 7)
})
const weekRange = computed(() => {
  const day = now.getDay()
  const mon = new Date(now); mon.setDate(now.getDate() - day + 1)
  const sun = new Date(mon); sun.setDate(mon.getDate() + 6)
  return `${mon.toLocaleDateString('en-PK',{month:'short',day:'numeric'})} – ${sun.toLocaleDateString('en-PK',{month:'short',day:'numeric'})}`
})

const weeklyScoresKey = `bap_weekly_${weekNumber.value}`
const weeklyScores = ref(JSON.parse(localStorage.getItem(weeklyScoresKey) || '{}'))
const streak = computed(() => JSON.parse(localStorage.getItem('bap_weekly_streak') || '1'))

const quizSubjects = computed(() => subjects.slice(0,6).map(s => ({ ...s, questions: 10, duration: 15 })))
const completedCount = computed(() => Object.keys(weeklyScores.value).length)
const weeklyAvg = computed(() => {
  const vals = Object.values(weeklyScores.value)
  if (!vals.length) return 0
  return Math.round(vals.reduce((s,v)=>s+v.pct,0)/vals.length)
})

function getStatusText(id) {
  return weeklyScores.value[id] ? `${weeklyScores.value[id].pct}% ✓` : 'Pending'
}
function getStatusClass(id) { return weeklyScores.value[id] ? 'done' : 'pending' }

const QUESTION_BANK = {
  physics: [
    {q:'What is the formula for Ohm\'s Law?',opts:['V = IR','V = I/R','I = VR','R = VI'],ans:0},
    {q:'Unit of power is:',opts:['Joule','Newton','Watt','Pascal'],ans:2},
    {q:'Acceleration due to gravity on Earth is approximately:',opts:['9.8 m/s²','10.8 m/s²','8.9 m/s²','11 m/s²'],ans:0},
    {q:'Wave speed = frequency × ___',opts:['Amplitude','Period','Wavelength','Phase'],ans:2},
    {q:'Kinetic energy = ½mv². If mass doubles, KE becomes:',opts:['Same','Half','Double','Quadruple'],ans:2},
    {q:'The SI unit of work is:',opts:['Watt','Newton','Joule','Pascal'],ans:2},
    {q:'Refractive index n = speed in vacuum / speed in medium. If n > 1, the medium is ___ than vacuum:',opts:['Faster','Slower','Same speed','Depends'],ans:1},
    {q:'Frequency and wavelength are:',opts:['Directly proportional','Inversely proportional','Unrelated','Equal'],ans:1},
    {q:'Ohm\'s Law states that V is directly proportional to I when ___ is constant:',opts:['Power','Temperature','Resistance','Frequency'],ans:2},
    {q:'In which medium does sound travel fastest?',opts:['Air','Water','Vacuum','Solids'],ans:3},
  ],
  chemistry: [
    {q:'What type of bond forms between Na and Cl in NaCl?',opts:['Covalent','Ionic','Metallic','Hydrogen'],ans:1},
    {q:'pH of pure water at 25°C is:',opts:['0','7','14','6'],ans:1},
    {q:'The process of converting liquid to gas is called:',opts:['Condensation','Evaporation','Sublimation','Deposition'],ans:1},
    {q:'Which element has the highest electronegativity?',opts:['Oxygen','Nitrogen','Chlorine','Fluorine'],ans:3},
    {q:'Endothermic reactions:',opts:['Release heat','Absorb heat','Neither','Both'],ans:1},
    {q:'The formula of sulfuric acid is:',opts:['HCl','H₂SO₄','H₂SO₃','H₂S'],ans:1},
    {q:'During electrolysis, oxidation occurs at the:',opts:['Cathode','Anode','Both electrodes','Neither'],ans:1},
    {q:'Alkanes have the general formula:',opts:['CₙH₂ₙ','CₙH₂ₙ₊₂','CₙH₂ₙ₋₂','CₙHₙ'],ans:1},
    {q:'A neutral atom has equal numbers of protons and:',opts:['Neutrons','Electrons','Both','Neither'],ans:1},
    {q:'What is produced when an acid reacts with a metal carbonate?',opts:['Salt only','Salt + water','Salt + water + CO₂','Salt + H₂'],ans:2},
  ],
  biology: [
    {q:'The cell membrane is made mainly of:',opts:['Proteins','Lipids','Carbohydrates','Phospholipid bilayer'],ans:3},
    {q:'Chlorophyll absorbs mainly ___ light:',opts:['Green','Red and blue','Yellow','White'],ans:1},
    {q:'The primary site of photosynthesis in a leaf is:',opts:['Epidermis','Stomata','Mesophyll cells','Veins'],ans:2},
    {q:'Which organ produces bile?',opts:['Pancreas','Kidney','Liver','Stomach'],ans:2},
    {q:'A recessive allele is expressed only when:',opts:['One copy is present','Two copies are present','Combined with dominant','Never expressed'],ans:1},
    {q:'The process of diffusion requires ___ energy:',opts:['A lot','Some','No','ATP'],ans:2},
    {q:'ADH (Anti-Diuretic Hormone) regulates:',opts:['Blood glucose','Water reabsorption','Temperature','Digestion'],ans:1},
    {q:'Ribosomes are the site of:',opts:['DNA replication','Protein synthesis','Lipid synthesis','ATP production'],ans:1},
    {q:'Natural selection was proposed by:',opts:['Mendel','Darwin','Watson','Lamarck'],ans:1},
    {q:'During meiosis II, ___ cells are produced:',opts:['2 diploid','4 haploid','2 haploid','4 diploid'],ans:1},
  ],
  mathematics: [
    {q:'If f(x) = x³, then f\'(x) is:',opts:['x²','2x','3x²','3x³'],ans:2},
    {q:'sin²θ + cos²θ = ?',opts:['0','1','2','sin2θ'],ans:1},
    {q:'The equation of a circle with center (0,0) and radius r is:',opts:['y=mx+c','x²+y²=r²','x²+y²=r','ax+by=c'],ans:1},
    {q:'log(ab) = ?',opts:['log a - log b','log a + log b','log a × log b','log a / log b'],ans:1},
    {q:'nCr = n! / (r! × ___)',opts:['n!','(n+r)!','(n-r)!','r²!'],ans:2},
    {q:'The sum of an arithmetic series with first term a, last term l, n terms is:',opts:['n(a+l)/2','n(a+l)','(a+l)/2','na+l'],ans:0},
    {q:'A complex number a+bi has modulus:',opts:['a+b','a-b','√(a²+b²)','a×b'],ans:2},
    {q:'Geometric mean of 4 and 16 is:',opts:['8','10','6','12'],ans:0},
    {q:'If P(A) = 0.3, then P(A\') = ?',opts:['0.3','0.7','1','0'],ans:1},
    {q:'The derivative of sin(x) is:',opts:['-sin(x)','cos(x)','-cos(x)','tan(x)'],ans:1},
  ],
}

function startQuiz(subj) {
  activeSubject.value = subj
  const bank = QUESTION_BANK[subj.id] || QUESTION_BANK.physics
  questions.value = bank.map(q=>({ question:q.q, options:q.opts, correct:q.ans }))
  answers.value = new Array(questions.value.length).fill(null)
  currentIdx.value = 0; selectedAns.value = null; showAns.value = false
  timeLeft.value = subj.duration * 60
  phase.value = 'taking'
  timer = setInterval(()=>{ timeLeft.value--; if(timeLeft.value<=0){clearInterval(timer);finish()} },1000)
}

const currentQ = computed(()=>questions.value[currentIdx.value])
function answer(idx) { selectedAns.value = idx; showAns.value = true; answers.value[currentIdx.value] = idx }
function next() {
  if (currentIdx.value < questions.value.length - 1) { currentIdx.value++; selectedAns.value = null; showAns.value = false }
  else { clearInterval(timer); finish() }
}
function finish() {
  clearInterval(timer)
  phase.value = 'result'
  const correct = answers.value.filter((a,i)=>a===questions.value[i]?.correct).length
  const pct = Math.round(correct/questions.value.length*100)
  weeklyScores.value[activeSubject.value.id] = { correct, total: questions.value.length, pct }
  localStorage.setItem(weeklyScoresKey, JSON.stringify(weeklyScores.value))
  student.saveTest({ subject: activeSubject.value.id, type: 'weekly', score: correct, total: questions.value.length, bookId: 'weekly' })
}
const resultData = computed(()=>{ const correct=answers.value.filter((a,i)=>a===questions.value[i]?.correct).length; return { correct, total:questions.value.length, pct:Math.round(correct/questions.value.length*100) } })
onUnmounted(()=>clearInterval(timer))
</script>

<style scoped>
.wq-root { max-width: 760px; margin: 0 auto; padding: 1.5rem; }
.wq-hero { text-align: center; padding: 1.5rem 0; }
.wq-hero-badge { display: inline-block; padding: 0.3rem 0.85rem; background: rgba(76,175,80,0.1); color: #4caf50; border-radius: 99px; font-size: 0.8rem; font-weight: 700; margin-bottom: 0.5rem; }
.wq-hero-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.wq-hero-sub { color: var(--t-text3); font-size: 0.875rem; }
.wq-week-info { margin-bottom: 1.5rem; }
.wq-week-card { display: flex; justify-content: space-between; align-items: center; padding: 0.85rem 1.25rem; border: 1px solid var(--t-border); border-radius: 14px; background: var(--t-surface); }
.wq-week-dates { font-size: 0.875rem; color: var(--t-text2); font-weight: 600; }
.wq-week-streak { font-size: 0.875rem; color: #f59e0b; font-weight: 700; }
.wq-subjects-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
.wq-subj-card { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.25rem; background: var(--t-surface); }
.wq-subj-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; }
.wq-subj-icon { font-size: 1.75rem; flex-shrink: 0; }
.wq-subj-name { font-weight: 700; color: var(--t-text1); font-size: 0.9rem; }
.wq-subj-meta { font-size: 0.72rem; color: var(--t-text3); }
.wq-subj-status { margin-left: auto; font-size: 0.72rem; font-weight: 700; padding: 0.2rem 0.55rem; border-radius: 99px; white-space: nowrap; }
.wq-subj-status.done { background: rgba(76,175,80,0.1); color: #4caf50; }
.wq-subj-status.pending { background: var(--t-hover); color: var(--t-text3); }
.wq-subj-score { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.75rem; }
.wq-score-bar { flex: 1; height: 5px; background: var(--t-border); border-radius: 99px; overflow: hidden; }
.wq-score-fill { height: 100%; border-radius: 99px; transition: width 0.5s; }
.wq-score-label { font-size: 0.72rem; font-weight: 700; color: var(--t-text2); }
.wq-start-btn { width: 100%; padding: 0.6rem; border-radius: 10px; background: linear-gradient(135deg, #4caf50, #00bcd4); color: white; border: none; font-weight: 700; cursor: pointer; font-size: 0.85rem; }
.wq-start-btn.done { background: var(--t-hover); color: var(--t-text2); border: 1px solid var(--t-border); }
.wq-summary { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.25rem; background: var(--t-surface); }
.wq-summary-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 1rem; }
.wq-summary-stats { display: flex; gap: 2rem; }
.wq-stat-val { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); }
.wq-stat-label { font-size: 0.72rem; color: var(--t-text3); }
.wq-take-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; }
.wq-back-btn { padding: 0.4rem 0.85rem; background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 99px; color: var(--t-text2); font-size: 0.8rem; cursor: pointer; }
.wq-take-title { flex: 1; font-weight: 800; color: var(--t-text1); }
.wq-take-timer { font-size: 0.9rem; font-weight: 800; padding: 0.3rem 0.75rem; border-radius: 99px; background: var(--t-hover); color: var(--t-text1); }
.wq-take-timer.warn { background: rgba(239,68,68,0.1); color: #ef4444; }
.wq-take-progress { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.25rem; }
.wq-take-bar { flex: 1; height: 5px; background: var(--t-border); border-radius: 99px; overflow: hidden; }
.wq-take-fill { height: 100%; background: linear-gradient(90deg, #4caf50, #00bcd4); border-radius: 99px; transition: width 0.3s; }
.wq-take-progress > span { font-size: 0.78rem; color: var(--t-text3); white-space: nowrap; }
.wq-question { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.5rem; background: var(--t-surface); }
.wq-q-num { font-size: 0.72rem; font-weight: 700; color: var(--t-text2); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.5rem; }
.wq-q-text { font-size: 1.05rem; font-weight: 700; color: var(--t-text1); line-height: 1.5; margin-bottom: 1.25rem; }
.wq-q-opts { display: flex; flex-direction: column; gap: 0.5rem; }
.wq-opt { display: flex; align-items: center; gap: 0.75rem; padding: 0.65rem 1rem; border: 1px solid var(--t-border); border-radius: 12px; background: var(--t-surface); color: var(--t-text1); text-align: left; cursor: pointer; font-size: 0.9rem; transition: all 0.1s; }
.wq-opt:hover:not(:disabled) { border-color: #4caf50; }
.wq-opt.selected { border-color: #4caf50; background: rgba(76,175,80,0.06); }
.wq-opt.correct { border-color: var(--t-success); background: var(--t-success-bg); color: var(--t-success); font-weight: 700; }
.wq-opt.wrong { border-color: var(--t-danger); background: var(--t-danger-bg); color: var(--t-danger); }
.wq-opt-letter { width: 26px; height: 26px; border-radius: 50%; background: var(--t-hover2); color: var(--t-text2); font-size: 0.75rem; font-weight: 800; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.wq-ans-feedback { margin-top: 1rem; display: flex; flex-direction: column; gap: 0.5rem; }
.wq-correct { padding: 0.6rem 1rem; border-radius: 10px; background: var(--t-success-bg); color: var(--t-success); font-weight: 700; font-size: 0.9rem; }
.wq-wrong { padding: 0.6rem 1rem; border-radius: 10px; background: var(--t-danger-bg); color: var(--t-danger); font-weight: 700; font-size: 0.9rem; }
.wq-next-btn { align-self: flex-end; padding: 0.6rem 1.25rem; background: linear-gradient(135deg, #4caf50, #00bcd4); color: white; border: none; border-radius: 10px; font-weight: 700; cursor: pointer; }
.wq-result { text-align: center; padding: 2rem 1rem; }
.wq-result-emoji { font-size: 4rem; margin-bottom: 0.5rem; }
.wq-result-title { font-size: 1.1rem; font-weight: 700; color: var(--t-text2); margin-bottom: 0.25rem; }
.wq-result-big { font-size: 4rem; font-weight: 800; color: var(--t-text1); }
.wq-result-detail { color: var(--t-text3); font-size: 0.9rem; margin: 0.25rem 0 0.75rem; }
.wq-result-message { color: var(--t-text2); font-size: 0.9rem; margin-bottom: 1.5rem; }
.wq-result-btns { display: flex; gap: 0.75rem; justify-content: center; }
.wq-result-btn { padding: 0.75rem 1.5rem; border-radius: 14px; font-weight: 700; font-size: 0.9rem; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; }
.wq-result-btn.secondary { border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text1); }
.wq-result-btn.primary { background: linear-gradient(135deg, #4caf50, #00bcd4); color: white; border: none; }
</style>
