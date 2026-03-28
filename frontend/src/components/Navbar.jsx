import { Link } from 'react-router-dom'
import { FileText, Moon, Sun, History, Sparkles } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import LanguageSelector from './LanguageSelector'
import styles from './Navbar.module.css'

export default function Navbar({ dark, onToggleDark }) {
  const { t } = useTranslation()

  return (
    <nav className={styles.nav}>
      <Link to="/" className={styles.brand}>
        <FileText size={22} />
        <span>AutoReport</span>
      </Link>
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
    </nav>
  )
}
