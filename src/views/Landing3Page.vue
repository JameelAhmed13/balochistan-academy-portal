<template>
  <div ref="pageEl" class="l2 l3" @mousemove="onMouseMove">

    <!-- ── atmosphere: neural canvas (L1) · aurora · cursor spotlight ── -->
    <canvas ref="bgCanvas" class="l3-canvas" aria-hidden="true" />
    <div class="l2-aurora" aria-hidden="true"><i class="a1" /><i class="a2" /><i class="a3" /></div>
    <div class="l2-spotlight" :style="{ background: `radial-gradient(480px circle at ${mx}px ${my}px, rgba(124,106,245,0.09), transparent 65%)` }" />

    <!-- ── Scroll-drawn journey line ── -->
    <svg
      class="l2-line-svg"
      aria-hidden="true"
      :width="svgW"
      :height="svgH"
      :viewBox="`0 0 ${svgW} ${svgH}`"
      fill="none"
    >
      <defs>
        <linearGradient id="l3-line-grad" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" :y2="svgH">
          <stop offset="0%"  stop-color="#8a7fff" />
          <stop offset="45%" stop-color="#7c6af5" />
          <stop offset="75%" stop-color="#fbbf24" />
          <stop offset="100%" stop-color="#f59e0b" />
        </linearGradient>
        <filter id="l3-line-glow" x="-50%" y="-50%" width="200%" height="200%">
          <feGaussianBlur stdDeviation="6" result="blur" />
          <feMerge>
            <feMergeNode in="blur" /><feMergeNode in="SourceGraphic" />
          </feMerge>
        </filter>
      </defs>
      <path :d="pathD" class="l2-path-ghost" />
      <path
        ref="lineEl"
        :d="pathD"
        class="l2-path-main l3-path"
        filter="url(#l3-line-glow)"
        :stroke-dasharray="pathLen"
        :stroke-dashoffset="dashOffset"
      />
    </svg>

    <div v-show="tipVisible" class="l2-tip" :style="{ transform: `translate(${tip.x}px, ${tip.y}px)` }">
      <span class="l2-tip-core" /><span class="l2-tip-halo" />
    </div>

    <!-- ── NAV ── -->
    <nav class="l2-nav" :class="{ scrolled: scrolled }">
      <RouterLink to="/" class="l2-logo">
        <img src="@/assets/logo.png" alt="Balochistan Academy" class="l2-logo-mark" />
        <span class="l2-logo-text">Balochistan Academy<em>·Ultra</em></span>
      </RouterLink>
      <div class="l2-nav-right">
        <a href="#features" class="l2-nav-lnk">Features</a>
        <a href="#tutors" class="l2-nav-lnk">Tutors</a>
        <a href="#pricing" class="l2-nav-lnk">Pricing</a>
        <a href="#join" class="l2-nav-lnk">Join</a>
        <button class="l2-nav-theme" :aria-label="themeStore.isDark ? 'Switch to light mode' : 'Switch to dark mode'" @click="themeStore.toggle()">
          <Sun v-if="themeStore.isDark" :size="15" />
          <Moon v-else :size="15" />
        </button>
        <RouterLink to="/login" class="l2-nav-cta">Start Free <ArrowRight :size="15" /></RouterLink>
      </div>
    </nav>

    <!-- ── HERO — L1 split layout + 3D tilt slider, L2 robots ── -->
    <header class="l3-hero">
      <div class="l3-hero-grid">
        <!-- left: rotating copy -->
        <div class="l3-hero-copy">
          <p class="l2-kicker reveal"><span class="l2-kicker-dot" /> Pakistan's #1 AI Exam Platform · Grade 9</p>
          <Transition name="slide-txt" mode="out-in">
            <div :key="currentSlide" class="l3-htext">
              <h1 class="l3-h1">
                <span v-for="(line, li) in slideData[currentSlide].heading" :key="li"
                  class="l3-h1-line" :class="{ gold: li === slideData[currentSlide].goldLine }">{{ line }}</span>
              </h1>
              <p class="l3-hero-p">{{ slideData[currentSlide].desc }}</p>
            </div>
          </Transition>
          <div class="l2-hero-btns reveal" style="justify-content: flex-start; margin-bottom: 44px;">
            <RouterLink to="/login" class="l2-btn-primary">Start Learning Free <ArrowRight :size="16" /></RouterLink>
            <a href="#path" class="l2-btn-ghost"><ArrowDown :size="16" /> Trace the path</a>
          </div>
          <div class="l3-hstats reveal">
            <div v-for="s in heroStats" :key="s.label" class="l3-hstat">
              <span class="l3-hstat-v">{{ s.val }}</span>
              <span class="l3-hstat-l">{{ s.label }}</span>
            </div>
          </div>
        </div>

        <!-- right: 3D tilt slider card -->
        <div class="l3-slider-wrap reveal">
          <div
            ref="sliderRef"
            class="l3-scene"
            @mousemove="onSliderMouseMove"
            @mouseleave="onSliderMouseLeave"
            @mouseenter="sliderHovering = true"
          >
            <div class="l3-tilt" :style="tiltStyle">
              <div class="l3-card">
                <div class="l3-sheen" :style="sheenStyle" />
                <div class="l3-card-head">
                  <span class="l3-brand"><span class="l3-brand-dot" />Balochistan Academy</span>
                  <span class="l3-status" :class="`tone-${sceneMeta[currentSlide].tone}`">
                    <i />{{ sceneMeta[currentSlide].label }}
                  </span>
                </div>

                <Transition :name="s3dName">
                  <div :key="currentSlide" class="l3-slide">

                    <!-- slide 0 · flashcard deck -->
                    <div v-if="currentSlide === 0" class="sl-fc">
                      <div class="fc3-flip">
                        <div class="fc3-face fc3-front">
                          <span class="fc3-tag">PHYSICS · CH 3</span>
                          <p class="fc3-q">If <b>F = 10 N</b> and <b>m = 2 kg</b>, find the acceleration <b>a</b>.</p>
                          <span class="fc3-hint">flips automatically…</span>
                        </div>
                        <div class="fc3-face fc3-back">
                          <span class="fc3-tag gold">ANSWER</span>
                          <p class="fc3-q">a = F / m = <b>5 m/s²</b></p>
                          <span class="fc3-coin"><Check :size="12" /> Correct · +50 coins</span>
                        </div>
                      </div>
                      <Bot pose="bot-point" class="fc3-bot" />
                      <div class="fc3-pills">
                        <span class="active">Physics</span><span>Chemistry</span><span>Biology</span><span class="urdu">اردو</span><span>Math</span>
                      </div>
                    </div>

                    <!-- slide 1 · AI grading -->
                    <div v-else-if="currentSlide === 1" class="sl-gr">
                      <div class="gr3-sheet">
                        <span class="gr3-title">Answer Sheet · Physics</span>
                        <div v-for="(r, ri) in gradeRows" :key="r.q" class="gr3-row" :style="{ animationDelay: `${ri * 0.35 + 0.2}s` }">
                          <span class="gr3-q">{{ r.q }}</span>
                          <span v-if="r.pending" class="gr3-pen">analyzing…</span>
                          <span v-else class="gr3-mark"><Check :size="11" /> {{ r.mark }}</span>
                        </div>
                        <div class="gr3-note">AI · revise <b>Q3 Kinematics</b> — 2 more MCQs</div>
                      </div>
                      <div class="gr3-side">
                        <div class="gr3-ringwrap">
                          <svg viewBox="0 0 80 80" class="gr3-ring">
                            <circle class="ring-bg" cx="40" cy="40" r="34" />
                            <circle class="ring-fg" cx="40" cy="40" r="34"
                              :stroke-dasharray="ringCirc"
                              :stroke-dashoffset="ringCirc * (1 - scoreVal / 100)" />
                          </svg>
                          <span class="gr3-num">{{ scoreVal }}<i>%</i></span>
                        </div>
                        <span class="gr3-lbl">auto-graded</span>
                        <Bot pose="bot-write" class="gr3-bot" />
                      </div>
                    </div>

                    <!-- slide 2 · live tutor -->
                    <div v-else class="sl-lt">
                      <div class="lt3-head">
                        <Bot pose="bot-wave" class="lt3-bot" />
                        <div><b>Ghalib AI</b><span class="urdu">اُردو ادب · Classical Poetry</span></div>
                      </div>
                      <div class="lt3-chat">
                        <div class="lt3-bub user urdu">شعر کی تشریح سمجھائیں؟</div>
                        <div class="lt3-bub typing"><span /><span /><span /></div>
                        <div class="lt3-bub ai">
                          <span class="urdu">یہ شعر اُمید اور حوصلے کی بات کرتا ہے</span>
                          <em>“It speaks of hope &amp; resolve.”</em>
                        </div>
                      </div>
                      <div class="lt3-input"><span>Ask anything…</span><Send :size="13" /></div>
                    </div>

                  </div>
                </Transition>
              </div>
            </div>
          </div>

          <button class="l3-arrow prev" @click="prevSlide" aria-label="Previous slide">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M15 18l-6-6 6-6" /></svg>
          </button>
          <button class="l3-arrow next" @click="nextSlide" aria-label="Next slide">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 18l6-6-6-6" /></svg>
          </button>
          <div class="l3-dots">
            <button v-for="(_, i) in slideData" :key="i" class="l3-dot" :class="{ active: currentSlide === i }" :aria-label="`Slide ${i + 1}`" @click="goToSlide(i)" />
            <span class="l3-dot-lbl">{{ ['Flashcard Deck', 'AI Grading', 'Live Tutor'][currentSlide] }}</span>
          </div>
        </div>
      </div>
      <div ref="lineStartEl" class="l2-line-start" aria-hidden="true" />
    </header>

    <!-- ── SUBJECT TICKER (L1) ── -->
    <div class="l3-ticker" aria-hidden="true">
      <div class="l3-ticker-track">
        <template v-for="n in 2" :key="n">
          <span v-for="s in tickerSubjects" :key="`${n}-${s}`" class="l3-tick">{{ s }}<i /></span>
        </template>
      </div>
    </div>

    <!-- ── JOURNEY STATIONS (L2 robot scenes) ── -->
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
          <div class="sc" aria-hidden="true">
            <div class="sc-head">
              <span>{{ m.scene }}</span>
              <span class="sc-live"><i />{{ i < activeStops ? 'ONLINE' : 'STANDBY' }}</span>
            </div>
            <div class="sc-standby">— STANDBY —</div>
            <span class="sc-scan" />

            <div v-if="i === 0" class="sc-stage">
              <Bot pose="bot-point" class="sc1-bot" />
              <div class="sc1-bubble gx">Concept clear? Try one yourself!</div>
              <div class="sc1-board">
                <span class="sc1-board-tag">PHYSICS · NEWTON'S 2nd LAW</span>
                <i class="sc1-tl sc1-tl1 gx" /><i class="sc1-tl sc1-tl2 gx" />
                <div class="sc1-eq gx">a&nbsp;=&nbsp;F&nbsp;/&nbsp;m&nbsp;=&nbsp;5&nbsp;m/s²</div>
              </div>
            </div>

            <div v-else-if="i === 1" class="sc-stage">
              <div class="sc2-paper">
                <span class="sc2-ptitle">MODEL PAPER · 9th</span>
                <i class="sc2-pl sc2-pl1 gx" /><i class="sc2-pl sc2-pl2 gx" /><i class="sc2-pl sc2-pl3 gx" />
                <div class="sc2-mcq">
                  <span v-for="(o, oi) in ['A', 'B', 'C', 'D']" :key="o" class="sc2-opt gx" :class="{ pick: oi === 1 }">{{ o }}</span>
                </div>
              </div>
              <svg class="sc2-gear" viewBox="0 0 36 36"><circle cx="18" cy="18" r="10" /></svg>
              <Bot pose="bot-write" class="sc2-bot" />
              <i class="sc-spark s1" /><i class="sc-spark s2" /><i class="sc-spark s3" />
            </div>

            <div v-else-if="i === 2" class="sc-stage">
              <div class="sc3-rows">
                <div class="sc3-row">
                  <i class="sc3-bar" style="width: 82%" />
                  <span class="sc3-stamp ok gx"><svg viewBox="0 0 24 24"><path d="M5 13l4 4L19 7" /></svg></span>
                </div>
                <div class="sc3-row">
                  <i class="sc3-bar" style="width: 68%" />
                  <span class="sc3-stamp ok gx" style="animation-delay: 1s"><svg viewBox="0 0 24 24"><path d="M5 13l4 4L19 7" /></svg></span>
                </div>
                <div class="sc3-row">
                  <i class="sc3-bar" style="width: 74%" />
                  <span class="sc3-stamp no gx"><svg viewBox="0 0 24 24"><path d="M6 6l12 12M18 6L6 18" /></svg></span>
                </div>
                <i class="sc3-fix gx" />
              </div>
              <div class="sc3-ringwrap">
                <svg class="sc3-ring" viewBox="0 0 84 84">
                  <circle class="ring-bg" cx="42" cy="42" r="32" />
                  <circle class="ring-fg" cx="42" cy="42" r="32" />
                </svg>
                <span class="sc3-score gx">87%</span>
              </div>
              <Bot pose="bot-write" class="sc3-bot" />
            </div>

            <div v-else class="sc-stage">
              <i v-for="c in 8" :key="c" class="sc4-conf" :class="`c${c}`" />
              <span class="sc4-rank gx">RANK #01 · QUETTA</span>
              <div class="sc4-podium">
                <div class="sc4-col p2 gx"><b>2</b></div>
                <div class="sc4-col p1 gx">
                  <svg class="sc4-cup gx" viewBox="0 0 24 24"><path d="M7 3h10v5a5 5 0 0 1-10 0V3zM7 5H4a3 3 0 0 0 3 4M17 5h3a3 3 0 0 1-3 4M11 13h2v3h-2zM8 16h8v3H8z" /></svg>
                  <b>1</b>
                </div>
                <div class="sc4-col p3 gx"><b>3</b></div>
              </div>
              <Bot pose="bot-cheer" class="sc4-bot" />
            </div>
          </div>

          <span class="l2-stop-step">{{ String(i + 1).padStart(2, '0') }}</span>
          <span class="l2-stop-icon"><component :is="m.icon" :size="22" /></span>
          <h3 class="l2-stop-h">{{ m.title }}</h3>
          <p class="l2-stop-p">{{ m.desc }}</p>
          <ul class="l2-stop-chips">
            <li v-for="c in m.chips" :key="c"><Check :size="12" /> {{ c }}</li>
          </ul>
        </article>
      </div>
    </section>

    <!-- ── AI BRAIN WATCHING STUDENTS (L1, line passes through the core) ── -->
    <section class="l3-brain" :class="{ lit: brainOn }">
      <p class="l2-kicker reveal"><BrainCircuit :size="14" /> AI Intelligence</p>
      <h2 class="l2-h2 reveal">Watch AI study <em>your students.</em></h2>
      <p class="l2-sub reveal">Every answer feeds the engine — it tracks, adapts and personalizes each session in real time.</p>

      <div class="l3-bstage reveal">
        <div class="l3-bcol">
          <div v-for="st in students.slice(0, 2)" :key="st.name" class="l3-scard">
            <div class="l3-sc-top">
              <span class="l3-sc-av">{{ st.name[0] }}</span>
              <span class="l3-sc-id"><b>{{ st.name }}</b><i>{{ st.subject }}</i></span>
              <span class="l3-sc-score" :style="{ color: st.color }">{{ st.score }}</span>
            </div>
            <div v-for="bar in st.bars" :key="bar.label" class="l3-bar">
              <span>{{ bar.label }}</span>
              <i class="l3-bar-track"><i class="l3-bar-fill" :style="{ width: brainOn ? bar.pct + '%' : '0%', background: bar.color }" /></i>
              <b>{{ bar.pct }}%</b>
            </div>
          </div>
        </div>

        <div class="l3-bwire"><span class="l2-pipe-pulse" /></div>

        <div class="l3-bcore">
          <div ref="brainAnchorEl" class="l2-center-anchor is-abs" aria-hidden="true" />
          <i class="l3-ring r1" /><i class="l3-ring r2" /><i class="l3-ring r3" />
          <div class="l3-bcore-in">
            <BrainCircuit :size="34" />
            <span>AI ENGINE</span>
          </div>
        </div>

        <div class="l3-bwire flip"><span class="l2-pipe-pulse" style="animation-delay: .7s" /></div>

        <div class="l3-bcol">
          <div v-for="st in students.slice(2)" :key="st.name" class="l3-scard">
            <div class="l3-sc-top">
              <span class="l3-sc-av">{{ st.name[0] }}</span>
              <span class="l3-sc-id"><b>{{ st.name }}</b><i>{{ st.subject }}</i></span>
              <span class="l3-sc-score" :style="{ color: st.color }">{{ st.score }}</span>
            </div>
            <div v-for="bar in st.bars" :key="bar.label" class="l3-bar">
              <span>{{ bar.label }}</span>
              <i class="l3-bar-track"><i class="l3-bar-fill" :style="{ width: brainOn ? bar.pct + '%' : '0%', background: bar.color }" /></i>
              <b>{{ bar.pct }}%</b>
            </div>
          </div>
        </div>

        <span class="l3-badge b1 gx"><Target :size="12" /> Weakness detected: Kinematics</span>
        <span class="l3-badge b2 gx"><Lightbulb :size="12" /> Recommended: 5 more MCQs</span>
        <span class="l3-badge b3 gx"><TrendingUp :size="12" /> +18% this week</span>
      </div>
    </section>

    <!-- ── STATS (L2 count-up) ── -->
    <section class="l2-stats" :class="{ lit: statsOn }">
      <div ref="statsAnchorEl" class="l2-center-anchor is-abs" aria-hidden="true" />
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[0].toLocaleString('en-US') }}+</span><span class="l2-stat-l">Students on the path</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[1] }}K+</span><span class="l2-stat-l">AI-graded answers</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">{{ statCounters[2] }}%</span><span class="l2-stat-l">Improved board scores</span></div>
      <div class="l2-stat reveal"><span class="l2-stat-v">24/7</span><span class="l2-stat-l">AI tutor on call</span></div>
    </section>

    <!-- ── FEATURES (L1 grid, L2 skin) ── -->
    <section id="features" class="l3-features" :class="{ lit: featOn }">
      <div ref="featAnchorEl" class="l2-center-anchor is-abs off-left" aria-hidden="true" />
      <p class="l2-kicker reveal"><Layers :size="14" /> Everything you need</p>
      <h2 class="l2-h2 reveal">One platform.<br /><em>Infinite possibilities.</em></h2>
      <div class="l3-feat-grid">
        <div v-for="(f, i) in features" :key="f.title" class="l3-feat reveal" :style="{ transitionDelay: `${i * 0.06}s` }">
          <span class="l3-feat-ic"><component :is="f.icon" :size="20" /></span>
          <h3>{{ f.title }}</h3>
          <p>{{ f.desc }}</p>
        </div>
      </div>
    </section>

    <!-- ── TUTOR PERSONAS (L1 marquee) ── -->
    <section id="tutors" class="l3-tutors">
      <p class="l2-kicker reveal"><GraduationCap :size="14" /> 8 legendary personas</p>
      <h2 class="l2-h2 reveal">Learn from the <em>greatest minds.</em></h2>
      <div class="l3-tut-marquee" aria-hidden="true">
        <div class="l3-tut-track">
          <div v-for="(t, ti) in [...tutors, ...tutors]" :key="ti" class="l3-tut-chip">
            <span class="l3-tut-av" :style="{ background: t.grad }">{{ t.name[0] }}</span>
            <span class="l3-tut-id"><b>{{ t.name }}</b><i>{{ t.subject }}</i></span>
          </div>
        </div>
      </div>
      <!-- full persona grid (from Landing 1) -->
      <div class="l3-tut-grid">
        <div v-for="(t, ti) in tutors" :key="t.name" class="l3-tg-card reveal" :style="{ transitionDelay: `${ti * 0.07}s` }">
          <div class="l3-tg-head" :style="{ background: t.grad }">
            <span class="l3-tg-glyph">{{ t.name[0] }}</span>
            <i class="l3-tg-shine" />
          </div>
          <div class="l3-tg-body">
            <b>{{ t.name }}</b>
            <i>{{ t.subject }}</i>
            <p :class="{ urdu: t.name === 'Ghalib' }">“{{ t.quote }}”</p>
            <div class="l3-tg-actions">
              <RouterLink to="/login" class="l3-tg-btn chat"><MessageSquare :size="13" /> Chat</RouterLink>
              <RouterLink to="/login" class="l3-tg-btn video"><Video :size="13" /> Video</RouterLink>
            </div>
          </div>
        </div>
      </div>
      <p class="l3-tut-note reveal">Chat or video, English &amp; Urdu medium — day or night, right up to exam day.</p>
    </section>

    <!-- ── PRICING (L1, line plugs into the card) ── -->
    <section id="pricing" class="l3-pricing" :class="{ lit: priceOn }">
      <div class="l3-price-grid">
        <div class="l3-price-copy">
          <p class="l2-kicker reveal"><Coins :size="14" /> Affordable for every family</p>
          <h2 class="l2-h2 reveal" style="text-align:left">Full access<br /><em>Rs. 999 / year.</em></h2>
          <p class="l2-sub reveal" style="margin-left:0; text-align:left">Under Rs. 83 a month — one subscription unlocks everything.</p>
          <ul class="l3-price-feats reveal">
            <li v-for="f in pricingFeats" :key="f"><Check :size="14" /> {{ f }}</li>
          </ul>
        </div>
        <div class="l3-price-cardwrap">
          <div ref="priceAnchorEl" class="l2-center-anchor is-abs" aria-hidden="true" />
          <div class="l3-price-card reveal">
            <span class="l3-price-badge gx">Most Popular</span>
            <span class="l3-price-amount">Rs. 999</span>
            <span class="l3-price-period">per year · unlimited access</span>
            <i class="l3-price-div" />
            <span class="l3-price-note">Easypaisa · JazzCash · Bank Transfer</span>
            <RouterLink to="/login" class="l2-btn-primary" style="justify-content:center; width:100%">Start Now <ArrowRight :size="15" /></RouterLink>
            <a href="https://wa.me/923001234567" target="_blank" rel="noopener" class="l3-wa">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor"><path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" /></svg>
              Pay via WhatsApp
            </a>
          </div>
        </div>
      </div>
    </section>

    <!-- ── TESTIMONIALS (L1) ── -->
    <section class="l3-testi">
      <p class="l2-kicker reveal"><Star :size="14" /> Student reviews</p>
      <h2 class="l2-h2 reveal">Real students. <em>Real results.</em></h2>
      <div class="l3-testi-grid">
        <figure v-for="t in testimonials" :key="t.name" class="l3-testi-card reveal">
          <span class="l3-stars">★★★★★</span>
          <blockquote>“{{ t.text }}”</blockquote>
          <figcaption>
            <span class="l3-testi-av">{{ t.name[0] }}</span>
            <span><b>{{ t.name }}</b><i>{{ t.school }}</i></span>
          </figcaption>
        </figure>
      </div>
    </section>

    <!-- ── n8n AUTOMATION (L2) ── -->
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
        <div class="l2-pipe-node is-src">
          <Send :size="18" />
          <span class="l2-pipe-name">Landing3 Form</span>
          <span class="l2-pipe-sub">POST · JSON</span>
        </div>
        <div class="l2-pipe-wire"><span class="l2-pipe-pulse" /></div>
        <div class="l2-pipe-node is-hub">
          <Webhook :size="18" />
          <span class="l2-pipe-name">n8n Webhook</span>
          <span class="l2-pipe-sub">localhost:5678</span>
        </div>
        <div class="l2-pipe-wire"><span class="l2-pipe-pulse" style="animation-delay: .6s" /></div>
        <div class="l2-pipe-fan">
          <div class="l2-pipe-node is-dst"><Database :size="16" /><span class="l2-pipe-name">Sheet / DB</span></div>
          <div class="l2-pipe-node is-dst"><Bell :size="16" /><span class="l2-pipe-name">WhatsApp Alert</span></div>
          <div class="l2-pipe-node is-dst"><Zap :size="16" /><span class="l2-pipe-name">Anything else</span></div>
        </div>
      </div>
    </section>

    <!-- ── LEAD FORM (L2) ── -->
    <section id="join" class="l2-join">
      <div ref="formAnchorEl" class="l2-center-anchor" aria-hidden="true" />
      <div class="l2-form-card reveal" :class="{ lit: formOn }">
        <template v-if="form.status !== 'success'">
          <h2 class="l2-h2 sm">End of the line.<br /><em>Start of yours.</em></h2>
          <p class="l2-form-p">Drop your details — they land directly in your local n8n workflow.</p>

          <form class="l2-form" @submit.prevent="submitLead">
            <div class="l2-field">
              <label for="l3-name">Student name</label>
              <input id="l3-name" v-model.trim="form.name" type="text" required autocomplete="name" placeholder="e.g. Ayesha Khan" />
            </div>
            <div class="l2-field">
              <label for="l3-phone">Phone / WhatsApp</label>
              <input id="l3-phone" v-model.trim="form.phone" type="tel" required autocomplete="tel" placeholder="03xx-xxxxxxx" />
            </div>
            <div class="l2-row">
              <div class="l2-field">
                <label for="l3-grade">Grade</label>
                <select id="l3-grade" v-model="form.grade">
                  <option value="9">Grade 9</option>
                  <option value="10">Grade 10</option>
                </select>
              </div>
              <div class="l2-field">
                <label for="l3-city">City</label>
                <input id="l3-city" v-model.trim="form.city" type="text" placeholder="Quetta" />
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

    <!-- ── FOOTER (L1 columns, L2 skin) ── -->
    <footer class="l3-footer">
      <div ref="lineEndEl" class="l2-line-end" aria-hidden="true" />
      <div class="l3-foot-grid">
        <div class="l3-foot-brand">
          <img src="@/assets/logo.png" alt="" class="l2-logo-mark sm" />
          <p>AI-powered exam preparation for Grade 9 Balochistan Board students — English &amp; Urdu medium.</p>
          <i>New Century Educational System</i>
        </div>
        <nav class="l3-foot-col" aria-label="Platform">
          <b>Platform</b>
          <a href="#features">Features</a><a href="#tutors">AI Tutors</a><a href="#pricing">Pricing</a><a href="#join">Join</a>
        </nav>
        <nav class="l3-foot-col" aria-label="Versions">
          <b>Landings</b>
          <RouterLink to="/">Classic (v1)</RouterLink>
          <RouterLink to="/landing2">The Path (v2)</RouterLink>
          <RouterLink to="/login">Student Login</RouterLink>
        </nav>
        <nav class="l3-foot-col" aria-label="Contact">
          <b>Contact</b>
          <a href="https://wa.me/923001234567" target="_blank" rel="noopener">WhatsApp Support</a>
        </nav>
      </div>
      <p class="l3-foot-bottom">© 2026 Balochistan Academy Portal · New Century Educational System</p>
    </footer>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount, nextTick, h } from 'vue'
