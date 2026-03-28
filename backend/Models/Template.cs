namespace AutoReportGenerator.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // daily | weekly | worklog
    public string Description { get; set; } = string.Empty;
    public bool IsPremium { get; set; } = false;
}
