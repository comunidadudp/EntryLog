namespace EntryLog.Web.Models
{
    public record UserViewModel(
        int NameIdentifier,
        string Email,
        string Role,
        string Name
    );
}
