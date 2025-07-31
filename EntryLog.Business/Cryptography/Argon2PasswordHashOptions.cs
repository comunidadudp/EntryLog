namespace EntryLog.Business.Cryptography
{
    public class Argon2PasswordHashOptions
    {
        public int DegreeOfParallelism { get; init; }
        public int MemorySize { get; init; }
        public int Iterations { get; init; }
        public int SaltSize { get; init; }
        public int HashSize { get; init; }
    }
}
