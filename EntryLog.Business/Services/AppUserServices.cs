using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mailtrap.Models;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Services
{
    internal class AppUserServices(
        IEmployeeRepository employeeRepository,
        IAppUserRepository userRepository,
        IPasswordHasherService hasherService,
        IEncryptionService encryptionService,
        IEmailSenderService emailSenderService,
        IUriService uriService) : IAppUserServices
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IAppUserRepository _userRepository = userRepository;
        private readonly IPasswordHasherService _hasherService = hasherService;
        private readonly IEncryptionService _encryptionService = encryptionService;
        private readonly IEmailSenderService _emailSenderService = emailSenderService;
        private readonly IUriService _uriService = uriService;

        public async Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDTO recoveryDTO)
        {
            if (string.IsNullOrWhiteSpace(recoveryDTO.Token))
                return (false, "Token inválido");

            string recoveryTokenPlain;

            try
            {
                recoveryTokenPlain = _encryptionService.Decrypt(recoveryDTO.Token);
            }
            catch
            {
                return (false, "Token inválido");
            }

            if (string.IsNullOrEmpty(recoveryTokenPlain) || !recoveryTokenPlain.Contains(':'))
                return (false, "Token inválido");

            string[] parts = recoveryTokenPlain.Split(':');

            if (parts.Length != 2 || !long.TryParse(parts[0], out long ticks))
                return (false, "Token inválido");

            string username = parts[1];

            AppUser user = await _userRepository.GetByRecoveryTokenAsync(recoveryDTO.Token);
            if (user == null || !string.Equals(username, user.Email, StringComparison.OrdinalIgnoreCase))
                return (false, "Token inválido");
                
            var tokenDate = DateTime.FromBinary(ticks);
            var now = DateTime.UtcNow;

            const int expirationMinutes = 30;

            if ((now - tokenDate).TotalMinutes <= expirationMinutes)
            {
                user.Password = _hasherService.Hash(recoveryDTO.Password);
                await FinalizeRecovery(user);
                return (true, "Contraseña actualizada correctamente");
            }
            else
            {
                await FinalizeRecovery(user);
                return (false, "Token vencido");
            }
        }

        private async Task FinalizeRecovery(AppUser user)
        {
            user.RecoveryToken = null;
            user.RecoveryTokenActive = false;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<(bool success, string message)> AccountRecoveryStartAsync(string username)
        {
            AppUser user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
                return (false, "No se ha podido recuperar tu cuenta");

            if (!user.Active)
                return (false, "No se ha podido recuperar tu cuenta");

            string recoveyTokenPlain = $"{DateTime.UtcNow.Ticks}:{user.Email}";

            string recoveryToken = _encryptionService.Encrypt(recoveyTokenPlain);

            user.RecoveryToken = recoveryToken;
            user.RecoveryTokenActive = true;

            await _userRepository.UpdateAsync(user);

            var vars = new RecoveryAccountVariables
            {
                Name = user.Name,
                Url = $"{_uriService.ApplicationURL}/account/recovery?token={recoveryToken}"
            };

            bool isSend = await _emailSenderService.SendEmailWithTemplateAsync("RecoveryToken", user.Email, vars);

            return (isSend, isSend ? $"Se ha enviado las instrucciones a tu cuenta {user.Email}" : "Ha ocurrido un error al enviar el correo");
        }

        public async Task<(bool success, string message, LoginResponseDTO? data)> RegisterEmployeeAsync(CreateEmployeeUserDTO userDTO)
        {
            int code = int.Parse(userDTO.DocumentNumber);

            Employee? employee = await _employeeRepository.GetByCodeAsync(code);

            if (employee == null)
                return (false, "El empleado no existe", null);

            AppUser user = await _userRepository.GetByCodeAsync(code);

            if (user != null)
                return (false, "El usuario ya existe", null);

            user = await _userRepository.GetByUsernameAsync(userDTO.Username);

            if (user != null)
                return (false, "El usuario ya existe", null);

            if (!userDTO.Password.Equals(userDTO.PasswordConf))
                return (false, "Las contraseñas no coinciden", null);

            user = new AppUser
            {
                Code = code,
                Name = employee.FullName,
                Role = Entities.Enums.RoleType.Employee,
                Email = userDTO.Username,
                CellPhone = userDTO.CellPhone,
                Password = _hasherService.Hash(userDTO.Password),
                Attempts = 0,
                RecoveryTokenActive = false,
                Active = true
            };

            await _userRepository.CreateAsync(user);

            return (true, "Empleado creado exitosamente", new LoginResponseDTO(user.Code, user.Role.ToString(), user.Email, user.Name));
        }

        public async Task<(bool success, string message, LoginResponseDTO? data)> UserLoginAsync(UserCredentialsDTO credentialsDTO)
        {
            AppUser user = await _userRepository.GetByUsernameAsync(credentialsDTO.Username);

            if (user == null)
                return (false, "Usuario y/o contraseña incorrecta", null);

            if (!user.Active)
                return (false, "Ha ocurrido un error. Contacte el administrador", null);

            bool accessGranted = _hasherService.Verify(credentialsDTO.Password, user.Password);

            if (!accessGranted)
                return (false, "Usuario y/o contraseña incorrecta", null);

            return (true, "Login successfull", new LoginResponseDTO(user.Code, user.Role.ToString(), user.Email, user.Name));
        }
    }
}
