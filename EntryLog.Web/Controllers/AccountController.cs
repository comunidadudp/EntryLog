using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<JsonResult> RegisterEmployeeUserAsync(CreateEmployeeUserDTO model)
        {
            (bool success, string message, LoginResponseDTO? data) = await userServices.RegisterEmployeeAsync(model);

            if (success)
            {
                //loguear al empleado
                

                return Json(new
                {
                    success,
                    path = "/main"
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
    }
}
