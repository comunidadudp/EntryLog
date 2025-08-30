using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mailtrap.Models;
using EntryLog.Business.Mappers;
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
        IUriService uriService,
        IHttpClientFactory httpClientFactory) : IAppUserServices
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IAppUserRepository _userRepository = userRepository;
        private readonly IPasswordHasherService _hasherService = hasherService;
        private readonly IEncryptionService _encryptionService = encryptionService;
        private readonly IEmailSenderService _emailSenderService = emailSenderService;
        private readonly IUriService _uriService = uriService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDTO recoveryDTO)
        {
            (bool success, string message, AppUser? user) = await ValidateRecoveryTokenInternalAsync(recoveryDTO.Token);

            if (!success && user is null)
                return (success, message);

            if (success)
            {
                user!.Password = _hasherService.Hash(recoveryDTO.Password);
                await FinalizeRecovery(user);
                return (true, "Contraseña actualizada correctamente");
            }
            else
            {
                await FinalizeRecovery(user!);
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
                Url = $"{_uriService.ApplicationURL}/cuenta/completar_recuperar?token={Uri.EscapeDataString(recoveryToken)}"
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

        public async Task<UserInfoDTO> GetUserInfoAsync(int code)
        {
            AppUser user = await _userRepository.GetByCodeAsync(code);
            Employee? employee = await _employeeRepository.GetByCodeAsync(code);

            string base64Image = string.Empty;

            if (user?.FaceID is not null)
            {
                base64Image = await GenerateBase64PngImageAsync(user.FaceID.ImageURL);
            }

            return AppUserMapper.MapToUserInfoDTO(user, employee!, base64Image);
        }


        private async Task<string> GenerateBase64PngImageAsync(string imageUrl)
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
            var prefix = $"data:{contentType};base64,";
            return prefix + Convert.ToBase64String(imageBytes);
        }

        public async Task<(bool success, string message)> ValidateRecoveryTokenAsync(string token)
        {
            (bool success, string message, AppUser? _) = await ValidateRecoveryTokenInternalAsync(token);
            return (success, message);
        }

        private async Task<(bool success, string message, AppUser? user)> 
            ValidateRecoveryTokenInternalAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return (false, "Token inválido", null);

            string recoveryTokenPlain;

            try
            {
                recoveryTokenPlain = _encryptionService.Decrypt(token);
            }
            catch
            {
                return (false, "Token inválido", null);
            }

            if (string.IsNullOrEmpty(recoveryTokenPlain) || !recoveryTokenPlain.Contains(':'))
                return (false, "Token inválido", null);

            string[] parts = recoveryTokenPlain.Split(':');

            if (parts.Length != 2 || !long.TryParse(parts[0], out long ticks))
                return (false, "Token inválido", null);

            string username = parts[1];

            AppUser user = await _userRepository.GetByRecoveryTokenAsync(token);
            if (user == null || !string.Equals(username, user.Email, StringComparison.OrdinalIgnoreCase))
                return (false, "Token inválido", null);

            var tokenDate = DateTime.FromBinary(ticks);
            var now = DateTime.UtcNow;

            const int expirationMinutes = 30;

            if ((now - tokenDate).TotalMinutes <= expirationMinutes)
            {
                return (true, "Token válido", user!);
            }
            else
            {
                return (false, "Token vencido", user!);
            }
        }
    }
}
