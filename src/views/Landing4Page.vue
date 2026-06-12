<template>
  <div ref="pageEl" class="l2 l4" @mousemove="onMouseMove">

    <!-- ── atmosphere ── -->
    <canvas ref="bgCanvas" class="l3-canvas" aria-hidden="true" />
    <div class="l2-aurora" aria-hidden="true"><i class="a1" /><i class="a2" /><i class="a3" /></div>
    <div class="l2-spotlight" :style="{ background: `radial-gradient(480px circle at ${mx}px ${my}px, rgba(124,106,245,0.09), transparent 65%)` }" />

    <!-- ── Scroll-drawn journey line ── -->
    <svg class="l2-line-svg" aria-hidden="true" :width="svgW" :height="svgH" :viewBox="`0 0 ${svgW} ${svgH}`" fill="none">
      <defs>
        <linearGradient id="l4-line-grad" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" :y2="svgH">
          <stop offset="0%" stop-color="#8a7fff" />
          <stop offset="45%" stop-color="#7c6af5" />
          <stop offset="75%" stop-color="#fbbf24" />
          <stop offset="100%" stop-color="#f59e0b" />
        </linearGradient>
        <filter id="l4-line-glow" x="-50%" y="-50%" width="200%" height="200%">
          <feGaussianBlur stdDeviation="6" result="blur" />
          <feMerge><feMergeNode in="blur" /><feMergeNode in="SourceGraphic" /></feMerge>
        </filter>
      </defs>
      <path :d="pathD" class="l2-path-ghost" />
      <path ref="lineEl" :d="pathD" class="l2-path-main" filter="url(#l4-line-glow)" :stroke-dasharray="pathLen" :stroke-dashoffset="dashOffset" style="stroke: url(#l4-line-grad)" />
    </svg>

    <div v-show="tipVisible" class="l2-tip" :style="{ transform: `translate(${tip.x}px, ${tip.y}px)` }">
      <span class="l2-tip-core" /><span class="l2-tip-halo" />
    </div>

    <!-- ── NAV ── -->
    <nav class="l2-nav" :class="{ scrolled: scrolled }">
      <RouterLink to="/" class="l2-logo">
        <span class="l2-logo-mark">eS</span>
        <span class="l2-logo-text">Balochistan Academy<em>·Mobile</em></span>
      </RouterLink>
      <div class="l2-nav-right">
        <a href="#path" class="l2-nav-lnk">The Path</a>
        <a href="#pricing" class="l2-nav-lnk">Pricing</a>
        <a href="#join" class="l2-nav-lnk">Join</a>
        <button class="l2-nav-theme" :aria-label="themeStore.isDark ? 'Switch to light mode' : 'Switch to dark mode'" @click="themeStore.toggle()">
          <Sun v-if="themeStore.isDark" :size="15" />
          <Moon v-else :size="15" />
        </button>
        <RouterLink to="/login" class="l2-nav-cta">Start Free <ArrowRight :size="15" /></RouterLink>
      </div>
    </nav>

    <!-- ── HERO — copy left · tilting iPhone right ── -->
    <header class="l4-hero">
      <div class="l4-hero-grid">
        <div class="l4-hero-copy">
          <p class="l2-kicker reveal"><Smartphone :size="14" /> The whole academy · in your pocket</p>
          <Transition name="slide-txt" mode="out-in">
            <div :key="currentSlide" class="l4-htext">
              <h1 class="l4-h1">
                <span v-for="(line, li) in slideData[currentSlide].heading" :key="li"
                  class="l4-h1-line" :class="{ gold: li === slideData[currentSlide].goldLine }">{{ line }}</span>
              </h1>
              <p class="l4-hero-p">{{ slideData[currentSlide].desc }}</p>
            </div>
          </Transition>
          <div class="l2-hero-btns reveal" style="justify-content: flex-start; margin-bottom: 44px;">
            <RouterLink to="/login" class="l2-btn-primary">Start Learning Free <ArrowRight :size="16" /></RouterLink>
            <a href="#path" class="l2-btn-ghost"><ArrowDown :size="16" /> Trace the path</a>
          </div>
          <div class="l4-hstats reveal">
            <div v-for="s in heroStats" :key="s.label" class="l4-hstat">
              <span class="l4-hstat-v">{{ s.val }}</span>
              <span class="l4-hstat-l">{{ s.label }}</span>
            </div>
          </div>
        </div>

        <!-- hero iPhone with 3D tilt + rotating app scenes -->
        <div class="l4-slider-wrap reveal">
          <div ref="sliderRef" class="l4-scene" @mousemove="onSliderMouseMove" @mouseleave="onSliderMouseLeave" @mouseenter="sliderHovering = true">
            <div class="l4-tilt" :style="tiltStyle">
              <IPhone size="lg">
                <Transition :name="s3dName">
                  <div :key="currentSlide" class="l4-slide">

                    <!-- slide 0 · flashcards -->
                    <div v-if="currentSlide === 0" class="ps">
                      <span class="ps-app">FLASHCARDS · PHYSICS CH 3</span>
                      <div class="ps0-flip">
                        <div class="ps0-face ps0-front">
                          <span class="fc4-tag">QUESTION</span>
                          <p class="fc4-q">If <b>F = 10 N</b> and <b>m = 2 kg</b>, find the acceleration <b>a</b>.</p>
                          <span class="fc4-hint">auto-flips…</span>
                        </div>
                        <div class="ps0-face ps0-back">
                          <span class="fc4-tag gold">ANSWER</span>
                          <p class="fc4-q">a = F / m = <b>5 m/s²</b></p>
                          <span class="fc4-coin"><Check :size="12" /> Correct · +50 coins</span>
                        </div>
                      </div>
                      <div class="ps0-pills">
                        <span class="active">Physics</span><span>Chemistry</span><span>Biology</span><span class="urdu">اردو</span>
                      </div>
                      <Bot pose="bot-point" class="ps0-bot" />
                    </div>

                    <!-- slide 1 · AI grading -->
                    <div v-else-if="currentSlide === 1" class="ps">
                      <span class="ps-app">AI EXAMINER · MARKING</span>
                      <div class="ps1-sheet">
                        <div v-for="(r, ri) in gradeRows" :key="r.q" class="ps1-row" :style="{ animationDelay: `${ri * 0.35 + 0.2}s` }">
                          <span class="ps1-q">{{ r.q }}</span>
                          <span v-if="r.pending" class="ps1-pen">analyzing…</span>
                          <span v-else class="ps1-mark"><Check :size="10" /> {{ r.mark }}</span>
                        </div>
                      </div>
                      <div class="ps1-ringwrap">
                        <svg viewBox="0 0 80 80" class="ps1-ring">
                          <circle class="ring-bg" cx="40" cy="40" r="34" />
                          <circle class="ring-fg" cx="40" cy="40" r="34" :stroke-dasharray="ringCirc" :stroke-dashoffset="ringCirc * (1 - scoreVal / 100)" />
                        </svg>
                        <span class="ps1-num">{{ scoreVal }}<i>%</i></span>
                      </div>
                      <span class="ps1-lbl">auto-graded · +18% this week</span>
                      <Bot pose="bot-write" class="ps1-bot" />
                    </div>

                    <!-- slide 2 · live tutor chat -->
                    <div v-else class="ps">
                      <span class="ps-app">GHALIB AI · LIVE</span>
                      <div class="ps2-chat">
                        <div class="ps2-bub user urdu">شعر کی تشریح سمجھائیں؟</div>
                        <div class="ps2-bub typing"><span /><span /><span /></div>
                        <div class="ps2-bub ai">
                          <span class="urdu">یہ شعر اُمید اور حوصلے کی بات کرتا ہے</span>
                          <em>“It speaks of hope &amp; resolve.”</em>
                        </div>
                      </div>
                      <div class="ps2-input"><span>Ask anything…</span><Send :size="12" /></div>
                      <Bot pose="bot-wave" class="ps2-bot" />
                    </div>

                  </div>
                </Transition>
              </IPhone>
            </div>
          </div>

          <button class="l4-arrow prev" @click="prevSlide" aria-label="Previous slide">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M15 18l-6-6 6-6" /></svg>
          </button>
          <button class="l4-arrow next" @click="nextSlide" aria-label="Next slide">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 18l6-6-6-6" /></svg>
          </button>
          <div class="l4-dots">
            <button v-for="(_, i) in slideData" :key="i" class="l4-dot" :class="{ active: currentSlide === i }" :aria-label="`Slide ${i + 1}`" @click="goToSlide(i)" />
            <span class="l4-dot-lbl">{{ ['Flashcards', 'AI Grading', 'Live Tutor'][currentSlide] }}</span>
          </div>
        </div>
      </div>
      <div ref="lineStartEl" class="l2-line-start" aria-hidden="true" />
    </header>

    <!-- ── SUBJECT TICKER ── -->
    <div class="l4-ticker" aria-hidden="true">
      <div class="l4-ticker-track">
        <template v-for="n in 2" :key="n">
          <span v-for="s in tickerSubjects" :key="`${n}-${s}`" class="l4-tick">{{ s }}<i /></span>
        </template>
      </div>
    </div>

    <!-- ── JOURNEY STATIONS — each scene on an iPhone ── -->
    <section id="path" class="l2-journey">
      <div
        v-for="(m, i) in milestones"
        :key="m.title"
        class="l2-stop"
        :class="[i % 2 === 0 ? 'is-left' : 'is-right', { active: i < activeStops }]"
      >
        <div :ref="el => setStopNode(el, i)" class="l2-stop-node">
          <span class="l2-stop-ring" /><span class="l2-stop-dot" />
        </div>

        <article class="l2-stop-card reveal">
          <div class="l4-phone-wrap" aria-hidden="true">
            <IPhone>
              <div class="sc">
                <div class="sc-head">
                  <span>{{ m.scene }}</span>
                  <span class="sc-live"><i />{{ i < activeStops ? 'ONLINE' : 'STANDBY' }}</span>
                </div>
                <div class="sc-standby">— STANDBY —</div>
                <span class="sc-scan" />

                <!-- scene 1 · AI tutor (portrait) -->
                <div v-if="i === 0" class="sc-stage">
                  <div class="p1-board">
                    <span class="p1-tag">PHYSICS · NEWTON'S 2nd LAW</span>
                    <i class="p1-tl p1-tl1 gx" /><i class="p1-tl p1-tl2 gx" />
                    <div class="p1-eq gx">a&nbsp;=&nbsp;F&nbsp;/&nbsp;m&nbsp;=&nbsp;5&nbsp;m/s²</div>
                  </div>
                  <div class="p1-bubble gx">Concept clear? Try one yourself!</div>
                  <Bot pose="bot-point" class="p1-bot" />
                </div>

                <!-- scene 2 · paper engine (portrait) -->
                <div v-else-if="i === 1" class="sc-stage">
                  <div class="p2-paper">
                    <span class="p2-ptitle">MODEL PAPER · 9th</span>
                    <i class="p2-pl p2-pl1 gx" /><i class="p2-pl p2-pl2 gx" /><i class="p2-pl p2-pl3 gx" />
                    <div class="p2-mcq">
                      <span v-for="(o, oi) in ['A', 'B', 'C', 'D']" :key="o" class="p2-opt gx" :class="{ pick: oi === 1 }">{{ o }}</span>
                    </div>
                  </div>
                  <svg class="p2-gear" viewBox="0 0 36 36"><circle cx="18" cy="18" r="10" /></svg>
                  <i class="p-spark s1" /><i class="p-spark s2" /><i class="p-spark s3" />
                  <Bot pose="bot-write" class="p2-bot" />
                </div>

                <!-- scene 3 · AI examiner (portrait) -->
                <div v-else-if="i === 2" class="sc-stage">
                  <div class="p3-rows">
                    <div class="p3-row"><i class="p3-bar" style="width: 78%" /><span class="p3-stamp ok gx"><svg viewBox="0 0 24 24"><path d="M5 13l4 4L19 7" /></svg></span></div>
                    <div class="p3-row"><i class="p3-bar" style="width: 64%" /><span class="p3-stamp ok gx" style="animation-delay: 1s"><svg viewBox="0 0 24 24"><path d="M5 13l4 4L19 7" /></svg></span></div>
                    <div class="p3-row"><i class="p3-bar" style="width: 70%" /><span class="p3-stamp no gx"><svg viewBox="0 0 24 24"><path d="M6 6l12 12M18 6L6 18" /></svg></span></div>
                    <i class="p3-fix gx" />
                  </div>
                  <div class="p3-ringwrap">
                    <svg class="p3-ring" viewBox="0 0 84 84">
                      <circle class="ring-bg" cx="42" cy="42" r="32" />
                      <circle class="ring-fg" cx="42" cy="42" r="32" />
                    </svg>
                    <span class="p3-score gx">87%</span>
                  </div>
                  <Bot pose="bot-write" class="p3-bot" />
                </div>

                <!-- scene 4 · arena (portrait) -->
                <div v-else class="sc-stage">
                  <i v-for="c in 8" :key="c" class="p4-conf" :class="`c${c}`" />
                  <span class="p4-rank gx">RANK #01 · QUETTA</span>
                  <div class="p4-podium">
                    <div class="p4-col p2c gx"><b>2</b></div>
                    <div class="p4-col p1c gx">
                      <svg class="p4-cup gx" viewBox="0 0 24 24"><path d="M7 3h10v5a5 5 0 0 1-10 0V3zM7 5H4a3 3 0 0 0 3 4M17 5h3a3 3 0 0 1-3 4M11 13h2v3h-2zM8 16h8v3H8z" /></svg>
                      <b>1</b>
                    </div>
                    <div class="p4-col p3c gx"><b>3</b></div>
                  </div>
                  <Bot pose="bot-cheer" class="p4-bot" />
                </div>
              </div>
            </IPhone>
          </div>

          <span class="l2-stop-icon"><component :is="m.icon" :size="22" /></span>
          <span class="l4-stop-step">{{ String(i + 1).padStart(2, '0') }}</span>
          <h3 class="l2-stop-h">{{ m.title }}</h3>
          <p class="l2-stop-p">{{ m.desc }}</p>
          <ul class="l2-stop-chips">
            <li v-for="c in m.chips" :key="c"><Check :size="12" /> {{ c }}</li>
          </ul>
        </article>
      </div>
    </section>

    <!-- ── AI BRAIN — the engine runs on a phone ── -->
    <section class="l4-brain" :class="{ lit: brainOn }">
      <p class="l2-kicker reveal"><BrainCircuit :size="14" /> AI Intelligence</p>
      <h2 class="l2-h2 reveal">Watch AI study <em>your students.</em></h2>
      <p class="l2-sub reveal">Every answer feeds the engine — it tracks, adapts and personalizes each session in real time.</p>

      <div class="l4-bstage reveal">
        <div class="l4-bcol">
          <div v-for="st in students.slice(0, 2)" :key="st.name" class="l4-scard">
            <div class="l4-sc-top">
              <span class="l4-sc-av">{{ st.name[0] }}</span>
              <span class="l4-sc-id"><b>{{ st.name }}</b><i>{{ st.subject }}</i></span>
              <span class="l4-sc-score" :style="{ color: st.color }">{{ st.score }}</span>
            </div>
            <div v-for="bar in st.bars" :key="bar.label" class="l4-bar">
              <span>{{ bar.label }}</span>
              <i class="l4-bar-track"><i class="l4-bar-fill" :style="{ width: brainOn ? bar.pct + '%' : '0%', background: bar.color }" /></i>
              <b>{{ bar.pct }}%</b>
            </div>
          </div>
        </div>

        <div class="l4-bwire"><span class="l4-pulse" /></div>

        <div class="l4-bcore">
          <div ref="brainAnchorEl" class="l2-center-anchor is-abs" aria-hidden="true" />
          <i class="l4-ring r1" /><i class="l4-ring r2" />
          <IPhone size="xs">
            <div class="l4-engine">
              <BrainCircuit :size="30" />
              <span>AI ENGINE</span>
              <i class="l4-eng-bar b1" /><i class="l4-eng-bar b2" /><i class="l4-eng-bar b3" />
            </div>
          </IPhone>
        </div>

        <div class="l4-bwire flip"><span class="l4-pulse" style="animation-delay: .7s" /></div>

        <div class="l4-bcol">
          <div v-for="st in students.slice(2)" :key="st.name" class="l4-scard">
            <div class="l4-sc-top">
              <span class="l4-sc-av">{{ st.name[0] }}</span>
              <span class="l4-sc-id"><b>{{ st.name }}</b><i>{{ st.subject }}</i></span>
              <span class="l4-sc-score" :style="{ color: st.color }">{{ st.score }}</span>
            </div>
            <div v-for="bar in st.bars" :key="bar.label" class="l4-bar">
              <span>{{ bar.label }}</span>
              <i class="l4-bar-track"><i class="l4-bar-fill" :style="{ width: brainOn ? bar.pct + '%' : '0%', background: bar.color }" /></i>
              <b>{{ bar.pct }}%</b>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── STATS ── -->
    <section class="l2-stats" :class="{ lit: statsOn }">
      <div ref="statsAnchorEl" class="l2-center-anchor is-abs" aria-hidden="true" />
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[0].toLocaleString('en-US') }}+</span><span class="l2-stat-l">Students on the path</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[1] }}K+</span><span class="l2-stat-l">AI-graded answers</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[2] }}%</span><span class="l2-stat-l">Improved board scores</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">24/7</span><span class="l2-stat-l">AI tutor on call</span></div>
    </section>

    <!-- ── PRICING — checkout on a phone ── -->
    <section id="pricing" class="l4-pricing" :class="{ lit: priceOn }">
      <div class="l4-price-grid">
        <div>
          <p class="l2-kicker reveal"><Coins :size="14" /> Affordable for every family</p>
          <h2 class="l2-h2 reveal" style="text-align:left">Full access<br /><em>Rs. 999 / year.</em></h2>
          <p class="l2-sub reveal" style="margin-left:0; text-align:left">Under Rs. 83 a month — one subscription unlocks everything, right from your phone.</p>
          <ul class="l4-price-feats reveal">
            <li v-for="f in pricingFeats" :key="f"><Check :size="14" /> {{ f }}</li>
          </ul>
        </div>
        <div class="l4-price-phone reveal">
          <div ref="priceAnchorEl" class="l2-center-anchor is-abs" aria-hidden="true" />
          <span class="l4-price-badge gx">Most Popular</span>
          <IPhone>
            <div class="l4-checkout">
              <span class="ps-app">CHECKOUT · YEARLY PLAN</span>
              <span class="l4-co-amount">Rs. 999</span>
              <span class="l4-co-period">per year · unlimited access</span>
              <i class="l4-co-div" />
              <ul class="l4-co-list">
                <li><Check :size="11" /> 8 AI tutors, all subjects</li>
                <li><Check :size="11" /> Unlimited AI-graded tests</li>
                <li><Check :size="11" /> Competitions &amp; coins</li>
              </ul>
              <span class="l4-co-btn">Start Now <ArrowRight :size="13" /></span>
              <span class="l4-co-note">Easypaisa · JazzCash · Bank</span>
            </div>
          </IPhone>
        </div>
      </div>
    </section>

    <!-- ── n8n AUTOMATION ── -->
    <section class="l2-n8n">
      <div ref="n8nAnchorEl" class="l2-center-anchor" aria-hidden="true" />
      <p class="l2-kicker reveal"><Workflow :size="14" /> Automation · powered by your local n8n</p>
      <h2 class="l2-h2 reveal">This page is <em>wired</em>, not just pretty.</h2>
      <p class="l2-sub reveal">
        The line ends in a webhook. Every signup below is posted straight into
        <strong>n8n running on your machine</strong> — route it to Sheets, WhatsApp,
        your CRM or anywhere else, without touching this code again.
      </p>
      <div class="l2-pipe reveal" :class="{ lit: n8nOn }" role="img" aria-label="Form submission flows from this page to an n8n webhook, then to any destination you connect">
        <div class="l2-pipe-node is-src"><Send :size="18" /><span class="l2-pipe-name">Landing4 Form</span><span class="l2-pipe-sub">POST · JSON</span></div>
        <div class="l2-pipe-wire"><span class="l2-pipe-pulse" /></div>
        <div class="l2-pipe-node is-hub"><Webhook :size="18" /><span class="l2-pipe-name">n8n Webhook</span><span class="l2-pipe-sub">localhost:5678</span></div>
        <div class="l2-pipe-wire"><span class="l2-pipe-pulse" style="animation-delay: .6s" /></div>
        <div class="l2-pipe-fan">
          <div class="l2-pipe-node is-dst"><Database :size="16" /><span class="l2-pipe-name">Sheet / DB</span></div>
          <div class="l2-pipe-node is-dst"><Bell :size="16" /><span class="l2-pipe-name">WhatsApp Alert</span></div>
          <div class="l2-pipe-node is-dst"><Zap :size="16" /><span class="l2-pipe-name">Anything else</span></div>
        </div>
      </div>
    </section>

    <!-- ── LEAD FORM ── -->
    <section id="join" class="l2-join">
      <div ref="formAnchorEl" class="l2-center-anchor" aria-hidden="true" />
      <div class="l2-form-card reveal" :class="{ lit: formOn }">
        <template v-if="form.status !== 'success'">
          <h2 class="l2-h2 sm">End of the line.<br /><em>Start of yours.</em></h2>
          <p class="l2-form-p">Drop your details — they land directly in your local n8n workflow.</p>
          <form class="l2-form" @submit.prevent="submitLead">
            <div class="l2-field">
              <label for="l4-name">Student name</label>
              <input id="l4-name" v-model.trim="form.name" type="text" required autocomplete="name" placeholder="e.g. Ayesha Khan" />
            </div>
            <div class="l2-field">
              <label for="l4-phone">Phone / WhatsApp</label>
              <input id="l4-phone" v-model.trim="form.phone" type="tel" required autocomplete="tel" placeholder="03xx-xxxxxxx" />
            </div>
            <div class="l2-row">
              <div class="l2-field">
                <label for="l4-grade">Grade</label>
                <select id="l4-grade" v-model="form.grade">
                  <option value="9">Grade 9</option>
                  <option value="10">Grade 10</option>
                </select>
              </div>
              <div class="l2-field">
                <label for="l4-city">City</label>
                <input id="l4-city" v-model.trim="form.city" type="text" placeholder="Quetta" />
              </div>
            </div>
            <button type="submit" class="l2-btn-primary l2-btn-submit" :disabled="form.status === 'sending'">
              <span v-if="form.status === 'sending'" class="l2-spinner" aria-hidden="true" />
              <Send v-else :size="16" />
              {{ form.status === 'sending' ? 'Sending to n8n…' : 'Join via n8n Webhook' }}
            </button>
            <p v-if="form.status === 'error'" class="l2-form-err" role="alert">
              <X :size="14" /> Couldn’t reach the webhook. Is n8n running at
              <code>{{ webhookUrl }}</code> with the workflow <strong>active</strong>?
            </p>
          </form>
        </template>
        <div v-else class="l2-form-done" role="status">
          <span class="l2-done-badge"><Check :size="26" /></span>
          <h2 class="l2-h2 sm">You’re on the line.</h2>
          <p class="l2-form-p">n8n confirmed your spot — we’ll be in touch on WhatsApp.</p>
          <RouterLink to="/login" class="l2-btn-primary">Open the App <ArrowRight :size="16" /></RouterLink>
        </div>
      </div>
    </section>

    <!-- ── FOOTER ── -->
    <footer class="l4-footer">
      <div ref="lineEndEl" class="l2-line-end" aria-hidden="true" />
      <span class="l2-logo-mark sm">eS</span>
      <p>Balochistan Academy Portal — the line from page one to position one, now in your pocket.</p>
      <nav class="l4-foot-links">
        <RouterLink to="/">Classic (v1)</RouterLink>
        <RouterLink to="/landing2">The Path (v2)</RouterLink>
        <RouterLink to="/landing3">Ultra (v3)</RouterLink>
        <RouterLink to="/login">Login</RouterLink>
      </nav>
    </footer>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount, nextTick, h } from 'vue'
