using Identity.API.Applications.Data;
using Identity.API.Applications.Models.Entities;
using Identity.API.Applications.Models.Responses;
using Identity.API.Services.Interfaces;
using Identity.API.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;

namespace Identity.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly RoleManager<Entitlement> _roleManager;
        private readonly IConfiguration _config;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<UserModel> userManager, 
            RoleManager<Entitlement> roleManager,IConfiguration config, ILogger<UserService> logger, IEntitlementService entitlementService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _config = config ?? throw new ArgumentNullException(nameof(_config));
            _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
        }

        public async Task<UserResponse> GetUserAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                return await GetUserResponseFromModel(user);

            }
            return null;
        }

        public async Task<UserResponse> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return await GetUserResponseFromModel(user);

            }
            return null;
        }
        private async Task<UserResponse> GetUserResponseFromModel(UserModel model)
        {
            UserResponse response = new UserResponse()
            {
                Email = model.Email,
                Provider = model.Provider,
                Provider_id = model.Provider_id,
                Username = model.UserName,
                User_id = model.Id
            };
            
            IList<EntitlementResponse> list = new List<EntitlementResponse>();
            var roles = await _userManager.GetRolesAsync(model);
            foreach (var role in roles.Where( r => !SystemRoles.Roles.Contains(r)))
            {
                
                    var itemRole = await _roleManager.FindByNameAsync(role.ToString());
                    EntitlementResponse entitlement = new EntitlementResponse()
                    {
                        Bank_id = itemRole.Bank_id,
                        Entitlement_id = itemRole.Id,
                        Role_name = itemRole.Name
                    };
                    list.Add(entitlement);
                
            }
            response.Entitlements = new Entitlements() { list = list };
            return response;
        }
    }
}
