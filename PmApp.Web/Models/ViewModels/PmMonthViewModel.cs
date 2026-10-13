using PmApp.Web.Models.Entities;

namespace PmApp.Web.Models.ViewModels;

public class PmMonthViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";

    public List<PmWeekGroup> Weeks { get; set; } = new();

    public int TotalTask { get; set; }
    public int TotalDone { get; set; }
    public int TotalPending { get; set; }
    public int TotalDelay { get; set; }
}

public class PmWeekGroup
{
    public int WeekNumber { get; set; }      // 1, 2, 3, 4
    public string WeekLabel { get; set; } = "";  // "Week 1 (01-07 Okt)"
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }

    public List<PmWeekRow> Rows { get; set; } = new();

    public int TotalTask { get; set; }
    public int TotalDone { get; set; }
    public int TotalPending { get; set; }
    public int TotalDelay { get; set; }
}