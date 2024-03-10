using Helper.Utils.Models;

namespace Helper.Utils.Interfaces
{
    public interface IJwtUtils
    {
        LoggedUser ValidateTokenAndGetLoggedUser(string token);
    }
}
