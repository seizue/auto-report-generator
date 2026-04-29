import axios from 'axios'

// Use environment variable for API URL, fallback to relative path for development
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

// Anonymous browser identity — generated once, persisted in localStorage.
// This lets each browser see only its own report history without requiring login.
function getClientId() {
  const key = 'arg_client_id'
  let id = localStorage.getItem(key)
  if (!id) {
    id = crypto.randomUUID()
    localStorage.setItem(key, id)
  }
  return id
}

const api = axios.create({ baseURL: API_BASE_URL })

// Attach the client ID to every request
api.interceptors.request.use(config => {
  config.headers['X-Client-Id'] = getClientId()
  return config
})

export const generateReport = (data) => api.post('/generate-report', data)
export const updateReport = (id, data) => api.put(`/reports/${id}`, data)
export const parseText = (data) => api.post('/parse-text', data)
export const generateSummary = (data) => api.post('/summary-report', data)
export const exportSummaryPdf = (data) => api.post('/summary/export/pdf', data, { responseType: 'blob' })
export const exportSummaryDocx = (data) => api.post('/summary/export/docx', data, { responseType: 'blob' })
export const getReports = () => api.get('/reports')
export const getReport = (id) => api.get(`/reports/${id}`)
export const deleteReport = (id) => api.delete(`/reports/${id}`)
export const deleteAllReports = () => api.delete('/reports')
export const exportPdf = (id) => api.post(`/export/pdf/${id}`, {}, { responseType: 'blob' })
export const exportDocx = (id) => api.post(`/export/docx/${id}`, {}, { responseType: 'blob' })
export const getTemplates = () => api.get('/templates')
export const extractTextFromImage = (formData) => api.post('/ocr/extract', formData, {
  headers: { 'Content-Type': 'multipart/form-data' }
})

// Smart Suggestions
export const getTaskSuggestions = (employeeName, limit = 10) => api.get(`/suggestions/tasks/${employeeName}?limit=${limit}`)
export const getProductivityInsights = (employeeName) => api.get(`/suggestions/insights/${employeeName}`)
export const categorizeTask = (task) => api.post('/suggestions/categorize', { task })
