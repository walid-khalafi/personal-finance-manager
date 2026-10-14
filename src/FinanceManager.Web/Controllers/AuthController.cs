using System.Security.Claims;
using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

[AllowAnonymous]
public class AuthController(AuthService auth) : Controller
{
    [HttpGet("/auth/login")]
    public async Task<IActionResult> Login(string? returnUrl = null, CancellationToken ct = default)
    {
        if (!await auth.HasAnyUserAsync(ct))
        {
            return RedirectToAction(nameof(Setup));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await auth.ValidateCredentialsAsync(model.Username, model.Password, ct);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "نام کاربری یا گذرواژه نادرست است.");
            return View(model);
        }

        await SignInAsync(user, model.RememberMe);
        var target = Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/";
        return LocalRedirect(target);
    }

    [HttpGet("/auth/setup")]
    public async Task<IActionResult> Setup(CancellationToken ct = default)
    {
        if (await auth.HasAnyUserAsync(ct))
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new SetupViewModel());
    }

    [HttpPost("/auth/setup")]
    public async Task<IActionResult> Setup(SetupViewModel model, CancellationToken ct = default)
    {
        if (await auth.HasAnyUserAsync(ct))
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await auth.CreateInitialUserAsync(model.Username, model.Password, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return View(model);
        }

        var user = await auth.ValidateCredentialsAsync(model.Username, model.Password, ct);
        if (user is not null)
        {
            await SignInAsync(user, persistent: true);
        }

        TempData["Success"] = "حساب کاربری ساخته شد؛ خوش آمدید!";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("/auth/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(AppUser user, bool persistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = new AuthenticationProperties
        {
            IsPersistent = persistent,
        };

        if (persistent)
        {
            properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            properties);
    }
}
