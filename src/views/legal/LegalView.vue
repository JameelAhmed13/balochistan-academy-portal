<template>
  <div class="lg">
    <div class="lg-aurora" aria-hidden="true"><i class="a1" /><i class="a2" /></div>

    <!-- ── NAV ── -->
    <nav class="lg-nav glass">
      <RouterLink to="/" class="lg-logo">
        <img src="@/assets/logo.png" alt="Balochistan Academy Portal" class="lg-logo-mark" />
        <span class="lg-logo-text">Balochistan Academy<em>&nbsp;Portal</em></span>
      </RouterLink>
      <div class="lg-nav-right">
        <button class="lg-icon-btn" :aria-label="theme.isDark ? 'Switch to light' : 'Switch to dark'" @click="theme.toggle()">
          <Sun v-if="theme.isDark" :size="16" /><Moon v-else :size="16" />
        </button>
        <RouterLink to="/" class="lg-btn lg-btn-ghost"><ArrowLeft :size="15" /> Back to home</RouterLink>
      </div>
    </nav>

    <!-- ── HEADER ── -->
    <header class="lg-head">
      <span class="lg-kicker"><ShieldCheck :size="14" /> Legal</span>
      <h1 class="lg-title">{{ current.title }}</h1>
      <p class="lg-updated">Last updated: 18 June 2026</p>
      <p class="lg-intro">{{ current.intro }}</p>
    </header>

    <!-- ── BODY ── -->
    <main class="lg-body">
      <article class="lg-doc glass">
        <section v-for="(s, i) in current.sections" :key="i" class="lg-sec">
          <h2>{{ s.h }}</h2>
          <template v-for="(b, j) in s.body" :key="j">
            <ul v-if="Array.isArray(b)" class="lg-list">
              <li v-for="(li, k) in b" :key="k"><Check :size="14" /> <span>{{ li }}</span></li>
            </ul>
            <p v-else>{{ b }}</p>
          </template>
        </section>

        <section class="lg-sec lg-contact-block">
          <h2>Questions?</h2>
          <p>For anything related to this policy, reach our team at:</p>
          <div class="lg-contact-rows">
            <a :href="`mailto:${info.email}`" class="lg-crow"><Mail :size="15" /> {{ info.email }}</a>
            <a :href="`tel:${info.phone.replace(/\s/g, '')}`" class="lg-crow"><Phone :size="15" /> {{ info.phone }}</a>
            <div class="lg-crow"><MapPin :size="15" /> {{ info.address }}</div>
          </div>
        </section>
      </article>

      <!-- cross links -->
      <nav class="lg-cross" aria-label="Other policies">
        <RouterLink v-for="d in others" :key="d.path" :to="d.path" class="lg-cross-link">
          {{ d.title }} <ArrowRight :size="13" />
        </RouterLink>
      </nav>
    </main>

    <!-- ── FOOTER ── -->
    <footer class="lg-footer">
      <div class="lg-foot-links">
        <RouterLink :to="{ path: '/', hash: '#about' }">About</RouterLink>
        <RouterLink :to="{ path: '/', hash: '#how' }">How it works</RouterLink>
        <RouterLink :to="{ path: '/', hash: '#features' }">Features</RouterLink>
        <RouterLink to="/login">Login / Register</RouterLink>
      </div>
      <div class="lg-foot-bottom">© 2026 Balochistan Academy Portal · All Rights Reserved · Ultrasoft System</div>
    </footer>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeft, ArrowRight, Check, Mail, Phone, MapPin, ShieldCheck, Sun, Moon } from '@lucide/vue'
import { useThemeStore } from '@/stores/theme'

const theme = useThemeStore()
const route = useRoute()

const info = {
  email: 'info@balochistanacademyportal.com',
  phone: '+92 370 3153540',
  address: 'Ultrasoft System, Technology Hub, Building #1, Baba Fareed Housing Scheme, Airport Road, Quetta',
}

