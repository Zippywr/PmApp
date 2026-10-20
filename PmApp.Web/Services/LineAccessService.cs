
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Services;

/// <summary>
/// Membatasi akses data PM berdasarkan Role dan Line pengguna.
/// Pembatasan dilakukan di server, bukan hanya melalui UI.
/// </summary>
public class LineAccessService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public LineAccessService(
        AppDbContext db,
        IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    private ClaimsPrincipal User =>
        _http.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsLineHead =>
        User.IsInRole(AppRoles.LineHead);

    public bool IsManager =>
        User.IsInRole(AppRoles.Admin) ||
        User.IsInRole(AppRoles.TechnicalSupport);

    // ==========================================
    // LINE YANG DITUGASKAN KEPADA PENGGUNA
    // ==========================================

    public async Task<int[]> AssignedLineIdsAsync()
    {
        if (!IsLineHead ||
            !int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var id))
        {
            return Array.Empty<int>();
        }

        return await _db.AppUserLines
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .Select(x => x.LineId)
            .ToArrayAsync();
    }

    // ==========================================
    // FILTER DATA BERDASARKAN LINE
    // ==========================================

    public IQueryable<Asset> ScopeAssets(
        IQueryable<Asset> query,
        int[] lines)
    {
        if (IsManager)
            return query;

        if (!IsLineHead)
            return query.Where(a => false);

        return query.Where(a =>
            a.LineId.HasValue &&
            lines.Contains(a.LineId.Value));
    }

    public IQueryable<PmTaskTemplate> ScopeTasks(
        IQueryable<PmTaskTemplate> query,
        int[] lines)
    {
        if (IsManager)
            return query;

        if (!IsLineHead)
            return query.Where(t => false);

        return query.Where(t =>
            t.Asset != null &&
            t.Asset.LineId.HasValue &&
            lines.Contains(t.Asset.LineId.Value));
    }

    public IQueryable<PmSchedule> ScopeSchedules(
        IQueryable<PmSchedule> query,
        int[] lines)
    {
        if (IsManager)
            return query;

        if (!IsLineHead)
            return query.Where(s => false);

        return query.Where(s =>
            s.TaskTemplate != null &&
            s.TaskTemplate.Asset != null &&
            s.TaskTemplate.Asset.LineId.HasValue &&
            lines.Contains(
                s.TaskTemplate.Asset.LineId.Value));
    }

    // ==========================================
    // VALIDASI AKSES ASSET
    // ==========================================

    public async Task<bool> CanAccessAssetAsync(int assetId)
    {
        if (IsManager)
            return true;

        if (!IsLineHead)
            return false;

        var ids = await AssignedLineIdsAsync();

        return await _db.Assets
            .AsNoTracking()
            .AnyAsync(a =>
                a.Id == assetId &&
                a.LineId.HasValue &&
                ids.Contains(a.LineId.Value));
    }

    // ==========================================
    // VALIDASI AKSES TASK PM
    // ==========================================

    public async Task<bool> CanAccessTaskAsync(int taskId)
    {
        if (IsManager)
            return true;

        if (!IsLineHead)
            return false;

        var ids = await AssignedLineIdsAsync();

        return await _db.PmTaskTemplates
            .AsNoTracking()
            .AnyAsync(t =>
                t.Id == taskId &&
                t.Asset != null &&
                t.Asset.LineId.HasValue &&
                ids.Contains(t.Asset.LineId.Value));
    }

    // ==========================================
    // VALIDASI AKSES JADWAL PM
    // ==========================================

    public async Task<bool> CanAccessScheduleAsync(int scheduleId)
    {
        if (IsManager)
            return true;

        if (!IsLineHead)
            return false;

        var ids = await AssignedLineIdsAsync();

        return await _db.PmSchedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id == scheduleId &&
                s.TaskTemplate != null &&
                s.TaskTemplate.Asset != null &&
                s.TaskTemplate.Asset.LineId.HasValue &&
                ids.Contains(
                    s.TaskTemplate.Asset.LineId.Value));
    }
}
