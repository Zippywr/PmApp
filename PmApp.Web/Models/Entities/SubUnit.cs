using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public class SubUnit : BaseEntity
{
    [Required(ErrorMessage = "Kode sub unit wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Sub Unit")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama sub unit wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Sub Unit")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unit wajib dipilih")]
    [Display(Name = "Unit")]
    public int UnitId { get; set; }
    public Unit? Unit { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}