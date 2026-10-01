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
        // 1. ASSETS — 2 mesin (sesuai gambar)
        // ============================================================
        var assets = new List<Asset>
        {
            new()
            {
                Code = "342131",
                Name = "CNC Mesin",
                Line = "Line-1",
                Criticality = Criticality.Critical,
                Description = "Mesin CNC 3-axis",
                Location = "Area Produksi A",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            },
            new()
            {
                Code = "432242",
                Name = "BUBUT Mesin",
                Line = "Line-1",
                Criticality = Criticality.High,
                Description = "Mesin Bubut CNC",
                Location = "Area Produksi B",
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            }
        };
        db.Assets.AddRange(assets);

        // ============================================================
        // 2. PARTS — 32 part (16 untuk CNC + 16 untuk BUBUT)
        // ============================================================
        var parts = new List<Part>();

        // --- Part 321–336 (untuk CNC) ---
        parts.Add(new() { PartNo = "321", Name = "Bearing 6205", Category = "Bearing", Brand = "SKF", Unit = "PCS", Description = "Deep groove ball bearing", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "322", Name = "Bearing 6206", Category = "Bearing", Brand = "SKF", Unit = "PCS", Description = "Deep groove ball bearing", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "323", Name = "Seal 25mm", Category = "Seal", Brand = "NOK", Unit = "PCS", Description = "Oil seal 25mm", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "324", Name = "Seal 30mm", Category = "Seal", Brand = "NOK", Unit = "PCS", Description = "Oil seal 30mm", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "325", Name = "Belt V A-40", Category = "Belt", Brand = "Mitsuboshi", Unit = "PCS", Description = "V-belt A-40", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "326", Name = "Belt V A-42", Category = "Belt", Brand = "Mitsuboshi", Unit = "PCS", Description = "V-belt A-42", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "327", Name = "Oli Hydraulic 46", Category = "Oil", Brand = "Shell", Unit = "Liter", Description = "Hydraulic oil ISO 46", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "328", Name = "Oli Gear 220", Category = "Oil", Brand = "Shell", Unit = "Liter", Description = "Gear oil ISO 220", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "329", Name = "Grease EP2", Category = "Grease", Brand = "Shell", Unit = "KG", Description = "Lithium grease EP2", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "330", Name = "Filter Udara", Category = "Filter", Brand = "Festo", Unit = "PCS", Description = "Air filter element", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "331", Name = "Filter Oli", Category = "Filter", Brand = "Festo", Unit = "PCS", Description = "Oil filter element", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "332", Name = "Selang Hidrolik 1/2", Category = "Hose", Brand = "Parker", Unit = "Meter", Description = "Hydraulic hose 1/2 inci", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "333", Name = "Fitting 1/2", Category = "Fitting", Brand = "Parker", Unit = "PCS", Description = "Hydraulic fitting 1/2", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "334", Name = "Sensor Proximity", Category = "Sensor", Brand = "Omron", Unit = "PCS", Description = "Inductive proximity sensor", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "335", Name = "Limit Switch", Category = "Sensor", Brand = "Omron", Unit = "PCS", Description = "Mechanical limit switch", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "336", Name = "Relay 24V", Category = "Electrical", Brand = "Omron", Unit = "PCS", Description = "24VDC relay", CreatedBy = "system", CreatedDate = DateTime.Now });

        // --- Part 432–447 (untuk BUBUT) ---
        parts.Add(new() { PartNo = "432", Name = "Bearing 6207", Category = "Bearing", Brand = "SKF", Unit = "PCS", Description = "Deep groove ball bearing", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "433", Name = "Bearing 6208", Category = "Bearing", Brand = "SKF", Unit = "PCS", Description = "Deep groove ball bearing", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "434", Name = "Seal 35mm", Category = "Seal", Brand = "NOK", Unit = "PCS", Description = "Oil seal 35mm", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "435", Name = "Seal 40mm", Category = "Seal", Brand = "NOK", Unit = "PCS", Description = "Oil seal 40mm", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "436", Name = "Belt V B-50", Category = "Belt", Brand = "Mitsuboshi", Unit = "PCS", Description = "V-belt B-50", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "437", Name = "Belt V B-52", Category = "Belt", Brand = "Mitsuboshi", Unit = "PCS", Description = "V-belt B-52", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "438", Name = "Oli Way Lube 68", Category = "Oil", Brand = "Shell", Unit = "Liter", Description = "Way lube ISO 68", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "439", Name = "Oli Spindle 32", Category = "Oil", Brand = "Shell", Unit = "Liter", Description = "Spindle oil ISO 32", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "440", Name = "Grease MP2", Category = "Grease", Brand = "Mobil", Unit = "KG", Description = "Multi-purpose grease MP2", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "441", Name = "Filter Coolant", Category = "Filter", Brand = "Festo", Unit = "PCS", Description = "Coolant filter element", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "442", Name = "Filter Hidrolik", Category = "Filter", Brand = "Festo", Unit = "PCS", Description = "Hydraulic filter element", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "443", Name = "Selang Pneumatic 3/8", Category = "Hose", Brand = "SMC", Unit = "Meter", Description = "Pneumatic hose 3/8 inci", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "444", Name = "Fitting 3/8", Category = "Fitting", Brand = "SMC", Unit = "PCS", Description = "Pneumatic fitting 3/8", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "445", Name = "Sensor Pressure", Category = "Sensor", Brand = "SMC", Unit = "PCS", Description = "Pressure sensor 0-1 MPa", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "446", Name = "Solenoid Valve 24V", Category = "Electrical", Brand = "SMC", Unit = "PCS", Description = "Solenoid valve 24VDC", CreatedBy = "system", CreatedDate = DateTime.Now });
        parts.Add(new() { PartNo = "447", Name = "Contactor 25A", Category = "Electrical", Brand = "Schneider", Unit = "PCS", Description = "Contactor 25A", CreatedBy = "system", CreatedDate = DateTime.Now });

        db.Parts.AddRange(parts);

        

        // Simpan dulu supaya Id terisi
        await db.SaveChangesAsync();

        // ============================================================
        // 4. ASSET PARTS (BOM) — sesuai gambar
        // ============================================================
        var bom = new List<AssetPart>();

        // --- CNC-342131: 16 part (321–336) ---
        for (int i = 0; i < 16; i++)
        {
            bom.Add(new AssetPart
            {
                AssetId = assets[0].Id,
                PartId = parts[i].Id,
                Quantity = 1,
                Position = i < 4 ? "Spindle" : (i < 8 ? "Axis" : (i < 12 ? "Coolant" : "Electrical")),
                Notes = "Maintenance 6w",
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        // --- BUBUT-432242: 16 part (432–447) ---
        for (int i = 16; i < 32; i++)
        {
            bom.Add(new AssetPart
            {
                AssetId = assets[1].Id,
                PartId = parts[i].Id,
                Quantity = 1,
                Position = i < 20 ? "Chuck" : (i < 24 ? "Tailstock" : (i < 28 ? "Turret" : "Electrical")),
                Notes = "Maintenance 6w",
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
        }

        db.AssetParts.AddRange(bom);

        await db.SaveChangesAsync();
    }
}