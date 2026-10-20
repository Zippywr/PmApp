using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.ViewModels;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

public class ReportController : Controller
{
    private readonly AppDbContext _db;
    private readonly LineAccessService _lineAccess;

    private static readonly string[] MonthNames =
    {
        "", "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember"
    };

    private static readonly string[] MonthShort =
    {
        "", "Jan", "Feb", "Mar", "Apr", "Mei", "Jun",
        "Jul", "Agu", "Sep", "Okt", "Nov", "Des"
    };

    public ReportController(AppDbContext db, LineAccessService lineAccess)
    {
        _db = db;
        _lineAccess = lineAccess;
    }

    public async Task<IActionResult> PmSchedule(int? year, int? month, int? lineId, int? assetId, string? status, string? result)
    {
        var y = year ?? DateTime.Today.Year;
        var data = await BuildReportAsync(y, month, lineId, assetId, status, result);
        var filter = await BuildFilterAsync(y, month, lineId, assetId, status, result);
        ViewBag.Filter = filter;
        return View(data);
    }

    public async Task<IActionResult> PmGraph(int? year, int? month, int? lineId, int? assetId, string? status, string? result)
    {
        var y = year ?? DateTime.Today.Year;
        var data = await BuildReportAsync(y, month, lineId, assetId, status, result);
        var filter = await BuildFilterAsync(y, month, lineId, assetId, status, result);
        ViewBag.Filter = filter;
        return View(data);
    }

