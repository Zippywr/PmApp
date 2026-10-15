using System;
using System.Collections.Generic;

namespace PmApp.Web.Models.ViewModels
{
    // ============================================================
    // VIEW MODEL UTAMA
    // ============================================================
    public class HomeDashboardViewModel
    {
        // -------- Filter periode --------
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public string MonthName { get; set; } = "";
        public string ActiveTab { get; set; } = "active";
        public int SelectedDelayWeek { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        // -------- KPI --------
        public int TotalAssets { get; set; }
        public int TotalParts { get; set; }

        public int TotalScheduled { get; set; }
        public int TotalDone { get; set; }
        public int TotalOnTime { get; set; }

        public double Compliance { get; set; }   // % on-time
        public double Completion { get; set; }   // % done

        // -------- DELAY KANBAN --------
        public int DelayCountTotal { get; set; }
        public Dictionary<int, int> DelayCountByWeek { get; set; } = new();
        public List<HomeTaskRow> DelayTasks { get; set; } = new();

        // -------- MINGGU INI --------
        public int ThisWeekCount { get; set; }
        public List<HomeTaskRow> ThisWeekTasks { get; set; } = new();

        // -------- MINGGU DEPAN --------
        public int NextWeekCount { get; set; }
        public List<HomeTaskRow> NextWeekTasks { get; set; } = new();

        // -------- SELESAI KANBAN --------
        public int CompletedCount { get; set; }
        public List<HomeTaskRow> CompletedKanban { get; set; } = new();

        // -------- TAB AKTIF (pagination) --------
        public int ActivePage { get; set; } = 1;
        public int ActiveTotalPages { get; set; } = 1;
        public int ActiveTotalItems { get; set; }
        public List<HomeTaskRow> ActiveTasks { get; set; } = new();

        // -------- TAB SELESAI (pagination) --------
        public int DonePage { get; set; } = 1;
        public int DoneTotalPages { get; set; } = 1;
        public int DoneTotalItems { get; set; }
        public List<HomeTaskRow> DoneTasks { get; set; } = new();

        // -------- WEEKLY CHART --------
        public List<HomeWeeklyTrend> Weekly { get; set; } = new();
    }

    // ============================================================
    // BARIS TASK (dipakai di kanban & tabel)
    // ============================================================
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
        public int WeekNumber { get; set; }
    }

    // ============================================================
    // TREND MINGGUAN (untuk chart bar + line)
    // ============================================================
    public class HomeWeeklyTrend
    {
        public int WeekNumber { get; set; }
        public string WeekLabel { get; set; } = "";
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }

        public int Scheduled { get; set; }   // Plan
        public int Done { get; set; }        // Actual
        public int Delay { get; set; }       // Delay aktif

        // Performance % = Done / Scheduled * 100
        public double Performance => Scheduled == 0
            ? 0
            : Math.Round((double)Done / Scheduled * 100, 1);
    }
}