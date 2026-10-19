using System.Data;
using Microsoft.AspNetCore.Authorization;
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
    private readonly PmIdGeneratorService _pmIdGenerator;

    public PmTaskTemplateController(
        AppDbContext db,
        PmScheduleGeneratorService generator,
        PmIdGeneratorService pmIdGenerator)
    {
        _db = db;
        _generator = generator;
        _pmIdGenerator = pmIdGenerator;
    }

    // ============================================================
    // INDEX — Redirect ke ListPm
    // ============================================================
    public IActionResult Index()
    {
        return RedirectToAction(nameof(ListPm));
    }

    // ============================================================
    // LIST PM — Grouped by Machine
    // ============================================================
    public async Task<IActionResult> ListPm(int? lineId, int? productId, string? search)
    {
        var tasksQuery = _db.PmTaskTemplates
            .Include(t => t.Asset).ThenInclude(a => a!.Line)
            .Include(t => t.Asset).ThenInclude(a => a!.Area)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .Where(t => t.IsActive)
            .AsQueryable();

        if (productId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.Asset!.ProductId == productId.Value);

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
        ViewBag.Products = await _db.Products.AsNoTracking()
            .OrderBy(p => p.Name).ToListAsync();
        ViewBag.LineId = lineId;
        ViewBag.ProductId = productId;
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
        await ValidateMachineHierarchyAsync(vm);
        if (string.IsNullOrWhiteSpace(vm.UnitName))
            ModelState.AddModelError(nameof(vm.UnitName), "Unit wajib diisi.");
        if (string.IsNullOrWhiteSpace(vm.SubUnitName))
            ModelState.AddModelError(nameof(vm.SubUnitName), "Sub Unit wajib diisi.");

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // Gunakan transaksi supaya pembangkitan ID dan penyimpanan
        // berada dalam satu unit kerja. Unique index IdPm tetap wajib.
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        try
        {
            var idPm = await _pmIdGenerator.GenerateAsync(
                vm.AssetId, vm.TaskSequence);

            var (unitId, subUnitId) = await GetOrCreateUnitSubUnitAsync(vm.UnitName, vm.SubUnitName);
            var methodId = await GetOrCreateMetodeAsync(vm.MethodName);
            var standardId = await GetOrCreateStandarAsync(vm.StandardName);
            var week = await GetWeekNumberAsync(vm.AssetId, vm.Week);

            var task = new PmTaskTemplate
            {
                IdPm = idPm,
                AssetId = vm.AssetId,
                UnitId = unitId,
                SubUnitId = subUnitId,
                MethodId = methodId,
                StandardId = standardId,
                WorkHourMinutes = vm.WorkHourMinutes,
                ManPower = vm.ManPower,
                MachineState = vm.MachineState,
                PeriodeMonth = vm.PeriodeMonth,
                StartMonth = vm.StartMonth,
                Week = week,
                Remark = vm.Remark,
                IsActive = vm.IsActive
            };

            _db.PmTaskTemplates.Add(task);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Task PM " + idPm + " berhasil ditambahkan.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(nameof(vm.TaskSequence), ex.Message);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty,
                "Gagal menyimpan Task PM. ID-PM mungkin sudah digunakan atau terjadi perubahan data bersamaan. Coba lagi.");
        }

        _db.ChangeTracker.Clear();
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    // ============================================================
    // EDIT — GET
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _db.PmTaskTemplates
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
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
            UnitName = task.Unit?.Name ?? "",
            SubUnitName = task.SubUnit?.Name ?? "",
            MethodName = task.Method?.Name ?? string.Empty,
            StandardName = task.Standard?.Name ?? string.Empty,
            WorkHourMinutes = task.WorkHourMinutes,
            ManPower = task.ManPower,
            MachineState = task.MachineState,
            PeriodeMonth = task.PeriodeMonth,
            StartMonth = task.StartMonth,
            Week = task.Week,
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
    public async Task<IActionResult> Edit(
        int id,
        PmTaskTemplateFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        // Ambil Task asli dari database.
        var task = await _db.PmTaskTemplates
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        // Mesin dan ID-PM selalu berasal dari database.
        // Jangan percaya nilai AssetId / IdPm dari form.
        vm.AssetId = task.AssetId;
        vm.IdPm = task.IdPm;

        // Hapus nilai AssetId yang mungkin dikirim pengguna.
        // Mesin asal tetap ditentukan oleh data database.
        ModelState.Remove(nameof(vm.AssetId));

        // Validasi lagi setelah memakai mesin asli.
        if (!TryValidateModel(vm))
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var (unitId, subUnitId) = await GetOrCreateUnitSubUnitAsync(vm.UnitName, vm.SubUnitName);
            var methodId = await GetOrCreateMetodeAsync(vm.MethodName);
            var standardId = await GetOrCreateStandarAsync(vm.StandardName);

            // Mesin dan ID-PM tidak diubah.
            task.UnitId = unitId;
            task.SubUnitId = subUnitId;
            task.MethodId = methodId;
            task.StandardId = standardId;
            task.WorkHourMinutes = vm.WorkHourMinutes;
            task.ManPower = vm.ManPower;
            task.MachineState = vm.MachineState;
            task.PeriodeMonth = vm.PeriodeMonth;
            task.StartMonth = vm.StartMonth;
            task.Week = vm.Week;
            task.Remark = vm.Remark;
            task.IsActive = vm.IsActive;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or DbUpdateException)
        {
            await transaction.RollbackAsync();
            _db.ChangeTracker.Clear();
            ModelState.AddModelError(string.Empty,
                "Gagal menyimpan Unit/Sub Unit atau Task. Periksa duplikasi dan coba lagi.");
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["Success"] =
            $"Task PM {task.IdPm} berhasil diperbarui.";

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
    // HELPER — Auto-assign Week 1-4 dengan rotasi per mesin
    // ============================================================
    private async Task<int> GetWeekNumberAsync(int assetId, int inputWeek)
    {
        // Kalau user pilih manual (1-4), pakai itu
        if (inputWeek >= 1 && inputWeek <= 4)
            return inputWeek;

        // Kalau 0 (Auto) → hitung jumlah task di mesin ini
        var existingCount = await _db.PmTaskTemplates
            .Where(t => t.AssetId == assetId && !t.IsDeleted)
            .CountAsync();

        // Rotasi: 1, 2, 3, 4, 1, 2, 3, 4, ...
        return (existingCount % 4) + 1;
    }

    // UNIT / SUB UNIT — mencari atau membuat dalam transaksi milik caller.
    private async Task<(int UnitId, int SubUnitId)> GetOrCreateUnitSubUnitAsync(
        string? unitInput, string? subUnitInput)
    {
        var unitName = unitInput?.Trim() ?? "";
        var subName = subUnitInput?.Trim() ?? "";
        if (unitName.Length is < 1 or > 200 || subName.Length is < 1 or > 200)
            throw new InvalidOperationException("Nama Unit/Sub Unit harus 1–200 karakter.");

        var normalizedUnit = unitName.ToLower();
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.Name.ToLower() == normalizedUnit);
        if (unit == null)
        {
            unit = new Unit
            {
                Code = "AUTO-" + Guid.NewGuid().ToString("N"),
                Name = unitName,
                Description = "Auto-created dari form PM"
            };
            _db.Units.Add(unit);
            await _db.SaveChangesAsync();
        }

        var normalizedSub = subName.ToLower();
        var sub = await _db.SubUnits.FirstOrDefaultAsync(s =>
            s.UnitId == unit.Id && s.Name.ToLower() == normalizedSub);
        if (sub == null)
        {
            sub = new SubUnit
            {
                Code = "AUTO-" + Guid.NewGuid().ToString("N"),
                Name = subName,
                UnitId = unit.Id,
                Description = "Auto-created dari form PM"
            };
            _db.SubUnits.Add(sub);
            await _db.SaveChangesAsync();
        }
        return (unit.Id, sub.Id);
    }

    [HttpGet]
    public async Task<IActionResult> GetSubUnitNames(string? unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName)) return Json(Array.Empty<string>());
        var normalized = unitName.Trim().ToLower();
        var data = await _db.SubUnits.AsNoTracking()
            .Where(s => s.Unit != null && s.Unit.Name.ToLower() == normalized)
            .OrderBy(s => s.Name).Select(s => s.Name).ToListAsync();
        return Json(data);
    }

    // ============================================================
    // HELPER — Get or Create Metode
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

    private async Task ValidateMachineHierarchyAsync(
    PmTaskTemplateFormViewModel vm)
    {
        if (!vm.PlantId.HasValue)
            ModelState.AddModelError(
                nameof(vm.PlantId), "Plant wajib dipilih.");

        if (!vm.ProductId.HasValue)
            ModelState.AddModelError(
                nameof(vm.ProductId), "Product wajib dipilih.");

        if (!vm.LineId.HasValue)
            ModelState.AddModelError(
                nameof(vm.LineId), "Line wajib dipilih.");

        if (vm.AssetId <= 0)
            ModelState.AddModelError(
                nameof(vm.AssetId), "Mesin wajib dipilih.");

        if (!vm.PlantId.HasValue ||
            !vm.ProductId.HasValue ||
            !vm.LineId.HasValue ||
            vm.AssetId <= 0)
            return;

        bool valid = await _db.Assets
            .AsNoTracking()
            .AnyAsync(a =>
                a.Id == vm.AssetId &&
                a.IsActive &&
                a.LineId == vm.LineId &&
                a.ProductId == vm.ProductId &&
                a.Line != null &&
                a.Line.ProductId == vm.ProductId &&
                a.Line.PlantId == vm.PlantId &&
                a.Line.Product != null &&
                a.Line.Product.PlantId == vm.PlantId);

        if (!valid)
        {
            ModelState.AddModelError(
                nameof(vm.AssetId),
                "Mesin tidak sesuai dengan Plant, Product, dan Line.");
        }
    }

    // ============================================================
    // HELPER — Get or Create Standar
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

    // ==========================================
    // DROPDOWN PRODUCT BERDASARKAN PLANT
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> GetProducts(int plantId)
    {
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.PlantId == plantId)
            .OrderBy(p => p.Code)
            .Select(p => new
            {
                id = p.Id,
                text = p.Code + " - " + p.Name
            })
            .ToListAsync();

        return Json(products);
    }

    // ==========================================
    // DROPDOWN LINE BERDASARKAN PRODUCT
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> GetLines(int productId)
    {
        var lines = await _db.Lines
            .AsNoTracking()
            .Where(l => l.ProductId == productId)
            .OrderBy(l => l.Code)
            .Select(l => new
            {
                id = l.Id,
                text = l.Code + " - " + l.Name
            })
            .ToListAsync();

        return Json(lines);
    }

    // ==========================================
    // DROPDOWN MACHINE BERDASARKAN LINE
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> GetMachines(int lineId)
    {
        var machines = await _db.Assets
            .AsNoTracking()
            .Where(a => a.LineId == lineId && a.IsActive)
            .OrderBy(a => a.Code)
            .Select(a => new
            {
                id = a.Id,
                text = a.Code
            })
            .ToListAsync();

        return Json(machines);
    }


    [HttpGet]
    public async Task<IActionResult> PreviewIdPm(
        int assetId,
        int? taskSequence)
    {
        if (assetId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Pilih mesin terlebih dahulu."
            });
        }

        try
        {
            string idPm = await _pmIdGenerator.GenerateAsync(
                assetId,
                taskSequence
            );

            return Json(new
            {
                success = true,
                idPm,
                mode = taskSequence.HasValue
                    ? "Manual"
                    : "Otomatis"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }


    // ============================================================
    // HELPER — Populate Dropdowns
    // Create: Filter Plant -> Product -> Line -> Machine
    // Edit  : Machine terkunci pada mesin asal
    // ============================================================
    private async Task PopulateDropdownsAsync(
        PmTaskTemplateFormViewModel vm)
    {
        // ========================================================
        // 1. PLANT
        // ========================================================
        vm.PlantList = await _db.Plants
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Code + " - " + p.Name
            })
            .ToListAsync();

        // ========================================================
        // 2. PRODUCT — Berdasarkan Plant
        // ========================================================
        vm.ProductList = new List<SelectListItem>();

        if (vm.PlantId.HasValue)
        {
            vm.ProductList = await _db.Products
                .AsNoTracking()
                .Where(p => p.PlantId == vm.PlantId.Value)
                .OrderBy(p => p.Code)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Code + " - " + p.Name
                })
                .ToListAsync();
        }

        // ========================================================
        // 3. LINE — Berdasarkan Plant dan Product
        // ========================================================
        vm.LineList = new List<SelectListItem>();

        if (vm.PlantId.HasValue && vm.ProductId.HasValue)
        {
            vm.LineList = await _db.Lines
                .AsNoTracking()
                .Where(l =>
                    l.PlantId == vm.PlantId.Value &&
                    l.ProductId == vm.ProductId.Value)
                .OrderBy(l => l.Code)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Code + " - " + l.Name
                })
                .ToListAsync();
        }

        // ========================================================
        // 4. MACHINE
        // ========================================================
        var assetsQuery = _db.Assets
            .AsNoTracking();

        if (vm.Id == 0)
        {
            // CREATE:
            // Hanya mesin aktif yang sesuai Line dan Product.
            if (vm.PlantId.HasValue &&
                vm.ProductId.HasValue &&
                vm.LineId.HasValue)
            {
                assetsQuery = assetsQuery.Where(a =>
                    a.IsActive &&
                    a.LineId == vm.LineId.Value &&
                    a.ProductId == vm.ProductId.Value &&
                    a.Line != null &&
                    a.Line.PlantId == vm.PlantId.Value &&
                    a.Line.ProductId == vm.ProductId.Value);
            }
            else
            {
                // Belum memilih hierarki lengkap.
                assetsQuery = assetsQuery.Where(a => false);
            }
        }
        else
        {
            // EDIT:
            // Tampilkan hanya mesin yang sudah digunakan Task.
            // Tidak dibatasi IsActive agar mesin nonaktif
            // tetap bisa dilihat.
            assetsQuery = assetsQuery.Where(a =>
                a.Id == vm.AssetId);
        }

        vm.AssetList = await assetsQuery
            .OrderBy(a => a.Code)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Code
            })
            .ToListAsync();

        vm.UnitSuggestion = await _db.Units.AsNoTracking()
            .OrderBy(u => u.Name).Select(u => u.Name).ToListAsync();
        vm.SubUnitSuggestion = string.IsNullOrWhiteSpace(vm.UnitName)
            ? new List<string>()
            : await _db.SubUnits.AsNoTracking()
                .Where(s => s.Unit != null && s.Unit.Name.ToLower() == vm.UnitName.Trim().ToLower())
                .OrderBy(s => s.Name).Select(s => s.Name).ToListAsync();

        // ========================================================
        // 5. UNIT
        // ========================================================
        vm.UnitList = await _db.Units
            .AsNoTracking()
            .OrderBy(u => u.Code)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Name
            })
            .ToListAsync();

        // ========================================================
        // 6. SUB UNIT
        // ========================================================
        vm.SubUnitList = await _db.SubUnits
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToListAsync();

        // ========================================================
        // 7. METODE — Select2 (Pilih atau ketik baru)
        // ========================================================
        vm.MethodSuggestion = await _db.Metodes
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => m.Name)
            .ToListAsync();

        // ========================================================
        // 8. STANDAR — Select2 (Pilih atau ketik baru)
        // ========================================================
        vm.StandardSuggestion = await _db.Standars
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => s.Name)
            .ToListAsync();
    }


}