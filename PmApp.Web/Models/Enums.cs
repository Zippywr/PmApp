namespace PmApp.Web.Models;

public enum Criticality
{
    Low,
    Normal,
    High,
    Critical
}

// ============================================================
// FASE B — PREVENTIVE MAINTENANCE
// ============================================================

public enum FrequencyType
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public enum MaintenanceMethod
{
    Visual,
    Measure,
    Test,
    Replace,
    Calibrate,
    Clean,
    Lubricate
}

public enum PmStatus
{
    Scheduled,
    InProgress,
    Done,
    Overdue,
    Cancelled
}

public enum MachineCondition
{
    Pending,
    OK,
    Partial,
    NG
}
public enum MachineRunningState
{
    ON,       // Mesin harus jalan saat cek
    OFF,      // Mesin harus mati saat cek
    Either    // Bisa ON atau OFF
}