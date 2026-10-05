using Microsoft.EntityFrameworkCore;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ============================================================
    // DB SETS — MASTER LOKASI
    // ============================================================
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Line> Lines => Set<Line>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();

    // ============================================================
    // DB SETS — MASTER KATEGORI MESIN
    // ============================================================
    public DbSet<McCategory> McCategories => Set<McCategory>();
    public DbSet<MachineFunction> MachineFunctions => Set<MachineFunction>();

    // ============================================================
    // DB SETS — MASTER MESIN & PART
    // ============================================================
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<PartCodeCategory> PartCodeCategories => Set<PartCodeCategory>();
    public DbSet<SubGrupCategory> SubGrupCategories => Set<SubGrupCategory>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<AssetPart> AssetParts => Set<AssetPart>();

    // ============================================================
    // DB SETS — PM
    // ============================================================
    public DbSet<PmTaskTemplate> PmTaskTemplates => Set<PmTaskTemplate>();
    public DbSet<PmSchedule> PmSchedules => Set<PmSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------------------------------------------------
        // PLANT
        // ---------------------------------------------------------
        modelBuilder.Entity<Plant>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------------------------------------------------------
        // LINE
        // ---------------------------------------------------------
        modelBuilder.Entity<Line>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();

            e.HasOne(x => x.Plant)
             .WithMany(p => p.Lines)
             .HasForeignKey(x => x.PlantId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------------------------------------------------
        // AREA
        // ---------------------------------------------------------
        modelBuilder.Entity<Area>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();

            e.HasOne(x => x.Line)
             .WithMany(l => l.Areas)
             .HasForeignKey(x => x.LineId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------------------------------------------------
        // PRODUCT
        // ---------------------------------------------------------
        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------------------------------------------------------
        // BRAND
        // ---------------------------------------------------------
        modelBuilder.Entity<Brand>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------------------------------------------------------
        // MC CATEGORY
        // ---------------------------------------------------------
        modelBuilder.Entity<McCategory>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------------------------------------------------------
        // MACHINE FUNCTION
        // ---------------------------------------------------------
        modelBuilder.Entity<MachineFunction>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------------------------------------------------------
        // ASSET (Mesin) — dengan FK ke 6 master
        // ---------------------------------------------------------
        modelBuilder.Entity<Asset>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();

            e.HasOne(x => x.Line)
             .WithMany()
             .HasForeignKey(x => x.LineId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Area)
             .WithMany()
             .HasForeignKey(x => x.AreaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.McCategory)
             .WithMany()
             .HasForeignKey(x => x.McCategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.MachineFunction)
             .WithMany()
             .HasForeignKey(x => x.MachineFunctionId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Brand)
             .WithMany()
             .HasForeignKey(x => x.BrandId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------------------------------------------------
        // PART CODE CATEGORY
        // ---------------------------------------------------------
        modelBuilder.Entity<PartCodeCategory>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ---------------------------------------------------------
        // SUB GRUP CATEGORY
        // ---------------------------------------------------------
        modelBuilder.Entity<SubGrupCategory>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();

            e.HasOne(x => x.PartCodeCategory)
             .WithMany(c => c.SubCategories)
             .HasForeignKey(x => x.PartCodeCategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------------------------------------------------
        // PART
        // ---------------------------------------------------------
        modelBuilder.Entity<Part>(e =>
        {
            e.HasIndex(x => x.PartNo).IsUnique();

            e.HasOne(x => x.PartCodeCategory)
             .WithMany(c => c.Parts)
             .HasForeignKey(x => x.PartCodeCategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SubGrupCategory)
             .WithMany(s => s.Parts)
             .HasForeignKey(x => x.SubGrupCategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Brand)
             .WithMany()
             .HasForeignKey(x => x.BrandId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Price).HasPrecision(18, 2);
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

            e.HasIndex(x => new { x.AssetId, x.PartId }).IsUnique();
        });

        // ---------------------------------------------------------
        // PM TASK TEMPLATE
        // ---------------------------------------------------------
        modelBuilder.Entity<PmTaskTemplate>(e =>
        {
            e.HasOne(x => x.AssetPart)
             .WithMany()
             .HasForeignKey(x => x.AssetPartId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.AssetPartId);
            e.HasIndex(x => x.IsActive);
        });

        // ---------------------------------------------------------
        // PM SCHEDULE
        // ---------------------------------------------------------
        modelBuilder.Entity<PmSchedule>(e =>
        {
            e.HasOne(x => x.TaskTemplate)
             .WithMany()
             .HasForeignKey(x => x.TaskTemplateId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.AssetPart)
             .WithMany()
             .HasForeignKey(x => x.AssetPartId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.DueDate);
            e.HasIndex(x => x.Status);
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