<template>
  <div class="landing" @mousemove="onMouseMove">
    <!-- Cursor spotlight specific to landing dark bg -->
    <div class="land-spotlight" :style="{ background: `radial-gradient(500px circle at ${mx}px ${my}px, rgba(124,106,245,0.1), transparent 65%)` }" />

    <!-- Neural net canvas background -->
    <canvas ref="bgCanvas" class="bg-canvas" />

    <!-- Aurora layers -->
    <div class="aurora-wrap">
      <div class="au au1"/><div class="au au2"/><div class="au au3"/>
    </div>

    <!-- ── NAV ── -->
    <nav class="land-nav" :class="{ scrolled: scrollY > 60 }">
      <div class="nav-in">
        <RouterLink to="/" class="nav-logo">
          <img src="@/assets/logo.png" alt="" class="nl-mark" />
          <span class="nl-text">Balochistan Academy Portal</span>
        </RouterLink>
        <div class="nav-links" :class="{ open: menuOpen }">
          <a href="#features" @click="menuOpen=false" class="nav-lnk">Features</a>
          <a href="#ai-scene" @click="menuOpen=false" class="nav-lnk">AI Tutors</a>
          <a href="#how" @click="menuOpen=false" class="nav-lnk">How It Works</a>
          <a href="#pricing" @click="menuOpen=false" class="nav-lnk">Pricing</a>
          <button class="nav-theme-btn" @click="themeStore.toggle()" :title="themeStore.isDark ? 'Switch to light mode' : 'Switch to dark mode'">
            <Sun v-if="themeStore.isDark" :size="16" />
            <Moon v-else :size="16" />
          </button>
          <RouterLink to="/login" class="nav-btn" @click="menuOpen=false">Get Started →</RouterLink>
        </div>
        <button class="nav-ham" @click="menuOpen=!menuOpen" :class="{active:menuOpen}">
          <span/><span/><span/>
        </button>
      </div>
    </nav>

    <!-- ── HERO ── -->
    <section class="hero">
      <div class="hero-in">
        <!-- Left content -->
        <div class="hero-copy" ref="heroCopy">
          <div class="hero-badge" ref="heroBadge">
            <span class="badge-pulse"/>&nbsp;Pakistan's #1 AI Exam Platform · Grade 9 Balochistan Board
          </div>

          <Transition name="slide-txt" mode="out-in">
            <div :key="currentSlide" class="hero-text-block">
              <h1 class="hero-h1">
                <span v-for="(line, li) in slideData[currentSlide].heading" :key="li"
                  class="h1-line"
                  :class="{ 'h1-gold': li === slideData[currentSlide].goldLine }">{{ line }}</span>
              </h1>
              <p class="hero-p">{{ slideData[currentSlide].desc }}</p>
            </div>
          </Transition>

          <div class="hero-btns" ref="heroBtns">
            <RouterLink to="/login" class="btn-hero-primary">
              <span>Start Learning Free</span>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
            </RouterLink>
            <a href="#ai-scene" class="btn-hero-ghost">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polygon points="10,8 16,12 10,16"/></svg>
              <span>See AI in Action</span>
            </a>
          </div>

          <!-- Live stats strip -->
          <div class="hero-stats" ref="heroStats">
            <div v-for="s in heroStats" :key="s.label" class="hstat">
              <span class="hstat-v">{{ s.val }}</span>
              <span class="hstat-l">{{ s.label }}</span>
            </div>
          </div>
        </div>

        <!-- ── Hero Slider 3D ── -->
        <div class="hero-slider-wrap" ref="heroVisual">

          <!-- 3D Scene: perspective + mouse-tracking tilt -->
          <div class="hs-scene"
               ref="sliderRef"
               @mousemove="onSliderMouseMove"
               @mouseleave="onSliderMouseLeave"
               @mouseenter="sliderHovering = true">

            <!-- Tilt layer rotates based on mouse position -->
            <div class="hs-tilt" :style="tiltStyle">

              <!-- The card surface with specular sheen + slide content -->
              <div class="hs-card">
                <div class="hs-sheen" :style="sheenStyle"/>
                <div class="hs-edge-glow"/>
                <div class="hs-scan-line"/>

                <!-- ── Product Scenes: Flashcards · AI Grading · Live Tutor ── -->
                <Transition :name="s3dName">
                  <div :key="currentSlide" class="hs-slide">

                    <!-- Shared scene header -->
                    <div class="sx-head">
                      <span class="sx-brand"><span class="sx-brand-dot"/>Balochistan Academy Portal</span>
                      <span class="sx-status" :class="`sx-${sceneMeta[currentSlide].tone}`">
                        <span class="sx-status-dot"/>{{ sceneMeta[currentSlide].label }}
                      </span>
                    </div>

                    <!-- ░░ SCENE 0 — Living Flashcard Deck ░░ -->
                    <div v-if="currentSlide === 0" class="sx-body fc-body">
                      <div class="fc-deck">
                        <div class="fc-peek fc-peek-2"/>
                        <div class="fc-peek fc-peek-1"/>
                        <div class="fc-flip">
                          <div class="fc-face fc-front">
                            <div class="fc-tag">PHYSICS · Ch 3</div>
                            <div class="fc-q-label">Question</div>
                            <div class="fc-q">If <b>F = 10 N</b> and <b>m = 2 kg</b>, find the acceleration <b>a</b>.</div>
                            <div class="fc-hint"><span class="fc-hint-key">↩</span> flip for answer</div>
                          </div>
                          <div class="fc-face fc-back">
                            <div class="fc-tutor">
                              <span class="fc-tutor-av">⚛</span>
                              <div>
                                <div class="fc-tutor-name">Einstein AI</div>
                                <div class="fc-tutor-role">Physics mentor</div>
                              </div>
                            </div>
                            <div class="fc-a-eq">a = F ÷ m = <b>5 m/s²</b></div>
                            <div class="fc-a-row">
                              <span class="fc-correct">✓ Correct</span>
                              <span class="fc-coin">+50 🪙</span>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="fc-pills">
                        <span class="fc-pill active">Physics</span>
                        <span class="fc-pill">Chemistry</span>
                        <span class="fc-pill">Biology</span>
                        <span class="fc-pill">اردو</span>
                        <span class="fc-pill">Math</span>
                      </div>
                    </div>

                    <!-- ░░ SCENE 1 — AI Grading a Paper ░░ -->
                    <div v-else-if="currentSlide === 1" class="sx-body gr-body">
                      <div class="gr-sheet">
                        <div class="gr-sheet-title">Answer Sheet · Physics</div>
                        <div v-for="(r, i) in gradeRows" :key="i"
                             class="gr-row" :class="{ 'gr-row-flag': r.pending }"
                             :style="{ '--d': i * 0.4 + 0.3 + 's' }">
                          <span class="gr-q">{{ r.q }}</span>
                          <span v-if="r.pending" class="gr-pencil">✎ analyzing…</span>
                          <span v-else class="gr-mark"><span class="gr-tick">✓</span>{{ r.mark }}</span>
                        </div>
                        <div class="gr-note">AI · revise <b>Q3 Kinematics</b> — 2 more MCQs</div>
                      </div>
                      <div class="gr-score">
                        <div class="gr-ring-wrap">
                          <svg class="gr-ring" viewBox="0 0 80 80" aria-hidden="true">
                            <circle class="gr-ring-bg" cx="40" cy="40" r="34"/>
                            <circle class="gr-ring-fg" cx="40" cy="40" r="34"
                              :stroke-dasharray="ringCirc"
                              :stroke-dashoffset="ringCirc * (1 - scoreVal / 100)"/>
                          </svg>
                          <div class="gr-score-num">{{ scoreVal }}<span>%</span></div>
                        </div>
                        <div class="gr-score-lbl">auto-graded</div>
                        <div class="gr-trend">▲ +18% this week</div>
                      </div>
                    </div>

                    <!-- ░░ SCENE 2 — Live Tutor Demo ░░ -->
                    <div v-else class="sx-body lt-body">
                      <div class="lt-tutor">
                        <span class="lt-av">🌹</span>
                        <div class="lt-id">
                          <div class="lt-name">Ghalib AI</div>
                          <div class="lt-subj">اُردو ادب · Classical Poetry</div>
                        </div>
                      </div>
                      <div class="lt-chat">
                        <div class="lt-msg lt-user">
                          <div class="lt-bub lt-bub-user urdu">شعر کی تشریح سمجھائیں؟</div>
                        </div>
                        <div class="lt-msg lt-ai lt-typing-row">
                          <span class="lt-ava">🌹</span>
                          <div class="lt-bub lt-typing"><span/><span/><span/></div>
                        </div>
                        <div class="lt-msg lt-ai lt-reply">
                          <span class="lt-ava">🌹</span>
                          <div class="lt-bub lt-bub-ai">
                            <span class="lt-ur urdu">یہ شعر اُمید اور حوصلے کی بات کرتا ہے</span>
                            <span class="lt-en">“It speaks of hope &amp; resolve.”</span>
                          </div>
                        </div>
                      </div>
                      <div class="lt-input">
                        <span class="lt-lang">اردو<span class="lt-lang-sep">·</span><b>EN</b></span>
                        <span class="lt-input-ph">Ask anything…</span>
                        <span class="lt-mic">🎙</span>
                        <span class="lt-send">➤</span>
                      </div>
                    </div>

                  </div>
                </Transition>

              </div><!-- end hs-card -->
            </div><!-- end hs-tilt -->
          </div><!-- end hs-scene -->

          <!-- ── Controls outside the 3D scene ── -->
          <button class="hs-arrow hs-prev" @click="prevSlide" aria-label="Previous slide">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M15 18l-6-6 6-6"/></svg>
          </button>
          <button class="hs-arrow hs-next" @click="nextSlide" aria-label="Next slide">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 18l6-6-6-6"/></svg>
          </button>

          <div class="hs-bottom-bar">
            <div class="hs-dots">
              <button v-for="(_, i) in slideData" :key="i"
                class="hs-dot" :class="{ active: currentSlide === i }"
                @click="goToSlide(i)"/>
            </div>
            <Transition name="fade-lbl" mode="out-in">
              <span :key="currentSlide" class="hs-lbl">
                {{ ['Flashcard Deck','AI Grading','Live Tutor'][currentSlide] }}
              </span>
            </Transition>
          </div>

        </div><!-- end hero-slider-wrap -->

      </div>

      <!-- Scroll indicator -->
      <div class="scroll-hint">
        <div class="scroll-mouse"><div class="scroll-wheel"/></div>
        <span>Scroll to explore</span>
      </div>
    </section>

    <!-- ── TICKER ── -->
    <div class="ticker-row">
      <div class="ticker-inner">
        <template v-for="n in 3" :key="n">
          <span v-for="s in tickerSubjects" :key="`${n}-${s.name}`" class="tick-item">
            <span class="tick-icon">{{ s.icon }}</span>{{ s.name }}
          </span>
        </template>
      </div>
    </div>

    <!-- ── AI STUDYING STUDENTS SCENE ── -->
    <section id="ai-scene" class="ai-scene" ref="sceneRef">
      <div class="section-wrap">
        <div class="sec-tag">AI Intelligence</div>
        <h2 class="sec-heading">Watch AI Study<br /><span class="grad-text">Your Students</span></h2>
        <p class="sec-sub">Our AI doesn't just answer — it tracks, adapts, and personalizes every session.</p>
      </div>

      <!-- The main visual scene -->
      <div class="scene-stage" ref="stageRef">
        <!-- Central AI brain -->
        <div class="ai-brain" :class="{ visible: sceneVisible }">
          <div class="brain-rings">
            <div class="brain-ring br1" />
            <div class="brain-ring br2" />
            <div class="brain-ring br3" />
          </div>
          <div class="brain-core">
            <svg width="56" height="56" viewBox="0 0 56 56" fill="none">
              <circle cx="28" cy="28" r="26" stroke="url(#brainG)" stroke-width="1.5" fill="rgba(124,106,245,0.08)" />
              <path d="M18 28c0-5.5 4.5-10 10-10s10 4.5 10 10c0 3-1.3 5.7-3.4 7.6M28 18v4M28 38v-4M21 25l2.5 1.5M35 25l-2.5 1.5" stroke="url(#brainG)" stroke-width="1.5" stroke-linecap="round" />
              <defs>
                <linearGradient id="brainG" x1="0" y1="0" x2="56" y2="56">
                  <stop stop-color="#7c6af5"/><stop offset="1" stop-color="#06b6d4"/>
                </linearGradient>
              </defs>
            </svg>
            <div class="brain-label">AI ENGINE</div>
          </div>
        </div>

        <!-- Connection lines -->
        <svg class="conn-svg" viewBox="0 0 1000 500" preserveAspectRatio="xMidYMid meet">
          <defs>
            <linearGradient id="lineG1" x1="0" y1="0" x2="1" y2="0">
              <stop stop-color="#7c6af5" stop-opacity="0.6"/>
              <stop offset="1" stop-color="#7c6af5" stop-opacity="0"/>
            </linearGradient>
            <linearGradient id="lineG2" x1="0" y1="0" x2="1" y2="0">
              <stop stop-color="#fbbf24" stop-opacity="0.6"/>
              <stop offset="1" stop-color="#fbbf24" stop-opacity="0"/>
            </linearGradient>
            <linearGradient id="lineG3" x1="1" y1="0" x2="0" y2="0">
              <stop stop-color="#06b6d4" stop-opacity="0.6"/>
              <stop offset="1" stop-color="#06b6d4" stop-opacity="0"/>
            </linearGradient>
          </defs>
          <!-- Lines from students to brain -->
          <line x1="160" y1="140" x2="500" y2="250" stroke="url(#lineG1)" stroke-width="1.5" stroke-dasharray="6 4" class="conn-line" />
          <line x1="160" y1="360" x2="500" y2="250" stroke="url(#lineG2)" stroke-width="1.5" stroke-dasharray="6 4" class="conn-line" style="animation-delay:0.4s" />
          <line x1="840" y1="140" x2="500" y2="250" stroke="url(#lineG3)" stroke-width="1.5" stroke-dasharray="6 4" class="conn-line" style="animation-delay:0.8s" />
          <line x1="840" y1="360" x2="500" y2="250" stroke="url(#lineG1)" stroke-width="1.5" stroke-dasharray="6 4" class="conn-line" style="animation-delay:1.2s" />
          <!-- Animated data pulses -->
          <circle r="4" fill="#7c6af5" opacity="0.8">
            <animateMotion dur="2s" repeatCount="indefinite" path="M160,140 Q330,190 500,250" />
          </circle>
          <circle r="4" fill="#fbbf24" opacity="0.8">
            <animateMotion dur="2.4s" repeatCount="indefinite" begin="0.6s" path="M160,360 Q330,310 500,250" />
          </circle>
          <circle r="4" fill="#06b6d4" opacity="0.8">
            <animateMotion dur="2.2s" repeatCount="indefinite" begin="1.2s" path="M840,140 Q670,190 500,250" />
          </circle>
          <circle r="4" fill="#a78bfa" opacity="0.8">
            <animateMotion dur="2.6s" repeatCount="indefinite" begin="1.8s" path="M840,360 Q670,310 500,250" />
          </circle>
        </svg>

        <!-- Student cards -->
        <div v-for="(st, i) in students" :key="st.name"
          class="student-card"
          :class="[`sc-${i+1}`, { visible: sceneVisible }]"
          :style="{ animationDelay: `${i*0.2}s` }"
        >
          <div class="sc-top">
            <div class="sc-avatar" :style="{ background: st.grad }">{{ st.avatar }}</div>
            <div class="sc-info">
              <div class="sc-name">{{ st.name }}</div>
              <div class="sc-subject">{{ st.subject }}</div>
            </div>
            <div class="sc-score" :style="{ color: st.scoreColor }">{{ st.score }}</div>
          </div>
          <!-- AI analysis overlay -->
          <div class="sc-analysis">
            <div class="analysis-bar" v-for="(bar, j) in st.bars" :key="j">
              <div class="ab-label">{{ bar.label }}</div>
              <div class="ab-track">
                <div class="ab-fill" :style="{ width: sceneVisible ? bar.pct + '%' : '0%', background: bar.color, transitionDelay: `${0.5 + j*0.15 + i*0.2}s` }" />
              </div>
              <div class="ab-pct">{{ bar.pct }}%</div>
            </div>
          </div>
          <!-- Scan overlay -->
          <div class="sc-scan" />
        </div>

        <!-- AI insight badges -->
        <div v-for="(b, i) in aiBadges" :key="i" :class="['ai-badge', b.pos, { visible: sceneVisible }]" :style="{ animationDelay: `${1.5+i*0.3}s` }">
          <span class="aib-icon">{{ b.icon }}</span>
          <span>{{ b.text }}</span>
        </div>
      </div>
    </section>

    <!-- ── FEATURES ── -->
    <section id="features" class="features-section" ref="featRef">
      <div class="section-wrap">
        <div class="sec-tag">Everything You Need</div>
        <h2 class="sec-heading">One Platform.<br /><span class="grad-text">Infinite Possibilities.</span></h2>
        <p class="sec-sub">From AI tutoring to competitive tests, coins to reports — built for Balochistan Board Grade 9.</p>
      </div>
      <div class="feat-grid">
        <div v-for="(f, i) in features" :key="f.title"
          class="feat-card" ref="featCards"
          :style="{ animationDelay: `${i * 0.08}s` }"
          :class="{ visible: featVisible }"
        >
          <div class="feat-icon-wrap" :style="{ background: f.grd }">
            <span class="feat-icon-inner">{{ f.icon }}</span>
          </div>
          <h3 class="feat-title">{{ f.title }}</h3>
          <p class="feat-desc">{{ f.desc }}</p>
          <div class="feat-arrow">→</div>
        </div>
      </div>
    </section>

    <!-- ── AI TUTORS ── -->
    <section id="tutors" class="tutors-section">
      <div class="section-wrap">
        <div class="sec-tag">8 Legendary Personas</div>
        <h2 class="sec-heading">Learn from the<br /><span class="grad-text">Greatest Minds</span></h2>
      </div>
      <div class="tutors-scroll">
        <div class="tutors-track">
          <div v-for="t in [...tutors, ...tutors]" :key="t.name+Math.random()" class="tutor-chip">
            <div class="tc-avatar" :style="{ background: t.grad }">{{ t.icon }}</div>
            <div>
              <div class="tc-name">{{ t.name }}</div>
              <div class="tc-subj">{{ t.subject }}</div>
            </div>
          </div>
        </div>
      </div>
      <div class="tutors-grid-full">
        <div v-for="t in tutors" :key="t.name" class="tg-card">
          <div class="tgc-head" :style="{ background: t.grad }">
            <span class="tgc-icon">{{ t.icon }}</span>
          </div>
          <div class="tgc-body">
            <div class="tgc-name">{{ t.name }}</div>
            <div class="tgc-subj">{{ t.subject }}</div>
            <div class="tgc-quote">"{{ t.quote }}"</div>
            <div class="tgc-actions">
              <RouterLink to="/login" class="tgc-btn chat">Chat</RouterLink>
              <RouterLink to="/login" class="tgc-btn video">Video</RouterLink>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── HOW IT WORKS ── -->
    <section id="how" class="how-section">
      <div class="section-wrap">
        <div class="sec-tag">3 Easy Steps</div>
        <h2 class="sec-heading">Start in Minutes.<br /><span class="grad-text">Improve in Days.</span></h2>
      </div>
      <div class="steps-row">
        <div v-for="(step, i) in steps" :key="i" class="step-block">
          <div class="step-num-glow">
            <span class="step-num">{{ String(i+1).padStart(2,'0') }}</span>
          </div>
          <div class="step-icon-big">{{ step.icon }}</div>
          <h3 class="step-title">{{ step.title }}</h3>
          <p class="step-desc">{{ step.desc }}</p>
          <div v-if="i < 2" class="step-arrow">↓</div>
        </div>
      </div>
    </section>

    <!-- ── PRICING ── -->
    <section id="pricing" class="pricing-section">
      <div class="pricing-wrap">
        <div class="pricing-left">
          <div class="sec-tag">Affordable</div>
          <h2 class="sec-heading">Full Access<br /><span class="grad-text">Rs. 999/Year</span></h2>
          <p class="sec-sub">Under Rs. 83/month. One subscription unlocks everything.</p>
          <ul class="pricing-feats">
            <li v-for="f in pricingFeats" :key="f">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#34d399" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg>
              {{ f }}
            </li>
          </ul>
          <div class="pricing-btns">
            <RouterLink to="/login" class="btn-hero-primary">Get Full Access →</RouterLink>
            <a href="https://wa.me/923001234567" target="_blank" class="btn-wa">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z"/></svg>
              Pay via WhatsApp
            </a>
          </div>
        </div>
        <div class="pricing-card-wrap">
          <div class="pricing-card-glow" />
          <div class="pricing-card">
            <div class="pc-badge">Most Popular</div>
            <div class="pc-amount">Rs. 999</div>
            <div class="pc-period">per year · unlimited access</div>
            <div class="pc-divider" />
            <div class="pc-subjects">
              <span v-for="s in subjectIcons" :key="s" class="pc-sicon">{{ s }}</span>
            </div>
            <div class="pc-note">Easypaisa / JazzCash / Bank Transfer</div>
            <RouterLink to="/login" class="pc-cta">Start Now →</RouterLink>
          </div>
        </div>
      </div>
    </section>

    <!-- ── TESTIMONIALS ── -->
    <section class="testi-section">
      <div class="section-wrap">
        <div class="sec-tag">Student Reviews</div>
        <h2 class="sec-heading">Real Students.<br /><span class="grad-text">Real Results.</span></h2>
      </div>
      <div class="testi-grid">
        <div v-for="t in testimonials" :key="t.name" class="testi-card">
          <div class="testi-stars">★★★★★</div>
          <p class="testi-text">"{{ t.text }}"</p>
          <div class="testi-author">
            <div class="testi-av">{{ t.name[0] }}</div>
            <div>
              <div class="testi-name">{{ t.name }}</div>
              <div class="testi-school">{{ t.school }}</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── FINAL CTA ── -->
    <section class="final-cta">
      <div class="fca-inner">
        <div class="fca-glow" />
        <div class="sec-tag" style="margin:0 auto 1rem;display:table">Limited Time</div>
        <h2 class="fca-h2">Ready to Transform<br /><span class="grad-text">Your Exam Results?</span></h2>
        <p class="fca-p">Join thousands of Grade 9 students already crushing their board exams with Balochistan Academy Portal.</p>
        <RouterLink to="/login" class="btn-hero-primary large">
          <span>Start for Free — No Card Required</span>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
        </RouterLink>
      </div>
    </section>

    <!-- ── FOOTER ── -->
    <footer class="land-footer">
      <div class="footer-in">
        <div class="footer-brand">
          <RouterLink to="/" class="nav-logo" style="margin-bottom:0.75rem;display:flex">
            <img src="@/assets/logo.png" alt="" class="nl-mark" />
            <span class="nl-text">Balochistan Academy Portal</span>
          </RouterLink>
          <p>AI-powered exam preparation for Grade 9 Balochistan Board students in English & Urdu medium.</p>
          <div class="footer-inst">New Century Educational System</div>
        </div>
        <div class="footer-cols">
          <div class="footer-col">
            <div class="footer-hd">Platform</div>
            <a href="#features">Features</a><a href="#ai-scene">AI Tutors</a>
            <a href="#how">How It Works</a><a href="#pricing">Pricing</a>
          </div>
          <div class="footer-col">
            <div class="footer-hd">Subjects</div>
            <span v-for="s in footerSubjs" :key="s">{{ s }}</span>
          </div>
          <div class="footer-col">
            <div class="footer-hd">Contact</div>
            <a href="https://wa.me/923001234567" target="_blank">WhatsApp Support</a>
            <RouterLink to="/login">Student Login</RouterLink>
          </div>
        </div>
      </div>
      <div class="footer-bottom">
        <span>© 2026 Balochistan Academy Portal · New Century Educational System</span>
        <span>Built with ❤️ for Pakistan's students</span>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { Sun, Moon } from '@lucide/vue'