    private async Task<PmReportViewModel> BuildReportAsync(
        int year, int? month, int? lineId, int? assetId, string? status, string? result)
    {
        var assigned = await _lineAccess.AssignedLineIdsAsync();
        var query = _lineAccess.ScopeSchedules(_db.PmSchedules, assigned)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Asset).ThenInclude(a => a!.Line)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.SubUnit)
            .Include(s => s.TaskTemplate).ThenInclude(t => t!.Method)
            .Where(s => s.Year == year)
            .AsQueryable();

        if (month.HasValue)
            query = query.Where(s => s.Month == month.Value);

        if (assetId.HasValue)
            query = query.Where(s => s.AssetId == assetId.Value);

        if (lineId.HasValue)
            query = query.Where(s => s.TaskTemplate!.Asset!.LineId == lineId.Value);

        var schedules = await query.ToListAsync();

        if (!string.IsNullOrWhiteSpace(status))
            schedules = schedules.Where(s => s.Status.ToString() == status).ToList();

        if (!string.IsNullOrWhiteSpace(result))
            schedules = schedules.Where(s => s.Result.ToString() == result).ToList();

        var rows = schedules.Select(s => new PmReportRow
        {
            ScheduleId = s.Id,
            IdPm = s.TaskTemplate?.IdPm ?? "",
            AssetCode = s.TaskTemplate?.Asset?.Code ?? "",
            AssetName = s.TaskTemplate?.Asset?.Name ?? "",
            LineName = s.TaskTemplate?.Asset?.Line?.Name ?? "",
            SubUnitName = s.TaskTemplate?.SubUnit?.Name ?? "",
            MethodName = s.TaskTemplate?.Method?.Name ?? "",
            WorkHourMinutes = s.TaskTemplate?.WorkHourMinutes ?? 0,
            ManPower = s.TaskTemplate?.ManPower ?? 0,
            Month = s.Month,
            MonthName = MonthShort[s.Month],
            PlanDate = s.PlanDate,
            ActualDate = s.ActualDate,
            PIC = s.PIC,
            Status = s.Status.ToString(),
            Result = s.Result.ToString(),
            Notes = s.Notes,
            IsOnTime = s.Status == PmStatus.Done
                       && s.ActualDate.HasValue
                       && s.PlanDate.HasValue
                       && s.ActualDate.Value.Date <= s.PlanDate.Value.Date
        })
        .OrderBy(r => r.Month)
        .ThenBy(r => r.AssetCode)
        .ThenBy(r => r.IdPm)
        .ToList();

        var totalScheduled = schedules.Count;
        var totalDone = schedules.Count(s => s.Status == PmStatus.Done);
        var totalOnTime = rows.Count(r => r.IsOnTime);
        var totalOverdue = schedules.Count(s => s.Status == PmStatus.Overdue);
        var totalPending = schedules.Count(s => s.Status == PmStatus.Scheduled || s.Status == PmStatus.InProgress);

        var compliance = totalScheduled == 0 ? 0 : Math.Round((double)totalOnTime / totalScheduled * 100, 1);
        var completion = totalScheduled == 0 ? 0 : Math.Round((double)totalDone / totalScheduled * 100, 1);

        var rOK = schedules.Count(s => s.Result == MachineCondition.OK);
        var rPartial = schedules.Count(s => s.Result == MachineCondition.Partial);
        var rNG = schedules.Count(s => s.Result == MachineCondition.NG);
        var rPending = schedules.Count(s => s.Result == MachineCondition.Pending);

        var statusChart = schedules
            .GroupBy(s => s.Status)
            .Select(g => new ChartDataPoint { Label = g.Key.ToString(), Value = g.Count() })
            .OrderBy(x => x.Label).ToList();

        var resultChart = schedules
            .GroupBy(s => s.Result)
            .Select(g => new ChartDataPoint { Label = g.Key.ToString(), Value = g.Count() })
            .OrderBy(x => x.Label).ToList();

        // ============================================================
        // MONTHLY TREND — Plan vs Actual vs Delay + Performance %
        // ============================================================
        var monthlyTrend = Enumerable.Range(1, 12).Select(m =>
        {
            var ms = schedules.Where(s => s.Month == m).ToList();
            var plan = ms.Count;
            var done = ms.Count(s => s.Status == PmStatus.Done);
            var delay = ms.Count(s => s.Status == PmStatus.Overdue);
            var perfRate = plan == 0 ? 0 : Math.Round((double)done / plan * 100, 1);

            return new MonthlyTrendPoint
            {
                Month = MonthShort[m],
                Scheduled = plan,
                Done = done,
                OnTime = ms.Count(s =>
                    s.Status == PmStatus.Done
                    && s.ActualDate.HasValue
                    && s.PlanDate.HasValue
                    && s.ActualDate.Value.Date <= s.PlanDate.Value.Date),
                Delay = delay,
                PerformanceRate = perfRate
            };
        }).ToList();

        var perLine = schedules
            .GroupBy(s => s.TaskTemplate?.Asset?.Line?.Name ?? "Tanpa Line")
            .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value).ToList();

        var perMachine = schedules
            .GroupBy(s => s.TaskTemplate?.Asset?.Code ?? "-")
            .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10).ToList();

        string? lineName = null;
        if (lineId.HasValue)
        {
            var l = await _db.Lines.FirstOrDefaultAsync(x => x.Id == lineId.Value &&
                (_lineAccess.IsManager || assigned.Contains(x.Id)));
            lineName = l?.Name;
        }
        string? assetName = null;
        if (assetId.HasValue)
        {
            var a = await _lineAccess.ScopeAssets(_db.Assets, assigned)
                .FirstOrDefaultAsync(x => x.Id == assetId.Value);
            assetName = a?.Code + " - " + a?.Name;
        }

        return new PmReportViewModel
        {
            Year = year,
            Month = month,
            LineName = lineName,
            AssetName = assetName,
            Rows = rows,
            TotalScheduled = totalScheduled,
            TotalDone = totalDone,
            TotalOnTime = totalOnTime,
            TotalOverdue = totalOverdue,
            TotalPending = totalPending,
            ResultOK = rOK,
            ResultPartial = rPartial,
            ResultNG = rNG,
            ResultPending = rPending,
            ComplianceRate = compliance,
            CompletionRate = completion,
            StatusChart = statusChart,
            ResultChart = resultChart,
            MonthlyTrend = monthlyTrend,
            PerLineChart = perLine,
            PerMachineChart = perMachine
        };
    }

    private async Task<PmReportFilterViewModel> BuildFilterAsync(
        int year, int? month, int? lineId, int? assetId, string? status, string? result)
    {
        var assigned = await _lineAccess.AssignedLineIdsAsync();
        var years = await _lineAccess.ScopeSchedules(_db.PmSchedules, assigned)
            .Select(s => s.Year).Distinct()
            .OrderByDescending(y => y).ToListAsync();

        if (!years.Contains(year))
            years.Insert(0, year);

        return new PmReportFilterViewModel
        {
            Year = year,
            Month = month,
            LineId = lineId,
            AssetId = assetId,
            Status = status,
            Result = result,

            Years = years.Select(y => new SelectListItem
            {
                Value = y.ToString(),
                Text = y.ToString(),
                Selected = y == year
            }).ToList(),

            Months = Enumerable.Range(1, 12).Select(m => new SelectListItem
            {
                Value = m.ToString(),
                Text = MonthNames[m],
                Selected = month == m
            }).ToList(),

            Lines = await _db.Lines
                .Where(l => _lineAccess.IsManager || assigned.Contains(l.Id))
                .OrderBy(l => l.Code)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Code + " - " + l.Name,
                    Selected = lineId == l.Id
                }).ToListAsync(),

            Assets = await _lineAccess.ScopeAssets(_db.Assets, assigned)
                .OrderBy(a => a.Code)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Code + " - " + a.Name,
                    Selected = assetId == a.Id
                }).ToListAsync(),

            Statuses = Enum.GetNames(typeof(PmStatus))
                .Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s == "Overdue" ? "Delay" : s,
                    Selected = status == s
                }).ToList(),

            Results = Enum.GetNames(typeof(MachineCondition))
                .Select(r => new SelectListItem
                {
                    Value = r,
                    Text = r,
                    Selected = result == r
                }).ToList()
        };
    }
}