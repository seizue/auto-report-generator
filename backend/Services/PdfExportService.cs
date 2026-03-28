using AutoReportGenerator.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoReportGenerator.Services;

public class PdfExportService
{
    public PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Export(Report report)
    {
        var timeIn   = report.TimeIn.ToString(@"hh\:mm");
        var timeOut  = report.TimeOut.ToString(@"hh\:mm");
        var duration = Math.Max(0, (report.TimeOut - report.TimeIn).TotalHours);

        var statusGroups = report.Items
            .GroupBy(i => i.Status)
            .Select(g => (Status: g.Key, Count: g.Count()))
            .ToList();
        var maxCount = statusGroups.Any() ? statusGroups.Max(s => s.Count) : 1;

        var statusColors = new Dictionary<string, string>
        {
            ["Completed"]   = "#10b981",
            ["In Progress"] = "#f59e0b",
            ["Pending"]     = "#ef4444"
        };

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(10));

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // Title
                    col.Item().AlignCenter()
                        .Text(GetTitle(report.TemplateType))
                        .FontSize(18).Bold().FontColor("#1e40af");

                    col.Item().AlignCenter()
                        .Text(new string('─', 55))
                        .FontColor("#cbd5e1");

                    // Info block
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); });
                        void InfoRow(string label, string value)
                        {
                            t.Cell().PaddingVertical(3).Text(label).Bold().FontColor("#64748b");
                            t.Cell().PaddingVertical(3).Text(value);
                        }
                        InfoRow("Name",       report.Name);
                        InfoRow("Department", report.Department);
                        InfoRow("Date",       report.Date.ToString("MMMM dd, yyyy"));
                        InfoRow("Time In",    timeIn);
                        InfoRow("Time Out",   timeOut);
                        InfoRow("Hours",      $"{duration:F1} hrs");
                    });

                    // Tasks heading
                    col.Item().Text("TASKS / ACCOMPLISHMENTS")
                        .FontSize(11).Bold().FontColor("#1e40af");

                    // Tasks — numbered table or bullet list depending on ListStyle
                    var isBullets = string.Equals(report.ListStyle, "bullets", StringComparison.OrdinalIgnoreCase);

                    if (isBullets)
                    {
                        col.Item().Column(taskCol =>
                        {
                            taskCol.Spacing(4);
                            foreach (var (item, idx) in report.Items.OrderBy(i => i.Order).Select((x, i) => (x, i)))
                            {
                                var sc = statusColors.GetValueOrDefault(item.Status, "#64748b");
                                var bg = idx % 2 == 0 ? "#ffffff" : "#f8fafc";
                                taskCol.Item().Background(bg).Padding(4).Row(row =>
                                {
                                    row.ConstantItem(16).Text("•").FontColor("#1e40af").Bold();
                                    row.RelativeItem().Text(item.Task);
                                    row.ConstantItem(80).AlignRight().Text(item.Status).FontColor(sc).Bold().FontSize(9);
                                });
                            }
                        });
                    }
                    else
                    {
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(24);
                                c.RelativeColumn(4);
                                c.RelativeColumn(1);
                            });

                            t.Cell().Background("#eff6ff").Padding(4).Text("#").Bold().FontColor("#1e40af");
                            t.Cell().Background("#eff6ff").Padding(4).Text("Task").Bold().FontColor("#1e40af");
                            t.Cell().Background("#eff6ff").Padding(4).Text("Status").Bold().FontColor("#1e40af");

                            foreach (var (item, idx) in report.Items.OrderBy(i => i.Order).Select((x, i) => (x, i)))
                            {
                                var bg = idx % 2 == 0 ? "#ffffff" : "#f8fafc";
                                var sc = statusColors.GetValueOrDefault(item.Status, "#64748b");
                                t.Cell().Background(bg).Padding(4).Text($"{idx + 1}");
                                t.Cell().Background(bg).Padding(4).Text(item.Task);
                                t.Cell().Background(bg).Padding(4).Text(item.Status).FontColor(sc).Bold();
                            }
                        });
                    }

                    // Status chart
                    if (statusGroups.Any())
                    {
                        col.Item().Text("TASK STATUS CHART")
                            .FontSize(11).Bold().FontColor("#1e40af");

                        col.Item().Border(1).BorderColor("#e2e8f0").Padding(12).Column(chartCol =>
                        {
                            chartCol.Spacing(6);
                            foreach (var (status, count) in statusGroups)
                            {
                                var barColor = statusColors.GetValueOrDefault(status, "#3b82f6");
                                var pct = (float)count / maxCount;

                                chartCol.Item().Row(row =>
                                {
                                    // Label
                                    row.ConstantItem(90)
                                        .AlignMiddle()
                                        .Text(status).FontSize(9).FontColor("#64748b");

                                    // Bar track (full width background)
                                    row.RelativeItem().AlignMiddle().Height(14).Row(barRow =>
                                    {
                                        // Filled portion
                                        barRow.RelativeItem(pct)
                                            .Background(barColor)
                                            .Height(14);

                                        // Empty portion
                                        if (pct < 1f)
                                            barRow.RelativeItem(1f - pct)
                                                .Background("#f1f5f9")
                                                .Height(14);
                                    });

                                    // Count
                                    row.ConstantItem(28)
                                        .AlignMiddle()
                                        .AlignRight()
                                        .Text($"{count}").FontSize(9).Bold();
                                });
                            }
                        });
                    }

                    // Notes
                    if (!string.IsNullOrWhiteSpace(report.Notes))
                    {
                        col.Item().Text("NOTES").FontSize(11).Bold().FontColor("#1e40af");
                        col.Item().Background("#f8fafc").Padding(8)
                            .Text(report.Notes).FontColor("#475569");
                    }

                    // Footer
                    col.Item().PaddingTop(10)
                        .BorderTop(1).BorderColor("#e2e8f0")
                        .PaddingTop(6)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text($"Prepared by: {report.Name}").Bold();
                            row.RelativeItem().AlignRight()
                                .Text($"Generated: {DateTime.Now:MMM dd, yyyy}")
                                .FontColor("#94a3b8");
                        });
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static string GetTitle(string type) => type.ToLower() switch
    {
        "weekly"  => "WEEKLY SUMMARY REPORT",
        "worklog" => "WORK LOG REPORT",
        _         => "DAILY ACCOMPLISHMENT REPORT"
    };
}
