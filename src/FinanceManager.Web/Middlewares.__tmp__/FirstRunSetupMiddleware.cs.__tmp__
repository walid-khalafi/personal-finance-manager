using FinanceManager.Infrastructure.Services;

namespace FinanceManager.Web.Middlewares;

/// <summary>
/// در نخستین اجرا (وقتی هنوز هیچ کاربری ساخته نشده) همه درخواستها
/// به صفحه راهاندازی هدایت میشوند تا دسترسی به دادههای مالی قفل بماند.
/// </summary>
public class FirstRunSetupMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AuthService authService)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            var path = context.Request.Path;
            var isAuthPath = path.StartsWithSegments("/auth/setup")
                || path.StartsWithSegments("/auth/login")
                || path.StartsWithSegments("/auth/logout");

            if (!isAuthPath && !await authService.HasAnyUserAsync(context.RequestAborted))
            {
                context.Response.Redirect("/auth/setup");
                return;
            }
        }

        await next(context);
    }
}
