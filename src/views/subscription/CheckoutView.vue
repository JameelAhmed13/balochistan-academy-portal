<template>
  <div class="co">
    <button type="button" class="co-back" @click="$router.back()"><ArrowLeft :size="16" /> Back</button>

    <div class="co-grid">
      <!-- Payment panel -->
      <section class="card co-pay">
        <h1 class="co-title">Activate Full Access</h1>
        <p class="co-sub">Secure checkout · Balochistan Academy Portal</p>

        <template v-if="status !== 'done'">
          <div class="co-methods" role="tablist" aria-label="Payment method">
            <button v-for="m in methods" :key="m.id" type="button" role="tab" :aria-selected="method === m.id"
              class="co-method" :class="{ on: method === m.id }" @click="method = m.id">
              <span class="co-method-ic">{{ m.icon }}</span> {{ m.label }}
            </button>
          </div>

          <form class="co-form" @submit.prevent="pay">
            <template v-if="method === 'card'">
              <div>
                <label class="label">Cardholder Name</label>
                <input v-model.trim="card.name" class="input" type="text" placeholder="Name on card" required />
              </div>
              <div>
                <label class="label">Card Number</label>
                <input v-model="card.number" class="input" inputmode="numeric" maxlength="19" placeholder="4242 4242 4242 4242" required />
              </div>
              <div class="co-row">
                <div>
                  <label class="label">Expiry</label>
                  <input v-model="card.exp" class="input" maxlength="5" placeholder="MM/YY" required />
                </div>
                <div>
                  <label class="label">CVC</label>
                  <input v-model="card.cvc" class="input" inputmode="numeric" maxlength="4" placeholder="123" required />
                </div>
              </div>
            </template>

            <template v-else>
              <div>
                <label class="label">{{ method === 'easypaisa' ? 'Easypaisa' : 'JazzCash' }} Mobile Number</label>
                <input v-model.trim="wallet.phone" class="input" inputmode="tel" placeholder="03xx-xxxxxxx" required />
              </div>
              <p class="co-note">
                You'll get a prompt in your {{ method === 'easypaisa' ? 'Easypaisa' : 'JazzCash' }} app to approve
                <strong>Rs. {{ plan.price }}</strong>.
              </p>
            </template>

            <button type="submit" class="btn-primary co-pay-btn" :disabled="status === 'processing'">
              <span v-if="status === 'processing'" class="co-spin" />
              {{ status === 'processing' ? 'Processing…' : `Pay Rs. ${plan.price}` }}
            </button>
            <p class="co-secure"><Lock :size="13" /> 256-bit encrypted · demo gateway (no real charge)</p>
          </form>
        </template>

        <div v-else class="co-success">
          <div class="co-success-badge"><Check :size="34" /></div>
          <h2 class="co-success-title">Payment Successful!</h2>
          <p class="co-success-sub">Your Full Access subscription is active until <strong>{{ renewDate }}</strong>.</p>
          <RouterLink to="/app/hub" class="btn-primary">Start Learning →</RouterLink>
        </div>
      </section>

      <!-- Order summary -->
      <aside class="card co-summary">
        <div class="co-plan-name">Full Access</div>
        <div class="co-price"><b>Rs. {{ plan.price }}</b><span>/ year</span></div>
        <ul class="co-feats">
          <li v-for="f in plan.features" :key="f"><Check :size="14" /> <span>{{ f }}</span></li>
        </ul>
        <div class="co-total"><span>Total due today</span><b>Rs. {{ plan.price }}</b></div>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { ArrowLeft, Check, Lock } from '@lucide/vue'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '@/stores/auth'

const toast = useToast()
const auth = useAuthStore()

const plan = {
  price: 999,
  features: [
    'Unlimited AI Tutor sessions',
    'Full question bank access',
    'AI-graded subjective answers',
    'Coin rewards & withdrawals',
    'All daily & competition tests',
  ],
}
const methods = [
  { id: 'card', label: 'Card', icon: '💳' },
  { id: 'easypaisa', label: 'Easypaisa', icon: '📱' },
  { id: 'jazzcash', label: 'JazzCash', icon: '📲' },
]
const method = ref('card')
const card = reactive({ name: '', number: '', exp: '', cvc: '' })
const wallet = reactive({ phone: '' })
const status = ref('idle') // idle | processing | done

const renewDate = computed(() =>
  new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toLocaleDateString('en-PK', { dateStyle: 'medium' }),
)