import {
  ArrowRight, ArrowDown, Check, X, Send, Zap, Bell, Database,
  Webhook, Workflow, Bot as BotIcon, Sparkles, ClipboardCheck, Trophy,
  BrainCircuit, Coins, Sun, Moon, Smartphone,
} from '@lucide/vue'
import { useThemeStore } from '@/stores/theme'

const themeStore = useThemeStore()

/* ── reusable SVG robot mascot ───────────────────────────── */
function Bot(props) {
  return h('svg', { viewBox: '0 0 120 150', class: ['bot', props.pose], 'aria-hidden': 'true' }, [
    h('line', { x1: 60, y1: 10, x2: 60, y2: 22, class: 'bot-neck' }),
    h('circle', { cx: 60, cy: 8, r: 5, class: 'bot-ant' }),
    h('rect', { x: 14, y: 76, width: 12, height: 32, rx: 6, class: 'bot-limb bot-arm-l' }),
    h('rect', { x: 94, y: 76, width: 12, height: 32, rx: 6, class: 'bot-limb bot-arm-r' }),
    h('rect', { x: 44, y: 120, width: 10, height: 18, rx: 5, class: 'bot-limb' }),
    h('rect', { x: 66, y: 120, width: 10, height: 18, rx: 5, class: 'bot-limb' }),
    h('rect', { x: 34, y: 72, width: 52, height: 48, rx: 14, class: 'bot-shell' }),
    h('circle', { cx: 60, cy: 96, r: 8, class: 'bot-core' }),
    h('rect', { x: 28, y: 22, width: 64, height: 46, rx: 16, class: 'bot-shell' }),
    h('rect', { x: 36, y: 32, width: 48, height: 24, rx: 10, class: 'bot-visor' }),
    h('circle', { cx: 50, cy: 44, r: 4.5, class: 'bot-eye' }),
    h('circle', { cx: 70, cy: 44, r: 4.5, class: 'bot-eye bot-eye2' }),
    h('path', { d: 'M52 61 Q60 66 68 61', class: 'bot-mouth' }),
  ])
}
Bot.props = { pose: { type: String, default: '' } }

