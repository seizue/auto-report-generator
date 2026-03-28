using AutoReportGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoReportGenerator.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportItem> ReportItems => Set<ReportItem>();
    public DbSet<Template> Templates => Set<Template>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>()
            .HasMany(r => r.Items)
            .WithOne(i => i.Report)
            .HasForeignKey(i => i.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed templates
        modelBuilder.Entity<Template>().HasData(
            new Template { Id = 1, Name = "Daily Accomplishment Report", Type = "daily", Description = "Track your daily tasks and accomplishments." },
            new Template { Id = 2, Name = "Weekly Summary Report", Type = "weekly", Description = "Summarize your week's work and progress." },
            new Template { Id = 3, Name = "Work Log Report", Type = "worklog", Description = "Detailed time-based work log." }
        );
    }
}
