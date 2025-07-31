namespace EntryLog.Business.DTOs
{
    public record class LoginResponseDTO(
        int DocumentNumber,
        string Role,
        string Email);
}
