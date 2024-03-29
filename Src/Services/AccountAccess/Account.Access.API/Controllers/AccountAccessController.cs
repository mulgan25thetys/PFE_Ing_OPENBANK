using Account.Access.API.Models;
using Account.Access.API.Models.Requests;
using Account.Access.API.Models.Responses;
using Account.Access.API.Services.Interfaces;
using Account.Access.API.Services.Grpc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Helper.Models;

namespace Account.Access.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    //[Authorize]
    public class AccountAccessController : ControllerBase
    {
        private readonly IAccountAccessService _service;
        private readonly ILogger<AccountAccessController> _logger;
        private readonly AccountService _accountService;

        public AccountAccessController(IAccountAccessService service, AccountService accountService, ILogger<AccountAccessController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        //[Authorize]
        [HttpPost("{account_id}/{bank_id}")]
        [ProducesResponseType(typeof(AccountAccessResponse), 200)]
        [ProducesResponseType(typeof(AccountModel), 404)]
        [ProducesResponseType(typeof(AddViewRequest), 400)]
        public async Task<IActionResult> CreateView(string account_id, string bank_id, AddViewRequest request)
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse () { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }
            var account = await _accountService.GetAccountDataAsync(account_id);
            if (account.Id.Length == 0)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
            if( account.Bankid != bank_id) 
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
            IList<string> alias = new List<string>() { "public", "private", ""};
            if (!alias.Contains(request.Alias.ToLower()))
            {
                return this.StatusCode(400, new MessageResponse() { Code = 400, Message = "The Alias value must be in '" + String.Join(",", alias) + "'" });
            }

            string ownerId = HttpContext.Items["userId"].ToString();

            if (account.Ownerid != ownerId)
            {
                return this.StatusCode(403, new MessageResponse () { Message = "User does not have owner access" , Code = 403});
            }
            return this.StatusCode(201, await _service.CreateView(account.Id, account.Bankid, request, ownerId));
        }

        [HttpPut("{account_id}/{bank_id}/{view_id}")]
        [ProducesResponseType(typeof(AccountAccessResponse), 200)]
        [ProducesResponseType(typeof(AccountModel), 404)]
        [ProducesResponseType(typeof(AddViewRequest), 400)]
        public async Task<IActionResult> UpdateView(string account_id, string bank_id, int view_id, UpdateViewRequest request)
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }
            var account = await _accountService.GetAccountDataAsync(account_id);
            if (account.Id.Length == 0)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
            if (account.Bankid != bank_id)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }

            if (await _service.GetOneAccessAsync(view_id) == null)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30005: View not found for Account. Please specify a valid value for VIEW_ID" });
            }
            IList<string> alias = new List<string>() { "public", "private", "" };
            if (!alias.Contains(request.Alias.ToLower()))
            {
                return this.StatusCode(400, new MessageResponse() { Code = 400, Message = "The Alias value must be in '" + String.Join(",", alias) + "'" });
            }
            string ownerId = HttpContext.Items["userId"].ToString() ?? "";
            if (account.Ownerid != ownerId)
            {
                return this.StatusCode(403, new MessageResponse() { Message = "User does not have owner access", Code = 403 });
            }
            return Ok(await _service.UpdateView(account.Id, account.Bankid, view_id, request));
        }

        [HttpDelete("[action]/{account_id}/{bank_id}/{view_id}")]
        [ProducesResponseType(typeof(AccountAccessResponse), 200)]
        [ProducesResponseType(typeof(AccountModel), 404)]
        [ProducesResponseType(typeof(AddViewRequest), 400)]
        public async Task<IActionResult> DeleteView(string account_id, string bank_id, int view_id)
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }
            var account = await _accountService.GetAccountDataAsync(account_id);
            if (account.Id.Length == 0)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
            if (account.Bankid != bank_id)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }

            AccountAccessResponse accessResponse = await _service.GetOneAccessAsync(view_id);
            if (accessResponse == null)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30005: View not found for Account. Please specify a valid value for VIEW_ID" });
            }
            

            if (accessResponse.Owner_id != HttpContext.Items["userId"].ToString())
            {
                return this.StatusCode(403, new MessageResponse() { Message = "User does not have access to owner view on accounts", Code = 403 });
            }
            await _service.DeleteViewAsync(account.Id, account.Bankid, view_id);
            return Ok();
        }

        [HttpPost("[action]/{account_id}/{bank_id}/{provider}/{provider_id}/{view_id}")]
        [ProducesResponseType(typeof(AccountAccessResponse), 200)]
        [ProducesResponseType(typeof(AccountAccessResponse), 404)]
        public async Task<IActionResult> GrantUserAccessToView(string account_id, string bank_id,string provider,string provider_id, int view_id)
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }

            IList<string> requiredRole = new List<string> { "SUPERADMIN", "CanGrantAccessToViews" };
            string userAuthorisations = (string)(HttpContext.Items["userRoles"] ?? "");

            if (!requiredRole.Intersect(userAuthorisations.Split(",").ToList()).Any())
            {
                return this.StatusCode(403, new MessageResponse() { Message = "OBP-20006: User is missing one or more roles:", Code = 403 });
            }
            var account = await _accountService.GetAccountDataAsync(account_id);
            if (account.Id.Length == 0)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
            if (account.Bankid != bank_id)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }

            AccountAccessResponse accessResponse = await _service.GetOneAccessAsync(view_id);
            if (accessResponse == null)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30005: View not found for Account. Please specify a valid value for VIEW_ID" });
            }


            if (accessResponse.Owner_id != HttpContext.Items["userId"].ToString())
            {
                return this.StatusCode(403, new MessageResponse() { Message = "User does not have access to owner view on account", Code = 403 });
            }

            AccountAccessResponse response = await _service.GrantUserAccessToView(provider, provider_id, view_id);
            if (response.Code > 0)
            {
                return this.StatusCode(response.Code, new MessageResponse() { Code = response.Code, Message = response.ErrorMessage });
            }
            return Ok(response);
        }

        [HttpDelete("[action]/{account_id}/{bank_id}/{provider}/{provider_id}/{view_id}")]
        [ProducesResponseType(typeof(AccountAccessResponse), 200)]
        [ProducesResponseType(typeof(AccountAccessResponse), 404)]
        public async Task<IActionResult> RevokeAccessToOneView(string account_id, string bank_id, string provider, string provider_id, int view_id)
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }

            IList<string> requiredRole = new List<string> { "SUPERADMIN", "CanGrantAccessToViews" };
            string userAuthorisations = (string)(HttpContext.Items["userRoles"] ?? "");

            if (!requiredRole.Intersect(userAuthorisations.Split(",").ToList()).Any())
            {
                return this.StatusCode(403, new MessageResponse() { Message = "OBP-20006: User is missing one or more roles:", Code = 403 });
            }
            var account = await _accountService.GetAccountDataAsync(account_id);
            if (account.Id.Length == 0)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
            if (account.Bankid != bank_id)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }

            AccountAccessResponse accessResponse = await _service.GetOneAccessAsync(view_id);
            if (accessResponse == null)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30005: View not found for Account. Please specify a valid value for VIEW_ID" });
            }


            if (accessResponse.Owner_id != HttpContext.Items["userId"].ToString())
            {
                return this.StatusCode(403, new MessageResponse() { Message = "User does not have access to owner view on account", Code = 403 });
            }

            return Ok(await _service.RevokeAccessToOneView(provider, provider_id, view_id));
        }

        [HttpGet("[action]/{account_id}/{bank_id}/{provider}/{provider_id}")]
        [ProducesResponseType(typeof(AccountAccessResponse), 200)]
        [ProducesResponseType(typeof(AccountAccessResponse), 404)]
        public async Task<IActionResult> GetAccountAccessForUser(string account_id, string bank_id, string provider, string provider_id)
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }
            var account = await _accountService.GetAccountDataAsync(account_id);
            if (account.Id.Length == 0)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
            if (account.Bankid != bank_id)
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }

            return Ok(await _service.GetAccountAccessForUser(provider, provider_id));
        }
    }
}
