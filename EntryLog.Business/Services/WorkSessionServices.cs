using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;
using EntryLog.Business.Specs;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using System.Text.Json;

namespace EntryLog.Business.Services
{
    internal class WorkSessionServices(
        IEmployeeRepository employeeRepository,
        IAppUserRepository userRepository,
        IWorkSessionRepository sessionRepository,
        ILoadImagesService loadImagesService,
        IUriService uriService) : IWorkSessionServices
    {
        private const int DescriptorLength = 128;

        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IAppUserRepository _userRepository = userRepository;
        private readonly IWorkSessionRepository _sessionRepository = sessionRepository;
        private readonly ILoadImagesService _loadImagesService = loadImagesService;
        private readonly IUriService _uriService = uriService;


        public async Task<(bool success, string message, GetWorkSessionDTO? data)> OpenSessionAsync(OpenSessionDTO sessionDTO)
        {
            int code = int.Parse(sessionDTO.UserId);

            var (success, message) = await ValidateEmployeeUserAsync(code);

            if (!success)
                return (success, message, null);

            // Verificar si el usuario actual tiene una sesion abierta
            WorkSession session = await _sessionRepository.GetActiveSessionByEmployeeIdAsync(code);

            if (session != null)
            {
                return (false, "El empleado tiene una sesiòn activa", null);
            }

            //Insercion de imagen
            string ext = Path.GetExtension(sessionDTO.Image.FileName);
            string filename = $"checkin-{DateTime.UtcNow}{ext}";

            string imageUrl = await _loadImagesService
                .UploadAsync(sessionDTO.Image.OpenReadStream(), sessionDTO.Image.ContentType, filename);

            List<float>? descriptor;

            try
            {
                descriptor = JsonSerializer.Deserialize<List<float>>(sessionDTO.Descriptor);
            }
            catch (JsonException)
            {
                return (false, "Descriptor JSON no válido", null);
            }

            if (descriptor is null || descriptor.Count != DescriptorLength)
                return (false, "Descriptor no válido", null);

            (success, message) = await ValidateEmployeeFaceDescriptorAsync(code, [.. descriptor]);

            if (!success)
                return (success, message, null);

            //Crear la nueva sesion
            session = new WorkSession
            {
                EmployeeId = code,
                CheckIn = new Check
                {
                    Method = _uriService.UserAgent,
                    DeviceName = _uriService.Platform,
                    Date = DateTime.UtcNow,
                    Location = new Location
                    {
                        Latitude = sessionDTO.Latitude,
                        Longitude = sessionDTO.Longitude,
                        Country = sessionDTO.Country,
                        City = sessionDTO.City,
                        Neighbourhood = sessionDTO.Neighbourhood,
                        IpAddress = _uriService.RemoteIpAddress,
                    },
                    Notes = sessionDTO.Notes,
                    PhotoUrl = imageUrl,
                    Descriptor = descriptor
                },
                Status = SessionStatus.InProgress
            };

            //Guardar en base de datos
            await _sessionRepository.CreateAsync(session);

            //Respuesta
            return (true, "Sesión abierta exitosamente", WorkSessionMapper.MapToGetWorkSessionDTO(session));
        }

        public async Task<(bool success, string message, GetWorkSessionDTO? data)> CloseSessionAsync(CloseSessionDTO sessionDTO)
        {
            int code = int.Parse(sessionDTO.UserId);

            var (success, message) = await ValidateEmployeeUserAsync(code);

            if (!success)
                return (success, message, null);

            WorkSession activeSession = await _sessionRepository.GetActiveSessionByEmployeeIdAsync(code);

            if (activeSession is null)
            {
                return (false, "No existe una sesión activa para el empleado", null);
            }

            //Insercion de imagen
            string ext = Path.GetExtension(sessionDTO.Image.FileName);
            string filename = $"checkout-{DateTime.UtcNow}{ext}";

            string imageUrl = await _loadImagesService
                .UploadAsync(sessionDTO.Image.OpenReadStream(), sessionDTO.Image.ContentType, filename);

            List<float>? descriptor;

            try
            {
                descriptor = JsonSerializer.Deserialize<List<float>>(sessionDTO.Descriptor);
            }
            catch (JsonException)
            {
                return (false, "Descriptor JSON no válido", null);
            }

            if (descriptor is null || descriptor.Count != DescriptorLength)
                return (false, "Descriptor no válido", null);

            activeSession.CheckOut ??= new Check();
            activeSession.CheckOut.Method = _uriService.UserAgent;
            activeSession.CheckOut.DeviceName = _uriService.Platform;
            activeSession.CheckOut.Date = DateTime.UtcNow;
            activeSession.CheckOut.Location.Latitude = sessionDTO.Latitude;
            activeSession.CheckOut.Location.Longitude = sessionDTO.Longitude;
            activeSession.CheckOut.Location.Country = sessionDTO.Country;
            activeSession.CheckOut.Location.City = sessionDTO.City;
            activeSession.CheckOut.Location.Neighbourhood = sessionDTO.Neighbourhood;
            activeSession.CheckOut.Location.IpAddress = _uriService.RemoteIpAddress;
            activeSession.CheckOut.PhotoUrl = imageUrl;
            activeSession.CheckOut.Notes = sessionDTO.Notes;
            activeSession.CheckOut.Descriptor = descriptor;
            activeSession.Status = SessionStatus.Completed;

            await _sessionRepository.UpdateAsync(activeSession);

            return (true, "Sesión cerrada exitosamente", WorkSessionMapper.MapToGetWorkSessionDTO(activeSession));
        }

        public async Task<PaginatedResult<GetWorkSessionDTO>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter)
        {
            var spec = new WorkSessionSpec();

            //filtrar por empleado
            if (filter.EmployeeId.HasValue)
            {
                spec.AndAlso(x => x.EmployeeId == filter.EmployeeId.Value);
            }

            //contar los registros que coinciden con ese filtro
            int count = await _sessionRepository.CountAsync(spec);

            //aplicar la paginación
            filter.PageIndex ??= 1;
            filter.PageSize ??= 10;
            filter.PageSize = Math.Min(filter.PageSize.Value, 50);

            spec.ApplyPagging(filter.PageSize.Value, filter.PageSize.Value * (filter.PageIndex.Value - 1));

            //aplicar ordenamiento
            switch (filter.Sort)
            {
                case Enums.SortType.Ascending:
                    spec.AddOrderBy(x => x.CheckIn.Date);
                    break;
                case Enums.SortType.Descending:
                    spec.AddOrderByDescending(x => x.CheckIn.Date);
                    break;
                default:
                    spec.AddOrderByDescending(x => x.CheckIn.Date);
                    break;
            }

            //paginacion en el servidor de base de datos
            IEnumerable<WorkSession> sessions = await _sessionRepository.GetAllAsync(spec);

            IEnumerable<GetWorkSessionDTO> results = sessions.Select(WorkSessionMapper.MapToGetWorkSessionDTO);

            return PaginatedResult<GetWorkSessionDTO>.Create(results, filter, count);
        }


