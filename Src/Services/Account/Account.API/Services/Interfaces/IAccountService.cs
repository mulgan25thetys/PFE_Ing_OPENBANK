using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;

namespace Account.API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountModel> AddAccount(AccountRequest account, string? ownerId);
        Task<bool> UpdateAccount(AccountModel account, Int64 transactionId);
        Task<AccountModel> GetAccount(Int64 accountNumber, string? ownerId="");
        Task<AccountList> GetAllAccounts(string? ownerId = "");
        Task<AccountList> GetAllFilteringAccounts(string filter, string? ownerId = "");
    }
}