import {
  ArrowRight, ArrowDown, Check, X, Send, Zap, Bell, Database,
  Webhook, Workflow, Bot as BotIcon, Sparkles, ClipboardCheck, Trophy,
  BrainCircuit, Target, Lightbulb, TrendingUp, Layers, GraduationCap,
  Coins, Star, FileText, Video, PenLine, Library, Sun, Moon, MessageSquare,
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

/* ── hero slider (from Landing 1) ────────────────────────── */
const currentSlide = ref(0)
const direction = ref(1)
const sliderHovering = ref(false)
const sliderRef = ref(null)
const cardMx = ref(0), cardMy = ref(0)
let sliderInterval = null

const s3dName = computed(() => (direction.value > 0 ? 's3d-next' : 's3d-prev'))
const tiltStyle = computed(() => {
  const rx = sliderHovering.value ? cardMy.value * -10 : 0
  const ry = sliderHovering.value ? cardMx.value * 14 : 0
  return { transform: `perspective(1200px) rotateX(${rx}deg) rotateY(${ry}deg)` }
})
const sheenStyle = computed(() => {
  if (!sliderHovering.value) return {}
  const px = (cardMx.value + 0.5) * 100
  const py = (cardMy.value + 0.5) * 100
  return { background: `radial-gradient(280px circle at ${px}% ${py}%, rgba(255,255,255,0.07), transparent 60%)` }
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
    desc: 'Bite-size AI flashcards for every Grade 9 chapter — question on the front, a legendary tutor’s explanation on the back.' },
  { heading: ['Submit Once.', 'AI Grades It', 'in Seconds.'], goldLine: 1,
    desc: 'Snap or type your answer and watch it marked instantly — score, mistakes and a personalised fix-list.' },
  { heading: ['Ask Anything,', 'in Urdu or', 'English.'], goldLine: 1,
    desc: '8 legendary AI tutors answer live — chat or voice, English & Urdu medium, available 24/7 right up to exam day.' },
]
const sceneMeta = [
  { label: 'FLASHCARDS', tone: 'violet' },
  { label: 'AI MARKING', tone: 'cyan' },
  { label: 'LIVE SESSION', tone: 'green' },
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

/* ── content data (merged L1 + L2) ───────────────────────── */
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

const features = [
  { icon: BotIcon, title: '8 AI Tutors', desc: 'Einstein, Ghalib, Curie & more — each subject gets a legendary AI mentor.' },
  { icon: FileText, title: 'Daily Tests', desc: 'Auto-generated MCQ and subjective tests with instant AI grading.' },
  { icon: Trophy, title: 'Competition', desc: 'Monthly institute-wide exams with leaderboards and mega challenges.' },
  { icon: Coins, title: 'Coins & Rewards', desc: 'Earn real coins for test performance. Withdraw via Easypaisa.' },
  { icon: Video, title: 'Video Lessons', desc: 'AI-voiced video lessons with voice Q&A powered by Google TTS.' },
  { icon: TrendingUp, title: 'Reports', desc: 'Detailed charts for students, parents and teachers in one dashboard.' },
  { icon: PenLine, title: 'Subjective AI', desc: 'Write long answers with voice input. Gemini grades & gives feedback.' },
  { icon: Library, title: 'Digital Library', desc: '10 subjects, key notes, past papers, simulations and virtual labs.' },
]

const tutors = [
  { name: 'Einstein', subject: 'Physics', grad: 'linear-gradient(135deg,#1e1a40,#4a35a6)', quote: 'Imagination is the key to discovery.' },
  { name: 'Al-Khwarizmi', subject: 'Mathematics', grad: 'linear-gradient(135deg,#0d2a0d,#16a34a)', quote: 'Every problem has a beautiful solution.' },
  { name: 'Marie Curie', subject: 'Chemistry', grad: 'linear-gradient(135deg,#2a0d1a,#be185d)', quote: 'Nothing is to be feared — only understood.' },
  { name: 'Ibn Sina', subject: 'Biology', grad: 'linear-gradient(135deg,#0d2233,#0891b2)', quote: 'The body and soul work as one.' },
  { name: 'Turing', subject: 'Computer Sci', grad: 'linear-gradient(135deg,#1e2a0d,#4d7c0f)', quote: 'We can only see ahead if we look.' },
  { name: 'Allama Iqbal', subject: 'Pak Studies', grad: 'linear-gradient(135deg,#1a1200,#b45309)', quote: "Nations are born in poets' hearts." },
  { name: 'Shakespeare', subject: 'English', grad: 'linear-gradient(135deg,#2a1a00,#92400e)', quote: 'We know what we are, not what we may be.' },
  { name: 'Ghalib', subject: 'Urdu', grad: 'linear-gradient(135deg,#2a0d0d,#991b1b)', quote: 'ہزاروں خواہشیں ایسی…' },
]

const pricingFeats = [
  'Unlimited AI Tutor sessions (all 8 subjects)',
  'Full question bank — 1,200+ MCQ + Subjective',
  'AI-graded answers with detailed feedback',
  'Daily & monthly competitions with leaderboards',
  'Coin rewards redeemable for real cash',
  'Parent-visible performance reports',
  'Voice-driven AI video lessons',
  'English & Urdu medium support',
]

const testimonials = [
  { name: 'Ahmed Khan', school: 'New Century School, Quetta', text: 'Einstein AI explained Physics better than any teacher! My score jumped from 55% to 92% in one month.' },
  { name: 'Fatima Raza', school: 'Govt Girls High School, Quetta', text: 'The Ghalib AI tutor explains Urdu beautifully in proper Urdu with examples. I got A+ in my board exam!' },
  { name: 'Bilal Mengal', school: 'Balochistan Model School', text: 'I earned Rs. 650 in coins last month just by doing daily tests. My parents are so proud of me!' },
]

/* ── n8n lead form ───────────────────────────────────────── */
const webhookUrl = import.meta.env.VITE_N8N_WEBHOOK_URL || 'http://localhost:5678/webhook/estudy-lead'
const form = reactive({ name: '', phone: '', grade: '9', city: '', status: 'idle' })

async function submitLead() {
  form.status = 'sending'
  try {
    const res = await fetch(webhookUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: form.name, phone: form.phone, grade: form.grade, city: form.city,
        source: 'landing3', submittedAt: new Date().toISOString(),
      }),
    })
    if (!res.ok) throw new Error(`Webhook responded ${res.status}`)
    form.status = 'success'
  } catch (err) {
    console.error('[landing3] n8n webhook failed:', err)
    form.status = 'error'
  }
}

