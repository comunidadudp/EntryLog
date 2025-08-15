using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