const docs = {
  privacy: {
    path: '/privacy',
    title: 'Privacy Policy',
    intro: 'This Privacy Policy explains how Balochistan Academy Portal, operated by Ultrasoft System, collects, uses, and protects your information when you use our platform.',
    sections: [
      { h: 'Information We Collect', body: [
        'We collect the information needed to give every student a personalised learning experience:',
        ['Account details — name, phone number, grade, board, and institute.',
         'Learning activity — tests taken, scores, study progress, and coins earned.',
         'Usage data — device type, browser, and pages visited to improve the service.'] ] },
      { h: 'How We Use Your Information', body: [
        'Your information is used only to operate and improve the platform — to track your progress, generate reports for students and parents, personalise AI tutoring and practice tests, and provide support. We do not sell your personal data to third parties.'] },
      { h: 'AI Features', body: [
        'Our AI Tutor, paper generation, and grading features process the questions and answers you submit to produce explanations, marks, and feedback. This content is used to deliver the feature and to improve learning quality.'] },
      { h: 'Data Security', body: [
        'We apply reasonable technical and organisational measures to protect your data against unauthorised access, loss, or misuse. Access to personal data is restricted to authorised staff only.'] },
      { h: 'Your Rights', body: [
        'You may request access to, correction of, or deletion of your personal data at any time by contacting us using the details below.'] },
    ],
  },
  refund: {
    path: '/refund',
    title: 'Refund Policy',
    intro: 'We want you to be confident in your subscription to Balochistan Academy Portal. This Refund Policy describes when and how refunds are issued.',
    sections: [
      { h: 'Eligibility', body: [
        'Refund requests are considered under the following conditions:',
        ['The request is made within 7 days of the original payment.',
         'The subscription has not been substantially used (for example, fewer than 3 paid tests or tutoring sessions).',
         'Payment was made through an approved channel and can be verified.'] ] },
      { h: 'Non-Refundable Items', body: [
        'Coins, rewards, and promotional credits that have already been earned or withdrawn are non-refundable. Discounted or promotional subscriptions may be excluded from refunds where stated at the time of purchase.'] },
      { h: 'How to Request a Refund', body: [
        'Email us at info@balochistanacademyportal.com with your registered phone number and payment reference. Approved refunds are processed to the original payment method within 7–14 business days.'] },
    ],
  },
  terms: {
    path: '/terms',
    title: 'Terms & Conditions',
    intro: 'By accessing or using Balochistan Academy Portal, operated by Ultrasoft System, you agree to these Terms & Conditions.',
    sections: [
      { h: 'Use of the Platform', body: [
        'Balochistan Academy Portal provides educational resources, AI tutoring, tests, and competitions for students. You agree to use the platform only for lawful, educational purposes.'] },
      { h: 'Accounts', body: [
        'You are responsible for keeping your login credentials secure and for all activity under your account. Accounts must contain accurate information and may not be shared or sold.'] },
      { h: 'Acceptable Conduct', body: [
        'You agree not to:',
        ['Attempt to cheat, manipulate scores, or disrupt competitions and leaderboards.',
         'Copy, redistribute, or resell platform content without permission.',
         'Upload unlawful, offensive, or infringing material.'] ] },
      { h: 'Intellectual Property', body: [
        'All content, branding, and software on the platform are owned by Ultrasoft System or its licensors and are protected by applicable laws. Curriculum content remains aligned with the respective educational boards.'] },
      { h: 'Limitation of Liability', body: [
        'The platform is provided “as is”. While we strive for accuracy, we do not guarantee that AI-generated content is error-free, and it should be used as a study aid alongside official materials.'] },
    ],
  },
  cancellation: {
    path: '/cancellation',
    title: 'Cancellation Policy',
    intro: 'This Cancellation Policy explains how you can cancel your subscription to Balochistan Academy Portal and what happens after cancellation.',
    sections: [
      { h: 'Cancelling Your Subscription', body: [
        'You may cancel your subscription at any time from your account settings or by contacting our support team. Cancellation stops future renewals.'] },
      { h: 'Access After Cancellation', body: [
        'When you cancel, you keep access to paid features until the end of your current billing period. After that, your account reverts to the free tier.'] },
      { h: 'Auto-Renewal', body: [
        'Subscriptions renew automatically unless cancelled before the renewal date. To avoid the next charge, cancel at least 24 hours before your renewal date.'] },
      { h: 'Refunds on Cancellation', body: [
        'Cancellation does not automatically trigger a refund for the current period. Any refund eligibility is governed by our Refund Policy.'] },
    ],
  },
}

const current = computed(() => docs[route.meta.doc] || docs.privacy)
const others = computed(() => Object.values(docs).filter((d) => d.path !== current.value.path))
</script>

<style scoped>
.lg {
  position: relative; min-height: 100vh; overflow-x: clip;
  background: var(--t-bg); color: var(--t-text1);
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
  /* match the landing + login — cosmic violet → cyan → gold */
  --t-accent:  #7c6af5;
  --t-accent2: #06b6d4;
  --t-yellow:  #fbbf24;
  --t-acc-alpha-sm: rgba(124, 106, 245, 0.10);
  --t-acc-alpha-md: rgba(124, 106, 245, 0.18);
  --t-logo-glow:    rgba(124, 106, 245, 0.40);
}
html.dark .lg {
  --t-aurora1: rgba(124, 106, 245, 0.22);
  --t-aurora2: rgba(6, 182, 212, 0.16);
}
.lg-aurora { position: fixed; inset: 0; z-index: 0; overflow: hidden; pointer-events: none; }
.lg-aurora i { position: absolute; width: 480px; height: 480px; border-radius: 50%; filter: blur(110px); }
.lg-aurora .a1 { top: -160px; left: -120px; background: var(--t-aurora1); }
.lg-aurora .a2 { top: 30%; right: -180px; background: var(--t-aurora2); }

h1, h2 { font-family: 'Space Grotesk', system-ui, sans-serif; }

