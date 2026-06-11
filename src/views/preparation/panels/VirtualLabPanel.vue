<template>
  <div class="sp">
    <!-- Header -->
    <div class="sp-head">
      <span class="sp-head-icon" :class="subject?.color || 'grad-rose'">{{ subject?.icon || '⚗️' }}</span>
      <div class="sp-head-info">
        <div class="sp-eyebrow">Hands-on · Practice</div>
        <h1 class="sp-title">{{ subject?.name || 'Virtual Lab' }}</h1>
        <p class="sp-sub">Guided experiments — read the method, then run the lab right here.</p>
      </div>
    </div>

    <!-- ── Grid ── -->
    <template v-if="!active">
      <div class="sp-toolbar">
        <span class="sp-scope-note">
          <TestTube2 class="w-3.5 h-3.5" />
          <template v-if="scoped">{{ exps.length }} {{ subject?.name }} experiment{{ exps.length === 1 ? '' : 's' }}</template>
          <template v-else>No {{ subject?.name }}-specific lab — explore all guided science experiments</template>
        </span>
      </div>

      <div class="sp-grid">
        <button v-for="e in exps" :key="e.id" type="button" class="sp-card" @click="open(e)">
          <span class="sp-card-icon">{{ e.icon }}</span>
          <span class="sp-card-title">{{ e.title }}</span>
          <span class="sp-card-desc">{{ e.desc }}</span>
          <span class="sp-card-foot">
            <span class="sp-chip">{{ e.subject }}</span>
            <span class="sp-go">Open lab →</span>
          </span>
        </button>
      </div>
    </template>

    <!-- ── Detail ── -->
    <template v-else>
      <button type="button" class="sp-back" @click="close">
        <ArrowLeft class="w-3.5 h-3.5" /> All experiments
      </button>

      <div class="sp-head" style="margin-bottom:0.85rem">
        <span class="sp-head-icon grad-rose">{{ active.icon }}</span>
        <div class="sp-head-info">
          <div class="sp-eyebrow">{{ active.subject }}</div>
          <h2 class="sp-title">{{ active.title }}</h2>
        </div>
      </div>

      <!-- Launch / inline lab -->
      <div v-if="!labOpen" class="vl-launch-row">
        <button type="button" class="btn-primary" @click="labOpen = true; labLoaded = false">
          <TestTube2 class="w-4 h-4" /> Launch Virtual Lab
        </button>
        <a :href="active.labUrl" target="_blank" rel="noopener" class="sp-pill">Open in new tab ↗</a>
      </div>
      <div v-else>
        <div class="sp-iframe-wrap">
          <div v-if="!labLoaded" class="sp-iframe-loading">
            <span class="sp-load-dot" /><span class="sp-load-dot" /><span class="sp-load-dot" />
            <span>Loading lab…</span>
          </div>
          <iframe
            :src="active.labUrl"
            :class="{ 'is-ready': labLoaded }"
            allowfullscreen allow="fullscreen"
            :title="active.title"
            @load="labLoaded = true" />
        </div>
        <div class="sim-credit">
          <button type="button" class="sp-go vl-collapse" @click="labOpen = false">Hide lab</button>
          <span>{{ active.provider }}</span>
        </div>
      </div>

      <!-- Method -->
      <div class="sp-section">
        <div class="sp-section-title">🎯 Objective</div>
        <p class="sp-prose">{{ active.objective }}</p>
      </div>
      <div class="sp-section">
        <div class="sp-section-title">🧰 Materials</div>
        <div class="vl-mats">
          <span v-for="m in active.materials" :key="m" class="vl-mat">{{ m }}</span>
        </div>
      </div>
      <div class="sp-section">
        <div class="sp-section-title">📋 Procedure</div>
        <div v-for="(p, i) in active.procedure" :key="i" class="vl-step">
          <span class="vl-step-num">{{ i + 1 }}</span>
          <span class="sp-prose">{{ p }}</span>
        </div>
      </div>
      <div class="sp-section">
        <div class="sp-section-title">📊 Expected Result</div>
        <p class="sp-prose vl-result">{{ active.result }}</p>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeft, TestTube2 } from '@lucide/vue'
import { findPrepSubject } from '@/views/preparation/prepSubjects'
import { experiments } from '@/assets/data/phet'

const props = defineProps({ bookId: { type: [String, Number], default: null } })
const route = useRoute()

const subject = computed(() => findPrepSubject(props.bookId ?? route.params.bookId))
const scoped = computed(() => experiments.some((e) => e.subject === subject.value?.name))
const exps = computed(() =>
  scoped.value ? experiments.filter((e) => e.subject === subject.value?.name) : experiments,
)

const active = ref(null)
const labOpen = ref(false)
const labLoaded = ref(false)
function open(e) { active.value = e; labOpen.value = false; labLoaded.value = false }
function close() { active.value = null; labOpen.value = false }
</script>

<style scoped>
.vl-launch-row { display: flex; gap: 0.6rem; align-items: center; flex-wrap: wrap; margin-bottom: 1.25rem; }
.sim-credit { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; font-size: 0.68rem; color: var(--t-text3); margin-bottom: 1.25rem; }
.vl-collapse { background: none; border: none; cursor: pointer; }
.vl-mats { display: flex; flex-wrap: wrap; gap: 0.4rem; }
.vl-mat { font-size: 0.74rem; padding: 0.25rem 0.6rem; border-radius: 8px; background: var(--t-hover); color: var(--t-text2); border: 1px solid var(--t-border); }
.vl-step { display: flex; gap: 0.7rem; align-items: flex-start; padding: 0.3rem 0; }
.vl-step-num { width: 22px; height: 22px; border-radius: 50%; flex-shrink: 0; display: flex; align-items: center; justify-content: center; font-size: 0.72rem; font-weight: 800; background: color-mix(in srgb, var(--t-accent) 12%, transparent); color: var(--t-accent); margin-top: 0.1rem; }
.vl-result { background: color-mix(in srgb, var(--t-accent) 7%, transparent); border-left: 3px solid var(--t-accent); padding: 0.7rem 0.85rem; border-radius: 0 8px 8px 0; }
</style>
