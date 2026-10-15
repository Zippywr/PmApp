
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

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;

    public AccountController(
        AppDbContext db,
        IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // HALAMAN LOGIN
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    // PROSES LOGIN
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var username = model.Username.Trim();

        var user = await _db.AppUsers
            .FirstOrDefaultAsync(x =>
                x.Username == username && x.IsActive);

        bool passwordValid = false;

        if (user != null)
        {
            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password);

            passwordValid =
                result != PasswordVerificationResult.Failed;

            if (result ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _hasher.HashPassword(user, model.Password);

                await _db.SaveChangesAsync();
            }
        }

        if (user == null || !passwordValid)
        {
            ModelState.AddModelError(
                "",
                "Username atau password salah.");

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                "FullName",
                user.FullName),

            new Claim(
                ClaimTypes.Role,
                user.Role),

            new Claim(
                "SecurityStamp",
                user.SecurityStamp)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (!string.IsNullOrEmpty(model.ReturnUrl) &&
            Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // LOGOUT
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    // AKSES DITOLAK
    [HttpGet]
    [Authorize]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
