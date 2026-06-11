<template>
  <div class="pt-root">
    <!-- Tab bar -->
    <div class="pt-tabs">
      <button @click="activeTab = 'quick'" class="pt-tab" :class="{ active: activeTab === 'quick' }">
        🔍 Quick Reference
      </button>
      <button @click="activeTab = 'full'" class="pt-tab" :class="{ active: activeTab === 'full' }">
        📊 Full Periodic Table (Ptable)
      </button>
    </div>

    <!-- Quick Reference (existing) -->
    <div v-if="activeTab === 'quick'">
      <div class="pt-legend">
        <span class="pt-lg pt-alkali">Alkali Metal</span>
        <span class="pt-lg pt-ae">Alkali Earth</span>
        <span class="pt-lg pt-metal">Transition Metal</span>
        <span class="pt-lg pt-nonmetal">Non-metal</span>
        <span class="pt-lg pt-noble">Noble Gas</span>
        <span class="pt-lg pt-halogen">Halogen</span>
      </div>

      <div class="pt-grid">
        <div
          v-for="el in elements"
          :key="el.symbol"
          class="pt-cell"
          :class="el.group"
          @click="selected = el">
          <div class="pt-num">{{ el.atomic }}</div>
          <div class="pt-sym">{{ el.symbol }}</div>
          <div class="pt-name">{{ el.name }}</div>
          <div class="pt-mass">{{ el.mass }}</div>
        </div>
      </div>

      <Teleport to="body">
        <Transition name="mfade">
          <div v-if="selected" class="el-overlay" @click.self="selected = null">
            <div class="el-card" :class="selected.group">
              <button class="el-close" @click="selected = null">✕</button>
              <div class="el-num">{{ selected.atomic }}</div>
              <div class="el-sym">{{ selected.symbol }}</div>
              <div class="el-name">{{ selected.name }}</div>
              <div class="el-mass">Atomic Mass: {{ selected.mass }}</div>
              <div class="el-group-label">{{ selected.groupLabel }}</div>
              <div v-if="selected.config" class="el-config">{{ selected.config }}</div>
              <div v-if="selected.facts" class="el-facts">
                <p v-for="f in selected.facts" :key="f">{{ f }}</p>
              </div>
            </div>
          </div>
        </Transition>
      </Teleport>
    </div>

    <!-- Full Ptable iframe -->
    <div v-if="activeTab === 'full'" class="pt-iframe-section">
      <div class="pt-iframe-header">
        <span>Interactive Periodic Table</span>
        <a href="https://ptable.com" target="_blank" rel="noopener" class="pt-open-link">Open ptable.com ↗</a>
      </div>
      <div class="pt-iframe-wrap">
        <div v-if="!ptableLoaded" class="pt-loading">
          <div class="ldots"><span/><span/><span/></div>
          <p>Loading periodic table…</p>
          <p class="pt-credit">Powered by ptable.com</p>
        </div>
        <iframe
          src="https://ptable.com/#Properties"
          class="pt-iframe"
          :class="{ ready: ptableLoaded }"
          frameborder="0"
          allowfullscreen
          title="Interactive Periodic Table — ptable.com"
          @load="ptableLoaded = true" />
      </div>
      <div class="pt-iframe-credit">
        Interactive Periodic Table by <a href="https://ptable.com" target="_blank" rel="noopener">ptable.com</a>
        — click any element for properties, electron configuration, isotopes and more.
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const activeTab = ref('quick')
const selected = ref(null)
const ptableLoaded = ref(false)