        private async Task<(bool success, string message)> ValidateEmployeeUserAsync(int code)
        {
            Employee? employee = await _employeeRepository.GetByCodeAsync(code);

            if (employee == null)
                return (false, "El empleado no existe");

            AppUser user = await _userRepository.GetByCodeAsync(code);

            if (user == null)
                return (false, "El usuario no existe");

            return (true, "");
        }

        private async Task<(bool success, string message)> ValidateEmployeeFaceDescriptorAsync
            (int employeeCode, float[] currentDescriptor)
        {
            if (currentDescriptor is null || currentDescriptor.Length != 128)
                return (false, "Descriptor no válido");

            AppUser user = await _userRepository.GetByCodeAsync(employeeCode);
            List<float> storedDescriptor = user.FaceID!.Descriptor;

            double distance = EuclideanDistance(currentDescriptor, [.. storedDescriptor]);
            bool match = distance < 0.5;

            return (match, match ? "" : "El rostro no coincide con el FaceId registrado");
        }

        private static double EuclideanDistance(float[] a, float[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(sum);
        }

        public async Task<bool> HasActiveAnySessionAsync(int employeeCode)
        {
            WorkSession session = await _sessionRepository.GetActiveSessionByEmployeeIdAsync(employeeCode);
            return session is not null;
        }

        public async Task<GetWorkSessionDTO> GetSessionByIdAsync(string id)
        {
            Guid guid = Guid.Parse(id);
            WorkSession session = await _sessionRepository.GetByIdAsync(guid);
            return WorkSessionMapper.MapToGetWorkSessionDTO(session);
        }

        public async Task<IEnumerable<GetLocationDTO>> GetLastLocationsByEmployeeAsync(int employeeCode)
        {
            var filter = new WorkSessionQueryFilter
            {
                EmployeeId = employeeCode,
                Sort = Enums.SortType.Descending,
                PageIndex = 1,
                PageSize = 5
            };

            PaginatedResult<GetWorkSessionDTO> paginatedResult = await GetSessionListByFilterAsync(filter);
            IEnumerable<GetWorkSessionDTO>? lastSessions = paginatedResult.ResultsCount > 0 ? paginatedResult.Results : [];
            IEnumerable<GetLocationDTO> lastCheckInLocations = lastSessions?.Select(session => session.CheckIn.Location) ?? [];
            return lastCheckInLocations;
        }
    }
}
