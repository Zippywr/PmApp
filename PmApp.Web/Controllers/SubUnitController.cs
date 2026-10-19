using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class SubUnitController : Controller
{
    private readonly AppDbContext _db;

    public SubUnitController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.SubUnits
            .Include(s => s.Unit)
            .OrderBy(s => s.Unit!.Name)
            .ThenBy(s => s.Name)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
        => View(new SubUnitFormViewModel { UnitList = await GetUnitListAsync() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubUnitFormViewModel vm)
    {
        await ValidateInputAsync(vm);
        if (!ModelState.IsValid)
        {
            vm.UnitList = await GetUnitListAsync();
            return View(vm);
        }

        _db.SubUnits.Add(new SubUnit
        {
            // Sementara: database lama masih mensyaratkan Code unik, NOT NULL.
            Code = "AUTO-" + Guid.NewGuid().ToString("N"),
            Name = vm.Name.Trim(),
            UnitId = vm.UnitId,
            Description = vm.Description?.Trim()
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub Unit berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.SubUnits.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();

        return View(new SubUnitFormViewModel
        {
            Id = item.Id,
            Name = item.Name,
            UnitId = item.UnitId,
            Description = item.Description,
            UnitList = await GetUnitListAsync(item.UnitId)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SubUnitFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var item = await _db.SubUnits.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();

        await ValidateInputAsync(vm, id);
        if (!ModelState.IsValid)
        {
            vm.UnitList = await GetUnitListAsync(item.UnitId);
            return View(vm);
        }

        // Code lama tetap: jangan ubah identitas internal saat Edit.
        item.Name = vm.Name.Trim();
        item.UnitId = vm.UnitId;
        item.Description = vm.Description?.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub Unit berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.SubUnits.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null)
        {
            TempData["Error"] = "Sub Unit tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        // Jangan hapus Sub Unit yang sudah direferensikan Task PM.
        if (await _db.PmTaskTemplates.IgnoreQueryFilters()
            .AnyAsync(t => t.SubUnitId == id))
        {
            TempData["Error"] = "Sub Unit tidak dapat dihapus karena sudah digunakan oleh Task PM.";
            return RedirectToAction(nameof(Index));
        }

        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub Unit berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateInputAsync(SubUnitFormViewModel vm, int? excludeId = null)
    {
        if (vm.UnitId <= 0 ||
            !await _db.Units.AnyAsync(u => u.Id == vm.UnitId))
        {
            ModelState.AddModelError(nameof(vm.UnitId), "Pilih Unit induk yang valid.");
        }

        var name = vm.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        // Satu Unit tidak boleh memiliki dua Sub Unit dengan nama sama.
        // Nama yang sama di Unit berbeda diperbolehkan.
        // Termasuk data soft deleted agar tidak ambigu di masa depan.
        var duplicate = await _db.SubUnits.IgnoreQueryFilters().AnyAsync(s =>
            s.UnitId == vm.UnitId &&
            s.Name.ToLower() == name.ToLower() &&
            (!excludeId.HasValue || s.Id != excludeId.Value));

        if (duplicate)
        {
            ModelState.AddModelError(nameof(vm.Name),
                "Nama Sub Unit sudah pernah digunakan pada Unit ini.");
        }
    }

    private async Task<List<SelectListItem>> GetUnitListAsync(int? currentUnitId = null)
    {
        // Unit saat ini tetap ditampilkan ketika Edit, bahkan bila nonaktif/soft deleted.
        return await _db.Units.IgnoreQueryFilters()
            .Where(u => !u.IsDeleted || (currentUnitId.HasValue && u.Id == currentUnitId.Value))
            .OrderBy(u => u.Name)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Name
            })
            .ToListAsync();
    }
}
