using AutoReportGenerator.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoReportGenerator.Services;

/// <summary>
/// Background service that automatically deletes reports older than configured retention period
/// Runs periodically to check and clean up old data
/// </summary>
public class DataCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private TimeSpan _retentionPeriod;
    private TimeSpan _checkInterval;

    public DataCleanupService(
        IServiceProvider serviceProvider, 
        ILogger<DataCleanupService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;

        // Load configuration values with defaults
        var retentionDays = _configuration.GetValue<int>("DataRetention:RetentionDays", 2);
        var cleanupIntervalHours = _configuration.GetValue<int>("DataRetention:CleanupIntervalHours", 1);

        _retentionPeriod = TimeSpan.FromDays(retentionDays);
        _checkInterval = TimeSpan.FromHours(cleanupIntervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Data Cleanup Service started. Reports will be deleted after {Days} days. Checking every {Hours} hours.", 
            _retentionPeriod.TotalDays, 
            _checkInterval.TotalHours);

        // Run cleanup immediately on startup
        await CleanupOldReportsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
                
                if (!stoppingToken.IsCancellationRequested)
                {
                    await CleanupOldReportsAsync(stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when application is shutting down
                _logger.LogInformation("Data Cleanup Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up old reports");
                // Continue running even if one cleanup fails
            }
        }
        
        _logger.LogInformation("Data Cleanup Service stopped");
    }

    private async Task CleanupOldReportsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoffDate = DateTime.UtcNow - _retentionPeriod;

        // Find all reports older than the retention period
        var oldReports = await db.Reports
            .Where(r => r.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        if (oldReports.Any())
        {
            _logger.LogInformation(
                "Deleting {Count} reports older than {Date} (created before {Cutoff})", 
                oldReports.Count, 
                _retentionPeriod.TotalDays + " days ago",
                cutoffDate);
            
            db.Reports.RemoveRange(oldReports);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted {Count} old reports", oldReports.Count);
        }
        else
        {
            _logger.LogDebug("No old reports to delete. Cutoff date: {Date}", cutoffDate);
        }
    }
}
