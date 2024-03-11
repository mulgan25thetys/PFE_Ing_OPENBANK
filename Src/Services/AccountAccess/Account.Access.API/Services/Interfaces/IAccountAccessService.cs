using Account.Access.API.Models;
using Account.Access.API.Models.Responses;

namespace Account.Access.API.Services.Interfaces
{
    public interface IAccountAccessService
    {
        public Task<bool> CheckAccessAsync(string providerId, long accountNumber);
        public Task<AccountAccess> GetOneAccessAsync(string providerId, long accountNumber);
        public Task<AccountAccessList> GetAllAccessAsync(string providerId);
        public Task<AccountModel> GetAccountAsync(string providerId, long accountNumber);
    }
}
