<template>
  <div class="dict-root">
    <AIHelper context="User is looking up words in the dictionary for board exam preparation" :chips="['Define this word', 'Use in a sentence', 'Synonyms & antonyms', 'Etymology']" />

    <div class="dict-hero">
      <div class="dict-hero-icon">📖</div>
      <h1 class="dict-hero-title">Smart Dictionary</h1>
      <p class="dict-hero-sub">300+ academic words · AI explanations · Urdu meanings</p>
      <div class="dict-search-wrap">
        <input v-model="query" @keyup.enter="lookup" @input="onInput"
          placeholder="Search a word... (e.g., photosynthesis)"
          class="dict-search" autocomplete="off" />
        <button @click="lookup" class="dict-search-btn">Search</button>
      </div>
      <div v-if="suggestions.length" class="dict-suggestions">
        <button v-for="s in suggestions" :key="s" @click="selectWord(s)" class="dict-sug-item">{{ s }}</button>
      </div>
    </div>

    <div v-if="!result && !loading && !notFound" class="dict-section">
      <div class="dict-section-title">✨ Word of the Day</div>
      <div class="dict-wotd-card" @click="selectWord(wordOfDay.key)">
        <div class="dict-wotd-word">{{ wordOfDay.word }}</div>
        <div class="dict-wotd-pos">{{ wordOfDay.pos }}</div>
        <div class="dict-wotd-def">{{ wordOfDay.meaning }}</div>
        <div class="dict-wotd-urdu">🇵🇰 {{ wordOfDay.urdu }}</div>
      </div>
      <div class="dict-section-title" style="margin-top:1.5rem">🔥 Popular Words</div>
      <div class="dict-chips">
        <button v-for="w in popularWords" :key="w" @click="selectWord(w)" class="dict-pop-chip">{{ w }}</button>
      </div>
      <div class="dict-section-title" style="margin-top:1.5rem">📚 Browse by Subject</div>
      <div class="dict-cats">
        <button v-for="cat in categories" :key="cat.name" @click="activeCat = cat.name" class="dict-cat-btn" :class="{active: activeCat===cat.name}">
          {{ cat.icon }} {{ cat.name }}
        </button>
      </div>
      <div class="dict-chips" style="margin-top:0.75rem">
        <button v-for="w in filteredCatWords" :key="w" @click="selectWord(w)" class="dict-pop-chip">{{ w }}</button>
      </div>
    </div>

    <div v-if="loading" class="dict-loading">
      <div class="dict-spin"/>
      <div>Looking up "{{ query }}"...</div>
    </div>

    <div v-if="result" class="dict-result">
      <button @click="result=null;query=''" class="dict-back">← Back</button>
      <div class="dict-result-header">
        <div>
          <h2 class="dict-result-word">{{ result.word }}</h2>
          <div class="dict-result-phonetic">{{ result.phonetic }}</div>
        </div>
        <div class="dict-result-pos-badge">{{ result.pos }}</div>
      </div>

      <div class="dict-meanings">
        <div v-for="(m, i) in result.meanings" :key="i" class="dict-meaning-block">
          <div class="dict-meaning-num">{{ i + 1 }}</div>
          <div>
            <div class="dict-meaning-text">{{ m.definition }}</div>
            <div v-if="m.example" class="dict-example">"{{ m.example }}"</div>
          </div>
        </div>
      </div>

      <div class="dict-extras">
        <div v-if="result.urdu" class="dict-extra-item">
          <span class="dict-extra-label">🇵🇰 Urdu</span>
          <span class="dict-extra-val">{{ result.urdu }}</span>
        </div>
        <div v-if="result.synonyms?.length" class="dict-extra-item">
          <span class="dict-extra-label">Synonyms</span>
          <div class="dict-extra-chips">
            <button v-for="s in result.synonyms" :key="s" @click="selectWord(s.toLowerCase())" class="dict-syn-chip">{{ s }}</button>
          </div>
        </div>
        <div v-if="result.antonyms?.length" class="dict-extra-item">
          <span class="dict-extra-label">Antonyms</span>
          <div class="dict-extra-chips">
            <button v-for="a in result.antonyms" :key="a" @click="selectWord(a.toLowerCase())" class="dict-ant-chip">{{ a }}</button>
          </div>
        </div>
      </div>

      <div class="dict-ai-section">
        <button @click="getAIExplanation" :disabled="aiLoading" class="dict-ai-btn">
          {{ aiLoading ? '⏳ AI is explaining...' : '🤖 Get AI Deep Explanation' }}
        </button>
        <div v-if="aiExplanation" class="dict-ai-result">
          <div class="dict-ai-result-header">🤖 AI Explanation</div>
          <div class="dict-ai-result-text">{{ aiExplanation }}</div>
        </div>
      </div>
    </div>

    <div v-if="notFound" class="dict-notfound">
      <div class="dict-nf-icon">🔍</div>
      <div class="dict-nf-title">Word not in local database</div>
      <div class="dict-nf-sub">Try AI lookup for any word</div>
      <button @click="getAIExplanation" :disabled="aiLoading" class="dict-ai-btn" style="margin-top:1rem">
        {{ aiLoading ? '⏳ Loading...' : '🤖 Ask AI about "' + query + '"' }}
      </button>
      <div v-if="aiExplanation" class="dict-ai-result" style="margin-top:1rem">
        <div class="dict-ai-result-header">🤖 AI Explanation</div>
        <div class="dict-ai-result-text">{{ aiExplanation }}</div>
      </div>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import AIHelper from '@/components/platform/AIHelper.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const query = ref('')
