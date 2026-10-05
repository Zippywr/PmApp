using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

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
}