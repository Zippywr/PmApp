using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

public class ImportController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelImportService _excel;

    public ImportController(AppDbContext db, ExcelImportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public IActionResult Index() => View();

    // ============================================================
    // HELPER — set ViewBag untuk view Master generik
    // ============================================================
    private void SetConfig(string title, string icon, string actionName,
                           string[] headers, string templateFileName)
    {
        ViewBag.Title = title;
        ViewBag.Icon = icon;
        ViewBag.ActionName = actionName;
        ViewBag.TemplateActionName = "Template" + actionName;   // ← TAMBAH
        ViewBag.Headers = headers;
        ViewBag.TemplateFileName = templateFileName;
    }

    // ============================================================
    // ======================== PLANT =============================
    // ============================================================
    public IActionResult Plant() => View();
    public IActionResult TemplatePlant()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Plant", "Deskripsi" }, "Plant");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Plant.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Plant(IFormFile file)
    {
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View(); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Plant", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama Plant kosong"); continue; }
            if (await _db.Plants.AnyAsync(p => p.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }
            _db.Plants.Add(new Plant { Code = code, Name = name, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View();
    }

    // ============================================================
    // ======================== LINE ==============================
    // ============================================================
    public IActionResult Line()
    {
        SetConfig("Line", "bi-diagram-2", "Line",
            new[] { "Kode", "Nama Line", "Kode Plant", "Deskripsi" }, "Template_Line.xlsx");
        return View("Master");
    }
    public IActionResult TemplateLine()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Line", "Kode Plant", "Deskripsi" }, "Line");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Line.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Line(IFormFile file)
    {
        SetConfig("Line", "bi-diagram-2", "Line",
            new[] { "Kode", "Nama Line", "Kode Plant", "Deskripsi" }, "Template_Line.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var plants = await _db.Plants.ToListAsync();

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Line", "").Trim();
            var plantCode = row.GetValueOrDefault("Kode Plant", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama Line kosong"); continue; }

            var plant = plants.FirstOrDefault(p => p.Code == plantCode);
            if (plant == null)
            { errors.Add($"Baris {rowNum}: Kode Plant '{plantCode}' tidak ditemukan"); continue; }

            if (await _db.Lines.AnyAsync(l => l.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.Lines.Add(new Line { Code = code, Name = name, PlantId = plant.Id, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ======================== AREA ==============================
    // ============================================================
    public IActionResult Area()
    {
        SetConfig("Area", "bi-grid-3x3-gap", "Area",
            new[] { "Kode", "Nama Area", "Kode Line", "Deskripsi" }, "Template_Area.xlsx");
        return View("Master");
    }
    public IActionResult TemplateArea()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Area", "Kode Line", "Deskripsi" }, "Area");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Area.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Area(IFormFile file)
    {
        SetConfig("Area", "bi-grid-3x3-gap", "Area",
            new[] { "Kode", "Nama Area", "Kode Line", "Deskripsi" }, "Template_Area.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var lines = await _db.Lines.ToListAsync();

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Area", "").Trim();
            var lineCode = row.GetValueOrDefault("Kode Line", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama Area kosong"); continue; }

            var line = lines.FirstOrDefault(l => l.Code == lineCode);
            if (line == null)
            { errors.Add($"Baris {rowNum}: Kode Line '{lineCode}' tidak ditemukan"); continue; }

            if (await _db.Areas.AnyAsync(a => a.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.Areas.Add(new Area { Code = code, Name = name, LineId = line.Id, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ===================== PRODUCT ==============================
    // ============================================================
    public IActionResult Product()
    {
        SetConfig("Product", "bi-box", "Product",
            new[] { "Kode", "Nama Produk", "Deskripsi" }, "Template_Product.xlsx");
        return View("Master");
    }
    public IActionResult TemplateProduct()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Produk", "Deskripsi" }, "Product");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Product.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Product(IFormFile file)
    {
        SetConfig("Product", "bi-box", "Product",
            new[] { "Kode", "Nama Produk", "Deskripsi" }, "Template_Product.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Produk", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama Produk kosong"); continue; }

            if (await _db.Products.AnyAsync(p => p.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.Products.Add(new Product { Code = code, Name = name, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ====================== BRAND ===============================
    // ============================================================
    public IActionResult Brand()
    {
        SetConfig("Brand", "bi-award", "Brand",
            new[] { "Kode", "Nama Brand", "Deskripsi" }, "Template_Brand.xlsx");
        return View("Master");
    }
    public IActionResult TemplateBrand()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Brand", "Deskripsi" }, "Brand");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Brand.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Brand(IFormFile file)
    {
        SetConfig("Brand", "bi-award", "Brand",
            new[] { "Kode", "Nama Brand", "Deskripsi" }, "Template_Brand.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Brand", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama Brand kosong"); continue; }

            if (await _db.Brands.AnyAsync(b => b.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.Brands.Add(new Brand { Code = code, Name = name, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ MC CATEGORY ===============================
    // ============================================================
    public IActionResult McCategory()
    {
        SetConfig("Kategori Mesin", "bi-cpu", "McCategory",
            new[] { "Kode", "Nama Kategori", "Deskripsi" }, "Template_McCategory.xlsx");
        return View("Master");
    }

    public IActionResult TemplateMcCategory()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Kategori", "Deskripsi" }, "McCategory");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_McCategory.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> McCategory(IFormFile file)
    {
        SetConfig("Kategori Mesin", "bi-cpu", "McCategory",
            new[] { "Kode", "Nama Kategori", "Deskripsi" }, "Template_McCategory.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Kategori", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama kosong"); continue; }

            if (await _db.McCategories.AnyAsync(m => m.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.McCategories.Add(new McCategory { Code = code, Name = name, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ MACHINE FUNCTION ==========================
    // ============================================================
    public IActionResult MachineFunction()
    {
        SetConfig("Fungsi Mesin", "bi-tools", "MachineFunction",
            new[] { "Kode", "Nama Fungsi", "Deskripsi" }, "Template_MachineFunction.xlsx");
        return View("Master");
    }

    public IActionResult TemplateMachineFunction()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Fungsi", "Deskripsi" }, "MachineFunction");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_MachineFunction.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MachineFunction(IFormFile file)
    {
        SetConfig("Fungsi Mesin", "bi-tools", "MachineFunction",
            new[] { "Kode", "Nama Fungsi", "Deskripsi" }, "Template_MachineFunction.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Fungsi", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama kosong"); continue; }

            if (await _db.MachineFunctions.AnyAsync(m => m.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.MachineFunctions.Add(new MachineFunction { Code = code, Name = name, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ UNIT ======================================
    // ============================================================
    public IActionResult Unit()
    {
        SetConfig("Unit", "bi-boxes", "Unit",
            new[] { "Kode", "Nama Unit", "Deskripsi" }, "Template_Unit.xlsx");
        return View("Master");
    }

    public IActionResult TemplateUnit()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Unit", "Deskripsi" }, "Unit");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Unit.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unit(IFormFile file)
    {
        SetConfig("Unit", "bi-boxes", "Unit",
            new[] { "Kode", "Nama Unit", "Deskripsi" }, "Template_Unit.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Unit", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama kosong"); continue; }

            if (await _db.Units.AnyAsync(u => u.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.Units.Add(new Unit { Code = code, Name = name, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ SUB UNIT ==================================
    // ============================================================
    public IActionResult SubUnit()
    {
        SetConfig("Sub Unit", "bi-box-seam", "SubUnit",
            new[] { "Kode", "Nama Sub Unit", "Kode Unit", "Deskripsi" }, "Template_SubUnit.xlsx");
        return View("Master");
    }

    public IActionResult TemplateSubUnit()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Kode", "Nama Sub Unit", "Kode Unit", "Deskripsi" }, "SubUnit");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_SubUnit.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubUnit(IFormFile file)
    {
        SetConfig("Sub Unit", "bi-box-seam", "SubUnit",
            new[] { "Kode", "Nama Sub Unit", "Kode Unit", "Deskripsi" }, "Template_SubUnit.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var units = await _db.Units.ToListAsync();

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Sub Unit", "").Trim();
            var unitCode = row.GetValueOrDefault("Kode Unit", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama kosong"); continue; }

            var unit = units.FirstOrDefault(u => u.Code == unitCode);
            if (unit == null)
            { errors.Add($"Baris {rowNum}: Kode Unit '{unitCode}' tidak ditemukan"); continue; }

            if (await _db.SubUnits.AnyAsync(s => s.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            _db.SubUnits.Add(new SubUnit { Code = code, Name = name, UnitId = unit.Id, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ METODE ====================================
    // ============================================================
    public IActionResult Metode()
    {
        SetConfig("Metode", "bi-wrench", "Metode",
            new[] { "Kode", "Nama Metode", "Initial", "Machine State", "Deskripsi" }, "Template_Metode.xlsx");
        return View("Master");
    }

    public IActionResult TemplateMetode()
    {
        var bytes = _excel.GenerateTemplate(
            new[] { "Kode", "Nama Metode", "Initial", "Machine State", "Deskripsi" }, "Metode");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Metode.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Metode(IFormFile file)
    {
        SetConfig("Metode", "bi-wrench", "Metode",
            new[] { "Kode", "Nama Metode", "Initial", "Machine State", "Deskripsi" }, "Template_Metode.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Metode", "").Trim();
            var initial = row.GetValueOrDefault("Initial", "").Trim();
            var stateStr = row.GetValueOrDefault("Machine State", "OFF").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama kosong"); continue; }

            if (await _db.Metodes.AnyAsync(m => m.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            // Parse Machine State (ON/OFF/Either)
            var machineState = MachineRunningState.OFF;
            if (stateStr.Equals("ON", StringComparison.OrdinalIgnoreCase))
                machineState = MachineRunningState.ON;
            else if (stateStr.Equals("Either", StringComparison.OrdinalIgnoreCase))
                machineState = MachineRunningState.Either;

            _db.Metodes.Add(new Metode
            {
                Code = code,
                Name = name,
                Initial = initial.NullIfEmpty(),
                MachineState = machineState,
                Description = desc.NullIfEmpty(),
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ STANDAR ===================================
    // ============================================================
    public IActionResult Standar()
    {
        SetConfig("Standar", "bi-rulers", "Standar",
            new[] { "Nama Standar", "Deskripsi" }, "Template_Standar.xlsx");
        return View("Master");
    }

    public IActionResult TemplateStandar()
    {
        var bytes = _excel.GenerateTemplate(new[] { "Nama Standar", "Deskripsi" }, "Standar");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Standar.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Standar(IFormFile file)
    {
        SetConfig("Standar", "bi-rulers", "Standar",
            new[] { "Nama Standar", "Deskripsi" }, "Template_Standar.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        int success = 0; var errors = new List<string>(); int rowNum = 1;
        foreach (var row in rows)
        {
            rowNum++;
            var name = row.GetValueOrDefault("Nama Standar", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Nama Standar kosong"); continue; }

            if (await _db.Standars.AnyAsync(s => s.Name.ToLower() == name.ToLower()))
            { errors.Add($"Baris {rowNum}: Standar '{name}' sudah ada"); continue; }

            _db.Standars.Add(new Standar { Name = name, Description = desc.NullIfEmpty(), CreatedBy = "system", CreatedDate = DateTime.Now });
            success++;
        }
        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ ASSET (MESIN) =============================
    // ============================================================
    public IActionResult Asset()
    {
        SetConfig("Mesin", "bi-gear", "Asset",
            new[]
            {
                "Kode", "Nama Mesin", "OP No",
                "Kode Line", "Kode Area", "Kode Kategori", "Kode Fungsi",
                "Kode Brand", "Kode Product",
                "Model", "Serial Number", "Tahun", "Kapasitas",
                "Kritikalitas", "Deskripsi"
            }, "Template_Asset.xlsx");
        return View("Master");
    }

    public IActionResult TemplateAsset()
    {
        var headers = new[]
        {
            "Kode", "Nama Mesin", "OP No",
            "Kode Line", "Kode Area", "Kode Kategori", "Kode Fungsi",
            "Kode Brand", "Kode Product",
            "Model", "Serial Number", "Tahun", "Kapasitas",
            "Kritikalitas", "Deskripsi"
        };
        var bytes = _excel.GenerateTemplate(headers, "Asset");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Asset.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Asset(IFormFile file)
    {
        SetConfig("Mesin", "bi-gear", "Asset",
            new[]
            {
                "Kode", "Nama Mesin", "OP No",
                "Kode Line", "Kode Area", "Kode Kategori", "Kode Fungsi",
                "Kode Brand", "Kode Product",
                "Model", "Serial Number", "Tahun", "Kapasitas",
                "Kritikalitas", "Deskripsi"
            }, "Template_Asset.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        // Load master untuk validasi FK
        var lines = await _db.Lines.ToListAsync();
        var areas = await _db.Areas.ToListAsync();
        var mcCats = await _db.McCategories.ToListAsync();
        var functions = await _db.MachineFunctions.ToListAsync();
        var brands = await _db.Brands.ToListAsync();
        var products = await _db.Products.ToListAsync();

        int success = 0; var errors = new List<string>(); int rowNum = 1;

        foreach (var row in rows)
        {
            rowNum++;

            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Mesin", "").Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Kode atau Nama Mesin kosong"); continue; }

            if (await _db.Assets.AnyAsync(a => a.Code == code))
            { errors.Add($"Baris {rowNum}: Kode '{code}' sudah ada"); continue; }

            // Cek FK
            var lineCode = row.GetValueOrDefault("Kode Line", "").Trim();
            var areaCode = row.GetValueOrDefault("Kode Area", "").Trim();
            var mcCatCode = row.GetValueOrDefault("Kode Kategori", "").Trim();
            var funcCode = row.GetValueOrDefault("Kode Fungsi", "").Trim();
            var brandCode = row.GetValueOrDefault("Kode Brand", "").Trim();
            var productCode = row.GetValueOrDefault("Kode Product", "").Trim();

            var line = string.IsNullOrEmpty(lineCode) ? null : lines.FirstOrDefault(x => x.Code == lineCode);
            var area = string.IsNullOrEmpty(areaCode) ? null : areas.FirstOrDefault(x => x.Code == areaCode);
            var mcCat = string.IsNullOrEmpty(mcCatCode) ? null : mcCats.FirstOrDefault(x => x.Code == mcCatCode);
            var func = string.IsNullOrEmpty(funcCode) ? null : functions.FirstOrDefault(x => x.Code == funcCode);
            var brand = string.IsNullOrEmpty(brandCode) ? null : brands.FirstOrDefault(x => x.Code == brandCode);
            var product = string.IsNullOrEmpty(productCode) ? null : products.FirstOrDefault(x => x.Code == productCode);

            if (!string.IsNullOrEmpty(lineCode) && line == null)
            { errors.Add($"Baris {rowNum}: Kode Line '{lineCode}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(areaCode) && area == null)
            { errors.Add($"Baris {rowNum}: Kode Area '{areaCode}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(mcCatCode) && mcCat == null)
            { errors.Add($"Baris {rowNum}: Kode Kategori '{mcCatCode}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(funcCode) && func == null)
            { errors.Add($"Baris {rowNum}: Kode Fungsi '{funcCode}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(brandCode) && brand == null)
            { errors.Add($"Baris {rowNum}: Kode Brand '{brandCode}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(productCode) && product == null)
            { errors.Add($"Baris {rowNum}: Kode Product '{productCode}' tidak ditemukan"); continue; }

            // Parse Criticality
            var critStr = row.GetValueOrDefault("Kritikalitas", "Normal").Trim();
            var crit = Criticality.Normal;
            if (critStr.Equals("Critical", StringComparison.OrdinalIgnoreCase)) crit = Criticality.Critical;
            else if (critStr.Equals("High", StringComparison.OrdinalIgnoreCase)) crit = Criticality.High;
            else if (critStr.Equals("Low", StringComparison.OrdinalIgnoreCase)) crit = Criticality.Low;

            // Parse Tahun
            var yearStr = row.GetValueOrDefault("Tahun", "").Trim();
            int? yearMade = null;
            if (int.TryParse(yearStr, out var y)) yearMade = y;

            _db.Assets.Add(new Asset
            {
                Code = code,
                Name = name,
                OpNo = row.GetValueOrDefault("OP No", "").Trim().NullIfEmpty(),
                LineId = line?.Id,
                AreaId = area?.Id,
                McCategoryId = mcCat?.Id,
                MachineFunctionId = func?.Id,
                BrandId = brand?.Id,
                ProductId = product?.Id,
                Model = row.GetValueOrDefault("Model", "").Trim().NullIfEmpty(),
                SerialNumber = row.GetValueOrDefault("Serial Number", "").Trim().NullIfEmpty(),
                YearMade = yearMade,
                Capacity = row.GetValueOrDefault("Kapasitas", "").Trim().NullIfEmpty(),
                Criticality = crit,
                Description = row.GetValueOrDefault("Deskripsi", "").Trim().NullIfEmpty(),
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
            success++;
        }

        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ PART ======================================
    // ============================================================
    public IActionResult Part()
    {
        SetConfig("Part", "bi-box-seam", "Part",
            new[]
            {
                "Part No", "Nama Part", "Type", "Kategori",
                "Kode Brand", "Satuan", "Stok", "Min Stok", "Max Stok",
                "Lokasi", "Harga"
            }, "Template_Part.xlsx");
        return View("Master");
    }

    public IActionResult TemplatePart()
    {
        var headers = new[]
        {
            "Part No", "Nama Part", "Type", "Kategori",
            "Kode Brand", "Satuan", "Stok", "Min Stok", "Max Stok",
            "Lokasi", "Harga"
        };
        var bytes = _excel.GenerateTemplate(headers, "Part");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Part.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Part(IFormFile file)
    {
        SetConfig("Part", "bi-box-seam", "Part",
            new[]
            {
                "Part No", "Nama Part", "Type", "Kategori",
                "Kode Brand", "Satuan", "Stok", "Min Stok", "Max Stok",
                "Lokasi", "Harga"
            }, "Template_Part.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var brands = await _db.Brands.ToListAsync();

        int success = 0; var errors = new List<string>(); int rowNum = 1;

        foreach (var row in rows)
        {
            rowNum++;

            var partNo = row.GetValueOrDefault("Part No", "").Trim();
            var name = row.GetValueOrDefault("Nama Part", "").Trim();

            if (string.IsNullOrEmpty(partNo) || string.IsNullOrEmpty(name))
            { errors.Add($"Baris {rowNum}: Part No atau Nama kosong"); continue; }

            if (await _db.Parts.AnyAsync(p => p.PartNo == partNo))
            { errors.Add($"Baris {rowNum}: Part No '{partNo}' sudah ada"); continue; }

            // FK Brand
            var brandCode = row.GetValueOrDefault("Kode Brand", "").Trim();
            var brand = string.IsNullOrEmpty(brandCode) ? null : brands.FirstOrDefault(b => b.Code == brandCode);

            if (!string.IsNullOrEmpty(brandCode) && brand == null)
            { errors.Add($"Baris {rowNum}: Kode Brand '{brandCode}' tidak ditemukan"); continue; }

            // Parse angka
            int.TryParse(row.GetValueOrDefault("Stok", "0"), out var stockQty);
            int.TryParse(row.GetValueOrDefault("Min Stok", "0"), out var minQty);
            int.TryParse(row.GetValueOrDefault("Max Stok", "0"), out var maxQty);
            decimal.TryParse(row.GetValueOrDefault("Harga", "0"), out var price);

            _db.Parts.Add(new Part
            {
                PartNo = partNo,
                Name = name,
                Type = row.GetValueOrDefault("Type", "").Trim().NullIfEmpty(),
                Category = row.GetValueOrDefault("Kategori", "").Trim().NullIfEmpty(),
                BrandId = brand?.Id,
                Unit = row.GetValueOrDefault("Satuan", "PCS").Trim().NullIfEmpty() ?? "PCS",
                StockQty = stockQty,
                MinQty = minQty,
                MaxQty = maxQty,
                Location = row.GetValueOrDefault("Lokasi", "").Trim().NullIfEmpty(),
                Price = price,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
            success++;
        }

        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // ================ TASK PM ===================================
    // ============================================================
    public IActionResult PmTask()
    {
        SetConfig("Task PM", "bi-list-ul", "PmTask",
            new[]
            {
                "Kode Mesin", "Unit", "Sub Unit", "Metode", "Standar",
                "Waktu (menit)", "Man Power", "Kondisi Mesin",
                "Periode (bulan)", "Mulai Bulan", "Week", "Remark"
            }, "Template_PmTask.xlsx");
        return View("Master");
    }

    public IActionResult TemplatePmTask()
    {
        var headers = new[]
        {
            "Kode Mesin", "Unit", "Sub Unit", "Metode", "Standar",
            "Waktu (menit)", "Man Power", "Kondisi Mesin",
            "Periode (bulan)", "Mulai Bulan", "Week", "Remark"
        };
        var bytes = _excel.GenerateTemplate(headers, "TaskPM");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_PmTask.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PmTask(IFormFile file)
    {
        SetConfig("Task PM", "bi-list-ul", "PmTask",
            new[]
            {
                "Kode Mesin", "Unit", "Sub Unit", "Metode", "Standar",
                "Waktu (menit)", "Man Power", "Kondisi Mesin",
                "Periode (bulan)", "Mulai Bulan", "Week", "Remark"
            }, "Template_PmTask.xlsx");

        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var assets = await _db.Assets.ToListAsync();
        var units = await _db.Units.ToListAsync();
        var subUnits = await _db.SubUnits.ToListAsync();
        var metodes = await _db.Metodes.ToListAsync();
        var standars = await _db.Standars.ToListAsync();

        int success = 0; var errors = new List<string>(); int rowNum = 1;

        foreach (var row in rows)
        {
            rowNum++;

            var assetCode = row.GetValueOrDefault("Kode Mesin", "").Trim();

            if (string.IsNullOrEmpty(assetCode))
            { errors.Add($"Baris {rowNum}: Kode Mesin kosong"); continue; }

            var asset = assets.FirstOrDefault(a => a.Code == assetCode);
            if (asset == null)
            { errors.Add($"Baris {rowNum}: Kode Mesin '{assetCode}' tidak ditemukan"); continue; }

            // Cek FK (opsional)
            var unitName = row.GetValueOrDefault("Unit", "").Trim();
            var subUnitName = row.GetValueOrDefault("Sub Unit", "").Trim();
            var metodeName = row.GetValueOrDefault("Metode", "").Trim();
            var standarName = row.GetValueOrDefault("Standar", "").Trim();

            var unit = string.IsNullOrEmpty(unitName) ? null : units.FirstOrDefault(u => u.Name.ToLower() == unitName.ToLower());
            var subUnit = string.IsNullOrEmpty(subUnitName) ? null : subUnits.FirstOrDefault(s => s.Name.ToLower() == subUnitName.ToLower());
            var metode = string.IsNullOrEmpty(metodeName) ? null : metodes.FirstOrDefault(m => m.Name.ToLower() == metodeName.ToLower());
            var standar = string.IsNullOrEmpty(standarName) ? null : standars.FirstOrDefault(s => s.Name.ToLower() == standarName.ToLower());

            if (!string.IsNullOrEmpty(unitName) && unit == null)
            { errors.Add($"Baris {rowNum}: Unit '{unitName}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(subUnitName) && subUnit == null)
            { errors.Add($"Baris {rowNum}: Sub Unit '{subUnitName}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(metodeName) && metode == null)
            { errors.Add($"Baris {rowNum}: Metode '{metodeName}' tidak ditemukan"); continue; }
            if (!string.IsNullOrEmpty(standarName) && standar == null)
            { errors.Add($"Baris {rowNum}: Standar '{standarName}' tidak ditemukan"); continue; }

            // Parse angka
            int.TryParse(row.GetValueOrDefault("Waktu (menit)", "0"), out var wh);
            int.TryParse(row.GetValueOrDefault("Man Power", "1"), out var mp);
            if (mp < 1) mp = 1;
            int.TryParse(row.GetValueOrDefault("Periode (bulan)", "1"), out var periode);
            if (periode < 1) periode = 1;
            int.TryParse(row.GetValueOrDefault("Mulai Bulan", "1"), out var startMonth);
            if (startMonth < 1 || startMonth > 12) startMonth = 1;
            int.TryParse(row.GetValueOrDefault("Week", "0"), out var week);
            if (week < 0 || week > 4) week = 0;

            // Parse Machine State
            var stateStr = row.GetValueOrDefault("Kondisi Mesin", "OFF").Trim();
            var machineState = MachineRunningState.OFF;
            if (stateStr.Equals("ON", StringComparison.OrdinalIgnoreCase))
                machineState = MachineRunningState.ON;
            else if (stateStr.Equals("Either", StringComparison.OrdinalIgnoreCase))
                machineState = MachineRunningState.Either;

            // Auto-generate ID-PM
            var idPm = await GenerateUniqueIdPmAsync(asset.Id);

            // Auto-assign Week kalau 0
            if (week == 0)
            {
                var existingCount = await _db.PmTaskTemplates
                    .Where(t => t.AssetId == asset.Id && !t.IsDeleted)
                    .CountAsync();
                week = (existingCount % 4) + 1;
            }

            _db.PmTaskTemplates.Add(new PmTaskTemplate
            {
                IdPm = idPm,
                AssetId = asset.Id,
                UnitId = unit?.Id,
                SubUnitId = subUnit?.Id,
                MethodId = metode?.Id,
                StandardId = standar?.Id,
                WorkHourMinutes = wh,
                ManPower = mp,
                MachineState = machineState,
                PeriodeMonth = periode,
                StartMonth = startMonth,
                Week = week,
                Remark = row.GetValueOrDefault("Remark", "").Trim().NullIfEmpty(),
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = DateTime.Now
            });
            success++;
        }

        if (success > 0) await _db.SaveChangesAsync();
        ViewBag.Uploaded = true; ViewBag.SuccessCount = success; ViewBag.TotalRows = rows.Count; ViewBag.Errors = errors;
        return View("Master");
    }

    // ============================================================
    // HELPER — Generate ID-PM unik untuk import
    // ============================================================
    private async Task<string> GenerateUniqueIdPmAsync(int assetId)
    {
        // Format: {LineSeq:D2}{MachineSeq:D2}{TaskSeq:D2}
        // Ambil semua task di asset ini
        var existing = await _db.PmTaskTemplates
            .IgnoreQueryFilters()
            .Where(t => t.AssetId == assetId)
            .Select(t => t.IdPm)
            .ToListAsync();

        // Prefix berdasarkan assetId (contoh: 01, 02, dst)
        // Kita pakai 2 digit line + 2 digit machine = dari asset.Code atau urutan
        // Simplifikasi: pakai urutan global saja
        var allTasks = await _db.PmTaskTemplates
            .IgnoreQueryFilters()
            .Select(t => t.IdPm)
            .ToListAsync();

        // Cari nomor urut yang belum dipakai
        for (int i = 1; i <= 9999; i++)
        {
            var candidate = i.ToString("D6");
            if (!allTasks.Contains(candidate))
                return candidate;
        }

        return DateTime.Now.Ticks.ToString().Substring(10);
    }

    // ============================================================
    // HELPER — Baca file atau return error
    // ============================================================
    private List<Dictionary<string, string>>? ReadOrError(IFormFile file, out string? error)
    {
        error = null;
        if (file == null || file.Length == 0) { error = "File kosong atau tidak dipilih."; return null; }
        if (file.Length > 50 * 1024 * 1024) { error = "File terlalu besar. Max 50 MB."; return null; }
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx" && ext != ".xls") { error = "Hanya file Excel (.xlsx/.xls)."; return null; }
        try
        {
            using var stream = file.OpenReadStream();
            var rows = _excel.ReadExcel(stream);
            if (rows.Count == 0) { error = "File kosong atau format tidak sesuai."; return null; }
            return rows;
        }
        catch (Exception ex) { error = "Gagal baca file: " + ex.Message; return null; }
    }
}

// ============================================================
// EXTENSION — String NullIfEmpty
// ============================================================
public static class StringExtensions
{
    public static string? NullIfEmpty(this string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s;
}