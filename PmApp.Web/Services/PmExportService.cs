using ClosedXML.Excel;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models;
using PmApp.Web.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace PmApp.Web.Services;

public class PmExportService
{
    private readonly AppDbContext _db;

    public PmExportService(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // QUERY — LIST PM
    // ============================================================
    public async Task<List<PmTaskTemplate>> GetTaskTemplatesAsync(int? assetId)
    {
        var query = _db.PmTaskTemplates
            .Include(t => t.Asset).ThenInclude(a => a!.Line)
            .Include(t => t.Asset).ThenInclude(a => a!.Area)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .AsQueryable();

        if (assetId.HasValue)
            query = query.Where(t => t.AssetId == assetId.Value);

        return await query
            .OrderBy(t => t.Asset!.Code)
            .ThenBy(t => t.TaskSequence)
            .ToListAsync();
    }

    public async Task<List<PmTaskTemplate>> GetTasksByAssetAsync(int assetId)
    {
        return await _db.PmTaskTemplates
            .Include(t => t.Asset).ThenInclude(a => a!.Line)
            .Include(t => t.Asset).ThenInclude(a => a!.Area)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Include(t => t.Standard)
            .Where(t => t.AssetId == assetId)
            .OrderBy(t => t.TaskSequence)
            .ToListAsync();
    }

    public async Task<Asset?> GetAssetDetailAsync(int assetId)
    {
        return await _db.Assets
            .Include(a => a.Line)
            .Include(a => a.Area)
            .Include(a => a.McCategory)
            .Include(a => a.MachineFunction)
            .Include(a => a.Brand)
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == assetId);
    }

    // ============================================================
    // QUERY — JADWAL PM (Matrix 12 bulan)
    // ============================================================
    public async Task<ScheduleMatrixData> GetScheduleMatrixAsync(int year, int? assetId)
    {
        var tasksQuery = _db.PmTaskTemplates
            .Include(t => t.Asset)
            .Include(t => t.Unit)
            .Include(t => t.SubUnit)
            .Include(t => t.Method)
            .Where(t => t.IsActive)
            .AsQueryable();

        if (assetId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.AssetId == assetId.Value);

        var tasks = await tasksQuery
            .OrderBy(t => t.IdPm)
            .ToListAsync();

        var schedules = await _db.PmSchedules
            .Where(s => s.Year == year)
            .ToListAsync();

        var result = new ScheduleMatrixData { Year = year };

        foreach (var task in tasks)
        {
            var row = new ScheduleMatrixRow
            {
                IdPm = task.IdPm,
                AssetCode = task.Asset?.Code ?? "",
                AssetName = task.Asset?.Name ?? "",
                SubUnitName = task.SubUnit?.Name ?? "",
                MethodName = task.Method?.Name ?? "",
                WorkHourMinutes = task.WorkHourMinutes,
                ManPower = task.ManPower
            };

            for (int m = 1; m <= 12; m++)
            {
                var sched = schedules.FirstOrDefault(s =>
                    s.TaskTemplateId == task.Id && s.Month == m);

                row.Months.Add(new ScheduleCellData
                {
                    Month = m,
                    IsPlanned = sched != null,
                    Status = sched?.Status.ToString(),
                    ActualDate = sched?.ActualDate,
                    PIC = sched?.PIC
                });
            }

            result.Rows.Add(row);
        }

        return result;
    }

    // ============================================================
    // EXCEL — LIST PM (semua mesin)
    // ============================================================
    public byte[] ToExcel(List<PmTaskTemplate> data)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("List PM");

