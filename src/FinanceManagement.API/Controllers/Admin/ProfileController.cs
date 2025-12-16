using FinanceManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.API.Controllers.Admin;

/// <summary>
/// Admin Profile Controller (MVC)
/// Route: /admin/profile
/// </summary>
[Authorize]
[Route("admin/profile")]
public class ProfileController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(UserManager<User> userManager, ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// GET: /admin/profile
    /// Show user profile
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı";
                return RedirectToAction("Index", "Dashboard");
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profile");
            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    /// <summary>
    /// POST: /admin/profile
    /// Update user profile
    /// </summary>
    [HttpPost("")]
    [HttpPost("index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı";
                return RedirectToAction("Index", "Dashboard");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            TempData["Success"] = "Profil başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            return View(model);
        }
    }

    /// <summary>
    /// GET: /admin/profile/change-password
    /// Show change password form
    /// </summary>
    [HttpGet("change-password")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    /// <summary>
    /// POST: /admin/profile/change-password
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            TempData["Success"] = "Şifre başarıyla değiştirildi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            return View(model);
        }
    }
}

// View Models
public class ProfileViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
