
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

[Authorize(Roles = "Admin,TS")]
public class AssetController : Controller
{
    private readonly AppDbContext _db;

    public AssetController(AppDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // INDEX - DAFTAR MACHINE
    // =====================================================
    public async Task<IActionResult> Index()
    {
        var list = await _db.Assets
            .AsNoTracking()
            .Include(a => a.Line)
                .ThenInclude(l => l!.Product)
                    .ThenInclude(p => p!.Plant)
            .Include(a => a.Line)
                .ThenInclude(l => l!.Plant)
            .Include(a => a.Product)
            .Include(a => a.McCategory)
            .OrderBy(a => a.Code)
            .ToListAsync();

        return View(list);
    }

    // =====================================================
    // DETAILS MACHINE & BOM
    // =====================================================
    public async Task<IActionResult> Details(int id)
    {
        var asset = await _db.Assets
            .AsNoTracking()
            .Include(a => a.Line)
                .ThenInclude(l => l!.Product)
                    .ThenInclude(p => p!.Plant)
            .Include(a => a.Area)
            .Include(a => a.McCategory)
            .Include(a => a.MachineFunction)
            .Include(a => a.Brand)
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
            return NotFound();

        var bom = await _db.AssetParts
            .AsNoTracking()
            .Include(ap => ap.Part)
                .ThenInclude(p => p!.Brand)
            .Where(ap => ap.AssetId == id)
            .OrderBy(ap => ap.Part!.PartNo)
            .ToListAsync();

        var usedPartIds = bom
            .Select(b => b.PartId)
            .ToList();

        var availableParts = await _db.Parts
            .AsNoTracking()
            .Where(p => !usedPartIds.Contains(p.Id))
            .OrderBy(p => p.PartNo)
            .ToListAsync();

        ViewBag.Asset = asset;
        ViewBag.AvailableParts = availableParts;

        return View(bom);
    }

    // =====================================================
    // CREATE - GET
    // =====================================================
    [Authorize(Roles = "Admin,TS")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new AssetFormViewModel();

        await PopulateDropdownsAsync(vm);

        return View(vm);
    }

    // =====================================================
    // CREATE - POST
    // =====================================================
    [Authorize(Roles = "Admin,TS")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AssetFormViewModel vm)
    {
        NormalizeForm(vm);

        await ValidateHierarchyAsync(vm);

        await ValidateCategoryAsync(vm);

        bool duplicateCode = await _db.Assets
            .AnyAsync(a => a.Code == vm.Code);

        if (duplicateCode)
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode mesin sudah digunakan.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var asset = new Asset
        {
            Code = vm.Code,
            Name = vm.Code,
            OpNo = vm.OpNo,

            // Hierarki baru
            LineId = vm.LineId,
            ProductId = vm.ProductId,

            // Area tidak lagi diisi oleh form baru
            AreaId = null,

            // Informasi mesin
            McCategoryId = vm.McCategoryId,
            MachineFunctionId = vm.MachineFunctionId,
            BrandId = vm.BrandId,

            Model = vm.Model,
            SerialNumber = vm.SerialNumber,
            YearMade = vm.YearMade,
            Capacity = vm.Capacity,
            InstallDate = vm.InstallDate,
            Description = vm.Description,
            Criticality = vm.Criticality,
            IsActive = vm.IsActive
        };

        _db.Assets.Add(asset);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (IsDuplicateKey(ex))
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode mesin sudah digunakan.");

            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["Success"] =
            "Mesin berhasil ditambahkan.";

        return RedirectToAction(
            nameof(Details),
            new { id = asset.Id });
    }

    // =====================================================
    // EDIT - GET
    // =====================================================
    [Authorize(Roles = "Admin,TS")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _db.Assets
            .Include(a => a.Line)
                .ThenInclude(l => l!.Product)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
            return NotFound();

        var vm = new AssetFormViewModel
        {
            Id = asset.Id,

            Code = asset.Code,
            OpNo = asset.OpNo,

            // Plant berasal dari Product milik Line
            PlantId = asset.Line?.Product?.PlantId,

            ProductId = asset.ProductId,
            LineId = asset.LineId,

            // Dipertahankan selama transisi
            AreaId = asset.AreaId,

            McCategoryId = asset.McCategoryId,
            MachineFunctionId = asset.MachineFunctionId,
            BrandId = asset.BrandId,

            Model = asset.Model,
            SerialNumber = asset.SerialNumber,
            YearMade = asset.YearMade,
            Capacity = asset.Capacity,
            InstallDate = asset.InstallDate,
            Description = asset.Description,
            Criticality = asset.Criticality,
            IsActive = asset.IsActive
        };

        await PopulateDropdownsAsync(vm);

        return View(vm);
    }

    // =====================================================
    // EDIT - POST
    // =====================================================
    [Authorize(Roles = "Admin,TS")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AssetFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        var asset = await _db.Assets
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
            return NotFound();

        NormalizeForm(vm);

        await ValidateHierarchyAsync(vm);

        await ValidateCategoryAsync(vm);

        bool duplicateCode = await _db.Assets
            .AnyAsync(a =>
                a.Code == vm.Code &&
                a.Id != id);

        if (duplicateCode)
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode mesin sudah digunakan.");
        }

        // Selama transisi, Line/Product mesin yang
        // mempunyai jadwal PM tidak boleh dipindahkan
        // melalui Edit biasa.
        bool hierarchyChanged =
            asset.LineId != vm.LineId ||
            asset.ProductId != vm.ProductId;

        if (hierarchyChanged)
        {
            bool hasPmHistory = await _db.PmSchedules
                .IgnoreQueryFilters()
                .AnyAsync(s => s.AssetId == id);

            if (hasPmHistory)
            {
                ModelState.AddModelError(
                    nameof(vm.LineId),
                    "Mesin sudah memiliki data jadwal PM. " +
                    "Perubahan Line atau Product harus " +
                    "melalui proses pemindahan khusus.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        asset.Code = vm.Code;
        asset.Name = vm.Code;
        asset.OpNo = vm.OpNo;

        asset.LineId = vm.LineId;
        asset.ProductId = vm.ProductId;

        // AreaId lama sengaja tidak diubah.
        // asset.AreaId = vm.AreaId;  <-- jangan digunakan

        asset.McCategoryId = vm.McCategoryId;
        asset.MachineFunctionId = vm.MachineFunctionId;
        asset.BrandId = vm.BrandId;

        asset.Model = vm.Model;
        asset.SerialNumber = vm.SerialNumber;
        asset.YearMade = vm.YearMade;
        asset.Capacity = vm.Capacity;
        asset.InstallDate = vm.InstallDate;
        asset.Description = vm.Description;
        asset.Criticality = vm.Criticality;
        asset.IsActive = vm.IsActive;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (IsDuplicateKey(ex))
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode mesin sudah digunakan.");

            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["Success"] =
            "Mesin berhasil diperbarui.";

        return RedirectToAction(
            nameof(Details),
            new { id = asset.Id });
    }

    // =====================================================
    // DELETE - SOFT DELETE
    // =====================================================
    [Authorize(Roles = "Admin,TS")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var asset = await _db.Assets
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
        {
            TempData["Error"] =
                "Mesin tidak ditemukan.";

            return RedirectToAction(nameof(Index));
        }

        // Periksa BOM termasuk yang sudah soft delete
        bool hasBomHistory = await _db.AssetParts
            .IgnoreQueryFilters()
            .AnyAsync(ap => ap.AssetId == id);

        if (hasBomHistory)
        {
            TempData["Error"] =
                "Mesin tidak dapat dihapus karena " +
                "memiliki riwayat BOM atau part.";

            return RedirectToAction(nameof(Index));
        }

        // Periksa riwayat PM termasuk soft delete
        bool hasPmHistory = await _db.PmSchedules
            .IgnoreQueryFilters()
            .AnyAsync(s => s.AssetId == id);

        if (hasPmHistory)
        {
            TempData["Error"] =
                "Mesin tidak dapat dihapus karena " +
                "sudah memiliki jadwal atau riwayat PM.";

            return RedirectToAction(nameof(Index));
        }

        asset.IsDeleted = true;

        await _db.SaveChangesAsync();

        TempData["Success"] =
            "Mesin berhasil dihapus.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // BOM - ADD PART
    // =====================================================
    [Authorize(Roles = "Admin,TS")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(
        int AssetId,
        int PartId,
        int Quantity,
        string? Position,
        string? Notes)
    {
        var asset = await _db.Assets
            .FirstOrDefaultAsync(a => a.Id == AssetId);

        if (asset == null)
        {
            TempData["Error"] =
                "Mesin tidak ditemukan.";

            return RedirectToAction(nameof(Index));
        }

        var part = await _db.Parts
            .FirstOrDefaultAsync(p => p.Id == PartId);

        if (part == null)
        {
            TempData["Error"] =
                "Part tidak ditemukan.";

            return RedirectToAction(
                nameof(Details),
                new { id = AssetId });
        }

        if (Quantity < 1)
        {
            TempData["Error"] =
                "Jumlah part harus minimal 1.";

            return RedirectToAction(
                nameof(Details),
                new { id = AssetId });
        }

        bool exists = await _db.AssetParts
            .IgnoreQueryFilters()
            .AnyAsync(ap =>
                ap.AssetId == AssetId &&
                ap.PartId == PartId);

        if (exists)
        {
            TempData["Error"] =
                "Part pernah terdaftar pada mesin ini. " +
                "Periksa data BOM sebelum menambahkan ulang.";

            return RedirectToAction(
                nameof(Details),
                new { id = AssetId });
        }

        var assetPart = new AssetPart
        {
            AssetId = AssetId,
            PartId = PartId,
            Quantity = Quantity,
            Position = Position?.Trim(),
            Notes = Notes?.Trim()
        };

        _db.AssetParts.Add(assetPart);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (IsDuplicateKey(ex))
        {
            TempData["Error"] =
                "Part sudah terdaftar pada mesin ini.";

            return RedirectToAction(
                nameof(Details),
                new { id = AssetId });
        }

        TempData["Success"] =
            "Part berhasil ditambahkan.";

        return RedirectToAction(
            nameof(Details),
            new { id = AssetId });
    }

    // =====================================================
    // BOM - REMOVE PART (SOFT DELETE)
    // =====================================================
    [Authorize(Roles = "Admin,TS")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePart(int id)
    {
        var assetPart = await _db.AssetParts
            .FirstOrDefaultAsync(ap => ap.Id == id);

        if (assetPart == null)
        {
            TempData["Error"] =
                "Data BOM tidak ditemukan.";

            return RedirectToAction(nameof(Index));
        }

        int assetId = assetPart.AssetId;

        assetPart.IsDeleted = true;

        await _db.SaveChangesAsync();

        TempData["Success"] =
            "Part berhasil dihapus dari mesin.";

        return RedirectToAction(
            nameof(Details),
            new { id = assetId });
    }

    // =====================================================
    // AJAX - PRODUCT BERDASARKAN PLANT
    // =====================================================
    [HttpGet]
    public async Task<IActionResult> GetProducts(int plantId)
    {
        var data = await _db.Products
            .AsNoTracking()
            .Where(p => p.PlantId == plantId)
            .OrderBy(p => p.Code)
            .Select(p => new
            {
                id = p.Id,
                text = p.Code + " - " + p.Name
            })
            .ToListAsync();

        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetLines(int productId)
    {
        var data = await _db.Lines
            .AsNoTracking()
            .Where(l => l.ProductId == productId)
            .OrderBy(l => l.Code)
            .Select(l => new
            {
                id = l.Id,
                text = l.Code + " - " + l.Name
            })
            .ToListAsync();

        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetMachines(int lineId)
    {
        var data = await _db.Assets
            .AsNoTracking()
            .Where(a => a.LineId == lineId && a.IsActive)
            .OrderBy(a => a.Code)
            .Select(a => new
            {
                id = a.Id,
                text = a.Code + (a.OpNo != null && a.OpNo != "" ? "/" + a.OpNo : "")
            })
            .ToListAsync();

        return Json(data);
    }

    // =====================================================
    // VALIDASI PLANT -> PRODUCT -> LINE
    // =====================================================
    private async Task ValidateHierarchyAsync(
        AssetFormViewModel vm)
    {
        if (!vm.PlantId.HasValue)
        {
            ModelState.AddModelError(
                nameof(vm.PlantId),
                "Plant wajib dipilih.");

            return;
        }

        bool validPlant = await _db.Plants
            .AnyAsync(p => p.Id == vm.PlantId.Value);

        if (!validPlant)
        {
            ModelState.AddModelError(
                nameof(vm.PlantId),
                "Plant tidak ditemukan.");

            return;
        }

        if (!vm.ProductId.HasValue)
        {
            ModelState.AddModelError(
                nameof(vm.ProductId),
                "Product wajib dipilih.");

            return;
        }

        bool validProduct = await _db.Products
            .AnyAsync(p =>
                p.Id == vm.ProductId.Value &&
                p.PlantId == vm.PlantId.Value);

        if (!validProduct)
        {
            ModelState.AddModelError(
                nameof(vm.ProductId),
                "Product tidak sesuai dengan Plant.");

            return;
        }

        if (!vm.LineId.HasValue)
        {
            ModelState.AddModelError(
                nameof(vm.LineId),
                "Line wajib dipilih.");

            return;
        }

        bool validLine = await _db.Lines
            .AnyAsync(l =>
                l.Id == vm.LineId.Value &&
                l.ProductId == vm.ProductId.Value &&
                l.PlantId == vm.PlantId.Value);

        if (!validLine)
        {
            ModelState.AddModelError(
                nameof(vm.LineId),
                "Line tidak sesuai dengan Plant dan Product.");
        }
    }

    // =====================================================
    // VALIDASI KATEGORI MESIN
    // =====================================================
    private async Task ValidateCategoryAsync(
        AssetFormViewModel vm)
    {
        if (!vm.McCategoryId.HasValue)
        {
            ModelState.AddModelError(
                nameof(vm.McCategoryId),
                "Kategori mesin wajib dipilih.");

            return;
        }

        bool exists = await _db.McCategories
            .AnyAsync(c => c.Id == vm.McCategoryId.Value);

        if (!exists)
        {
            ModelState.AddModelError(
                nameof(vm.McCategoryId),
                "Kategori mesin tidak ditemukan.");
        }

        if (vm.MachineFunctionId.HasValue)
        {
            bool validFunction = await _db.MachineFunctions
                .AnyAsync(m =>
                    m.Id == vm.MachineFunctionId.Value);

            if (!validFunction)
            {
                ModelState.AddModelError(
                    nameof(vm.MachineFunctionId),
                    "Fungsi mesin tidak ditemukan.");
            }
        }

        if (vm.BrandId.HasValue)
        {
            bool validBrand = await _db.Brands
                .AnyAsync(b => b.Id == vm.BrandId.Value);

            if (!validBrand)
            {
                ModelState.AddModelError(
                    nameof(vm.BrandId),
                    "Brand tidak ditemukan.");
            }
        }
    }

    // =====================================================
    // POPULATE DROPDOWNS
    // =====================================================
    private async Task PopulateDropdownsAsync(
        AssetFormViewModel vm)
    {
        // Plant
        vm.PlantList = await _db.Plants
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Code + " - " + p.Name
            })
            .ToListAsync();

        // Product sesuai Plant
        vm.ProductList = new List<SelectListItem>();

        if (vm.PlantId.HasValue)
        {
            vm.ProductList = await _db.Products
                .AsNoTracking()
                .Where(p =>
                    p.PlantId == vm.PlantId.Value)
                .OrderBy(p => p.Code)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Code + " - " + p.Name
                })
                .ToListAsync();
        }

        // Line sesuai Product
        vm.LineList = new List<SelectListItem>();

        if (vm.ProductId.HasValue)
        {
            vm.LineList = await _db.Lines
                .AsNoTracking()
                .Where(l =>
                    l.ProductId == vm.ProductId.Value &&
                    l.PlantId == vm.PlantId)
                .OrderBy(l => l.Code)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Code + " - " + l.Name
                })
                .ToListAsync();
        }

        // Area hanya untuk kompatibilitas ViewModel lama.
        // Form baru tidak akan menampilkannya.
        vm.AreaList = new List<SelectListItem>();

        // Kategori Mesin
        vm.McCategoryList = await _db.McCategories
            .AsNoTracking()
            .OrderBy(m => m.Code)
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Code + " - " + m.Name
            })
            .ToListAsync();

        // Fungsi Mesin
        vm.MachineFunctionList = await _db.MachineFunctions
            .AsNoTracking()
            .OrderBy(m => m.Code)
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Code + " - " + m.Name
            })
            .ToListAsync();

        // Brand
        vm.BrandList = await _db.Brands
            .AsNoTracking()
            .OrderBy(b => b.Code)
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Code + " - " + b.Name
            })
            .ToListAsync();
    }

    // =====================================================
    // NORMALISASI INPUT
    // =====================================================
    private static void NormalizeForm(
        AssetFormViewModel vm)
    {
        vm.Code = vm.Code?.Trim() ?? "";
        vm.OpNo = string.IsNullOrWhiteSpace(vm.OpNo) ? null : vm.OpNo.Trim();

        vm.Model = vm.Model?.Trim();
        vm.SerialNumber = vm.SerialNumber?.Trim();
        vm.Capacity = vm.Capacity?.Trim();
        vm.Description = vm.Description?.Trim();
    }

    // =====================================================
    // SQL SERVER DUPLICATE KEY
    // =====================================================
    private static bool IsDuplicateKey(
        DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql &&
               (sql.Number == 2601 ||
                sql.Number == 2627);
    }
}
