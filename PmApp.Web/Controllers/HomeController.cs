using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

[Authorize(Roles = "Admin,TS,LineHead")]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly ProductAccessService _productAccess;
    private const int PageSize = 20;
    private const int BoardLimit = 8;

    public HomeController(AppDbContext db, ProductAccessService productAccess)
    {
        _db = db;
        _productAccess = productAccess;
    }

    private static DateTime MondayOf(DateTime date)
    {
        int difference = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-difference);
    }

    // Sama dengan pengelompokan ByMonth: tanggal 1-7, 8-14, 15-21, 22-akhir bulan.
    private static int WeekOfMonth(DateTime date)
        => Math.Min((date.Day - 1) / 7 + 1, 4);

    public async Task<IActionResult> Index(
        int? year, int? month, int? productId,
        string? tab = "active", int activePage = 1,
        int donePage = 1, int delayWeek = 1)
    {
        var today = DateTime.Today;
        int y = year ?? today.Year;
        int m = month ?? today.Month;
        if (y < 2000 || y > 2100 || m < 1 || m > 12)
            return BadRequest("Bulan atau tahun tidak valid.");
        if (productId.HasValue && productId.Value <= 0)
            return BadRequest("Product tidak valid.");

        var start = new DateTime(y, m, 1);
        var end = start.AddMonths(1);
        var monday = MondayOf(today);
        var nextMonday = monday.AddDays(7);
        var mondayAfterNext = nextMonday.AddDays(7);

        // Selalu filter pada server, termasuk permintaan URL manual.
        var allowed = await _productAccess.AssignedProductIdsAsync();
        var availableProducts = await _db.Products.AsNoTracking()
            .Where(p => _productAccess.IsManager || allowed.Contains(p.Id))
            .OrderBy(p => p.Name)
            .Select(p => new HomeProductOption { Id = p.Id, Name = p.Name })
            .ToListAsync();
        if (productId.HasValue && !availableProducts.Any(p => p.Id == productId.Value))
            return NotFound();

        var assetsQuery = _productAccess.ScopeAssets(_db.Assets.AsNoTracking(), allowed);
        var scheduleQuery = _productAccess.ScopeSchedules(
                _db.PmSchedules.AsNoTracking(), allowed)
            .Where(s => s.PlanDate >= start && s.PlanDate < end
                        && s.Status != PmStatus.Cancelled);

        if (productId.HasValue)
        {
            int selectedId = productId.Value;
            assetsQuery = assetsQuery.Where(a => a.ProductId == selectedId);
            scheduleQuery = scheduleQuery.Where(s => s.TaskTemplate != null
                && s.TaskTemplate.Asset != null
                && s.TaskTemplate.Asset.ProductId == selectedId);
        }

        var vm = new HomeDashboardViewModel
        {
            SelectedYear = y,
            SelectedMonth = m,
            SelectedProductId = productId,
            ProductOptions = availableProducts,
            MonthName = start.ToString("MMMM yyyy", new System.Globalization.CultureInfo("id-ID")),
            ActiveTab = string.Equals(tab, "done", StringComparison.OrdinalIgnoreCase) ? "done" : "active",
            SelectedDelayWeek = Math.Clamp(delayWeek, 1, 4),
            PageSize = PageSize,
            TotalAssets = await assetsQuery.CountAsync()
        };

        // Read-only; akses Product diterapkan sebelum mengambil data.
        // Dashboard membaca sumber jadwal yang sama dengan Pelaksanaan PM.
        var schedules = await scheduleQuery
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .OrderBy(s => s.PlanDate)
            .ThenBy(s => s.Id)
            .ToListAsync();

        // Selaras dengan status Overdue tersimpan, juga mendeteksi Plan yang sudah lewat
        // walaupun pembaruan status terjadwal belum dijalankan.
        bool IsDelay(PmSchedule s) =>
            s.Status != PmStatus.Done && (s.Status == PmStatus.Overdue ||
            (s.PlanDate.HasValue && s.PlanDate.Value.Date < today));

        HomeTaskRow Row(PmSchedule s) => new()
        {
            Id = s.Id,
            IdPm = s.TaskTemplate?.IdPm ?? "-",
            AssetCode = s.TaskTemplate?.Asset?.DisplayCode ?? "-",
            AssetName = s.TaskTemplate?.Asset?.Name ?? "-",
            SubUnitName = s.TaskTemplate?.SubUnit?.Name ?? "-",
            MethodName = s.TaskTemplate?.Method?.Name ?? "-",
            PlanDate = s.PlanDate,
            ActualDate = s.ActualDate,
            Status = s.Status.ToString(),
            IsDelayed = IsDelay(s),
            PIC = s.PIC,
            DaysLeft = s.PlanDate.HasValue ? (s.PlanDate.Value.Date - today).Days : null,
            WeekNumber = s.PlanDate.HasValue ? WeekOfMonth(s.PlanDate.Value) : 0
        };

        var completed = schedules.Where(s => s.Status == PmStatus.Done)
            .OrderByDescending(s => s.ActualDate).ThenByDescending(s => s.Id).ToList();
        var active = schedules.Where(s => s.Status != PmStatus.Done).ToList();
        var delayed = active.Where(IsDelay).ToList();

        vm.TotalScheduled = schedules.Count;
        vm.TotalDone = completed.Count;
        vm.DelayCountTotal = delayed.Count;
        vm.TotalPending = active.Count - delayed.Count;
        vm.TotalOnTime = completed.Count(s => s.PlanDate.HasValue
            && s.ActualDate.HasValue
            && s.PlanDate.Value.Year == s.ActualDate.Value.Year
            && s.PlanDate.Value.Month == s.ActualDate.Value.Month
            && WeekOfMonth(s.PlanDate.Value) == WeekOfMonth(s.ActualDate.Value));
        vm.Completion = vm.TotalScheduled == 0 ? 0
            : Math.Round(100.0 * vm.TotalDone / vm.TotalScheduled, 1);
        vm.Compliance = vm.TotalScheduled == 0 ? 0
            : Math.Round(100.0 * vm.TotalOnTime / vm.TotalScheduled, 1);

        vm.DelayCountByWeek = Enumerable.Range(1, 4).ToDictionary(
            w => w, w => delayed.Count(s => WeekOfMonth(s.PlanDate!.Value) == w));
        vm.DelayTasks = delayed
            .Where(s => WeekOfMonth(s.PlanDate!.Value) == vm.SelectedDelayWeek)
            .OrderBy(s => s.PlanDate).Take(BoardLimit).Select(Row).ToList();

        // Hanya task pada bulan terpilih. Card mingguan tidak mengambil data bulan lain.
        var thisWeek = active.Where(s => s.PlanDate.HasValue
            && s.PlanDate.Value.Date >= today
            && s.PlanDate.Value.Date < nextMonday)
            .OrderBy(s => s.PlanDate).ToList();
        var nextWeek = active.Where(s => s.PlanDate.HasValue
            && s.PlanDate.Value.Date >= nextMonday
            && s.PlanDate.Value.Date < mondayAfterNext)
            .OrderBy(s => s.PlanDate).ToList();
        vm.ThisWeekCount = thisWeek.Count;
        vm.ThisWeekTasks = thisWeek.Take(BoardLimit).Select(Row).ToList();
        vm.NextWeekCount = nextWeek.Count;
        vm.NextWeekTasks = nextWeek.Take(BoardLimit).Select(Row).ToList();
        vm.CompletedCount = completed.Count;
        vm.CompletedKanban = completed.Take(BoardLimit).Select(Row).ToList();

        vm.ActiveTotalItems = active.Count;
        vm.ActiveTotalPages = Math.Max(1, (int)Math.Ceiling(active.Count / (double)PageSize));
        vm.ActivePage = Math.Clamp(activePage, 1, vm.ActiveTotalPages);
        vm.ActiveTasks = active.Skip((vm.ActivePage - 1) * PageSize)
            .Take(PageSize).Select(Row).ToList();

        vm.DoneTotalItems = completed.Count;
        vm.DoneTotalPages = Math.Max(1, (int)Math.Ceiling(completed.Count / (double)PageSize));
        vm.DonePage = Math.Clamp(donePage, 1, vm.DoneTotalPages);
        vm.DoneTasks = completed.Skip((vm.DonePage - 1) * PageSize)
            .Take(PageSize).Select(Row).ToList();

        for (int w = 1; w <= 4; w++)
        {
            int first = (w - 1) * 7 + 1;
            int last = w == 4 ? DateTime.DaysInMonth(y, m) : first + 6;
            var week = schedules.Where(s => s.PlanDate.HasValue
                && WeekOfMonth(s.PlanDate.Value) == w).ToList();
            vm.Weekly.Add(new HomeWeeklyTrend
            {
                WeekNumber = w,
                WeekLabel = $"W{w}",
                WeekStart = new DateTime(y, m, first),
                WeekEnd = new DateTime(y, m, last),
                Scheduled = week.Count,
                Done = week.Count(s => s.Status == PmStatus.Done),
                Delay = week.Count(IsDelay)
            });
        }

        return View(vm);
    }

    public IActionResult Privacy() => View();
}
