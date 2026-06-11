<template>
  <div class="gl-root">
    <div class="gl-hero">
      <div class="gl-hero-icon">📚</div>
      <h1 class="gl-hero-title">Global Study Library</h1>
      <p class="gl-hero-sub">Curated free educational resources for Balochistan Board students</p>
    </div>

    <!-- Search -->
    <div class="gl-search-row">
      <div class="gl-search-wrap">
        <span class="gl-search-icon">🔍</span>
        <input v-model="search" class="gl-search" placeholder="Search resources…" />
        <button v-if="search" @click="search = ''" class="gl-search-clear">✕</button>
      </div>
    </div>

    <!-- Category filter -->
    <div class="gl-filters">
      <button v-for="cat in categories" :key="cat.id"
        @click="activeCategory = cat.id" class="gl-filter-btn"
        :class="{ active: activeCategory === cat.id }">
        {{ cat.icon }} {{ cat.label }}
      </button>
    </div>

    <!-- Resource count -->
    <div class="gl-count" v-if="filteredResources.length">
      {{ filteredResources.length }} resource{{ filteredResources.length !== 1 ? 's' : '' }}
      <span v-if="search || activeCategory !== 'all'"> found</span>
    </div>

    <!-- Resource grid -->
    <div v-if="filteredResources.length" class="gl-grid">
      <a
        v-for="res in filteredResources"
        :key="res.url"
        :href="res.url"
        target="_blank"
        rel="noopener noreferrer"
        class="gl-card">
        <div class="gl-card-top">
          <div class="gl-card-icon">{{ res.icon }}</div>
          <div class="gl-card-tags">
            <span class="gl-tag gl-cat-tag">{{ res.catLabel }}</span>
            <span v-if="res.free" class="gl-tag gl-free-tag">Free</span>
            <span v-if="res.urdu" class="gl-tag gl-urdu-tag">اردو</span>
          </div>
        </div>
        <div class="gl-card-title">{{ res.title }}</div>
        <div class="gl-card-desc">{{ res.desc }}</div>
        <div class="gl-card-footer">
          <span class="gl-card-domain">{{ res.domain }}</span>
          <span class="gl-visit">Visit →</span>
        </div>
      </a>
    </div>

    <div v-else class="gl-empty">
      <div class="gl-empty-icon">🔎</div>
      <p>No resources match "<strong>{{ search }}</strong>"</p>
      <button @click="search = ''; activeCategory = 'all'" class="gl-clear-btn">Clear filters</button>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const search = ref('')
const activeCategory = ref('all')

const categories = [
  { id: 'all',       icon: '🌐', label: 'All' },
  { id: 'simulations', icon: '🔬', label: 'Simulations' },
  { id: 'video',     icon: '🎬', label: 'Video Lessons' },
  { id: 'textbooks', icon: '📖', label: 'Textbooks' },
  { id: 'practice',  icon: '✏️', label: 'Practice & Tests' },
  { id: 'reference', icon: '📋', label: 'Reference' },
  { id: 'pakistan',  icon: '🇵🇰', label: 'Pakistan' },
]

