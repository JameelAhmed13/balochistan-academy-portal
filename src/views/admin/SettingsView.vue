<template>
  <div class="animate-fade-in space-y-6">

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">⚙️</div>
      <div class="ag-banner-title">Platform Settings</div>
      <div class="ag-banner-sub">Configure the platform, AI, subscriptions, and branding</div>
      <div class="ag-banner-actions">
        <RouterLink to="/app/admin" class="btn-ghost flex items-center gap-1.5 text-sm">
          <ArrowLeft class="w-4 h-4" /> Back
        </RouterLink>
      </div>
    </div>

    <!-- General Platform -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:var(--t-accent);">
        <Globe class="w-4 h-4" /> General Platform
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="label">Platform Name</label>
            <input v-model="general.platformName" class="input mt-1" placeholder="Balochistan Academy Portal" />
          </div>
          <div>
            <label class="label">Institute Name</label>
            <input v-model="general.instituteName" class="input mt-1" placeholder="Enter institute name" />
          </div>
          <div>
            <label class="label">Support WhatsApp</label>
            <input v-model="general.whatsapp" class="input mt-1" placeholder="+92 300 0000000" />
          </div>
          <div>
            <label class="label">Grade Level</label>
            <select v-model="general.gradeLevel" class="input mt-1">
              <option value="9">Grade 9</option>
              <option value="10">Grade 10</option>
            </select>
          </div>
          <div>
            <label class="label">Max Students</label>
            <input v-model.number="general.maxStudents" type="number" class="input mt-1" placeholder="500" />
          </div>
        </div>
        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" @click="save('general')">
            <Save class="w-4 h-4" /> Save General Settings
          </button>
        </div>
      </div>
    </div>

    <!-- Subscription & Pricing -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:#10b981;">
        <Shield class="w-4 h-4" /> Subscription &amp; Pricing
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div>
            <label class="label">Annual Price (PKR)</label>
            <input v-model.number="subscription.annualPrice" type="number" class="input mt-1" placeholder="999" />
          </div>
          <div>
            <label class="label">Monthly Price (PKR)</label>
            <input v-model.number="subscription.monthlyPrice" type="number" class="input mt-1" placeholder="99" />
          </div>
          <div>
            <label class="label">Trial Days</label>
            <input v-model.number="subscription.trialDays" type="number" class="input mt-1" placeholder="7" />
          </div>
        </div>
        <div class="flex flex-wrap gap-6 mt-4">
          <label class="toggle-row">
            <span class="label mb-0">Auto-Expire Subscriptions</span>
            <div class="toggle-wrap">
              <input type="checkbox" v-model="subscription.autoExpire" id="autoExpire" />
              <label for="autoExpire" class="toggle-pill"></label>
            </div>
          </label>
          <label class="toggle-row">
            <span class="label mb-0">Activate on Payment</span>
            <div class="toggle-wrap">
              <input type="checkbox" v-model="subscription.activateOnPayment" id="activatePayment" />
              <label for="activatePayment" class="toggle-pill"></label>
            </div>
          </label>
        </div>
        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" @click="save('subscription')">
            <Save class="w-4 h-4" /> Save Subscription Settings
          </button>
        </div>
      </div>
    </div>

    <!-- Coin Economy -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:var(--t-gold);">
        <Coins class="w-4 h-4" /> Coin Economy
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div>
            <label class="label">Coin Rate (PKR per coin)</label>
            <input v-model.number="coins.coinRate" type="number" class="input mt-1" placeholder="1" />
          </div>
          <div>
            <label class="label">Min Questions for Eligibility</label>
            <input v-model.number="coins.minQuestions" type="number" class="input mt-1" placeholder="50" />
          </div>
          <div>
            <label class="label">Min Withdrawal Coins</label>
            <input v-model.number="coins.minWithdrawal" type="number" class="input mt-1" placeholder="200" />
          </div>
          <div>
            <label class="label">Bonus for Perfect Score</label>
            <input v-model.number="coins.perfectScoreBonus" type="number" class="input mt-1" placeholder="10" />
          </div>
          <div>
            <label class="label">Daily Login Bonus</label>
            <input v-model.number="coins.dailyLoginBonus" type="number" class="input mt-1" placeholder="2" />
          </div>
        </div>
        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" @click="save('coins')">
            <Save class="w-4 h-4" /> Save Coin Settings
          </button>
        </div>
      </div>
    </div>

    <!-- AI & Features -->
    <div class="ag-card">
      <div class="ag-card-section-label" style="color:#8b5cf6;">
        <Bot class="w-4 h-4" /> AI &amp; Features
      </div>
      <div class="ag-card-body">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="label">Gemini API Key</label>
            <input v-model="ai.geminiKey" type="password" class="input mt-1 font-mono" placeholder="AIza••••••••••••••••••••••" />
          </div>
          <div>
            <label class="label">TTS API Key</label>
            <input v-model="ai.ttsKey" type="password" class="input mt-1 font-mono" placeholder="AIza••••••••••••••••••••••" />
          </div>
          <div>
            <label class="label">Default AI Language</label>
            <select v-model="ai.defaultLanguage" class="input mt-1">
              <option value="en">English</option>
              <option value="ur">Urdu</option>
              <option value="both">Both</option>
            </select>
          </div>
        </div>
        <div class="flex flex-wrap gap-6 mt-4">
          <label class="toggle-row">
            <span class="label mb-0">Enable AI Tutor</span>
            <div class="toggle-wrap">
              <input type="checkbox" v-model="ai.enableTutor" id="enableTutor" />
              <label for="enableTutor" class="toggle-pill"></label>
            </div>
          </label>
          <label class="toggle-row">
            <span class="label mb-0">Enable Video Lessons</span>
            <div class="toggle-wrap">
              <input type="checkbox" v-model="ai.enableVideo" id="enableVideo" />
              <label for="enableVideo" class="toggle-pill"></label>
            </div>
          </label>
          <label class="toggle-row">
            <span class="label mb-0">Enable Subjective AI Grading</span>
            <div class="toggle-wrap">
              <input type="checkbox" v-model="ai.enableGrading" id="enableGrading" />
              <label for="enableGrading" class="toggle-pill"></label>
            </div>
          </label>
        </div>
        <div class="mt-4">
          <button class="btn-primary flex items-center gap-2" @click="save('ai')">
            <Save class="w-4 h-4" /> Save AI Settings
          </button>
        </div>
      </div>
    </div>

    <!-- Save success toast -->
    <Transition name="toast">
      <div v-if="toastVisible" class="fixed bottom-6 right-6 z-50 px-5 py-3 rounded-xl shadow-lg text-sm font-medium flex items-center gap-2"
        style="background:var(--t-accent);color:#fff;">
        <Save class="w-4 h-4" /> Settings saved successfully!
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ArrowLeft, Settings, Globe, Coins, Bot, Shield, Save } from '@lucide/vue'

