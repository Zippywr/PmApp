using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public class PmTaskTemplate : BaseEntity
{
    [MaxLength(20)]
    [Display(Name = "ID PM")]
    public string IdPm { get; set; } = string.Empty;

    [Display(Name = "No Line")]
    public int LineSequence { get; set; }

    [Display(Name = "No Mesin")]
    public int MachineSequence { get; set; }

    [Display(Name = "No Task")]
    public int TaskSequence { get; set; }

    [Required]
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public int? SubUnitId { get; set; }
    public SubUnit? SubUnit { get; set; }

    public int? MethodId { get; set; }
    public Metode? Method { get; set; }

    public int? StandardId { get; set; }
    public Standar? Standard { get; set; }

    [Display(Name = "Waktu (menit)")]
    public int WorkHourMinutes { get; set; }

    [Display(Name = "Man Power")]
    public int ManPower { get; set; } = 1;

    [Display(Name = "Kondisi Mesin")]
    public MachineRunningState MachineState { get; set; } = MachineRunningState.OFF;

    [Display(Name = "Periode (bulan)")]
    public int PeriodeMonth { get; set; } = 1;

    [Display(Name = "Mulai Bulan")]
    public int StartMonth { get; set; } = 1;

    [MaxLength(500)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}