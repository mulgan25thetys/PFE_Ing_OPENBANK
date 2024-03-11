using Account.Access.API.Models;
using Account.Access.API.Models.Responses;
using Account.Access.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Account.Access.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountAccessController : ControllerBase
    {
        private readonly IAccountAccessService _service;
        private readonly ILogger<AccountAccessController> _logger;

        public AccountAccessController(IAccountAccessService service, ILogger<AccountAccessController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = "PROVIDER,ADMIN")]
        [HttpGet]
        [ProducesResponseType(typeof(AccountAccessList), 200)]
        public async Task<ActionResult<AccountAccessList>> GetAllAccountAccess()
        {
            return await _service.GetAllAccessAsync(HttpContext.Items["userId"].ToString());
        }

        [Authorize(Roles = "PROVIDER")]
        [Route("[action]/{account_number}", Name = "VerifyAccess")]
        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> VerifyAccess(long account_number)
        {
            return await _service.CheckAccessAsync(HttpContext.Items["userId"].ToString(), account_number);
        }

        [Authorize(Roles = "PROVIDER")]
        [Route("[action]/{account_number}", Name = "GetAccess")]
        [HttpGet]
        [ProducesResponseType(typeof(AccountAccess), 200)]
        [ProducesResponseType(typeof(AccountAccess), 404)]
        public async Task<ActionResult<AccountAccess>> GetAccess(long account_number)
        {
            return await _service.GetOneAccessAsync(HttpContext.Items["userId"].ToString(), account_number);
        }

        [Authorize(Roles = "PROVIDER")]
        [Route("[action]/{account_number}", Name = "GetAccount")]
        [HttpGet]
        [ProducesResponseType(typeof(AccountModel), 200)]
        [ProducesResponseType(typeof(AccountModel), 404)]
        public async Task<ActionResult<AccountModel>> GetAccount(long account_number)
        {
            return await _service.GetAccountAsync(HttpContext.Items["userId"].ToString(), account_number);
        }
    }
}
