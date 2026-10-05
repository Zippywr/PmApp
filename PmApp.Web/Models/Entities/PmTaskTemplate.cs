using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

/// <summary>
/// Template task PM — terhubung ke PART di BOM (bukan langsung ke mesin).
/// 1 part bisa punya banyak task dengan frekuensi berbeda.
/// </summary>
public class PmTaskTemplate : BaseEntity
{
    [Display(Name = "Part BOM")]
    public int AssetPartId { get; set; }
    public AssetPart? AssetPart { get; set; }

    [Required(ErrorMessage = "Nama task wajib diisi")]
    [MaxLength(300)]
    [Display(Name = "Nama Task")]
    public string TaskName { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Sub Unit")]
    public string? SubUnit { get; set; }

    [Display(Name = "Metode")]
    public MaintenanceMethod Method { get; set; } = MaintenanceMethod.Visual;

    [MaxLength(300)]
    [Display(Name = "Standar")]
    public string? Standard { get; set; }

    [Display(Name = "Frekuensi")]
    public FrequencyType FrequencyType { get; set; } = FrequencyType.Monthly;

    [Range(1, 3650, ErrorMessage = "Nilai frekuensi 1-3650")]
    [Display(Name = "Nilai Frekuensi")]
    public int FrequencyValue { get; set; } = 1;

    [MaxLength(200)]
    [Display(Name = "Penanggung Jawab")]
    public string? PIC { get; set; }

    [MaxLength(500)]
    [Display(Name = "Catatan")]
    public string? Notes { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}