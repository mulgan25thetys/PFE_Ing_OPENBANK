using User.grpc.Models;

namespace User.grpc.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserModel> GetUserModelByIdAsync(string userId);
        public Task<UserModel> GetUserModelByEmailAsync(string email);
    }
}
