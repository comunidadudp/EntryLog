namespace EntryLog.Entities.POCOEntities
{
    public class Check
    {
        public string Method { get; set; } = "";
        public string? DeviceName { get; set; }
        public DateTime Date { get; set; }
        public Location Location { get; set; } = new();
        public string PhotoUrl { get; set; } = "";
        public List<float> Descriptor { get; set; } = [];
        public string? Notes { get; set; }
    }
}
