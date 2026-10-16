
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;

namespace PmApp.Web.Services;

public sealed class PmIdGeneratorService
{
    private readonly AppDbContext _db;

    private static readonly Regex MachineCodePattern = new(
        @"^(?<line>\d{1,2})-[A-Za-z0-9]+-(?<machine>\d{1,2})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public PmIdGeneratorService(AppDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // MEMBACA PREFIX DARI CODE MESIN
    // Contoh:
    // 01-MC-01 -> 0101
    // 01-MC-02 -> 0102
    // 1-MC-01  -> 0101 (kompatibilitas kode lama)
    // =====================================================
    public static string GetPrefix(string machineCode)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            throw new InvalidOperationException(
                "Kode mesin tidak boleh kosong.");
        }

        var match = MachineCodePattern.Match(
            machineCode.Trim());

        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"Format kode mesin '{machineCode}' tidak sesuai. " +
                "Gunakan format seperti 01-MC-01.");
        }

        int lineNumber = int.Parse(
            match.Groups["line"].Value);

        int machineNumber = int.Parse(
            match.Groups["machine"].Value);

        if (lineNumber < 1 || lineNumber > 99 ||
            machineNumber < 1 || machineNumber > 99)
        {
            throw new InvalidOperationException(
                "Nomor Line dan Mesin harus antara 01–99.");
        }

        return lineNumber.ToString("D2") +
               machineNumber.ToString("D2");
    }

    // =====================================================
    // GENERATE HYBRID ID-PM
    //
    // manualSequence:
    // null = otomatis
    // 1-99 = manual
    //
    // reservedIds:
    // ID yang sudah dicadangkan selama satu batch import
    // =====================================================
    public async Task<string> GenerateAsync(
        int assetId,
        int? manualSequence = null,
        IEnumerable<string>? reservedIds = null,
        CancellationToken cancellationToken = default)
    {
        // Ambil Mesin beserta Line
        var asset = await _db.Assets
            .AsNoTracking()
            .Include(a => a.Line)
            .FirstOrDefaultAsync(
                a => a.Id == assetId,
                cancellationToken);

        if (asset == null)
        {
            throw new InvalidOperationException(
                "Mesin tidak ditemukan.");
        }

        if (asset.Line == null)
        {
            throw new InvalidOperationException(
                "Mesin belum terhubung ke Line.");
        }

        string prefix = GetPrefix(asset.Code);

        // Cocokkan kode Line pada Code Mesin
        // dengan master Line yang dipilih.
        if (!int.TryParse(asset.Line.Code,
                out int masterLineNumber) ||
            masterLineNumber < 1 ||
            masterLineNumber > 99)
        {
            throw new InvalidOperationException(
                $"Kode Line '{asset.Line.Code}' " +
                "harus berupa nomor 01–99.");
        }

        string masterLinePrefix =
            masterLineNumber.ToString("D2");

        if (!prefix.StartsWith(
                masterLinePrefix,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Kode Mesin '{asset.Code}' tidak sesuai " +
                $"dengan Line '{asset.Line.Code}'.");
        }

        // ================================================
        // PERIKSA PREFIX YANG BERPOTENSI BENTROK
        // Contoh 01-MC-01 dan 01-AB-01
        // ================================================
        var otherMachines = await _db.Assets
            .AsNoTracking()
            .Where(a => a.Id != assetId)
            .Select(a => a.Code)
            .ToListAsync(cancellationToken);

        foreach (var otherCode in otherMachines)
        {
            string otherPrefix;

            try
            {
                otherPrefix = GetPrefix(otherCode);
            }
            catch (InvalidOperationException)
            {
                // Format kode lama yang tidak sesuai
                // tidak bisa dibandingkan melalui parser ini.
                continue;
            }

            if (otherPrefix == prefix)
            {
                throw new InvalidOperationException(
                    $"Prefix ID-PM {prefix} berpotensi bentrok " +
                    $"dengan mesin '{otherCode}'. " +
                    "Periksa penomoran mesin terlebih dahulu.");
            }
        }

        // ================================================
        // AMBIL SEMUA ID-PM YANG PERNAH DIGUNAKAN
        // TERMASUK SOFT DELETE
        // ================================================
        var existingIds = await _db.PmTaskTemplates
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.IdPm.StartsWith(prefix))
            .Select(t => t.IdPm)
            .ToListAsync(cancellationToken);

        var usedIds = new HashSet<string>(
            existingIds,
            StringComparer.Ordinal);

        // Termasuk ID yang baru dialokasikan
        // tetapi belum disimpan dalam batch Excel.
        if (reservedIds != null)
        {
            usedIds.UnionWith(reservedIds);
        }

        // ================================================
        // MANUAL: GUNAKAN 2 DIGIT YANG DIPILIH
        // ================================================
        if (manualSequence.HasValue)
        {
            int sequence = manualSequence.Value;

            if (sequence < 1 || sequence > 99)
            {
                throw new InvalidOperationException(
                    "Task Sequence manual harus 01–99.");
            }

            string manualId =
                prefix + sequence.ToString("D2");

            if (usedIds.Contains(manualId))
            {
                throw new InvalidOperationException(
                    $"ID-PM {manualId} sudah pernah digunakan.");
            }

            return manualId;
        }

        // ================================================
        // OTOMATIS: CARI NOMOR TERBESAR + 1
        // ================================================
        int maxSequence = usedIds
            .Where(id =>
                id.Length == 6 &&
                id.StartsWith(
                    prefix,
                    StringComparison.Ordinal) &&
                int.TryParse(id[4..], out int number) &&
                number >= 1 &&
                number <= 99)
            .Select(id => int.Parse(id[4..]))
            .DefaultIfEmpty(0)
            .Max();

        if (maxSequence >= 99)
        {
            throw new InvalidOperationException(
                $"Task Sequence untuk prefix {prefix} " +
                "sudah mencapai batas 99.");
        }

        int nextSequence = maxSequence + 1;

        return prefix + nextSequence.ToString("D2");
    }
}
