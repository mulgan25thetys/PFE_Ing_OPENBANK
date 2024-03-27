using User.grpc.Models;
using User.grpc.Services.Interfaces;

namespace User.grpc.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private string endPointUrl = "";
        private readonly ILogger<UserService> _logger;

        public UserService(IConfiguration configuration, HttpClient httpClient, ILogger<UserService> logger)
        {

            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //Setting endPoint
            endPointUrl = _config.GetValue<string>("OrdsSettings:Uri");
        }
        public async Task<UserModel> GetUserModelByEmailAsync(string email)
        {
            string filter = "?q={ \"email\": { \"$eq\":\" "+email+" \" } }";
            var response = await _client.GetAsync(endPointUrl + filter);
            try
            {
                UserList list = await response.Content.ReadAsAsync<UserList>();
                return list.Items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UserModel();
            }
        }

        public async Task<UserModel> GetUserModelByIdAsync(string userId)
        {
            var response = await _client.GetAsync(endPointUrl + "/" + userId);
            try
            {
                return await response.Content.ReadAsAsync<UserModel>();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UserModel();
            }
        }
    }
}
