using FinanceManagement.Application.Transactions.DTOs;

namespace FinanceManagement.Application.Dashboard.DTOs;

/// <summary>
/// Genel dashboard istatistikleri
/// </summary>
public class DashboardStatsDto
{
    public decimal TotalBalance { get; set; }
    public int TotalAccounts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalTransactions { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyNet { get; set; }
}

/// <summary>
/// Aylık gelir/gider özeti
/// </summary>
public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Net { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Kategori bazlı harcama analizi
/// </summary>
public class CategoryAnalysisDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Dashboard ana response
/// </summary>
public class DashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<MonthlyReportDto> MonthlyReports { get; set; } = new();
    public List<CategoryAnalysisDto> TopExpenseCategories { get; set; } = new();
    public List<CategoryAnalysisDto> TopIncomeCategories { get; set; } = new();
    public List<TransactionDto> RecentTransactions { get; set; } = new();
}

/// <summary>
/// Tarih aralığı filtresi
/// </summary>
public class DateRangeDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public DateRangeDto()
    {
        // Default: Son 30 gün
        EndDate = DateTime.UtcNow.Date;
        StartDate = EndDate.Value.AddDays(-30);
    }
}
