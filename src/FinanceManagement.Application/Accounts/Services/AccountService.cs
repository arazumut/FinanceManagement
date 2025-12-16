using FinanceManagement.Application.Accounts.DTOs;
using FinanceManagement.Application.Accounts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Models;
using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Accounts.Services;

public class AccountService : IAccountService
{
    private readonly IApplicationDbContext _context;

    public AccountService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AccountDto>> CreateAccountAsync(CreateAccountDto dto, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Balance = dto.InitialBalance,
                Currency = dto.Currency,
                Description = dto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);

            var accountDto = MapToDto(account);
            return Result<AccountDto>.Success(accountDto);
        }
        catch (Exception ex)
        {
            return Result<AccountDto>.Failure($"Hesap oluşturulurken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<AccountDto>> GetAccountByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (account == null)
            return Result<AccountDto>.Failure("Hesap bulunamadı");

        var accountDto = MapToDto(account);
        return Result<AccountDto>.Success(accountDto);
    }

    public async Task<Result<List<AccountDto>>> GetUserAccountsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var accountDtos = accounts.Select(MapToDto).ToList();
        return Result<List<AccountDto>>.Success(accountDtos);
    }

    public async Task<Result<AccountDto>> UpdateAccountAsync(UpdateAccountDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.Id && a.UserId == userId, cancellationToken);

        if (account == null)
            return Result<AccountDto>.Failure("Hesap bulunamadı");

        account.Name = dto.Name;
        account.Description = dto.Description;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var accountDto = MapToDto(account);
        return Result<AccountDto>.Success(accountDto);
    }

    public async Task<Result> DeleteAccountAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (account == null)
            return Result.Failure("Hesap bulunamadı");

        // Soft delete
        account.IsDeleted = true;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<decimal>> GetTotalBalanceAsync(string userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
        
        var totalBalance = accounts.Sum(a => a.Balance);

        return Result<decimal>.Success(totalBalance);
    }

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Balance = account.Balance,
            Currency = account.Currency,
            Description = account.Description,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }
}
