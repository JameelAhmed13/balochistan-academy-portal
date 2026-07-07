<template>
  <div class="rd animate-fade-in">
    <button type="button" class="rd-back" @click="$router.back()"><ArrowLeft :size="16" /> Back</button>

    <div class="rd-head">
      <h1 class="rd-title">Redeem Coins</h1>
      <div class="rd-balance">
        <span class="rd-balance-label">Your Balance</span>
        <span class="rd-balance-value">{{ student.totalCoins.toLocaleString() }} 🪙</span>
      </div>
    </div>

    <!-- Tabs -->
    <div class="rd-tabs" role="tablist">
      <button v-for="t in tabs" :key="t.id" type="button" role="tab" :aria-selected="tab === t.id"
        class="rd-tab" :class="{ on: tab === t.id }" @click="tab = t.id">
        {{ t.label }}
      </button>
    </div>

    <!-- ── Buy New Subscription ── -->
    <section v-if="tab === 'buy'" class="rd-panel">
      <div v-if="student.hasActiveSubscription" class="rd-empty">
        You already have an active subscription ({{ student.subscription.planName }}).
        <RouterLink to="#" @click.prevent="tab = 'renew'" class="rd-link">Renew it instead →</RouterLink>
      </div>
      <div v-else-if="loadingPlans" class="rd-empty">Loading plans…</div>
      <div v-else-if="!student.plans.length" class="rd-empty">No subscription plans are available right now.</div>
      <div v-else class="rd-plan-grid">
        <div v-for="p in student.plans" :key="p.id" class="card rd-plan" :class="{ afford: student.totalCoins >= p.coinCost }">
          <div class="rd-plan-name">{{ p.name }}</div>
          <div class="rd-plan-price">Rs. {{ p.price.toLocaleString() }} <small>/ {{ p.durationDays }} days</small></div>
          <p v-if="p.description" class="rd-plan-desc" v-html="p.description"></p>
          <div class="rd-plan-meta"><span>AI Tokens</span><b>{{ p.aiTokenQuota.toLocaleString() }}</b></div>
          <div class="rd-plan-cost">{{ p.coinCost.toLocaleString() }} coins</div>
          <button type="button" class="btn-primary w-full justify-center" :disabled="student.totalCoins < p.coinCost || buying"
            @click="buy(p)">
            {{ student.totalCoins >= p.coinCost ? `Redeem for ${p.coinCost.toLocaleString()} coins` : `Need ${(p.coinCost - student.totalCoins).toLocaleString()} more coins` }}
          </button>
        </div>
      </div>
    </section>

    <!-- ── Renew Current Plan ── -->
    <section v-else-if="tab === 'renew'" class="rd-panel">
      <div v-if="!student.subscription" class="rd-empty">
        You don't have a subscription to renew yet. <RouterLink to="#" @click.prevent="tab = 'buy'" class="rd-link">Buy one first →</RouterLink>
      </div>
      <div v-else class="card rd-renew">
        <div class="rd-renew-info">
          <span class="k">Current Plan</span>
          <span class="v">{{ student.subscription.planName }} · {{ student.subscription.status === 'active' ? 'expires' : 'expired' }} {{ formatDate(student.subscription.endDate) }}</span>
        </div>
        <div class="rd-arrow">→</div>
        <div class="rd-renew-info">
          <span class="k">After Renewal</span>
          <span class="v">Extends {{ student.subscription.status === 'active' ? 'from current end date' : 'from today' }} · tokens reset to {{ student.subscription.aiTokenQuota }}</span>
        </div>
        <button type="button" class="btn-primary" :disabled="renewCost == null || student.totalCoins < renewCost || renewing" @click="renew">
          {{ renewCost == null ? 'Loading cost…' : (student.totalCoins >= renewCost ? `Renew for ${renewCost.toLocaleString()} coins` : `Need ${(renewCost - student.totalCoins).toLocaleString()} more coins`) }}
        </button>
      </div>
    </section>

    <!-- ── Buy AI Tokens ── -->
    <section v-else class="rd-panel">
      <div class="card rd-tokens">
        <div class="rd-token-stats">
          <div class="rd-token-stat">
            <span class="k">This Cycle</span>
            <span class="v">{{ student.subscription?.tokensRemainingThisPeriod ?? 0 }} <small>/ {{ student.subscription?.aiTokenQuota ?? 0 }} left</small></span>
          </div>
          <div class="rd-token-stat">
            <span class="k">Bonus (never expires)</span>
            <span class="v bonus">{{ student.bonusAiTokens }}</span>
          </div>
        </div>
        <div class="rd-stepper">
          <button type="button" @click="tokenQty = Math.max(5, tokenQty - 5)">−</button>
          <span class="val">{{ tokenQty }}</span>
          <button type="button" @click="tokenQty += 5">+</button>
        </div>
        <button type="button" class="btn-primary w-full justify-center" :disabled="tokenCost == null || student.totalCoins < tokenCost || buyingTokens" @click="buyTokensNow">
          {{ tokenCost == null ? 'Loading cost…' : `Buy ${tokenQty} tokens for ${tokenCost.toLocaleString()} coins` }}
        </button>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { ArrowLeft } from '@lucide/vue'