const elements = [
  { atomic:1,  symbol:'H',  name:'Hydrogen',   mass:'1.008',  group:'pt-nonmetal', groupLabel:'Non-metal',       config:'1s¹',       facts:['Most abundant element in the universe','Makes up ~75% of all matter by mass'] },
  { atomic:2,  symbol:'He', name:'Helium',      mass:'4.003',  group:'pt-noble',    groupLabel:'Noble Gas',       config:'1s²',       facts:['Lightest noble gas','Used in balloons and cooling MRI machines'] },
  { atomic:3,  symbol:'Li', name:'Lithium',     mass:'6.941',  group:'pt-alkali',   groupLabel:'Alkali Metal',    config:'[He] 2s¹',  facts:['Used in rechargeable batteries','Reacts vigorously with water'] },
  { atomic:4,  symbol:'Be', name:'Beryllium',   mass:'9.012',  group:'pt-ae',       groupLabel:'Alkaline Earth',  config:'[He] 2s²' },
  { atomic:5,  symbol:'B',  name:'Boron',       mass:'10.81',  group:'pt-metal',    groupLabel:'Metalloid',       config:'[He] 2s² 2p¹' },
  { atomic:6,  symbol:'C',  name:'Carbon',      mass:'12.011', group:'pt-nonmetal', groupLabel:'Non-metal',       config:'[He] 2s² 2p²', facts:['Basis of all organic life','Diamond is the hardest natural substance'] },
  { atomic:7,  symbol:'N',  name:'Nitrogen',    mass:'14.007', group:'pt-nonmetal', groupLabel:'Non-metal',       config:'[He] 2s² 2p³', facts:['Makes up 78% of the atmosphere','Essential for proteins and DNA'] },
  { atomic:8,  symbol:'O',  name:'Oxygen',      mass:'15.999', group:'pt-nonmetal', groupLabel:'Non-metal',       config:'[He] 2s² 2p⁴', facts:['21% of Earth\'s atmosphere','Required for combustion and respiration'] },
  { atomic:9,  symbol:'F',  name:'Fluorine',    mass:'18.998', group:'pt-halogen',  groupLabel:'Halogen',         config:'[He] 2s² 2p⁵', facts:['Most electronegative element','Used in toothpaste (fluoride)'] },
  { atomic:10, symbol:'Ne', name:'Neon',        mass:'20.18',  group:'pt-noble',    groupLabel:'Noble Gas',       config:'[He] 2s² 2p⁶', facts:['Used in neon signs','Glows orange-red in discharge tubes'] },
  { atomic:11, symbol:'Na', name:'Sodium',      mass:'22.99',  group:'pt-alkali',   groupLabel:'Alkali Metal',    config:'[Ne] 3s¹',  facts:['Sodium chloride (table salt) is NaCl','Burns with a bright yellow flame'] },
  { atomic:12, symbol:'Mg', name:'Magnesium',   mass:'24.305', group:'pt-ae',       groupLabel:'Alkaline Earth',  config:'[Ne] 3s²',  facts:['Burns with a brilliant white flame','Essential mineral for the human body'] },
  { atomic:13, symbol:'Al', name:'Aluminium',   mass:'26.982', group:'pt-metal',    groupLabel:'Post-Transition', config:'[Ne] 3s² 3p¹', facts:['Most abundant metal in Earth\'s crust','Very light and corrosion-resistant'] },
  { atomic:14, symbol:'Si', name:'Silicon',     mass:'28.085', group:'pt-metal',    groupLabel:'Metalloid',       config:'[Ne] 3s² 3p²', facts:['Basis of semiconductors and computer chips','Second most abundant element in crust'] },
  { atomic:15, symbol:'P',  name:'Phosphorus',  mass:'30.974', group:'pt-nonmetal', groupLabel:'Non-metal',       config:'[Ne] 3s² 3p³' },
  { atomic:16, symbol:'S',  name:'Sulfur',      mass:'32.06',  group:'pt-nonmetal', groupLabel:'Non-metal',       config:'[Ne] 3s² 3p⁴', facts:['Yellow solid at room temperature','Used in manufacturing sulfuric acid'] },
  { atomic:17, symbol:'Cl', name:'Chlorine',    mass:'35.45',  group:'pt-halogen',  groupLabel:'Halogen',         config:'[Ne] 3s² 3p⁵', facts:['Yellow-green toxic gas','Used to purify water'] },
  { atomic:18, symbol:'Ar', name:'Argon',       mass:'39.948', group:'pt-noble',    groupLabel:'Noble Gas',       config:'[Ne] 3s² 3p⁶', facts:['Most abundant noble gas on Earth','Used in light bulbs to prevent filament oxidation'] },
  { atomic:19, symbol:'K',  name:'Potassium',   mass:'39.098', group:'pt-alkali',   groupLabel:'Alkali Metal',    config:'[Ar] 4s¹',  facts:['Essential for nerve signals in the body','Burns with a lilac/violet flame'] },
  { atomic:20, symbol:'Ca', name:'Calcium',     mass:'40.078', group:'pt-ae',       groupLabel:'Alkaline Earth',  config:'[Ar] 4s²',  facts:['Essential for bones and teeth','Used in cement (calcium carbonate)'] },
  { atomic:26, symbol:'Fe', name:'Iron',        mass:'55.845', group:'pt-metal',    groupLabel:'Transition Metal', config:'[Ar] 3d⁶ 4s²', facts:['Most used metal in the world (steel)','Core of the Earth is mostly iron'] },
  { atomic:29, symbol:'Cu', name:'Copper',      mass:'63.546', group:'pt-metal',    groupLabel:'Transition Metal', config:'[Ar] 3d¹⁰ 4s¹', facts:['Excellent electrical conductor','Used in wiring and plumbing'] },
  { atomic:30, symbol:'Zn', name:'Zinc',        mass:'65.38',  group:'pt-metal',    groupLabel:'Transition Metal', config:'[Ar] 3d¹⁰ 4s²', facts:['Used to galvanize iron (rust prevention)','Essential trace element in human body'] },
  { atomic:47, symbol:'Ag', name:'Silver',      mass:'107.87', group:'pt-metal',    groupLabel:'Transition Metal', config:'[Kr] 4d¹⁰ 5s¹', facts:['Best electrical conductor','Used in jewelry and photography'] },
  { atomic:79, symbol:'Au', name:'Gold',        mass:'196.97', group:'pt-metal',    groupLabel:'Transition Metal', config:'[Xe] 4f¹⁴ 5d¹⁰ 6s¹', facts:['Does not rust or tarnish','Currency standard for centuries'] },
  { atomic:82, symbol:'Pb', name:'Lead',        mass:'207.2',  group:'pt-metal',    groupLabel:'Post-Transition', config:'[Xe] 4f¹⁴ 5d¹⁰ 6s² 6p²', facts:['Very dense and soft metal','Used in batteries and radiation shielding'] },
]
</script>

