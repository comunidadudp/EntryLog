using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EntryLog.Web.Controllers
{
    public class AccountController(IAppUserServices userServices) : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterEmployeeUser()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RegisterEmployeeUserAsync(CreateEmployeeUserDTO model)
        {
            (bool success, string message, LoginResponseDTO? data) = await userServices.RegisterEmployeeAsync(model);

            if (success)
            {
                //loguear al empleado
                await HttpContext.SignInCookiesAsync(data!);

                return Json(new
                {
                    success,
                    path = "/main/index"
                });
            }
            else
            {
                return Json(new
                {
                    success,
                    message
                });
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ClaimsPrincipal principal = HttpContext.User;

            if (principal.Identity != null)
            {
                if (principal.Identity.IsAuthenticated)
                {
                    return RedirectToAction("index", "main");
                }
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoginAsync(UserCredentialsDTO model)
        {
            (bool success, string message, LoginResponseDTO? data) = await userServices.UserLoginAsync(model);

            if (success)
            {
                //loguear al empleado
                await HttpContext.SignInCookiesAsync(data!);

                return Json(new
                {
                    success,
                    path = "/main/index"
                });
            }
            else
            {
                return Json(new
                {
                    success,
                    message
                });
            }
        }

        [HttpGet("cuenta/salir")]
        [Authorize]
        public async Task<IActionResult> LogOutAsync()
        {
            await HttpContext.SignOutCookiesAsync();
            return RedirectToAction("Login");
        }
    }
}
