using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;


namespace PmApp.Web.Controllers;

public class BrandController : Controller
{
    private readonly AppDbContext _db;

    public BrandController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Brands.OrderBy(b => b.Code).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new BrandFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BrandFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.Brands.AnyAsync(b => b.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode brand sudah digunakan");
            return View(vm);
        }

        _db.Brands.Add(new Brand
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Brand '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand == null) return NotFound();

        return View(new BrandFormViewModel
        {
            Id = brand.Id,
            Code = brand.Code,
            Name = brand.Name,
            Description = brand.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BrandFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand == null) return NotFound();

        if (await _db.Brands.AnyAsync(b => b.Code == vm.Code && b.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode brand sudah digunakan");
            return View(vm);
        }

        brand.Code = vm.Code;
        brand.Name = vm.Name;
        brand.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Brand '{brand.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand == null)
        {
            TempData["Error"] = "Brand tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        brand.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Brand '{brand.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}