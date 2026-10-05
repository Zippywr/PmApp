using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public async Task<IActionResult> Index()
    {
        var list = await _db.Assets
            .Include(a => a.Line)
            .Include(a => a.Area)
            .Include(a => a.McCategory)
            .Include(a => a.MachineFunction)
            .Include(a => a.Brand)
            .Include(a => a.Product)
            .OrderBy(a => a.Code)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var asset = await _db.Assets
            .Include(a => a.Line)
            .Include(a => a.Area)
            .Include(a => a.McCategory)
            .Include(a => a.MachineFunction)
            .Include(a => a.Brand)
            .Include(a => a.Product)
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

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new AssetFormViewModel();
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        if (await _db.Assets.AnyAsync(a => a.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode mesin sudah digunakan");
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var asset = new Asset
        {
            Code = vm.Code,
            Name = vm.Name,
            OpNo = vm.OpNo,
            LineId = vm.LineId,
            AreaId = vm.AreaId,
            McCategoryId = vm.McCategoryId,
            MachineFunctionId = vm.MachineFunctionId,
            BrandId = vm.BrandId,
            ProductId = vm.ProductId,
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
        await _db.SaveChangesAsync();

        TempData["Success"] = "Mesin berhasil ditambahkan.";
        return RedirectToAction(nameof(Details), new { id = asset.Id });
    }

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
            OpNo = asset.OpNo,
            LineId = asset.LineId,
            AreaId = asset.AreaId,
            McCategoryId = asset.McCategoryId,
            MachineFunctionId = asset.MachineFunctionId,
            BrandId = asset.BrandId,
            ProductId = asset.ProductId,
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssetFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return NotFound();

        if (await _db.Assets.AnyAsync(a => a.Code == vm.Code && a.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode mesin sudah digunakan");
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        asset.Code = vm.Code;
        asset.Name = vm.Name;
        asset.OpNo = vm.OpNo;
        asset.LineId = vm.LineId;
        asset.AreaId = vm.AreaId;
        asset.McCategoryId = vm.McCategoryId;
        asset.MachineFunctionId = vm.MachineFunctionId;
        asset.BrandId = vm.BrandId;
        asset.ProductId = vm.ProductId;
        asset.Model = vm.Model;
        asset.SerialNumber = vm.SerialNumber;
        asset.YearMade = vm.YearMade;
        asset.Capacity = vm.Capacity;
        asset.InstallDate = vm.InstallDate;
        asset.Description = vm.Description;
        asset.Criticality = vm.Criticality;
        asset.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Mesin berhasil diperbarui.";
        return RedirectToAction(nameof(Details), new { id = asset.Id });
    }

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
            TempData["Error"] = "Mesin tidak bisa dihapus, masih punya part terpasang.";
            return RedirectToAction(nameof(Index));
        }

        asset.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Mesin berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(int AssetId, int PartId, int Quantity, string? Position, string? Notes)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == AssetId);
        if (asset == null)
        {
            TempData["Error"] = "Mesin tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var part = await _db.Parts.FirstOrDefaultAsync(p => p.Id == PartId);
        if (part == null)
        {
            TempData["Error"] = "Part tidak ditemukan.";
            return RedirectToAction(nameof(Details), new { id = AssetId });
        }

        var exists = await _db.AssetParts
            .AnyAsync(ap => ap.AssetId == AssetId && ap.PartId == PartId);

        if (exists)
        {
            TempData["Error"] = "Part sudah terpasang di mesin ini.";
            return RedirectToAction(nameof(Details), new { id = AssetId });
        }

        _db.AssetParts.Add(new AssetPart
        {
            AssetId = AssetId,
            PartId = PartId,
            Quantity = Quantity < 1 ? 1 : Quantity,
            Position = Position,
            Notes = Notes
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Part berhasil ditambahkan.";
        return RedirectToAction(nameof(Details), new { id = AssetId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePart(int id)
    {
        var assetPart = await _db.AssetParts
            .FirstOrDefaultAsync(ap => ap.Id == id);

        if (assetPart == null)
        {
            TempData["Error"] = "Data BOM tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var assetId = assetPart.AssetId;

        assetPart.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Part berhasil dihapus dari mesin.";
        return RedirectToAction(nameof(Details), new { id = assetId });
    }

    private async Task PopulateDropdownsAsync(AssetFormViewModel vm)
    {
        vm.LineList = await _db.Lines
            .OrderBy(l => l.Code)
            .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Code + " - " + l.Name })
            .ToListAsync();

        vm.AreaList = await _db.Areas
            .OrderBy(a => a.Code)
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Code + " - " + a.Name })
            .ToListAsync();

        vm.McCategoryList = await _db.McCategories
            .OrderBy(m => m.Code)
            .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Code + " - " + m.Name })
            .ToListAsync();

        vm.MachineFunctionList = await _db.MachineFunctions
            .OrderBy(m => m.Code)
            .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Code + " - " + m.Name })
            .ToListAsync();

        vm.BrandList = await _db.Brands
            .OrderBy(b => b.Code)
            .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Code + " - " + b.Name })
            .ToListAsync();

        vm.ProductList = await _db.Products
            .OrderBy(p => p.Code)
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Code + " - " + p.Name })
            .ToListAsync();
    }
}