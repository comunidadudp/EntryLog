using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace EntryLog.Business.Cryptography
{
    internal class RSAAsymmetricEncryptionService(
        IOptions<EncryptionKeyValues> options) : IEncryptionService
    {
        private readonly EncryptionKeyValues _keys = options.Value;
        private readonly RSACryptoServiceProvider _csp = new RSACryptoServiceProvider((int)KeySize.SIZE_2048);

        public string Encrypt(string plainText)
        {
            try
            {
                _csp.FromXmlString(_keys.PublicKey);
                byte[] data = Encoding.Unicode.GetBytes(plainText);
                byte[] cypher = _csp.Encrypt(data, true);
                return Convert.ToBase64String(cypher);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                _csp.PersistKeyInCsp = false;
            }
        }

        public string Decrypt(string cypherText)
        {
            try
            {
                _csp.FromXmlString(_keys.PrivateKey);
                byte[] data = Convert.FromBase64String(cypherText);
                byte[] plain = _csp.Decrypt(data, true);
                return Encoding.Unicode.GetString(plain);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                _csp.PersistKeyInCsp = false;
            }
        }
        
    }

    public enum KeySize
    {
        SIZE_512 = 512,
        SIZE_1024 = 1024,
        SIZE_2048 = 2048,
        SIZE_952 = 952,
        SIZE_1369 = 1369
    }
}
