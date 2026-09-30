using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class AssetPartFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "Part wajib dipilih")]
    [Display(Name = "Part")]
    public int PartId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity minimal 1")]
    [Display(Name = "Jumlah")]
    public int Quantity { get; set; } = 1;

    [MaxLength(100)]
    [Display(Name = "Posisi")]
    public string? Position { get; set; }

    [MaxLength(500)]
    [Display(Name = "Catatan")]
    public string? Notes { get; set; }
}