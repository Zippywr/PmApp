namespace PmApp.Web.Models.ViewModels;

public class PmScheduleDetailViewModel
{
    public int TaskId { get; set; }
    public string IdPm { get; set; } = "";
    public int Year { get; set; }

    public string AssetCode { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string LineName { get; set; } = "";
    public string AreaName { get; set; } = "";
    public string OpNo { get; set; } = "";

    public string UnitName { get; set; } = "";
    public string SubUnitName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public string StandardName { get; set; } = "";
    public int WorkHourMinutes { get; set; }
    public int ManPower { get; set; }
    public string MachineState { get; set; } = "";
    public int PeriodeMonth { get; set; }
    public int StartMonth { get; set; }
    public string? Remark { get; set; }

    public List<PmScheduleDetailRow> Months { get; set; } = new();
}

public class PmScheduleDetailRow
{
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public bool IsPlanned { get; set; }
    public int? ScheduleId { get; set; }
    public DateTime? PlanDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string? PIC { get; set; }
    public string Status { get; set; } = "";
    public string Result { get; set; } = "";
    public string? Notes { get; set; }

    // Delay info
    public bool IsDelay { get; set; }
    public int MonthsDelay { get; set; }
}