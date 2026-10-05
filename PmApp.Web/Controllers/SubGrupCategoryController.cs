using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class SubGrupCategoryController : Controller
{
    private readonly AppDbContext _db;

    public SubGrupCategoryController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.SubGrupCategories
            .Include(s => s.PartCodeCategory)
            .OrderBy(s => s.Code)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new SubGrupCategoryFormViewModel
        {
            CategoryList = await GetCategoryListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubGrupCategoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CategoryList = await GetCategoryListAsync();
            return View(vm);
        }

        if (await _db.SubGrupCategories.AnyAsync(s => s.Code == vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode sudah digunakan");
            vm.CategoryList = await GetCategoryListAsync();
            return View(vm);
        }

        _db.SubGrupCategories.Add(new SubGrupCategory
        {
            Code = vm.Code,
            Name = vm.Name,
            PartCodeCategoryId = vm.PartCodeCategoryId,
            Description = vm.Description
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub kategori berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.SubGrupCategories.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();

        return View(new SubGrupCategoryFormViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            PartCodeCategoryId = item.PartCodeCategoryId,
            Description = item.Description,
            CategoryList = await GetCategoryListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SubGrupCategoryFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.CategoryList = await GetCategoryListAsync();
            return View(vm);
        }

        var item = await _db.SubGrupCategories.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();

        if (await _db.SubGrupCategories.AnyAsync(s => s.Code == vm.Code && s.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode sudah digunakan");
            vm.CategoryList = await GetCategoryListAsync();
            return View(vm);
        }

        item.Code = vm.Code;
        item.Name = vm.Name;
        item.PartCodeCategoryId = vm.PartCodeCategoryId;
        item.Description = vm.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub kategori berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.SubGrupCategories
            .Include(s => s.Parts)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (item == null)
        {
            TempData["Error"] = "Sub kategori tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (item.Parts.Any())
        {
            TempData["Error"] = "Sub kategori tidak bisa dihapus, masih dipakai part.";
            return RedirectToAction(nameof(Index));
        }

        item.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Sub kategori berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCategoryListAsync()
    {
        return await _db.PartCodeCategories
            .OrderBy(c => c.Code)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Code + " - " + c.Name
            }).ToListAsync();
    }
}