using Microsoft.EntityFrameworkCore;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ============================================================
    // DB SETS — FASE A: MASTER DATA ONLY
    // ============================================================
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<AssetPart> AssetParts => Set<AssetPart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------------------------------------------------
        // ASSET
        // ---------------------------------------------------------
        modelBuilder.Entity<Asset>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------------------------------------------------------
        // PART
        // ---------------------------------------------------------
        modelBuilder.Entity<Part>(e =>
        {
            e.HasIndex(x => x.PartNo).IsUnique();
        });

        // ---------------------------------------------------------
        // ASSET PART (BOM)
        // ---------------------------------------------------------
        modelBuilder.Entity<AssetPart>(e =>
        {
            e.HasOne(x => x.Asset)
             .WithMany(a => a.AssetParts)
             .HasForeignKey(x => x.AssetId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Part)
             .WithMany(p => p.AssetParts)
             .HasForeignKey(x => x.PartId)
             .OnDelete(DeleteBehavior.Restrict);

            // 1 part hanya boleh tercatat sekali per mesin
            e.HasIndex(x => new { x.AssetId, x.PartId }).IsUnique();
        });

        

        // =========================================================
        // GLOBAL QUERY FILTER — SOFT DELETE
        // =========================================================
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(
                        property,
                        System.Linq.Expressions.Expression.Constant(false)),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}