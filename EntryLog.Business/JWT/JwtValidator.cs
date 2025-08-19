using Microsoft.IdentityModel.Tokens;

namespace EntryLog.Business.JWT
{
    internal static class JwtValidator
    {
        public static bool CustomLifeTimeValidator(
            DateTime? notBefore,
            DateTime? expires,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            if (expires != null && expires < DateTime.UtcNow)
            {
                return false;
            }

            if (notBefore != null && notBefore > DateTime.UtcNow) 
            {
                return false;
            }

            return true;
        }
    }
}
