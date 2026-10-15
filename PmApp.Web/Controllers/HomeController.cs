using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private const int PAGE_SIZE = 25;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // HELPER — Cari hari Senin dari suatu tanggal
    // ============================================================
    private static DateTime GetMonday(DateTime d)
    {
        int diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff).Date;
    }

    // ============================================================
    // HELPER — Minggu ke berapa dalam bulan (1-5)
    // ============================================================

    private static int GetWeekOfMonth(DateTime d)
    {
        return Math.Min(((d.Day - 1) / 7) + 1, 4);
    }


    public async Task<IActionResult> Index(
        int? year, int? month,
        string? tab = "active",
        int activePage = 1,
        int donePage = 1,
        int delayWeek = 1)
    {
        var today = DateTime.Today;
        var thisMonday = GetMonday(today);
        var nextMonday = thisMonday.AddDays(7);
        var nextNextMonday = thisMonday.AddDays(14);

        var y = year ?? today.Year;
        var m = month ?? today.Month;

        var monthStart = new DateTime(y, m, 1);
        var monthEnd = monthStart.AddMonths(1);

        var vm = new HomeDashboardViewModel
        {
            SelectedYear = y,
            SelectedMonth = m,
            MonthName = new DateTime(y, m, 1).ToString("MMMM yyyy"),
            ActiveTab = tab ?? "active",
            SelectedDelayWeek = delayWeek,
            PageSize = PAGE_SIZE,
            TotalAssets = await _db.Assets.CountAsync(),
            TotalParts = await _db.Parts.CountAsync()
        };

        // ============================================================
        // LOAD TASK DI BULAN TERPILIH
        // ============================================================
        var monthTasks = await _db.PmSchedules
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .Where(s => s.PlanDate >= monthStart
                     && s.PlanDate < monthEnd
                     && s.Status != PmStatus.Cancelled)
            .OrderBy(s => s.PlanDate)
            .ToListAsync();

        HomeTaskRow MapToRow(Models.Entities.PmSchedule s) => new HomeTaskRow
        {
            Id = s.Id,
            IdPm = s.TaskTemplate?.IdPm ?? "",
            AssetCode = s.TaskTemplate?.Asset?.Code ?? "",
            AssetName = s.TaskTemplate?.Asset?.Name ?? "",
            SubUnitName = s.TaskTemplate?.SubUnit?.Name ?? "",
            MethodName = s.TaskTemplate?.Method?.Name ?? "",
            PlanDate = s.PlanDate,
            ActualDate = s.ActualDate,
            Status = s.Status.ToString(),
            PIC = s.PIC,
            DaysLeft = s.PlanDate.HasValue ? (s.PlanDate.Value.Date - today).Days : null,
            WeekNumber = s.PlanDate.HasValue ? GetWeekOfMonth(s.PlanDate.Value) : 0
        };

        // ============================================================
        // KPI
        // ============================================================
        var totalMonth = monthTasks.Count;
        var totalDone = monthTasks.Count(s => s.Status == PmStatus.Done);

        var totalOnTime = monthTasks.Count(s =>
        {
            if (s.Status != PmStatus.Done) return false;
            if (!s.ActualDate.HasValue || !s.PlanDate.HasValue) return false;
            return GetMonday(s.PlanDate.Value) == GetMonday(s.ActualDate.Value);
        });

        vm.TotalScheduled = totalMonth;
        vm.TotalDone = totalDone;
        vm.TotalOnTime = totalOnTime;
        vm.Compliance = totalMonth == 0 ? 0 : Math.Round((double)totalOnTime / totalMonth * 100, 1);
        vm.Completion = totalMonth == 0 ? 0 : Math.Round((double)totalDone / totalMonth * 100, 1);

        // ============================================================
        // DELAY — dengan breakdown per minggu
        // ============================================================
        var delayAll = monthTasks
            .Where(s => s.Status != PmStatus.Done && s.PlanDate!.Value.Date < today)
            .OrderBy(s => s.PlanDate)
            .ToList();

        vm.DelayCountTotal = delayAll.Count;


        vm.DelayCountByWeek = delayAll
            .GroupBy(s => GetWeekOfMonth(s.PlanDate!.Value))
            .ToDictionary(g => g.Key, g => g.Count());

        // Pastikan semua minggu 1-4 tersedia
        for (int w = 1; w <= 4; w++)
        {
            if (!vm.DelayCountByWeek.ContainsKey(w))
                vm.DelayCountByWeek[w] = 0;
        }

        // Batasi pilihan minggu 1-4
        vm.SelectedDelayWeek = Math.Clamp(delayWeek, 1, 4);

        // Filter delay berdasarkan minggu terpilih
        var delayFiltered = delayAll
            .Where(s => GetWeekOfMonth(s.PlanDate!.Value)
                == vm.SelectedDelayWeek)
            .ToList();

        vm.DelayTasks = delayFiltered.Take(10).Select(MapToRow).ToList();


        // ============================================================
        // MINGGU INI — today <= PlanDate < nextMonday
        // ============================================================
        var thisWeekList = monthTasks
            .Where(s => s.Status != PmStatus.Done
                     && s.PlanDate!.Value.Date >= today
                     && s.PlanDate.Value.Date < nextMonday)
            .OrderBy(s => s.PlanDate)
            .ToList();

        vm.ThisWeekCount = thisWeekList.Count;
        vm.ThisWeekTasks = thisWeekList.Take(10).Select(MapToRow).ToList();

        // ============================================================
        // MINGGU DEPAN — nextMonday <= PlanDate < nextNextMonday
        // ============================================================
        var nextWeekList = monthTasks
            .Where(s => s.Status != PmStatus.Done
                     && s.PlanDate!.Value.Date >= nextMonday
                     && s.PlanDate.Value.Date < nextNextMonday)
            .OrderBy(s => s.PlanDate)
            .ToList();

        vm.NextWeekCount = nextWeekList.Count;
        vm.NextWeekTasks = nextWeekList.Take(10).Select(MapToRow).ToList();

        // ============================================================
        // SELESAI
        // ============================================================
        var completedList = monthTasks
            .Where(s => s.Status == PmStatus.Done)
            .OrderByDescending(s => s.ActualDate)
            .ToList();

        vm.CompletedCount = completedList.Count;
        vm.CompletedKanban = completedList.Take(10).Select(MapToRow).ToList();

        // ============================================================
        // TAB AKTIF
        // ============================================================
        var activeList = monthTasks
            .Where(s => s.Status != PmStatus.Done)
            .OrderBy(s => s.PlanDate)
            .ToList();

        vm.ActiveTotalItems = activeList.Count;
        vm.ActiveTotalPages = Math.Max(1, (int)Math.Ceiling((double)activeList.Count / PAGE_SIZE));
        vm.ActivePage = Math.Max(1, Math.Min(activePage, vm.ActiveTotalPages));

        vm.ActiveTasks = activeList
            .Skip((vm.ActivePage - 1) * PAGE_SIZE)
            .Take(PAGE_SIZE)
            .Select(MapToRow)
            .ToList();

        // ============================================================
        // TAB SELESAI
        // ============================================================
        var doneList = monthTasks
            .Where(s => s.Status == PmStatus.Done)
            .OrderByDescending(s => s.ActualDate)
            .ToList();

        vm.DoneTotalItems = doneList.Count;
        vm.DoneTotalPages = Math.Max(1, (int)Math.Ceiling((double)doneList.Count / PAGE_SIZE));
        vm.DonePage = Math.Max(1, Math.Min(donePage, vm.DoneTotalPages));

        vm.DoneTasks = doneList
            .Skip((vm.DonePage - 1) * PAGE_SIZE)
            .Take(PAGE_SIZE)
            .Select(MapToRow)
            .ToList();

        // ============================================================
        // WEEKLY CHART — per minggu dalam bulan terpilih
        // ============================================================

        int maxDay = DateTime.DaysInMonth(y, m);

        for (int w = 1; w <= 4; w++)
        {
            int startDay = (w - 1) * 7 + 1;

            // Minggu ke-4 mencakup tanggal 22 sampai akhir bulan
            int endDay = w == 4
                ? maxDay
                : Math.Min(startDay + 6, maxDay);

            var weekStart = new DateTime(y, m, startDay);
            var weekEnd = new DateTime(y, m, endDay);

            var weekSchedules = monthTasks
                .Where(s => s.PlanDate.HasValue
                         && s.PlanDate.Value.Date >= weekStart
                         && s.PlanDate.Value.Date <= weekEnd)
                .ToList();

            var delayCount = weekSchedules.Count(s =>
                s.Status != PmStatus.Done &&
                s.PlanDate.HasValue &&
                s.PlanDate.Value.Date < today);

            vm.Weekly.Add(new HomeWeeklyTrend
            {
                WeekNumber = w,
                WeekLabel = $"Minggu {w}",
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Scheduled = weekSchedules.Count,
                Done = weekSchedules.Count(s => s.Status == PmStatus.Done),
                Delay = delayCount
            });
        }


        return View(vm);
    }

    public IActionResult Privacy() => View();
}