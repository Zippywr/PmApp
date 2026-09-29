using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class TechnicianController : Controller
{
    private readonly AppDbContext _db;

    public TechnicianController(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // INDEX — Daftar Teknisi
    // ============================================================
    public async Task<IActionResult> Index()
    {
        var technicians = await _db.Technicians
            .OrderBy(t => t.Code)
            .ToListAsync();
        return View(technicians);
    }

    // ============================================================
    // CREATE — GET (form)
    // ============================================================
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TechnicianFormViewModel());
    }

    // ============================================================
    // CREATE — POST (simpan)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TechnicianFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var exists = await _db.Technicians.AnyAsync(t => t.Code == vm.Code);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode teknisi sudah digunakan");
            return View(vm);
        }

        var technician = new Technician
        {
            Code = vm.Code,
            Name = vm.Name,
            Skill = vm.Skill,
            Shift = vm.Shift,
            Phone = vm.Phone,
            Email = vm.Email,
            IsActive = vm.IsActive
        };

        _db.Technicians.Add(technician);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Teknisi '{technician.Name}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // EDIT — GET (form)
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var technician = await _db.Technicians.FirstOrDefaultAsync(t => t.Id == id);
        if (technician == null) return NotFound();

        var vm = new TechnicianFormViewModel
        {
            Id = technician.Id,
            Code = technician.Code,
            Name = technician.Name,
            Skill = technician.Skill,
            Shift = technician.Shift,
            Phone = technician.Phone,
            Email = technician.Email,
            IsActive = technician.IsActive
        };

        return View(vm);
    }

    // ============================================================
    // EDIT — POST (simpan)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TechnicianFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var technician = await _db.Technicians.FirstOrDefaultAsync(t => t.Id == id);
        if (technician == null) return NotFound();

        var exists = await _db.Technicians.AnyAsync(t => t.Code == vm.Code && t.Id != id);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Code), "Kode teknisi sudah digunakan");
            return View(vm);
        }

        technician.Code = vm.Code;
        technician.Name = vm.Name;
        technician.Skill = vm.Skill;
        technician.Shift = vm.Shift;
        technician.Phone = vm.Phone;
        technician.Email = vm.Email;
        technician.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Teknisi '{technician.Name}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // DELETE — POST (soft delete)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var technician = await _db.Technicians
            .FirstOrDefaultAsync(t => t.Id == id);

        if (technician == null)
        {
            TempData["Error"] = "Teknisi tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        technician.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Teknisi '{technician.Name}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}