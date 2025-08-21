using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class FaceIdController(IFaceIdService faceIdService) : Controller
    {
        public async Task< IActionResult> Index()
        {
            UserViewModel userData = User.GetUserData()!;
            return View(await faceIdService.GetFaceIdAsync(userData.NameIdentifier));
        }

        [HttpPost("empleado/faceid")]
        public async Task<JsonResult> CreateAsync([FromForm] AddEmployeeFaceIdDTO faceIdDTO)
        {
            UserViewModel user = User.GetUserData()!;

            (bool success, string message, EmployeeFaceIdDTO? data) = await faceIdService.CreateEmployeeFaceIdAsync(
                new AddEmployeeFaceIdDTO(user.NameIdentifier, faceIdDTO.Image, faceIdDTO.Descriptor));

            return Json(new
            {
                success,
                message,
                data
            });
        }
    }
}
