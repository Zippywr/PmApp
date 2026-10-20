
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

[Authorize(Roles = "Admin,TS")]
public class ProductController : Controller
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    private async Task LoadPlants(ProductFormViewModel vm)
    {
        vm.PlantOptions = await _db.Plants
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Code + " - " + x.Name
            })
            .ToListAsync();
    }

    private async Task<bool> ValidPlant(int? plantId)
    {
        return plantId.HasValue &&
            await _db.Plants.AnyAsync(x => x.Id == plantId.Value);
    }

    private static bool IsDuplicate(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627);
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Products
            .AsNoTracking()
            .Include(x => x.Plant)
            .OrderBy(x => x.Code)
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ProductFormViewModel();
        await LoadPlants(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        vm.Code = vm.Code?.Trim() ?? "";
        vm.Name = vm.Name?.Trim() ?? "";

        if (!await ValidPlant(vm.PlantId))
            ModelState.AddModelError(nameof(vm.PlantId),
                "Pilih Plant yang valid.");

        if (await _db.Products.AnyAsync(x => x.Code == vm.Code))
            ModelState.AddModelError(nameof(vm.Code),
                "Kode produk sudah digunakan.");

        if (!ModelState.IsValid)
        {
            await LoadPlants(vm);
            return View(vm);
        }

        var product = new Product
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description,
            PlantId = vm.PlantId
        };

        _db.Products.Add(product);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        {
            ModelState.AddModelError(nameof(vm.Code),
                "Kode produk sudah digunakan.");

            await LoadPlants(vm);
            return View(vm);
        }

        TempData["Success"] =
            $"Produk '{product.Name}' berhasil ditambahkan.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null) return NotFound();

        var vm = new ProductFormViewModel
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            PlantId = product.PlantId
        };

        await LoadPlants(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, ProductFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        vm.Code = vm.Code?.Trim() ?? "";
        vm.Name = vm.Name?.Trim() ?? "";

        var product = await _db.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null) return NotFound();

        if (!await ValidPlant(vm.PlantId))
            ModelState.AddModelError(nameof(vm.PlantId),
                "Pilih Plant yang valid.");

        if (await _db.Products.AnyAsync(x =>
            x.Code == vm.Code && x.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code),
                "Kode produk sudah digunakan.");
        }

        // Jangan pindahkan Product ke Plant lain
        // jika Line aktifnya masih berada di Plant lama.
        bool conflictingLine = await _db.Lines.AnyAsync(x =>
            x.ProductId == id &&
            x.PlantId != vm.PlantId);

        if (conflictingLine)
        {
            ModelState.AddModelError(nameof(vm.PlantId),
                "Product masih digunakan Line pada Plant lain.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPlants(vm);
            return View(vm);
        }

        product.Code = vm.Code;
        product.Name = vm.Name;
        product.Description = vm.Description;
        product.PlantId = vm.PlantId;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        {
            ModelState.AddModelError(nameof(vm.Code),
                "Kode produk sudah digunakan.");

            await LoadPlants(vm);
            return View(vm);
        }

        TempData["Success"] =
            $"Produk '{product.Name}' berhasil diperbarui.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            TempData["Error"] = "Produk tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        // Jangan hapus Product yang masih digunakan.
        bool usedByLine = await _db.Lines
            .AnyAsync(x => x.ProductId == id);

        bool usedByMachine = await _db.Assets
            .AnyAsync(x => x.ProductId == id);

        if (usedByLine || usedByMachine)
        {
            TempData["Error"] =
                "Produk masih digunakan Line atau Mesin aktif. " +
                "Produk tidak dapat dihapus.";

            return RedirectToAction(nameof(Index));
        }

        product.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] =
            $"Produk '{product.Name}' berhasil dihapus.";

        return RedirectToAction(nameof(Index));
    }
}
