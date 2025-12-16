using FinanceManagement.Application.Accounts.Interfaces;
using FinanceManagement.Application.Accounts.Services;
using FinanceManagement.Application.Categories.Interfaces;
using FinanceManagement.Application.Categories.Services;
using FinanceManagement.Application.Dashboard.Interfaces;
using FinanceManagement.Application.Dashboard.Services;
using FinanceManagement.Application.Transactions.Interfaces;
using FinanceManagement.Application.Transactions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
