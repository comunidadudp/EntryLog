namespace EntryLog.Business.Interfaces
{
    internal interface IJwtService
    {
        Task<string> GenerateTokenAsync(string userId, string purpose, TimeSpan expiresIn);
        IDictionary<string, string> ValidateToken(string token);
    }
}