        string[] headers =
        {
            "ID PM", "Kode Mesin", "Nama Mesin", "Line", "Area",
            "No Line", "No Mesin", "No Task",
            "Unit", "Sub Unit", "Metode", "Standar",
            "Waktu (menit)", "Man Power", "Kondisi Mesin",
            "Periode (bulan)", "Mulai Bulan", "Remark", "Aktif"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.IdPm;
            ws.Cell(row, 2).Value = item.Asset?.Code ?? "";
            ws.Cell(row, 3).Value = item.Asset?.Name ?? "";
            ws.Cell(row, 4).Value = item.Asset?.Line?.Name ?? "";
            ws.Cell(row, 5).Value = item.Asset?.Area?.Name ?? "";
            ws.Cell(row, 6).Value = item.LineSequence;
            ws.Cell(row, 7).Value = item.MachineSequence;
            ws.Cell(row, 8).Value = item.TaskSequence;
            ws.Cell(row, 9).Value = item.Unit?.Name ?? "";
            ws.Cell(row, 10).Value = item.SubUnit?.Name ?? "";
            ws.Cell(row, 11).Value = item.Method?.Name ?? "";
            ws.Cell(row, 12).Value = item.Standard?.Name ?? "";
            ws.Cell(row, 13).Value = item.WorkHourMinutes;
            ws.Cell(row, 14).Value = item.ManPower;
            ws.Cell(row, 15).Value = item.MachineState.ToString();
            ws.Cell(row, 16).Value = item.PeriodeMonth;
            ws.Cell(row, 17).Value = item.StartMonth;
            ws.Cell(row, 18).Value = item.Remark ?? "";
            ws.Cell(row, 19).Value = item.IsActive ? "Ya" : "Tidak";
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ============================================================
    // EXCEL — PER MESIN
    // ============================================================
    public byte[] ToMachineExcel(Asset asset, List<PmTaskTemplate> tasks)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("PM " + asset.Code);

        int row = 1;

        ws.Cell(row, 1).Value = "DETAIL PM MESIN";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        ws.Range(row, 1, row, 8).Merge();
        row += 2;

        void AddDetail(string label, string? value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Cell(row, 2).Value = value ?? "-";
            ws.Range(row, 2, row, 4).Merge();
            row++;
        }

        AddDetail("Kode Mesin", asset.Code);
        AddDetail("Nama Mesin", asset.Name);
        AddDetail("OP No", asset.OpNo);
        AddDetail("Line", asset.Line?.Name);
        AddDetail("Area", asset.Area?.Name);
        AddDetail("Kategori Mesin", asset.McCategory?.Name);
        AddDetail("Fungsi Mesin", asset.MachineFunction?.Name);
        AddDetail("Brand", asset.Brand?.Name);
        AddDetail("Model / Type", asset.Model);
        AddDetail("Serial Number", asset.SerialNumber);
        AddDetail("Tahun", asset.YearMade?.ToString());
        AddDetail("Kapasitas", asset.Capacity);
        AddDetail("Kritikalitas", asset.Criticality.ToString());
        AddDetail("Status", asset.IsActive ? "Aktif" : "Nonaktif");

        row += 2;

        string[] headers =
        {
            "ID PM", "No Task", "Unit", "Sub Unit", "Metode", "Standar",
            "W/H (menit)", "MP", "Kondisi", "Periode", "Mulai Bulan", "Remark"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(row, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        row++;

        foreach (var t in tasks)
        {
            ws.Cell(row, 1).Value = t.IdPm;
            ws.Cell(row, 2).Value = t.TaskSequence;
            ws.Cell(row, 3).Value = t.Unit?.Name ?? "";
            ws.Cell(row, 4).Value = t.SubUnit?.Name ?? "";
            ws.Cell(row, 5).Value = t.Method?.Name ?? "";
            ws.Cell(row, 6).Value = t.Standard?.Name ?? "";
            ws.Cell(row, 7).Value = t.WorkHourMinutes;
            ws.Cell(row, 8).Value = t.ManPower;
            ws.Cell(row, 9).Value = t.MachineState.ToString();
            ws.Cell(row, 10).Value = t.PeriodeMonth + " bln";
            ws.Cell(row, 11).Value = "Bulan " + t.StartMonth;
            ws.Cell(row, 12).Value = t.Remark ?? "";
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ============================================================
    // EXCEL — JADWAL PM (Matrix 12 bulan)
    // ============================================================
    public byte[] ToScheduleExcel(ScheduleMatrixData data)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Jadwal PM " + data.Year);

        string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun",
                                "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };

        int totalCols = 6 + 12;

        // Title
        ws.Cell(1, 1).Value = "JADWAL PM TAHUN " + data.Year;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, totalCols).Merge();
        ws.Range(1, 1, 1, totalCols).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell(2, 1).Value = "Dicetak: " + DateTime.Now.ToString("dd MMM yyyy HH:mm");
        ws.Cell(2, 1).Style.Font.FontSize = 8;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
        ws.Range(2, 1, 2, totalCols).Merge();

        // Header row 1
        int headerRow1 = 4;
        int headerRow2 = 5;

        string[] infoHeaders = { "ID-PM", "Mesin", "Sub Unit", "Metode", "W/H", "MP" };
        for (int i = 0; i < infoHeaders.Length; i++)
        {
            var cell = ws.Cell(headerRow1, i + 1);
            cell.Value = infoHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(headerRow1, i + 1, headerRow2, i + 1).Merge();
        }

        // Header "Bulan"
        ws.Cell(headerRow1, 7).Value = "Bulan";
        ws.Range(headerRow1, 7, headerRow1, 6 + 12).Merge();
        ws.Range(headerRow1, 7, headerRow1, 6 + 12).Style.Font.Bold = true;
        ws.Range(headerRow1, 7, headerRow1, 6 + 12).Style.Fill.BackgroundColor = XLColor.LightGray;
        ws.Range(headerRow1, 7, headerRow1, 6 + 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(headerRow1, 7, headerRow1, 6 + 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Header month names
        for (int m = 0; m < 12; m++)
        {
            var cell = ws.Cell(headerRow2, 7 + m);
            cell.Value = monthNames[m];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        int row = 6;
        foreach (var r in data.Rows)
        {
            ws.Cell(row, 1).Value = r.IdPm;
            ws.Cell(row, 2).Value = r.AssetCode + " - " + r.AssetName;
            ws.Cell(row, 3).Value = r.SubUnitName;
            ws.Cell(row, 4).Value = r.MethodName;
            ws.Cell(row, 5).Value = r.WorkHourMinutes;
            ws.Cell(row, 6).Value = r.ManPower;

            for (int i = 0; i < 6; i++)
            {
                ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(row, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                ws.Cell(row, i + 1).Style.Font.FontSize = 9;
            }

            for (int m = 0; m < 12; m++)
            {
                var cellData = r.Months[m];
                var cell = ws.Cell(row, 7 + m);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                cell.Style.Font.FontSize = 8;

                if (cellData.IsPlanned)
                {
                    var text = cellData.Status ?? "-";

                    if (cellData.ActualDate.HasValue)
                        text += "\n" + cellData.ActualDate.Value.ToString("dd/MM");

                    if (!string.IsNullOrEmpty(cellData.PIC))
                        text += "\n" + cellData.PIC;

                    cell.Value = text;

                    // Warna berdasarkan status
                    cell.Style.Fill.BackgroundColor = cellData.Status switch
                    {
                        "Done" => XLColor.LightGreen,
                        "Overdue" => XLColor.LightPink,
                        "InProgress" => XLColor.LightBlue,
                        "Cancelled" => XLColor.LightGray,
                        _ => XLColor.LightYellow
                    };
                }
            }

            row++;
        }

        // Set column widths
        ws.Column(1).Width = 10;
        ws.Column(2).Width = 18;
        ws.Column(3).Width = 16;
        ws.Column(4).Width = 12;
        ws.Column(5).Width = 6;
        ws.Column(6).Width = 5;
        for (int m = 0; m < 12; m++)
        {
            ws.Column(7 + m).Width = 12;
        }

        ws.SheetView.FreezeRows(5);
        ws.SheetView.FreezeColumns(2);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ============================================================
    // CSV — LIST PM
    // ============================================================
    public byte[] ToCsv(List<PmTaskTemplate> data)
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms, new System.Text.UTF8Encoding(true));
        using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);

        csv.WriteField("ID PM");
        csv.WriteField("Kode Mesin");
        csv.WriteField("Nama Mesin");
        csv.WriteField("Line");
        csv.WriteField("Area");
        csv.WriteField("No Line");
        csv.WriteField("No Mesin");
        csv.WriteField("No Task");
        csv.WriteField("Unit");
        csv.WriteField("Sub Unit");
        csv.WriteField("Metode");
        csv.WriteField("Standar");
        csv.WriteField("Waktu (menit)");
        csv.WriteField("Man Power");
        csv.WriteField("Kondisi Mesin");
        csv.WriteField("Periode (bulan)");
        csv.WriteField("Mulai Bulan");
        csv.WriteField("Remark");
        csv.WriteField("Aktif");
        csv.NextRecord();

        foreach (var item in data)
        {
            csv.WriteField(item.IdPm);
            csv.WriteField(item.Asset?.Code ?? "");
            csv.WriteField(item.Asset?.Name ?? "");
            csv.WriteField(item.Asset?.Line?.Name ?? "");
            csv.WriteField(item.Asset?.Area?.Name ?? "");
            csv.WriteField(item.LineSequence);
            csv.WriteField(item.MachineSequence);
            csv.WriteField(item.TaskSequence);
            csv.WriteField(item.Unit?.Name ?? "");
            csv.WriteField(item.SubUnit?.Name ?? "");
            csv.WriteField(item.Method?.Name ?? "");
            csv.WriteField(item.Standard?.Name ?? "");
            csv.WriteField(item.WorkHourMinutes);
            csv.WriteField(item.ManPower);
            csv.WriteField(item.MachineState.ToString());
            csv.WriteField(item.PeriodeMonth);
            csv.WriteField(item.StartMonth);
            csv.WriteField(item.Remark ?? "");
            csv.WriteField(item.IsActive ? "Ya" : "Tidak");
            csv.NextRecord();
        }

        sw.Flush();
        return ms.ToArray();
    }

    // ============================================================
    // CSV — PER MESIN
    // ============================================================
    public byte[] ToMachineCsv(Asset asset, List<PmTaskTemplate> tasks)
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms, new System.Text.UTF8Encoding(true));
        using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);

        csv.WriteField("=== DETAIL MESIN ===");
        csv.NextRecord();
        csv.WriteField("Kode Mesin"); csv.WriteField(asset.Code); csv.NextRecord();
        csv.WriteField("Nama Mesin"); csv.WriteField(asset.Name); csv.NextRecord();
        csv.WriteField("OP No"); csv.WriteField(asset.OpNo ?? "-"); csv.NextRecord();
        csv.WriteField("Line"); csv.WriteField(asset.Line?.Name ?? "-"); csv.NextRecord();
        csv.WriteField("Area"); csv.WriteField(asset.Area?.Name ?? "-"); csv.NextRecord();
        csv.WriteField("Kategori"); csv.WriteField(asset.McCategory?.Name ?? "-"); csv.NextRecord();
        csv.WriteField("Fungsi"); csv.WriteField(asset.MachineFunction?.Name ?? "-"); csv.NextRecord();
        csv.WriteField("Brand"); csv.WriteField(asset.Brand?.Name ?? "-"); csv.NextRecord();
        csv.WriteField("Model"); csv.WriteField(asset.Model ?? "-"); csv.NextRecord();
        csv.WriteField("Serial Number"); csv.WriteField(asset.SerialNumber ?? "-"); csv.NextRecord();
        csv.WriteField("Tahun"); csv.WriteField(asset.YearMade?.ToString() ?? "-"); csv.NextRecord();
        csv.WriteField("Kapasitas"); csv.WriteField(asset.Capacity ?? "-"); csv.NextRecord();
        csv.NextRecord();

        csv.WriteField("=== DAFTAR TASK PM ===");
        csv.NextRecord();

        csv.WriteField("ID PM");
        csv.WriteField("No Task");
        csv.WriteField("Unit");
        csv.WriteField("Sub Unit");
        csv.WriteField("Metode");
        csv.WriteField("Standar");
        csv.WriteField("Waktu (menit)");
        csv.WriteField("MP");
        csv.WriteField("Kondisi");
        csv.WriteField("Periode (bulan)");
        csv.WriteField("Mulai Bulan");
        csv.WriteField("Remark");
        csv.NextRecord();

        foreach (var t in tasks)
        {
            csv.WriteField(t.IdPm);
            csv.WriteField(t.TaskSequence);
            csv.WriteField(t.Unit?.Name ?? "");
            csv.WriteField(t.SubUnit?.Name ?? "");
            csv.WriteField(t.Method?.Name ?? "");
            csv.WriteField(t.Standard?.Name ?? "");
            csv.WriteField(t.WorkHourMinutes);
            csv.WriteField(t.ManPower);
            csv.WriteField(t.MachineState.ToString());
            csv.WriteField(t.PeriodeMonth);
            csv.WriteField(t.StartMonth);
            csv.WriteField(t.Remark ?? "");
            csv.NextRecord();
        }

        sw.Flush();
        return ms.ToArray();
    }

    // ============================================================
    // CSV — JADWAL PM (Matrix 12 bulan)
    // ============================================================
    public byte[] ToScheduleCsv(ScheduleMatrixData data)
    {
        string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun",
                                "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms, new System.Text.UTF8Encoding(true));
        using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);

        csv.WriteField("JADWAL PM TAHUN " + data.Year);
        csv.NextRecord();
        csv.NextRecord();

        csv.WriteField("ID-PM");
        csv.WriteField("Mesin");
        csv.WriteField("Sub Unit");
        csv.WriteField("Metode");
        csv.WriteField("W/H");
        csv.WriteField("MP");
        foreach (var m in monthNames)
            csv.WriteField(m);
        csv.NextRecord();

        foreach (var r in data.Rows)
        {
            csv.WriteField(r.IdPm);
            csv.WriteField(r.AssetCode + " - " + r.AssetName);
            csv.WriteField(r.SubUnitName);
            csv.WriteField(r.MethodName);
            csv.WriteField(r.WorkHourMinutes);
            csv.WriteField(r.ManPower);

            foreach (var cell in r.Months)
            {
                if (!cell.IsPlanned)
                {
                    csv.WriteField("-");
                    continue;
                }

                var text = cell.Status ?? "-";
                if (cell.ActualDate.HasValue)
                    text += " | " + cell.ActualDate.Value.ToString("dd/MM");
                if (!string.IsNullOrEmpty(cell.PIC))
                    text += " | " + cell.PIC;

                csv.WriteField(text);
            }
            csv.NextRecord();
        }

        sw.Flush();
        return ms.ToArray();
    }

