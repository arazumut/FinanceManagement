using System.Security.Claims;
using FinanceManagement.Application.Accounts.DTOs;
using FinanceManagement.Application.Accounts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        var result = await _accountService.CreateAccountAsync(dto, GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetAccountById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var result = await _accountService.GetAccountByIdAsync(id, GetUserId());
        
        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAccounts()
    {
        var result = await _accountService.GetUserAccountsAsync(GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID uyuşmazlığı" });

        var result = await _accountService.UpdateAccountAsync(dto, GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var result = await _accountService.DeleteAccountAsync(id, GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpGet("total-balance")]
    public async Task<IActionResult> GetTotalBalance()
    {
        var result = await _accountService.GetTotalBalanceAsync(GetUserId());
        
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { totalBalance = result.Data });
    }
}
