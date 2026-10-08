using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

public class PmTaskTemplateController : Controller
{
    private readonly AppDbContext _db;
    private readonly PmScheduleGeneratorService _generator;

    public PmTaskTemplateController(AppDbContext db, PmScheduleGeneratorService generator)
    {
        _db = db;
        _generator = generator;
    }

    // ============================================================
    // INDEX
    // ============================================================
    public async Task<IActionResult> Index()
    {
        var list = await _db.PmTaskTemplates
            .Include(t => t.Asset)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .OrderBy(t => t.IdPm)
            .ToListAsync();
        return View(list);
    }

    // ============================================================
    // LIST PM — Grouped by Machine
    // ============================================================
    public async Task<IActionResult> ListPm(int? lineId, int? assetId, string? search)
    {
        var tasksQuery = _db.PmTaskTemplates
            .Include(t => t.Asset)
                .ThenInclude(a => a!.Line)
            .Include(t => t.Asset)
                .ThenInclude(a => a!.Area)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .Where(t => t.IsActive)
            .AsQueryable();

        if (assetId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.AssetId == assetId.Value);

        if (lineId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.Asset!.LineId == lineId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            tasksQuery = tasksQuery.Where(t =>
                t.IdPm.ToLower().Contains(s) ||
                t.Asset!.Code.ToLower().Contains(s) ||
                t.Asset!.Name.ToLower().Contains(s) ||
                (t.SubUnit != null && t.SubUnit.Name.ToLower().Contains(s)) ||
                (t.Unit != null && t.Unit.Name.ToLower().Contains(s)));
        }

        var tasks = await tasksQuery
            .OrderBy(t => t.Asset!.Code)
            .ThenBy(t => t.IdPm)
            .ToListAsync();

        var grouped = tasks
            .GroupBy(t => t.Asset)
            .Select(g => new PmMachineGroupViewModel
            {
                Asset = g.Key!,
                Tasks = g.ToList()
            })
            .OrderBy(g => g.Asset.Code)
            .ToList();

        ViewBag.Lines = await _db.Lines.OrderBy(l => l.Code).ToListAsync();
        ViewBag.Assets = await _db.Assets.OrderBy(a => a.Code).ToListAsync();
        ViewBag.LineId = lineId;
        ViewBag.AssetId = assetId;
        ViewBag.Search = search;

        return View(grouped);
    }

    // ============================================================
    // CREATE — GET
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new PmTaskTemplateFormViewModel();
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    // ============================================================
    // CREATE — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PmTaskTemplateFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var idPm = await GenerateUniqueIdPmAsync(vm.AssetId);

        var methodId = await GetOrCreateMetodeAsync(vm.MethodName);
        var standardId = await GetOrCreateStandarAsync(vm.StandardName);

        var task = new PmTaskTemplate
        {
            IdPm = idPm,
            AssetId = vm.AssetId,
            UnitId = vm.UnitId,
            SubUnitId = vm.SubUnitId,
            MethodId = methodId,
            StandardId = standardId,
            WorkHourMinutes = vm.WorkHourMinutes,
            ManPower = vm.ManPower,
            MachineState = vm.MachineState,
            PeriodeMonth = vm.PeriodeMonth,
            StartMonth = vm.StartMonth,
            Remark = vm.Remark,
            IsActive = vm.IsActive
        };

        _db.PmTaskTemplates.Add(task);
        await _db.SaveChangesAsync();

        await _generator.GenerateScheduleForTaskAsync(task.Id, DateTime.Today.Year);

        TempData["Success"] = "Task PM " + idPm + " berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // EDIT — GET
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _db.PmTaskTemplates
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return NotFound();

        var vm = new PmTaskTemplateFormViewModel
        {
            Id = task.Id,
            IdPm = task.IdPm,
            AssetId = task.AssetId,
            UnitId = task.UnitId,
            SubUnitId = task.SubUnitId,
            MethodName = task.Method?.Name ?? string.Empty,
            StandardName = task.Standard?.Name ?? string.Empty,
            WorkHourMinutes = task.WorkHourMinutes,
            ManPower = task.ManPower,
            MachineState = task.MachineState,
            PeriodeMonth = task.PeriodeMonth,
            StartMonth = task.StartMonth,
            Remark = task.Remark,
            IsActive = task.IsActive
        };

        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    // ============================================================
    // EDIT — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PmTaskTemplateFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var task = await _db.PmTaskTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return NotFound();

        var methodId = await GetOrCreateMetodeAsync(vm.MethodName);
        var standardId = await GetOrCreateStandarAsync(vm.StandardName);

