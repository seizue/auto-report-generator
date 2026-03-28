namespace AutoReportGenerator.Models;

public class Report
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan TimeIn { get; set; }
    public TimeSpan TimeOut { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string TemplateType { get; set; } = "daily"; // daily | weekly | worklog
    public string ListStyle { get; set; } = "numbered"; // numbered | bullets
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ReportItem> Items { get; set; } = new();
}
