using PmApp.Web.Models.Entities;

namespace PmApp.Web.Models.ViewModels;

public class PmWeekViewModel
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }

    public List<PmWeekRow> Rows { get; set; } = new();

    public int TotalTask { get; set; }
    public int TotalDone { get; set; }
    public int TotalPending { get; set; }
    public int TotalDelay { get; set; }
}

public class PmWeekRow
{
    public int ScheduleId { get; set; }
    public int TaskTemplateId { get; set; }   // ← TAMBAH INI
    public string IdPm { get; set; } = "";
    public string AssetCode { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string SubUnitName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public int WorkHourMinutes { get; set; }
    public int ManPower { get; set; }

    public DateTime? PlanDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string? PIC { get; set; }
    public PmStatus Status { get; set; }
    public MachineCondition Result { get; set; }
    public string? Notes { get; set; }
}