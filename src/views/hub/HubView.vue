<template>
  <div class="hub-page">
    <!-- Galaxy background -->
    <div class="hub-galaxy">
      <div class="hub-glow hub-g1"/>
      <div class="hub-glow hub-g2"/>
      <div class="hub-glow hub-g3"/>
      <div class="hub-stars">
        <span v-for="n in 60" :key="n" class="hub-star" :style="starStyle(n)"/>
      </div>
    </div>

    <!-- Tiles orbit layout -->
    <div class="hub-tiles">
      <RouterLink
        v-for="(tile, i) in tiles"
        :key="tile.id"
        :to="tile.path"
        class="hub-tile"
        :style="tileStyle(i)"
        :aria-label="tile.title + ' — ' + tile.sub"
      >
        <span class="hub-tile-icon">{{ tile.icon }}</span>
        <div class="hub-tile-body">
          <div class="hub-tile-name">{{ tile.title }}</div>
          <div class="hub-tile-sub">{{ tile.sub }}</div>
        </div>
      </RouterLink>
    </div>

    <!-- Central CTA -->
    <div class="hub-cta-wrap">
      <h1 class="hub-headline">Start Your Learning Journey</h1>
      <p class="hub-tagline">Pakistan's most advanced AI-powered board exam prep platform</p>
      <RouterLink to="/app" class="hub-cta-btn">
        <span>Explore Dashboard</span>
        <span class="hub-cta-arrow">→</span>
      </RouterLink>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import PageFooter from '@/components/platform/PageFooter.vue'

const tiles = [
  { id:'video',     title:'Video Lectures',      sub:'Watch & learn',     icon:'🎬', path:'/app/video' },
  { id:'ebooks',    title:'E-Books',              sub:'PTB textbooks',     icon:'📚', path:'/app/ebooks' },
  { id:'chtest',    title:'Chapter Tests',        sub:'Unit-wise MCQs',    icon:'📖', path:'/app/preparation' },
  { id:'obj',       title:'Objective (NPO)',      sub:'MCQ bank',          icon:'🎯', path:'/app/objective-tests' },
  { id:'subj',      title:'Subjective',           sub:'Long & short Qs',   icon:'✍️', path:'/app/self-test' },
  { id:'self',      title:'Self Assessments',     sub:'Self-test tools',   icon:'📝', path:'/app/self-test' },
  { id:'mcqs',      title:'MCQs Bank',            sub:'1200+ questions',   icon:'🗃️', path:'/app/mcqs-bank' },
  { id:'past',      title:'Past Papers',          sub:'Previous years',    icon:'🗒️', path:'/app/past-papers' },
  { id:'sim',       title:'Simulations',          sub:'Interactive labs',  icon:'🔬', path:'/app/simulations' },
  { id:'slo',       title:'SLOs Mapping',         sub:'Outcome maps',      icon:'🗺️', path:'/app/slos-mapping' },
  { id:'mdcat',     title:'MDCAT Prep',           sub:'Entry test prep',   icon:'🏥', path:'/app/mdcat' },
  { id:'etea',      title:'ETEA Prep',            sub:'Engineering entry', icon:'⚙️', path:'/app/etea' },
  { id:'gk',        title:'General Knowledge',    sub:'Pakistan & world',  icon:'🌐', path:'/app/general-knowledge' },
  { id:'concept',   title:'Conceptual',           sub:'Deep concepts',     icon:'💡', path:'/app/conceptual' },
  { id:'exp',       title:'Experiments',          sub:'Lab practicals',    icon:'🧪', path:'/app/experiments' },
  { id:'calc',      title:'Calculator',           sub:'Scientific calc',   icon:'🔢', path:'/app/calculator' },
  { id:'periodic',  title:'Periodic Table',       sub:'All 118 elements',  icon:'⚗️', path:'/app/periodic-table' },
  { id:'log',       title:'Log Table',            sub:'Math tables',       icon:'📊', path:'/app/log-table' },
  { id:'dict',      title:'Dictionary',           sub:'Urdu & English',    icon:'📖', path:'/app/dictionary' },
  { id:'progress',  title:'Progress Tracking',    sub:'Your analytics',    icon:'📈', path:'/app/progress-tracking' },
  { id:'tt',        title:'Time Table',           sub:'Schedule planner',  icon:'🗓️', path:'/app/time-table' },
  { id:'notebook',  title:'Study Notebook',      sub:'AI-powered notes',  icon:'📓', path:'/app/study-notebook' },
  { id:'notes',     title:'Smart Notes',          sub:'Auto-summarize',    icon:'🧠', path:'/app/smart-notes' },
  { id:'fullprep',  title:'Full Prep',            sub:'Complete bundle',   icon:'🎓', path:'/app/full-prep' },
  { id:'library',   title:'Global Library',       sub:'World resources',   icon:'🏛️', path:'/app/global-library' },
  { id:'coding',    title:'Learn Coding',         sub:'Python, HTML...',   icon:'💻', path:'/app/learn-coding' },
  { id:'puzzle',    title:'Puzzle Games',         sub:'Learn with fun',    icon:'🧩', path:'/app/puzzle-games' },
  { id:'online',    title:'Online Classes',       sub:'Live lectures',     icon:'📡', path:'/app/online-classes' },
  { id:'career',    title:'Career Counselor',     sub:'Guidance & tips',   icon:'🧭', path:'/app/career-counselor' },
  { id:'forces',    title:'Join Forces',          sub:'Study groups',      icon:'🤝', path:'/app/join-forces' },
  { id:'typing',    title:'Typing Master',        sub:'Speed & accuracy',  icon:'⌨️', path:'/app/typing-master' },
  { id:'msg',       title:'Messages',             sub:'Inbox & chat',      icon:'💬', path:'/app/messages' },
  { id:'saathi',    title:'Saathi AI',            sub:'Your study companion', icon:'🤝', path:'/app/saathi' },
  { id:'tutor',     title:'AI Tutor',             sub:'Learn from legends', icon:'🧑‍🏫', path:'/app/ai-tutor' },
  { id:'dash',      title:'Dashboard',            sub:'Your overview',     icon:'🏠', path:'/app' },
]

