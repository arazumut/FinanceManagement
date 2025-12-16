using FinanceManagement.Application.Common.Models;
using FinanceManagement.Application.Transactions.DTOs;

namespace FinanceManagement.Application.Transactions.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionDto>> CreateTransactionAsync(CreateTransactionDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<TransactionDto>> GetTransactionByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<TransactionDto>>> GetUserTransactionsAsync(TransactionFilterDto filter, string userId, CancellationToken cancellationToken = default);
    Task<Result<TransactionDto>> UpdateTransactionAsync(UpdateTransactionDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteTransactionAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<Result<decimal>> GetTotalIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Result<decimal>> GetTotalExpenseAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