function pay() {
  status.value = 'processing'
  // Demo gateway: simulate processing, then activate.
  setTimeout(() => {
    status.value = 'done'
    localStorage.setItem('bap_subscribed', '1')
    toast.add({ severity: 'success', summary: 'Subscription activated', detail: `Welcome to Full Access, ${auth.user?.name || 'student'}!`, life: 3500 })
  }, 1600)
}
</script>

<style scoped>
.co { max-width: 980px; margin: 0 auto; padding: 8px 4px 40px; }
.co-back {
  display: inline-flex; align-items: center; gap: 0.4rem; margin-bottom: 1rem;
  padding: 0.45rem 0.9rem; border-radius: 99px; background: var(--t-hover);
  border: 1px solid var(--t-border); color: var(--t-text2); font-size: 0.82rem; font-weight: 600; cursor: pointer;
  transition: all 0.15s;
}
.co-back:hover { background: var(--t-hover2); color: var(--t-text1); }

.co-grid { display: grid; grid-template-columns: 1.5fr 1fr; gap: 1.25rem; align-items: start; }
@media (max-width: 780px) { .co-grid { grid-template-columns: 1fr; } }

.co-pay { padding: 1.75rem; }
.co-title { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); margin: 0; }
.co-sub { font-size: 0.82rem; color: var(--t-text3); margin: 0.25rem 0 1.25rem; }

.co-methods { display: flex; gap: 0.5rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
.co-method {
  flex: 1; min-width: 92px; display: inline-flex; align-items: center; justify-content: center; gap: 0.45rem;
  padding: 0.7rem 0.5rem; border-radius: 12px; cursor: pointer; font-size: 0.85rem; font-weight: 600;
  background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2); transition: all 0.15s;
}
.co-method:hover { border-color: var(--t-accent); color: var(--t-text1); }
.co-method.on {
  border-color: var(--t-accent); color: var(--t-accent);
  background: color-mix(in srgb, var(--t-accent) 10%, transparent);
}
.co-method-ic { font-size: 1.1rem; }

.co-form { display: flex; flex-direction: column; gap: 1rem; }
.co-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.co-note { font-size: 0.8rem; color: var(--t-text2); margin: 0; line-height: 1.5; }
.co-pay-btn { width: 100%; justify-content: center; margin-top: 0.25rem; padding: 0.85rem; font-size: 0.95rem; }
.co-secure { display: flex; align-items: center; justify-content: center; gap: 0.4rem; font-size: 0.74rem; color: var(--t-text3); margin: 0.25rem 0 0; }
.co-spin { width: 15px; height: 15px; border-radius: 50%; border: 2px solid rgba(255,255,255,0.4); border-top-color: #fff; animation: coSpin 0.7s linear infinite; }
@keyframes coSpin { to { transform: rotate(360deg); } }

.co-success { text-align: center; padding: 1.5rem 0.5rem; display: flex; flex-direction: column; align-items: center; gap: 0.5rem; }
.co-success-badge {
  width: 72px; height: 72px; border-radius: 50%; display: grid; place-items: center; color: #fff;
  background: linear-gradient(135deg, var(--t-success), #10b981); margin-bottom: 0.5rem;
  box-shadow: 0 0 30px color-mix(in srgb, var(--t-success) 40%, transparent);
}
.co-success-title { font-size: 1.4rem; font-weight: 800; color: var(--t-text1); margin: 0; }
.co-success-sub { font-size: 0.88rem; color: var(--t-text2); margin: 0 0 0.75rem; }

.co-summary { padding: 1.5rem; position: sticky; top: 84px; }
.co-plan-name { font-size: 0.72rem; font-weight: 800; letter-spacing: 0.1em; text-transform: uppercase; color: var(--t-accent); }
.co-price { display: flex; align-items: baseline; gap: 0.35rem; margin: 0.35rem 0 1rem; }
.co-price b { font-size: 2rem; font-weight: 800; color: var(--t-text1); }
.co-price span { font-size: 0.85rem; color: var(--t-text3); }
.co-feats { list-style: none; padding: 0; margin: 0 0 1rem; display: flex; flex-direction: column; gap: 0.6rem; }
.co-feats li { display: flex; align-items: flex-start; gap: 0.5rem; font-size: 0.85rem; color: var(--t-text2); line-height: 1.45; }
.co-feats li :deep(svg) { color: var(--t-accent); flex-shrink: 0; margin-top: 0.15rem; }
.co-total { display: flex; align-items: center; justify-content: space-between; padding-top: 1rem; border-top: 1px solid var(--t-border); }
.co-total span { font-size: 0.85rem; color: var(--t-text3); }
.co-total b { font-size: 1.1rem; font-weight: 800; color: var(--t-text1); }
</style>
