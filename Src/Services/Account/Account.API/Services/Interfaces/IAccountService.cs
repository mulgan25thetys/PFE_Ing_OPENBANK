using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;

namespace Account.API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountModel> AddAccount(AccountRequest account);
        Task<bool> UpdateAccount(AccountModel account, Int64 transactionId);
        Task<AccountModel> GetAccount(Int64 accountNumber);
        Task<AccountList> GetAllAccounts();
        Task<AccountList> GetAllFilteringAccounts(string filter);
    }
}
