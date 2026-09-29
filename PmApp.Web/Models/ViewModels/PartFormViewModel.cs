using System.ComponentModel.DataAnnotations;

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
    [Display(Name = "Kategori")]
    public string? Category { get; set; }

    [MaxLength(100)]
    [Display(Name = "Merek")]
    public string? Brand { get; set; }

    [MaxLength(20)]
    [Display(Name = "Satuan")]
    public string Unit { get; set; } = "PCS";

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}