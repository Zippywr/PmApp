using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Data;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;
using PmApp.Web.Services;

namespace PmApp.Web.Controllers;

[Authorize(Roles = "Admin,TS")]
public class ImportController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelImportService _excel;
    private readonly PmIdGeneratorService _pmIdGenerator;

    public ImportController(AppDbContext db, ExcelImportService excel, PmIdGeneratorService pmIdGenerator)
    {
        _db = db;
        _excel = excel;
        _pmIdGenerator = pmIdGenerator;
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
    [HttpGet]
    public IActionResult Line()
    {
        ConfigureLine();
        return View("Master");
    }

    private void ConfigureLine() => SetConfig("Line", "bi-diagram-2", "Line",
        new[] { "Kode", "Nama Line", "Kode Plant", "Kode Product", "Deskripsi" }, "Template_Line.xlsx");

    [HttpGet]
    public IActionResult TemplateLine()
    {
        var bytes = _excel.GenerateTemplate(
            new[] { "Kode", "Nama Line", "Kode Plant", "Kode Product", "Deskripsi" }, "Line");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Line.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Line(IFormFile file)
    {
        ConfigureLine();
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var plants = await _db.Plants.AsNoTracking().ToListAsync();
        var products = await _db.Products.AsNoTracking().ToListAsync();
        var codes = new HashSet<string>(await _db.Lines.IgnoreQueryFilters()
            .Select(x => x.Code).ToListAsync(), StringComparer.OrdinalIgnoreCase);
        var pending = new List<Line>();
        var errors = new List<string>();
        int rowNum = 1;

        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Line", "").Trim();
            var plantCode = row.GetValueOrDefault("Kode Plant", "").Trim();
            var productCode = row.GetValueOrDefault("Kode Product", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(plantCode) || string.IsNullOrWhiteSpace(productCode))
            { errors.Add($"Baris {rowNum}: Kode, Nama Line, Kode Plant, dan Kode Product wajib diisi."); continue; }
            if (code.Length > 50 || name.Length > 200 || desc.Length > 500)
            { errors.Add($"Baris {rowNum}: Panjang kolom melebihi batas."); continue; }
            var plant = plants.FirstOrDefault(p => p.Code.Equals(plantCode, StringComparison.OrdinalIgnoreCase));
            if (plant == null)
            { errors.Add($"Baris {rowNum}: Kode Plant '{plantCode}' tidak ditemukan."); continue; }
            var product = products.FirstOrDefault(p => p.PlantId == plant.Id && p.Code.Equals(productCode, StringComparison.OrdinalIgnoreCase));
            if (product == null)
            { errors.Add($"Baris {rowNum}: Kode Product '{productCode}' tidak terdaftar pada Plant '{plantCode}'."); continue; }
            if (!codes.Add(code))
            { errors.Add($"Baris {rowNum}: Kode Line '{code}' duplikat atau pernah digunakan."); continue; }
            pending.Add(new Line { Code = code, Name = name, PlantId = plant.Id, ProductId = product.Id, Description = desc.NullIfEmpty() });
        }

        int success = await SaveBatchAsync(pending, errors);
        ReportImport(rows.Count, success, errors);
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
    [HttpGet]
    public IActionResult Product()
    {
        ConfigureProduct();
        return View("Master");
    }

    private void ConfigureProduct() => SetConfig("Product", "bi-box", "Product",
        new[] { "Kode", "Nama Produk", "Kode Plant", "Deskripsi" }, "Template_Product.xlsx");

    [HttpGet]
    public IActionResult TemplateProduct()
    {
        var bytes = _excel.GenerateTemplate(
            new[] { "Kode", "Nama Produk", "Kode Plant", "Deskripsi" }, "Product");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Product.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Product(IFormFile file)
    {
        ConfigureProduct();
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var plants = await _db.Plants.AsNoTracking().ToListAsync();
        var codes = new HashSet<string>(await _db.Products.IgnoreQueryFilters()
            .Select(x => x.Code).ToListAsync(), StringComparer.OrdinalIgnoreCase);
        var pending = new List<Product>();
        var errors = new List<string>();
        int rowNum = 1;

        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var name = row.GetValueOrDefault("Nama Produk", "").Trim();
            var plantCode = row.GetValueOrDefault("Kode Plant", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(plantCode))
            { errors.Add($"Baris {rowNum}: Kode, Nama Produk, dan Kode Plant wajib diisi."); continue; }
            if (code.Length > 50 || name.Length > 200 || desc.Length > 500)
            { errors.Add($"Baris {rowNum}: Panjang kolom melebihi batas."); continue; }
            var plant = plants.FirstOrDefault(p => p.Code.Equals(plantCode, StringComparison.OrdinalIgnoreCase));
            if (plant == null)
            { errors.Add($"Baris {rowNum}: Kode Plant '{plantCode}' tidak ditemukan."); continue; }
            if (!codes.Add(code))
            { errors.Add($"Baris {rowNum}: Kode Product '{code}' duplikat atau pernah digunakan."); continue; }
            pending.Add(new Product { Code = code, Name = name, PlantId = plant.Id, Description = desc.NullIfEmpty() });
        }

        int success = await SaveBatchAsync(pending, errors);
        ReportImport(rows.Count, success, errors);
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
    // ================ UNIT (TANPA INPUT CODE) ====================
    // ============================================================
    private static readonly string[] UnitHeaders = { "Nama Unit", "Deskripsi" };

    [HttpGet]
    public IActionResult Unit()
    {
        ConfigureUnit();
        return View("Master");
    }

    private void ConfigureUnit() => SetConfig("Unit", "bi-boxes", "Unit",
        UnitHeaders, "Template_Unit.xlsx");

    [HttpGet]
    public IActionResult TemplateUnit()
    {
        var bytes = _excel.GenerateTemplate(UnitHeaders, "Unit");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Unit.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unit(IFormFile file)
    {
        ConfigureUnit();
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var existing = new HashSet<string>(
            await _db.Units.AsNoTracking().Select(u => u.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);
        var pending = new List<Unit>();
        var errors = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var name = row.GetValueOrDefault("Nama Unit", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();
            int number = i + 2;

            if (string.IsNullOrWhiteSpace(name) || name.Length > 200 || desc.Length > 500)
            { errors.Add($"Baris {number}: Nama Unit wajib diisi (maks. 200 karakter), Deskripsi maks. 500 karakter."); continue; }
            if (!existing.Add(name))
            { errors.Add($"Baris {number}: Unit '{name}' sudah ada atau berulang dalam file."); continue; }

            pending.Add(new Unit
            {
                Code = "AUTO-" + Guid.NewGuid().ToString("N"),
                Name = name,
                Description = desc.NullIfEmpty()
            });
        }

        if (errors.Count > 0)
        {
            errors.Insert(0, "Import dibatalkan. Perbaiki baris yang salah lalu upload kembali.");
            ReportImport(rows.Count, 0, errors);
            return View("Master");
        }

        int success = await SaveBatchAsync(pending, errors);
        ReportImport(rows.Count, success, errors);
        return View("Master");
    }

    // ============================================================
    // ================ SUB UNIT (BERDASARKAN NAMA UNIT) ===========
    // ============================================================
    private static readonly string[] SubUnitHeaders =
        { "Nama Unit", "Nama Sub Unit", "Deskripsi" };

    [HttpGet]
    public IActionResult SubUnit()
    {
        ConfigureSubUnit();
        return View("Master");
    }

    private void ConfigureSubUnit() => SetConfig("Sub Unit", "bi-box-seam", "SubUnit",
        SubUnitHeaders, "Template_SubUnit.xlsx");

    [HttpGet]
    public IActionResult TemplateSubUnit()
    {
        var bytes = _excel.GenerateTemplate(SubUnitHeaders, "SubUnit");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_SubUnit.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubUnit(IFormFile file)
    {
        ConfigureSubUnit();
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var units = await _db.Units.AsNoTracking().ToListAsync();
        var existing = new HashSet<string>(
            (await _db.SubUnits.AsNoTracking()
                .Select(s => new { s.UnitId, s.Name }).ToListAsync())
                .Select(s => $"{s.UnitId}\u001f{s.Name}"),
            StringComparer.OrdinalIgnoreCase);
        var pending = new List<SubUnit>();
        var errors = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int number = i + 2;
            var unitName = row.GetValueOrDefault("Nama Unit", "").Trim();
            var name = row.GetValueOrDefault("Nama Sub Unit", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrWhiteSpace(unitName) || string.IsNullOrWhiteSpace(name) ||
                name.Length > 200 || desc.Length > 500)
            { errors.Add($"Baris {number}: Nama Unit dan Nama Sub Unit wajib diisi. Batas nama 200 dan deskripsi 500 karakter."); continue; }

            var matches = units.Where(u =>
                u.Name.Equals(unitName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count != 1)
            { errors.Add($"Baris {number}: Nama Unit '{unitName}' tidak ditemukan atau ambigu. Import Unit terlebih dahulu."); continue; }

            var unit = matches[0];
            if (!existing.Add($"{unit.Id}\u001f{name}"))
            { errors.Add($"Baris {number}: Sub Unit '{name}' sudah terdaftar atau berulang pada Unit '{unitName}'."); continue; }

            pending.Add(new SubUnit
            {
                Code = "AUTO-" + Guid.NewGuid().ToString("N"),
                UnitId = unit.Id,
                Name = name,
                Description = desc.NullIfEmpty()
            });
        }

        if (errors.Count > 0)
        {
            errors.Insert(0, "Import dibatalkan. Perbaiki baris yang salah lalu upload kembali.");
            ReportImport(rows.Count, 0, errors);
            return View("Master");
        }

        int success = await SaveBatchAsync(pending, errors);
        ReportImport(rows.Count, success, errors);
        return View("Master");
    }

    // ============================================================
    // ================ METODE (TANPA INPUT CODE) ==================
    // ============================================================
    private static readonly string[] MetodeHeaders =
        { "Nama Metode", "Initial", "Machine State", "Deskripsi" };

    [HttpGet]
    public IActionResult Metode()
    {
        ConfigureMetode();
        return View("Master");
    }

    private void ConfigureMetode() => SetConfig("Metode", "bi-wrench", "Metode",
        MetodeHeaders, "Template_Metode.xlsx");

    [HttpGet]
    public IActionResult TemplateMetode()
    {
        var bytes = _excel.GenerateTemplate(MetodeHeaders, "Metode");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Metode.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Metode(IFormFile file)
    {
        ConfigureMetode();
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var existing = new HashSet<string>(
            await _db.Metodes.AsNoTracking().Select(m => m.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);
        var pending = new List<Metode>();
        var errors = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int number = i + 2;
            var name = row.GetValueOrDefault("Nama Metode", "").Trim();
            var initial = row.GetValueOrDefault("Initial", "").Trim();
            var stateText = row.GetValueOrDefault("Machine State", "OFF").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 200 || desc.Length > 500)
            { errors.Add($"Baris {number}: Nama Metode wajib diisi (maks. 200 karakter), deskripsi maks. 500 karakter."); continue; }

            if (string.IsNullOrEmpty(initial))
            {
                var letters = new string(name.Where(char.IsLetter).ToArray());
                initial = letters.Length > 3 ? letters[..3].ToUpperInvariant() : letters.ToUpperInvariant();
            }
            if (initial.Length > 20)
            { errors.Add($"Baris {number}: Initial maksimal 20 karakter."); continue; }

            if (string.IsNullOrEmpty(stateText)) stateText = "OFF";
            if (!Enum.TryParse<MachineRunningState>(stateText, true, out var state) ||
                !Enum.IsDefined(typeof(MachineRunningState), state))
            { errors.Add($"Baris {number}: Machine State '{stateText}' tidak valid (ON/OFF/Either)."); continue; }

            if (!existing.Add(name))
            { errors.Add($"Baris {number}: Metode '{name}' sudah ada atau berulang dalam file."); continue; }

            pending.Add(new Metode
            {
                Code = "AUTO-" + Guid.NewGuid().ToString("N"),
                Name = name,
                Initial = initial.NullIfEmpty(),
                MachineState = state,
                Description = desc.NullIfEmpty()
            });
        }

        if (errors.Count > 0)
        {
            errors.Insert(0, "Import dibatalkan. Perbaiki baris yang salah lalu upload kembali.");
            ReportImport(rows.Count, 0, errors);
            return View("Master");
        }

        int success = await SaveBatchAsync(pending, errors);
        ReportImport(rows.Count, success, errors);
        return View("Master");
    }

    // ============================================================
    // ================ STANDAR (BERDASARKAN NAMA) =================
    // ============================================================
    private static readonly string[] StandarHeaders =
        { "Nama Standar", "Deskripsi" };

    [HttpGet]
    public IActionResult Standar()
    {
        ConfigureStandar();
        return View("Master");
    }

    private void ConfigureStandar() => SetConfig("Standar", "bi-rulers", "Standar",
        StandarHeaders, "Template_Standar.xlsx");

    [HttpGet]
    public IActionResult TemplateStandar()
    {
        var bytes = _excel.GenerateTemplate(StandarHeaders, "Standar");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Standar.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Standar(IFormFile file)
    {
        ConfigureStandar();
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var existing = new HashSet<string>(
            await _db.Standars.AsNoTracking().Select(s => s.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);
        var pending = new List<Standar>();
        var errors = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int number = i + 2;
            var name = row.GetValueOrDefault("Nama Standar", "").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 500 || desc.Length > 500)
            { errors.Add($"Baris {number}: Nama Standar wajib diisi (maks. 500 karakter), deskripsi maks. 500 karakter."); continue; }
            if (!existing.Add(name))
            { errors.Add($"Baris {number}: Standar '{name}' sudah ada atau berulang dalam file."); continue; }

            pending.Add(new Standar { Name = name, Description = desc.NullIfEmpty() });
        }

        if (errors.Count > 0)
        {
            errors.Insert(0, "Import dibatalkan. Perbaiki baris yang salah lalu upload kembali.");
            ReportImport(rows.Count, 0, errors);
            return View("Master");
        }

        int success = await SaveBatchAsync(pending, errors);
        ReportImport(rows.Count, success, errors);
        return View("Master");
    }

    // ============================================================
    // ================ ASSET (MESIN) =============================
    // ============================================================
    private static readonly string[] AssetHeaders =
    {
        "Kode", "OP No", "Kode Plant", "Kode Product", "Kode Line", "Kode Kategori",
        "Kode Fungsi", "Kode Brand", "Model", "Serial Number", "Tahun",
        "Kapasitas", "Kritikalitas", "Deskripsi"
    };

    [HttpGet]
    public IActionResult Asset()
    {
        SetConfig("Mesin", "bi-gear", "Asset", AssetHeaders, "Template_Asset.xlsx");
        return View("Master");
    }

    [HttpGet]
    public IActionResult TemplateAsset()
    {
        var bytes = _excel.GenerateTemplate(AssetHeaders, "Asset");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Asset.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Asset(IFormFile file)
    {
        SetConfig("Mesin", "bi-gear", "Asset", AssetHeaders, "Template_Asset.xlsx");
        var rows = ReadOrError(file, out var err);
        if (rows == null) { ViewBag.Error = err; return View("Master"); }

        var plants = await _db.Plants.AsNoTracking().ToListAsync();
        var products = await _db.Products.AsNoTracking().ToListAsync();
        var lines = await _db.Lines.AsNoTracking().ToListAsync();
        var categories = await _db.McCategories.AsNoTracking().ToListAsync();
        var functions = await _db.MachineFunctions.AsNoTracking().ToListAsync();
        var brands = await _db.Brands.AsNoTracking().ToListAsync();
        var codes = new HashSet<string>(await _db.Assets.IgnoreQueryFilters()
            .Select(a => a.Code).ToListAsync(), StringComparer.OrdinalIgnoreCase);
        var pending = new List<Asset>();
        var errors = new List<string>();
        int rowNum = 1;

        foreach (var row in rows)
        {
            rowNum++;
            var code = row.GetValueOrDefault("Kode", "").Trim();
            var opNo = row.GetValueOrDefault("OP No", "").Trim();
            var plantCode = row.GetValueOrDefault("Kode Plant", "").Trim();
            var productCode = row.GetValueOrDefault("Kode Product", "").Trim();
            var lineCode = row.GetValueOrDefault("Kode Line", "").Trim();
            var catCode = row.GetValueOrDefault("Kode Kategori", "").Trim();
            var funcCode = row.GetValueOrDefault("Kode Fungsi", "").Trim();
            var brandCode = row.GetValueOrDefault("Kode Brand", "").Trim();
            var model = row.GetValueOrDefault("Model", "").Trim();
            var serial = row.GetValueOrDefault("Serial Number", "").Trim();
            var yearString = row.GetValueOrDefault("Tahun", "").Trim();
            var capacity = row.GetValueOrDefault("Kapasitas", "").Trim();
            var criticalityString = row.GetValueOrDefault("Kritikalitas", "Normal").Trim();
            var desc = row.GetValueOrDefault("Deskripsi", "").Trim();

            if (new[] { code, opNo, plantCode, productCode, lineCode, catCode }.Any(string.IsNullOrWhiteSpace))
            { errors.Add($"Baris {rowNum}: Kode, OP No, Plant, Product, Line, dan Kategori wajib diisi."); continue; }
            if (code.Length > 50 || opNo.Length > 50 || model.Length > 100 || serial.Length > 100 || capacity.Length > 100 || desc.Length > 500)
            { errors.Add($"Baris {rowNum}: Panjang kolom melebihi batas."); continue; }
            var plant = plants.FirstOrDefault(p => p.Code.Equals(plantCode, StringComparison.OrdinalIgnoreCase));
            if (plant == null)
            { errors.Add($"Baris {rowNum}: Plant '{plantCode}' tidak ditemukan."); continue; }
            var product = products.FirstOrDefault(p => p.PlantId == plant.Id && p.Code.Equals(productCode, StringComparison.OrdinalIgnoreCase));
            if (product == null)
            { errors.Add($"Baris {rowNum}: Product '{productCode}' tidak ditemukan pada Plant '{plantCode}'."); continue; }
            var line = lines.FirstOrDefault(l => l.ProductId == product.Id && l.PlantId == plant.Id && l.Code.Equals(lineCode, StringComparison.OrdinalIgnoreCase));
            if (line == null)
            { errors.Add($"Baris {rowNum}: Line '{lineCode}' tidak ditemukan pada Product '{productCode}'."); continue; }
            var category = categories.FirstOrDefault(c => c.Code.Equals(catCode, StringComparison.OrdinalIgnoreCase));
            if (category == null)
            { errors.Add($"Baris {rowNum}: Kategori mesin '{catCode}' tidak ditemukan."); continue; }
            var function = string.IsNullOrWhiteSpace(funcCode) ? null : functions.FirstOrDefault(f => f.Code.Equals(funcCode, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(funcCode) && function == null)
            { errors.Add($"Baris {rowNum}: Fungsi mesin '{funcCode}' tidak ditemukan."); continue; }
            var brand = string.IsNullOrWhiteSpace(brandCode) ? null : brands.FirstOrDefault(b => b.Code.Equals(brandCode, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(brandCode) && brand == null)
            { errors.Add($"Baris {rowNum}: Brand '{brandCode}' tidak ditemukan."); continue; }
            int? year = null;
            if (!string.IsNullOrWhiteSpace(yearString))
            {
                if (!int.TryParse(yearString, out var y) || y < 1900 || y > 2100)
                { errors.Add($"Baris {rowNum}: Tahun pembuatan tidak valid."); continue; }
                year = y;
            }
            var criticality = Criticality.Normal;
            if (!string.IsNullOrWhiteSpace(criticalityString) && !Enum.TryParse<Criticality>(criticalityString, true, out criticality))
            { errors.Add($"Baris {rowNum}: Kritikalitas '{criticalityString}' tidak valid."); continue; }
            if (!codes.Add(code))
            { errors.Add($"Baris {rowNum}: Kode Mesin '{code}' duplikat atau pernah digunakan."); continue; }

            pending.Add(new Asset
            {
                Code = code,
                Name = code,
                OpNo = opNo,
                LineId = line.Id,
                ProductId = product.Id,
                AreaId = null,
                McCategoryId = category.Id,
                MachineFunctionId = function?.Id,
                BrandId = brand?.Id,
                Model = model.NullIfEmpty(),
                SerialNumber = serial.NullIfEmpty(),
                YearMade = year,
                Capacity = capacity.NullIfEmpty(),
                Criticality = criticality,
                Description = desc.NullIfEmpty(),
                IsActive = true
            });
        }

        int success = await SaveBatchAsync(pending, errors);
        ReportImport(rows.Count, success, errors);
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
    // ================ TASK PM — HYBRID ID-PM =====================
    // ============================================================
    private static readonly string[] PmTaskHeaders =
    {
        "Kode Mesin", "Task Sequence", "Unit", "Sub Unit", "Metode", "Standar",
        "Waktu (menit)", "Man Power", "Kondisi Mesin",
        "Periode (bulan)", "Mulai Bulan", "Week", "Remark"
    };

    [HttpGet]
    public IActionResult PmTask()
    {
        ConfigurePmTask();
        return View("Master");
    }

    private void ConfigurePmTask() => SetConfig(
        "Task PM", "bi-list-ul", "PmTask", PmTaskHeaders, "Template_PmTask.xlsx");

    [HttpGet]
    public IActionResult TemplatePmTask()
    {
        var bytes = _excel.GenerateTemplate(PmTaskHeaders, "TaskPM");
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Template_PmTask.xlsx");
    }

    private sealed class PmTaskImportRow
    {
        public int RowNumber { get; init; }
        public int AssetId { get; init; }
        public string UnitName { get; init; } = "";
        public string SubUnitName { get; init; } = "";
        public string MethodName { get; init; } = "";
        public string StandardName { get; init; } = "";
        public int WorkHourMinutes { get; init; }
        public int ManPower { get; init; }
        public int PeriodeMonth { get; init; }
        public int StartMonth { get; init; }
        public int Week { get; init; }
        public MachineRunningState MachineState { get; init; }
        public string? Remark { get; init; }
        public int? ManualSequence { get; init; }
        public string IdPm { get; set; } = "";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PmTask(IFormFile file)
    {
        ConfigurePmTask();
        var rows = ReadOrError(file, out var readError);
        if (rows == null)
        {
            ViewBag.Error = readError;
            return View("Master");
        }

        var errors = new List<string>();
        var drafts = new List<PmTaskImportRow>();

        var assets = await _db.Assets.AsNoTracking().ToListAsync();


        // TAHAP 1: Validasi seluruh isi Excel sebelum menyimpan apa pun.
        for (int index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            int rowNumber = index + 2; // Baris 1 = header
            string Get(string key, string fallback = "") =>
                row.GetValueOrDefault(key, fallback).Trim();

            var assetCode = Get("Kode Mesin");
            var asset = assets.FirstOrDefault(a =>
                a.Code.Equals(assetCode, StringComparison.OrdinalIgnoreCase));
            if (asset == null || !asset.IsActive)
            {
                errors.Add($"Baris {rowNumber}: Mesin '{assetCode}' tidak ditemukan atau nonaktif.");
                continue;
            }

            var unitName = Get("Unit");
            var subUnitName = Get("Sub Unit");
            var methodName = Get("Metode");
            var standardName = Get("Standar");
            if (string.IsNullOrWhiteSpace(unitName) || string.IsNullOrWhiteSpace(subUnitName) ||
                unitName.Length > 200 || subUnitName.Length > 200)
            {
                errors.Add($"Baris {rowNumber}: Unit dan Sub Unit wajib diisi (maksimal 200 karakter).");
                continue;
            }
            if (string.IsNullOrWhiteSpace(methodName) ||
                string.IsNullOrWhiteSpace(standardName) ||
                methodName.Length > 200 || standardName.Length > 500)
            {
                errors.Add($"Baris {rowNumber}: Metode/Standar wajib diisi dan panjangnya harus sesuai batas.");
                continue;
            }

            string sequenceText = Get("Task Sequence");
            int? manualSequence = null;
            if (!string.IsNullOrEmpty(sequenceText))
            {
                if (!int.TryParse(sequenceText, out int seq) || seq < 1 || seq > 99)
                {
                    errors.Add($"Baris {rowNumber}: Task Sequence harus 01–99 atau kosong.");
                    continue;
                }
                manualSequence = seq;
            }

            if (!int.TryParse(Get("Waktu (menit)"), out int minutes) || minutes < 1 || minutes > 10000 ||
                !int.TryParse(Get("Man Power", "1"), out int manpower) || manpower < 1 || manpower > 100 ||
                !int.TryParse(Get("Periode (bulan)", "1"), out int periode) || periode < 1 || periode > 120 ||
                !int.TryParse(Get("Mulai Bulan", "1"), out int startMonth) || startMonth < 1 || startMonth > 12 ||
                !int.TryParse(Get("Week", "0"), out int week) || week < 0 || week > 4)
            {
                errors.Add($"Baris {rowNumber}: Waktu, Man Power, Periode, Mulai Bulan, atau Week tidak valid.");
                continue;
            }

            string stateText = Get("Kondisi Mesin", "OFF");
            if (!Enum.TryParse<MachineRunningState>(stateText, true, out var state) ||
                !Enum.IsDefined(typeof(MachineRunningState), state))
            {
                errors.Add($"Baris {rowNumber}: Kondisi Mesin '{stateText}' tidak valid.");
                continue;
            }

            var remark = Get("Remark");
            if (remark.Length > 500)
            {
                errors.Add($"Baris {rowNumber}: Remark maksimal 500 karakter.");
                continue;
            }

            drafts.Add(new PmTaskImportRow
            {
                RowNumber = rowNumber,
                AssetId = asset.Id,
                UnitName = unitName,
                SubUnitName = subUnitName,
                MethodName = methodName,
                StandardName = standardName,
                WorkHourMinutes = minutes,
                ManPower = manpower,
                PeriodeMonth = periode,
                StartMonth = startMonth,
                Week = week,
                MachineState = state,
                Remark = remark.NullIfEmpty(),
                ManualSequence = manualSequence
            });
        }

        if (errors.Count > 0)
        {
            errors.Insert(0, "Import dibatalkan: perbaiki seluruh baris yang bermasalah, lalu upload ulang.");
            ReportImport(rows.Count, 0, errors);
            return View("Master");
        }

        // Semua ID dialokasikan dan semua record disimpan dalam satu transaksi.
        // Serializable mengurangi risiko alokasi bersamaan; unique index IdPm
        // pada database tetap wajib dipertahankan.
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);
        try
        {
            var reservedIds = new HashSet<string>(StringComparer.Ordinal);

            // TAHAP 2: Cadangkan semua nomor manual lebih dulu.
            foreach (var draft in drafts.Where(d => d.ManualSequence.HasValue))
            {
                try
                {
                    draft.IdPm = await _pmIdGenerator.GenerateAsync(
                        draft.AssetId, draft.ManualSequence, reservedIds);
                    reservedIds.Add(draft.IdPm);
                }
                catch (InvalidOperationException ex)
                {
                    errors.Add($"Baris {draft.RowNumber}: {ex.Message}");
                }
            }
            if (errors.Count > 0)
            {
                await transaction.RollbackAsync();
                ReportImport(rows.Count, 0, errors);
                return View("Master");
            }

            // TAHAP 3: Otomatis melanjutkan MAX historis + 1.
            // Nomor manual di file yang sama sudah dicadangkan.
            foreach (var draft in drafts.Where(d => !d.ManualSequence.HasValue))
            {
                try
                {
                    draft.IdPm = await _pmIdGenerator.GenerateAsync(
                        draft.AssetId, null, reservedIds);
                    reservedIds.Add(draft.IdPm);
                }
                catch (InvalidOperationException ex)
                {
                    errors.Add($"Baris {draft.RowNumber}: {ex.Message}");
                }
            }
            if (errors.Count > 0)
            {
                await transaction.RollbackAsync();
                ReportImport(rows.Count, 0, errors);
                return View("Master");
            }

            // TAHAP 4: Cari/buat Unit dan Sub Unit; satu transaksi dengan Task PM.
            var units = await _db.Units.ToListAsync();
            var subUnits = await _db.SubUnits.ToListAsync();
            var unitIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var subIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in drafts.Select(d => d.UnitName)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var unit = units.FirstOrDefault(u =>
                    u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (unit == null)
                {
                    unit = new Unit
                    {
                        Code = "AUTO-" + Guid.NewGuid().ToString("N"),
                        Name = name,
                        Description = "Auto-created dari Import Task PM"
                    };
                    _db.Units.Add(unit);
                    units.Add(unit);
                    await _db.SaveChangesAsync();
                }
                unitIds[name] = unit.Id;
            }

            foreach (var draft in drafts)
            {
                int unitId = unitIds[draft.UnitName];
                string key = unitId + "\u001f" + draft.SubUnitName.ToLowerInvariant();
                if (subIds.ContainsKey(key)) continue;
                var sub = subUnits.FirstOrDefault(s => s.UnitId == unitId &&
                    s.Name.Equals(draft.SubUnitName, StringComparison.OrdinalIgnoreCase));
                if (sub == null)
                {
                    sub = new SubUnit
                    {
                        Code = "AUTO-" + Guid.NewGuid().ToString("N"),
                        Name = draft.SubUnitName,
                        UnitId = unitId,
                        Description = "Auto-created dari Import Task PM"
                    };
                    _db.SubUnits.Add(sub);
                    subUnits.Add(sub);
                    await _db.SaveChangesAsync();
                }
                subIds[key] = sub.Id;
            }

            // TAHAP 4: Pilih/buat Metode dan Standar dalam transaksi.
            var methods = await _db.Metodes.ToListAsync();
            var standards = await _db.Standars.ToListAsync();
            var methodNames = drafts.Select(d => d.MethodName)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var standardNames = drafts.Select(d => d.StandardName)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var codes = await _db.Metodes.IgnoreQueryFilters()
                .Select(m => m.Code).ToListAsync();
            int nextMethodNumber = 1;
            foreach (var code in codes)
            {
                if (code.StartsWith("M", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(code.Substring(1), out int n) && n >= nextMethodNumber)
                    nextMethodNumber = n + 1;
            }

            foreach (var name in methodNames)
            {
                if (methods.Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var letters = new string(name.Where(char.IsLetter).ToArray());
                var initial = letters.Length > 3
                    ? letters.Substring(0, 3).ToUpperInvariant()
                    : letters.ToUpperInvariant();
                var method = new Metode
                {
                    Code = "M" + (nextMethodNumber++).ToString("D2"),
                    Name = name,
                    Initial = initial,
                    MachineState = MachineRunningState.OFF,
                    Description = "Auto-created dari Import Task PM"
                };
                _db.Metodes.Add(method);
                methods.Add(method);
            }
            foreach (var name in standardNames)
            {
                if (standards.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var standard = new Standar
                {
                    Name = name,
                    Description = "Auto-created dari Import Task PM"
                };
                _db.Standars.Add(standard);
                standards.Add(standard);
            }
            await _db.SaveChangesAsync();

            // TAHAP 5: Simpan Task PM dan rotasi Week per mesin.
            var assetIds = drafts.Select(d => d.AssetId).Distinct().ToList();
            var counts = await _db.PmTaskTemplates
                .Where(t => assetIds.Contains(t.AssetId))
                .GroupBy(t => t.AssetId)
                .Select(g => new { AssetId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AssetId, x => x.Count);

            foreach (var draft in drafts)
            {
                counts.TryGetValue(draft.AssetId, out int existingCount);
                int week = draft.Week == 0
                    ? (existingCount % 4) + 1
                    : draft.Week;
                counts[draft.AssetId] = existingCount + 1;

                var method = methods.First(m =>
                    m.Name.Equals(draft.MethodName, StringComparison.OrdinalIgnoreCase));
                var standard = standards.First(s =>
                    s.Name.Equals(draft.StandardName, StringComparison.OrdinalIgnoreCase));

                _db.PmTaskTemplates.Add(new PmTaskTemplate
                {
                    IdPm = draft.IdPm,
                    AssetId = draft.AssetId,
                    UnitId = unitIds[draft.UnitName],
                    SubUnitId = subIds[unitIds[draft.UnitName] + "\u001f" + draft.SubUnitName.ToLowerInvariant()],
                    MethodId = method.Id,
                    StandardId = standard.Id,
                    WorkHourMinutes = draft.WorkHourMinutes,
                    ManPower = draft.ManPower,
                    MachineState = draft.MachineState,
                    PeriodeMonth = draft.PeriodeMonth,
                    StartMonth = draft.StartMonth,
                    Week = week,
                    Remark = draft.Remark,
                    IsActive = true
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            ReportImport(rows.Count, drafts.Count, errors);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _db.ChangeTracker.Clear();
            errors.Add("Import dibatalkan. Terjadi konflik database/ID-PM atau data tidak valid. " + ex.GetBaseException().Message);
            ReportImport(rows.Count, 0, errors);
        }
        return View("Master");
    }

    // Simpan satu batch; bila terjadi kegagalan SQL, jangan laporkan baris sukses.
    private async Task<int> SaveBatchAsync<T>(List<T> entities, List<string> errors) where T : BaseEntity
    {
        if (entities.Count == 0) return 0;
        _db.Set<T>().AddRange(entities);
        try
        {
            await _db.SaveChangesAsync();
            return entities.Count;
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            errors.Add("Penyimpanan batch gagal. Periksa index unik dan relasi data; tidak ada baris dari batch ini yang berhasil disimpan.");
            return 0;
        }
    }

    private void ReportImport(int total, int success, List<string> errors)
    {
        ViewBag.Uploaded = true;
        ViewBag.SuccessCount = success;
        ViewBag.TotalRows = total;
        ViewBag.Errors = errors;
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