using Branch.API.Models;
using Branch.API.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;
using Branch.API.Models.Response;
using Branch.API.Models.Requests;

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

        public async Task<bool> AddBranch(BranchRequest branch)
        {
            _logger.LogInformation("Addition of a branch in progress...");
                BranchModel model = new BranchModel() { ADDRESS = branch.ADDRESS, SPECIALISATION = branch.SPECIALISATION,
                EMAIL = branch.EMAIL, FAX = branch.FAX, MANAGER = branch.MANAGER, MANAGERNET = branch.MANAGERNET, NAME = branch.NAME,
                PHONE = branch.PHONE, REGION = branch.REGION, STATUS = BranchStatus.UNKNOWN.ToString(), CODE = branch.CODE, CREATEDAT = DateTime.Now,
                UPDATEDAT = DateTime.Now};

                var branchPost = JsonConvert.SerializeObject(model);

                var response = await _client.PostAsync(endPointUrl, new StringContent(branchPost, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode == false)
            {
                _logger.LogError($"Addition of the branch with code ${branch.CODE} failed");
            }
            else
            {
                _logger.LogInformation($"Branch with code ${branch.CODE} added successfully!");
            }
                await response.Content.ReadAsAsync<BranchModel>();
                return true;
        }

        public async Task<BranchList> GetAllBranches(int? page, int? size)
        {
            try
            {
                var result = await _client.GetAsync($"{endPointUrl}?offet={page}&limit={size}");

                return await result.Content.ReadAsAsync<BranchList>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new BranchList();
            }
        }

        public async Task<BranchModel> GetBranch(int id)
        {
            var result = await _client.GetAsync(endPointUrl + id);

            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsAsync<BranchModel>();
           
        }

        public async Task<BranchList> GetBranchesByFilter(string filter, int? page, int? size)
        {
            try
            {
                var filterJson = JsonConvert.DeserializeObject(filter);
                var result = await _client.GetAsync($"{endPointUrl}?q={filter}&offet={page}&limit={size}");

                return await result.Content.ReadAsAsync<BranchList>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return await GetAllBranches(1,10);
            }
        }

        public async Task<bool> UpdateBranch(BranchModel branch)
        {
            var exitedBranch = await this.GetBranch(branch.CODE);
            branch.CREATEDAT = exitedBranch.CREATEDAT;
            branch.UPDATEDAT = DateTime.Now;
            try
            {
                var branchPost = JsonConvert.SerializeObject(branch);

                var response = _client.PutAsync(endPointUrl + branch.CODE, new StringContent(branchPost, Encoding.UTF8, "application/json"));

                await response.Result.Content.ReadAsAsync<BranchModel>();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Updating branch failed! "+ex.Message);
                return false;
            }
        }
    }
}
