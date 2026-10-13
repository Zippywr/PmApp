using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;

namespace PmApp.Web.Services;

public class PmOverdueService
{
    private readonly AppDbContext _db;

    public PmOverdueService(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // UPDATE OVERDUE — jalankan saat load halaman
    // ============================================================
    public async Task<int> UpdateOverdueAsync()
    {
        var today = DateTime.Today;

        var overdueList = await _db.PmSchedules
            .Where(s =>
                (s.Year < today.Year || (s.Year == today.Year && s.Month < today.Month))
                && (s.Status == PmStatus.Scheduled || s.Status == PmStatus.InProgress))
            .ToListAsync();

        foreach (var s in overdueList)
        {
            s.Status = PmStatus.Overdue;
        }

        if (overdueList.Any())
            await _db.SaveChangesAsync();

        return overdueList.Count;
    }

    // ============================================================
    // HELPER — hitung bulan delay
    // ============================================================
    public static int GetMonthsOverdue(int year, int month)
    {
        var today = DateTime.Today;

        if (year > today.Year || (year == today.Year && month >= today.Month))
            return 0;

        var months = (today.Year - year) * 12 + (today.Month - month);
        return Math.Max(0, months);
    }

    // ============================================================
    // STATISTIK — untuk Dashboard
    // ============================================================
    public async Task<OverdueStat> GetStatAsync()
    {
        var today = DateTime.Today;

        var all = await _db.PmSchedules
            .Where(s =>
                (s.Year < today.Year || (s.Year == today.Year && s.Month < today.Month))
                && s.Status == PmStatus.Overdue)
            .ToListAsync();

        return new OverdueStat
        {
            Total = all.Count
        };
    }
}

public class OverdueStat
{
    public int Total { get; set; }
}