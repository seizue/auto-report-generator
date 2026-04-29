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

    public async Task<List<Report>> GetAllAsync(string? clientId = null)
    {
        var query = db.Reports.Include(r => r.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(r => r.ClientId == clientId);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Report?> GetByIdAsync(int id, string? clientId = null)
    {
        var query = db.Reports.Include(r => r.Items).Where(r => r.Id == id);
        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(r => r.ClientId == clientId);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<Report?> UpdateAsync(int id, Report updated, string? clientId = null)
    {
        var query = db.Reports.Include(r => r.Items).Where(r => r.Id == id);
        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(r => r.ClientId == clientId);
        var existing = await query.FirstOrDefaultAsync();
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

    public async Task<bool> DeleteAsync(int id, string? clientId = null)
    {
        var query = db.Reports.Where(r => r.Id == id);
        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(r => r.ClientId == clientId);
        var report = await query.FirstOrDefaultAsync();
        if (report is null) return false;
        db.Reports.Remove(report);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAllAsync(string? clientId = null)
    {
        if (!string.IsNullOrWhiteSpace(clientId))
            db.Reports.RemoveRange(db.Reports.Where(r => r.ClientId == clientId));
        else
            db.Reports.RemoveRange(db.Reports);
        await db.SaveChangesAsync();
    }
}
