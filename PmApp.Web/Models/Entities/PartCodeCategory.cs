using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Kategori utama part — contoh: Bearing, Seal, Belt, Oil.
/// </summary>
public class PartCodeCategory : BaseEntity
{
    [Required(ErrorMessage = "Kode kategori wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Kategori")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama kategori wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Kategori")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    // Navigation
    public ICollection<SubGrupCategory> SubCategories { get; set; } = new List<SubGrupCategory>();
    public ICollection<Part> Parts { get; set; } = new List<Part>();
}