using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Merek/maker — dipakai untuk mesin DAN part.
/// Auto-generate saat input part dengan brand baru (lihat PartController nanti).
/// </summary>
public class Brand : BaseEntity
{
    [Required(ErrorMessage = "Kode brand wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Brand")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama brand wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Brand")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}