<template>
  <div class="exp-root">
    <div class="exp-hero">
      <div class="exp-hero-icon">⚗️</div>
      <h1 class="exp-hero-title">Virtual Science Lab</h1>
      <p class="exp-hero-sub">PhET Interactive Simulations · University of Colorado Boulder</p>
    </div>

    <div class="exp-filter">
      <button v-for="cat in ['All','Physics','Chemistry','Biology']" :key="cat"
        @click="activeFilter = cat" class="exp-filter-btn" :class="{ active: activeFilter === cat }">
        {{ cat }}
      </button>
    </div>

    <div class="exp-grid">
      <div v-for="exp in filteredExps" :key="exp.id" class="exp-card">
        <div class="exp-card-top">
          <div class="exp-card-icon">{{ exp.icon }}</div>
          <div class="exp-subject-tag">{{ exp.subject }}</div>
        </div>
        <div class="exp-card-title">{{ exp.title }}</div>
        <div class="exp-card-desc">{{ exp.desc }}</div>
        <div class="exp-tags">
          <span v-for="t in exp.tags" :key="t" class="exp-tag">{{ t }}</span>
        </div>
        <div class="exp-actions">
          <button @click="openTheory(exp)" class="exp-btn-theory">📋 Theory</button>
          <button @click="openLab(exp)" class="exp-btn-lab">🔬 Launch Virtual Lab →</button>
        </div>
      </div>
    </div>

    <!-- Theory modal -->
    <Teleport to="body">
      <Transition name="mfade">
        <div v-if="theoryExp" class="modal-overlay" @click.self="theoryExp = null">
          <div class="modal-box">
            <div class="modal-hdr">
              <span class="modal-icon">{{ theoryExp.icon }}</span>
              <div class="modal-hdr-text">
                <div class="modal-subject">{{ theoryExp.subject }}</div>
                <div class="modal-title">{{ theoryExp.title }}</div>
              </div>
              <button class="modal-x" @click="theoryExp = null">✕</button>
            </div>
            <div class="modal-body">
              <div class="msec">
                <div class="msec-title">🎯 Objective</div>
                <p class="msec-text">{{ theoryExp.objective }}</p>
              </div>
              <div class="msec">
                <div class="msec-title">🧰 Materials</div>
                <ul class="msec-list">
                  <li v-for="m in theoryExp.materials" :key="m">{{ m }}</li>
                </ul>
              </div>
              <div class="msec">
                <div class="msec-title">📋 Procedure</div>
                <ol class="msec-list msec-ol">
                  <li v-for="p in theoryExp.procedure" :key="p">{{ p }}</li>
                </ol>
              </div>
              <div class="msec">
                <div class="msec-title">📊 Expected Result</div>
                <p class="msec-text msec-result">{{ theoryExp.result }}</p>
              </div>
            </div>
            <div class="modal-foot">
              <button @click="openLabFromTheory" class="modal-lab-btn">🔬 Open Virtual Lab</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Virtual Lab iframe modal -->
    <Teleport to="body">
      <Transition name="mfade">
        <div v-if="labExp" class="lab-overlay" @click.self="closeLab">
          <div class="lab-box">
            <div class="lab-hdr">
              <div class="lab-hdr-left">
                <span class="lab-icon">{{ labExp.icon }}</span>
                <div>
                  <div class="lab-subject">{{ labExp.subject }}</div>
                  <div class="lab-title">{{ labExp.title }}</div>
                </div>
              </div>
              <div class="lab-hdr-right">
                <a :href="labExp.labUrl" target="_blank" rel="noopener" class="lab-fullbtn">↗ Full Screen</a>
                <button class="lab-closebtn" @click="closeLab">✕ Close</button>
              </div>
            </div>
            <div class="lab-frame-wrap">
              <div v-if="!labLoaded" class="lab-loading">
                <div class="ldots"><span/><span/><span/></div>
                <p>Loading simulation…</p>
                <p class="lab-prov">{{ labExp.provider }}</p>
              </div>
              <iframe
                :src="labExp.labUrl"
                class="lab-iframe"
                :class="{ ready: labLoaded }"
                frameborder="0" allowfullscreen allow="fullscreen"
                :title="labExp.title"
                @load="labLoaded = true" />
            </div>
            <div class="lab-foot">
              <span class="lab-prov-badge">{{ labExp.provider }}</span>
              <span class="lab-tip">💡 Interact directly in the simulation above</span>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'
