using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class McCategoryController : Controller
{
    private readonly AppDbContext _db;

    public McCategoryController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.McCategories.OrderBy(m => m.Code).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new McCategoryFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(McCategoryFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.McCategories.AnyAsync(m => m.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode kategori sudah digunakan");
            return View(vm);
        }

        _db.McCategories.Add(new McCategory
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Kategori '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var mc = await _db.McCategories.FirstOrDefaultAsync(m => m.Id == id);
        if (mc == null) return NotFound();

        return View(new McCategoryFormViewModel
        {
            Id = mc.Id,
            Code = mc.Code,
            Name = mc.Name,
            Description = mc.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, McCategoryFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var mc = await _db.McCategories.FirstOrDefaultAsync(m => m.Id == id);
        if (mc == null) return NotFound();

        if (await _db.McCategories.AnyAsync(m => m.Code == vm.Code && m.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode kategori sudah digunakan");
            return View(vm);
        }

        mc.Code = vm.Code;
        mc.Name = vm.Name;
        mc.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Kategori '{mc.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var mc = await _db.McCategories.FirstOrDefaultAsync(m => m.Id == id);
        if (mc == null)
        {
            TempData["Error"] = "Kategori tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        mc.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Kategori '{mc.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}