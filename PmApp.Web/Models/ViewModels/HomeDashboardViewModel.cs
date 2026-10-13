namespace PmApp.Web.Models.ViewModels;

public class HomeDashboardViewModel
{
    // ============================================
    // FILTER
    // ============================================
    public int SelectedYear { get; set; }
    public int SelectedMonth { get; set; }
    public string MonthName { get; set; } = "";
    public string ActiveTab { get; set; } = "active";

    // ============================================
    // SUMMARY
    // ============================================
    public int TotalAssets { get; set; }
    public int TotalParts { get; set; }

    // ============================================
    // KPI
    // ============================================
    public int TotalScheduled { get; set; }
    public int TotalDone { get; set; }
    public int TotalOnTime { get; set; }
    public double Compliance { get; set; }
    public double Completion { get; set; }

    // ============================================
    // KANBAN (4 kolom, preview max 10)
    // ============================================
    public List<HomeTaskRow> DelayTasks { get; set; } = new();
    public List<HomeTaskRow> ThisWeekTasks { get; set; } = new();
    public List<HomeTaskRow> NextWeekTasks { get; set; } = new();
    public List<HomeTaskRow> CompletedKanban { get; set; } = new();

    // Counter total (bukan cuma preview)
    public int DelayCount { get; set; }
    public int ThisWeekCount { get; set; }
    public int NextWeekCount { get; set; }
    public int CompletedCount { get; set; }

    // ============================================
    // TAB AKTIF (dengan pagination)
    // ============================================
    public List<HomeTaskRow> ActiveTasks { get; set; } = new();
    public int ActivePage { get; set; } = 1;
    public int ActiveTotalPages { get; set; } = 1;
    public int ActiveTotalItems { get; set; }

    // ============================================
    // TAB SELESAI (dengan pagination)
    // ============================================
    public List<HomeTaskRow> DoneTasks { get; set; } = new();
    public int DonePage { get; set; } = 1;
    public int DoneTotalPages { get; set; } = 1;
    public int DoneTotalItems { get; set; }

    public int PageSize { get; set; } = 25;

    // ============================================
    // TREND CHART
    // ============================================
    public List<HomeMonthlyTrend> Monthly { get; set; } = new();
}

public class HomeTaskRow
{
    public int Id { get; set; }
    public string IdPm { get; set; } = "";
    public string AssetCode { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string SubUnitName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public DateTime? PlanDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string Status { get; set; } = "";
    public string? PIC { get; set; }
    public int? DaysLeft { get; set; }
}

public class HomeMonthlyTrend
{
    public string Month { get; set; } = "";
    public int Scheduled { get; set; }
    public int Done { get; set; }
}