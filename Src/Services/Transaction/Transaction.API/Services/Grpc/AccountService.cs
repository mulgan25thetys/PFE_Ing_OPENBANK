using Account.Grpc.Protos;

namespace Transaction.API.Services.Grpc
{
    public class AccountService
    {
        private readonly AccountProtoService.AccountProtoServiceClient _grpcClient;

        public AccountService(AccountProtoService.AccountProtoServiceClient grpcClient)
        {
            _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
        }

        public async Task<AccountObject> GetAccountDataAsync(Int64 accountNumber)
        {
            var accountRequest  = new GetAccountRequest() { AccountNumber = accountNumber };
            return await _grpcClient.GetAccountAsync(accountRequest);
        }
    }
}
