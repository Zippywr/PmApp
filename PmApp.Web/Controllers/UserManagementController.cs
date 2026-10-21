using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class UserManagementController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;

    public UserManagementController(AppDbContext db, IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? role)
    {
        var query = _db.AppUsers.AsNoTracking()
            .Include(u => u.AssignedProducts).ThenInclude(a => a.Product)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(u => u.Username.Contains(s) || u.FullName.Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);
        ViewBag.Search = search;
        ViewBag.Role = role;
        return View(await query.OrderBy(u => u.FullName).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new UserFormViewModel();
        await LoadProductsAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel vm)
    {
        await ValidateInputAsync(vm);
        if (!ModelState.IsValid)
        {
            await LoadProductsAsync(vm);
            return View(vm);
        }
        var password = TemporaryPasswordService.Generate();
        var entity = new AppUser
        {
            Username = vm.Username.Trim(),
            FullName = vm.FullName.Trim(),
            Role = vm.Role,
            IsActive = vm.IsActive,
            MustChangePassword = true
        };
        entity.PasswordHash = _hasher.HashPassword(entity, password);
        foreach (var id in vm.Role == AppRoles.LineHead ? vm.ProductIds.Distinct() : Enumerable.Empty<int>())
            entity.AssignedProducts.Add(new AppUserProduct { ProductId = id });
        _db.AppUsers.Add(entity);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(vm.Username), "Username sudah dipakai atau penyimpanan gagal.");
            await LoadProductsAsync(vm);
            return View(vm);
        }
        return ShowIssued(vm.Username, password, "Akun baru dibuat. Wajib ganti password saat login pertama.");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.AppUsers.Include(u => u.AssignedProducts)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        var vm = new UserFormViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            ProductIds = user.AssignedProducts.Select(a => a.ProductId).ToList()
        };
        await LoadProductsAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        var user = await _db.AppUsers.Include(u => u.AssignedProducts)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        vm.Username = user.Username; // Username identitas tetap, tidak menerima nilai form.
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId) && actorId == id)
            ModelState.AddModelError("", "Tidak dapat mengubah role/status akun sendiri. Gunakan Profil untuk mengganti password.");
        await ValidateInputAsync(vm, user.Id);
        if (user.Role == AppRoles.Admin && (vm.Role != AppRoles.Admin || !vm.IsActive))
        {
            if (await _db.AppUsers.CountAsync(u => u.Role == AppRoles.Admin && u.IsActive) <= 1)
                ModelState.AddModelError("", "Admin aktif terakhir tidak boleh dinonaktifkan atau diganti role.");
        }
        if (!ModelState.IsValid)
        {
            await LoadProductsAsync(vm);
            return View(vm);
        }
        user.FullName = vm.FullName.Trim();
        user.Role = vm.Role;
        user.IsActive = vm.IsActive;
        user.SecurityStamp = Guid.NewGuid().ToString("N"); // Akhiri semua sesi lama.
        var oldIds = user.AssignedProducts.Select(a => a.ProductId).ToHashSet();
        var newIds = (vm.Role == AppRoles.LineHead ? vm.ProductIds : new List<int>()).ToHashSet();
        foreach (var old in user.AssignedProducts.Where(a => !newIds.Contains(a.ProductId)).ToList())
            _db.AppUserProducts.Remove(old);
        foreach (var newId in newIds.Where(line => !oldIds.Contains(line)))
            _db.AppUserProducts.Add(new AppUserProduct { UserId = id, ProductId = newId });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Pengguna berhasil diperbarui; sesi lama otomatis dibatalkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId) && actorId == id)
            return BadRequest("Gunakan Profil untuk mengganti password akun sendiri.");
        if (!user.IsActive) return BadRequest("Aktifkan akun sebelum reset password.");
        var password = TemporaryPasswordService.Generate();
        user.PasswordHash = _hasher.HashPassword(user, password);
        user.MustChangePassword = true;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        await _db.SaveChangesAsync();
        // Jangan masukkan password plaintext ke TempData, URL, logs, ataupun email.
        return ShowIssued(user.Username, password, "Password sementara baru; sesi lama telah dibatalkan.");
    }

    private IActionResult ShowIssued(string name, string pass, string reason)
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache";
        Response.Headers["Pragma"] = "no-cache";
        return View("PasswordIssued", new PasswordIssuedViewModel
        {
            Username = name,
            TemporaryPassword = pass,
            Reason = reason
        });
    }

    private async Task LoadProductsAsync(UserFormViewModel vm)
    {
        vm.ProductOptions = await _db.Products.AsNoTracking().OrderBy(p => p.Code)
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Code + " - " + p.Name })
            .ToListAsync();
    }

    private async Task ValidateInputAsync(UserFormViewModel vm, int? editingId = null)
    {
        if (!new[] { AppRoles.Admin, AppRoles.TechnicalSupport, AppRoles.LineHead }.Contains(vm.Role))
            ModelState.AddModelError(nameof(vm.Role), "Role tidak valid.");
        if (vm.Role == AppRoles.LineHead && vm.ProductIds.Count == 0)
            ModelState.AddModelError(nameof(vm.ProductIds), "Line Head wajib memiliki minimal satu Product.");
        if (vm.ProductIds.Count > 0 && vm.Role == AppRoles.LineHead)
        {
            var count = await _db.Products.CountAsync(p => vm.ProductIds.Contains(p.Id));
            if (count != vm.ProductIds.Distinct().Count())
                ModelState.AddModelError(nameof(vm.ProductIds), "Salah satu Product tidak ditemukan.");
        }
        if (editingId == null && await _db.AppUsers.IgnoreQueryFilters()
            .AnyAsync(u => u.Username.ToLower() == vm.Username.Trim().ToLower()))
            ModelState.AddModelError(nameof(vm.Username), "Username sudah ada.");
    }
}
