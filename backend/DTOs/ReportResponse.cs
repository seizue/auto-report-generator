namespace AutoReportGenerator.DTOs;

public class ReportResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string TimeIn { get; set; } = string.Empty;
    public string TimeOut { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string ListStyle { get; set; } = "numbered";
    public string FormattedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<TaskItemResponse> Tasks { get; set; } = new();
}

public class TaskItemResponse
{
    public string Task { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
