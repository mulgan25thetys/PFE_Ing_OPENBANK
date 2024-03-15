using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Account.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Account.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _service;
        private readonly ILogger<AccountsController> _logger;
        private readonly IConfiguration _cfg;

        public AccountsController(IAccountService service, ILogger<AccountsController> logger, IConfiguration cfg)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(AccountList), 200)]
        public async Task<ActionResult<AccountList>> GetAllAccounts()
        {
            if (HttpContext.Request.Query.Count > 0 && HttpContext.Request.Query["q"].ToString() != null)
            {
                return await _service.GetAllFilteringAccounts(HttpContext.Request.Query["q"].ToString());
            }
            return await _service.GetAllAccounts();
        }

        [HttpGet]
        [Route("Custom")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(AccountList), 200)]
        public async Task<ActionResult<AccountList>> GetCustomAllAccounts()
        {
            if (HttpContext.Request.Query.Count > 0 && HttpContext.Request.Query["q"].ToString() != null)
            {
                return await _service.GetAllFilteringAccounts(HttpContext.Request.Query["q"].ToString(), HttpContext.Items["userId"].ToString());
            }
            return await _service.GetAllAccounts(HttpContext.Items["userId"].ToString());
        }

        [HttpGet("{account_number}", Name = "GetByAccountNumber")]
        [Authorize(Roles = "ADMIN, CUSTOMER")]
        [ProducesResponseType(typeof(AccountModel), 200)]
        [ProducesResponseType(typeof(AccountModel), 404)]
        public async Task<ActionResult<AccountModel>> GetByAccountNumber(Int64 account_number)
        {
            if (HttpContext.Items["userRole"] != null && HttpContext.Items["userRole"].ToString() == _cfg.GetValue<string>("AppRoles:CustomerRole"))
            {
                return await _service.GetAccount(account_number, HttpContext.Items["userId"].ToString());
            }
            return await _service.GetAccount(account_number);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "ADMIN, CUSTOMER")]
        public async Task<ActionResult<AccountModel>> AddAccount([FromBody] AccountRequest account)
        {
            string ownerId = "";
            if (HttpContext.Items["userId"] != null)
            {
                ownerId = HttpContext.Items["userId"].ToString();
            }
            return await _service.AddAccount(account, ownerId);
        }
    }
}
