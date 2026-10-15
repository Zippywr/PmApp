using PmApp.Web.Models;

namespace PmApp.Web.Models.ViewModels;

public class PmUpdateCellViewModel
{
    // ============================================================
    // IDs
    // ============================================================
    public int ScheduleId { get; set; }
    public int TaskTemplateId { get; set; }
    public int AssetId { get; set; }

    // ============================================================
    // INFO TASK (readonly, display only)
    // ============================================================
    public string IdPm { get; set; } = "";
    public string AssetCode { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string? LineName { get; set; }
    public string? AreaName { get; set; }
    public string? UnitName { get; set; }
    public string? SubUnitName { get; set; }
    public string? MethodName { get; set; }
    public string? StandardName { get; set; }
    public int WorkHourMinutes { get; set; }
    public int ManPower { get; set; }
    public string? MachineState { get; set; }
    public string? Remark { get; set; }

    // ============================================================
    // PERIODE
    // ============================================================
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";

    // ============================================================
    // DATA YANG BISA DIEDIT
    // ============================================================
    public DateTime? PlanDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string? PIC { get; set; }
    public PmStatus Status { get; set; } = PmStatus.Scheduled;
    public MachineCondition Result { get; set; } = MachineCondition.Pending;
    public string? Notes { get; set; }

    // ============================================================
    // INFO DELAY (kalau ada)
    // ============================================================
    public int MonthsDelay { get; set; }
    public bool IsDelay => MonthsDelay > 0;

    // ============================================================
    // UNTUK DROPDOWN
    // ============================================================
    public List<string> PICs { get; set; } = new();
}