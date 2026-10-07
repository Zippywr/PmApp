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
            new() { Code = "PLT-01", Name = "Plant Utama", Description = "Plant produksi utama", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PLT-02", Name = "Plant 2", Description = "Plant produksi kedua", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PLT-03", Name = "Gudang", Description = "Area pergudangan", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Plants.AddRange(plants);
        await db.SaveChangesAsync();

        // ============================================================
        // 2. LINES
        // ============================================================
        var lines = new List<Line>
        {
            new() { Code = "LN-01", Name = "Conrod", PlantId = plants[0].Id, Description = "Line Conrod", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-02", Name = "Line Produksi 2", PlantId = plants[0].Id, Description = "Line kedua", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-03", Name = "Line Assembly", PlantId = plants[0].Id, Description = "Line perakitan", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-04", Name = "Line Packing", PlantId = plants[1].Id, Description = "Line pengepakan", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "LN-05", Name = "Line Utility", PlantId = plants[1].Id, Description = "Line utilitas", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Lines.AddRange(lines);
        await db.SaveChangesAsync();

        // ============================================================
        // 3. AREAS
        // ============================================================
        var areas = new List<Area>
        {
            new() { Code = "AR-01", Name = "5C & QC Machining", LineId = lines[0].Id, Description = "Area mesin besar", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-02", Name = "Area Produksi B", LineId = lines[0].Id, Description = "Area mesin kecil", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-03", Name = "Area QC", LineId = lines[0].Id, Description = "Quality control", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-04", Name = "Area Gudang", LineId = lines[3].Id, Description = "Penyimpanan", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "AR-05", Name = "Area Utility", LineId = lines[4].Id, Description = "Area utilitas", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Areas.AddRange(areas);
        await db.SaveChangesAsync();

        // ============================================================
        // 4. PRODUCTS
        // ============================================================
        var products = new List<Product>
        {
            new() { Code = "PRD-001", Name = "Conrod", Description = "Connecting Rod", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-002", Name = "Shaft B", Description = "Shaft baja tipe B", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-003", Name = "Gear C", Description = "Gear baja tipe C", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-004", Name = "Housing D", Description = "Housing cast iron tipe D", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "PRD-005", Name = "Cover E", Description = "Tutup plastik tipe E", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        // ============================================================
        // 5. BRANDS
        // ============================================================
        var brands = new List<Brand>
        {
            new() { Code = "BRD-001", Name = "Yasunaga", Description = "Maker mesin CNC", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-002", Name = "Fanuc", Description = "Maker controller & servo", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-003", Name = "SKF", Description = "Maker bearing", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-004", Name = "NOK", Description = "Maker seal", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-005", Name = "Shell", Description = "Maker oli & grease", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-006", Name = "Mitsuboshi", Description = "Maker belt", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-007", Name = "Siemens", Description = "Maker electrical", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-008", Name = "Omron", Description = "Maker sensor", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "BRD-009", Name = "CKI", Description = "Maker mesin numbering", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Brands.AddRange(brands);
        await db.SaveChangesAsync();

        // ============================================================
        // 6. MC CATEGORIES
        // ============================================================
        var mcCategories = new List<McCategory>
        {
            new() { Code = "MCC-01", Name = "Machining Center", Description = "Pusat permesinan CNC", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-02", Name = "Numbering / Barcode", Description = "Mesin penomoran", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-03", Name = "Welding", Description = "Mesin las", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-04", Name = "Grinding", Description = "Mesin gerinda", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MCC-05", Name = "Turning", Description = "Mesin bubut", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.McCategories.AddRange(mcCategories);
        await db.SaveChangesAsync();

        // ============================================================
        // 7. MACHINE FUNCTIONS
        // ============================================================
        var machineFunctions = new List<MachineFunction>
        {
            new() { Code = "MF-01", Name = "Main Machine", Description = "Mesin utama produksi", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-02", Name = "Support", Description = "Mesin pendukung", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-03", Name = "Utility", Description = "Mesin utilitas", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-04", Name = "Quality Control", Description = "Mesin QC", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "MF-05", Name = "Packing", Description = "Mesin pengepakan", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.MachineFunctions.AddRange(machineFunctions);
        await db.SaveChangesAsync();

        // ============================================================
        // 8. UNITS — WAJIB SAVE DULU sebelum SubUnits
        // ============================================================
        var units = new List<Unit>
        {
            new() { Code = "U-01", Name = "Hydraulic Equipment", Description = "Peralatan hidrolik", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "U-02", Name = "Electrical Equipment", Description = "Peralatan listrik", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "U-03", Name = "Mechanical Equipment", Description = "Peralatan mekanik", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "U-04", Name = "Pneumatic Equipment", Description = "Peralatan pneumatik", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "U-05", Name = "Cooling System", Description = "Sistem pendingin", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "U-06", Name = "Lubrication System", Description = "Sistem pelumasan", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Units.AddRange(units);
        await db.SaveChangesAsync();

        // ============================================================
        // 9. SUB UNITS
        // ============================================================
        var subUnits = new List<SubUnit>
        {
            new() { Code = "SU-01", Name = "Tanki Hidrolik 1", UnitId = units[0].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SU-02", Name = "Tanki Hidrolik 2", UnitId = units[0].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SU-03", Name = "Motorpump", UnitId = units[0].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SU-04", Name = "Oil Cooler Pompa Hidrolik 1", UnitId = units[0].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SU-05", Name = "Oil Cooler Pompa Hidrolik 2", UnitId = units[0].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SU-06", Name = "Panel Listrik Utama", UnitId = units[1].Id, CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "SU-07", Name = "Kipas Pendingin", UnitId = units[4].Id, CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.SubUnits.AddRange(subUnits);
        await db.SaveChangesAsync();

        // ============================================================
        // 10. METODES
        // ============================================================
        var metodes = new List<Metode>
        {
            new() { Code = "M01", Name = "Change", Initial = "Chg", MachineState = MachineRunningState.OFF, Description = "Ganti part/komponen", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "M02", Name = "Check", Initial = "Chk", MachineState = MachineRunningState.ON, Description = "Pengecekan kondisi", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "M03", Name = "Cleaning", Initial = "Cln", MachineState = MachineRunningState.OFF, Description = "Pembersihan", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "M04", Name = "Repair", Initial = "Rpr", MachineState = MachineRunningState.OFF, Description = "Perbaikan", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "M05", Name = "Inspect", Initial = "Ins", MachineState = MachineRunningState.ON, Description = "Inspeksi visual", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "M06", Name = "Replace", Initial = "Rpl", MachineState = MachineRunningState.OFF, Description = "Ganti total", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Code = "M07", Name = "Greasing", Initial = "Grs", MachineState = MachineRunningState.OFF, Description = "Pelumasan", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Metodes.AddRange(metodes);
        await db.SaveChangesAsync();

        // ============================================================
        // 11. STANDARS
        // ============================================================
        var standars = new List<Standar>
        {
            new() { Name = "Ganti oli dengan Shell Tellus 32", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Name = "Tidak over noise, tidak vibrasi", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Name = "Bersih dari debu, tidak ada endapan kotoran", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Name = "Level oli antara MIN-MAX", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Name = "Tidak ada kebocoran", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Name = "Vibrasi < 2 mm/s", CreatedBy = "system", CreatedDate = DateTime.Now },
            new() { Name = "Temperatur normal < 70 C", CreatedBy = "system", CreatedDate = DateTime.Now }
        };
        db.Standars.AddRange(standars);
        await db.SaveChangesAsync();

        // ============================================================
        // 12. ASSETS
        // ============================================================
        var assets = new List<Asset>
        {
            new()
            {
                Code = "1-MC-01",
                Name = "CNC Milling 01",
                OpNo = "OP-120",
                LineId = lines[0].Id,
                AreaId = areas[0].Id,
                McCategoryId = mcCategories[0].Id,
                MachineFunctionId = machineFunctions[0].Id,
                BrandId = brands[0].Id,
                ProductId = products[0].Id,
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
                McCategoryId = mcCategories[1].Id,
                MachineFunctionId = machineFunctions[0].Id,
                BrandId = brands[8].Id,
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
        // 13. PARTS
        // ============================================================
        var parts = new List<Part>();

        parts.Add(new() { PartNo = "321", Name = "Bearing 6205", Type = "DGBB", Category = "Bearing", Unit = "PCS", BrandId = brands[2].Id, StockQty = 12, MinQty = 5, MaxQty = 20, Price = 150000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "322", Name = "Bearing 6206", Type = "DGBB", Category = "Bearing", Unit = "PCS", BrandId = brands[2].Id, StockQty = 8, MinQty = 5, MaxQty = 20, Price = 175000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "323", Name = "Seal 25mm", Type = "Oil Seal", Category = "Seal", Unit = "PCS", BrandId = brands[3].Id, StockQty = 15, MinQty = 5, MaxQty = 20, Price = 85000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "324", Name = "Seal 30mm", Type = "Oil Seal", Category = "Seal", Unit = "PCS", BrandId = brands[3].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 95000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "325", Name = "Belt V A-40", Type = "V-Belt", Category = "Belt", Unit = "PCS", BrandId = brands[5].Id, StockQty = 6, MinQty = 3, MaxQty = 10, Price = 120000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "326", Name = "Belt V A-42", Type = "V-Belt", Category = "Belt", Unit = "PCS", BrandId = brands[5].Id, StockQty = 5, MinQty = 3, MaxQty = 10, Price = 130000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "327", Name = "Oli Hydraulic 46", Type = "Hydraulic", Category = "Oil", Unit = "Liter", BrandId = brands[4].Id, StockQty = 100, MinQty = 50, MaxQty = 200, Price = 45000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "328", Name = "Oli Gear 220", Type = "Gear", Category = "Oil", Unit = "Liter", BrandId = brands[4].Id, StockQty = 80, MinQty = 30, MaxQty = 150, Price = 55000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "329", Name = "Grease EP2", Type = "MP Grease", Category = "Grease", Unit = "KG", BrandId = brands[4].Id, StockQty = 20, MinQty = 10, MaxQty = 50, Price = 120000m, Location = "C-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "330", Name = "Filter Udara", Type = "Air Filter", Category = "Filter", Unit = "PCS", BrandId = brands[6].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 250000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "331", Name = "Filter Oli", Type = "Oil Filter", Category = "Filter", Unit = "PCS", BrandId = brands[6].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 200000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "332", Name = "Selang Hidrolik 1/2", Type = "Hydraulic", Category = "Hose", Unit = "Meter", BrandId = brands[6].Id, StockQty = 50, MinQty = 20, MaxQty = 100, Price = 75000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "333", Name = "Fitting 1/2", Type = "Hydraulic", Category = "Fitting", Unit = "PCS", BrandId = brands[6].Id, StockQty = 40, MinQty = 20, MaxQty = 80, Price = 35000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "334", Name = "Sensor Proximity", Type = "Inductive", Category = "Sensor", Unit = "PCS", BrandId = brands[7].Id, StockQty = 5, MinQty = 2, MaxQty = 10, Price = 650000m, Location = "E-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "335", Name = "Limit Switch", Type = "Mechanical", Category = "Sensor", Unit = "PCS", BrandId = brands[7].Id, StockQty = 8, MinQty = 3, MaxQty = 15, Price = 450000m, Location = "E-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "336", Name = "Relay 24V", Type = "Relay", Category = "Electrical", Unit = "PCS", BrandId = brands[7].Id, StockQty = 12, MinQty = 5, MaxQty = 20, Price = 180000m, Location = "E-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "432", Name = "Bearing 6207", Type = "DGBB", Category = "Bearing", Unit = "PCS", BrandId = brands[2].Id, StockQty = 10, MinQty = 5, MaxQty = 20, Price = 250000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "433", Name = "Bearing 6208", Type = "DGBB", Category = "Bearing", Unit = "PCS", BrandId = brands[2].Id, StockQty = 6, MinQty = 3, MaxQty = 12, Price = 320000m, Location = "A-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "434", Name = "Seal 35mm", Type = "Oil Seal", Category = "Seal", Unit = "PCS", BrandId = brands[3].Id, StockQty = 12, MinQty = 5, MaxQty = 20, Price = 105000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "435", Name = "Seal 40mm", Type = "Oil Seal", Category = "Seal", Unit = "PCS", BrandId = brands[3].Id, StockQty = 8, MinQty = 5, MaxQty = 20, Price = 115000m, Location = "A-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "436", Name = "Belt V B-50", Type = "V-Belt", Category = "Belt", Unit = "PCS", BrandId = brands[5].Id, StockQty = 6, MinQty = 3, MaxQty = 10, Price = 180000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "437", Name = "Belt V B-52", Type = "V-Belt", Category = "Belt", Unit = "PCS", BrandId = brands[5].Id, StockQty = 5, MinQty = 3, MaxQty = 10, Price = 195000m, Location = "B-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "438", Name = "Oli Way Lube 68", Type = "Way Lube", Category = "Oil", Unit = "Liter", BrandId = brands[4].Id, StockQty = 60, MinQty = 30, MaxQty = 120, Price = 52000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "439", Name = "Oli Spindle 32", Type = "Spindle", Category = "Oil", Unit = "Liter", BrandId = brands[4].Id, StockQty = 50, MinQty = 20, MaxQty = 100, Price = 62000m, Location = "C-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "440", Name = "Grease MP2", Type = "MP Grease", Category = "Grease", Unit = "KG", BrandId = brands[4].Id, StockQty = 25, MinQty = 10, MaxQty = 50, Price = 135000m, Location = "C-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "441", Name = "Filter Coolant", Type = "Coolant", Category = "Filter", Unit = "PCS", BrandId = brands[6].Id, StockQty = 8, MinQty = 4, MaxQty = 15, Price = 220000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "442", Name = "Filter Hidrolik", Type = "Hydraulic", Category = "Filter", Unit = "PCS", BrandId = brands[6].Id, StockQty = 8, MinQty = 4, MaxQty = 15, Price = 240000m, Location = "D-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "443", Name = "Selang Pneumatic 3/8", Type = "Pneumatic", Category = "Hose", Unit = "Meter", BrandId = brands[6].Id, StockQty = 40, MinQty = 20, MaxQty = 80, Price = 68000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "444", Name = "Fitting 3/8", Type = "Pneumatic", Category = "Fitting", Unit = "PCS", BrandId = brands[6].Id, StockQty = 35, MinQty = 15, MaxQty = 60, Price = 32000m, Location = "D-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "445", Name = "Sensor Pressure", Type = "Pressure", Category = "Sensor", Unit = "PCS", BrandId = brands[7].Id, StockQty = 4, MinQty = 2, MaxQty = 8, Price = 750000m, Location = "E-01", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "446", Name = "Solenoid Valve 24V", Type = "Solenoid", Category = "Electrical", Unit = "PCS", BrandId = brands[6].Id, StockQty = 6, MinQty = 2, MaxQty = 10, Price = 480000m, Location = "E-02", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "447", Name = "Contactor 25A", Type = "Contactor", Category = "Electrical", Unit = "PCS", BrandId = brands[6].Id, StockQty = 5, MinQty = 2, MaxQty = 8, Price = 320000m, Location = "E-02", CreatedBy = "system", CreatedDate = DateTime.Now });

        db.Parts.AddRange(parts);
        await db.SaveChangesAsync();

        // ============================================================
        // 14. BOM
        // ============================================================
        var bom = new List<AssetPart>();

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
        // 15. PM TASK TEMPLATES
        // ============================================================
        var pmTasks = new List<PmTaskTemplate>
        {
            new()
            {
                IdPm = "010100", LineSequence = 1, MachineSequence = 1, TaskSequence = 0,
                AssetId = assets[0].Id, UnitId = units[0].Id, SubUnitId = subUnits[0].Id,
                MethodId = metodes[0].Id, StandardId = standars[0].Id,
                WorkHourMinutes = 120, ManPower = 2,
                MachineState = MachineRunningState.OFF, PeriodeMonth = 6, StartMonth = 6,
                IsActive = true, CreatedBy = "system", CreatedDate = DateTime.Now
            },
            new()
            {
                IdPm = "010101", LineSequence = 1, MachineSequence = 1, TaskSequence = 1,
                AssetId = assets[0].Id, UnitId = units[0].Id, SubUnitId = subUnits[1].Id,
                MethodId = metodes[0].Id, StandardId = standars[3].Id,
                WorkHourMinutes = 120, ManPower = 2,
                MachineState = MachineRunningState.OFF, PeriodeMonth = 4, StartMonth = 4,
                IsActive = true, CreatedBy = "system", CreatedDate = DateTime.Now
            },
            new()
            {
                IdPm = "010102", LineSequence = 1, MachineSequence = 1, TaskSequence = 2,
                AssetId = assets[0].Id, UnitId = units[0].Id, SubUnitId = subUnits[2].Id,
                MethodId = metodes[1].Id, StandardId = standars[1].Id,
                WorkHourMinutes = 50, ManPower = 2,
                MachineState = MachineRunningState.ON, PeriodeMonth = 6, StartMonth = 1,
                IsActive = true, CreatedBy = "system", CreatedDate = DateTime.Now
            },
            new()
            {
                IdPm = "010103", LineSequence = 1, MachineSequence = 1, TaskSequence = 3,
                AssetId = assets[0].Id, UnitId = units[0].Id, SubUnitId = subUnits[3].Id,
                MethodId = metodes[2].Id, StandardId = standars[2].Id,
                WorkHourMinutes = 15, ManPower = 1,
                MachineState = MachineRunningState.OFF, PeriodeMonth = 12, StartMonth = 7,
                IsActive = true, CreatedBy = "system", CreatedDate = DateTime.Now
            },
            new()
            {
                IdPm = "010104", LineSequence = 1, MachineSequence = 1, TaskSequence = 4,
                AssetId = assets[0].Id, UnitId = units[0].Id, SubUnitId = subUnits[4].Id,
                MethodId = metodes[2].Id, StandardId = standars[2].Id,
                WorkHourMinutes = 15, ManPower = 1,
                MachineState = MachineRunningState.OFF, PeriodeMonth = 12, StartMonth = 8,
                IsActive = true, CreatedBy = "system", CreatedDate = DateTime.Now
            }
        };

        db.PmTaskTemplates.AddRange(pmTasks);
        await db.SaveChangesAsync();
    }
}