using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities;

public class Account : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? Description { get; set; }
    
    // Foreign keys
    public string UserId { get; set; } = string.Empty;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