/* ── neural-net canvas background (from Landing 1) ───────── */
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

/* ── scroll-drawn SVG line (from Landing 2) ──────────────── */
const pageEl = ref(null)
const lineEl = ref(null)
const lineStartEl = ref(null)
const lineEndEl = ref(null)
const brainAnchorEl = ref(null)
const statsAnchorEl = ref(null)
const featAnchorEl = ref(null)
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
const featOn = ref(false)
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
let secY = { brain: Infinity, stats: Infinity, feat: Infinity, price: Infinity, n8n: Infinity, form: Infinity }
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
  const h = page.scrollHeight
  svgW.value = w
  svgH.value = h

  const pts = [{ x: w / 2, y: relY(lineStartEl.value) }]
  stopNodes.forEach((el) => { if (el) pts.push({ x: relX(el), y: relY(el) }) })
  const anchors = [brainAnchorEl, statsAnchorEl, featAnchorEl, priceAnchorEl, n8nAnchorEl, formAnchorEl]
  anchors.forEach((a) => { if (a.value) pts.push({ x: relX(a.value), y: relY(a.value) }) })
  pts.push({ x: w / 2, y: relY(lineEndEl.value) })

  stopYs = stopNodes.map((el) => (el ? relY(el) : Infinity))
  secY = {
    brain: brainAnchorEl.value ? relY(brainAnchorEl.value) : Infinity,
    stats: statsAnchorEl.value ? relY(statsAnchorEl.value) : Infinity,
    feat: featAnchorEl.value ? relY(featAnchorEl.value) : Infinity,
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
      brainOn.value = statsOn.value = featOn.value = priceOn.value = n8nOn.value = formOn.value = true
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
    if (!featOn.value && pt.y >= secY.feat - 10) featOn.value = true
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

/* scroll-driven reveal — deterministic, shrinks as elements come in */
let revealEls = []
function checkReveals() {
  if (!revealEls.length) return
  const vh = window.innerHeight
  revealEls = revealEls.filter((el) => {
    const r = el.getBoundingClientRect()
    // data attribute, not a class — Vue's :class patching would wipe a manual class
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
/* ════ base canvas (shared tokens with Landing 2) ════ */
.l2 {
  /* theme tokens — overridden under html.light below */
  --tx1: #f5f3ff;
  --tx2: #b9b3d9;
  --tx3: #8d86b8;
  --panel: rgba(15, 11, 34, 0.78);
  --panel2: rgba(5, 3, 13, 0.7);
  --bd: rgba(138, 127, 255, 0.16);
  --bd2: rgba(138, 127, 255, 0.3);
  --grad-a: #a8a4ff;
  --grad-b: #fbbf24;
  --gold: #fbbf24;
  --btn-txt: #05030d;
  --nav-bg: rgba(7, 5, 18, 0.72);
  position: relative;
  min-height: 100vh;
  overflow-x: clip;
  background:
    radial-gradient(1100px 600px at 85% -5%, rgba(124, 106, 245, 0.16), transparent 60%),
    radial-gradient(900px 500px at 10% 30%, rgba(251, 191, 36, 0.05), transparent 60%),
    radial-gradient(1000px 700px at 50% 105%, rgba(124, 106, 245, 0.12), transparent 60%),
    #05030d;
  color: var(--tx1);
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
}
html.light .l2 {
  --tx1: #1a1333;
  --tx2: #453a70;
  --tx3: #6f679c;
  --panel: rgba(255, 255, 255, 0.86);
  --panel2: #ffffff;
  --bd: rgba(91, 67, 204, 0.16);
  --bd2: rgba(91, 67, 204, 0.34);
  --grad-a: #6d54e8;
  --grad-b: #d97706;
  --gold: #b45309;
  --btn-txt: #ffffff;
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
html.light .l2-stop-step { -webkit-text-stroke-color: rgba(91, 67, 204, 0.35); }

.l2-aurora { position: absolute; inset: 0; z-index: 0; overflow: hidden; pointer-events: none; }
.l2-aurora i { position: absolute; width: 560px; height: 560px; border-radius: 50%; filter: blur(95px); }
.l2-aurora .a1 { top: -160px; left: -120px; background: rgba(59, 42, 132, 0.55); animation: l2-drift1 19s ease-in-out infinite alternate; }
.l2-aurora .a2 { top: 32%; right: -200px; background: rgba(35, 25, 103, 0.6); animation: l2-drift2 24s ease-in-out infinite alternate; }
.l2-aurora .a3 { bottom: -220px; left: 28%; width: 480px; background: rgba(245, 158, 11, 0.22); animation: l2-drift1 28s ease-in-out infinite alternate-reverse; }
@keyframes l2-drift1 { to { transform: translate(70px, 50px) scale(1.12); } }
@keyframes l2-drift2 { to { transform: translate(-80px, -40px) scale(0.92); } }

.l2-spotlight { position: fixed; inset: 0; z-index: 1; pointer-events: none; }

/* ════ the line ════ */
.l2-line-svg { position: absolute; inset: 0; z-index: 0; pointer-events: none; }
.l2-path-ghost { stroke: rgba(138, 127, 255, 0.10); stroke-width: 2; stroke-dasharray: 3 9; }
.l2-path-main { stroke: url(#l3-line-grad); stroke-width: 3; stroke-linecap: round; }

.l2-tip { position: absolute; top: 0; left: 0; z-index: 1; pointer-events: none; will-change: transform; }
.l2-tip-core, .l2-tip-halo { position: absolute; border-radius: 9999px; transform: translate(-50%, -50%); }
.l2-tip-core { width: 10px; height: 10px; background: #fff; box-shadow: 0 0 12px 3px rgba(251, 191, 36, 0.9); }
.l2-tip-halo { width: 34px; height: 34px; background: radial-gradient(circle, rgba(251,191,36,0.35), transparent 70%); animation: l2-halo 1.6s ease-in-out infinite; }
@keyframes l2-halo { 0%, 100% { opacity: 0.6; } 50% { opacity: 1; } }

.l2-line-start, .l2-line-end, .l2-center-anchor { width: 2px; height: 2px; margin: 0 auto; }
.l2-center-anchor.is-abs { position: absolute; left: 50%; top: 50%; margin: 0; }
.l2-center-anchor.is-abs.off-left { left: 26%; }

/* ════ robot mascot ════ */
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
  width: 40px; height: 40px; border-radius: 11px; flex-shrink: 0;
  object-fit: contain; padding: 3px; background: #fff;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.18);
}
.l2-logo-mark.sm { width: 32px; height: 32px; border-radius: 9px; padding: 2px; }
.l2-logo-text { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 14.5px; color: var(--tx1); letter-spacing: 0.01em; }
.l2-logo-text em { font-style: normal; color: var(--gold); }
.l2-nav-right { display: flex; align-items: center; gap: 6px; }
.l2-nav-lnk {
  padding: 9px 13px; border-radius: 10px; font-size: 13.5px; font-weight: 600;
  color: var(--tx2); text-decoration: none; transition: color 0.2s, background 0.2s;
}
.l2-nav-lnk:hover { color: var(--tx1); background: rgba(138, 127, 255, 0.12); }
.l2-nav-cta {
  display: inline-flex; align-items: center; gap: 6px; padding: 10px 16px; border-radius: 11px;
  font-size: 13.5px; font-weight: 700; color: var(--btn-txt); text-decoration: none;
  background: linear-gradient(135deg, var(--grad-a), var(--grad-b)); transition: opacity 0.2s, box-shadow 0.2s;
}
.l2-nav-cta:hover { opacity: 0.92; box-shadow: 0 0 24px rgba(251, 191, 36, 0.35); }
@media (max-width: 760px) { .l2-nav-lnk { display: none; } }

/* ════ hero — split layout + 3D slider ════ */
.l3-hero { position: relative; z-index: 2; max-width: 1240px; margin: 0 auto; padding: 130px 24px 40px; }
.l3-hero-grid { display: grid; grid-template-columns: 1.05fr 1fr; gap: 56px; align-items: center; }
@media (max-width: 980px) { .l3-hero-grid { grid-template-columns: 1fr; gap: 44px; } }

.l2-kicker {
  display: inline-flex; align-items: center; gap: 8px;
  font-size: 12.5px; font-weight: 700; letter-spacing: 0.14em; text-transform: uppercase;
  color: var(--grad-a); margin-bottom: 24px;
}
.l2-kicker-dot { width: 7px; height: 7px; border-radius: 99px; background: var(--grad-b); box-shadow: 0 0 10px rgba(251,191,36,0.9); animation: l2-halo 2s infinite; }

.l3-htext { min-height: 320px; }
.l3-h1 {
  font-family: 'Syne', sans-serif; font-weight: 800;
  font-size: clamp(2.5rem, 4.8vw, 4.2rem); line-height: 1.02; letter-spacing: -0.035em;
  margin: 0 0 20px; color: var(--tx1);
}
.l3-h1-line { display: block; }
.l3-h1-line.gold {
  font-style: italic;
  background: linear-gradient(100deg, var(--grad-b), var(--grad-a));
  -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l3-hero-p { max-width: 460px; margin: 0 0 8px; font-size: 16px; line-height: 1.7; color: var(--tx2); }
.slide-txt-enter-active, .slide-txt-leave-active { transition: opacity 0.35s ease, transform 0.35s ease; }
.slide-txt-enter-from { opacity: 0; transform: translateY(16px); }
.slide-txt-leave-to { opacity: 0; transform: translateY(-12px); }

.l3-hstats { display: flex; gap: 28px; flex-wrap: wrap; }
.l3-hstat { display: flex; flex-direction: column; }
.l3-hstat-v {
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 24px;
  background: linear-gradient(120deg, var(--grad-a), var(--grad-b)); -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l3-hstat-l { font-size: 11.5px; font-weight: 600; letter-spacing: 0.07em; text-transform: uppercase; color: var(--tx3); }

/* 3D slider — card interior stays dark in both themes (it's a screen) */
.l3-slider-wrap { position: relative; }
.l3-scene { perspective: 1200px; animation: l3-cardfloat 7s ease-in-out infinite; }
@keyframes l3-cardfloat { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-8px); } }
.l3-tilt { transition: transform 0.18s ease-out; transform-style: preserve-3d; }
.l3-card {
  position: relative; height: 380px; border-radius: 24px; overflow: hidden;
  perspective: 900px;
  background: linear-gradient(160deg, #0d0823, #140d33);
  border: 1px solid rgba(138, 127, 255, 0.28);
  box-shadow: 0 40px 90px rgba(0, 0, 0, 0.55), 0 0 60px rgba(124, 106, 245, 0.1);
}
.l3-sheen { position: absolute; inset: 0; z-index: 4; pointer-events: none; }
.l3-card-head {
  position: absolute; top: 0; left: 0; right: 0; z-index: 3;
  display: flex; justify-content: space-between; align-items: center; padding: 14px 18px;
  background: linear-gradient(180deg, rgba(5, 3, 13, 0.7), transparent);
}
.l3-brand { display: inline-flex; align-items: center; gap: 7px; font-size: 11px; font-weight: 700; letter-spacing: 0.1em; color: #b9b3d9; text-transform: uppercase; }
.l3-brand-dot { width: 7px; height: 7px; border-radius: 99px; background: linear-gradient(135deg, #8a7fff, #fbbf24); }
.l3-status { display: inline-flex; align-items: center; gap: 6px; padding: 4px 10px; border-radius: 99px; font-size: 10px; font-weight: 800; letter-spacing: 0.12em; }
.l3-status i { width: 6px; height: 6px; border-radius: 99px; background: currentColor; animation: l2-halo 1.6s infinite; }
.tone-violet { color: #a99cff; background: rgba(124, 106, 245, 0.14); border: 1px solid rgba(138, 127, 255, 0.35); }
.tone-cyan { color: #67e8f9; background: rgba(6, 182, 212, 0.12); border: 1px solid rgba(6, 182, 212, 0.35); }
.tone-green { color: #6ee7b7; background: rgba(16, 185, 129, 0.12); border: 1px solid rgba(16, 185, 129, 0.35); }

.l3-slide { position: absolute; inset: 0; padding-top: 48px; }
.s3d-next-enter-active, .s3d-next-leave-active, .s3d-prev-enter-active, .s3d-prev-leave-active { transition: opacity 0.45s ease, transform 0.45s cubic-bezier(0.22, 1, 0.36, 1); }
.s3d-next-enter-from { opacity: 0; transform: translateX(80px) rotateY(-18deg) scale(0.94); }
.s3d-next-leave-to { opacity: 0; transform: translateX(-80px) rotateY(18deg) scale(0.94); }
.s3d-prev-enter-from { opacity: 0; transform: translateX(-80px) rotateY(18deg) scale(0.94); }
.s3d-prev-leave-to { opacity: 0; transform: translateX(80px) rotateY(-18deg) scale(0.94); }

.l3-arrow {
  position: absolute; top: 50%; z-index: 5; width: 36px; height: 36px; margin-top: -18px;
  display: grid; place-items: center; border-radius: 99px; cursor: pointer;
  color: var(--tx1); background: var(--panel); border: 1px solid var(--bd2);
  transition: border-color 0.2s, color 0.2s, background 0.2s;
}
.l3-arrow:hover { color: var(--gold); border-color: var(--gold); }
.l3-arrow.prev { left: -16px; } .l3-arrow.next { right: -16px; }
.l3-dots { display: flex; align-items: center; gap: 8px; margin-top: 16px; justify-content: center; }
.l3-dot { width: 22px; height: 5px; border-radius: 99px; border: 0; cursor: pointer; background: var(--bd2); transition: background 0.25s; }
.l3-dot.active { background: linear-gradient(90deg, var(--grad-a), var(--grad-b)); }
.l3-dot-lbl { margin-left: 8px; font-size: 11.5px; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: var(--tx3); }

/* slide 0 · flashcards */
.sl-fc { position: relative; height: 100%; padding: 16px 22px; }
.fc3-flip {
  position: relative; width: min(300px, 70%); height: 190px; margin-left: 8px;
  transform-style: preserve-3d; animation: l3-flip 7s ease-in-out infinite;
}
@keyframes l3-flip { 0%, 38% { transform: rotateY(0); } 50%, 88% { transform: rotateY(180deg); } 100% { transform: rotateY(360deg); } }
.fc3-face {
  position: absolute; inset: 0; padding: 18px 20px; border-radius: 16px; backface-visibility: hidden;
  background: rgba(5, 3, 13, 0.75); border: 1px solid rgba(138, 127, 255, 0.3);
  display: flex; flex-direction: column; gap: 10px; text-align: left;
}
.fc3-back { transform: rotateY(180deg); border-color: rgba(251, 191, 36, 0.4); }
.fc3-tag { font-size: 9.5px; font-weight: 800; letter-spacing: 0.14em; color: #a99cff; }
.fc3-tag.gold { color: #fbbf24; }
.fc3-q { margin: 0; font-size: 14.5px; line-height: 1.55; color: #e9e6fa; }
.fc3-q b { color: #fbbf24; }
.fc3-hint { margin-top: auto; font-size: 11px; color: #5d5687; }
.fc3-coin { margin-top: auto; display: inline-flex; align-items: center; gap: 5px; font-size: 12px; font-weight: 700; color: #6ee7b7; }
.fc3-bot { position: absolute; right: 12px; bottom: 50px; width: 70px; animation: l2-float 4.6s ease-in-out infinite; }
@keyframes l2-float { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-10px); } }
.fc3-pills { position: absolute; left: 22px; right: 22px; bottom: 14px; display: flex; gap: 7px; flex-wrap: wrap; }
.fc3-pills span {
  padding: 4px 11px; border-radius: 99px; font-size: 11px; font-weight: 600;
  color: #b9b3d9; background: rgba(138, 127, 255, 0.1); border: 1px solid rgba(138, 127, 255, 0.22);
}
.fc3-pills span.active { color: #05030d; background: linear-gradient(135deg, #a8a4ff, #fbbf24); border-color: transparent; font-weight: 700; }

/* slide 1 · grading */
.sl-gr { position: relative; height: 100%; display: flex; gap: 16px; padding: 14px 22px 18px; }
.gr3-sheet {
  flex: 1.3; padding: 13px 15px; border-radius: 14px; text-align: left;
  background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.25);
}
.gr3-title { display: block; font-size: 10px; font-weight: 800; letter-spacing: 0.12em; color: #8d86b8; text-transform: uppercase; margin-bottom: 10px; }
.gr3-row {
  display: flex; justify-content: space-between; align-items: center; padding: 7px 10px; margin-bottom: 6px;
  border-radius: 9px; background: rgba(138, 127, 255, 0.07); border: 1px solid rgba(138, 127, 255, 0.14);
  opacity: 0; animation: l3-rowin 0.4s ease forwards;
}
@keyframes l3-rowin { from { opacity: 0; transform: translateX(-10px); } to { opacity: 1; transform: none; } }
.gr3-q { font-size: 12px; font-weight: 600; color: #d9d5f2; }
.gr3-mark { display: inline-flex; align-items: center; gap: 4px; font-size: 11.5px; font-weight: 800; color: #6ee7b7; }
.gr3-pen { font-size: 11px; font-style: italic; color: #fbbf24; animation: l2-halo 1.2s infinite; }
.gr3-note { margin-top: 8px; font-size: 11px; color: #8d86b8; }
.gr3-note b { color: #fbbf24; }
.gr3-side { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 4px; position: relative; }
.gr3-ringwrap { position: relative; width: 92px; height: 92px; }
.gr3-ring { width: 92px; height: 92px; transform: rotate(-90deg); }
.gr3-ring .ring-bg { fill: none; stroke: rgba(138, 127, 255, 0.18); stroke-width: 7; }
.gr3-ring .ring-fg { fill: none; stroke: #fbbf24; stroke-width: 7; stroke-linecap: round; transition: stroke-dashoffset 0.2s linear; }
.gr3-num { position: absolute; inset: 0; display: grid; place-items: center; font-family: 'Syne', sans-serif; font-weight: 800; font-size: 22px; color: #fbbf24; }
.gr3-num i { font-style: normal; font-size: 12px; }
.gr3-lbl { font-size: 10.5px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: #8d86b8; }
.gr3-bot { position: absolute; right: -8px; bottom: -14px; width: 52px; }

/* slide 2 · live tutor */
.sl-lt { position: relative; height: 100%; display: flex; flex-direction: column; padding: 12px 22px 16px; text-align: left; }
.lt3-head { display: flex; align-items: center; gap: 10px; margin-bottom: 10px; }
.lt3-bot { width: 44px; }
.lt3-head b { display: block; font-size: 14px; color: #f5f3ff; }
.lt3-head span { display: block; font-size: 11.5px; color: #8d86b8; }
.lt3-chat { flex: 1; display: flex; flex-direction: column; gap: 8px; overflow: hidden; }
.lt3-bub { max-width: 78%; padding: 9px 13px; border-radius: 14px; font-size: 13px; line-height: 1.6; }
.lt3-bub.user { align-self: flex-end; color: #f5f3ff; background: rgba(124, 106, 245, 0.3); border: 1px solid rgba(138, 127, 255, 0.4); border-bottom-right-radius: 4px; }
.lt3-bub.ai { align-self: flex-start; color: #e9e6fa; background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.25); border-bottom-left-radius: 4px; }
.lt3-bub.ai em { display: block; margin-top: 4px; font-size: 11.5px; color: #8d86b8; }
.lt3-bub.typing { align-self: flex-start; display: inline-flex; gap: 4px; padding: 11px 14px; background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.25); }
.lt3-bub.typing span { width: 6px; height: 6px; border-radius: 99px; background: #8a7fff; animation: l3-dots 1.2s ease-in-out infinite; }
.lt3-bub.typing span:nth-child(2) { animation-delay: 0.18s; }
.lt3-bub.typing span:nth-child(3) { animation-delay: 0.36s; }
@keyframes l3-dots { 0%, 100% { opacity: 0.3; transform: translateY(0); } 50% { opacity: 1; transform: translateY(-3px); } }
.lt3-input {
  margin-top: 10px; display: flex; align-items: center; justify-content: space-between;
  padding: 10px 14px; border-radius: 12px; font-size: 12.5px; color: #5d5687;
  background: rgba(5, 3, 13, 0.7); border: 1px solid rgba(138, 127, 255, 0.22);
}
.lt3-input svg { color: #fbbf24; }

/* ════ ticker ════ */
.l3-ticker { position: relative; z-index: 2; overflow: hidden; padding: 18px 0; border-top: 1px solid var(--bd); border-bottom: 1px solid var(--bd); }
.l3-ticker-track { display: flex; width: max-content; animation: l3-marquee 26s linear infinite; }
.l3-ticker:hover .l3-ticker-track { animation-play-state: paused; }
.l3-tick {
  display: inline-flex; align-items: center; gap: 18px; padding-right: 18px;
  font-family: 'Syne', sans-serif; font-weight: 700; font-size: 14px; letter-spacing: 0.06em;
  color: var(--tx3); text-transform: uppercase; white-space: nowrap;
}
.l3-tick i { width: 5px; height: 5px; border-radius: 99px; background: var(--grad-b); opacity: 0.7; }
@keyframes l3-marquee { to { transform: translateX(-50%); } }

/* ════ hero buttons (shared) ════ */
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

/* ════ journey stations (Landing 2) ════ */
.l2-journey { position: relative; z-index: 2; max-width: 1180px; margin: 0 auto; padding: 60px 24px 80px; }
.l2-stop { position: relative; min-height: 300px; display: flex; align-items: center; padding: 56px 0; }
.l2-stop-node { position: absolute; top: 50%; width: 2px; height: 2px; }
.l2-stop.is-left  .l2-stop-node { left: 21%; }
.l2-stop.is-right .l2-stop-node { left: 79%; }
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
  position: relative; width: min(480px, 100%); padding: 26px 28px 30px; border-radius: 22px;
  background: var(--panel); border: 1px solid var(--bd);
  backdrop-filter: blur(8px); transition: border-color 0.3s, box-shadow 0.4s, transform 0.3s;
}
.l2-stop-card:hover { border-color: rgba(251, 191, 36, 0.35); transform: translateY(-3px); }
.l2-stop.active .l2-stop-card { box-shadow: 0 24px 70px rgba(124, 106, 245, 0.12); }
.l2-stop.is-left  .l2-stop-card { margin-left: auto;  margin-right: 4%; }
.l2-stop.is-right .l2-stop-card { margin-right: auto; margin-left: 4%; }
.l2-stop-step {
  position: absolute; top: 240px; right: 26px;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 30px;
  color: transparent; -webkit-text-stroke: 1px rgba(138, 127, 255, 0.4);
}
.l2-stop-icon {
  display: grid; place-items: center; width: 46px; height: 46px; border-radius: 13px; margin-bottom: 16px;
  color: #fbbf24; background: rgba(251, 191, 36, 0.1); border: 1px solid rgba(251, 191, 36, 0.25);
}
.l2-stop-h { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 24px; letter-spacing: -0.02em; margin: 0 0 10px; color: var(--tx1); }
.l2-stop-p { margin: 0 0 18px; font-size: 15px; line-height: 1.65; color: var(--tx2); }
.l2-stop-chips { display: flex; flex-wrap: wrap; gap: 8px; margin: 0; padding: 0; list-style: none; }
.l2-stop-chips li {
  display: inline-flex; align-items: center; gap: 5px; padding: 5px 11px; border-radius: 99px;
  font-size: 12px; font-weight: 600; color: var(--grad-a);
  background: rgba(138, 127, 255, 0.1); border: 1px solid var(--bd2);
}
@media (max-width: 860px) {
  .l2-stop { min-height: 0; padding: 36px 0; }
  .l2-stop.is-left .l2-stop-card, .l2-stop.is-right .l2-stop-card { margin: 0 auto; }
  .l2-stop.is-left .l2-stop-node { left: 8%; }
  .l2-stop.is-right .l2-stop-node { left: 92%; }
}

/* ════ scene viewports (Landing 2) ════ */
.sc {
  position: relative; height: 200px; margin-bottom: 22px; border-radius: 16px; overflow: hidden;
  background:
    radial-gradient(120% 120% at 80% 0%, rgba(124, 106, 245, 0.14), transparent 50%),
    linear-gradient(160deg, #0a0620, #100a28);
  border: 1px solid rgba(138, 127, 255, 0.2);
  filter: saturate(0.15) brightness(0.6);
  transition: filter 0.8s ease;
}
.l2-stop.active .sc { filter: none; }
.sc-stage { position: absolute; inset: 0; }
.sc-head {
  position: absolute; top: 10px; left: 14px; right: 14px; z-index: 2;
  display: flex; justify-content: space-between; align-items: center;
  font-size: 10px; font-weight: 700; letter-spacing: 0.14em; color: #8d86b8; text-transform: uppercase;
}
.sc-live { display: inline-flex; align-items: center; gap: 5px; color: #fbbf24; }
.sc-live i { width: 6px; height: 6px; border-radius: 99px; background: #5d5687; }
.l2-stop.active .sc-live i { background: #fbbf24; animation: l2-halo 1.4s infinite; }
.sc-standby {
  position: absolute; inset: 0; z-index: 3; display: grid; place-items: center;
  font-size: 11px; font-weight: 700; letter-spacing: 0.3em; color: #5d5687;
  background: rgba(5, 3, 13, 0.45); transition: opacity 0.6s; pointer-events: none;
}
.l2-stop.active .sc-standby { opacity: 0; }
.sc-scan {
  position: absolute; top: 0; bottom: 0; width: 70px; z-index: 1; opacity: 0;
  background: linear-gradient(90deg, transparent, rgba(138, 127, 255, 0.07), transparent);
}
.l2-stop.active .sc-scan { opacity: 1; animation: l2-scan 3.4s linear infinite; }
@keyframes l2-scan { from { transform: translateX(-90px); } to { transform: translateX(540px); } }

.sc1-bot { position: absolute; left: 18px; bottom: 8px; width: 76px; animation: l2-float 4.5s ease-in-out infinite; }
.sc1-bubble {
  position: absolute; left: 24px; top: 30px; max-width: 150px; z-index: 2;
  padding: 7px 11px; border-radius: 12px 12px 12px 3px; font-size: 11px; font-weight: 600; color: #f5f3ff;
  background: rgba(124, 106, 245, 0.22); border: 1px solid rgba(138, 127, 255, 0.45);
  opacity: 0; transform: scale(0.5); transform-origin: bottom left;
}
.l2-stop.active .sc1-bubble { animation: l2-pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) 2.6s forwards; }
@keyframes l2-pop { to { opacity: 1; transform: scale(1); } }
.sc1-board {
  position: absolute; right: 14px; top: 38px; width: 54%; padding: 14px 16px; border-radius: 12px;
  background: rgba(5, 3, 13, 0.65); border: 1px solid rgba(138, 127, 255, 0.25); text-align: left;
}
.sc1-board-tag { display: block; font-size: 9px; font-weight: 700; letter-spacing: 0.12em; color: #8d86b8; margin-bottom: 10px; }
.sc1-tl { display: block; height: 8px; border-radius: 99px; background: rgba(138, 127, 255, 0.35); margin-bottom: 8px; transform: scaleX(0); transform-origin: left; }
.sc1-tl1 { width: 92%; } .sc1-tl2 { width: 64%; }
.l2-stop.active .sc1-tl1 { animation: l2-grow 0.5s ease 0.3s forwards; }
.l2-stop.active .sc1-tl2 { animation: l2-grow 0.5s ease 0.8s forwards; }
.sc1-eq {
  margin-top: 12px; font-family: 'Syne', sans-serif; font-weight: 700; font-size: 15px; color: #fbbf24;
  white-space: nowrap; overflow: hidden; width: 0; border-right: 2px solid rgba(251, 191, 36, 0);
}
.l2-stop.active .sc1-eq { border-right-color: #fbbf24; animation: l2-type 1.1s steps(16) 1.3s forwards, l2-caret 0.8s step-end 1.3s 4; }
@keyframes l2-grow { to { transform: scaleX(1); } }
@keyframes l2-type { to { width: 100%; } }
@keyframes l2-caret { 50% { border-right-color: transparent; } }

.sc2-paper {
  position: absolute; left: 18px; top: 38px; width: 52%; padding: 13px 15px; border-radius: 10px;
  background: #f3f0ff; transform: rotate(-2deg); text-align: left;
  box-shadow: 0 14px 34px rgba(0, 0, 0, 0.45);
}
.sc2-ptitle { display: block; font-size: 9px; font-weight: 800; letter-spacing: 0.12em; color: #4a35a6; margin-bottom: 9px; }
.sc2-pl { display: block; height: 7px; border-radius: 99px; background: #c9c2ec; margin-bottom: 7px; transform: scaleX(0); transform-origin: left; }
.sc2-pl1 { width: 95%; } .sc2-pl2 { width: 78%; } .sc2-pl3 { width: 86%; }
.l2-stop.active .sc2-pl1 { animation: l2-grow 0.45s ease 0.3s forwards; }
.l2-stop.active .sc2-pl2 { animation: l2-grow 0.45s ease 0.75s forwards; }
.l2-stop.active .sc2-pl3 { animation: l2-grow 0.45s ease 1.2s forwards; }
.sc2-mcq { display: flex; gap: 7px; margin-top: 11px; }
.sc2-opt {
  display: grid; place-items: center; width: 24px; height: 24px; border-radius: 8px;
  font-size: 11px; font-weight: 800; color: #4a35a6; border: 1.5px solid #c9c2ec;
  opacity: 0; transform: scale(0.4);
}
.l2-stop.active .sc2-opt { animation: l2-pop 0.35s cubic-bezier(0.34, 1.56, 0.64, 1) forwards; }
.l2-stop.active .sc2-opt:nth-child(1) { animation-delay: 1.6s; }
.l2-stop.active .sc2-opt:nth-child(2) { animation-delay: 1.75s; }
.l2-stop.active .sc2-opt:nth-child(3) { animation-delay: 1.9s; }
.l2-stop.active .sc2-opt:nth-child(4) { animation-delay: 2.05s; }
.l2-stop.active .sc2-opt.pick { background: #fbbf24; border-color: #f59e0b; box-shadow: 0 0 14px rgba(251, 191, 36, 0.7); }
.sc2-bot { position: absolute; right: 16px; bottom: 8px; width: 72px; animation: l2-float 5s ease-in-out infinite; }
.sc2-gear { position: absolute; right: 30px; top: 26px; width: 30px; height: 30px; }
.sc2-gear circle { fill: none; stroke: #8a7fff; stroke-width: 6; stroke-dasharray: 4 3.5; }
.l2-stop.active .sc2-gear { animation: l2-spin 3s linear infinite; }
@keyframes l2-spin { to { transform: rotate(360deg); } }
.sc-spark { position: absolute; width: 5px; height: 5px; border-radius: 99px; background: #fbbf24; opacity: 0; }
.sc-spark.s1 { right: 78px; top: 50px; } .sc-spark.s2 { right: 110px; top: 92px; } .sc-spark.s3 { right: 60px; top: 120px; }
.l2-stop.active .sc-spark { animation: l3-twinkle 1.8s ease-in-out infinite; }
.l2-stop.active .sc-spark.s2 { animation-delay: 0.5s; }
.l2-stop.active .sc-spark.s3 { animation-delay: 1.1s; }
@keyframes l3-twinkle { 0%, 100% { opacity: 0.2; } 50% { opacity: 1; } }

.sc3-rows { position: absolute; left: 18px; top: 44px; width: 52%; }
.sc3-row { position: relative; display: flex; align-items: center; margin-bottom: 15px; }
.sc3-bar { display: block; height: 9px; border-radius: 99px; background: rgba(138, 127, 255, 0.3); }
.sc3-stamp {
  position: absolute; right: -32px; top: 50%; width: 23px; height: 23px; margin-top: -12px;
  display: grid; place-items: center; border-radius: 99px;
  opacity: 0; transform: scale(0.3);
}
.sc3-stamp svg { width: 13px; height: 13px; fill: none; stroke: currentColor; stroke-width: 3.4; stroke-linecap: round; }
.sc3-stamp.ok { color: #34d399; background: rgba(52, 211, 153, 0.14); border: 1.5px solid rgba(52, 211, 153, 0.5); }
.sc3-stamp.no { color: #fb7185; background: rgba(251, 113, 133, 0.14); border: 1.5px solid rgba(251, 113, 133, 0.5); }
.l2-stop.active .sc3-stamp { animation: l2-stamp 0.45s cubic-bezier(0.34, 1.8, 0.64, 1) 0.5s forwards; }
.l2-stop.active .sc3-stamp.no { animation-delay: 1.5s; }
@keyframes l2-stamp { 60% { transform: scale(1.25); opacity: 1; } 100% { transform: scale(1); opacity: 1; } }
.sc3-fix { display: block; width: 58%; height: 6px; border-radius: 99px; background: rgba(251, 191, 36, 0.75); transform: scaleX(0); transform-origin: left; }
.l2-stop.active .sc3-fix { animation: l2-grow 0.5s ease 2.1s forwards; }
.sc3-ringwrap { position: absolute; right: 18px; top: 38px; width: 84px; height: 84px; }
.sc3-ring { width: 84px; height: 84px; transform: rotate(-90deg); }
.sc3-ring .ring-bg { fill: none; stroke: rgba(138, 127, 255, 0.18); stroke-width: 8; }
.sc3-ring .ring-fg { fill: none; stroke: #fbbf24; stroke-width: 8; stroke-linecap: round; stroke-dasharray: 201; stroke-dashoffset: 201; }
.l2-stop.active .sc3-ring .ring-fg { animation: l2-ring 1.5s cubic-bezier(0.22, 1, 0.36, 1) 0.7s forwards; }
@keyframes l2-ring { to { stroke-dashoffset: 26; } }
.sc3-score {
  position: absolute; inset: 0; display: grid; place-items: center;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 19px; color: #fbbf24; opacity: 0;
}
.l2-stop.active .sc3-score { animation: l2-fadein 0.5s ease 1.9s forwards; }
@keyframes l2-fadein { to { opacity: 1; } }
.sc3-bot { position: absolute; right: 26px; bottom: -6px; width: 58px; }

.sc4-podium { position: absolute; left: 26px; bottom: 14px; display: flex; align-items: flex-end; gap: 9px; }
.sc4-col {
  position: relative; width: 38px; border-radius: 8px 8px 0 0; display: flex; justify-content: center;
  background: linear-gradient(180deg, rgba(138, 127, 255, 0.4), rgba(138, 127, 255, 0.12));
  border: 1px solid rgba(138, 127, 255, 0.35); border-bottom: 0;
  transform: scaleY(0); transform-origin: bottom;
}
.sc4-col b { margin-top: 7px; font-family: 'Syne', sans-serif; font-size: 14px; color: #d9d5f2; }
.sc4-col.p1 { height: 86px; background: linear-gradient(180deg, rgba(251, 191, 36, 0.5), rgba(251, 191, 36, 0.12)); border-color: rgba(251, 191, 36, 0.5); }
.sc4-col.p2 { height: 58px; } .sc4-col.p3 { height: 42px; }
.l2-stop.active .sc4-col.p2 { animation: l2-grow-y 0.6s cubic-bezier(0.22, 1, 0.36, 1) 0.3s forwards; }
.l2-stop.active .sc4-col.p1 { animation: l2-grow-y 0.6s cubic-bezier(0.22, 1, 0.36, 1) 0.65s forwards; }
.l2-stop.active .sc4-col.p3 { animation: l2-grow-y 0.6s cubic-bezier(0.22, 1, 0.36, 1) 1s forwards; }
@keyframes l2-grow-y { to { transform: scaleY(1); } }
.sc4-cup {
  position: absolute; top: -34px; left: 50%; width: 26px; height: 26px; margin-left: -13px;
  fill: none; stroke: #fbbf24; stroke-width: 1.8; stroke-linejoin: round;
  opacity: 0; transform: translateY(-26px);
}
.l2-stop.active .sc4-cup { animation: l2-drop 0.55s cubic-bezier(0.34, 1.56, 0.64, 1) 1.5s forwards; }
@keyframes l2-drop { to { opacity: 1; transform: translateY(0); } }
.sc4-rank {
  position: absolute; right: 16px; top: 40px; padding: 6px 12px; border-radius: 99px;
  font-size: 10.5px; font-weight: 800; letter-spacing: 0.1em; color: #05030d;
  background: linear-gradient(135deg, #a8a4ff, #fbbf24);
  opacity: 0; transform: scale(0.5);
}
.l2-stop.active .sc4-rank { animation: l2-pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) 2s forwards; }
.sc4-bot { position: absolute; right: 24px; bottom: 4px; width: 70px; }
.sc4-conf { position: absolute; top: -12px; width: 5px; height: 10px; border-radius: 2px; opacity: 0; }
.sc4-conf.c1 { left: 12%; background: #fbbf24; } .sc4-conf.c2 { left: 26%; background: #8a7fff; }
.sc4-conf.c3 { left: 40%; background: #34d399; } .sc4-conf.c4 { left: 54%; background: #fbbf24; }
.sc4-conf.c5 { left: 66%; background: #fb7185; } .sc4-conf.c6 { left: 78%; background: #8a7fff; }
.sc4-conf.c7 { left: 88%; background: #34d399; } .sc4-conf.c8 { left: 33%; background: #fb7185; }
.l2-stop.active .sc4-conf { animation: l2-fall 2.4s linear infinite; }
.l2-stop.active .sc4-conf.c2 { animation-delay: 0.4s; } .l2-stop.active .sc4-conf.c3 { animation-delay: 0.9s; }
.l2-stop.active .sc4-conf.c4 { animation-delay: 1.3s; } .l2-stop.active .sc4-conf.c5 { animation-delay: 0.2s; }
.l2-stop.active .sc4-conf.c6 { animation-delay: 1.7s; } .l2-stop.active .sc4-conf.c7 { animation-delay: 0.7s; }
.l2-stop.active .sc4-conf.c8 { animation-delay: 2s; }
@keyframes l2-fall {
  0% { opacity: 0; transform: translateY(0) rotate(0deg); }
  8% { opacity: 1; }
  100% { opacity: 0.4; transform: translateY(230px) rotate(480deg); }
}

/* ════ AI brain scene (merged from Landing 1) ════ */
.l3-brain { position: relative; z-index: 2; max-width: 1180px; margin: 0 auto; padding: 70px 24px 110px; text-align: center; }
.l3-bstage {
  position: relative; display: flex; align-items: center; justify-content: center; gap: 0;
  margin-top: 50px; flex-wrap: wrap;
}
.l3-bcol { display: flex; flex-direction: column; gap: 16px; }
.l3-scard {
  width: 250px; padding: 15px 17px; border-radius: 16px; text-align: left;
  background: var(--panel); border: 1px solid var(--bd);
  opacity: 0.45; transition: opacity 0.6s, border-color 0.6s;
}
.l3-brain.lit .l3-scard { opacity: 1; border-color: var(--bd2); }
.l3-sc-top { display: flex; align-items: center; gap: 10px; margin-bottom: 11px; }
.l3-sc-av {
  display: grid; place-items: center; width: 34px; height: 34px; border-radius: 11px;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 14px; color: #f5f3ff;
  background: linear-gradient(135deg, #3b2a84, #6b54e8);
}
.l3-sc-id { flex: 1; }
.l3-sc-id b { display: block; font-size: 13.5px; color: var(--tx1); }
.l3-sc-id i { display: block; font-style: normal; font-size: 11px; color: var(--tx3); }
.l3-sc-score { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 16px; }
.l3-bar { display: grid; grid-template-columns: 62px 1fr 34px; align-items: center; gap: 8px; margin-bottom: 6px; }
.l3-bar span { font-size: 10.5px; font-weight: 600; color: var(--tx3); }
.l3-bar b { font-size: 10.5px; font-weight: 700; color: var(--tx2); text-align: right; }
.l3-bar-track { display: block; height: 5px; border-radius: 99px; background: rgba(138, 127, 255, 0.15); overflow: hidden; }
.l3-bar-fill { display: block; height: 100%; border-radius: 99px; width: 0; transition: width 1.1s cubic-bezier(0.22, 1, 0.36, 1) 0.3s; }

.l3-bwire { position: relative; width: 56px; height: 2px; background: var(--bd2); overflow: hidden; }
.l3-bwire .l2-pipe-pulse { animation-play-state: paused; }
.l3-brain.lit .l3-bwire .l2-pipe-pulse { animation-play-state: running; }
.l3-bwire.flip .l2-pipe-pulse { animation-direction: reverse; }

.l3-bcore { position: relative; width: 170px; height: 170px; display: grid; place-items: center; }
.l3-ring { position: absolute; border-radius: 50%; border: 1px dashed rgba(138, 127, 255, 0.3); }
.l3-ring.r1 { inset: 0; } .l3-ring.r2 { inset: 18px; border-color: rgba(251, 191, 36, 0.25); } .l3-ring.r3 { inset: 36px; }
.l3-brain.lit .l3-ring.r1 { animation: l2-spin 22s linear infinite; }
.l3-brain.lit .l3-ring.r2 { animation: l2-spin 16s linear infinite reverse; }
.l3-brain.lit .l3-ring.r3 { animation: l2-spin 12s linear infinite; }
.l3-bcore-in {
  position: relative; z-index: 1; display: flex; flex-direction: column; align-items: center; gap: 6px;
  width: 96px; height: 96px; justify-content: center; border-radius: 50%;
  color: #a99cff; background: rgba(13, 8, 35, 0.92); border: 1px solid rgba(138, 127, 255, 0.4);
  transition: color 0.6s, border-color 0.6s, box-shadow 0.6s;
}
.l3-brain.lit .l3-bcore-in { color: #fbbf24; border-color: rgba(251, 191, 36, 0.55); box-shadow: 0 0 44px rgba(251, 191, 36, 0.22); }
.l3-bcore-in span { font-size: 9px; font-weight: 800; letter-spacing: 0.18em; }

.l3-badge {
  position: absolute; display: inline-flex; align-items: center; gap: 6px;
  padding: 7px 13px; border-radius: 99px; font-size: 11.5px; font-weight: 700; color: var(--tx1);
  background: var(--panel2); border: 1px solid var(--bd2);
  opacity: 0; transform: scale(0.6); z-index: 2;
}
.l3-badge svg { color: var(--gold); }
.l3-brain.lit .l3-badge { animation: l2-pop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1) forwards; }
.l3-badge.b1 { top: -16px; left: 50%; transform-origin: center; margin-left: -110px; }
.l3-brain.lit .l3-badge.b1 { animation-delay: 0.8s; }
.l3-badge.b2 { bottom: -10px; left: 50%; margin-left: -90px; }
.l3-brain.lit .l3-badge.b2 { animation-delay: 1.2s; }
.l3-badge.b3 { top: 38%; right: 2%; }
.l3-brain.lit .l3-badge.b3 { animation-delay: 1.6s; }
@media (max-width: 980px) {
  .l3-bstage { flex-direction: column; gap: 18px; }
  .l3-bwire { width: 2px; height: 36px; }
  .l3-badge { display: none; }
  .l3-bcol { flex-direction: row; flex-wrap: wrap; justify-content: center; }
}

/* ════ headings + stats (shared) ════ */
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
.l2-stats.lit .l2-stat { border-color: rgba(251, 191, 36, 0.3); box-shadow: 0 0 34px rgba(251, 191, 36, 0.07); animation: l3-statpop 0.55s cubic-bezier(0.34, 1.56, 0.64, 1) both; }
.l2-stats.lit .l2-stat:nth-child(3) { animation-delay: 0.08s; }
.l2-stats.lit .l2-stat:nth-child(4) { animation-delay: 0.16s; }
.l2-stats.lit .l2-stat:nth-child(5) { animation-delay: 0.24s; }
@keyframes l3-statpop { 0% { transform: scale(0.94); } 60% { transform: scale(1.035); } 100% { transform: scale(1); } }
.l2-stat-v {
  display: block; font-family: 'Syne', sans-serif; font-weight: 800; font-size: clamp(1.6rem, 3.2vw, 2.4rem);
  font-variant-numeric: tabular-nums;
  background: linear-gradient(120deg, var(--grad-a), var(--grad-b)); -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l2-stat-l { display: block; margin-top: 6px; font-size: 12.5px; font-weight: 600; letter-spacing: 0.06em; text-transform: uppercase; color: var(--tx3); }
@media (max-width: 760px) { .l2-stats { grid-template-columns: repeat(2, 1fr); } }

/* ════ features ════ */
.l3-features { position: relative; z-index: 2; max-width: 1180px; margin: 0 auto; padding: 40px 24px 110px; text-align: center; }
.l3-feat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-top: 44px; }
@media (max-width: 1000px) { .l3-feat-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 560px) { .l3-feat-grid { grid-template-columns: 1fr; } }
.l3-feat {
  padding: 24px 22px; border-radius: 18px; text-align: left;
  background: var(--panel); border: 1px solid var(--bd);
  transition: border-color 0.3s, transform 0.3s, opacity 0.7s ease, box-shadow 0.6s, filter 0.7s ease;
}
.l3-feat:hover { border-color: rgba(251, 191, 36, 0.4); transform: translateY(-4px); }
.l3-features.lit .l3-feat { box-shadow: 0 0 26px rgba(124, 106, 245, 0.08); }
.l3-feat-ic {
  display: grid; place-items: center; width: 42px; height: 42px; border-radius: 12px; margin-bottom: 14px;
  color: var(--gold); background: rgba(251, 191, 36, 0.1); border: 1px solid rgba(251, 191, 36, 0.25);
  transition: transform 0.3s;
}
.l3-feat:hover .l3-feat-ic { transform: rotate(-8deg) scale(1.08); }
.l3-feat h3 { margin: 0 0 7px; font-family: 'Syne', sans-serif; font-weight: 700; font-size: 16.5px; color: var(--tx1); }
.l3-feat p { margin: 0; font-size: 13.5px; line-height: 1.6; color: var(--tx2); }

/* ════ tutors marquee + persona grid ════ */
.l3-tutors { position: relative; z-index: 2; padding: 30px 0 110px; text-align: center; }
.l3-tut-marquee { overflow: hidden; margin-top: 40px; padding: 6px 0; }
.l3-tut-track { display: flex; gap: 14px; width: max-content; animation: l3-marquee 30s linear infinite; }
.l3-tut-marquee:hover .l3-tut-track { animation-play-state: paused; }
.l3-tut-chip {
  display: inline-flex; align-items: center; gap: 11px; padding: 10px 18px 10px 11px; border-radius: 99px;
  background: var(--panel); border: 1px solid var(--bd);
}
.l3-tut-av {
  display: grid; place-items: center; width: 38px; height: 38px; border-radius: 99px;
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: 15px; color: #f5f3ff;
}
.l3-tut-id { text-align: left; }
.l3-tut-id b { display: block; font-size: 13.5px; color: var(--tx1); }
.l3-tut-id i { display: block; font-style: normal; font-size: 11px; color: var(--tx3); }
.l3-tut-note { max-width: 640px; margin: 34px auto 0; padding: 0 24px; font-size: 14px; line-height: 1.7; color: var(--tx3); }

.l3-tut-grid {
  max-width: 1180px; margin: 44px auto 0; padding: 0 24px;
  display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px;
}
@media (max-width: 1000px) { .l3-tut-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 560px) { .l3-tut-grid { grid-template-columns: 1fr; } }
.l3-tg-card {
  border-radius: 18px; overflow: hidden; text-align: left;
  background: var(--panel); border: 1px solid var(--bd);
  transition: transform 0.3s, border-color 0.3s, box-shadow 0.3s, opacity 0.7s ease, filter 0.7s ease;
}
.l3-tg-card:hover { transform: translateY(-5px); border-color: rgba(251, 191, 36, 0.45); box-shadow: 0 18px 44px rgba(0, 0, 0, 0.25); }
.l3-tg-head { position: relative; height: 84px; display: grid; place-items: center; overflow: hidden; }
.l3-tg-glyph { font-family: 'Syne', sans-serif; font-weight: 800; font-size: 28px; color: rgba(255, 255, 255, 0.95); }
.l3-tg-shine {
  position: absolute; top: 0; bottom: 0; width: 60px;
  background: linear-gradient(100deg, transparent, rgba(255, 255, 255, 0.2), transparent);
  transform: translateX(-90px) skewX(-12deg); transition: transform 0.6s ease;
}
.l3-tg-card:hover .l3-tg-shine { transform: translateX(340px) skewX(-12deg); }
.l3-tg-body { padding: 15px 17px 17px; }
.l3-tg-body b { display: block; font-family: 'Syne', sans-serif; font-size: 15px; color: var(--tx1); }
.l3-tg-body i { display: block; font-style: normal; font-size: 11.5px; color: var(--tx3); margin-bottom: 9px; }
.l3-tg-body p { margin: 0 0 13px; min-height: 42px; font-size: 12.5px; font-style: italic; line-height: 1.6; color: var(--tx2); }
.l3-tg-actions { display: flex; gap: 8px; }
.l3-tg-btn {
  flex: 1; display: inline-flex; justify-content: center; align-items: center; gap: 6px;
  padding: 8px 0; border-radius: 9px; font-size: 12px; font-weight: 700; text-decoration: none;
  transition: opacity 0.2s, border-color 0.2s, transform 0.2s;
}
.l3-tg-btn:hover { transform: translateY(-1px); }
.l3-tg-btn.chat { color: var(--btn-txt); background: linear-gradient(135deg, var(--grad-a), var(--grad-b)); }
.l3-tg-btn.chat:hover { opacity: 0.9; }
.l3-tg-btn.video { color: var(--tx1); border: 1px solid var(--bd2); }
.l3-tg-btn.video:hover { border-color: var(--gold); }

/* ════ pricing ════ */
.l3-pricing { position: relative; z-index: 2; max-width: 1080px; margin: 0 auto; padding: 30px 24px 120px; }
.l3-price-grid { display: grid; grid-template-columns: 1.2fr 1fr; gap: 56px; align-items: center; }
@media (max-width: 900px) { .l3-price-grid { grid-template-columns: 1fr; gap: 36px; } }
.l3-price-feats { margin: 18px 0 0; padding: 0; list-style: none; text-align: left; }
.l3-price-feats li { display: flex; align-items: flex-start; gap: 9px; margin-bottom: 10px; font-size: 14.5px; line-height: 1.55; color: var(--tx2); }
.l3-price-feats svg { flex-shrink: 0; margin-top: 3px; color: #34d399; }
.l3-price-cardwrap { position: relative; }
.l3-price-card {
  position: relative; display: flex; flex-direction: column; align-items: center; gap: 8px;
  padding: 44px 36px 36px; border-radius: 26px; text-align: center;
  background: var(--panel); border: 1px solid var(--bd2);
  transition: border-color 0.7s, box-shadow 0.7s;
}
.l3-pricing.lit .l3-price-card { border-color: rgba(251, 191, 36, 0.5); box-shadow: 0 0 80px rgba(251, 191, 36, 0.12); }
.l3-price-badge {
  position: absolute; top: -14px; left: 50%; transform: translateX(-50%) scale(0.6); opacity: 0;
  padding: 6px 16px; border-radius: 99px; font-size: 11px; font-weight: 800; letter-spacing: 0.1em;
  text-transform: uppercase; color: var(--btn-txt); background: linear-gradient(135deg, var(--grad-a), var(--grad-b));
}
.l3-pricing.lit .l3-price-badge { animation: l3-badge-pop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1) 0.4s forwards; }
@keyframes l3-badge-pop { to { opacity: 1; transform: translateX(-50%) scale(1); } }
.l3-price-amount {
  font-family: 'Syne', sans-serif; font-weight: 800; font-size: clamp(2.6rem, 5vw, 3.6rem); letter-spacing: -0.03em;
  background: linear-gradient(120deg, var(--grad-a), var(--grad-b)); -webkit-background-clip: text; background-clip: text; color: transparent;
}
.l3-price-period { font-size: 13px; font-weight: 600; color: var(--tx3); }
.l3-price-div { width: 100%; height: 1px; margin: 14px 0; background: var(--bd); }
.l3-price-note { font-size: 12px; font-weight: 600; color: var(--tx3); margin-bottom: 14px; }
.l3-wa {
  display: inline-flex; align-items: center; gap: 8px; margin-top: 10px; padding: 12px 20px; border-radius: 12px;
  font-size: 13.5px; font-weight: 700; color: #6ee7b7; text-decoration: none;
  border: 1px solid rgba(52, 211, 153, 0.35); transition: background 0.2s, border-color 0.2s;
}
.l3-wa:hover { background: rgba(52, 211, 153, 0.08); border-color: rgba(52, 211, 153, 0.6); }

/* ════ testimonials ════ */
.l3-testi { position: relative; z-index: 2; max-width: 1120px; margin: 0 auto; padding: 30px 24px 120px; text-align: center; }
.l3-testi-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 18px; margin-top: 44px; }
@media (max-width: 900px) { .l3-testi-grid { grid-template-columns: 1fr; } }
.l3-testi-card {
  margin: 0; padding: 26px 26px 24px; border-radius: 20px; text-align: left;
  background: var(--panel); border: 1px solid var(--bd);
  transition: border-color 0.3s, transform 0.3s, opacity 0.7s ease, filter 0.7s ease;
}
.l3-testi-card:hover { border-color: rgba(251, 191, 36, 0.35); transform: translateY(-4px); }
.l3-stars { display: block; margin-bottom: 12px; font-size: 14px; letter-spacing: 0.2em; color: var(--gold); }
.l3-testi-card blockquote { margin: 0 0 18px; font-size: 14.5px; line-height: 1.7; color: var(--tx2); }
.l3-testi-card figcaption { display: flex; align-items: center; gap: 11px; }
.l3-testi-av {
  display: grid; place-items: center; width: 38px; height: 38px; border-radius: 99px;
  font-family: 'Syne', sans-serif; font-weight: 800; color: var(--btn-txt);
  background: linear-gradient(135deg, var(--grad-a), var(--grad-b));
}
.l3-testi-card figcaption b { display: block; font-size: 13.5px; color: var(--tx1); }
.l3-testi-card figcaption i { display: block; font-style: normal; font-size: 11.5px; color: var(--tx3); }

/* ════ n8n pipeline (Landing 2) ════ */
.l2-n8n { position: relative; z-index: 2; max-width: 980px; margin: 0 auto; padding: 60px 24px 130px; text-align: center; }
.l2-pipe { display: flex; align-items: center; justify-content: center; gap: 0; margin-top: 54px; flex-wrap: wrap; }
.l2-pipe-node {
  display: flex; flex-direction: column; align-items: center; gap: 5px;
  padding: 18px 22px; border-radius: 16px; color: var(--tx2);
  background: var(--panel); border: 1px solid var(--bd);
  opacity: 0.4; transition: opacity 0.5s, border-color 0.5s, box-shadow 0.5s;
}
.l2-pipe.lit .l2-pipe-node { opacity: 1; }
.l2-pipe.lit .l2-pipe-node.is-src { transition-delay: 0s; }
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
  animation: l2-pulse 1.8s linear infinite; animation-play-state: paused;
}
.l2-pipe.lit .l2-pipe-pulse, .l3-brain.lit .l2-pipe-pulse { animation-play-state: running; }
@keyframes l2-pulse { from { transform: translateX(-24px); } to { transform: translateX(70px); } }
.l2-pipe-fan { display: flex; flex-direction: column; gap: 10px; }
.l2-pipe-fan .l2-pipe-node { flex-direction: row; padding: 11px 16px; gap: 9px; }
@media (max-width: 760px) {
  .l2-pipe { flex-direction: column; gap: 12px; }
  .l2-pipe-wire { width: 2px; height: 40px; }
  .l2-pipe-pulse { width: 2px; height: 18px; background: linear-gradient(180deg, transparent, #fbbf24, transparent); animation-name: l2-pulse-v; }
  @keyframes l2-pulse-v { from { transform: translateY(-20px); } to { transform: translateY(46px); } }
}

/* ════ lead form (Landing 2) ════ */
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
  animation: l2-spin 0.7s linear infinite;
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
.l3-footer { position: relative; z-index: 2; padding: 20px 24px 46px; border-top: 1px solid var(--bd); }
.l3-foot-grid {
  max-width: 1080px; margin: 24px auto 0; display: grid;
  grid-template-columns: 1.4fr 1fr 1fr 1fr; gap: 32px;
}
@media (max-width: 800px) { .l3-foot-grid { grid-template-columns: 1fr 1fr; } }
.l3-foot-brand p { margin: 12px 0 8px; font-size: 13.5px; line-height: 1.65; color: var(--tx3); max-width: 300px; }
.l3-foot-brand i { font-style: normal; font-size: 12px; font-weight: 700; color: var(--grad-a); }
.l3-foot-col { display: flex; flex-direction: column; gap: 9px; }
.l3-foot-col b { font-family: 'Syne', sans-serif; font-size: 13.5px; color: var(--tx1); margin-bottom: 4px; }
.l3-foot-col a { font-size: 13px; font-weight: 600; color: var(--tx3); text-decoration: none; transition: color 0.2s; }
.l3-foot-col a:hover { color: var(--gold); }
.l3-foot-bottom { max-width: 1080px; margin: 30px auto 0; padding-top: 18px; border-top: 1px solid var(--bd); font-size: 12.5px; color: var(--tx3); text-align: center; }
.l2-line-end { margin-bottom: 6px; }

/* ════ reveal — scale + blur entrance (data attr survives Vue class patching) ════ */
.reveal { opacity: 0; transform: translateY(26px) scale(0.985); filter: blur(5px); transition: opacity 0.7s ease, transform 0.7s ease, filter 0.7s ease; }
.reveal[data-in] { opacity: 1; transform: none; filter: none; }

/* ════ reduced motion ════ */
@media (prefers-reduced-motion: reduce) {
  .l2 *, .l2 *::before, .l2 *::after { animation: none !important; transition: none !important; }
  .reveal { opacity: 1; transform: none; filter: none; }
  .gx { opacity: 1 !important; transform: none !important; }
  .sc { filter: none; }
  .sc-standby { opacity: 0; }
  .sc1-eq { width: 100% !important; border-right-color: transparent !important; }
  .sc3-ring .ring-fg { stroke-dashoffset: 26 !important; }
  .sc4-conf { opacity: 0 !important; }
  .l2-pipe-node, .l3-scard { opacity: 1; }
  .l3-price-badge { opacity: 1; transform: translateX(-50%) scale(1); }
  .l3-badge { opacity: 1; transform: scale(1); }
  .fc3-flip { animation: none; }
  .gr3-row { opacity: 1; animation: none; }
}
</style>
