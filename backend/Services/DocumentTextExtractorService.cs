using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Xceed.Words.NET;
using PDFtoImage;
using SkiaSharp;

namespace AutoReportGenerator.Services;

/// <summary>
/// Service for extracting text from various document formats (PDF, DOCX, Images)
/// </summary>
public class DocumentTextExtractorService
{
    private readonly OcrService _ocrService;
    private readonly ILogger<DocumentTextExtractorService> _logger;

    public DocumentTextExtractorService(OcrService ocrService, ILogger<DocumentTextExtractorService> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    public async Task<string> ExtractTextFromFileAsync(IFormFile file)
    {
        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await ExtractFromPdfAsync(file),
            ".docx" => await ExtractFromDocxAsync(file),
            ".doc" => throw new NotSupportedException("Legacy .doc format is not supported. Please use .docx"),
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tiff" or ".webp" => await Task.Run(() => ExtractFromImageAsync(file)),
            _ => throw new NotSupportedException($"File type {extension} is not supported")
        };
    }

    private string ExtractFromImageAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        return _ocrService.ExtractText(stream);
    }

    private async Task<string> ExtractFromPdfAsync(IFormFile file)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var pdfReader = new PdfReader(stream);
                using var pdfDocument = new PdfDocument(pdfReader);
                
                var text = new System.Text.StringBuilder();
                int totalPages = pdfDocument.GetNumberOfPages();
                
                _logger.LogInformation("Processing PDF with {Pages} pages", totalPages);
                
                for (int page = 1; page <= totalPages; page++)
                {
                    try
                    {
                        // Try LocationTextExtractionStrategy first (better for complex layouts)
                        var strategy = new LocationTextExtractionStrategy();
                        var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);
                        
                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            text.AppendLine(pageText);
                            _logger.LogDebug("Extracted {Length} chars from page {Page}", pageText.Length, page);
                        }
                        else
                        {
                            _logger.LogWarning("Page {Page} contains no extractable text", page);
                        }
                    }
                    catch (Exception pageEx)
                    {
                        _logger.LogWarning(pageEx, "Failed to extract text from page {Page}, skipping", page);
                    }
                }

                var result = text.ToString().Trim();
                
                if (string.IsNullOrWhiteSpace(result))
                {
                    _logger.LogInformation("No text extracted from PDF using text extraction. Attempting OCR fallback...");
                    // Close the current document before trying OCR
                    pdfDocument.Close();
                    
                    // Try OCR fallback
                    return ExtractFromPdfWithOcr(file);
                }
                
                _logger.LogInformation("Successfully extracted {Length} characters from PDF with {Pages} pages", 
                    result.Length, totalPages);
                
                return result;
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw our custom exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from PDF: {Message}", ex.Message);
                
                // Try OCR as fallback
                _logger.LogInformation("Text extraction failed. Attempting OCR fallback...");
                try
                {
                    return ExtractFromPdfWithOcr(file);
                }
                catch (Exception ocrEx)
                {
                    _logger.LogError(ocrEx, "OCR fallback also failed");
                    throw new InvalidOperationException(
                        "Failed to extract text from PDF using both text extraction and OCR. The file may be corrupted or password-protected.", ex);
                }
            }
        });
    }

    private string ExtractFromPdfWithOcr(IFormFile file)
    {
        try
        {
            _logger.LogInformation("Converting PDF pages to images for OCR processing");
            
            using var stream = file.OpenReadStream();
            var text = new System.Text.StringBuilder();
            
            // Convert PDF pages to images and run OCR on each
            #pragma warning disable CA1416 // Platform compatibility - PDFtoImage supports Windows/Linux/macOS
            var images = Conversion.ToImages(stream);
            #pragma warning restore CA1416
            int pageNum = 0;
            
            foreach (var image in images)
            {
                pageNum++;
                _logger.LogDebug("Processing page {Page} with OCR", pageNum);
                
                try
                {
                    // Convert SKBitmap to stream for OCR
                    using var imageStream = new MemoryStream();
                    image.Encode(imageStream, SKEncodedImageFormat.Png, 100);
                    imageStream.Position = 0;
                    
                    var pageText = _ocrService.ExtractText(imageStream);
                    
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        text.AppendLine(pageText);
                        _logger.LogDebug("OCR extracted {Length} chars from page {Page}", pageText.Length, pageNum);
                    }
                }
                catch (Exception pageEx)
                {
                    _logger.LogWarning(pageEx, "OCR failed for page {Page}", pageNum);
                }
                finally
                {
                    image.Dispose();
                }
            }
            
            var result = text.ToString().Trim();
            
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException(
                    "Unable to extract text from this PDF using OCR. The document may not contain readable text or the image quality may be too low.");
            }
            
            _logger.LogInformation("Successfully extracted {Length} characters from PDF using OCR on {Pages} pages", 
                result.Length, pageNum);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF to image OCR failed: {Message}", ex.Message);
            throw new InvalidOperationException(
                "Failed to extract text from PDF using OCR. The file may be corrupted or contain no readable text.", ex);
        }
    }

    private async Task<string> ExtractFromDocxAsync(IFormFile file)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var doc = DocX.Load(stream);
                
                var text = doc.Text;
                
                _logger.LogInformation("Extracted {Length} characters from DOCX", text.Length);
                
                return text.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from DOCX");
                throw new InvalidOperationException("Failed to extract text from DOCX. The file may be corrupted or password-protected.", ex);
            }
        });
    }

    public static bool IsSupportedFileType(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".pdf" or ".docx" or ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tiff" or ".webp";
    }

    public static string GetSupportedFormatsDescription()
    {
        return "PDF, DOCX, JPG, PNG, BMP, TIFF, WEBP";
    }
}
