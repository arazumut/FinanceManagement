using FinanceManagement.Application.Dashboard.DTOs;
using FinanceManagement.Application.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.API.Controllers.Admin;

/// <summary>
/// Admin Dashboard Controller
/// Route: /admin/dashboard
/// </summary>
[Authorize]
[Route("admin/dashboard")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// GET: /admin/dashboard or /admin/dashboard/index
    /// Shows the main dashboard
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation("Dashboard yükleniyor. UserId: {UserId}, User: {UserName}", 
                userId, User.Identity?.Name ?? "Unknown");

            var dateRange = new DateRangeDto();
            var result = await _dashboardService.GetDashboardAsync(userId, dateRange);
            
            if (!result.Succeeded)
            {
                _logger.LogError("Dashboard yüklenemedi: {Errors}", string.Join(", ", result.Errors));
                ViewBag.Error = string.Join(", ", result.Errors);
                return View();
            }

            _logger.LogInformation("✅ Dashboard başarıyla yüklendi");
            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard yüklenirken exception oluştu");
            ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            return View();
        }
    }
}
