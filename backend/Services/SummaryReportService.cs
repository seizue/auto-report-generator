using System.Text;
using System.Text.RegularExpressions;
using AutoReportGenerator.DTOs;

namespace AutoReportGenerator.Services;

/// <summary>
/// Transforms raw free-form text into a structured summary report with metrics.
/// Supports both AI-enhanced and heuristic analysis with automatic fallback.
/// </summary>
public class SummaryReportService
{
    private readonly AiReportEnhancementService? _aiService;

    public SummaryReportService(AiReportEnhancementService? aiService = null)
    {
        _aiService = aiService;
    }

    // ── Category keyword maps ──────────────────────────────────────────────
    private static readonly Dictionary<string, string[]> CategoryKeywords = new()
    {
        ["Meetings & Communication"] = ["meeting","standup","call","discussion","presentation","briefing","conference","sync","huddle","interview","client","stakeholder","email","replied","responded","sent","notified","informed","coordinated"],
        ["Development & Technical"]  = ["developed","coded","built","implemented","deployed","fixed","bug","patch","commit","push","pull","merge","branch","release","tested","debug","refactor","api","database","server","config","install","upgrade","migrate","setup"],
        ["Documentation & Reporting"]= ["documented","report","wrote","drafted","updated","reviewed","edited","prepared","submitted","created","spreadsheet","presentation","slides","notes","minutes","summary","proposal","memo"],
        ["Research & Analysis"]      = ["researched","analyzed","investigated","studied","evaluated","assessed","reviewed","compared","tested","explored","identified","discovered","found","checked"],
        ["Administrative"]           = ["scheduled","organized","arranged","planned","coordinated","managed","handled","processed","filed","approved","signed","requested","followed up","tracked"],
        ["Maintenance & Support"]    = ["maintained","repaired","fixed","cleaned","replaced","installed","configured","monitored","checked","inspected","troubleshot","resolved","supported","assisted","helped"],
    };

    private static readonly string[] PendingKeywords   = ["pending","not yet","will do","todo","to do","haven't","havent","not done","incomplete","backlog","deferred","postponed"];
    private static readonly string[] InProgressKeywords= ["in progress","ongoing","working on","still","partially","started but","half","continuing","wip"];
    private static readonly string[] CompletedKeywords = ["completed","finished","done","resolved","fixed","submitted","deployed","sent","approved","closed"];

    private static readonly Regex SentenceSplitter =
        new(@"(?<=[.!?])\s+|\n+|(?:\s*[;]\s*)|(?:\s+(?:then|also|and then|after that|additionally|furthermore|next)\s+)",
            RegexOptions.IgnoreCase);

    private static readonly Regex TimePattern =
        new(@"\b(\d{1,2})(?::(\d{2}))?\s*(am|pm)\b|\b([01]?\d|2[0-3]):([0-5]\d)\b",
            RegexOptions.IgnoreCase);

    private static readonly Regex DatePattern =
        new(@"\b(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)[a-z]*\.?\s+\d{1,2},?\s+\d{4}\b|" +
            @"\b\d{1,2}[\/\-]\d{1,2}[\/\-]\d{2,4}\b",
            RegexOptions.IgnoreCase);

    // ── Sentiment & Priority Keywords ──────────────────────────────────────
    private static readonly string[] PositiveKeywords = 
        ["excellent", "outstanding", "great", "successful", "achieved", "completed", "improved", "increased", "effective", "efficient", "strong", "positive", "good", "well", "exceeded", "ahead"];
    
    private static readonly string[] NegativeKeywords = 
        ["failed", "poor", "delayed", "missed", "below", "decreased", "declined", "issue", "problem", "concern", "risk", "critical", "urgent", "overdue", "behind", "insufficient"];
    
    private static readonly string[] HighPriorityKeywords = 
        ["urgent", "critical", "immediate", "asap", "priority", "important", "deadline", "overdue", "emergency", "must", "required", "essential"];
    
    private static readonly string[] OpportunityKeywords = 
        ["opportunity", "potential", "growth", "expand", "improve", "optimize", "enhance", "increase", "develop", "innovate", "new"];
    
    private static readonly string[] RiskKeywords = 
        ["risk", "threat", "concern", "issue", "problem", "challenge", "obstacle", "delay", "shortage", "deficit", "vulnerability"];

    // ── Public entry point ─────────────────────────────────────────────────
    public async Task<SummaryReportResult> GenerateAsync(SummaryReportRequest request)
    {
        var raw = request.RawText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
            return new SummaryReportResult { Title = "Empty Report" };

        // Normalize text to fix common spelling and grammar issues
        raw = NormalizeText(raw);

        // Check if this is a certificate or formal document
        if (IsCertificateOrFormalDocument(raw))
        {
            return await GenerateCertificateSummary(request, raw);
        }

        // Check if this is financial/salary data
        if (IsFinancialReport(raw))
        {
            return await GenerateFinancialSummary(request, raw);
        }

        // Check if this is an incident/issue report
        if (IsIncidentReport(raw))
        {
            return await GenerateIncidentReport(request, raw);
        }

        // Check if this is meeting minutes
        if (IsMeetingMinutes(raw))
        {
            return await GenerateMeetingReport(request, raw);
        }

        // Check if this is data gathering/research report
        if (IsDataGatheringReport(raw))
        {
            return await GenerateDataGatheringReport(request, raw);
        }

        // Check if this is inventory/asset report
        if (IsInventoryReport(raw))
        {
            return await GenerateInventoryReport(request, raw);
        }

        // Check if this is strategy/planning report
        if (IsStrategyReport(raw))
        {
            return await GenerateStrategyReport(request, raw);
        }

        // Check if this is performance review
        if (IsPerformanceReview(raw))
        {
            return GeneratePerformanceReview(request, raw);
        }

        // Check if this is risk assessment
        if (IsRiskAssessment(raw))
        {
            return GenerateRiskAssessment(request, raw);
        }

        // Check if this is audit report
        if (IsAuditReport(raw))
        {
            return await GenerateAuditReport(request, raw);
        }

        // Check if this is training/learning report
        if (IsTrainingReport(raw))
        {
            return await GenerateTrainingReport(request, raw);
        }

        // Check if this is sales/marketing report
        if (IsSalesReport(raw))
        {
            return await GenerateSalesReport(request, raw);
        }

        // Check if this is academic/student report
        if (IsAcademicReport(raw))
        {
            return await GenerateAcademicReport(request, raw);
        }

        // Check if this is assignment/homework report
        if (IsAssignmentReport(raw))
        {
            return await GenerateAssignmentReport(request, raw);
        }

        // Check if this is research/thesis report
        if (IsResearchReport(raw))
        {
            return await GenerateResearchReport(request, raw);
        }

        // Check if this is attendance report
        if (IsAttendanceReport(raw))
        {
            return await GenerateAttendanceReport(request, raw);
        }

        // Check if this is grade/transcript report
        if (IsGradeReport(raw))
        {
            return await GenerateGradeReport(request, raw);
        }

        var sentences = ExtractSentences(raw);
        var activities = sentences
            .Select(s => new ActivityEntry(Formalize(s), InferStatus(s)))
            .Where(a => a.Text.Length > 8)
            .ToList();

        var groups = CategorizeActivities(activities);
        var metrics = BuildMetrics(activities, raw);
        var period = string.IsNullOrWhiteSpace(request.Period)
            ? DetectPeriod(raw) ?? DateTime.Today.ToString("MMMM yyyy")
            : request.Period;

        var summary = BuildExecutiveSummary(activities, groups, request.AuthorName, request.Department, period);
        var conclusion = BuildConclusion(metrics);
        
        // Try AI enhancement first, fallback to heuristic
        string enhancedInsights;
        string aiProvider = "Heuristic";
        bool aiEnhanced = false;
        
        if (_aiService != null)
        {
            var (aiInsights, provider) = await _aiService.EnhanceReportAsync(raw, request.ReportType, metrics);
            if (!string.IsNullOrWhiteSpace(aiInsights))
            {
                enhancedInsights = FormatAiInsights(aiInsights, provider);
                aiProvider = provider;
                aiEnhanced = true;
            }
            else
            {
                enhancedInsights = BuildEnhancedInsightsSection(raw, request.ReportType, metrics);
                aiProvider = "Heuristic";
                aiEnhanced = false;
            }
        }
        else
        {
            enhancedInsights = BuildEnhancedInsightsSection(raw, request.ReportType, metrics);
            aiProvider = "Heuristic";
            aiEnhanced = false;
        }
        
        var formatted = BuildFormattedText(request, groups, summary, conclusion, metrics, period, enhancedInsights);

        return new SummaryReportResult
        {
            Title           = request.ReportType,
            AuthorName      = request.AuthorName,
            Department      = request.Department,
            Period          = period,
            ExecutiveSummary= summary,
            ActivityGroups  = groups.Select(g => new ActivityGroup
            {
                Category = g.Key,
                Items    = g.Value.Select(a => a.Text).ToList()
            }).ToList(),
            ActivityChart   = BuildActivityChart(groups),
            StatusChart     = BuildStatusChart(metrics),
            Metrics         = metrics,
            Conclusion      = conclusion,
            FormattedText   = formatted,
            AiProvider      = aiProvider,
            AiEnhanced      = aiEnhanced
        };
    }

    /// <summary>
    /// Synchronous wrapper for backward compatibility
    /// </summary>
    public SummaryReportResult Generate(SummaryReportRequest request)
    {
        return GenerateAsync(request).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Format AI-generated insights into our standard format
    /// </summary>
    private static string FormatAiInsights(string aiOutput, string providerName)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        
        // Add provider badge
        var providerBadge = providerName switch
        {
            "Groq" => "🤖 AI-POWERED INSIGHTS (Groq - Llama 3.1)",
            "HuggingFace" => "🤖 AI-POWERED INSIGHTS (Hugging Face - Mistral)",
            "TogetherAI" => "🤖 AI-POWERED INSIGHTS (Together AI - Mixtral)",
            _ => "🤖 AI-POWERED INSIGHTS"
        };
        
        sb.AppendLine(providerBadge);
        sb.AppendLine("──────────────────────────────────────────────────────────────");
        sb.AppendLine(aiOutput);
        return sb.ToString();
    }

