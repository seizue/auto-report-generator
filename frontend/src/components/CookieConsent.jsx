import { useState, useEffect } from 'react'
import { Cookie, X, Shield } from 'lucide-react'
import styles from './CookieConsent.module.css'

export default function CookieConsent() {
  const [accepted, setAccepted] = useState(() => {
    return localStorage.getItem('cookieConsent') !== null
  })
  const [showDetails, setShowDetails] = useState(false)

  const handleAccept = () => {
    localStorage.setItem('cookieConsent', 'accepted')
    setAccepted(true)
  }

  const handleReject = () => {
    localStorage.setItem('cookieConsent', 'rejected')
    setAccepted(true)
  }

  if (accepted) return null

  return (
    <>
      {/* Overlay to block interaction */}
      <div className={styles.overlay} />
      
      {/* Cookie consent banner */}
      <div className={styles.banner}>
        <div className={styles.header}>
          <div className={styles.icon}>
            <Cookie size={24} />
          </div>
          <div>
            <h3 className={styles.title}>Cookie Consent</h3>
            <div className={styles.gdprBadge}>
              <Shield size={12} /> GDPR Compliant
            </div>
          </div>
        </div>

        <div className={styles.content}>
          <p className={styles.text}>
            We use cookies and similar technologies to provide essential functionality, 
            improve your experience, and analyze site usage. You have the right to accept or reject non-essential cookies.
          </p>

          {showDetails && (
            <div className={styles.details}>
              <div className={styles.detailSection}>
                <strong>Essential Cookies (Required)</strong>
                <p>These cookies are necessary for the website to function and cannot be disabled. 
                They include session management and security features.</p>
              </div>
              <div className={styles.detailSection}>
                <strong>Functional Cookies (Optional)</strong>
                <p>These cookies enable enhanced functionality like remembering your preferences 
                (theme, language) and providing personalized features.</p>
              </div>
              <div className={styles.detailSection}>
                <strong>Analytics Cookies (Optional)</strong>
                <p>These cookies help us understand how visitors interact with our website, 
                allowing us to improve user experience.</p>
              </div>
            </div>
          )}

          <button 
            className={styles.detailsToggle} 
            onClick={() => setShowDetails(!showDetails)}
          >
            {showDetails ? 'Hide Details' : 'Show Details'}
          </button>

          <p className={styles.privacy}>
            By clicking "Accept All", you consent to our use of cookies. 
            You can change your preferences at any time. 
            Read our <a href="/privacy-policy" className={styles.link}>Privacy Policy</a> and{' '}
            <a href="/cookie-policy" className={styles.link}>Cookie Policy</a> for more information.
          </p>
        </div>

        <div className={styles.actions}>
          <button className={styles.rejectBtn} onClick={handleReject}>
            Reject Non-Essential
          </button>
          <button className={styles.acceptBtn} onClick={handleAccept}>
            Accept All Cookies
          </button>
        </div>
      </div>
    </>
  )
}
