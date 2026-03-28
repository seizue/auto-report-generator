using AutoReportGenerator.Data;
using AutoReportGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoReportGenerator.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplatesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Template>>> GetAll() =>
        await db.Templates.ToListAsync();
}
