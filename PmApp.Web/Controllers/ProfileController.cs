using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;

    public ProfileController(AppDbContext db, IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    private int CurrentId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _db.AppUsers.AsNoTracking()
            .Include(u => u.AssignedProducts).ThenInclude(a => a.Product)
            .FirstOrDefaultAsync(u => u.Id == CurrentId);
        return user == null ? NotFound() : View(user);
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        var mustChange = await _db.AppUsers.AsNoTracking()
            .AnyAsync(u => u.Id == CurrentId && u.MustChangePassword);
        ViewBag.MustChange = mustChange;
        return View(new ChangePasswordViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == CurrentId);
        if (user == null) return NotFound();
        var valid = _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.CurrentPassword);
        if (valid == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(vm.CurrentPassword), "Password saat ini salah.");
            return View(vm);
        }
        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, vm.NewPassword) != PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(vm.NewPassword), "Password baru harus berbeda dari password sebelumnya.");
            return View(vm);
        }
        user.PasswordHash = _hasher.HashPassword(user, vm.NewPassword);
        user.MustChangePassword = false;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        await _db.SaveChangesAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Password berhasil diperbarui. Silakan login kembali.";
        return RedirectToAction("Login", "Account");
    }
}
