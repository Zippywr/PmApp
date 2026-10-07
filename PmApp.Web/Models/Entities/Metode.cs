using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public class Metode : BaseEntity
{
    [Required(ErrorMessage = "Kode metode wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Metode")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama metode wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Metode")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Initial")]
    public string? Initial { get; set; }

    [Display(Name = "Kondisi Mesin Default")]
    public MachineRunningState MachineState { get; set; } = MachineRunningState.OFF;

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }
}