import { useThemeStore } from '@/stores/theme'

const themeStore = useThemeStore()

const mx = ref(0), my = ref(0)
const menuOpen = ref(false)
const scrollY = ref(0)
const bgCanvas = ref(null)
const sceneRef = ref(null)
const stageRef = ref(null)
const featRef = ref(null)
const sceneVisible = ref(false)
const featVisible = ref(false)
let canvasAnim = null
const cPts = []
const currentSlide = ref(0)
const direction = ref(1)        // 1=forward, -1=backward
const sliderHovering = ref(false)
const sliderRef = ref(null)
const cardMx = ref(0), cardMy = ref(0)
let sliderInterval = null

const s3dName = computed(() => direction.value > 0 ? 's3d-next' : 's3d-prev')

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

const slideData = [
  {
    heading: ['Flip a Card.', 'Master Any', 'Concept.'],
    goldLine: 2,
    desc: 'Bite-size AI flashcards for every Grade 9 chapter — the question on the front, a legendary tutor’s explanation on the back. Tap, learn, repeat.',
  },
  {
    heading: ['Submit Once.', 'AI Grades It', 'in Seconds.'],
    goldLine: 1,
    desc: 'Snap or type your answer and watch it marked instantly — score, mistakes and a personalised fix-list, just like a real Balochistan Board examiner.',
  },
  {
    heading: ['Ask Anything,', 'in Urdu or', 'English.'],
    goldLine: 1,
    desc: '8 legendary AI tutors answer your questions live — chat or voice, English & Urdu medium, available 24/7 right up to exam day.',
  },
]

