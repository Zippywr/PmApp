using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class SubGrupCategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode sub kategori wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Sub Kategori")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama sub kategori wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Sub Kategori")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori utama wajib dipilih")]
    [Display(Name = "Kategori Utama")]
    public int PartCodeCategoryId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    public List<SelectListItem> CategoryList { get; set; } = new();
}