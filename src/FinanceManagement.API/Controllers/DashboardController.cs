using System.Security.Claims;
using FinanceManagement.Application.Dashboard.DTOs;
using FinanceManagement.Application.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
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
    /// Tam dashboard verilerini getir (istatistikler, grafikler, son işlemler)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard([FromQuery] DateRangeDto? dateRange = null)
    {
        var result = await _dashboardService.GetDashboardAsync(GetUserId(), dateRange);
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Sadece genel istatistikleri getir
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dashboardService.GetStatsAsync(GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Aylık raporları getir (varsayılan: son 6 ay)
    /// </summary>
    [HttpGet("monthly-reports")]
    public async Task<IActionResult> GetMonthlyReports([FromQuery] int monthCount = 6)
    {
        if (monthCount < 1 || monthCount > 24)
            return BadRequest(new { message = "Ay sayısı 1-24 arasında olmalıdır" });

        var result = await _dashboardService.GetMonthlyReportsAsync(GetUserId(), monthCount);
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Gider kategorileri analizi
    /// </summary>
    [HttpGet("expense-analysis")]
    public async Task<IActionResult> GetExpenseAnalysis([FromQuery] DateRangeDto? dateRange = null)
    {
        var result = await _dashboardService.GetExpenseCategoryAnalysisAsync(GetUserId(), dateRange);
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Gelir kategorileri analizi
    /// </summary>
    [HttpGet("income-analysis")]
    public async Task<IActionResult> GetIncomeAnalysis([FromQuery] DateRangeDto? dateRange = null)
    {
        var result = await _dashboardService.GetIncomeCategoryAnalysisAsync(GetUserId(), dateRange);
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
