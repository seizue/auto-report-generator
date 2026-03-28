namespace AutoReportGenerator.DTOs;

public class SummaryReportRequest
{
    public string RawText { get; set; } = string.Empty;
    public string ReportType { get; set; } = "General Summary Report";
    public string AuthorName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty; // e.g. "March 2026" or auto-detected
    public bool? SaveToHistory { get; set; } = true; // Save to database by default
}

public class SummaryReportResult
{
    public int? Id { get; set; } // Database ID if saved
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<ActivityGroup> ActivityGroups { get; set; } = new();
    public List<ChartDataPoint> ActivityChart { get; set; } = new();
    public List<ChartDataPoint> StatusChart { get; set; } = new();
    public SummaryMetrics Metrics { get; set; } = new();
    public string Conclusion { get; set; } = string.Empty;
    public string FormattedText { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    // Base64 PNG images captured from browser charts (optional, for export)
    public string? ActivityChartImage { get; set; }
    public string? StatusChartImage { get; set; }
    // AI Enhancement metadata
    public string? AiProvider { get; set; } // "Groq", "HuggingFace", "TogetherAI", "Heuristic", or null
    public bool AiEnhanced { get; set; } = false; // True if any AI provider was used
}

public class ActivityGroup
{
    public string Category { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "#3b82f6";
}

public class SummaryMetrics
{
    public int TotalActivities { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public int PendingCount { get; set; }
    public double CompletionRate { get; set; }
    public int WordCount { get; set; }
}
