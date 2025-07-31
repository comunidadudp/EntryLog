using EntryLog.Business.DTOs;
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
                    session.CheckIn.Date,
                    new GetLocationDTO(
                        session.CheckIn.Location.Latitude,
                        session.CheckIn.Location.Longitude,
                        session.CheckIn.Location.IpAddress),
                    session.CheckIn.PhotoUrl,
                    session.CheckIn.Notes),
                session.CheckOut != null ? new GetCheckDTO(
                    session.CheckOut.Method,
                    session.CheckOut.DeviceName,
                    session.CheckOut.Date,
                    new GetLocationDTO(
                        session.CheckOut.Location.Latitude,
                        session.CheckOut.Location.Longitude,
                        session.CheckOut.Location.IpAddress),
                    session.CheckOut.PhotoUrl,
                    session.CheckOut.Notes) : null,
                session.TotalWorked,
                session.Status.ToString());
        }
    }
}
