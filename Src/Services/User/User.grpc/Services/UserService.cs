using User.grpc.Models;
using User.grpc.Services.Interfaces;

namespace User.grpc.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private string endPointUrl = "";

        public UserService(IConfiguration configuration, HttpClient httpClient)
        {

            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            //Setting endPoint
            endPointUrl = _config.GetValue<string>("OrdsSettings:Uri");
        }
        public async Task<UserModel> GetUserModelByEmailAsync(string email)
        {
            string filter = "?q={ \"email\": { \"$eq\":\" "+email+" \" } }";
            var response = await _client.GetAsync(endPointUrl + filter);
            //response.EnsureSuccessStatusCode();
            UserList list = await response.Content.ReadAsAsync<UserList>();
            return list.Items.FirstOrDefault();
        }

        public async Task<UserModel> GetUserModelByIdAsync(string userId)
        {
            var response = await _client.GetAsync(endPointUrl + userId);    
            return await response.Content.ReadAsAsync<UserModel>();
        }
    }
}
