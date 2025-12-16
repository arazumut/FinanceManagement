using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Category> Categories { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
