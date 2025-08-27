using EntryLog.Business.DTOs;
using EntryLog.Business.Utils;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Mappers
{
    internal static class WorkSessionMapper
    {
        public static GetWorkSessionDTO MapToGetWorkSessionDTO(WorkSession session)
        {
            return new GetWorkSessionDTO(
                session.Id.ToString(),
                session.EmployeeId,
                new GetCheckDTO(
                    session.CheckIn.Method,
                    session.CheckIn.DeviceName,
                    TimeFunctions.GetSAPacificStandardTime(session.CheckIn.Date).ToString("yyyy-MM-dd hh:mm tt"),
                    new GetLocationDTO(
                        session.CheckIn.Location.Latitude,
                        session.CheckIn.Location.Longitude,
                        session.CheckIn.Location.IpAddress),
                    session.CheckIn.PhotoUrl,
                    session.CheckIn.Notes),
                session.CheckOut != null ? new GetCheckDTO(
                    session.CheckOut.Method,
                    session.CheckOut.DeviceName,
                    TimeFunctions.GetSAPacificStandardTime(session.CheckOut.Date).ToString("yyyy-MM-dd hh:mm tt"),
                    new GetLocationDTO(
                        session.CheckOut.Location.Latitude,
                        session.CheckOut.Location.Longitude,
                        session.CheckOut.Location.IpAddress),
                    session.CheckOut.PhotoUrl,
                    session.CheckOut.Notes) : null,
                session.TotalWorked?.ToString(@"hh\:mm\:ss"),
                session.Status.ToString());
        }
    }
}
