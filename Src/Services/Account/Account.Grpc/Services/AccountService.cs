using Account.Grpc.Models;
using Account.Grpc.Models.Responses;
using Account.Grpc.Services.Interfaces;
using Newtonsoft.Json;

namespace Account.Grpc.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private string endPointUrl = "";

        public AccountService(ILogger<AccountService> logger,
            IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            //Setting endPoint
            endPointUrl = _config.GetValue<string>("OrdsSettings:Uri");
        }

        public async Task<AccountModel> GetAccount(long accountNumber)
        {
            string requestString = "{ \"acc_number\":{ \"$eq\":\"" + accountNumber + "\"} }";
            AccountList list = await GetAllFilteringAccounts(requestString);
            return list.Items.FirstOrDefault<AccountModel>();
        }

        private async Task<AccountList> GetAllFilteringAccounts(string filter)
        {
            try
            {
                var filterJson = JsonConvert.DeserializeObject(filter);
                var result = await _client.GetAsync($"{endPointUrl}?q={filter}");

                return await result.Content.ReadAsAsync<AccountList>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AccountList();
            }
        }
    }
}
