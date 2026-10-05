using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class PartCodeCategoryController : Controller
{
    private readonly AppDbContext _db;

    public PartCodeCategoryController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.PartCodeCategories.OrderBy(c => c.Code).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new PartCodeCategoryFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartCodeCategoryFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.PartCodeCategories.AnyAsync(c => c.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode sudah digunakan");
            return View(vm);
        }

        _db.PartCodeCategories.Add(new PartCodeCategory
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Kategori berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.PartCodeCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return NotFound();

        return View(new PartCodeCategoryFormViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PartCodeCategoryFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var item = await _db.PartCodeCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return NotFound();

        if (await _db.PartCodeCategories.AnyAsync(c => c.Code == vm.Code && c.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode sudah digunakan");
            return View(vm);
        }

        item.Code = vm.Code;
        item.Name = vm.Name;
        item.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Kategori berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.PartCodeCategories
            .Include(c => c.SubCategories)
            .Include(c => c.Parts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (item == null)
        {
            TempData["Error"] = "Kategori tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (item.SubCategories.Any() || item.Parts.Any())
        {
            TempData["Error"] = "Kategori tidak bisa dihapus, masih punya sub-kategori atau part.";
            return RedirectToAction(nameof(Index));
        }

        item.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Kategori berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}