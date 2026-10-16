using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Produk yang dihasilkan.
/// Dipakai untuk mengetahui posisi mesin saat produksi produk tertentu.
/// </summary>
public class Product : BaseEntity
{
    [Required(ErrorMessage = "Kode produk wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Produk")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama produk wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Produk")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    // Relasi baru: Product berada di satu Plant
    [Display(Name = "Plant")]
    public int? PlantId { get; set; }

    public Plant? Plant { get; set; }

    // Satu Product mempunyai banyak Line
    public ICollection<Line> Lines { get; set; }
        = new List<Line>();
}