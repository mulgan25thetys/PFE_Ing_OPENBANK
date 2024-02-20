using Account.API.Models;
using Account.API.Models.Responses;

namespace Account.API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountModel> AddAccount(AccountModel account);
        Task<AccountModel> GetAccount(int accountNumber);
        Task<AccountList> GetAllAccounts();
        Task<AccountList> GetAllFilteringAccounts(string filter);
    }
}
