using Tesseract;

namespace AutoReportGenerator.Services;

public class OcrService
{
    private readonly string _tessDataPath;

    public OcrService(IWebHostEnvironment env)
    {
        _tessDataPath = Path.Combine(env.ContentRootPath, "tessdata");
    }

    public string ExtractText(Stream imageStream)
    {
        using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
        engine.SetVariable("tessedit_char_whitelist", "");

        using var ms = new MemoryStream();
        imageStream.CopyTo(ms);
        var bytes = ms.ToArray();

        using var img = Pix.LoadFromMemory(bytes);
        using var page = engine.Process(img);

        var text = page.GetText();
        return CleanText(text);
    }

    private static string CleanText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        // Normalize line endings, collapse excessive blank lines
        var lines = raw.Replace("\r\n", "\n").Replace("\r", "\n")
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0);

        return string.Join("\n", lines);
    }
}
