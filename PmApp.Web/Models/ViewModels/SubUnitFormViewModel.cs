using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class SubUnitFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode sub unit wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Sub Unit")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama sub unit wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Sub Unit")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unit wajib dipilih")]
    [Display(Name = "Unit")]
    public int UnitId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    public List<SelectListItem> UnitList { get; set; } = new();
}