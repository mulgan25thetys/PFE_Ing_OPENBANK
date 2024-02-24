using Account.Grpc.Protos;

namespace Statement.API.Services.Grpc
{
    public class AccountService
    {
        private readonly AccountProtoService.AccountProtoServiceClient _grpcClient;

        public AccountService(AccountProtoService.AccountProtoServiceClient grpcClient)
        {
            _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
        }

        public async Task<AccountObject> GetAccountAsync(Int64 accountId)
        {
            var getAccountRequest = new GetAccountRequest() { AccountNumber = accountId };
            return await _grpcClient.GetAccountAsync(getAccountRequest);
        }
    }
}