/* nav */
.lg-nav { position: sticky; top: 12px; margin: 12px 16px 0; z-index: 50; border-radius: 16px; display: flex; align-items: center; justify-content: space-between; padding: 10px 16px; border: 1px solid var(--t-glass-border); }
.lg-logo { display: inline-flex; align-items: center; gap: 9px; text-decoration: none; }
.lg-logo-mark { width: 36px; height: 36px; border-radius: 10px; object-fit: contain; }
.lg-logo-text { font-family: 'Space Grotesk'; font-weight: 700; font-size: 15px; color: var(--t-text1); white-space: nowrap; }
.lg-logo-text em { font-style: normal; color: var(--t-accent); }
.lg-nav-right { display: flex; align-items: center; gap: 8px; }
.lg-icon-btn { display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; border: 1px solid var(--t-border); background: var(--t-surface); color: var(--t-text2); cursor: pointer; transition: color 0.2s, border-color 0.2s; }
.lg-icon-btn:hover { color: var(--t-accent); border-color: var(--t-accent); }
.lg-btn { display: inline-flex; align-items: center; gap: 7px; padding: 9px 16px; border-radius: 11px; font-size: 13.5px; font-weight: 700; text-decoration: none; cursor: pointer; border: 1px solid transparent; transition: transform 0.2s, border-color 0.2s, color 0.2s; }
.lg-btn-ghost { color: var(--t-text1); border-color: var(--t-border); background: var(--t-surface); }
.lg-btn-ghost:hover { border-color: var(--t-accent); color: var(--t-accent); }
@media (max-width: 600px) { .lg-logo-text { display: none; } }

/* header */
.lg-head { position: relative; z-index: 2; max-width: 820px; margin: 0 auto; padding: 56px 24px 24px; text-align: center; }
.lg-kicker { display: inline-flex; align-items: center; gap: 7px; font-size: 12.5px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: var(--t-accent); margin-bottom: 14px; }
.lg-title { font-weight: 700; font-size: clamp(2rem, 4vw, 3rem); line-height: 1.05; letter-spacing: -0.025em; margin: 0 0 10px; }
.lg-updated { font-size: 13px; color: var(--t-text3); margin: 0 0 18px; }
.lg-intro { font-size: clamp(1rem, 1.4vw, 1.1rem); line-height: 1.7; color: var(--t-text2); max-width: 640px; margin: 0 auto; }

/* body */
.lg-body { position: relative; z-index: 2; max-width: 820px; margin: 0 auto; padding: 8px 24px 40px; }
.lg-doc { border-radius: 22px; padding: clamp(1.5rem, 4vw, 2.75rem); }
.lg-sec { margin-bottom: 1.9rem; }
.lg-sec:last-child { margin-bottom: 0; }
.lg-sec h2 { font-size: 1.25rem; font-weight: 700; color: var(--t-text1); margin: 0 0 0.7rem; }
.lg-sec p { font-size: 0.95rem; line-height: 1.75; color: var(--t-text2); margin: 0 0 0.8rem; }
.lg-sec p:last-child { margin-bottom: 0; }
.lg-list { list-style: none; padding: 0; margin: 0.3rem 0 0.4rem; display: flex; flex-direction: column; gap: 0.55rem; }
.lg-list li { display: flex; gap: 0.6rem; align-items: flex-start; font-size: 0.92rem; line-height: 1.6; color: var(--t-text2); }
.lg-list li :deep(svg) { color: var(--t-accent); flex-shrink: 0; margin-top: 0.2rem; }

.lg-contact-block { border-top: 1px solid var(--t-border); padding-top: 1.6rem; }
.lg-contact-rows { display: flex; flex-direction: column; gap: 0.6rem; margin-top: 0.5rem; }
.lg-crow { display: inline-flex; align-items: center; gap: 0.55rem; font-size: 0.9rem; color: var(--t-text2); text-decoration: none; transition: color 0.2s; }
.lg-crow:hover { color: var(--t-accent); }
.lg-crow :deep(svg) { color: var(--t-accent); flex-shrink: 0; }

/* cross links */
.lg-cross { display: flex; flex-wrap: wrap; gap: 0.6rem; margin-top: 1.4rem; }
.lg-cross-link { display: inline-flex; align-items: center; gap: 0.4rem; padding: 0.55rem 1rem; border-radius: 99px; font-size: 0.82rem; font-weight: 600; color: var(--t-text2); text-decoration: none; background: var(--t-surface); border: 1px solid var(--t-border); transition: all 0.18s; }
.lg-cross-link:hover { color: var(--t-accent); border-color: var(--t-accent); transform: translateY(-1px); }

/* footer */
.lg-footer { position: relative; z-index: 2; border-top: 1px solid var(--t-border); margin-top: 32px; padding: 28px 24px; text-align: center; }
.lg-foot-links { display: flex; flex-wrap: wrap; justify-content: center; gap: 18px; margin-bottom: 14px; }
.lg-foot-links a { font-size: 13px; font-weight: 600; color: var(--t-text2); text-decoration: none; transition: color 0.2s; }
.lg-foot-links a:hover { color: var(--t-accent); }
.lg-foot-bottom { font-size: 12.5px; color: var(--t-text3); }
</style>
