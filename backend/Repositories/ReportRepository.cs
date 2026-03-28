using AutoReportGenerator.Data;
using AutoReportGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoReportGenerator.Repositories;

public class ReportRepository(AppDbContext db)
{
    public async Task<Report> CreateAsync(Report report)
    {
        db.Reports.Add(report);
        await db.SaveChangesAsync();
        
        // Reload the report with items to ensure navigation properties are populated
        return await db.Reports
            .Include(r => r.Items)
            .FirstAsync(r => r.Id == report.Id);
    }

    public async Task<List<Report>> GetAllAsync() =>
        await db.Reports.Include(r => r.Items).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<Report?> GetByIdAsync(int id) =>
        await db.Reports.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Report?> UpdateAsync(int id, Report updated)
    {
        var existing = await db.Reports.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        if (existing is null) return null;

        existing.Name         = updated.Name;
        existing.Department   = updated.Department;
        existing.Date         = updated.Date;
        existing.TimeIn       = updated.TimeIn;
        existing.TimeOut      = updated.TimeOut;
        existing.Notes        = updated.Notes;
        existing.TemplateType = updated.TemplateType;
        existing.ListStyle    = updated.ListStyle;

        db.ReportItems.RemoveRange(existing.Items);
        existing.Items = updated.Items;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var report = await db.Reports.FindAsync(id);
        if (report is null) return false;
        db.Reports.Remove(report);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAllAsync()
    {
        db.Reports.RemoveRange(db.Reports);
        await db.SaveChangesAsync();
    }
}