/* Scene header metadata (status chip per slide) */
const sceneMeta = [
  { label: 'FLASHCARDS', tone: 'violet' },
  { label: 'AI MARKING', tone: 'cyan' },
  { label: 'LIVE SESSION', tone: 'green' },
]

/* Scene 1 — answer-sheet rows */
const gradeRows = [
  { q: 'Q1 · Newton II law', mark: '+2' },
  { q: 'Q2 · Ionic bonding', mark: '+2' },
  { q: 'Q3 · Kinematics', pending: true },
  { q: 'Q4 · Ohm’s law', mark: '+2' },
]

/* Scene 1 — score ring that counts up each time the slide is shown */
const ringCirc = 2 * Math.PI * 34
const scoreVal = ref(0)
let scoreRaf = null
function animateScore() {
  cancelAnimationFrame(scoreRaf)
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

function nextSlide() { direction.value = 1; currentSlide.value = (currentSlide.value + 1) % 3; resetSliderTimer() }
function prevSlide() { direction.value = -1; currentSlide.value = (currentSlide.value + 2) % 3; resetSliderTimer() }
function goToSlide(i) { direction.value = i > currentSlide.value ? 1 : -1; currentSlide.value = i; resetSliderTimer() }
function startSlider() { sliderInterval = setInterval(() => { direction.value = 1; currentSlide.value = (currentSlide.value + 1) % 3 }, 5000) }
function resetSliderTimer() { clearInterval(sliderInterval); startSlider() }

function onMouseMove(e) { mx.value = e.clientX; my.value = e.clientY }
function onScroll() { scrollY.value = window.scrollY }

// Intersection observer for scroll animations
function makeObserver(target, flag, threshold = 0.2) {
  if (!target) return
  const obs = new IntersectionObserver(([e]) => { if (e.isIntersecting) { flag.value = true; obs.disconnect() } }, { threshold })
  obs.observe(target)
}

// Background neural net canvas
function initBgCanvas() {
  const c = bgCanvas.value
  if (!c) return
  c.width = window.innerWidth; c.height = window.innerHeight
  for (let i = 0; i < 70; i++) {
    cPts.push({ x: Math.random()*c.width, y: Math.random()*c.height, vx:(Math.random()-.5)*.3, vy:(Math.random()-.5)*.3, r:Math.random()*1.5+.5 })
  }
  function draw() {
    const ctx = c.getContext('2d')
    ctx.clearRect(0, 0, c.width, c.height)
    cPts.forEach(p => {
      p.x+=p.vx; p.y+=p.vy
      if(p.x<0||p.x>c.width)p.vx*=-1; if(p.y<0||p.y>c.height)p.vy*=-1
      ctx.beginPath(); ctx.arc(p.x,p.y,p.r,0,Math.PI*2)
      ctx.fillStyle='rgba(124,106,245,0.25)'; ctx.fill()
    })
    for(let i=0;i<cPts.length;i++) for(let j=i+1;j<cPts.length;j++){
      const dx=cPts[i].x-cPts[j].x,dy=cPts[i].y-cPts[j].y,d=Math.sqrt(dx*dx+dy*dy)
      if(d<130){ ctx.beginPath(); ctx.moveTo(cPts[i].x,cPts[i].y); ctx.lineTo(cPts[j].x,cPts[j].y)
        ctx.strokeStyle=`rgba(124,106,245,${0.12*(1-d/130)})`; ctx.lineWidth=.5; ctx.stroke() }
    }
    canvasAnim = requestAnimationFrame(draw)
  }
  draw()
}

onMounted(() => {
  window.addEventListener('scroll', onScroll, { passive: true })
  initBgCanvas()
  startSlider()
  setTimeout(() => {
    makeObserver(sceneRef.value, sceneVisible)
    makeObserver(featRef.value, featVisible)
  }, 100)
})
onUnmounted(() => {
  window.removeEventListener('scroll', onScroll)
  cancelAnimationFrame(canvasAnim)
  cancelAnimationFrame(scoreRaf)
  clearInterval(sliderInterval)
})

// Data
const heroStats = [
  { val: '10+', label: 'Subjects' }, { val: '8', label: 'AI Tutors' },
  { val: '1200+', label: 'Questions' }, { val: '99%', label: 'Pass Rate' },
]
const tickerSubjects = [
  { icon: '⚛', name: 'Physics' }, { icon: '➕', name: 'Mathematics' },
  { icon: '⚗', name: 'Chemistry' }, { icon: '🧬', name: 'Biology' },
  { icon: '💻', name: 'Computer Sc.' }, { icon: '🇵🇰', name: 'Pak Studies' },
  { icon: '📖', name: 'English' }, { icon: '✍', name: 'Urdu' },
  { icon: '📐', name: 'Math (Urdu)' }, { icon: '🌍', name: 'Islamiyat' },
]

const students = [
  { name: 'Ahmed K.', subject: 'Physics', avatar: '👨‍🎓', score: '92%', scoreColor: '#4ade80',
    grad: 'linear-gradient(135deg,#1e1a40,#4a35a6)',
    bars: [{ label: 'Accuracy', pct: 92, color: '#7c6af5' }, { label: 'Speed', pct: 78, color: '#06b6d4' }, { label: 'Retention', pct: 85, color: '#fbbf24' }] },
  { name: 'Fatima R.', subject: 'Chemistry', avatar: '👩‍🎓', score: '87%', scoreColor: '#06b6d4',
    grad: 'linear-gradient(135deg,#1a0d2e,#6d28d9)',
    bars: [{ label: 'Accuracy', pct: 87, color: '#a78bfa' }, { label: 'Speed', pct: 91, color: '#06b6d4' }, { label: 'Retention', pct: 82, color: '#f59e0b' }] },
  { name: 'Bilal M.', subject: 'Math', avatar: '🧑‍🎓', score: '95%', scoreColor: '#fbbf24',
    grad: 'linear-gradient(135deg,#0d2a0d,#15803d)',
    bars: [{ label: 'Accuracy', pct: 95, color: '#4ade80' }, { label: 'Speed', pct: 88, color: '#7c6af5' }, { label: 'Retention', pct: 90, color: '#fbbf24' }] },
  { name: 'Sara A.', subject: 'Biology', avatar: '👩‍🔬', score: '89%', scoreColor: '#f87171',
    grad: 'linear-gradient(135deg,#2a0d0d,#991b1b)',
    bars: [{ label: 'Accuracy', pct: 89, color: '#f87171' }, { label: 'Speed', pct: 74, color: '#06b6d4' }, { label: 'Retention', pct: 93, color: '#a78bfa' }] },
]
const aiBadges = [
  { icon: '🎯', text: 'Weakness detected: Kinematics', pos: 'ab-top' },
  { icon: '💡', text: 'Recommended: 5 more MCQs', pos: 'ab-mid' },
  { icon: '📈', text: 'Score improved +18% this week', pos: 'ab-bot' },
]

const features = [
  { icon: '🤖', grd: 'linear-gradient(135deg,rgba(124,106,245,0.3),rgba(91,67,204,0.1))', title: '8 AI Tutors', desc: 'Einstein, Ghalib, Curie & more — each subject gets a legendary AI mentor.' },
  { icon: '📝', grd: 'linear-gradient(135deg,rgba(16,185,129,0.3),rgba(5,150,105,0.1))', title: 'Daily Tests', desc: 'Auto-generated MCQ and subjective tests with instant AI grading.' },
  { icon: '🏆', grd: 'linear-gradient(135deg,rgba(245,158,11,0.3),rgba(217,119,6,0.1))', title: 'Competition', desc: 'Monthly institute-wide exams with leaderboards and mega challenges.' },
  { icon: '🪙', grd: 'linear-gradient(135deg,rgba(251,191,36,0.3),rgba(245,158,11,0.1))', title: 'Coins & Rewards', desc: 'Earn real coins for test performance. Withdraw via Easypaisa.' },
  { icon: '🎥', grd: 'linear-gradient(135deg,rgba(139,92,246,0.3),rgba(109,40,217,0.1))', title: 'Video Lessons', desc: 'AI-voiced video lessons with voice Q&A powered by Google TTS.' },
  { icon: '📊', grd: 'linear-gradient(135deg,rgba(6,182,212,0.3),rgba(8,145,178,0.1))', title: 'Reports', desc: 'Detailed charts for students, parents and teachers in one dashboard.' },
  { icon: '✍️', grd: 'linear-gradient(135deg,rgba(236,72,153,0.3),rgba(219,39,119,0.1))', title: 'Subjective AI', desc: 'Write long answers with voice input. Gemini grades & gives feedback.' },
  { icon: '📚', grd: 'linear-gradient(135deg,rgba(20,184,166,0.3),rgba(13,148,136,0.1))', title: 'Digital Library', desc: '10 subjects, key notes, past papers, simulations and virtual labs.' },
]

const tutors = [
  { icon: '⚛', name: 'Einstein', subject: 'Physics', grad: 'linear-gradient(135deg,#1e1a40,#4a35a6)', quote: 'Imagination is the key to discovery.' },
  { icon: '∑', name: 'Al-Khwarizmi', subject: 'Mathematics', grad: 'linear-gradient(135deg,#0d2a0d,#16a34a)', quote: 'Every problem has a beautiful solution.' },
  { icon: '⚗', name: 'Marie Curie', subject: 'Chemistry', grad: 'linear-gradient(135deg,#2a0d1a,#be185d)', quote: 'Nothing is to be feared — only understood.' },
  { icon: '🧬', name: 'Ibn Sina', subject: 'Biology', grad: 'linear-gradient(135deg,#0d2233,#0891b2)', quote: 'The body and soul work as one.' },
  { icon: '💡', name: 'Turing', subject: 'Computer Sci', grad: 'linear-gradient(135deg,#1e2a0d,#4d7c0f)', quote: 'We can only see ahead if we look.' },
  { icon: '🌙', name: 'Allama Iqbal', subject: 'Pak Studies', grad: 'linear-gradient(135deg,#1a1200,#b45309)', quote: "Nations are born in poets' hearts." },
  { icon: '📜', name: 'Shakespeare', subject: 'English', grad: 'linear-gradient(135deg,#2a1a00,#92400e)', quote: 'We know what we are, not what we may be.' },
  { icon: '🌹', name: 'Ghalib', subject: 'Urdu', grad: 'linear-gradient(135deg,#2a0d0d,#991b1b)', quote: 'ہزاروں خواہشیں ایسی…' },
]

const steps = [
  { icon: '📱', title: 'Create Account', desc: 'Sign up in 30 seconds with your phone number. No email required.' },
  { icon: '📚', title: 'Pick a Subject', desc: 'Browse 10 subjects, choose your AI tutor and start learning instantly.' },
  { icon: '🏆', title: 'Learn & Earn', desc: 'Take daily tests, earn coins, track progress and smash your board exams.' },
]
const pricingFeats = [
  'Unlimited AI Tutor sessions (all 8 subjects)',
  'Full question bank — 1,200+ MCQ + Subjective',
  'AI-graded subjective answers with detailed feedback',
  'Daily & monthly competition tests with leaderboards',
  'Coin rewards redeemable for real cash',
  'Parent-visible performance reports with graphs',
  'Voice-driven AI video lessons (Google TTS)',
  'English & Urdu medium support',
]
const subjectIcons = ['⚛', '∑', '⚗', '🧬', '💻', '🇵🇰', '📖', '✍', '📐', '🌍']
const testimonials = [
  { name: 'Ahmed Khan', school: 'New Century School, Quetta', text: 'Einstein AI explained Physics better than any teacher! My score jumped from 55% to 92% in one month.' },
  { name: 'Fatima Raza', school: 'Govt Girls High School, Quetta', text: 'The Ghalib AI tutor explains Urdu beautifully in proper Urdu with examples. I got A+ in my board exam!' },
  { name: 'Bilal Mengal', school: 'Balochistan Model School', text: 'I earned Rs. 650 in coins last month just by doing daily tests. My parents are so proud of me!' },
]
const footerSubjs = ['Physics', 'Mathematics', 'Chemistry', 'Biology', 'Computer Sci.', 'Pakistan Studies']
</script>

<style scoped>
/* ─── Tokens ─── */
.landing {
  --bg: var(--t-bg);
  --bg2: var(--t-bg2);
  --accent: var(--t-accent);
  --accent2: var(--t-accent2);
  --gold: var(--t-gold);
  --cyan: #06b6d4;
  --text1: var(--t-text1);
  --text2: var(--t-text2);
  --text3: var(--t-text3);
  --border: var(--t-border);
  --card: var(--t-hover);
  background: var(--t-bg);
  color: var(--text1);
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
  overflow-x: hidden;
  cursor: none;
}
/* Spotlight */
.land-spotlight { position: fixed; inset: 0; z-index: 0; pointer-events: none; transition: none; }

/* BG canvas */
.bg-canvas { position: fixed; inset: 0; width: 100%; height: 100%; z-index: 0; pointer-events: none; opacity: 0.7; }

/* Aurora */
.aurora-wrap { position: fixed; inset: 0; z-index: 0; pointer-events: none; }
.au { position: absolute; border-radius: 50%; filter: blur(100px); animation: auMove 15s ease-in-out infinite alternate; }
.au1 { width: 700px; height: 700px; top: -200px; left: -200px; background: radial-gradient(circle, var(--t-aurora1), transparent 70%); }
.au2 { width: 600px; height: 600px; bottom: -200px; right: -100px; background: radial-gradient(circle, var(--t-aurora2), transparent 70%); animation-delay: 5s; }
.au3 { width: 500px; height: 500px; top: 50%; left: 50%; transform: translate(-50%,-50%); background: radial-gradient(circle, var(--t-aurora3), transparent 70%); animation-delay: 10s; }
@keyframes auMove { from { transform: translate(0,0) } to { transform: translate(40px, 30px) } }

/* ─── NAV ─── */
.land-nav {
  position: fixed; top: 0; left: 0; right: 0; z-index: 200;
  transition: background 0.3s, box-shadow 0.3s;
}
.land-nav.scrolled {
  background: var(--bg); backdrop-filter: blur(20px) saturate(180%);
  box-shadow: 0 1px 0 var(--border);
}
.nav-in {
  max-width: 1280px; margin: 0 auto; padding: 0 1.5rem; height: 68px;
  display: flex; align-items: center; gap: 2rem; position: relative; z-index: 1;
}
.nav-logo { display: flex; align-items: center; gap: 0.65rem; text-decoration: none; flex-shrink: 0; }
.nl-mark {
  width: 46px; height: 46px; border-radius: 11px;
  object-fit: contain; flex-shrink: 0;
}
.nl-text { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1.15rem; color: var(--text1); }
.nav-links { display: flex; align-items: center; gap: 0.25rem; margin-left: auto; }
.nav-lnk {
  padding: 0.5rem 1rem; border-radius: 10px; color: var(--text2);
  font-size: 0.875rem; font-weight: 500; text-decoration: none; transition: all 0.15s;
}
.nav-lnk:hover { background: var(--card); color: var(--text1); }
.nav-btn {
  margin-left: 0.5rem; padding: 0.55rem 1.25rem; border-radius: 11px;
  background: linear-gradient(135deg, var(--accent), var(--accent2));
  color: white; font-size: 0.875rem; font-weight: 600; text-decoration: none;
  transition: all 0.2s; box-shadow: 0 0 18px rgba(124,106,245,0.4);
}
.nav-btn:hover { transform: translateY(-1px); box-shadow: 0 0 28px rgba(124,106,245,0.6); }
.nav-theme-btn {
  width: 34px; height: 34px; border-radius: 10px; display: flex; align-items: center; justify-content: center;
  background: var(--card); border: 1px solid var(--border); color: var(--text2);
  cursor: none; transition: all 0.15s; flex-shrink: 0;
}
.nav-theme-btn:hover { color: var(--text1); background: rgba(255,255,255,0.08); }
.nav-ham { display: none; flex-direction: column; gap: 5px; background: none; border: none; cursor: none; margin-left: auto; padding: 4px; }
.nav-ham span { display: block; width: 22px; height: 2px; background: var(--text2); border-radius: 2px; transition: 0.3s; }
.nav-ham.active span:nth-child(1) { transform: rotate(45deg) translate(5px,5px); }
.nav-ham.active span:nth-child(2) { opacity: 0; }
.nav-ham.active span:nth-child(3) { transform: rotate(-45deg) translate(5px,-5px); }
@media (max-width: 768px) {
  .nav-ham { display: flex; }
  .nav-links {
    display: none; position: fixed; inset: 68px 0 0 0;
    background: var(--bg); flex-direction: column; align-items: stretch;
    padding: 1.5rem; gap: 0.5rem; z-index: 99;
  }
  .nav-links.open { display: flex; }
  .nav-lnk, .nav-btn { padding: 1rem; text-align: center; border-radius: 12px; font-size: 1rem; }
  .nav-btn { margin-left: 0; }
}

/* ─── HERO ─── */
.hero {
  min-height: 100vh; padding: 100px 1.5rem 80px;
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  position: relative; z-index: 1;
}
.hero-in {
  max-width: 1280px; width: 100%; margin: 0 auto;
  display: flex; align-items: center; gap: 4rem;
}
.hero-copy { flex: 1; max-width: 560px; }
.hero-badge {
  display: inline-flex; align-items: center; gap: 0.5rem;
  padding: 0.45rem 1.1rem; border-radius: 99px;
  border: 1px solid rgba(124,106,245,0.3); background: rgba(124,106,245,0.08);
  color: var(--text2); font-size: 0.78rem; font-weight: 500;
  margin-bottom: 1.75rem; animation: fadeUp 0.6s ease-out;
}
.badge-pulse { width: 6px; height: 6px; border-radius: 50%; background: var(--accent); box-shadow: 0 0 8px var(--accent); animation: blink 1.5s infinite; }
@keyframes blink { 0%,100%{opacity:1}50%{opacity:.3} }
@keyframes fadeUp { from{opacity:0;transform:translateY(16px)}to{opacity:1;transform:translateY(0)} }

.hero-h1 {
  font-family: 'Syne', sans-serif; font-weight: 800;
  font-size: clamp(2.8rem, 6vw, 4.5rem);
  line-height: 1.02; letter-spacing: -0.035em; margin: 0 0 1.5rem;
  animation: fadeUp 0.6s ease-out 0.1s both;
}
.h1-line { display: block; }
.h1-gold {
  background: linear-gradient(90deg, var(--gold) 0%, #f97316 40%, var(--accent) 100%);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
}
.hero-p {
  color: var(--text2); font-size: 1.05rem; line-height: 1.75; max-width: 480px; margin: 0 0 2rem;
  animation: fadeUp 0.6s ease-out 0.2s both;
}
.hero-btns { display: flex; gap: 1rem; flex-wrap: wrap; margin-bottom: 2.5rem; animation: fadeUp 0.6s ease-out 0.3s both; }
.btn-hero-primary {
  display: inline-flex; align-items: center; gap: 0.5rem;
  padding: 0.9rem 1.9rem; border-radius: 14px;
  background: linear-gradient(135deg, var(--accent), var(--accent2));
  color: white; font-weight: 700; font-size: 0.95rem; text-decoration: none;
  transition: all 0.2s; box-shadow: 0 0 28px rgba(124,106,245,0.45); cursor: none;
}
.btn-hero-primary:hover { transform: translateY(-3px); box-shadow: 0 0 44px rgba(124,106,245,0.65); }
.btn-hero-primary.large { padding: 1.1rem 2.5rem; font-size: 1.05rem; }
.btn-hero-ghost {
  display: inline-flex; align-items: center; gap: 0.5rem;
  padding: 0.9rem 1.5rem; border-radius: 14px;
  border: 1px solid var(--border); color: var(--text2);
  font-weight: 500; font-size: 0.95rem; text-decoration: none; transition: all 0.2s; cursor: none;
}
.btn-hero-ghost:hover { border-color: rgba(255,255,255,0.15); color: var(--text1); background: var(--card); }
.hero-stats {
  display: flex; gap: 2.5rem; flex-wrap: wrap;
  animation: fadeUp 0.6s ease-out 0.4s both;
}
.hstat { display: flex; flex-direction: column; gap: 0.1rem; }
.hstat-v { font-family: 'Orbitron', sans-serif; font-weight: 700; font-size: 1.35rem; color: var(--gold); line-height: 1; }
.hstat-l { color: var(--text3); font-size: 0.75rem; }

/* ─── HERO SLIDER 3D ─── */
.hero-slider-wrap {
  position: relative; flex-shrink: 0; width: 480px;
  animation: fadeUp 0.7s ease-out 0.2s both;
}
@media (max-width: 960px) { .hero-slider-wrap { display: none; } }

/* 3D scene container */
.hs-scene { width: 480px; position: relative; }

/* Tilt layer — receives mouse-tracking 3D rotation */
.hs-tilt {
  transition: transform 0.12s cubic-bezier(0.23, 1, 0.32, 1);
  will-change: transform;
}

/* The card face — clipped rounded rectangle, theme-aware */
.hs-card {
  width: 480px; height: 440px; position: relative;
  border-radius: 24px; overflow: hidden;
  background: #ffffff;
  border: 1px solid var(--border);
  box-shadow:
    0 22px 60px rgba(15,23,42,0.14),
    0 2px 10px rgba(15,23,42,0.06),
    0 0 44px rgba(109,84,232,0.08);
}
html.dark .hs-card {
  background: rgba(9, 13, 28, 0.96);
  border-color: rgba(124,106,245,0.22);
  box-shadow:
    0 0 0 1px rgba(124,106,245,0.07),
    0 28px 90px rgba(0,0,0,0.65),
    0 0 80px rgba(124,106,245,0.12),
    inset 0 1px 0 rgba(255,255,255,0.05);
}

/* Specular sheen overlay — tracks mouse for a light-reflection illusion */
.hs-sheen {
  position: absolute; inset: 0; border-radius: 24px;
  pointer-events: none; z-index: 20;
  transition: background 0.05s ease;
}

/* Animated top-edge glow that pulses */
.hs-edge-glow {
  position: absolute; top: 0; left: 20%; right: 20%; height: 1px;
  background: linear-gradient(90deg, transparent, rgba(124,106,245,0.6), rgba(6,182,212,0.4), transparent);
  pointer-events: none; z-index: 21;
  animation: edgeShimmer 3s ease-in-out infinite;
}
@keyframes edgeShimmer { 0%,100%{opacity:0.4;transform:scaleX(0.8)} 50%{opacity:1;transform:scaleX(1.2)} }

/* Slow scanning line — adds depth and life */
.hs-scan-line {
  position: absolute; top: 0; left: 0; right: 0; height: 2px;
  background: linear-gradient(90deg, transparent 0%, rgba(124,106,245,0.12) 50%, transparent 100%);
  pointer-events: none; z-index: 19;
  animation: scanDown 6s linear infinite;
}
@keyframes scanDown { 0%{top:0;opacity:0} 5%{opacity:1} 95%{opacity:1} 100%{top:100%;opacity:0} }

/* Slide container — positioned relative so entering/leaving slides stack */
.hs-slide {
  position: absolute; inset: 0;
  display: flex; flex-direction: column;
  padding: 15px 16px 17px; gap: 12px;
}

/* ── 3D Directional Slide Transitions ── */
.s3d-next-enter-active, .s3d-next-leave-active,
.s3d-prev-enter-active, .s3d-prev-leave-active {
  transition: transform 0.55s cubic-bezier(0.23, 1, 0.32, 1),
              opacity 0.4s cubic-bezier(0.23, 1, 0.32, 1);
  backface-visibility: hidden;
}

/* ▶ Next (forward): enter from right with depth twist, leave to left with depth twist */
.s3d-next-enter-from {
  transform: perspective(900px) translateX(55px) rotateY(22deg) scale(0.91);
  opacity: 0;
}
.s3d-next-leave-to {
  transform: perspective(900px) translateX(-55px) rotateY(-22deg) scale(0.91);
  opacity: 0;
}

/* ◀ Prev (backward): enter from left, leave to right */
.s3d-prev-enter-from {
  transform: perspective(900px) translateX(-55px) rotateY(-22deg) scale(0.91);
  opacity: 0;
}
.s3d-prev-leave-to {
  transform: perspective(900px) translateX(55px) rotateY(22deg) scale(0.91);
  opacity: 0;
}

/* ── Slide text transition ── */
.hero-text-block { position: relative; }
.slide-txt-enter-active { transition: opacity 0.4s ease, transform 0.4s ease; }
.slide-txt-leave-active { transition: opacity 0.2s ease; position: absolute; inset: 0; }
.slide-txt-enter-from { opacity: 0; transform: translateY(14px); }
.slide-txt-leave-to { opacity: 0; }

/* ════════════ HERO PRODUCT SCENES ════════════ */
/* Shared scene header */
.sx-head {
  display: flex; align-items: center; justify-content: space-between;
  flex-shrink: 0; position: relative; z-index: 3;
}
.sx-brand {
  display: inline-flex; align-items: center; gap: 0.4rem;
  font-family: 'Orbitron', sans-serif; font-weight: 700;
  font-size: 0.58rem; letter-spacing: 0.14em; text-transform: uppercase;
  color: var(--text3);
}
.sx-brand-dot {
  width: 8px; height: 8px; border-radius: 3px;
  background: linear-gradient(135deg, var(--accent), var(--accent2));
  box-shadow: 0 0 8px rgba(124,106,245,0.6);
}
.sx-status {
  display: inline-flex; align-items: center; gap: 0.35rem;
  padding: 0.22rem 0.6rem; border-radius: 99px; border: 1px solid;
  font-family: 'Orbitron', sans-serif; font-size: 0.54rem; font-weight: 700; letter-spacing: 0.1em;
}
.sx-status-dot { width: 5px; height: 5px; border-radius: 50%; background: currentColor; }
.sx-violet { color: var(--accent); border-color: color-mix(in srgb, var(--accent) 35%, transparent); background: color-mix(in srgb, var(--accent) 12%, transparent); }
.sx-cyan   { color: var(--cyan);   border-color: rgba(6,182,212,0.4);  background: rgba(6,182,212,0.12); }
.sx-green  { color: #10b981;       border-color: rgba(16,185,129,0.4); background: rgba(16,185,129,0.12); }
.sx-green .sx-status-dot { animation: sxBlink 1.3s ease-in-out infinite; }
@keyframes sxBlink { 0%,100%{opacity:1} 50%{opacity:0.25} }

.sx-body { flex: 1; min-height: 0; display: flex; flex-direction: column; position: relative; z-index: 2; }

/* ── Scene 0 · Living flashcard deck ── */
.fc-body { align-items: center; justify-content: center; gap: 16px; }
.fc-deck { position: relative; width: 300px; height: 196px; perspective: 1100px; animation: fcFloat 5s ease-in-out infinite; }
.fc-deck::before {
  content: ''; position: absolute; inset: -26px; z-index: -1; border-radius: 50%;
  background: radial-gradient(circle, color-mix(in srgb, var(--accent) 18%, transparent), transparent 70%);
  filter: blur(8px);
}
@keyframes fcFloat { 0%,100%{transform:translateY(0)} 50%{transform:translateY(-7px)} }
.fc-peek {
  position: absolute; inset: 0; border-radius: 18px;
  background: var(--t-surface); border: 1px solid var(--border);
  box-shadow: 0 8px 22px rgba(15,23,42,0.10);
}
.fc-peek-1 { transform: translate(11px, 13px) rotate(4.5deg); border-top: 3px solid var(--cyan); opacity: 0.7; }
.fc-peek-2 { transform: translate(21px, 24px) rotate(9deg); border-top: 3px solid var(--gold); opacity: 0.45; }
.fc-flip { position: absolute; inset: 0; transform-style: preserve-3d; animation: fcFlip 4.6s cubic-bezier(0.7,0,0.25,1) forwards; }
@keyframes fcFlip { 0%,32%{transform:rotateY(0)} 50%,100%{transform:rotateY(180deg)} }
.fc-face {
  position: absolute; inset: 0; border-radius: 18px; overflow: hidden;
  backface-visibility: hidden; -webkit-backface-visibility: hidden;
  padding: 15px 17px; display: flex; flex-direction: column;
  border: 1px solid var(--border); box-shadow: 0 12px 32px rgba(15,23,42,0.14);
}
.fc-front { background: linear-gradient(158deg, color-mix(in srgb, var(--accent) 7%, var(--t-surface)), var(--t-surface)); border-top: 3px solid var(--accent); }
.fc-back  { background: linear-gradient(158deg, color-mix(in srgb, var(--cyan) 9%, var(--t-surface)), var(--t-surface)); border-top: 3px solid var(--cyan); transform: rotateY(180deg); }
.fc-tag {
  align-self: flex-start; font-family: 'Orbitron', sans-serif; font-size: 0.5rem; font-weight: 700; letter-spacing: 0.08em;
  padding: 0.22rem 0.55rem; border-radius: 6px;
  background: color-mix(in srgb, var(--accent) 15%, transparent); color: var(--accent);
}
.fc-q-label { margin-top: 13px; font-size: 0.56rem; text-transform: uppercase; letter-spacing: 0.12em; color: var(--text3); }
.fc-q { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1.02rem; line-height: 1.32; color: var(--text1); margin-top: 4px; }
.fc-q b { color: var(--accent); }
.fc-hint { margin-top: auto; display: flex; align-items: center; gap: 0.4rem; font-size: 0.62rem; color: var(--text3); }
.fc-hint-key { display: inline-flex; align-items: center; justify-content: center; width: 18px; height: 18px; border: 1px solid var(--border); border-radius: 5px; font-size: 0.62rem; }
.fc-tutor { display: flex; align-items: center; gap: 0.5rem; }
.fc-tutor-av { width: 30px; height: 30px; border-radius: 9px; display: flex; align-items: center; justify-content: center; font-size: 16px; background: linear-gradient(135deg,#1e1a40,#4a35a6); border: 1px solid rgba(124,106,245,0.45); }
.fc-tutor-name { font-weight: 700; font-size: 0.8rem; color: var(--text1); }
.fc-tutor-role { font-size: 0.6rem; color: var(--text3); }
.fc-a-eq { margin-top: auto; font-family: 'Orbitron', sans-serif; font-size: 1.1rem; font-weight: 700; color: var(--text1); }
.fc-a-eq b { color: var(--cyan); }
.fc-a-row { margin-top: 10px; display: flex; align-items: center; gap: 0.5rem; }
.fc-correct { font-size: 0.6rem; font-weight: 700; color: #10b981; background: rgba(16,185,129,0.12); border: 1px solid rgba(16,185,129,0.3); padding: 0.2rem 0.5rem; border-radius: 99px; }
.fc-coin { font-size: 0.6rem; font-weight: 700; color: var(--gold); background: color-mix(in srgb, var(--gold) 15%, transparent); padding: 0.2rem 0.5rem; border-radius: 99px; }
.fc-pills { display: flex; flex-wrap: wrap; gap: 6px; justify-content: center; }
.fc-pill { font-size: 0.6rem; font-weight: 600; padding: 0.26rem 0.62rem; border-radius: 99px; color: var(--text3); background: var(--card); border: 1px solid var(--border); }
.fc-pill.active { color: #fff; background: linear-gradient(135deg, var(--accent), var(--accent2)); border-color: transparent; box-shadow: 0 4px 14px color-mix(in srgb, var(--accent) 40%, transparent); }

/* ── Scene 1 · AI grading a paper ── */
.gr-body { flex-direction: row; gap: 13px; align-items: stretch; }
.gr-sheet { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 6px; padding: 12px; border-radius: 14px; background: var(--card); border: 1px solid var(--border); }
.gr-sheet-title { font-family: 'Orbitron', sans-serif; font-size: 0.54rem; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text3); margin-bottom: 2px; }
.gr-row {
  display: flex; align-items: center; justify-content: space-between; gap: 0.5rem;
  padding: 0.42rem 0.6rem; border-radius: 9px; font-size: 0.7rem; color: var(--text2);
  background: var(--t-surface); border: 1px solid var(--border);
  opacity: 0; transform: translateY(6px); animation: grRow 0.45s ease forwards; animation-delay: var(--d);
}
@keyframes grRow { to { opacity: 1; transform: translateY(0); } }
.gr-q { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.gr-mark { display: inline-flex; align-items: center; gap: 0.3rem; font-weight: 700; color: #10b981; flex-shrink: 0; }
.gr-tick { display: inline-flex; align-items: center; justify-content: center; width: 15px; height: 15px; border-radius: 50%; background: rgba(16,185,129,0.16); font-size: 0.6rem; }
.gr-row-flag { border-color: color-mix(in srgb, var(--gold) 45%, transparent); background: color-mix(in srgb, var(--gold) 9%, var(--t-surface)); }
.gr-pencil { font-size: 0.62rem; font-weight: 700; color: var(--gold); flex-shrink: 0; }
.gr-note { margin-top: 4px; font-size: 0.6rem; color: #ef4444; border-left: 2px solid #ef4444; padding-left: 0.5rem; line-height: 1.4; opacity: 0; animation: grRow 0.5s ease forwards 2s; }
.gr-note b { color: #ef4444; }
.gr-score { width: 134px; flex-shrink: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 10px; border-radius: 14px; background: color-mix(in srgb, var(--accent) 8%, var(--t-surface)); border: 1px solid color-mix(in srgb, var(--accent) 22%, transparent); }
.gr-ring-wrap { position: relative; width: 84px; height: 84px; }
.gr-ring { width: 84px; height: 84px; transform: rotate(-90deg); }
.gr-ring-bg { fill: none; stroke: var(--border); stroke-width: 7; }
.gr-ring-fg { fill: none; stroke: var(--accent); stroke-width: 7; stroke-linecap: round; filter: drop-shadow(0 0 4px color-mix(in srgb, var(--accent) 60%, transparent)); }
.gr-score-num { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; font-family: 'Orbitron', sans-serif; font-weight: 800; font-size: 1.3rem; color: var(--text1); }
.gr-score-num span { font-size: 0.7rem; color: var(--text3); align-self: flex-start; margin-top: 8px; margin-left: 1px; }
.gr-score-lbl { margin-top: 8px; font-size: 0.56rem; text-transform: uppercase; letter-spacing: 0.1em; color: var(--text3); }
.gr-trend { margin-top: 6px; font-size: 0.6rem; font-weight: 700; color: #10b981; }

/* ── Scene 2 · Live tutor demo ── */
.lt-body { gap: 10px; }
.lt-tutor { display: flex; align-items: center; gap: 0.5rem; flex-shrink: 0; padding-bottom: 10px; border-bottom: 1px solid var(--border); }
.lt-av { width: 34px; height: 34px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 18px; background: linear-gradient(135deg,#991b1b,#7c3aed); border: 1px solid rgba(255,255,255,0.15); box-shadow: 0 0 14px rgba(153,27,27,0.35); }
.lt-name { font-weight: 700; font-size: 0.82rem; color: var(--text1); }
.lt-subj { font-size: 0.62rem; color: var(--text3); }
.lt-chat { flex: 1; min-height: 0; display: flex; flex-direction: column; gap: 8px; justify-content: center; }
.lt-msg { display: flex; align-items: flex-end; gap: 0.45rem; }
.lt-user { flex-direction: row-reverse; }
.lt-ava { width: 24px; height: 24px; border-radius: 50%; flex-shrink: 0; display: flex; align-items: center; justify-content: center; font-size: 13px; background: linear-gradient(135deg,#991b1b,#7c3aed); }
.lt-bub { max-width: 80%; padding: 0.5rem 0.8rem; border-radius: 14px; font-size: 0.78rem; line-height: 1.5; }
.lt-bub-user { background: linear-gradient(135deg, var(--accent), var(--accent2)); color: #fff; border-bottom-right-radius: 5px; opacity: 0; animation: ltIn 0.4s ease forwards 0.25s; }
.lt-bub-ai { background: var(--card); border: 1px solid var(--border); color: var(--text1); border-bottom-left-radius: 5px; }
.lt-ur { display: block; font-size: 0.95rem; line-height: 1.7; }
.lt-en { display: block; margin-top: 3px; font-size: 0.62rem; color: var(--text3); font-style: italic; }
@keyframes ltIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
.lt-typing-row { max-height: 44px; overflow: hidden; opacity: 0; animation: ltIn 0.3s ease forwards 0.9s, ltCollapse 0.4s ease forwards 2s; }
.lt-typing { display: inline-flex; gap: 4px; align-items: center; padding: 0.6rem 0.8rem; }
.lt-typing span { width: 6px; height: 6px; border-radius: 50%; background: var(--text3); animation: ltDot 1.2s infinite; }
.lt-typing span:nth-child(2) { animation-delay: 0.18s; }
.lt-typing span:nth-child(3) { animation-delay: 0.36s; }
@keyframes ltDot { 0%,60%,100%{transform:translateY(0);opacity:0.4} 30%{transform:translateY(-5px);opacity:1} }
@keyframes ltCollapse { to { opacity: 0; max-height: 0; margin-top: -8px; } }
.lt-reply { opacity: 0; animation: ltIn 0.45s ease forwards 2.1s; }
.lt-input { flex-shrink: 0; display: flex; align-items: center; gap: 0.5rem; padding: 0.45rem 0.55rem; border-radius: 12px; background: var(--card); border: 1px solid var(--border); }
.lt-lang { display: inline-flex; align-items: center; gap: 0.25rem; font-size: 0.58rem; font-weight: 700; color: var(--text3); padding: 0.22rem 0.45rem; border-radius: 7px; background: color-mix(in srgb, var(--accent) 12%, transparent); }
.lt-lang b { color: var(--accent); }
.lt-lang-sep { opacity: 0.4; }
.lt-input-ph { flex: 1; font-size: 0.7rem; color: var(--text3); }
.lt-mic { font-size: 0.85rem; opacity: 0.6; }
.lt-send { width: 26px; height: 26px; border-radius: 8px; display: inline-flex; align-items: center; justify-content: center; font-size: 0.7rem; color: #fff; background: linear-gradient(135deg, var(--accent), var(--accent2)); box-shadow: 0 4px 12px color-mix(in srgb, var(--accent) 40%, transparent); }

/* Respect reduced motion: reveal end states, drop looping/transition animations */
@media (prefers-reduced-motion: reduce) {
  .fc-deck { animation: none; }
  .fc-flip { animation: none; transform: rotateY(180deg); }
  .gr-row, .gr-note, .lt-bub-user, .lt-reply { animation: none; opacity: 1; transform: none; }
  .lt-typing-row { display: none; }
}

/* ── Slider controls ── */
.hs-arrow {
  position: absolute; top: 210px; transform: translateY(-50%);
  width: 36px; height: 36px; border-radius: 50%;
  border: 1px solid rgba(124,106,245,0.25);
  background: rgba(7,10,22,0.85);
  color: rgba(255,255,255,0.6);
  display: flex; align-items: center; justify-content: center;
  cursor: none; z-index: 30; transition: all 0.2s cubic-bezier(0.23,1,0.32,1);
  backdrop-filter: blur(16px);
  box-shadow: 0 4px 16px rgba(0,0,0,0.4), 0 0 0 1px rgba(255,255,255,0.04);
}
.hs-arrow:hover {
  border-color: rgba(124,106,245,0.6); color: var(--accent);
  background: rgba(124,106,245,0.15);
  box-shadow: 0 0 16px rgba(124,106,245,0.3), 0 4px 16px rgba(0,0,0,0.4);
  transform: translateY(-50%) scale(1.08);
}
.hs-arrow:active { transform: translateY(-50%) scale(0.96); }
.hs-prev { left: -18px; }
.hs-next { right: -18px; }

.hs-bottom-bar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 0.65rem 0.25rem 0;
}
.hs-dots { display: flex; gap: 6px; align-items: center; }
.hs-dot {
  width: 6px; height: 6px; border-radius: 99px; border: none;
  background: var(--text3); cursor: none; transition: all 0.35s; padding: 0;
}
.hs-dot.active { width: 28px; background: var(--accent); }
.hs-dot:not(.active):hover { background: var(--text2); }
.hs-lbl {
  font-size: 0.67rem; font-weight: 600; letter-spacing: 0.06em; text-transform: uppercase;
  color: var(--text3);
}
.fade-lbl-enter-active { transition: opacity 0.3s, transform 0.3s; }
.fade-lbl-leave-active { transition: opacity 0.2s; }
.fade-lbl-enter-from { opacity: 0; transform: translateY(5px); }
.fade-lbl-leave-to { opacity: 0; }
.scroll-hint { display: flex; flex-direction: column; align-items: center; gap: 0.6rem; margin-top: 3rem; color: var(--text3); font-size: 0.75rem; animation: fadeUp 1s ease-out 0.8s both; }
.scroll-mouse { width: 24px; height: 36px; border: 1.5px solid rgba(255,255,255,0.2); border-radius: 12px; display: flex; justify-content: center; padding-top: 6px; }
.scroll-wheel { width: 4px; height: 8px; border-radius: 2px; background: rgba(255,255,255,0.4); animation: scrollWh 1.5s infinite; }
@keyframes scrollWh { 0%{transform:translateY(0);opacity:1}100%{transform:translateY(12px);opacity:0} }

/* ─── TICKER ─── */
.ticker-row { overflow: hidden; padding: 0.7rem 0; border-top: 1px solid var(--border); border-bottom: 1px solid var(--border); background: rgba(255,255,255,0.015); position: relative; z-index: 1; }
.ticker-inner { display: flex; width: max-content; animation: tickMove 30s linear infinite; }
@keyframes tickMove { from{transform:translateX(0)}to{transform:translateX(-33.33%)} }
.tick-item { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0 2rem; font-size: 0.8rem; color: var(--text3); font-weight: 500; white-space: nowrap; }
.tick-icon { font-size: 0.95rem; }

/* ─── SECTIONS ─── */
.section-wrap { max-width: 680px; margin: 0 auto; text-align: center; margin-bottom: 4rem; }
.sec-tag {
  display: inline-block; padding: 0.3rem 0.9rem; border-radius: 99px;
  border: 1px solid rgba(251,191,36,0.25); background: rgba(251,191,36,0.06);
  color: var(--gold); font-size: 0.75rem; font-weight: 600; letter-spacing: 0.07em; text-transform: uppercase;
  margin-bottom: 1.25rem;
}
.sec-heading {
  font-family: 'Syne', sans-serif; font-weight: 800;
  font-size: clamp(2rem, 4.5vw, 3.2rem); line-height: 1.1; letter-spacing: -0.025em;
  color: var(--text1); margin: 0 0 1rem;
}
.sec-sub { color: var(--text2); font-size: 1rem; line-height: 1.7; margin: 0; }
.grad-text {
  background: linear-gradient(90deg, var(--gold), #f97316, var(--accent));
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
}

/* ─── AI SCENE ─── */
.ai-scene { padding: 100px 1.5rem; position: relative; z-index: 1; }
.scene-stage {
  position: relative; max-width: 1100px; margin: 0 auto; height: 520px;
  display: flex; align-items: center; justify-content: center;
}
/* AI Brain center */
.ai-brain { position: absolute; left: 50%; top: 50%; transform: translate(-50%,-50%); z-index: 10; opacity: 0; transition: opacity 0.8s 0.3s; }
.ai-brain.visible { opacity: 1; }
.brain-rings { position: absolute; inset: -60px; }
.brain-ring { position: absolute; border-radius: 50%; border: 1px solid; }
.br1 { inset: 20px; border-color: rgba(124,106,245,0.3); animation: spinRing 8s linear infinite; }
.br2 { inset: 5px; border-color: rgba(6,182,212,0.2); animation: spinRing 12s linear infinite reverse; }
.br3 { inset: -15px; border-color: rgba(251,191,36,0.15); animation: spinRing 16s linear infinite; }
@keyframes spinRing { from{transform:rotate(0)}to{transform:rotate(360deg)} }
.brain-core {
  width: 90px; height: 90px; border-radius: 50%;
  background: rgba(124,106,245,0.08); border: 1px solid rgba(124,106,245,0.3);
  box-shadow: 0 0 40px rgba(124,106,245,0.3); display: flex; flex-direction: column;
  align-items: center; justify-content: center; gap: 0.25rem;
}
.brain-label { font-family: 'Orbitron', sans-serif; font-size: 0.45rem; color: var(--accent); letter-spacing: 0.1em; }

/* Connection SVG */
.conn-svg { position: absolute; inset: 0; width: 100%; height: 100%; pointer-events: none; }
.conn-line { animation: connAnim 2s linear infinite; }
@keyframes connAnim { 0%{stroke-dashoffset:50}100%{stroke-dashoffset:0} }

/* Student cards */
.student-card {
  position: absolute; width: 200px;
  background: rgba(255,255,255,0.04); border: 1px solid var(--border);
  border-radius: 18px; padding: 1rem;
  backdrop-filter: blur(16px);
  box-shadow: 0 8px 32px rgba(0,0,0,0.5);
  opacity: 0; transform: translateY(20px); transition: opacity 0.6s, transform 0.6s;
}
.student-card.visible { opacity: 1; transform: translateY(0); }
.sc-1 { top: 5%; left: 3%; }
.sc-2 { bottom: 5%; left: 3%; }
.sc-3 { top: 5%; right: 3%; }
.sc-4 { bottom: 5%; right: 3%; }
@media (max-width: 768px) {
  .scene-stage { height: auto; flex-direction: column; gap: 1rem; }
  .student-card { position: static; width: 100%; }
  .ai-brain { position: static; transform: none; margin: 1rem auto; }
  .conn-svg { display: none; }
  .ai-badge { display: none; }
}
.sc-top { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.75rem; }
.sc-avatar { width: 32px; height: 32px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 16px; flex-shrink: 0; }
.sc-name { font-weight: 700; font-size: 0.82rem; color: var(--text1); }
.sc-subject { font-size: 0.68rem; color: var(--text3); }
.sc-score { margin-left: auto; font-family: 'Orbitron', sans-serif; font-weight: 700; font-size: 0.9rem; }
.sc-analysis { display: flex; flex-direction: column; gap: 0.4rem; }
.analysis-bar { display: flex; align-items: center; gap: 0.4rem; }
.ab-label { font-size: 0.62rem; color: var(--text3); width: 50px; flex-shrink: 0; }
.ab-track { flex: 1; height: 4px; background: rgba(255,255,255,0.08); border-radius: 2px; overflow: hidden; }
.ab-fill { height: 100%; border-radius: 2px; transition: width 1.2s ease-out; }
.ab-pct { font-size: 0.62rem; color: var(--text2); width: 24px; text-align: right; }
.sc-scan { position: absolute; inset: 0; border-radius: 18px; overflow: hidden; pointer-events: none; }
.sc-scan::after { content: ''; position: absolute; left: 0; right: 0; height: 2px; background: linear-gradient(90deg, transparent, rgba(6,182,212,0.5), transparent); animation: scanCard 3s linear infinite; }
@keyframes scanCard { from{top:-2px}to{top:100%} }

/* AI insight badges */
.ai-badge {
  position: absolute; display: flex; align-items: center; gap: 0.4rem;
  padding: 0.4rem 0.9rem; border-radius: 99px;
  background: rgba(7,12,26,0.95); border: 1px solid rgba(6,182,212,0.3);
  color: var(--cyan); font-size: 0.72rem; font-weight: 500;
  box-shadow: 0 4px 20px rgba(0,0,0,0.5); animation: pulseBadge 2s ease-in-out infinite;
  white-space: nowrap;
}
.ab-top { top: 8%; left: 50%; transform: translateX(-50%); }
.ab-mid { top: 45%; right: 1%; }
.ab-bot { bottom: 8%; left: 50%; transform: translateX(-50%); }
.aib-icon { font-size: 0.85rem; }
@keyframes pulseBadge { 0%,100%{box-shadow:0 4px 20px rgba(0,0,0,0.5),0 0 0 0 rgba(6,182,212,0)}50%{box-shadow:0 4px 20px rgba(0,0,0,0.5),0 0 0 6px rgba(6,182,212,0)} }

/* ─── FEATURES ─── */
.features-section { padding: 100px 1.5rem; position: relative; z-index: 1; }
.feat-grid {
  max-width: 1280px; margin: 0 auto;
  display: grid; grid-template-columns: repeat(auto-fill, minmax(260px,1fr)); gap: 1.25rem;
}
.feat-card {
  background: var(--card); border: 1px solid var(--border); border-radius: 20px; padding: 1.75rem;
  transition: all 0.35s; cursor: none; position: relative; overflow: hidden;
  opacity: 0; transform: translateY(20px);
}
.feat-card.visible { opacity: 1; transform: translateY(0); animation: cardIn 0.5s ease-out both; }
@keyframes cardIn { from{opacity:0;transform:translateY(20px)}to{opacity:1;transform:translateY(0)} }
.feat-card::before { content: ''; position: absolute; inset: 0; opacity: 0; transition: opacity 0.3s; border-radius: 20px; background: linear-gradient(135deg, rgba(124,106,245,0.06), transparent); }
.feat-card:hover { border-color: rgba(124,106,245,0.25); transform: translateY(-5px); box-shadow: 0 16px 48px rgba(0,0,0,0.4); }
.feat-card:hover::before { opacity: 1; }
.feat-icon-wrap { width: 52px; height: 52px; border-radius: 16px; display: flex; align-items: center; justify-content: center; margin-bottom: 1rem; border: 1px solid rgba(255,255,255,0.08); }
.feat-icon-inner { font-size: 22px; }
.feat-title { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1rem; color: var(--text1); margin: 0 0 0.5rem; }
.feat-desc { color: var(--text2); font-size: 0.86rem; line-height: 1.6; margin: 0; }
.feat-arrow { position: absolute; bottom: 1.5rem; right: 1.5rem; color: var(--text3); font-size: 1.1rem; transition: all 0.2s; }
.feat-card:hover .feat-arrow { color: var(--accent); transform: translate(3px, -3px); }

/* ─── TUTORS ─── */
.tutors-section { padding: 100px 1.5rem; position: relative; z-index: 1; }
.tutors-scroll { overflow: hidden; margin-bottom: 3rem; }
.tutors-track { display: flex; gap: 0.75rem; width: max-content; animation: tutorScroll 20s linear infinite; }
@keyframes tutorScroll { from{transform:translateX(0)}to{transform:translateX(-50%)} }
.tutor-chip {
  display: flex; align-items: center; gap: 0.65rem; padding: 0.6rem 1rem; border-radius: 12px;
  background: var(--card); border: 1px solid var(--border); flex-shrink: 0; white-space: nowrap;
}
.tc-avatar { width: 32px; height: 32px; border-radius: 9px; display: flex; align-items: center; justify-content: center; font-size: 16px; flex-shrink: 0; }
.tc-name { font-weight: 600; font-size: 0.82rem; color: var(--text1); }
.tc-subj { font-size: 0.7rem; color: var(--text3); }
.tutors-grid-full { max-width: 1280px; margin: 0 auto; display: grid; grid-template-columns: repeat(auto-fill, minmax(220px,1fr)); gap: 1.25rem; }
.tg-card { background: var(--card); border: 1px solid var(--border); border-radius: 20px; overflow: hidden; transition: all 0.3s; }
.tg-card:hover { border-color: rgba(124,106,245,0.25); transform: translateY(-4px); box-shadow: 0 12px 40px rgba(0,0,0,0.4); }
.tgc-head { height: 80px; display: flex; align-items: center; justify-content: center; }
.tgc-icon { font-size: 2.5rem; }
.tgc-body { padding: 1.25rem; }
.tgc-name { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1rem; color: var(--text1); }
.tgc-subj { color: var(--text3); font-size: 0.75rem; margin-bottom: 0.6rem; }
.tgc-quote { color: var(--text2); font-size: 0.78rem; font-style: italic; line-height: 1.5; margin-bottom: 1rem; border-left: 2px solid rgba(251,191,36,0.3); padding-left: 0.65rem; min-height: 2.5rem; }
.tgc-actions { display: flex; gap: 0.5rem; }
.tgc-btn { flex: 1; padding: 0.45rem; border-radius: 10px; font-size: 0.78rem; font-weight: 600; text-decoration: none; text-align: center; transition: all 0.2s; cursor: none; }
.tgc-btn.chat { background: rgba(124,106,245,0.15); color: #a78bfa; border: 1px solid rgba(124,106,245,0.2); }
.tgc-btn.chat:hover { background: rgba(124,106,245,0.25); }
.tgc-btn.video { background: rgba(6,182,212,0.1); color: #67e8f9; border: 1px solid rgba(6,182,212,0.2); }
.tgc-btn.video:hover { background: rgba(6,182,212,0.2); }

/* ─── HOW IT WORKS ─── */
.how-section { padding: 100px 1.5rem; position: relative; z-index: 1; }
.steps-row { max-width: 900px; margin: 0 auto; display: grid; grid-template-columns: repeat(auto-fit, minmax(240px,1fr)); gap: 2rem; }
.step-block { position: relative; display: flex; flex-direction: column; align-items: center; text-align: center; }
.step-num-glow { position: relative; margin-bottom: 1rem; }
.step-num { font-family: 'Orbitron', sans-serif; font-weight: 900; font-size: 3.5rem; color: rgba(251,191,36,0.1); line-height: 1; }
.step-icon-big { font-size: 2.5rem; margin-bottom: 1rem; }
.step-title { font-family: 'Syne', sans-serif; font-weight: 700; font-size: 1.1rem; color: var(--text1); margin: 0 0 0.5rem; }
.step-desc { color: var(--text2); font-size: 0.875rem; line-height: 1.65; margin: 0; }
.step-arrow { position: absolute; bottom: -2.5rem; left: 50%; transform: translateX(-50%); color: var(--accent); font-size: 1.5rem; opacity: 0.4; }

/* ─── PRICING ─── */
.pricing-section { padding: 100px 1.5rem; position: relative; z-index: 1; }
.pricing-wrap { max-width: 1100px; margin: 0 auto; display: grid; grid-template-columns: 1fr 1fr; gap: 5rem; align-items: center; }
@media (max-width: 768px) { .pricing-wrap { grid-template-columns: 1fr; gap: 3rem; } }
.pricing-feats { list-style: none; padding: 0; margin: 0 0 2.5rem; display: flex; flex-direction: column; gap: 0.75rem; }
.pricing-feats li { display: flex; align-items: center; gap: 0.75rem; color: var(--text2); font-size: 0.9rem; }
.pricing-btns { display: flex; gap: 1rem; flex-wrap: wrap; }
.btn-wa {
  display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.9rem 1.75rem;
  border-radius: 14px; background: #16a34a; color: white; font-weight: 600;
  font-size: 0.95rem; text-decoration: none; transition: all 0.2s; cursor: none;
}
.btn-wa:hover { background: #15803d; transform: translateY(-2px); }
.pricing-card-wrap { position: relative; }
.pricing-card-glow { position: absolute; inset: -30px; border-radius: 50%; background: radial-gradient(circle, rgba(124,106,245,0.15), transparent 70%); pointer-events: none; }
.pricing-card {
  background: rgba(124,106,245,0.06); border: 1px solid rgba(124,106,245,0.3);
  border-radius: 28px; padding: 2.5rem; text-align: center;
  box-shadow: 0 0 0 1px rgba(124,106,245,0.1);
  backdrop-filter: blur(12px);
}
.pc-badge { display: inline-block; padding: 0.3rem 1rem; border-radius: 99px; background: var(--accent); color: white; font-size: 0.78rem; font-weight: 600; margin-bottom: 1.5rem; }
.pc-amount { font-family: 'Orbitron', sans-serif; font-weight: 800; font-size: 3.5rem; color: var(--gold); letter-spacing: -0.04em; line-height: 1; }
.pc-period { color: var(--text3); font-size: 0.875rem; margin-top: 0.25rem; }
.pc-divider { height: 1px; background: var(--border); margin: 1.5rem 0; }
.pc-subjects { display: flex; flex-wrap: wrap; gap: 0.5rem; justify-content: center; margin-bottom: 1.5rem; }
.pc-sicon { width: 38px; height: 38px; border-radius: 10px; background: var(--card); border: 1px solid var(--border); display: flex; align-items: center; justify-content: center; font-size: 18px; }
.pc-note { color: var(--text3); font-size: 0.8rem; margin-bottom: 1.5rem; }
.pc-cta {
  display: block; padding: 0.9rem; border-radius: 14px;
  background: linear-gradient(135deg, var(--accent), var(--accent2));
  color: white; font-weight: 700; text-decoration: none; transition: all 0.2s;
  box-shadow: 0 0 24px rgba(124,106,245,0.4); cursor: none;
}
.pc-cta:hover { transform: translateY(-2px); box-shadow: 0 0 36px rgba(124,106,245,0.6); }

/* ─── TESTIMONIALS ─── */
.testi-section { padding: 100px 1.5rem; position: relative; z-index: 1; text-align: center; }
.testi-grid { max-width: 1100px; margin: 0 auto; display: grid; grid-template-columns: repeat(auto-fill,minmax(280px,1fr)); gap: 1.25rem; }
.testi-card { background: var(--card); border: 1px solid var(--border); border-radius: 20px; padding: 1.75rem; text-align: left; transition: all 0.3s; }
.testi-card:hover { border-color: rgba(124,106,245,0.25); transform: translateY(-3px); }
.testi-stars { color: var(--gold); font-size: 0.9rem; letter-spacing: 2px; margin-bottom: 0.75rem; }
.testi-text { color: var(--text2); font-size: 0.9rem; line-height: 1.65; margin: 0 0 1.25rem; }
.testi-author { display: flex; align-items: center; gap: 0.75rem; }
.testi-av { width: 38px; height: 38px; border-radius: 50%; background: linear-gradient(135deg, var(--accent), var(--accent2)); color: white; font-weight: 700; font-size: 0.875rem; display: flex; align-items: center; justify-content: center; }
.testi-name { font-weight: 600; font-size: 0.875rem; color: var(--text1); }
.testi-school { color: var(--text3); font-size: 0.75rem; }

/* ─── FINAL CTA ─── */
.final-cta {
  padding: 100px 1.5rem; text-align: center; position: relative; z-index: 1;
  background: linear-gradient(135deg, rgba(124,106,245,0.08), rgba(251,191,36,0.04));
  border-top: 1px solid var(--border); border-bottom: 1px solid var(--border);
}
.fca-inner { max-width: 640px; margin: 0 auto; position: relative; }
.fca-glow { position: absolute; inset: -80px; border-radius: 50%; background: radial-gradient(circle, rgba(124,106,245,0.1), transparent 70%); pointer-events: none; }
.fca-h2 { font-family: 'Syne', sans-serif; font-weight: 800; font-size: clamp(2rem,5vw,3.2rem); color: var(--text1); line-height: 1.1; letter-spacing: -0.025em; margin: 0 0 1rem; position: relative; }
.fca-p { color: var(--text2); font-size: 1rem; margin: 0 0 2.5rem; position: relative; }

/* ─── FOOTER ─── */
.land-footer { padding: 60px 1.5rem 32px; background: rgba(3,7,18,0.7); border-top: 1px solid var(--border); position: relative; z-index: 1; }
.footer-in { max-width: 1280px; margin: 0 auto; display: grid; grid-template-columns: 1fr 2fr; gap: 4rem; padding-bottom: 3rem; }
@media (max-width: 768px) { .footer-in { grid-template-columns: 1fr; gap: 2.5rem; } }
.footer-brand p { color: var(--text3); font-size: 0.875rem; line-height: 1.65; margin: 0.75rem 0 1rem; }
.footer-inst { display: inline-block; padding: 0.3rem 0.85rem; border-radius: 8px; border: 1px solid var(--border); color: var(--text3); font-size: 0.75rem; }
.footer-cols { display: grid; grid-template-columns: repeat(3,1fr); gap: 2rem; }
@media (max-width: 520px) { .footer-cols { grid-template-columns: 1fr 1fr; } }
.footer-col { display: flex; flex-direction: column; gap: 0.6rem; }
.footer-hd { font-weight: 600; font-size: 0.8rem; color: var(--text1); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.25rem; }
.footer-col a, .footer-col span { color: var(--text3); font-size: 0.875rem; text-decoration: none; transition: color 0.15s; cursor: none; }
.footer-col a:hover { color: var(--text2); }
.footer-bottom { max-width: 1280px; margin: 0 auto; display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; padding-top: 1.5rem; border-top: 1px solid var(--border); color: var(--text3); font-size: 0.78rem; }

/* ─── LIGHT MODE OVERRIDES ─── */
html:not(.dark) .land-footer { background: var(--bg2); border-top-color: var(--border); }
html:not(.dark) .ticker-row { background: rgba(0,0,0,0.02); }
html:not(.dark) .feat-card { background: var(--bg2); }
html:not(.dark) .feat-card:hover { box-shadow: 0 8px 32px rgba(109,84,232,0.1); }
html:not(.dark) .tg-card, html:not(.dark) .testi-card { background: var(--bg2); }
html:not(.dark) .pricing-card { background: rgba(109,84,232,0.05); border-color: rgba(109,84,232,0.2); }
html:not(.dark) .tutor-chip { background: var(--bg2); }
html:not(.dark) .sec-tag { border-color: rgba(217,119,6,0.2); background: rgba(217,119,6,0.05); }
html:not(.dark) .ai-badge { background: rgba(255,255,255,0.9); border-color: rgba(6,182,212,0.3); }
html:not(.dark) .student-card { background: rgba(255,255,255,0.85); }
</style>
