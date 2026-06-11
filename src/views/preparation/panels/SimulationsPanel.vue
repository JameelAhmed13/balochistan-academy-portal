<template>
  <div class="sp">
    <!-- Header -->
    <div class="sp-head">
      <span class="sp-head-icon" :class="subject?.color || 'grad-emerald'">{{ subject?.icon || '🔬' }}</span>
      <div class="sp-head-info">
        <div class="sp-eyebrow">Interactive · Explore</div>
        <h1 class="sp-title">{{ subject?.name || 'Simulations' }}</h1>
        <p class="sp-sub">Live PhET simulations — University of Colorado Boulder.</p>
      </div>
    </div>

    <!-- ── Grid ── -->
    <template v-if="!active">
      <div class="sp-toolbar">
        <span class="sp-scope-note">
          <FlaskConical class="w-3.5 h-3.5" />
          <template v-if="scoped">{{ sims.length }} {{ subject?.name }} simulation{{ sims.length === 1 ? '' : 's' }}</template>
          <template v-else>Interactive labs aren't subject-specific for {{ subject?.name }} — explore all science simulations</template>
        </span>
      </div>

      <div class="sp-grid">
        <button v-for="s in sims" :key="s.id" type="button" class="sp-card" @click="open(s)">
          <span class="sp-card-icon">{{ s.icon }}</span>
          <span class="sp-card-title">{{ s.title }}</span>
          <span class="sp-card-desc">{{ s.desc }}</span>
          <span class="sp-card-foot">
            <span class="sp-chip">{{ s.subject }}</span>
            <span class="sp-go">Launch →</span>
          </span>
        </button>
      </div>
    </template>

    <!-- ── Detail ── -->
    <template v-else>
      <button type="button" class="sp-back" @click="active = null">
        <ArrowLeft class="w-3.5 h-3.5" /> All simulations
      </button>

      <div class="sp-head" style="margin-bottom:0.85rem">
        <span class="sp-head-icon grad-emerald">{{ active.icon }}</span>
        <div class="sp-head-info">
          <div class="sp-eyebrow">{{ active.subject }} · {{ active.grade }}</div>
          <h2 class="sp-title">{{ active.title }}</h2>
        </div>
      </div>

      <!-- Inline PhET iframe -->
      <div class="sp-iframe-wrap">
        <div v-if="!loaded" class="sp-iframe-loading">
          <span class="sp-load-dot" /><span class="sp-load-dot" /><span class="sp-load-dot" />
          <span>Loading simulation…</span>
        </div>
        <iframe
          :src="active.phetUrl"
          :class="{ 'is-ready': loaded }"
          allowfullscreen allow="fullscreen"
          :title="active.title + ' — PhET Interactive Simulation'"
          @load="loaded = true" />
      </div>
      <div class="sim-credit">
        <a :href="active.phetUrl" target="_blank" rel="noopener" class="sp-go">Open full screen ↗</a>
        <span>PhET Interactive Simulations · University of Colorado Boulder</span>
      </div>

      <!-- Theory steps -->
      <div class="sp-section" v-if="active.steps?.length">
        <div class="sp-section-title">📚 Theory &amp; Steps</div>
        <div v-for="(st, i) in active.steps" :key="i" class="sim-step">
          <div class="sim-step-num">{{ i + 1 }}</div>
          <div>
            <div class="sim-step-heading">{{ st.heading }}</div>
            <div class="sp-prose">{{ st.text }}</div>
            <div v-if="st.formula" class="sp-formula">{{ st.formula }}</div>
          </div>
        </div>
      </div>

      <!-- Variables -->
      <div class="sp-section" v-if="active.variables?.length">
        <div class="sp-section-title">🔧 Key Variables</div>
        <div class="sim-vars">
          <div v-for="v in active.variables" :key="v.symbol" class="sim-var">
            <div class="sim-var-sym">{{ v.symbol }}</div>
            <div class="sim-var-name">{{ v.name }}</div>
            <div class="sim-var-unit">{{ v.unit }}</div>
          </div>
        </div>
      </div>

      <!-- Board questions -->
      <div class="sp-section" v-if="active.examQuestions?.length">
        <div class="sp-section-title">📝 Board Exam Questions</div>
        <div v-for="(q, i) in active.examQuestions" :key="i" class="sim-exq">
          <span class="sim-exq-num">Q{{ i + 1 }}.</span> {{ q }}
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeft, FlaskConical } from '@lucide/vue'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { simulations } from '@/assets/data/phet'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()

const subject = computed(() => findPrepSubject(props.bookId ?? route.params.bookId))
const scoped = computed(() => simulations.some((s) => s.subject === subject.value?.name))
const sims = computed(() =>
  scoped.value ? simulations.filter((s) => s.subject === subject.value?.name) : simulations,
)

const active = ref(null)
const loaded = ref(false)
function open(s) { active.value = s; loaded.value = false }
</script>

<style scoped>
.sim-credit { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; font-size: 0.68rem; color: var(--t-text3); margin-bottom: 1.25rem; }
.sim-step { display: flex; gap: 0.85rem; margin-bottom: 0.9rem; }
.sim-step-num { width: 26px; height: 26px; border-radius: 50%; flex-shrink: 0; display: flex; align-items: center; justify-content: center; font-size: 0.78rem; font-weight: 800; background: color-mix(in srgb, var(--t-accent) 12%, transparent); color: var(--t-accent); }
.sim-step-heading { font-size: 0.88rem; font-weight: 700; color: var(--t-text1); margin-bottom: 0.2rem; }
.sim-vars { display: grid; grid-template-columns: repeat(auto-fill, minmax(130px, 1fr)); gap: 0.5rem; }
.sim-var { padding: 0.7rem; border: 1px solid var(--t-border); border-radius: 12px; text-align: center; background: var(--t-surface); }
.sim-var-sym { font-size: 1.35rem; font-weight: 800; color: var(--t-text1); font-family: 'JetBrains Mono', serif; }
.sim-var-name { font-size: 0.72rem; color: var(--t-text2); }
.sim-var-unit { font-size: 0.66rem; color: var(--t-text3); }
.sim-exq { display: flex; gap: 0.5rem; padding: 0.5rem 0; border-bottom: 1px solid var(--t-border); font-size: 0.84rem; color: var(--t-text1); line-height: 1.5; }
.sim-exq:last-child { border-bottom: none; }
.sim-exq-num { color: var(--t-accent); font-weight: 700; flex-shrink: 0; }
</style>
