import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { Download, FileText, Copy, ArrowLeft, CheckCheck, Pencil } from 'lucide-react'
import toast from 'react-hot-toast'
import { getReport, exportPdf, exportDocx } from '../services/api'
import TaskStatusChart from '../components/TaskStatusChart'
import styles from './PreviewPage.module.css'

export default function PreviewPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [report, setReport] = useState(null)
  const [copied, setCopied] = useState(false)

  useEffect(() => {
    getReport(id)
      .then(r => setReport(r.data))
      .catch(() => toast.error('Could not load report'))
  }, [id])

  const downloadBlob = (blob, filename) => {
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = filename; a.click()
    URL.revokeObjectURL(url)
  }

  const handlePdf = async () => {
    try {
      const { data } = await exportPdf(id)
      downloadBlob(data, `report_${id}.pdf`)
      toast.success('PDF downloaded')
    } catch { toast.error('PDF export failed') }
  }

  const handleDocx = async () => {
    try {
      const { data } = await exportDocx(id)
      downloadBlob(data, `report_${id}.docx`)
      toast.success('DOCX downloaded')
    } catch { toast.error('DOCX export failed') }
  }

  const handleCopy = () => {
    navigator.clipboard.writeText(report.formattedContent)
    setCopied(true)
    toast.success('Copied to clipboard')
    setTimeout(() => setCopied(false), 2000)
  }

  if (!report) return <div className={styles.loading}>Loading report...</div>

  const templateLabel = {
    daily: 'Daily Accomplishment Report',
    weekly: 'Weekly Summary Report',
    worklog: 'Work Log Report',
  }[report.templateType] ?? 'Report'

  return (
    <main className={styles.main}>
      <div className={styles.topBar}>
        <button className={styles.back} onClick={() => navigate('/')}>
          <ArrowLeft size={16} /> New Report
        </button>
        <div className={styles.exportBtns}>
          <button className={styles.editBtn} onClick={() => navigate(`/?edit=${id}`)}>
            <Pencil size={15} /> Edit & Regenerate
          </button>
          <button className={styles.copyBtn} onClick={handleCopy}>
            {copied ? <CheckCheck size={15} /> : <Copy size={15} />}
            {copied ? 'Copied!' : 'Copy Text'}
          </button>
          <button className={styles.exportBtn} onClick={handleDocx}>
            <FileText size={15} /> Export DOCX
          </button>
          <button className={`${styles.exportBtn} ${styles.primary}`} onClick={handlePdf}>
            <Download size={15} /> Export PDF
          </button>
        </div>
      </div>

      <div className={styles.previewCard}>
        <div className={styles.reportHeader}>
          <h2 className={styles.reportTitle}>{templateLabel}</h2>
          <span className={styles.badge}>{report.templateType}</span>
        </div>

        <div className={styles.metaGrid}>
          <MetaItem label="Name" value={report.name} />
          <MetaItem label="Department" value={report.department} />
          <MetaItem label="Date" value={new Date(report.date).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })} />
          <MetaItem label="Time" value={`${report.timeIn} – ${report.timeOut}`} />
        </div>

        <div className={styles.divider} />

        <h3 className={styles.sectionLabel}>Tasks</h3>
        <ul className={styles.taskList}>
          {report.tasks.map((t, i) => (
            <li key={i} className={styles.taskItem}>
              <span className={styles.taskNum}>{i + 1}</span>
              <span className={styles.taskText}>{t.task}</span>
              <span className={`${styles.statusBadge} ${styles[t.status.replace(' ', '')]}`}>{t.status}</span>
            </li>
          ))}
        </ul>

        {report.notes && (
          <>
            <div className={styles.divider} />
            <h3 className={styles.sectionLabel}>Notes</h3>
            <p className={styles.notes}>{report.notes}</p>
          </>
        )}

        <div className={styles.divider} />
        <h3 className={styles.sectionLabel}>Task Status Breakdown</h3>
        <TaskStatusChart tasks={report.tasks} />

        <div className={styles.divider} />
        <h3 className={styles.sectionLabel}>Formatted Output</h3>
        <pre className={styles.formatted}>{report.formattedContent}</pre>
      </div>
    </main>
  )
}

function MetaItem({ label, value }) {
  return (
    <div>
      <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '.05em' }}>{label}</div>
      <div style={{ fontSize: 14, marginTop: 2 }}>{value}</div>
    </div>
  )
}
