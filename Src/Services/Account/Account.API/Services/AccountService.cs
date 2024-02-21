using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Account.API.Services.Grpc;
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
        private readonly BranchService _branchService;

        public AccountService(ILogger<AccountService> logger, BranchService branchService,
            IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _branchService = branchService ?? throw new ArgumentNullException(nameof(branchService));

            //Setting endPoint
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<AccountModel> AddAccount(AccountRequest account)
        {
            #region Account Model Construction
            Random generator = new Random();
            AccountModel model = new AccountModel();
            model.ACC_NUMBER = generator.NextInt64(0, 100000000000);
            model.ACC_BANK_DETAILS_KEY = _config.GetValue<int>("BankSetting:BankDetailsKey");
            model.BANK_CODE = _config.GetValue<string>("BankSetting:CodeBIC");
            model.BANK_NAME = _config.GetValue<string>("BankSetting:Name");
            model.ACC_CREATED_AT = DateTime.Now;
            model.ACC_UPDATED_AT = DateTime.Now;
            model.ACC_BALANCE = 0;
            model.ACC_OWNER_ADDRESS = account.Acc_Owner_Address;
            model.ACC_OWNER_EMAIL = account.Acc_Owner_Email;
            model.ACC_OWNER_FIRSTNAME = account.Acc_Owner_Firstname;
            model.ACC_OWNER_LASTNAME = account.Acc_Owner_Lastname;
            model.ACC_OWNER_PHONE = account.Acc_Owner_Phone;
            model.ACC_OWNER_POST_CODE = account.Acc_Owner_Post_Code;
            model.ACC_STATUS = AccountStatus.ENABLED.ToString(); 
            model.ACC_IBAN = _config.GetValue<string>("AccounSettings:CountryCode")+ generator.Next(0, 100)+ generator.NextInt64(1000000000000000000) + model.ACC_BANK_DETAILS_KEY;
            //branch grpc service 
            var branch = await _branchService.GetBranch(account.Branch_Name);
            model.BRANCH_CODE = branch.Code;

            #endregion

            try
            {
                var accountPost = JsonConvert.SerializeObject(model);

                var response = _client.PostAsync(endPointUrl, new StringContent(accountPost, Encoding.UTF8, "application/json"));
                _logger.LogInformation("Adding account success!");
                return await response.Result.Content.ReadAsAsync<AccountModel>();
            }
            catch (Exception ex) {
                _logger.LogError("Adding account failed! "+ex.Message);
                return new AccountModel();
            }

        }

        public async Task<AccountModel> GetAccount(Int64 accountNumber)
        {
            string requestString  =  "{ \"acc_number\":{ \"$eq\":\""+ accountNumber +"\"} }";
            AccountList list = await GetAllFilteringAccounts(requestString);
            return list.Items.FirstOrDefault<AccountModel>();
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
