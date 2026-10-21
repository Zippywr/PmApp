using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

public class PmScheduleController : Controller
{
    private readonly AppDbContext _db;
    private readonly PmScheduleHistoryService _history;
    private readonly ProductAccessService _productAccess;

    private static readonly string[] MonthNames =
    {
        "", "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember"
    };

    public PmScheduleController(AppDbContext db, PmScheduleHistoryService history, ProductAccessService productAccess)
    {
        _db = db;
        _history = history;
        _productAccess = productAccess;
    }

    private static int GetMonthsDelay(int year, int month)
    {
        var today = DateTime.Today;

        if (year > today.Year || (year == today.Year && month >= today.Month))
            return 0;

        var months = (today.Year - year) * 12 + (today.Month - month);
        return Math.Max(0, months);
    }

    private async Task UpdateDelayStatusAsync()
    {
        var today = DateTime.Today;

        var allowed = await _productAccess.AssignedProductIdsAsync();
        var delayList = await _productAccess.ScopeSchedules(_db.PmSchedules, allowed)
            .Where(s =>
                (s.Year < today.Year || (s.Year == today.Year && s.Month < today.Month))
                && (s.Status == PmStatus.Scheduled || s.Status == PmStatus.InProgress))
            .ToListAsync();

        foreach (var s in delayList)
        {
            s.Status = PmStatus.Overdue;
        }

        if (delayList.Any())
            await _db.SaveChangesAsync();
    }

    // ============================================================
    // INDEX
    // ============================================================
    public async Task<IActionResult> Index(int? year, int? assetId, bool onlyDelay = false)
    {
        await UpdateDelayStatusAsync();

        var y = year ?? DateTime.Today.Year;

        var tasksQuery = _db.PmTaskTemplates
            .Include(t => t.Asset)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .Where(t => t.IsActive)
            .AsQueryable();

        var assignedProducts = await _productAccess.AssignedProductIdsAsync();
        tasksQuery = _productAccess.ScopeTasks(tasksQuery, assignedProducts);

        if (assetId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.AssetId == assetId.Value);

        var tasks = await tasksQuery.OrderBy(t => t.IdPm).ToListAsync();

        var schedules = await _productAccess.ScopeSchedules(_db.PmSchedules, assignedProducts)
            .Where(s => s.Year == y)
            .ToListAsync();

        var rows = new List<PmScheduleRowViewModel>();

        foreach (var task in tasks)
        {
            var row = new PmScheduleRowViewModel { Task = task };
            bool hasDelay = false;

            for (int month = 1; month <= 12; month++)
            {
                var sched = schedules.FirstOrDefault(s =>
                    s.TaskTemplateId == task.Id && s.Month == month);

                var cell = new PmScheduleCell
                {
                    Month = month,
                    ScheduleId = sched?.Id,
                    PlanDate = sched?.PlanDate,
                    ActualDate = sched?.ActualDate,
                    PIC = sched?.PIC,
                    Status = sched?.Status.ToString(),
                    Result = sched?.Result.ToString(),
                    Notes = sched?.Notes,
                    IsPlanned = sched != null
                };

                if (sched != null && sched.Status == PmStatus.Overdue)
                {
                    cell.IsDelay = true;
                    cell.MonthsDelay = GetMonthsDelay(sched.Year, sched.Month);
                    hasDelay = true;
                }

                row.Months.Add(cell);
            }

            if (onlyDelay && !hasDelay) continue;
            rows.Add(row);
        }

        ViewBag.Year = y;
        ViewBag.AssetId = assetId;
        ViewBag.OnlyDelay = onlyDelay;
        ViewBag.Assets = await _productAccess.ScopeAssets(_db.Assets, assignedProducts)
            .OrderBy(a => a.Code).ToListAsync();

        return View(rows);
    }

