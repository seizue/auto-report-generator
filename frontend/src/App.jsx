import { Routes, Route } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { useState, useEffect } from 'react'
import Navbar from './components/Navbar'
import Footer from './components/Footer'
import CookieConsent from './components/CookieConsent'
import HomePage from './pages/HomePage'
import PreviewPage from './pages/PreviewPage'
import HistoryPage from './pages/HistoryPage'
import SummaryPage from './pages/SummaryPage'
import ComparisonPage from './pages/ComparisonPage'
import PrivacyPolicyPage from './pages/PrivacyPolicyPage'
import CookiePolicyPage from './pages/CookiePolicyPage'

export default function App() {
  const [dark, setDark] = useState(() => localStorage.getItem('theme') === 'dark')

  useEffect(() => {
    document.body.classList.toggle('dark', dark)
    localStorage.setItem('theme', dark ? 'dark' : 'light')
  }, [dark])

  return (
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <Toaster position="top-right" />
      <CookieConsent />
      <Navbar dark={dark} onToggleDark={() => setDark(d => !d)} />

      <div style={{ flex: 1 }}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/summary" element={<SummaryPage />} />
          <Route path="/preview/:id" element={<PreviewPage />} />
          <Route path="/history" element={<HistoryPage />} />
          <Route path="/compare" element={<ComparisonPage />} />
          <Route path="/privacy-policy" element={<PrivacyPolicyPage />} />
          <Route path="/cookie-policy" element={<CookiePolicyPage />} />
        </Routes>
      </div>

      {/* Bottom ad banner */}
      <div className="ad-banner" style={{ height: 60, maxWidth: 960, margin: '0 auto 0', width: '100%', padding: '0 16px' }}>
        Advertisement
      </div>

      <Footer />
    </div>
  )
}
