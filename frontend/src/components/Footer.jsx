import { FileText, Shield } from 'lucide-react'
import styles from './Footer.module.css'

export default function Footer() {
  const year = new Date().getFullYear()
  return (
    <footer className={styles.footer}>
      <div className={styles.inner}>
        <div className={styles.brand}>
          <FileText size={16} />
          <span>AutoReport</span>
        </div>
        <p className={styles.tagline}>Generate professional reports in seconds.</p>
        <div className={styles.links}>
          <a href="/">Home</a>
          <a href="/summary">Summary</a>
          <a href="/history">History</a>
          <span className={styles.divider}>|</span>
          <a href="/privacy-policy" className={styles.policyLink}>
            <Shield size={13} /> Privacy Policy
          </a>
          <a href="/cookie-policy" className={styles.policyLink}>
            <Shield size={13} /> Cookie Policy
          </a>
        </div>
        <p className={styles.copy}>© {year} AutoReport. All rights reserved.</p>
      </div>
    </footer>
  )
}
