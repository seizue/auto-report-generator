import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Eye, Trash2, FileText, BarChart2, ChevronLeft, ChevronRight, Trash, GitCompare } from 'lucide-react'
import toast from 'react-hot-toast'
import { getReports, deleteReport, deleteAllReports } from '../services/api'
import ReportCharts from '../components/ReportCharts'
import styles from './HistoryPage.module.css'

const PAGE_SIZE = 8

export default function HistoryPage() {
  const [reports, setReports] = useState([])
  const [loading, setLoading] = useState(true)
  const [showCharts, setShowCharts] = useState(true)
  const [page, setPage] = useState(1)
  const [compareMode, setCompareMode] = useState(false)
  const [selectedForCompare, setSelectedForCompare] = useState([])
  const navigate = useNavigate()

  useEffect(() => {
    getReports()
      .then(r => setReports(r.data))
      .catch(() => toast.error('Could not load history'))
      .finally(() => setLoading(false))
  }, [])

  const handleDelete = async (id, e) => {
    e.stopPropagation()
    if (!confirm('Delete this report?')) return
    try {
      await deleteReport(id)
      setReports(r => r.filter(x => x.id !== id))
      // adjust page if last item on page was deleted
      const newTotal = reports.length - 1
      const maxPage = Math.max(1, Math.ceil(newTotal / PAGE_SIZE))
      if (page > maxPage) setPage(maxPage)
      toast.success('Deleted')
    } catch { toast.error('Delete failed') }
  }

  const handleDeleteAll = async () => {
    if (!confirm(`Delete all ${reports.length} reports? This cannot be undone.`)) return
    try {
      await deleteAllReports()
      setReports([])
      setPage(1)
      toast.success('All reports deleted')
    } catch { toast.error('Delete all failed') }
  }

  const toggleCompareMode = () => {
    setCompareMode(m => !m)
    setSelectedForCompare([])
  }

  const toggleSelectForCompare = (id, e) => {
    e.stopPropagation()
    setSelectedForCompare(prev => {
      if (prev.includes(id)) {
        return prev.filter(x => x !== id)
      } else if (prev.length < 2) {
        return [...prev, id]
      } else {
        toast.error('You can only compare 2 reports at a time')
        return prev
      }
    })
  }

  const handleCompare = () => {
    if (selectedForCompare.length !== 2) {
      toast.error('Please select exactly 2 reports to compare')
      return
    }
    navigate(`/compare?report1=${selectedForCompare[0]}&report2=${selectedForCompare[1]}`)
  }

  const templateLabel = (type) => ({
    daily: 'Daily',
    weekly: 'Weekly',
    worklog: 'Work Log',
  }[type] ?? type)

  const totalPages = Math.max(1, Math.ceil(reports.length / PAGE_SIZE))
  const paginated = reports.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  if (loading) return <div className={styles.empty}>Loading...</div>

  return (
    <main className={styles.main}>
      <div className={styles.header}>
        <h1 className={styles.title}>Report History</h1>
        <div className={styles.headerRight}>
          {reports.length > 1 && (
            <button
              className={`${styles.compareBtn} ${compareMode ? styles.active : ''}`}
              onClick={toggleCompareMode}
            >
              <GitCompare size={15} /> {compareMode ? 'Cancel' : 'Compare'}
            </button>
          )}
          {compareMode && selectedForCompare.length === 2 && (
            <button className={styles.compareGoBtn} onClick={handleCompare}>
              Compare Selected
            </button>
          )}
          {reports.length > 0 && (
            <>
              <button
                className={`${styles.chartToggle} ${showCharts ? styles.active : ''}`}
                onClick={() => setShowCharts(s => !s)}
              >
                <BarChart2 size={15} /> {showCharts ? 'Hide Charts' : 'Show Charts'}
              </button>
              <button className={styles.deleteAllBtn} onClick={handleDeleteAll}>
                <Trash size={14} /> Delete All
              </button>
            </>
          )}
          <span className={styles.count}>{reports.length} report{reports.length !== 1 ? 's' : ''}</span>
        </div>
      </div>

      {showCharts && reports.length > 0 && <ReportCharts reports={reports} />}

      {reports.length === 0 ? (
        <div className={styles.empty}>
          <FileText size={40} style={{ opacity: .3 }} />
          <p>No reports yet. Generate your first one!</p>
        </div>
      ) : (
        <>
          <div className={styles.list}>
            {paginated.map(r => (
              <div 
                key={r.id} 
                className={`${styles.row} ${compareMode && selectedForCompare.includes(r.id) ? styles.selected : ''}`}
                onClick={(e) => compareMode ? toggleSelectForCompare(r.id, e) : navigate(`/preview/${r.id}`)}
              >
                {compareMode && (
                  <input
                    type="checkbox"
                    checked={selectedForCompare.includes(r.id)}
                    onChange={(e) => toggleSelectForCompare(r.id, e)}
                    className={styles.checkbox}
                  />
                )}
                <div className={styles.rowLeft}>
                  <span className={styles.typeBadge}>{templateLabel(r.templateType)}</span>
                  <div>
                    <div className={styles.rowName}>{r.name}</div>
                    <div className={styles.rowMeta}>{r.department} · {new Date(r.date).toLocaleDateString()}</div>
                  </div>
                </div>
                {!compareMode && (
                  <div className={styles.rowActions}>
                    <span className={styles.rowDate}>{new Date(r.createdAt).toLocaleDateString()}</span>
                    <button className={styles.viewBtn} onClick={() => navigate(`/preview/${r.id}`)}>
                      <Eye size={14} />
                    </button>
                    <button className={styles.deleteBtn} onClick={(e) => handleDelete(r.id, e)}>
                      <Trash2 size={14} />
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className={styles.pagination}>
              <button
                className={styles.pageBtn}
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                <ChevronLeft size={15} />
              </button>

              {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
                <button
                  key={p}
                  className={`${styles.pageBtn} ${p === page ? styles.pageActive : ''}`}
                  onClick={() => setPage(p)}
                >
                  {p}
                </button>
              ))}

              <button
                className={styles.pageBtn}
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
              >
                <ChevronRight size={15} />
              </button>

              <span className={styles.pageInfo}>
                {(page - 1) * PAGE_SIZE + 1}–{Math.min(page * PAGE_SIZE, reports.length)} of {reports.length}
              </span>
            </div>
          )}
        </>
      )}
    </main>
  )
}
