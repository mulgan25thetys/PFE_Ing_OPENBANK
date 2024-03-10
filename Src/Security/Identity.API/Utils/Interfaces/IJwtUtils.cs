using Microsoft.AspNetCore.Identity;

namespace Identity.API.Utils.Interfaces
{
    public interface IJwtUtils
    {
        Task<string> GetToken(IdentityUser user);
        string ValidateToken(string token);
    }
}
