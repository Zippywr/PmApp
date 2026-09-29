using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Bill of Materials (BOM) — relasi Mesin ↔ Part
/// 1 mesin bisa punya banyak part.
/// </summary>
public class AssetPart : BaseEntity
{
    [Display(Name = "Aset")]
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    [Display(Name = "Part")]
    public int PartId { get; set; }
    public Part? Part { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity minimal 1")]
    [Display(Name = "Jumlah")]
    public int Quantity { get; set; } = 1;

    [MaxLength(100)]
    [Display(Name = "Posisi")]
    public string? Position { get; set; }

    [MaxLength(500)]
    [Display(Name = "Catatan")]
    public string? Notes { get; set; }
}