    // ── Certificate/Formal Document Handler ────────────────────────────────
    private Task<SummaryReportResult> GenerateCertificateSummary(SummaryReportRequest request, string raw)
    {
        var lines = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        // Extract key information
        var recipientName = ExtractRecipientName(raw);
        var eventTitle = ExtractEventTitle(raw);
        var date = ExtractCertificateDate(raw);
        var issuer = ExtractIssuer(raw);

        var summary = BuildCertificateSummary(recipientName, eventTitle, date, issuer);
        var formatted = BuildCertificateFormattedText(request, raw, recipientName, eventTitle, date, issuer);

        // Create a single activity group for the certificate
        var activityGroups = new List<ActivityGroup>
        {
            new ActivityGroup
            {
                Category = "Certificate Information",
                Items = new List<string>
                {
                    $"Recipient: {recipientName ?? "Not specified"}",
                    $"Event/Program: {eventTitle ?? "Not specified"}",
                    $"Date: {date ?? "Not specified"}",
                    $"Issued by: {issuer ?? "Not specified"}"
                }
            }
        };

        return Task.FromResult(new SummaryReportResult
        {
            Title = "Certificate Summary",
            AuthorName = recipientName ?? request.AuthorName,
            Department = issuer ?? request.Department,
            Period = date ?? DateTime.Today.ToString("MMMM dd, yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Certificate", Value = 1, Color = "#10b981" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Completed", Value = 1, Color = "#10b981" }
            },
            Metrics = new SummaryMetrics
            {
                TotalActivities = 1,
                CompletedCount = 1,
                InProgressCount = 0,
                PendingCount = 0,
                CompletionRate = 100,
                WordCount = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
            },
            Conclusion = "This certificate has been successfully documented.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        });
    }

    private static string? ExtractRecipientName(string text)
    {
        var patterns = new[]
        {
            @"(?:presented to|awarded to|this certifies that|is hereby awarded to)\s+([A-Z][a-z]+(?:\s+[A-Z]\.?)?\s+[A-Z][a-z]+)",
            @"(?:presented to|awarded to)\s*\n\s*([A-Z][a-z]+(?:\s+[A-Z]\.?)?\s+[A-Z][a-z]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private static string? ExtractEventTitle(string text)
    {
        var patterns = new[]
        {
            @"(?:for (?:participating in|completing|attending))\s+(?:the\s+)?([^.\n]+(?:Session|Training|Course|Workshop|Program|Seminar|Conference)[^.\n]*)",
            @"(?:Information Session on)\s+([^.\n]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private static string? ExtractCertificateDate(string text)
    {
        var patterns = new[]
        {
            @"(?:conducted on|given this|dated)\s+([A-Za-z]+\s+\d{1,2},?\s+\d{4})",
            @"(\d{1,2}\s+[A-Za-z]+\s+\d{4})",
            @"([A-Za-z]+\s+\d{1,2},\s+\d{4})"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private static string? ExtractIssuer(string text)
    {
        var patterns = new[]
        {
            @"(?:by the|issued by)\s+([^.\n]+(?:Department|Bureau|Office|Organization|Institute|University|Company)[^.\n]*)",
            @"(Department of [^.\n]+)",
            @"(\([A-Z]+\)[^.\n]+Office[^.\n]*)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private static string BuildCertificateSummary(string? recipient, string? eventTitle, string? date, string? issuer)
    {
        var sb = new StringBuilder();
        
        if (!string.IsNullOrWhiteSpace(recipient))
            sb.Append($"{recipient} ");
        else
            sb.Append("The recipient ");

        sb.Append("received a certificate ");

        if (!string.IsNullOrWhiteSpace(eventTitle))
            sb.Append($"for {eventTitle.ToLower()} ");

        if (!string.IsNullOrWhiteSpace(date))
            sb.Append($"on {date} ");

        if (!string.IsNullOrWhiteSpace(issuer))
            sb.Append($"from {issuer}");

        sb.Append(".");

        return sb.ToString();
    }

    private static string BuildCertificateFormattedText(
        SummaryReportRequest req,
        string raw,
        string? recipient,
        string? eventTitle,
        string? date,
        string? issuer)
    {
        var sb = new StringBuilder();
        var line = new string('═', 50);
        var thin = new string('─', 50);

        sb.AppendLine("CERTIFICATE SUMMARY");
        sb.AppendLine(line);
        sb.AppendLine($"Generated   : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine(thin);

        sb.AppendLine();
        sb.AppendLine("CERTIFICATE DETAILS");
        sb.AppendLine(thin);
        
        if (!string.IsNullOrWhiteSpace(recipient))
            sb.AppendLine($"Recipient   : {recipient}");
        
        if (!string.IsNullOrWhiteSpace(eventTitle))
            sb.AppendLine($"Event/Program: {eventTitle}");
        
        if (!string.IsNullOrWhiteSpace(date))
            sb.AppendLine($"Date        : {date}");
        
        if (!string.IsNullOrWhiteSpace(issuer))
            sb.AppendLine($"Issued By   : {issuer}");

        sb.AppendLine();
        sb.AppendLine("SUMMARY");
        sb.AppendLine(thin);
        sb.AppendLine(BuildCertificateSummary(recipient, eventTitle, date, issuer));

        return sb.ToString();
    }

    // ── Sentence extraction ────────────────────────────────────────────────
    private static List<string> ExtractSentences(string text)
    {
        // Check if this looks like a certificate or formal document (not task-based)
        var isCertificateOrFormal = IsCertificateOrFormalDocument(text);
        
        if (isCertificateOrFormal)
        {
            // For certificates/formal docs, treat the whole text as one activity
            // or split only on major breaks (double newlines)
            var majorParts = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return majorParts
                .Select(p => p.Replace("\n", " ").Replace("\r", " ").Trim())
                .Where(p => p.Length > 20) // Only keep substantial paragraphs
                .Take(5) // Limit to avoid too many entries
                .ToList();
        }
        
        // Normal task-based text processing
        var parts = SentenceSplitter.Split(text);
        return parts
            .Select(p => p.Trim().TrimEnd('.', ',', ';', '-'))
            .Where(p => p.Length > 5)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    
    private static bool IsCertificateOrFormalDocument(string text)
    {
        var lower = text.ToLower();
        
        // Check for certificate/formal document indicators
        var certificateKeywords = new[]
        {
            "certificate", "certification", "certify", "awarded", "presented to",
            "in recognition", "participation", "completion", "achievement",
            "hereby", "witness whereof", "given this", "signed", "authorized"
        };
        
        var keywordCount = certificateKeywords.Count(k => lower.Contains(k));
        
        // If it has 2+ certificate keywords and lacks task indicators, it's likely a certificate
        if (keywordCount >= 2)
        {
            var taskIndicators = new[] { "completed", "fixed", "updated", "reviewed", "attended meeting" };
            var hasTaskIndicators = taskIndicators.Any(t => lower.Contains(t));
            
            if (!hasTaskIndicators)
                return true;
        }
        
        return false;
    }

    // ── Financial Report Detection & Handler ───────────────────────────────
    private static bool IsFinancialReport(string text)
    {
        var lower = text.ToLower();
        
        // Strong financial indicators - must have at least one
        var strongFinancialKeywords = new[]
        {
            "salary", "income", "expense", "expenses", "revenue", "financial summary",
            "financial report", "budget", "net balance", "total income", "total expenses"
        };
        
        // Currency or money indicators
        var hasCurrency = lower.Contains("₱") || lower.Contains("$") || 
                         Regex.IsMatch(text, @"\d+[,\s]\d{3}") || // formatted numbers like 20,000
                         lower.Contains("php") || lower.Contains("usd");
        
        // Month indicators for financial periods
        var monthKeywords = new[]
        {
            "january", "february", "march", "april", "may", "june",
            "july", "august", "september", "october", "november", "december",
            "jan", "feb", "mar", "apr", "jun", "jul", "aug", "sep", "oct", "nov", "dec"
        };
        
        var hasStrongFinancial = strongFinancialKeywords.Any(k => FuzzyContains(text, k, 2));
        var hasMonthlyData = monthKeywords.Count(k => lower.Contains(k)) >= 3;
        var hasMultipleAmounts = Regex.Matches(text, @"\d{1,3}(?:[,\s]\d{3})+").Count >= 2;
        
        // Must have strong financial keyword AND (currency OR multiple months with amounts)
        return hasStrongFinancial && (hasCurrency || (hasMonthlyData && hasMultipleAmounts));
    }

    private async Task<SummaryReportResult> GenerateFinancialSummary(SummaryReportRequest request, string raw)
    {
        var lines = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        // Extract financial data
        var financialEntries = new List<FinancialEntry>();
        var totalIncome = 0m;
        var totalExpenses = 0m;
        
        // Check for monthly salary pattern (e.g., "Salary is 20,000 per month")
        var monthlySalaryMatch = Regex.Match(raw, @"salary\s+is\s+(\d{1,3}(?:[,\s]\d{3})*(?:\.\d{2})?)\s+per\s+month", RegexOptions.IgnoreCase);
        // More flexible expense pattern to handle typos and variations
        var monthlyExpenseMatch = Regex.Match(raw, @"e[px]pense?\s+(?:per\s+month\s+is|is\s+per\s+month|per\s+month)\s+(\d{1,3}(?:[,\s]\d{3})*(?:\.\d{2})?)", RegexOptions.IgnoreCase);
        
        var months = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        
        // If monthly salary pattern found, generate entries for all 12 months
        if (monthlySalaryMatch.Success)
        {
            var salaryStr = monthlySalaryMatch.Groups[1].Value.Replace(",", "").Replace(" ", "");
            if (decimal.TryParse(salaryStr, out var monthlySalary))
            {
                foreach (var month in months)
                {
                    financialEntries.Add(new FinancialEntry
                    {
                        Month = month,
                        Category = "Salary/Income",
                        Amount = monthlySalary,
                        IsExpense = false,
                        Description = $"{month} Salary"
                    });
                    totalIncome += monthlySalary;
                }
            }
        }
        
        // If monthly expense pattern found, generate entries for all 12 months
        if (monthlyExpenseMatch.Success)
        {
            var expenseStr = monthlyExpenseMatch.Groups[1].Value.Replace(",", "").Replace(" ", "");
            if (decimal.TryParse(expenseStr, out var monthlyExpense))
            {
                // Try to extract expense categories
                var expenseCategories = ExtractExpenseCategories(raw);
                
                if (expenseCategories.Any())
                {
                    // Distribute expenses across categories
                    var amountPerCategory = monthlyExpense / expenseCategories.Count;
                    foreach (var month in months)
                    {
                        foreach (var category in expenseCategories)
                        {
                            financialEntries.Add(new FinancialEntry
                            {
                                Month = month,
                                Category = category,
                                Amount = amountPerCategory,
                                IsExpense = true,
                                Description = $"{month} {category}"
                            });
                        }
                        totalExpenses += monthlyExpense;
                    }
                }
                else
                {
                    // No categories specified, use generic expense
                    foreach (var month in months)
                    {
                        financialEntries.Add(new FinancialEntry
                        {
                            Month = month,
                            Category = "General Expenses",
                            Amount = monthlyExpense,
                            IsExpense = true,
                            Description = $"{month} Expenses"
                        });
                        totalExpenses += monthlyExpense;
                    }
                }
            }
        }
        
        // If no monthly patterns found, use line-by-line extraction (original logic)
        if (!financialEntries.Any())
        {
            foreach (var line in lines)
            {
                var amounts = ExtractAmounts(line);
                if (amounts.Any())
                {
                    var month = ExtractMonth(line);
                    var category = InferFinancialCategory(line);
                    var isExpense = IsExpenseLine(line);
                    
                    foreach (var amount in amounts)
                    {
                        financialEntries.Add(new FinancialEntry
                        {
                            Month = month,
                            Category = category,
                            Amount = amount,
                            IsExpense = isExpense,
                            Description = line
                        });
                        
                        if (isExpense)
                            totalExpenses += amount;
                        else
                            totalIncome += amount;
                    }
                }
            }
        }

        var netAmount = totalIncome - totalExpenses;
        var period = DetectFinancialPeriod(raw);
        
        var summary = BuildFinancialSummary(financialEntries, totalIncome, totalExpenses, netAmount, period);
        var formatted = BuildFinancialFormattedText(request, financialEntries, totalIncome, totalExpenses, netAmount, period);
        
        // Group by category for chart
        var categoryGroups = financialEntries
            .GroupBy(e => e.Category)
            .Select(g => new ActivityGroup
            {
                Category = g.Key,
                Items = g.Select(e => $"{e.Month}: ₱{e.Amount:N2}").Distinct().ToList()
            })
            .ToList();

        var activityChart = financialEntries
            .GroupBy(e => e.Category)
            .Select((g, i) => new ChartDataPoint
            {
                Label = g.Key,
                Value = (int)g.Sum(e => e.Amount),
                Color = new[] { "#10b981", "#3b82f6", "#f59e0b", "#ef4444", "#8b5cf6" }[i % 5]
            })
            .ToList();

        var statusChart = new List<ChartDataPoint>
        {
            new() { Label = "Income", Value = (int)totalIncome, Color = "#10b981" },
            new() { Label = "Expenses", Value = (int)totalExpenses, Color = "#ef4444" },
        };

        return new SummaryReportResult
        {
            Title = "Financial Summary Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = period,
            ExecutiveSummary = summary,
            ActivityGroups = categoryGroups,
            ActivityChart = activityChart,
            StatusChart = statusChart,
            Metrics = new SummaryMetrics
            {
                TotalActivities = financialEntries.Count,
                CompletedCount = financialEntries.Count,
                InProgressCount = 0,
                PendingCount = 0,
                CompletionRate = 100,
                WordCount = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
            },
            Conclusion = netAmount >= 0 
                ? $"Net positive balance of ₱{netAmount:N2}. Financial position is healthy."
                : $"Net deficit of ₱{Math.Abs(netAmount):N2}. Review expenses and consider budget adjustments.",
            FormattedText = await AppendAiInsightsToFormattedText(formatted, raw, "Financial Summary Report", new SummaryMetrics { TotalActivities = financialEntries.Count, CompletedCount = financialEntries.Count, CompletionRate = 100 }),
            AiProvider = (await GetAiProviderInfoAsync(raw, "Financial Summary Report", new SummaryMetrics { TotalActivities = financialEntries.Count })).provider,
            AiEnhanced = (await GetAiProviderInfoAsync(raw, "Financial Summary Report", new SummaryMetrics { TotalActivities = financialEntries.Count })).enhanced
        };
    }

    private static List<string> ExtractExpenseCategories(string text)
    {
        var categories = new List<string>();
        
        // Use fuzzy matching for better category detection
        if (FuzzyContains(text, "food", 1) || FuzzyContains(text, "groceries", 2) || FuzzyContains(text, "meal", 1)) 
            categories.Add("Food & Groceries");
        if (FuzzyContains(text, "rent", 1) || FuzzyContains(text, "housing", 2)) 
            categories.Add("Housing/Rent");
        if (FuzzyContains(text, "transport", 2) || FuzzyContains(text, "transportation", 2)) 
            categories.Add("Transportation");
        if (FuzzyContains(text, "utility", 2) || FuzzyContains(text, "utilities", 2)) 
            categories.Add("Utilities");
        if (FuzzyContains(text, "medical", 2) || FuzzyContains(text, "health", 2)) 
            categories.Add("Healthcare");
        if (FuzzyContains(text, "entertainment", 2)) 
            categories.Add("Entertainment");
        if (FuzzyContains(text, "other", 1) || FuzzyContains(text, "necessity", 2) || FuzzyContains(text, "necessities", 2)) 
        {
            if (!categories.Any()) categories.Add("Other Necessities");
        }
        
        return categories;
    }

    private static List<decimal> ExtractAmounts(string text)
    {
        var amounts = new List<decimal>();
        
        // Match patterns like: 20,000 or 20000 or 20,000.00
        var matches = Regex.Matches(text, @"\b(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)\b");
        
        foreach (Match match in matches)
        {
            var numStr = match.Value.Replace(",", "");
            if (decimal.TryParse(numStr, out var amount) && amount >= 100) // Filter out small numbers
            {
                amounts.Add(amount);
            }
        }
        
        return amounts;
    }

    private static string ExtractMonth(string text)
    {
        var monthPattern = @"\b(january|february|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)\b";
        var match = Regex.Match(text, monthPattern, RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var month = match.Value;
            return char.ToUpper(month[0]) + month[1..].ToLower();
        }
        
        return "Unspecified";
    }

    private static string InferFinancialCategory(string line)
    {
        var lower = line.ToLower();
        
        if (lower.Contains("salary") || lower.Contains("income") || lower.Contains("wage"))
            return "Salary/Income";
        if (lower.Contains("rent") || lower.Contains("housing"))
            return "Housing";
        if (lower.Contains("food") || lower.Contains("groceries") || lower.Contains("meal"))
            return "Food & Groceries";
        if (lower.Contains("transport") || lower.Contains("gas") || lower.Contains("fare"))
            return "Transportation";
        if (lower.Contains("utility") || lower.Contains("electric") || lower.Contains("water") || lower.Contains("internet"))
            return "Utilities";
        if (lower.Contains("medical") || lower.Contains("health") || lower.Contains("insurance"))
            return "Healthcare";
        if (lower.Contains("entertainment") || lower.Contains("leisure"))
            return "Entertainment";
        if (lower.Contains("saving") || lower.Contains("investment"))
            return "Savings/Investment";
        
        return "Other";
    }

    private static bool IsExpenseLine(string line)
    {
        var lower = line.ToLower();
        var expenseKeywords = new[] { "expense", "spent", "paid", "cost", "bill", "rent", "fee" };
        var incomeKeywords = new[] { "salary", "income", "revenue", "earned", "received" };
        
        var hasExpenseKeyword = expenseKeywords.Any(k => lower.Contains(k));
        var hasIncomeKeyword = incomeKeywords.Any(k => lower.Contains(k));
        
        // If has income keyword, it's not an expense
        if (hasIncomeKeyword) return false;
        
        // If has expense keyword or no clear indicator, assume expense
        return hasExpenseKeyword || !hasIncomeKeyword;
    }

    private static string DetectFinancialPeriod(string text)
    {
        var yearMatch = Regex.Match(text, @"\b(20\d{2})\b");
        var year = yearMatch.Success ? yearMatch.Value : DateTime.Today.Year.ToString();
        
        var monthCount = Regex.Matches(text, @"\b(january|february|march|april|may|june|july|august|september|october|november|december)\b", RegexOptions.IgnoreCase).Count;
        
        if (monthCount >= 6)
            return $"Year {year}";
        
        return $"{year}";
    }

    private static string BuildFinancialSummary(List<FinancialEntry> entries, decimal totalIncome, decimal totalExpenses, decimal netAmount, string period)
    {
        var sb = new StringBuilder();
        
        sb.Append($"Financial summary for {period} shows ");
        sb.Append($"total income of ₱{totalIncome:N2} and total expenses of ₱{totalExpenses:N2}, ");
        sb.Append($"resulting in a net {(netAmount >= 0 ? "surplus" : "deficit")} of ₱{Math.Abs(netAmount):N2}. ");
        
        var expenseRate = totalIncome > 0 ? (totalExpenses / totalIncome * 100) : 0;
        sb.Append($"Expense ratio is {expenseRate:F1}% of income. ");
        
        if (expenseRate > 90)
            sb.Append("Consider reducing expenses to improve savings.");
        else if (expenseRate > 70)
            sb.Append("Expense management is moderate, room for improvement.");
        else
            sb.Append("Good financial discipline with healthy savings rate.");
        
        return sb.ToString();
    }

    private static string BuildFinancialFormattedText(
        SummaryReportRequest req,
        List<FinancialEntry> entries,
        decimal totalIncome,
        decimal totalExpenses,
        decimal netAmount,
        string period)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("              FINANCIAL SUMMARY REPORT");
        sb.AppendLine(line);
        sb.AppendLine();

        sb.AppendLine("REPORT INFORMATION");
        sb.AppendLine(thin);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Prepared by : {req.AuthorName}");
        if (!string.IsNullOrWhiteSpace(req.Department))
            sb.AppendLine($"Department  : {req.Department}");
        sb.AppendLine($"Period      : {period}");
        sb.AppendLine($"Generated   : {DateTime.Now:MMMM dd, yyyy}");

        sb.AppendLine();
        sb.AppendLine("FINANCIAL OVERVIEW");
        sb.AppendLine(thin);
        sb.AppendLine($"Total Income    : ₱{totalIncome:N2}");
        sb.AppendLine($"Total Expenses  : ₱{totalExpenses:N2}");
        sb.AppendLine($"Net Balance     : ₱{netAmount:N2}");
        sb.AppendLine($"Expense Ratio   : {(totalIncome > 0 ? totalExpenses / totalIncome * 100 : 0):F1}%");
        sb.AppendLine($"Savings Rate    : {(totalIncome > 0 ? (totalIncome - totalExpenses) / totalIncome * 100 : 0):F1}%");

        // Group by category
        var byCategory = entries.GroupBy(e => e.Category).OrderByDescending(g => g.Sum(e => e.Amount));
        
        sb.AppendLine();
        sb.AppendLine("BREAKDOWN BY CATEGORY");
        sb.AppendLine(thin);
        foreach (var group in byCategory)
        {
            var categoryTotal = group.Sum(e => e.Amount);
            var percentage = totalIncome > 0 ? (categoryTotal / totalIncome * 100) : 0;
            sb.AppendLine();
            sb.AppendLine($"▸ {group.Key.ToUpper()}");
            sb.AppendLine($"  Total: ₱{categoryTotal:N2} ({percentage:F1}% of income)");
            
            foreach (var entry in group.Take(5)) // Show top 5 entries per category
            {
                sb.AppendLine($"  • {entry.Month}: ₱{entry.Amount:N2}");
            }
        }

        // Monthly breakdown if available
        var byMonth = entries.GroupBy(e => e.Month).Where(g => g.Key != "Unspecified").OrderBy(g => g.Key);
        if (byMonth.Any())
        {
            sb.AppendLine();
            sb.AppendLine("MONTHLY BREAKDOWN");
            sb.AppendLine(thin);
            foreach (var month in byMonth)
            {
                var monthIncome = month.Where(e => !e.IsExpense).Sum(e => e.Amount);
                var monthExpenses = month.Where(e => e.IsExpense).Sum(e => e.Amount);
                var monthNet = monthIncome - monthExpenses;
                
                sb.AppendLine($"{month.Key,-12} | Income: ₱{monthIncome,10:N2} | Expenses: ₱{monthExpenses,10:N2} | Net: ₱{monthNet,10:N2}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("FINANCIAL ANALYSIS");
        sb.AppendLine(thin);
        
        var expenseRate = totalIncome > 0 ? (totalExpenses / totalIncome * 100) : 0;
        if (expenseRate > 90)
            sb.AppendLine("⚠ High expense ratio detected. Consider budget optimization.");
        else if (expenseRate > 70)
            sb.AppendLine("ℹ Moderate expense ratio. Room for savings improvement.");
        else
            sb.AppendLine("✓ Healthy expense ratio with good savings discipline.");
        
        if (netAmount >= 0)
            sb.AppendLine($"✓ Positive net balance of ₱{netAmount:N2}.");
        else
            sb.AppendLine($"⚠ Deficit of ₱{Math.Abs(netAmount):N2}. Review spending patterns.");

        sb.AppendLine();
        sb.AppendLine(line);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Prepared by: {req.AuthorName}");
        sb.AppendLine($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}");
        sb.AppendLine(line);

        return sb.ToString();
    }

    private record FinancialEntry
    {
        public string Month { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public bool IsExpense { get; init; }
        public string Description { get; init; } = string.Empty;
    }

    // ── Incident Report Detection & Handler ────────────────────────────────
    private static bool IsIncidentReport(string text)
    {
        var lower = text.ToLower();
        var incidentKeywords = new[]
        {
            "incident", "issue", "problem", "bug", "error", "failure", "outage",
            "crash", "down", "not working", "broken", "malfunction", "defect",
            "severity", "priority", "root cause", "impact", "affected"
        };
        
        return incidentKeywords.Count(k => lower.Contains(k)) >= 3;
    }

    private async Task<SummaryReportResult> GenerateIncidentReport(SummaryReportRequest request, string raw)
    {
        var lines = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        var severity = ExtractSeverity(raw);
        var affectedSystems = ExtractAffectedSystems(raw);
        var rootCause = ExtractRootCause(raw);
        var resolution = ExtractResolution(raw);
        
        var summary = $"Incident report documenting {(severity != "Unknown" ? severity.ToLower() + " severity " : "")}issue. " +
                     $"{(affectedSystems.Any() ? $"Affected systems: {string.Join(", ", affectedSystems)}. " : "")}" +
                     $"{(rootCause != null ? $"Root cause identified: {rootCause}. " : "")}" +
                     $"{(resolution != null ? $"Resolution: {resolution}" : "Investigation ongoing.")}";

        var formatted = BuildIncidentFormattedText(request, raw, severity, affectedSystems, rootCause, resolution);

        var activityGroups = new List<ActivityGroup>
        {
            new() { Category = "Incident Details", Items = lines.Take(10).ToList() }
        };

        // Add AI enhancement
        var metrics = new SummaryMetrics { TotalActivities = 1, CompletedCount = resolution != null ? 1 : 0, CompletionRate = resolution != null ? 100 : 0 };
        var (aiProvider, aiEnhanced) = await GetAiProviderInfoAsync(raw, "Incident Report", metrics);
        var formattedWithAi = await AppendAiInsightsToFormattedText(formatted, raw, "Incident Report", metrics);

        return new SummaryReportResult
        {
            Title = "Incident Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM dd, yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = severity, Value = 1, Color = severity == "Critical" ? "#ef4444" : severity == "High" ? "#f59e0b" : "#3b82f6" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = resolution != null ? "Resolved" : "Open", Value = 1, Color = resolution != null ? "#10b981" : "#f59e0b" }
            },
            Metrics = metrics,
            Conclusion = resolution != null ? "Incident has been resolved." : "Incident is under investigation.",
            FormattedText = formattedWithAi,
            AiProvider = aiProvider,
            AiEnhanced = aiEnhanced
        };
    }

    private static string ExtractSeverity(string text)
    {
        var lower = text.ToLower();
        if (lower.Contains("critical") || lower.Contains("severe")) return "Critical";
        if (lower.Contains("high") || lower.Contains("urgent")) return "High";
        if (lower.Contains("medium") || lower.Contains("moderate")) return "Medium";
        if (lower.Contains("low") || lower.Contains("minor")) return "Low";
        return "Unknown";
    }

    private static List<string> ExtractAffectedSystems(string text)
    {
        var systems = new List<string>();
        var lower = text.ToLower();
        
        var systemKeywords = new[] { "server", "database", "api", "website", "application", "service", "network", "system" };
        foreach (var keyword in systemKeywords)
        {
            if (lower.Contains(keyword))
                systems.Add(keyword);
        }
        
        return systems.Distinct().ToList();
    }

    private static string? ExtractRootCause(string text)
    {
        var match = Regex.Match(text, @"(?:root cause|caused by|due to)[:\s]+([^.\n]+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractResolution(string text)
    {
        var match = Regex.Match(text, @"(?:resolved|fixed|solution|workaround)[:\s]+([^.\n]+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string BuildIncidentFormattedText(SummaryReportRequest req, string raw, string severity, List<string> affected, string? rootCause, string? resolution)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("                  INCIDENT REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("INCIDENT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Reported by : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Date        : {DateTime.Now:MMMM dd, yyyy hh:mm tt}");
        sb.AppendLine($"Severity    : {severity}");
        sb.AppendLine($"Status      : {(resolution != null ? "Resolved" : "Open")}");
        
        if (affected.Any())
        {
            sb.AppendLine();
            sb.AppendLine("AFFECTED SYSTEMS");
            sb.AppendLine(thin);
            foreach (var system in affected)
                sb.AppendLine($"  • {system}");
        }

        sb.AppendLine();
        sb.AppendLine("INCIDENT DESCRIPTION");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        if (rootCause != null)
        {
            sb.AppendLine();
            sb.AppendLine("ROOT CAUSE");
            sb.AppendLine(thin);
            sb.AppendLine(rootCause);
        }

        if (resolution != null)
        {
            sb.AppendLine();
            sb.AppendLine("RESOLUTION");
            sb.AppendLine(thin);
            sb.AppendLine(resolution);
        }

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Meeting Minutes Detection & Handler ────────────────────────────────
    private static bool IsMeetingMinutes(string text)
    {
        var lower = text.ToLower();
        var meetingKeywords = new[]
        {
            "meeting", "attendees", "agenda", "minutes", "discussion", "action items",
            "decisions", "next steps", "follow up", "participants", "present:"
        };
        
        return meetingKeywords.Count(k => lower.Contains(k)) >= 3;
    }

    private async Task<SummaryReportResult> GenerateMeetingReport(SummaryReportRequest request, string raw)
    {
        var attendees = ExtractAttendees(raw);
        var actionItems = ExtractActionItems(raw);
        var decisions = ExtractDecisions(raw);
        
        var summary = $"Meeting held with {attendees.Count} participant{(attendees.Count != 1 ? "s" : "")}. " +
                     $"{actionItems.Count} action item{(actionItems.Count != 1 ? "s" : "")} identified. " +
                     $"{decisions.Count} decision{(decisions.Count != 1 ? "s" : "")} made.";

        var formatted = BuildMeetingFormattedText(request, raw, attendees, actionItems, decisions);

        var activityGroups = new List<ActivityGroup>();
        if (actionItems.Any())
            activityGroups.Add(new() { Category = "Action Items", Items = actionItems });
        if (decisions.Any())
            activityGroups.Add(new() { Category = "Decisions", Items = decisions });

        return new SummaryReportResult
        {
            Title = "Meeting Minutes",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM dd, yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Action Items", Value = actionItems.Count, Color = "#3b82f6" },
                new() { Label = "Decisions", Value = decisions.Count, Color = "#10b981" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Pending", Value = actionItems.Count, Color = "#f59e0b" }
            },
            Metrics = new SummaryMetrics { TotalActivities = actionItems.Count + decisions.Count, CompletedCount = decisions.Count },
            Conclusion = $"Meeting concluded with {actionItems.Count} action items to follow up.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static List<string> ExtractAttendees(string text)
    {
        var attendees = new List<string>();
        var match = Regex.Match(text, @"(?:attendees|participants|present)[:\s]+([^\n]+)", RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var names = match.Groups[1].Value.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            attendees.AddRange(names.Select(n => n.Trim()));
        }
        
        return attendees;
    }

    private static List<string> ExtractActionItems(string text)
    {
        var items = new List<string>();
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"(?:action|todo|task|follow.?up)", RegexOptions.IgnoreCase))
                items.Add(line.Trim());
        }
        
        return items;
    }

    private static List<string> ExtractDecisions(string text)
    {
        var decisions = new List<string>();
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"(?:decided|agreed|approved|decision)", RegexOptions.IgnoreCase))
                decisions.Add(line.Trim());
        }
        
        return decisions;
    }

    private static string BuildMeetingFormattedText(SummaryReportRequest req, string raw, List<string> attendees, List<string> actionItems, List<string> decisions)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("                 MEETING MINUTES");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("MEETING INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Recorded by : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Date        : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Attendees   : {attendees.Count}");

        if (attendees.Any())
        {
            sb.AppendLine();
            sb.AppendLine("PARTICIPANTS");
            sb.AppendLine(thin);
            foreach (var attendee in attendees)
                sb.AppendLine($"  • {attendee}");
        }

        sb.AppendLine();
        sb.AppendLine("DISCUSSION NOTES");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        if (decisions.Any())
        {
            sb.AppendLine();
            sb.AppendLine("DECISIONS MADE");
            sb.AppendLine(thin);
            foreach (var decision in decisions)
                sb.AppendLine($"  ✓ {decision}");
        }

        if (actionItems.Any())
        {
            sb.AppendLine();
            sb.AppendLine("ACTION ITEMS");
            sb.AppendLine(thin);
            foreach (var item in actionItems)
                sb.AppendLine($"  ☐ {item}");
        }

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Data Gathering Report Detection & Handler ──────────────────────────
    private static bool IsDataGatheringReport(string text)
    {
        var lower = text.ToLower();
        var dataKeywords = new[]
        {
            "survey", "data", "statistics", "respondents", "participants", "sample",
            "results", "findings", "analysis", "percentage", "%", "total:", "average",
            "collected", "responses", "questionnaire"
        };
        
        return dataKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateDataGatheringReport(SummaryReportRequest request, string raw)
    {
        var numbers = ExtractStatistics(raw);
        var totalResponses = ExtractTotalResponses(raw);
        
        var summary = $"Data gathering report with {totalResponses} total responses. " +
                     $"{numbers.Count} statistical data points collected and analyzed.";

        var formatted = BuildDataGatheringFormattedText(request, raw, numbers, totalResponses);

        var activityGroups = new List<ActivityGroup>
        {
            new() { Category = "Data Points", Items = numbers.Select(n => $"{n.Key}: {n.Value}").ToList() }
        };

        return new SummaryReportResult
        {
            Title = "Data Gathering Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM dd, yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = numbers.Take(5).Select((n, i) => new ChartDataPoint
            {
                Label = n.Key,
                Value = (int)n.Value,
                Color = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6" }[i % 5]
            }).ToList(),
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Collected", Value = totalResponses, Color = "#10b981" }
            },
            Metrics = new SummaryMetrics { TotalActivities = numbers.Count, CompletedCount = numbers.Count, CompletionRate = 100 },
            Conclusion = $"Data collection completed with {totalResponses} responses gathered.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static Dictionary<string, decimal> ExtractStatistics(string text)
    {
        var stats = new Dictionary<string, decimal>();
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"([^:]+):\s*(\d+(?:\.\d+)?)\s*(%)?");
            if (match.Success)
            {
                var label = match.Groups[1].Value.Trim();
                var value = decimal.Parse(match.Groups[2].Value);
                stats[label] = value;
            }
        }
        
        return stats;
    }

    private static int ExtractTotalResponses(string text)
    {
        var match = Regex.Match(text, @"(?:total|respondents?|participants?|responses?)[:\s]+(\d+)", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static string BuildDataGatheringFormattedText(SummaryReportRequest req, string raw, Dictionary<string, decimal> stats, int total)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("            DATA GATHERING REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("REPORT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Prepared by : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Date        : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Total Responses: {total}");

        sb.AppendLine();
        sb.AppendLine("STATISTICAL SUMMARY");
        sb.AppendLine(thin);
        foreach (var stat in stats)
            sb.AppendLine($"{stat.Key,-30} : {stat.Value}");

        sb.AppendLine();
        sb.AppendLine("DETAILED FINDINGS");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Inventory Report Detection & Handler ───────────────────────────────
    private static bool IsInventoryReport(string text)
    {
        var lower = text.ToLower();
        var inventoryKeywords = new[]
        {
            "inventory", "stock", "quantity", "qty", "items", "assets", "equipment",
            "available", "in stock", "out of stock", "units", "count", "total items"
        };
        
        return inventoryKeywords.Count(k => lower.Contains(k)) >= 3;
    }

    private async Task<SummaryReportResult> GenerateInventoryReport(SummaryReportRequest request, string raw)
    {
        var items = ExtractInventoryItems(raw);
        var totalItems = items.Sum(i => i.Quantity);
        
        var summary = $"Inventory report listing {items.Count} item types with total quantity of {totalItems} units.";

        var formatted = BuildInventoryFormattedText(request, raw, items, totalItems);

        var activityGroups = new List<ActivityGroup>
        {
            new() { Category = "Inventory Items", Items = items.Select(i => $"{i.Name}: {i.Quantity} units").ToList() }
        };

        return new SummaryReportResult
        {
            Title = "Inventory Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM dd, yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = items.Take(5).Select((item, i) => new ChartDataPoint
            {
                Label = item.Name,
                Value = item.Quantity,
                Color = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6" }[i % 5]
            }).ToList(),
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "In Stock", Value = items.Count(i => i.Quantity > 0), Color = "#10b981" },
                new() { Label = "Out of Stock", Value = items.Count(i => i.Quantity == 0), Color = "#ef4444" }
            },
            Metrics = new SummaryMetrics { TotalActivities = items.Count, CompletedCount = items.Count, CompletionRate = 100 },
            Conclusion = $"Inventory contains {totalItems} total units across {items.Count} item types.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static List<InventoryItem> ExtractInventoryItems(string text)
    {
        var items = new List<InventoryItem>();
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"([^:]+?)[\s:-]+(\d+)\s*(?:units?|pcs?|items?)?", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                items.Add(new InventoryItem
                {
                    Name = match.Groups[1].Value.Trim(),
                    Quantity = int.Parse(match.Groups[2].Value)
                });
            }
        }
        
        return items;
    }

    private static string BuildInventoryFormattedText(SummaryReportRequest req, string raw, List<InventoryItem> items, int total)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("               INVENTORY REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("REPORT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Prepared by : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Date        : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Total Items : {items.Count}");
        sb.AppendLine($"Total Units : {total}");

        sb.AppendLine();
        sb.AppendLine("INVENTORY LISTING");
        sb.AppendLine(thin);
        foreach (var item in items)
        {
            var status = item.Quantity > 0 ? "✓" : "✗";
            sb.AppendLine($"{status} {item.Name,-40} : {item.Quantity,6} units");
        }

        sb.AppendLine();
        sb.AppendLine("STOCK STATUS");
        sb.AppendLine(thin);
        var inStock = items.Count(i => i.Quantity > 0);
        var outOfStock = items.Count(i => i.Quantity == 0);
        sb.AppendLine($"In Stock     : {inStock} items");
        sb.AppendLine($"Out of Stock : {outOfStock} items");

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    private record InventoryItem
    {
        public string Name { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }

    // ── Strategy/Planning Report Detection & Handler ───────────────────────
    private static bool IsStrategyReport(string text)
    {
        var lower = text.ToLower();
        var strategyKeywords = new[]
        {
            "strategy", "strategic", "planning", "roadmap", "vision", "mission",
            "objectives", "goals", "initiatives", "kpi", "key performance indicator",
            "swot", "strengths", "weaknesses", "opportunities", "threats",
            "long-term", "short-term", "quarterly plan", "annual plan", "milestone"
        };
        
        return strategyKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateStrategyReport(SummaryReportRequest request, string raw)
    {
        var objectives = ExtractListItems(raw, new[] { "objective", "goal", "target" });
        var initiatives = ExtractListItems(raw, new[] { "initiative", "project", "program" });
        
        var summary = $"Strategic planning report outlining {objectives.Count} key objective{(objectives.Count != 1 ? "s" : "")} " +
                     $"and {initiatives.Count} strategic initiative{(initiatives.Count != 1 ? "s" : "")}.";

        var formatted = BuildStrategyFormattedText(request, raw, objectives, initiatives);

        var activityGroups = new List<ActivityGroup>();
        if (objectives.Any())
            activityGroups.Add(new() { Category = "Strategic Objectives", Items = objectives });
        if (initiatives.Any())
            activityGroups.Add(new() { Category = "Key Initiatives", Items = initiatives });

        return new SummaryReportResult
        {
            Title = "Strategic Planning Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = objectives.Take(5).Select((obj, i) => new ChartDataPoint
            {
                Label = obj.Length > 30 ? obj[..27] + "..." : obj,
                Value = 1,
                Color = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6" }[i % 5]
            }).ToList(),
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Objectives", Value = objectives.Count, Color = "#3b82f6" },
                new() { Label = "Initiatives", Value = initiatives.Count, Color = "#10b981" }
            },
            Metrics = new SummaryMetrics { TotalActivities = objectives.Count + initiatives.Count, CompletedCount = 0, CompletionRate = 0 },
            Conclusion = "Strategic plan requires execution and monitoring of defined objectives and initiatives.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static string BuildStrategyFormattedText(SummaryReportRequest req, string raw, List<string> objectives, List<string> initiatives)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("           STRATEGIC PLANNING REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("REPORT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Prepared by : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Date        : {DateTime.Now:MMMM dd, yyyy}");

        if (objectives.Any())
        {
            sb.AppendLine();
            sb.AppendLine("STRATEGIC OBJECTIVES");
            sb.AppendLine(thin);
            for (int i = 0; i < objectives.Count; i++)
                sb.AppendLine($"{i + 1}. {objectives[i]}");
        }

        if (initiatives.Any())
        {
            sb.AppendLine();
            sb.AppendLine("KEY INITIATIVES");
            sb.AppendLine(thin);
            foreach (var initiative in initiatives)
                sb.AppendLine($"  • {initiative}");
        }

        sb.AppendLine();
        sb.AppendLine("STRATEGIC OVERVIEW");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Performance Review Detection & Handler ─────────────────────────────
    private static bool IsPerformanceReview(string text)
    {
        var lower = text.ToLower();
        var performanceKeywords = new[]
        {
            "performance", "evaluation", "review", "assessment", "rating",
            "strengths", "areas for improvement", "achievements", "competencies",
            "exceeded expectations", "meets expectations", "below expectations",
            "employee", "appraisal", "feedback", "development plan"
        };
        
        return performanceKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private static SummaryReportResult GeneratePerformanceReview(SummaryReportRequest request, string raw)
    {
        var rating = ExtractPerformanceRating(raw);
        var strengths = ExtractListItems(raw, new[] { "strength", "positive", "achievement", "exceeded" });
        var improvements = ExtractListItems(raw, new[] { "improvement", "development", "weakness", "area for growth" });
        
        var summary = $"Performance review with overall rating: {rating}. " +
                     $"{strengths.Count} strength{(strengths.Count != 1 ? "s" : "")} identified, " +
                     $"{improvements.Count} area{(improvements.Count != 1 ? "s" : "")} for improvement noted.";

        var formatted = BuildPerformanceFormattedText(request, raw, rating, strengths, improvements);

        var activityGroups = new List<ActivityGroup>();
        if (strengths.Any())
            activityGroups.Add(new() { Category = "Strengths & Achievements", Items = strengths });
        if (improvements.Any())
            activityGroups.Add(new() { Category = "Areas for Improvement", Items = improvements });

        return new SummaryReportResult
        {
            Title = "Performance Review",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Strengths", Value = strengths.Count, Color = "#10b981" },
                new() { Label = "Improvements", Value = improvements.Count, Color = "#f59e0b" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = rating, Value = 1, Color = rating.Contains("Exceed") ? "#10b981" : rating.Contains("Meets") ? "#3b82f6" : "#f59e0b" }
            },
            Metrics = new SummaryMetrics { TotalActivities = strengths.Count + improvements.Count, CompletedCount = strengths.Count },
            Conclusion = $"Overall performance rating: {rating}. Continue leveraging strengths while addressing development areas.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static string ExtractPerformanceRating(string text)
    {
        var lower = text.ToLower();
        if (lower.Contains("exceeds expectations") || lower.Contains("outstanding") || lower.Contains("excellent"))
            return "Exceeds Expectations";
        if (lower.Contains("meets expectations") || lower.Contains("satisfactory") || lower.Contains("good"))
            return "Meets Expectations";
        if (lower.Contains("below expectations") || lower.Contains("needs improvement"))
            return "Needs Improvement";
        return "Not Rated";
    }

    private static string BuildPerformanceFormattedText(SummaryReportRequest req, string raw, string rating, List<string> strengths, List<string> improvements)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("              PERFORMANCE REVIEW");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("REVIEW INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Employee    : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Review Date : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Overall Rating: {rating}");

        if (strengths.Any())
        {
            sb.AppendLine();
            sb.AppendLine("STRENGTHS & ACHIEVEMENTS");
            sb.AppendLine(thin);
            foreach (var strength in strengths)
                sb.AppendLine($"  ✓ {strength}");
        }

        if (improvements.Any())
        {
            sb.AppendLine();
            sb.AppendLine("AREAS FOR IMPROVEMENT");
            sb.AppendLine(thin);
            foreach (var improvement in improvements)
                sb.AppendLine($"  → {improvement}");
        }

        sb.AppendLine();
        sb.AppendLine("DETAILED REVIEW");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Risk Assessment Detection & Handler ────────────────────────────────
    private static bool IsRiskAssessment(string text)
    {
        var lower = text.ToLower();
        var riskKeywords = new[]
        {
            "risk", "threat", "vulnerability", "likelihood", "impact", "probability",
            "mitigation", "contingency", "risk level", "high risk", "medium risk", "low risk",
            "risk assessment", "risk analysis", "risk management"
        };
        
        return riskKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private static SummaryReportResult GenerateRiskAssessment(SummaryReportRequest request, string raw)
    {
        var risks = ExtractRisks(raw);
        var highRisks = risks.Count(r => r.Level == "High");
        var mediumRisks = risks.Count(r => r.Level == "Medium");
        var lowRisks = risks.Count(r => r.Level == "Low");
        
        var summary = $"Risk assessment identified {risks.Count} total risk{(risks.Count != 1 ? "s" : "")}: " +
                     $"{highRisks} high, {mediumRisks} medium, {lowRisks} low severity.";

        var formatted = BuildRiskFormattedText(request, raw, risks);

        var activityGroups = risks.GroupBy(r => r.Level)
            .Select(g => new ActivityGroup
            {
                Category = $"{g.Key} Risk",
                Items = g.Select(r => r.Description).ToList()
            }).ToList();

        return new SummaryReportResult
        {
            Title = "Risk Assessment Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "High Risk", Value = highRisks, Color = "#ef4444" },
                new() { Label = "Medium Risk", Value = mediumRisks, Color = "#f59e0b" },
                new() { Label = "Low Risk", Value = lowRisks, Color = "#10b981" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Identified", Value = risks.Count, Color = "#3b82f6" }
            },
            Metrics = new SummaryMetrics { TotalActivities = risks.Count, CompletedCount = 0, PendingCount = risks.Count },
            Conclusion = highRisks > 0 ? $"Immediate attention required for {highRisks} high-risk item{(highRisks != 1 ? "s" : "")}." : "Risk levels are manageable with proper monitoring.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static List<RiskItem> ExtractRisks(string text)
    {
        var risks = new List<RiskItem>();
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            if (line.Trim().Length < 10) continue;
            
            var level = "Medium";
            var lower = line.ToLower();
            if (lower.Contains("high") || lower.Contains("critical") || lower.Contains("severe"))
                level = "High";
            else if (lower.Contains("low") || lower.Contains("minor"))
                level = "Low";
            
            if (lower.Contains("risk") || lower.Contains("threat") || lower.Contains("vulnerability"))
            {
                risks.Add(new RiskItem { Description = line.Trim(), Level = level });
            }
        }
        
        return risks;
    }

    private static string BuildRiskFormattedText(SummaryReportRequest req, string raw, List<RiskItem> risks)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("            RISK ASSESSMENT REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("ASSESSMENT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Prepared by : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Date        : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Total Risks : {risks.Count}");

        var byLevel = risks.GroupBy(r => r.Level).OrderByDescending(g => g.Key == "High" ? 3 : g.Key == "Medium" ? 2 : 1);
        foreach (var group in byLevel)
        {
            sb.AppendLine();
            sb.AppendLine($"{group.Key.ToUpper()} RISK ({group.Count()})");
            sb.AppendLine(thin);
            foreach (var risk in group)
                sb.AppendLine($"  ⚠ {risk.Description}");
        }

        sb.AppendLine();
        sb.AppendLine("DETAILED ASSESSMENT");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    private record RiskItem
    {
        public string Description { get; init; } = string.Empty;
        public string Level { get; init; } = "Medium";
    }

    // ── Audit Report Detection & Handler ───────────────────────────────────
    private static bool IsAuditReport(string text)
    {
        var lower = text.ToLower();
        var auditKeywords = new[]
        {
            "audit", "compliance", "findings", "non-compliance", "compliant",
            "audit trail", "verification", "inspection", "review", "standards",
            "regulations", "policies", "procedures", "controls", "discrepancies"
        };
        
        return auditKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateAuditReport(SummaryReportRequest request, string raw)
    {
        var findings = ExtractListItems(raw, new[] { "finding", "issue", "non-compliance", "discrepancy" });
        var compliant = ExtractListItems(raw, new[] { "compliant", "passed", "satisfactory", "meets standard" });
        
        var summary = $"Audit report with {findings.Count} finding{(findings.Count != 1 ? "s" : "")} identified and " +
                     $"{compliant.Count} compliant area{(compliant.Count != 1 ? "s" : "")}.";

        var formatted = BuildAuditFormattedText(request, raw, findings, compliant);

        var activityGroups = new List<ActivityGroup>();
        if (findings.Any())
            activityGroups.Add(new() { Category = "Audit Findings", Items = findings });
        if (compliant.Any())
            activityGroups.Add(new() { Category = "Compliant Areas", Items = compliant });

        return new SummaryReportResult
        {
            Title = "Audit Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Findings", Value = findings.Count, Color = "#ef4444" },
                new() { Label = "Compliant", Value = compliant.Count, Color = "#10b981" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Non-Compliant", Value = findings.Count, Color = "#ef4444" },
                new() { Label = "Compliant", Value = compliant.Count, Color = "#10b981" }
            },
            Metrics = new SummaryMetrics { TotalActivities = findings.Count + compliant.Count, CompletedCount = compliant.Count },
            Conclusion = findings.Count > 0 ? $"Audit identified {findings.Count} area{(findings.Count != 1 ? "s" : "")} requiring corrective action." : "Audit completed with full compliance.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static string BuildAuditFormattedText(SummaryReportRequest req, string raw, List<string> findings, List<string> compliant)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("                 AUDIT REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("AUDIT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Auditor     : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Audit Date  : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Total Items : {findings.Count + compliant.Count}");

        if (findings.Any())
        {
            sb.AppendLine();
            sb.AppendLine("AUDIT FINDINGS");
            sb.AppendLine(thin);
            foreach (var finding in findings)
                sb.AppendLine($"  ✗ {finding}");
        }

        if (compliant.Any())
        {
            sb.AppendLine();
            sb.AppendLine("COMPLIANT AREAS");
            sb.AppendLine(thin);
            foreach (var item in compliant)
                sb.AppendLine($"  ✓ {item}");
        }

        sb.AppendLine();
        sb.AppendLine("DETAILED AUDIT REPORT");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Training Report Detection & Handler ────────────────────────────────
    private static bool IsTrainingReport(string text)
    {
        var lower = text.ToLower();
        var trainingKeywords = new[]
        {
            "training", "course", "workshop", "seminar", "learning", "certification",
            "participants", "attendees", "modules", "lessons", "curriculum",
            "instructor", "trainer", "completion", "assessment", "skills"
        };
        
        return trainingKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateTrainingReport(SummaryReportRequest request, string raw)
    {
        var participants = ExtractTotalResponses(raw);
        var modules = ExtractListItems(raw, new[] { "module", "lesson", "topic", "session" });
        
        var summary = $"Training report with {participants} participant{(participants != 1 ? "s" : "")} " +
                     $"covering {modules.Count} module{(modules.Count != 1 ? "s" : "")}.";

        var formatted = BuildTrainingFormattedText(request, raw, participants, modules);

        var activityGroups = new List<ActivityGroup>();
        if (modules.Any())
            activityGroups.Add(new() { Category = "Training Modules", Items = modules });

        return new SummaryReportResult
        {
            Title = "Training Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = modules.Take(5).Select((mod, i) => new ChartDataPoint
            {
                Label = mod.Length > 20 ? mod[..17] + "..." : mod,
                Value = 1,
                Color = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6" }[i % 5]
            }).ToList(),
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Participants", Value = participants, Color = "#10b981" }
            },
            Metrics = new SummaryMetrics { TotalActivities = modules.Count, CompletedCount = modules.Count, CompletionRate = 100 },
            Conclusion = $"Training successfully delivered to {participants} participant{(participants != 1 ? "s" : "")}.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static string BuildTrainingFormattedText(SummaryReportRequest req, string raw, int participants, List<string> modules)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("               TRAINING REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("TRAINING INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Trainer     : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Date        : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Participants: {participants}");

        if (modules.Any())
        {
            sb.AppendLine();
            sb.AppendLine("TRAINING MODULES");
            sb.AppendLine(thin);
            for (int i = 0; i < modules.Count; i++)
                sb.AppendLine($"{i + 1}. {modules[i]}");
        }

        sb.AppendLine();
        sb.AppendLine("TRAINING DETAILS");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Sales/Marketing Report Detection & Handler ─────────────────────────
    private static bool IsSalesReport(string text)
    {
        var lower = text.ToLower();
        var salesKeywords = new[]
        {
            "sales", "revenue", "customers", "clients", "leads", "conversions",
            "marketing", "campaign", "roi", "return on investment", "target",
            "quota", "pipeline", "deals", "closed", "prospects", "growth"
        };
        
        return salesKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateSalesReport(SummaryReportRequest request, string raw)
    {
        var amounts = ExtractAmounts(raw);
        var totalRevenue = amounts.Sum();
        var deals = ExtractListItems(raw, new[] { "deal", "sale", "client", "customer", "contract" });
        
        var summary = $"Sales report showing total revenue of ₱{totalRevenue:N2} from {deals.Count} deal{(deals.Count != 1 ? "s" : "")}.";

        var formatted = BuildSalesFormattedText(request, raw, totalRevenue, deals);

        var activityGroups = new List<ActivityGroup>();
        if (deals.Any())
            activityGroups.Add(new() { Category = "Sales & Deals", Items = deals });

        return new SummaryReportResult
        {
            Title = "Sales & Marketing Report",
            AuthorName = request.AuthorName,
            Department = request.Department,
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Revenue", Value = (int)totalRevenue, Color = "#10b981" },
                new() { Label = "Deals", Value = deals.Count, Color = "#3b82f6" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Closed", Value = deals.Count, Color = "#10b981" }
            },
            Metrics = new SummaryMetrics { TotalActivities = deals.Count, CompletedCount = deals.Count, CompletionRate = 100 },
            Conclusion = $"Total revenue: ₱{totalRevenue:N2} from {deals.Count} closed deal{(deals.Count != 1 ? "s" : "")}.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static string BuildSalesFormattedText(SummaryReportRequest req, string raw, decimal revenue, List<string> deals)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("          SALES & MARKETING REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("REPORT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Prepared by : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Department  : {req.Department ?? "N/A"}");
        sb.AppendLine($"Period      : {DateTime.Now:MMMM yyyy}");
        sb.AppendLine($"Total Revenue: ₱{revenue:N2}");
        sb.AppendLine($"Total Deals  : {deals.Count}");

        if (deals.Any())
        {
            sb.AppendLine();
            sb.AppendLine("SALES ACTIVITIES");
            sb.AppendLine(thin);
            foreach (var deal in deals)
                sb.AppendLine($"  • {deal}");
        }

        sb.AppendLine();
        sb.AppendLine("DETAILED REPORT");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Helper Methods ──────────────────────────────────────────────────────
    private static List<string> ExtractListItems(string text, string[] keywords)
    {
        var items = new List<string>();
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            var lower = line.ToLower();
            if (keywords.Any(k => lower.Contains(k)) && line.Trim().Length > 10)
            {
                items.Add(line.Trim());
            }
        }
        
        return items.Distinct().ToList();
    }

    // ── Academic/Student Report Detection & Handlers ────────────────────────
    
    // Academic Progress Report
    private static bool IsAcademicReport(string text)
    {
        var lower = text.ToLower();
        var academicKeywords = new[]
        {
            "gpa", "grade point average", "semester", "academic", "student", "course",
            "subject", "credits", "units", "dean's list", "honors", "academic standing",
            "cumulative", "term", "enrolled", "major", "minor"
        };
        
        return academicKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateAcademicReport(SummaryReportRequest request, string raw)
    {
        var gpa = ExtractGPA(raw);
        var courses = ExtractListItems(raw, new[] { "course", "subject", "class" });
        var semester = ExtractSemester(raw);
        
        var summary = $"Academic progress report for {semester}. " +
                     $"{(gpa > 0 ? $"GPA: {gpa:F2}. " : "")}" +
                     $"{courses.Count} course{(courses.Count != 1 ? "s" : "")} enrolled.";

        var formatted = BuildAcademicFormattedText(request, raw, gpa, courses, semester);

        var activityGroups = new List<ActivityGroup>();
        if (courses.Any())
            activityGroups.Add(new() { Category = "Enrolled Courses", Items = courses });

        return new SummaryReportResult
        {
            Title = "Academic Progress Report",
            AuthorName = request.AuthorName,
            Department = semester,
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = courses.Take(5).Select((course, i) => new ChartDataPoint
            {
                Label = course.Length > 25 ? course[..22] + "..." : course,
                Value = 1,
                Color = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6" }[i % 5]
            }).ToList(),
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "GPA", Value = (int)(gpa * 25), Color = gpa >= 3.5m ? "#10b981" : gpa >= 2.5m ? "#3b82f6" : "#f59e0b" }
            },
            Metrics = new SummaryMetrics { TotalActivities = courses.Count, CompletedCount = courses.Count },
            Conclusion = gpa >= 3.5m ? "Excellent academic performance!" : gpa >= 2.5m ? "Good academic standing." : "Consider academic support resources.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static decimal ExtractGPA(string text)
    {
        var match = Regex.Match(text, @"gpa[:\s]+(\d+\.\d+)", RegexOptions.IgnoreCase);
        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var gpa))
            return gpa;
        return 0;
    }

    private static string ExtractSemester(string text)
    {
        var match = Regex.Match(text, @"(first|second|summer|fall|spring|winter)\s+(semester|term)", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Value;
        
        match = Regex.Match(text, @"(semester|term)\s+(\d+)", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Value;
        
        return "Current Semester";
    }

    private static string BuildAcademicFormattedText(SummaryReportRequest req, string raw, decimal gpa, List<string> courses, string semester)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("           ACADEMIC PROGRESS REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("STUDENT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Student Name: {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Semester    : {semester}");
        sb.AppendLine($"Report Date : {DateTime.Now:MMMM dd, yyyy}");
        if (gpa > 0)
            sb.AppendLine($"GPA         : {gpa:F2}");

        if (courses.Any())
        {
            sb.AppendLine();
            sb.AppendLine("ENROLLED COURSES");
            sb.AppendLine(thin);
            for (int i = 0; i < courses.Count; i++)
                sb.AppendLine($"{i + 1}. {courses[i]}");
        }

        sb.AppendLine();
        sb.AppendLine("ACADEMIC DETAILS");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // Assignment/Homework Report
    private static bool IsAssignmentReport(string text)
    {
        var lower = text.ToLower();
        var assignmentKeywords = new[]
        {
            "assignment", "homework", "project", "due date", "deadline", "submitted",
            "pending", "completed", "essay", "paper", "quiz", "exam", "test",
            "submission", "late", "on time"
        };
        
        return assignmentKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateAssignmentReport(SummaryReportRequest request, string raw)
    {
        var completed = ExtractListItems(raw, new[] { "completed", "submitted", "done", "finished" });
        var pending = ExtractListItems(raw, new[] { "pending", "due", "upcoming", "not yet" });
        
        var summary = $"Assignment tracking report: {completed.Count} completed, {pending.Count} pending.";

        var formatted = BuildAssignmentFormattedText(request, raw, completed, pending);

        var activityGroups = new List<ActivityGroup>();
        if (completed.Any())
            activityGroups.Add(new() { Category = "Completed Assignments", Items = completed });
        if (pending.Any())
            activityGroups.Add(new() { Category = "Pending Assignments", Items = pending });

        return new SummaryReportResult
        {
            Title = "Assignment Report",
            AuthorName = request.AuthorName,
            Department = "Student",
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Completed", Value = completed.Count, Color = "#10b981" },
                new() { Label = "Pending", Value = pending.Count, Color = "#f59e0b" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Completed", Value = completed.Count, Color = "#10b981" },
                new() { Label = "Pending", Value = pending.Count, Color = "#f59e0b" }
            },
            Metrics = new SummaryMetrics 
            { 
                TotalActivities = completed.Count + pending.Count, 
                CompletedCount = completed.Count,
                PendingCount = pending.Count,
                CompletionRate = (completed.Count + pending.Count) > 0 ? completed.Count * 100.0 / (completed.Count + pending.Count) : 0
            },
            Conclusion = pending.Count > 0 ? $"{pending.Count} assignment{(pending.Count != 1 ? "s" : "")} require{(pending.Count == 1 ? "s" : "")} attention." : "All assignments completed!",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static string BuildAssignmentFormattedText(SummaryReportRequest req, string raw, List<string> completed, List<string> pending)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("              ASSIGNMENT REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("STUDENT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Student     : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Report Date : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Total       : {completed.Count + pending.Count} assignments");

        if (completed.Any())
        {
            sb.AppendLine();
            sb.AppendLine($"COMPLETED ASSIGNMENTS ({completed.Count})");
            sb.AppendLine(thin);
            foreach (var item in completed)
                sb.AppendLine($"  ✓ {item}");
        }

        if (pending.Any())
        {
            sb.AppendLine();
            sb.AppendLine($"PENDING ASSIGNMENTS ({pending.Count})");
            sb.AppendLine(thin);
            foreach (var item in pending)
                sb.AppendLine($"  ☐ {item}");
        }

        sb.AppendLine();
        sb.AppendLine("ASSIGNMENT DETAILS");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // Research/Thesis Report
    private static bool IsResearchReport(string text)
    {
        var lower = text.ToLower();
        var researchKeywords = new[]
        {
            "research", "thesis", "dissertation", "chapter", "methodology", "literature review",
            "data collection", "analysis", "findings", "conclusion", "abstract", "bibliography",
            "citations", "references", "hypothesis", "research question"
        };
        
        return researchKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateResearchReport(SummaryReportRequest request, string raw)
    {
        var chapters = ExtractListItems(raw, new[] { "chapter", "section" });
        var progress = ExtractResearchProgress(raw);
        
        var summary = $"Research progress report: {chapters.Count} chapter{(chapters.Count != 1 ? "s" : "")} documented. {progress}";

        var formatted = BuildResearchFormattedText(request, raw, chapters, progress);

        var activityGroups = new List<ActivityGroup>();
        if (chapters.Any())
            activityGroups.Add(new() { Category = "Research Chapters", Items = chapters });

        return new SummaryReportResult
        {
            Title = "Research/Thesis Report",
            AuthorName = request.AuthorName,
            Department = "Research",
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = chapters.Take(5).Select((ch, i) => new ChartDataPoint
            {
                Label = ch.Length > 20 ? ch[..17] + "..." : ch,
                Value = 1,
                Color = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6" }[i % 5]
            }).ToList(),
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Chapters", Value = chapters.Count, Color = "#3b82f6" }
            },
            Metrics = new SummaryMetrics { TotalActivities = chapters.Count, CompletedCount = chapters.Count },
            Conclusion = "Continue research progress according to timeline.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static string ExtractResearchProgress(string text)
    {
        var lower = text.ToLower();
        if (lower.Contains("completed") || lower.Contains("finished"))
            return "Research completed.";
        if (lower.Contains("in progress") || lower.Contains("ongoing"))
            return "Research in progress.";
        if (lower.Contains("proposal") || lower.Contains("planning"))
            return "Research planning phase.";
        return "Research status not specified.";
    }

    private static string BuildResearchFormattedText(SummaryReportRequest req, string raw, List<string> chapters, string progress)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("            RESEARCH/THESIS REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("RESEARCHER INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Researcher  : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Report Date : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine($"Status      : {progress}");

        if (chapters.Any())
        {
            sb.AppendLine();
            sb.AppendLine("RESEARCH STRUCTURE");
            sb.AppendLine(thin);
            for (int i = 0; i < chapters.Count; i++)
                sb.AppendLine($"{i + 1}. {chapters[i]}");
        }

        sb.AppendLine();
        sb.AppendLine("RESEARCH DETAILS");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // Attendance Report
    private static bool IsAttendanceReport(string text)
    {
        var lower = text.ToLower();
        var attendanceKeywords = new[]
        {
            "attendance", "present", "absent", "tardy", "late", "excused", "unexcused",
            "classes", "days", "attended", "missed", "attendance rate", "punctuality"
        };
        
        return attendanceKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateAttendanceReport(SummaryReportRequest request, string raw)
    {
        var present = ExtractAttendanceCount(raw, "present");
        var absent = ExtractAttendanceCount(raw, "absent");
        var late = ExtractAttendanceCount(raw, "late");
        var total = present + absent;
        var rate = total > 0 ? (present * 100.0 / total) : 0;
        
        var summary = $"Attendance report: {present} present, {absent} absent, {late} late. Attendance rate: {rate:F1}%.";

        var formatted = BuildAttendanceFormattedText(request, raw, present, absent, late, rate);

        return new SummaryReportResult
        {
            Title = "Attendance Report",
            AuthorName = request.AuthorName,
            Department = "Student",
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = new List<ActivityGroup>
            {
                new() { Category = "Attendance Summary", Items = new List<string>
                {
                    $"Present: {present} days",
                    $"Absent: {absent} days",
                    $"Late: {late} times",
                    $"Attendance Rate: {rate:F1}%"
                }}
            },
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Present", Value = present, Color = "#10b981" },
                new() { Label = "Absent", Value = absent, Color = "#ef4444" },
                new() { Label = "Late", Value = late, Color = "#f59e0b" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "Present", Value = present, Color = "#10b981" },
                new() { Label = "Absent", Value = absent, Color = "#ef4444" }
            },
            Metrics = new SummaryMetrics { TotalActivities = total, CompletedCount = present, CompletionRate = rate },
            Conclusion = rate >= 95 ? "Excellent attendance!" : rate >= 85 ? "Good attendance." : "Attendance needs improvement.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static int ExtractAttendanceCount(string text, string type)
    {
        var match = Regex.Match(text, $@"{type}[:\s]+(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var count))
            return count;
        return 0;
    }

    private static string BuildAttendanceFormattedText(SummaryReportRequest req, string raw, int present, int absent, int late, double rate)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("              ATTENDANCE REPORT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("STUDENT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Student     : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Report Date : {DateTime.Now:MMMM dd, yyyy}");

        sb.AppendLine();
        sb.AppendLine("ATTENDANCE SUMMARY");
        sb.AppendLine(thin);
        sb.AppendLine($"Present     : {present} days");
        sb.AppendLine($"Absent      : {absent} days");
        sb.AppendLine($"Late        : {late} times");
        sb.AppendLine($"Total Days  : {present + absent}");
        sb.AppendLine($"Rate        : {rate:F1}%");

        sb.AppendLine();
        sb.AppendLine("ATTENDANCE DETAILS");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // Grade/Transcript Report
    private static bool IsGradeReport(string text)
    {
        var lower = text.ToLower();
        var gradeKeywords = new[]
        {
            "grade", "grades", "transcript", "marks", "score", "final grade",
            "midterm", "final exam", "quiz", "test score", "percentage", "letter grade",
            "a+", "a-", "b+", "b-", "c+", "passed", "failed"
        };
        
        return gradeKeywords.Count(k => lower.Contains(k)) >= 4;
    }

    private async Task<SummaryReportResult> GenerateGradeReport(SummaryReportRequest request, string raw)
    {
        var grades = ExtractGrades(raw);
        var gpa = ExtractGPA(raw);
        var passed = grades.Count(g => !g.ToLower().Contains("fail"));
        var failed = grades.Count - passed;
        
        var summary = $"Grade report: {grades.Count} course{(grades.Count != 1 ? "s" : "")} graded. " +
                     $"{passed} passed, {failed} failed. " +
                     $"{(gpa > 0 ? $"GPA: {gpa:F2}" : "")}";

        var formatted = BuildGradeFormattedText(request, raw, grades, gpa);

        var activityGroups = new List<ActivityGroup>();
        if (grades.Any())
            activityGroups.Add(new() { Category = "Course Grades", Items = grades });

        return new SummaryReportResult
        {
            Title = "Grade Report / Transcript",
            AuthorName = request.AuthorName,
            Department = "Student",
            Period = DateTime.Now.ToString("MMMM yyyy"),
            ExecutiveSummary = summary,
            ActivityGroups = activityGroups,
            ActivityChart = new List<ChartDataPoint>
            {
                new() { Label = "Passed", Value = passed, Color = "#10b981" },
                new() { Label = "Failed", Value = failed, Color = "#ef4444" }
            },
            StatusChart = new List<ChartDataPoint>
            {
                new() { Label = "GPA", Value = (int)(gpa * 25), Color = gpa >= 3.5m ? "#10b981" : gpa >= 2.5m ? "#3b82f6" : "#f59e0b" }
            },
            Metrics = new SummaryMetrics 
            { 
                TotalActivities = grades.Count, 
                CompletedCount = passed,
                CompletionRate = grades.Count > 0 ? passed * 100.0 / grades.Count : 0
            },
            Conclusion = gpa >= 3.5m ? "Outstanding academic achievement!" : gpa >= 2.5m ? "Satisfactory academic performance." : "Academic improvement recommended.",
            FormattedText = formatted,
            AiProvider = "Heuristic",
            AiEnhanced = false
        };
    }

    private static List<string> ExtractGrades(string text)
    {
        var grades = new List<string>();
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"[A-F][+-]?|\d+%|passed|failed|grade", RegexOptions.IgnoreCase) && line.Trim().Length > 5)
            {
                grades.Add(line.Trim());
            }
        }
        
        return grades;
    }

    private static string BuildGradeFormattedText(SummaryReportRequest req, string raw, List<string> grades, decimal gpa)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("           GRADE REPORT / TRANSCRIPT");
        sb.AppendLine(line);
        sb.AppendLine();
        sb.AppendLine("STUDENT INFORMATION");
        sb.AppendLine(thin);
        sb.AppendLine($"Student     : {req.AuthorName ?? "N/A"}");
        sb.AppendLine($"Report Date : {DateTime.Now:MMMM dd, yyyy}");
        if (gpa > 0)
            sb.AppendLine($"GPA         : {gpa:F2}");

        if (grades.Any())
        {
            sb.AppendLine();
            sb.AppendLine("COURSE GRADES");
            sb.AppendLine(thin);
            foreach (var grade in grades)
                sb.AppendLine($"  • {grade}");
        }

        sb.AppendLine();
        sb.AppendLine("GRADE DETAILS");
        sb.AppendLine(thin);
        sb.AppendLine(raw);

        sb.AppendLine();
        sb.AppendLine(line);
        return sb.ToString();
    }

    // ── Categorization ─────────────────────────────────────────────────────
    private static Dictionary<string, List<ActivityEntry>> CategorizeActivities(List<ActivityEntry> activities)
    {
        var result = new Dictionary<string, List<ActivityEntry>>();

        foreach (var activity in activities)
        {
            var lower = activity.Text.ToLower();
            var matched = false;

            foreach (var (category, keywords) in CategoryKeywords)
            {
                if (keywords.Any(k => lower.Contains(k)))
                {
                    if (!result.ContainsKey(category)) result[category] = new();
                    result[category].Add(activity);
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                const string other = "General Activities";
                if (!result.ContainsKey(other)) result[other] = new();
                result[other].Add(activity);
            }
        }

        return result;
    }

    // ── Status inference ───────────────────────────────────────────────────
    private static string InferStatus(string sentence)
    {
        var lower = sentence.ToLower();
        if (PendingKeywords.Any(k => lower.Contains(k)))    return "Pending";
        if (InProgressKeywords.Any(k => lower.Contains(k))) return "In Progress";
        return "Completed";
    }

    // ── Metrics ────────────────────────────────────────────────────────────
    private static SummaryMetrics BuildMetrics(List<ActivityEntry> activities, string raw)
    {
        var completed   = activities.Count(a => a.Status == "Completed");
        var inProgress  = activities.Count(a => a.Status == "In Progress");
        var pending     = activities.Count(a => a.Status == "Pending");
        var total       = activities.Count;

        return new SummaryMetrics
        {
            TotalActivities = total,
            CompletedCount  = completed,
            InProgressCount = inProgress,
            PendingCount    = pending,
            CompletionRate  = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0,
            WordCount       = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
        };
    }

    // ── Chart data ─────────────────────────────────────────────────────────
    private static List<ChartDataPoint> BuildActivityChart(Dictionary<string, List<ActivityEntry>> groups)
    {
        var colors = new[] { "#1e40af","#3b82f6","#60a5fa","#10b981","#f59e0b","#8b5cf6","#ef4444" };
        return groups
            .OrderByDescending(g => g.Value.Count)
            .Select((g, i) => new ChartDataPoint
            {
                Label = g.Key,
                Value = g.Value.Count,
                Color = colors[i % colors.Length]
            })
            .ToList();
    }

    private static List<ChartDataPoint> BuildStatusChart(SummaryMetrics m) =>
    [
        new() { Label = "Completed",   Value = m.CompletedCount,  Color = "#10b981" },
        new() { Label = "In Progress", Value = m.InProgressCount, Color = "#f59e0b" },
        new() { Label = "Pending",     Value = m.PendingCount,    Color = "#ef4444" },
    ];

    // ── Narrative generation ───────────────────────────────────────────────
    private static string BuildExecutiveSummary(
        List<ActivityEntry> activities,
        Dictionary<string, List<ActivityEntry>> groups,
        string author, string dept, string period)
    {
        var sb = new StringBuilder();
        var who = !string.IsNullOrWhiteSpace(author) ? author
                : !string.IsNullOrWhiteSpace(dept)   ? $"the {dept} team"
                : "the team";

        sb.Append($"During {period}, {who} carried out a total of {activities.Count} recorded activit{(activities.Count == 1 ? "y" : "ies")} across {groups.Count} area{(groups.Count == 1 ? "" : "s")}. ");

        var topGroups = groups.OrderByDescending(g => g.Value.Count).Take(3).ToList();
        if (topGroups.Any())
        {
            sb.Append("Primary focus areas included ");
            sb.Append(string.Join(", ", topGroups.Select(g => g.Key.ToLower())));
            sb.Append(". ");
        }

        var completed = activities.Count(a => a.Status == "Completed");
        var rate = activities.Count > 0 ? (double)completed / activities.Count * 100 : 0;
        sb.Append($"An overall completion rate of {rate:F0}% was achieved");

        var pending = activities.Count(a => a.Status == "Pending");
        if (pending > 0)
            sb.Append($", with {pending} item{(pending == 1 ? "" : "s")} still pending follow-up");

        sb.Append('.');
        return sb.ToString();
    }

    private static string BuildConclusion(SummaryMetrics m)
    {
        if (m.TotalActivities == 0) return string.Empty;

        if (m.CompletionRate >= 90)
            return $"Overall performance was excellent, with {m.CompletionRate}% of activities successfully completed. Continued momentum is encouraged for the next period.";
        if (m.CompletionRate >= 70)
            return $"Performance was satisfactory with a {m.CompletionRate}% completion rate. Attention should be given to the {m.PendingCount + m.InProgressCount} remaining item(s) to ensure timely resolution.";
        return $"The period recorded a {m.CompletionRate}% completion rate. It is recommended to review and prioritize the {m.PendingCount} pending and {m.InProgressCount} in-progress items in the upcoming period.";
    }

    // ── Plain text formatted output ────────────────────────────────────────
    private static string BuildFormattedText(
        SummaryReportRequest req,
        Dictionary<string, List<ActivityEntry>> groups,
        string summary, string conclusion,
        SummaryMetrics metrics, string period,
        string? enhancedInsights = null)
    {
        // Determine template type from report type
        var reportType = req.ReportType.ToLower();
        
        if (reportType.Contains("daily"))
            return BuildDailyFormat(req, groups, summary, conclusion, metrics, period, enhancedInsights);
        if (reportType.Contains("weekly"))
            return BuildWeeklyFormat(req, groups, summary, conclusion, metrics, period, enhancedInsights);
        if (reportType.Contains("work log") || reportType.Contains("worklog"))
            return BuildWorkLogFormat(req, groups, summary, conclusion, metrics, period, enhancedInsights);
        
        // Default general format
        return BuildGeneralFormat(req, groups, summary, conclusion, metrics, period, enhancedInsights);
    }

    private static string BuildDailyFormat(
        SummaryReportRequest req,
        Dictionary<string, List<ActivityEntry>> groups,
        string summary, string conclusion,
        SummaryMetrics metrics, string period,
        string? enhancedInsights = null)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("              DAILY ACCOMPLISHMENT REPORT");
        sb.AppendLine(line);
        sb.AppendLine();

        sb.AppendLine("EMPLOYEE INFORMATION");
        sb.AppendLine(thin);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Name       : {req.AuthorName}");
        if (!string.IsNullOrWhiteSpace(req.Department))
            sb.AppendLine($"Department : {req.Department}");
        sb.AppendLine($"Date       : {period}");
        sb.AppendLine($"Generated  : {DateTime.Now:MMMM dd, yyyy hh:mm tt}");

        sb.AppendLine();
        sb.AppendLine("PERFORMANCE METRICS");
        sb.AppendLine(thin);
        sb.AppendLine($"Total Tasks      : {metrics.TotalActivities}");
        sb.AppendLine($"Completed        : {metrics.CompletedCount} ({metrics.CompletionRate:F0}%)");
        sb.AppendLine($"In Progress      : {metrics.InProgressCount}");
        sb.AppendLine($"Pending          : {metrics.PendingCount}");
        var progressBar = new string('█', (int)(metrics.CompletionRate / 5)) + 
                         new string('░', 20 - (int)(metrics.CompletionRate / 5));
        sb.AppendLine($"Completion Rate  : {progressBar} {metrics.CompletionRate:F0}%");

        sb.AppendLine();
        sb.AppendLine("TASKS ACCOMPLISHED");
        sb.AppendLine(thin);
        foreach (var (category, items) in groups.OrderByDescending(g => g.Value.Count))
        {
            sb.AppendLine();
            sb.AppendLine($"▸ {category.ToUpper()}");
            foreach (var item in items)
                sb.AppendLine($"  • {item.Text} — {item.Status}");
        }

        sb.AppendLine();
        sb.AppendLine("SUMMARY");
        sb.AppendLine(thin);
        sb.AppendLine(summary);

        // Add enhanced insights if available
        if (!string.IsNullOrWhiteSpace(enhancedInsights))
        {
            sb.AppendLine(enhancedInsights);
        }

        if (!string.IsNullOrWhiteSpace(conclusion))
        {
            sb.AppendLine();
            sb.AppendLine("NOTES & REMARKS");
            sb.AppendLine(thin);
            sb.AppendLine(conclusion);
        }

        sb.AppendLine();
        sb.AppendLine(line);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Prepared by: {req.AuthorName}");
        sb.AppendLine($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}");
        sb.AppendLine(line);

        return sb.ToString();
    }

    private static string BuildWeeklyFormat(
        SummaryReportRequest req,
        Dictionary<string, List<ActivityEntry>> groups,
        string summary, string conclusion,
        SummaryMetrics metrics, string period,
        string? enhancedInsights = null)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("                WEEKLY SUMMARY REPORT");
        sb.AppendLine(line);
        sb.AppendLine();

        sb.AppendLine("REPORTING PERIOD");
        sb.AppendLine(thin);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Employee   : {req.AuthorName}");
        if (!string.IsNullOrWhiteSpace(req.Department))
            sb.AppendLine($"Department : {req.Department}");
        sb.AppendLine($"Week Of    : {period}");
        sb.AppendLine($"Report Date: {DateTime.Now:MMMM dd, yyyy}");

        sb.AppendLine();
        sb.AppendLine("WEEKLY METRICS");
        sb.AppendLine(thin);
        sb.AppendLine($"Total Activities : {metrics.TotalActivities}");
        sb.AppendLine($"Completed        : {metrics.CompletedCount} tasks");
        sb.AppendLine($"In Progress      : {metrics.InProgressCount} tasks");
        sb.AppendLine($"Pending          : {metrics.PendingCount} tasks");
        sb.AppendLine($"Success Rate     : {metrics.CompletionRate:F0}%");
        sb.AppendLine();
        var progressBar = new string('█', (int)(metrics.CompletionRate / 5)) + 
                         new string('░', 20 - (int)(metrics.CompletionRate / 5));
        sb.AppendLine($"Progress Bar: {progressBar} {metrics.CompletionRate:F0}%");

        sb.AppendLine();
        sb.AppendLine("ACCOMPLISHMENTS & ACTIVITIES");
        sb.AppendLine(thin);
        foreach (var (category, items) in groups.OrderByDescending(g => g.Value.Count))
        {
            sb.AppendLine();
            sb.AppendLine($"▸ {category.ToUpper()}");
            foreach (var item in items)
                sb.AppendLine($"  • {item.Text} — {item.Status}");
        }

        sb.AppendLine();
        sb.AppendLine("WEEKLY INSIGHTS");
        sb.AppendLine(thin);
        var weeklyInsight = metrics.CompletionRate >= 80
            ? "Strong weekly performance with consistent task completion."
            : metrics.CompletionRate >= 60
            ? "Solid weekly progress with good momentum on key deliverables."
            : metrics.CompletionRate >= 40
            ? "Moderate weekly output, consider prioritizing high-impact tasks."
            : "Week requires attention, focus on completing pending items.";
        sb.AppendLine(weeklyInsight);

        sb.AppendLine();
        sb.AppendLine("KEY HIGHLIGHTS");
        sb.AppendLine(thin);
        sb.AppendLine($"✓ Completed {metrics.CompletedCount} out of {metrics.TotalActivities} planned activities");
        sb.AppendLine($"✓ {metrics.InProgressCount} task{(metrics.InProgressCount != 1 ? "s" : "")} currently in progress");
        if (metrics.PendingCount > 0)
            sb.AppendLine($"⚠ {metrics.PendingCount} task{(metrics.PendingCount != 1 ? "s" : "")} pending attention");
        else
            sb.AppendLine("✓ No pending tasks");

        // Add enhanced insights if available
        if (!string.IsNullOrWhiteSpace(enhancedInsights))
        {
            sb.AppendLine(enhancedInsights);
        }

        if (!string.IsNullOrWhiteSpace(conclusion))
        {
            sb.AppendLine();
            sb.AppendLine("ADDITIONAL NOTES");
            sb.AppendLine(thin);
            sb.AppendLine(conclusion);
        }

        sb.AppendLine();
        sb.AppendLine(line);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Prepared by: {req.AuthorName}");
        sb.AppendLine($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}");
        sb.AppendLine(line);

        return sb.ToString();
    }

    private static string BuildWorkLogFormat(
        SummaryReportRequest req,
        Dictionary<string, List<ActivityEntry>> groups,
        string summary, string conclusion,
        SummaryMetrics metrics, string period,
        string? enhancedInsights = null)
    {
        var sb = new StringBuilder();
        var line = new string('═', 62);
        var thin = new string('─', 62);

        sb.AppendLine(line);
        sb.AppendLine("                  WORK LOG REPORT");
        sb.AppendLine(line);
        sb.AppendLine();

        sb.AppendLine("WORK SESSION DETAILS");
        sb.AppendLine(thin);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Employee   : {req.AuthorName}");
        if (!string.IsNullOrWhiteSpace(req.Department))
            sb.AppendLine($"Department : {req.Department}");
        sb.AppendLine($"Date       : {period}");
        sb.AppendLine($"Generated  : {DateTime.Now:MMMM dd, yyyy hh:mm tt}");

        sb.AppendLine();
        sb.AppendLine("PRODUCTIVITY METRICS");
        sb.AppendLine(thin);
        sb.AppendLine($"Total Entries    : {metrics.TotalActivities}");
        sb.AppendLine($"Completed        : {metrics.CompletedCount}");
        sb.AppendLine($"In Progress      : {metrics.InProgressCount}");
        sb.AppendLine($"Pending          : {metrics.PendingCount}");
        
        var productivity = metrics.TotalActivities > 0 ? metrics.TotalActivities / 8.0 : 0; // Assume 8hr day
        sb.AppendLine($"Tasks/Hour       : {productivity:F2}");
        
        var productivityRating = productivity >= 2 
            ? "High productivity - multiple tasks per hour"
            : productivity >= 1 
            ? "Good productivity - steady task completion"
            : productivity >= 0.5
            ? "Moderate productivity - focus on efficiency"
            : "Low productivity - consider time management strategies";
        sb.AppendLine($"Efficiency       : {productivityRating}");

        sb.AppendLine();
        sb.AppendLine("WORK LOG ENTRIES");
        sb.AppendLine(thin);
        foreach (var (category, items) in groups.OrderByDescending(g => g.Value.Count))
        {
            sb.AppendLine();
            sb.AppendLine($"▸ {category.ToUpper()}");
            foreach (var item in items)
                sb.AppendLine($"  • {item.Text} — {item.Status}");
        }

        sb.AppendLine();
        sb.AppendLine("TIME ALLOCATION SUMMARY");
        sb.AppendLine(thin);
        var completedPct = metrics.TotalActivities > 0 ? metrics.CompletedCount * 100.0 / metrics.TotalActivities : 0;
        var inProgressPct = metrics.TotalActivities > 0 ? metrics.InProgressCount * 100.0 / metrics.TotalActivities : 0;
        var pendingPct = metrics.TotalActivities > 0 ? metrics.PendingCount * 100.0 / metrics.TotalActivities : 0;
        
        sb.AppendLine($"• Completed Tasks    : {metrics.CompletedCount} ({completedPct:F0}%)");
        sb.AppendLine($"• Ongoing Work       : {metrics.InProgressCount} ({inProgressPct:F0}%)");
        sb.AppendLine($"• Pending Items      : {metrics.PendingCount} ({pendingPct:F0}%)");

        sb.AppendLine();
        sb.AppendLine("PRODUCTIVITY ANALYSIS");
        sb.AppendLine(thin);
        sb.AppendLine(productivityRating);
        if (metrics.TotalActivities > 0)
        {
            var avgTime = 480.0 / metrics.TotalActivities; // 480 min = 8 hours
            sb.AppendLine($"Average time per task: {avgTime:F1} minutes");
        }

        // Add enhanced insights if available
        if (!string.IsNullOrWhiteSpace(enhancedInsights))
        {
            sb.AppendLine(enhancedInsights);
        }

        if (!string.IsNullOrWhiteSpace(conclusion))
        {
            sb.AppendLine();
            sb.AppendLine("REMARKS & OBSERVATIONS");
            sb.AppendLine(thin);
            sb.AppendLine(conclusion);
        }

        sb.AppendLine();
        sb.AppendLine(line);
        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Prepared by: {req.AuthorName}");
        sb.AppendLine($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}");
        sb.AppendLine(line);

        return sb.ToString();
    }

    private static string BuildGeneralFormat(
        SummaryReportRequest req,
        Dictionary<string, List<ActivityEntry>> groups,
        string summary, string conclusion,
        SummaryMetrics metrics, string period,
        string? enhancedInsights = null)
    {
        var sb = new StringBuilder();
        var line = new string('═', 50);
        var thin = new string('─', 50);

        sb.AppendLine(req.ReportType.ToUpper());
        sb.AppendLine(line);

        if (!string.IsNullOrWhiteSpace(req.AuthorName))
            sb.AppendLine($"Prepared by : {req.AuthorName}");
        if (!string.IsNullOrWhiteSpace(req.Department))
            sb.AppendLine($"Department  : {req.Department}");
        sb.AppendLine($"Period      : {period}");
        sb.AppendLine($"Generated   : {DateTime.Now:MMMM dd, yyyy}");
        sb.AppendLine(thin);

        sb.AppendLine();
        sb.AppendLine("EXECUTIVE SUMMARY");
        sb.AppendLine(thin);
        sb.AppendLine(summary);

        sb.AppendLine();
        sb.AppendLine("KEY ACTIVITIES");
        sb.AppendLine(thin);

        foreach (var (category, items) in groups.OrderByDescending(g => g.Value.Count))
        {
            sb.AppendLine();
            sb.AppendLine($"▸ {category.ToUpper()}");
            foreach (var item in items)
                sb.AppendLine($"  • {item.Text}");
        }

        sb.AppendLine();
        sb.AppendLine("METRICS SUMMARY");
        sb.AppendLine(thin);
        sb.AppendLine($"  Total Activities : {metrics.TotalActivities}");
        sb.AppendLine($"  Completed        : {metrics.CompletedCount}");
        sb.AppendLine($"  In Progress      : {metrics.InProgressCount}");
        sb.AppendLine($"  Pending          : {metrics.PendingCount}");
        sb.AppendLine($"  Completion Rate  : {metrics.CompletionRate}%");

        // Add enhanced insights if available
        if (!string.IsNullOrWhiteSpace(enhancedInsights))
        {
            sb.AppendLine(enhancedInsights);
        }

        if (!string.IsNullOrWhiteSpace(conclusion))
        {
            sb.AppendLine();
            sb.AppendLine("CONCLUSION");
            sb.AppendLine(thin);
            sb.AppendLine(conclusion);
        }

        return sb.ToString();
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static string? DetectPeriod(string text)
    {
        var m = DatePattern.Match(text);
        if (m.Success && DateTime.TryParse(m.Value, out var dt))
            return dt.ToString("MMMM yyyy");
        return null;
    }

    private static string Formalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var s = input.TrimStart('-', '*', '•', ' ');
        s = char.ToUpper(s[0]) + s[1..];
        if (!s.EndsWith('.') && !s.EndsWith('!') && !s.EndsWith('?')) s += ".";
        return s;
    }

    // ── Spelling & Grammar Correction Helpers ──────────────────────────────
    
    /// <summary>
    /// Normalizes text by correcting common misspellings and grammar issues
    /// </summary>
    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        
        var normalized = text;
        
        // Common misspellings dictionary
        var corrections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Financial terms
            { "epense", "expense" },
            { "expence", "expense" },
            { "expens", "expense" },
            { "salery", "salary" },
            { "sallary", "salary" },
            { "incom", "income" },
            { "incme", "income" },
            { "revenu", "revenue" },
            { "budjet", "budget" },
            { "buget", "budget" },
            
            // Common words
            { "recieve", "receive" },
            { "recieved", "received" },
            { "occured", "occurred" },
            { "occurence", "occurrence" },
            { "seperate", "separate" },
            { "definately", "definitely" },
            { "accomodate", "accommodate" },
            { "necesary", "necessary" },
            { "necesity", "necessity" },
            { "neccesity", "necessity" },
            { "necesities", "necessities" },
            
            // Work/task terms
            { "complted", "completed" },
            { "complet", "completed" },
            { "finised", "finished" },
            { "finsh", "finish" },
            { "taks", "task" },
            { "tak", "task" },
            { "meting", "meeting" },
            { "meetting", "meeting" },
            { "atended", "attended" },
            { "attendd", "attended" },
            
            // Status terms
            { "pening", "pending" },
            { "pendng", "pending" },
            { "progres", "progress" },
            { "proggress", "progress" },
            
            // Academic terms
            { "asignment", "assignment" },
            { "assignement", "assignment" },
            { "homwork", "homework" },
            { "attendence", "attendance" },
            { "absense", "absence" },
            { "absance", "absence" },
            { "gradde", "grade" },
            { "cours", "course" },
            { "corse", "course" },
            
            // Time/date
            { "januray", "january" },
            { "febuary", "february" },
            { "feburary", "february" },
            { "wendsday", "wednesday" },
            { "wensday", "wednesday" },
            { "thrusday", "thursday" },
            { "thirsday", "thursday" },
            
            // Other common
            { "reccomend", "recommend" },
            { "recomend", "recommend" },
            { "occassion", "occasion" },
            { "ocasion", "occasion" },
            { "untill", "until" },
            { "sucessful", "successful" },
            { "succesful", "successful" },
        };
        
        // Apply corrections using word boundaries
        foreach (var (wrong, correct) in corrections)
        {
            normalized = Regex.Replace(normalized, $@"\b{Regex.Escape(wrong)}\b", correct, RegexOptions.IgnoreCase);
        }
        
        // Fix common grammar issues
        normalized = FixCommonGrammar(normalized);
        
        // Normalize spacing around punctuation
        normalized = Regex.Replace(normalized, @"\s+([,.])", "$1"); // Remove space before comma/period
        normalized = Regex.Replace(normalized, @"([,.])\s*", "$1 "); // Add space after comma/period
        normalized = Regex.Replace(normalized, @"\s+", " "); // Normalize multiple spaces
        
        return normalized.Trim();
    }
    
    /// <summary>
    /// Fixes common grammar issues
    /// </summary>
    private static string FixCommonGrammar(string text)
    {
        // Fix "a" vs "an"
        text = Regex.Replace(text, @"\ba\s+([aeiou])", "an $1", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\ban\s+([^aeiou])", "a $1", RegexOptions.IgnoreCase);
        
        // Fix double spaces
        text = Regex.Replace(text, @"\s{2,}", " ");
        
        // Fix missing spaces after periods
        text = Regex.Replace(text, @"\.([A-Z])", ". $1");
        
        // Fix missing spaces after commas
        text = Regex.Replace(text, @",([^\s\d])", ", $1");
        
        return text;
    }
    
    /// <summary>
    /// Fuzzy match for keywords with tolerance for typos
    /// </summary>
    private static bool FuzzyContains(string text, string keyword, int maxDistance = 2)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
            return false;
        
        var lowerText = text.ToLower();
        var lowerKeyword = keyword.ToLower();
        
        // Exact match
        if (lowerText.Contains(lowerKeyword))
            return true;
        
        // Check for close matches using Levenshtein distance
        var words = lowerText.Split(new[] { ' ', ',', '.', ';', ':', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            if (LevenshteinDistance(word, lowerKeyword) <= maxDistance)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculate Levenshtein distance between two strings
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1)) return s2?.Length ?? 0;
        if (string.IsNullOrEmpty(s2)) return s1.Length;
        
        var len1 = s1.Length;
        var len2 = s2.Length;
        var matrix = new int[len1 + 1, len2 + 1];
        
        for (int i = 0; i <= len1; i++) matrix[i, 0] = i;
        for (int j = 0; j <= len2; j++) matrix[0, j] = j;
        
        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }
        
        return matrix[len1, len2];
    }

    // ══════════════════════════════════════════════════════════════════════
    // SMART ENHANCEMENTS - Context-Aware Analysis
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Analyze sentiment of text (positive, negative, neutral)
    /// </summary>
    private static (string Sentiment, int Score) AnalyzeSentiment(string text)
    {
        var lower = text.ToLower();
        var positiveCount = PositiveKeywords.Count(k => lower.Contains(k));
        var negativeCount = NegativeKeywords.Count(k => lower.Contains(k));
        
        var score = positiveCount - negativeCount;
        
        if (score > 2) return ("Positive", score);
        if (score < -2) return ("Negative", score);
        return ("Neutral", score);
    }

    /// <summary>
    /// Calculate priority score for an item (0-100)
    /// </summary>
    private static int CalculatePriority(string text)
    {
        var lower = text.ToLower();
        var score = 50; // Base priority
        
        // High priority keywords
        if (HighPriorityKeywords.Any(k => lower.Contains(k)))
            score += 30;
        
        // Negative sentiment increases priority
        if (NegativeKeywords.Any(k => lower.Contains(k)))
            score += 15;
        
        // Deadlines or dates increase priority
        if (DatePattern.IsMatch(text))
            score += 10;
        
        // Numbers suggest measurable items (higher priority)
        if (Regex.IsMatch(text, @"\d+"))
            score += 5;
        
        return Math.Min(100, Math.Max(0, score));
    }

    /// <summary>
    /// Detect opportunities in text
    /// </summary>
    private static List<string> DetectOpportunities(string text)
    {
        var opportunities = new List<string>();
        var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var lower = line.ToLower();
            if (OpportunityKeywords.Any(k => lower.Contains(k)))
            {
                opportunities.Add(line.Trim());
            }
        }
        
        return opportunities.Take(5).ToList(); // Top 5 opportunities
    }

    /// <summary>
    /// Detect risks in text
    /// </summary>
    private static List<string> DetectRisks(string text)
    {
        var risks = new List<string>();
        var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var lower = line.ToLower();
            if (RiskKeywords.Any(k => lower.Contains(k)))
            {
                risks.Add(line.Trim());
            }
        }
        
        return risks.Take(5).ToList(); // Top 5 risks
    }

    /// <summary>
    /// Generate smart recommendations based on context
    /// </summary>
    private static List<string> GenerateRecommendations(string text, string reportType)
    {
        var recommendations = new List<string>();
        var lower = text.ToLower();
        
        // Financial recommendations
        if (reportType.Contains("Financial") || reportType.Contains("Budget"))
        {
            if (lower.Contains("expense") && Regex.IsMatch(text, @"\d{1,3}(?:[,\s]\d{3})+"))
            {
                recommendations.Add("Consider creating a detailed expense breakdown by category");
                recommendations.Add("Set up monthly budget alerts to track spending patterns");
            }
            if (lower.Contains("savings") || lower.Contains("balance"))
            {
                recommendations.Add("Aim to maintain 3-6 months of expenses as emergency fund");
            }
        }
        
        // Academic recommendations
        if (reportType.Contains("Academic") || reportType.Contains("Student"))
        {
            if (lower.Contains("grade") || lower.Contains("score"))
            {
                recommendations.Add("Focus on subjects with lower performance for improvement");
                recommendations.Add("Consider forming study groups for challenging topics");
            }
            if (lower.Contains("attendance") && lower.Contains("absent"))
            {
                recommendations.Add("Improve attendance to ensure better academic performance");
            }
        }
        
        // Meeting recommendations
        if (reportType.Contains("Meeting"))
        {
            if (lower.Contains("action") || lower.Contains("follow"))
            {
                recommendations.Add("Schedule follow-up meeting to review action item progress");
                recommendations.Add("Assign clear owners and deadlines for each action item");
            }
        }
        
        // Performance recommendations
        if (reportType.Contains("Performance") || reportType.Contains("Review"))
        {
            recommendations.Add("Set SMART goals for the next review period");
            recommendations.Add("Schedule regular check-ins to track progress");
        }
        
        // Strategy recommendations
        if (reportType.Contains("Strategy") || reportType.Contains("Planning"))
        {
            recommendations.Add("Define clear KPIs to measure strategy success");
            recommendations.Add("Conduct quarterly reviews to adjust strategy as needed");
        }
        
        // Generic recommendations based on sentiment
        var (sentiment, _) = AnalyzeSentiment(text);
        if (sentiment == "Negative")
        {
            recommendations.Add("Address identified issues promptly to prevent escalation");
        }
        else if (sentiment == "Positive")
        {
            recommendations.Add("Document successful practices for future reference");
        }
        
        return recommendations.Take(5).ToList();
    }

    /// <summary>
    /// Generate key insights from text
    /// </summary>
    private static List<string> GenerateInsights(string text, SummaryMetrics metrics)
    {
        var insights = new List<string>();
        
        // Completion insights
        if (metrics.CompletionRate >= 90)
            insights.Add($"✓ Excellent completion rate of {metrics.CompletionRate:F0}% demonstrates strong execution");
        else if (metrics.CompletionRate >= 70)
            insights.Add($"⚠ Completion rate of {metrics.CompletionRate:F0}% is good but has room for improvement");
        else if (metrics.CompletionRate < 50)
            insights.Add($"⚠ Low completion rate of {metrics.CompletionRate:F0}% requires immediate attention");
        
        // Activity volume insights
        if (metrics.TotalActivities > 20)
            insights.Add($"High activity volume ({metrics.TotalActivities} items) indicates busy period");
        else if (metrics.TotalActivities < 5)
            insights.Add($"Low activity volume ({metrics.TotalActivities} items) - consider if this reflects actual workload");
        
        // Pending items insights
        if (metrics.PendingCount > metrics.CompletedCount)
            insights.Add($"⚠ More pending items ({metrics.PendingCount}) than completed ({metrics.CompletedCount}) - prioritization needed");
        
        // Sentiment insights
        var (sentiment, score) = AnalyzeSentiment(text);
        if (sentiment == "Positive" && score > 3)
            insights.Add("✓ Overall positive tone indicates successful outcomes and good morale");
        else if (sentiment == "Negative" && score < -3)
            insights.Add("⚠ Negative tone detected - may indicate challenges or concerns requiring attention");
        
        return insights;
    }

    /// <summary>
    /// Build enhanced insights section for formatted text
    /// </summary>
    private static string BuildEnhancedInsightsSection(string text, string reportType, SummaryMetrics metrics)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine();
        sb.AppendLine("SMART INSIGHTS & ANALYSIS");
        sb.AppendLine("──────────────────────────────────────────────────────────────");
        
        // Key insights
        var insights = GenerateInsights(text, metrics);
        if (insights.Any())
        {
            sb.AppendLine();
            sb.AppendLine("▸ KEY INSIGHTS");
            foreach (var insight in insights)
            {
                sb.AppendLine($"  • {insight}");
            }
        }
        
        // Opportunities
        var opportunities = DetectOpportunities(text);
        if (opportunities.Any())
        {
            sb.AppendLine();
            sb.AppendLine("▸ OPPORTUNITIES IDENTIFIED");
            foreach (var opp in opportunities)
            {
                sb.AppendLine($"  • {opp}");
            }
        }
        
        // Risks
        var risks = DetectRisks(text);
        if (risks.Any())
        {
            sb.AppendLine();
            sb.AppendLine("▸ RISKS & CONCERNS");
            foreach (var risk in risks)
            {
                sb.AppendLine($"  ⚠ {risk}");
            }
        }
        
        // Recommendations
        var recommendations = GenerateRecommendations(text, reportType);
        if (recommendations.Any())
        {
            sb.AppendLine();
            sb.AppendLine("▸ RECOMMENDATIONS");
            foreach (var rec in recommendations)
            {
                sb.AppendLine($"  • {rec}");
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Add AI enhancement to any report type
    /// </summary>
    private async Task<string> GetAiEnhancedInsightsAsync(string rawText, string reportType, SummaryMetrics metrics)
    {
        if (_aiService == null)
            return string.Empty;

        try
        {
            var (aiInsights, provider) = await _aiService.EnhanceReportAsync(rawText, reportType, metrics);
            if (!string.IsNullOrWhiteSpace(aiInsights))
            {
                return FormatAiInsights(aiInsights, provider);
            }
        }
        catch (Exception)
        {
            // If AI fails, just return empty - report will use heuristic
        }

        return string.Empty;
    }

    /// <summary>
    /// Enhance an existing report with AI (post-processing)
    /// </summary>
    public async Task<SummaryReportResult> EnhanceReportWithAiAsync(SummaryReportResult report, string rawText)
    {
        if (_aiService == null)
            return report;

        try
        {
            var (aiInsights, provider) = await _aiService.EnhanceReportAsync(rawText, report.Title, report.Metrics);
            
            if (!string.IsNullOrWhiteSpace(aiInsights))
            {
                // Append AI insights to formatted text
                var aiSection = FormatAiInsights(aiInsights, provider);
                report.FormattedText += "\n" + aiSection;
                report.AiProvider = provider;
                report.AiEnhanced = true;
            }
            else
            {
                // AI failed, add heuristic insights
                var heuristicInsights = BuildEnhancedInsightsSection(rawText, report.Title, report.Metrics);
                report.FormattedText += "\n" + heuristicInsights;
                report.AiProvider = "Heuristic";
                report.AiEnhanced = false;
            }
        }
        catch (Exception)
        {
            // If AI fails completely, add heuristic insights
            var heuristicInsights = BuildEnhancedInsightsSection(rawText, report.Title, report.Metrics);
            report.FormattedText += "\n" + heuristicInsights;
            report.AiProvider = "Heuristic";
            report.AiEnhanced = false;
        }

        return report;
    }

    /// <summary>
    /// Append AI insights to formatted text
    /// </summary>
    private async Task<string> AppendAiInsightsToFormattedText(string formattedText, string rawText, string reportType, SummaryMetrics metrics)
    {
        var aiInsights = await GetAiEnhancedInsightsAsync(rawText, reportType, metrics);
        
        if (!string.IsNullOrWhiteSpace(aiInsights))
        {
            return formattedText + "\n" + aiInsights;
        }
        
        // If no AI insights, add heuristic insights
        var heuristicInsights = BuildEnhancedInsightsSection(rawText, reportType, metrics);
        return formattedText + "\n" + heuristicInsights;
    }

    /// <summary>
    /// Get AI provider name for metadata
    /// </summary>
    private async Task<(string provider, bool enhanced)> GetAiProviderInfoAsync(string rawText, string reportType, SummaryMetrics metrics)
    {
        if (_aiService == null)
            return ("Heuristic", false);

        try
        {
            var (aiInsights, provider) = await _aiService.EnhanceReportAsync(rawText, reportType, metrics);
            if (!string.IsNullOrWhiteSpace(aiInsights))
            {
                return (provider, true);
            }
        }
        catch (Exception)
        {
            // If AI fails, fall back to heuristic
        }

        return ("Heuristic", false);
    }

    private record ActivityEntry(string Text, string Status);
}
