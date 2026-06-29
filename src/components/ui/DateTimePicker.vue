<template>
  <div class="dtp-root" ref="rootRef">

    <!-- Trigger -->
    <button
      type="button"
      class="input dtp-trigger"
      :class="{ 'dtp-active': open }"
      @click="toggle"
      :disabled="disabled"
      :aria-label="activePlaceholder"
      :aria-expanded="open"
      aria-haspopup="dialog"
    >
      <span class="dtp-display">
        <span v-if="displayValue" class="dtp-val">{{ displayValue }}</span>
        <span v-else class="dtp-ph">{{ activePlaceholder }}</span>
      </span>
      <span class="dtp-suffix">
        <span v-if="modelValue" class="dtp-clear" role="button" aria-label="Clear" title="Clear" @click.stop="clear">×</span>
        <CalendarDays class="dtp-cal-icon" :size="15" />
      </span>
    </button>

    <!-- Floating popup -->
    <Teleport to="body">
      <Transition name="dtp-pop">
        <div
          v-if="open"
          ref="popupRef"
          class="dtp-popup"
          :style="popupPos"
          role="dialog"
          :aria-label="mode === 'datetime' ? 'Date and time picker' : 'Date picker'"
        >
          <!-- Month / year navigation -->
          <div class="dtp-nav">
            <button type="button" class="dtp-arr" @click="prevMonth" aria-label="Previous month">&#8249;</button>
            <button type="button" class="dtp-heading" @click="yearMode = !yearMode">
              {{ MONTHS[viewMonth] }} {{ viewYear }}
              <ChevronDown class="dtp-chev" :size="13" :class="{ 'dtp-chev--up': yearMode }" />
            </button>
            <button type="button" class="dtp-arr" @click="nextMonth" aria-label="Next month">&#8250;</button>
          </div>

          <!-- Year grid -->
          <div v-if="yearMode" class="dtp-year-grid">
            <button
              v-for="y in yearRange" :key="y" type="button" class="dtp-yr"
              :class="{ 'dtp-yr--on': y === viewYear }" @click="pickYear(y)"
            >{{ y }}</button>
          </div>

          <template v-else>
            <!-- Weekday row -->
            <div class="dtp-dow-row">
              <span v-for="d in DOW" :key="d" class="dtp-dow">{{ d }}</span>
            </div>

            <!-- Day grid -->
            <div class="dtp-day-grid" role="grid">
              <button
                v-for="(cell, i) in calCells" :key="i" type="button" class="dtp-cell"
                :class="{
                  'dtp-cell--out':      !cell.inMonth,
                  'dtp-cell--today':     cell.today,
                  'dtp-cell--sel':       cell.selected,
                  'dtp-cell--disabled':  cell.disabled,
                }"
                :disabled="cell.disabled"
                @click="pickDay(cell)"
                :aria-pressed="cell.selected"
                :aria-label="cell.d.toDateString()"
                role="gridcell"
              >{{ cell.d.getDate() }}</button>
            </div>

            <!-- Time strip (datetime only) -->
            <div v-if="mode === 'datetime'" class="dtp-time-strip" :class="{'dtp-time-strip--err': !!timeError}">
              <Clock :size="14" class="dtp-time-icon" />
              <div class="dtp-tg">
                <input type="number" class="dtp-ti" v-model.number="tHour"   min="0" max="23" @blur="clampH" @input="timeError = ''" aria-label="Hour" />
                <span class="dtp-tsep">:</span>
                <input type="number" class="dtp-ti" v-model.number="tMinute" min="0" max="59" @blur="clampM" @input="timeError = ''" aria-label="Minute" />
              </div>
              <button type="button" class="dtp-clk-btn" :class="{'dtp-clk-btn--on': showClock}"
                      @click="toggleClock" title="Pick from clock" aria-label="Toggle clock picker">
                <Clock :size="13" />
              </button>
              <button type="button" class="dtp-set" @click="commitDatetime">Set</button>
            </div>

            <!-- Time validation error -->
            <p v-if="timeError" class="dtp-time-err">{{ timeError }}</p>

            <!-- Analog clock face -->
            <div v-if="mode === 'datetime' && showClock" class="dtp-clock-wrap">
              <div class="dtp-clock-tabs">
                <button type="button" class="dtp-clock-tab" :class="{'dtp-clock-tab--on': clockMode === 'hour'}"   @click="clockMode = 'hour'">Hour</button>
                <button type="button" class="dtp-clock-tab" :class="{'dtp-clock-tab--on': clockMode === 'minute'}" @click="clockMode = 'minute'">Min</button>
                <span class="dtp-clock-preview">{{ String(tHour).padStart(2,'0') }}:{{ String(tMinute).padStart(2,'0') }}</span>
              </div>
              <svg class="dtp-clock-svg" viewBox="0 0 200 200" @pointerdown.prevent="onClockDrag">
                <circle cx="100" cy="100" r="88" class="dtp-ck-ring" />
                <line x1="100" y1="100" :x2="handPos.x" :y2="handPos.y" class="dtp-ck-hand" />
                <circle cx="100" cy="100" r="4" class="dtp-ck-pivot" />
                <circle :cx="handPos.x" :cy="handPos.y" r="13" class="dtp-ck-thumb" />
                <template v-if="clockMode === 'hour'">
                  <g v-for="item in hourNumbers" :key="'h'+item.h" class="dtp-ck-grp" @pointerdown.stop="pickClockHour(item.h)">
                    <circle :cx="item.x" :cy="item.y" r="14" class="dtp-ck-nbg" :class="{'dtp-ck-sel': item.selected}" />
                    <text   :x="item.x"  :y="item.y"  class="dtp-ck-num" :class="{'dtp-ck-num--sel': item.selected, 'dtp-ck-num--sm': item.inner}">{{ item.h }}</text>
                  </g>
                </template>
                <template v-else>
                  <g v-for="item in minuteNumbers" :key="'m'+item.m" class="dtp-ck-grp" @pointerdown.stop="pickClockMinute(item.m)">
                    <circle :cx="item.x" :cy="item.y" r="14" class="dtp-ck-nbg" :class="{'dtp-ck-sel': item.selected}" />
                    <text   :x="item.x"  :y="item.y"  class="dtp-ck-num" :class="{'dtp-ck-num--sel': item.selected}">{{ item.label }}</text>
                  </g>
                </template>
              </svg>
            </div>

            <!-- Footer -->
            <div class="dtp-foot">
              <button type="button" class="dtp-today" @click="jumpToday">Today</button>
            </div>
          </template>

        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue'
