namespace AutoReportGenerator.DTOs;

public class ParseRequest
{
    public string RawText { get; set; } = string.Empty;
    public string TemplateType { get; set; } = "daily";
}

public class ParsedReportData
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string TimeIn { get; set; } = "08:00";
    public string TimeOut { get; set; } = "17:00";
    public string Notes { get; set; } = string.Empty;
    public string TemplateType { get; set; } = "daily";
    public List<TaskItem> Tasks { get; set; } = new();
}
