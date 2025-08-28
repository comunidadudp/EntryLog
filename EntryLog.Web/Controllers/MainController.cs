using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class MainController(IWorkSessionServices workSessionServices) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("menu/ultimas_locaciones")]
        public async Task<JsonResult> LastEmployeeLocationsAsync()
        {
            UserViewModel user = User.GetUserData()!;
            var locations = await workSessionServices.GetLastLocationsByEmployeeAsync(user.NameIdentifier);
            return Json(new
            {
                locations = locations.ToArray()
            });
        }
    }
}