import { CalendarDays, Clock, ChevronDown } from '@lucide/vue'

const props = defineProps({
  modelValue:  { type: String,  default: '' },
  mode:        { type: String,  default: 'date' },
  placeholder: { type: String,  default: '' },
  disabled:    { type: Boolean, default: false },
  min:         { type: String,  default: '' },  // ISO lower-bound — no date/time before this
})
const emit = defineEmits(['update:modelValue'])

const MONTHS = ['January','February','March','April','May','June',
                'July','August','September','October','November','December']
const DOW    = ['Su','Mo','Tu','We','Th','Fr','Sa']

// ── Refs ──────────────────────────────────────────────────────────
const rootRef  = ref(null)
const popupRef = ref(null)
const open     = ref(false)
const yearMode = ref(false)
const popupPos = ref({})
const timeError = ref('')

// ── View / time state ─────────────────────────────────────────────
const viewYear  = ref(new Date().getFullYear())
const viewMonth = ref(new Date().getMonth())
const tHour     = ref(12)
const tMinute   = ref(0)
const selDate   = ref(null)

// ── Clock state ───────────────────────────────────────────────────
const showClock = ref(false)
const clockMode = ref('hour')
const OUTER_R   = 72
const INNER_R   = 50

// ── Min bound ─────────────────────────────────────────────────────
const minDate = computed(() => {
  if (!props.min) return null
  const d = new Date(props.min)
  return isNaN(d.getTime()) ? null : d
})

// ── Sync from modelValue ──────────────────────────────────────────
function syncFromModel(v) {
  if (!v) { selDate.value = null; return }
  const d = new Date(v)
  if (isNaN(d.getTime())) { selDate.value = null; return }
  selDate.value   = d
  viewYear.value  = d.getFullYear()
  viewMonth.value = d.getMonth()
  tHour.value     = d.getHours()
  tMinute.value   = d.getMinutes()
}
watch(() => props.modelValue, syncFromModel, { immediate: true })

