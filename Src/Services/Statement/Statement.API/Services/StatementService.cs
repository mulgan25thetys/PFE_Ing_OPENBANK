using AutoMapper;
using Statement.API.Models;
using Statement.API.Models.Responses;
using Statement.API.Services.Grpc;
using Statement.API.Services.Interfaces;

namespace Statement.API.Services
{
    public class StatementService : IStatementService
    {
        private readonly ILogger<StatementService> _logger;
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private string endPointUrl = "";

        private readonly AccountService _accountService;
        private readonly BranchService _branchService;

        public StatementService(ILogger<StatementService> logger, BranchService branchService, AccountService accountService,
            IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _branchService = branchService ?? throw new ArgumentNullException(nameof(branchService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

            //Setting endPoint
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<StatementModel> GetStatementAsync(long account_number, string? date_filter)
        {
            var accountModel = await _accountService.GetAccountAsync(account_number);
            var brancModel = await _branchService.GetBranchAsync(accountModel.Branchcode);

            var response = await _client.GetAsync(endPointUrl + date_filter);  
            response.EnsureSuccessStatusCode();
            TransactionList transactionList = await response.Content.ReadAsAsync<TransactionList>();

            return new StatementModel() {Account_Number = accountModel.Accnumber, Bank_Name = accountModel.Bankname,
            Branch_Address = brancModel.Address, Statement_Name = accountModel.Bankname + " " + brancModel.Address, 
            Statement_Owner_Address = accountModel.Accowneraddress, Statement_Owner_Name = accountModel.Accownerlastname + " "+accountModel.Accownerfirstname,
            Transactions = transactionList};
        }
    }
}
