using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class MachineFunctionController : Controller
{
    private readonly AppDbContext _db;

    public MachineFunctionController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.MachineFunctions.OrderBy(m => m.Code).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new MachineFunctionFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MachineFunctionFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.MachineFunctions.AnyAsync(m => m.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode fungsi sudah digunakan");
            return View(vm);
        }

        _db.MachineFunctions.Add(new MachineFunction
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Fungsi '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var mf = await _db.MachineFunctions.FirstOrDefaultAsync(m => m.Id == id);
        if (mf == null) return NotFound();

        return View(new MachineFunctionFormViewModel
        {
            Id = mf.Id,
            Code = mf.Code,
            Name = mf.Name,
            Description = mf.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MachineFunctionFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var mf = await _db.MachineFunctions.FirstOrDefaultAsync(m => m.Id == id);
        if (mf == null) return NotFound();

        if (await _db.MachineFunctions.AnyAsync(m => m.Code == vm.Code && m.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode fungsi sudah digunakan");
            return View(vm);
        }

        mf.Code = vm.Code;
        mf.Name = vm.Name;
        mf.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Fungsi '{mf.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var mf = await _db.MachineFunctions.FirstOrDefaultAsync(m => m.Id == id);
        if (mf == null)
        {
            TempData["Error"] = "Fungsi tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        mf.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Fungsi '{mf.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}