using Branchs.Grpc.Models;
using Branchs.Grpc.Models.Response;
using System.Linq.Expressions;

namespace Branchs.Grpc.Services.Interfaces
{
    public interface IBranchService
    {
        Task<BranchList> GetBranchesByFilter(string filter);
        Task<BranchModel> GetBranch(int id);
        Task<BranchModel> GetOneBranchByName(string name);
    }
}
