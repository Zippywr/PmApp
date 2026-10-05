using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class McCategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode kategori wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Kategori")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama kategori wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Kategori")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}