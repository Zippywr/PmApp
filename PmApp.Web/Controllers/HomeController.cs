using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmApp.Web.Data;

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
        // Statistik untuk dashboard
        ViewBag.TotalAssets = await _db.Assets.CountAsync();
        ViewBag.TotalParts = await _db.Parts.CountAsync();
        ViewBag.TotalBom = await _db.AssetParts.CountAsync();
        ViewBag.TotalPlants = await _db.Plants.CountAsync();
        ViewBag.TotalLines = await _db.Lines.CountAsync();
        ViewBag.TotalAreas = await _db.Areas.CountAsync();
        ViewBag.TotalBrands = await _db.Brands.CountAsync();
        ViewBag.TotalMcCategories = await _db.McCategories.CountAsync();
        ViewBag.TotalMachineFunctions = await _db.MachineFunctions.CountAsync();
        ViewBag.TotalProducts = await _db.Products.CountAsync();

        // Mesin terbaru (5 terakhir)
        var recentAssets = await _db.Assets
            .Include(a => a.Line)
            .Include(a => a.Area)
            .Include(a => a.Brand)
            .OrderByDescending(a => a.CreatedDate)
            .Take(5)
            .ToListAsync();

        return View(recentAssets);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}