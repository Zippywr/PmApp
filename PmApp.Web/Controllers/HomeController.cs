using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;
using PmApp.Web.Models.ViewModels;

namespace PmApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            TotalAssets = await _db.Assets.CountAsync(),
            TotalParts = await _db.Parts.CountAsync(),
            TotalTechnicians = await _db.Technicians.CountAsync(),
            TotalBom = await _db.AssetParts.CountAsync(),
            ActiveAssets = await _db.Assets.CountAsync(a => a.IsActive),
            CriticalAssets = await _db.Assets.CountAsync(a => a.Criticality == Models.Criticality.Critical),
            RecentAssets = await _db.Assets
                .OrderByDescending(a => a.CreatedDate)
                .Take(5)
                .Select(a => new AssetSummary
                {
                    Id = a.Id,
                    Code = a.Code,
                    Name = a.Name,
                    Line = a.Line,
                    Criticality = a.Criticality.ToString(),
                    PartCount = a.AssetParts.Count(ap => !ap.IsDeleted)
                })
                .ToListAsync()
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}