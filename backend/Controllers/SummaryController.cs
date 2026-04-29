using AutoReportGenerator.DTOs;
using AutoReportGenerator.Models;
using AutoReportGenerator.Repositories;
using AutoReportGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoReportGenerator.Controllers;

[ApiController]
[Route("api")]
public class SummaryController : ControllerBase
{
    private readonly SummaryReportService _summaryService;
    private readonly SummaryExportService _exportService;
    private readonly ReportRepository _repo;

    public SummaryController(
        SummaryReportService summaryService, 
        SummaryExportService exportService,
        ReportRepository repo)
    {
        _summaryService = summaryService;
        _exportService = exportService;
        _repo = repo;
    }

    [HttpPost("summary-report")]
    public async Task<ActionResult<SummaryReportResult>> Generate([FromBody] SummaryReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
            return BadRequest("Raw text is required.");
        
        var result = await _summaryService.GenerateAsync(request);

        // Post-process: If report doesn't have AI enhancement yet, add it automatically
        if (result.AiProvider == "Heuristic" && !result.AiEnhanced)
        {
            result = await _summaryService.EnhanceReportWithAiAsync(result, request.RawText);
        }

        // Save to database if requested (default: true)
        if (request.SaveToHistory ?? true)
        {
            var clientId = Request.Headers.TryGetValue("X-Client-Id", out var v)
                ? v.ToString().Trim()
                : string.Empty;

            var report = new Report
            {
                ClientId = clientId,
                Name = result.AuthorName ?? "Summary Report",
                Department = result.Department ?? "General",
                Date = DateTime.TryParse(result.Period, out var dt) ? dt : DateTime.Today,
                TimeIn = TimeSpan.Zero,
                TimeOut = TimeSpan.Zero,
                Notes = result.ExecutiveSummary,
                TemplateType = "summary",
                ListStyle = "bullets",
                Items = result.ActivityGroups
                    .SelectMany((g, i) => g.Items.Select((item, j) => new ReportItem
                    {
                        Task = $"{g.Category}: {item}",
                        Status = "Completed",
                        Order = i * 100 + j
                    }))
                    .ToList()
            };

            var saved = await _repo.CreateAsync(report);
            result.Id = saved.Id;
        }

        return Ok(result);
    }

    [HttpPost("summary/export/pdf")]
    public ActionResult ExportPdf([FromBody] SummaryReportResult result)
    {
        var bytes = _exportService.ExportPdf(result);
        return File(bytes, "application/pdf", "summary_report.pdf");
    }

    [HttpPost("summary/export/docx")]
    public ActionResult ExportDocx([FromBody] SummaryReportResult result)
    {
        var bytes = _exportService.ExportDocx(result);
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "summary_report.docx");
    }
}
