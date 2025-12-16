using FinanceManagement.Application.Dashboard.DTOs;
using FinanceManagement.Application.Dashboard.Interfaces;
using FinanceManagement.Application.Transactions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.API.Controllers.Admin;

/// <summary>
/// Admin Reports Controller (MVC)
/// Route: /admin/reports
/// </summary>
[Authorize]
[Route("admin/reports")]
public class ReportsController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IDashboardService dashboardService,
        ITransactionService transactionService,
        ILogger<ReportsController> logger)
    {
        _dashboardService = dashboardService;
        _transactionService = transactionService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// GET: /admin/reports
    /// Main reports page - Income/Expense overview
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetUserId();
            
            var dateRange = new DateRangeDto
            {
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-6).Date,
                EndDate = endDate ?? DateTime.UtcNow.Date
            };
            
            var result = await _dashboardService.GetDashboardAsync(userId, dateRange);
            
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View();
            }

            ViewBag.StartDate = dateRange.StartDate;
            ViewBag.EndDate = dateRange.EndDate;
            
            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reports");
            ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            return View();
        }
    }

    /// <summary>
    /// GET: /admin/reports/monthly
    /// Monthly detailed report
    /// </summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var userId = GetUserId();
            
            var selectedYear = year ?? DateTime.UtcNow.Year;
            var selectedMonth = month ?? DateTime.UtcNow.Month;
            
            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var dateRange = new DateRangeDto
            {
                StartDate = startDate,
                EndDate = endDate
            };
            
            var result = await _dashboardService.GetDashboardAsync(userId, dateRange);
            
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View();
            }

            ViewBag.Year = selectedYear;
            ViewBag.Month = selectedMonth;
            ViewBag.MonthName = startDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
            
            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading monthly report");
            ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            return View();
        }
    }

    /// <summary>
    /// GET: /admin/reports/category-analysis
    /// Category-based spending analysis
    /// </summary>
    [HttpGet("category-analysis")]
    public async Task<IActionResult> CategoryAnalysis([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetUserId();
            
            var dateRange = new DateRangeDto
            {
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-3).Date,
                EndDate = endDate ?? DateTime.UtcNow.Date
            };
            
            var result = await _dashboardService.GetDashboardAsync(userId, dateRange);
            
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View();
            }

            ViewBag.StartDate = dateRange.StartDate;
            ViewBag.EndDate = dateRange.EndDate;
            
            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category analysis");
            ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            return View();
        }
    }
}
