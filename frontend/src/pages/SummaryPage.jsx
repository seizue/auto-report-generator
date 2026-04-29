import { useState, useEffect } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { Sparkles, Copy, CheckCheck, Download, ChevronDown, ChevronUp, FileText, PenLine, RotateCcw, LayoutList, AlignLeft } from 'lucide-react'
import toast from 'react-hot-toast'
import html2canvas from 'html2canvas'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend, CartesianGrid
} from 'recharts'
import { generateSummary, exportSummaryPdf, exportSummaryDocx } from '../services/api'
import UniversalFileUpload from '../components/UniversalFileUpload'
import styles from './SummaryPage.module.css'

const REPORT_TYPES = [
  'General Summary Report',
  'Daily Accomplishment Report',
  'Weekly Summary Report',
  'Monthly Progress Report',
  'Project Status Report',
  'Incident Report',
]

const EXAMPLES = [
  `fixed printer then updated pc and also checked internet slow. attended standup at 9am. reviewed pull requests and merged 3 branches. deployed hotfix to production server. pending: update documentation`,
  `met with client at 9am to discuss project requirements. prepared proposal document and sent it by 4pm. coordinated with design team for mockups. researched competitor pricing. still working on budget breakdown`,
  `conducted monthly inventory check. repaired 2 workstations. installed software updates on all office computers. responded to 15 support tickets. resolved network connectivity issue in conference room. scheduled maintenance for next week`,
]

const tooltipStyle = {
  backgroundColor: 'var(--surface)',
  border: '1px solid var(--border)',
  borderRadius: 8,
  fontSize: 12,
}

const EMPTY_FORM = {
  rawText: '',
  reportType: 'General Summary Report',
  authorName: '',
  department: '',
  period: '',
}

