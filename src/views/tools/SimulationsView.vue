<template>
  <div class="sim-root">
    <AIHelper context="User is studying physics and chemistry simulations for board exams" :chips="['Explain this experiment', 'What happens if I change the variable?', 'Formula for this?', 'Board exam questions on this topic']" />

    <div class="sim-hero">
      <div class="sim-hero-icon">🔬</div>
      <h1 class="sim-hero-title">Interactive Simulations</h1>
      <p class="sim-hero-sub">PhET Interactive Simulations · University of Colorado · AI explanations · Board exam prep</p>
      <a href="https://phet.colorado.edu" target="_blank" rel="noopener" class="sim-phet-badge">
        Powered by PhET · University of Colorado Boulder
      </a>
    </div>

    <div class="sim-filter">
      <button v-for="cat in categories" :key="cat" @click="activeFilter = cat" class="sim-filter-btn" :class="{ active: activeFilter === cat }">{{ cat }}</button>
    </div>

    <!-- Simulation Grid -->
    <div v-if="!activeSim" class="sim-grid">
      <div v-for="sim in filteredSims" :key="sim.id" @click="openSim(sim)" class="sim-card">
        <div class="sim-card-icon">{{ sim.icon }}</div>
        <div class="sim-card-subject">{{ sim.subject }}</div>
        <div class="sim-card-title">{{ sim.title }}</div>
        <div class="sim-card-desc">{{ sim.desc }}</div>
        <div class="sim-card-footer">
          <span class="sim-card-grade">{{ sim.grade }}</span>
          <span class="sim-launch">Launch PhET →</span>
        </div>
      </div>
    </div>

    <!-- Simulation Detail -->
    <div v-if="activeSim" class="sim-detail">
      <button @click="activeSim = null; iframeLoaded = false" class="sim-back">← All Simulations</button>
      <div class="sim-detail-header">
        <div class="sim-detail-icon">{{ activeSim.icon }}</div>
        <div>
          <div class="sim-detail-sub">{{ activeSim.subject }} · {{ activeSim.grade }}</div>
          <h2 class="sim-detail-title">{{ activeSim.title }}</h2>
        </div>
      </div>

      <!-- PhET iframe -->
      <div class="sim-visual-panel">
        <div class="sim-visual-title">
          <span>⚗️ Interactive Simulation (PhET)</span>
          <a :href="activeSim.phetUrl" target="_blank" rel="noopener" class="sim-open-full">Open Full Screen ↗</a>
        </div>
        <div class="sim-iframe-wrap">
          <div v-if="!iframeLoaded" class="sim-iframe-loading">
            <div class="sim-loading-dot" /><div class="sim-loading-dot" /><div class="sim-loading-dot" />
            <span>Loading simulation…</span>
          </div>
          <iframe
            :src="activeSim.phetUrl"
            class="sim-iframe"
            :class="{ 'sim-iframe-visible': iframeLoaded }"
            frameborder="0"
            allowfullscreen
            allow="fullscreen"
            :title="activeSim.title + ' – PhET Interactive Simulation'"
            @load="iframeLoaded = true">
          </iframe>
        </div>
        <div class="sim-phet-credit">
          PhET Interactive Simulations, University of Colorado Boulder —
          <a href="https://phet.colorado.edu" target="_blank" rel="noopener">phet.colorado.edu</a>
        </div>
      </div>

      <!-- Theory Steps -->
      <div class="sim-theory">
        <div class="sim-theory-title">📚 Theory & Steps</div>
        <div v-for="(step, i) in activeSim.steps" :key="i" class="sim-step">
          <div class="sim-step-num">{{ i + 1 }}</div>
          <div class="sim-step-content">
            <div class="sim-step-heading">{{ step.heading }}</div>
            <div class="sim-step-text">{{ step.text }}</div>
            <div v-if="step.formula" class="sim-step-formula">{{ step.formula }}</div>
          </div>
        </div>
      </div>

      <!-- Variables -->
      <div v-if="activeSim.variables?.length" class="sim-variables">
        <div class="sim-vars-title">🔧 Key Variables</div>
        <div class="sim-vars-grid">
          <div v-for="v in activeSim.variables" :key="v.symbol" class="sim-var-item">
            <div class="sim-var-symbol">{{ v.symbol }}</div>
            <div class="sim-var-name">{{ v.name }}</div>
            <div class="sim-var-unit">{{ v.unit }}</div>
          </div>
        </div>
      </div>

      <!-- AI Section -->
      <div class="sim-ai-section">
        <button @click="getAIExplanation" :disabled="aiLoading" class="sim-ai-btn">
          {{ aiLoading ? '⏳ AI explaining...' : '🤖 AI Deep Explanation' }}
        </button>
        <div v-if="aiText" class="sim-ai-result">
          <div class="sim-ai-header">🤖 AI Explanation</div>
          <div class="sim-ai-text">{{ aiText }}</div>
        </div>
      </div>

      <!-- Board Exam Corner -->
      <div class="sim-exam-corner">
        <div class="sim-exam-title">📝 Board Exam Questions</div>
        <div v-for="(q, i) in activeSim.examQuestions" :key="i" class="sim-exam-q">
          <span class="sim-exam-q-num">Q{{ i+1 }}.</span> {{ q }}
        </div>
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import AIHelper from '@/components/platform/AIHelper.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const activeSim = ref(null)
const activeFilter = ref('All')
const aiText = ref('')
const aiLoading = ref(false)
const iframeLoaded = ref(false)

