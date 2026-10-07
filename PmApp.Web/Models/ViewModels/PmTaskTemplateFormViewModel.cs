using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PmApp.Web.Models;

namespace PmApp.Web.Models.ViewModels;

public class PmTaskTemplateFormViewModel
{
    public int Id { get; set; }
    public string? IdPm { get; set; }

    [Required(ErrorMessage = "Mesin wajib dipilih")]
    [Display(Name = "Mesin")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "Unit wajib dipilih")]
    [Display(Name = "Unit")]
    public int? UnitId { get; set; }

    [Required(ErrorMessage = "Sub Unit wajib dipilih")]
    [Display(Name = "Sub Unit")]
    public int? SubUnitId { get; set; }

    [Required(ErrorMessage = "Metode wajib diisi")]
    [Display(Name = "Metode")]
    public string MethodName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Standar wajib diisi")]
    [Display(Name = "Standar")]
    public string StandardName { get; set; } = string.Empty;

    [Range(1, 10000, ErrorMessage = "Waktu 1-10000 menit")]
    [Display(Name = "Waktu (menit)")]
    public int WorkHourMinutes { get; set; } = 60;

    [Range(1, 100)]
    [Display(Name = "Man Power")]
    public int ManPower { get; set; } = 1;

    [Display(Name = "Kondisi Mesin")]
    public MachineRunningState MachineState { get; set; } = MachineRunningState.OFF;

    [Range(1, 120, ErrorMessage = "Periode 1-120 bulan")]
    [Display(Name = "Periode (bulan)")]
    public int PeriodeMonth { get; set; } = 1;

    [Range(1, 12, ErrorMessage = "Mulai bulan 1-12")]
    [Display(Name = "Mulai Bulan")]
    public int StartMonth { get; set; } = 1;

    [MaxLength(500)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    // Dropdown lists
    public List<SelectListItem> AssetList { get; set; } = new();
    public List<SelectListItem> UnitList { get; set; } = new();
    public List<SelectListItem> SubUnitList { get; set; } = new();

    // Datalist suggestions
    public List<string> MethodSuggestion { get; set; } = new();
    public List<string> StandardSuggestion { get; set; } = new();
}