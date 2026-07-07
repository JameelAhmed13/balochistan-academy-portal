<template>
  <div class="co">
    <button type="button" class="co-back" @click="$router.back()"><ArrowLeft :size="16" /> Back</button>

    <div class="co-grid">
      <!-- Payment panel -->
      <section class="card co-pay">
        <template v-if="student.pendingCashPurchase">
          <div class="co-pending">
            <div class="co-pending-badge">⏳</div>
            <h2 class="co-pending-title">Payment Submitted</h2>
            <p class="co-pending-sub">
              Your payment for <strong>{{ student.pendingCashPurchase.planName }}</strong> via
              {{ methodLabel(student.pendingCashPurchase.paymentMethod) }}
              (ref: {{ student.pendingCashPurchase.referenceNumber }}) is awaiting verification.
              We'll activate your subscription as soon as it's confirmed.
            </p>
            <RouterLink to="/app/hub" class="btn-primary">Back to Dashboard</RouterLink>
          </div>
        </template>

        <template v-else-if="status === 'done'">
          <div class="co-success">
            <div class="co-success-badge"><Check :size="34" /></div>
            <h2 class="co-success-title">Payment Submitted!</h2>
            <p class="co-success-sub">
              We'll verify your {{ methodLabel(method) }} payment and activate
              <strong>{{ selectedPlan?.name }}</strong> shortly.
            </p>
            <RouterLink to="/app/hub" class="btn-primary">Back to Dashboard</RouterLink>
          </div>
        </template>

        <template v-else>
          <h1 class="co-title">Activate a Subscription</h1>
          <p class="co-sub">Pay via mobile wallet or bank transfer, then submit your reference for verification</p>

          <div v-if="loadingPlans" class="co-note">Loading plans…</div>
          <div v-else-if="!student.plans.length" class="co-note">No subscription plans are available right now.</div>
          <form v-else class="co-form" @submit.prevent="pay">
            <div>
              <label class="label">Select Plan</label>
              <select v-model.number="planId" class="input">
                <option v-for="p in student.plans" :key="p.id" :value="p.id">
                  {{ p.name }} — Rs. {{ p.price.toLocaleString() }} / {{ p.durationDays }} days
                </option>
              </select>
            </div>

            <div class="co-methods" role="tablist" aria-label="Payment method">
              <button v-for="m in methods" :key="m.id" type="button" role="tab" :aria-selected="method === m.id"
                class="co-method" :class="{ on: method === m.id }" @click="method = m.id">
                <span class="co-method-ic">{{ m.icon }}</span> {{ m.label }}
              </button>
            </div>

            <p class="co-note">
              Send <strong>Rs. {{ (selectedPlan?.price ?? 0).toLocaleString() }}</strong> via {{ methodLabel(method) }}
              to the account shared by your institute admin, then enter the transaction reference below.
            </p>

            <div>
              <label class="label">Transaction / Reference Number</label>
              <input v-model.trim="referenceNumber" class="input" type="text" placeholder="e.g. TXN123456789" required />
            </div>

            <button type="submit" class="btn-primary co-pay-btn" :disabled="status === 'processing' || !planId || !referenceNumber">
              <span v-if="status === 'processing'" class="co-spin" />
              {{ status === 'processing' ? 'Submitting…' : 'Submit Payment for Verification' }}
            </button>
            <p class="co-secure"><Lock :size="13" /> Manually verified by our team — no card details needed</p>
          </form>
        </template>
      </section>

      <!-- Order summary -->
      <aside class="card co-summary">
        <div class="co-plan-name">{{ selectedPlan?.name || 'Subscription' }}</div>
        <div class="co-price">
          <b>Rs. {{ (selectedPlan?.price ?? 0).toLocaleString() }}</b>
          <span>/ {{ selectedPlan?.durationDays ?? 0 }} days</span>
        </div>
        <ul class="co-feats">
          <li><Check :size="14" /> <span>{{ (selectedPlan?.aiTokenQuota ?? 0).toLocaleString() }} AI tutor tokens</span></li>
          <li><Check :size="14" /> <span>Full question bank access</span></li>
          <li><Check :size="14" /> <span>All daily &amp; competition tests</span></li>
          <li><Check :size="14" /> <span>Coin rewards — redeem for renewals &amp; extra AI tokens</span></li>
        </ul>
        <div class="co-total"><span>Total due</span><b>Rs. {{ (selectedPlan?.price ?? 0).toLocaleString() }}</b></div>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ArrowLeft, Check, Lock } from '@lucide/vue'
