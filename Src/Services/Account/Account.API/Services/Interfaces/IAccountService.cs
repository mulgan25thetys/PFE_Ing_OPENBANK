using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Helper.Models;

namespace Account.API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountCreated> AddAccount(string ACCOUNT_ID,string BANK_ID,AccountRequest account, string? ownerId);
        Task<bool> UpdateAccount(AccountModel account, Int64 transactionId);
        Task<AccountResponse> GetAccount(Int64 accountNumber, string? ownerId="");
        Task<AccountResponse> GetAccountById(string id, string? ownerId = "");
        Task<AccountListResponse> GetAllAccounts(string bank_Id);
        Task<AccountListResponse> GetAllFilteringAccounts(string filter, string? ownerId = "");
        Task<MessageSuccess> UpdateAccount(UpdateLabelRequest request);
    }
}
