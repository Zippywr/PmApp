using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class PlantFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode plant wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Plant")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama plant wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Plant")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}