// ── Display ───────────────────────────────────────────────────────
const activePlaceholder = computed(() =>
  props.placeholder || (props.mode === 'datetime' ? 'Select date & time' : 'Select a date')
)
const displayValue = computed(() => {
  if (!selDate.value) return ''
  const d  = selDate.value
  const ds = d.toLocaleDateString('en-PK', { day: 'numeric', month: 'short', year: 'numeric' })
  if (props.mode !== 'datetime') return ds
  const h = String(d.getHours()).padStart(2, '0')
  const m = String(d.getMinutes()).padStart(2, '0')
  return `${ds}  ·  ${h}:${m}`
})

// ── Calendar cells ────────────────────────────────────────────────
const calCells = computed(() => {
  const now   = new Date(); now.setHours(0, 0, 0, 0)
  const first = new Date(viewYear.value, viewMonth.value, 1)
  const last  = new Date(viewYear.value, viewMonth.value + 1, 0)
  const pad   = first.getDay()
  const cells = []

  // midnight of the min date — anything strictly before this is disabled
  const minDay = minDate.value ? (() => { const d = new Date(minDate.value); d.setHours(0,0,0,0); return d })() : null

  const mk = (d, inMonth) => {
    const today    = d.getTime() === now.getTime()
    const sel      = selDate.value
      ? d.getFullYear() === selDate.value.getFullYear() &&
        d.getMonth()    === selDate.value.getMonth()    &&
        d.getDate()     === selDate.value.getDate()
      : false
    const disabled = minDay ? d < minDay : false
    return { d, inMonth, today, selected: sel, disabled }
  }

  for (let i = 0; i < pad; i++)
    cells.push(mk(new Date(viewYear.value, viewMonth.value, 1 - pad + i), false))
  for (let i = 1; i <= last.getDate(); i++)
    cells.push(mk(new Date(viewYear.value, viewMonth.value, i), true))
  let nxt = 1
  while (cells.length % 7 !== 0 || cells.length < 35)
    cells.push(mk(new Date(viewYear.value, viewMonth.value + 1, nxt++), false))
  return cells
})

// ── Year range ────────────────────────────────────────────────────
const yearRange = computed(() => {
  const base = new Date().getFullYear()
  const r = []
  for (let y = base - 10; y <= base + 15; y++) r.push(y)
  return r
})

// ── Clock geometry ────────────────────────────────────────────────
function circlePos(index, r) {
  const angle = (index / 12) * Math.PI * 2 - Math.PI / 2
  return { x: +(100 + r * Math.cos(angle)).toFixed(2), y: +(100 + r * Math.sin(angle)).toFixed(2) }
}

const hourNumbers = computed(() => {
  const items = []
  for (let i = 0; i < 12; i++) {
    const h = i === 0 ? 12 : i
    items.push({ h, ...circlePos(i, OUTER_R), inner: false, selected: tHour.value === h })
  }
  for (let i = 0; i < 12; i++) {
    const h = i === 0 ? 0 : i + 12
    items.push({ h, ...circlePos(i, INNER_R), inner: true, selected: tHour.value === h })
  }
  return items
})

const minuteNumbers = computed(() => {
  const nearest = Math.round(tMinute.value / 5) * 5 % 60
  return Array.from({ length: 12 }, (_, i) => {
    const m = i * 5
    return { m, label: String(m).padStart(2, '0'), ...circlePos(i, OUTER_R), selected: nearest === m }
  })
})

const handPos = computed(() => {
  if (clockMode.value === 'hour') {
    const h     = tHour.value
    const index = h === 0 ? 0 : h <= 12 ? h % 12 : h - 12
    const r     = (h === 0 || h > 12) ? INNER_R : OUTER_R
    return circlePos(index, r)
  }
  const angle = (tMinute.value / 60) * Math.PI * 2 - Math.PI / 2
  return { x: +(100 + OUTER_R * Math.cos(angle)).toFixed(2), y: +(100 + OUTER_R * Math.sin(angle)).toFixed(2) }
})

function pickClockHour(h) {
  tHour.value = h; clockMode.value = 'minute'; timeError.value = ''
}
function pickClockMinute(m) {
  tMinute.value = m; timeError.value = ''
}

