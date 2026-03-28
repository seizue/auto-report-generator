using System.Text.RegularExpressions;
using AutoReportGenerator.DTOs;

namespace AutoReportGenerator.Services;

/// <summary>
/// Parses free-form raw text into structured report data.
/// No AI required — uses heuristics, regex, and NLP-lite patterns.
/// </summary>
public class TextParserService
{
    // Time patterns: 8am, 8:00am, 08:00, 8:00 AM, 8 AM
    private static readonly Regex TimePattern =
        new(@"\b(\d{1,2})(?::(\d{2}))?\s*(am|pm)\b", RegexOptions.IgnoreCase);

    // 24h time: 08:00, 17:30
    private static readonly Regex Time24Pattern =
        new(@"\b([01]?\d|2[0-3]):([0-5]\d)\b");

    // "time in", "arrived", "started", "clocked in"
    private static readonly Regex TimeInPattern =
        new(@"(?:time[\s-]?in|arrived?|started?|clocked[\s-]?in|began?|logged[\s-]?in)[^\d]*(\d{1,2}(?::\d{2})?\s*(?:am|pm)?)",
            RegexOptions.IgnoreCase);

    // "time out", "left", "finished", "clocked out"
    private static readonly Regex TimeOutPattern =
        new(@"(?:time[\s-]?out|left|finished?|clocked[\s-]?out|ended?|logged[\s-]?out)[^\d]*(\d{1,2}(?::\d{2})?\s*(?:am|pm)?)",
            RegexOptions.IgnoreCase);

    // Date patterns
    private static readonly Regex DatePattern =
        new(@"\b(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{2,4})\b|" +
            @"\b(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)[a-z]*\.?\s+(\d{1,2}),?\s+(\d{4})\b",
            RegexOptions.IgnoreCase);

    // Sentence/task splitters: "then", "also", "and", bullet chars, semicolons, newlines
    private static readonly Regex TaskSplitter =
        new(@"\s*(?:then|also|after that|afterwards|next|additionally|furthermore|and then)\s+|[;\n•\-\*]+\s*",
            RegexOptions.IgnoreCase);

