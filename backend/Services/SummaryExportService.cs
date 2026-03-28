using AutoReportGenerator.DTOs;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using OxmlColor = DocumentFormat.OpenXml.Wordprocessing.Color;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPdfDocument = QuestPDF.Fluent.Document;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace AutoReportGenerator.Services;

public class SummaryExportService
{
    public SummaryExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ── PDF ────────────────────────────────────────────────────────────────
    public byte[] ExportPdf(SummaryReportResult r)
    {
        var doc = QuestPdfDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(10));

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    // Parse and format the formatted text intelligently
                    var lines = r.FormattedText.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
                    
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        
                        // Skip empty lines but add spacing
                        if (string.IsNullOrWhiteSpace(trimmed))
                        {
                            col.Item().PaddingTop(4);
                            continue;
                        }
                        
                        // Main title (all caps with ═══)
                        if (trimmed.All(c => c == '═'))
                        {
                            col.Item().LineHorizontal(2).LineColor("#1e40af");
                            continue;
                        }
                        
                        // Section headers (ALL CAPS without special chars at start)
                        if (trimmed.Length > 0 && trimmed == trimmed.ToUpper() && 
                            !trimmed.StartsWith("•") && !trimmed.StartsWith("▸") && 
                            !trimmed.StartsWith("✓") && !trimmed.StartsWith("⚠") &&
                            trimmed.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '&' || c == '/'))
                        {
                            col.Item().PaddingTop(6).Text(trimmed)
                                .FontSize(11).Bold().FontColor("#1e40af");
                            continue;
                        }
                        
                        // Thin separator lines (────)
                        if (trimmed.All(c => c == '─'))
                        {
                            col.Item().LineHorizontal(1).LineColor("#cbd5e1");
                            continue;
                        }
                        
                        // Category headers (▸ prefix)
                        if (trimmed.StartsWith("▸"))
                        {
                            col.Item().PaddingTop(4).Text(trimmed.Substring(1).Trim())
                                .FontSize(10).Bold().FontColor("#3b82f6");
                            continue;
                        }
                        
                        // Bullet points (• prefix)
                        if (trimmed.StartsWith("•"))
                        {
                            col.Item().PaddingLeft(12).Text(trimmed)
                                .FontSize(9).LineHeight(1.5f);
                            continue;
                        }
                        
                        // Success indicators (✓ prefix)
                        if (trimmed.StartsWith("✓"))
                        {
                            col.Item().PaddingLeft(8).Text(trimmed)
                                .FontSize(9).FontColor("#10b981");
                            continue;
                        }
                        
                        // Warning indicators (⚠ prefix)
                        if (trimmed.StartsWith("⚠"))
                        {
                            col.Item().PaddingLeft(8).Text(trimmed)
                                .FontSize(9).FontColor("#f59e0b");
                            continue;
                        }
                        
                        // Key-value pairs (Label : Value)
                        if (trimmed.Contains(":") && !trimmed.Contains("http"))
                        {
                            var parts = trimmed.Split(new[] { ':' }, 2);
                            if (parts.Length == 2)
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(140).Text(parts[0].Trim() + " :")
                                        .FontSize(9).FontColor("#64748b");
                                    row.RelativeItem().Text(parts[1].Trim())
                                        .FontSize(9);
                                });
                                continue;
                            }
                        }
                        
                        // Progress bars (█ and ░ characters)
                        if (trimmed.Contains("█") || trimmed.Contains("░"))
                        {
                            col.Item().Text(trimmed)
                                .FontFamily("Courier New")
                                .FontSize(8)
                                .FontColor("#3b82f6");
                            continue;
                        }
                        
                        // Regular text
                        col.Item().Text(trimmed)
                            .FontSize(9)
                            .LineHeight(1.5f);
                    }
                    
                    // Add charts if available
                    if (!string.IsNullOrWhiteSpace(r.ActivityChartImage) || !string.IsNullOrWhiteSpace(r.StatusChartImage))
                    {
                        col.Item().PaddingTop(20);
                        col.Item().LineHorizontal(2).LineColor("#1e40af");
                        col.Item().PaddingTop(8).Text("VISUAL CHARTS")
                            .FontSize(11).Bold().FontColor("#1e40af");
                        col.Item().PaddingTop(8);
                        col.Item().Row(row =>
                        {
                            if (!string.IsNullOrWhiteSpace(r.ActivityChartImage))
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Activity Distribution").FontSize(9).Bold().FontColor("#64748b");
                                    c.Item().PaddingTop(4).Image(Convert.FromBase64String(r.ActivityChartImage.Split(',')[1]))
                                        .FitArea();
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(r.StatusChartImage))
                            {
                                row.RelativeItem().PaddingLeft(8).Column(c =>
                                {
                                    c.Item().Text("Status Distribution").FontSize(9).Bold().FontColor("#64748b");
                                    c.Item().PaddingTop(4).Image(Convert.FromBase64String(r.StatusChartImage.Split(',')[1]))
                                        .FitArea();
                                });
                            }
                        });
                    }
                });
            });
        });

        return doc.GeneratePdf();
    }

    // ── DOCX ───────────────────────────────────────────────────────────────
    public byte[] ExportDocx(SummaryReportResult r)
    {
        using var ms = new MemoryStream();
        using var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Parse and format the formatted text intelligently
        var lines = r.FormattedText.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Skip empty lines but add spacing
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                body.AppendChild(new Paragraph(new ParagraphProperties(
                    new SpacingBetweenLines { Before = "40", After = "0" })));
                continue;
            }
            
            // Main title separator (all ═══)
            if (trimmed.All(c => c == '═'))
            {
                body.AppendChild(new Paragraph(new ParagraphProperties(
                    new ParagraphBorders(new TopBorder { Val = BorderValues.Single, Size = 12, Color = "1E40AF" }),
                    new SpacingBetweenLines { Before = "80", After = "80" })));
                continue;
            }
            
            // Section headers (ALL CAPS without special chars at start)
            if (trimmed.Length > 0 && trimmed == trimmed.ToUpper() && 
                !trimmed.StartsWith("•") && !trimmed.StartsWith("▸") && 
                !trimmed.StartsWith("✓") && !trimmed.StartsWith("⚠") &&
                trimmed.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '&' || c == '/'))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { Before = "120", After = "80" }),
                    new Run(new RunProperties(
                        new Bold(), 
                        new FontSize { Val = "22" }, 
                        new OxmlColor { Val = "1E40AF" }),
                        new Text(trimmed))));
                continue;
            }
            
            // Thin separator lines (────)
            if (trimmed.All(c => c == '─'))
            {
                body.AppendChild(new Paragraph(new ParagraphProperties(
                    new ParagraphBorders(new TopBorder { Val = BorderValues.Single, Size = 4, Color = "CBD5E1" }),
                    new SpacingBetweenLines { Before = "40", After = "40" })));
                continue;
            }
            
            // Category headers (▸ prefix)
            if (trimmed.StartsWith("▸"))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { Before = "80", After = "40" }),
                    new Run(new RunProperties(
                        new Bold(), 
                        new FontSize { Val = "20" }, 
                        new OxmlColor { Val = "3B82F6" }),
                        new Text(trimmed.Substring(1).Trim()))));
                continue;
            }
            
            // Bullet points (• prefix)
            if (trimmed.StartsWith("•"))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new Indentation { Left = "360" },
                        new SpacingBetweenLines { Before = "0", After = "40", Line = "276" }),
                    new Run(new RunProperties(new FontSize { Val = "18" }),
                        new Text(trimmed) { Space = SpaceProcessingModeValues.Preserve })));
                continue;
            }
            
            // Success indicators (✓ prefix)
            if (trimmed.StartsWith("✓"))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new Indentation { Left = "240" },
                        new SpacingBetweenLines { Before = "0", After = "40" }),
                    new Run(new RunProperties(
                        new FontSize { Val = "18" }, 
                        new OxmlColor { Val = "10B981" }),
                        new Text(trimmed) { Space = SpaceProcessingModeValues.Preserve })));
                continue;
            }
            
            // Warning indicators (⚠ prefix)
            if (trimmed.StartsWith("⚠"))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new Indentation { Left = "240" },
                        new SpacingBetweenLines { Before = "0", After = "40" }),
                    new Run(new RunProperties(
                        new FontSize { Val = "18" }, 
                        new OxmlColor { Val = "F59E0B" }),
                        new Text(trimmed) { Space = SpaceProcessingModeValues.Preserve })));
                continue;
            }
            
            // Key-value pairs (Label : Value) - use table-like layout
            if (trimmed.Contains(":") && !trimmed.Contains("http"))
            {
                var parts = trimmed.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    var para = new Paragraph(new ParagraphProperties(
                        new SpacingBetweenLines { Before = "0", After = "40" }));
                    
                    // Label part (gray)
                    para.AppendChild(new Run(
                        new RunProperties(
                            new FontSize { Val = "18" }, 
                            new OxmlColor { Val = "64748B" }),
                        new Text(parts[0].Trim() + " : ") { Space = SpaceProcessingModeValues.Preserve }));
                    
                    // Value part (normal)
                    para.AppendChild(new Run(
                        new RunProperties(new FontSize { Val = "18" }),
                        new Text(parts[1].Trim()) { Space = SpaceProcessingModeValues.Preserve }));
                    
                    body.AppendChild(para);
                    continue;
                }
            }
            
            // Progress bars (█ and ░ characters) - use monospace
            if (trimmed.Contains("█") || trimmed.Contains("░"))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { Before = "0", After = "40" }),
                    new Run(new RunProperties(
                        new RunFonts { Ascii = "Courier New", HighAnsi = "Courier New" },
                        new FontSize { Val = "16" },
                        new OxmlColor { Val = "3B82F6" }),
                        new Text(trimmed) { Space = SpaceProcessingModeValues.Preserve })));
                continue;
            }
            
            // Regular text
            body.AppendChild(new Paragraph(
                new ParagraphProperties(new SpacingBetweenLines { Before = "0", After = "80", Line = "276" }),
                new Run(new RunProperties(new FontSize { Val = "18" }),
                    new Text(trimmed) { Space = SpaceProcessingModeValues.Preserve })));
        }

        // Add charts if available
        if (!string.IsNullOrWhiteSpace(r.ActivityChartImage) || !string.IsNullOrWhiteSpace(r.StatusChartImage))
        {
            body.AppendChild(new Paragraph(new ParagraphProperties(
                new SpacingBetweenLines { Before = "240", After = "0" })));
            body.AppendChild(new Paragraph(new ParagraphProperties(
                new ParagraphBorders(new TopBorder { Val = BorderValues.Single, Size = 12, Color = "1E40AF" }),
                new SpacingBetweenLines { Before = "80", After = "80" })));
            body.AppendChild(new Paragraph(
                new ParagraphProperties(new SpacingBetweenLines { Before = "80", After = "120" }),
                new Run(new RunProperties(
                    new Bold(), 
                    new FontSize { Val = "22" }, 
                    new OxmlColor { Val = "1E40AF" }),
                    new Text("VISUAL CHARTS"))));
            
            if (!string.IsNullOrWhiteSpace(r.ActivityChartImage))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { Before = "80", After = "40" }),
                    new Run(new RunProperties(
                        new Bold(), 
                        new FontSize { Val = "18" }, 
                        new OxmlColor { Val = "64748B" }),
                        new Text("Activity Distribution"))));
                body.AppendChild(CreateImageParagraph(mainPart, r.ActivityChartImage));
            }
            if (!string.IsNullOrWhiteSpace(r.StatusChartImage))
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { Before = "120", After = "40" }),
                    new Run(new RunProperties(
                        new Bold(), 
                        new FontSize { Val = "18" }, 
                        new OxmlColor { Val = "64748B" }),
                        new Text("Status Distribution"))));
                body.AppendChild(CreateImageParagraph(mainPart, r.StatusChartImage));
            }
        }

        body.AppendChild(new SectionProperties(
            new PageMargin { Top = 900, Bottom = 900, Left = 1080, Right = 1080 }));

        mainPart.Document.Save();
        doc.Dispose();
        return ms.ToArray();
    }

    // ── DOCX helpers ───────────────────────────────────────────────────────
    private static Paragraph CenteredHeading(string text) =>
        new(new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = "120" }),
            new Run(new RunProperties(
                    new Bold(), new FontSize { Val = "34" }, new OxmlColor { Val = "1E40AF" }),
                new Text(text)));

    private static Paragraph Heading2(string text) =>
        new(new ParagraphProperties(new SpacingBetweenLines { Before = "160", After = "80" }),
            new Run(new RunProperties(
                    new Bold(), new FontSize { Val = "22" }, new OxmlColor { Val = "1E40AF" }),
                new Text(text)));

    private static Paragraph Para(string text, bool bold = false, string? color = null)
    {
        var rp = new RunProperties(new FontSize { Val = "20" });
        if (bold) rp.AppendChild(new Bold());
        if (color != null) rp.AppendChild(new OxmlColor { Val = color });
        return new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Before = "0", After = "80" }),
            new Run(rp, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }

    private static Paragraph HorizontalRule() =>
        new(new ParagraphProperties(
            new ParagraphBorders(
                new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "E2E8F0" })));

    private static Paragraph CreateImageParagraph(MainDocumentPart mainPart, string base64Image)
    {
        try
        {
            // Remove data:image/png;base64, prefix if present
            var imageData = base64Image.Contains(",") 
                ? base64Image.Split(',')[1] 
                : base64Image;
            
            var imageBytes = Convert.FromBase64String(imageData);
            
            var imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (var stream = new MemoryStream(imageBytes))
            {
                imagePart.FeedData(stream);
            }

            var relationshipId = mainPart.GetIdOfPart(imagePart);

            // Better sizing - 6 inches wide (5760000 EMUs), maintain aspect ratio
            var widthEmus = 5760000L;
            var heightEmus = 3200000L; // Approximately 3.33 inches

            // Create the image element
            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent { Cx = widthEmus, Cy = heightEmus },
                    new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties { Id = 1U, Name = "Chart" },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties { Id = 0U, Name = "Chart.png" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip { Embed = relationshipId },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset { X = 0L, Y = 0L },
                                        new A.Extents { Cx = widthEmus, Cy = heightEmus }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                ) { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U });

            return new Paragraph(new Run(element));
        }
        catch
        {
            // If image embedding fails, return empty paragraph
            return new Paragraph();
        }
    }
}
