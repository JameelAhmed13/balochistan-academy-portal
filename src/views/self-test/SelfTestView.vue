<template>
  <div class="animate-fade-in space-y-6">
    <div class="page-header">
      <h2 class="section-title">Student Self Test</h2>
      <div class="flex gap-2">
        <button v-for="m in ['All','English','Urdu']" :key="m"
          @click="mediumFilter = m"
          :class="['btn-secondary text-xs', mediumFilter === m ? 'bg-brand-600 text-white border-brand-600' : '']">
          {{ m }}
        </button>
      </div>
    </div>

    <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
      <button v-for="subj in filteredSubjects" :key="subj.id"
        type="button"
        @click="selectSubject(subj)"
        class="card-hover group p-4 flex flex-col items-center text-center gap-3"
        :aria-label="subj.name + ' — ' + subj.nameUr"
      >
        <div class="w-16 h-20 rounded-xl flex items-center justify-center text-4xl shadow-sm transition-transform group-hover:scale-105"
          :class="subj.color">
          {{ subj.icon }}
        </div>
        <div>
          <div class="font-semibold text-sm" style="color:var(--t-text1)">{{ subj.name }}</div>
          <div class="text-xs mt-0.5" :class="subj.medium === 'urdu' ? 'urdu' : ''" style="color:var(--t-text3)">{{ subj.nameUr }}</div>
        </div>
        <div class="flex gap-1 flex-wrap justify-center">
          <span class="badge-blue">Grade 9</span>
          <span class="badge-purple">Balochistan</span>
        </div>
      </button>
    </div>

    <!-- Type selection modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showModal" class="modal-overlay" @click.self="showModal = false" @keydown.escape="showModal = false">
          <div class="modal-box" role="dialog" aria-modal="true" aria-labelledby="modal-heading">
            <button type="button" @click="showModal = false" class="modal-close" aria-label="Close test type selector">✕</button>
            <p id="modal-heading" class="modal-heading">Choose Test Type</p>
            <div class="space-y-3 pt-2">
              <button type="button" @click="goTest('general')" class="type-btn type-green">
                <span class="type-icon" aria-hidden="true">🎯</span> GENERAL SELF TEST
              </button>
              <button type="button" @click="goTest('past')" class="type-btn type-blue">
                <span class="type-icon" aria-hidden="true">📄</span> TEST YOURSELF IN PAST PAPER
              </button>
              <button type="button" @click="goTest('predicted')" class="type-btn type-cyan">
                <span class="type-icon" aria-hidden="true">🤖</span> TEST YOURSELF IN PREDICTED EXAM
              </button>
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
import { useRouter } from 'vue-router'
import { SUBJECTS } from '@/stores/content'
import PageFooter from '@/components/platform/PageFooter.vue'

const router = useRouter()
const mediumFilter = ref('All')
const showModal = ref(false)
const selectedSubject = ref(null)

const filteredSubjects = computed(() => {
  if (mediumFilter.value === 'All') return SUBJECTS
  return SUBJECTS.filter(s => s.medium === mediumFilter.value.toLowerCase())
})

function selectSubject(subj) {
  selectedSubject.value = subj
  showModal.value = true
}

function goTest(type) {
  showModal.value = false
  router.push(`/app/self-test/${selectedSubject.value.id}/config?type=${type}`)
}
</script>

<style scoped>
.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,0.65);
  display: flex; align-items: center; justify-content: center; z-index: 100;
}
.modal-box {
  background: #141b2d; border-radius: 16px; padding: 2rem 1.75rem;
  width: 340px; max-width: 95vw; position: relative;
  border: 1px solid rgba(124,106,245,0.2);
  box-shadow: 0 20px 60px rgba(0,0,0,0.6);
}
.modal-heading {
  font-size: 0.8rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase;
  color: rgba(255,255,255,0.45); margin: 0 0 0.25rem; padding-right: 2rem;
}
.modal-close {
  position: absolute; top: 0.85rem; right: 1rem;
  color: rgba(255,255,255,0.4); font-size: 1rem; background: none; border: none;
  cursor: pointer; transition: color 0.15s; width: 28px; height: 28px;
  display: flex; align-items: center; justify-content: center; border-radius: 6px;
}
.modal-close:hover { color: white; background: rgba(255,255,255,0.1); }
.modal-close:focus-visible { outline: 2px solid rgba(124,106,245,0.7); outline-offset: 2px; }
.type-btn {
  width: 100%; padding: 1rem 1.25rem; border-radius: 12px; border: none;
  display: flex; align-items: center; gap: 0.85rem; font-size: 0.9rem; font-weight: 700;
  cursor: pointer; transition: opacity 0.15s; color: white;
}
.type-btn:hover { opacity: 0.85; }
.type-icon { font-size: 1.2rem; }
.type-green { background: linear-gradient(135deg, #2ecc71, #27ae60); }
.type-blue  { background: linear-gradient(135deg, #3498db, #2980b9); }
.type-cyan  { background: linear-gradient(135deg, #00bcd4, #0097a7); }
.modal-enter-active, .modal-leave-active { transition: opacity 0.2s; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.modal-enter-active .modal-box, .modal-leave-active .modal-box { transition: transform 0.25s; }
.modal-enter-from .modal-box { transform: scale(0.92) translateY(20px); }
</style>
