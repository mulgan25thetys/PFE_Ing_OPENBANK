using Account.API.Models;
using Account.API.Models.Responses;
using Account.API.Services.Interfaces;

namespace Account.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IConfiguration _configuration;

        public AccountService(ILogger<AccountService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<AccountModel> AddAccount(AccountModel account)
        {
            throw new NotImplementedException();
        }

        public Task<AccountModel> GetAccount(int accountNumber)
        {
            throw new NotImplementedException();
        }

        public Task<AccountList> GetAllAccounts()
        {
            throw new NotImplementedException();
        }

        public Task<AccountList> GetAllFilteringAccounts(string filter)
        {
            throw new NotImplementedException();
        }
    }
}
