using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Services;

public class PmScheduleGeneratorService
{
    private readonly AppDbContext _db;

    public PmScheduleGeneratorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateIdPmAsync(int assetId)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
        if (asset == null) return string.Empty;

        var parts = asset.Code.Split('-');
        int lineSeq = 1;
        int machineSeq = 1;

        if (parts.Length >= 1 && int.TryParse(parts[0], out var ls)) lineSeq = ls;
        if (parts.Length >= 3 && int.TryParse(parts[2], out var ms)) machineSeq = ms;

        var maxTaskSeq = await _db.PmTaskTemplates
            .Where(t => t.AssetId == assetId)
            .Select(t => (int?)t.TaskSequence)
            .MaxAsync() ?? -1;

        var taskSeq = maxTaskSeq + 1;

        return lineSeq.ToString("D2") + machineSeq.ToString("D2") + taskSeq.ToString("D2");
    }

    public async Task<int> GenerateScheduleForTaskAsync(int taskTemplateId, int year)
    {
        var task = await _db.PmTaskTemplates.FirstOrDefaultAsync(t => t.Id == taskTemplateId);
        if (task == null) return 0;

        int added = 0;
        var startMonth = task.StartMonth;
        var periode = task.PeriodeMonth;

        for (int month = 1; month <= 12; month++)
        {
            if (periode <= 0) continue;
            if ((month - startMonth) % periode != 0) continue;

            var exists = await _db.PmSchedules
                .AnyAsync(s => s.TaskTemplateId == taskTemplateId
                            && s.Year == year
                            && s.Month == month);

            if (exists) continue;

            _db.PmSchedules.Add(new PmSchedule
            {
                TaskTemplateId = taskTemplateId,
                AssetId = task.AssetId,
                Year = year,
                Month = month,
                PlanDate = new DateTime(year, month, 1),
                Status = PmStatus.Scheduled,
                Notes = "Auto-generated",
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });

            added++;
        }

        if (added > 0)
            await _db.SaveChangesAsync();

        return added;
    }

    public async Task<int> DeleteSchedulesForTaskAsync(int taskTemplateId)
    {
        var list = await _db.PmSchedules
            .Where(s => s.TaskTemplateId == taskTemplateId)
            .ToListAsync();

        foreach (var s in list)
        {
            s.IsDeleted = true;
        }

        if (list.Any())
            await _db.SaveChangesAsync();

        return list.Count;
    }
}