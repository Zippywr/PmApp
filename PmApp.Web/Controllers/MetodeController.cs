using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

[Authorize(Roles = "Admin,TS")]
public class MetodeController : Controller
{
    private readonly AppDbContext _db;

    public MetodeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Metodes.OrderBy(m => m.Name).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new MetodeFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MetodeFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        string name = vm.Name.Trim();
        if (await _db.Metodes.AnyAsync(m => m.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError(nameof(vm.Name), "Nama Metode sudah digunakan.");
            return View(vm);
        }

        _db.Metodes.Add(new Metode
        {
            // Kompatibilitas database lama: Code wajib dan unik, tetapi bukan input user.
            Code = "AUTO-" + Guid.NewGuid().ToString("N"),
            Name = name,
            Initial = string.IsNullOrWhiteSpace(vm.Initial) ? null : vm.Initial.Trim(),
            MachineState = vm.MachineState,
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim()
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Metode berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Metodes.FirstOrDefaultAsync(m => m.Id == id);
        if (item == null) return NotFound();

        return View(new MetodeFormViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Initial = item.Initial,
            MachineState = item.MachineState,
            Description = item.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MetodeFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var item = await _db.Metodes.FirstOrDefaultAsync(m => m.Id == id);
        if (item == null) return NotFound();

        string name = vm.Name.Trim();
        if (await _db.Metodes.AnyAsync(m => m.Id != id && m.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError(nameof(vm.Name), "Nama Metode sudah digunakan.");
            return View(vm);
        }

        // Code lama tetap: tidak diubah saat Edit.
        item.Name = name;
        item.Initial = string.IsNullOrWhiteSpace(vm.Initial) ? null : vm.Initial.Trim();
        item.MachineState = vm.MachineState;
        item.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = "Metode berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Metodes.FirstOrDefaultAsync(m => m.Id == id);
        if (item == null)
        {
            TempData["Error"] = "Metode tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        // Metode yang dipakai Task PM tidak boleh dihapus.
        if (await _db.PmTaskTemplates.IgnoreQueryFilters().AnyAsync(t => t.MethodId == id))
        {
            TempData["Error"] = "Metode tidak bisa dihapus karena sudah digunakan pada Task PM.";
            return RedirectToAction(nameof(Index));
        }

        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Metode berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}
