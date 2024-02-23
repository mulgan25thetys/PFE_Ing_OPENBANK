using Newtonsoft.Json;
using System.Text;
using Transaction.API.Models;
using Transaction.API.Models.Responses;
using Transaction.API.Services.Grpc;
using Transaction.API.Services.Interfaces;

namespace Transaction.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private string endPointUrl = "";
        private readonly AccountService _accountService;
        private readonly ILogger<TransactionService> _logger;

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
        }
        public async Task<TransactionModel> AddTransactionAsync(TransactionModel transaction)
        {
            var transactionPost = JsonConvert.SerializeObject(transaction);

                var response = await _client.PostAsync(endPointUrl, new StringContent(transactionPost, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Adding transaction success!");
                return await response.Content.ReadAsAsync<TransactionModel>();
        }

        public async Task<TransactionList> GetAllTransactionsAsync(long account_number)
        {
            string requestFilter = "{\"$or\":[{\"trans_credited_acc\":{\"$eq\":\""+ account_number +"\"}},{\"trans_debited_acc\":{\"$eq\":\""+ account_number +"\"}}]}";
            var response = await _client.GetAsync( endPointUrl +"?q=" + requestFilter);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<TransactionList>(); 
        }

        public async Task<TransactionModel> GetTransactionAsync(long transactionId)
        {
            var response = await _client.GetAsync(endPointUrl + transactionId);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<TransactionModel>();
        }

        public async Task<bool> UpdateTransactionAsync(TransactionModel transaction)
        {
            TransactionModel model = await _client.GetAsync(endPointUrl + transaction.TRANS_ID).Result.Content.ReadAsAsync<TransactionModel>();
            model.TRANS_UPDATED_AT = DateTime.Now;
            model.TRANS_STATUS = transaction.TRANS_STATUS;
            var transactionPost = JsonConvert.SerializeObject(model);

            var response = await _client.PutAsync(endPointUrl + model.TRANS_ID, new StringContent(transactionPost, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Updating transaction success!");
            return true;
        }
    }
}
