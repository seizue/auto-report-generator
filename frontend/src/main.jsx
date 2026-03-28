import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import './index.css'
import './i18n/config'

// Set initial RTL direction based on saved language
const savedLang = localStorage.getItem('i18nextLng')
const rtlLanguages = ['ar', 'he']
if (savedLang && rtlLanguages.includes(savedLang)) {
  document.documentElement.dir = 'rtl'
  document.documentElement.lang = savedLang
}

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </React.StrictMode>
)
