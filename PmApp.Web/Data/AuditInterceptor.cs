
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Ambil username dari pengguna yang sedang login
    private string GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.FindFirstValue(ClaimTypes.Name)
                   ?? "system";
        }

        // Untuk seed data atau proses tanpa sesi login
        return "system";
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context == null)
            return;

        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker
                     .Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.CreatedDate = now;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    // Data pembuat asli tidak boleh berubah
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;

                    entry.Entity.UpdatedBy = currentUser;
                    entry.Entity.UpdatedDate = now;

                    if (entry.Entity.IsDeleted &&
                        entry.Property(x => x.IsDeleted).IsModified)
                    {
                        entry.Entity.DeletedBy = currentUser;
                        entry.Entity.DeletedDate = now;
                    }

                    break;

                case EntityState.Deleted:
                    // Hard delete diubah menjadi soft delete
                    entry.State = EntityState.Modified;

                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;

                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedBy = currentUser;
                    entry.Entity.DeletedDate = now;
                    entry.Entity.UpdatedBy = currentUser;
                    entry.Entity.UpdatedDate = now;
                    break;
            }
        }
    }
}
