namespace EntryLog.Business.DTOs
{
    public record UserInfoDTO(
        string ProfileImage,
        string Code,
        string Name,
        string Role,
        bool IsActive,
        string PositionName,
        string PositionDescription,
        string Email,
        string CellPhone,
        string DateOfBirthDay,
        string City,
        bool IsFaceIdActive
    );
}
