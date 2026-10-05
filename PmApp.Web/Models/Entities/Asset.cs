using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public class Asset : BaseEntity
{
    // ============================================================
    // IDENTITAS
    // ============================================================

    [Required(ErrorMessage = "Kode mesin wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Mesin (HMMI/No Mesin)")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama mesin wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Mesin")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "OP No")]
    public string? OpNo { get; set; }

    // ============================================================
    // RELASI KE MASTER LOKASI
    // ============================================================

    [Display(Name = "Line")]
    public int? LineId { get; set; }
    public Line? Line { get; set; }

    [Display(Name = "Area")]
    public int? AreaId { get; set; }
    public Area? Area { get; set; }

    // ============================================================
    // RELASI KE MASTER KATEGORI MESIN
    // ============================================================

    [Display(Name = "Kategori Mesin")]
    public int? McCategoryId { get; set; }
    public McCategory? McCategory { get; set; }

    [Display(Name = "Fungsi Mesin")]
    public int? MachineFunctionId { get; set; }
    public MachineFunction? MachineFunction { get; set; }

    [Display(Name = "Brand / Maker")]
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    // ============================================================
    // RELASI KE PRODUCT
    // ============================================================

    [Display(Name = "Product")]
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // ============================================================
    // DETAIL
    // ============================================================

    [MaxLength(100)]
    [Display(Name = "Model / Type")]
    public string? Model { get; set; }

    [MaxLength(100)]
    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Tahun Pembuatan")]
    public int? YearMade { get; set; }

    [MaxLength(100)]
    [Display(Name = "Kapasitas")]
    public string? Capacity { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Instalasi")]
    public DateTime? InstallDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    [Display(Name = "Kritikalitas")]
    public Criticality Criticality { get; set; } = Criticality.Normal;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    // ============================================================
    // NAVIGATION
    // ============================================================
    public ICollection<AssetPart> AssetParts { get; set; } = new List<AssetPart>();
}