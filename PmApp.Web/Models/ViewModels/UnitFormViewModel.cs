using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class UnitFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nama unit wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Unit")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}