const resources = [
  /* Simulations */
  {
    icon:'⚡', category:'simulations', catLabel:'Simulations', free:true,
    title:'PhET Interactive Simulations',
    desc:'180+ free, peer-reviewed interactive simulations for Physics, Chemistry, Biology, and Math by University of Colorado Boulder.',
    url:'https://phet.colorado.edu',
    domain:'phet.colorado.edu',
    subjects:['physics','chemistry','biology','math'],
  },
  {
    icon:'🗺️', category:'simulations', catLabel:'Simulations', free:true,
    title:'GeoGebra – Math Simulations',
    desc:'Interactive graphing calculators, geometry tools, 3D visualizations, and classroom activities for all math levels.',
    url:'https://www.geogebra.org',
    domain:'geogebra.org',
    subjects:['math'],
  },
  {
    icon:'🔬', category:'simulations', catLabel:'Simulations', free:true,
    title:'ChemCollective – Virtual Lab',
    desc:'Carnegie Mellon\'s virtual chemistry laboratory with realistic stockroom simulations and pre-lab activities.',
    url:'https://chemcollective.org',
    domain:'chemcollective.org',
    subjects:['chemistry'],
  },
  {
    icon:'🧬', category:'simulations', catLabel:'Simulations', free:true,
    title:'HHMI BioInteractive',
    desc:'Free biology resources including films, animations, and data analysis activities from Howard Hughes Medical Institute.',
    url:'https://www.biointeractive.org',
    domain:'biointeractive.org',
    subjects:['biology'],
  },

  /* Video Lessons */
  {
    icon:'▶️', category:'video', catLabel:'Video Lessons', free:true,
    title:'Khan Academy – Full Curriculum',
    desc:'Free world-class education. Complete courses in Math, Science, Computing, and Humanities with practice exercises.',
    url:'https://www.khanacademy.org',
    domain:'khanacademy.org',
    subjects:['math','physics','chemistry','biology','english'],
  },
  {
    icon:'🎓', category:'video', catLabel:'Video Lessons', free:true, urdu:true,
    title:'Sabaq.pk – Urdu Video Lessons',
    desc:'Pakistani educational platform with Urdu-language video lessons for matric and intermediate students following provincial curricula.',
    url:'https://sabaq.pk',
    domain:'sabaq.pk',
    subjects:['physics','chemistry','biology','math','urdu'],
  },
  {
    icon:'🎬', category:'video', catLabel:'Video Lessons', free:true,
    title:'CrashCourse – YouTube',
    desc:'Engaging, fast-paced video series on Biology, Chemistry, Physics, History, and more. Perfect for quick concept review.',
    url:'https://www.youtube.com/@crashcourse',
    domain:'youtube.com/@crashcourse',
    subjects:['physics','chemistry','biology','history'],
  },
  {
    icon:'🏛️', category:'video', catLabel:'Video Lessons', free:true,
    title:'MIT OpenCourseWare',
    desc:'Free MIT course materials including lecture notes, problem sets, and exams for advanced learners and teachers.',
    url:'https://ocw.mit.edu',
    domain:'ocw.mit.edu',
    subjects:['physics','chemistry','math','engineering'],
  },
  {
    icon:'🌍', category:'video', catLabel:'Video Lessons', free:true,
    title:'National Geographic Education',
    desc:'Free geography, science, and history resources with interactive maps, videos, and articles for students.',
    url:'https://education.nationalgeographic.org',
    domain:'education.nationalgeographic.org',
    subjects:['biology','geography','general'],
  },

  /* Textbooks & Reading */
  {
    icon:'📗', category:'textbooks', catLabel:'Textbooks', free:true,
    title:'OpenStax – Free Textbooks',
    desc:'Peer-reviewed, openly licensed textbooks for college and high school. Chemistry, Biology, Physics, Algebra, and more.',
    url:'https://openstax.org',
    domain:'openstax.org',
    subjects:['physics','chemistry','biology','math'],
  },
  {
    icon:'🌐', category:'textbooks', catLabel:'Reference', free:true,
    title:'Wikipedia – Encyclopedia',
    desc:'The world\'s largest encyclopedia. Excellent for reading about any science topic with references and further reading.',
    url:'https://en.wikipedia.org',
    domain:'en.wikipedia.org',
    subjects:['general'],
  },
  {
    icon:'📘', category:'textbooks', catLabel:'Textbooks', free:true, urdu:true,
    title:'PCTB – Punjab Textbook Board',
    desc:'Official digital textbooks from Punjab Curriculum and Textbook Board for Grades 9–12 in Urdu and English.',
    url:'https://www.pctb.punjab.gov.pk',
    domain:'pctb.punjab.gov.pk',
    subjects:['general'],
  },

  /* Practice & Tests */
  {
    icon:'✏️', category:'practice', catLabel:'Practice', free:true,
    title:'Khan Academy – Practice Exercises',
    desc:'Adaptive practice problems in Math and Science with step-by-step hints, aligned to international curricula.',
    url:'https://www.khanacademy.org/math',
    domain:'khanacademy.org',
    subjects:['math','physics','chemistry'],
  },
  {
    icon:'🧩', category:'practice', catLabel:'Practice', free:true,
    title:'Brilliant.org – Problem Solving',
    desc:'Interactive courses and daily challenges in Math, Science, and Computer Science built on active learning.',
    url:'https://brilliant.org',
    domain:'brilliant.org',
    subjects:['math','physics','logic'],
  },
  {
    icon:'🔢', category:'practice', catLabel:'Practice', free:true,
    title:'Desmos – Graphing Calculator',
    desc:'Free, powerful graphing calculator and classroom activities for algebra, geometry, statistics and calculus.',
    url:'https://www.desmos.com',
    domain:'desmos.com',
    subjects:['math'],
  },

  /* Reference */
  {
    icon:'⚗️', category:'reference', catLabel:'Reference', free:true,
    title:'Ptable – Interactive Periodic Table',
    desc:'The best interactive periodic table on the web. Click any element for detailed properties, isotopes, orbitals and more.',
    url:'https://ptable.com',
    domain:'ptable.com',
    subjects:['chemistry'],
  },
  {
    icon:'🧮', category:'reference', catLabel:'Reference', free:true,
    title:'Wolfram Alpha – Computation Engine',
    desc:'Compute answers to complex math, science, and general knowledge questions using AI-powered computation.',
    url:'https://www.wolframalpha.com',
    domain:'wolframalpha.com',
    subjects:['math','physics','chemistry'],
  },
  {
    icon:'📐', category:'reference', catLabel:'Reference', free:true,
    title:'GeoGebra – Geometry Reference',
    desc:'Construct geometric figures, explore proofs, and visualize algebra interactively. Used by students worldwide.',
    url:'https://www.geogebra.org/geometry',
    domain:'geogebra.org/geometry',
    subjects:['math'],
  },
  {
    icon:'🌡️', category:'reference', catLabel:'Reference', free:true,
    title:'Engineering ToolBox',
    desc:'Reference data, equations, and calculators for physics, chemistry, and engineering. Great for formulas.',
    url:'https://www.engineeringtoolbox.com',
    domain:'engineeringtoolbox.com',
    subjects:['physics','chemistry'],
  },

  /* Pakistan-specific */
  {
    icon:'🇵🇰', category:'pakistan', catLabel:'Pakistan', free:true, urdu:true,
    title:'Sabaq Foundation',
    desc:'Non-profit Pakistani educational platform offering free Urdu-medium video tutorials for matric and intermediate.',
    url:'https://sabaq.pk',
    domain:'sabaq.pk',
    subjects:['general'],
  },
  {
    icon:'📋', category:'pakistan', catLabel:'Pakistan', free:true,
    title:'BSEK – Balochistan Board',
    desc:'Official website of Board of Secondary Education Karachi with past papers, results, and board notifications.',
    url:'http://www.bsek.edu.pk',
    domain:'bsek.edu.pk',
    subjects:['general'],
  },
  {
    icon:'📑', category:'pakistan', catLabel:'Pakistan', free:true, urdu:true,
    title:'Ilmkidunya – Past Papers',
    desc:'Large collection of past papers, model papers, and guess papers for Matric and Intermediate across all Pakistani boards.',
    url:'https://www.ilmkidunya.com',
    domain:'ilmkidunya.com',
    subjects:['general'],
  },
  {
    icon:'🎯', category:'pakistan', catLabel:'Pakistan', free:true, urdu:true,
    title:'Top Study World',
    desc:'Free past papers, notes, and model papers for Federal Board, Punjab Board, and Balochistan Board students.',
    url:'https://www.topstudyworld.com',
    domain:'topstudyworld.com',
    subjects:['general'],
  },
]

