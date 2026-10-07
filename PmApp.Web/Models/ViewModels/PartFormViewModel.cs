using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class PartFormViewModel
{
    public int Id { get; set; }

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

    [MaxLength(100)]
    [Display(Name = "Kategori")]
    public string? Category { get; set; }

    [MaxLength(20)]
    [Display(Name = "Satuan")]
    public string Unit { get; set; } = "PCS";

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    [Display(Name = "Brand")]
    public int? BrandId { get; set; }

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

    public List<SelectListItem> BrandList { get; set; } = new();
}