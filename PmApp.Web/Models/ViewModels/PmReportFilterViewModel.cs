using Microsoft.AspNetCore.Mvc.Rendering;

namespace PmApp.Web.Models.ViewModels;

public class PmReportFilterViewModel
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? LineId { get; set; }
    public int? AssetId { get; set; }
    public string? Status { get; set; }
    public string? Result { get; set; }

    public List<SelectListItem> Years { get; set; } = new();
    public List<SelectListItem> Months { get; set; } = new();
    public List<SelectListItem> Lines { get; set; } = new();
    public List<SelectListItem> Assets { get; set; } = new();
    public List<SelectListItem> Statuses { get; set; } = new();
    public List<SelectListItem> Results { get; set; } = new();
}