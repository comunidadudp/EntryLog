using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Business.QueryFilters;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using EntryLog.Business.Specs;

namespace EntryLog.Business.Services
{
    internal class WorkSessionServices(
        IEmployeeRepository employeeRepository,
        IAppUserRepository userRepository,
        IWorkSessionRepository sessionRepository) : IWorkSessionServices
    {
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IAppUserRepository _userRepository = userRepository;
        private readonly IWorkSessionRepository _sessionRepository = sessionRepository;

        public async Task<(bool success, string message)> CloseJobSession(CloseJobSessionDTO sessionDTO)
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

            activeSession.CheckOut ??= new Check();
            activeSession.CheckOut.Method = sessionDTO.Method;
            activeSession.CheckOut.DeviceName = sessionDTO.DeviceName;
            activeSession.CheckOut.Date = DateTime.UtcNow;
            activeSession.CheckOut.Location.Latitude = sessionDTO.Latitude;
            activeSession.CheckOut.Location.Longitude = sessionDTO.Longitude;
            activeSession.CheckOut.Location.IpAddress = sessionDTO.IpAddress;
            activeSession.CheckOut.PhotoUrl = "";
            activeSession.CheckOut.Notes = sessionDTO.Notes;
            activeSession.Status = SessionStatus.Completed;

            await _sessionRepository.UpdateAsync(activeSession);

            return (true, "Se registró exitosamente la sesión");
        }

        public async Task<IEnumerable<GetWorkSessionDTO>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter)
        {
            var spec = new WorkSessionSpec();

            if (filter.EmployeeId.HasValue)
            {
                spec.AndAlso(x => x.EmployeeId == filter.EmployeeId.Value);
            }

            IEnumerable<WorkSession> sessions = await _sessionRepository.GetAllAsync(spec);

            return sessions.Select(WorkSessionMapper.MapToGetWorkSessionDTO);
        }


        public async Task<(bool success, string message)> OpenJobSession(CreateJoSessionDTO sessionDTO)
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

            //Crear la nueva sesion
            session = new WorkSession
            {
                EmployeeId = code,
                CheckIn = new Check
                {
                    Method = sessionDTO.Method,
                    DeviceName = sessionDTO.DeviceName,
                    Date = DateTime.UtcNow,
                    Location = new Location
                    {
                        Latitude = sessionDTO.Latitude,
                        Longitude = sessionDTO.Longitude,
                        IpAddress = sessionDTO.IpAddress,
                    },
                    Notes = sessionDTO.Notes,
                    PhotoUrl = ""
                },
                Status = SessionStatus.InProgress
            };

            //Guardar en base de datos
            await _sessionRepository.CreateAsync(session);

            //Respuesta
            return (true, "Sessión abierta exitosamente");
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
