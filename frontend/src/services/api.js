import axios from 'axios'

const api = axios.create({ baseURL: '/api' })

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
