using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class SubUnitController : Controller
{
    private readonly AppDbContext _db;

    public SubUnitController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.SubUnits
            .Include(s => s.Unit)
            .OrderBy(s => s.Code)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new SubUnitFormViewModel { UnitList = await GetUnitListAsync() };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubUnitFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.UnitList = await GetUnitListAsync();
            return View(vm);
        }

        if (await _db.SubUnits.AnyAsync(s => s.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode sub unit sudah digunakan");
            vm.UnitList = await GetUnitListAsync();
            return View(vm);
        }

        _db.SubUnits.Add(new SubUnit
        {
            Code = vm.Code,
            Name = vm.Name,
            UnitId = vm.UnitId,
            Description = vm.Description
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
            Code = item.Code,
            Name = item.Name,
            UnitId = item.UnitId,
            Description = item.Description,
            UnitList = await GetUnitListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SubUnitFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.UnitList = await GetUnitListAsync();
            return View(vm);
        }

        var item = await _db.SubUnits.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();

        if (await _db.SubUnits.AnyAsync(s => s.Code == vm.Code && s.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode sub unit sudah digunakan");
            vm.UnitList = await GetUnitListAsync();
            return View(vm);
        }

        item.Code = vm.Code;
        item.Name = vm.Name;
        item.UnitId = vm.UnitId;
        item.Description = vm.Description;

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

        item.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Sub Unit berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetUnitListAsync()
    {
        return await _db.Units
            .OrderBy(u => u.Code)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Code + " - " + u.Name
            }).ToListAsync();
    }
}