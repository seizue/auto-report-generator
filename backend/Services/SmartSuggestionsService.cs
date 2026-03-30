using AutoReportGenerator.Data;
using AutoReportGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoReportGenerator.Services;

/// <summary>
/// Service for providing smart suggestions and AI enhancements based on user history
/// </summary>
public class SmartSuggestionsService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SmartSuggestionsService> _logger;

    public SmartSuggestionsService(AppDbContext db, ILogger<SmartSuggestionsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Get task suggestions based on user's historical data
    /// </summary>
    public async Task<List<string>> GetTaskSuggestionsAsync(string employeeName, int limit = 10)
    {
        try
        {
            var recentReports = await _db.Reports
                .Where(r => r.Name == employeeName)
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .Include(r => r.Items)
                .ToListAsync();

            if (!recentReports.Any())
                return new List<string>();

            // Get task frequency
            var taskFrequency = recentReports
                .SelectMany(r => r.Items)
                .GroupBy(i => i.Task.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Task = g.Key, Count = g.Count(), LastUsed = g.Max(i => i.Report?.CreatedAt ?? DateTime.MinValue) })
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.LastUsed)
                .Take(limit)
                .Select(x => x.Task)
                .ToList();

            return taskFrequency;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task suggestions for {EmployeeName}", employeeName);
            return new List<string>();
        }
    }

    /// <summary>
    /// Analyze productivity patterns
    /// </summary>
    public async Task<ProductivityInsights> GetProductivityInsightsAsync(string employeeName)
    {
        try
        {
            var reports = await _db.Reports
                .Where(r => r.Name == employeeName)
                .Include(r => r.Items)
                .OrderByDescending(r => r.CreatedAt)
                .Take(30)
                .ToListAsync();

            if (!reports.Any())
                return new ProductivityInsights();

            var totalTasks = reports.Sum(r => r.Items.Count);
            var completedTasks = reports.Sum(r => r.Items.Count(i => i.Status == "Completed"));
            var inProgressTasks = reports.Sum(r => r.Items.Count(i => i.Status == "In Progress"));
            var pendingTasks = reports.Sum(r => r.Items.Count(i => i.Status == "Pending"));

            var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;
            var avgTasksPerReport = reports.Any() ? (double)totalTasks / reports.Count : 0;

            // Most productive day of week
            var dayOfWeekStats = reports
                .GroupBy(r => r.Date.DayOfWeek)
                .Select(g => new { Day = g.Key, CompletionRate = g.Average(r => r.Items.Any() ? (double)r.Items.Count(i => i.Status == "Completed") / r.Items.Count * 100 : 0) })
                .OrderByDescending(x => x.CompletionRate)
                .FirstOrDefault();

            // Common task categories (simple keyword extraction)
            var taskKeywords = reports
                .SelectMany(r => r.Items)
                .SelectMany(i => ExtractKeywords(i.Task))
                .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { Keyword = g.Key, Count = g.Count() })
                .ToList();

            return new ProductivityInsights
            {
                TotalReports = reports.Count,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                PendingTasks = pendingTasks,
                CompletionRate = completionRate,
                AverageTasksPerReport = avgTasksPerReport,
                MostProductiveDay = dayOfWeekStats?.Day.ToString() ?? "N/A",
                TopTaskCategories = taskKeywords.Select(k => k.Keyword).ToList(),
                RecentTrend = CalculateTrend(reports)
            };
        }
        catch (Exception ex)
        {
            var safeName = employeeName.ReplaceLineEndings(" ").Trim();
            _logger.LogError(ex, "Failed to get productivity insights for {EmployeeName}", safeName);
            return new ProductivityInsights();
        }
    }

    /// <summary>
    /// Smart categorization of tasks
    /// </summary>
    public string CategorizeTask(string task)
    {
        var taskLower = task.ToLower();

        if (taskLower.Contains("meeting") || taskLower.Contains("call") || taskLower.Contains("discussion"))
            return "Communication";
        
        if (taskLower.Contains("code") || taskLower.Contains("develop") || taskLower.Contains("implement") || taskLower.Contains("program"))
            return "Development";
        
        if (taskLower.Contains("test") || taskLower.Contains("debug") || taskLower.Contains("fix") || taskLower.Contains("bug"))
            return "Testing & QA";
        
        if (taskLower.Contains("review") || taskLower.Contains("analyze") || taskLower.Contains("research"))
            return "Analysis";
        
        if (taskLower.Contains("document") || taskLower.Contains("write") || taskLower.Contains("report"))
            return "Documentation";
        
        if (taskLower.Contains("deploy") || taskLower.Contains("release") || taskLower.Contains("build"))
            return "Deployment";
        
        if (taskLower.Contains("design") || taskLower.Contains("ui") || taskLower.Contains("ux"))
            return "Design";
        
        if (taskLower.Contains("plan") || taskLower.Contains("organize") || taskLower.Contains("schedule"))
            return "Planning";

        return "General";
    }

    private List<string> ExtractKeywords(string task)
    {
        var commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "from", "as", "is", "was", "are", "were", "be", "been", "being"
        };

        return task
            .Split(new[] { ' ', ',', '.', ';', ':', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3 && !commonWords.Contains(w))
            .Take(3)
            .ToList();
    }

    private string CalculateTrend(List<Report> reports)
    {
        if (reports.Count < 2) return "Stable";

        var recentReports = reports.Take(5).ToList();
        var olderReports = reports.Skip(5).Take(5).ToList();

        if (!olderReports.Any()) return "Stable";

        var recentCompletionRate = recentReports.Average(r => r.Items.Any() ? (double)r.Items.Count(i => i.Status == "Completed") / r.Items.Count * 100 : 0);
        var olderCompletionRate = olderReports.Average(r => r.Items.Any() ? (double)r.Items.Count(i => i.Status == "Completed") / r.Items.Count * 100 : 0);

        var diff = recentCompletionRate - olderCompletionRate;

        if (diff > 10) return "Improving";
        if (diff < -10) return "Declining";
        return "Stable";
    }
}

public class ProductivityInsights
{
    public int TotalReports { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int PendingTasks { get; set; }
    public double CompletionRate { get; set; }
    public double AverageTasksPerReport { get; set; }
    public string MostProductiveDay { get; set; } = string.Empty;
    public List<string> TopTaskCategories { get; set; } = new();
    public string RecentTrend { get; set; } = string.Empty;
}
