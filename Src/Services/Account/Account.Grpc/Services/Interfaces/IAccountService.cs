using Account.Grpc.Models;

namespace Account.Grpc.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<AccountModel> GetAccount(Int64 accountNumber);
    }
}
