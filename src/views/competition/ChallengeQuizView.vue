<template>
  <div class="cq-root">
    <AIHelper context="User is doing a challenge quiz — competitive timed test for board exams" :chips="['Explain why my answer was wrong', 'What is the formula?', 'Give a similar question', 'Tips for this topic']" />

    <!-- Setup phase -->
    <div v-if="phase === 'setup'" class="cq-setup">
      <div class="cq-setup-icon">⚔️</div>
      <h1 class="cq-setup-title">Challenge Quiz</h1>
      <p class="cq-setup-sub">Timed competitive questions · Earn leaderboard points · Beat your best score</p>

      <div class="cq-setup-card">
        <div class="cq-form-label">Select Subject</div>
        <div class="cq-subject-grid">
          <button v-for="s in subjects" :key="s.id"
            @click="config.subjectId = s.id"
            class="cq-subject-btn"
            :class="{ active: config.subjectId === s.id }">
            <span>{{ s.icon }}</span>{{ s.name }}
          </button>
        </div>
        <div class="cq-form-row">
          <div>
            <div class="cq-form-label">Difficulty</div>
            <div class="cq-difficulty-row">
              <button v-for="d in ['easy','medium','hard']" :key="d" @click="config.difficulty = d" class="cq-diff-btn" :class="{ active: config.difficulty === d }">{{ d }}</button>
            </div>
          </div>
          <div>
            <div class="cq-form-label">Questions</div>
            <div class="cq-difficulty-row">
              <button v-for="n in [5, 10, 15]" :key="n" @click="config.count = n" class="cq-diff-btn" :class="{ active: config.count === n }">{{ n }}</button>
            </div>
          </div>
          <div>
            <div class="cq-form-label">Time per Q</div>
            <div class="cq-difficulty-row">
              <button v-for="t in [30, 45, 60]" :key="t" @click="config.timePerQ = t" class="cq-diff-btn" :class="{ active: config.timePerQ === t }">{{ t }}s</button>
            </div>
          </div>
        </div>
        <div class="cq-setup-info">
          <div class="cq-info-item"><span>📊</span> {{ config.count }} questions</div>
          <div class="cq-info-item"><span>⏱️</span> {{ config.count * config.timePerQ }}s total</div>
          <div class="cq-info-item"><span>🏆</span> {{ config.count * 10 }} max points</div>
          <div class="cq-info-item"><span>⚡</span> {{ config.difficulty }}</div>
        </div>
        <button @click="startQuiz" :disabled="!config.subjectId" class="cq-start-btn">
          ⚔️ Start Challenge
        </button>
      </div>

      <!-- Past attempts -->
      <div v-if="pastAttempts.length" class="cq-history">
        <div class="cq-history-title">📈 Your Past Challenges</div>
        <div class="cq-history-list">
          <div v-for="a in pastAttempts.slice(0,5)" :key="a.id" class="cq-history-item">
            <div class="cq-history-subj">{{ a.subject }}</div>
            <div class="cq-history-score">{{ a.score }}/{{ a.total }}</div>
            <div class="cq-history-pct" :class="a.pct >= 70 ? 'pass' : 'fail'">{{ a.pct }}%</div>
            <div class="cq-history-date">{{ a.date }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Taking phase -->
    <div v-if="phase === 'taking'" class="cq-taking">
      <div class="cq-take-header">
        <div class="cq-take-progress">
          <div class="cq-take-prog-bar">
            <div class="cq-take-prog-fill" :style="{width: ((currentIdx+1)/questions.length*100)+'%'}"/>
          </div>
          <span class="cq-take-q-count">{{ currentIdx + 1 }} / {{ questions.length }}</span>
        </div>
        <div class="cq-take-timer" :class="{ warning: timeLeft <= 10 }">
          <span>⏱️</span> {{ timeLeft }}s
        </div>
        <div class="cq-take-score">🏆 {{ liveScore }}</div>
      </div>

      <div v-if="currentQ" class="cq-question-card">
        <div class="cq-q-subject">{{ currentSubjectName }}</div>
        <div class="cq-q-text">{{ currentQ.question }}</div>
        <div class="cq-q-options">
          <button v-for="(opt, i) in currentQ.options" :key="i"
            @click="selectAnswer(i)"
            class="cq-option"
            :class="{
              selected: selectedAnswer === i && !answered,
              correct: answered && i === currentQ.correct,
              wrong: answered && selectedAnswer === i && i !== currentQ.correct,
            }"
            :disabled="answered">
            <span class="cq-option-letter">{{ 'ABCD'[i] }}</span>
            {{ opt }}
          </button>
        </div>
        <div v-if="answered" class="cq-feedback">
          <div class="cq-feedback-badge" :class="lastCorrect ? 'cq-correct' : 'cq-wrong'">
            {{ lastCorrect ? '✅ Correct! +' + pointsEarned + ' pts' : '❌ Wrong!' }}
          </div>
          <div v-if="!lastCorrect" class="cq-feedback-explain">
            ✓ Correct answer: {{ currentQ.options[currentQ.correct] }}
          </div>
          <button @click="nextQuestion" class="cq-next-btn">
            {{ currentIdx < questions.length - 1 ? 'Next Question →' : 'See Results →' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Result phase -->
    <div v-if="phase === 'result'" class="cq-result">
      <div class="cq-result-icon">{{ resultData.pct >= 80 ? '🏆' : resultData.pct >= 60 ? '🌟' : '📚' }}</div>
      <h2 class="cq-result-title">{{ resultData.pct >= 80 ? 'Excellent!' : resultData.pct >= 60 ? 'Good Job!' : 'Keep Practicing!' }}</h2>
      <div class="cq-result-score-row">
        <div class="cq-result-stat">
          <div class="cq-result-stat-val">{{ resultData.score }}</div>
          <div class="cq-result-stat-label">Score</div>
        </div>
        <div class="cq-result-stat">
          <div class="cq-result-stat-val">{{ resultData.correct }}/{{ resultData.total }}</div>
          <div class="cq-result-stat-label">Correct</div>
        </div>
        <div class="cq-result-stat">
          <div class="cq-result-stat-val">{{ resultData.pct }}%</div>
          <div class="cq-result-stat-label">Accuracy</div>
        </div>
        <div class="cq-result-stat">
          <div class="cq-result-stat-val">{{ resultData.time }}s</div>
          <div class="cq-result-stat-label">Time Used</div>
        </div>
      </div>

      <div class="cq-result-review">
        <div class="cq-review-title">📋 Answer Review</div>
        <div v-for="(q, i) in questions" :key="i" class="cq-review-item" :class="{ correct: answers[i] === q.correct }">
          <div class="cq-review-icon">{{ answers[i] === q.correct ? '✅' : '❌' }}</div>
          <div>
            <div class="cq-review-q">{{ i+1 }}. {{ q.question }}</div>
            <div v-if="answers[i] !== q.correct" class="cq-review-correct">Correct: {{ q.options[q.correct] }}</div>
          </div>
        </div>
      </div>

      <div class="cq-result-actions">
        <button @click="phase='setup'" class="cq-action-btn secondary">Try Again</button>
        <RouterLink to="/app/competition/leaderboard" class="cq-action-btn primary">🏆 View Leaderboard</RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onUnmounted } from 'vue'
import { SUBJECTS } from '@/stores/content'
import { useStudentStore } from '@/stores/student'
import AIHelper from '@/components/platform/AIHelper.vue'

const student = useStudentStore()
const subjects = SUBJECTS || []
const phase = ref('setup')
const config = ref({ subjectId: '', difficulty: 'medium', count: 10, timePerQ: 45 })
const questions = ref([])
const currentIdx = ref(0)
const selectedAnswer = ref(null)
const answered = ref(false)
const lastCorrect = ref(false)
const pointsEarned = ref(0)
const answers = ref([])
const liveScore = ref(0)
const timeLeft = ref(45)
const startTime = ref(0)
let timer = null

const currentQ = computed(() => questions.value[currentIdx.value])
const currentSubjectName = computed(() => subjects.find(s=>s.id===config.value.subjectId)?.name || '')
const pastAttempts = computed(() => JSON.parse(localStorage.getItem('bap_challenges') || '[]'))
const resultData = computed(() => {
  const correct = answers.value.filter((a,i)=>a===questions.value[i]?.correct).length
  const total = questions.value.length
  const pct = total ? Math.round(correct/total*100) : 0
  return { score: liveScore.value, correct, total, pct, time: Math.round((Date.now()-startTime.value)/1000) }
})

function generateQuestions(subjectId, count) {
  const banks = {
    physics: [
      { question:'What is the SI unit of force?', options:['Joule','Newton','Watt','Pascal'], correct:1 },
      { question:'An object at rest stays at rest unless acted upon by an external force. This is Newton\'s:', options:['Second law','Third law','First law','Law of Gravitation'], correct:2 },
      { question:'Speed of light in vacuum is approximately:', options:['3 × 10⁶ m/s','3 × 10⁸ m/s','3 × 10¹⁰ m/s','3 × 10⁴ m/s'], correct:1 },
      { question:'Work done = Force × ___', options:['Time','Velocity','Displacement','Acceleration'], correct:2 },
      { question:'The unit of electrical resistance is:', options:['Volt','Ampere','Ohm','Watt'], correct:2 },
      { question:'Power = Work done / ___', options:['Force','Distance','Time','Mass'], correct:2 },
      { question:'Frequency is measured in:', options:['Meters','Seconds','Hertz','Joules'], correct:2 },
      { question:'In a simple electric circuit, V = IR is:', options:['Kirchhoff\'s law','Ohm\'s law','Faraday\'s law','Lenz\'s law'], correct:1 },
      { question:'The bending of light at a surface is called:', options:['Reflection','Diffraction','Refraction','Dispersion'], correct:2 },
      { question:'Kinetic energy formula is:', options:['mgh','mv','½mv²','Fd'], correct:2 },
    ],
    chemistry: [
      { question:'The atomic number of Hydrogen is:', options:['1','2','3','4'], correct:0 },
      { question:'Chemical formula of water is:', options:['H₂O₂','HO','H₂O','HO₂'], correct:2 },
      { question:'Which gas is produced at the cathode during electrolysis of water?', options:['Oxygen','Carbon dioxide','Hydrogen','Nitrogen'], correct:2 },
      { question:'pH of a neutral solution is:', options:['0','7','14','1'], correct:1 },
      { question:'NaCl is an example of a(n):', options:['Acid','Base','Salt','Oxide'], correct:2 },
      { question:'Oxidation involves the ___ of electrons:', options:['Gain','Loss','Transfer','Sharing'], correct:1 },
      { question:'The number of protons in an atom defines its:', options:['Mass number','Atomic mass','Atomic number','Valence'], correct:2 },
      { question:'Which is a noble gas?', options:['Chlorine','Neon','Sodium','Oxygen'], correct:1 },
      { question:'Acids have pH ___ than 7:', options:['Greater','Equal','Less','Independent'], correct:2 },
      { question:'Organic compounds always contain:', options:['Nitrogen','Hydrogen','Carbon','Oxygen'], correct:2 },
    ],
    biology: [
      { question:'The powerhouse of the cell is the:', options:['Nucleus','Ribosome','Mitochondria','Vacuole'], correct:2 },
      { question:'Photosynthesis produces:', options:['CO₂ and water','Glucose and O₂','Protein and fat','ATP only'], correct:1 },
      { question:'DNA stands for:', options:['Deoxyribonucleic Acid','Diribonucleic Acid','Deoxyribose Amino Acid','Dynamic Nucleic Acid'], correct:0 },
      { question:'The number of chromosomes in a normal human cell is:', options:['23','46','44','48'], correct:1 },
      { question:'Which blood cells fight infection?', options:['Red blood cells','Platelets','White blood cells','Plasma'], correct:2 },
      { question:'Osmosis is the movement of ___ molecules:', options:['Glucose','Protein','Water','Salt'], correct:2 },
      { question:'Mitosis produces ___ daughter cells:', options:['One','Two','Three','Four'], correct:1 },
      { question:'The study of heredity is called:', options:['Ecology','Genetics','Taxonomy','Physiology'], correct:1 },
      { question:'Insulin is produced by the:', options:['Liver','Kidney','Pancreas','Thyroid'], correct:2 },
      { question:'The main function of red blood cells is to carry:', options:['Nutrients','Hormones','Oxygen','Waste'], correct:2 },
    ],
    mathematics: [
      { question:'The value of π (pi) is approximately:', options:['3.14','2.71','1.41','1.73'], correct:0 },
      { question:'Quadratic formula solves ax² + bx + c = 0. The discriminant is:', options:['a² - 4bc','b² - 4ac','b - 4ac','a - 4bc'], correct:1 },
      { question:'log₁₀(100) = ?', options:['1','10','2','100'], correct:2 },
      { question:'sin(90°) = ?', options:['0','1','-1','√2/2'], correct:1 },
      { question:'The area of a circle with radius r is:', options:['πr','2πr','πr²','2πr²'], correct:2 },
      { question:'If f(x) = x², then f\'(x) (derivative) = ?', options:['x','2x','x²','2'], correct:1 },
      { question:'What is the sum of angles in a triangle?', options:['90°','180°','270°','360°'], correct:1 },
      { question:'√(-1) is represented by:', options:['i','e','π','∞'], correct:0 },
      { question:'5! (5 factorial) = ?', options:['25','100','120','60'], correct:2 },
      { question:'The hypotenuse² = ?', options:['base + height','base × height','base² + height²','(base+height)²'], correct:2 },
    ],
  }
  const bank = banks[subjectId] || banks.physics
  const shuffled = [...bank].sort(() => Math.random() - 0.5)
  return shuffled.slice(0, Math.min(count, bank.length))
}

function startQuiz() {
  questions.value = generateQuestions(config.value.subjectId, config.value.count)
  answers.value = new Array(questions.value.length).fill(null)
  currentIdx.value = 0; selectedAnswer.value = null; answered.value = false; liveScore.value = 0
  startTime.value = Date.now()
  phase.value = 'taking'
  startTimer()
}

function startTimer() {
  clearInterval(timer)
  timeLeft.value = config.value.timePerQ
  timer = setInterval(() => {
    timeLeft.value--
    if (timeLeft.value <= 0) { clearInterval(timer); if (!answered.value) autoNext() }
  }, 1000)
}

function autoNext() { answered.value = true; lastCorrect.value = false; pointsEarned.value = 0; answers.value[currentIdx.value] = -1 }

function selectAnswer(idx) {
  if (answered.value) return
  clearInterval(timer)
  selectedAnswer.value = idx
  answered.value = true
  const correct = idx === currentQ.value.correct
  lastCorrect.value = correct
  answers.value[currentIdx.value] = idx
  if (correct) {
    pointsEarned.value = Math.max(5, Math.round(timeLeft.value / config.value.timePerQ * 10))
    liveScore.value += pointsEarned.value
  } else pointsEarned.value = 0
}

function nextQuestion() {
  if (currentIdx.value < questions.value.length - 1) {
    currentIdx.value++; selectedAnswer.value = null; answered.value = false
    startTimer()
  } else {
    clearInterval(timer)
    phase.value = 'result'
    saveAttempt()
  }
}

function saveAttempt() {
  const attempts = JSON.parse(localStorage.getItem('bap_challenges') || '[]')
  const subj = subjects.find(s=>s.id===config.value.subjectId)?.name || config.value.subjectId
  attempts.unshift({ id: Date.now(), subject: subj, score: resultData.value.score, total: resultData.value.total, pct: resultData.value.pct, date: new Date().toLocaleDateString('en-PK') })
  localStorage.setItem('bap_challenges', JSON.stringify(attempts.slice(0,20)))
  student.saveTest({ subject: config.value.subjectId, type: 'challenge', score: resultData.value.correct, total: resultData.value.total, bookId: 'challenge' })
}
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.cq-root { max-width: 700px; margin: 0 auto; padding: 1.5rem; }
.cq-setup { text-align: center; }
.cq-setup-icon { font-size: 3.5rem; margin-bottom: 0.5rem; }
.cq-setup-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.cq-setup-sub { color: var(--t-text3); font-size: 0.875rem; margin-bottom: 1.5rem; }
.cq-setup-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 20px; padding: 1.5rem; text-align: left; margin-bottom: 1.5rem; }
.cq-form-label { font-size: 0.75rem; font-weight: 700; color: var(--t-text3); letter-spacing: 0.05em; text-transform: uppercase; margin-bottom: 0.6rem; }
.cq-subject-grid { display: flex; flex-wrap: wrap; gap: 0.4rem; margin-bottom: 1.25rem; }
.cq-subject-btn { padding: 0.4rem 0.85rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.8rem; cursor: pointer; display: flex; align-items: center; gap: 0.3rem; }
.cq-subject-btn.active, .cq-subject-btn:hover { background: rgba(76,175,80,0.1); color: #4caf50; border-color: rgba(76,175,80,0.3); }
.cq-form-row { display: flex; gap: 1.5rem; flex-wrap: wrap; margin-bottom: 1.25rem; }
.cq-difficulty-row { display: flex; gap: 0.4rem; }
.cq-diff-btn { padding: 0.3rem 0.75rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.8rem; cursor: pointer; }
.cq-diff-btn.active, .cq-diff-btn:hover { background: rgba(0,188,212,0.1); color: #00bcd4; border-color: rgba(0,188,212,0.3); }
.cq-setup-info { display: flex; gap: 0.75rem; flex-wrap: wrap; padding: 0.85rem; background: var(--t-hover); border-radius: 12px; margin-bottom: 1.25rem; }
.cq-info-item { display: flex; align-items: center; gap: 0.35rem; font-size: 0.82rem; color: var(--t-text2); }
.cq-start-btn { width: 100%; padding: 0.9rem; background: linear-gradient(135deg, #f59e0b, #ef4444); color: white; border: none; border-radius: 14px; font-weight: 800; font-size: 1rem; cursor: pointer; }
.cq-start-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.cq-history { text-align: left; }
.cq-history-title { font-size: 0.75rem; font-weight: 700; color: var(--t-text3); letter-spacing: 0.05em; text-transform: uppercase; margin-bottom: 0.6rem; }
.cq-history-list { display: flex; flex-direction: column; gap: 0.4rem; }
.cq-history-item { display: flex; align-items: center; gap: 1rem; padding: 0.65rem 1rem; border: 1px solid var(--t-border); border-radius: 12px; background: var(--t-surface); }
.cq-history-subj { flex: 1; font-size: 0.85rem; color: var(--t-text1); font-weight: 600; }
.cq-history-score { font-size: 0.82rem; color: var(--t-text2); }
.cq-history-pct { font-size: 0.8rem; font-weight: 700; padding: 0.15rem 0.5rem; border-radius: 99px; }
.cq-history-pct.pass { background: rgba(76,175,80,0.1); color: #4caf50; }
.cq-history-pct.fail { background: rgba(239,68,68,0.1); color: #ef4444; }
.cq-history-date { font-size: 0.72rem; color: var(--t-text3); }
.cq-take-header { display: flex; align-items: center; gap: 1rem; padding: 0.85rem 1rem; border: 1px solid var(--t-border); border-radius: 14px; background: var(--t-surface); margin-bottom: 1.25rem; }
.cq-take-progress { flex: 1; display: flex; align-items: center; gap: 0.75rem; }
.cq-take-prog-bar { flex: 1; height: 6px; background: var(--t-border); border-radius: 99px; overflow: hidden; }
.cq-take-prog-fill { height: 100%; background: linear-gradient(90deg, #f59e0b, #ef4444); border-radius: 99px; transition: width 0.3s; }
.cq-take-q-count { font-size: 0.78rem; font-weight: 700; color: var(--t-text2); white-space: nowrap; }
.cq-take-timer { font-size: 0.9rem; font-weight: 800; color: var(--t-text1); padding: 0.3rem 0.75rem; border-radius: 99px; background: var(--t-hover); }
.cq-take-timer.warning { background: rgba(239,68,68,0.1); color: #ef4444; animation: pulse 0.5s infinite alternate; }
@keyframes pulse { from{opacity:1}to{opacity:0.5} }
.cq-take-score { font-size: 0.85rem; font-weight: 700; color: #f59e0b; white-space: nowrap; }
.cq-question-card { border: 1px solid var(--t-border); border-radius: 20px; padding: 1.5rem; background: var(--t-surface); }
.cq-q-subject { font-size: 0.72rem; font-weight: 700; color: var(--t-text2); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.5rem; }
.cq-q-text { font-size: 1.1rem; font-weight: 700; color: var(--t-text1); line-height: 1.5; margin-bottom: 1.25rem; }
.cq-q-options { display: flex; flex-direction: column; gap: 0.5rem; }
.cq-option { display: flex; align-items: center; gap: 0.75rem; padding: 0.7rem 1rem; border: 1px solid var(--t-border); border-radius: 12px; background: var(--t-surface); color: var(--t-text1); text-align: left; cursor: pointer; font-size: 0.9rem; transition: all 0.12s; }
.cq-option:hover:not(:disabled) { border-color: #f59e0b; background: rgba(245,158,11,0.05); }
.cq-option.selected { border-color: #f59e0b; background: rgba(245,158,11,0.08); }
.cq-option.correct { border-color: var(--t-success); background: var(--t-success-bg); color: var(--t-success); font-weight: 700; }
.cq-option.wrong { border-color: var(--t-danger); background: var(--t-danger-bg); color: var(--t-danger); }
.cq-option-letter { width: 26px; height: 26px; border-radius: 50%; background: var(--t-hover2); color: var(--t-text2); font-size: 0.75rem; font-weight: 800; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.cq-feedback { margin-top: 1.25rem; display: flex; flex-direction: column; gap: 0.5rem; }
.cq-feedback-badge { padding: 0.6rem 1rem; border-radius: 10px; font-weight: 700; font-size: 0.9rem; }
.cq-correct { background: var(--t-success-bg); color: var(--t-success); }
.cq-wrong { background: var(--t-danger-bg); color: var(--t-danger); }
.cq-feedback-explain { font-size: 0.85rem; color: var(--t-text2); padding: 0 0.25rem; }
.cq-next-btn { align-self: flex-end; padding: 0.6rem 1.25rem; background: linear-gradient(135deg, #f59e0b, #ef4444); color: white; border: none; border-radius: 10px; font-weight: 700; cursor: pointer; }
.cq-result { text-align: center; }
.cq-result-icon { font-size: 4rem; margin-bottom: 0.5rem; }
.cq-result-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); margin-bottom: 1.5rem; }
.cq-result-score-row { display: flex; justify-content: center; gap: 1.5rem; flex-wrap: wrap; padding: 1.5rem; border: 1px solid var(--t-border); border-radius: 20px; background: var(--t-surface); margin-bottom: 1.5rem; }
.cq-result-stat-val { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.cq-result-stat-label { font-size: 0.72rem; color: var(--t-text3); font-weight: 600; text-transform: uppercase; }
.cq-result-review { text-align: left; margin-bottom: 1.5rem; }
.cq-review-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text3); letter-spacing: 0.05em; text-transform: uppercase; margin-bottom: 0.75rem; }
.cq-review-item { display: flex; gap: 0.75rem; padding: 0.6rem 0; border-bottom: 1px solid var(--t-border); align-items: flex-start; }
.cq-review-icon { font-size: 1rem; flex-shrink: 0; padding-top: 0.1rem; }
.cq-review-q { font-size: 0.85rem; color: var(--t-text1); line-height: 1.5; }
.cq-review-correct { font-size: 0.78rem; color: var(--t-success); font-weight: 600; margin-top: 0.2rem; }
.cq-result-actions { display: flex; gap: 0.75rem; justify-content: center; flex-wrap: wrap; }
.cq-action-btn { padding: 0.75rem 1.5rem; border-radius: 14px; font-weight: 700; font-size: 0.9rem; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; }
.cq-action-btn.secondary { border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text1); }
.cq-action-btn.primary { background: linear-gradient(135deg, #f59e0b, #d97706); color: white; border: none; }
</style>
