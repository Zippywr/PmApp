using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class AssetController : Controller
{
    private readonly AppDbContext _db;

    public AssetController(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // INDEX — Daftar Mesin
    // ============================================================
    public async Task<IActionResult> Index()
    {
        var assets = await _db.Assets
            .OrderBy(a => a.Code)
            .ToListAsync();
        return View(assets);
    }

    // ============================================================
    // DETAILS — Info Mesin + Part Terpasang
    // ============================================================
    public async Task<IActionResult> Details(int id)
    {
        var asset = await _db.Assets
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null) return NotFound();

        var bom = await _db.AssetParts
            .Include(ap => ap.Part)
            .Where(ap => ap.AssetId == id)
            .OrderBy(ap => ap.Part!.PartNo)
            .ToListAsync();

        ViewBag.Asset = asset;
        return View(bom);
    }

    // ============================================================
    // CREATE — GET (form)
    // ============================================================
    [HttpGet]
    public IActionResult Create()
    {
        return View(new AssetFormViewModel());
    }

    // ============================================================
    // CREATE — POST (simpan)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // Cek duplikat Code
        var exists = await _db.Assets.AnyAsync(a => a.Code == vm.Code);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode aset sudah digunakan");
            return View(vm);
        }

        var asset = new Asset
        {
            Code = vm.Code,
            Name = vm.Name,
            Line = vm.Line,
            Criticality = vm.Criticality,
            Description = vm.Description,
            Location = vm.Location,
            IsActive = vm.IsActive
        };

        _db.Assets.Add(asset);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Mesin '{asset.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // EDIT — GET (form)
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return NotFound();

        var vm = new AssetFormViewModel
        {
            Id = asset.Id,
            Code = asset.Code,
            Name = asset.Name,
            Line = asset.Line,
            Criticality = asset.Criticality,
            Description = asset.Description,
            Location = asset.Location,
            IsActive = asset.IsActive
        };

        return View(vm);
    }

    // ============================================================
    // EDIT — POST (simpan)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssetFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return NotFound();

        // Cek duplikat Code (selain dirinya sendiri)
        var exists = await _db.Assets.AnyAsync(a => a.Code == vm.Code && a.Id != id);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode aset sudah digunakan");
            return View(vm);
        }

        asset.Code = vm.Code;
        asset.Name = vm.Name;
        asset.Line = vm.Line;
        asset.Criticality = vm.Criticality;
        asset.Description = vm.Description;
        asset.Location = vm.Location;
        asset.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Mesin '{asset.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // DELETE — POST (soft delete)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var asset = await _db.Assets
            .Include(a => a.AssetParts)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
        {
            TempData["Error"] = "Mesin tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        // Cegah hapus kalau masih punya part terpasang
        if (asset.AssetParts.Any())
        {
            TempData["Error"] = $"Mesin '{asset.Name}' tidak bisa dihapus karena masih punya {asset.AssetParts.Count} part terpasang. Hapus part dulu.";
            return RedirectToAction(nameof(Index));
        }

        // Soft delete
        asset.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Mesin '{asset.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}