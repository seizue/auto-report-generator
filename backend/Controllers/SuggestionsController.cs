using AutoReportGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoReportGenerator.Controllers;

[ApiController]
[Route("api/suggestions")]
public class SuggestionsController : ControllerBase
{
    private readonly SmartSuggestionsService _suggestions;

    public SuggestionsController(SmartSuggestionsService suggestions)
    {
        _suggestions = suggestions;
    }

    [HttpGet("tasks/{employeeName}")]
    public async Task<IActionResult> GetTaskSuggestions(string employeeName, [FromQuery] int limit = 10)
    {
        var suggestions = await _suggestions.GetTaskSuggestionsAsync(employeeName, limit);
        return Ok(new { suggestions });
    }

    [HttpGet("insights/{employeeName}")]
    public async Task<IActionResult> GetProductivityInsights(string employeeName)
    {
        var insights = await _suggestions.GetProductivityInsightsAsync(employeeName);
        return Ok(insights);
    }

    [HttpPost("categorize")]
    public IActionResult CategorizeTask([FromBody] TaskCategorizationRequest request)
    {
        var category = _suggestions.CategorizeTask(request.Task);
        return Ok(new { task = request.Task, category });
    }
}

public class TaskCategorizationRequest
{
    public string Task { get; set; } = string.Empty;
}
