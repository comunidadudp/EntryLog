namespace EntryLog.Business.Interfaces
{
    internal interface IPasswordHasherService
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}
