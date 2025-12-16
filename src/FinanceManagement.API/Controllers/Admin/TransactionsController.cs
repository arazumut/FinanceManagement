using FinanceManagement.Application.Accounts.Interfaces;
using FinanceManagement.Application.Categories.Interfaces;
using FinanceManagement.Application.Transactions.DTOs;
using FinanceManagement.Application.Transactions.Interfaces;
using FinanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.API.Controllers.Admin;

/// <summary>
/// Admin Transactions Management Controller (MVC)
/// Route: /admin/transactions
/// </summary>
[Authorize]
[Route("admin/transactions")]
public class TransactionsController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        IAccountService accountService,
        ICategoryService categoryService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _categoryService = categoryService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// GET: /admin/transactions
    /// List all transactions with filtering and pagination
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index([FromQuery] TransactionFilterDto filter)
    {
        try
        {
            var userId = GetUserId();
            
            // Set default pagination if not provided
            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageSize < 1) filter.PageSize = 20;
            
            var result = await _transactionService.GetUserTransactionsAsync(filter, userId);
            
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View(result.Data);
            }

            // Load accounts and categories for filter dropdowns
            var accountsResult = await _accountService.GetUserAccountsAsync(userId);
            var categoriesResult = await _categoryService.GetUserCategoriesAsync(userId);
            
            ViewBag.Accounts = accountsResult.Succeeded ? accountsResult.Data : new List<FinanceManagement.Application.Accounts.DTOs.AccountDto>();
            ViewBag.Categories = categoriesResult.Succeeded ? categoriesResult.Data : new List<FinanceManagement.Application.Categories.DTOs.CategoryDto>();
            ViewBag.Filter = filter;

            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading transactions");
            ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            return View();
        }
    }

    /// <summary>
    /// GET: /admin/transactions/create
    /// Show create form
    /// </summary>
    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var userId = GetUserId();
            
            // Load accounts and categories for dropdowns
            var accountsResult = await _accountService.GetUserAccountsAsync(userId);
            var categoriesResult = await _categoryService.GetUserCategoriesAsync(userId);
            
            ViewBag.Accounts = accountsResult.Succeeded ? accountsResult.Data : new List<FinanceManagement.Application.Accounts.DTOs.AccountDto>();
            ViewBag.Categories = categoriesResult.Succeeded ? categoriesResult.Data : new List<FinanceManagement.Application.Categories.DTOs.CategoryDto>();

            return View(new CreateTransactionDto { Date = DateTime.Now });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create transaction form");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// POST: /admin/transactions/create
    /// Create new transaction
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTransactionDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownData();
            return View(dto);
        }

        try
        {
            var userId = GetUserId();
            var result = await _transactionService.CreateTransactionAsync(dto, userId);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                await LoadDropdownData();
                return View(dto);
            }

            TempData["Success"] = "İşlem başarıyla oluşturuldu!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            await LoadDropdownData();
            return View(dto);
        }
    }

    /// <summary>
    /// GET: /admin/transactions/edit/{id}
    /// Show edit form
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.GetTransactionByIdAsync(id, userId);
            
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new UpdateTransactionDto
            {
                Id = result.Data!.Id,
                Amount = result.Data.Amount,
                Description = result.Data.Description,
                Date = result.Data.Date,
                Notes = result.Data.Notes,
                CategoryId = result.Data.CategoryId,
                AccountId = result.Data.AccountId
            };

            await LoadDropdownData();
            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading transaction for edit");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// POST: /admin/transactions/edit/{id}
    /// Update transaction
    /// </summary>
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateTransactionDto dto)
    {
        if (id != dto.Id)
        {
            ModelState.AddModelError("", "ID uyuşmazlığı");
            await LoadDropdownData();
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownData();
            return View(dto);
        }

        try
        {
            var userId = GetUserId();
            var result = await _transactionService.UpdateTransactionAsync(dto, userId);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                await LoadDropdownData();
                return View(dto);
            }

            TempData["Success"] = "İşlem başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            await LoadDropdownData();
            return View(dto);
        }
    }

    /// <summary>
    /// POST: /admin/transactions/delete/{id}
    /// Delete transaction
    /// </summary>
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.DeleteTransactionAsync(id, userId);
            
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
            }
            else
            {
                TempData["Success"] = "İşlem başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadDropdownData()
    {
        var userId = GetUserId();
        var accountsResult = await _accountService.GetUserAccountsAsync(userId);
        var categoriesResult = await _categoryService.GetUserCategoriesAsync(userId);
        
        ViewBag.Accounts = accountsResult.Succeeded ? accountsResult.Data : new List<FinanceManagement.Application.Accounts.DTOs.AccountDto>();
        ViewBag.Categories = categoriesResult.Succeeded ? categoriesResult.Data : new List<FinanceManagement.Application.Categories.DTOs.CategoryDto>();
    }
}
