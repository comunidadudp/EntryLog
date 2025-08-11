using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(
        IAppUserServices appUserServices) : ControllerBase
    {
        [HttpPost("register-employee-user")]
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


        [HttpPost("login")]
        public async Task<object> UserLoginAsync([FromBody] UserCredentialsDTO credentialsDTO)
        {
            (bool success, string message, LoginResponseDTO? data) = await appUserServices.UserLoginAsync(credentialsDTO);
            return Ok(new
            {
                success,
                message,
                data
            });
        }

        [HttpPost("recovery-start")]
        public async Task<object> AccountRecoveryStartAsync([FromBody] string username)
        {
            (bool success, string message) = await appUserServices.AccountRecoveryStartAsync(username);
            return Ok(new
            {
                success,
                message,
            });
        }

        [HttpPost("recovery-complete")]
        public async Task<object> AccountRecoveryCompleteAsync([FromBody] AccountRecoveryDTO recoveryDTO)
        {
            (bool success, string message) = await appUserServices.AccountRecoveryCompleteAsync(recoveryDTO);
            return Ok(new
            {
                success,
                message,
            });
        }
    }
}
