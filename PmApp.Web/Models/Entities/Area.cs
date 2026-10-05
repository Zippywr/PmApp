using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Area di dalam Line.
/// </summary>
public class Area : BaseEntity
{
    [Required(ErrorMessage = "Kode area wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Area")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama area wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Area")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Line wajib dipilih")]
    [Display(Name = "Line")]
    public int LineId { get; set; }
    public Line? Line { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}