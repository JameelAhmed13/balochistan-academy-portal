import { defineStore, acceptHMRUpdate } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

// Central curriculum store backed by the API: grades (ECD–12), subjects per grade,
// AI tutors, and syllabus trees. Replaces the hardcoded SUBJECTS/AI_TUTORS/GRADE_SUBJECTS
// constants for the dynamic admin/student flows.
export const useCatalogStore = defineStore('catalog', () => {
  const grades = ref([])
  const bands = ref([])
  const mediums = ref([])
  const subjects = ref([])        // all subjects (admin); per-grade via subjectsForGrade
  const tutors = ref([])
  const gradeSubjects = ref({})   // gradeCode -> [subject]
  const syllabus = ref({})        // `${grade}:${subjectId}` -> units[]
  const loaded = ref(false)
  const loading = ref(false)
  const error = ref('')

  // normalize a subject row to carry both API (name_ur) and legacy (nameUr) keys
  const decorate = (s) => ({ ...s, nameUr: s.name_ur ?? s.nameUr })

  const enabledGrades = computed(() => grades.value.filter((g) => g.enabled !== 0))
  const gradeByCode = (code) => grades.value.find((g) => String(g.code) === String(code)) || null
  const subjectsForGrade = (code) => gradeSubjects.value[code] || []
  const findSubject = (id) => subjects.value.find((s) => String(s.id) === String(id)) || null
  const tutorsForGrade = (code) => {
    const subj = new Set(subjectsForGrade(code).map((s) => s.id))
    return tutors.value.filter((t) => t.subject_id == null || subj.has(t.subject_id))
  }

  // ── reads ──────────────────────────────────────────────────────────
  async function fetchBands() { bands.value = (await api.get('/bands')).data }
  async function fetchMediums() { mediums.value = (await api.get('/mediums')).data }
  async function fetchGrades() { grades.value = (await api.get('/grades')).data }
  async function fetchTutors() { tutors.value = (await api.get('/tutors')).data }
  async function fetchSubjectsForGrade(code) {
    const data = (await api.get(`/grades/${code}/subjects`)).data.map(decorate)
    gradeSubjects.value = { ...gradeSubjects.value, [code]: data }
    return data
  }
  async function fetchSyllabus(code, subjectId) {
    const key = `${code}:${subjectId}`
    const data = (await api.get(`/grades/${code}/subjects/${subjectId}/syllabus`)).data
    syllabus.value = { ...syllabus.value, [key]: data }
    return data
  }
  function syllabusFor(code, subjectId) { return syllabus.value[`${code}:${subjectId}`] || [] }

  async function bootstrap() {
    if (loaded.value || loading.value) return
    loading.value = true
    error.value = ''
    try {
      const [g, t] = await Promise.all([api.get('/grades'), api.get('/tutors')])
      grades.value = g.data
      tutors.value = t.data
      loaded.value = true
    } catch (e) {
      error.value = e.message || 'Failed to load catalog'
    } finally {
      loading.value = false
    }
  }

  // ── admin: bands ────────────────────────────────────────────────────
  async function createBand(payload) { await api.post('/admin/bands', payload); await fetchBands() }
  async function updateBand(id, payload) { await api.put(`/admin/bands/${id}`, payload); await fetchBands() }
  async function deleteBand(id) { await api.delete(`/admin/bands/${id}`); await fetchBands() }

  async function createMedium(payload) { await api.post('/admin/mediums', payload); await fetchMediums() }
  async function updateMedium(id, payload) { await api.put(`/admin/mediums/${id}`, payload); await fetchMediums() }
  async function deleteMedium(id) { await api.delete(`/admin/mediums/${id}`); await fetchMediums() }

  // ── admin: subjects (all) + writes ──────────────────────────────────
  async function fetchAllSubjects() { subjects.value = (await api.get('/admin/subjects')).data.map(decorate) }

  async function createGrade(payload) { await api.post('/admin/grades', payload); await fetchGrades() }
  async function updateGrade(code, payload) { await api.put(`/admin/grades/${code}`, payload); await fetchGrades() }
  async function deleteGrade(code) { await api.delete(`/admin/grades/${code}`); await fetchGrades() }
  async function setGradeSubjects(code, subjectIds) {
    await api.put(`/admin/grades/${code}/subjects`, { subjectIds })
    await fetchSubjectsForGrade(code)
  }

  async function createSubject(payload) { await api.post('/admin/subjects', payload); await fetchAllSubjects() }
  async function updateSubject(id, payload) { await api.put(`/admin/subjects/${id}`, payload); await fetchAllSubjects() }
  async function deleteSubject(id) { await api.delete(`/admin/subjects/${id}`); await fetchAllSubjects() }

  async function createTutor(payload) { await api.post('/admin/tutors', payload); await fetchTutors() }
  async function updateTutor(id, payload) { await api.put(`/admin/tutors/${id}`, payload); await fetchTutors() }
  async function deleteTutor(id) { await api.delete(`/admin/tutors/${id}`); await fetchTutors() }

  // ── admin: books ────────────────────────────────────────────────────────
  async function fetchBookSyllabus(bookId) { return (await api.get(`/admin/books/${bookId}/syllabus`)).data }
  async function fetchGradesForSubject(subjectId) { return (await api.get(`/admin/subjects/${subjectId}/grades`)).data }
  async function fetchBooks(subjectId) { return (await api.get(`/admin/subjects/${subjectId}/books`)).data }
  async function createBook(subjectId, payload) { return (await api.post(`/admin/subjects/${subjectId}/books`, payload)).data }
  async function updateBook(subjectId, id, payload) { return (await api.put(`/admin/subjects/${subjectId}/books/${id}`, payload)).data }
  async function deleteBook(subjectId, id) { await api.delete(`/admin/subjects/${subjectId}/books/${id}`) }

  // ── admin: book units ────────────────────────────────────────────────────
  async function fetchBookUnits(bookId) { return (await api.get(`/admin/books/${bookId}/units`)).data }
  async function createBookUnit(bookId, payload) { return (await api.post(`/admin/books/${bookId}/units`, payload)).data }
  async function updateBookUnit(bookId, id, payload) { return (await api.put(`/admin/books/${bookId}/units/${id}`, payload)).data }
  async function deleteBookUnit(bookId, id) { await api.delete(`/admin/books/${bookId}/units/${id}`) }

  // ── admin: unit topics ───────────────────────────────────────────────────
  async function fetchUnitTopics(unitId) { return (await api.get(`/admin/units/${unitId}/topics`)).data }
  async function createUnitTopic(unitId, payload) { return (await api.post(`/admin/units/${unitId}/topics`, payload)).data }
  async function updateUnitTopic(unitId, id, payload) { return (await api.put(`/admin/units/${unitId}/topics/${id}`, payload)).data }
  async function deleteUnitTopic(unitId, id) { await api.delete(`/admin/units/${unitId}/topics/${id}`) }

  // ── admin: unit resources (ContentItems) ────────────────────────────────
  async function fetchUnitResources(unitId) { return (await api.get(`/admin/units/${unitId}/resources`)).data }
  async function createResource(payload) { return (await api.post('/admin/content', payload)).data }
  async function updateResource(id, payload) { return (await api.put(`/admin/content/${id}`, payload)).data }
  async function deleteResource(id) { await api.delete(`/admin/content/${id}`) }

  // ── admin: topic resources (ContentItems per Topic) ──────────────────────
  async function fetchTopicResources(topicId) { return (await api.get(`/admin/topics/${topicId}/resources`)).data }
  async function createTopicResource(topicId, payload) { return (await api.post(`/admin/topics/${topicId}/resources`, payload)).data }
  async function updateTopicResource(topicId, id, payload) { return (await api.put(`/admin/topics/${topicId}/resources/${id}`, payload)).data }
  async function deleteTopicResource(topicId, id) { await api.delete(`/admin/topics/${topicId}/resources/${id}`) }

  async function addUnit(payload) { return (await api.post('/admin/syllabus/units', payload)).data }
  async function updateUnit(id, payload) { return (await api.put(`/admin/syllabus/units/${id}`, payload)).data }
  async function deleteUnit(id) { await api.delete(`/admin/syllabus/units/${id}`) }
  async function addTopic(payload) { return (await api.post('/admin/syllabus/topics', payload)).data }
  async function updateSyllabusTopic(id, payload) { return (await api.put(`/admin/syllabus/topics/${id}`, payload)).data }
  async function deleteTopic(id) { await api.delete(`/admin/syllabus/topics/${id}`) }
  async function addObjective(payload) { return (await api.post('/admin/syllabus/objectives', payload)).data }
  async function updateObjective(id, payload) { return (await api.put(`/admin/syllabus/objectives/${id}`, payload)).data }
  async function deleteObjective(id) { await api.delete(`/admin/syllabus/objectives/${id}`) }

  return {
    grades, bands, mediums, subjects, tutors, gradeSubjects, syllabus, loaded, loading, error,
    enabledGrades, gradeByCode, subjectsForGrade, findSubject, tutorsForGrade, syllabusFor,
    fetchBands, fetchMediums, fetchGrades, fetchTutors, fetchSubjectsForGrade, fetchSyllabus, bootstrap, fetchAllSubjects,
    createBand, updateBand, deleteBand,
    createMedium, updateMedium, deleteMedium,
    createGrade, updateGrade, deleteGrade, setGradeSubjects,
    createSubject, updateSubject, deleteSubject,
    createTutor, updateTutor, deleteTutor,
    fetchBookSyllabus, fetchGradesForSubject, fetchBooks, createBook, updateBook, deleteBook,
    fetchBookUnits, createBookUnit, updateBookUnit, deleteBookUnit,
    fetchUnitTopics, createUnitTopic, updateUnitTopic, deleteUnitTopic,
    fetchUnitResources, createResource, updateResource, deleteResource,
    fetchTopicResources, createTopicResource, updateTopicResource, deleteTopicResource,
    addUnit, updateUnit, deleteUnit, addTopic, updateSyllabusTopic, deleteTopic, addObjective, updateObjective, deleteObjective,
  }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useCatalogStore, import.meta.hot))
}
