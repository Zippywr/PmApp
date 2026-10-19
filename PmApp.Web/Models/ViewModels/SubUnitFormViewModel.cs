using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class SubUnitFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nama Sub Unit wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Sub Unit")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Unit wajib dipilih")]
    [Display(Name = "Unit Induk")]
    public int UnitId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    public List<SelectListItem> UnitList { get; set; } = new();
}
