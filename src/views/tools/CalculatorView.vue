<template>
  <div class="cv-root">
    <div class="cv-tabs">
      <button @click="activeTab = 'builtin'" class="cv-tab" :class="{ active: activeTab === 'builtin' }">
        🔢 Built-in
      </button>
      <button @click="activeTab = 'desmos'" class="cv-tab" :class="{ active: activeTab === 'desmos' }">
        📈 Desmos
      </button>
      <button @click="activeTab = 'geogebra'" class="cv-tab" :class="{ active: activeTab === 'geogebra' }">
        📐 GeoGebra
      </button>
    </div>

    <!-- Built-in scientific calculator -->
    <div v-if="activeTab === 'builtin'" class="calc-wrap">
      <div class="calc-display">
        <div class="calc-expr">{{ expression || '0' }}</div>
        <div v-if="result !== null" class="calc-result">= {{ result }}</div>
      </div>
      <div class="calc-btns">
        <button v-for="btn in buttons" :key="btn.label"
          @click="handleBtn(btn)"
          class="calc-btn"
          :class="[btn.type]">
          {{ btn.label }}
        </button>
      </div>
      <div v-if="error" class="calc-error">{{ error }}</div>
    </div>

    <!-- Desmos Scientific iframe -->
    <div v-if="activeTab === 'desmos'" class="ext-section">
      <div class="ext-header">
        <span>Desmos Scientific Calculator</span>
        <a href="https://www.desmos.com/scientific" target="_blank" rel="noopener" class="ext-link">Open desmos.com ↗</a>
      </div>
      <div class="ext-iframe-wrap">
        <div v-if="!desmosLoaded" class="ext-loading">
          <div class="ldots"><span/><span/><span/></div>
          <p>Loading Desmos…</p>
        </div>
        <iframe
          src="https://www.desmos.com/scientific"
          class="ext-iframe"
          :class="{ ready: desmosLoaded }"
          frameborder="0" allowfullscreen allow="fullscreen"
          title="Desmos Scientific Calculator"
          @load="desmosLoaded = true" />
      </div>
      <div class="ext-credit">
        Desmos Scientific Calculator by <a href="https://www.desmos.com" target="_blank" rel="noopener">desmos.com</a>
        — supports trigonometry, logarithms, statistics and more.
      </div>
    </div>

    <!-- GeoGebra Scientific iframe -->
    <div v-if="activeTab === 'geogebra'" class="ext-section">
      <div class="ext-header">
        <span>GeoGebra Scientific Calculator</span>
        <a href="https://www.geogebra.org/scientific" target="_blank" rel="noopener" class="ext-link">Open geogebra.org ↗</a>
      </div>
      <div class="ext-iframe-wrap">
        <div v-if="!geogebraLoaded" class="ext-loading">
          <div class="ldots"><span/><span/><span/></div>
          <p>Loading GeoGebra…</p>
        </div>
        <iframe
          src="https://www.geogebra.org/scientific?embed"
          class="ext-iframe"
          :class="{ ready: geogebraLoaded }"
          frameborder="0" allowfullscreen allow="fullscreen"
          title="GeoGebra Scientific Calculator"
          @load="geogebraLoaded = true" />
      </div>
      <div class="ext-credit">
        GeoGebra Scientific Calculator by <a href="https://www.geogebra.org" target="_blank" rel="noopener">geogebra.org</a>
        — geometry, algebra, and calculus suite.
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const activeTab = ref('builtin')
const expression = ref('')
const result = ref(null)
const error = ref('')
const desmosLoaded = ref(false)
const geogebraLoaded = ref(false)

const buttons = [
  { label: 'C',    type: 'op-clear',    action: 'clear' },
  { label: '(',    type: 'op-paren',    action: 'append', value: '(' },
  { label: ')',    type: 'op-paren',    action: 'append', value: ')' },
  { label: '⌫',    type: 'op-del',      action: 'delete' },
  { label: 'sin',  type: 'fn',          action: 'append', value: 'Math.sin(deg(' },
  { label: 'cos',  type: 'fn',          action: 'append', value: 'Math.cos(deg(' },
  { label: 'tan',  type: 'fn',          action: 'append', value: 'Math.tan(deg(' },
  { label: '÷',    type: 'op',          action: 'append', value: '/' },
  { label: '7',    type: 'num',         action: 'append', value: '7' },
  { label: '8',    type: 'num',         action: 'append', value: '8' },
  { label: '9',    type: 'num',         action: 'append', value: '9' },
  { label: '×',    type: 'op',          action: 'append', value: '*' },
  { label: '4',    type: 'num',         action: 'append', value: '4' },
  { label: '5',    type: 'num',         action: 'append', value: '5' },
  { label: '6',    type: 'num',         action: 'append', value: '6' },
  { label: '−',    type: 'op',          action: 'append', value: '-' },
  { label: '1',    type: 'num',         action: 'append', value: '1' },
  { label: '2',    type: 'num',         action: 'append', value: '2' },
  { label: '3',    type: 'num',         action: 'append', value: '3' },
  { label: '+',    type: 'op',          action: 'append', value: '+' },
  { label: '0',    type: 'num wide',    action: 'append', value: '0' },
  { label: '.',    type: 'num',         action: 'append', value: '.' },
  { label: '=',    type: 'op-eq',       action: 'calculate' },
  { label: 'π',    type: 'fn',          action: 'append', value: 'Math.PI' },
  { label: '√',    type: 'fn',          action: 'append', value: 'Math.sqrt(' },
  { label: 'log',  type: 'fn',          action: 'append', value: 'Math.log10(' },
  { label: 'ln',   type: 'fn',          action: 'append', value: 'Math.log(' },
  { label: 'xʸ',   type: 'fn',          action: 'append', value: '**' },
  { label: '%',    type: 'op',          action: 'append', value: '/100' },
]

