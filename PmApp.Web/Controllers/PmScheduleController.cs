using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class PmScheduleController : Controller
{
    private readonly AppDbContext _db;

    public PmScheduleController(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // INDEX — Tampilan 12 bulan seperti Excel
    // ============================================================
    public async Task<IActionResult> Index(int? year, int? assetId)
    {
        var y = year ?? DateTime.Today.Year;

        var tasksQuery = _db.PmTaskTemplates
            .Include(t => t.Asset)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .Where(t => t.IsActive)
            .AsQueryable();

        if (assetId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.AssetId == assetId.Value);

        var tasks = await tasksQuery
            .OrderBy(t => t.IdPm)
            .ToListAsync();

        var schedules = await _db.PmSchedules
            .Where(s => s.Year == y)
            .ToListAsync();

        var rows = new List<PmScheduleRowViewModel>();

        foreach (var task in tasks)
        {
            var row = new PmScheduleRowViewModel { Task = task };

            for (int month = 1; month <= 12; month++)
            {
                var sched = schedules.FirstOrDefault(s =>
                    s.TaskTemplateId == task.Id && s.Month == month);

                row.Months.Add(new PmScheduleCell
                {
                    Month = month,
                    ScheduleId = sched?.Id,
                    PlanDate = sched?.PlanDate,
                    ActualDate = sched?.ActualDate,
                    PIC = sched?.PIC,
                    Status = sched?.Status.ToString(),
                    Notes = sched?.Notes,
                    IsPlanned = sched != null
                });
            }

            rows.Add(row);
        }

        ViewBag.Year = y;
        ViewBag.AssetId = assetId;
        ViewBag.Assets = await _db.Assets.OrderBy(a => a.Code).ToListAsync();

        return View(rows);
    }

    // ============================================================
    // UPDATE CELL — GET
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> UpdateCell(int id)
    {
        var schedule = await _db.PmSchedules
            .Include(s => s.TaskTemplate)
            .Include(s => s.Asset)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null) return NotFound();

        ViewBag.ScheduleId = schedule.Id;
        ViewBag.TaskIdPm = schedule.TaskTemplate?.IdPm;
        ViewBag.AssetCode = schedule.Asset?.Code;
        ViewBag.Month = schedule.Month;
        ViewBag.Year = schedule.Year;

        return View(schedule);
    }

    // ============================================================
    // UPDATE CELL — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCell(int id, DateTime? actualDate, string? pic, PmStatus status, string? notes)
    {
        var schedule = await _db.PmSchedules.FirstOrDefaultAsync(s => s.Id == id);
        if (schedule == null) return NotFound();

        schedule.ActualDate = actualDate;
        schedule.PIC = pic;
        schedule.Status = status;
        schedule.Notes = notes;

        if (actualDate.HasValue && status == PmStatus.Scheduled)
            schedule.Status = PmStatus.Done;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Jadwal berhasil diperbarui.";
        return RedirectToAction(nameof(Index), new { year = schedule.Year });
    }

    // ============================================================
    // REGENERATE — untuk semua task tahun ini
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Regenerate(int year)
    {
        var tasks = await _db.PmTaskTemplates
            .Where(t => t.IsActive)
            .ToListAsync();

        var existing = await _db.PmSchedules
            .Where(s => s.Year == year)
            .ToListAsync();

        int added = 0;

        foreach (var task in tasks)
        {
            for (int month = 1; month <= 12; month++)
            {
                if (task.PeriodeMonth <= 0) continue;
                if ((month - task.StartMonth) % task.PeriodeMonth != 0) continue;

                var exists = existing.Any(s =>
                    s.TaskTemplateId == task.Id && s.Month == month);

                if (exists) continue;

                _db.PmSchedules.Add(new PmSchedule
                {
                    TaskTemplateId = task.Id,
                    AssetId = task.AssetId,
                    Year = year,
                    Month = month,
                    PlanDate = new DateTime(year, month, 1),
                    Status = PmStatus.Scheduled,
                    Notes = "Auto-generated",
                    CreatedBy = "system",
                    CreatedDate = DateTime.Now
                });

                added++;
            }
        }

        if (added > 0)
            await _db.SaveChangesAsync();

        TempData["Success"] = added + " jadwal baru ditambahkan untuk tahun " + year + ".";
        return RedirectToAction(nameof(Index), new { year = year });
    }
}