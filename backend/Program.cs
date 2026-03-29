using AutoReportGenerator.Data;
using AutoReportGenerator.Repositories;
using AutoReportGenerator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    // Disable file watching in production
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

// Disable file watching for configuration
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);

// Database — use SQLite for local dev, PostgreSQL for production
var usePostgres = builder.Configuration.GetValue<bool>("UsePostgres");
Console.WriteLine($"UsePostgres: {usePostgres}");

if (usePostgres)
{
    // Try to get from environment variable first, then fall back to configuration
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") 
                          ?? builder.Configuration.GetConnectionString("Postgres");
    
    Console.WriteLine($"Connection string is null: {connectionString == null}");
    Console.WriteLine($"Connection string length: {connectionString?.Length ?? 0}");
    
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        // Trim whitespace
        connectionString = connectionString.Trim();
        Console.WriteLine($"FULL Connection string: {connectionString}");
        
        // If it's a PostgreSQL URI (starts with postgresql://), convert to Npgsql format
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            var uri = new Uri(connectionString.Replace("postgresql://", "postgres://"));
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            Console.WriteLine($"Converted to Npgsql format");
        }
    }
    
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "PostgreSQL connection string is not configured. " +
            "Please set the ConnectionStrings__Postgres environment variable.");
    }
    
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(connectionString));
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