/* ── iPhone frame component (CSS-only) ───────────────────── */
function IPhone(props, { slots }) {
  return h('div', { class: ['iph', `iph-${props.size}`] }, [
    h('span', { class: 'iph-btn pwr', 'aria-hidden': 'true' }),
    h('span', { class: 'iph-btn vol1', 'aria-hidden': 'true' }),
    h('span', { class: 'iph-btn vol2', 'aria-hidden': 'true' }),
    h('div', { class: 'iph-screen' }, [
      h('div', { class: 'iph-island', 'aria-hidden': 'true' }, [h('i', { class: 'iph-cam' })]),
      h('div', { class: 'iph-status', 'aria-hidden': 'true' }, [
        h('b', '9:41'),
        h('span', { class: 'iph-sig' }, [h('i'), h('i'), h('i'), h('i'), h('em', { class: 'iph-batt' })]),
      ]),
      h('div', { class: 'iph-content' }, slots.default ? slots.default() : []),
      h('span', { class: 'iph-home', 'aria-hidden': 'true' }),
    ]),
  ])
}
IPhone.props = { size: { type: String, default: 'md' } }

/* ── hero slider ─────────────────────────────────────────── */
const currentSlide = ref(0)
const direction = ref(1)
const sliderHovering = ref(false)
const sliderRef = ref(null)
const cardMx = ref(0), cardMy = ref(0)
let sliderInterval = null

const s3dName = computed(() => (direction.value > 0 ? 's3d-next' : 's3d-prev'))
const tiltStyle = computed(() => {
  const rx = sliderHovering.value ? cardMy.value * -8 : 0
  const ry = sliderHovering.value ? cardMx.value * 12 : 0
  return { transform: `perspective(1200px) rotateX(${rx}deg) rotateY(${ry}deg)` }
})
function onSliderMouseMove(e) {
  const el = sliderRef.value
  if (!el) return
  const r = el.getBoundingClientRect()
  cardMx.value = (e.clientX - r.left) / r.width - 0.5
  cardMy.value = (e.clientY - r.top) / r.height - 0.5
}
function onSliderMouseLeave() {
  sliderHovering.value = false
  cardMx.value = 0
  cardMy.value = 0
}
function nextSlide() { direction.value = 1; currentSlide.value = (currentSlide.value + 1) % 3; resetSliderTimer() }
function prevSlide() { direction.value = -1; currentSlide.value = (currentSlide.value + 2) % 3; resetSliderTimer() }
function goToSlide(i) { direction.value = i > currentSlide.value ? 1 : -1; currentSlide.value = i; resetSliderTimer() }
function startSlider() { sliderInterval = setInterval(() => { direction.value = 1; currentSlide.value = (currentSlide.value + 1) % 3 }, 5000) }
function resetSliderTimer() { clearInterval(sliderInterval); startSlider() }

const slideData = [
  { heading: ['Flip a Card.', 'Master Any', 'Concept.'], goldLine: 2,
    desc: 'Bite-size AI flashcards for every Grade 9 chapter — on the bus, in bed, between classes. Your pocket never stops teaching.' },
  { heading: ['Submit Once.', 'AI Grades It', 'in Seconds.'], goldLine: 1,
    desc: 'Snap your answer with your phone camera and watch it marked instantly — score, mistakes and a personalised fix-list.' },
  { heading: ['Ask Anything,', 'in Urdu or', 'English.'], goldLine: 1,
    desc: '8 legendary AI tutors in your pocket — chat or voice, English & Urdu medium, available 24/7 right up to exam day.' },
]
const gradeRows = [
  { q: 'Q1 · Newton II law', mark: '+2' },
  { q: 'Q2 · Ionic bonding', mark: '+2' },
  { q: 'Q3 · Kinematics', pending: true },
  { q: 'Q4 · Ohm’s law', mark: '+2' },
]
const ringCirc = 2 * Math.PI * 34
const scoreVal = ref(0)
let scoreRaf = null
function animateScore() {
  cancelAnimationFrame(scoreRaf)
  if (reduceMotion) { scoreVal.value = 87; return }
  const target = 87, dur = 1300, start = performance.now()
  const step = (now) => {
    const t = Math.min(1, (now - start) / dur)
    scoreVal.value = Math.round((1 - Math.pow(1 - t, 3)) * target)
    if (t < 1) scoreRaf = requestAnimationFrame(step)
  }
  scoreVal.value = 0
  scoreRaf = requestAnimationFrame(step)
}
watch(currentSlide, (v) => { if (v === 1) animateScore() })

/* ── content ─────────────────────────────────────────────── */
const heroStats = [
  { val: '10+', label: 'Subjects' }, { val: '8', label: 'AI Tutors' },
  { val: '1200+', label: 'Questions' }, { val: '99%', label: 'Pass Rate' },
]
const tickerSubjects = ['Physics', 'Mathematics', 'Chemistry', 'Biology', 'Computer Sc.', 'Pak Studies', 'English', 'Urdu', 'Islamiyat', 'General Science']

const milestones = [
  { icon: BotIcon, scene: 'AI TUTOR · CH 3', title: 'Learn live with your AI tutor',
    desc: 'Ask anything, anytime — your robot teacher explains every concept in Urdu or English until it clicks, with chat and video lessons.',
    chips: ['24/7 doubt solving', 'Chat + video tutor', 'Urdu + English'] },
  { icon: Sparkles, scene: 'PAPER ENGINE', title: 'AI writes your paper',
    desc: 'Gemini drafts fresh objective and subjective papers in the exact Balochistan Board pattern — never the same test twice.',
    chips: ['Board pattern', 'Unlimited papers', 'Difficulty dial'] },
  { icon: ClipboardCheck, scene: 'AI EXAMINER', title: 'Marked by AI in seconds',
    desc: 'Submit your answers and watch the robot examiner stamp every line — marks, corrections and a model answer, instantly.',
    chips: ['Instant marks', 'Per-line feedback', 'Weak-topic alerts'] },
  { icon: Trophy, scene: 'ARENA · LIVE', title: 'Compete across the province',
    desc: 'Daily streaks, weekly quizzes and monthly competitions — earn coins and race the whole province up the merit list.',
    chips: ['Daily streaks', 'Coins & rewards', 'Province ranks'] },
]

const students = [
  { name: 'Ahmed K.', subject: 'Physics', score: '92%', color: '#4ade80',
    bars: [{ label: 'Accuracy', pct: 92, color: '#7c6af5' }, { label: 'Speed', pct: 78, color: '#06b6d4' }, { label: 'Retention', pct: 85, color: '#fbbf24' }] },
  { name: 'Fatima R.', subject: 'Chemistry', score: '87%', color: '#06b6d4',
    bars: [{ label: 'Accuracy', pct: 87, color: '#a78bfa' }, { label: 'Speed', pct: 91, color: '#06b6d4' }, { label: 'Retention', pct: 82, color: '#f59e0b' }] },
  { name: 'Bilal M.', subject: 'Math', score: '95%', color: '#fbbf24',
    bars: [{ label: 'Accuracy', pct: 95, color: '#4ade80' }, { label: 'Speed', pct: 88, color: '#7c6af5' }, { label: 'Retention', pct: 90, color: '#fbbf24' }] },
  { name: 'Sara A.', subject: 'Biology', score: '89%', color: '#f87171',
    bars: [{ label: 'Accuracy', pct: 89, color: '#f87171' }, { label: 'Speed', pct: 74, color: '#06b6d4' }, { label: 'Retention', pct: 93, color: '#a78bfa' }] },
]

const pricingFeats = [
  'Unlimited AI Tutor sessions (all 8 subjects)',
  'Full question bank — 1,200+ MCQ + Subjective',
  'AI-graded answers with detailed feedback',
  'Daily & monthly competitions with leaderboards',
  'Coin rewards redeemable for real cash',
  'Parent-visible performance reports',
  'English & Urdu medium support',
]

/* ── n8n lead form ───────────────────────────────────────── */
const webhookUrl = import.meta.env.VITE_N8N_WEBHOOK_URL || 'http://localhost:5678/webhook/bap-lead'
const form = reactive({ name: '', phone: '', grade: '9', city: '', status: 'idle' })

async function submitLead() {
  form.status = 'sending'
  try {
    const res = await fetch(webhookUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: form.name, phone: form.phone, grade: form.grade, city: form.city,
        source: 'landing4', submittedAt: new Date().toISOString(),
      }),
    })
    if (!res.ok) throw new Error(`Webhook responded ${res.status}`)
    form.status = 'success'
  } catch (err) {
    console.error('[landing4] n8n webhook failed:', err)
    form.status = 'error'
  }
}

/* ── neural-net canvas ───────────────────────────────────── */
const bgCanvas = ref(null)
let canvasAnim = null
const cPts = []
function initBgCanvas() {
  const c = bgCanvas.value
  if (!c || reduceMotion) return
  c.width = window.innerWidth; c.height = window.innerHeight
  for (let i = 0; i < 55; i++) {
    cPts.push({ x: Math.random() * c.width, y: Math.random() * c.height, vx: (Math.random() - 0.5) * 0.3, vy: (Math.random() - 0.5) * 0.3, r: Math.random() * 1.5 + 0.5 })
  }
  const draw = () => {
    const ctx = c.getContext('2d')
    ctx.clearRect(0, 0, c.width, c.height)
    cPts.forEach((p) => {
      p.x += p.vx; p.y += p.vy
      if (p.x < 0 || p.x > c.width) p.vx *= -1
      if (p.y < 0 || p.y > c.height) p.vy *= -1
      ctx.beginPath(); ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2)
      ctx.fillStyle = 'rgba(124,106,245,0.25)'; ctx.fill()
    })
    for (let i = 0; i < cPts.length; i++) {
      for (let j = i + 1; j < cPts.length; j++) {
        const dx = cPts[i].x - cPts[j].x, dy = cPts[i].y - cPts[j].y, d = Math.sqrt(dx * dx + dy * dy)
        if (d < 130) {
          ctx.beginPath(); ctx.moveTo(cPts[i].x, cPts[i].y); ctx.lineTo(cPts[j].x, cPts[j].y)
          ctx.strokeStyle = `rgba(124,106,245,${0.12 * (1 - d / 130)})`; ctx.lineWidth = 0.5; ctx.stroke()
        }
      }
    }
    canvasAnim = requestAnimationFrame(draw)
  }
  draw()
}

/* ── scroll-drawn SVG line ───────────────────────────────── */
const pageEl = ref(null)
const lineEl = ref(null)
const lineStartEl = ref(null)
const lineEndEl = ref(null)
const brainAnchorEl = ref(null)
const statsAnchorEl = ref(null)
const priceAnchorEl = ref(null)
const n8nAnchorEl = ref(null)
const formAnchorEl = ref(null)
const stopNodes = []
function setStopNode(el, i) { if (el) stopNodes[i] = el }

const svgW = ref(0)
const svgH = ref(0)
const pathD = ref('')
const pathLen = ref(0)
const dashOffset = ref(0)
const tip = reactive({ x: -100, y: -100 })
const tipVisible = ref(false)
const activeStops = ref(0)
const scrolled = ref(false)
const brainOn = ref(false)
const statsOn = ref(false)
const priceOn = ref(false)
const n8nOn = ref(false)
const formOn = ref(false)
const statCounters = reactive([0, 0, 0])
const mx = ref(-500)
const my = ref(-500)

const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches

function onMouseMove(e) { mx.value = e.clientX; my.value = e.clientY }

function relY(el) {
  const pr = pageEl.value.getBoundingClientRect()
  const r = el.getBoundingClientRect()
  return r.top - pr.top + r.height / 2
}
function relX(el) {
  const pr = pageEl.value.getBoundingClientRect()
  const r = el.getBoundingClientRect()
  return r.left - pr.left + r.width / 2
}

let stopYs = []
let secY = { brain: Infinity, stats: Infinity, price: Infinity, n8n: Infinity, form: Infinity }
let countersDone = false

function runCounters() {
  if (countersDone) return
  countersDone = true
  const targets = [12000, 350, 92]
  if (reduceMotion) { targets.forEach((t, i) => { statCounters[i] = t }); return }
  const t0 = performance.now()
  const dur = 1500
  const step = (t) => {
    const p = Math.min(1, (t - t0) / dur)
    const e = 1 - Math.pow(1 - p, 3)
    targets.forEach((tv, i) => { statCounters[i] = Math.round(tv * e) })
    if (p < 1) requestAnimationFrame(step)
  }
  requestAnimationFrame(step)
}

