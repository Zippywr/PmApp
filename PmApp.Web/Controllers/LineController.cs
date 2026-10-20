
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
public class LineController : Controller
{
    private readonly AppDbContext _db;

    public LineController(AppDbContext db)
    {
        _db = db;
    }

    // ========================================
    // INDEX
    // ========================================

    public async Task<IActionResult> Index()
    {
        var lines = await _db.Lines
            .AsNoTracking()
            .Include(x => x.Plant)
            .Include(x => x.Product)
            .OrderBy(x => x.Code)
            .ToListAsync();

        return View(lines);
    }

    // ========================================
    // CREATE - GET
    // ========================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new LineFormViewModel();

        await LoadDropdownsAsync(vm);

        return View(vm);
    }

    // ========================================
    // CREATE - POST
    // ========================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        LineFormViewModel vm)
    {
        vm.Code = vm.Code?.Trim() ?? "";
        vm.Name = vm.Name?.Trim() ?? "";
        vm.Description = vm.Description?.Trim();

        await ValidateHierarchyAsync(vm);

        if (await _db.Lines.AnyAsync(x =>
            x.Code == vm.Code))
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode Line sudah digunakan.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(vm);
            return View(vm);
        }

        var line = new Line
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description,

            // Relasi lama masih dipertahankan
            PlantId = vm.PlantId!.Value,

            // Relasi baru
            ProductId = vm.ProductId
        };

        _db.Lines.Add(line);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode Line sudah digunakan.");

            await LoadDropdownsAsync(vm);
            return View(vm);
        }

        TempData["Success"] =
            $"Line '{line.Name}' berhasil ditambahkan.";

        return RedirectToAction(nameof(Index));
    }

    // ========================================
    // EDIT - GET
    // ========================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var line = await _db.Lines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return NotFound();

        var vm = new LineFormViewModel
        {
            Id = line.Id,
            Code = line.Code,
            Name = line.Name,
            Description = line.Description,
            PlantId = line.PlantId,
            ProductId = line.ProductId
        };

        await LoadDropdownsAsync(vm);

        return View(vm);
    }

    // ========================================
    // EDIT - POST
    // ========================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        LineFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        vm.Code = vm.Code?.Trim() ?? "";
        vm.Name = vm.Name?.Trim() ?? "";
        vm.Description = vm.Description?.Trim();

        var line = await _db.Lines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return NotFound();

        await ValidateHierarchyAsync(vm);

        if (await _db.Lines.AnyAsync(x =>
            x.Code == vm.Code &&
            x.Id != id))
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode Line sudah digunakan.");
        }

        // Mesin masih memiliki referensi langsung
        // ke Plant/Product pada struktur transisi.
        // Perpindahan hierarki harus melalui proses
        // migrasi tersendiri.
        bool hierarchyChanged =
            line.PlantId != vm.PlantId ||
            line.ProductId != vm.ProductId;

        if (hierarchyChanged)
        {
            bool hasMachines = await _db.Assets
                .AnyAsync(x => x.LineId == id);

            if (hasMachines)
            {
                ModelState.AddModelError(
                    nameof(vm.ProductId),
                    "Plant atau Product tidak bisa " +
                    "dipindahkan karena Line masih " +
                    "digunakan oleh Mesin aktif.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(vm);
            return View(vm);
        }

        line.Code = vm.Code;
        line.Name = vm.Name;
        line.Description = vm.Description;
        line.PlantId = vm.PlantId!.Value;
        line.ProductId = vm.ProductId;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        {
            ModelState.AddModelError(
                nameof(vm.Code),
                "Kode Line sudah digunakan.");

            await LoadDropdownsAsync(vm);
            return View(vm);
        }

        TempData["Success"] =
            $"Line '{line.Name}' berhasil diperbarui.";

        return RedirectToAction(nameof(Index));
    }

    // ========================================
    // DELETE - SOFT DELETE
    // ========================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var line = await _db.Lines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
        {
            TempData["Error"] =
                "Line tidak ditemukan.";

            return RedirectToAction(nameof(Index));
        }

        bool hasMachines = await _db.Assets
            .AnyAsync(x => x.LineId == id);

        // Area masih diperiksa selama masa transisi.
        bool hasAreas = await _db.Areas
            .AnyAsync(x => x.LineId == id);

        if (hasMachines || hasAreas)
        {
            TempData["Error"] =
                "Line tidak dapat dihapus karena " +
                "masih digunakan Mesin atau Area.";

            return RedirectToAction(nameof(Index));
        }

        line.IsDeleted = true;

        await _db.SaveChangesAsync();

        TempData["Success"] =
            $"Line '{line.Name}' berhasil dihapus.";

        return RedirectToAction(nameof(Index));
    }

    // ========================================
    // AJAX - PRODUCT BERDASARKAN PLANT
    // ========================================

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        int plantId)
    {
        if (plantId <= 0)
            return Json(Array.Empty<object>());

        var products = await _db.Products
            .AsNoTracking()
            .Where(x => x.PlantId == plantId)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                id = x.Id,
                code = x.Code,
                name = x.Name
            })
            .ToListAsync();

        return Json(products);
    }

    // ========================================
    // VALIDASI HIERARKI
    // ========================================

    private async Task ValidateHierarchyAsync(
        LineFormViewModel vm)
    {
        if (!vm.PlantId.HasValue ||
            vm.PlantId.Value <= 0)
        {
            ModelState.AddModelError(
                nameof(vm.PlantId),
                "Pilih Plant yang valid.");

            return;
        }

        bool validPlant = await _db.Plants
            .AnyAsync(x => x.Id == vm.PlantId.Value);

        if (!validPlant)
        {
            ModelState.AddModelError(
                nameof(vm.PlantId),
                "Plant tidak ditemukan.");

            return;
        }

        if (!vm.ProductId.HasValue ||
            vm.ProductId.Value <= 0)
        {
            ModelState.AddModelError(
                nameof(vm.ProductId),
                "Pilih Product yang valid.");

            return;
        }

        bool validProduct = await _db.Products
            .AnyAsync(x =>
                x.Id == vm.ProductId.Value &&
                x.PlantId == vm.PlantId.Value);

        if (!validProduct)
        {
            ModelState.AddModelError(
                nameof(vm.ProductId),
                "Product tidak sesuai dengan Plant.");
        }
    }

    // ========================================
    // DROPDOWN CREATE / EDIT
    // ========================================

    private async Task LoadDropdownsAsync(
        LineFormViewModel vm)
    {
        vm.PlantList = await _db.Plants
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Code + " - " + p.Name
            })
            .ToListAsync();

        vm.ProductList = new List<SelectListItem>();

        if (vm.PlantId.HasValue)
        {
            vm.ProductList = await _db.Products
                .AsNoTracking()
                .Where(x =>
                    x.PlantId == vm.PlantId.Value)
                .OrderBy(x => x.Code)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Code + " - " + x.Name
                })
                .ToListAsync();
        }
    }

    // ========================================
    // SQL UNIQUE CONSTRAINT
    // ========================================

    private static bool IsDuplicate(
        DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql &&
               (sql.Number == 2601 ||
                sql.Number == 2627);
    }
}
