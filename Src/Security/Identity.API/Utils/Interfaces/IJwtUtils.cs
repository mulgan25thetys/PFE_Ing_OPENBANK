using Identity.API.Utils.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Utils.Interfaces
{
    public interface IJwtUtils
    {
        LoggedUser GetLoggedUser(string token);
        Task<string> GetToken(IdentityUser user);
        Task<string> GetNotAuthenticatedToken(IdentityUser user);
        string ValidateToken(string token);
    }
}