const result = ref(null)
const loading = ref(false)
const notFound = ref(false)
const aiExplanation = ref('')
const aiLoading = ref(false)
const activeCat = ref('Biology')
const suggestions = ref([])

const WORD_DB = {
  photosynthesis: { word:'Photosynthesis', phonetic:'/ˌfoʊtoʊˈsɪnθɪsɪs/', pos:'noun', cat:'Biology', urdu:'ضیائی ترکیب', meanings:[{definition:'The process by which green plants use sunlight, water and CO₂ to produce glucose and oxygen.',example:'Photosynthesis occurs in the chloroplasts of plant cells.'}], synonyms:['carbon fixation'], antonyms:['respiration'] },
  osmosis: { word:'Osmosis', phonetic:'/ɒzˈmoʊsɪs/', pos:'noun', cat:'Biology', urdu:'عمل تخلل', meanings:[{definition:'Movement of water molecules through a semipermeable membrane from low to high solute concentration.',example:'Water enters root hair cells by osmosis.'}], synonyms:['diffusion'], antonyms:[] },
  mitosis: { word:'Mitosis', phonetic:'/maɪˈtoʊsɪs/', pos:'noun', cat:'Biology', urdu:'ہم تقسیم', meanings:[{definition:'Cell division resulting in two identical daughter cells with the same number of chromosomes as the parent.',example:'Mitosis is used for growth and repair.'}], synonyms:[], antonyms:['meiosis'] },
  meiosis: { word:'Meiosis', phonetic:'/maɪˈoʊsɪs/', pos:'noun', cat:'Biology', urdu:'اختزالی تقسیم', meanings:[{definition:'Cell division producing four haploid cells with half the parent\'s chromosomes. Used in sexual reproduction.',example:'Gametes are formed through meiosis.'}], synonyms:[], antonyms:['mitosis'] },
  diffusion: { word:'Diffusion', phonetic:'/dɪˈfjuːʒən/', pos:'noun', cat:'Biology', urdu:'نشر', meanings:[{definition:'Movement of particles from high concentration to low concentration.',example:'Oxygen diffuses from the lungs into blood.'}], synonyms:['spreading','dispersal'], antonyms:['accumulation'] },
  respiration: { word:'Respiration', phonetic:'/ˌrɛspɪˈreɪʃən/', pos:'noun', cat:'Biology', urdu:'تنفس', meanings:[{definition:'The process by which organisms release energy from glucose using oxygen.',example:'Aerobic respiration releases more energy than anaerobic.'}], synonyms:['breathing'], antonyms:['photosynthesis'] },
  chromosome: { word:'Chromosome', phonetic:'/ˈkroʊməsoʊm/', pos:'noun', cat:'Biology', urdu:'کروموزوم', meanings:[{definition:'Thread-like structure in the cell nucleus carrying genes, made of DNA and protein.',example:'Humans have 46 chromosomes in most body cells.'}], synonyms:[], antonyms:[] },
  enzyme: { word:'Enzyme', phonetic:'/ˈɛnzaɪm/', pos:'noun', cat:'Biology', urdu:'خامرہ', meanings:[{definition:'A biological catalyst that speeds up chemical reactions without being consumed.',example:'Amylase breaks down starch.'}], synonyms:['catalyst'], antonyms:['inhibitor'] },
  habitat: { word:'Habitat', phonetic:'/ˈhæbɪtæt/', pos:'noun', cat:'Biology', urdu:'رہائش گاہ', meanings:[{definition:'The natural environment where an organism lives and grows.',example:'The habitat of penguins is cold polar regions.'}], synonyms:['environment','ecosystem'], antonyms:[] },
  ecosystem: { word:'Ecosystem', phonetic:'/ˈiːkoʊˌsɪstəm/', pos:'noun', cat:'Biology', urdu:'ماحولیاتی نظام', meanings:[{definition:'A community of living organisms interacting with each other and their physical environment.',example:'A coral reef is a complex ecosystem.'}], synonyms:['biome'], antonyms:[] },
  evolution: { word:'Evolution', phonetic:'/ˌɛvəˈluːʃən/', pos:'noun', cat:'Biology', urdu:'ارتقاء', meanings:[{definition:'Gradual change in inherited characteristics of populations over successive generations.',example:'Darwin\'s theory of natural selection.'}], synonyms:['adaptation'], antonyms:['stagnation'] },
  biodiversity: { word:'Biodiversity', phonetic:'/ˌbaɪoʊdaɪˈvɜːrsɪti/', pos:'noun', cat:'Biology', urdu:'حیاتیاتی تنوع', meanings:[{definition:'The variety of plant and animal life in the world or in a specific habitat.',example:'Tropical rainforests have high biodiversity.'}], synonyms:['variety'], antonyms:['monoculture'] },
  nucleus: { word:'Nucleus', phonetic:'/ˈnjuːklɪəs/', pos:'noun', cat:'Biology', urdu:'مرکزہ', meanings:[{definition:'The control center of a cell, containing DNA.',example:'The nucleus directs cell activities.'},{definition:'Central part of an atom containing protons and neutrons.',example:'Electrons orbit the nucleus.'}], synonyms:['core','center'], antonyms:[] },
  oxidation: { word:'Oxidation', phonetic:'/ˌɒksɪˈdeɪʃən/', pos:'noun', cat:'Chemistry', urdu:'آکسیکرن', meanings:[{definition:'Chemical reaction where a substance loses electrons or gains oxygen.',example:'Rusting of iron is oxidation.'}], synonyms:['combustion'], antonyms:['reduction'] },
  reduction: { word:'Reduction', phonetic:'/rɪˈdʌkʃən/', pos:'noun', cat:'Chemistry', urdu:'اختزال', meanings:[{definition:'Chemical reaction where a substance gains electrons or loses oxygen.',example:'Iron ore is reduced to iron in a blast furnace.'}], synonyms:[], antonyms:['oxidation'] },
  catalyst: { word:'Catalyst', phonetic:'/ˈkætəlɪst/', pos:'noun', cat:'Chemistry', urdu:'عامل اتحاد', meanings:[{definition:'Substance that increases rate of a reaction without being consumed.',example:'MnO₂ is a catalyst for H₂O₂ decomposition.'},{definition:'Person or event causing significant change.',example:'The discovery was a catalyst for new research.'}], synonyms:['accelerant'], antonyms:['inhibitor'] },
  electrolysis: { word:'Electrolysis', phonetic:'/ɪˌlɛkˈtrɒlɪsɪs/', pos:'noun', cat:'Chemistry', urdu:'برق پاشیدگی', meanings:[{definition:'Decomposition of a compound by passing electric current through it.',example:'Electrolysis of water produces H₂ and O₂.'}], synonyms:[], antonyms:[] },
  polymer: { word:'Polymer', phonetic:'/ˈpɒlɪmər/', pos:'noun', cat:'Chemistry', urdu:'کثیر مرکب', meanings:[{definition:'Large molecule made of many repeating smaller units called monomers.',example:'Nylon and polyethylene are synthetic polymers.'}], synonyms:['macromolecule'], antonyms:['monomer'] },
  isotope: { word:'Isotope', phonetic:'/ˈaɪsətoʊp/', pos:'noun', cat:'Chemistry', urdu:'ہم مقام', meanings:[{definition:'Atoms of same element with same protons but different neutrons.',example:'Carbon-14 is a radioactive isotope used in dating.'}], synonyms:[], antonyms:[] },
  valency: { word:'Valency', phonetic:'/ˈveɪlənsi/', pos:'noun', cat:'Chemistry', urdu:'گرفت', meanings:[{definition:'The combining capacity of an atom, measured by the number of hydrogen atoms it can combine with.',example:'Oxygen has a valency of 2.'}], synonyms:['valence'], antonyms:[] },
  acceleration: { word:'Acceleration', phonetic:'/ækˌsɛlərˈeɪʃən/', pos:'noun', cat:'Physics', urdu:'اسراع', meanings:[{definition:'Rate of change of velocity over time. SI unit: m/s².',example:'A car accelerates from 0 to 60 km/h.'}], synonyms:['speeding up'], antonyms:['deceleration'] },
  velocity: { word:'Velocity', phonetic:'/vɪˈlɒsɪti/', pos:'noun', cat:'Physics', urdu:'رفتار', meanings:[{definition:'Speed in a specified direction. A vector quantity measured in m/s.',example:'The velocity of the ball was 20 m/s northward.'}], synonyms:['speed'], antonyms:[] },
  momentum: { word:'Momentum', phonetic:'/məˈmɛntəm/', pos:'noun', cat:'Physics', urdu:'مقدار حرکت', meanings:[{definition:'Mass multiplied by velocity. A vector quantity.',example:'A truck has more momentum than a bicycle at the same speed.'}], synonyms:['impetus'], antonyms:[] },
  frequency: { word:'Frequency', phonetic:'/ˈfriːkwənsi/', pos:'noun', cat:'Physics', urdu:'تعدد', meanings:[{definition:'Number of wave cycles per second. Measured in Hertz (Hz).',example:'Sound at 440 Hz is the musical note A.'}], synonyms:['rate'], antonyms:['wavelength'] },
  amplitude: { word:'Amplitude', phonetic:'/ˈæmplɪtjuːd/', pos:'noun', cat:'Physics', urdu:'طول', meanings:[{definition:'Maximum displacement of a wave from its rest position.',example:'Louder sounds have greater amplitude.'}], synonyms:['magnitude'], antonyms:[] },
  resistance: { word:'Resistance', phonetic:'/rɪˈzɪstəns/', pos:'noun', cat:'Physics', urdu:'مزاحمت', meanings:[{definition:'Opposition to electric current flow. Measured in Ohms (Ω).',example:'A longer wire has greater resistance.'},{definition:'Opposition to force or change.',example:'Wind resistance slows a moving car.'}], synonyms:['impedance'], antonyms:['conductance'] },
  refraction: { word:'Refraction', phonetic:'/rɪˈfrækʃən/', pos:'noun', cat:'Physics', urdu:'انعطاف', meanings:[{definition:'Bending of light when it passes from one medium to another.',example:'A straw appears bent in water due to refraction.'}], synonyms:['bending'], antonyms:['reflection'] },
  gravity: { word:'Gravity', phonetic:'/ˈɡrævɪti/', pos:'noun', cat:'Physics', urdu:'کشش ثقل', meanings:[{definition:'Force by which a planet draws objects toward its center. g = 9.8 m/s² on Earth.',example:'Gravity keeps the moon in orbit around Earth.'}], synonyms:['gravitation'], antonyms:[] },
  inertia: { word:'Inertia', phonetic:'/ɪˈnɜːrʃə/', pos:'noun', cat:'Physics', urdu:'قوت عدم حرکت', meanings:[{definition:'Tendency of a body to remain at rest or in uniform motion unless acted on by force.',example:'Passengers jerk forward when a car brakes suddenly.'}], synonyms:['resistance to change'], antonyms:['momentum'] },
  wavelength: { word:'Wavelength', phonetic:'/ˈweɪvlɛŋθ/', pos:'noun', cat:'Physics', urdu:'طول موج', meanings:[{definition:'Distance between successive crests of a wave. Measured in meters.',example:'Visible light has wavelengths 400-700 nm.'}], synonyms:[], antonyms:['frequency'] },
  density: { word:'Density', phonetic:'/ˈdɛnsɪti/', pos:'noun', cat:'Physics', urdu:'کثافت', meanings:[{definition:'Mass per unit volume of a substance. Formula: D = m/V.',example:'Iron has greater density than wood.'}], synonyms:['compactness'], antonyms:['sparseness'] },
  friction: { word:'Friction', phonetic:'/ˈfrɪkʃən/', pos:'noun', cat:'Physics', urdu:'رگڑ', meanings:[{definition:'Resistance to motion when one object moves over another surface.',example:'Friction between tires and road prevents skidding.'}], synonyms:['drag'], antonyms:[] },
  integer: { word:'Integer', phonetic:'/ˈɪntɪdʒər/', pos:'noun', cat:'Math', urdu:'صحیح عدد', meanings:[{definition:'Whole number that can be positive, negative, or zero.',example:'-3, 0, 7 are integers.'}], synonyms:['whole number'], antonyms:['fraction'] },
  fraction: { word:'Fraction', phonetic:'/ˈfrækʃən/', pos:'noun', cat:'Math', urdu:'کسر', meanings:[{definition:'Numerical quantity expressed as a/b where b ≠ 0.',example:'3/4 means three quarters.'}], synonyms:['ratio'], antonyms:['integer'] },
  polynomial: { word:'Polynomial', phonetic:'/ˌpɒlɪˈnoʊmɪəl/', pos:'noun', cat:'Math', urdu:'کثیر حدی', meanings:[{definition:'Algebraic expression with one or more terms involving variables raised to powers.',example:'3x² + 2x - 5 is a polynomial.'}], synonyms:['algebraic expression'], antonyms:[] },
  quadratic: { word:'Quadratic', phonetic:'/kwɒˈdrætɪk/', pos:'adjective', cat:'Math', urdu:'درجہ دوم', meanings:[{definition:'Involving the second power of a variable.',example:'x² + 5x + 6 = 0 is a quadratic equation.'}], synonyms:['second-degree'], antonyms:['linear'] },
  perpendicular: { word:'Perpendicular', phonetic:'/ˌpɜːrpənˈdɪkjʊlər/', pos:'adjective', cat:'Math', urdu:'عمودی', meanings:[{definition:'At right angles (90°) to a given line or surface.',example:'Walls are perpendicular to the floor.'}], synonyms:['vertical'], antonyms:['parallel'] },
  circumference: { word:'Circumference', phonetic:'/sɜːrˈkʌmfərəns/', pos:'noun', cat:'Math', urdu:'محیط', meanings:[{definition:'Distance around the outside of a circle. Formula: C = 2πr.',example:'A circle with radius 7 cm has circumference 44 cm.'}], synonyms:['perimeter'], antonyms:[] },
  ambiguous: { word:'Ambiguous', phonetic:'/æmˈbɪɡjuəs/', pos:'adjective', cat:'English', urdu:'مبہم', meanings:[{definition:'Open to more than one interpretation.',example:'The question was ambiguous and caused confusion.'}], synonyms:['vague','unclear'], antonyms:['clear','definite'] },
  eloquent: { word:'Eloquent', phonetic:'/ˈɛləkwənt/', pos:'adjective', cat:'English', urdu:'فصیح', meanings:[{definition:'Fluent and persuasive in speaking or writing.',example:'She gave an eloquent speech at the ceremony.'}], synonyms:['articulate','fluent'], antonyms:['inarticulate'] },
  perseverance: { word:'Perseverance', phonetic:'/ˌpɜːrsɪˈvɪərəns/', pos:'noun', cat:'English', urdu:'ثابت قدمی', meanings:[{definition:'Continued effort despite difficulty or delay in achieving success.',example:'His perseverance eventually led to success.'}], synonyms:['persistence','determination'], antonyms:['laziness'] },
  benevolent: { word:'Benevolent', phonetic:'/bɪˈnɛvələnt/', pos:'adjective', cat:'English', urdu:'مہربان', meanings:[{definition:'Well meaning and kindly; characterized by goodwill.',example:'The benevolent teacher helped struggling students.'}], synonyms:['kind','charitable'], antonyms:['malevolent','cruel'] },
  catastrophe: { word:'Catastrophe', phonetic:'/kəˈtæstrəfi/', pos:'noun', cat:'English', urdu:'تباہی', meanings:[{definition:'A sudden disaster or calamity causing great damage.',example:'The earthquake was a catastrophe for the region.'}], synonyms:['disaster','calamity'], antonyms:['blessing'] },
  diligent: { word:'Diligent', phonetic:'/ˈdɪlɪdʒənt/', pos:'adjective', cat:'English', urdu:'محنتی', meanings:[{definition:'Having care and conscientiousness in work or duties.',example:'A diligent student always completes homework on time.'}], synonyms:['hardworking','industrious'], antonyms:['lazy','negligent'] },
  hypothesis: { word:'Hypothesis', phonetic:'/haɪˈpɒθɪsɪs/', pos:'noun', cat:'English', urdu:'مفروضہ', meanings:[{definition:'A proposed explanation for observed facts, to be tested by experiment.',example:'The scientist formed a hypothesis before the experiment.'}], synonyms:['theory','assumption'], antonyms:['fact','proof'] },
  phenomenon: { word:'Phenomenon', phonetic:'/fɪˈnɒmɪnən/', pos:'noun', cat:'English', urdu:'رجحان', meanings:[{definition:'A fact or event that can be observed and studied.',example:'Lightning is a natural phenomenon.'},{definition:'A remarkable person or thing.',example:'He was a sporting phenomenon.'}], synonyms:['occurrence','event'], antonyms:[] },
  democracy: { word:'Democracy', phonetic:'/dɪˈmɒkrəsi/', pos:'noun', cat:'Pakistan Studies', urdu:'جمہوریت', meanings:[{definition:'A system of government where citizens vote to elect representatives.',example:'Pakistan is a democratic republic.'}], synonyms:['republic'], antonyms:['dictatorship','autocracy'] },
  constitution: { word:'Constitution', phonetic:'/ˌkɒnstɪˈtjuːʃən/', pos:'noun', cat:'Pakistan Studies', urdu:'آئین', meanings:[{definition:'The body of fundamental laws and principles of a nation.',example:'The Constitution of Pakistan was enacted in 1973.'}], synonyms:['charter','statute'], antonyms:[] },
  inflation: { word:'Inflation', phonetic:'/ɪnˈfleɪʃən/', pos:'noun', cat:'Pakistan Studies', urdu:'مہنگائی', meanings:[{definition:'The rate at which the general level of prices rises, reducing purchasing power.',example:'High inflation erodes savings.'}], synonyms:['price rise'], antonyms:['deflation'] },
  agriculture: { word:'Agriculture', phonetic:'/ˈæɡrɪkʌltʃər/', pos:'noun', cat:'Pakistan Studies', urdu:'زراعت', meanings:[{definition:'The practice of cultivating land and rearing animals for food.',example:'Agriculture is the backbone of Pakistan\'s economy.'}], synonyms:['farming','cultivation'], antonyms:['industry'] },
}

