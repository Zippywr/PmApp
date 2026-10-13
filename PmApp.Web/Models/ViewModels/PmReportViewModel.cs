namespace PmApp.Web.Models.ViewModels;

public class PmReportViewModel
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public string? LineName { get; set; }
    public string? AssetName { get; set; }

    public List<PmReportRow> Rows { get; set; } = new();

    public int TotalScheduled { get; set; }
    public int TotalDone { get; set; }
    public int TotalOnTime { get; set; }
    public int TotalOverdue { get; set; }
    public int TotalPending { get; set; }

    public int ResultOK { get; set; }
    public int ResultPartial { get; set; }
    public int ResultNG { get; set; }
    public int ResultPending { get; set; }

    public double ComplianceRate { get; set; }
    public double CompletionRate { get; set; }

    public List<ChartDataPoint> StatusChart { get; set; } = new();
    public List<ChartDataPoint> ResultChart { get; set; } = new();
    public List<MonthlyTrendPoint> MonthlyTrend { get; set; } = new();
    public List<ChartDataPoint> PerLineChart { get; set; } = new();
    public List<ChartDataPoint> PerMachineChart { get; set; } = new();
}

public class PmReportRow
{
    public int ScheduleId { get; set; }
    public string IdPm { get; set; } = "";
    public string AssetCode { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string LineName { get; set; } = "";
    public string SubUnitName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public int WorkHourMinutes { get; set; }
    public int ManPower { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public DateTime? PlanDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string? PIC { get; set; }
    public string Status { get; set; } = "";
    public string Result { get; set; } = "";
    public string? Notes { get; set; }
    public bool IsOnTime { get; set; }
}

public class ChartDataPoint
{
    public string Label { get; set; } = "";
    public int Value { get; set; }
}

public class MonthlyTrendPoint
{
    public string Month { get; set; } = "";
    public int Scheduled { get; set; }  // Plan
    public int Done { get; set; }       // Actual
    public int OnTime { get; set; }
    public int Delay { get; set; }      // Delay
    public double PerformanceRate { get; set; } // Performance %
}