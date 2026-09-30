using System.ComponentModel.DataAnnotations;
using PmApp.Web.Models;

namespace PmApp.Web.Models.ViewModels;

public class AssetFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode aset wajib diisi")]
    [MaxLength(50)]
    [Display(Name = "Kode Aset")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama aset wajib diisi")]
    [MaxLength(200)]
    [Display(Name = "Nama Aset")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Line / Area")]
    public string? Line { get; set; }

    [Display(Name = "Kritikalitas")]
    public Criticality Criticality { get; set; } = Criticality.Normal;

    [MaxLength(200)]
    [Display(Name = "Manufaktur")]
    public string? Manufacturer { get; set; }

    [MaxLength(200)]
    [Display(Name = "Model")]
    public string? Model { get; set; }

    [MaxLength(100)]
    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Tahun Pembuatan")]
    public int? YearMade { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Instalasi")]
    public DateTime? InstallDate { get; set; }

    [MaxLength(100)]
    [Display(Name = "Kapasitas")]
    public string? Capacity { get; set; }

    [MaxLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Description { get; set; }

    [MaxLength(200)]
    [Display(Name = "Lokasi")]
    public string? Location { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}