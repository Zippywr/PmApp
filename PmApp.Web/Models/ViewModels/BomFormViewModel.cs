using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class BomFormViewModel
{
    public int AssetId { get; set; }

    // ============================================================
    // MODE: "existing" (pilih dari master) atau "new" (input baru)
    // ============================================================
    public string Mode { get; set; } = "existing";

    // ============================================================
    // MODE EXISTING — pilih dari master part
    // ============================================================
    public int? PartId { get; set; }

    // ============================================================
    // MODE NEW — input part baru
    // ============================================================
    [MaxLength(50)]
    [Display(Name = "Part No")]
    public string? NewPartNo { get; set; }

    [MaxLength(200)]
    [Display(Name = "Nama Part")]
    public string? NewPartName { get; set; }

    [MaxLength(100)]
    [Display(Name = "Kategori")]
    public string? NewCategory { get; set; }

    [MaxLength(100)]
    [Display(Name = "Merek")]
    public string? NewBrand { get; set; }

    [MaxLength(20)]
    [Display(Name = "Satuan")]
    public string NewUnit { get; set; } = "PCS";

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? NewDescription { get; set; }

    // ============================================================
    // BOM FIELDS (selalu ada)
    // ============================================================
    [Range(1, int.MaxValue, ErrorMessage = "Qty minimal 1")]
    [Display(Name = "Jumlah")]
    public int Quantity { get; set; } = 1;

    [MaxLength(100)]
    [Display(Name = "Posisi")]
    public string? Position { get; set; }

    [MaxLength(500)]
    [Display(Name = "Catatan")]
    public string? Notes { get; set; }

    // ============================================================
    // UNTUK DROPDOWN
    // ============================================================
    public List<SelectListItem> PartList { get; set; } = new();
    public string? AssetName { get; set; }
    public string? AssetCode { get; set; }
}