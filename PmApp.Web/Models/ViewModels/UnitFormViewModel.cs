using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class UnitFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode unit wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Unit")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama unit wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Unit")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}