import { useToast } from 'primevue/usetoast'
import { useStudentStore } from '@/stores/student'

const student = useStudentStore()
const toast = useToast()

const tabs = [
  { id: 'buy',    label: 'Buy New Subscription' },
  { id: 'renew',  label: 'Renew Current Plan' },
  { id: 'tokens', label: 'Buy AI Tokens' },
]
const tab = ref('buy')

const loadingPlans = ref(false)
const buying       = ref(false)
const renewing     = ref(false)
const buyingTokens = ref(false)

const tokenQty  = ref(25)
const renewCost = computed(() => {
  const plan = student.plans.find(p => p.id === student.subscription?.planId)
  return plan?.coinCost ?? null
})
const tokenCost = computed(() => Math.ceil(tokenQty.value * student.coinsPerAiToken))

onMounted(async () => {
  loadingPlans.value = true
  try {
    await Promise.allSettled([student.fetchPlans(), student.fetchSubscription(), student.fetchBalance()])
    if (student.hasActiveSubscription) tab.value = 'renew'
  } finally {
    loadingPlans.value = false
  }
})

// Renew tab needs the current plan's coin cost, which only comes from the /plans list —
// make sure plans are loaded even if the user lands directly on the renew tab.
watch(tab, (t) => { if (t === 'renew' && !student.plans.length) student.fetchPlans() })

