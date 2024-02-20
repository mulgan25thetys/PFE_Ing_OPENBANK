using Branch.GRPC.Models;
using Branch.GRPC.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;
using Branch.GRPC.Models.Response;

namespace Branch.GRPC.Services
{
    public class BranchService : IBranchService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<BranchService> _logger;
        private readonly HttpClient _client;
        private string endPointUrl = "";

        public BranchService(IConfiguration configuration, ILogger<BranchService> logger, HttpClient httpClient) {

            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            //Setting endPoint
            endPointUrl = $"/ords/{_config.GetValue<string>("OrdsSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OrdsSettings:DatabaseTableName")}/";
        }

        private async Task<BranchList> GetAllBranches()
        {
                var result = await _client.GetAsync(endPointUrl);

                return await result.Content.ReadAsAsync<BranchList>();
        }

        public async Task<BranchModel> GetBranch(int branchCode)
        {
                string requestString = "{'code':{'$eq':"+branchCode+"}}";
                var result = await _client.GetAsync($"{endPointUrl}?q={requestString}");

                return await result.Content.ReadAsAsync<BranchModel>();
        }

        public async Task<BranchList> GetBranchesByFilter(string filter)
        {
            try
            {
                var filterJson = JsonConvert.DeserializeObject(filter);
                var result = await _client.GetAsync($"{endPointUrl}?q={filter}");

                return await result.Content.ReadAsAsync<BranchList>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return await GetAllBranches();
            }
        }
    }
}
