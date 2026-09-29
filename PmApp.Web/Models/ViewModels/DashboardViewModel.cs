namespace PmApp.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalAssets { get; set; }
    public int TotalParts { get; set; }
    public int TotalTechnicians { get; set; }
    public int TotalBom { get; set; }

    // Statistik tambahan (opsional)
    public int ActiveAssets { get; set; }
    public int CriticalAssets { get; set; }

    public List<AssetSummary> RecentAssets { get; set; } = new();
}

public class AssetSummary
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Line { get; set; }
    public string Criticality { get; set; } = "";
    public int PartCount { get; set; }
}