namespace EntryLog.Entities.POCOEntities
{
    public sealed class Position
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Descripcion { get; set; } = "";

        public ICollection<Employee> Employees { get; set; } = [];
    }
}
