using Branch.API.Models;
using Branch.API.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;
using Branch.API.Models.Response;

namespace Branch.API.Services
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
            endPointUrl = $"/ords/{_config.GetValue<string>("OracleSettings:DatabaseUser")}" +
                $"/{_config.GetValue<string>("OracleSettings:DatabaseTableName")}/";
        }

        public async Task<BranchModel> AddBranch(BranchModel branch)
        {
            var branchPost = JsonConvert.SerializeObject(branch);

            var response = _client.PostAsync(endPointUrl, new StringContent(branchPost, Encoding.UTF8, "application/json"));

            return await response.Result.Content.ReadAsAsync<BranchModel>();
        }

        public async Task<BranchList> GetAllBranches()
        {
                var result = await _client.GetAsync(endPointUrl);

                return await result.Content.ReadAsAsync<BranchList>();
        }

        public async Task<BranchModel> GetBranch(int id)
        {
                var result = await _client.GetAsync(endPointUrl + id);

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

        public async Task<BranchModel> UpdateBranch(BranchModel branch)
        {
                var branchPost = JsonConvert.SerializeObject(branch);

                var response = _client.PutAsync(endPointUrl + branch.IDBRANCH, new StringContent(branchPost, Encoding.UTF8, "application/json"));

                return await response.Result.Content.ReadAsAsync<BranchModel>();
            
        }
    }
}