// General
const general = ref({
  platformName: 'Balochistan Academy Portal',
  instituteName: 'UltraSoft Systems',
  whatsapp: '+92 300 1234567',
  gradeLevel: '10',
  maxStudents: 500,
})

// Subscription
const subscription = ref({
  annualPrice: 999,
  monthlyPrice: 99,
  trialDays: 7,
  autoExpire: true,
  activateOnPayment: false,
})

// Coins
const coins = ref({
  coinRate: 1,
  minQuestions: 50,
  minWithdrawal: 200,
  perfectScoreBonus: 10,
  dailyLoginBonus: 2,
})

// AI
const ai = ref({
  geminiKey: '',
  ttsKey: '',
  defaultLanguage: 'both',
  enableTutor: true,
  enableVideo: true,
  enableGrading: true,
})

// Toast
const toastVisible = ref(false)
let toastTimer = null

function save(section) {
  clearTimeout(toastTimer)
  toastVisible.value = true
  toastTimer = setTimeout(() => { toastVisible.value = false }, 2500)
}
</script>

<style scoped>
/* Toggle pill switch */
.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.toggle-wrap {
  position: relative;
  width: 44px;
  height: 24px;
}

.toggle-wrap input[type="checkbox"] {
  opacity: 0;
  width: 0;
  height: 0;
  position: absolute;
}

.toggle-pill {
  position: absolute;
  inset: 0;
  border-radius: 9999px;
  background: var(--t-border);
  cursor: pointer;
  transition: background 0.2s ease;
}

.toggle-pill::after {
  content: '';
  position: absolute;
  top: 3px;
  left: 3px;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: #fff;
  transition: transform 0.2s ease;
  box-shadow: 0 1px 3px rgba(0,0,0,.2);
}

.toggle-wrap input:checked + .toggle-pill {
  background: var(--t-accent);
}

.toggle-wrap input:checked + .toggle-pill::after {
  transform: translateX(20px);
}

/* Toast transition */
.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from, .toast-leave-to { opacity: 0; transform: translateY(12px); }
</style>