export default function SummaryPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [form, setForm] = useState(() => {
    // If navigated from home quick-generate, restore the original form data
    const s = location.state
    if (s?.form) return s.form
    return EMPTY_FORM
  })
  const [result, setResult] = useState(location.state?.result ?? null)
  const [loading, setLoading] = useState(false)
  const [editing, setEditing] = useState(false)   // show edit panel alongside result
  const [copied, setCopied] = useState(false)
  const [collapsed, setCollapsed] = useState({})

  // Refs to capture chart DOM nodes — removed, exports use formatted text now

  useEffect(() => {
    if (location.state?.result) {
      setTimeout(() => document.getElementById('summary-result')?.scrollIntoView({ behavior: 'smooth' }), 100)
    }
  }, [])

  const set = (f, v) => setForm(p => ({ ...p, [f]: v }))

  const handleGenerate = async () => {
    if (!form.rawText.trim()) return toast.error('Paste some text first')
    setLoading(true)
    setResult(null)
    setEditing(false)
    try {
      const { data } = await generateSummary(form)
      setResult(data)
      toast.success('Summary report generated')
      setTimeout(() => document.getElementById('summary-result')?.scrollIntoView({ behavior: 'smooth' }), 100)
    } catch {
      toast.error('Generation failed. Is the backend running?')
    } finally {
      setLoading(false)
    }
  }

  const handleNew = () => {
    setResult(null)
    setEditing(false)
    setForm(EMPTY_FORM)
  }

  const handleEdit = () => {
    // Pre-fill form with the raw text that was used (stored in form state)
    setEditing(e => !e)
  }

  const toggleGroup = (cat) => setCollapsed(c => ({ ...c, [cat]: !c[cat] }))

  const downloadBlob = (blob, filename) => {
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = filename; a.click()
    URL.revokeObjectURL(url)
  }

  const handleCopy = () => {
    navigator.clipboard.writeText(result.formattedText)
    setCopied(true)
    toast.success('Copied to clipboard')
    setTimeout(() => setCopied(false), 2000)
  }

  const captureCharts = async () => {
    try {
      // Wait for charts to fully render
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const activityChartEl = document.getElementById('activity-chart')
      const statusChartEl = document.getElementById('status-chart')
      
      if (!activityChartEl || !statusChartEl) {
        console.error('Chart elements not found')
        return { activityChartImage: null, statusChartImage: null }
      }
      
      // Use html2canvas to capture the entire chart container
      const captureWithHtml2Canvas = async (element) => {
        try {
          const canvas = await html2canvas(element, {
            scale: 2, // High resolution
            backgroundColor: '#ffffff',
            logging: false,
            useCORS: true,
            allowTaint: true,
            imageTimeout: 0,
          })
          return canvas.toDataURL('image/png', 1.0)
        } catch (error) {
          console.error('html2canvas capture failed:', error)
          return null
        }
      }
      
      const [activityImage, statusImage] = await Promise.all([
        captureWithHtml2Canvas(activityChartEl),
        captureWithHtml2Canvas(statusChartEl)
      ])
      
      return {
        activityChartImage: activityImage,
        statusChartImage: statusImage
      }
    } catch (error) {
      console.error('Failed to capture charts:', error)
      return { activityChartImage: null, statusChartImage: null }
    }
  }

  const handlePdf = async () => {
    try {
      toast.loading('Preparing PDF with charts...', { id: 'pdf-export' })
      const charts = await captureCharts()
      const resultWithCharts = { ...result, ...charts }
      const { data } = await exportSummaryPdf(resultWithCharts)
      downloadBlob(data, `${result.title.replace(/\s+/g, '_')}.pdf`)
      toast.success('PDF downloaded', { id: 'pdf-export' })
    } catch (error) { 
      console.error('PDF export error:', error)
      toast.error('PDF export failed', { id: 'pdf-export' })
    }
  }

  const handleDocx = async () => {
    try {
      toast.loading('Preparing DOCX with charts...', { id: 'docx-export' })
      const charts = await captureCharts()
      const resultWithCharts = { ...result, ...charts }
      const { data } = await exportSummaryDocx(resultWithCharts)
      downloadBlob(data, `${result.title.replace(/\s+/g, '_')}.docx`)
      toast.success('DOCX downloaded', { id: 'docx-export' })
    } catch (error) { 
      console.error('DOCX export error:', error)
      toast.error('DOCX export failed', { id: 'docx-export' })
    }
  }

  return (
    <main className={styles.main}>
      <div className={styles.hero}>
        <div className={styles.heroBadge}><Sparkles size={13} /> Smart Summary</div>
        <h1 className={styles.title}>Free Text → Summary Report</h1>
        <p className={styles.subtitle}>
          Paste anything — raw notes, bullet points, a brain dump — and get a structured report with charts instantly.
        </p>
      </div>

      {/* Mode toggle */}
      <div className={styles.modeToggle}>
        <button
          className={`${styles.modeBtn}`}
          onClick={() => navigate('/')}
        >
          <LayoutList size={15} /> Form Input
        </button>
        <button
          className={`${styles.modeBtn} ${styles.modeActive}`}
        >
          <AlignLeft size={15} /> Paste Text
        </button>
      </div>

      {/* Input form — shown when no result, or when editing */}
      {(!result || editing) && (
        <div className={styles.card}>
          {editing && (
            <div className={styles.editingBanner}>
              <PenLine size={13} /> Editing — change your text and regenerate
            </div>
          )}
          <div className={styles.topFields}>
            <div className={styles.field}>
              <label>Report Type</label>
              <select value={form.reportType} onChange={e => set('reportType', e.target.value)}>
                {REPORT_TYPES.map(t => <option key={t}>{t}</option>)}
              </select>
            </div>
          </div>

          <div className={styles.field}>
            <label>Your Raw Text *</label>
            <UniversalFileUpload onExtracted={(text) => set('rawText', form.rawText ? form.rawText + '\n' + text : text)} />
            <textarea
              className={styles.textarea}
              value={form.rawText}
              onChange={e => set('rawText', e.target.value)}
              placeholder="Paste anything here — notes, bullet points, a paragraph, a brain dump..."
              rows={7}
            />
          </div>

          {!editing && (
            <div className={styles.examples}>
              <span>Try an example:</span>
              {EXAMPLES.map((ex, i) => (
                <button key={i} className={styles.chip} onClick={() => set('rawText', ex)}>
                  Example {i + 1}
                </button>
              ))}
            </div>
          )}

          <div className={styles.actions}>
            {editing && (
              <button className={styles.cancelBtn} onClick={() => setEditing(false)}>
                Cancel
              </button>
            )}
            <button className={styles.generateBtn} onClick={handleGenerate} disabled={loading}>
              <Sparkles size={16} />
              {loading ? 'Generating...' : editing ? 'Regenerate Report' : 'Generate Summary Report'}
            </button>
          </div>
        </div>
      )}

      {/* Result skeleton while generating */}
      {loading && (
        <div className={styles.skeleton}>
          {/* top bar */}
          <div className={styles.skeletonBar}>
            <div className={`${styles.skeletonTitle} ${styles.skeletonBase}`} />
            <div className={styles.skeletonBtns}>
              <div className={`${styles.skeletonBtn} ${styles.skeletonBase}`} />
              <div className={`${styles.skeletonBtn} ${styles.skeletonBase}`} />
              <div className={`${styles.skeletonBtn} ${styles.skeletonBase}`} />
            </div>
          </div>
          {/* heading */}
          <div className={`${styles.skeletonHeading} ${styles.skeletonBase}`} />
          {/* metrics strip */}
          <div className={styles.skeletonMetrics}>
            {[...Array(5)].map((_, i) => (
              <div key={i} className={styles.skeletonMetric}>
                <div className={`${styles.skeletonMetricVal} ${styles.skeletonBase}`} />
                <div className={`${styles.skeletonMetricLbl} ${styles.skeletonBase}`} />
              </div>
            ))}
          </div>
          {/* charts */}
          <div className={styles.skeletonCharts}>
            <div className={`${styles.skeletonChart} ${styles.skeletonBase}`} />
            <div className={`${styles.skeletonChart} ${styles.skeletonBase}`} />
          </div>
          {/* summary paragraph */}
          <div className={styles.skeletonPara}>
            <div className={`${styles.skeletonLine} ${styles.skeletonBase}`} />
            <div className={`${styles.skeletonLine} ${styles.skeletonBase}`} />
            <div className={`${styles.skeletonLineShort} ${styles.skeletonBase}`} />
          </div>
          {/* activity groups */}
          <div className={styles.skeletonGroups}>
            {[...Array(4)].map((_, i) => (
              <div key={i} className={`${styles.skeletonGroup} ${styles.skeletonBase}`} />
            ))}
          </div>
        </div>
      )}

      {/* Result */}
      {!loading && result && (
        <div id="summary-result" className={styles.result}>

          <div className={styles.resultBar}>
            <button className={styles.backBtn} onClick={handleNew}>
              ← New Report
            </button>
            <div className={styles.resultActions}>
              <button className={`${styles.actionBtn} ${styles.actionEdit} ${editing ? styles.actionActive : ''}`} onClick={handleEdit}>
                <PenLine size={14} /> {editing ? 'Editing' : 'Edit'} & Regenerate
              </button>
              {result.id && (
                <button className={styles.actionBtn} onClick={() => navigate(`/preview/${result.id}`)}>
                  <FileText size={14} /> View in History
                </button>
              )}
              <button className={styles.actionBtn} onClick={handleCopy}>
                {copied ? <CheckCheck size={14} /> : <Copy size={14} />}
                {copied ? 'Copied' : 'Copy Text'}
              </button>
              <button className={styles.actionBtn} onClick={handleDocx}>
                <FileText size={14} /> Export DOCX
              </button>
              <button className={`${styles.actionBtn} ${styles.actionPrimary}`} onClick={handlePdf}>
                <Download size={14} /> Export PDF
              </button>
            </div>
          </div>

          <div className={styles.reportHeader}>
            <div className={styles.resultTitle}>
              {result.title}
              {result.aiProvider ? (
                <span className={styles.aiProviderBadge} data-provider={result.aiProvider.toLowerCase()}>
                  {result.aiProvider === 'Groq' && '🤖 AI: Groq'}
                  {result.aiProvider === 'HuggingFace' && '🤖 AI: Hugging Face'}
                  {result.aiProvider === 'TogetherAI' && '🤖 AI: Together AI'}
                  {result.aiProvider === 'Heuristic' && '🧠 Smart Analysis'}
                </span>
              ) : (
                <span className={styles.aiProviderBadge} data-provider="heuristic">
                  🧠 Smart Analysis
                </span>
              )}
            </div>
            <div className={styles.resultMeta}>
              {result.authorName && <span>{result.authorName}</span>}
              {result.department && <span>· {result.department}</span>}
              {result.period && <span>· {result.period}</span>}
            </div>
          </div>

          <div className={styles.metricsStrip}>
            <Metric label="Total Activities" value={result.metrics.totalActivities} color="#1e40af" />
            <Metric label="Completed"        value={result.metrics.completedCount}   color="#10b981" />
            <Metric label="In Progress"      value={result.metrics.inProgressCount}  color="#f59e0b" />
            <Metric label="Pending"          value={result.metrics.pendingCount}      color="#ef4444" />
            <Metric label="Completion Rate"  value={`${result.metrics.completionRate}%`} color="#8b5cf6" />
          </div>

          {/* Charts — wrapped in refs for html2canvas capture */}
          <div className={styles.chartsRow}>
            <div className={styles.chartCard} id="activity-chart">
              <div className={styles.chartTitle}>Activities by Category</div>
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={result.activityChart} margin={{ top: 4, right: 8, left: -20, bottom: 60 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                  <XAxis dataKey="label" tick={{ fontSize: 10 }} angle={-35} textAnchor="end" interval={0} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 10 }} />
                  <Tooltip contentStyle={tooltipStyle} />
                  <Bar dataKey="value" radius={[4, 4, 0, 0]}>
                    {result.activityChart.map((d, i) => <Cell key={i} fill={d.color} />)}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </div>

            <div className={styles.chartCard} id="status-chart">
              <div className={styles.chartTitle}>Status Breakdown</div>
              <ResponsiveContainer width="100%" height={220}>
                <PieChart>
                  <Pie
                    data={result.statusChart.filter(d => d.value > 0)}
                    cx="50%" cy="45%"
                    innerRadius={50} outerRadius={80}
                    dataKey="value" paddingAngle={3}
                  >
                    {result.statusChart.filter(d => d.value > 0).map((d, i) => (
                      <Cell key={i} fill={d.color} />
                    ))}
                  </Pie>
                  <Tooltip contentStyle={tooltipStyle} />
                  <Legend iconType="circle" iconSize={8} wrapperStyle={{ fontSize: 11 }} />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>

          <div className={styles.section}>
            <div className={styles.sectionLabel}>Executive Summary</div>
            <p className={styles.summaryText}>{result.executiveSummary}</p>
          </div>

          <div className={styles.divider} />

          <div className={styles.sectionLabel}>Key Activities</div>
          <div className={styles.groups}>
            {result.activityGroups.map(group => (
              <div key={group.category} className={styles.group}>
                <button className={styles.groupHeader} onClick={() => toggleGroup(group.category)}>
                  <span className={styles.groupDot} />
                  <span className={styles.groupName}>{group.category}</span>
                  <span className={styles.groupCount}>{group.items.length}</span>
                  {collapsed[group.category] ? <ChevronDown size={14} /> : <ChevronUp size={14} />}
                </button>
                {!collapsed[group.category] && (
                  <ul className={styles.groupItems}>
                    {group.items.map((item, i) => (
                      <li key={i} className={styles.groupItem}>
                        <span className={styles.bullet} />
                        {item}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            ))}
          </div>

          {result.conclusion && (
            <>
              <div className={styles.divider} />
              <div className={styles.sectionLabel}>Conclusion</div>
              <p className={styles.summaryText}>{result.conclusion}</p>
            </>
          )}

          <div className={styles.divider} />
          <div className={styles.sectionLabel}>Formatted Report Output</div>
          <div className={styles.formatted}>
            <ReactMarkdown remarkPlugins={[remarkGfm]}>{result.formattedText}</ReactMarkdown>
          </div>
        </div>
      )}
    </main>
  )
}

function Metric({ label, value, color }) {
  return (
    <div className={styles.metric}>
      <div className={styles.metricValue} style={{ color }}>{value}</div>
      <div className={styles.metricLabel}>{label}</div>
    </div>
  )
}
