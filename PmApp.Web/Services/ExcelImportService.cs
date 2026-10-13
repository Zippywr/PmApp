using ClosedXML.Excel;

namespace PmApp.Web.Services;

public class ExcelImportService
{
    // ============================================================
    // BACA EXCEL → List<Dictionary<Kolom, Nilai>>
    // ============================================================
    public List<Dictionary<string, string>> ReadExcel(Stream stream)
    {
        var result = new List<Dictionary<string, string>>();

        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheet(1);

        // Baca header dari baris 1
        var headerCells = ws.Row(1).CellsUsed().ToList();
        var headers = headerCells.Select(c => c.GetString().Trim()).ToList();

        if (headers.Count == 0)
            return result;

        // Baca data dari baris 2 ke bawah
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < headers.Count; i++)
            {
                var val = row.Cell(i + 1).GetString().Trim();
                dict[headers[i]] = val;
            }

            // Skip baris kosong
            if (dict.Values.All(string.IsNullOrWhiteSpace))
                continue;

            result.Add(dict);
        }

        return result;
    }

    // ============================================================
    // GENERATE TEMPLATE EXCEL
    // ============================================================
    public byte[] GenerateTemplate(string[] headers, string sheetName)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(sheetName);

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}