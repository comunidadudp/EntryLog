using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
