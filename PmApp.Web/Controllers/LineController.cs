using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class LineController : Controller
{
    private readonly AppDbContext _db;

    public LineController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Lines
            .Include(l => l.Plant)
            .OrderBy(l => l.Code)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new LineFormViewModel
        {
            PlantList = await GetPlantListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LineFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.PlantList = await GetPlantListAsync();
            return View(vm);
        }

        if (await _db.Lines.AnyAsync(l => l.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode line sudah digunakan");
            vm.PlantList = await GetPlantListAsync();
            return View(vm);
        }

        _db.Lines.Add(new Line
        {
            Code = vm.Code,
            Name = vm.Name,
            PlantId = vm.PlantId,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Line '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var line = await _db.Lines.FirstOrDefaultAsync(l => l.Id == id);
        if (line == null) return NotFound();

        return View(new LineFormViewModel
        {
            Id = line.Id,
            Code = line.Code,
            Name = line.Name,
            PlantId = line.PlantId,
            Description = line.Description,
            PlantList = await GetPlantListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LineFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.PlantList = await GetPlantListAsync();
            return View(vm);
        }

        var line = await _db.Lines.FirstOrDefaultAsync(l => l.Id == id);
        if (line == null) return NotFound();

        if (await _db.Lines.AnyAsync(l => l.Code == vm.Code && l.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode line sudah digunakan");
            vm.PlantList = await GetPlantListAsync();
            return View(vm);
        }

        line.Code = vm.Code;
        line.Name = vm.Name;
        line.PlantId = vm.PlantId;
        line.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Line '{line.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var line = await _db.Lines
            .Include(l => l.Areas)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (line == null)
        {
            TempData["Error"] = "Line tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (line.Areas.Any())
        {
            TempData["Error"] = $"Line '{line.Name}' tidak bisa dihapus karena masih punya {line.Areas.Count} area.";
            return RedirectToAction(nameof(Index));
        }

        line.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Line '{line.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetPlantListAsync()
    {
        return await _db.Plants
            .OrderBy(p => p.Code)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Code} — {p.Name}"
            })
            .ToListAsync();
    }
}