<style scoped>
.pt-root { max-width: 960px; margin: 0 auto; padding: 1.5rem; }
.pt-tabs { display: flex; gap: .5rem; margin-bottom: 1.25rem; }
.pt-tab { padding: .55rem 1.25rem; border-radius: 12px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: .875rem; font-weight: 600; cursor: pointer; transition: all .15s; }
.pt-tab.active, .pt-tab:hover { background: rgba(0,188,212,.1); color: #00bcd4; border-color: rgba(0,188,212,.3); }

/* Legend */
.pt-legend { display: flex; flex-wrap: wrap; gap: .5rem; margin-bottom: 1rem; }
.pt-lg { padding: .25rem .7rem; border-radius: 8px; font-size: .7rem; font-weight: 700; cursor: default; }
.pt-alkali { background: rgba(239,68,68,.12); color: #ef4444; }
.pt-ae { background: rgba(251,146,60,.12); color: #f97316; }
.pt-metal { background: rgba(234,179,8,.12); color: #ca8a04; }
.pt-nonmetal { background: rgba(34,197,94,.12); color: #16a34a; }
.pt-noble { background: rgba(99,102,241,.12); color: #6366f1; }
.pt-halogen { background: rgba(168,85,247,.12); color: #a855f7; }

.pt-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(90px,1fr)); gap: .5rem; }
.pt-cell { border-radius: 12px; padding: .6rem .4rem; cursor: pointer; border: 1px solid transparent; transition: all .15s; text-align: center; }
.pt-cell:hover { transform: translateY(-2px); border-color: currentColor; }
.pt-alkali.pt-cell { background: rgba(239,68,68,.07); color: #ef4444; }
.pt-ae.pt-cell { background: rgba(251,146,60,.07); color: #f97316; }
.pt-metal.pt-cell { background: rgba(234,179,8,.07); color: #ca8a04; }
.pt-nonmetal.pt-cell { background: rgba(34,197,94,.07); color: #16a34a; }
.pt-noble.pt-cell { background: rgba(99,102,241,.07); color: #6366f1; }
.pt-halogen.pt-cell { background: rgba(168,85,247,.07); color: #a855f7; }
.pt-num { font-size: .62rem; font-weight: 600; opacity: .7; text-align: left; }
.pt-sym { font-size: 1.4rem; font-weight: 800; line-height: 1.1; }
.pt-name { font-size: .6rem; color: var(--t-text2); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.pt-mass { font-size: .6rem; color: var(--t-text3); }

/* Element detail overlay */
.el-overlay { position: fixed; inset: 0; z-index: 100; background: rgba(0,0,0,.65); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.el-card { position: relative; border-radius: 20px; padding: 2rem; width: 100%; max-width: 360px; text-align: center; border: 2px solid; }
.el-card.pt-alkali { background: rgba(239,68,68,.08); border-color: rgba(239,68,68,.3); color: #ef4444; }
.el-card.pt-ae { background: rgba(251,146,60,.08); border-color: rgba(251,146,60,.3); color: #f97316; }
.el-card.pt-metal { background: rgba(234,179,8,.08); border-color: rgba(234,179,8,.3); color: #ca8a04; }
.el-card.pt-nonmetal { background: rgba(34,197,94,.08); border-color: rgba(34,197,94,.3); color: #16a34a; }
.el-card.pt-noble { background: rgba(99,102,241,.08); border-color: rgba(99,102,241,.3); color: #6366f1; }
.el-card.pt-halogen { background: rgba(168,85,247,.08); border-color: rgba(168,85,247,.3); color: #a855f7; }
.el-close { position: absolute; top: .75rem; right: .75rem; background: none; border: none; font-size: 1.1rem; cursor: pointer; opacity: .6; color: inherit; }
.el-num { font-size: .9rem; font-weight: 600; opacity: .7; }
.el-sym { font-size: 5rem; font-weight: 900; line-height: 1; }
.el-name { font-size: 1.2rem; font-weight: 700; margin-bottom: .35rem; color: var(--t-text1); }
.el-mass, .el-group-label { font-size: .8rem; color: var(--t-text2); }
.el-config { margin-top: .75rem; padding: .5rem .85rem; border-radius: 8px; background: var(--t-hover); font-family: monospace; font-size: .85rem; color: var(--t-text1); display: inline-block; }
.el-facts { margin-top: .85rem; text-align: left; display: flex; flex-direction: column; gap: .35rem; }
.el-facts p { font-size: .8rem; color: var(--t-text2); margin: 0; padding: .4rem .7rem; background: var(--t-hover); border-radius: 8px; line-height: 1.45; }

/* Ptable iframe */
.pt-iframe-section { }
.pt-iframe-header { display: flex; align-items: center; justify-content: space-between; padding: .65rem 1rem; background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 14px 14px 0 0; font-size: .82rem; font-weight: 700; color: var(--t-text2); }
.pt-open-link { font-size: .75rem; color: #00bcd4; text-decoration: none; font-weight: 600; }
.pt-iframe-wrap { position: relative; width: 100%; height: 640px; background: #fff; border: 1px solid var(--t-border); border-top: none; }
.pt-loading { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: .75rem; background: var(--t-bg); }
.ldots { display: flex; gap: .35rem; }
.ldots span { width: 8px; height: 8px; border-radius: 50%; background: #00bcd4; animation: ldot 1.2s ease-in-out infinite; }
.ldots span:nth-child(2) { animation-delay: .2s; }
.ldots span:nth-child(3) { animation-delay: .4s; }
@keyframes ldot { 0%,80%,100%{transform:scale(.7);opacity:.4} 40%{transform:scale(1.1);opacity:1} }
.pt-loading p { font-size: .875rem; color: var(--t-text2); margin: 0; }
.pt-credit { font-size: .7rem; color: var(--t-text3); }
.pt-iframe { width: 100%; height: 100%; border: none; opacity: 0; transition: opacity .3s; }
.pt-iframe.ready { opacity: 1; }
.pt-iframe-credit { padding: .5rem 1rem; font-size: .68rem; color: var(--t-text3); border: 1px solid var(--t-border); border-top: none; border-radius: 0 0 14px 14px; background: var(--t-hover); }
.pt-iframe-credit a { color: #00bcd4; text-decoration: none; }

.mfade-enter-active, .mfade-leave-active { transition: opacity .2s; }
.mfade-enter-from, .mfade-leave-to { opacity: 0; }

@media (max-width: 600px) {
  .pt-grid { grid-template-columns: repeat(auto-fill, minmax(72px,1fr)); }
  .pt-iframe-wrap { height: 420px; }
}
</style>
