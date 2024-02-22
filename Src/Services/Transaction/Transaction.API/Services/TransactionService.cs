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
        public async Task<bool> AddTransactionAsync(TransactionModel transaction)
        {
            var transactionPost = JsonConvert.SerializeObject(transaction);

                var response = await _client.PostAsync(endPointUrl, new StringContent(transactionPost, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Adding transaction success!");
                return true;
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
    }
}
