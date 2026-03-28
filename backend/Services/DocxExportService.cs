using AutoReportGenerator.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AutoReportGenerator.Services;

public class DocxExportService
{
    public byte[] Export(Report report)
    {
        var timeIn  = report.TimeIn.ToString(@"hh\:mm");
        var timeOut = report.TimeOut.ToString(@"hh\:mm");
        var duration = Math.Max(0, (report.TimeOut - report.TimeIn).TotalHours);

        using var ms = new MemoryStream();
        using var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Page margins
        var sectPr = new SectionProperties(new PageMargin
        {
            Top = 720, Bottom = 720, Left = 900, Right = 900
        });

        // Title
        body.AppendChild(CreateHeading(GetTitle(report.TemplateType)));
        body.AppendChild(CreateParagraph(""));

        // Info table
        body.AppendChild(CreateInfoTable(new[]
        {
            ("Name",       report.Name),
            ("Department", report.Department),
            ("Date",       report.Date.ToString("MMMM dd, yyyy")),
            ("Time In",    timeIn),
            ("Time Out",   timeOut),
            ("Hours",      $"{duration:F1} hrs"),
        }));

        body.AppendChild(CreateParagraph(""));
        body.AppendChild(CreateHeading2("TASKS / ACCOMPLISHMENTS"));

        // Tasks — respect ListStyle
        body.AppendChild(CreateTasksTable(report.Items.OrderBy(i => i.Order).ToList(), report.ListStyle));

        body.AppendChild(CreateParagraph(""));
        body.AppendChild(CreateHeading2("TASK STATUS CHART"));

        // Status chart (visual bar using shaded cells)
        var statusGroups = report.Items
            .GroupBy(i => i.Status)
            .Select(g => (Status: g.Key, Count: g.Count()))
            .ToList();
        body.AppendChild(CreateStatusChart(statusGroups));

        // Notes
        if (!string.IsNullOrWhiteSpace(report.Notes))
        {
            body.AppendChild(CreateParagraph(""));
            body.AppendChild(CreateHeading2("NOTES"));
            body.AppendChild(CreateParagraph(report.Notes));
        }

        body.AppendChild(CreateParagraph(""));
        body.AppendChild(CreateParagraph($"Prepared by: {report.Name}", bold: true));
        body.AppendChild(CreateParagraph($"Generated: {DateTime.Now:MMMM dd, yyyy}", color: "94A3B8"));

        body.AppendChild(sectPr);
        mainPart.Document.Save();
        doc.Dispose();
        return ms.ToArray();
    }

