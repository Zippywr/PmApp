using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

[Authorize(Roles = "Admin,TS")]
public class UnitController : Controller
{
    private readonly AppDbContext _db;

    public UnitController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Units.OrderBy(u => u.Name).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new UnitFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var name = vm.Name.Trim();
        if (await _db.Units.AnyAsync(u => u.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError(nameof(vm.Name), "Nama Unit sudah digunakan.");
            return View(vm);
        }

        _db.Units.Add(new Unit
        {
            // Sementara tetap diisi karena kolom Code NOT NULL + unique index.
            // Tidak ditampilkan/diinput pengguna.
            Code = "AUTO-" + Guid.NewGuid().ToString("N"),
            Name = name,
            Description = vm.Description?.Trim()
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Unit berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (item == null) return NotFound();

        return View(new UnitFormViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UnitFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var item = await _db.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (item == null) return NotFound();

        var name = vm.Name.Trim();
        if (await _db.Units.AnyAsync(u => u.Id != id && u.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError(nameof(vm.Name), "Nama Unit sudah digunakan.");
            return View(vm);
        }

        // Code lama tidak diubah saat Edit.
        item.Name = name;
        item.Description = vm.Description?.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = "Unit berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Units
            .Include(u => u.SubUnits)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (item == null)
        {
            TempData["Error"] = "Unit tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (item.SubUnits.Any())
        {
            TempData["Error"] = "Unit tidak bisa dihapus, masih punya sub unit.";
            return RedirectToAction(nameof(Index));
        }

        item.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Unit berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}