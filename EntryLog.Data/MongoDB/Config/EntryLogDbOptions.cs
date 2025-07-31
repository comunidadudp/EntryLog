namespace EntryLog.Data.MongoDB.Config
{
    internal sealed class EntryLogDbOptions
    {
        public string ConnectionUri { get; init; } = "";
        public string DatabaseName { get; init; } = "";
    }
}