async function buy(plan) {
  buying.value = true
  try {
    await student.purchaseSubscription(plan.id)
    toast.add({ severity: 'success', summary: 'Subscription activated', detail: `You're now on ${plan.name}.`, life: 4000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not redeem', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    buying.value = false
  }
}

async function renew() {
  renewing.value = true
  try {
    await student.renewSubscription()
    toast.add({ severity: 'success', summary: 'Subscription renewed', life: 4000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not renew', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    renewing.value = false
  }
}

async function buyTokensNow() {
  buyingTokens.value = true
  try {
    await student.buyTokens(tokenQty.value)
    toast.add({ severity: 'success', summary: `+${tokenQty.value} AI tokens added`, life: 4000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Could not buy tokens', detail: e.response?.data?.error || e.message, life: 4000 })
  } finally {
    buyingTokens.value = false
  }
}

function formatDate(iso) { return new Date(iso).toLocaleDateString('en-PK', { timeZone: 'Asia/Karachi', day: 'numeric', month: 'short', year: 'numeric' }) }
</script>

<style scoped>
.rd { max-width: 980px; margin: 0 auto; padding: 8px 4px 40px; }
.rd-back {
  display: inline-flex; align-items: center; gap: 0.4rem; margin-bottom: 1rem;
  padding: 0.45rem 0.9rem; border-radius: 99px; background: var(--t-hover);
  border: 1px solid var(--t-border); color: var(--t-text2); font-size: 0.82rem; font-weight: 600; cursor: pointer;
  transition: all 0.15s;
}
.rd-back:hover { background: var(--t-hover2); color: var(--t-text1); }

.rd-head { display: flex; align-items: center; justify-content: space-between; gap: 1rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
.rd-title { font-size: 1.5rem; font-weight: 800; color: var(--t-text1); margin: 0; }
.rd-balance { display: flex; flex-direction: column; align-items: flex-end; gap: 0.1rem; }
.rd-balance-label { font-size: 0.72rem; color: var(--t-text3); text-transform: uppercase; letter-spacing: 0.05em; }
.rd-balance-value { font-size: 1.25rem; font-weight: 800; color: var(--t-gold, #d97706); }

.rd-tabs { display: flex; gap: 0.5rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
.rd-tab {
  padding: 0.55rem 1rem; border-radius: 10px; cursor: pointer; font-size: 0.85rem; font-weight: 600;
  background: var(--t-surface); border: 1px solid var(--t-border); color: var(--t-text2); transition: all 0.15s;
}
.rd-tab:hover { border-color: var(--t-accent); color: var(--t-text1); }
.rd-tab.on { border-color: var(--t-accent); color: var(--t-accent); background: color-mix(in srgb, var(--t-accent) 10%, transparent); }

.rd-empty { padding: 2.5rem 1rem; text-align: center; color: var(--t-text3); font-size: 0.9rem; }
.rd-link { color: var(--t-accent); font-weight: 600; }

.rd-plan-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; }
@media (max-width: 820px) { .rd-plan-grid { grid-template-columns: 1fr 1fr; } }
@media (max-width: 560px) { .rd-plan-grid { grid-template-columns: 1fr; } }
.rd-plan { padding: 1.25rem; display: flex; flex-direction: column; gap: 0.6rem; }
.rd-plan.afford { border-color: color-mix(in srgb, #10b981 40%, var(--t-border)); }
.rd-plan-name { font-size: 1rem; font-weight: 700; color: var(--t-text1); }
.rd-plan-price { font-size: 1.25rem; font-weight: 800; color: var(--t-text1); }
.rd-plan-price small { font-size: 0.75rem; font-weight: 600; color: var(--t-text3); }
.rd-plan-desc { font-size: 0.8rem; color: var(--t-text2); line-height: 1.5; margin: 0; }
.rd-plan-meta { display: flex; justify-content: space-between; font-size: 0.8rem; color: var(--t-text2); }
.rd-plan-cost {
  padding: 0.5rem; border-radius: 8px; text-align: center; font-weight: 700; font-size: 0.85rem;
  background: color-mix(in srgb, var(--t-gold, #d97706) 12%, transparent); color: var(--t-gold, #d97706);
}

.rd-renew { padding: 1.5rem; display: flex; align-items: center; justify-content: space-between; gap: 1.25rem; flex-wrap: wrap; }
.rd-renew-info { display: flex; flex-direction: column; gap: 0.2rem; }
.rd-renew-info .k { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.05em; color: var(--t-text3); font-weight: 700; }
.rd-renew-info .v { font-size: 0.88rem; font-weight: 600; color: var(--t-text1); }
.rd-arrow { color: var(--t-text3); font-size: 1.1rem; }

.rd-tokens { padding: 1.5rem; display: flex; flex-direction: column; gap: 1.25rem; }
.rd-token-stats { display: flex; gap: 1.5rem; flex-wrap: wrap; }
.rd-token-stat { display: flex; flex-direction: column; gap: 0.2rem; }
.rd-token-stat .k { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.05em; color: var(--t-text3); font-weight: 700; }
.rd-token-stat .v { font-size: 1.35rem; font-weight: 800; color: var(--t-text1); }
.rd-token-stat .v.bonus { color: #10b981; }
.rd-token-stat .v small { font-size: 0.8rem; color: var(--t-text3); font-weight: 600; }
.rd-stepper { display: flex; align-items: center; gap: 0.75rem; }
.rd-stepper button {
  width: 34px; height: 34px; border-radius: 9px; border: 1px solid var(--t-border); background: var(--t-surface);
  font-size: 1.1rem; font-weight: 700; color: var(--t-text1); cursor: pointer;
}
.rd-stepper .val { font-size: 1.1rem; font-weight: 800; min-width: 32px; text-align: center; color: var(--t-text1); }
</style>
