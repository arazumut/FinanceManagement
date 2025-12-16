using FinanceManagement.Application.Accounts.DTOs;
using FinanceManagement.Application.Common.Models;

namespace FinanceManagement.Application.Accounts.Interfaces;

public interface IAccountService
{
    Task<Result<AccountDto>> CreateAccountAsync(CreateAccountDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<AccountDto>> GetAccountByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<Result<List<AccountDto>>> GetUserAccountsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<AccountDto>> UpdateAccountAsync(UpdateAccountDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAccountAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<Result<decimal>> GetTotalBalanceAsync(string userId, CancellationToken cancellationToken = default);
}
