using FinanceManagement.Application.Categories.DTOs;
using FinanceManagement.Application.Categories.Interfaces;
using FinanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.API.Controllers.Admin;

/// <summary>
/// Admin Categories Management Controller (MVC)
/// Route: /admin/categories
/// </summary>
[Authorize]
[Route("admin/categories")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// GET: /admin/categories
    /// List all categories
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index([FromQuery] TransactionType? type = null)
    {
        try
        {
            var userId = GetUserId();
            var result = await _categoryService.GetUserCategoriesAsync(userId, type);
            
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View(new List<CategoryDto>());
            }

            ViewBag.FilterType = type;
            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
            ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            return View(new List<CategoryDto>());
        }
    }

    /// <summary>
    /// GET: /admin/categories/create
    /// Show create form
    /// </summary>
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// POST: /admin/categories/create
    /// Create new category
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var userId = GetUserId();
            var result = await _categoryService.CreateCategoryAsync(dto, userId);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(dto);
            }

            TempData["Success"] = "Kategori başarıyla oluşturuldu!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            return View(dto);
        }
    }

    /// <summary>
    /// GET: /admin/categories/edit/{id}
    /// Show edit form
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _categoryService.GetCategoryByIdAsync(id, userId);
            
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new UpdateCategoryDto
            {
                Id = result.Data!.Id,
                Name = result.Data.Name,
                Icon = result.Data.Icon,
                Color = result.Data.Color,
                Description = result.Data.Description
            };

            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category for edit");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// POST: /admin/categories/edit/{id}
    /// Update category
    /// </summary>
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCategoryDto dto)
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
            var result = await _categoryService.UpdateCategoryAsync(dto, userId);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(dto);
            }

            TempData["Success"] = "Kategori başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            return View(dto);
        }
    }

    /// <summary>
    /// POST: /admin/categories/delete/{id}
    /// Delete category
    /// </summary>
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _categoryService.DeleteCategoryAsync(id, userId);
            
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
            }
            else
            {
                TempData["Success"] = "Kategori başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
