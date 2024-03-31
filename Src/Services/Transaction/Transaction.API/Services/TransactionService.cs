using Account.Grpc.Protos;
using Newtonsoft.Json;
using System.Text;
using System.Transactions;
using Transaction.API.Features.Commands;
using Transaction.API.Models;
using Transaction.API.Models.Requests;
using Transaction.API.Models.Responses;
using Transaction.API.Models.Responses.Accounts;
using Transaction.API.Services.Grpc;
using Transaction.API.Services.Interfaces;

namespace Transaction.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private readonly AccountService _accountService;
        private readonly ILogger<TransactionService> _logger;

        private string endPointUrl = "";
        private string endPointChallengeUrl = "";
        private string endPoinRequesttUrl = "";

        public TransactionService(ILogger<TransactionService> logger, AccountService accountService,
            IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

            //Setting endPoint
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";

            endPointChallengeUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseChallengeTableName")}/";

            endPoinRequesttUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseRequestTableName")}/";
        }
        public async Task<TransactionModel> AddTransactionAsync(TransactionModel transaction)
        {
            var transactionPost = JsonConvert.SerializeObject(transaction);

            var response = await _client.PostAsync(endPointUrl, new StringContent(transactionPost, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Adding transaction success!");
            return await response.Content.ReadAsAsync<TransactionModel>();
        }

        Task<TransactionResponse> ITransactionService.CreateTransactionRequest(string account_id, string bank_id, TransactionRequestReq req)
        {
            throw new NotImplementedException();
        }

        Task<TransactionResponse> ITransactionService.AnswerTransactionRequest(AnswerTransactionReq answer)
        {
            throw new NotImplementedException();
        }

        Task<TransactionRequestList> ITransactionService.GetTransactionsRequest(string account_id)
        {
            throw new NotImplementedException();
        }

        async Task<TransactionResponseList> ITransactionService.GetTransactionsForAccount(string account_id)
        {
            var result = await _client.GetAsync(endPointUrl);
            try
            {
                var transactions = await result.Content.ReadAsAsync<TransactionList>();
                if (transactions.Items.Count > 0)
                {
                    TransactionResponseList responseList = new TransactionResponseList() { Count = transactions.Count, 
                    HasMore = transactions.HasMore, Limit = transactions.Limit, Offset = transactions.Offset};
                    foreach (var item in transactions.Items)
                    {
                        var transactionResponse = await GetTransactionResponseFromModel(item);
                        responseList.transactions.Add(transactionResponse);
                    }
                }
                return new TransactionResponseList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new TransactionResponseList();
            }
        }

        async Task<TransactionResponse> ITransactionService.GetTransactionAsync(string transactionId, string view_id)
        {
            var result = await _client.GetAsync(endPointUrl + transactionId);
            try
            {
                var transaction = await result.Content.ReadAsAsync<TransactionModel>();
                return await GetTransactionResponseFromModel(transaction, view_id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new TransactionResponse() { Code = 500, ErrorMessage = "OBP-50000: Unknown Error." };
            }
        }
        #region Private methods
        //Get transaction response from Model
        private async Task<TransactionResponse> GetTransactionResponseFromModel(TransactionModel model, string? view_id = null)
        {
            TransactionResponse response = new TransactionResponse();
            response.Details = await GetTransactionDetail(model);
            response.Other_account = await GetToAccountAsync(response.Details.New_balance.Account_id, view_id);
            response.This_account = await GetFromAccountAsync(response.Details.Value.Account_id, view_id);
            response.Id = model.Id;
            return response;
        }
        //return Account Object From Account.Grpc
        private async Task<AccountObject> GetAccountObject(string account_id)
        {
            return await _accountService.GetAccountDataAsync(account_id);
        }
        //Get From Account Detail Of transaction
        private async Task<FromAccount> GetFromAccountAsync(string account_id, string view_id)
        {
            return new FromAccount();
        }
        //Get To Account Detail Of transaction
        private async Task<ToAccount> GetToAccountAsync(string account_id, string view_id)
        {
            return new ToAccount();
        }
        // Get Transaction request for transaction object
        private async Task<TransactionRequest?> GetTransactionRequest(string transactionId, string? account_id = null)
        {
            string filter = "?q = { \"transaction_ids\": { \"$eq\" : \"" + transactionId + "\" } }";
            if (account_id != null)
            {
                filter = "?q = { \"$or\" : [ \"from_account_id\": { \"$eq\" : \"" + account_id + "\" }, \"to_account_id\": { \"$eq\":\""+account_id+"\" } ] }";
            }
            var result = await _client.GetAsync(endPoinRequesttUrl + filter);
            try
            {
                var transactionReqList = await result.Content.ReadAsAsync<TransactionRequestList>();
                return transactionReqList.Transaction_requests_with_charges.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }
        // Get detail of a transaction
        private async Task<TransactionDetail> GetTransactionDetail(TransactionModel model)
        {
            var detail = new TransactionDetail();
            //Get Detail of the transaction
            var detailResponse = await GetTransactionRequest(model.Id);
            
            if (detailResponse == null)
            {
                return detail;
            }
            //Get From and To account 
            var toAccount = await GetAccountObject(detailResponse.To_account_id);
            var fromAccount = await GetAccountObject(detailResponse.From_account_id);


            detail.Type = detailResponse.Type;
            detail.Posted = model.Posted;
            detail.Completed = model.Completed;
            if (toAccount != null && fromAccount != null)
            {
                detail.New_balance = new TransactionValue() { Amount = toAccount.Amount, Currency = toAccount.Currency, Account_id = toAccount.Id };
                detail.Value = new TransactionValue() { Amount = fromAccount.Amount, Currency = fromAccount.Currency, Account_id = fromAccount.Id };
            }
            return detail;
        }
        #endregion
    }
}
