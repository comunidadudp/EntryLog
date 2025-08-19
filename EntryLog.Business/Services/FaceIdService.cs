using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace EntryLog.Business.Services
{
    internal class FaceIdService(
        IAppUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILoadImagesService imagesService,
        IHttpClientFactory httpClientFactory,
        IJwtService jwtService) : IFaceIdService
    {
        private const int DescriptorLength = 128;

        private readonly IAppUserRepository _userRepository = userRepository;
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly ILoadImagesService _imagesService = imagesService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IJwtService _jwtService = jwtService;

        public async Task<(bool success, string message, EmployeeFaceIdDTO? data)>
            CreateEmployeeFaceIdAsync(AddEmployeeFaceIdDTO faceIdDTO)
        {
            if (faceIdDTO.Image == null || faceIdDTO.Image.Length == 0)
                return (false, "La imagen es olbigatoria", null);

            if (string.IsNullOrWhiteSpace(faceIdDTO.Descriptor))
                return (false, "El descriptor es obligatorio", null);

            List<float>? descriptor;

            try
            {
                descriptor = JsonSerializer.Deserialize<List<float>>(faceIdDTO.Descriptor);
            }
            catch (JsonException)
            {
                return (false, "Descriptor JSON no válido", null);
            }

            if (descriptor is null || descriptor.Count != DescriptorLength)
                return (false, "Descriptor no válido", null);


            var userTask = _userRepository.GetByCodeAsync(faceIdDTO.EmployeeCode);
            var employeeTask = _employeeRepository.GetByCodeAsync(faceIdDTO.EmployeeCode);

            await Task.WhenAll(userTask, employeeTask);

            AppUser user = await userTask;
            Employee? employee = await employeeTask;

            if (user == null)
                return (false, "Usuario no encontrado", null);

            if (employee == null)
                return (false, "Empleado no encontrado", null);

            if (user.FaceID != null && user.FaceID.Active)
                return (false, "FaceID ya fue configurado", null);

            var ext = Path.GetExtension(faceIdDTO.Image.FileName);
            if (string.IsNullOrEmpty(ext))
                ext = ".png";

            string fileName = $"faceid-{user.Id}{ext}";

            string imageUrl;

            try
            {
                imageUrl = await _imagesService.UploadAsync(faceIdDTO.Image.OpenReadStream(), faceIdDTO.Image.ContentType, fileName);
            }
            catch (Exception)
            {
                return (false, "No se puedo cargar la imagen", null);
            }

            user.FaceID = new FaceID
            {
                ImageURL = imageUrl,
                RegisterDate = DateTime.UtcNow,
                Descriptor = descriptor,
                Active = true
            };

            await _userRepository.UpdateAsync(user);

            string base64Image = await GenerateBase64PngImageAsync(imageUrl);

            return (true, "Se ha creado correctamente el FaceID", FaceIdMapper.MapToEmployeeFaceIdDTO(user.FaceID, base64Image));
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

        public async Task<string> GenerateReferenceImageTokenAsync(string userId)
            => await _jwtService.GenerateTokenAsync(userId, "faceid_reference", TimeSpan.FromSeconds(60));

        public async Task<EmployeeFaceIdDTO> GetFaceIdAsync(int employeeCode)
        {
            AppUser user = await _userRepository.GetByCodeAsync(employeeCode);

            if (user == null)
                return FaceIdMapper.Empty();

            FaceID? faceId = user.FaceID;

            string base64Image = faceId != null ? await GenerateBase64PngImageAsync(faceId!.ImageURL) : string.Empty;

            EmployeeFaceIdDTO faceIdDTO = faceId != null
                ? FaceIdMapper.MapToEmployeeFaceIdDTO(faceId, base64Image)
                : FaceIdMapper.Empty();

            return faceIdDTO;
        }

        public async Task<string> GetReferenceImageAsync(string authHeader)
        {
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var claims = _jwtService.ValidateToken(token);

            if (claims == null || !claims.TryGetValue("purpose", out var purpose) || purpose?.ToString() != "faceid_reference")
                return string.Empty;

            if (!claims.TryGetValue(JwtRegisteredClaimNames.Sub, out var nameId) || int.TryParse(nameId?.ToString(), out var code))
                return string.Empty;

            var faceIdDTO = await GetFaceIdAsync(code);
            return faceIdDTO?.Base64Image ?? string.Empty;
        }
    }
}
