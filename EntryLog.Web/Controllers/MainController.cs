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
        public async Task< IActionResult> Index()
        {
            UserViewModel user = User.GetUserData()!;
            var lastLocations = await workSessionServices.GetLastLocationsByEmployeeAsync(user.NameIdentifier);

            return View();
        }


        //[HttpGet("menu/ultimas_locaciones")]
        //public IActionResult LastEmployeeLocationsAsync()
        //{

        //}
    }
}