    // ============================================================
    // BY WEEK
    // ============================================================
    public IActionResult ByWeek(DateTime? weekStart)
    {
        // Halaman mingguan lama dialihkan ke Pelaksanaan PM.
        // Pemilihan periode tetap dilakukan melalui filter bulanan.
        var date = weekStart ?? DateTime.Today;
        return RedirectToAction(nameof(ByMonth), new { year = date.Year, month = date.Month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,TS")]
    public IActionResult UpdateWeek()
    {
        TempData["Error"] = "Update Mingguan lama sudah dinonaktifkan. Gunakan halaman Pelaksanaan PM dan form Isi PM.";
        return RedirectToAction(nameof(ByMonth));
    }

    // ============================================================
    // DETAIL
    // ============================================================
    public async Task<IActionResult> Detail(int id, int? year)
    {
        var y = year ?? DateTime.Today.Year;
        if (!await _productAccess.CanAccessTaskAsync(id)) return Forbid();

        var task = await _db.PmTaskTemplates
            .Include(t => t.Asset).ThenInclude(a => a!.Line)
            .Include(t => t.Asset).ThenInclude(a => a!.Area)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return NotFound();

        var schedules = await _db.PmSchedules
            .Where(s => s.TaskTemplateId == id && s.Year == y)
            .ToListAsync();

        var vm = new PmScheduleDetailViewModel
        {
            TaskId = task.Id,
            IdPm = task.IdPm,
            Year = y,
            AssetCode = task.Asset?.DisplayCode ?? "-",
            AssetName = task.Asset?.Name ?? "-",
            LineName = task.Asset?.Line?.Name ?? "-",
            AreaName = task.Asset?.Area?.Name ?? "-",
            OpNo = task.Asset?.OpNo ?? "-",
            UnitName = task.Unit?.Name ?? "-",
            SubUnitName = task.SubUnit?.Name ?? "-",
            MethodName = task.Method?.Name ?? "-",
            StandardName = task.Standard?.Name ?? "-",
            WorkHourMinutes = task.WorkHourMinutes,
            ManPower = task.ManPower,
            MachineState = task.MachineState.ToString(),
            PeriodeMonth = task.PeriodeMonth,
            StartMonth = task.StartMonth,
            Remark = task.Remark
        };

        for (int m = 1; m <= 12; m++)
        {
            var sched = schedules.FirstOrDefault(s => s.Month == m);

            var row = new PmScheduleDetailRow
            {
                Month = m,
                MonthName = MonthNames[m],
                IsPlanned = sched != null,
                ScheduleId = sched?.Id,
                PlanDate = sched?.PlanDate,
                ActualDate = sched?.ActualDate,
                PIC = sched?.PIC,
                Status = sched?.Status.ToString() ?? "-",
                Result = sched?.Result.ToString() ?? "-",
                Notes = sched?.Notes
            };

            if (sched != null && sched.Status == PmStatus.Overdue)
            {
                row.IsDelay = true;
                row.MonthsDelay = GetMonthsDelay(sched.Year, sched.Month);
            }

            vm.Months.Add(row);
        }

        return View(vm);
    }

    // ============================================================
    // UPDATE CELL — GET
    // ============================================================
    [HttpGet]
    [Authorize(Roles = "Admin,TS,LineHead")]
    public async Task<IActionResult> UpdateCell(int id)
    {
        var schedule = await _db.PmSchedules
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset).ThenInclude(a => a!.Line)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset).ThenInclude(a => a!.Area)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Unit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Standard)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null) return NotFound();
        if (!await _productAccess.CanAccessScheduleAsync(id)) return Forbid();
        ViewBag.CanEditDone = _productAccess.IsManager;

        var task = schedule.TaskTemplate;

