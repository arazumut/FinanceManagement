using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Models;
using FinanceManagement.Application.Transactions.DTOs;
using FinanceManagement.Application.Transactions.Interfaces;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Transactions.Services;

public class TransactionService : ITransactionService
{
    private readonly IApplicationDbContext _context;

    public TransactionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TransactionDto>> CreateTransactionAsync(CreateTransactionDto dto, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify account belongs to user
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.UserId == userId, cancellationToken);

            if (account == null)
                return Result<TransactionDto>.Failure("Hesap bulunamadı veya size ait değil");

            // Verify category belongs to user and matches transaction type
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId, cancellationToken);

            if (category == null)
                return Result<TransactionDto>.Failure("Kategori bulunamadı veya size ait değil");

            if (category.Type != dto.Type)
                return Result<TransactionDto>.Failure("Kategori tipi işlem tipi ile uyuşmuyor");

            // Create transaction
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                Notes = dto.Notes,
                CategoryId = dto.CategoryId,
                AccountId = dto.AccountId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Update account balance
            if (dto.Type == TransactionType.Income)
            {
                account.Balance += dto.Amount;
            }
            else // Expense
            {
                account.Balance -= dto.Amount;
            }
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Load related entities for response
            var transactionDto = await GetTransactionDtoAsync(transaction.Id, userId, cancellationToken);
            return Result<TransactionDto>.Success(transactionDto!);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"İşlem oluşturulurken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> GetTransactionByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var transaction = await GetTransactionDtoAsync(id, userId, cancellationToken);

        if (transaction == null)
            return Result<TransactionDto>.Failure("İşlem bulunamadı");

        return Result<TransactionDto>.Success(transaction);
    }

    public async Task<Result<PaginatedList<TransactionDto>>> GetUserTransactionsAsync(TransactionFilterDto filter, string userId, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.UserId == userId);

        // Apply filters
        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.AccountId.HasValue)
            query = query.Where(t => t.AccountId == filter.AccountId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.Date >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.Date <= filter.EndDate.Value);

        // Order by date descending (newest first)
        query = query.OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt);

        // Map to DTO
        var dtoQuery = query.Select(t => new TransactionDto
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
        });

        var paginatedList = await PaginatedList<TransactionDto>.CreateAsync(
            dtoQuery,
            filter.PageNumber,
            filter.PageSize);

        return Result<PaginatedList<TransactionDto>>.Success(paginatedList);
    }

    public async Task<Result<TransactionDto>> UpdateTransactionAsync(UpdateTransactionDto dto, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == dto.Id && t.UserId == userId, cancellationToken);

            if (transaction == null)
                return Result<TransactionDto>.Failure("İşlem bulunamadı");

            // Verify new account belongs to user
            var newAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.UserId == userId, cancellationToken);

            if (newAccount == null)
                return Result<TransactionDto>.Failure("Hesap bulunamadı veya size ait değil");

            // Verify new category belongs to user
            var newCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId, cancellationToken);

            if (newCategory == null)
                return Result<TransactionDto>.Failure("Kategori bulunamadı veya size ait değil");

            if (newCategory.Type != transaction.Type)
                return Result<TransactionDto>.Failure("Kategori tipi işlem tipi ile uyuşmuyor");

            // Revert old balance change
            if (transaction.Type == TransactionType.Income)
            {
                transaction.Account.Balance -= transaction.Amount;
            }
            else
            {
                transaction.Account.Balance += transaction.Amount;
            }

            // Update transaction
            transaction.Amount = dto.Amount;
            transaction.Description = dto.Description;
            transaction.Date = dto.Date;
            transaction.Notes = dto.Notes;
            transaction.CategoryId = dto.CategoryId;
            transaction.UpdatedAt = DateTime.UtcNow;

            // Apply new balance change
            if (transaction.Type == TransactionType.Income)
            {
                newAccount.Balance += dto.Amount;
            }
            else
            {
                newAccount.Balance -= dto.Amount;
            }
            newAccount.UpdatedAt = DateTime.UtcNow;

            // Update account if changed
            if (transaction.AccountId != dto.AccountId)
            {
                transaction.Account.UpdatedAt = DateTime.UtcNow;
                transaction.AccountId = dto.AccountId;
            }

            await _context.SaveChangesAsync(cancellationToken);

            var transactionDto = await GetTransactionDtoAsync(transaction.Id, userId, cancellationToken);
            return Result<TransactionDto>.Success(transactionDto!);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"İşlem güncellenirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteTransactionAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

            if (transaction == null)
                return Result.Failure("İşlem bulunamadı");

            // Revert balance change
            if (transaction.Type == TransactionType.Income)
            {
                transaction.Account.Balance -= transaction.Amount;
            }
            else
            {
                transaction.Account.Balance += transaction.Amount;
            }
            transaction.Account.UpdatedAt = DateTime.UtcNow;

            // Soft delete
            transaction.IsDeleted = true;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"İşlem silinirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> GetTotalIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var total = await query.SumAsync(t => t.Amount, cancellationToken);

        return Result<decimal>.Success(total);
    }

    public async Task<Result<decimal>> GetTotalExpenseAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var total = await query.SumAsync(t => t.Amount, cancellationToken);

        return Result<decimal>.Success(total);
    }

    private async Task<TransactionDto?> GetTransactionDtoAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.Id == id && t.UserId == userId)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
