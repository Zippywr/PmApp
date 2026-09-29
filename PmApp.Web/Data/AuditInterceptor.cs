using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    // User yang sedang login — sementara hardcoded "system"
    // Nanti setelah autentikasi diimplementasikan, ambil dari HttpContext.User.Identity.Name
    private static string CurrentUser => "system";

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
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.Now;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = CurrentUser;
                    entry.Entity.CreatedDate = now;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    // Jangan timpa CreatedBy / CreatedDate
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;

                    entry.Entity.UpdatedBy = CurrentUser;
                    entry.Entity.UpdatedDate = now;

                    // Soft delete otomatis
                    if (entry.Entity.IsDeleted &&
                        entry.Property(x => x.IsDeleted).IsModified &&
                        entry.Property(x => x.DeletedBy).CurrentValue is null)
                    {
                        entry.Entity.DeletedBy = CurrentUser;
                        entry.Entity.DeletedDate = now;
                    }
                    break;

                case EntityState.Deleted:
                    // Cegah hard delete — ubah jadi soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedBy = CurrentUser;
                    entry.Entity.DeletedDate = now;
                    break;
            }
        }
    }
}