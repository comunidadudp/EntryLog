namespace EntryLog.Entities.POCOEntities
{
    public class Employee
    {
        public int Code { get; set; }
        public string FullName { get; set; } = "";
        public int PositionId { get; set; }
        public DateTime DateOfBirthday { get; set; }
        public string TownName { get; set; } = "";
        public Position Position { get; set; } = default!;
    }
}