function buildPath() {
  const page = pageEl.value
  if (!page || !lineStartEl.value || !lineEndEl.value) return
  const w = page.clientWidth
  const hgt = page.scrollHeight
  svgW.value = w
  svgH.value = hgt

  const pts = [{ x: w / 2, y: relY(lineStartEl.value) }]
  stopNodes.forEach((el) => { if (el) pts.push({ x: relX(el), y: relY(el) }) })
  const anchors = [brainAnchorEl, statsAnchorEl, priceAnchorEl, n8nAnchorEl, formAnchorEl]
  anchors.forEach((a) => { if (a.value) pts.push({ x: relX(a.value), y: relY(a.value) }) })
  pts.push({ x: w / 2, y: relY(lineEndEl.value) })

  stopYs = stopNodes.map((el) => (el ? relY(el) : Infinity))
  secY = {
    brain: brainAnchorEl.value ? relY(brainAnchorEl.value) : Infinity,
    stats: statsAnchorEl.value ? relY(statsAnchorEl.value) : Infinity,
    price: priceAnchorEl.value ? relY(priceAnchorEl.value) : Infinity,
    n8n: n8nAnchorEl.value ? relY(n8nAnchorEl.value) : Infinity,
    form: formAnchorEl.value ? relY(formAnchorEl.value) : Infinity,
  }

  let d = `M ${pts[0].x.toFixed(1)} ${pts[0].y.toFixed(1)}`
  for (let i = 1; i < pts.length; i++) {
    const a = pts[i - 1], b = pts[i]
    const cy = (b.y - a.y) * 0.55
    d += ` C ${a.x.toFixed(1)} ${(a.y + cy).toFixed(1)}, ${b.x.toFixed(1)} ${(b.y - cy).toFixed(1)}, ${b.x.toFixed(1)} ${b.y.toFixed(1)}`
  }
  pathD.value = d

  nextTick(() => {
    if (!lineEl.value) return
    pathLen.value = lineEl.value.getTotalLength()
    if (reduceMotion) {
      dashOffset.value = 0
      activeStops.value = milestones.length
      brainOn.value = statsOn.value = priceOn.value = n8nOn.value = formOn.value = true
      runCounters()
      tipVisible.value = false
    } else {
      drawn = -1
      updateLine(true)
    }
  })
}

let drawn = 0
let rafId = 0
let animating = false

function targetLen() {
  const page = pageEl.value
  if (!page || !pathLen.value) return 0
  const pr = page.getBoundingClientRect()
  const lead = window.innerHeight * 0.82
  const scrolledPx = -pr.top + lead
  const progress = Math.min(1, Math.max(0, scrolledPx / (page.scrollHeight - (window.innerHeight - lead))))
  return pathLen.value * progress
}

function updateLine(instant = false) {
  const t = targetLen()
  drawn = instant ? t : drawn + (t - drawn) * 0.18
  if (Math.abs(t - drawn) < 0.5) drawn = t
  dashOffset.value = Math.max(0, pathLen.value - drawn)

  if (lineEl.value && drawn > 1) {
    const pt = lineEl.value.getPointAtLength(drawn)
    tip.x = pt.x
    tip.y = pt.y
    tipVisible.value = drawn < pathLen.value - 2
    activeStops.value = stopYs.filter((y) => pt.y >= y - 4).length
    if (!brainOn.value && pt.y >= secY.brain - 10) brainOn.value = true
    if (!statsOn.value && pt.y >= secY.stats - 10) { statsOn.value = true; runCounters() }
    if (!priceOn.value && pt.y >= secY.price - 10) priceOn.value = true
    if (!n8nOn.value && pt.y >= secY.n8n - 10) n8nOn.value = true
    if (!formOn.value && pt.y >= secY.form - 10) formOn.value = true
  }
  if (!instant && Math.abs(t - drawn) >= 0.5) {
    rafId = requestAnimationFrame(() => updateLine())
  } else {
    animating = false
  }
}

/* scroll-driven reveal — data attribute survives Vue class patching */
let revealEls = []
function checkReveals() {
  if (!revealEls.length) return
  const vh = window.innerHeight
  revealEls = revealEls.filter((el) => {
    const r = el.getBoundingClientRect()
    if (r.top < vh * 0.88 && r.bottom > 0) { el.dataset.in = '1'; return false }
    return true
  })
}

function onScroll() {
  scrolled.value = window.scrollY > 40
  checkReveals()
  if (reduceMotion || animating) return
  animating = true
  rafId = requestAnimationFrame(() => updateLine())
}

let resizeObs = null

onMounted(async () => {
  await nextTick()
  initBgCanvas()
  startSlider()
  buildPath()
  if (document.fonts?.ready) document.fonts.ready.then(buildPath)
  setTimeout(buildPath, 600)

  resizeObs = new ResizeObserver(() => { buildPath(); checkReveals() })
  resizeObs.observe(pageEl.value)

  revealEls = [...pageEl.value.querySelectorAll('.reveal')]
  if (reduceMotion) {
    revealEls.forEach((el) => { el.dataset.in = '1' })
    revealEls = []
  }

  window.addEventListener('scroll', onScroll, { passive: true })
  onScroll()
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', onScroll)
  cancelAnimationFrame(rafId)
  cancelAnimationFrame(canvasAnim)
  cancelAnimationFrame(scoreRaf)
  clearInterval(sliderInterval)
  resizeObs?.disconnect()
})
</script>

<style scoped>
/* ════ base + theme tokens (shared with Landing 3) ════ */
.l2 {
  --tx1: #f5f3ff; --tx2: #b9b3d9; --tx3: #8d86b8;
  --panel: rgba(15, 11, 34, 0.78); --panel2: rgba(5, 3, 13, 0.7);
  --bd: rgba(138, 127, 255, 0.16); --bd2: rgba(138, 127, 255, 0.3);
  --grad-a: #a8a4ff; --grad-b: #fbbf24; --gold: #fbbf24; --btn-txt: #05030d;
  --nav-bg: rgba(7, 5, 18, 0.72);
  position: relative; min-height: 100vh; overflow-x: clip;
  background:
    radial-gradient(1100px 600px at 85% -5%, rgba(124, 106, 245, 0.16), transparent 60%),
    radial-gradient(900px 500px at 10% 30%, rgba(251, 191, 36, 0.05), transparent 60%),
    radial-gradient(1000px 700px at 50% 105%, rgba(124, 106, 245, 0.12), transparent 60%),
    #05030d;
  color: var(--tx1);
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
}
html.light .l2 {
  --tx1: #1a1333; --tx2: #453a70; --tx3: #6f679c;
  --panel: rgba(255, 255, 255, 0.86); --panel2: #ffffff;
  --bd: rgba(91, 67, 204, 0.16); --bd2: rgba(91, 67, 204, 0.34);
  --grad-a: #6d54e8; --grad-b: #d97706; --gold: #b45309; --btn-txt: #ffffff;
  --nav-bg: rgba(255, 255, 255, 0.82);
  background:
    radial-gradient(1100px 600px at 85% -5%, rgba(124, 106, 245, 0.14), transparent 60%),
    radial-gradient(900px 500px at 10% 30%, rgba(217, 119, 6, 0.07), transparent 60%),
    radial-gradient(1000px 700px at 50% 105%, rgba(124, 106, 245, 0.1), transparent 60%),
    #f2f1fb;
}
.urdu { font-family: 'Noto Nastaliq Urdu', serif; }

.l3-canvas { position: fixed; inset: 0; width: 100%; height: 100%; z-index: 0; pointer-events: none; opacity: 0.55; }
html.light .l3-canvas { opacity: 0.3; }
html.light .l2-aurora { opacity: 0.5; }
html.light .l2-path-ghost { stroke: rgba(91, 67, 204, 0.16); }

.l2-aurora { position: absolute; inset: 0; z-index: 0; overflow: hidden; pointer-events: none; }
.l2-aurora i { position: absolute; width: 560px; height: 560px; border-radius: 50%; filter: blur(95px); }
.l2-aurora .a1 { top: -160px; left: -120px; background: rgba(59, 42, 132, 0.55); animation: l2-drift1 19s ease-in-out infinite alternate; }
.l2-aurora .a2 { top: 32%; right: -200px; background: rgba(35, 25, 103, 0.6); animation: l2-drift2 24s ease-in-out infinite alternate; }
.l2-aurora .a3 { bottom: -220px; left: 28%; width: 480px; background: rgba(245, 158, 11, 0.22); animation: l2-drift1 28s ease-in-out infinite alternate-reverse; }
@keyframes l2-drift1 { to { transform: translate(70px, 50px) scale(1.12); } }
@keyframes l2-drift2 { to { transform: translate(-80px, -40px) scale(0.92); } }
.l2-spotlight { position: fixed; inset: 0; z-index: 1; pointer-events: none; }

