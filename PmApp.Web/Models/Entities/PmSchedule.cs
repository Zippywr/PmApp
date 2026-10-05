using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Jadwal PM yang digenerate otomatis dari PmTaskTemplate.
/// </summary>
public class PmSchedule : BaseEntity
{
    [Display(Name = "Template Task")]
    public int TaskTemplateId { get; set; }
    public PmTaskTemplate? TaskTemplate { get; set; }

    [Display(Name = "Part BOM")]
    public int AssetPartId { get; set; }
    public AssetPart? AssetPart { get; set; }

    [Required, MaxLength(300)]
    [Display(Name = "Nama Task")]
    public string TaskName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Jadwal")]
    public DateTime DueDate { get; set; }

    [Display(Name = "Status")]
    public PmStatus Status { get; set; } = PmStatus.Scheduled;

    [MaxLength(200)]
    [Display(Name = "Penanggung Jawab")]
    public string? PIC { get; set; }

    [Display(Name = "Kondisi Mesin")]
    public MachineCondition MachineCondition { get; set; } = MachineCondition.Pending;

    [Display(Name = "Selesai Pada")]
    public DateTime? CompletedAt { get; set; }

    [MaxLength(500)]
    [Display(Name = "Catatan")]
    public string? Notes { get; set; }
}