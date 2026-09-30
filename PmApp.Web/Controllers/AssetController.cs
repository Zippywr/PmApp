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

        var usedPartIds = bom.Select(b => b.PartId).ToList();
        var availableParts = await _db.Parts
            .Where(p => !usedPartIds.Contains(p.Id))
            .OrderBy(p => p.PartNo)
            .ToListAsync();

        ViewBag.Asset = asset;
        ViewBag.AvailableParts = availableParts;
        return View(bom);
    }

    // ============================================================
    // CREATE — GET
    // ============================================================
    [HttpGet]
    public IActionResult Create()
    {
        return View(new AssetFormViewModel());
    }

    // ============================================================
    // CREATE — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var exists = await _db.Assets.AnyAsync(a => a.Code == vm.Code);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode mesin sudah digunakan");
            return View(vm);
        }

        var asset = new Asset
        {
            Code = vm.Code,
            Name = vm.Name,
            Line = vm.Line,
            Criticality = vm.Criticality,
            Manufacturer = vm.Manufacturer,
            Model = vm.Model,
            SerialNumber = vm.SerialNumber,
            YearMade = vm.YearMade,
            InstallDate = vm.InstallDate,
            Capacity = vm.Capacity,
            Location = vm.Location,
            Description = vm.Description,
            IsActive = vm.IsActive
        };

        _db.Assets.Add(asset);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Mesin '{asset.Name}' berhasil ditambahkan. Silakan tambahkan part di bawah ini.";
        return RedirectToAction(nameof(Details), new { id = asset.Id });
    }

    // ============================================================
    // EDIT — GET
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
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            SerialNumber = asset.SerialNumber,
            YearMade = asset.YearMade,
            InstallDate = asset.InstallDate,
            Capacity = asset.Capacity,
            Location = asset.Location,
            Description = asset.Description,
            IsActive = asset.IsActive
        };

        return View(vm);
    }

    // ============================================================
    // EDIT — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssetFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return NotFound();

        var exists = await _db.Assets.AnyAsync(a => a.Code == vm.Code && a.Id != id);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode mesin sudah digunakan");
            return View(vm);
        }

        asset.Code = vm.Code;
        asset.Name = vm.Name;
        asset.Line = vm.Line;
        asset.Criticality = vm.Criticality;
        asset.Manufacturer = vm.Manufacturer;
        asset.Model = vm.Model;
        asset.SerialNumber = vm.SerialNumber;
        asset.YearMade = vm.YearMade;
        asset.InstallDate = vm.InstallDate;
        asset.Capacity = vm.Capacity;
        asset.Location = vm.Location;
        asset.Description = vm.Description;
        asset.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Mesin '{asset.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Details), new { id = asset.Id });
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

        if (asset.AssetParts.Any())
        {
            TempData["Error"] = $"Mesin '{asset.Name}' tidak bisa dihapus karena masih punya {asset.AssetParts.Count} part terpasang. Hapus part dulu.";
            return RedirectToAction(nameof(Index));
        }

        asset.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Mesin '{asset.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // BOM — TAMBAH PART KE MESIN
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(AssetPartFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Data tidak valid. Periksa kembali input Anda.";
            return RedirectToAction(nameof(Details), new { id = vm.AssetId });
        }

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == vm.AssetId);
        if (asset == null)
        {
            TempData["Error"] = "Mesin tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var part = await _db.Parts.FirstOrDefaultAsync(p => p.Id == vm.PartId);
        if (part == null)
        {
            TempData["Error"] = "Part tidak ditemukan.";
            return RedirectToAction(nameof(Details), new { id = vm.AssetId });
        }

        var exists = await _db.AssetParts
            .AnyAsync(ap => ap.AssetId == vm.AssetId && ap.PartId == vm.PartId);

        if (exists)
        {
            TempData["Error"] = $"Part '{part.Name}' sudah terpasang di mesin ini. Silakan edit quantity-nya.";
            return RedirectToAction(nameof(Details), new { id = vm.AssetId });
        }

        _db.AssetParts.Add(new AssetPart
        {
            AssetId = vm.AssetId,
            PartId = vm.PartId,
            Quantity = vm.Quantity,
            Position = vm.Position,
            Notes = vm.Notes
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Part '{part.Name}' berhasil ditambahkan ke mesin {asset.Name}.";
        return RedirectToAction(nameof(Details), new { id = vm.AssetId });
    }

    // ============================================================
    // BOM — EDIT QTY / POSISI
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPart(AssetPartFormViewModel vm)
    {
        var assetPart = await _db.AssetParts
            .Include(ap => ap.Part)
            .FirstOrDefaultAsync(ap => ap.Id == vm.Id);

        if (assetPart == null)
        {
            TempData["Error"] = "Data BOM tidak ditemukan.";
            return RedirectToAction(nameof(Details), new { id = vm.AssetId });
        }

        if (vm.Quantity < 1)
        {
            TempData["Error"] = "Quantity minimal 1.";
            return RedirectToAction(nameof(Details), new { id = assetPart.AssetId });
        }

        assetPart.Quantity = vm.Quantity;
        assetPart.Position = vm.Position;
        assetPart.Notes = vm.Notes;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Part '{assetPart.Part?.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Details), new { id = assetPart.AssetId });
    }

    // ============================================================
    // BOM — HAPUS PART DARI MESIN
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePart(int id)
    {
        var assetPart = await _db.AssetParts
            .Include(ap => ap.Part)
            .FirstOrDefaultAsync(ap => ap.Id == id);

        if (assetPart == null)
        {
            TempData["Error"] = "Data BOM tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var assetId = assetPart.AssetId;
        var partName = assetPart.Part?.Name ?? "Part";

        assetPart.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Part '{partName}' berhasil dihapus dari mesin.";
        return RedirectToAction(nameof(Details), new { id = assetId });
    }
}