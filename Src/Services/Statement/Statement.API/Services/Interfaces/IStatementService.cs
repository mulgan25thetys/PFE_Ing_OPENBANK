using Statement.API.Models;

namespace Statement.API.Services.Interfaces
{
    public interface IStatementService
    {
        public Task<StatementModel> GetStatementAsync(Int64 account_number, string? date_filter);
    }
}
