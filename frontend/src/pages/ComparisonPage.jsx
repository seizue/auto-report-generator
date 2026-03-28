import { useState, useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ArrowLeft, Calendar, User, GitCompare, TrendingUp, TrendingDown, Minus } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import toast from 'react-hot-toast'
import { getReports } from '../services/api'
import styles from './ComparisonPage.module.css'

export default function ComparisonPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [reports, setReports] = useState([])
  const [selectedReport1, setSelectedReport1] = useState(null)
  const [selectedReport2, setSelectedReport2] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadReports()
  }, [])

  useEffect(() => {
    const id1 = searchParams.get('report1')
    const id2 = searchParams.get('report2')
    if (id1 && reports.length > 0) {
      const r1 = reports.find(r => r.id === parseInt(id1))
      if (r1) setSelectedReport1(r1)
    }
    if (id2 && reports.length > 0) {
      const r2 = reports.find(r => r.id === parseInt(id2))
      if (r2) setSelectedReport2(r2)
    }
  }, [searchParams, reports])

  const loadReports = async () => {
    try {
      const { data } = await getReports()
      setReports(data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)))
    } catch (error) {
      toast.error('Failed to load reports')
    } finally {
      setLoading(false)
    }
  }

  const getDiff = () => {
    if (!selectedReport1 || !selectedReport2) return null

    const items1 = selectedReport1.tasks || []
    const items2 = selectedReport2.tasks || []

    const added = items2.filter(i2 => !items1.some(i1 => i1.task === i2.task))
    const removed = items1.filter(i1 => !items2.some(i2 => i2.task === i1.task))
    const modified = items2.filter(i2 => {
      const i1 = items1.find(i => i.task === i2.task)
      return i1 && (i1.status !== i2.status || i1.notes !== i2.notes)
    })
    const unchanged = items2.filter(i2 => {
      const i1 = items1.find(i => i.task === i2.task)
      return i1 && i1.status === i2.status && i1.notes === i2.notes
    })

    return { added, removed, modified, unchanged }
  }

  const getProgressMetrics = () => {
    if (!selectedReport1 || !selectedReport2) return null

    const items1 = selectedReport1.tasks || []
    const items2 = selectedReport2.tasks || []

    const completed1 = items1.filter(i => i.status === 'Completed').length
    const completed2 = items2.filter(i => i.status === 'Completed').length
    const total1 = items1.length
    const total2 = items2.length

    const completionRate1 = total1 > 0 ? (completed1 / total1) * 100 : 0
    const completionRate2 = total2 > 0 ? (completed2 / total2) * 100 : 0

    return {
      completionChange: completionRate2 - completionRate1,
      taskCountChange: total2 - total1,
      completedChange: completed2 - completed1
    }
  }

  const diff = getDiff()
  const metrics = getProgressMetrics()

  if (loading) {
    return <div className={styles.loading}>Loading reports...</div>
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <button onClick={() => navigate('/history')} className={styles.backBtn}>
          <ArrowLeft size={20} />
          Back to History
        </button>
        <h1 className={styles.title}>
          <GitCompare size={28} />
          Report Comparison
        </h1>
      </div>

      <div className={styles.selectors}>
        <div className={styles.selector}>
          <label>Report 1 (Older)</label>
          <select 
            value={selectedReport1?.id || ''} 
            onChange={(e) => setSelectedReport1(reports.find(r => r.id === parseInt(e.target.value)))}
          >
            <option value="">Select a report...</option>
            {reports.map(r => (
              <option key={r.id} value={r.id}>
                {r.name} - {new Date(r.createdAt).toLocaleDateString()}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.selector}>
          <label>Report 2 (Newer)</label>
          <select 
            value={selectedReport2?.id || ''} 
            onChange={(e) => setSelectedReport2(reports.find(r => r.id === parseInt(e.target.value)))}
          >
            <option value="">Select a report...</option>
            {reports.map(r => (
              <option key={r.id} value={r.id}>
                {r.name} - {new Date(r.createdAt).toLocaleDateString()}
              </option>
            ))}
          </select>
        </div>
      </div>

      {selectedReport1 && selectedReport2 && metrics && (
        <div className={styles.metrics}>
          <div className={styles.metric}>
            <div className={styles.metricLabel}>Completion Rate Change</div>
            <div className={`${styles.metricValue} ${metrics.completionChange > 0 ? styles.positive : metrics.completionChange < 0 ? styles.negative : ''}`}>
              {metrics.completionChange > 0 ? <TrendingUp size={20} /> : metrics.completionChange < 0 ? <TrendingDown size={20} /> : <Minus size={20} />}
              {metrics.completionChange > 0 ? '+' : ''}{metrics.completionChange.toFixed(1)}%
            </div>
          </div>
          <div className={styles.metric}>
            <div className={styles.metricLabel}>Task Count Change</div>
            <div className={`${styles.metricValue} ${metrics.taskCountChange > 0 ? styles.positive : metrics.taskCountChange < 0 ? styles.negative : ''}`}>
              {metrics.taskCountChange > 0 ? '+' : ''}{metrics.taskCountChange}
            </div>
          </div>
          <div className={styles.metric}>
            <div className={styles.metricLabel}>Completed Tasks Change</div>
            <div className={`${styles.metricValue} ${metrics.completedChange > 0 ? styles.positive : metrics.completedChange < 0 ? styles.negative : ''}`}>
              {metrics.completedChange > 0 ? '+' : ''}{metrics.completedChange}
            </div>
          </div>
        </div>
      )}

      {selectedReport1 && selectedReport2 && diff && (
        <div className={styles.comparison}>
          <div className={styles.reportColumn}>
            <div className={styles.reportHeader}>
              <h2>{selectedReport1.name}</h2>
              <div className={styles.reportMeta}>
                <span><User size={14} /> {selectedReport1.employeeName}</span>
                <span><Calendar size={14} /> {new Date(selectedReport1.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
            <div className={styles.reportContent}>
              {selectedReport1.tasks?.map((item, idx) => {
                const isRemoved = diff.removed.some(i => i.task === item.task)
                const isModified = diff.modified.some(i => i.task === item.task)
                return (
                  <div key={idx} className={`${styles.item} ${isRemoved ? styles.removed : isModified ? styles.modified : ''}`}>
                    <div className={styles.itemTask}>{item.task}</div>
                    <div className={styles.itemStatus}>{item.status}</div>
                    {item.notes && <div className={styles.itemNotes}>{item.notes}</div>}
                  </div>
                )
              })}
            </div>
          </div>

          <div className={styles.reportColumn}>
            <div className={styles.reportHeader}>
              <h2>{selectedReport2.name}</h2>
              <div className={styles.reportMeta}>
                <span><User size={14} /> {selectedReport2.employeeName}</span>
                <span><Calendar size={14} /> {new Date(selectedReport2.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
            <div className={styles.reportContent}>
              {selectedReport2.tasks?.map((item, idx) => {
                const isAdded = diff.added.some(i => i.task === item.task)
                const isModified = diff.modified.some(i => i.task === item.task)
                return (
                  <div key={idx} className={`${styles.item} ${isAdded ? styles.added : isModified ? styles.modified : ''}`}>
                    <div className={styles.itemTask}>{item.task}</div>
                    <div className={styles.itemStatus}>{item.status}</div>
                    {item.notes && <div className={styles.itemNotes}>{item.notes}</div>}
                  </div>
                )
              })}
            </div>
          </div>
        </div>
      )}

      {selectedReport1 && selectedReport2 && diff && (
        <div className={styles.legend}>
          <div className={styles.legendItem}>
            <span className={`${styles.legendColor} ${styles.added}`}></span>
            Added ({diff.added.length})
          </div>
          <div className={styles.legendItem}>
            <span className={`${styles.legendColor} ${styles.removed}`}></span>
            Removed ({diff.removed.length})
          </div>
          <div className={styles.legendItem}>
            <span className={`${styles.legendColor} ${styles.modified}`}></span>
            Modified ({diff.modified.length})
          </div>
          <div className={styles.legendItem}>
            <span className={`${styles.legendColor} ${styles.unchanged}`}></span>
            Unchanged ({diff.unchanged.length})
          </div>
        </div>
      )}

      {(!selectedReport1 || !selectedReport2) && (
        <div className={styles.emptyState}>
          <GitCompare size={48} />
          <p>Select two reports to compare</p>
        </div>
      )}
    </div>
  )
}
