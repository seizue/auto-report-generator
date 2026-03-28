import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, CartesianGrid
} from 'recharts'
import styles from './ReportCharts.module.css'

const COLORS = ['#1e40af', '#3b82f6', '#60a5fa', '#93c5fd']
const STATUS_COLORS = { Completed: '#10b981', 'In Progress': '#f59e0b', Pending: '#ef4444' }

const tooltipStyle = {
  backgroundColor: 'var(--surface)',
  border: '1px solid var(--border)',
  borderRadius: 8,
  fontSize: 12,
}

function ChartCard({ title, children }) {
  return (
    <div className={styles.card}>
      <h3 className={styles.cardTitle}>{title}</h3>
      {children}
    </div>
  )
}

export default function ReportCharts({ reports }) {
  if (!reports.length) return null

  // 1. Reports per day (last 7 days)
  const last7 = Array.from({ length: 7 }, (_, i) => {
    const d = new Date()
    d.setDate(d.getDate() - (6 - i))
    const key = d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
    const count = reports.filter(r =>
      new Date(r.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) === key
    ).length
    return { day: key, reports: count }
  })

  // 2. Template type breakdown
  const typeCounts = reports.reduce((acc, r) => {
    const label = { daily: 'Daily', weekly: 'Weekly', worklog: 'Work Log' }[r.templateType] ?? r.templateType
    acc[label] = (acc[label] || 0) + 1
    return acc
  }, {})
  const pieData = Object.entries(typeCounts).map(([name, value]) => ({ name, value }))

  // 3. Tasks per report (last 8)
  const taskData = reports.slice(0, 8).map(r => ({
    date: new Date(r.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    tasks: r.tasks?.length ?? 0,
  })).reverse()

  // 4. Hours worked per report (last 8)
  const hoursData = reports.slice(0, 8).map(r => {
    const [inH, inM] = r.timeIn.split(':').map(Number)
    const [outH, outM] = r.timeOut.split(':').map(Number)
    const hours = ((outH * 60 + outM) - (inH * 60 + inM)) / 60
    return {
      name: new Date(r.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
      hours: Math.max(0, parseFloat(hours.toFixed(1)))
    }
  }).reverse()

  // 5. Task status breakdown
  const statusCounts = reports.flatMap(r => r.tasks ?? []).reduce((acc, t) => {
    acc[t.status] = (acc[t.status] || 0) + 1
    return acc
  }, {})
  const statusData = Object.entries(statusCounts).map(([name, value]) => ({ name, value }))

  return (
    <>
      {/* Row 1: 2 wide charts */}
      <div className={styles.row2}>
        <ChartCard title="Reports Generated (Last 7 Days)">
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={last7} margin={{ top: 4, right: 8, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="day" tick={{ fontSize: 11 }} />
              <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
              <Tooltip contentStyle={tooltipStyle} />
              <Bar dataKey="reports" fill="#1e40af" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Report Type Breakdown">
          <ResponsiveContainer width="100%" height={200}>
            <PieChart>
              <Pie
                data={pieData} cx="50%" cy="50%"
                innerRadius={50} outerRadius={80}
                dataKey="value" paddingAngle={3}
                label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                labelLine={false}
              >
                {pieData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
              </Pie>
              <Tooltip contentStyle={tooltipStyle} />
            </PieChart>
          </ResponsiveContainer>
        </ChartCard>
      </div>

      {/* Row 2: 3 narrower charts */}
      <div className={styles.row3}>
        <ChartCard title="Tasks per Report (Recent 8)">
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={taskData} margin={{ top: 4, right: 8, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="date" tick={{ fontSize: 11 }} />
              <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
              <Tooltip contentStyle={tooltipStyle} />
              <Bar dataKey="tasks" fill="#3b82f6" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Hours Worked per Report (Recent 8)">
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={hoursData} margin={{ top: 4, right: 8, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip contentStyle={tooltipStyle} formatter={(v) => [`${v} hrs`, 'Hours']} />
              <Bar dataKey="hours" fill="#60a5fa" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        {statusData.length > 0 && (
          <ChartCard title="Task Status Overview">
            <ResponsiveContainer width="100%" height={200}>
              <BarChart data={statusData} layout="vertical" margin={{ top: 4, right: 16, left: 16, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" horizontal={false} />
                <XAxis type="number" allowDecimals={false} tick={{ fontSize: 11 }} />
                <YAxis type="category" dataKey="name" tick={{ fontSize: 11 }} width={80} />
                <Tooltip contentStyle={tooltipStyle} />
                <Bar dataKey="value" radius={[0, 4, 4, 0]}>
                  {statusData.map((entry, i) => (
                    <Cell key={i} fill={STATUS_COLORS[entry.name] ?? COLORS[i % COLORS.length]} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>
        )}
      </div>
    </>
  )
}
