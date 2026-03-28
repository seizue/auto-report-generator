import { useState, useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { Plus, Trash2, Zap, Pencil, AlignLeft, LayoutList, Sparkles } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import toast from 'react-hot-toast'
import { generateReport, updateReport, getReport, parseText, generateSummary } from '../services/api'
import UniversalFileUpload from '../components/UniversalFileUpload'
import DraggableTaskList from '../components/DraggableTaskList'
import SmartSuggestions from '../components/SmartSuggestions'
import styles from './HomePage.module.css'

const STATUS_OPTIONS = ['Completed', 'In Progress', 'Pending']
const defaultTask = () => ({ id: crypto.randomUUID(), task: '', status: 'Completed' })

export default function HomePage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const editId = searchParams.get('edit')
  const { t } = useTranslation()

  const TEMPLATES = [
    { value: 'daily',   label: t('templates.daily') },
    { value: 'weekly',  label: t('templates.weekly') },
    { value: 'worklog', label: t('templates.worklog') },
  ]

  // mode: 'form' | 'paste'
  const [mode, setMode] = useState(editId ? 'form' : 'paste')
  const [rawText, setRawText] = useState('')
  const [parsing, setParsing] = useState(false)
  // 'idle' | 'extracted' — paste mode step
  const [pasteStep, setPasteStep] = useState('idle')

  const [loading, setLoading]         = useState(false)
  const [loadingEdit, setLoadingEdit] = useState(!!editId)

  const [form, setForm] = useState({
    name: '',
    department: '',
    date: new Date().toISOString().split('T')[0],
    timeIn: '08:00',
    timeOut: '17:00',
    templateType: 'daily',
    listStyle: 'numbered',
    notes: '',
  })
  const [tasks, setTasks] = useState([defaultTask()])

  // Load existing report when editing
  useEffect(() => {
    if (!editId) return
    getReport(editId)
      .then(({ data }) => {
        setForm({
          name:         data.name,
          department:   data.department,
          date:         data.date.split('T')[0],
          timeIn:       data.timeIn,
          timeOut:      data.timeOut,
          templateType: data.templateType,
          listStyle:    data.listStyle ?? 'numbered',
          notes:        data.notes ?? '',
        })
        setTasks(data.tasks.length
          ? data.tasks.map(t => ({ id: crypto.randomUUID(), task: t.task, status: t.status }))
          : [defaultTask()])
      })
      .catch(() => toast.error('Could not load report for editing'))
      .finally(() => setLoadingEdit(false))
  }, [editId])

  const set = (field, value) => setForm(f => ({ ...f, [field]: value }))
  const addTask    = () => setTasks(t => [...t, defaultTask()])
  const removeTask = (id) => setTasks(t => t.filter((task) => task.id !== id))
  const updateTask = (id, field, value) =>
    setTasks(t => t.map((task) => task.id === id ? { ...task, [field]: value } : task))

  // Extract structured data from pasted text
  const handleExtract = async () => {
    if (!rawText.trim()) return toast.error('Paste some text first')
    setParsing(true)
    try {
      const { data } = await parseText({ rawText, templateType: form.templateType })
      setForm(f => ({
        ...f,
        name:         data.name         || f.name,
        department:   data.department   || f.department,
        date:         data.date         || f.date,
        timeIn:       data.timeIn       || f.timeIn,
        timeOut:      data.timeOut      || f.timeOut,
        notes:        data.notes        || f.notes,
        templateType: data.templateType || f.templateType,
      }))
      setTasks(data.tasks.length ? data.tasks.map(t => ({ id: crypto.randomUUID(), ...t })) : [defaultTask()])
      setPasteStep('extracted')
      toast.success(`Extracted ${data.tasks.length} task${data.tasks.length !== 1 ? 's' : ''} — review and edit below`)
    } catch {
      toast.error('Extraction failed. Is the backend running?')
    } finally {
      setParsing(false)
    }
  }

  // Quick Generate — calls summary service, produces paragraph report, navigates to summary result
  const handleGenerateDirect = async () => {
    if (!rawText.trim()) return toast.error('Paste some text first')
    setParsing(true)
    try {
      const summaryForm = {
        rawText,
        reportType: TEMPLATES.find(t => t.value === form.templateType)?.label ?? 'General Summary Report',
        authorName: '',
        department: '',
        period: '',
        saveToHistory: true, // Save to database
      }
      const { data } = await generateSummary(summaryForm)
      toast.success('Report generated and saved to history!')
      navigate('/summary', { state: { result: data, form: summaryForm } })
    } catch {
      toast.error('Something went wrong. Is the backend running?')
    } finally {
      setParsing(false)
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.name.trim()) return toast.error(t('toast.nameRequired'))
    if (tasks.every(t => !t.task.trim())) return toast.error(t('toast.taskRequired'))

    setLoading(true)
    try {
      const payload = {
        ...form,
        tasks: tasks.filter(t => t.task.trim()).map(t => ({ task: t.task, status: t.status }))
      }
      if (editId) {
        await updateReport(editId, payload)
        toast.success(t('toast.reportUpdated'))
        navigate(`/preview/${editId}`)
      } else {
        const { data } = await generateReport(payload)
        toast.success(t('toast.reportGenerated'))
        navigate(`/preview/${data.id}`)
      }
    } catch {
      toast.error(t('toast.error'))
    } finally {
      setLoading(false)
    }
  }

  const clearForm = () => {
    setForm({
      name: '',
      department: '',
      date: new Date().toISOString().split('T')[0],
      timeIn: '08:00',
      timeOut: '17:00',
      templateType: form.templateType,
      listStyle: form.listStyle,
      notes: '',
    })
    setTasks([defaultTask()])
    toast('Form cleared', { icon: '🗑️' })
  }

  const prefill = () => {
    setForm(f => ({ ...f, name: 'Juan dela Cruz', department: 'Information Technology', timeIn: '08:00', timeOut: '17:00' }))
    setTasks([
      { id: crypto.randomUUID(), task: 'Attended morning standup meeting',    status: 'Completed' },
      { id: crypto.randomUUID(), task: 'Reviewed and merged pull requests',   status: 'Completed' },
      { id: crypto.randomUUID(), task: 'Fixed login bug reported by QA',      status: 'Completed' },
      { id: crypto.randomUUID(), task: 'Updated project documentation',       status: 'In Progress' },
    ])
    toast('Pre-filled with sample data', { icon: '📋' })
  }

  if (loadingEdit) return <div className={styles.loadingEdit}>Loading report...</div>

  return (
    <main className={styles.main}>
      <div className={styles.hero}>
        {editId ? (
          <>
            <div className={styles.editBadge}><Pencil size={14} /> {t('home.editTitle')}</div>
            <h1 className={styles.title}>{t('home.editTitle')}</h1>
            <p className={styles.subtitle}>{t('home.editSubtitle')}</p>
          </>
        ) : (
          <>
            <h1 className={styles.title}>{t('home.title')}</h1>
            <p className={styles.subtitle}>{t('home.subtitle')}</p>
          </>
        )}
      </div>

      {/* Mode toggle */}
      <div className={styles.modeToggle}>
        <button
          className={`${styles.modeBtn} ${mode === 'form' ? styles.modeActive : ''}`}
          onClick={() => { 
            setMode('form')
            setPasteStep('idle')
            // Clear extracted data when switching to form mode
            if (pasteStep === 'extracted') {
              setTasks([defaultTask()])
              setRawText('')
            }
          }}
        >
          <LayoutList size={15} /> {t('home.formInput')}
        </button>
        <button
          className={`${styles.modeBtn} ${mode === 'paste' ? styles.modeActive : ''}`}
          onClick={() => setMode('paste')}
        >
          <AlignLeft size={15} /> {t('home.pasteText')}
        </button>
      </div>

      {/* ── PASTE MODE ── */}
      {mode === 'paste' && pasteStep === 'idle' && (
        <div className={styles.card}>
          {/* Template selector */}
          <div className={styles.templateRow}>
            {TEMPLATES.map(t => (
              <button key={t.value} type="button"
                className={`${styles.templateBtn} ${form.templateType === t.value ? styles.active : ''}`}
                onClick={() => set('templateType', t.value)}
              >{t.label}</button>
            ))}
          </div>

          <div className={styles.pasteHint}>
            <Sparkles size={14} />
            Dump your raw notes — times, tasks, anything. We'll handle the rest.
          </div>

          <div className={styles.field}>
            <label>Your Raw Text</label>
            <UniversalFileUpload onExtracted={(text) => setRawText(prev => prev ? prev + '\n' + text : text)} />
            <textarea
              className={styles.pasteArea}
              value={rawText}
              onChange={e => setRawText(e.target.value)}
              placeholder={'Example:\nfixed printer then updated pc and also checked internet slow, arrived 8am left 5pm'}
              rows={8}
            />
          </div>

          <div className={styles.pasteExamples}>
            <span>Try:</span>
            {[
              'fixed printer then updated pc and also checked internet slow, arrived 8am left 5pm',
              'attended standup, reviewed PRs, deployed hotfix to production. pending: update docs',
              'met with client at 9am, prepared proposal, sent report by 4pm, left at 6pm',
            ].map((ex, i) => (
              <button key={i} className={styles.exampleChip} onClick={() => setRawText(ex)}>
                Example {i + 1}
              </button>
            ))}
            {rawText && (
              <button className={styles.clearChip} onClick={() => { setRawText(''); toast('Cleared', { icon: '🗑️' }) }}>
                Clear
              </button>
            )}
          </div>

          {/* Two clear action paths */}
          <div className={styles.pastePaths}>
            <div className={styles.pathCard}>
              <div className={styles.pathLabel}>
                <Zap size={14} /> Quick Generate
              </div>
              <p className={styles.pathDesc}>Generates a paragraph-style summary report instantly — no form, no tasks.</p>
              <button type="button" className={styles.generateBtn} onClick={handleGenerateDirect} disabled={parsing}>
                {parsing ? 'Generating...' : 'Generate Now'}
              </button>
            </div>
            <div className={styles.pathDivider}>or</div>
            <div className={styles.pathCard}>
              <div className={styles.pathLabel}>
                <Sparkles size={14} /> Extract & Review
              </div>
              <p className={styles.pathDesc}>Extract tasks from your text, review and edit them, then generate a task-based report.</p>
              <button type="button" className={styles.extractBtn} onClick={handleExtract} disabled={parsing}>
                {parsing ? 'Extracting...' : 'Extract & Edit First'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Smart Suggestions */}
      {mode === 'form' && form.name && (
        <SmartSuggestions 
          employeeName={form.name} 
          onAddTask={(task) => {
            setTasks(t => [...t, { id: crypto.randomUUID(), task, status: 'Completed' }])
            toast.success('Task added from suggestion')
          }}
        />
      )}

      {/* ── FORM (both form mode and paste extracted step) ── */}
      {(mode === 'form' || pasteStep === 'extracted') && (
        <form className={styles.card} onSubmit={handleSubmit}>

          {mode === 'paste' && pasteStep === 'extracted' && (
            <div className={styles.extractedBanner}>
              <Sparkles size={14} />
              Extracted from your text — review and edit before generating.
              <button type="button" className={styles.backToText} onClick={() => setPasteStep('idle')}>
                ← Back to text
              </button>
            </div>
          )}

          {/* Template selector */}
          <div className={styles.templateRow}>
            {TEMPLATES.map(t => (
              <button key={t.value} type="button"
                className={`${styles.templateBtn} ${form.templateType === t.value ? styles.active : ''}`}
                onClick={() => set('templateType', t.value)}
              >{t.label}</button>
            ))}
          </div>

          {/* Info fields */}
          <div className={styles.grid2}>
            <div className={styles.field}>
              <label>Full Name *</label>
              <input value={form.name} onChange={e => set('name', e.target.value)} placeholder="e.g. Juan dela Cruz" required />
            </div>
            <div className={styles.field}>
              <label>Department / Role</label>
              <input value={form.department} onChange={e => set('department', e.target.value)} placeholder="e.g. IT Department" />
            </div>
            <div className={styles.field}>
              <label>Date</label>
              <input type="date" value={form.date} onChange={e => set('date', e.target.value)} />
            </div>
            <div className={styles.grid2} style={{ gap: 12 }}>
              <div className={styles.field}>
                <label>Time In</label>
                <input type="time" value={form.timeIn} onChange={e => set('timeIn', e.target.value)} />
              </div>
              <div className={styles.field}>
                <label>Time Out</label>
                <input type="time" value={form.timeOut} onChange={e => set('timeOut', e.target.value)} />
              </div>
            </div>
          </div>

          {/* Tasks */}
          <div className={styles.section}>
            <div className={styles.sectionHeader}>
              <span className={styles.sectionTitle}>Tasks</span>
              <div className={styles.sectionHeaderRight}>
                <div className={styles.listStyleToggle}>
                  <button
                    type="button"
                    className={`${styles.listStyleBtn} ${form.listStyle === 'numbered' ? styles.listStyleActive : ''}`}
                    onClick={() => set('listStyle', 'numbered')}
                    title="Numbered list"
                  >
                    1. 2. 3.
                  </button>
                  <button
                    type="button"
                    className={`${styles.listStyleBtn} ${form.listStyle === 'bullets' ? styles.listStyleActive : ''}`}
                    onClick={() => set('listStyle', 'bullets')}
                    title="Bullet list"
                  >
                    • • •
                  </button>
                </div>
                <button type="button" className={styles.addBtn} onClick={addTask}>
                  <Plus size={14} /> {t('home.addTask')}
                </button>
              </div>
            </div>
            
            <UniversalFileUpload onExtracted={async (text) => {
              // Auto-extract tasks from uploaded file
              setParsing(true)
              try {
                const { data } = await parseText({ rawText: text, templateType: form.templateType })
                if (data.tasks.length > 0) {
                  setTasks(data.tasks.map(t => ({ id: crypto.randomUUID(), ...t })))
                  toast.success(`Extracted ${data.tasks.length} task${data.tasks.length !== 1 ? 's' : ''} from file`)
                } else {
                  toast('No tasks found, text added to notes', { icon: 'ℹ️' })
                  set('notes', form.notes ? form.notes + '\n' + text : text)
                }
              } catch {
                toast.error('Failed to extract tasks')
                set('notes', form.notes ? form.notes + '\n' + text : text)
              } finally {
                setParsing(false)
              }
            }} />
            
            <DraggableTaskList
              tasks={tasks}
              setTasks={setTasks}
              listStyle={form.listStyle}
              statusOptions={STATUS_OPTIONS}
            />
          </div>

          {/* Notes */}
          <div className={styles.field}>
            <label>Notes / Remarks</label>
            <UniversalFileUpload onExtracted={(text) => set('notes', form.notes ? form.notes + '\n' + text : text)} />
            <textarea
              value={form.notes}
              onChange={e => set('notes', e.target.value)}
              placeholder="Any additional notes..."
              rows={3}
            />
          </div>

          {/* Actions */}
          <div className={styles.actions}>
            {editId ? (
              <button type="button" className={styles.prefillBtn} onClick={() => navigate(`/preview/${editId}`)}>
                Cancel
              </button>
            ) : (
              <>
                <button type="button" className={styles.clearBtn} onClick={clearForm}>
                  Clear
                </button>
                <button type="button" className={styles.prefillBtn} onClick={prefill}>
                  Pre-fill Sample
                </button>
              </>
            )}
            <button type="submit" className={styles.generateBtn} disabled={loading}>
              {editId ? <Pencil size={16} /> : <Zap size={16} />}
              {loading
                ? (editId ? 'Updating...' : 'Generating...')
                : (editId ? 'Regenerate Report' : 'Generate Report')}
            </button>
          </div>
        </form>
      )}
    </main>
  )
}
