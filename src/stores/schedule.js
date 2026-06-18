import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'

// Student study-schedule planner (spec §3–5, §9)
export const useScheduleStore = defineStore('schedule', () => {
  const plans = ref([])
  const loading = ref(false)

  async function fetchAll() {
    loading.value = true
    try { plans.value = (await api.get('/schedule')).data }
    finally { loading.value = false }
  }
  async function generate(payload) {
    const { data } = await api.post('/schedule/generate', payload)
    await fetchAll()
    return data
  }
  async function setDayStatus(dayId, status) {
    await api.put(`/schedule/day/${dayId}`, { status })
    await fetchAll()
  }
  async function moveDay(dayId, toDate) {
    await api.put(`/schedule/day/${dayId}/move`, { toDate })
    await fetchAll()
  }
  async function recompute(subjectId) {
    await api.post(`/schedule/${subjectId}/recompute`)
    await fetchAll()
  }
  async function remove(subjectId) {
    await api.delete(`/schedule/${subjectId}`)
    await fetchAll()
  }

  return { plans, loading, fetchAll, generate, setDayStatus, moveDay, recompute, remove }
})
