using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EntryLog.Web.Controllers
{
    public class AccountController(IAppUserServices userServices) : Controller
    {
        [HttpGet("registro", Name = "Register")]
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


        //[HttpGet("login")]
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


        [HttpGet("cuenta/miperfil", Name = "MiPerfil")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyProfileAsync()
        {
            UserViewModel user = User.GetUserData()!;
            return View(await userServices.GetUserInfoAsync(user.NameIdentifier));
        }


        [HttpGet("cuenta/salir")]
        [Authorize]
        public async Task<IActionResult> LogOutAsync()
        {
            await HttpContext.SignOutCookiesAsync();
            return RedirectToAction("Login");
        }

        [HttpGet("cuenta/recuperar", Name = "GetRecover")]
        public IActionResult Recover()
        {
            return View();
        }

        [HttpPost("cuenta/recuperar", Name = "PostRecover")]
        public async Task<JsonResult> RecoverAsync(string email)
        {
            (bool success, string message) = await userServices.AccountRecoveryStartAsync(email);
            return Json(new
            {
                success,
                message
            });
        }

        [HttpGet("cuenta/completar_recuperar", Name = "GetCompleteRecover")]
        public async Task<IActionResult> CompleteRecover([FromQuery] string token)
        {
            (bool success, string message) = await userServices.ValidateRecoveryTokenAsync(token);
            ViewBag.Success = success;
            ViewBag.Message = message;
            return View();
        }

        [HttpPost("cuenta/completar_recuperar", Name = "PostCompleteRecover")]
        public async Task<JsonResult> CompleteRecoverAsync(AccountRecoveryDTO model)
        {
            (bool success, string message) = await userServices.AccountRecoveryCompleteAsync(model);
            return Json(new
            {
                success,
                message
            });
        }
    }
}
