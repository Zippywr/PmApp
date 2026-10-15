using Microsoft.AspNetCore.Mvc;
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

    private static readonly string[] MonthNames =
    {
        "", "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember"
    };

    public PmScheduleController(AppDbContext db, PmScheduleHistoryService history)
    {
        _db = db;
        _history = history;
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

        var delayList = await _db.PmSchedules
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

        if (assetId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.AssetId == assetId.Value);

        var tasks = await tasksQuery.OrderBy(t => t.IdPm).ToListAsync();

        var schedules = await _db.PmSchedules
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
        ViewBag.Assets = await _db.Assets.OrderBy(a => a.Code).ToListAsync();

        return View(rows);
    }

    // ============================================================
    // BY WEEK
    // ============================================================
    public async Task<IActionResult> ByWeek(DateTime? weekStart)
    {
        if (!weekStart.HasValue)
        {
            var today = DateTime.Today;
            var currentStart = today;
            while (currentStart.DayOfWeek != DayOfWeek.Monday)
                currentStart = currentStart.AddDays(-1);

            var currentEnd = currentStart.AddDays(7);

            var hasThisWeek = await _db.PmSchedules
                .AnyAsync(s => s.PlanDate >= currentStart && s.PlanDate < currentEnd);

            if (hasThisWeek)
            {
                weekStart = today;
            }
            else
            {
                var nearest = await _db.PmSchedules
                    .Where(s => s.PlanDate != null)
                    .OrderBy(s => s.PlanDate)
                    .Select(s => s.PlanDate)
                    .FirstOrDefaultAsync();

                weekStart = nearest ?? today;
            }
        }

        var start = weekStart.Value;
        while (start.DayOfWeek != DayOfWeek.Monday)
            start = start.AddDays(-1);

        var end = start.AddDays(7);

        var schedules = await _db.PmSchedules
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .Where(s => s.PlanDate >= start && s.PlanDate < end)
            .OrderBy(s => s.PlanDate)
            .ThenBy(s => s.TaskTemplate!.Asset!.Code)
            .ToListAsync();

        var rows = schedules.Select(s => new PmWeekRow
        {
            ScheduleId = s.Id,
            TaskTemplateId = s.TaskTemplateId,
            IdPm = s.TaskTemplate?.IdPm ?? "",
            AssetCode = s.TaskTemplate?.Asset?.Code ?? "-",
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

        var vm = new PmWeekViewModel
        {
            WeekStart = start,
            WeekEnd = end.AddDays(-1),
            Rows = rows,
            TotalTask = rows.Count,
            TotalDone = rows.Count(r => r.Status == PmStatus.Done),
            TotalPending = rows.Count(r => r.Status == PmStatus.Scheduled || r.Status == PmStatus.InProgress),
            TotalDelay = rows.Count(r => r.Status == PmStatus.Overdue)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateWeek(
        int[] scheduleIds,
        DateTime?[] planDates,
        DateTime?[] actualDates,
        string?[] pics,
        PmStatus[] statuses,
        MachineCondition[] results,
        string?[] notes,
        DateTime weekStart)
    {
        int updated = 0;

        for (int i = 0; i < scheduleIds.Length; i++)
        {
            var s = await _db.PmSchedules.FirstOrDefaultAsync(x => x.Id == scheduleIds[i]);
            if (s == null) continue;

            // Simpan state BEFORE
            var statusBefore = s.Status.ToString();
            var resultBefore = s.Result.ToString();
            var picBefore = s.PIC;
            var planBefore = s.PlanDate;
            var actualBefore = s.ActualDate;
            var notesBefore = s.Notes;

            // Update
            s.PlanDate = i < planDates.Length ? planDates[i] : s.PlanDate;
            s.ActualDate = i < actualDates.Length ? actualDates[i] : null;
            s.PIC = i < pics.Length ? pics[i] : null;
            s.Status = i < statuses.Length ? statuses[i] : PmStatus.Scheduled;
            s.Result = i < results.Length ? results[i] : MachineCondition.Pending;
            s.Notes = i < notes.Length ? notes[i] : null;

            if (s.ActualDate.HasValue && s.Status == PmStatus.Scheduled)
                s.Status = PmStatus.Done;

            await _db.SaveChangesAsync();

            // Log history
            await _history.LogAsync(
                s,
                statusBefore, resultBefore, picBefore,
                planBefore, actualBefore, notesBefore);

            updated++;
        }

        TempData["Success"] = $"{updated} task berhasil diupdate.";
        return RedirectToAction(nameof(ByWeek), new { weekStart = weekStart.ToString("yyyy-MM-dd") });
    }

    // ============================================================
    // DETAIL
    // ============================================================
    public async Task<IActionResult> Detail(int id, int? year)
    {
        var y = year ?? DateTime.Today.Year;

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
            AssetCode = task.Asset?.Code ?? "-",
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

        var task = schedule.TaskTemplate;

        var vm = new PmUpdateCellViewModel
        {
            ScheduleId = schedule.Id,
            TaskTemplateId = schedule.TaskTemplateId,
            AssetId = schedule.AssetId,

            // Info task
            IdPm = task?.IdPm ?? "",
            AssetCode = task?.Asset?.Code ?? "",
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
    public async Task<IActionResult> UpdateCell(
        int id,
        DateTime? planDate,
        DateTime? actualDate,
        string? pic,
        PmStatus status,
        MachineCondition result,
        string? notes)
    {
        var schedule = await _db.PmSchedules.FirstOrDefaultAsync(s => s.Id == id);
        if (schedule == null) return NotFound();

        // Simpan state BEFORE
        var statusBefore = schedule.Status.ToString();
        var resultBefore = schedule.Result.ToString();
        var picBefore = schedule.PIC;
        var planBefore = schedule.PlanDate;
        var actualBefore = schedule.ActualDate;
        var notesBefore = schedule.Notes;

        // Update
        schedule.PlanDate = planDate;
        schedule.ActualDate = actualDate;
        schedule.PIC = pic;
        schedule.Status = status;
        schedule.Result = result;
        schedule.Notes = notes;

        if (actualDate.HasValue && status == PmStatus.Scheduled)
            schedule.Status = PmStatus.Done;

        if (status == PmStatus.Overdue && actualDate.HasValue)
            schedule.Status = PmStatus.Done;

        await _db.SaveChangesAsync();

        // Log history
        await _history.LogAsync(
            schedule,
            statusBefore, resultBefore, picBefore,
            planBefore, actualBefore, notesBefore);

        TempData["Success"] = "Jadwal berhasil diperbarui.";
        return RedirectToAction(nameof(Index), new { year = schedule.Year });
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

        var history = await _db.PmScheduleHistories
            .Where(h => h.PmScheduleId == id)
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        ViewBag.Schedule = schedule;
        ViewBag.IdPm = schedule.TaskTemplate?.IdPm;
        ViewBag.AssetCode = schedule.TaskTemplate?.Asset?.Code;
        ViewBag.SubUnitName = schedule.TaskTemplate?.SubUnit?.Name;
        ViewBag.MethodName = schedule.TaskTemplate?.Method?.Name;
        ViewBag.Month = schedule.Month;
        ViewBag.Year = schedule.Year;

        return View(history);
    }
    // ============================================================
    // BY MONTH — Update Bulanan (group Week 1-4)
    // ============================================================
    public async Task<IActionResult> ByMonth(int? year, int? month)
    {
        var y = year ?? DateTime.Today.Year;
        var m = month ?? DateTime.Today.Month;

        var start = new DateTime(y, m, 1);
        var end = start.AddMonths(1);

        var schedules = await _db.PmSchedules
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .Where(s => s.PlanDate >= start && s.PlanDate < end)
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
            var weekEndDay = Math.Min(weekStartDay + 6, maxDay);
            var weekEnd = new DateTime(y, m, weekEndDay);

            var weekSchedules = schedules
                .Where(s => s.PlanDate >= weekStart && s.PlanDate <= weekEnd)
                .ToList();

            var rows = weekSchedules.Select(s => new PmWeekRow
            {
                ScheduleId = s.Id,
                TaskTemplateId = s.TaskTemplateId,
                IdPm = s.TaskTemplate?.IdPm ?? "",
                AssetCode = s.TaskTemplate?.Asset?.Code ?? "-",
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
    public async Task<IActionResult> UpdateMonth(
        int[] scheduleIds,
        DateTime?[] planDates,
        DateTime?[] actualDates,
        string?[] pics,
        PmStatus[] statuses,
        MachineCondition[] results,
        string?[] notes,
        int year,
        int month)
    {
        int updated = 0;

        for (int i = 0; i < scheduleIds.Length; i++)
        {
            var s = await _db.PmSchedules.FirstOrDefaultAsync(x => x.Id == scheduleIds[i]);
            if (s == null) continue;

            // Simpan state BEFORE
            var statusBefore = s.Status.ToString();
            var resultBefore = s.Result.ToString();
            var picBefore = s.PIC;
            var planBefore = s.PlanDate;
            var actualBefore = s.ActualDate;
            var notesBefore = s.Notes;

            // Update
            s.PlanDate = i < planDates.Length ? planDates[i] : s.PlanDate;
            s.ActualDate = i < actualDates.Length ? actualDates[i] : null;
            s.PIC = i < pics.Length ? pics[i] : null;
            s.Status = i < statuses.Length ? statuses[i] : PmStatus.Scheduled;
            s.Result = i < results.Length ? results[i] : MachineCondition.Pending;
            s.Notes = i < notes.Length ? notes[i] : null;

            if (s.ActualDate.HasValue && s.Status == PmStatus.Scheduled)
                s.Status = PmStatus.Done;

            await _db.SaveChangesAsync();

            // Log history
            await _history.LogAsync(
                s,
                statusBefore, resultBefore, picBefore,
                planBefore, actualBefore, notesBefore);

            updated++;
        }

        TempData["Success"] = $"{updated} task berhasil diupdate.";
        return RedirectToAction(nameof(ByMonth), new { year = year, month = month });
    }
    // ============================================================
    // REGENERATE
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