function onClockDrag(e) {
  const svg = e.currentTarget
  const pt  = svg.createSVGPoint()
  pt.x = e.clientX; pt.y = e.clientY
  const { x, y } = pt.matrixTransform(svg.getScreenCTM().inverse())
  const dx = x - 100, dy = y - 100
  const dist = Math.sqrt(dx * dx + dy * dy)
  if (dist < 15) return
  const angle = Math.atan2(dy, dx) + Math.PI / 2
  const norm  = ((angle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2)
  if (clockMode.value === 'hour') {
    const h12 = Math.round(norm / (Math.PI * 2) * 12) % 12
    tHour.value = dist > 60 ? (h12 === 0 ? 12 : h12) : (h12 === 0 ? 0 : h12 + 12)
    clockMode.value = 'minute'
  } else {
    tMinute.value = Math.round(norm / (Math.PI * 2) * 60) % 60
  }
  timeError.value = ''
}

// ── Navigation ────────────────────────────────────────────────────
function prevMonth() {
  if (viewMonth.value === 0) { viewMonth.value = 11; viewYear.value-- }
  else viewMonth.value--
}
function nextMonth() {
  if (viewMonth.value === 11) { viewMonth.value = 0; viewYear.value++ }
  else viewMonth.value++
}
function pickYear(y) { viewYear.value = y; yearMode.value = false }

// ── Day pick ──────────────────────────────────────────────────────
function pickDay(cell) {
  if (!cell.inMonth || cell.disabled) return
  selDate.value = new Date(cell.d.getFullYear(), cell.d.getMonth(), cell.d.getDate(), tHour.value, tMinute.value)
  if (props.mode === 'date') { emitISO(); close() }
}

// ── Commit datetime ───────────────────────────────────────────────
function commitDatetime() {
  if (!selDate.value) return
  const d = new Date(selDate.value.getFullYear(), selDate.value.getMonth(), selDate.value.getDate(), tHour.value, tMinute.value)
  if (minDate.value && d <= minDate.value) {
    const fmt = minDate.value.toLocaleString('en-PK', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit', hour12: false })
    timeError.value = `Must be after ${fmt}`
    return
  }
  timeError.value = ''
  selDate.value = d
  emitISO(); close()
}

// ── Emit ──────────────────────────────────────────────────────────
function emitISO() {
  const d = selDate.value; if (!d) return
  const y  = d.getFullYear()
  const mo = String(d.getMonth() + 1).padStart(2, '0')
  const dt = String(d.getDate()).padStart(2, '0')
  if (props.mode === 'date') {
    emit('update:modelValue', `${y}-${mo}-${dt}`)
  } else {
    const h = String(d.getHours()).padStart(2, '0')
    const m = String(d.getMinutes()).padStart(2, '0')
    emit('update:modelValue', `${y}-${mo}-${dt}T${h}:${m}`)
  }
}

// ── Time clamp ────────────────────────────────────────────────────
function clampH() { tHour.value   = Math.max(0, Math.min(23, +tHour.value   || 0)) }
function clampM() { tMinute.value = Math.max(0, Math.min(59, +tMinute.value || 0)) }

// ── Today ─────────────────────────────────────────────────────────
function jumpToday() {
  const d = new Date()
  viewYear.value = d.getFullYear(); viewMonth.value = d.getMonth()
  selDate.value  = new Date(d.getFullYear(), d.getMonth(), d.getDate(), tHour.value, tMinute.value)
  if (props.mode === 'date') { emitISO(); close() }
}

// ── Clear ─────────────────────────────────────────────────────────
function clear() { selDate.value = null; timeError.value = ''; emit('update:modelValue', '') }

// ── Positioning ───────────────────────────────────────────────────
async function calcPopupPos() {
  await new Promise(r => requestAnimationFrame(r))
  if (!rootRef.value || !popupRef.value) return
  const rect = rootRef.value.getBoundingClientRect()
  const ph   = popupRef.value.offsetHeight || (props.mode === 'datetime' ? 390 : 330)
  const pw   = 280
  let top  = rect.bottom + 8
  let left = rect.left
  if (left + pw > window.innerWidth  - 16) left = rect.right - pw
  if (left < 16) left = 16
  if (top  + ph > window.innerHeight - 16) top  = rect.top - ph - 8
  if (top  < 16) top  = 16
  popupPos.value = { top: `${top}px`, left: `${left}px` }
}

// ── Open / close ──────────────────────────────────────────────────
function toggle() { if (props.disabled) return; open.value ? close() : openPicker() }

async function openPicker() {
  open.value = true; yearMode.value = false; showClock.value = false; timeError.value = ''
  await nextTick()
  await calcPopupPos()
}

async function toggleClock() {
  showClock.value = !showClock.value
  if (showClock.value) { clockMode.value = 'hour' }
  await nextTick()
  await calcPopupPos()
}

function close() { open.value = false; timeError.value = '' }

function onMousedown(e) {
  if (!open.value) return
  if (rootRef.value?.contains(e.target) || popupRef.value?.contains(e.target)) return
  close()
}
function onAncestorScroll() { if (open.value) close() }

onMounted(() => {
  document.addEventListener('mousedown', onMousedown)
  document.addEventListener('scroll', onAncestorScroll, true)
})
onUnmounted(() => {
  document.removeEventListener('mousedown', onMousedown)
  document.removeEventListener('scroll', onAncestorScroll, true)
})
</script>

<style>
/* ── DateTimePicker — non-scoped (Teleport-compatible) ──────────── */

.dtp-root { position: relative; display: block; }

.dtp-trigger {
  display: flex !important; align-items: center; gap: 0.5rem;
  cursor: pointer; text-align: left; user-select: none;
}
.dtp-trigger:hover {
  border-color: color-mix(in srgb, var(--t-accent) 50%, var(--t-input-border)) !important;
}
.dtp-trigger.dtp-active {
  border-color: var(--t-accent) !important;
  box-shadow: 0 0 0 3px var(--t-input-focus) !important;
}
.dtp-display  { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dtp-val      { color: var(--t-text1); font-size: 0.875rem; }
.dtp-ph       { color: var(--t-text3); font-size: 0.875rem; }
.dtp-suffix   { display: flex; align-items: center; gap: 0.3rem; flex-shrink: 0; }
.dtp-cal-icon { color: var(--t-text3); pointer-events: none; }
.dtp-clear {
  width: 18px; height: 18px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 13px; line-height: 1; color: var(--t-text3);
  background: var(--t-hover); cursor: pointer; transition: background 0.1s, color 0.1s;
}
.dtp-clear:hover { background: var(--t-hover2); color: var(--t-text1); }

/* ── Popup ──────────────────────────────────────────────────────── */
.dtp-popup {
  position: fixed; z-index: 10000;
  background: var(--t-bg2); border: 1px solid var(--t-border);
  border-radius: 18px; box-shadow: var(--t-shadow-md);
  width: 280px; overflow: hidden; padding-bottom: 0.4rem;
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
}

.dtp-nav { display: flex; align-items: center; padding: 0.75rem 0.6rem 0.35rem; gap: 0.2rem; }
.dtp-arr {
  width: 32px; height: 32px; border-radius: 10px; border: none; cursor: pointer;
  background: none; color: var(--t-text2); font-size: 1.5rem; line-height: 1;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
  transition: background 0.1s, color 0.1s;
}
.dtp-arr:hover { background: var(--t-hover2); color: var(--t-text1); }
.dtp-heading {
  flex: 1; border: none; background: none; cursor: pointer;
  font-weight: 700; font-size: 0.88rem; color: var(--t-text1); font-family: inherit;
  display: flex; align-items: center; justify-content: center; gap: 0.25rem;
  border-radius: 8px; padding: 0.3rem 0.4rem; transition: background 0.1s;
}
.dtp-heading:hover { background: var(--t-hover); }
.dtp-chev     { color: var(--t-text3); transition: transform 0.18s; }
.dtp-chev--up { transform: rotate(180deg); }

.dtp-dow-row { display: grid; grid-template-columns: repeat(7, 1fr); padding: 0 0.5rem; margin-bottom: 0.15rem; }
.dtp-dow {
  text-align: center; font-size: 0.62rem; font-weight: 800;
  text-transform: uppercase; letter-spacing: 0.04em; color: var(--t-text3); padding: 0.18rem 0;
}

.dtp-day-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 1px; padding: 0 0.5rem; }
.dtp-cell {
  aspect-ratio: 1; border: none; border-radius: 8px; cursor: pointer;
  background: none; color: var(--t-text1); font-size: 0.8rem;
  display: flex; align-items: center; justify-content: center;
  font-family: inherit; font-variant-numeric: tabular-nums;
  transition: background 0.1s, color 0.1s; position: relative;
}
.dtp-cell:hover:not(.dtp-cell--sel):not(.dtp-cell--disabled) { background: var(--t-hover2); }
.dtp-cell--out      { color: var(--t-text3); }
.dtp-cell--out:hover { background: var(--t-hover); color: var(--t-text2); }
.dtp-cell--today    { color: var(--t-accent); font-weight: 700; }
.dtp-cell--today::after {
  content: ''; position: absolute; bottom: 3px; left: 50%; transform: translateX(-50%);
  width: 4px; height: 4px; border-radius: 50%; background: var(--t-accent);
}
.dtp-cell--sel {
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)) !important;
  color: #fff !important; font-weight: 600;
  box-shadow: 0 2px 10px color-mix(in srgb, var(--t-accent) 45%, transparent);
}
.dtp-cell--sel.dtp-cell--today::after { background: rgba(255,255,255,0.8); }
.dtp-cell--disabled {
  opacity: 0.28;
  cursor: not-allowed !important;
  pointer-events: none;
}

