using System.ComponentModel.DataAnnotations;
using PmApp.Web.Models;

namespace PmApp.Web.Models.ViewModels;

public class MetodeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nama Metode wajib diisi")]
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