    // Action verbs that signal a task
    private static readonly HashSet<string> ActionVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "fixed","updated","checked","reviewed","attended","completed","finished","prepared",
        "submitted","sent","created","developed","tested","deployed","configured","installed",
        "resolved","addressed","handled","managed","coordinated","discussed","presented",
        "analyzed","documented","drafted","edited","approved","scheduled","called","met",
        "assisted","supported","trained","monitored","investigated","implemented","designed",
        "built","wrote","read","replied","responded","followed","cleaned","repaired","setup",
        "set up","upgraded","migrated","merged","pushed","pulled","committed","released"
    };

    // Words that indicate notes/remarks rather than tasks
    private static readonly HashSet<string> NoteKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "note","remark","observation","issue","problem","concern","reminder","todo","follow up",
        "pending","blocker","challenge","difficulty"
    };

    public ParsedReportData Parse(string rawText, string templateType)
    {
        var result = new ParsedReportData
        {
            TemplateType = templateType,
            Date = DateTime.Today.ToString("yyyy-MM-dd"),
            TimeIn = "08:00",
            TimeOut = "17:00"
        };

        if (string.IsNullOrWhiteSpace(rawText))
            return result;

        var text = rawText.Trim();

        // Extract times
        ExtractTimes(text, result);

        // Extract date
        ExtractDate(text, result);

        // Strip time/date fragments before splitting into tasks
        var cleaned = StripMetadata(text);

        // Split into candidate sentences
        var sentences = SplitIntoSentences(cleaned);

        var tasks = new List<TaskItem>();
        var notes = new List<string>();

        foreach (var sentence in sentences)
        {
            var s = sentence.Trim().TrimEnd('.', ',', ';');
            if (s.Length < 4) continue;

            if (IsNote(s))
                notes.Add(Formalize(s));
            else if (IsTask(s))
                tasks.Add(new TaskItem { Task = Formalize(s), Status = InferStatus(s) });
            else if (s.Length > 10)
                // Ambiguous — treat as task if it has enough content
                tasks.Add(new TaskItem { Task = Formalize(s), Status = "Completed" });
        }

        result.Tasks = tasks.Take(20).ToList();
        result.Notes = string.Join(" ", notes);

        return result;
    }

    private static void ExtractTimes(string text, ParsedReportData result)
    {
        // Try labeled patterns first
        var inMatch  = TimeInPattern.Match(text);
        var outMatch = TimeOutPattern.Match(text);

        if (inMatch.Success)
            result.TimeIn = NormalizeTime(inMatch.Groups[1].Value) ?? "08:00";

        if (outMatch.Success)
            result.TimeOut = NormalizeTime(outMatch.Groups[1].Value) ?? "17:00";

        // Fallback: grab first two times in text
        if (!inMatch.Success || !outMatch.Success)
        {
            var allTimes = TimePattern.Matches(text)
                .Cast<Match>()
                .Select(m => NormalizeTime(m.Value))
                .Where(t => t != null)
                .Distinct()
                .ToList();

            if (allTimes.Count >= 1 && !inMatch.Success)
                result.TimeIn = allTimes[0]!;
            if (allTimes.Count >= 2 && !outMatch.Success)
                result.TimeOut = allTimes[1]!;
        }
    }

    private static void ExtractDate(string text, ParsedReportData result)
    {
        var m = DatePattern.Match(text);
        if (!m.Success) return;

        try
        {
            if (DateTime.TryParse(m.Value, out var dt))
                result.Date = dt.ToString("yyyy-MM-dd");
        }
        catch { /* keep default */ }
    }

    private static string StripMetadata(string text)
    {
        // Remove time expressions
        text = TimePattern.Replace(text, " ");
        text = Time24Pattern.Replace(text, " ");
        text = TimeInPattern.Replace(text, " ");
        text = TimeOutPattern.Replace(text, " ");
        text = DatePattern.Replace(text, " ");

        // Remove common filler phrases
        text = Regex.Replace(text,
            @"\b(today|yesterday|this morning|this afternoon|this evening|at the office|in the office)\b",
            " ", RegexOptions.IgnoreCase);

        return Regex.Replace(text, @"\s{2,}", " ").Trim();
    }

    private static List<string> SplitIntoSentences(string text)
    {
        // Split on task splitter patterns first
        var parts = TaskSplitter.Split(text);

        var result = new List<string>();
        foreach (var part in parts)
        {
            // Also split on ". " sentence boundaries
            var subParts = Regex.Split(part.Trim(), @"(?<=[.!?])\s+");
            result.AddRange(subParts.Select(s => s.Trim()).Where(s => s.Length > 3));
        }
        return result;
    }

    private static bool IsTask(string sentence)
    {
        var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return false;

        // Starts with action verb
        var firstWord = words[0].TrimStart('-', '*', '•').ToLower();
        if (ActionVerbs.Contains(firstWord)) return true;

        // Contains action verb anywhere in first 4 words
        return words.Take(4).Any(w => ActionVerbs.Contains(w.ToLower()));
    }

    private static bool IsNote(string sentence)
    {
        return NoteKeywords.Any(k =>
            sentence.StartsWith(k, StringComparison.OrdinalIgnoreCase) ||
            sentence.Contains($" {k} ", StringComparison.OrdinalIgnoreCase));
    }

    private static string InferStatus(string sentence)
    {
        var lower = sentence.ToLower();

        if (Regex.IsMatch(lower, @"\b(pending|not yet|will do|to do|todo|haven't|havent|not done|incomplete)\b"))
            return "Pending";

        if (Regex.IsMatch(lower, @"\b(in progress|ongoing|working on|still|partially|half|started but)\b"))
            return "In Progress";

        return "Completed";
    }

    private static string Formalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var s = input.TrimStart('-', '*', '•', ' ');

        // Capitalize first letter
        s = char.ToUpper(s[0]) + s[1..];

        // Ensure ends with period
        if (!s.EndsWith('.') && !s.EndsWith('!') && !s.EndsWith('?'))
            s += ".";

        return s;
    }

    private static string? NormalizeTime(string raw)
    {
        raw = raw.Trim();
        if (DateTime.TryParse(raw, out var dt))
            return dt.ToString("HH:mm");

        // Handle "8am", "8pm" without colon
        var m = Regex.Match(raw, @"(\d{1,2})(?::(\d{2}))?\s*(am|pm)", RegexOptions.IgnoreCase);
        if (!m.Success) return null;

        var hour   = int.Parse(m.Groups[1].Value);
        var minute = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
        var period = m.Groups[3].Value.ToLower();

        if (period == "pm" && hour != 12) hour += 12;
        if (period == "am" && hour == 12) hour = 0;

        return $"{hour:D2}:{minute:D2}";
    }
}