.dtp-year-grid {
  display: grid; grid-template-columns: repeat(4, 1fr);
  gap: 3px; padding: 0.5rem; max-height: 220px; overflow-y: auto; scrollbar-width: thin;
}
.dtp-yr {
  border: none; border-radius: 8px; background: none; color: var(--t-text2);
  font-size: 0.82rem; padding: 0.45rem; cursor: pointer;
  font-family: inherit; transition: all 0.1s; text-align: center; font-variant-numeric: tabular-nums;
}
.dtp-yr:hover { background: var(--t-hover2); color: var(--t-text1); }
.dtp-yr--on   { background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); color: #fff; font-weight: 600; }

/* ── Time strip ─────────────────────────────────────────────────── */
.dtp-time-strip {
  display: flex; align-items: center; gap: 0.4rem;
  margin: 0.55rem 0.6rem 0; padding: 0.45rem 0.65rem;
  border-radius: 12px; background: var(--t-hover); border: 1px solid var(--t-border);
  transition: border-color 0.15s;
}
.dtp-time-strip--err { border-color: #f87171 !important; }
.dtp-time-icon { color: var(--t-text3); flex-shrink: 0; }
.dtp-tg { display: flex; align-items: center; gap: 0.2rem; flex: 1; }
.dtp-ti {
  width: 40px; text-align: center; font-size: 0.9rem; font-weight: 700;
  background: var(--t-input-bg); border: 1px solid var(--t-input-border);
  border-radius: 8px; padding: 0.28rem 0.15rem; color: var(--t-text1);
  font-family: inherit; font-variant-numeric: tabular-nums; -moz-appearance: textfield;
}
.dtp-ti::-webkit-inner-spin-button,
.dtp-ti::-webkit-outer-spin-button { -webkit-appearance: none; }
.dtp-ti:focus { outline: none; border-color: var(--t-accent); box-shadow: 0 0 0 2px var(--t-input-focus); }
.dtp-tsep { font-weight: 800; font-size: 1rem; color: var(--t-text2); }

.dtp-clk-btn {
  width: 28px; height: 28px; border-radius: 8px; border: none; cursor: pointer; flex-shrink: 0;
  background: var(--t-bg2); color: var(--t-text3);
  display: flex; align-items: center; justify-content: center;
  transition: background 0.12s, color 0.12s;
}
.dtp-clk-btn:hover     { background: var(--t-hover2); color: var(--t-accent); }
.dtp-clk-btn--on       { background: var(--t-accent); color: #fff; }
.dtp-clk-btn--on:hover { opacity: 0.88; }

.dtp-set {
  flex-shrink: 0; padding: 0.28rem 0.75rem; border-radius: 8px; border: none; cursor: pointer;
  background: linear-gradient(135deg, var(--t-accent), var(--t-accent2));
  color: #fff; font-size: 0.75rem; font-weight: 700; font-family: inherit;
  box-shadow: 0 2px 8px color-mix(in srgb, var(--t-accent) 35%, transparent);
  transition: opacity 0.1s, transform 0.1s;
}
.dtp-set:hover  { opacity: 0.87; transform: translateY(-1px); }
.dtp-set:active { transform: scale(0.95); }

/* Time validation error */
.dtp-time-err {
  margin: 0.25rem 0.7rem 0;
  font-size: 0.7rem; font-weight: 600;
  color: #ef4444;
  display: flex; align-items: center; gap: 0.3rem;
}
.dtp-time-err::before { content: '⚠'; font-size: 0.65rem; }

/* ── Analog clock ───────────────────────────────────────────────── */
.dtp-clock-wrap  { margin: 0.4rem 0.6rem 0.1rem; }
.dtp-clock-tabs  { display: flex; align-items: center; gap: 0.3rem; margin-bottom: 0.45rem; }
.dtp-clock-tab {
  border: none; border-radius: 8px; cursor: pointer;
  padding: 0.22rem 0.75rem; font-size: 0.72rem; font-weight: 700;
  font-family: inherit; letter-spacing: 0.03em;
  background: var(--t-hover); color: var(--t-text3); transition: all 0.12s;
}
.dtp-clock-tab:hover     { background: var(--t-hover2); color: var(--t-text1); }
.dtp-clock-tab--on       { background: linear-gradient(135deg, var(--t-accent), var(--t-accent2)); color: #fff; }
.dtp-clock-preview {
  margin-left: auto; font-size: 0.95rem; font-weight: 800; letter-spacing: 0.06em;
  color: var(--t-text1); font-variant-numeric: tabular-nums; font-family: inherit;
}
.dtp-clock-svg  { display: block; width: 100%; touch-action: none; cursor: crosshair; }

.dtp-ck-ring  { fill: var(--t-hover); stroke: var(--t-border); stroke-width: 1.5; }
.dtp-ck-hand  { stroke: var(--t-accent); stroke-width: 2.5; stroke-linecap: round; }
.dtp-ck-pivot { fill: var(--t-accent); }
.dtp-ck-thumb {
  fill: var(--t-accent);
  filter: drop-shadow(0 1px 4px color-mix(in srgb, var(--t-accent) 55%, transparent));
}
.dtp-ck-grp   { cursor: pointer; }
.dtp-ck-nbg   { fill: transparent; transition: fill 0.1s; }
.dtp-ck-grp:hover .dtp-ck-nbg { fill: var(--t-hover2); }
.dtp-ck-sel   { fill: var(--t-accent) !important; }
.dtp-ck-num {
  dominant-baseline: central; text-anchor: middle;
  font-size: 12px; font-weight: 600; fill: var(--t-text2); pointer-events: none;
  font-family: 'Plus Jakarta Sans', system-ui, sans-serif;
}
.dtp-ck-num--sel { fill: #fff; font-weight: 700; }
.dtp-ck-num--sm  { font-size: 10px; font-weight: 500; }

/* ── Footer ─────────────────────────────────────────────────────── */
.dtp-foot { padding: 0.3rem 0.7rem; display: flex; justify-content: flex-start; }
.dtp-today {
  border: none; background: none; font-size: 0.74rem; font-weight: 600;
  color: var(--t-accent); cursor: pointer; padding: 0.2rem 0.4rem;
  border-radius: 6px; font-family: inherit; transition: background 0.1s;
}
.dtp-today:hover { background: var(--t-acc-alpha-sm); }

/* ── Popup animation ─────────────────────────────────────────────── */
.dtp-pop-enter-active { transition: opacity 0.15s ease, transform 0.15s cubic-bezier(0.22, 1, 0.36, 1); }
.dtp-pop-leave-active { transition: opacity 0.1s ease, transform 0.1s ease; }
.dtp-pop-enter-from   { opacity: 0; transform: translateY(-6px) scale(0.97); }
.dtp-pop-leave-to     { opacity: 0; transform: translateY(-4px) scale(0.98); }
@media (prefers-reduced-motion: reduce) {
  .dtp-pop-enter-active, .dtp-pop-leave-active { transition: none; }
}
</style>