        task.AssetId = vm.AssetId;
        task.UnitId = vm.UnitId;
        task.SubUnitId = vm.SubUnitId;
        task.MethodId = methodId;
        task.StandardId = standardId;
        task.WorkHourMinutes = vm.WorkHourMinutes;
        task.ManPower = vm.ManPower;
        task.MachineState = vm.MachineState;
        task.PeriodeMonth = vm.PeriodeMonth;
        task.StartMonth = vm.StartMonth;
        task.Remark = vm.Remark;
        task.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        await _generator.DeleteSchedulesForTaskAsync(task.Id);
        await _generator.GenerateScheduleForTaskAsync(task.Id, DateTime.Today.Year);

        TempData["Success"] = "Task PM berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // DELETE — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.PmTaskTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            TempData["Error"] = "Task tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        task.IsDeleted = true;

        var schedules = await _db.PmSchedules
            .Where(s => s.TaskTemplateId == id)
            .ToListAsync();
        foreach (var s in schedules)
        {
            s.IsDeleted = true;
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "Task PM berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // SAFETY — Generate IdPm unik, cek duplikat, increment sampai unik
    // ============================================================
    private async Task<string> GenerateUniqueIdPmAsync(int assetId)
    {
        var idPm = await _generator.GenerateIdPmAsync(assetId);

        int safety = 0;
        while (await _db.PmTaskTemplates.AnyAsync(t => t.IdPm == idPm))
        {
            if (idPm.Length != 6) break;

            var prefix = idPm.Substring(0, 4);
            var taskSeqStr = idPm.Substring(4, 2);

            if (!int.TryParse(taskSeqStr, out var taskSeq))
                break;

            taskSeq++;

            if (taskSeq > 99)
            {
                taskSeq = 0;
            }

            idPm = prefix + taskSeq.ToString("D2");

            safety++;
            if (safety > 200) break;
        }

        return idPm;
    }

    // ============================================================
    // HELPER — Get or Create Metode (auto-create dari form)
    // ============================================================
    private async Task<int?> GetOrCreateMetodeAsync(string? methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName)) return null;

        var name = methodName.Trim();

        var existing = await _db.Metodes
            .FirstOrDefaultAsync(m => m.Name.ToLower() == name.ToLower());

        if (existing != null) return existing.Id;

        var allCodes = await _db.Metodes
            .Where(m => m.Code.StartsWith("M"))
            .Select(m => m.Code)
            .ToListAsync();

        int maxNum = 0;
        foreach (var c in allCodes)
        {
            if (c.Length > 1 && int.TryParse(c.Substring(1), out var n))
            {
                if (n > maxNum) maxNum = n;
            }
        }

        var newCode = "M" + (maxNum + 1).ToString("D2");

        var letters = new string(name.Where(char.IsLetter).ToArray());
        var initial = letters.Length >= 3
            ? letters.Substring(0, 3).ToUpper()
            : letters.ToUpper();

        var newMetode = new Metode
        {
            Code = newCode,
            Name = name,
            Initial = initial,
            MachineState = MachineRunningState.OFF,
            Description = "Auto-created dari form PM"
        };

        _db.Metodes.Add(newMetode);
        await _db.SaveChangesAsync();

        return newMetode.Id;
    }

    // ============================================================
    // HELPER — Get or Create Standar (auto-create dari form)
    // ============================================================
    private async Task<int?> GetOrCreateStandarAsync(string? standardName)
    {
        if (string.IsNullOrWhiteSpace(standardName)) return null;

        var name = standardName.Trim();

        var existing = await _db.Standars
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());

        if (existing != null) return existing.Id;

        var newStandar = new Standar
        {
            Name = name,
            Description = "Auto-created dari form PM"
        };

        _db.Standars.Add(newStandar);
        await _db.SaveChangesAsync();

        return newStandar.Id;
    }

    // ============================================================
    // HELPER — Populate Dropdowns
    // ============================================================
    private async Task PopulateDropdownsAsync(PmTaskTemplateFormViewModel vm)
    {
        vm.AssetList = await _db.Assets
            .OrderBy(a => a.Code)
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Code + " - " + a.Name })
            .ToListAsync();

        vm.UnitList = await _db.Units
            .OrderBy(u => u.Code)
            .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Code + " - " + u.Name })
            .ToListAsync();

        vm.SubUnitList = await _db.SubUnits
            .OrderBy(s => s.Code)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Code + " - " + s.Name })
            .ToListAsync();

        vm.MethodSuggestion = await _db.Metodes
            .OrderBy(m => m.Name)
            .Select(m => m.Name)
            .ToListAsync();

        vm.StandardSuggestion = await _db.Standars
            .OrderBy(s => s.Name)
            .Select(s => s.Name)
            .ToListAsync();
    }
}