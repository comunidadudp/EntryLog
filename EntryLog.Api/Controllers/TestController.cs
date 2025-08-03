using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EntryLog.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class TestController(
        IAppUserServices appUserServices,
        IWorkSessionServices workSessionServices) : ControllerBase
    {
        [HttpPost("user/register-employee-user")]
        public async Task<object> CreateUserEmployeeAsync([FromBody] CreateEmployeeUserDTO employeeUserDTO)
        {
            (bool success, string message, LoginResponseDTO? data) = await appUserServices.RegisterEmployeeAsync(employeeUserDTO);
            return Ok(new
            {
                success,
                message,
                data
            });
        }


        [HttpPost("user/login")]
        public async Task<object> UserLoginAsync([FromBody] UserCredentialsDTO credentialsDTO)
        {
            (bool success,string message, LoginResponseDTO? data) = await appUserServices.UserLoginAsync(credentialsDTO);
            return Ok(new
            {
                success,
                message,
                data
            });
        }

        [HttpPost("user/recovery-start")]
        public async Task<object> AccountRecoveryStartAsync([FromBody] string username)
        {
            (bool success, string message) = await appUserServices.AccountRecoveryStartAsync(username);
            return Ok(new
            {
                success,
                message,
            });
        }

        [HttpPost("user/recovery-complete")]
        public async Task<object> AccountRecoveryCompleteAsync([FromBody] AccountRecoveryDTO recoveryDTO)
        {
            (bool success, string message) = await appUserServices.AccountRecoveryCompleteAsync(recoveryDTO);
            return Ok(new
            {
                success,
                message,
            });
        }

        [HttpPost("test-image")]
        [Consumes("multipart/form-data")]
        public async Task<object> ImageTestAsync([FromForm] ImageUploadRequest request)
        {
            (bool success, string message) = await workSessionServices.ImageTestAsync(request.Image);
            return Ok(new
            {
                success,
                message,
            });
        }
    }

    public class ImageUploadRequest
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}
