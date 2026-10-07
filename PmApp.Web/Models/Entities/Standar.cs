using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public class Standar : BaseEntity
{
    [Required(ErrorMessage = "Nama standar wajib diisi")]
    [MaxLength(500)]
    [Display(Name = "Standar")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}