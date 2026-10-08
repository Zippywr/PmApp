using Microsoft.AspNetCore.Mvc;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

public class ExportController : Controller
{
    private readonly PmExportService _export;

    public ExportController(PmExportService export)
    {
        _export = export;
    }

    public IActionResult Index()
    {
        return View();
    }

    // ============================================================
    // LIST PM (SEMUA MESIN)
    // ============================================================
    public async Task<IActionResult> ListPmExcel(int? assetId)
    {
        var data = await _export.GetTaskTemplatesAsync(assetId);
        var bytes = _export.ToExcel(data);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ListPM_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ListPmPdf(int? assetId)
    {
        var data = await _export.GetTaskTemplatesAsync(assetId);
        var bytes = _export.ToPdf(data, "List PM");
        return File(bytes, "application/pdf",
            $"ListPM_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    public async Task<IActionResult> ListPmCsv(int? assetId)
    {
        var data = await _export.GetTaskTemplatesAsync(assetId);
        var bytes = _export.ToCsv(data);
        return File(bytes, "text/csv",
            $"ListPM_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    // ============================================================
    // PER MESIN
    // ============================================================
    public async Task<IActionResult> MachineExcel(int assetId)
    {
        var asset = await _export.GetAssetDetailAsync(assetId);
        if (asset == null) return NotFound();

        var tasks = await _export.GetTasksByAssetAsync(assetId);
        var bytes = _export.ToMachineExcel(asset, tasks);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PM_{asset.Code}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> MachinePdf(int assetId)
    {
        var asset = await _export.GetAssetDetailAsync(assetId);
        if (asset == null) return NotFound();

        var tasks = await _export.GetTasksByAssetAsync(assetId);
        var bytes = _export.ToMachinePdf(asset, tasks);

        return File(bytes, "application/pdf",
            $"PM_{asset.Code}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    public async Task<IActionResult> MachineCsv(int assetId)
    {
        var asset = await _export.GetAssetDetailAsync(assetId);
        if (asset == null) return NotFound();

        var tasks = await _export.GetTasksByAssetAsync(assetId);
        var bytes = _export.ToMachineCsv(asset, tasks);

        return File(bytes, "text/csv",
            $"PM_{asset.Code}_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    // ============================================================
    // JADWAL PM (MATRIX 12 BULAN)
    // ============================================================
    public async Task<IActionResult> ScheduleExcel(int? year, int? assetId)
    {
        var y = year ?? DateTime.Today.Year;
        var data = await _export.GetScheduleMatrixAsync(y, assetId);
        var bytes = _export.ToScheduleExcel(data);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"JadwalPM_{y}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> SchedulePdf(int? year, int? assetId)
    {
        var y = year ?? DateTime.Today.Year;
        var data = await _export.GetScheduleMatrixAsync(y, assetId);
        var bytes = _export.ToSchedulePdf(data);

        return File(bytes, "application/pdf",
            $"JadwalPM_{y}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    public async Task<IActionResult> ScheduleCsv(int? year, int? assetId)
    {
        var y = year ?? DateTime.Today.Year;
        var data = await _export.GetScheduleMatrixAsync(y, assetId);
        var bytes = _export.ToScheduleCsv(data);

        return File(bytes, "text/csv",
            $"JadwalPM_{y}_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }
}