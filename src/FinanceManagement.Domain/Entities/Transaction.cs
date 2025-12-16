using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Enums;

namespace FinanceManagement.Domain.Entities;

public class Transaction : BaseEntity
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; }
    public string? Notes { get; set; }
    
    // Foreign keys
    public Guid CategoryId { get; set; }
    public Guid AccountId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public User User { get; set; } = null!;
}
