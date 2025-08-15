using EntryLog.Business.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace EntryLog.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task SignInCookiesAsync(this HttpContext context, LoginResponseDTO userData)
        {
            List<Claim> claims =
            [
              new Claim(ClaimTypes.NameIdentifier, userData.DocumentNumber.ToString()),
              new Claim(ClaimTypes.Role, userData.Role),
              new Claim(ClaimTypes.Email, userData.Email),
              new Claim(ClaimTypes.Name, userData.Name),
            ];

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await context.SignInAsync(claimsPrincipal, properties);
        }

        public static async Task SignOutCookiesAsync(this HttpContext context)
        {
            context.Session.Clear();
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