    private static Table CreateInfoTable(IEnumerable<(string Label, string Value)> rows)
    {
        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableBorders(
                new TopBorder    { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new LeftBorder   { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new RightBorder  { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new InsideVerticalBorder   { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" }
            ),
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
        ));

        foreach (var (label, value) in rows)
        {
            var row = new TableRow();
            row.AppendChild(CreateTableCell(label, bold: true, bgColor: "F8FAFC", width: 1500));
            row.AppendChild(CreateTableCell(value));
            table.AppendChild(row);
        }
        return table;
    }

    private static Table CreateTasksTable(List<ReportItem> items, string listStyle)
    {
        var isBullets = string.Equals(listStyle, "bullets", StringComparison.OrdinalIgnoreCase);
        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableBorders(
                new TopBorder    { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new LeftBorder   { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new RightBorder  { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                new InsideVerticalBorder   { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" }
            ),
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
        ));

        // Header row
        var header = new TableRow();
        if (!isBullets)
            header.AppendChild(CreateTableCell("#", bold: true, bgColor: "EFF6FF", color: "1E40AF", width: 400));
        else
            header.AppendChild(CreateTableCell("•", bold: true, bgColor: "EFF6FF", color: "1E40AF", width: 400));
        header.AppendChild(CreateTableCell("Task",   bold: true, bgColor: "EFF6FF", color: "1E40AF"));
        header.AppendChild(CreateTableCell("Status", bold: true, bgColor: "EFF6FF", color: "1E40AF", width: 1400));
        table.AppendChild(header);

        foreach (var (item, idx) in items.Select((x, i) => (x, i)))
        {
            var bg = idx % 2 == 0 ? "FFFFFF" : "F8FAFC";
            var statusColor = item.Status switch
            {
                "Completed"   => "10B981",
                "In Progress" => "F59E0B",
                "Pending"     => "EF4444",
                _             => "64748B"
            };
            var marker = isBullets ? "•" : $"{idx + 1}";
            var row = new TableRow();
            row.AppendChild(CreateTableCell(marker, bgColor: bg, width: 400,
                color: isBullets ? "1E40AF" : null, bold: isBullets));
            row.AppendChild(CreateTableCell(item.Task, bgColor: bg));
            row.AppendChild(CreateTableCell(item.Status, bgColor: bg, color: statusColor, bold: true, width: 1400));
            table.AppendChild(row);
        }
        return table;
    }

    private static Table CreateStatusChart(List<(string Status, int Count)> groups)
    {
        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
        ));

        if (!groups.Any()) return table;

        var max = groups.Max(g => g.Count);
        var totalCols = 20; // bar resolution

        foreach (var (status, count) in groups)
        {
            var filled = (int)Math.Round((double)count / max * totalCols);
            var statusColor = status switch
            {
                "Completed"   => "10B981",
                "In Progress" => "F59E0B",
                "Pending"     => "EF4444",
                _             => "3B82F6"
            };

            var row = new TableRow();

            // Label cell
            row.AppendChild(CreateTableCell(status, width: 1400, color: "64748B"));

            // Bar cells (filled)
            for (int i = 0; i < totalCols; i++)
            {
                var cellBg = i < filled ? statusColor : "F1F5F9";
                var cell = new TableCell(
                    new TableCellProperties(
                        new TableCellWidth { Width = "150", Type = TableWidthUnitValues.Dxa },
                        new Shading { Fill = cellBg, Val = ShadingPatternValues.Clear }
                    ),
                    new Paragraph(new ParagraphProperties(
                        new SpacingBetweenLines { Before = "0", After = "0" }
                    ))
                );
                row.AppendChild(cell);
            }

            // Count label
            row.AppendChild(CreateTableCell($"{count}", width: 400, bold: true));
            table.AppendChild(row);
        }
        return table;
    }

    private static TableCell CreateTableCell(string text, bool bold = false, string? bgColor = null,
        string? color = null, int? width = null)
    {
        var runProps = new RunProperties(new FontSize { Val = "20" });
        if (bold) runProps.AppendChild(new Bold());
        if (color != null) runProps.AppendChild(new Color { Val = color });

        var cellProps = new TableCellProperties();
        if (bgColor != null)
            cellProps.AppendChild(new Shading { Fill = bgColor, Val = ShadingPatternValues.Clear });
        if (width.HasValue)
            cellProps.AppendChild(new TableCellWidth { Width = width.Value.ToString(), Type = TableWidthUnitValues.Dxa });

        cellProps.AppendChild(new TableCellMargin(
            new TopMargin    { Width = "60",  Type = TableWidthUnitValues.Dxa },
            new BottomMargin { Width = "60",  Type = TableWidthUnitValues.Dxa },
            new LeftMargin   { Width = "100", Type = TableWidthUnitValues.Dxa },
            new RightMargin  { Width = "100", Type = TableWidthUnitValues.Dxa }
        ));

        var run = new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        var para = new Paragraph(new ParagraphProperties(
            new SpacingBetweenLines { Before = "0", After = "0" }
        ), run);

        var cell = new TableCell(cellProps, para);
        return cell;
    }

    private static Paragraph CreateHeading(string text)
    {
        var run = new Run(
            new RunProperties(new Bold(), new FontSize { Val = "32" }, new Color { Val = "1E40AF" }),
            new Text(text));
        return new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            run);
    }

    private static Paragraph CreateHeading2(string text)
    {
        var run = new Run(
            new RunProperties(new Bold(), new FontSize { Val = "22" }, new Color { Val = "1E40AF" }),
            new Text(text));
        return new Paragraph(run);
    }

    private static Paragraph CreateParagraph(string text, bool bold = false, string? color = null)
    {
        var runProps = new RunProperties(new FontSize { Val = "20" });
        if (bold) runProps.AppendChild(new Bold());
        if (color != null) runProps.AppendChild(new Color { Val = color });
        var run = new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return new Paragraph(new ParagraphProperties(
            new SpacingBetweenLines { Before = "0", After = "80" }
        ), run);
    }

    private static string GetTitle(string type) => type.ToLower() switch
    {
        "weekly"  => "WEEKLY SUMMARY REPORT",
        "worklog" => "WORK LOG REPORT",
        _         => "DAILY ACCOMPLISHMENT REPORT"
    };
}
