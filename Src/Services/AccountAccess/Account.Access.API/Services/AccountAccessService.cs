using Account.Access.API.Models;
using Account.Access.API.Models.Responses;
using Account.Access.API.Services.Grpc;
using Account.Access.API.Services.Interfaces;

namespace Account.Access.API.Services
{
    public class AccountAccessService : IAccountAccessService
    {
        private readonly HttpClient _client;
        private readonly ILogger<AccountAccessService> _logger;
        private readonly AccountService _accountService;
        private readonly IConfiguration _config;
        private string endPointUrl = "";

        public AccountAccessService(HttpClient client, ILogger<AccountAccessService> logger,
        AccountService accountService, IConfiguration config)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _config = config;
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<bool> CheckAccessAsync(string providerId, long accountNumber)
        {
            string filter = "{ \"providerid\": { \"$eq\": \"" + providerId + "\" }, \"accnumber\": { \"$eq\": \"" + accountNumber + "\"  }, \"status\": { \"$eq\": \"" + ACCESS_STATUS.GRANTED.ToString() +"\" } }";
            var response = await _client.GetAsync(endPointUrl + "?q=" + filter);
            response.EnsureSuccessStatusCode();
             AccountAccessList list = await response.Content.ReadAsAsync<AccountAccessList>();
            if (list != null && list.Items.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<AccountModel> GetAccountAsync(string providerId, long accountNumber)
        {
            if (await this.CheckAccessAsync(providerId, accountNumber))
            {
                var account = await _accountService.GetAccountDataAsync(accountNumber);
                AccountModel model = new AccountModel() { ACCNUMBER = accountNumber, BALANCE = account.Balance,
                BANKCODE = account.Bankcode, BANKNAME = account.Bankname, BRANCHCODE = account.Branchcode, IBAN = account.Iban};
                return model;
            }
            _logger.LogError($"Retrieving account failed providerid ={providerId} account number = {accountNumber}");
            return null;
        }

        public async Task<AccountAccessList> GetAllAccessAsync(string providerId)
        {
            return await getAllAccess(providerId);
        }

        public async Task<AccountAccess> GetOneAccessAsync(string providerId, long accountNumber)
        {
            string filter = "{ \"providerid\": { \"$eq\": \"" + providerId + "\" }, \"accnumber\": { \"$eq\": \""+ accountNumber +"\"  } }";
            var response = await _client.GetAsync(endPointUrl + "?q=" + filter);
            response.EnsureSuccessStatusCode();
            AccountAccessList list = await response.Content.ReadAsAsync<AccountAccessList>();
            return list.Items.FirstOrDefault();
        }

        private async Task<AccountAccessList> getAllAccess(string providerId)
        {
            string filter = "{ \"providerid\": { \"$eq\": \""+ providerId +"\" } }";
            var response = await _client.GetAsync(endPointUrl + "?q=" + filter);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<AccountAccessList>();
        }
    }
}
