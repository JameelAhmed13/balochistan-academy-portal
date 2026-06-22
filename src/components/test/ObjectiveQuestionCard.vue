<template>
  <div class="card p-5 space-y-3 animate-fade-in">
    <!-- Meta badges -->
    <div class="flex items-center gap-2 flex-wrap">
      <span class="text-xs font-semibold text-slate-500">#{{ index + 1 }}</span>
      <span :class="diffBadge">{{ question.difficulty }}</span>
      <span class="badge-indigo">{{ question.cognitiveLevel }}</span>
      <span class="badge-purple">{{ question.type }}</span>
      <span v-if="question.entranceExam" class="badge bg-red-100 text-red-700">Entrance</span>
      <span class="text-xs text-slate-400 ml-auto">{{ question.slo }}</span>
    </div>

    <!-- Stem -->
    <p class="text-slate-800 font-medium leading-relaxed">{{ question.stem }}</p>

    <!-- Options -->
    <div class="space-y-2">
      <div v-for="(opt, i) in question.options" :key="i"
        @click="!showAnswer && !testMode ? null : handleSelect(i)"
        :class="[
          'flex items-center gap-3 px-4 py-3 rounded-xl border text-sm transition-all duration-150',
          getOptionClass(i),
          (testMode && !submitted) ? 'cursor-pointer hover:border-brand-300 hover:bg-brand-50' : '',
        ]"
      >
        <span class="w-6 h-6 rounded-full border-2 flex items-center justify-center text-xs font-bold shrink-0"
          :class="getCircleClass(i)">{{ 'ABCD'[i] }}</span>
        <span>{{ opt }}</span>
        <Check v-if="showAnswer && i === question.correct" class="w-4 h-4 text-emerald-600 ml-auto shrink-0" />
        <X v-if="testMode && submitted && selected === i && i !== question.correct" class="w-4 h-4 text-red-500 ml-auto shrink-0" />
      </div>
    </div>

    <!-- Reason panel (study mode or after submit) -->
    <Transition name="fade-in">
      <div v-if="(showAnswer || (testMode && submitted)) && question.reason"
        class="p-3 rounded-xl bg-accent-50 border border-accent-100">
        <div class="text-xs font-semibold text-accent-700 mb-1">Explanation</div>
        <p class="text-sm text-accent-800">{{ question.reason }}</p>
      </div>
    </Transition>

    <!-- Test mode submit -->
    <div v-if="testMode && !submitted" class="pt-1">
      <button @click="submitAnswer" :disabled="selected === null" class="btn-primary text-xs">
        <CheckCircle class="w-3.5 h-3.5" /> Confirm Answer
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Check, X, CheckCircle } from '@lucide/vue'

const props = defineProps({
  question: Object,
  index: Number,
  showAnswer: { type: Boolean, default: false },
  testMode: { type: Boolean, default: false },
})
const emit = defineEmits(['answer'])

const selected = ref(null)
const submitted = ref(false)

function handleSelect(i) {
  if (!submitted.value) selected.value = i
}

function submitAnswer() {
  submitted.value = true
  emit('answer', { questionId: props.question.id, selected: selected.value, correct: props.question.correct })
}

function getOptionClass(i) {
  if (props.showAnswer && !props.testMode) {
    return i === props.question.correct
      ? 'border-emerald-300 bg-emerald-50 text-emerald-800'
      : 'border-slate-100 bg-white text-slate-600'
  }
  if (props.testMode) {
    if (!submitted.value) return selected.value === i ? 'border-brand-400 bg-brand-50' : 'border-slate-200 bg-white'
    if (i === props.question.correct) return 'border-emerald-300 bg-emerald-50'
    if (selected.value === i) return 'border-red-300 bg-red-50'
    return 'border-slate-100 bg-white opacity-60'
  }
  return 'border-slate-200 bg-white text-slate-700'
}

function getCircleClass(i) {
  if (props.showAnswer && !props.testMode) return i === props.question.correct ? 'border-emerald-400 text-emerald-600 bg-emerald-50' : 'border-slate-300 text-slate-500'
  if (selected.value === i) return 'border-brand-500 text-brand-600 bg-brand-50'
  return 'border-slate-300 text-slate-500'
}

const diffBadge = computed(() => ({
  Easy: 'badge-green', Medium: 'badge-amber', Hard: 'badge-red',
}[props.question.difficulty] || 'badge-indigo'))
</script>

