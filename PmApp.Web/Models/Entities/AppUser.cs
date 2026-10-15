
using System;
using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator";
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

    [Required]
    [StringLength(40)]
    public string SecurityStamp { get; set; }
        = Guid.NewGuid().ToString("N");
}
