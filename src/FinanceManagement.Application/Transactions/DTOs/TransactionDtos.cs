using FinanceManagement.Domain.Enums;

namespace FinanceManagement.Application.Transactions.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string? Notes { get; set; }
    
    // Related entities
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTransactionDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; }
    public string? Notes { get; set; }
    public Guid CategoryId { get; set; }
    public Guid AccountId { get; set; }
}

public class UpdateTransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public Guid CategoryId { get; set; }
    public Guid AccountId { get; set; }
}

public class TransactionFilterDto
{
    public TransactionType? Type { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AccountId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