const categories = ['All', 'Physics', 'Chemistry', 'Biology']

const simulations = [
  {
    id:'ohms', icon:'⚡', subject:'Physics', grade:'Grade 9-10', title:"Ohm's Law",
    desc:'Explore the relationship between voltage, current and resistance interactively.',
    phetUrl:'https://phet.colorado.edu/sims/html/ohms-law/latest/ohms-law_en.html',
    steps:[
      { heading:"What is Ohm's Law?", text:"Ohm's Law states that the current through a conductor is directly proportional to the voltage across it, provided temperature stays constant.", formula:'V = I × R   (Voltage = Current × Resistance)' },
      { heading:'Understanding the Variables', text:'V (Voltage) is measured in Volts (V). I (Current) is measured in Amperes (A). R (Resistance) is measured in Ohms (Ω).' },
      { heading:'Experiment Steps', text:'1. Set up a simple circuit with a battery, resistor, and ammeter.\n2. Increase the voltage and record the current.\n3. Plot V on x-axis and I on y-axis.\n4. The slope of the line = 1/R (resistance).' },
      { heading:'Key Observations', text:'When voltage doubles, current also doubles (if resistance is constant). Higher resistance means lower current for the same voltage.' },
    ],
    variables:[
      { symbol:'V', name:'Voltage', unit:'Volts (V)' },
      { symbol:'I', name:'Current', unit:'Amperes (A)' },
      { symbol:'R', name:'Resistance', unit:'Ohms (Ω)' },
    ],
    examQuestions:["State Ohm's Law and write its mathematical form.",'A 12V battery is connected to a 4Ω resistor. Calculate the current.','What does the graph of V vs I represent? What is its slope?'],
  },
  {
    id:'projectile', icon:'🏀', subject:'Physics', grade:'Grade 10-11', title:'Projectile Motion',
    desc:'Launch objects at any angle and observe parabolic paths under gravity.',
    phetUrl:'https://phet.colorado.edu/sims/html/projectile-motion/latest/projectile-motion_en.html',
    steps:[
      { heading:'What is Projectile Motion?', text:'When an object is launched into the air and moves under the influence of gravity only (no air resistance), it follows a curved parabolic path.' },
      { heading:'Key Equations', text:'The horizontal and vertical motions are independent of each other. Horizontal velocity remains constant; vertical velocity changes due to gravity.', formula:'Range: R = v²sin(2θ)/g   |   Max Height: H = v²sin²θ/(2g)' },
      { heading:'Experiment Analysis', text:'1. At 45°, the projectile achieves maximum range.\n2. Vertical component: affected by gravity (g = 9.8 m/s²).\n3. Horizontal component: remains constant throughout flight.' },
    ],
    variables:[
      { symbol:'v', name:'Initial Velocity', unit:'m/s' },
      { symbol:'θ', name:'Launch Angle', unit:'degrees' },
      { symbol:'g', name:'Gravity', unit:'m/s²' },
      { symbol:'R', name:'Range', unit:'meters' },
    ],
    examQuestions:['What angle gives maximum range in projectile motion?','A ball is thrown horizontally from a 20m high cliff at 10 m/s. How long does it take to land?','Differentiate between horizontal and vertical components of projectile motion.'],
  },
  {
    id:'waves', icon:'🌊', subject:'Physics', grade:'Grade 10-11', title:'Wave Properties',
    desc:'Adjust amplitude, frequency and damping on a live animated string.',
    phetUrl:'https://phet.colorado.edu/sims/html/wave-on-a-string/latest/wave-on-a-string_en.html',
    steps:[
      { heading:'What are Waves?', text:'Waves are disturbances that transfer energy from one place to another. They are described by wavelength, frequency, amplitude, and wave speed.' },
      { heading:'Key Wave Properties', text:'Wavelength (λ): Distance between successive crests\nFrequency (f): Number of waves per second (Hz)\nAmplitude (A): Maximum displacement from rest\nWave Speed (v): How fast the wave travels', formula:'v = f × λ   (Wave speed = frequency × wavelength)' },
      { heading:'Transverse vs Longitudinal', text:'Transverse waves: displacement is perpendicular to direction (light, water waves).\nLongitudinal waves: displacement is parallel to direction (sound waves, seismic P-waves).' },
    ],
    variables:[
      { symbol:'λ', name:'Wavelength', unit:'meters (m)' },
      { symbol:'f', name:'Frequency', unit:'Hertz (Hz)' },
      { symbol:'A', name:'Amplitude', unit:'meters (m)' },
      { symbol:'v', name:'Wave Speed', unit:'m/s' },
    ],
    examQuestions:['What is the relationship between frequency and wavelength?','A wave has frequency 500 Hz and wavelength 0.68 m. Find its speed.','Differentiate between transverse and longitudinal waves.'],
  },
  {
    id:'refraction_sim', icon:'🔮', subject:'Physics', grade:'Grade 9-10', title:'Light Refraction',
    desc:'Visualize bending light rays and measure angles with Snell\'s Law.',
    phetUrl:'https://phet.colorado.edu/sims/html/bending-light/latest/bending-light_en.html',
    steps:[
      { heading:'What is Refraction?', text:"Refraction is the bending of light when it passes from one medium to another of different optical density. The light changes direction at the boundary." },
      { heading:"Snell's Law", text:'The ratio of sine of angle of incidence to sine of angle of refraction is constant (equal to the refractive index).', formula:'n₁ sin(θ₁) = n₂ sin(θ₂)   |   n = sin(i)/sin(r)' },
      { heading:'Total Internal Reflection', text:'When light passes from denser to rarer medium at an angle greater than the critical angle, total internal reflection occurs. This is the basis of optical fibers.' },
    ],
    variables:[
      { symbol:'n', name:'Refractive Index', unit:'no unit' },
      { symbol:'i', name:'Angle of Incidence', unit:'degrees' },
      { symbol:'r', name:'Angle of Refraction', unit:'degrees' },
    ],
    examQuestions:["State Snell's Law of refraction.",'The refractive index of glass is 1.5. Find the angle of refraction if angle of incidence is 30°.','What is total internal reflection? Give one application.'],
  },
  {
    id:'circuit', icon:'🔌', subject:'Physics', grade:'Grade 9-10', title:'Circuit Construction',
    desc:'Build DC circuits, add batteries, bulbs and switches — measure voltage and current.',
    phetUrl:'https://phet.colorado.edu/sims/html/circuit-construction-kit-dc/latest/circuit-construction-kit-dc_en.html',
    steps:[
      { heading:'Electric Circuits', text:'An electric circuit is a closed path through which current can flow. It must contain a source (battery), conducting wires, and a load (resistor/bulb).' },
      { heading:'Series vs Parallel', text:'Series: Components connected end-to-end. Same current flows through all. Total resistance adds up.\nParallel: Components connected side by side. Voltage is same across all. Total resistance decreases.', formula:'Series: R_total = R₁ + R₂ + R₃\nParallel: 1/R_total = 1/R₁ + 1/R₂ + 1/R₃' },
      { heading:'Building a Circuit', text:'1. Add a battery for voltage source.\n2. Connect wires to form a closed loop.\n3. Add a resistor or light bulb.\n4. Use ammeter and voltmeter to measure values.' },
    ],
    variables:[
      { symbol:'V', name:'Voltage', unit:'Volts (V)' },
      { symbol:'I', name:'Current', unit:'Amperes (A)' },
      { symbol:'R', name:'Resistance', unit:'Ohms (Ω)' },
    ],
    examQuestions:['Draw a series circuit with 3 resistors and write the formula for total resistance.','A 6V battery is connected to two 3Ω resistors in parallel. Find total current.','What is the difference between EMF and terminal voltage?'],
  },
  {
    id:'titration', icon:'🧪', subject:'Chemistry', grade:'Grade 10-11', title:'Acid-Base Solutions',
    desc:'Explore pH, strong/weak acids and bases with real-time indicators.',
    phetUrl:'https://phet.colorado.edu/sims/html/acid-base-solutions/latest/acid-base-solutions_en.html',
    steps:[
      { heading:'What is Titration?', text:'Titration is a technique used to determine the concentration of a solution by reacting it with another solution of known concentration.' },
      { heading:'Acid-Base Reaction', text:'In acid-base titration, an acid (like HCl) is neutralized by a base (like NaOH). An indicator (phenolphthalein) changes color at the equivalence point.', formula:'HCl + NaOH → NaCl + H₂O  (neutralization)' },
      { heading:'Calculating Concentration', text:'At the equivalence point: moles of acid = moles of base\nUsing: C₁V₁ = C₂V₂\nWhere C = concentration (mol/L) and V = volume (L)', formula:'C₁V₁ = C₂V₂' },
      { heading:'pH Scale', text:'pH < 7: Acidic\npH = 7: Neutral\npH > 7: Basic\nStrong acids/bases fully ionize; weak ones partially ionize.' },
    ],
    variables:[
      { symbol:'C₁', name:'Concentration of acid', unit:'mol/L' },
      { symbol:'V₁', name:'Volume of acid', unit:'liters' },
      { symbol:'pH', name:'Potential of Hydrogen', unit:'0–14 scale' },
    ],
    examQuestions:['What is the principle of acid-base titration?','25 cm³ of HCl was neutralized by 20 cm³ of 0.1 mol/L NaOH. Find the concentration of HCl.','Why is phenolphthalein used as an indicator in NaOH/HCl titration?'],
  },
  {
    id:'balancing', icon:'⚖️', subject:'Chemistry', grade:'Grade 9-10', title:'Balancing Equations',
    desc:'Balance chemical equations interactively by adjusting coefficients.',
    phetUrl:'https://phet.colorado.edu/sims/html/balancing-chemical-equations/latest/balancing-chemical-equations_en.html',
    steps:[
      { heading:'Why Balance Equations?', text:'Chemical equations must be balanced to satisfy the Law of Conservation of Mass — atoms are neither created nor destroyed in a chemical reaction.' },
      { heading:'Rules for Balancing', text:'1. Count atoms of each element on both sides.\n2. Add coefficients (not subscripts) to balance.\n3. Start with the most complex molecule.\n4. Balance hydrogen and oxygen last.' },
      { heading:'Example: Water Formation', text:'Unbalanced: H₂ + O₂ → H₂O\nBalanced: 2H₂ + O₂ → 2H₂O\nNow: 4H on left = 4H on right; 2O on left = 2O on right ✓', formula:'2H₂ + O₂ → 2H₂O' },
    ],
    variables:[
      { symbol:'R', name:'Reactants', unit:'left side' },
      { symbol:'P', name:'Products', unit:'right side' },
    ],
    examQuestions:['Balance: Fe + O₂ → Fe₂O₃','State the Law of Conservation of Mass.','Why can we only change coefficients (not subscripts) when balancing equations?'],
  },
  {
    id:'states', icon:'🧊', subject:'Chemistry', grade:'Grade 9', title:'States of Matter',
    desc:'Explore how temperature and pressure affect the state of matter at molecular level.',
    phetUrl:'https://phet.colorado.edu/sims/html/states-of-matter/latest/states-of-matter_en.html',
    steps:[
      { heading:'Three States of Matter', text:'Matter exists in three states: Solid, Liquid, and Gas. The difference is the arrangement and energy of particles.' },
      { heading:'Kinetic Theory', text:'All matter is made of particles in constant motion. The energy of these particles determines the state:\n• Solid: low energy, fixed positions\n• Liquid: medium energy, free to flow\n• Gas: high energy, random fast movement' },
      { heading:'Changes of State', text:'Melting: solid → liquid (absorbs heat)\nBoiling: liquid → gas (absorbs heat)\nFreezing: liquid → solid (releases heat)\nCondensation: gas → liquid (releases heat)', formula:'Q = mL   (Heat = mass × Latent Heat)' },
    ],
    variables:[
      { symbol:'T', name:'Temperature', unit:'Kelvin (K)' },
      { symbol:'P', name:'Pressure', unit:'Pa or atm' },
      { symbol:'L', name:'Latent Heat', unit:'J/kg' },
    ],
    examQuestions:['Explain the kinetic theory of matter.','What happens to gas pressure when temperature increases at constant volume?','Define latent heat and explain why it is needed during a change of state.'],
  },
  {
    id:'photosyn_sim', icon:'🌿', subject:'Biology', grade:'Grade 9-10', title:'Gene Expression',
    desc:'See how DNA transcription and translation produce proteins — the basis of life.',
    phetUrl:'https://phet.colorado.edu/sims/html/gene-expression-essentials/latest/gene-expression-essentials_en.html',
    steps:[
      { heading:'DNA → RNA → Protein', text:'Gene expression is the process by which information from a gene is used to make a functional product (usually a protein). It has two main steps: Transcription and Translation.' },
      { heading:'Transcription', text:'In the cell nucleus, the DNA double helix unwinds and RNA polymerase reads the template strand to produce mRNA. The mRNA sequence is complementary to the DNA template strand.' },
      { heading:'Translation', text:'mRNA travels to ribosomes in the cytoplasm. tRNA molecules bring amino acids that match the mRNA codons. The ribosome joins amino acids to build a protein chain.' },
    ],
    variables:[
      { symbol:'DNA', name:'Deoxyribonucleic acid', unit:'nucleus' },
      { symbol:'mRNA', name:'Messenger RNA', unit:'cytoplasm' },
      { symbol:'tRNA', name:'Transfer RNA', unit:'ribosome' },
    ],
    examQuestions:['What is gene expression?','Explain the difference between transcription and translation.','Where does transcription take place in the cell?'],
  },
  {
    id:'osmosis_sim', icon:'💧', subject:'Biology', grade:'Grade 9', title:'Natural Selection',
    desc:'Simulate evolution through natural selection and observe population changes over time.',
    phetUrl:'https://phet.colorado.edu/sims/html/natural-selection/latest/natural-selection_en.html',
    steps:[
      { heading:"What is Natural Selection?", text:"Natural selection is the process by which organisms with favorable traits survive and reproduce more successfully than those with less favorable traits. It is the main mechanism of evolution." },
      { heading:'Darwin\'s Four Principles', text:'1. Variation: Individuals in a population differ.\n2. Inheritance: Traits are passed to offspring.\n3. Selection: Environment favors certain traits.\n4. Time: Over many generations, favorable traits become common.' },
      { heading:'Types of Natural Selection', text:'• Directional: Extreme trait favored (e.g., longer necks in giraffes)\n• Stabilizing: Intermediate trait favored\n• Disruptive: Extreme traits at both ends favored' },
    ],
    variables:[
      { symbol:'f', name:'Allele frequency', unit:'proportion' },
      { symbol:'w', name:'Fitness', unit:'survival rate' },
    ],
    examQuestions:["State Darwin's theory of natural selection in your own words.",'Differentiate between natural selection and artificial selection.','What is meant by "survival of the fittest"?'],
  },
]

