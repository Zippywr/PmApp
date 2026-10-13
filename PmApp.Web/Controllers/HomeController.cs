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

    public async Task<IActionResult> Index(
        int? year, int? month,
        string? tab = "active",
        int activePage = 1,
        int donePage = 1)
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
            DaysLeft = s.PlanDate.HasValue ? (s.PlanDate.Value.Date - today).Days : null
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
        // KATEGORI KANBAN (4 KOLOM)
        // ============================================================

        // 1. DELAY — PlanDate < hari ini, belum Done
        var delayList = monthTasks
            .Where(s => s.Status != PmStatus.Done && s.PlanDate!.Value.Date < today)
            .OrderBy(s => s.PlanDate)
            .ToList();

        // 2. MINGGU INI — today <= PlanDate < nextMonday, belum Done
        var thisWeekList = monthTasks
            .Where(s => s.Status != PmStatus.Done
                     && s.PlanDate!.Value.Date >= today
                     && s.PlanDate.Value.Date < nextMonday)
            .OrderBy(s => s.PlanDate)
            .ToList();

        // 3. MINGGU DEPAN — nextMonday <= PlanDate < nextNextMonday, belum Done
        var nextWeekList = monthTasks
            .Where(s => s.Status != PmStatus.Done
                     && s.PlanDate!.Value.Date >= nextMonday
                     && s.PlanDate.Value.Date < nextNextMonday)
            .OrderBy(s => s.PlanDate)
            .ToList();

        // 4. SELESAI — Status Done
        var completedList = monthTasks
            .Where(s => s.Status == PmStatus.Done)
            .OrderByDescending(s => s.ActualDate)
            .ToList();

        // Counter
        vm.DelayCount = delayList.Count;
        vm.ThisWeekCount = thisWeekList.Count;
        vm.NextWeekCount = nextWeekList.Count;
        vm.CompletedCount = completedList.Count;

        // Preview max 10 untuk kanban
        vm.DelayTasks = delayList.Take(10).Select(MapToRow).ToList();
        vm.ThisWeekTasks = thisWeekList.Take(10).Select(MapToRow).ToList();
        vm.NextWeekTasks = nextWeekList.Take(10).Select(MapToRow).ToList();
        vm.CompletedKanban = completedList.Take(10).Select(MapToRow).ToList();

        // ============================================================
        // TAB AKTIF — semua task belum Done
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
        // TAB SELESAI — semua task Done
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
        // TREND 6 BULAN
        // ============================================================
        for (int i = 5; i >= 0; i--)
        {
            var mm = monthStart.AddMonths(-i);
            var mmEnd = mm.AddMonths(1);

            var list = await _db.PmSchedules
                .Where(s => s.PlanDate >= mm && s.PlanDate < mmEnd)
                .ToListAsync();

            vm.Monthly.Add(new HomeMonthlyTrend
            {
                Month = mm.ToString("MMM yyyy"),
                Scheduled = list.Count,
                Done = list.Count(s => s.Status == PmStatus.Done)
            });
        }

        return View(vm);
    }

    public IActionResult Privacy() => View();
}