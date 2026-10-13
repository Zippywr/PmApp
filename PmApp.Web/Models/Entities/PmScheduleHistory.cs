using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public class PmScheduleHistory : BaseEntity
{
    [Display(Name = "PM Schedule")]
    public int PmScheduleId { get; set; }
    public PmSchedule? PmSchedule { get; set; }

    [MaxLength(200)]
    [Display(Name = "Diubah Oleh")]
    public string ActionBy { get; set; } = "system";

    [Display(Name = "Waktu Perubahan")]
    public DateTime ActionDate { get; set; } = DateTime.Now;

    // ============================================================
    // BEFORE → AFTER
    // ============================================================
    [MaxLength(50)]
    public string? StatusBefore { get; set; }

    [MaxLength(50)]
    public string? StatusAfter { get; set; }

    [MaxLength(50)]
    public string? ResultBefore { get; set; }

    [MaxLength(50)]
    public string? ResultAfter { get; set; }

    [MaxLength(200)]
    public string? PICBefore { get; set; }

    [MaxLength(200)]
    public string? PICAfter { get; set; }

    public DateTime? PlanDateBefore { get; set; }
    public DateTime? PlanDateAfter { get; set; }

    public DateTime? ActualDateBefore { get; set; }
    public DateTime? ActualDateAfter { get; set; }

    [MaxLength(500)]
    public string? NotesBefore { get; set; }

    [MaxLength(500)]
    public string? NotesAfter { get; set; }

    // ============================================================
    // RINGKASAN PERUBAHAN
    // ============================================================
    [MaxLength(500)]
    [Display(Name = "Ringkasan")]
    public string? Summary { get; set; }
}