using DocumentFormat.OpenXml.Vml;
using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Gedung / pabrik — level tertinggi dari hirarki lokasi.
/// </summary>
public class Plant : BaseEntity
{
    [Required(ErrorMessage = "Kode plant wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Plant")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama plant wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Plant")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Line> Lines { get; set; } = new List<Line>();
}