/* ════ line ════ */
.l2-line-svg { position: absolute; inset: 0; z-index: 0; pointer-events: none; }
.l2-path-ghost { stroke: rgba(138, 127, 255, 0.10); stroke-width: 2; stroke-dasharray: 3 9; }
.l2-path-main { stroke-width: 3; stroke-linecap: round; }
.l2-tip { position: absolute; top: 0; left: 0; z-index: 1; pointer-events: none; will-change: transform; }
.l2-tip-core, .l2-tip-halo { position: absolute; border-radius: 9999px; transform: translate(-50%, -50%); }
.l2-tip-core { width: 10px; height: 10px; background: #fff; box-shadow: 0 0 12px 3px rgba(251, 191, 36, 0.9); }
.l2-tip-halo { width: 34px; height: 34px; background: radial-gradient(circle, rgba(251,191,36,0.35), transparent 70%); animation: l2-halo 1.6s ease-in-out infinite; }
@keyframes l2-halo { 0%, 100% { opacity: 0.6; } 50% { opacity: 1; } }
.l2-line-start, .l2-line-end, .l2-center-anchor { width: 2px; height: 2px; margin: 0 auto; }
.l2-center-anchor.is-abs { position: absolute; left: 50%; top: 50%; margin: 0; }

/* ════ robot ════ */
:deep(.bot) { overflow: visible; display: block; }
:deep(.bot-shell) { fill: #161038; stroke: rgba(138, 127, 255, 0.55); stroke-width: 2; }
:deep(.bot-visor) { fill: #07041c; }
:deep(.bot-eye) { fill: #a99cff; transform-box: fill-box; transform-origin: center; animation: l2-blink 4.2s infinite; }
:deep(.bot-eye2) { animation-delay: 0.15s; }
:deep(.bot-mouth) { fill: none; stroke: #a99cff; stroke-width: 2.5; stroke-linecap: round; }
:deep(.bot-neck) { stroke: rgba(138, 127, 255, 0.55); stroke-width: 2.5; }
:deep(.bot-ant) { fill: #fbbf24; animation: l2-antpulse 2.2s ease-in-out infinite; }
:deep(.bot-limb) { fill: #161038; stroke: rgba(138, 127, 255, 0.45); stroke-width: 2; }
:deep(.bot-core) { fill: #fbbf24; animation: l2-antpulse 2.6s ease-in-out infinite; }
:deep(.bot-arm-l), :deep(.bot-arm-r) { transform-box: fill-box; transform-origin: 50% 12%; }
:deep(.bot-wave .bot-arm-r) { animation: l2-wave 1.7s ease-in-out infinite; }
:deep(.bot-point .bot-arm-r) { transform: rotate(-115deg); }
:deep(.bot-write .bot-arm-l) { animation: l2-scribble 0.7s ease-in-out infinite; }
:deep(.bot-cheer .bot-arm-l) { animation: l2-cheerL 1s ease-in-out infinite; }
:deep(.bot-cheer .bot-arm-r) { animation: l2-cheerR 1s ease-in-out infinite; }
@keyframes l2-blink { 0%, 91%, 100% { transform: scaleY(1); } 94% { transform: scaleY(0.12); } }
@keyframes l2-antpulse { 0%, 100% { opacity: 0.55; } 50% { opacity: 1; } }
@keyframes l2-wave { 0%, 100% { transform: rotate(10deg); } 50% { transform: rotate(-110deg); } }
@keyframes l2-scribble { 0%, 100% { transform: rotate(35deg); } 50% { transform: rotate(55deg); } }
@keyframes l2-cheerL { 0%, 100% { transform: rotate(140deg); } 50% { transform: rotate(165deg); } }
@keyframes l2-cheerR { 0%, 100% { transform: rotate(-140deg); } 50% { transform: rotate(-165deg); } }
@keyframes l2-float { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-10px); } }

/* ════ iPhone frame ════ */
:deep(.iph) {
  position: relative; width: 252px; height: 516px; flex-shrink: 0; margin: 0 auto;
  padding: 9px; border-radius: 44px;
  background: linear-gradient(160deg, #342d56, #14102a 60%, #2b2447);
  box-shadow: 0 34px 80px rgba(0, 0, 0, 0.55), 0 0 40px rgba(124, 106, 245, 0.12), inset 0 0 0 2px rgba(168, 164, 255, 0.22);
}
:deep(.iph-lg) { width: 300px; height: 614px; border-radius: 52px; }
:deep(.iph-xs) { width: 150px; height: 308px; border-radius: 28px; padding: 6px; }
:deep(.iph-btn) { position: absolute; width: 3px; border-radius: 2px; background: #3d3566; }
:deep(.iph-btn.pwr)  { right: -3px; top: 130px; height: 64px; }
:deep(.iph-btn.vol1) { left: -3px; top: 110px; height: 36px; }
:deep(.iph-btn.vol2) { left: -3px; top: 154px; height: 36px; }
:deep(.iph-screen) {
  position: relative; width: 100%; height: 100%; overflow: hidden; border-radius: 35px;
  background:
    radial-gradient(120% 100% at 80% 0%, rgba(124, 106, 245, 0.16), transparent 55%),
    linear-gradient(170deg, #0a0620, #120b2e);
}
:deep(.iph-lg .iph-screen) { border-radius: 43px; }
:deep(.iph-xs .iph-screen) { border-radius: 22px; }
:deep(.iph-island) {
  position: absolute; top: 9px; left: 50%; transform: translateX(-50%); z-index: 6;
  width: 72px; height: 21px; border-radius: 99px; background: #020108;
  display: flex; align-items: center; justify-content: flex-end; padding-right: 7px;
}
:deep(.iph-xs .iph-island) { width: 46px; height: 14px; top: 6px; }
:deep(.iph-cam) { width: 8px; height: 8px; border-radius: 99px; background: radial-gradient(circle at 35% 35%, #2c3f6b, #0a1020 70%); }
:deep(.iph-status) {
  position: absolute; top: 10px; left: 20px; right: 18px; z-index: 5;
  display: flex; justify-content: space-between; align-items: center;
  font-size: 11px; font-weight: 700; color: #eceaf6;
}
:deep(.iph-xs .iph-status) { display: none; }
:deep(.iph-sig) { display: inline-flex; align-items: flex-end; gap: 1.5px; }
:deep(.iph-sig i) { width: 2.5px; border-radius: 1px; background: #eceaf6; }
:deep(.iph-sig i:nth-child(1)) { height: 4px; }
:deep(.iph-sig i:nth-child(2)) { height: 6px; }
:deep(.iph-sig i:nth-child(3)) { height: 8px; }
:deep(.iph-sig i:nth-child(4)) { height: 10px; opacity: 0.4; }
:deep(.iph-batt) { width: 19px; height: 9px; margin-left: 5px; border-radius: 3px; border: 1px solid rgba(236, 234, 246, 0.5); position: relative; }
:deep(.iph-batt::before) { content: ''; position: absolute; inset: 1px; right: 5px; border-radius: 1.5px; background: #fbbf24; }
:deep(.iph-content) { position: absolute; inset: 0; padding-top: 40px; }
:deep(.iph-xs .iph-content) { padding-top: 24px; }
:deep(.iph-home) { position: absolute; bottom: 7px; left: 50%; transform: translateX(-50%); z-index: 6; width: 94px; height: 4px; border-radius: 99px; background: rgba(255, 255, 255, 0.35); }
:deep(.iph-xs .iph-home) { width: 50px; height: 3px; }

/* ════ nav ════ */
.l2-nav {
  position: fixed; top: 14px; left: 16px; right: 16px; z-index: 50;
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 18px; border-radius: 16px;
  border: 1px solid transparent; transition: background 0.25s, border-color 0.25s, backdrop-filter 0.25s;
}
.l2-nav.scrolled { background: var(--nav-bg); border-color: var(--bd); backdrop-filter: blur(14px); }
.l2-nav-theme {
  display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; cursor: pointer;
  color: var(--tx2); background: transparent; border: 1px solid var(--bd2);
  transition: color 0.2s, border-color 0.2s, transform 0.2s;
}
.l2-nav-theme:hover { color: var(--gold); border-color: var(--gold); transform: rotate(15deg); }
.l2-logo { display: inline-flex; align-items: center; gap: 10px; text-decoration: none; }
.l2-logo-mark {
  display: grid; place-items: center; width: 36px; height: 36px; border-radius: 11px;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 15px; color: #05030d;
  background: linear-gradient(135deg, #8a7fff, #fbbf24);
}
.l2-logo-mark.sm { width: 30px; height: 30px; font-size: 13px; border-radius: 9px; }
.l2-logo-text { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 14.5px; color: var(--tx1); letter-spacing: 0.01em; }
.l2-logo-text em { font-style: normal; color: var(--gold); }
.l2-nav-right { display: flex; align-items: center; gap: 6px; }
.l2-nav-lnk { padding: 9px 13px; border-radius: 10px; font-size: 13.5px; font-weight: 600; color: var(--tx2); text-decoration: none; transition: color 0.2s, background 0.2s; }
.l2-nav-lnk:hover { color: var(--tx1); background: rgba(138, 127, 255, 0.12); }
.l2-nav-cta {
  display: inline-flex; align-items: center; gap: 6px; padding: 10px 16px; border-radius: 11px;
  font-size: 13.5px; font-weight: 700; color: var(--btn-txt); text-decoration: none;
  background: linear-gradient(135deg, var(--grad-a), var(--grad-b)); transition: opacity 0.2s, box-shadow 0.2s;
}
.l2-nav-cta:hover { opacity: 0.92; box-shadow: 0 0 24px rgba(251, 191, 36, 0.35); }
@media (max-width: 760px) { .l2-nav-lnk { display: none; } }

/* ════ hero ════ */
.l4-hero { position: relative; z-index: 2; max-width: 1240px; margin: 0 auto; padding: 130px 24px 50px; }
.l4-hero-grid { display: grid; grid-template-columns: 1.1fr 1fr; gap: 56px; align-items: center; }
@media (max-width: 980px) { .l4-hero-grid { grid-template-columns: 1fr; gap: 44px; } }
.l2-kicker {
  display: inline-flex; align-items: center; gap: 8px;
  font-size: 12.5px; font-weight: 700; letter-spacing: 0.14em; text-transform: uppercase;
  color: var(--grad-a); margin-bottom: 24px;
}
.l4-htext { min-height: 320px; }
.l4-h1 {
  font-family: 'Syne', sans-serif; font-weight: 800;
  font-size: clamp(2.5rem, 4.8vw, 4.2rem); line-height: 1.02; letter-spacing: -0.035em;
  margin: 0 0 20px; color: var(--tx1);
}
.l4-h1-line { display: block; }
.l4-h1-line.gold {
  font-style: italic;
  background: linear-gradient(100deg, var(--grad-b), var(--grad-a));
  -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l4-hero-p { max-width: 460px; margin: 0 0 8px; font-size: 16px; line-height: 1.7; color: var(--tx2); }
.slide-txt-enter-active, .slide-txt-leave-active { transition: opacity 0.35s ease, transform 0.35s ease; }
.slide-txt-enter-from { opacity: 0; transform: translateY(16px); }
.slide-txt-leave-to { opacity: 0; transform: translateY(-12px); }
.l4-hstats { display: flex; gap: 28px; flex-wrap: wrap; }
.l4-hstat { display: flex; flex-direction: column; }
.l4-hstat-v {
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 24px;
  background: linear-gradient(120deg, var(--grad-a), var(--grad-b)); -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l4-hstat-l { font-size: 11.5px; font-weight: 600; letter-spacing: 0.07em; text-transform: uppercase; color: var(--tx3); }

.l4-slider-wrap { position: relative; display: flex; flex-direction: column; align-items: center; }
.l4-scene { perspective: 1200px; animation: l2-float 7s ease-in-out infinite; }
.l4-tilt { transition: transform 0.18s ease-out; transform-style: preserve-3d; }
.l4-slide { position: absolute; inset: 0; }
.s3d-next-enter-active, .s3d-next-leave-active, .s3d-prev-enter-active, .s3d-prev-leave-active { transition: opacity 0.45s ease, transform 0.45s cubic-bezier(0.22, 1, 0.36, 1); }
.s3d-next-enter-from { opacity: 0; transform: translateX(60px) scale(0.95); }
.s3d-next-leave-to { opacity: 0; transform: translateX(-60px) scale(0.95); }
.s3d-prev-enter-from { opacity: 0; transform: translateX(-60px) scale(0.95); }
.s3d-prev-leave-to { opacity: 0; transform: translateX(60px) scale(0.95); }
.l4-arrow {
  position: absolute; top: 45%; z-index: 5; width: 36px; height: 36px;
  display: grid; place-items: center; border-radius: 99px; cursor: pointer;
  color: var(--tx1); background: var(--panel); border: 1px solid var(--bd2);
  transition: border-color 0.2s, color 0.2s;
}
.l4-arrow:hover { color: var(--gold); border-color: var(--gold); }
.l4-arrow.prev { left: 6px; } .l4-arrow.next { right: 6px; }
@media (max-width: 1100px) { .l4-arrow.prev { left: -10px; } .l4-arrow.next { right: -10px; } }
.l4-dots { display: flex; align-items: center; gap: 8px; margin-top: 20px; justify-content: center; }
.l4-dot { width: 22px; height: 5px; border-radius: 99px; border: 0; cursor: pointer; background: var(--bd2); transition: background 0.25s; }
.l4-dot.active { background: linear-gradient(90deg, var(--grad-a), var(--grad-b)); }
.l4-dot-lbl { margin-left: 8px; font-size: 11.5px; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: var(--tx3); }

/* phone app screens (hero slides) */
.ps { position: absolute; inset: 0; }
.ps-app {
  position: absolute; top: 44px; left: 0; right: 0; text-align: center;
  font-size: 9.5px; font-weight: 800; letter-spacing: 0.16em; color: #8d86b8;
}
.ps0-flip {
  position: absolute; left: 20px; right: 20px; top: 76px; height: 250px;
  transform-style: preserve-3d; animation: l4-flip 7s ease-in-out infinite;
}
@keyframes l4-flip { 0%, 38% { transform: rotateY(0); } 50%, 88% { transform: rotateY(180deg); } 100% { transform: rotateY(360deg); } }
.ps0-face {
  position: absolute; inset: 0; padding: 18px; border-radius: 18px; backface-visibility: hidden;
  background: rgba(5, 3, 13, 0.75); border: 1px solid rgba(138, 127, 255, 0.3);
  display: flex; flex-direction: column; gap: 10px; text-align: left;
}
.ps0-back { transform: rotateY(180deg); border-color: rgba(251, 191, 36, 0.4); }
.fc4-tag { font-size: 9.5px; font-weight: 800; letter-spacing: 0.14em; color: #a99cff; }
.fc4-tag.gold { color: #fbbf24; }
.fc4-q { margin: 0; font-size: 14.5px; line-height: 1.6; color: #e9e6fa; }
.fc4-q b { color: #fbbf24; }
.fc4-hint { margin-top: auto; font-size: 10.5px; color: #5d5687; }
.fc4-coin { margin-top: auto; display: inline-flex; align-items: center; gap: 5px; font-size: 12px; font-weight: 700; color: #6ee7b7; }
.ps0-pills { position: absolute; left: 20px; right: 20px; top: 348px; display: flex; gap: 6px; flex-wrap: wrap; }
.ps0-pills span {
  padding: 4px 10px; border-radius: 99px; font-size: 10.5px; font-weight: 600;
  color: #b9b3d9; background: rgba(138, 127, 255, 0.1); border: 1px solid rgba(138, 127, 255, 0.22);
}
.ps0-pills span.active { color: #05030d; background: linear-gradient(135deg, #a8a4ff, #fbbf24); border-color: transparent; font-weight: 700; }
.ps0-bot { position: absolute; right: 16px; bottom: 18px; width: 86px; animation: l2-float 4.6s ease-in-out infinite; }

.ps1-sheet { position: absolute; left: 18px; right: 18px; top: 74px; }
.ps1-row {
  display: flex; justify-content: space-between; align-items: center; padding: 9px 12px; margin-bottom: 7px;
  border-radius: 11px; background: rgba(138, 127, 255, 0.08); border: 1px solid rgba(138, 127, 255, 0.16);
  opacity: 0; animation: l4-rowin 0.4s ease forwards;
}
@keyframes l4-rowin { from { opacity: 0; transform: translateX(-10px); } to { opacity: 1; transform: none; } }
.ps1-q { font-size: 11.5px; font-weight: 600; color: #d9d5f2; }
.ps1-mark { display: inline-flex; align-items: center; gap: 4px; font-size: 11px; font-weight: 800; color: #6ee7b7; }
.ps1-pen { font-size: 10.5px; font-style: italic; color: #fbbf24; animation: l2-halo 1.2s infinite; }
.ps1-ringwrap { position: absolute; left: 50%; top: 292px; width: 104px; height: 104px; margin-left: -52px; }
.ps1-ring { width: 104px; height: 104px; transform: rotate(-90deg); }
.ps1-ring .ring-bg { fill: none; stroke: rgba(138, 127, 255, 0.18); stroke-width: 7; }
.ps1-ring .ring-fg { fill: none; stroke: #fbbf24; stroke-width: 7; stroke-linecap: round; transition: stroke-dashoffset 0.2s linear; }
.ps1-num { position: absolute; inset: 0; display: grid; place-items: center; font-family: 'Syne', sans-serif; font-weight: 800; font-size: 24px; color: #fbbf24; }
.ps1-num i { font-style: normal; font-size: 13px; }
.ps1-lbl { position: absolute; left: 0; right: 0; top: 408px; text-align: center; font-size: 10px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: #8d86b8; }
.ps1-bot { position: absolute; left: 16px; bottom: 14px; width: 64px; }

.ps2-chat { position: absolute; left: 16px; right: 16px; top: 74px; display: flex; flex-direction: column; gap: 9px; text-align: left; }
.ps2-bub { max-width: 85%; padding: 9px 13px; border-radius: 15px; font-size: 13px; line-height: 1.6; }
.ps2-bub.user { align-self: flex-end; color: #f5f3ff; background: rgba(124, 106, 245, 0.3); border: 1px solid rgba(138, 127, 255, 0.4); border-bottom-right-radius: 4px; }
.ps2-bub.ai { align-self: flex-start; color: #e9e6fa; background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.25); border-bottom-left-radius: 4px; }
.ps2-bub.ai em { display: block; margin-top: 4px; font-size: 11px; color: #8d86b8; }
.ps2-bub.typing { display: inline-flex; gap: 4px; padding: 11px 14px; background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.25); }
.ps2-bub.typing span { width: 6px; height: 6px; border-radius: 99px; background: #8a7fff; animation: l4-dots 1.2s ease-in-out infinite; }
.ps2-bub.typing span:nth-child(2) { animation-delay: 0.18s; }
.ps2-bub.typing span:nth-child(3) { animation-delay: 0.36s; }
@keyframes l4-dots { 0%, 100% { opacity: 0.3; transform: translateY(0); } 50% { opacity: 1; transform: translateY(-3px); } }
.ps2-input {
  position: absolute; left: 16px; right: 16px; bottom: 96px;
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 14px; border-radius: 13px; font-size: 12px; color: #5d5687;
  background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.22);
}
.ps2-input svg { color: #fbbf24; }
.ps2-bot { position: absolute; right: 14px; bottom: 14px; width: 66px; }

/* ════ ticker ════ */
.l4-ticker { position: relative; z-index: 2; overflow: hidden; padding: 18px 0; border-top: 1px solid var(--bd); border-bottom: 1px solid var(--bd); }
.l4-ticker-track { display: flex; width: max-content; animation: l4-marquee 26s linear infinite; }
.l4-ticker:hover .l4-ticker-track { animation-play-state: paused; }
.l4-tick {
  display: inline-flex; align-items: center; gap: 18px; padding-right: 18px;
  font-family: 'Syne', sans-serif; font-weight: 700; font-size: 14px; letter-spacing: 0.06em;
  color: var(--tx3); text-transform: uppercase; white-space: nowrap;
}
.l4-tick i { width: 5px; height: 5px; border-radius: 99px; background: var(--grad-b); opacity: 0.7; }
@keyframes l4-marquee { to { transform: translateX(-50%); } }

/* ════ buttons (shared) ════ */
.l2-hero-btns { display: flex; flex-wrap: wrap; gap: 14px; }
.l2-btn-primary {
  display: inline-flex; align-items: center; gap: 9px; padding: 15px 28px; border: 0; border-radius: 14px;
  font-family: inherit; font-size: 15px; font-weight: 800; color: var(--btn-txt); text-decoration: none; cursor: pointer;
  background: linear-gradient(135deg, var(--grad-a), var(--grad-b));
  transition: transform 0.2s, box-shadow 0.2s; box-shadow: 0 10px 36px rgba(124, 106, 245, 0.3);
}
.l2-btn-primary:hover { transform: translateY(-2px); box-shadow: 0 14px 44px rgba(251, 191, 36, 0.4); }
.l2-btn-primary:disabled { opacity: 0.7; cursor: wait; transform: none; }
.l2-btn-ghost {
  display: inline-flex; align-items: center; gap: 9px; padding: 15px 24px; border-radius: 14px;
  font-size: 15px; font-weight: 700; color: var(--tx1); text-decoration: none; cursor: pointer;
  border: 1px solid var(--bd2); transition: border-color 0.2s, background 0.2s;
}
.l2-btn-ghost:hover { border-color: var(--gold); background: rgba(138, 127, 255, 0.08); }

/* ════ journey stations ════ */
.l2-journey { position: relative; z-index: 2; max-width: 1180px; margin: 0 auto; padding: 60px 24px 80px; }
.l2-stop { position: relative; display: flex; align-items: center; padding: 56px 0; }
.l2-stop-node { position: absolute; top: 50%; width: 2px; height: 2px; }
.l2-stop.is-left  .l2-stop-node { left: 16%; }
.l2-stop.is-right .l2-stop-node { left: 84%; }
.l2-stop-dot, .l2-stop-ring { position: absolute; border-radius: 9999px; transform: translate(-50%, -50%); }
.l2-stop-dot { width: 14px; height: 14px; background: var(--panel2); border: 2px solid var(--bd2); transition: background 0.4s, border-color 0.4s, box-shadow 0.4s; }
.l2-stop-ring { width: 40px; height: 40px; border: 1px solid rgba(138, 127, 255, 0.25); transition: border-color 0.4s; }
.l2-stop.active .l2-stop-dot { background: #fbbf24; border-color: #fde68a; box-shadow: 0 0 18px rgba(251, 191, 36, 0.8); }
.l2-stop.active .l2-stop-ring { border-color: rgba(251, 191, 36, 0.5); }
.l2-stop.active .l2-stop-node::after {
  content: ''; position: absolute; width: 14px; height: 14px; border-radius: 9999px;
  transform: translate(-50%, -50%); border: 2px solid #fbbf24;
  animation: l2-ping 0.9s cubic-bezier(0, 0, 0.2, 1) 1 forwards;
}
@keyframes l2-ping { from { opacity: 0.9; } to { transform: translate(-50%, -50%) scale(5); opacity: 0; } }

.l2-stop-card {
  position: relative; width: min(520px, 100%); padding: 28px 30px 30px; border-radius: 24px; text-align: center;
  background: var(--panel); border: 1px solid var(--bd);
  backdrop-filter: blur(8px); transition: border-color 0.3s, box-shadow 0.4s;
}
.l2-stop-card:hover { border-color: rgba(251, 191, 36, 0.35); }
.l2-stop.active .l2-stop-card { box-shadow: 0 24px 70px rgba(124, 106, 245, 0.12); }
.l2-stop.is-left  .l2-stop-card { margin-left: auto;  margin-right: 2%; }
.l2-stop.is-right .l2-stop-card { margin-right: auto; margin-left: 2%; }
.l4-phone-wrap { margin-bottom: 22px; }
.l4-stop-step {
  position: absolute; top: 28px; right: 28px;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 30px;
  color: transparent; -webkit-text-stroke: 1px rgba(138, 127, 255, 0.4);
}
html.light .l4-stop-step { -webkit-text-stroke-color: rgba(91, 67, 204, 0.35); }
.l2-stop-icon {
  display: inline-grid; place-items: center; width: 46px; height: 46px; border-radius: 13px; margin-bottom: 14px;
  color: var(--gold); background: rgba(251, 191, 36, 0.1); border: 1px solid rgba(251, 191, 36, 0.25);
}
.l2-stop-h { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 24px; letter-spacing: -0.02em; margin: 0 0 10px; color: var(--tx1); }
.l2-stop-p { margin: 0 0 18px; font-size: 15px; line-height: 1.65; color: var(--tx2); }
.l2-stop-chips { display: flex; flex-wrap: wrap; gap: 8px; margin: 0; padding: 0; list-style: none; justify-content: center; }
.l2-stop-chips li {
  display: inline-flex; align-items: center; gap: 5px; padding: 5px 11px; border-radius: 99px;
  font-size: 12px; font-weight: 600; color: var(--grad-a);
  background: rgba(138, 127, 255, 0.1); border: 1px solid var(--bd2);
}
@media (max-width: 860px) {
  .l2-stop { padding: 36px 0; }
  .l2-stop.is-left .l2-stop-card, .l2-stop.is-right .l2-stop-card { margin: 0 auto; }
  .l2-stop.is-left .l2-stop-node { left: 6%; }
  .l2-stop.is-right .l2-stop-node { left: 94%; }
}

/* ════ phone scene shell (standby → online) ════ */
.sc { position: absolute; inset: 0; filter: saturate(0.15) brightness(0.6); transition: filter 0.8s ease; }
.l2-stop.active .sc { filter: none; }
.sc-stage { position: absolute; inset: 0; }
.sc-head {
  position: absolute; top: 44px; left: 16px; right: 16px; z-index: 2;
  display: flex; justify-content: space-between; align-items: center;
  font-size: 9px; font-weight: 700; letter-spacing: 0.12em; color: #8d86b8; text-transform: uppercase;
}
.sc-live { display: inline-flex; align-items: center; gap: 5px; color: #fbbf24; }
.sc-live i { width: 6px; height: 6px; border-radius: 99px; background: #5d5687; }
.l2-stop.active .sc-live i { background: #fbbf24; animation: l2-halo 1.4s infinite; }
.sc-standby {
  position: absolute; inset: 0; z-index: 3; display: grid; place-items: center;
  font-size: 10px; font-weight: 700; letter-spacing: 0.3em; color: #5d5687;
  background: rgba(5, 3, 13, 0.45); transition: opacity 0.6s; pointer-events: none;
}
.l2-stop.active .sc-standby { opacity: 0; }
.sc-scan {
  position: absolute; top: 0; bottom: 0; width: 60px; z-index: 1; opacity: 0;
  background: linear-gradient(90deg, transparent, rgba(138, 127, 255, 0.07), transparent);
}
.l2-stop.active .sc-scan { opacity: 1; animation: l4-scan 3.4s linear infinite; }
@keyframes l4-scan { from { transform: translateX(-70px); } to { transform: translateX(300px); } }

/* — portrait scene 1 · tutor — */
.p1-board {
  position: absolute; left: 16px; right: 16px; top: 76px; padding: 15px 16px; border-radius: 14px;
  background: rgba(5, 3, 13, 0.65); border: 1px solid rgba(138, 127, 255, 0.25); text-align: left;
}
.p1-tag { display: block; font-size: 8.5px; font-weight: 700; letter-spacing: 0.12em; color: #8d86b8; margin-bottom: 10px; }
.p1-tl { display: block; height: 8px; border-radius: 99px; background: rgba(138, 127, 255, 0.35); margin-bottom: 8px; transform: scaleX(0); transform-origin: left; }
.p1-tl1 { width: 92%; } .p1-tl2 { width: 64%; }
.l2-stop.active .p1-tl1 { animation: l2-grow 0.5s ease 0.3s forwards; }
.l2-stop.active .p1-tl2 { animation: l2-grow 0.5s ease 0.8s forwards; }
.p1-eq {
  margin-top: 12px; font-family: 'Syne', sans-serif; font-weight: 700; font-size: 14px; color: #fbbf24;
  white-space: nowrap; overflow: hidden; width: 0; border-right: 2px solid rgba(251, 191, 36, 0);
}
.l2-stop.active .p1-eq { border-right-color: #fbbf24; animation: l2-type 1.1s steps(16) 1.3s forwards, l2-caret 0.8s step-end 1.3s 4; }
@keyframes l2-grow { to { transform: scaleX(1); } }
@keyframes l2-type { to { width: 100%; } }
@keyframes l2-caret { 50% { border-right-color: transparent; } }
.p1-bubble {
  position: absolute; left: 100px; bottom: 160px; max-width: 120px; z-index: 2; text-align: left;
  padding: 8px 11px; border-radius: 12px 12px 12px 3px; font-size: 10.5px; font-weight: 600; color: #f5f3ff;
  background: rgba(124, 106, 245, 0.22); border: 1px solid rgba(138, 127, 255, 0.45);
  opacity: 0; transform: scale(0.5); transform-origin: bottom left;
}
.l2-stop.active .p1-bubble { animation: l2-pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) 2.6s forwards; }
@keyframes l2-pop { to { opacity: 1; transform: scale(1); } }
.p1-bot { position: absolute; left: 18px; bottom: 18px; width: 88px; animation: l2-float 4.5s ease-in-out infinite; }

/* — portrait scene 2 · paper — */
.p2-paper {
  position: absolute; left: 22px; right: 22px; top: 80px; padding: 15px 16px; border-radius: 12px;
  background: #f3f0ff; transform: rotate(-2deg); text-align: left;
  box-shadow: 0 14px 34px rgba(0, 0, 0, 0.45);
}
.p2-ptitle { display: block; font-size: 8.5px; font-weight: 800; letter-spacing: 0.12em; color: #4a35a6; margin-bottom: 9px; }
.p2-pl { display: block; height: 7px; border-radius: 99px; background: #c9c2ec; margin-bottom: 7px; transform: scaleX(0); transform-origin: left; }
.p2-pl1 { width: 95%; } .p2-pl2 { width: 78%; } .p2-pl3 { width: 86%; }
.l2-stop.active .p2-pl1 { animation: l2-grow 0.45s ease 0.3s forwards; }
.l2-stop.active .p2-pl2 { animation: l2-grow 0.45s ease 0.75s forwards; }
.l2-stop.active .p2-pl3 { animation: l2-grow 0.45s ease 1.2s forwards; }
.p2-mcq { display: flex; gap: 8px; margin-top: 12px; }
.p2-opt {
  display: grid; place-items: center; width: 26px; height: 26px; border-radius: 8px;
  font-size: 11px; font-weight: 800; color: #4a35a6; border: 1.5px solid #c9c2ec;
  opacity: 0; transform: scale(0.4);
}
.l2-stop.active .p2-opt { animation: l2-pop 0.35s cubic-bezier(0.34, 1.56, 0.64, 1) forwards; }
.l2-stop.active .p2-opt:nth-child(1) { animation-delay: 1.6s; }
.l2-stop.active .p2-opt:nth-child(2) { animation-delay: 1.75s; }
.l2-stop.active .p2-opt:nth-child(3) { animation-delay: 1.9s; }
.l2-stop.active .p2-opt:nth-child(4) { animation-delay: 2.05s; }
.l2-stop.active .p2-opt.pick { background: #fbbf24; border-color: #f59e0b; box-shadow: 0 0 14px rgba(251, 191, 36, 0.7); }
.p2-gear { position: absolute; right: 26px; top: 268px; width: 32px; height: 32px; }
.p2-gear circle { fill: none; stroke: #8a7fff; stroke-width: 6; stroke-dasharray: 4 3.5; }
.l2-stop.active .p2-gear { animation: l4-spin 3s linear infinite; }
@keyframes l4-spin { to { transform: rotate(360deg); } }
.p-spark { position: absolute; width: 5px; height: 5px; border-radius: 99px; background: #fbbf24; opacity: 0; }
.p-spark.s1 { left: 36px; top: 278px; } .p-spark.s2 { right: 70px; top: 316px; } .p-spark.s3 { left: 70px; top: 346px; }
.l2-stop.active .p-spark { animation: l4-twinkle 1.8s ease-in-out infinite; }
.l2-stop.active .p-spark.s2 { animation-delay: 0.5s; }
.l2-stop.active .p-spark.s3 { animation-delay: 1.1s; }
@keyframes l4-twinkle { 0%, 100% { opacity: 0.2; } 50% { opacity: 1; } }
.p2-bot { position: absolute; right: 20px; bottom: 16px; width: 84px; animation: l2-float 5s ease-in-out infinite; }

/* — portrait scene 3 · examiner — */
.p3-rows { position: absolute; left: 18px; top: 82px; width: 62%; }
.p3-row { position: relative; display: flex; align-items: center; margin-bottom: 17px; }
.p3-bar { display: block; height: 9px; border-radius: 99px; background: rgba(138, 127, 255, 0.3); }
.p3-stamp {
  position: absolute; right: -36px; top: 50%; width: 24px; height: 24px; margin-top: -12px;
  display: grid; place-items: center; border-radius: 99px;
  opacity: 0; transform: scale(0.3);
}
.p3-stamp svg { width: 13px; height: 13px; fill: none; stroke: currentColor; stroke-width: 3.4; stroke-linecap: round; }
.p3-stamp.ok { color: #34d399; background: rgba(52, 211, 153, 0.14); border: 1.5px solid rgba(52, 211, 153, 0.5); }
.p3-stamp.no { color: #fb7185; background: rgba(251, 113, 133, 0.14); border: 1.5px solid rgba(251, 113, 133, 0.5); }
.l2-stop.active .p3-stamp { animation: l4-stamp 0.45s cubic-bezier(0.34, 1.8, 0.64, 1) 0.5s forwards; }
.l2-stop.active .p3-stamp.no { animation-delay: 1.5s; }
@keyframes l4-stamp { 60% { transform: scale(1.25); opacity: 1; } 100% { transform: scale(1); opacity: 1; } }
.p3-fix { display: block; width: 58%; height: 6px; border-radius: 99px; background: rgba(251, 191, 36, 0.75); transform: scaleX(0); transform-origin: left; }
.l2-stop.active .p3-fix { animation: l2-grow 0.5s ease 2.1s forwards; }
.p3-ringwrap { position: absolute; left: 50%; top: 244px; width: 100px; height: 100px; margin-left: -50px; }
.p3-ring { width: 100px; height: 100px; transform: rotate(-90deg); }
.p3-ring .ring-bg { fill: none; stroke: rgba(138, 127, 255, 0.18); stroke-width: 8; }
.p3-ring .ring-fg { fill: none; stroke: #fbbf24; stroke-width: 8; stroke-linecap: round; stroke-dasharray: 201; stroke-dashoffset: 201; }
.l2-stop.active .p3-ring .ring-fg { animation: l4-ring 1.5s cubic-bezier(0.22, 1, 0.36, 1) 0.7s forwards; }
@keyframes l4-ring { to { stroke-dashoffset: 26; } }
.p3-score {
  position: absolute; inset: 0; display: grid; place-items: center;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 21px; color: #fbbf24; opacity: 0;
}
.l2-stop.active .p3-score { animation: l4-fadein 0.5s ease 1.9s forwards; }
@keyframes l4-fadein { to { opacity: 1; } }
.p3-bot { position: absolute; right: 20px; bottom: 14px; width: 72px; }

/* — portrait scene 4 · arena — */
.p4-rank {
  position: absolute; left: 50%; top: 82px; transform: translateX(-50%) scale(0.5);
  padding: 6px 13px; border-radius: 99px; white-space: nowrap;
  font-size: 10px; font-weight: 800; letter-spacing: 0.1em; color: #05030d;
  background: linear-gradient(135deg, #a8a4ff, #fbbf24); opacity: 0;
}
.l2-stop.active .p4-rank { animation: l4-rankpop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) 2s forwards; }
@keyframes l4-rankpop { to { opacity: 1; transform: translateX(-50%) scale(1); } }
.p4-podium { position: absolute; left: 24px; bottom: 110px; display: flex; align-items: flex-end; gap: 10px; }
.p4-col {
  position: relative; width: 42px; border-radius: 9px 9px 0 0; display: flex; justify-content: center;
  background: linear-gradient(180deg, rgba(138, 127, 255, 0.4), rgba(138, 127, 255, 0.12));
  border: 1px solid rgba(138, 127, 255, 0.35); border-bottom: 0;
  transform: scaleY(0); transform-origin: bottom;
}
.p4-col b { margin-top: 7px; font-family: 'Syne', sans-serif; font-size: 14px; color: #d9d5f2; }
.p4-col.p1c { height: 104px; background: linear-gradient(180deg, rgba(251, 191, 36, 0.5), rgba(251, 191, 36, 0.12)); border-color: rgba(251, 191, 36, 0.5); }
.p4-col.p2c { height: 70px; } .p4-col.p3c { height: 50px; }
.l2-stop.active .p4-col.p2c { animation: l4-growy 0.6s cubic-bezier(0.22, 1, 0.36, 1) 0.3s forwards; }
.l2-stop.active .p4-col.p1c { animation: l4-growy 0.6s cubic-bezier(0.22, 1, 0.36, 1) 0.65s forwards; }
.l2-stop.active .p4-col.p3c { animation: l4-growy 0.6s cubic-bezier(0.22, 1, 0.36, 1) 1s forwards; }
@keyframes l4-growy { to { transform: scaleY(1); } }
.p4-cup {
  position: absolute; top: -36px; left: 50%; width: 28px; height: 28px; margin-left: -14px;
  fill: none; stroke: #fbbf24; stroke-width: 1.8; stroke-linejoin: round;
  opacity: 0; transform: translateY(-26px);
}
.l2-stop.active .p4-cup { animation: l4-drop 0.55s cubic-bezier(0.34, 1.56, 0.64, 1) 1.5s forwards; }
@keyframes l4-drop { to { opacity: 1; transform: translateY(0); } }
.p4-bot { position: absolute; right: 18px; bottom: 16px; width: 84px; }
.p4-conf { position: absolute; top: -12px; width: 5px; height: 10px; border-radius: 2px; opacity: 0; }
.p4-conf.c1 { left: 12%; background: #fbbf24; } .p4-conf.c2 { left: 26%; background: #8a7fff; }
.p4-conf.c3 { left: 40%; background: #34d399; } .p4-conf.c4 { left: 54%; background: #fbbf24; }
.p4-conf.c5 { left: 66%; background: #fb7185; } .p4-conf.c6 { left: 78%; background: #8a7fff; }
.p4-conf.c7 { left: 88%; background: #34d399; } .p4-conf.c8 { left: 33%; background: #fb7185; }
.l2-stop.active .p4-conf { animation: l4-fall 2.6s linear infinite; }
.l2-stop.active .p4-conf.c2 { animation-delay: 0.4s; } .l2-stop.active .p4-conf.c3 { animation-delay: 0.9s; }
.l2-stop.active .p4-conf.c4 { animation-delay: 1.3s; } .l2-stop.active .p4-conf.c5 { animation-delay: 0.2s; }
.l2-stop.active .p4-conf.c6 { animation-delay: 1.7s; } .l2-stop.active .p4-conf.c7 { animation-delay: 0.7s; }
.l2-stop.active .p4-conf.c8 { animation-delay: 2s; }
@keyframes l4-fall {
  0% { opacity: 0; transform: translateY(0) rotate(0deg); }
  8% { opacity: 1; }
  100% { opacity: 0.4; transform: translateY(420px) rotate(480deg); }
}

/* ════ brain ════ */
.l4-brain { position: relative; z-index: 2; max-width: 1180px; margin: 0 auto; padding: 70px 24px 110px; text-align: center; }
.l4-bstage { position: relative; display: flex; align-items: center; justify-content: center; margin-top: 50px; flex-wrap: wrap; }
.l4-bcol { display: flex; flex-direction: column; gap: 16px; }
.l4-scard {
  width: 250px; padding: 15px 17px; border-radius: 16px; text-align: left;
  background: var(--panel); border: 1px solid var(--bd);
  opacity: 0.45; transition: opacity 0.6s, border-color 0.6s;
}
.l4-brain.lit .l4-scard { opacity: 1; border-color: var(--bd2); }
.l4-sc-top { display: flex; align-items: center; gap: 10px; margin-bottom: 11px; }
.l4-sc-av {
  display: grid; place-items: center; width: 34px; height: 34px; border-radius: 11px;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 14px; color: #f5f3ff;
  background: linear-gradient(135deg, #3b2a84, #6b54e8);
}
.l4-sc-id { flex: 1; }
.l4-sc-id b { display: block; font-size: 13.5px; color: var(--tx1); }
.l4-sc-id i { display: block; font-style: normal; font-size: 11px; color: var(--tx3); }
.l4-sc-score { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 16px; }
.l4-bar { display: grid; grid-template-columns: 62px 1fr 34px; align-items: center; gap: 8px; margin-bottom: 6px; }
.l4-bar span { font-size: 10.5px; font-weight: 600; color: var(--tx3); }
.l4-bar b { font-size: 10.5px; font-weight: 700; color: var(--tx2); text-align: right; }
.l4-bar-track { display: block; height: 5px; border-radius: 99px; background: rgba(138, 127, 255, 0.15); overflow: hidden; }
.l4-bar-fill { display: block; height: 100%; border-radius: 99px; width: 0; transition: width 1.1s cubic-bezier(0.22, 1, 0.36, 1) 0.3s; }
.l4-bwire { position: relative; width: 56px; height: 2px; background: var(--bd2); overflow: hidden; }
.l4-pulse {
  position: absolute; top: 0; left: 0; width: 22px; height: 2px;
  background: linear-gradient(90deg, transparent, #fbbf24, transparent);
  animation: l4-pulsemove 1.8s linear infinite; animation-play-state: paused;
}
.l4-brain.lit .l4-pulse { animation-play-state: running; }
@keyframes l4-pulsemove { from { transform: translateX(-24px); } to { transform: translateX(62px); } }
.l4-bwire.flip .l4-pulse { animation-direction: reverse; }
.l4-bcore { position: relative; padding: 36px; display: grid; place-items: center; }
.l4-ring { position: absolute; border-radius: 50%; border: 1px dashed rgba(138, 127, 255, 0.3); }
.l4-ring.r1 { inset: 0; } .l4-ring.r2 { inset: 20px; border-color: rgba(251, 191, 36, 0.25); }
.l4-brain.lit .l4-ring.r1 { animation: l4-spin 24s linear infinite; }
.l4-brain.lit .l4-ring.r2 { animation: l4-spin 16s linear infinite reverse; }
.l4-engine {
  position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 7px;
  color: #a99cff; transition: color 0.6s;
}
.l4-brain.lit .l4-engine { color: #fbbf24; }
.l4-engine span { font-size: 8.5px; font-weight: 800; letter-spacing: 0.2em; }
.l4-eng-bar { width: 64px; height: 4px; border-radius: 99px; background: rgba(138, 127, 255, 0.2); overflow: hidden; position: relative; }
.l4-eng-bar::after { content: ''; position: absolute; inset: 0; border-radius: 99px; background: linear-gradient(90deg, #8a7fff, #fbbf24); transform: scaleX(0); transform-origin: left; transition: transform 1s ease 0.4s; }
.l4-brain.lit .l4-eng-bar.b1::after { transform: scaleX(0.9); }
.l4-brain.lit .l4-eng-bar.b2::after { transform: scaleX(0.7); transition-delay: 0.6s; }
.l4-brain.lit .l4-eng-bar.b3::after { transform: scaleX(0.8); transition-delay: 0.8s; }
@media (max-width: 980px) {
  .l4-bstage { flex-direction: column; gap: 18px; }
  .l4-bwire { width: 2px; height: 36px; }
  .l4-bcol { flex-direction: row; flex-wrap: wrap; justify-content: center; }
}

/* ════ headings + stats ════ */
.l2-h2 {
  font-family: 'Syne', sans-serif; font-weight: 800; letter-spacing: -0.03em;
  font-size: clamp(2.1rem, 5.4vw, 3.6rem); line-height: 1.06; margin: 14px 0 22px; color: var(--tx1);
}
.l2-h2.sm { font-size: clamp(1.7rem, 4vw, 2.6rem); }
.l2-h2 em { font-style: italic; background: linear-gradient(100deg, var(--grad-b), var(--grad-a)); -webkit-background-clip: text; background-clip: text; color: transparent; }
.l2-sub { max-width: 620px; margin: 0 auto 20px; font-size: clamp(1rem, 1.6vw, 1.15rem); line-height: 1.7; color: var(--tx2); }
.l2-sub strong { color: var(--tx1); }
.l2-stats {
  position: relative; z-index: 2; max-width: 1080px; margin: 0 auto; padding: 30px 24px 110px;
  display: grid; grid-template-columns: repeat(4, 1fr); gap: 18px; text-align: center;
}
.l2-stat {
  padding: 26px 10px; border-radius: 18px; background: var(--panel);
  border: 1px solid var(--bd); transition: border-color 0.6s, box-shadow 0.6s;
}
.l2-stats.lit .l2-stat { border-color: rgba(251, 191, 36, 0.3); box-shadow: 0 0 34px rgba(251, 191, 36, 0.07); animation: l4-statpop 0.55s cubic-bezier(0.34, 1.56, 0.64, 1) both; }
.l2-stats.lit .l2-stat:nth-child(3) { animation-delay: 0.08s; }
.l2-stats.lit .l2-stat:nth-child(4) { animation-delay: 0.16s; }
.l2-stats.lit .l2-stat:nth-child(5) { animation-delay: 0.24s; }
@keyframes l4-statpop { 0% { transform: scale(0.94); } 60% { transform: scale(1.035); } 100% { transform: scale(1); } }
.l2-stat-v {
  display: block; font-family: 'Syne', sans-serif; font-weight: 800; font-size: clamp(1.6rem, 3.2vw, 2.4rem);
  font-variant-numeric: tabular-nums;
  background: linear-gradient(120deg, var(--grad-a), var(--grad-b)); -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l2-stat-l { display: block; margin-top: 6px; font-size: 12.5px; font-weight: 600; letter-spacing: 0.06em; text-transform: uppercase; color: var(--tx3); }
@media (max-width: 760px) { .l2-stats { grid-template-columns: repeat(2, 1fr); } }

/* ════ pricing — checkout phone ════ */
.l4-pricing { position: relative; z-index: 2; max-width: 1080px; margin: 0 auto; padding: 30px 24px 120px; }
.l4-price-grid { display: grid; grid-template-columns: 1.2fr 1fr; gap: 56px; align-items: center; }
@media (max-width: 900px) { .l4-price-grid { grid-template-columns: 1fr; gap: 36px; } }
.l4-price-feats { margin: 18px 0 0; padding: 0; list-style: none; text-align: left; }
.l4-price-feats li { display: flex; align-items: flex-start; gap: 9px; margin-bottom: 10px; font-size: 14.5px; line-height: 1.55; color: var(--tx2); }
.l4-price-feats svg { flex-shrink: 0; margin-top: 3px; color: #34d399; }
.l4-price-phone { position: relative; display: flex; justify-content: center; }
.l4-price-badge {
  position: absolute; top: -14px; left: 50%; transform: translateX(-50%) scale(0.6); opacity: 0; z-index: 3;
  padding: 6px 16px; border-radius: 99px; font-size: 11px; font-weight: 800; letter-spacing: 0.1em;
  text-transform: uppercase; color: var(--btn-txt); background: linear-gradient(135deg, var(--grad-a), var(--grad-b));
}
.l4-pricing.lit .l4-price-badge { animation: l4-badgepop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1) 0.4s forwards; }
@keyframes l4-badgepop { to { opacity: 1; transform: translateX(-50%) scale(1); } }
.l4-pricing.lit :deep(.iph) { box-shadow: 0 34px 80px rgba(0, 0, 0, 0.55), 0 0 70px rgba(251, 191, 36, 0.22), inset 0 0 0 2px rgba(251, 191, 36, 0.4); transition: box-shadow 0.8s; }
.l4-checkout { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; padding: 48px 22px 0; }
.l4-checkout .ps-app { position: static; margin-bottom: 26px; }
.l4-co-amount {
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 44px; letter-spacing: -0.03em;
  background: linear-gradient(120deg, #a8a4ff, #fbbf24); -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l4-co-period { font-size: 12px; font-weight: 600; color: #8d86b8; margin-top: 2px; }
.l4-co-div { width: 100%; height: 1px; margin: 20px 0; background: rgba(138, 127, 255, 0.18); }
.l4-co-list { margin: 0 0 24px; padding: 0; list-style: none; align-self: stretch; }
.l4-co-list li { display: flex; align-items: center; gap: 8px; margin-bottom: 11px; font-size: 12.5px; font-weight: 600; color: #d9d5f2; }
.l4-co-list svg { color: #34d399; flex-shrink: 0; }
.l4-co-btn {
  display: inline-flex; align-items: center; justify-content: center; gap: 7px; align-self: stretch;
  padding: 13px 0; border-radius: 13px; font-size: 13.5px; font-weight: 800; color: #05030d;
  background: linear-gradient(135deg, #a8a4ff, #fbbf24);
}
.l4-co-note { margin-top: 12px; font-size: 10.5px; font-weight: 600; color: #5d5687; }

/* ════ n8n pipeline ════ */
.l2-n8n { position: relative; z-index: 2; max-width: 980px; margin: 0 auto; padding: 60px 24px 130px; text-align: center; }
.l2-pipe { display: flex; align-items: center; justify-content: center; gap: 0; margin-top: 54px; flex-wrap: wrap; }
.l2-pipe-node {
  display: flex; flex-direction: column; align-items: center; gap: 5px;
  padding: 18px 22px; border-radius: 16px; color: var(--tx2);
  background: var(--panel); border: 1px solid var(--bd);
  opacity: 0.4; transition: opacity 0.5s, border-color 0.5s, box-shadow 0.5s;
}
.l2-pipe.lit .l2-pipe-node { opacity: 1; }
.l2-pipe.lit .l2-pipe-node.is-hub { transition-delay: 0.35s; border-color: rgba(251, 191, 36, 0.45); color: #fbbf24; box-shadow: 0 0 32px rgba(251, 191, 36, 0.14); }
.l2-pipe.lit .l2-pipe-fan .l2-pipe-node:nth-child(1) { transition-delay: 0.7s; }
.l2-pipe.lit .l2-pipe-fan .l2-pipe-node:nth-child(2) { transition-delay: 0.9s; }
.l2-pipe.lit .l2-pipe-fan .l2-pipe-node:nth-child(3) { transition-delay: 1.1s; }
.l2-pipe-name { font-size: 13px; font-weight: 700; color: var(--tx1); }
.l2-pipe-sub { font-size: 11px; font-weight: 600; color: var(--tx3); font-variant-numeric: tabular-nums; }
.l2-pipe-wire { position: relative; width: 64px; height: 2px; background: var(--bd2); overflow: hidden; }
.l2-pipe-pulse {
  position: absolute; top: 0; left: 0; width: 22px; height: 2px;
  background: linear-gradient(90deg, transparent, #fbbf24, transparent);
  animation: l4-pulsemove 1.8s linear infinite; animation-play-state: paused;
}
.l2-pipe.lit .l2-pipe-pulse { animation-play-state: running; }
.l2-pipe-fan { display: flex; flex-direction: column; gap: 10px; }
.l2-pipe-fan .l2-pipe-node { flex-direction: row; padding: 11px 16px; gap: 9px; }
@media (max-width: 760px) {
  .l2-pipe { flex-direction: column; gap: 12px; }
  .l2-pipe-wire { width: 2px; height: 40px; }
  .l2-pipe-pulse { width: 2px; height: 18px; background: linear-gradient(180deg, transparent, #fbbf24, transparent); animation-name: l4-pulse-v; }
  @keyframes l4-pulse-v { from { transform: translateY(-20px); } to { transform: translateY(46px); } }
}

/* ════ form ════ */
.l2-join { position: relative; z-index: 2; max-width: 640px; margin: 0 auto; padding: 30px 24px 140px; }
.l2-form-card {
  padding: 44px 42px; border-radius: 26px; text-align: center;
  background: var(--panel); border: 1px solid var(--bd2);
  backdrop-filter: blur(10px); box-shadow: 0 30px 80px rgba(0, 0, 0, 0.45);
  transition: border-color 0.7s, box-shadow 0.7s;
}
.l2-form-card.lit { border-color: rgba(251, 191, 36, 0.45); box-shadow: 0 0 70px rgba(251, 191, 36, 0.1), 0 30px 80px rgba(0, 0, 0, 0.45); }
.l2-form-p { margin: 0 0 30px; font-size: 15px; line-height: 1.6; color: var(--tx2); }
.l2-form { display: flex; flex-direction: column; gap: 16px; text-align: left; }
.l2-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.l2-field { display: flex; flex-direction: column; gap: 7px; }
.l2-field label { font-size: 12.5px; font-weight: 700; letter-spacing: 0.07em; text-transform: uppercase; color: var(--grad-a); }
.l2-field input, .l2-field select {
  padding: 13px 15px; border-radius: 12px; font: inherit; font-size: 15px; color: var(--tx1);
  background: var(--panel2); border: 1px solid var(--bd2);
  transition: border-color 0.2s, box-shadow 0.2s;
}
.l2-field input::placeholder { color: var(--tx3); }
.l2-field input:focus-visible, .l2-field select:focus-visible {
  outline: none; border-color: var(--grad-b); box-shadow: 0 0 0 3px rgba(251, 191, 36, 0.18);
}
.l2-btn-submit { justify-content: center; margin-top: 8px; }
.l2-spinner {
  width: 16px; height: 16px; border-radius: 99px;
  border: 2px solid color-mix(in srgb, var(--btn-txt) 30%, transparent); border-top-color: var(--btn-txt);
  animation: l4-spin 0.7s linear infinite;
}
.l2-form-err {
  display: flex; align-items: center; flex-wrap: wrap; gap: 6px; margin: 4px 0 0;
  padding: 11px 14px; border-radius: 11px; font-size: 13px; line-height: 1.5; color: #fda4af;
  background: rgba(244, 63, 94, 0.08); border: 1px solid rgba(244, 63, 94, 0.25);
}
.l2-form-err code { font-size: 12px; color: #fecdd3; word-break: break-all; }
.l2-form-done { display: flex; flex-direction: column; align-items: center; gap: 6px; padding: 12px 0; }
.l2-done-badge {
  display: grid; place-items: center; width: 64px; height: 64px; border-radius: 9999px; margin-bottom: 14px;
  color: var(--btn-txt); background: linear-gradient(135deg, var(--grad-a), var(--grad-b));
  box-shadow: 0 0 40px rgba(251, 191, 36, 0.4);
}
@media (max-width: 560px) { .l2-form-card { padding: 32px 22px; } .l2-row { grid-template-columns: 1fr; } }

/* ════ footer ════ */
.l4-footer {
  position: relative; z-index: 2; padding: 20px 24px 56px; text-align: center;
  display: flex; flex-direction: column; align-items: center; gap: 14px;
  border-top: 1px solid var(--bd);
}
.l4-footer p { margin: 0; font-size: 13.5px; color: var(--tx3); }
.l4-foot-links { display: flex; gap: 20px; flex-wrap: wrap; justify-content: center; }
.l4-foot-links a { font-size: 13px; font-weight: 600; color: var(--grad-a); text-decoration: none; transition: color 0.2s; }
.l4-foot-links a:hover { color: var(--gold); }
.l2-line-end { margin-bottom: 18px; }

/* ════ reveal ════ */
.reveal { opacity: 0; transform: translateY(26px) scale(0.985); filter: blur(5px); transition: opacity 0.7s ease, transform 0.7s ease, filter 0.7s ease; }
.reveal[data-in] { opacity: 1; transform: none; filter: none; }

/* ════ reduced motion ════ */
@media (prefers-reduced-motion: reduce) {
  .l2 *, .l2 *::before, .l2 *::after { animation: none !important; transition: none !important; }
  .reveal { opacity: 1; transform: none; filter: none; }
  .gx { opacity: 1 !important; transform: none !important; }
  .sc { filter: none; }
  .sc-standby { opacity: 0; }
  .p1-eq { width: 100% !important; border-right-color: transparent !important; }
  .p3-ring .ring-fg { stroke-dashoffset: 26 !important; }
  .p4-conf { opacity: 0 !important; }
  .p4-rank { opacity: 1 !important; transform: translateX(-50%) !important; }
  .l4-price-badge { opacity: 1; transform: translateX(-50%) scale(1); }
  .l2-pipe-node, .l4-scard { opacity: 1; }
  .ps0-flip { animation: none; }
  .ps1-row { opacity: 1; animation: none; }
}
</style>