const COLS = 7
const ROWS = 5

function tileStyle(i) {
  const col = i % COLS
  const row = Math.floor(i / COLS)
  const x = 4 + col * 13.5 + (row % 2) * 6.5
  const y = 6 + row * 16 + (col % 2) * 3
  const delay = (i * 0.04).toFixed(2)
  return {
    left: x + '%',
    top: y + '%',
    animationDelay: delay + 's',
  }
}

function starStyle(n) {
  const seed = n * 137.508
  const x = (seed % 100).toFixed(1)
  const y = ((seed * 1.618) % 100).toFixed(1)
  const size = (1 + (n % 3)) + 'px'
  const delay = (n * 0.18 % 4).toFixed(2)
  return { left: x + '%', top: y + '%', width: size, height: size, animationDelay: delay + 's' }
}
</script>

<style scoped>
.hub-page {
  position: relative; min-height: 100vh;
  background: radial-gradient(ellipse at 50% 40%, #0a1628, #050c18 70%, #020810);
  overflow: hidden; display: flex; flex-direction: column; align-items: center; justify-content: center;
  margin: -1.5rem; padding: 2rem 1rem;
}

.hub-galaxy { position: absolute; inset: 0; pointer-events: none; }
.hub-glow { position: absolute; border-radius: 50%; filter: blur(80px); }
.hub-g1 { width: 400px; height: 400px; top: -100px; left: -100px; background: radial-gradient(circle, rgba(124,106,245,0.25), transparent 60%); }
.hub-g2 { width: 350px; height: 350px; bottom: -80px; right: -80px; background: radial-gradient(circle, rgba(6,182,212,0.18), transparent 60%); }
.hub-g3 { width: 500px; height: 500px; top: 50%; left: 50%; transform: translate(-50%,-50%); background: radial-gradient(circle, rgba(109,84,232,0.12), transparent 60%); }

.hub-stars { position: absolute; inset: 0; }
.hub-star {
  position: absolute; border-radius: 50%; background: rgba(255,255,255,0.8);
  animation: starTwinkle 3s ease-in-out infinite;
}
@keyframes starTwinkle { 0%,100%{opacity:0.2;transform:scale(1)} 50%{opacity:1;transform:scale(1.5)} }

/* Tiles */
.hub-tiles { position: absolute; inset: 0; pointer-events: none; }
.hub-tile {
  position: absolute; pointer-events: all;
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.55rem 0.9rem; border-radius: 14px;
  background: rgba(15,22,45,0.85); backdrop-filter: blur(12px);
  border: 1px solid rgba(124,106,245,0.2);
  color: rgba(255,255,255,0.85); text-decoration: none;
  box-shadow: 0 4px 20px rgba(0,0,0,0.4), 0 0 0 1px rgba(124,106,245,0.08);
  transition: all 0.25s cubic-bezier(0.23,1,0.32,1);
  animation: tileFloat 6s ease-in-out infinite;
  white-space: nowrap;
}
.hub-tile:hover {
  border-color: rgba(124,106,245,0.6);
  background: rgba(25,32,65,0.95);
  box-shadow: 0 0 20px rgba(124,106,245,0.3), 0 8px 32px rgba(0,0,0,0.5);
  transform: translateY(-3px) scale(1.04);
}
@keyframes tileFloat { 0%,100%{transform:translateY(0)} 50%{transform:translateY(-6px)} }
.hub-tile-icon { font-size: 1rem; flex-shrink: 0; }
.hub-tile-body { min-width: 0; }
.hub-tile-name { font-size: 0.72rem; font-weight: 700; color: rgba(255,255,255,0.9); white-space: nowrap; }
.hub-tile-sub { font-size: 0.6rem; color: rgba(124,106,245,0.7); white-space: nowrap; }

/* Central CTA */
.hub-cta-wrap {
  position: relative; z-index: 10; text-align: center;
  display: flex; flex-direction: column; align-items: center; gap: 1rem;
  background: rgba(5,12,24,0.6); backdrop-filter: blur(20px);
  border: 1px solid rgba(124,106,245,0.25); border-radius: 24px;
  padding: 2rem 3rem; max-width: 480px;
  box-shadow: 0 0 60px rgba(124,106,245,0.15), 0 20px 60px rgba(0,0,0,0.6);
}
.hub-headline {
  font-size: 1.75rem; font-weight: 800; color: white; margin: 0;
  font-family: 'Syne', sans-serif; line-height: 1.2;
}
.hub-tagline { font-size: 0.875rem; color: rgba(255,255,255,0.55); margin: 0; }
.hub-cta-btn {
  display: inline-flex; align-items: center; gap: 0.75rem;
  padding: 0.85rem 2rem; border-radius: 99px; font-weight: 700; font-size: 0.95rem;
  background: linear-gradient(135deg, #06b6d4, #3b82f6);
  color: white; text-decoration: none;
  box-shadow: 0 0 30px rgba(6,182,212,0.4);
  transition: all 0.25s;
}
.hub-cta-btn:hover { box-shadow: 0 0 50px rgba(6,182,212,0.6); transform: scale(1.03); }
.hub-cta-btn:focus-visible { outline: 2px solid white; outline-offset: 3px; }
.hub-tile:focus-visible { outline: 2px solid white; outline-offset: 3px; border-radius: 10px; }
.hub-cta-arrow { font-size: 1.2rem; }

/* Reduced motion — keep layout but stop animations */
@media (prefers-reduced-motion: reduce) {
  .hub-star { animation: none; opacity: 0.5; }
  .hub-tile { animation: none; }
  .hub-glow { animation: none; }
}
</style>
