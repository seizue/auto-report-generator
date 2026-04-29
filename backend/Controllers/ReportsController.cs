using AutoReportGenerator.DTOs;
using AutoReportGenerator.Models;
using AutoReportGenerator.Repositories;
using AutoReportGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoReportGenerator.Controllers;

[ApiController]
[Route("api")]
public class ReportsController(
    ReportRepository repo,
    ReportFormatterService formatter,
    PdfExportService pdfService,
    DocxExportService docxService,
    TextParserService parser) : ControllerBase
{
    // Reads the anonymous browser identity sent by the frontend
    private string ClientId => Request.Headers.TryGetValue("X-Client-Id", out var v)
        ? v.ToString().Trim()
        : string.Empty;

    [HttpPost("parse-text")]
    public ActionResult<ParsedReportData> ParseText([FromBody] ParseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
            return BadRequest("Raw text is required.");

        var parsed = parser.Parse(request.RawText, request.TemplateType);
        return Ok(parsed);
    }

    [HttpPost("generate-report")]
    public async Task<ActionResult<ReportResponse>> Generate([FromBody] ReportRequest request)
    {
        if (!TimeSpan.TryParse(request.TimeIn, out var timeIn))
            timeIn = new TimeSpan(8, 0, 0);
        if (!TimeSpan.TryParse(request.TimeOut, out var timeOut))
            timeOut = new TimeSpan(17, 0, 0);

        var report = new Report
        {
            ClientId = ClientId,
            Name = request.Name,
            Department = request.Department,
            Date = request.Date,
            TimeIn = timeIn,
            TimeOut = timeOut,
            Notes = request.Notes,
            TemplateType = request.TemplateType,
            ListStyle = request.ListStyle,
            Items = request.Tasks.Select((t, i) => new ReportItem
            {
                Task = t.Task,
                Status = t.Status,
                Order = i
            }).ToList()
        };

        var saved = await repo.CreateAsync(report);
        return Ok(MapToResponse(saved));
    }

    [HttpGet("reports")]
    public async Task<ActionResult<List<ReportResponse>>> GetAll()
    {
        var reports = await repo.GetAllAsync(ClientId);
        return Ok(reports.Select(MapToResponse));
    }

    [HttpGet("reports/{id}")]
    public async Task<ActionResult<ReportResponse>> GetById(int id)
    {
        var report = await repo.GetByIdAsync(id, ClientId);
        if (report is null) return NotFound();
        return Ok(MapToResponse(report));
    }

    [HttpPut("reports/{id}")]
    public async Task<ActionResult<ReportResponse>> Update(int id, [FromBody] ReportRequest request)
    {
        if (!TimeSpan.TryParse(request.TimeIn, out var timeIn))
            timeIn = new TimeSpan(8, 0, 0);
        if (!TimeSpan.TryParse(request.TimeOut, out var timeOut))
            timeOut = new TimeSpan(17, 0, 0);

        var updated = new Report
        {
            Name         = request.Name,
            Department   = request.Department,
            Date         = request.Date,
            TimeIn       = timeIn,
            TimeOut      = timeOut,
            Notes        = request.Notes,
            TemplateType = request.TemplateType,
            ListStyle    = request.ListStyle,
            Items        = request.Tasks.Select((t, i) => new ReportItem
            {
                Task   = t.Task,
                Status = t.Status,
                Order  = i
            }).ToList()
        };

        var result = await repo.UpdateAsync(id, updated, ClientId);
        if (result is null) return NotFound();
        return Ok(MapToResponse(result));
    }

    [HttpDelete("reports/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await repo.DeleteAsync(id, ClientId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpDelete("reports")]
    public async Task<IActionResult> DeleteAll()
    {
        await repo.DeleteAllAsync(ClientId);
        return NoContent();
    }

    [HttpPost("export/pdf/{id}")]
    public async Task<IActionResult> ExportPdf(int id)
    {
        var report = await repo.GetByIdAsync(id, ClientId);
        if (report is null) return NotFound();
        var bytes = pdfService.Export(report);
        return File(bytes, "application/pdf", $"report_{id}.pdf");
    }

    [HttpPost("export/docx/{id}")]
    public async Task<IActionResult> ExportDocx(int id)
    {
        var report = await repo.GetByIdAsync(id, ClientId);
        if (report is null) return NotFound();
        var bytes = docxService.Export(report);
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"report_{id}.docx");
    }

    private ReportResponse MapToResponse(Report r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Department = r.Department,
        Date = r.Date,
        TimeIn = r.TimeIn.ToString(@"hh\:mm"),
        TimeOut = r.TimeOut.ToString(@"hh\:mm"),
        Notes = r.Notes,
        TemplateType = r.TemplateType,
        ListStyle = r.ListStyle,
        FormattedContent = formatter.Format(r),
        CreatedAt = r.CreatedAt,
        Tasks = r.Items.OrderBy(i => i.Order).Select(i => new TaskItemResponse
        {
            Task = i.Task,
            Status = i.Status
        }).ToList()
    };
}
