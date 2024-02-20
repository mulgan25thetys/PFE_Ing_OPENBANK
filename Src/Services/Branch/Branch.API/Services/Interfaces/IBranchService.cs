using Branch.API.Models;
using Branch.API.Models.Response;
using System.Linq.Expressions;

namespace Branch.API.Services.Interfaces
{
    public interface IBranchService
    {
        Task<BranchModel> AddBranch(BranchModel branch);
        Task<BranchModel> UpdateBranch(BranchModel branch);
        Task<BranchList> GetAllBranches();
        Task<BranchList> GetBranchesByFilter(string filter);
        Task<BranchModel> GetBranch(int id);
    }
}
