using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Account.API.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Account.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private string endPointUrl = "";

        public AccountService(ILogger<AccountService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            //Setting endPoint
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<AccountModel> AddAccount(AccountRequest account)
        {
            #region Account Model Construction
            Random generator = new Random();
            AccountModel model = new AccountModel();
            model.Acc_Number = generator.Next(0, 100000);
            model.Acc_Created_At = DateTime.Now;
            model.Acc_Updated_At = DateTime.Now;
            model.Acc_Balance = account.Acc_Balance;
            model.Acc_Owner_Address = account.Acc_Owner_Address;
            model.Acc_Owner_Email = account.Acc_Owner_Email;
            model.Acc_Owner_Firstname = account.Acc_Owner_Firstname;
            model.Acc_Owner_Lastname = account.Acc_Owner_Lastname;
            model.Acc_Owner_Phone = account.Acc_Owner_Phone;
            model.Acc_Owner_Post_Code = account.Acc_Owner_Post_Code;
            #endregion
            var accountPost = JsonConvert.SerializeObject(model);

            var response = _client.PostAsync(endPointUrl, new StringContent(accountPost, Encoding.UTF8, "application/json"));

            return await response.Result.Content.ReadAsAsync<AccountModel>();
        }

        public async Task<AccountModel> GetAccount(Int64 accountNumber)
        {
            string requestString  =  "{ \"acc_number\":{ \"$eq\":\""+ accountNumber +"\"} }";
            AccountList list = await GetAllFilteringAccounts(requestString);
            return list.Items[0];
        }

        public async Task<AccountList> GetAllAccounts()
        {
            var response = await _client.GetAsync(endPointUrl);

            return await response.Content.ReadAsAsync<AccountList>();
        }

        public async Task<AccountList> GetAllFilteringAccounts(string filter)
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
