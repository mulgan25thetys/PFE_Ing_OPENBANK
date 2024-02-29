using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Account.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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

        public AccountsController(IAccountService service, ILogger<AccountsController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(AccountList), 200)]
        public async Task<ActionResult<AccountList>> GetAllAccounts()
        {
            if (HttpContext.Request.Query.Count > 0 && HttpContext.Request.Query["q"].ToString() != null)
            {
                return await _service.GetAllFilteringAccounts(HttpContext.Request.Query["q"].ToString());
            }
            return await _service.GetAllAccounts();
        }

        [HttpGet("{account_number}", Name = "GetByAccountNumber")]
        [ProducesResponseType(typeof(AccountModel), 200)]
        [ProducesResponseType(typeof(AccountModel), 404)]
        public async Task<ActionResult<AccountModel>> GetByAccountNumber(Int64 account_number)
        {
            return await _service.GetAccount(account_number);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<AccountModel>> AddAccount([FromBody] AccountRequest account)
        {
            return await _service.AddAccount(account);
        }
    }
}
