
using System;
using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator"; // Legacy: akun lama dipetakan manual setelah migrasi
    public const string TechnicalSupport = "TS";
    public const string LineHead = "LineHead";
}

public class AppUser : BaseEntity
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = AppRoles.Operator;

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; }
    public ICollection<AppUserLine> AssignedLines { get; set; } = new List<AppUserLine>();

    [Required]
    [StringLength(40)]
    public string SecurityStamp { get; set; }
        = Guid.NewGuid().ToString("N");
}
