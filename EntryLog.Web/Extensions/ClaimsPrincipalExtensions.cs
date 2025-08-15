using EntryLog.Web.Models;
using System.Security.Claims;
using System.Security.Principal;

namespace EntryLog.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static UserViewModel? GetUserData(this ClaimsPrincipal claimsPrincipal)
        {
            IIdentity? identity = claimsPrincipal.Identity;
            if (identity != null && identity.IsAuthenticated)
            {
                IEnumerable<Claim> claims = claimsPrincipal.Claims;

                return new UserViewModel(
                    int.Parse(claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value),
                    claims.First(x => x.Type == ClaimTypes.Email).Value,
                    claims.First(x => x.Type == ClaimTypes.Role).Value,
                    claims.First(x => x.Type == ClaimTypes.Name).Value
                );
            }
            return null;
        }
    }
}
