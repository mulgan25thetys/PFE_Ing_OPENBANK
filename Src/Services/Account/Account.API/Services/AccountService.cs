using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Account.API.Services.Grpc;
using Account.API.Services.Interfaces;
using AutoMapper;
using EventBus.Message.Events;
using MassTransit;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Transactions;

namespace Account.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly HttpClient _client;
        private string endPointUrl = "";
        private readonly BranchService _branchService;
        private readonly IPublishEndpoint _publishEndpoint;

        public AccountService(ILogger<AccountService> logger, IMapper mapper,
            BranchService branchService, IPublishEndpoint publishEndpoint,
            IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _branchService = branchService ?? throw new ArgumentNullException(nameof(branchService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));

            //Setting endPoint
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<AccountModel> AddAccount(AccountRequest account, string? ownerId = "")
        {
            #region Account Model Construction
            Random generator = new Random();
            AccountModel model = new AccountModel();
            //model.OWNERID = ownerId;
            //model.ACCNUMBER = generator.NextInt64(0, 100000000000);
            //model.BANKDETAILSKEY = int.Parse(_config.GetValue<string>("BankSetting:BankDetailsKey"));
            //model.BANKCODE = _config.GetValue<string>("BankSetting:CodeBIC");
            //model.BANKNAME = _config.GetValue<string>("BankSetting:Name");
            //model.CREATEDAT = DateTime.Now;
            //model.UPDATEDAT = DateTime.Now;
            //model.BALANCE = 0;
            //model.OWNERADDRESS = account.Acc_Owner_Address;
            //model.OWNEREMAIL = account.Acc_Owner_Email;
            //model.OWNERFIRSTNAME = account.Acc_Owner_Firstname;
            //model.OWNERLASTNAME = account.Acc_Owner_Lastname;
            //model.OWNERPHONE = account.Acc_Owner_Phone;
            //model.OWNERPOSTCODE = account.Acc_Owner_Post_Code;
            //model.STATUS = AccountStatus.ENABLED.ToString(); 
            //model.IBAN = _config.GetValue<string>("AccounSettings:CountryCode")+ generator.Next(0, 100)+ generator.NextInt64(1000000000000000000) + model.BANKDETAILSKEY;
            ////branch grpc service 
            //var branch = await _branchService.GetBranch(account.Branch_Name);
            //model.BRANCHCODE = branch.Code;
            #endregion

           
                var accountPost = JsonConvert.SerializeObject(model);

                var response = await _client.PostAsync(endPointUrl, new StringContent(accountPost, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Adding account success!");
                return await response.Content.ReadAsAsync<AccountModel>();

        }

        public async Task<AccountModel> GetAccount(Int64 accountNumber, string? ownerId = "")
        {
            string filter = ownerId != "" ? "?q= { \"accnumber\":{ \"$eq\":\""+ accountNumber + "\" }, \"ownerid\" : { \"$eq\":\"" + ownerId + "\" }  }" : accountNumber.ToString();
            var response = await _client.GetAsync(endPointUrl + filter);
            response.EnsureSuccessStatusCode();
            AccountList list = await response.Content.ReadAsAsync<AccountList>();
            return list.Items.FirstOrDefault();
        }

        public async Task<AccountList> GetAllAccounts(string? ownerId = "")
        {
            if (ownerId == "")
            {
                var response = await _client.GetAsync(endPointUrl);

                return await response.Content.ReadAsAsync<AccountList>();
            }
            else
            {
                string filter = "?q= {\"ownerid\" : { \"$eq\":\"" + ownerId + "\" } }";
                var response = await _client.GetAsync(endPointUrl + filter);

                return await response.Content.ReadAsAsync<AccountList>();
            }
        }

        public async Task<AccountList> GetAllFilteringAccounts(string filter, string? ownerId = "")
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

        public async Task<bool> UpdateAccount(AccountModel account, Int64 transactionId)
        {
            AccountModel model = await _client.GetAsync(endPointUrl + account.NUMBER).Result.Content.ReadAsAsync<AccountModel>();
            //model.BALANCE = account.BALANCE;
            model.UPDATEDAT = DateTime.Now;
            var accountPost = JsonConvert.SerializeObject(model);

            var response = await _client.PutAsync(endPointUrl + account.NUMBER, new StringContent(accountPost, Encoding.UTF8, "application/json"));
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Updating account success!");

                await _publishEndpoint.Publish(new TransactionEvent() { TRANSID = transactionId, STATUS = TRANS_STATUS.SUCCESS.ToString() });
            }
            else
            {
                _logger.LogInformation("Updating account failed!");

                await _publishEndpoint.Publish(new TransactionEvent() { TRANSID = transactionId, STATUS = TRANS_STATUS.FAILED.ToString() });
            }
            return await Task.FromResult(true);
        }
    }
}
