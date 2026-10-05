using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Line produksi — di bawah Plant.
/// </summary>
public class Line : BaseEntity
{
    [Required(ErrorMessage = "Kode line wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Line")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama line wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Line")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Plant wajib dipilih")]
    [Display(Name = "Plant")]
    public int PlantId { get; set; }
    public Plant? Plant { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Area> Areas { get; set; } = new List<Area>();
}