        var vm = new PmUpdateCellViewModel
        {
            ScheduleId = schedule.Id,
            TaskTemplateId = schedule.TaskTemplateId,
            AssetId = schedule.AssetId,

            // Info task
            IdPm = task?.IdPm ?? "",
            AssetCode = task?.Asset?.DisplayCode ?? "",
            AssetName = task?.Asset?.Name ?? "",
            LineName = task?.Asset?.Line?.Name,
            AreaName = task?.Asset?.Area?.Name,
            UnitName = task?.Unit?.Name,
            SubUnitName = task?.SubUnit?.Name,
            MethodName = task?.Method?.Name,
            StandardName = task?.Standard?.Name,
            WorkHourMinutes = task?.WorkHourMinutes ?? 0,
            ManPower = task?.ManPower ?? 0,
            MachineState = task?.MachineState.ToString(),
            Remark = task?.Remark,

            // Periode
            Year = schedule.Year,
            Month = schedule.Month,
            MonthName = MonthNames[schedule.Month],

            // Data editable
            PlanDate = schedule.PlanDate,
            ActualDate = schedule.ActualDate,
            PIC = schedule.PIC,
            Status = schedule.Status,
            Result = schedule.Result,
            Notes = schedule.Notes,

            // Delay info
            MonthsDelay = GetMonthsDelay(schedule.Year, schedule.Month),

            // Dropdown PIC
            PICs = new List<string> { "Faqih", "Budi Santoso", "Andi Wijaya" }
        };

