using Branch.API.Models;
using Branch.API.Models.Requests;
using Branch.API.Models.Response;
using System.Linq.Expressions;

namespace Branch.API.Services.Interfaces
{
    public interface IBranchService
    {
        Task<bool> AddBranch(BranchRequest branch);
        Task<bool> UpdateBranch(BranchModel branch);
        Task<BranchList> GetAllBranches(int? page, int? size);
        Task<BranchList> GetBranchesByFilter(string filter, int? page, int? size);
        Task<BranchModel> GetBranch(int id);
    }
}
