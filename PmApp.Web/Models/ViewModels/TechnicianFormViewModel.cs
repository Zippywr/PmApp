using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class TechnicianFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode teknisi wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Teknisi")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama teknisi wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Lengkap")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Keahlian / Skill")]
    public string? Skill { get; set; }

    [MaxLength(50)]
    [Display(Name = "Shift")]
    public string? Shift { get; set; }

    [MaxLength(30)]
    [Phone(ErrorMessage = "Format telepon tidak valid")]
    [Display(Name = "Telepon")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}