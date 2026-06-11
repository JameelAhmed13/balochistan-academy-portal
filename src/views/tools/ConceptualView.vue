<template>
  <div class="animate-fade-in space-y-5">
    <h2 class="section-title">Conceptual Learning</h2>
    <div class="con-filters">
      <select v-model="selectedSubject" class="con-sel">
        <option :value="null">Select Subject</option>
        <option v-for="s in SUBJECTS" :key="s.id" :value="s">{{ s.icon }} {{ s.name }}</option>
      </select>
    </div>
    <div v-if="selectedSubject" class="con-concepts">
      <div v-for="(concept, i) in concepts" :key="i" class="con-card">
        <div class="con-card-header" @click="concept.open = !concept.open">
          <div class="con-card-title">{{ concept.title }}</div>
          <div class="con-card-arrow" :class="{ 'con-open': concept.open }">▾</div>
        </div>
        <div v-if="concept.open" class="con-card-body">
          <div class="con-body-text">{{ concept.explanation }}</div>
          <div class="con-formula" v-if="concept.formula">
            <span class="con-formula-label">Key Formula:</span> {{ concept.formula }}
          </div>
          <div class="con-example" v-if="concept.example">
            <span class="con-example-label">Example:</span> {{ concept.example }}
          </div>
        </div>
      </div>
    </div>
    <div v-else class="con-empty">Select a subject to explore conceptual explanations.</div>
    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed, reactive } from 'vue'
import { SUBJECTS } from '@/stores/content'
import PageFooter from '@/components/platform/PageFooter.vue'

const selectedSubject = ref(null)

const conceptData = {
  Physics: [
    { title: 'Newton\'s First Law of Motion', explanation: 'An object at rest remains at rest, and an object in motion continues in motion at constant velocity, unless acted upon by a net external force.', formula: 'ΣF = 0 → constant velocity', example: 'A book lying on a table stays still unless pushed.' },
    { title: 'Newton\'s Second Law of Motion', explanation: 'The acceleration of an object is directly proportional to the net force acting on it and inversely proportional to its mass.', formula: 'F = ma', example: 'A 10 N force on 2 kg mass gives 5 m/s² acceleration.' },
    { title: 'Law of Conservation of Energy', explanation: 'Energy cannot be created or destroyed; it can only be transformed from one form to another. The total energy of an isolated system remains constant.', formula: 'KE + PE = constant', example: 'A pendulum swings: PE converts to KE at bottom.' },
  ],
  Chemistry: [
    { title: 'Atomic Structure', explanation: 'Atoms consist of a nucleus (protons and neutrons) surrounded by electrons in shells. The number of protons determines the element.', formula: 'Mass Number = Protons + Neutrons', example: 'Carbon has 6 protons, 6 electrons, and typically 6 neutrons.' },
    { title: 'Chemical Bonding', explanation: 'Atoms form bonds to achieve stable electron configurations. Ionic bonds transfer electrons, covalent bonds share electrons.', formula: 'Electronegativity difference > 1.7 → Ionic bond', example: 'NaCl: Sodium gives electron to Chlorine (ionic bond).' },
  ],
  Biology: [
    { title: 'Cell Theory', explanation: 'All living things are composed of cells. The cell is the basic unit of life. All cells arise from pre-existing cells.', formula: 'N/A', example: 'Bacteria are single-celled; humans have ~37 trillion cells.' },
    { title: 'DNA and Genetics', explanation: 'DNA carries genetic information in sequences of nucleotides (A, T, G, C). Genes are segments of DNA that code for proteins.', formula: 'A-T, G-C base pairing', example: 'Eye color is determined by genes inherited from both parents.' },
  ],
  Mathematics: [
    { title: 'Quadratic Formula', explanation: 'Used to find roots of a quadratic equation ax² + bx + c = 0. Derived by completing the square.', formula: 'x = (-b ± √(b²-4ac)) / 2a', example: 'For x² - 5x + 6 = 0: roots are x = 2 and x = 3.' },
    { title: 'Pythagoras Theorem', explanation: 'In a right-angled triangle, the square of the hypotenuse equals the sum of squares of the other two sides.', formula: 'a² + b² = c²', example: 'A 3-4-5 right triangle: 3² + 4² = 9 + 16 = 25 = 5².' },
  ],
}

const concepts = computed(() => {
  if (!selectedSubject.value) return []
  const data = conceptData[selectedSubject.value.name] || [
    { title: 'Core Concepts', explanation: `Study the fundamental principles of ${selectedSubject.value.name} through your textbook chapters and practice questions.`, formula: null, example: null },
  ]
  return data.map(d => reactive({ ...d, open: false }))
})
</script>

<style scoped>
.con-filters { display: flex; gap: 0.5rem; }
.con-sel { padding: 0.5rem 0.75rem; border: 1px solid var(--t-border); border-radius: 8px; background: var(--t-surface); color: var(--t-text1); font-size: 0.875rem; }
.con-concepts { display: flex; flex-direction: column; gap: 0.5rem; }
.con-card { background: var(--t-surface); border: 1px solid var(--t-border); border-radius: 10px; overflow: hidden; }
.con-card-header { display: flex; justify-content: space-between; align-items: center; padding: 0.9rem 1.25rem; cursor: pointer; }
.con-card-header:hover { background: var(--t-hover); }
.con-card-title { font-size: 0.875rem; font-weight: 700; color: var(--t-text1); }
.con-card-arrow { color: var(--t-text3); transition: transform 0.2s; }
.con-open { transform: rotate(180deg); }
.con-card-body { padding: 0 1.25rem 1.25rem; border-top: 1px solid var(--t-border); }
.con-body-text { font-size: 0.875rem; color: var(--t-text2); line-height: 1.6; margin-top: 0.75rem; }
.con-formula { margin-top: 0.5rem; font-size: 0.82rem; color: var(--t-text1); background: var(--t-bg); padding: 0.5rem 0.75rem; border-radius: 6px; }
.con-formula-label, .con-example-label { font-weight: 700; color: #4caf50; }
.con-example { margin-top: 0.4rem; font-size: 0.8rem; color: var(--t-text2); font-style: italic; }
.con-empty { text-align: center; padding: 3rem; color: var(--t-text3); }
</style>
