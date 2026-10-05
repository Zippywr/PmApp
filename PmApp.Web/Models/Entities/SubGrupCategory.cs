using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Sub-kategori part — contoh: Deep Groove Ball Bearing, Taper Roller Bearing.
/// Child dari PartCodeCategory.
/// </summary>
public class SubGrupCategory : BaseEntity
{
    [Required(ErrorMessage = "Kode sub kategori wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Sub Kategori")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama sub kategori wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Sub Kategori")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori wajib dipilih")]
    [Display(Name = "Kategori Utama")]
    public int PartCodeCategoryId { get; set; }
    public PartCodeCategory? PartCodeCategory { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Part> Parts { get; set; } = new List<Part>();
}