using FinanceManagement.Application.Common.Models;
using FinanceManagement.Application.Dashboard.DTOs;

namespace FinanceManagement.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    /// <summary>
    /// Tam dashboard verilerini getir
    /// </summary>
    Task<Result<DashboardDto>> GetDashboardAsync(string userId, DateRangeDto? dateRange = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sadece genel istatistikleri getir
    /// </summary>
    Task<Result<DashboardStatsDto>> GetStatsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aylık raporları getir (varsayılan: son 6 ay)
    /// </summary>
    Task<Result<List<MonthlyReportDto>>> GetMonthlyReportsAsync(string userId, int monthCount = 6, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kategori bazlı analiz (Gider kategorileri)
    /// </summary>
    Task<Result<List<CategoryAnalysisDto>>> GetExpenseCategoryAnalysisAsync(string userId, DateRangeDto? dateRange = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kategori bazlı analiz (Gelir kategorileri)
    /// </summary>
    Task<Result<List<CategoryAnalysisDto>>> GetIncomeCategoryAnalysisAsync(string userId, DateRangeDto? dateRange = null, CancellationToken cancellationToken = default);
}
