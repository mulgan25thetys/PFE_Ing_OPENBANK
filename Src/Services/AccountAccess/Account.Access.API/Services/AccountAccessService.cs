using Account.Access.API.Models;
using Account.Access.API.Models.Responses;
using Account.Access.API.Services.Grpc;
using Account.Access.API.Services.Interfaces;
using AutoMapper;
using EventBus.Message.Events;
using MassTransit;
using Newtonsoft.Json;
using System.Text;

namespace Account.Access.API.Services
{
    public class AccountAccessService : IAccountAccessService
    {
        private readonly HttpClient _client;
        private readonly ILogger<AccountAccessService> _logger;
        private readonly AccountService _accountService;
        private readonly IConfiguration _config;
        private string endPointUrl = "";
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public AccountAccessService(HttpClient client, ILogger<AccountAccessService> logger, IMapper mapper,
        AccountService accountService, IConfiguration config, IPublishEndpoint publishEndpoint)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _config = config;
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<bool> CheckAccessAsync(string providerId, long accountNumber)
        {
            var account = await _accountService.GetAccountDataAsync(accountNumber);

            string filter = "{ \"providerid\": { \"$eq\": \"" + providerId + "\" }, \"accnumber\": { \"$eq\": \"" + account.Accnumber + "\"  } } }";
            var response = await _client.GetAsync(endPointUrl + "?q=" + filter);
            response.EnsureSuccessStatusCode();
            AccountAccessList list = await response.Content.ReadAsAsync<AccountAccessList>();
            if (list != null && list.Items.Count > 0)
            {
                AccountAccess access = list.Items.FirstOrDefault();
                if (access.STATUS == Models.ACCESS_STATUS.GRANTED.ToString())
                {
                    return true;
                }
                return false;
            }
            else
            {
                AccountAccess access = new AccountAccess()
                {
                    ACCNUMBER = account.Accnumber,
                    CUSTOMERID = account.Ownerid,
                    PROVIDERID = providerId,
                    CREATEDAT = DateTime.Now,
                    UPDATEDAT = DateTime.Now,
                    DURATION = 0,
                    STATUS = Models.ACCESS_STATUS.WAITING.ToString()
                };
                var accessJson = JsonConvert.SerializeObject(access);
                var result = await _client.PostAsync(endPointUrl, new StringContent(accessJson, Encoding.UTF8, "application/json"));
                result.EnsureSuccessStatusCode();
                _logger.LogInformation($"An account access has been up to date! customer : {access.CUSTOMERID}, provider : {access.PROVIDERID}");

                AccountAccessEvent eventMessage = new AccountAccessEvent()
                {
                    ACCNUMBER = access.ACCNUMBER,
                    CUSTOMERID = access.CUSTOMERID,
                    STATUS = access.STATUS
                };
                await _publishEndpoint.Publish(eventMessage);
                return false;
            }
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
