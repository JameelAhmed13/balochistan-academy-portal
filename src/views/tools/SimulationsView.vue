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
import { simulations } from '@/assets/data/phet'

const activeSim = ref(null)
const activeFilter = ref('All')
const aiText = ref('')
const aiLoading = ref(false)
const iframeLoaded = ref(false)

const categories = ['All', 'Physics', 'Chemistry', 'Biology']

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
