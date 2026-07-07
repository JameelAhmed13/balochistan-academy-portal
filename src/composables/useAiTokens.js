// Shared error-message formatting for the token-gated /api/ai/generate endpoint,
// used by the Dictionary, Join Forces, Learn Coding, and Simulations AI tools.
export function aiErrorMessage(e) {
  const outOfTokens = e?.response?.status === 402 && e.response.data?.outOfTokens
  if (outOfTokens) return "You're out of AI tokens. Visit Coins → Redeem to buy more."
  return e?.response?.data?.error || 'AI is temporarily unavailable. Please try again.'
}