const filteredSims = computed(() => activeFilter.value === 'All' ? simulations : simulations.filter(s => s.subject === activeFilter.value))

function openSim(sim) { activeSim.value = sim; aiText.value = ''; iframeLoaded.value = false }

async function getAIExplanation() {
  if (!activeSim.value) return
  aiLoading.value = true; aiText.value = ''
  try {
    const key = import.meta.env.VITE_GEMINI_API_KEY
    if (!key) throw new Error()
    const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=${key}`, {
      method:'POST', headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ contents:[{ parts:[{ text:`Explain the concept of "${activeSim.value.title}" in depth for a Pakistani board exam student (Grade 9-12). Cover: 1) Core concept in simple language, 2) Key formulas with explanation, 3) Real-world examples from Pakistan, 4) Common mistakes students make, 5) Tips to remember for exams. Make it engaging and educational.` }] }] })
    })
    const data = await res.json()
    aiText.value = data.candidates?.[0]?.content?.parts?.[0]?.text || 'No response.'
  } catch { aiText.value = 'AI unavailable. Check your VITE_GEMINI_API_KEY in .env' }
  aiLoading.value = false
}
</script>

<style scoped>
.sim-root { max-width: 960px; margin: 0 auto; padding: 1.5rem; }
.sim-hero { text-align: center; padding: 2rem 1rem 1.5rem; }
.sim-hero-icon { font-size: 3rem; margin-bottom: 0.5rem; }
.sim-hero-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.sim-hero-sub { color: var(--t-text3); font-size: 0.875rem; margin-bottom: 0.75rem; }
.sim-phet-badge {
  display: inline-block;
  padding: 0.3rem 0.85rem;
  border-radius: 99px;
  background: color-mix(in srgb, #d97706 12%, transparent);
  border: 1px solid color-mix(in srgb, #d97706 30%, transparent);
  color: #b45309;
  font-size: 0.7rem;
  font-weight: 700;
  text-decoration: none;
}
html.dark .sim-phet-badge { color: #fbbf24; }
.sim-filter { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1.5rem; }
.sim-filter-btn { padding: 0.4rem 1rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.85rem; cursor: pointer; }
.sim-filter-btn.active, .sim-filter-btn:hover { background: rgba(0,188,212,0.1); color: #00bcd4; border-color: rgba(0,188,212,0.3); }
.sim-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 1rem; }
.sim-card { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.25rem; cursor: pointer; transition: all 0.15s; background: var(--t-surface); }
.sim-card:hover { border-color: #00bcd4; transform: translateY(-2px); box-shadow: 0 4px 16px rgba(0,188,212,0.15); }
.sim-card-icon { font-size: 2rem; margin-bottom: 0.5rem; }
.sim-card-subject { font-size: 0.7rem; font-weight: 700; color: #00bcd4; letter-spacing: 0.05em; text-transform: uppercase; }
.sim-card-title { font-size: 1rem; font-weight: 800; color: var(--t-text1); margin: 0.2rem 0; }
.sim-card-desc { font-size: 0.8rem; color: var(--t-text2); line-height: 1.5; margin-bottom: 0.75rem; }
.sim-card-footer { display: flex; justify-content: space-between; align-items: center; }
.sim-card-grade { font-size: 0.7rem; color: var(--t-text3); }
.sim-launch { font-size: 0.78rem; font-weight: 700; color: #00bcd4; }
.sim-back { padding: 0.4rem 0.85rem; background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 99px; color: var(--t-text2); font-size: 0.8rem; cursor: pointer; margin-bottom: 1.25rem; }
.sim-detail-header { display: flex; gap: 1rem; align-items: flex-start; margin-bottom: 1.5rem; }
.sim-detail-icon { font-size: 3rem; }
.sim-detail-sub { font-size: 0.75rem; font-weight: 700; color: #00bcd4; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.25rem; }
.sim-detail-title { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); }

/* PhET iframe panel */
.sim-visual-panel { border: 1px solid var(--t-border); border-radius: 16px; overflow: hidden; margin-bottom: 1.5rem; }
.sim-visual-title {
  padding: 0.65rem 1rem;
  background: var(--t-hover);
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--t-text2);
  border-bottom: 1px solid var(--t-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.sim-open-full {
  font-size: 0.7rem;
  font-weight: 600;
  color: #00bcd4;
  text-decoration: none;
}
.sim-iframe-wrap { position: relative; width: 100%; height: 580px; background: #1a1a2e; }
.sim-iframe-loading {
  position: absolute; inset: 0;
  display: flex; align-items: center; justify-content: center;
  gap: 0.5rem;
  color: rgba(255,255,255,0.6);
  font-size: 0.85rem;
}
.sim-loading-dot {
  width: 8px; height: 8px; border-radius: 50%;
  background: #00bcd4;
  animation: simDot 1.2s ease-in-out infinite;
}
.sim-loading-dot:nth-child(2) { animation-delay: 0.2s; }
.sim-loading-dot:nth-child(3) { animation-delay: 0.4s; }
@keyframes simDot { 0%,80%,100%{transform:scale(0.8);opacity:0.5} 40%{transform:scale(1.2);opacity:1} }
.sim-iframe {
  width: 100%; height: 100%;
  border: none;
  opacity: 0;
  transition: opacity 0.3s;
}
.sim-iframe-visible { opacity: 1; }
.sim-phet-credit {
  padding: 0.5rem 1rem;
  font-size: 0.65rem;
  color: var(--t-text3);
  background: var(--t-hover);
  border-top: 1px solid var(--t-border);
}
.sim-phet-credit a { color: #00bcd4; text-decoration: none; }

.sim-theory { margin-bottom: 1.5rem; }
.sim-theory-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.75rem; }
.sim-step { display: flex; gap: 1rem; margin-bottom: 1rem; }
.sim-step-num { width: 28px; height: 28px; border-radius: 50%; background: rgba(0,188,212,0.1); color: #00bcd4; font-weight: 800; font-size: 0.8rem; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.sim-step-heading { font-weight: 700; color: var(--t-text1); font-size: 0.9rem; margin-bottom: 0.25rem; }
.sim-step-text { color: var(--t-text2); font-size: 0.875rem; line-height: 1.6; white-space: pre-line; }
.sim-step-formula { margin-top: 0.5rem; padding: 0.5rem 0.85rem; background: rgba(0,188,212,0.07); border-left: 3px solid #00bcd4; border-radius: 0 8px 8px 0; font-family: monospace; font-size: 0.875rem; color: var(--t-text1); }
.sim-variables { margin-bottom: 1.5rem; }
.sim-vars-title { font-size: 0.78rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.75rem; }
.sim-vars-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 0.5rem; }
.sim-var-item { padding: 0.75rem; border: 1px solid var(--t-border); border-radius: 12px; text-align: center; }
.sim-var-symbol { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); font-family: serif; }
.sim-var-name { font-size: 0.72rem; color: var(--t-text2); }
.sim-var-unit { font-size: 0.68rem; color: var(--t-text3); }
.sim-ai-section { margin-bottom: 1.5rem; }
.sim-ai-btn { padding: 0.65rem 1.25rem; background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; border: none; border-radius: 12px; font-weight: 700; cursor: pointer; font-size: 0.875rem; }
.sim-ai-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.sim-ai-result { margin-top: 0.75rem; padding: 1rem; background: linear-gradient(135deg, rgba(109,84,232,0.05), rgba(168,85,247,0.05)); border: 1px solid rgba(109,84,232,0.2); border-radius: 12px; }
.sim-ai-header { font-size: 0.78rem; font-weight: 700; color: #6d54e8; margin-bottom: 0.5rem; }
.sim-ai-text { font-size: 0.875rem; color: var(--t-text1); line-height: 1.7; white-space: pre-wrap; }
.sim-exam-corner { border: 1px solid rgba(245,124,0,0.2); border-radius: 16px; padding: 1.25rem; background: rgba(245,124,0,0.03); }
.sim-exam-title { font-size: 0.78rem; font-weight: 700; color: #f57c00; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.75rem; }
.sim-exam-q { display: flex; gap: 0.5rem; padding: 0.5rem 0; border-bottom: 1px solid var(--t-border); font-size: 0.875rem; color: var(--t-text1); line-height: 1.5; }
.sim-exam-q:last-child { border-bottom: none; }
.sim-exam-q-num { color: #f57c00; font-weight: 700; flex-shrink: 0; }

@media (max-width: 640px) {
  .sim-iframe-wrap { height: 400px; }
}
</style>
