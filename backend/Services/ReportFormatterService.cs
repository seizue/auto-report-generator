using AutoReportGenerator.DTOs;
using AutoReportGenerator.Models;

namespace AutoReportGenerator.Services;

public class ReportFormatterService
{
    public string Format(Report report)
    {
        return report.TemplateType.ToLower() switch
        {
            "weekly" => FormatWeekly(report),
            "worklog" => FormatWorkLog(report),
            _ => FormatDaily(report)
        };
    }

    private string FormatDaily(Report report)
    {
        var tasks = report.Items.OrderBy(i => i.Order).ToList();
        var completed = tasks.Count(t => t.Status == "Completed");
        var inProgress = tasks.Count(t => t.Status == "In Progress");
        var pending = tasks.Count(t => t.Status == "Pending");
        var completionRate = tasks.Count > 0 ? (completed * 100.0 / tasks.Count) : 0;

        var taskLines = tasks
            .Select((item, idx) => FormatTaskLine(item, idx, report.ListStyle))
            .ToList();

        var timeIn  = report.TimeIn.ToString(@"hh\:mm");
        var timeOut = report.TimeOut.ToString(@"hh\:mm");
        var duration = report.TimeOut - report.TimeIn;

        var performanceSummary = completionRate >= 90 
            ? "Excellent performance with outstanding task completion."
            : completionRate >= 70 
            ? "Good progress with most tasks completed successfully."
            : completionRate >= 50
            ? "Moderate progress, several tasks still in progress."
            : "Limited completion, focus needed on pending tasks.";

        return $"""
            ═══════════════════════════════════════════════════════════════
                        DAILY ACCOMPLISHMENT REPORT
            ═══════════════════════════════════════════════════════════════

            EMPLOYEE INFORMATION
            ────────────────────────────────────────────────────────────────
            Name       : {report.Name}
            Department : {report.Department}
            Date       : {report.Date:MMMM dd, yyyy (dddd)}
            Time In    : {timeIn} AM
            Time Out   : {timeOut} PM
            Duration   : {duration.TotalHours:F1} hours

            PERFORMANCE METRICS
            ────────────────────────────────────────────────────────────────
            Total Tasks      : {tasks.Count}
            Completed        : {completed} ({completionRate:F0}%)
            In Progress      : {inProgress}
            Pending          : {pending}
            Completion Rate  : {new string('█', (int)(completionRate / 5))}{new string('░', 20 - (int)(completionRate / 5))} {completionRate:F0}%

            TASKS ACCOMPLISHED
            ────────────────────────────────────────────────────────────────
            {string.Join("\n", taskLines)}

            SUMMARY
            ────────────────────────────────────────────────────────────────
            {performanceSummary}

            NOTES & REMARKS
            ────────────────────────────────────────────────────────────────
            {(string.IsNullOrWhiteSpace(report.Notes) ? "No additional notes." : report.Notes)}

            ═══════════════════════════════════════════════════════════════
            Prepared by: {report.Name}
            Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}
            ═══════════════════════════════════════════════════════════════
            """;
    }

    private string FormatWeekly(Report report)
    {
        var tasks = report.Items.OrderBy(i => i.Order).ToList();
        var completed = tasks.Count(t => t.Status == "Completed");
        var inProgress = tasks.Count(t => t.Status == "In Progress");
        var pending = tasks.Count(t => t.Status == "Pending");
        var completionRate = tasks.Count > 0 ? (completed * 100.0 / tasks.Count) : 0;

        var taskLines = tasks
            .Select((item, idx) => FormatTaskLine(item, idx, report.ListStyle))
            .ToList();

        var weekStart = report.Date.AddDays(-(int)report.Date.DayOfWeek + 1);
        var weekEnd   = weekStart.AddDays(4);

        var weeklyInsight = completionRate >= 80
            ? "Strong weekly performance with consistent task completion across all days."
            : completionRate >= 60
            ? "Solid weekly progress with good momentum on key deliverables."
            : completionRate >= 40
            ? "Moderate weekly output, consider prioritizing high-impact tasks."
            : "Week requires attention, focus on completing pending items.";

        return $"""
            ═══════════════════════════════════════════════════════════════
                          WEEKLY SUMMARY REPORT
            ═══════════════════════════════════════════════════════════════

            REPORTING PERIOD
            ────────────────────────────────────────────────────────────────
            Employee   : {report.Name}
            Department : {report.Department}
            Week Of    : {weekStart:MMMM dd} – {weekEnd:MMMM dd, yyyy}
            Report Date: {report.Date:MMMM dd, yyyy}

            WEEKLY METRICS
            ────────────────────────────────────────────────────────────────
            Total Activities : {tasks.Count}
            Completed        : {completed} tasks
            In Progress      : {inProgress} tasks
            Pending          : {pending} tasks
            Success Rate     : {completionRate:F0}%
            
            Progress Bar: {new string('█', (int)(completionRate / 5))}{new string('░', 20 - (int)(completionRate / 5))} {completionRate:F0}%

            ACCOMPLISHMENTS & ACTIVITIES
            ────────────────────────────────────────────────────────────────
            {string.Join("\n", taskLines)}

            WEEKLY INSIGHTS
            ────────────────────────────────────────────────────────────────
            {weeklyInsight}

            KEY HIGHLIGHTS
            ────────────────────────────────────────────────────────────────
            ✓ Completed {completed} out of {tasks.Count} planned activities
            ✓ {inProgress} task{(inProgress != 1 ? "s" : "")} currently in progress
            {(pending > 0 ? $"⚠ {pending} task{(pending != 1 ? "s" : "")} pending attention" : "✓ No pending tasks")}

            ADDITIONAL NOTES
            ────────────────────────────────────────────────────────────────
            {(string.IsNullOrWhiteSpace(report.Notes) ? "No additional notes for this week." : report.Notes)}

            ═══════════════════════════════════════════════════════════════
            Prepared by: {report.Name}
            Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}
            ═══════════════════════════════════════════════════════════════
            """;
    }