import { experiments } from '@/assets/data/phet'

const activeFilter = ref('All')
const theoryExp = ref(null)
const labExp = ref(null)
const labLoaded = ref(false)

const filteredExps = computed(() => activeFilter.value === 'All' ? experiments : experiments.filter(e => e.subject === activeFilter.value))

function openTheory(exp) { theoryExp.value = exp }
function openLab(exp) { labExp.value = exp; labLoaded.value = false }
function closeLab() { labExp.value = null; labLoaded.value = false }
function openLabFromTheory() { const e = theoryExp.value; theoryExp.value = null; if (e) openLab(e) }
</script>

<style scoped>
.exp-root { max-width: 960px; margin: 0 auto; padding: 1.5rem; }
.exp-hero { text-align: center; padding: 1.5rem 1rem 1rem; }
.exp-hero-icon { font-size: 3rem; margin-bottom: 0.5rem; }
.exp-hero-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.exp-hero-sub { font-size: 0.8rem; color: var(--t-text3); margin-top: 0.25rem; }
.exp-filter { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1.25rem; }
.exp-filter-btn { padding: 0.4rem 1rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.85rem; cursor: pointer; }
.exp-filter-btn.active, .exp-filter-btn:hover { background: rgba(0,188,212,.1); color: #00bcd4; border-color: rgba(0,188,212,.3); }
.exp-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(270px,1fr)); gap: 1rem; }
.exp-card { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.25rem; background: var(--t-surface); display: flex; flex-direction: column; gap: 0.7rem; }
.exp-card-top { display: flex; align-items: center; justify-content: space-between; }
.exp-card-icon { font-size: 2rem; }
.exp-subject-tag { font-size: 0.65rem; font-weight: 700; padding: 0.2rem 0.55rem; border-radius: 99px; background: rgba(0,188,212,.1); color: #00bcd4; border: 1px solid rgba(0,188,212,.25); text-transform: uppercase; letter-spacing: .05em; }
.exp-card-title { font-size: 1.05rem; font-weight: 800; color: var(--t-text1); }
.exp-card-desc { font-size: 0.82rem; color: var(--t-text2); line-height: 1.5; flex: 1; }
.exp-tags { display: flex; flex-wrap: wrap; gap: .35rem; }
.exp-tag { font-size: .65rem; padding: .2rem .5rem; border-radius: 6px; background: var(--t-hover); color: var(--t-text3); border: 1px solid var(--t-border); }
.exp-actions { display: flex; flex-direction: column; gap: .5rem; }
.exp-btn-theory { padding: .6rem; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: .82rem; font-weight: 600; cursor: pointer; }
.exp-btn-theory:hover { background: var(--t-border); }
.exp-btn-lab { padding: .6rem; border-radius: 10px; background: linear-gradient(135deg,#00bcd4,#0097a7); color: #fff; border: none; font-size: .82rem; font-weight: 700; cursor: pointer; }
.exp-btn-lab:hover { opacity: .9; }

/* Theory modal */
.modal-overlay { position: fixed; inset: 0; z-index: 100; background: rgba(0,0,0,.65); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-box { background: var(--t-bg); border: 1px solid var(--t-border); border-radius: 20px; width: 100%; max-width: 560px; max-height: 85vh; display: flex; flex-direction: column; overflow: hidden; }
.modal-hdr { display: flex; gap: .75rem; align-items: flex-start; padding: 1.25rem; border-bottom: 1px solid var(--t-border); }
.modal-icon { font-size: 2rem; flex-shrink: 0; }
.modal-hdr-text { flex: 1; }
.modal-subject { font-size: .7rem; font-weight: 700; color: #00bcd4; text-transform: uppercase; letter-spacing: .05em; }
.modal-title { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); }
.modal-x { background: none; border: none; color: var(--t-text3); font-size: 1.1rem; cursor: pointer; padding: .25rem .5rem; flex-shrink: 0; }
.modal-body { overflow-y: auto; padding: 1.25rem; flex: 1; display: flex; flex-direction: column; gap: 1rem; }
.msec-title { font-size: .78rem; font-weight: 700; color: var(--t-text3); text-transform: uppercase; letter-spacing: .05em; margin-bottom: .4rem; }
.msec-text { font-size: .875rem; color: var(--t-text1); line-height: 1.6; margin: 0; }
.msec-result { background: rgba(0,188,212,.06); border-left: 3px solid #00bcd4; padding: .75rem; border-radius: 0 8px 8px 0; }
.msec-list { margin: 0; padding-left: 1.25rem; display: flex; flex-direction: column; gap: .3rem; }
.msec-list li { font-size: .875rem; color: var(--t-text1); line-height: 1.5; }
.modal-foot { padding: 1rem 1.25rem; border-top: 1px solid var(--t-border); }
.modal-lab-btn { width: 100%; padding: .7rem; border-radius: 12px; background: linear-gradient(135deg,#00bcd4,#0097a7); color: #fff; border: none; font-weight: 700; cursor: pointer; font-size: .875rem; }

/* Lab iframe modal */
.lab-overlay { position: fixed; inset: 0; z-index: 100; background: rgba(0,0,0,.8); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.lab-box { background: #0a0a12; border: 1px solid rgba(255,255,255,.1); border-radius: 20px; width: 100%; max-width: 900px; max-height: 92vh; display: flex; flex-direction: column; overflow: hidden; }
.lab-hdr { display: flex; align-items: center; justify-content: space-between; padding: .85rem 1.25rem; border-bottom: 1px solid rgba(255,255,255,.08); flex-shrink: 0; }
.lab-hdr-left { display: flex; align-items: center; gap: .75rem; }
.lab-icon { font-size: 1.75rem; }
.lab-subject { font-size: .65rem; font-weight: 700; color: #00bcd4; text-transform: uppercase; letter-spacing: .05em; }
.lab-title { font-size: .95rem; font-weight: 700; color: rgba(255,255,255,.9); }
.lab-hdr-right { display: flex; align-items: center; gap: .5rem; }
.lab-fullbtn { padding: .4rem .85rem; border-radius: 8px; background: rgba(255,255,255,.08); color: rgba(255,255,255,.7); font-size: .75rem; font-weight: 600; text-decoration: none; border: 1px solid rgba(255,255,255,.1); }
.lab-closebtn { padding: .4rem .85rem; border-radius: 8px; background: rgba(248,113,113,.1); color: #f87171; font-size: .75rem; font-weight: 600; border: 1px solid rgba(248,113,113,.2); cursor: pointer; }
.lab-frame-wrap { position: relative; flex: 1; min-height: 400px; background: #0d0d1a; }
.lab-loading { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; color: rgba(255,255,255,.5); gap: .75rem; }
.ldots { display: flex; gap: .35rem; }
.ldots span { width: 8px; height: 8px; border-radius: 50%; background: #00bcd4; animation: ldot 1.2s ease-in-out infinite; }
.ldots span:nth-child(2) { animation-delay: .2s; }
.ldots span:nth-child(3) { animation-delay: .4s; }
@keyframes ldot { 0%,80%,100%{transform:scale(.7);opacity:.4} 40%{transform:scale(1.1);opacity:1} }
.lab-loading p { font-size: .875rem; margin: 0; }
.lab-prov { font-size: .7rem; color: rgba(255,255,255,.3); }
.lab-iframe { width: 100%; height: 100%; border: none; opacity: 0; transition: opacity .3s; }
.lab-iframe.ready { opacity: 1; }
.lab-foot { display: flex; align-items: center; justify-content: space-between; padding: .5rem 1.25rem; border-top: 1px solid rgba(255,255,255,.07); flex-shrink: 0; }
.lab-prov-badge { font-size: .65rem; color: rgba(255,255,255,.3); }
.lab-tip { font-size: .65rem; color: rgba(255,255,255,.35); }

.mfade-enter-active, .mfade-leave-active { transition: opacity .2s; }
.mfade-enter-from, .mfade-leave-to { opacity: 0; }

@media (max-width: 640px) {
  .lab-frame-wrap { min-height: 320px; }
  .exp-grid { grid-template-columns: 1fr; }
}
</style>
