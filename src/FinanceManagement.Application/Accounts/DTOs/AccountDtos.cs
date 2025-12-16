namespace FinanceManagement.Application.Accounts.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; } = 0;
    public string Currency { get; set; } = "TRY";
    public string? Description { get; set; }
}

public class UpdateAccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
