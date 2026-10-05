using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class AreaController : Controller
{
    private readonly AppDbContext _db;

    public AreaController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Areas
            .Include(a => a.Line)
                .ThenInclude(l => l!.Plant)
            .OrderBy(a => a.Code)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new AreaFormViewModel
        {
            LineList = await GetLineListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AreaFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.LineList = await GetLineListAsync();
            return View(vm);
        }

        if (await _db.Areas.AnyAsync(a => a.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode area sudah digunakan");
            vm.LineList = await GetLineListAsync();
            return View(vm);
        }

        _db.Areas.Add(new Area
        {
            Code = vm.Code,
            Name = vm.Name,
            LineId = vm.LineId,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Area '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var area = await _db.Areas.FirstOrDefaultAsync(a => a.Id == id);
        if (area == null) return NotFound();

        return View(new AreaFormViewModel
        {
            Id = area.Id,
            Code = area.Code,
            Name = area.Name,
            LineId = area.LineId,
            Description = area.Description,
            LineList = await GetLineListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AreaFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.LineList = await GetLineListAsync();
            return View(vm);
        }

        var area = await _db.Areas.FirstOrDefaultAsync(a => a.Id == id);
        if (area == null) return NotFound();

        if (await _db.Areas.AnyAsync(a => a.Code == vm.Code && a.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode area sudah digunakan");
            vm.LineList = await GetLineListAsync();
            return View(vm);
        }

        area.Code = vm.Code;
        area.Name = vm.Name;
        area.LineId = vm.LineId;
        area.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Area '{area.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var area = await _db.Areas.FirstOrDefaultAsync(a => a.Id == id);
        if (area == null)
        {
            TempData["Error"] = "Area tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        area.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Area '{area.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetLineListAsync()
    {
        return await _db.Lines
            .Include(l => l.Plant)
            .OrderBy(l => l.Code)
            .Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = $"{l.Plant!.Code} → {l.Code} — {l.Name}"
            })
            .ToListAsync();
    }
}