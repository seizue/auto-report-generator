import { useState, useEffect } from 'react'
import { Sparkles, TrendingUp, Plus } from 'lucide-react'
import { getTaskSuggestions, getProductivityInsights } from '../services/api'
import styles from './SmartSuggestions.module.css'

export default function SmartSuggestions({ employeeName, onAddTask }) {
  const [suggestions, setSuggestions] = useState([])
  const [insights, setInsights] = useState(null)
  const [loading, setLoading] = useState(false)
  const [showInsights, setShowInsights] = useState(false)

  useEffect(() => {
    if (employeeName) {
      loadSuggestions()
    }
  }, [employeeName])

  const loadSuggestions = async () => {
    if (!employeeName) return
    
    setLoading(true)
    try {
      const [suggestionsRes, insightsRes] = await Promise.all([
        getTaskSuggestions(employeeName, 5),
        getProductivityInsights(employeeName)
      ])
      setSuggestions(suggestionsRes.data.suggestions || [])
      setInsights(insightsRes.data)
    } catch (error) {
      console.error('Failed to load suggestions:', error)
    } finally {
      setLoading(false)
    }
  }

  if (!employeeName || loading) return null

  if (suggestions.length === 0 && !insights?.totalReports) return null

  return (
    <div className={styles.container}>
      {suggestions.length > 0 && (
        <div className={styles.section}>
          <div className={styles.header}>
            <Sparkles size={18} />
            <h3>Suggested Tasks</h3>
            <span className={styles.badge}>Based on your history</span>
          </div>
          <div className={styles.suggestions}>
            {suggestions.map((task, idx) => (
              <button
                key={idx}
                className={styles.suggestion}
                onClick={() => onAddTask(task)}
                title="Click to add this task"
              >
                <span>{task}</span>
                <Plus size={16} />
              </button>
            ))}
          </div>
        </div>
      )}

      {insights && insights.totalReports > 0 && (
        <div className={styles.section}>
          <div className={styles.header}>
            <TrendingUp size={18} />
            <h3>Productivity Insights</h3>
            <button 
              className={styles.toggleBtn}
              onClick={() => setShowInsights(!showInsights)}
            >
              {showInsights ? 'Hide' : 'Show'}
            </button>
          </div>

          {showInsights && (
            <div className={styles.insights}>
              <div className={styles.insightGrid}>
                <div className={styles.insightCard}>
                  <div className={styles.insightValue}>{insights.completionRate.toFixed(1)}%</div>
                  <div className={styles.insightLabel}>Completion Rate</div>
                </div>
                <div className={styles.insightCard}>
                  <div className={styles.insightValue}>{insights.averageTasksPerReport.toFixed(1)}</div>
                  <div className={styles.insightLabel}>Avg Tasks/Report</div>
                </div>
                <div className={styles.insightCard}>
                  <div className={styles.insightValue}>{insights.mostProductiveDay}</div>
                  <div className={styles.insightLabel}>Most Productive Day</div>
                </div>
                <div className={styles.insightCard}>
                  <div className={`${styles.insightValue} ${styles[insights.recentTrend.toLowerCase()]}`}>
                    {insights.recentTrend}
                  </div>
                  <div className={styles.insightLabel}>Recent Trend</div>
                </div>
              </div>

              {insights.topTaskCategories.length > 0 && (
                <div className={styles.categories}>
                  <div className={styles.categoriesLabel}>Top Task Keywords:</div>
                  <div className={styles.categoryTags}>
                    {insights.topTaskCategories.map((cat, idx) => (
                      <span key={idx} className={styles.categoryTag}>{cat}</span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  )
}
