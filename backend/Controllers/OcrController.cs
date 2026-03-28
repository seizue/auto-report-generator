using AutoReportGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoReportGenerator.Controllers;

[ApiController]
[Route("api/ocr")]
public class OcrController : ControllerBase
{
    private readonly DocumentTextExtractorService _extractor;

    public OcrController(DocumentTextExtractorService extractor)
    {
        _extractor = extractor;
    }

    [HttpPost("extract")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max
    public async Task<IActionResult> Extract(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        if (!DocumentTextExtractorService.IsSupportedFileType(file.FileName))
            return BadRequest(new { 
                error = $"Unsupported file type. Supported formats: {DocumentTextExtractorService.GetSupportedFormatsDescription()}" 
            });

        try
        {
            var text = await _extractor.ExtractTextFromFileAsync(file);

            if (string.IsNullOrWhiteSpace(text))
                return Ok(new { text = "", message = "No text detected in file." });

            return Ok(new { text, fileName = file.FileName, fileType = Path.GetExtension(file.FileName) });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Text extraction failed: " + ex.Message });
        }
    }
}