const filteredResources = computed(() => {
  let list = resources
  if (activeCategory.value !== 'all') {
    list = list.filter(r => r.category === activeCategory.value)
  }
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(r =>
      r.title.toLowerCase().includes(q) ||
      r.desc.toLowerCase().includes(q) ||
      r.domain.toLowerCase().includes(q) ||
      r.subjects.some(s => s.includes(q))
    )
  }
  return list
})
</script>

<style scoped>
.gl-root { max-width: 960px; margin: 0 auto; padding: 1.5rem; }
.gl-hero { text-align: center; padding: 1.5rem 1rem 1rem; }
.gl-hero-icon { font-size: 3rem; margin-bottom: .5rem; }
.gl-hero-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.gl-hero-sub { font-size: .875rem; color: var(--t-text3); margin-top: .25rem; }

.gl-search-row { margin-bottom: 1rem; }
.gl-search-wrap { position: relative; display: flex; align-items: center; }
.gl-search-icon { position: absolute; left: .85rem; font-size: 1rem; pointer-events: none; }
.gl-search { width: 100%; padding: .75rem .75rem .75rem 2.5rem; border-radius: 12px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text1); font-size: .95rem; }
.gl-search:focus { outline: none; border-color: #00bcd4; }
.gl-search-clear { position: absolute; right: .85rem; background: none; border: none; color: var(--t-text3); font-size: 1rem; cursor: pointer; }

.gl-filters { display: flex; gap: .4rem; flex-wrap: wrap; margin-bottom: 1rem; }
.gl-filter-btn { padding: .4rem .85rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: .8rem; cursor: pointer; transition: all .15s; white-space: nowrap; }
.gl-filter-btn.active, .gl-filter-btn:hover { background: rgba(0,188,212,.1); color: #00bcd4; border-color: rgba(0,188,212,.3); }

.gl-count { font-size: .78rem; color: var(--t-text3); margin-bottom: .85rem; }

.gl-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1rem; }
.gl-card {
  border: 1px solid var(--t-border);
  border-radius: 16px;
  padding: 1.1rem;
  background: var(--t-surface);
  text-decoration: none;
  display: flex; flex-direction: column; gap: .6rem;
  transition: all .15s;
  cursor: pointer;
}
.gl-card:hover { border-color: #00bcd4; transform: translateY(-2px); box-shadow: 0 4px 16px rgba(0,188,212,.12); }
.gl-card-top { display: flex; align-items: flex-start; justify-content: space-between; gap: .5rem; }
.gl-card-icon { font-size: 1.75rem; flex-shrink: 0; }
.gl-card-tags { display: flex; flex-wrap: wrap; gap: .3rem; }
.gl-tag { font-size: .62rem; font-weight: 700; padding: .2rem .5rem; border-radius: 6px; }
.gl-cat-tag { background: rgba(0,188,212,.08); color: #00bcd4; border: 1px solid rgba(0,188,212,.2); }
.gl-free-tag { background: rgba(34,197,94,.08); color: #16a34a; border: 1px solid rgba(34,197,94,.2); }
.gl-urdu-tag { background: rgba(168,85,247,.08); color: #a855f7; border: 1px solid rgba(168,85,247,.2); font-family: 'Noto Nastaliq Urdu', serif; }
.gl-card-title { font-size: 1rem; font-weight: 800; color: var(--t-text1); line-height: 1.3; }
.gl-card-desc { font-size: .8rem; color: var(--t-text2); line-height: 1.55; flex: 1; }
.gl-card-footer { display: flex; align-items: center; justify-content: space-between; }
.gl-card-domain { font-size: .7rem; color: var(--t-text3); font-family: monospace; }
.gl-visit { font-size: .75rem; font-weight: 700; color: #00bcd4; }

.gl-empty { text-align: center; padding: 3rem 1rem; }
.gl-empty-icon { font-size: 3rem; margin-bottom: .75rem; }
.gl-empty p { color: var(--t-text2); margin-bottom: 1rem; }
.gl-clear-btn { padding: .55rem 1.25rem; border-radius: 10px; background: rgba(0,188,212,.1); color: #00bcd4; border: 1px solid rgba(0,188,212,.25); font-weight: 600; cursor: pointer; }

@media (max-width: 600px) {
  .gl-grid { grid-template-columns: 1fr; }
}
</style>
