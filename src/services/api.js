// Single axios instance for the eStudy backend (/api). Attaches the JWT to every
// request and bounces to login on 401. The Ollama path (ollamaService.js) uses its
// own fetch and is intentionally NOT routed through here.
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE || '/api'

export const api = axios.create({ baseURL: API_BASE, timeout: 20000 })

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('bap_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

let onUnauthorized = null
export function setUnauthorizedHandler(fn) { onUnauthorized = fn }

api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401 && onUnauthorized) onUnauthorized()
    // surface the server's { error } message
    const msg = err.response?.data?.error || err.message || 'Request failed'
    return Promise.reject(Object.assign(new Error(msg), { status: err.response?.status, data: err.response?.data }))
  },
)

export default api
