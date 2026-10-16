
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PmApp.Web.Models;

namespace PmApp.Web.Models.ViewModels;

public class AssetFormViewModel
{
    public int Id { get; set; }

    // ============================================================
    // IDENTITAS MESIN
    // ============================================================

    [Required(ErrorMessage = "Kode mesin wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Mesin (HMMI/No Mesin)")]
    public string Code { get; set; } = string.Empty;


    [Required(ErrorMessage = "OP No wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "OP No")]
    public string? OpNo { get; set; }

    // ============================================================
    // HIERARKI PLANT -> PRODUCT -> LINE
    // ============================================================

    [Required(ErrorMessage = "Plant wajib dipilih")]
    [Display(Name = "Plant")]
    public int? PlantId { get; set; }

    [Required(ErrorMessage = "Product wajib dipilih")]
    [Display(Name = "Product")]
    public int? ProductId { get; set; }

    [Required(ErrorMessage = "Line wajib dipilih")]
    [Display(Name = "Line")]
    public int? LineId { get; set; }

    // ============================================================
    // KLASIFIKASI MESIN
    // ============================================================

    [Required(ErrorMessage = "Kategori mesin wajib dipilih")]
    [Display(Name = "Kategori Mesin")]
    public int? McCategoryId { get; set; }

    [Display(Name = "Fungsi Mesin")]
    public int? MachineFunctionId { get; set; }

    [Display(Name = "Brand / Maker")]
    public int? BrandId { get; set; }

    // Relasi lama tetap disimpan sementara.
    // Tidak ditampilkan pada form baru.
    [Display(Name = "Area")]
    public int? AreaId { get; set; }

    // ============================================================
    // INFORMASI TAMBAHAN (OPSIONAL)
    // ============================================================

    [MaxLength(100)]
    [Display(Name = "Model / Type")]
    public string? Model { get; set; }

    [MaxLength(100)]
    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [Range(1900, 2100, ErrorMessage = "Tahun pembuatan tidak valid")]
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
    public Criticality Criticality { get; set; }
        = Criticality.Normal;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    // ============================================================
    // DROPDOWN LISTS
    // ============================================================

    public List<SelectListItem> PlantList { get; set; } = new();

    public List<SelectListItem> ProductList { get; set; } = new();

    public List<SelectListItem> LineList { get; set; } = new();

    public List<SelectListItem> McCategoryList { get; set; } = new();

    public List<SelectListItem> MachineFunctionList { get; set; } = new();

    public List<SelectListItem> BrandList { get; set; } = new();

    // Dipertahankan untuk kompatibilitas kode lama.
    public List<SelectListItem> AreaList { get; set; } = new();
}
