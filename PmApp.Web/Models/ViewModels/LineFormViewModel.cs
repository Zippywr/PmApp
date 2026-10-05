using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class LineFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode line wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Line")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama line wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Line")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Plant wajib dipilih")]
    [Display(Name = "Plant")]
    public int PlantId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    // Untuk dropdown
    public List<SelectListItem> PlantList { get; set; } = new();
}