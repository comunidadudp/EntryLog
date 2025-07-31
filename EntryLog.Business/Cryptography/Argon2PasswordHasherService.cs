using EntryLog.Business.Interfaces;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace EntryLog.Business.Cryptography
{
    internal class Argon2PasswordHasherService(
        IOptions<Argon2PasswordHashOptions> options) : IPasswordHasherService
    {
        private readonly Argon2PasswordHashOptions _options = options.Value;

        public string Hash(string password)
        {
            byte[] salt = GenerateSalt(_options.SaltSize);
            byte[] hash = HashPasswordInternal(password, salt);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string hash)
        {
            string[] parts = hash.Split(':');
            if (parts.Length != 2)
                throw new FormatException("Hash format invalid");

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);
            byte[] computeHash = HashPasswordInternal(password, salt);

            return CryptographicOperations.FixedTimeEquals(storedHash, computeHash);
        }

        private byte[] HashPasswordInternal(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _options.DegreeOfParallelism,
                MemorySize = _options.MemorySize,
                Iterations = _options.Iterations
            };

            return argon2.GetBytes(_options.HashSize);
        }

        private static byte[] GenerateSalt(int size)
        {
            byte[] salt = new byte[size];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            return salt;
        }
    }
}
