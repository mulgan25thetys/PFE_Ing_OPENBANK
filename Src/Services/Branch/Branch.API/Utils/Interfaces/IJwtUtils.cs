namespace Branch.API.Utils.Interfaces
{
    public interface IJwtUtils
    {
        string GetToken(string userId);
        string ValidateToken(string token);
    }
}
