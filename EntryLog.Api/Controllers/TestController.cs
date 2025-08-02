using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class TestController(
        IAppUserServices appUserServices) : ControllerBase
    {
        [HttpPost("register-employee")]
        public async Task<object> CreateUserEmployeeAsync([FromBody] CreateEmployeeUserDTO employeeUserDTO)
        {
            return await appUserServices.RegisterEmployeeAsync(employeeUserDTO);
        }
    }
}
