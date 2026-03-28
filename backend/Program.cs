using AutoReportGenerator.Data;
using AutoReportGenerator.Repositories;
using AutoReportGenerator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database — use SQLite for local dev, PostgreSQL for production
var usePostgres = builder.Configuration.GetValue<bool>("UsePostgres");
if (usePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=reports.db"));
}

builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<ReportFormatterService>();
builder.Services.AddScoped<PdfExportService>();
builder.Services.AddScoped<DocxExportService>();
builder.Services.AddScoped<TextParserService>();
builder.Services.AddScoped<OcrService>();
builder.Services.AddScoped<DocumentTextExtractorService>();
builder.Services.AddScoped<SmartSuggestionsService>();

// AI Enhancement Service with HttpClient
builder.Services.AddHttpClient<AiReportEnhancementService>();
builder.Services.AddScoped<SummaryReportService>();
builder.Services.AddScoped<SummaryExportService>();

// Background service for automatic data cleanup (deletes reports older than 2 days)
builder.Services.AddHostedService<DataCleanupService>();

builder.Services.AddControllers();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.MapControllers();
app.Run();