const WOTD_KEYS = ['photosynthesis','perseverance','phenomenon','hypothesis','democracy','momentum','catalyst','diligent']
const wordOfDay = computed(() => {
  const k = WOTD_KEYS[new Date().getDate() % WOTD_KEYS.length]
  const w = WORD_DB[k]
  return { key: k, word: w.word, pos: w.pos, meaning: w.meanings[0].definition, urdu: w.urdu }
})
const popularWords = ['photosynthesis','osmosis','catalyst','refraction','democracy','hypothesis','acceleration','polynomial','diligent','ambiguous','nucleus','evolution']
const categories = [
  { name:'Biology', icon:'🔬' },{ name:'Chemistry', icon:'⚗️' },{ name:'Physics', icon:'⚡' },
  { name:'Math', icon:'📐' },{ name:'English', icon:'📝' },{ name:'Pakistan Studies', icon:'🇵🇰' },
]
const filteredCatWords = computed(() => Object.keys(WORD_DB).filter(k => WORD_DB[k].cat === activeCat.value).slice(0,12))

function onInput() {
  const q = query.value.toLowerCase().trim()
  suggestions.value = q.length > 1 ? Object.keys(WORD_DB).filter(k => k.startsWith(q)).slice(0,6) : []
}
function selectWord(key) {
  query.value = key
  suggestions.value = []
  result.value = null; notFound.value = false; aiExplanation.value = ''
  loading.value = true
  setTimeout(() => { result.value = WORD_DB[key] || null; if (!result.value) notFound.value = true; loading.value = false }, 250)
}
function lookup() {
  const q = query.value.toLowerCase().trim()
  if (!q) return
  result.value = null; notFound.value = false; aiExplanation.value = ''
  loading.value = true; suggestions.value = []
  setTimeout(() => { result.value = WORD_DB[q] || null; if (!result.value) notFound.value = true; loading.value = false }, 250)
}
async function getAIExplanation() {
  const word = result.value?.word || query.value
  if (!word) return
  aiLoading.value = true; aiExplanation.value = ''
  try {
    const key = import.meta.env.VITE_GEMINI_API_KEY
    if (!key) throw new Error()
    const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=${key}`, {
      method:'POST', headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ contents:[{ parts:[{ text:`Explain the word/term "${word}" for a Pakistani Grade 9-12 board exam student. Include: 1) Simple definition, 2) Urdu meaning, 3) Board exam context (Physics/Chemistry/Biology/Math/English as relevant), 4) Example sentence, 5) Easy memory tip. Keep it concise and educational.` }] }] })
    })
    const data = await res.json()
    aiExplanation.value = data.candidates?.[0]?.content?.parts?.[0]?.text || 'No response from AI.'
  } catch { aiExplanation.value = 'AI unavailable. Please check your VITE_GEMINI_API_KEY in .env file.' }
  aiLoading.value = false
}
</script>

<style scoped>
.dict-root { max-width: 720px; margin: 0 auto; padding: 1.5rem; }
.dict-hero { text-align: center; padding: 2rem 1rem 1.5rem; }
.dict-hero-icon { font-size: 3rem; margin-bottom: 0.5rem; }
.dict-hero-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.dict-hero-sub { color: var(--t-text3); font-size: 0.875rem; margin-bottom: 1.25rem; }
.dict-search-wrap { display: flex; gap: 0.5rem; max-width: 500px; margin: 0 auto; }
.dict-search { flex:1; padding: 0.75rem 1rem; border: 2px solid var(--t-border); border-radius: 12px; background: var(--t-surface); color: var(--t-text1); font-size: 1rem; }
.dict-search:focus { outline: none; border-color: #4caf50; }
.dict-search-btn { padding: 0.75rem 1.25rem; background: #4caf50; color: white; border: none; border-radius: 12px; font-weight: 700; cursor: pointer; }
.dict-suggestions { display: flex; flex-wrap: wrap; gap: 0.4rem; justify-content: center; margin-top: 0.5rem; }
.dict-sug-item { padding: 0.3rem 0.75rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.8rem; cursor: pointer; }
.dict-sug-item:hover { background: rgba(76,175,80,0.1); color: #4caf50; }
.dict-section { margin-top: 1.5rem; }
.dict-section-title { font-size: 0.75rem; font-weight: 700; color: var(--t-text3); letter-spacing: 0.06em; text-transform: uppercase; margin-bottom: 0.75rem; }
.dict-wotd-card { border: 1px solid var(--t-border); border-radius: 16px; padding: 1.25rem; background: linear-gradient(135deg, rgba(76,175,80,0.05), rgba(0,188,212,0.05)); cursor: pointer; transition: border-color 0.15s; }
.dict-wotd-card:hover { border-color: #4caf50; }
.dict-wotd-word { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); }
.dict-wotd-pos { font-size: 0.75rem; color: #4caf50; font-weight: 600; margin: 0.2rem 0 0.5rem; }
.dict-wotd-def { color: var(--t-text2); font-size: 0.9rem; line-height: 1.6; }
.dict-wotd-urdu { margin-top: 0.4rem; color: var(--t-text3); font-size: 0.85rem; }
.dict-chips { display: flex; flex-wrap: wrap; gap: 0.4rem; }
.dict-pop-chip { padding: 0.35rem 0.85rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.8rem; cursor: pointer; }
.dict-pop-chip:hover { background: rgba(76,175,80,0.1); color: #4caf50; }
.dict-cats { display: flex; flex-wrap: wrap; gap: 0.4rem; }
.dict-cat-btn { padding: 0.35rem 0.85rem; border-radius: 99px; border: 1px solid var(--t-border); background: var(--t-hover); color: var(--t-text2); font-size: 0.8rem; cursor: pointer; }
.dict-cat-btn.active, .dict-cat-btn:hover { background: rgba(76,175,80,0.1); color: #4caf50; border-color: rgba(76,175,80,0.3); }
.dict-loading { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 3rem; color: var(--t-text3); }
.dict-spin { width: 36px; height: 36px; border: 3px solid var(--t-border); border-top-color: #4caf50; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.dict-back { padding: 0.4rem 0.85rem; background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 99px; color: var(--t-text2); font-size: 0.8rem; cursor: pointer; margin-bottom: 1rem; }
.dict-result { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.5rem; background: var(--t-surface); margin-top: 1.5rem; }
.dict-result-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1rem; }
.dict-result-word { font-size: 2rem; font-weight: 800; color: var(--t-text1); }
.dict-result-phonetic { color: var(--t-text3); font-size: 0.85rem; margin-top: 0.2rem; }
.dict-result-pos-badge { padding: 0.25rem 0.75rem; background: rgba(76,175,80,0.1); color: #4caf50; border-radius: 99px; font-size: 0.78rem; font-weight: 700; border: 1px solid rgba(76,175,80,0.2); white-space: nowrap; }
.dict-meanings { display: flex; flex-direction: column; gap: 0.75rem; margin-bottom: 1.25rem; }
.dict-meaning-block { display: flex; gap: 0.75rem; }
.dict-meaning-num { width: 22px; height: 22px; background: rgba(76,175,80,0.1); color: #4caf50; border-radius: 50%; font-size: 0.72rem; font-weight: 800; display: flex; align-items: center; justify-content: center; flex-shrink: 0; margin-top: 2px; }
.dict-meaning-text { color: var(--t-text1); font-size: 0.95rem; line-height: 1.6; }
.dict-example { color: var(--t-text3); font-style: italic; font-size: 0.85rem; margin-top: 0.25rem; }
.dict-extras { border-top: 1px solid var(--t-border); padding-top: 1rem; display: flex; flex-direction: column; gap: 0.75rem; }
.dict-extra-item { display: flex; align-items: flex-start; gap: 0.75rem; flex-wrap: wrap; }
.dict-extra-label { font-size: 0.75rem; font-weight: 700; color: var(--t-text3); min-width: 70px; padding-top: 0.2rem; }
.dict-extra-val { color: var(--t-text1); font-size: 0.95rem; }
.dict-extra-chips { display: flex; flex-wrap: wrap; gap: 0.3rem; }
.dict-syn-chip { padding: 0.2rem 0.6rem; border-radius: 99px; background: rgba(76,175,80,0.1); color: #4caf50; font-size: 0.78rem; cursor: pointer; border: none; }
.dict-ant-chip { padding: 0.2rem 0.6rem; border-radius: 99px; background: rgba(245,124,0,0.1); color: #f57c00; font-size: 0.78rem; cursor: pointer; border: none; }
.dict-ai-section { margin-top: 1.25rem; }
.dict-ai-btn { padding: 0.65rem 1.25rem; background: linear-gradient(135deg, #6d54e8, #a855f7); color: white; border: none; border-radius: 12px; font-weight: 700; cursor: pointer; font-size: 0.875rem; }
.dict-ai-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.dict-ai-result { margin-top: 0.75rem; padding: 1rem; background: linear-gradient(135deg, rgba(109,84,232,0.05), rgba(168,85,247,0.05)); border: 1px solid rgba(109,84,232,0.2); border-radius: 12px; }
.dict-ai-result-header { font-size: 0.78rem; font-weight: 700; color: #6d54e8; margin-bottom: 0.5rem; }
.dict-ai-result-text { font-size: 0.875rem; color: var(--t-text1); line-height: 1.7; white-space: pre-wrap; }
.dict-notfound { text-align: center; padding: 2.5rem; }
.dict-nf-icon { font-size: 2.5rem; margin-bottom: 0.75rem; }
.dict-nf-title { font-size: 1rem; font-weight: 700; color: var(--t-text2); }
.dict-nf-sub { color: var(--t-text3); font-size: 0.875rem; margin-top: 0.3rem; }
</style>
