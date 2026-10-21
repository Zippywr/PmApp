using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username wajib diisi.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]{3,50}$", ErrorMessage = "Username 3-50 karakter, hanya huruf, angka, titik, minus, garis bawah.")]
    public string Username { get; set; } = "";

    [Required, StringLength(100)]
    [Display(Name = "Nama lengkap")]
    public string FullName { get; set; } = "";

    [Required]
    public string Role { get; set; } = "LineHead";

    public bool IsActive { get; set; } = true;
    public List<int> ProductIds { get; set; } = new();
    public List<SelectListItem> ProductOptions { get; set; } = new();
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "Password saat ini")]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(12), DataType(DataType.Password)]
    [Display(Name = "Password baru (minimal 12 karakter)")]
    public string NewPassword { get; set; } = "";

    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    [Display(Name = "Konfirmasi password baru")]
    public string ConfirmPassword { get; set; } = "";
}

public class PasswordIssuedViewModel
{
    public string Username { get; set; } = "";
    public string TemporaryPassword { get; set; } = "";
    public string Reason { get; set; } = "";
}
