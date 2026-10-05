using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Fungsi mesin — contoh: Main Machine, Support, Utility.
/// </summary>
public class MachineFunction : BaseEntity
{
    [Required(ErrorMessage = "Kode fungsi wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Fungsi")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama fungsi wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Fungsi")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}