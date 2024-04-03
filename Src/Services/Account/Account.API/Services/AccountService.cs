﻿using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Account.API.Services.Grpc;
using Account.API.Services.Interfaces;
using AutoMapper;
using EventBus.Message.Events;
using Helper.Models;
using MassTransit;
using Newtonsoft.Json;
using System.Text;

namespace Account.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly HttpClient _client;
        private string endPointUrl = "";
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly UserService _userService;

        public AccountService(ILogger<AccountService> logger, IMapper mapper,
            IPublishEndpoint publishEndpoint,UserService userService,
            IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
           
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            //Setting endPoint
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<AccountCreated> AddAccount(string ACCOUNT_ID, string BANK_ID,AccountRequest account, string? ownerId = "")
        {
            
            #region Account Model Construction
            Random generator = new Random();
            AccountModel model = new AccountModel();
            model.Id = ACCOUNT_ID;
            model.Bank_id = BANK_ID;
            model.Label = account.Label;
            model.Type = account.Type;
            model.AccNumber = generator.NextInt64(100000000000);
            model.Amount = 0;
            model.Currency = account.Balance.Currency.Trim().ToUpper(); 
            
            model.Iban = "TN" + generator.NextInt64(1000000000000000000);

            model.Owner_id = ownerId;
            model.Updated_at = DateTime.Now;
            model.Created_at = DateTime.Now;
            #endregion

            var accountPost = JsonConvert.SerializeObject(model);

            var response = await _client.PostAsync(endPointUrl, new StringContent(accountPost, Encoding.UTF8, "application/json"));

            try
            {
                _logger.LogInformation("Adding account success!");
                model = await response.Content.ReadAsAsync<AccountModel>();
                return new AccountCreated() { Bank_id = model.Bank_id, Id = model.Id, Label = model.Label };
            }
            catch (Exception ex)
            {
                return new AccountCreated() { Code = 500, ErrorMessage = "OBP-50000: Unknown Error." };
            }
            
        }

        public async Task<AccountResponse> GetAccount(Int64 accountNumber, string? ownerId = "")
        {
            string filter = ownerId != "" ? "?q= { \"accnumber\":{ \"$eq\":\""+ accountNumber + "\" }, \"ownerid\" : { \"$eq\":\"" + ownerId + "\" }  }" : accountNumber.ToString();
            var response = await _client.GetAsync(endPointUrl + filter);

            try
            {
                AccountList list = await response.Content.ReadAsAsync<AccountList>();
                return await GetAccountResponseFromModel(list.Items.FirstOrDefault());
            }
            catch(Exception ex)
            {
                return new AccountResponse() { Id = null };
            }
        }

        public async Task<AccountResponse> GetAccountById(string id, string? ownerId)
        {
            var response = await _client.GetAsync(endPointUrl + id);
            try
            {
                AccountModel model = await response.Content.ReadAsAsync<AccountModel>();
                if(ownerId != null && model.Owner_id != ownerId)
                {
                    return new AccountResponse() { Code = 403, ErrorMessage = "OBP-50000: Unknown Error." };
                }
                return await GetAccountResponseFromModel(model);
            }
            catch(Exception ex)
            {
                return new AccountResponse() { Code = 500, ErrorMessage = "OBP-50000: Unknown Error." };
            } 
        }

        public async Task<AccountListResponse> GetAllAccounts(string bank_Id)
        {
            string filter = "?q= {\"bank_id\" : { \"$eq\":\"" + bank_Id + "\" } }";
            var response = await _client.GetAsync(endPointUrl + filter);

            AccountList list = await response.Content.ReadAsAsync<AccountList>();
            AccountListResponse respList = new AccountListResponse() { Offset = list.Offset, Limit = list.Limit,
                HasMore = list.HasMore, Count = list.Count};
            foreach (var item in list.Items)
            {
                respList.Items.Add(await GetAccountResponseFromModel(item));
            }
            return respList;
        }

        public async Task<AccountListResponse> GetAllFilteringAccounts(string filter, string? ownerId = "")
        {
            try
            {
                var filterJson = JsonConvert.DeserializeObject(filter);
                var result = await _client.GetAsync($"{endPointUrl}?q={filter}");

                AccountList list = await result.Content.ReadAsAsync<AccountList>();
                AccountListResponse respList = new AccountListResponse()
                {
                    Offset = list.Offset,
                    Limit = list.Limit,
                    HasMore = list.HasMore,
                    Count = list.Count
                };
                foreach (var item in list.Items)
                {
                    respList.Items.Add(await GetAccountResponseFromModel(item));
                }
                return respList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AccountListResponse();
            }
        }

        public async Task<bool> UpdateAccount(AccountModel account, Int64 transactionId)
        {
            AccountModel model = await _client.GetAsync(endPointUrl + account.AccNumber).Result.Content.ReadAsAsync<AccountModel>();
            //model.BALANCE = account.BALANCE;
            model.Updated_at = DateTime.Now;
            var accountPost = JsonConvert.SerializeObject(model);

            var response = await _client.PutAsync(endPointUrl + account.AccNumber, new StringContent(accountPost, Encoding.UTF8, "application/json"));
            
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

        private async Task<AccountResponse> GetAccountResponseFromModel(AccountModel model)
        {
            AccountResponse resp = new AccountResponse()
            {
                Type = model.Type,
                Balance = new BalanceModel() { Amount = model.Amount, Currency = model.Currency },
                IBAN = model.Iban,
                Id = model.Id,
                Label = model.Label,
                Number = model.AccNumber,
                Swift_bic = model.Swift_bic
            };
            var user = await _userService.GetUserAsync(model.Owner_id);
            AccountOwnerModel ownerModel = new AccountOwnerModel() { Display_name = "", Id = user.UserId, Provider = user.Provider};
            resp.Owners.Add(ownerModel);
            return resp;
        }

        public async Task<MessageSuccess> UpdateAccount(UpdateLabelRequest request)
        {
            AccountModel model = await _client.GetAsync(endPointUrl + request.Id).Result.Content.ReadAsAsync<AccountModel>();
            model.Label = request.Label;
            model.Updated_at = DateTime.Now;
            var accountPut = JsonConvert.SerializeObject(model);

            var response = await _client.PutAsync(endPointUrl + model.Id, new StringContent(accountPut, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Updating account label success!");
                return new MessageSuccess() { Success = "Success" };
            }
            else
            {
                _logger.LogInformation("Updating account label failed!");
                return new MessageSuccess() { Success = "Failed" };
            }
        }
    }
}
