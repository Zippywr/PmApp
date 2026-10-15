
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using PmApp.Web.Models.Entities;
using PmApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// MVC
// ============================================================
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<
    IPasswordHasher<AppUser>,
    PasswordHasher<AppUser>>();

builder.Services.AddScoped<AppUserBootstrapService>();
// ============================================================
// COOKIE AUTHENTICATION
// ============================================================
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {

        options.Events.OnValidatePrincipal = async context =>
        {
            var idText = context.Principal?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            var securityStamp = context.Principal?
                .FindFirstValue("SecurityStamp");

            if (!int.TryParse(idText, out var userId) ||
                string.IsNullOrEmpty(securityStamp))
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return;
            }

            var db = context.HttpContext.RequestServices
                .GetRequiredService<AppDbContext>();

            var user = await db.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.IsActive);

            var cookieRole = context.Principal?
                .FindFirstValue(ClaimTypes.Role);

            if (user == null ||
                user.SecurityStamp != securityStamp ||
                user.Role != cookieRole)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };

        // Halaman login
        options.LoginPath = "/Account/Login";

        // Halaman jika hak akses ditolak
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Nama cookie
        options.Cookie.Name = "PmApp.Auth";

        // Keamanan cookie
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Masa berlaku login
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        // Memperpanjang masa aktif ketika digunakan
        options.SlidingExpiration = true;
    });

// ============================================================
// AUTHORIZATION
// ============================================================
builder.Services.AddAuthorization(options =>
{
    // Seluruh endpoint wajib login secara default
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(AppRoles.Admin));

    options.AddPolicy("OperatorOrAdmin", policy =>
        policy.RequireRole(
            AppRoles.Admin,
            AppRoles.Operator));
});

// ============================================================
// AUDIT
// ============================================================
builder.Services.AddSingleton<AuditInterceptor>();

// ============================================================
// SERVICES
// ============================================================
builder.Services.AddScoped<
    PmApp.Web.Services.PmScheduleGeneratorService>();

builder.Services.AddScoped<
    PmApp.Web.Services.PmExportService>();

builder.Services.AddScoped<
    PmApp.Web.Services.PmScheduleHistoryService>();

builder.Services.AddScoped<
    PmApp.Web.Services.ExcelImportService>();

builder.Services.AddScoped<
    PmApp.Web.Services.PmOverdueService>();

// ============================================================
// DATABASE — EF CORE + SQL SERVER
// ============================================================
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"));

    options.AddInterceptors(
        sp.GetRequiredService<AuditInterceptor>());
});

var app = builder.Build();

// ============================================================
// DATABASE SEED
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    // Seed data PM yang sudah ada
    await DbSeeder.SeedAsync(db);

}

// ============================================================
// ERROR HANDLING
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ============================================================
// MIDDLEWARE
// ============================================================
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Periksa siapa pengguna yang sedang login
app.UseAuthentication();

// Periksa hak akses pengguna
app.UseAuthorization();

// ============================================================
// ROUTING
// ============================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
