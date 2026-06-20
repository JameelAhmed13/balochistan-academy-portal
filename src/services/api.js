// Single axios instance for the Balochistan Academy Portal backend (/api). Attaches the JWT to every
// request, auto-refreshes on 401, and bounces to login only when refresh also fails.
import axios from 'axios'
import { useHttpLoaderStore } from '@/stores/httpLoader'

const API_BASE = import.meta.env.VITE_API_BASE || '/api'

export const api = axios.create({ baseURL: API_BASE, timeout: 20000 })

api.interceptors.request.use((config) => {
  useHttpLoaderStore().start()
  const token = localStorage.getItem('bap_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

let onUnauthorized = null
export function setUnauthorizedHandler(fn) { onUnauthorized = fn }

// Deduplicate concurrent 401 → refresh calls so only one refresh request flies
let _refreshPromise = null

api.interceptors.response.use(
  (res) => {
    useHttpLoaderStore().finish()
    return res
  },
  async (err) => {
    useHttpLoaderStore().finish()
    const original = err.config
    if (err.response?.status === 401 && !original._retry) {
      original._retry = true
      const refreshToken = localStorage.getItem('bap_refresh_token')
      if (refreshToken) {
        try {
          if (!_refreshPromise) {
            _refreshPromise = axios.post(`${API_BASE}/auth/refresh`, { refreshToken })
              .finally(() => { _refreshPromise = null })
          }
          const { data } = await _refreshPromise
          localStorage.setItem('bap_token', data.token)
          localStorage.setItem('bap_refresh_token', data.refreshToken)
          original.headers.Authorization = `Bearer ${data.token}`
          return api(original)
        } catch {
          // refresh failed — fall through to logout
        }
      }
      if (onUnauthorized) onUnauthorized()
    }
    const msg = err.response?.data?.error || err.message || 'Request failed'
    return Promise.reject(Object.assign(new Error(msg), { status: err.response?.status, data: err.response?.data }))
  },
)

export default api
