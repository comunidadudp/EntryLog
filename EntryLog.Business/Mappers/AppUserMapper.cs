using EntryLog.Business.DTOs;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Mappers
{
    internal static class AppUserMapper
    {
        public static UserInfoDTO MapToUserInfoDTO(AppUser user, Employee employee, string image)
            => new(
                image,
                user.Code.ToString(),
                user.Name,
                GetRoleName(user.Role),
                user.Active,
                employee.Position.Name,
                employee.Position.Descripcion,
                user.Email,
                user.CellPhone,
                employee.DateOfBirthday.ToString("yyyy/MM/dd"),
                employee.TownName,
                user?.FaceID?.Active ?? false
            );

        private static string GetRoleName(RoleType role)
        {
            string roleName = "";
            switch (role)
            {
                case RoleType.None:
                    roleName = "Ninguno";
                    break;
                case RoleType.Admin:
                    roleName = "Administrador";
                    break;
                case RoleType.Employee:
                    roleName = "Empleado";
                    break;
                default:
                    break;
            }
            return roleName;
        }
    }
}
