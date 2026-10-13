using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Services;

public class PmScheduleHistoryService
{
    private readonly AppDbContext _db;

    public PmScheduleHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        PmSchedule schedule,
        string? statusBefore,
        string? resultBefore,
        string? picBefore,
        DateTime? planDateBefore,
        DateTime? actualDateBefore,
        string? notesBefore,
        string? actionBy = null)
    {
        var statusAfter = schedule.Status.ToString();
        var resultAfter = schedule.Result.ToString();
        var picAfter = schedule.PIC;
        var planDateAfter = schedule.PlanDate;
        var actualDateAfter = schedule.ActualDate;
        var notesAfter = schedule.Notes;

        // Cek ada perubahan?
        bool hasChange =
            statusBefore != statusAfter ||
            resultBefore != resultAfter ||
            picBefore != picAfter ||
            planDateBefore != planDateAfter ||
            actualDateBefore != actualDateAfter ||
            notesBefore != notesAfter;

        if (!hasChange) return;

        // Buat ringkasan
        var parts = new List<string>();

        if (statusBefore != statusAfter)
            parts.Add($"Status: {statusBefore} → {statusAfter}");

        if (resultBefore != resultAfter)
            parts.Add($"Hasil: {resultBefore} → {resultAfter}");

        if (picBefore != picAfter)
            parts.Add($"PIC: {picBefore ?? "-"} → {picAfter ?? "-"}");

        if (actualDateBefore != actualDateAfter)
            parts.Add($"Actual: {actualDateBefore?.ToString("dd/MM/yyyy") ?? "-"} → {actualDateAfter?.ToString("dd/MM/yyyy") ?? "-"}");

        if (planDateBefore != planDateAfter)
            parts.Add($"Plan: {planDateBefore?.ToString("dd/MM/yyyy") ?? "-"} → {planDateAfter?.ToString("dd/MM/yyyy") ?? "-"}");

        if (notesBefore != notesAfter)
            parts.Add("Catatan diubah");

        var history = new PmScheduleHistory
        {
            PmScheduleId = schedule.Id,
            ActionBy = actionBy ?? "system",
            ActionDate = DateTime.Now,

            StatusBefore = statusBefore,
            StatusAfter = statusAfter,

            ResultBefore = resultBefore,
            ResultAfter = resultAfter,

            PICBefore = picBefore,
            PICAfter = picAfter,

            PlanDateBefore = planDateBefore,
            PlanDateAfter = planDateAfter,

            ActualDateBefore = actualDateBefore,
            ActualDateAfter = actualDateAfter,

            NotesBefore = notesBefore,
            NotesAfter = notesAfter,

            Summary = string.Join(" | ", parts),
            CreatedBy = actionBy ?? "system",
            CreatedDate = DateTime.Now
        };

        _db.PmScheduleHistories.Add(history);
        await _db.SaveChangesAsync();
    }
}