    // ============================================================
    // PDF — LIST PM (semua mesin)
    // ============================================================
    public byte[] ToPdf(List<PmTaskTemplate> data, string title)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(t => t.FontSize(8));

                page.Header().Column(col =>
                {
                    col.Item().Text(title).FontSize(14).Bold();
                    col.Item().Text("Dicetak: " + DateTime.Now.ToString("dd MMM yyyy HH:mm"))
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(8).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(50);
                        cols.ConstantColumn(60);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1f);
                        cols.RelativeColumn(2f);
                        cols.ConstantColumn(35);
                        cols.ConstantColumn(30);
                        cols.ConstantColumn(35);
                        cols.ConstantColumn(35);
                    });

                    table.Header(h =>
                    {
                        void Cell(string text)
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(3)
                                .Text(text).Bold().FontSize(8);
                        }
                        Cell("ID PM"); Cell("Kode"); Cell("Nama Mesin"); Cell("Line");
                        Cell("Unit"); Cell("Sub Unit"); Cell("Metode"); Cell("Standar");
                        Cell("Waktu"); Cell("MP"); Cell("State"); Cell("Periode");
                    });

                    foreach (var item in data)
                    {
                        void Cell(string text)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(2).Text(text ?? "-").FontSize(7);
                        }
                        Cell(item.IdPm);
                        Cell(item.Asset?.Code ?? "-");
                        Cell(item.Asset?.Name ?? "-");
                        Cell(item.Asset?.Line?.Name ?? "-");
                        Cell(item.Unit?.Name ?? "-");
                        Cell(item.SubUnit?.Name ?? "-");
                        Cell(item.Method?.Name ?? "-");
                        Cell(item.Standard?.Name ?? "-");
                        Cell(item.WorkHourMinutes + " m");
                        Cell(item.ManPower.ToString());
                        Cell(item.MachineState.ToString());
                        Cell(item.PeriodeMonth + " bln");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Halaman "); x.CurrentPageNumber();
                    x.Span(" dari "); x.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // ============================================================
    // PDF — PER MESIN
    // ============================================================
    public byte[] ToMachinePdf(Asset asset, List<PmTaskTemplate> tasks)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("DETAIL PM MESIN").FontSize(14).Bold();
                    col.Item().Text("Dicetak: " + DateTime.Now.ToString("dd MMM yyyy HH:mm"))
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(box =>
                    {
                        box.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(110);
                                c.RelativeColumn(2);
                                c.ConstantColumn(110);
                                c.RelativeColumn(2);
                            });

                            void Row(string l1, string? v1, string l2, string? v2)
                            {
                                tbl.Cell().Text(l1).Bold();
                                tbl.Cell().Text(v1 ?? "-");
                                tbl.Cell().Text(l2).Bold();
                                tbl.Cell().Text(v2 ?? "-");
                            }

                            Row("Kode Mesin", asset.Code, "OP No", asset.OpNo);
                            Row("Nama Mesin", asset.Name, "Line", asset.Line?.Name);
                            Row("Area", asset.Area?.Name, "Kategori", asset.McCategory?.Name);
                            Row("Fungsi", asset.MachineFunction?.Name, "Brand", asset.Brand?.Name);
                            Row("Model", asset.Model, "Serial Number", asset.SerialNumber);
                            Row("Tahun", asset.YearMade?.ToString(), "Kapasitas", asset.Capacity);
                            Row("Kritikalitas", asset.Criticality.ToString(), "Status", asset.IsActive ? "Aktif" : "Nonaktif");
                        });
                    });

                    col.Item().PaddingTop(10).Text("DAFTAR TASK PM").Bold().FontSize(11);

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(55);
                            cols.ConstantColumn(35);
                            cols.RelativeColumn(1.3f);
                            cols.RelativeColumn(1.3f);
                            cols.RelativeColumn(0.9f);
                            cols.RelativeColumn(2.2f);
                            cols.ConstantColumn(40);
                            cols.ConstantColumn(28);
                            cols.ConstantColumn(38);
                            cols.ConstantColumn(45);
                            cols.ConstantColumn(55);
                        });

                        table.Header(h =>
                        {
                            void Cell(string text)
                            {
                                h.Cell().Background(Colors.Blue.Lighten3).Padding(3)
                                    .Text(text).Bold().FontSize(8);
                            }
                            Cell("ID PM"); Cell("No"); Cell("Unit"); Cell("Sub Unit");
                            Cell("Metode"); Cell("Standar"); Cell("W/H"); Cell("MP");
                            Cell("Kondisi"); Cell("Periode"); Cell("Mulai");
                        });

                        foreach (var t in tasks)
                        {
                            void Cell(string text)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(2).Text(text ?? "-").FontSize(7.5f);
                            }
                            Cell(t.IdPm);
                            Cell(t.TaskSequence.ToString());
                            Cell(t.Unit?.Name ?? "-");
                            Cell(t.SubUnit?.Name ?? "-");
                            Cell(t.Method?.Name ?? "-");
                            Cell(t.Standard?.Name ?? "-");
                            Cell(t.WorkHourMinutes + " m");
                            Cell(t.ManPower.ToString());
                            Cell(t.MachineState.ToString());
                            Cell(t.PeriodeMonth + " bln");
                            Cell("Bln " + t.StartMonth);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Halaman "); x.CurrentPageNumber();
                    x.Span(" dari "); x.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    // ============================================================
    // PDF — JADWAL PM (Matrix 12 bulan)
    // ============================================================
    public byte[] ToSchedulePdf(ScheduleMatrixData data)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun",
                                "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(t => t.FontSize(7));

                page.Header().Column(col =>
                {
                    col.Item().Text("JADWAL PM TAHUN " + data.Year).FontSize(13).Bold();
                    col.Item().Text("Dicetak: " + DateTime.Now.ToString("dd MMM yyyy HH:mm"))
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(8).Table(table =>
                {
                    // 18 columns: 6 info + 12 months
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(48);   // ID-PM
                        cols.RelativeColumn(1.4f); // Mesin
                        cols.RelativeColumn(1.1f); // Sub Unit
                        cols.RelativeColumn(0.9f); // Metode
                        cols.ConstantColumn(24);   // W/H
                        cols.ConstantColumn(20);   // MP
                        for (int i = 0; i < 12; i++)
                            cols.RelativeColumn(0.75f); // 12 months
                    });

                    // Header
                    table.Header(h =>
                    {
                        void Cell(string text)
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(2)
                                .Text(text).Bold().FontSize(6.5f)
                                .AlignCenter();
                        }
                        Cell("ID-PM");
                        Cell("Mesin");
                        Cell("Sub Unit");
                        Cell("Metode");
                        Cell("W/H");
                        Cell("MP");
                        foreach (var m in monthNames)
                            Cell(m);
                    });

                    // Data
                    foreach (var r in data.Rows)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(2).Text(r.IdPm ?? "-").FontSize(6);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(2).Text(r.AssetCode + " - " + r.AssetName).FontSize(6);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(2).Text(r.SubUnitName ?? "-").FontSize(6);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(2).Text(r.MethodName ?? "-").FontSize(6);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(2).Text(r.WorkHourMinutes.ToString()).FontSize(6)
                            .AlignCenter();

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(2).Text(r.ManPower.ToString()).FontSize(6)
                            .AlignCenter();

                        foreach (var cell in r.Months)
                        {
                            var txt = "-";
                            if (cell.IsPlanned)
                            {
                                txt = cell.Status?.Substring(0, Math.Min(3, cell.Status.Length)) ?? "?";
                                if (cell.ActualDate.HasValue)
                                    txt += " " + cell.ActualDate.Value.ToString("dd/MM");
                            }
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(1).Text(txt).FontSize(5.5f).AlignCenter();
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Halaman "); x.CurrentPageNumber();
                    x.Span(" dari "); x.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }
}

// ============================================================
// DTO — Schedule Matrix
// ============================================================
public class ScheduleMatrixData
{
    public int Year { get; set; }
    public List<ScheduleMatrixRow> Rows { get; set; } = new();
}

public class ScheduleMatrixRow
{
    public string IdPm { get; set; } = "";
    public string AssetCode { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string SubUnitName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public int WorkHourMinutes { get; set; }
    public int ManPower { get; set; }
    public List<ScheduleCellData> Months { get; set; } = new();
}

public class ScheduleCellData
{
    public int Month { get; set; }
    public bool IsPlanned { get; set; }
    public string? Status { get; set; }
    public DateTime? ActualDate { get; set; }
    public string? PIC { get; set; }
}