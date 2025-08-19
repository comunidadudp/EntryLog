using EntryLog.Business.DTOs;
using EntryLog.Business.Utils;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Mappers
{
    internal static class FaceIdMapper
    {
        public static EmployeeFaceIdDTO MapToEmployeeFaceIdDTO(FaceID f, string base64Image)
            => new(
            base64Image,
            TimeFunctions
                .GetSAPacificStandardTime(f.RegisterDate)
                    .ToString("dd/MM/yyyy hh:mm tt"),
            f.Active
           );

        public static EmployeeFaceIdDTO Empty()
            => new("", "",false);
    }
}
