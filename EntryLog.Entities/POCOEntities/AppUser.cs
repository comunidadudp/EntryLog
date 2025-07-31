using EntryLog.Entities.Enums;

namespace EntryLog.Entities.POCOEntities
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public int Code { get; set; }
        public RoleType Role { get; set; }
        public string Email { get; set; } = "";
        public string CellPhone { get; set; } = "";
        public string Password { get; set; } = "";
        public int Attempts { get; set; }
        public string? RecoveryToken { get; set; }
        public bool RecoveryTokenActive { get; set; }
        public bool Active { get; set; }
    }
}