        return View(vm);
    }

    // ============================================================
    // UPDATE CELL — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,TS,LineHead")]
    public async Task<IActionResult> UpdateCell(
        int id,
        DateTime? actualDate,
        string? pic,
        MachineCondition result,
        string? notes)
    {
        var schedule = await _db.PmSchedules.FirstOrDefaultAsync(s => s.Id == id);
        if (schedule == null) return NotFound();
        if (!await _productAccess.CanAccessScheduleAsync(id)) return Forbid();

        // Line Head hanya dapat mengisi jika belum Done. Admin boleh mengoreksi PM Done.
        // Pemeriksaan di server mencegah bypass melalui POST langsung.
        bool isAdmin = _productAccess.IsManager;
        if (schedule.Status == PmStatus.Done && !isAdmin)
            return Forbid();

        if (!actualDate.HasValue || string.IsNullOrWhiteSpace(pic))
        {
            TempData["Error"] = "Actual Date dan PIC wajib diisi sebelum Done.";
            return RedirectToAction(nameof(UpdateCell), new { id });
        }

        pic = pic.Trim();
        notes = notes?.Trim();
        if (pic.Length > 200 || (notes?.Length ?? 0) > 500)
        {
            TempData["Error"] = "PIC maksimal 200 karakter dan Catatan maksimal 500 karakter.";
            return RedirectToAction(nameof(UpdateCell), new { id });
        }

        // Partial adalah nilai enum lama yang ditampilkan dengan label Comment.
        if (result != MachineCondition.OK && result != MachineCondition.NG &&
            result != MachineCondition.Partial)
        {
            TempData["Error"] = "Result wajib dipilih: OK, NG, atau Comment.";
            return RedirectToAction(nameof(UpdateCell), new { id });
        }
        if (result == MachineCondition.Partial && string.IsNullOrWhiteSpace(notes))
        {
            TempData["Error"] = "Catatan wajib diisi jika Result adalah Comment.";
            return RedirectToAction(nameof(UpdateCell), new { id });
        }

        var statusBefore = schedule.Status.ToString();
        var resultBefore = schedule.Result.ToString();
        var picBefore = schedule.PIC;
        var planBefore = schedule.PlanDate;
        var actualBefore = schedule.ActualDate;
        var notesBefore = schedule.Notes;

        // PlanDate tidak diedit oleh Line Head; hanya data pelaksanaan.
        schedule.ActualDate = actualDate;
        schedule.PIC = pic;
        schedule.Result = result;
        schedule.Notes = notes;
        schedule.Status = PmStatus.Done;

        // Status segera Done. Riwayat mencatat identitas akun yang mengubah.
        await _db.SaveChangesAsync();
        await _history.LogAsync(schedule, statusBefore, resultBefore,
            picBefore, planBefore, actualBefore, notesBefore,
            User.Identity?.Name ?? "system");

        TempData["Success"] = isAdmin && statusBefore == PmStatus.Done.ToString()
            ? "Hasil PM berhasil dikoreksi oleh Admin."
            : "PM berhasil disimpan dan berstatus Done.";
        return RedirectToAction(nameof(ByMonth), new { year = schedule.Year, month = schedule.Month });
    }

    // ============================================================
    // HISTORY — Timeline view
    // ============================================================
    public async Task<IActionResult> History(int id)
    {
        var schedule = await _db.PmSchedules
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null) return NotFound();
        if (!await _productAccess.CanAccessScheduleAsync(id)) return Forbid();

        var history = await _db.PmScheduleHistories
            .Where(h => h.PmScheduleId == id)
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        ViewBag.Schedule = schedule;
        ViewBag.IdPm = schedule.TaskTemplate?.IdPm;
        ViewBag.AssetCode = schedule.TaskTemplate?.Asset?.DisplayCode;
        ViewBag.SubUnitName = schedule.TaskTemplate?.SubUnit?.Name;
        ViewBag.MethodName = schedule.TaskTemplate?.Method?.Name;
        ViewBag.Month = schedule.Month;
        ViewBag.Year = schedule.Year;

        return View(history);
    }
    // ============================================================
    // BY MONTH — Update Bulanan (group Week 1-4)
    // ============================================================
    public async Task<IActionResult> ByMonth(
        int? year, int? month, int? productId, int? lineId,
        int? assetId, string? status, string? search)
    {
        var today = DateTime.Today;
        var y = year ?? today.Year;
        var m = month ?? today.Month;

        if (y < 2000 || y > 2100 || m < 1 || m > 12)
            return BadRequest("Bulan atau tahun tidak valid.");

        var start = new DateTime(y, m, 1);
        var end = start.AddMonths(1);

        var assignedProducts = await _productAccess.AssignedProductIdsAsync();
        // Pilihan filter tidak mengubah data PM.
        ViewBag.Products = await _db.Products.AsNoTracking()
            .Where(p => _productAccess.IsManager || assignedProducts.Contains(p.Id))
            .OrderBy(p => p.Name).ToListAsync();
        ViewBag.Lines = await _db.Lines.AsNoTracking()
            .Where(l => _productAccess.IsManager ||
                (l.ProductId.HasValue && assignedProducts.Contains(l.ProductId.Value)))
            .OrderBy(l => l.Name).ToListAsync();
        ViewBag.Assets = await _productAccess.ScopeAssets(_db.Assets.AsNoTracking(), assignedProducts)
            .OrderBy(a => a.Code).ToListAsync();
        ViewBag.ProductId = productId;
        ViewBag.LineId = lineId;
        ViewBag.AssetId = assetId;
        ViewBag.StatusFilter = status ?? "all";
        ViewBag.Search = search?.Trim() ?? "";

        var query = _productAccess.ScopeSchedules(_db.PmSchedules, assignedProducts)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .Where(s => s.PlanDate >= start && s.PlanDate < end);

        if (productId.HasValue)
            query = query.Where(s => s.TaskTemplate != null &&
                s.TaskTemplate.Asset != null &&
                s.TaskTemplate.Asset.ProductId == productId.Value);

        if (lineId.HasValue)
            query = query.Where(s => s.TaskTemplate != null &&
                s.TaskTemplate.Asset != null &&
                s.TaskTemplate.Asset.LineId == lineId.Value);

        if (assetId.HasValue)
            query = query.Where(s => s.TaskTemplate != null &&
                s.TaskTemplate.AssetId == assetId.Value);

        switch ((status ?? "all").Trim().ToLowerInvariant())
        {
            case "done": query = query.Where(s => s.Status == PmStatus.Done); break;
            case "pending": query = query.Where(s => s.Status == PmStatus.Scheduled || s.Status == PmStatus.InProgress); break;
            case "delay": query = query.Where(s => s.Status == PmStatus.Overdue); break;
            case "scheduled": query = query.Where(s => s.Status == PmStatus.Scheduled); break;
            case "progress": query = query.Where(s => s.Status == PmStatus.InProgress); break;
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.TaskTemplate != null && (
                s.TaskTemplate.IdPm.Contains(term) ||
                (s.TaskTemplate.Asset != null && (
                    s.TaskTemplate.Asset.Code.Contains(term) ||
                    (s.TaskTemplate.Asset.OpNo != null && s.TaskTemplate.Asset.OpNo.Contains(term)) ||
                    (s.TaskTemplate.Asset.OpNo != null &&
                     (s.TaskTemplate.Asset.Code + "/" + s.TaskTemplate.Asset.OpNo).Contains(term)))) ||
                (s.TaskTemplate.SubUnit != null && s.TaskTemplate.SubUnit.Name.Contains(term)) ||
                (s.TaskTemplate.Method != null && s.TaskTemplate.Method.Name.Contains(term))
            ));
        }

        var schedules = await query
            .OrderBy(s => s.PlanDate)
            .ThenBy(s => s.TaskTemplate!.Asset!.Code)
            .ToListAsync();

        // Group per Week (1-4)
        var weeks = new List<PmWeekGroup>();

        for (int w = 1; w <= 4; w++)
        {
            var weekStartDay = ((w - 1) * 7) + 1;
            var maxDay = DateTime.DaysInMonth(y, m);
            if (weekStartDay > maxDay) continue;

            var weekStart = new DateTime(y, m, weekStartDay);
            var weekEndDay = w == 4 ? maxDay : Math.Min(weekStartDay + 6, maxDay);
            var weekEnd = new DateTime(y, m, weekEndDay);

            var weekSchedules = schedules
                .Where(s => s.PlanDate >= weekStart && s.PlanDate <= weekEnd)
                .ToList();

            var rows = weekSchedules.Select(s => new PmWeekRow
            {
                ScheduleId = s.Id,
                TaskTemplateId = s.TaskTemplateId,
                IdPm = s.TaskTemplate?.IdPm ?? "",
                AssetCode = s.TaskTemplate?.Asset?.DisplayCode ?? "-",
                AssetName = s.TaskTemplate?.Asset?.Name ?? "-",
                SubUnitName = s.TaskTemplate?.SubUnit?.Name ?? "-",
                MethodName = s.TaskTemplate?.Method?.Name ?? "-",
                WorkHourMinutes = s.TaskTemplate?.WorkHourMinutes ?? 0,
                ManPower = s.TaskTemplate?.ManPower ?? 0,
                PlanDate = s.PlanDate,
                ActualDate = s.ActualDate,
                PIC = s.PIC,
                Status = s.Status,
                Result = s.Result,
                Notes = s.Notes
            }).ToList();

            weeks.Add(new PmWeekGroup
            {
                WeekNumber = w,
                WeekLabel = $"Week {w} ({weekStart:dd}-{weekEnd:dd} {MonthNames[m].Substring(0, 3)})",
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Rows = rows,
                TotalTask = rows.Count,
                TotalDone = rows.Count(r => r.Status == PmStatus.Done),
                TotalPending = rows.Count(r => r.Status == PmStatus.Scheduled || r.Status == PmStatus.InProgress),
                TotalDelay = rows.Count(r => r.Status == PmStatus.Overdue)
            });
        }

        var vm = new PmMonthViewModel
        {
            Year = y,
            Month = m,
            MonthName = MonthNames[m],
            Weeks = weeks,
            TotalTask = schedules.Count,
            TotalDone = schedules.Count(s => s.Status == PmStatus.Done),
            TotalPending = schedules.Count(s => s.Status == PmStatus.Scheduled || s.Status == PmStatus.InProgress),
            TotalDelay = schedules.Count(s => s.Status == PmStatus.Overdue)
        };

        return View(vm);
    }

    // ============================================================
    // UPDATE MONTH — POST (massal semua week)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,TS")]
    public IActionResult UpdateMonth()
    {
        TempData["Error"] = "Update massal lama sudah dinonaktifkan. Gunakan form Isi PM pada Pelaksanaan PM.";
        return RedirectToAction(nameof(ByMonth));
    }
    // ============================================================
    // REGENERATE
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,TS")]
    public async Task<IActionResult> Regenerate(int year)
    {
        var tasks = await _db.PmTaskTemplates
            .Where(t => t.IsActive)
            .ToListAsync();

        var existing = await _db.PmSchedules
            .Where(s => s.Year == year)
            .ToListAsync();

        int added = 0;
        int updated = 0;

        foreach (var task in tasks)
        {
            for (int month = 1; month <= 12; month++)
            {
                if (task.PeriodeMonth <= 0) continue;
                if ((month - task.StartMonth) % task.PeriodeMonth != 0) continue;

                var week = task.Week < 1 ? 1 : (task.Week > 4 ? 4 : task.Week);
                var day = ((week - 1) * 7) + 1;

                var maxDay = DateTime.DaysInMonth(year, month);
                if (day > maxDay) day = maxDay;

                var planDate = new DateTime(year, month, day);

                var existingSchedule = existing.FirstOrDefault(s =>
                    s.TaskTemplateId == task.Id && s.Month == month);

                if (existingSchedule != null)
                {
                    if (existingSchedule.Status == PmStatus.Scheduled ||
                        existingSchedule.Status == PmStatus.InProgress)
                    {
                        existingSchedule.PlanDate = planDate;
                        existingSchedule.Notes = "Auto-updated (Week " + week + ")";
                        updated++;
                    }
                }
                else
                {
                    _db.PmSchedules.Add(new PmSchedule
                    {
                        TaskTemplateId = task.Id,
                        AssetId = task.AssetId,
                        Year = year,
                        Month = month,
                        PlanDate = planDate,
                        Status = PmStatus.Scheduled,
                        Result = MachineCondition.Pending,
                        Notes = "Auto-generated (Week " + week + ")",
                        CreatedBy = "system",
                        CreatedDate = DateTime.Now
                    });
                    added++;
                }
            }
        }

        if (added > 0 || updated > 0)
            await _db.SaveChangesAsync();

        TempData["Success"] = $"{added} jadwal baru ditambahkan, {updated} jadwal diperbarui untuk tahun {year}.";
        return RedirectToAction(nameof(Index), new { year = year });
    }

    // ============================================================
    // RESET & REGENERATE
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,TS")]
    public async Task<IActionResult> ResetRegenerate(int year)
    {
        var toDelete = await _db.PmSchedules
            .Where(s => s.Year == year
                     && s.Status != PmStatus.Done
                     && s.Status != PmStatus.Cancelled)
            .ToListAsync();

        foreach (var s in toDelete)
        {
            s.IsDeleted = true;
        }

        if (toDelete.Any())
            await _db.SaveChangesAsync();

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

                var week = task.Week < 1 ? 1 : (task.Week > 4 ? 4 : task.Week);
                var day = ((week - 1) * 7) + 1;

                var maxDay = DateTime.DaysInMonth(year, month);
                if (day > maxDay) day = maxDay;

                _db.PmSchedules.Add(new PmSchedule
                {
                    TaskTemplateId = task.Id,
                    AssetId = task.AssetId,
                    Year = year,
                    Month = month,
                    PlanDate = new DateTime(year, month, day),
                    Status = PmStatus.Scheduled,
                    Result = MachineCondition.Pending,
                    Notes = "Auto-generated (Week " + week + ")",
                    CreatedBy = "system",
                    CreatedDate = DateTime.Now
                });

                added++;
            }
        }

        if (added > 0)
            await _db.SaveChangesAsync();

        TempData["Success"] = $"Reset selesai: {toDelete.Count} jadwal lama dihapus, {added} jadwal baru dibuat.";
        return RedirectToAction(nameof(Index), new { year = year });
    }
}