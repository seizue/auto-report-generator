namespace AutoReportGenerator.DTOs;

public class ReportRequest
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string TimeIn { get; set; } = "08:00";
    public string TimeOut { get; set; } = "17:00";
    public string Notes { get; set; } = string.Empty;
    public string TemplateType { get; set; } = "daily";
    public string ListStyle { get; set; } = "numbered"; // "numbered" | "bullets"
    public List<TaskItem> Tasks { get; set; } = new();
}

public class TaskItem
{
    public string Task { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
}