function deg(x) { return x * Math.PI / 180 }

function handleBtn(btn) {
  error.value = ''
  if (btn.action === 'clear')     { expression.value = ''; result.value = null }
  else if (btn.action === 'delete') { expression.value = expression.value.slice(0, -1) }
  else if (btn.action === 'append') { expression.value += btn.value; result.value = null }
  else if (btn.action === 'calculate') calculate()
}

function calculate() {
  try {
    // Replace display symbols with evaluable JS
    let expr = expression.value
      .replace(/×/g, '*')
      .replace(/÷/g, '/')
      .replace(/−/g, '-')
    // Close any unclosed parentheses
    const opens = (expr.match(/\(/g) || []).length
    const closes = (expr.match(/\)/g) || []).length
    expr += ')'.repeat(Math.max(0, opens - closes))
    // eslint-disable-next-line no-new-func
    const fn = new Function('Math', 'deg', `"use strict"; return (${expr})`)
    const val = fn(Math, deg)
    if (!isFinite(val)) throw new Error('Result out of range')
    result.value = parseFloat(val.toPrecision(12)).toString()
  } catch {
    error.value = 'Invalid expression'
    result.value = null
  }
}
</script>

<style scoped>
.cv-root { max-width: 600px; margin: 0 auto; padding: 1.5rem; }
.cv-tabs { display: flex; gap: .5rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
.cv-tab { padding: .55rem 1.1rem; border-radius: 12px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: .875rem; font-weight: 600; cursor: pointer; transition: all .15s; }
.cv-tab.active, .cv-tab:hover { background: rgba(0,188,212,.1); color: #00bcd4; border-color: rgba(0,188,212,.3); }

/* Built-in calculator */
.calc-wrap { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 20px; overflow: hidden; }
.calc-display { padding: 1.25rem 1.25rem .75rem; min-height: 80px; background: var(--t-hover); display: flex; flex-direction: column; align-items: flex-end; gap: .25rem; }
.calc-expr { font-size: 1.1rem; color: var(--t-text2); word-break: break-all; text-align: right; font-family: monospace; }
.calc-result { font-size: 2rem; font-weight: 800; color: var(--t-text1); font-family: monospace; }
.calc-error { padding: .5rem 1rem; font-size: .82rem; color: #f87171; text-align: center; background: rgba(248,113,113,.07); }
.calc-btns { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1px; background: var(--t-border); }
.calc-btn { padding: 1rem; font-size: 1rem; font-weight: 600; background: var(--t-surface); border: none; cursor: pointer; color: var(--t-text1); transition: background .12s; }
.calc-btn.wide { grid-column: span 2; }
.calc-btn:hover { background: var(--t-hover); }
.calc-btn.op { color: #00bcd4; }
.calc-btn.op-eq { background: linear-gradient(135deg, #00bcd4, #0097a7); color: white; }
.calc-btn.op-eq:hover { opacity: .9; }
.calc-btn.op-clear { color: #f87171; }
.calc-btn.op-del { color: #f87171; }
.calc-btn.fn { color: #a78bfa; font-size: .85rem; }
.calc-btn.op-paren { color: var(--t-text3); }

/* External iframe tools */
.ext-section { }
.ext-header { display: flex; align-items: center; justify-content: space-between; padding: .65rem 1rem; background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 14px 14px 0 0; font-size: .82rem; font-weight: 700; color: var(--t-text2); }
.ext-link { font-size: .75rem; color: #00bcd4; text-decoration: none; font-weight: 600; }
.ext-iframe-wrap { position: relative; width: 100%; height: 560px; background: var(--t-bg); border: 1px solid var(--t-border); border-top: none; }
.ext-loading { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: .75rem; }
.ldots { display: flex; gap: .35rem; }
.ldots span { width: 8px; height: 8px; border-radius: 50%; background: #00bcd4; animation: ldot 1.2s ease-in-out infinite; }
.ldots span:nth-child(2) { animation-delay: .2s; }
.ldots span:nth-child(3) { animation-delay: .4s; }
@keyframes ldot { 0%,80%,100%{transform:scale(.7);opacity:.4} 40%{transform:scale(1.1);opacity:1} }
.ext-loading p { font-size: .875rem; color: var(--t-text2); margin: 0; }
.ext-iframe { width: 100%; height: 100%; border: none; opacity: 0; transition: opacity .3s; }
.ext-iframe.ready { opacity: 1; }
.ext-credit { padding: .5rem 1rem; font-size: .68rem; color: var(--t-text3); border: 1px solid var(--t-border); border-top: none; border-radius: 0 0 14px 14px; background: var(--t-hover); }
.ext-credit a { color: #00bcd4; text-decoration: none; }

@media (max-width: 480px) {
  .ext-iframe-wrap { height: 420px; }
  .calc-btn { padding: .85rem .5rem; font-size: .9rem; }
}
</style>
