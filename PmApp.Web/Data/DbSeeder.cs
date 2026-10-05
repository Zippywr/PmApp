using Microsoft.EntityFrameworkCore;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;

namespace PmApp.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Assets.AnyAsync()) return;

        // ============================================================
        // 1. PLANTS
        // ============================================================
        var plants = new List<Plant>
        {
            new() { Code = "PLT-01", Name = "Plant Utama", Description = "Plant produksi utama",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PLT-02", Name = "Plant 2", Description = "Plant produksi kedua",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PLT-03", Name = "Gudang", Description = "Area pergudangan",
                    CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Plants.AddRange(plants);

        await db.SaveChangesAsync();

        // ============================================================
        // 2. LINES (di bawah Plant)
        // ============================================================
        var lines = new List<Line>
        {
            new() { Code = "LN-01", Name = "Line Produksi 1", PlantId = plants[0].Id,
                    Description = "Line utama produksi",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-02", Name = "Line Produksi 2", PlantId = plants[0].Id,
                    Description = "Line kedua",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-03", Name = "Line Assembly", PlantId = plants[0].Id,
                    Description = "Line perakitan",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-04", Name = "Line Packing", PlantId = plants[1].Id,
                    Description = "Line pengepakan",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-05", Name = "Line Utility", PlantId = plants[1].Id,
                    Description = "Line utilitas",
                    CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Lines.AddRange(lines);

        await db.SaveChangesAsync();

        // ============================================================
        // 3. AREAS (di bawah Line)
        // ============================================================
        var areas = new List<Area>
        {
            new() { Code = "AR-01", Name = "5C & QC Machining", LineId = lines[0].Id,
                    Description = "Area mesin besar",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-02", Name = "Area Produksi B", LineId = lines[0].Id,
                    Description = "Area mesin kecil",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-03", Name = "Area QC", LineId = lines[0].Id,
                    Description = "Quality control",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-04", Name = "Area Gudang", LineId = lines[3].Id,
                    Description = "Penyimpanan",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-05", Name = "Area Utility", LineId = lines[4].Id,
                    Description = "Area utilitas",
                    CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Areas.AddRange(areas);

        // ============================================================
        // 4. PRODUCTS
        // ============================================================
        var products = new List<Product>
        {
            new() { Code = "PRD-001", Name = "Conrod", Description = "Connecting Rod",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-002", Name = "Shaft B", Description = "Shaft baja tipe B",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-003", Name = "Gear C", Description = "Gear baja tipe C",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-004", Name = "Housing D", Description = "Housing cast iron tipe D",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-005", Name = "Cover E", Description = "Tutup plastik tipe E",
                    CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Products.AddRange(products);

        // ============================================================
        // 5. BRANDS (mesin + part)
        // ============================================================
        var brands = new List<Brand>
        {
            new() { Code = "BRD-001", Name = "Yasunaga", Description = "Maker mesin CNC",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-002", Name = "Fanuc", Description = "Maker controller & servo",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-003", Name = "SKF", Description = "Maker bearing",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-004", Name = "NOK", Description = "Maker seal",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-005", Name = "Shell", Description = "Maker oli & grease",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-006", Name = "Mitsuboshi", Description = "Maker belt",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-007", Name = "Siemens", Description = "Maker electrical",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-008", Name = "Omron", Description = "Maker sensor",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-009", Name = "CKI", Description = "Maker mesin numbering",
                    CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Brands.AddRange(brands);

        // ============================================================
        // 6. MC CATEGORY (Kategori Mesin)
        // ============================================================
        var mcCategories = new List<McCategory>
        {
            new() { Code = "MCC-01", Name = "Machining Center",
                    Description = "Pusat permesinan CNC",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-02", Name = "Numbering / Barcode",
                    Description = "Mesin penomoran / barcode",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-03", Name = "Welding",
                    Description = "Mesin las / welding",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-04", Name = "Grinding",
                    Description = "Mesin gerinda",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-05", Name = "Turning",
                    Description = "Mesin bubut",
                    CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.McCategories.AddRange(mcCategories);

        // ============================================================
        // 7. MACHINE FUNCTION (Fungsi Mesin)
        // ============================================================
        var machineFunctions = new List<MachineFunction>
        {
            new() { Code = "MF-01", Name = "Main Machine",
                    Description = "Mesin utama produksi",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-02", Name = "Support",
                    Description = "Mesin pendukung",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-03", Name = "Utility",
                    Description = "Mesin utilitas",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-04", Name = "Quality Control",
                    Description = "Mesin QC",
                    CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-05", Name = "Packing",
                    Description = "Mesin pengepakan",
                    CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.MachineFunctions.AddRange(machineFunctions);

        await db.SaveChangesAsync();

        // ============================================================
        // 8. ASSETS (Mesin) — pakai FK ke master
        // ============================================================
        var assets = new List<Asset>
        {
            new()
            {
                Code = "1-MC-01",
                Name = "CNC Milling 01",
                OpNo = "OP-120",
                LineId = lines[0].Id,                      // LN-01
                AreaId = areas[0].Id,                      // AR-01 5C & QC Machining
                McCategoryId = mcCategories[0].Id,         // MCC-01 Machining Center
                MachineFunctionId = machineFunctions[0].Id,// MF-01 Main Machine
                BrandId = brands[0].Id,                    // BRD-001 Yasunaga
                ProductId = products[0].Id,                // PRD-001 Conrod
                Model = "F25M",
                SerialNumber = "M3975",
                YearMade = 2012,
                Capacity = "500x400 mm",
                InstallDate = new DateTime(2012, 6, 15),
                Description = "Mesin milling utama",
                Criticality = Criticality.Critical,
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            },
            new()
            {
                Code = "1-MC-02",
                Name = "CNC Milling 02",
                OpNo = "OP-130",
                LineId = lines[0].Id,
                AreaId = areas[0].Id,
                McCategoryId = mcCategories[0].Id,
                MachineFunctionId = machineFunctions[0].Id,
                BrandId = brands[0].Id,
                ProductId = products[0].Id,
                Model = "SPM",
                SerialNumber = "M3977",
                YearMade = 2012,
                Capacity = "500x400 mm",
                InstallDate = new DateTime(2012, 8, 20),
                Description = "Mesin milling CNC kedua",
                Criticality = Criticality.High,
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            },
            new()
            {
                Code = "1-NB-01",
                Name = "Numbering Machine 01",
                OpNo = "OP-140",
                LineId = lines[0].Id,
                AreaId = areas[0].Id,
                McCategoryId = mcCategories[1].Id,         // MCC-02 Numbering/barcode
                MachineFunctionId = machineFunctions[0].Id,
                BrandId = brands[8].Id,                    // BRD-009 CKI
                ProductId = products[0].Id,
                Model = "SPM",
                SerialNumber = "M3978",
                YearMade = 2012,
                Description = "Mesin penomoran / barcode",
                Criticality = Criticality.Normal,
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            }
        };
        db.Assets.AddRange(assets);

        await db.SaveChangesAsync();

        // ============================================================
        // 9a. PART CODE CATEGORY
        // ============================================================
        var partCategories = new List<PartCodeCategory>
        {
            new() { Code = "CAT-01", Name = "Bearing",      Description = "Kategori bearing",      CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-02", Name = "Seal",         Description = "Kategori seal",         CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-03", Name = "Belt",         Description = "Kategori belt",         CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-04", Name = "Oil",          Description = "Kategori oli",          CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-05", Name = "Grease",       Description = "Kategori grease",       CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-06", Name = "Filter",       Description = "Kategori filter",       CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-07", Name = "Hose",         Description = "Kategori selang",       CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-08", Name = "Fitting",      Description = "Kategori fitting",      CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-09", Name = "Sensor",       Description = "Kategori sensor",       CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-10", Name = "Electrical",   Description = "Kategori electrical",   CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-11", Name = "Coupling",     Description = "Kategori coupling",     CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "CAT-12", Name = "Pump",         Description = "Kategori pompa",        CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.PartCodeCategories.AddRange(partCategories);

        await db.SaveChangesAsync();

        // ============================================================
        // 9b. SUB GRUP CATEGORY
        // ============================================================
        var subCategories = new List<SubGrupCategory>
        {
            new() { Code = "SUB-001", Name = "Deep Groove Ball Bearing", PartCodeCategoryId = partCategories[0].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-002", Name = "Taper Roller Bearing",      PartCodeCategoryId = partCategories[0].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-003", Name = "Oil Seal",                  PartCodeCategoryId = partCategories[1].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-004", Name = "V-Belt",                    PartCodeCategoryId = partCategories[2].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-005", Name = "Hydraulic Oil",             PartCodeCategoryId = partCategories[3].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-006", Name = "Gear Oil",                  PartCodeCategoryId = partCategories[3].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-007", Name = "Multi Purpose Grease",      PartCodeCategoryId = partCategories[4].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-008", Name = "Air Filter",                PartCodeCategoryId = partCategories[5].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-009", Name = "Hydraulic Hose",            PartCodeCategoryId = partCategories[6].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SUB-010", Name = "Proximity Sensor",          PartCodeCategoryId = partCategories[8].Id, CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.SubGrupCategories.AddRange(subCategories);

        await db.SaveChangesAsync();

        // ============================================================
        // 9c. PARTS — 32 part dengan FK
        // ============================================================
        var parts = new List<Part>();

        parts.Add(new() { PartNo = "321", Name = "Bearing 6205", Type = "DGBB", Unit = "PCS", PartCodeCategoryId = partCategories[0].Id, SubGrupCategoryId = subCategories[0].Id, BrandId = brands[2].Id, StockQty = 12, MinQty = 5, MaxQty = 20, Price = 150000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "322", Name = "Bearing 6206", Type = "DGBB", Unit = "PCS", PartCodeCategoryId = partCategories[0].Id, SubGrupCategoryId = subCategories[0].Id, BrandId = brands[2].Id, StockQty = 8, MinQty = 5, MaxQty = 20, Price = 175000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "323", Name = "Seal 25mm", Type = "Oil Seal", Unit = "PCS", PartCodeCategoryId = partCategories[1].Id, SubGrupCategoryId = subCategories[2].Id, BrandId = brands[3].Id, StockQty = 15, MinQty = 5, MaxQty = 20, Price = 85000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "324", Name = "Seal 30mm", Type = "Oil Seal", Unit = "PCS", PartCodeCategoryId = partCategories[1].Id, SubGrupCategoryId = subCategories[2].Id, BrandId = brands[3].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 95000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "325", Name = "Belt V A-40", Type = "V-Belt", Unit = "PCS", PartCodeCategoryId = partCategories[2].Id, SubGrupCategoryId = subCategories[3].Id, BrandId = brands[5].Id, StockQty = 6, MinQty = 3, MaxQty = 10, Price = 120000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "326", Name = "Belt V A-42", Type = "V-Belt", Unit = "PCS", PartCodeCategoryId = partCategories[2].Id, SubGrupCategoryId = subCategories[3].Id, BrandId = brands[5].Id, StockQty = 5, MinQty = 3, MaxQty = 10, Price = 130000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "327", Name = "Oli Hydraulic 46", Type = "Hydraulic", Unit = "Liter", PartCodeCategoryId = partCategories[3].Id, SubGrupCategoryId = subCategories[4].Id, BrandId = brands[4].Id, StockQty = 100, MinQty = 50, MaxQty = 200, Price = 45000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "328", Name = "Oli Gear 220", Type = "Gear", Unit = "Liter", PartCodeCategoryId = partCategories[3].Id, SubGrupCategoryId = subCategories[5].Id, BrandId = brands[4].Id, StockQty = 80, MinQty = 30, MaxQty = 150, Price = 55000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "329", Name = "Grease EP2", Type = "MP Grease", Unit = "KG", PartCodeCategoryId = partCategories[4].Id, SubGrupCategoryId = subCategories[6].Id, BrandId = brands[4].Id, StockQty = 20, MinQty = 10, MaxQty = 50, Price = 120000m, Location = "C-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "330", Name = "Filter Udara", Type = "Air Filter", Unit = "PCS", PartCodeCategoryId = partCategories[5].Id, SubGrupCategoryId = subCategories[7].Id, BrandId = brands[6].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 250000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "331", Name = "Filter Oli", Type = "Oil Filter", Unit = "PCS", PartCodeCategoryId = partCategories[5].Id, SubGrupCategoryId = subCategories[7].Id, BrandId = brands[6].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 200000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "332", Name = "Selang Hidrolik 1/2", Type = "Hydraulic", Unit = "Meter", PartCodeCategoryId = partCategories[6].Id, SubGrupCategoryId = subCategories[8].Id, BrandId = brands[6].Id, StockQty = 50, MinQty = 20, MaxQty = 100, Price = 75000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "333", Name = "Fitting 1/2", Type = "Hydraulic", Unit = "PCS", PartCodeCategoryId = partCategories[7].Id, BrandId = brands[6].Id, StockQty = 40, MinQty = 20, MaxQty = 80, Price = 35000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "334", Name = "Sensor Proximity", Type = "Inductive", Unit = "PCS", PartCodeCategoryId = partCategories[8].Id, SubGrupCategoryId = subCategories[9].Id, BrandId = brands[7].Id, StockQty = 5, MinQty = 2, MaxQty = 10, Price = 650000m, Location = "E-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "335", Name = "Limit Switch", Type = "Mechanical", Unit = "PCS", PartCodeCategoryId = partCategories[8].Id, BrandId = brands[7].Id, StockQty = 8, MinQty = 3, MaxQty = 15, Price = 450000m, Location = "E-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "336", Name = "Relay 24V", Type = "Relay", Unit = "PCS", PartCodeCategoryId = partCategories[9].Id, BrandId = brands[7].Id, StockQty = 12, MinQty = 5, MaxQty = 20, Price = 180000m, Location = "E-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "432", Name = "Bearing 6207", Type = "DGBB", Unit = "PCS", PartCodeCategoryId = partCategories[0].Id, SubGrupCategoryId = subCategories[0].Id, BrandId = brands[2].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 250000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "433", Name = "Bearing 6208", Type = "DGBB", Unit = "PCS", PartCodeCategoryId = partCategories[0].Id, SubGrupCategoryId = subCategories[0].Id, BrandId = brands[2].Id, StockQty = 6, MinQty = 3, MaxQty = 12, Price = 320000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "434", Name = "Seal 35mm", Type = "Oil Seal", Unit = "PCS", PartCodeCategoryId = partCategories[1].Id, SubGrupCategoryId = subCategories[2].Id, BrandId = brands[3].Id, StockQty = 12, MinQty = 5, MaxQty = 20, Price = 105000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "435", Name = "Seal 40mm", Type = "Oil Seal", Unit = "PCS", PartCodeCategoryId = partCategories[1].Id, SubGrupCategoryId = subCategories[2].Id, BrandId = brands[3].Id, StockQty = 8, MinQty = 5, MaxQty = 20, Price = 115000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "436", Name = "Belt V B-50", Type = "V-Belt", Unit = "PCS", PartCodeCategoryId = partCategories[2].Id, SubGrupCategoryId = subCategories[3].Id, BrandId = brands[5].Id, StockQty = 6, MinQty = 3, MaxQty = 10, Price = 180000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "437", Name = "Belt V B-52", Type = "V-Belt", Unit = "PCS", PartCodeCategoryId = partCategories[2].Id, SubGrupCategoryId = subCategories[3].Id, BrandId = brands[5].Id, StockQty = 5, MinQty = 3, MaxQty = 10, Price = 195000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "438", Name = "Oli Way Lube 68", Type = "Way Lube", Unit = "Liter", PartCodeCategoryId = partCategories[3].Id, SubGrupCategoryId = subCategories[4].Id, BrandId = brands[4].Id, StockQty = 60, MinQty = 30, MaxQty = 120, Price = 52000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "439", Name = "Oli Spindle 32", Type = "Spindle", Unit = "Liter", PartCodeCategoryId = partCategories[3].Id, SubGrupCategoryId = subCategories[4].Id, BrandId = brands[4].Id, StockQty = 50, MinQty = 20, MaxQty = 100, Price = 62000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "440", Name = "Grease MP2", Type = "MP Grease", Unit = "KG", PartCodeCategoryId = partCategories[4].Id, SubGrupCategoryId = subCategories[6].Id, BrandId = brands[4].Id, StockQty = 25, MinQty = 10, MaxQty = 50, Price = 135000m, Location = "C-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "441", Name = "Filter Coolant", Type = "Coolant", Unit = "PCS", PartCodeCategoryId = partCategories[5].Id, SubGrupCategoryId = subCategories[7].Id, BrandId = brands[6].Id, StockQty = 8, MinQty = 4, MaxQty = 15, Price = 220000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "442", Name = "Filter Hidrolik", Type = "Hydraulic", Unit = "PCS", PartCodeCategoryId = partCategories[5].Id, SubGrupCategoryId = subCategories[7].Id, BrandId = brands[6].Id, StockQty = 8, MinQty = 4, MaxQty = 15, Price = 240000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "443", Name = "Selang Pneumatic 3/8", Type = "Pneumatic", Unit = "Meter", PartCodeCategoryId = partCategories[6].Id, SubGrupCategoryId = subCategories[8].Id, BrandId = brands[6].Id, StockQty = 40, MinQty = 20, MaxQty = 80, Price = 68000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "444", Name = "Fitting 3/8", Type = "Pneumatic", Unit = "PCS", PartCodeCategoryId = partCategories[7].Id, BrandId = brands[6].Id, StockQty = 35, MinQty = 15, MaxQty = 60, Price = 32000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "445", Name = "Sensor Pressure", Type = "Pressure", Unit = "PCS", PartCodeCategoryId = partCategories[8].Id, BrandId = brands[7].Id, StockQty = 4, MinQty = 2, MaxQty = 8, Price = 750000m, Location = "E-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "446", Name = "Solenoid Valve 24V", Type = "Solenoid", Unit = "PCS", PartCodeCategoryId = partCategories[9].Id, BrandId = brands[6].Id, StockQty = 6, MinQty = 2, MaxQty = 10, Price = 480000m, Location = "E-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "447", Name = "Contactor 25A", Type = "Contactor", Unit = "PCS", PartCodeCategoryId = partCategories[9].Id, BrandId = brands[6].Id, StockQty = 5, MinQty = 2, MaxQty = 8, Price = 320000m, Location = "E-02", CreatedBy = "system", CreatedDate = DateTime.Now });

        db.Parts.AddRange(parts);

        await db.SaveChangesAsync();

        // ============================================================
        // 10. BOM — 32 baris (2 mesin × 16 part)
        // ============================================================
        var bom = new List<AssetPart>();

        // Mesin 1 (1-MC-01) — pakai part 321–336
        for (int i = 0; i < 16; i++)
        {
            bom.Add(new AssetPart
            {
                AssetId = assets[0].Id,
                PartId = parts[i].Id,
                Quantity = 1,
                Position = i < 4 ? "Spindle" : (i < 8 ? "Axis" : (i < 12 ? "Coolant" : "Electrical")),
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        // Mesin 2 (1-MC-02) — pakai part 432–447
        for (int i = 16; i < 32; i++)
        {
            bom.Add(new AssetPart
            {
                AssetId = assets[1].Id,
                PartId = parts[i].Id,
                Quantity = 1,
                Position = i < 20 ? "Chuck" : (i < 24 ? "Tailstock" : (i < 28 ? "Turret" : "Electrical")),
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        db.AssetParts.AddRange(bom);

        await db.SaveChangesAsync();

        // ============================================================
        // 11. PM TASK TEMPLATES — untuk mesin 1
        // ============================================================
        var bomCnc = await db.AssetParts
            .Where(ap => ap.AssetId == assets[0].Id)
            .OrderBy(ap => ap.Part!.PartNo)
            .Take(5)
            .ToListAsync();

        var tasks = new List<PmTaskTemplate>();

        if (bomCnc.Count >= 1)
        {
            tasks.Add(new PmTaskTemplate
            {
                AssetPartId = bomCnc[0].Id,
                TaskName = "Cek visual bearing",
                SubUnit = "Spindle Drive End",
                Method = MaintenanceMethod.Visual,
                Standard = "Tidak ada getaran abnormal, tidak ada kebocoran",
                FrequencyType = FrequencyType.Weekly,
                FrequencyValue = 6,
                PIC = "Budi Santoso",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });

            tasks.Add(new PmTaskTemplate
            {
                AssetPartId = bomCnc[0].Id,
                TaskName = "Greasing bearing",
                SubUnit = "Spindle Drive End",
                Method = MaintenanceMethod.Lubricate,
                Standard = "Grease EP2 secukupnya",
                FrequencyType = FrequencyType.Monthly,
                FrequencyValue = 3,
                PIC = "Budi Santoso",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });

            tasks.Add(new PmTaskTemplate
            {
                AssetPartId = bomCnc[0].Id,
                TaskName = "Ganti bearing",
                SubUnit = "Spindle Drive End",
                Method = MaintenanceMethod.Replace,
                Standard = "Bearing 6205 baru",
                FrequencyType = FrequencyType.Yearly,
                FrequencyValue = 2,
                PIC = "Andi Wijaya",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        if (bomCnc.Count >= 2)
        {
            tasks.Add(new PmTaskTemplate
            {
                AssetPartId = bomCnc[1].Id,
                TaskName = "Cek vibrasi bearing",
                SubUnit = "Spindle Non-Drive End",
                Method = MaintenanceMethod.Measure,
                Standard = "Vibrasi < 2 mm/s",
                FrequencyType = FrequencyType.Weekly,
                FrequencyValue = 6,
                PIC = "Budi Santoso",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        if (bomCnc.Count >= 3)
        {
            tasks.Add(new PmTaskTemplate
            {
                AssetPartId = bomCnc[2].Id,
                TaskName = "Cek kebocoran seal",
                SubUnit = "Spindle Shaft",
                Method = MaintenanceMethod.Visual,
                Standard = "Tidak ada kebocoran oli",
                FrequencyType = FrequencyType.Weekly,
                FrequencyValue = 6,
                PIC = "Andi Wijaya",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        if (bomCnc.Count >= 4)
        {
            tasks.Add(new PmTaskTemplate
            {
                AssetPartId = bomCnc[3].Id,
                TaskName = "Cek oli hydraulic",
                SubUnit = "Hydraulic Tank",
                Method = MaintenanceMethod.Measure,
                Standard = "Level oli MIN-MAX, warna bening",
                FrequencyType = FrequencyType.Daily,
                FrequencyValue = 7,
                PIC = "Budi Santoso",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        db.PmTaskTemplates.AddRange(tasks);

        await db.SaveChangesAsync();
    }
}