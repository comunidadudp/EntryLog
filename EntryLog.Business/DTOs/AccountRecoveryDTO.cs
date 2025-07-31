namespace EntryLog.Business.DTOs
{
    public record AccountRecoveryDTO(
        string Token,
        string Password,
        string PasswordConf);
}
