import { Link } from 'react-router-dom'
import { FileText, Moon, Sun, History, Sparkles, Menu, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useState } from 'react'
import LanguageSelector from './LanguageSelector'
import styles from './Navbar.module.css'

export default function Navbar({ dark, onToggleDark }) {
  const { t } = useTranslation()
  const [menuOpen, setMenuOpen] = useState(false)

  return (
    <nav className={styles.nav}>
      <Link to="/" className={styles.brand} onClick={() => setMenuOpen(false)}>
        <FileText size={22} />
        <span>AutoReport</span>
      </Link>

      {/* Desktop actions */}
      <div className={styles.actions}>
        <Link to="/summary" className={styles.summaryLink}>
          <Sparkles size={15} /> {t('nav.summary')}
        </Link>
        <Link to="/history" className={styles.link}>
          <History size={16} /> {t('nav.history')}
        </Link>
        <LanguageSelector />
        <button className={styles.toggle} onClick={onToggleDark} aria-label="Toggle dark mode">
          {dark ? <Sun size={18} /> : <Moon size={18} />}
        </button>
      </div>

      {/* Mobile right side */}
      <div className={styles.mobileRight}>
        <button className={styles.toggle} onClick={onToggleDark} aria-label="Toggle dark mode">
          {dark ? <Sun size={18} /> : <Moon size={18} />}
        </button>
        <button className={styles.hamburger} onClick={() => setMenuOpen(o => !o)} aria-label="Toggle menu">
          {menuOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
      </div>

      {/* Mobile menu */}
      {menuOpen && (
        <div className={styles.mobileMenu}>
          <Link to="/summary" className={styles.mobileLink} onClick={() => setMenuOpen(false)}>
            <Sparkles size={15} /> {t('nav.summary')}
          </Link>
          <Link to="/history" className={styles.mobileLink} onClick={() => setMenuOpen(false)}>
            <History size={16} /> {t('nav.history')}
          </Link>
          <div className={styles.mobileLang}>
            <LanguageSelector />
          </div>
        </div>
      )}
    </nav>
  )
}
