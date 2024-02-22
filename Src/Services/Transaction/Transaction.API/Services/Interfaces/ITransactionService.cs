using Transaction.API.Models;
using Transaction.API.Models.Responses;

namespace Transaction.API.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<bool> AddTransactionAsync(TransactionModel transaction);
        public Task<TransactionList> GetAllTransactionsAsync(Int64 account_number);
        public Task<TransactionModel> GetTransactionAsync(Int64 transactionId);
    }
}
