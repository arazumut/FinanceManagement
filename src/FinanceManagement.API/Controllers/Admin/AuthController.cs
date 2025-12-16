using FinanceManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.API.Controllers.Admin;

/// <summary>
/// Admin Panel Authentication Controller
/// Route: /admin/login, /admin/logout
/// </summary>
[Route("admin")]
public class AuthController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        ILogger<AuthController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// GET: /admin/login
    /// Displays the login page
    /// </summary>
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Eğer zaten giriş yapmışsa dashboard'a yönlendir
        if (User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("Kullanıcı zaten giriş yapmış, dashboard'a yönlendiriliyor");
            return RedirectToAction("Index", "Dashboard", new { area = "" });
        }

        ViewData["ReturnUrl"] = returnUrl;
        _logger.LogInformation("Login sayfası gösteriliyor. ReturnUrl: {ReturnUrl}", returnUrl ?? "null");
        return View();
    }

    /// <summary>
    /// POST: /admin/login
    /// Processes the login form
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        _logger.LogInformation("Login denemesi başladı. Email: {Email}, ReturnUrl: {ReturnUrl}", 
            email, returnUrl ?? "null");

        // Validation
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Login başarısız: Email veya şifre boş");
            ViewBag.Error = "Email ve şifre gereklidir";
            return View();
        }

        // Find user
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Login başarısız: Kullanıcı bulunamadı. Email: {Email}", email);
            ViewBag.Error = "Geçersiz email veya şifre";
            return View();
        }

        // Sign in
        var result = await _signInManager.PasswordSignInAsync(
            user, 
            password, 
            isPersistent: true, 
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("✅ Login başarılı! Kullanıcı: {Email} (ID: {UserId})", email, user.Id);
            
            // Return URL kontrolü
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                _logger.LogInformation("ReturnUrl'e yönlendiriliyor: {ReturnUrl}", returnUrl);
                return Redirect(returnUrl);
            }
            
            // Default: Dashboard'a yönlendir
            _logger.LogInformation("Dashboard'a yönlendiriliyor");
            return RedirectToAction("Index", "Dashboard", new { area = "" });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login başarısız: Hesap kilitli. Email: {Email}", email);
            ViewBag.Error = "Hesabınız kilitlenmiştir. Lütfen daha sonra tekrar deneyin.";
            return View();
        }

        if (result.IsNotAllowed)
        {
            _logger.LogWarning("Login başarısız: Giriş izni yok. Email: {Email}", email);
            ViewBag.Error = "Bu hesapla giriş yapmanıza izin verilmiyor.";
            return View();
        }

        _logger.LogWarning("Login başarısız: Geçersiz şifre. Email: {Email}", email);
        ViewBag.Error = "Geçersiz email veya şifre";
        return View();
    }

    /// <summary>
    /// GET/POST: /admin/logout
    /// Logs out the current user
    /// </summary>
    [HttpGet("logout")]
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.Identity?.Name;
        await _signInManager.SignOutAsync();
        
        _logger.LogInformation("✅ Logout başarılı. Kullanıcı: {Email}", userEmail ?? "Unknown");
        
        return RedirectToAction("Login");
    }

    /// <summary>
    /// GET: /admin/access-denied
    /// Shows access denied page
    /// </summary>
    [HttpGet("access-denied")]
    [AllowAnonymous]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        _logger.LogWarning("Access denied. ReturnUrl: {ReturnUrl}", returnUrl ?? "null");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
}
