using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class MetodeController : Controller
{
    private readonly AppDbContext _db;

    public MetodeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Metodes.OrderBy(m => m.Code).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new MetodeFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MetodeFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.Metodes.AnyAsync(m => m.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode metode sudah digunakan");
            return View(vm);
        }

        _db.Metodes.Add(new Metode
        {
            Code = vm.Code,
            Name = vm.Name,
            Initial = vm.Initial,
            MachineState = vm.MachineState,
            Description = vm.Description
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
            Code = item.Code,
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

        if (await _db.Metodes.AnyAsync(m => m.Code == vm.Code && m.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode metode sudah digunakan");
            return View(vm);
        }

        item.Code = vm.Code;
        item.Name = vm.Name;
        item.Initial = vm.Initial;
        item.MachineState = vm.MachineState;
        item.Description = vm.Description;

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

        item.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Metode berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}