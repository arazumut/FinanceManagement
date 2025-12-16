using System.Security.Claims;
using FinanceManagement.Application.Transactions.DTOs;
using FinanceManagement.Application.Transactions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Yeni işlem oluştur (Gelir veya Gider)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        var result = await _transactionService.CreateTransactionAsync(dto, GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// İşlem detayını getir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var result = await _transactionService.GetTransactionByIdAsync(id, GetUserId());
        
        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Tüm işlemleri filtreli ve sayfalı olarak getir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyTransactions([FromQuery] TransactionFilterDto filter)
    {
        var result = await _transactionService.GetUserTransactionsAsync(filter, GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new
        {
            items = result.Data!.Items,
            pageNumber = result.Data.PageNumber,
            totalPages = result.Data.TotalPages,
            totalCount = result.Data.TotalCount,
            hasPreviousPage = result.Data.HasPreviousPage,
            hasNextPage = result.Data.HasNextPage
        });
    }

    /// <summary>
    /// İşlemi güncelle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID uyuşmazlığı" });

        var result = await _transactionService.UpdateTransactionAsync(dto, GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// İşlemi sil (Hesap bakiyesi otomatik güncellenir)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var result = await _transactionService.DeleteTransactionAsync(id, GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    /// <summary>
    /// Toplam gelir (Opsiyonel tarih aralığı ile)
    /// </summary>
    [HttpGet("total-income")]
    public async Task<IActionResult> GetTotalIncome([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _transactionService.GetTotalIncomeAsync(GetUserId(), startDate, endDate);
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { totalIncome = result.Data });
    }

    /// <summary>
    /// Toplam gider (Opsiyonel tarih aralığı ile)
    /// </summary>
    [HttpGet("total-expense")]
    public async Task<IActionResult> GetTotalExpense([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _transactionService.GetTotalExpenseAsync(GetUserId(), startDate, endDate);
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { totalExpense = result.Data });
    }
}
