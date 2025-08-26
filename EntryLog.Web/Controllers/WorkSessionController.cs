using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class WorkSessionController(
        IWorkSessionServices workSessionServices,
        IFaceIdService faceIdService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            UserViewModel userData = User.GetUserData()!;

            EmployeeFaceIdDTO faceId = await faceIdService.GetFaceIdAsync(userData.NameIdentifier);
            ViewBag.IsFaceIdActive = faceId.Active;

            bool hasActiveSession = await workSessionServices.HasActiveAnySessionAsync(userData.NameIdentifier);
            ViewBag.HasActiveSession = hasActiveSession;

            PaginatedResult<GetWorkSessionDTO> model = await workSessionServices.GetSessionListByFilterAsync(new WorkSessionQueryFilter
            {
                EmployeeId = userData.NameIdentifier,
                Sort = Business.Enums.SortType.Descending
            });

            return View(model);
        }


        [HttpPost("empleado/sesiones/abrir")]
        public async Task<JsonResult> OpenWorkSessionAsync(OpenWorkSessionViewModel model)
        {
            UserViewModel userData = User.GetUserData()!;

            (bool success, string message, GetWorkSessionDTO? data) = await workSessionServices.OpenJobSessionAsync(
                new CreateWorkSessionDTO(
                    userData.NameIdentifier.ToString(),
                    model.Latitude,
                    model.Longitude,
                    model.Image,
                    model.Notes,
                    model.Descriptor));

            return Json(new
            {
                success,
                message,
                data
            });
        }

        [HttpPost("empleado/sesiones/cerrar")]
        public async Task<JsonResult> CloseWorkSessionAsync(CloseWorkSessionViewModel model)
        {
            UserViewModel userData = User.GetUserData()!;
            (bool success, string message, GetWorkSessionDTO? data) = await workSessionServices.CloseJobSessionAsync(
                new CloseWorkSessionDTO(
                    model.SessionId,
                    userData.NameIdentifier.ToString(),
                    model.Latitude,
                    model.Longitude,
                    model.Image,
                    model.Notes,
                    model.Descriptor));
            return Json(new
            {
                success,
                message,
                data
            });
        }
    }
}