    private string FormatWorkLog(Report report)
    {
        var tasks = report.Items.OrderBy(i => i.Order).ToList();
        var completed = tasks.Count(t => t.Status == "Completed");
        var inProgress = tasks.Count(t => t.Status == "In Progress");
        var pending = tasks.Count(t => t.Status == "Pending");

        var taskLines = tasks
            .Select((item, idx) => FormatTaskLine(item, idx, report.ListStyle))
            .ToList();

        var duration = report.TimeOut - report.TimeIn;
        var timeIn   = report.TimeIn.ToString(@"hh\:mm");
        var timeOut  = report.TimeOut.ToString(@"hh\:mm");

        var productivity = tasks.Count > 0 ? (tasks.Count / duration.TotalHours) : 0;
        var productivityRating = productivity >= 2 
            ? "High productivity - multiple tasks per hour"
            : productivity >= 1 
            ? "Good productivity - steady task completion"
            : productivity >= 0.5
            ? "Moderate productivity - focus on efficiency"
            : "Low productivity - consider time management strategies";

        return $"""
            ═══════════════════════════════════════════════════════════════
                            WORK LOG REPORT
            ═══════════════════════════════════════════════════════════════

            WORK SESSION DETAILS
            ────────────────────────────────────────────────────────────────
            Employee   : {report.Name}
            Department : {report.Department}
            Date       : {report.Date:MMMM dd, yyyy (dddd)}
            Time In    : {timeIn} AM
            Time Out   : {timeOut} PM
            Duration   : {duration.TotalHours:F2} hours ({(int)duration.TotalMinutes} minutes)

            PRODUCTIVITY METRICS
            ────────────────────────────────────────────────────────────────
            Total Entries    : {tasks.Count}
            Completed        : {completed}
            In Progress      : {inProgress}
            Pending          : {pending}
            Tasks/Hour       : {productivity:F2}
            Efficiency       : {productivityRating}

            WORK LOG ENTRIES
            ────────────────────────────────────────────────────────────────
            {string.Join("\n", taskLines)}

            TIME ALLOCATION SUMMARY
            ────────────────────────────────────────────────────────────────
            • Completed Tasks    : {completed} ({(tasks.Count > 0 ? completed * 100.0 / tasks.Count : 0):F0}%)
            • Ongoing Work       : {inProgress} ({(tasks.Count > 0 ? inProgress * 100.0 / tasks.Count : 0):F0}%)
            • Pending Items      : {pending} ({(tasks.Count > 0 ? pending * 100.0 / tasks.Count : 0):F0}%)

            PRODUCTIVITY ANALYSIS
            ────────────────────────────────────────────────────────────────
            {productivityRating}
            Average time per task: {(tasks.Count > 0 ? duration.TotalMinutes / tasks.Count : 0):F1} minutes

            REMARKS & OBSERVATIONS
            ────────────────────────────────────────────────────────────────
            {(string.IsNullOrWhiteSpace(report.Notes) ? "No additional remarks." : report.Notes)}

            ═══════════════════════════════════════════════════════════════
            Prepared by: {report.Name}
            Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}
            ═══════════════════════════════════════════════════════════════
            """;
    }

    private static string FormatTaskLine(ReportItem item, int idx, string listStyle)
    {
        var text = Formalize(item.Task);
        return string.Equals(listStyle, "bullets", StringComparison.OrdinalIgnoreCase)
            ? $"  • {text} — {item.Status}"
            : $"  {idx + 1}. {text} — {item.Status}";
    }

    // Converts informal bullet text into a more formal sentence
    private static string Formalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var trimmed = input.TrimStart('-', '*', '•', ' ');
        trimmed = char.ToUpper(trimmed[0]) + trimmed[1..];
        if (!trimmed.EndsWith('.') && !trimmed.EndsWith('!') && !trimmed.EndsWith('?'))
            trimmed += ".";
        return trimmed;
    }
}
