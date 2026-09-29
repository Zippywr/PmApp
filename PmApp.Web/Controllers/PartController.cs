using Microsoft.AspNetCore.Mvc;
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

    // ============================================================
    // INDEX — Daftar Part + jumlah mesin yang pakai
    // ============================================================
    public async Task<IActionResult> Index()
    {
        var parts = await _db.Parts
            .Include(p => p.AssetParts)
                .ThenInclude(ap => ap.Asset)
            .OrderBy(p => p.PartNo)
            .ToListAsync();
        return View(parts);
    }

    // ============================================================
    // DETAILS — Lihat part ini dipakai di mesin mana saja
    // ============================================================
    public async Task<IActionResult> Details(int id)
    {
        var part = await _db.Parts
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

    // GET: Create
    [HttpGet]
    public IActionResult Create() => View(new PartFormViewModel());

    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.Parts.AnyAsync(p => p.PartNo == vm.PartNo))
        {
            ModelState.AddModelError(nameof(vm.PartNo), "Part No sudah digunakan");
            return View(vm);
        }

        _db.Parts.Add(new Part
        {
            PartNo = vm.PartNo,
            Name = vm.Name,
            Category = vm.Category,
            Brand = vm.Brand,
            Unit = vm.Unit,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Part '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var part = await _db.Parts.FirstOrDefaultAsync(p => p.Id == id);
        if (part == null) return NotFound();

        return View(new PartFormViewModel
        {
            Id = part.Id,
            PartNo = part.PartNo,
            Name = part.Name,
            Category = part.Category,
            Brand = part.Brand,
            Unit = part.Unit,
            Description = part.Description
        });
    }

    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PartFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var part = await _db.Parts.FirstOrDefaultAsync(p => p.Id == id);
        if (part == null) return NotFound();

        if (await _db.Parts.AnyAsync(p => p.PartNo == vm.PartNo && p.Id != id))
        {
            ModelState.AddModelError(nameof(vm.PartNo), "Part No sudah digunakan");
            return View(vm);
        }

        part.PartNo = vm.PartNo;
        part.Name = vm.Name;
        part.Category = vm.Category;
        part.Brand = vm.Brand;
        part.Unit = vm.Unit;
        part.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Part '{part.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Delete
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
            TempData["Error"] = $"Part '{part.Name}' tidak bisa dihapus karena masih terpasang di {part.AssetParts.Count} mesin.";
            return RedirectToAction(nameof(Index));
        }

        part.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Part '{part.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}