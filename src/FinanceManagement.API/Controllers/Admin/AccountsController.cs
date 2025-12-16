using FinanceManagement.Application.Accounts.DTOs;
using FinanceManagement.Application.Accounts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.API.Controllers.Admin;

/// <summary>
/// Admin Accounts Management Controller (MVC)
/// Route: /admin/accounts
/// </summary>
[Authorize]
[Route("admin/accounts")]
public class AccountsController : Controller
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// GET: /admin/accounts
    /// List all accounts
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetUserId();
            var result = await _accountService.GetUserAccountsAsync(userId);
            
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View(new List<AccountDto>());
            }

            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accounts");
            ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            return View(new List<AccountDto>());
        }
    }

    /// <summary>
    /// GET: /admin/accounts/create
    /// Show create form
    /// </summary>
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// POST: /admin/accounts/create
    /// Create new account
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var userId = GetUserId();
            var result = await _accountService.CreateAccountAsync(dto, userId);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(dto);
            }

            TempData["Success"] = "Hesap başarıyla oluşturuldu!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            return View(dto);
        }
    }

    /// <summary>
    /// GET: /admin/accounts/edit/{id}
    /// Show edit form
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _accountService.GetAccountByIdAsync(id, userId);
            
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new UpdateAccountDto
            {
                Id = result.Data!.Id,
                Name = result.Data.Name,
                Description = result.Data.Description
            };

            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading account for edit");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// POST: /admin/accounts/edit/{id}
    /// Update account
    /// </summary>
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateAccountDto dto)
    {
        if (id != dto.Id)
        {
            ModelState.AddModelError("", "ID uyuşmazlığı");
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var userId = GetUserId();
            var result = await _accountService.UpdateAccountAsync(dto, userId);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(dto);
            }

            TempData["Success"] = "Hesap başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            return View(dto);
        }
    }

    /// <summary>
    /// POST: /admin/accounts/delete/{id}
    /// Delete account
    /// </summary>
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _accountService.DeleteAccountAsync(id, userId);
            
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
            }
            else
            {
                TempData["Success"] = "Hesap başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
