using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Master Part — dengan kategori, sub-kategori, brand, dan stock.
/// </summary>
public class Part : BaseEntity
{
    // ============================================================
    // IDENTITAS
    // ============================================================

    [Required(ErrorMessage = "Part No wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Part No")]
    public string PartNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama part wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Part")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Type")]
    public string? Type { get; set; }

    [MaxLength(20)]
    [Display(Name = "Satuan")]
    public string Unit { get; set; } = "PCS";

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    // ============================================================
    // RELASI MASTER
    // ============================================================

    [Display(Name = "Kategori")]
    public int? PartCodeCategoryId { get; set; }
    public PartCodeCategory? PartCodeCategory { get; set; }

    [Display(Name = "Sub Kategori")]
    public int? SubGrupCategoryId { get; set; }
    public SubGrupCategory? SubGrupCategory { get; set; }

    [Display(Name = "Brand")]
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    // ============================================================
    // STOCK & HARGA
    // ============================================================

    [Range(0, int.MaxValue)]
    [Display(Name = "Stok Saat Ini")]
    public int StockQty { get; set; } = 0;

    [Range(0, int.MaxValue)]
    [Display(Name = "Stok Minimum")]
    public int MinQty { get; set; } = 0;

    [Range(0, int.MaxValue)]
    [Display(Name = "Stok Maximum")]
    public int MaxQty { get; set; } = 0;

    [MaxLength(100)]
    [Display(Name = "Lokasi Rak")]
    public string? Location { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Harga (IDR)")]
    public decimal Price { get; set; } = 0;

    // ============================================================
    // NAVIGATION
    // ============================================================
    public ICollection<AssetPart> AssetParts { get; set; } = new List<AssetPart>();
}