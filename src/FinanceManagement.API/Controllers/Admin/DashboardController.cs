using FinanceManagement.Application.Dashboard.DTOs;
using FinanceManagement.Application.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.API.Controllers.Admin;

//[Authorize] // Temporarily disabled for testing
[Route("admin/[controller]")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    private string GetUserId() => "b62658b4-b799-46c2-952e-70bbdfbec6dd"; // Test user ID

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var dateRange = new DateRangeDto();
            var result = await _dashboardService.GetDashboardAsync(GetUserId(), dateRange);
            
            if (!result.Succeeded)
            {
                _logger.LogError("Dashboard yüklenemedi: {Errors}", string.Join(", ", result.Errors));
                return Content($"Hata: {string.Join(", ", result.Errors)}");
            }

            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard yüklenirken exception oluştu");
            return Content($"Exception: {ex.Message}\n\nStack: {ex.StackTrace}");
        }
    }
}
