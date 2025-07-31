namespace EntryLog.Business.Interfaces
{
    internal interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cypherText);
    }
}
