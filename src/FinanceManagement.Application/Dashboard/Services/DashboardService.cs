using System.Globalization;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Models;
using FinanceManagement.Application.Dashboard.DTOs;
using FinanceManagement.Application.Dashboard.Interfaces;
using FinanceManagement.Application.Transactions.DTOs;
using FinanceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardDto>> GetDashboardAsync(string userId, DateRangeDto? dateRange = null, CancellationToken cancellationToken = default)
    {
        try
        {
            dateRange ??= new DateRangeDto();

            var dashboard = new DashboardDto
            {
                Stats = await GetStatsInternalAsync(userId, cancellationToken),
                MonthlyReports = await GetMonthlyReportsInternalAsync(userId, 6, cancellationToken),
                TopExpenseCategories = await GetCategoryAnalysisInternalAsync(userId, TransactionType.Expense, dateRange, 5, cancellationToken),
                TopIncomeCategories = await GetCategoryAnalysisInternalAsync(userId, TransactionType.Income, dateRange, 5, cancellationToken),
                RecentTransactions = await GetRecentTransactionsAsync(userId, 10, cancellationToken)
            };

            return Result<DashboardDto>.Success(dashboard);
        }
        catch (Exception ex)
        {
            return Result<DashboardDto>.Failure($"Dashboard verileri alınırken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<DashboardStatsDto>> GetStatsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetStatsInternalAsync(userId, cancellationToken);
            return Result<DashboardStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<DashboardStatsDto>.Failure($"İstatistikler alınırken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<List<MonthlyReportDto>>> GetMonthlyReportsAsync(string userId, int monthCount = 6, CancellationToken cancellationToken = default)
    {
        try
        {
            var reports = await GetMonthlyReportsInternalAsync(userId, monthCount, cancellationToken);
            return Result<List<MonthlyReportDto>>.Success(reports);
        }
        catch (Exception ex)
        {
            return Result<List<MonthlyReportDto>>.Failure($"Aylık raporlar alınırken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryAnalysisDto>>> GetExpenseCategoryAnalysisAsync(string userId, DateRangeDto? dateRange = null, CancellationToken cancellationToken = default)
    {
        try
        {
            dateRange ??= new DateRangeDto();
            var analysis = await GetCategoryAnalysisInternalAsync(userId, TransactionType.Expense, dateRange, null, cancellationToken);
            return Result<List<CategoryAnalysisDto>>.Success(analysis);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryAnalysisDto>>.Failure($"Gider analizi alınırken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryAnalysisDto>>> GetIncomeCategoryAnalysisAsync(string userId, DateRangeDto? dateRange = null, CancellationToken cancellationToken = default)
    {
        try
        {
            dateRange ??= new DateRangeDto();
            var analysis = await GetCategoryAnalysisInternalAsync(userId, TransactionType.Income, dateRange, null, cancellationToken);
            return Result<List<CategoryAnalysisDto>>.Success(analysis);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryAnalysisDto>>.Failure($"Gelir analizi alınırken hata oluştu: {ex.Message}");
        }
    }

    // Private helper methods

    private async Task<DashboardStatsDto> GetStatsInternalAsync(string userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Get all accounts and calculate total balance on client side
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
        
        var totalBalance = accounts.Sum(a => a.Balance);
        var totalAccounts = accounts.Count;

        var totalCategories = await _context.Categories
            .CountAsync(c => c.UserId == userId, cancellationToken);

        var totalTransactions = await _context.Transactions
            .CountAsync(t => t.UserId == userId, cancellationToken);

        // Get monthly transactions and calculate sums on client side
        var monthlyTransactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth)
            .ToListAsync(cancellationToken);

        var monthlyIncome = monthlyTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var monthlyExpense = monthlyTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return new DashboardStatsDto
        {
            TotalBalance = totalBalance,
            TotalAccounts = totalAccounts,
            TotalCategories = totalCategories,
            TotalTransactions = totalTransactions,
            MonthlyIncome = monthlyIncome,
            MonthlyExpense = monthlyExpense,
            MonthlyNet = monthlyIncome - monthlyExpense
        };
    }

    private async Task<List<MonthlyReportDto>> GetMonthlyReportsInternalAsync(string userId, int monthCount, CancellationToken cancellationToken)
    {
        var reports = new List<MonthlyReportDto>();
        var now = DateTime.UtcNow;

        for (int i = monthCount - 1; i >= 0; i--)
        {
            var targetDate = now.AddMonths(-i);
            var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth)
                .ToListAsync(cancellationToken);

            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            reports.Add(new MonthlyReportDto
            {
                Year = targetDate.Year,
                Month = targetDate.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(targetDate.Month),
                TotalIncome = income,
                TotalExpense = expense,
                Net = income - expense,
                TransactionCount = transactions.Count
            });
        }

        return reports;
    }

    private async Task<List<CategoryAnalysisDto>> GetCategoryAnalysisInternalAsync(
        string userId, 
        TransactionType type, 
        DateRangeDto dateRange, 
        int? topCount,
        CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Type == type);

        if (dateRange.StartDate.HasValue)
            query = query.Where(t => t.Date >= dateRange.StartDate.Value);

        if (dateRange.EndDate.HasValue)
            query = query.Where(t => t.Date <= dateRange.EndDate.Value);

        // Get all transactions first, then group on client side
        var transactions = await query.ToListAsync(cancellationToken);

        var groupedData = transactions
            .GroupBy(t => new
            {
                t.CategoryId,
                t.Category.Name,
                t.Category.Icon,
                t.Category.Color
            })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.Name,
                g.Key.Icon,
                g.Key.Color,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        var totalAmount = groupedData.Sum(x => x.TotalAmount);

        var analysis = groupedData.Select(x => new CategoryAnalysisDto
        {
            CategoryId = x.CategoryId,
            CategoryName = x.Name,
            CategoryIcon = x.Icon ?? "",
            CategoryColor = x.Color,
            TotalAmount = x.TotalAmount,
            TransactionCount = x.TransactionCount,
            Percentage = totalAmount > 0 ? Math.Round((x.TotalAmount / totalAmount) * 100, 2) : 0
        }).ToList();

        if (topCount.HasValue)
            analysis = analysis.Take(topCount.Value).ToList();

        return analysis;
    }

    private async Task<List<TransactionDto>> GetRecentTransactionsAsync(string userId, int count, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(count)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Description = t.Description,
                Date = t.Date,
                Type = t.Type,
                Notes = t.Notes,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                CategoryIcon = t.Category.Icon ?? "",
                CategoryColor = t.Category.Color,
                AccountId = t.AccountId,
                AccountName = t.Account.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
