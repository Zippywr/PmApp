
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Services;

public class AppUserBootstrapService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly IConfiguration _configuration;

    public AppUserBootstrapService(
        AppDbContext db,
        IPasswordHasher<AppUser> hasher,
        IConfiguration configuration)
    {
        _db = db;
        _hasher = hasher;
        _configuration = configuration;
    }

    public async Task EnsureAdminAsync()
    {
        // Bootstrap hanya jika belum ada akun sama sekali
        if (await _db.AppUsers
            .IgnoreQueryFilters()
            .AnyAsync())
        {
            return;
        }

        var username = _configuration[
            "PM_BOOTSTRAP_ADMIN_USER"]?.Trim() ?? "admin";

        var password = _configuration[
            "PM_BOOTSTRAP_ADMIN_PASSWORD"];

        if (username.Length < 3 || username.Length > 50)
        {
            throw new InvalidOperationException(
                "Username Admin harus 3-50 karakter.");
        }

        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 12)
        {
            throw new InvalidOperationException(
                "Password Admin pertama wajib diatur " +
                "melalui PM_BOOTSTRAP_ADMIN_PASSWORD " +
                "dengan minimal 12 karakter.");
        }

        var user = new AppUser
        {
            Username = username,
            FullName = "Administrator",
            Role = AppRoles.Admin,
            IsActive = true
        };

        user.PasswordHash =
            _hasher.HashPassword(user, password);

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
    }
}
