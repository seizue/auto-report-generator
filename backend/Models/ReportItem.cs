namespace AutoReportGenerator.Models;

public class ReportItem
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string Task { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed"; // Completed | In Progress | Pending
    public int Order { get; set; }
    public Report? Report { get; set; }
}
