using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Products.OrderBy(p => p.Code).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new ProductFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.Products.AnyAsync(p => p.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode produk sudah digunakan");
            return View(vm);
        }

        _db.Products.Add(new Product
        {
            Code = vm.Code,
            Name = vm.Name,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Produk '{vm.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        if (await _db.Products.AnyAsync(p => p.Code == vm.Code && p.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode produk sudah digunakan");
            return View(vm);
        }

        product.Code = vm.Code;
        product.Name = vm.Name;
        product.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Produk '{product.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            TempData["Error"] = "Produk tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        product.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Produk '{product.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}