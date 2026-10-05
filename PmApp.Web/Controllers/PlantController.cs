using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class PlantController : Controller
{
    private readonly AppDbContext _db;

    public PlantController(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // INDEX
    // ============================================================
    public async Task<IActionResult> Index()
    {
        var list = await _db.Plants
            .OrderBy(p => p.Code)
            .ToListAsync();
        return View(list);
    }

    // ============================================================
    // CREATE — GET
    // ============================================================
    [HttpGet]
    public IActionResult Create() => View(new PlantFormViewModel());

    // ============================================================
    // CREATE — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlantFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.Plants.AnyAsync(p => p.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode plant sudah digunakan");
            return View(vm);
        }

        _db.Plants.Add(new Plant
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Plant '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // EDIT — GET
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var plant = await _db.Plants.FirstOrDefaultAsync(p => p.Id == id);
        if (plant == null) return NotFound();

        return View(new PlantFormViewModel
        {
            Id = plant.Id,
            Code = plant.Code,
            Name = plant.Name,
            Description = plant.Description
        });
    }

    // ============================================================
    // EDIT — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlantFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var plant = await _db.Plants.FirstOrDefaultAsync(p => p.Id == id);
        if (plant == null) return NotFound();

        if (await _db.Plants.AnyAsync(p => p.Code == vm.Code && p.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode plant sudah digunakan");
            return View(vm);
        }

        plant.Code = vm.Code;
        plant.Name = vm.Name;
        plant.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Plant '{plant.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // DELETE — POST
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var plant = await _db.Plants
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plant == null)
        {
            TempData["Error"] = "Plant tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (plant.Lines.Any())
        {
            TempData["Error"] = $"Plant '{plant.Name}' tidak bisa dihapus karena masih punya {plant.Lines.Count} line.";
            return RedirectToAction(nameof(Index));
        }

        plant.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Plant '{plant.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}