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
        opt.UseNpgsql(connectionString)
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
}
else
{
    // Always store the DB next to the executable so the path is consistent
    // regardless of which directory `dotnet run` is invoked from.
    var dbPath = Path.Combine(AppContext.BaseDirectory, "reports.db");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite($"Data Source={dbPath}")
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
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
    var isSqlite   = db.Database.ProviderName?.Contains("Sqlite")   == true;
    var isPostgres = db.Database.ProviderName?.Contains("Npgsql")   == true;

    var conn = db.Database.GetDbConnection();
    conn.Open();

    // Ensure __EFMigrationsHistory table exists
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = isSqlite
            ? "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL);"
            : "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" varchar(150) NOT NULL, \"ProductVersion\" varchar(32) NOT NULL, CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY (\"MigrationId\"));";
        cmd.ExecuteNonQuery();
    }

    // Check if Reports table already exists (legacy DB)
    bool reportsExists;
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = isSqlite
            ? "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Reports';"
            : "SELECT COUNT(*) FROM information_schema.tables WHERE table_name='Reports';";
        reportsExists = Convert.ToInt64(cmd.ExecuteScalar() ?? 0L) > 0;
    }

    // Check if migration history is empty
    bool historyEmpty;
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT COUNT(*) FROM \"__EFMigrationsHistory\";";
        historyEmpty = Convert.ToInt64(cmd.ExecuteScalar() ?? 0L) == 0;
    }

    if (reportsExists && historyEmpty)
    {
        // Legacy DB: tables exist but were never migration-tracked.
        // Stamp all 3 original migrations so Migrate() won't try to recreate tables.
        var toStamp = new[]
        {
            "20260329044844_InitialCreate",
            "20260329053844_FixPostgresColumnTypes",
            "20260330032459_AddIsPremiumToTemplate"
        };
        foreach (var migrationId in toStamp)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = isSqlite
                ? $"INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{migrationId}', '8.0.0');"
                : $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{migrationId}', '8.0.0') ON CONFLICT DO NOTHING;";
            cmd.ExecuteNonQuery();
        }

        // Add ClientId column directly if it doesn't exist yet
        bool clientIdExists;
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = isSqlite
                ? "SELECT COUNT(*) FROM pragma_table_info('Reports') WHERE name='ClientId';"
                : "SELECT COUNT(*) FROM information_schema.columns WHERE table_name='Reports' AND column_name='ClientId';";
            clientIdExists = Convert.ToInt64(cmd.ExecuteScalar() ?? 0L) > 0;
        }
        if (!clientIdExists)
        {
            using var cmd = conn.CreateCommand();
            // SQLite and Postgres both support this syntax
            cmd.CommandText = "ALTER TABLE \"Reports\" ADD COLUMN \"ClientId\" TEXT NOT NULL DEFAULT '';";
            cmd.ExecuteNonQuery();
            // Delete old rows — they have no client identity and are permanently invisible
            cmd.CommandText = "DELETE FROM \"Reports\" WHERE \"ClientId\" = '';";
            cmd.ExecuteNonQuery();
            Console.WriteLine("Added ClientId column and removed legacy rows.");
        }

        Console.WriteLine("Stamped legacy migrations into history.");
    }

    conn.Close();
    db.Database.Migrate();
}

app.UseCors();
app.MapControllers();
app.Run();
