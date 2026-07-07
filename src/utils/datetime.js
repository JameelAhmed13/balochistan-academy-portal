// Shared date/time formatting — this is a Pakistan-only platform, so every display renders in
// Asia/Karachi (UTC+5) regardless of the viewer's device timezone/locale.
const TZ = 'Asia/Karachi'

export function formatDate(iso, opts = {}) {
  if (!iso) return '—'
  return new Date(iso).toLocaleDateString('en-PK', { timeZone: TZ, day: 'numeric', month: 'short', year: 'numeric', ...opts })
}

export function formatDateTime(iso, opts = {}) {
  if (!iso) return '—'
  return new Date(iso).toLocaleString('en-PK', { timeZone: TZ, dateStyle: 'medium', timeStyle: 'short', ...opts })
}

export function formatTime(iso, opts = {}) {
  if (!iso) return '—'
  return new Date(iso).toLocaleTimeString('en-PK', { timeZone: TZ, hour: '2-digit', minute: '2-digit', ...opts })
}

// Relative "x ago" — timezone-independent (pure epoch-ms diff); only needs the backend to
// serialize a properly UTC-marked timestamp (the "Z" suffix) to be correct.
export function timeAgo(iso) {
  if (!iso) return ''
  const diffSec = Math.max(0, (Date.now() - new Date(iso).getTime()) / 1000)
  if (diffSec < 60) return 'Just now'
  if (diffSec < 3600) return `${Math.floor(diffSec / 60)} min ago`
  if (diffSec < 86400) return `${Math.floor(diffSec / 3600)} hr ago`
  if (diffSec < 172800) return 'Yesterday'
  return formatDate(iso)
}
