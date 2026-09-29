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
    // ADD PART (BOM) — GET (form)
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> AddPart(int assetId)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
        if (asset == null) return NotFound();

        var vm = new BomFormViewModel
        {
            AssetId = assetId,
            AssetName = asset.Name,
            AssetCode = asset.Code,
            Mode = "existing",
            PartList = await GetAvailablePartsAsync(assetId)
        };

        return View(vm);
    }

    // ============================================================
    // ADD PART (BOM) — POST (simpan)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(BomFormViewModel vm)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == vm.AssetId);
        if (asset == null) return NotFound();

        int? partIdToUse = null;

        // --------------------------------------------------------
        // MODE: EXISTING — pakai part dari master
        // --------------------------------------------------------
        if (vm.Mode == "existing")
        {
            if (!vm.PartId.HasValue || vm.PartId.Value <= 0)
            {
                ModelState.AddModelError(nameof(vm.PartId), "Part wajib dipilih");
            }
            else
            {
                partIdToUse = vm.PartId.Value;
            }
        }
        // --------------------------------------------------------
        // MODE: NEW — buat part baru di master
        // --------------------------------------------------------
        else if (vm.Mode == "new")
        {
            if (string.IsNullOrWhiteSpace(vm.NewPartNo))
                ModelState.AddModelError(nameof(vm.NewPartNo), "Part No wajib diisi");

            if (string.IsNullOrWhiteSpace(vm.NewPartName))
                ModelState.AddModelError(nameof(vm.NewPartName), "Nama part wajib diisi");

            if (!string.IsNullOrWhiteSpace(vm.NewPartNo))
            {
                var dup = await _db.Parts.AnyAsync(p => p.PartNo == vm.NewPartNo);
                if (dup)
                    ModelState.AddModelError(nameof(vm.NewPartNo), "Part No sudah digunakan");
            }

            if (ModelState.IsValid)
            {
                var newPart = new Part
                {
                    PartNo = vm.NewPartNo!.Trim(),
                    Name = vm.NewPartName!.Trim(),
                    Category = vm.NewCategory?.Trim(),
                    Brand = vm.NewBrand?.Trim(),
                    Unit = string.IsNullOrWhiteSpace(vm.NewUnit) ? "PCS" : vm.NewUnit.Trim(),
                    Description = vm.NewDescription?.Trim()
                };

                _db.Parts.Add(newPart);
                await _db.SaveChangesAsync();

                partIdToUse = newPart.Id;
            }
        }

        // --------------------------------------------------------
        // Kalau ada error validasi → reload form
        // --------------------------------------------------------
        if (!ModelState.IsValid || partIdToUse == null)
        {
            vm.AssetName = asset.Name;
            vm.AssetCode = asset.Code;
            vm.PartList = await GetAvailablePartsAsync(vm.AssetId);
            return View(vm);
        }

        // --------------------------------------------------------
        // Cek duplikat
        // --------------------------------------------------------
        var exists = await _db.AssetParts
            .AnyAsync(ap => ap.AssetId == vm.AssetId && ap.PartId == partIdToUse.Value);

        if (exists)
        {
            TempData["Error"] = "Part ini sudah terpasang di mesin tersebut.";
            return RedirectToAction(nameof(Details), new { id = vm.AssetId });
        }

        // --------------------------------------------------------
        // Simpan BOM
        // --------------------------------------------------------
        _db.AssetParts.Add(new AssetPart
        {
            AssetId = vm.AssetId,
            PartId = partIdToUse.Value,
            Quantity = vm.Quantity,
            Position = vm.Position,
            Notes = vm.Notes
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Part berhasil ditambahkan ke mesin.";
        return RedirectToAction(nameof(Details), new { id = vm.AssetId });
    }

    // ============================================================
    // REMOVE PART (BOM) — POST (hapus part dari mesin)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePart(int id)
    {
        var bom = await _db.AssetParts.FirstOrDefaultAsync(ap => ap.Id == id);
        if (bom == null)
        {
            TempData["Error"] = "Data BOM tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var assetId = bom.AssetId;
        bom.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Part berhasil dihapus dari mesin.";
        return RedirectToAction(nameof(Details), new { id = assetId });
    }

    // ============================================================
    // HELPER — Ambil part yang belum terpasang di mesin
    // ============================================================
    private async Task<List<SelectListItem>> GetAvailablePartsAsync(int assetId)
    {
        var usedPartIds = await _db.AssetParts
            .Where(ap => ap.AssetId == assetId)
            .Select(ap => ap.PartId)
            .ToListAsync();

        var available = await _db.Parts
            .Where(p => !usedPartIds.Contains(p.Id))
            .OrderBy(p => p.PartNo)
            .ToListAsync();

        return available.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.PartNo} — {p.Name} ({p.Brand})"
        }).ToList();
    }
}