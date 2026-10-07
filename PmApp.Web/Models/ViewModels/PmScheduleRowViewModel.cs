using PmApp.Web.Models.Entities;

namespace PmApp.Web.Models.ViewModels;

public class PmScheduleRowViewModel
{
    public PmTaskTemplate Task { get; set; } = null!;
    public List<PmScheduleCell> Months { get; set; } = new();
}

public class PmScheduleCell
{
    public int Month { get; set; }
    public int? ScheduleId { get; set; }
    public DateTime? PlanDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string? PIC { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public bool IsPlanned { get; set; }
}
