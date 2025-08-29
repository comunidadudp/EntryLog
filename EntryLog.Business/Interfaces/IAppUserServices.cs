using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces
{
    public interface IAppUserServices
    {
        Task<(bool success, string message, LoginResponseDTO? data)> RegisterEmployeeAsync(CreateEmployeeUserDTO userDTO);
        Task<(bool success, string message, LoginResponseDTO? data)> UserLoginAsync(UserCredentialsDTO credentialsDTO);
        Task<(bool success, string message)> AccountRecoveryStartAsync(string username);
        Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDTO recoveryDTO);
        Task<UserInfoDTO> GetUserInfoAsync(int code);
    }
}
