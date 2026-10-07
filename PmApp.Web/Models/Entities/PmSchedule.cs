using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public class PmSchedule : BaseEntity
{
    [Display(Name = "Task Template")]
    public int TaskTemplateId { get; set; }
    public PmTaskTemplate? TaskTemplate { get; set; }

    [Display(Name = "Mesin")]
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    [Display(Name = "Tahun")]
    public int Year { get; set; }

    [Display(Name = "Bulan")]
    public int Month { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Plan")]
    public DateTime? PlanDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Actual")]
    public DateTime? ActualDate { get; set; }

    [MaxLength(200)]
    [Display(Name = "PIC")]
    public string? PIC { get; set; }

    [Display(Name = "Status")]
    public PmStatus Status { get; set; } = PmStatus.Scheduled;

    [MaxLength(500)]
    [Display(Name = "Catatan")]
    public string? Notes { get; set; }
}