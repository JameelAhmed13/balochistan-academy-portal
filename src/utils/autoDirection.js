/**
 * Automatic text direction.
 *
 * Aligns every rendered text element to the language of its own content —
 * English / Latin → left (LTR), Urdu / Arabic → right (RTL) — with no manual
 * `dir`, `.urdu` class, or `lang` tagging anywhere in the app. New and updated
 * DOM is handled live via a MutationObserver, so it "just works" automatically.
 *
 * Form fields are handled separately by CSS (`unicode-bidi: plaintext`).
 */

// First strong directional character (the same heuristic the browser uses for dir="auto").
// RTL: Hebrew, Arabic, Syriac, Thaana, NKo, Arabic Extended-A, and Arabic/Hebrew presentation forms.
const STRONG_RTL = /[֑-߿ࡠ-ࣿיִ-﷽ﹰ-﻿]/
// LTR: Latin (+ extended), Greek, Cyrillic.
const STRONG_LTR = /[A-Za-zÀ-ʯͰ-ԯ]/

// Tags whose own text we align. Pure layout containers are intentionally excluded.
const TEXT_TAGS = new Set([
  'P', 'SPAN', 'DIV', 'LI', 'TD', 'TH', 'BUTTON', 'A', 'LABEL', 'SMALL',
  'STRONG', 'EM', 'B', 'I', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6', 'DD', 'DT',
  'BLOCKQUOTE', 'FIGCAPTION', 'SUMMARY', 'CAPTION',
])
const SELECTOR = Array.from(TEXT_TAGS).join(',')

function firstStrongDir(text) {
  for (let i = 0; i < text.length; i++) {
    const ch = text[i]
    if (STRONG_RTL.test(ch)) return 'rtl'
    if (STRONG_LTR.test(ch)) return 'ltr'
  }
  return null
}

// Only act on elements that directly hold meaningful text, so we don't flip a
// big container based on a single nested string.
function hasOwnText(el) {
  for (const node of el.childNodes) {
    if (node.nodeType === 3 && node.nodeValue.trim().length) return true
  }
  return false
}

function applyDir(el) {
  if (!el.isConnected || !TEXT_TAGS.has(el.tagName)) return
  if (el.isContentEditable) return // CSS plaintext handles editable fields
  if (!hasOwnText(el)) return

  const dir = firstStrongDir(el.textContent || '')
  if (dir === 'rtl') {
    el.setAttribute('dir', 'rtl')
    el.classList.add('urdu')
    // Flip horizontal alignment only when it was left/start — preserve centered/right text.
    const ta = getComputedStyle(el).textAlign
    if (ta === 'left' || ta === 'start') el.style.textAlign = 'right'
    el.dataset.autodir = 'rtl'
  } else if (dir === 'ltr' && el.dataset.autodir === 'rtl') {
    // Content switched back to English — undo a previous RTL pass.
    el.removeAttribute('dir')
    el.classList.remove('urdu')
    el.style.textAlign = ''
    el.dataset.autodir = 'ltr'
  } else if (dir === 'ltr') {
    el.dataset.autodir = 'ltr'
  }
}

const pending = new Set()
let queued = false
function flush() {
  queued = false
  pending.forEach(applyDir)
  pending.clear()
}
function schedule(el) {
  pending.add(el)
  if (!queued) {
    queued = true
    requestAnimationFrame(flush)
  }
}

function scan(root) {
  if (root.nodeType !== 1) return
  if (TEXT_TAGS.has(root.tagName)) schedule(root)
  root.querySelectorAll?.(SELECTOR).forEach(schedule)
}

let observer = null

export function initAutoDirection(target = document.body) {
  if (observer) return
  scan(target)
  observer = new MutationObserver((mutations) => {
    for (const m of mutations) {
      if (m.type === 'characterData') {
        if (m.target.parentElement) schedule(m.target.parentElement)
      } else {
        m.addedNodes.forEach(scan)
      }
    }
  })
  observer.observe(target, { childList: true, subtree: true, characterData: true })
}

export function stopAutoDirection() {
  observer?.disconnect()
  observer = null
}
