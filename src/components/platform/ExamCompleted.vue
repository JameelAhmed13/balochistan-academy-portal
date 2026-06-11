<template>
  <div class="ec-hero">
    <div class="ec-text">
      <div class="ec-title">🎉 EXAM COMPLETED!</div>
      <div class="ec-sub">{{ subtitle }}</div>
    </div>
    <div class="ec-ring">
      <svg viewBox="0 0 100 100" class="ec-ring-svg">
        <circle cx="50" cy="50" r="42" fill="none" stroke="rgba(255,255,255,0.2)" stroke-width="8"/>
        <circle cx="50" cy="50" r="42" fill="none" stroke="white" stroke-width="8"
          stroke-dasharray="263.9" :stroke-dashoffset="263.9 - (263.9 * Math.min(score,100) / 100)"
          stroke-linecap="round" transform="rotate(-90 50 50)" style="transition: stroke-dashoffset 1s ease"/>
      </svg>
      <div class="ec-score">
        <div class="ec-score-num">{{ score }}</div>
        <div class="ec-score-lbl">{{ label }}</div>
      </div>
    </div>
  </div>
</template>
<script setup>
defineProps({
  score: { type: Number, default: 0 },
  label: { type: String, default: 'OBJECTIVE SCORE' },
  subtitle: { type: String, default: 'You have successfully submitted your test.' },
})
</script>
<style scoped>
.ec-hero {
  background: linear-gradient(135deg, #1b5e20, #2e7d32, #388e3c);
  border-radius: 14px; padding: 2rem 2rem 2.5rem; margin-bottom: 1.5rem;
  display: flex; flex-direction: column; align-items: center; gap: 1.25rem;
  text-align: center; color: white;
}
.ec-title { font-size: 1.5rem; font-weight: 800; letter-spacing: 0.04em; }
.ec-sub { font-size: 0.875rem; opacity: 0.85; }
.ec-ring { position: relative; width: 90px; height: 90px; }
.ec-ring-svg { width: 90px; height: 90px; }
.ec-score {
  position: absolute; inset: 0; display: flex; flex-direction: column;
  align-items: center; justify-content: center; gap: 0.1rem;
}
.ec-score-num { font-size: 1.6rem; font-weight: 800; color: white; line-height: 1; }
.ec-score-lbl { font-size: 0.52rem; font-weight: 700; color: rgba(255,255,255,0.7); letter-spacing: 0.06em; text-align: center; }
</style>
