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

        [HttpGet("empleado/faceid/session")]
        public async Task<IActionResult> GenerateSecurityTokenAsync()
        {
            UserViewModel? user = User.GetUserData();

            if (user is null)
                return Unauthorized();

            var token = await faceIdService
                .GenerateReferenceImageTokenAsync(user.NameIdentifier.ToString());

            return Ok(new { token });
        }


        [HttpGet("empleado/faceid/reference")]
        public async Task<IActionResult> GetReferenceImageAsync([FromHeader(Name = "Authorization")] string authHeader)
        {
            UserViewModel? user = User.GetUserData();

            if (user is null)
                return Unauthorized();

            string imageBase64 = await faceIdService.GetReferenceImageAsync(authHeader);

            return Ok(new { imageBase64 });
        }
    }
}