import { useToast } from 'primevue/usetoast'
import { useStudentStore } from '@/stores/student'

const toast = useToast()
const student = useStudentStore()

const methods = [
  { id: 'Easypaisa', label: 'Easypaisa', icon: '📱' },
  { id: 'JazzCash', label: 'JazzCash', icon: '📲' },
  { id: 'BankTransfer', label: 'Bank Transfer', icon: '🏦' },
]
const method = ref('Easypaisa')
const planId = ref(null)
const referenceNumber = ref('')
const status = ref('idle') // idle | processing | done
const loadingPlans = ref(false)

const selectedPlan = computed(() => student.plans.find(p => p.id === planId.value))

function methodLabel(id) {
  return methods.find(m => m.id === id)?.label ?? id
}

onMounted(async () => {
  loadingPlans.value = true
  try {
    await Promise.allSettled([student.fetchPlans(), student.fetchSubscription()])
    if (student.plans.length) planId.value = student.plans[0].id
  } finally {
    loadingPlans.value = false
  }
})

async function pay() {
  status.value = 'processing'
  try {
    await student.purchaseCash(planId.value, method.value, referenceNumber.value)
    status.value = 'done'
    toast.add({ severity: 'success', summary: 'Payment submitted', detail: "We'll verify and activate your subscription shortly.", life: 4000 })
  } catch (e) {
    status.value = 'idle'
    toast.add({ severity: 'error', summary: 'Could not submit payment', detail: e.response?.data?.error || e.message, life: 4000 })
  }
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

.co-methods { display: flex; gap: 0.5rem; margin-bottom: 0.25rem; flex-wrap: wrap; }
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
.co-note { font-size: 0.8rem; color: var(--t-text2); margin: 0; line-height: 1.5; }
.co-pay-btn { width: 100%; justify-content: center; margin-top: 0.25rem; padding: 0.85rem; font-size: 0.95rem; }
.co-secure { display: flex; align-items: center; justify-content: center; gap: 0.4rem; font-size: 0.74rem; color: var(--t-text3); margin: 0.25rem 0 0; }
.co-spin { width: 15px; height: 15px; border-radius: 50%; border: 2px solid rgba(255,255,255,0.4); border-top-color: #fff; animation: coSpin 0.7s linear infinite; }
@keyframes coSpin { to { transform: rotate(360deg); } }

.co-success, .co-pending { text-align: center; padding: 1.5rem 0.5rem; display: flex; flex-direction: column; align-items: center; gap: 0.5rem; }
.co-success-badge {
  width: 72px; height: 72px; border-radius: 50%; display: grid; place-items: center; color: #fff;
  background: linear-gradient(135deg, var(--t-success), #10b981); margin-bottom: 0.5rem;
  box-shadow: 0 0 30px color-mix(in srgb, var(--t-success) 40%, transparent);
}
.co-pending-badge { font-size: 2.5rem; margin-bottom: 0.25rem; }
.co-success-title, .co-pending-title { font-size: 1.4rem; font-weight: 800; color: var(--t-text1); margin: 0; }
.co-success-sub, .co-pending-sub { font-size: 0.88rem; color: var(--t-text2); margin: 0 0 0.75rem; max-width: 420px; line-height: 1.5; }

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
