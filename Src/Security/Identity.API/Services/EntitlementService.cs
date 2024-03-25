using Identity.API.Applications.Data;
using Identity.API.Applications.Dtos;
using Identity.API.Applications.Models.Entities;
using Identity.API.Applications.Models.Responses;
using Identity.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.Data;

namespace Identity.API.Services
{
    public class EntitlementService : IEntitlementService
    {
        private readonly UserManager<UserModel>  _userManager;
        private readonly RoleManager<Entitlement> _roleManager;
        private readonly IdentityContext _context;

        public EntitlementService(UserManager<UserModel> userManager, RoleManager<Entitlement> roleManager,
            IdentityContext context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<EntitlementResponse> AddEntitlementForUserAsync(string userId,EntitlementRequest request)
        {
            bool checkIfRoleExist = await this._roleManager.RoleExistsAsync(request.Role_name);
            if (checkIfRoleExist)
            {
                return GetEntitlementResponseFromModelAsync(await _roleManager.FindByNameAsync(request.Role_name));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new EntitlementResponse();
            }
            Entitlement role = new Entitlement() { Bank_id = request.Bank_Id, Name = request.Role_name, NormalizedName = request.Role_name};
            
            await _roleManager.CreateAsync(role);
            IEnumerable<string> roles = new List<string>() { role.Name };
            await this._userManager.AddToRolesAsync(user, roles);

            return  GetEntitlementResponseFromModelAsync(await _roleManager.FindByNameAsync(request.Role_name));
        }

        public async Task<bool> DeleteEntitlementAsync(string entitlement_id)
        {
            var role = await _roleManager.FindByIdAsync(entitlement_id);
            IdentityResult result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

        public Entitlements GetAllEntitlements()
        {
            IList<EntitlementResponse> list = new List<EntitlementResponse>();
            var roles = _context.Entitlements.ToList();
            foreach (var item in roles)
            {
                list.Add(GetEntitlementResponseFromModelAsync(item));
            }
            return new Entitlements() { list = list };  
        }

        private EntitlementResponse GetEntitlementResponseFromModelAsync(Entitlement itemRole)
        {
            EntitlementResponse entitlement = new EntitlementResponse()
            {
                Bank_id = itemRole.Bank_id,
                Entitlement_id = itemRole.Id,
                Role_name = itemRole.Name
            };
            return entitlement;
        }
    }
}
