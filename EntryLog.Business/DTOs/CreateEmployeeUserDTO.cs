namespace EntryLog.Business.DTOs
{
    public record CreateEmployeeUserDTO(
        string DocumentNumber,
        string Username,
        string CellPhone,
        string Password,
        string PasswordConf);
}
