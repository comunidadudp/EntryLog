namespace EntryLog.Business.Interfaces
{
    internal interface IUriService
    {
        string ApplicationURL { get; }
        string UserAgent { get; }
        string Platform { get; }
        string RemoteIpAddress { get; }
    }
}
