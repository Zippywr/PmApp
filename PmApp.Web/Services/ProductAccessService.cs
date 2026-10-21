using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Services;

/// <summary>
/// Pemeriksaan akses berdasarkan Product. Line Head hanya boleh mengakses
/// mesin yang ProductId-nya terdaftar pada AppUserProducts.
/// ProductId null selalu ditolak untuk Line Head (fail closed).
/// </summary>
public class ProductAccessService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public ProductAccessService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    private ClaimsPrincipal User => _http.HttpContext?.User ?? new ClaimsPrincipal();
    public bool IsLineHead => User.IsInRole(AppRoles.LineHead);
    public bool IsManager => User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.TechnicalSupport);

    public async Task<int[]> AssignedProductIdsAsync()
    {
        if (!IsLineHead ||
            !int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Array.Empty<int>();

        return await _db.AppUserProducts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ProductId)
            .ToArrayAsync();
    }

    public IQueryable<Asset> ScopeAssets(IQueryable<Asset> query, int[] productIds)
        => IsManager ? query : IsLineHead
            ? query.Where(a => a.ProductId.HasValue && productIds.Contains(a.ProductId.Value))
            : query.Where(a => false);

    public IQueryable<PmTaskTemplate> ScopeTasks(IQueryable<PmTaskTemplate> query, int[] productIds)
        => IsManager ? query : IsLineHead
            ? query.Where(t => t.Asset != null && t.Asset.ProductId.HasValue &&
                productIds.Contains(t.Asset.ProductId.Value))
            : query.Where(t => false);

    public IQueryable<PmSchedule> ScopeSchedules(IQueryable<PmSchedule> query, int[] productIds)
        => IsManager ? query : IsLineHead
            ? query.Where(s => s.TaskTemplate != null && s.TaskTemplate.Asset != null &&
                s.TaskTemplate.Asset.ProductId.HasValue &&
                productIds.Contains(s.TaskTemplate.Asset.ProductId.Value))
            : query.Where(s => false);

    public async Task<bool> CanAccessAssetAsync(int assetId)
    {
        if (IsManager) return true;
        if (!IsLineHead) return false;
        var ids = await AssignedProductIdsAsync();
        return await _db.Assets.AsNoTracking()
            .AnyAsync(a => a.Id == assetId && a.ProductId.HasValue &&
                ids.Contains(a.ProductId.Value));
    }

    public async Task<bool> CanAccessTaskAsync(int taskId)
    {
        if (IsManager) return true;
        if (!IsLineHead) return false;
        var ids = await AssignedProductIdsAsync();
        return await _db.PmTaskTemplates.AsNoTracking()
            .AnyAsync(t => t.Id == taskId && t.Asset != null &&
                t.Asset.ProductId.HasValue && ids.Contains(t.Asset.ProductId.Value));
    }

    public async Task<bool> CanAccessScheduleAsync(int scheduleId)
    {
        if (IsManager) return true;
        if (!IsLineHead) return false;
        var ids = await AssignedProductIdsAsync();
        return await _db.PmSchedules.AsNoTracking()
            .AnyAsync(s => s.Id == scheduleId && s.TaskTemplate != null &&
                s.TaskTemplate.Asset != null && s.TaskTemplate.Asset.ProductId.HasValue &&
                ids.Contains(s.TaskTemplate.Asset.ProductId.Value));
    }
}
