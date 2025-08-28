namespace EntryLog.Entities.POCOEntities
{
    public class Location
    {
        public string Latitude { get; set; } = "";
        public string Longitude { get; set; } = "";
        public string Country { get; set; } = "";
        public string City { get; set; } = "";
        public string Neighbourhood { get; set; } = "";
        public string? ApproximateAddress { get; set; }
        public string IpAddress { get; set; } = "";
    }
}
