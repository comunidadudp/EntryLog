using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;
using EntryLog.Business.Specs;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Services
{
    internal class WorkSessionServices(
        IEmployeeRepository employeeRepository,
        IAppUserRepository userRepository,
        IWorkSessionRepository sessionRepository,
        ILoadImagesService loadImagesService,
        IUriService uriService) : IWorkSessionServices
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IAppUserRepository _userRepository = userRepository;
        private readonly IWorkSessionRepository _sessionRepository = sessionRepository;
        private readonly ILoadImagesService _loadImagesService = loadImagesService;
        private readonly IUriService _uriService = uriService;


        public async Task<(bool success, string message)> OpenJobSessionAsync(CreateJobSessionDTO sessionDTO)
        {
            int code = int.Parse(sessionDTO.UserId);

            var (success, message) = await ValidateEmployeeUserAsync(code);

            if (!success)
                return (success, message);

            // Verificar si el usuario actual tiene una sesion abierta
            WorkSession session = await _sessionRepository.GetActiveSessionByEmployeeIdAsync(code);

            if (session != null)
            {
                return (false, "El empleado tiene una sesiòn activa");
            }

            //Insercion de imagen
            string filename = sessionDTO.Image.FileName;
            string ext = Path.GetExtension(sessionDTO.Image.FileName);

            ImageBBResponseDTO imageBB = await _loadImagesService
                .UploadAsync(sessionDTO.Image.OpenReadStream(), sessionDTO.Image.ContentType, filename, ext);

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
                        IpAddress = _uriService.RemoteIpAddress,
                    },
                    Notes = sessionDTO.Notes,
                    PhotoUrl = imageBB.Data.Url
                },
                Status = SessionStatus.InProgress
            };

            //Guardar en base de datos
            await _sessionRepository.CreateAsync(session);

            //Respuesta
            return (true, "Sesión abierta exitosamente");
        }

        public async Task<(bool success, string message)> CloseJobSessionAsync(CloseJobSessionDTO sessionDTO)
        {
            int code = int.Parse(sessionDTO.UserId);

            var (success, message) = await ValidateEmployeeUserAsync(code);

            if (!success)
                return (success, message);

            WorkSession activeSession = await _sessionRepository.GetActiveSessionByEmployeeIdAsync(code);

            if (activeSession is null)
            {
                return (false, "No existe una sesión activa para el empleado");
            }

            //Insercion de imagen
            string filename = sessionDTO.Image.FileName;
            string ext = Path.GetExtension(sessionDTO.Image.FileName);

            ImageBBResponseDTO imageBB = await _loadImagesService
                .UploadAsync(sessionDTO.Image.OpenReadStream(), sessionDTO.Image.ContentType, filename, ext);

            activeSession.CheckOut ??= new Check();
            activeSession.CheckOut.Method = _uriService.UserAgent;
            activeSession.CheckOut.DeviceName = _uriService.Platform;
            activeSession.CheckOut.Date = DateTime.UtcNow;
            activeSession.CheckOut.Location.Latitude = sessionDTO.Latitude;
            activeSession.CheckOut.Location.Longitude = sessionDTO.Longitude;
            activeSession.CheckOut.Location.IpAddress = _uriService.RemoteIpAddress;
            activeSession.CheckOut.PhotoUrl = imageBB.Data.Url;
            activeSession.CheckOut.Notes = sessionDTO.Notes;
            activeSession.Status = SessionStatus.Completed;

            await _sessionRepository.UpdateAsync(activeSession);

            return (true, "Sesión cerrada exitosamente");
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
    }
}
