using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class PartController : Controller
{
    private readonly AppDbContext _db;

    public PartController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Parts
            .Include(p => p.PartCodeCategory)
            .Include(p => p.SubGrupCategory)
            .Include(p => p.Brand)
            .Include(p => p.AssetParts)
            .OrderBy(p => p.PartNo)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var part = await _db.Parts
            .Include(p => p.PartCodeCategory)
            .Include(p => p.SubGrupCategory)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (part == null) return NotFound();

        var usage = await _db.AssetParts
            .Include(ap => ap.Asset)
            .Where(ap => ap.PartId == id)
            .OrderBy(ap => ap.Asset!.Code)
            .ToListAsync();

        ViewBag.Part = part;
        return View(usage);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new PartFormViewModel();
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        if (await _db.Parts.AnyAsync(p => p.PartNo == vm.PartNo))
        {
            ModelState.AddModelError(nameof(vm.PartNo), "Part No sudah digunakan");
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var part = new Part
        {
            PartNo = vm.PartNo,
            Name = vm.Name,
            Type = vm.Type,
            Unit = vm.Unit,
            Description = vm.Description,
            PartCodeCategoryId = vm.PartCodeCategoryId,
            SubGrupCategoryId = vm.SubGrupCategoryId,
            BrandId = vm.BrandId,
            StockQty = vm.StockQty,
            MinQty = vm.MinQty,
            MaxQty = vm.MaxQty,
            Location = vm.Location,
            Price = vm.Price
        };

        _db.Parts.Add(part);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Part berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var part = await _db.Parts.FirstOrDefaultAsync(p => p.Id == id);
        if (part == null) return NotFound();

        var vm = new PartFormViewModel
        {
            Id = part.Id,
            PartNo = part.PartNo,
            Name = part.Name,
            Type = part.Type,
            Unit = part.Unit,
            Description = part.Description,
            PartCodeCategoryId = part.PartCodeCategoryId,
            SubGrupCategoryId = part.SubGrupCategoryId,
            BrandId = part.BrandId,
            StockQty = part.StockQty,
            MinQty = part.MinQty,
            MaxQty = part.MaxQty,
            Location = part.Location,
            Price = part.Price
        };

        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PartFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var part = await _db.Parts.FirstOrDefaultAsync(p => p.Id == id);
        if (part == null) return NotFound();

        if (await _db.Parts.AnyAsync(p => p.PartNo == vm.PartNo && p.Id != id))
        {
            ModelState.AddModelError(nameof(vm.PartNo), "Part No sudah digunakan");
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        part.PartNo = vm.PartNo;
        part.Name = vm.Name;
        part.Type = vm.Type;
        part.Unit = vm.Unit;
        part.Description = vm.Description;
        part.PartCodeCategoryId = vm.PartCodeCategoryId;
        part.SubGrupCategoryId = vm.SubGrupCategoryId;
        part.BrandId = vm.BrandId;
        part.StockQty = vm.StockQty;
        part.MinQty = vm.MinQty;
        part.MaxQty = vm.MaxQty;
        part.Location = vm.Location;
        part.Price = vm.Price;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Part berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var part = await _db.Parts
            .Include(p => p.AssetParts)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (part == null)
        {
            TempData["Error"] = "Part tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (part.AssetParts.Any())
        {
            TempData["Error"] = "Part tidak bisa dihapus, masih terpasang di mesin.";
            return RedirectToAction(nameof(Index));
        }

        part.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Part berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(PartFormViewModel vm)
    {
        vm.PartCodeCategoryList = await _db.PartCodeCategories
            .OrderBy(c => c.Code)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code + " - " + c.Name })
            .ToListAsync();

        vm.SubGrupCategoryList = await _db.SubGrupCategories
            .OrderBy(s => s.Code)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Code + " - " + s.Name })
            .ToListAsync();

        vm.BrandList = await _db.Brands
            .OrderBy(b => b.Code)
            .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Code + " - " + b.Name })
            .ToListAsync();
    }
}