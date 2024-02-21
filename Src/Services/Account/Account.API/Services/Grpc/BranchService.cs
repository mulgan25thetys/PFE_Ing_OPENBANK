using Branch.GRPC.Protos;

namespace Account.API.Services.Grpc
{
    public class BranchService
    {
        private readonly BranchProtoService.BranchProtoServiceClient _grpcClient;

        public BranchService(BranchProtoService.BranchProtoServiceClient grpcClient)
        {
            _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
        }
        public async Task<BranchResponse> GetBranch(string name)
        {
            var branchByNameRequest = new GetOneBranchByNameRequest() { Name = name };
            return _grpcClient.GetOneBranchByName(branchByNameRequest);
        }
    }
}
