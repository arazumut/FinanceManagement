using FinanceManagement.Application.Accounts.Interfaces;
using FinanceManagement.Application.Accounts.Services;
using FinanceManagement.Application.Categories.Interfaces;
using FinanceManagement.Application.Categories.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
