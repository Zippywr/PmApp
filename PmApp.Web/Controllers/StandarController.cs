using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class StandarController : Controller
{
    private readonly AppDbContext _db;

    public StandarController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Standars.OrderBy(s => s.Name).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new StandarFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StandarFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        _db.Standars.Add(new Standar
        {
            Name = vm.Name,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Standar berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Standars.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();

        return View(new StandarFormViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StandarFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var item = await _db.Standars.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();

        item.Name = vm.Name;
        item.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Standar berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Standars.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null)
        {
            TempData["Error"] = "Standar tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        item.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Standar berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}