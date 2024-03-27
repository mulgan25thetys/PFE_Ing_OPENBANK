using Account.API.Models;
using Account.API.Models.Requests;
using Account.API.Models.Responses;
using Account.API.Services.Grpc;
using Account.API.Services.Interfaces;
using AutoMapper;
using Grpc.Core;
using Helper.Models;
using Helper.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Account.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]

    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _service;
        private readonly ILogger<AccountsController> _logger;
        private readonly IConfiguration _cfg;
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public AccountsController(IAccountService service, UserService userService, ILogger<AccountsController> logger,
            IConfiguration cfg, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{bank_id}", Name = "GetAllAccounts")]
        [Authorize]
        [ProducesResponseType(typeof(AccountListResponse), 200)]
        public async Task<ActionResult<AccountListResponse>> GetAllAccounts(string bank_id)
        {
            if (HttpContext.Request.Query.Count > 0 && HttpContext.Request.Query["q"].ToString() != null)
            {
                return await _service.GetAllFilteringAccounts(HttpContext.Request.Query["q"].ToString());
            }
            return await _service.GetAllAccounts(bank_id);
        }

        [HttpGet]
        [Route("Custom")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(AccountListResponse), 200)]
        public async Task<ActionResult<AccountListResponse>> GetCustomAllAccounts()
        {
            if (HttpContext.Request.Query.Count > 0 && HttpContext.Request.Query["q"].ToString() != null)
            {
                return await _service.GetAllFilteringAccounts(HttpContext.Request.Query["q"].ToString(), HttpContext.Items["userId"].ToString());
            }
            return await _service.GetAllAccounts(HttpContext.Items["userId"].ToString());
        }

        [HttpGet("[action]/{id}", Name = "GetByAccountById")]
        [Authorize]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        [ProducesResponseType(typeof(AccountResponse), 404)]
        public async Task<ActionResult<AccountResponse>> GetByAccountById(string id)
        {
            if (HttpContext.Items["userRole"] != null && HttpContext.Items["userRole"].ToString() == _cfg.GetValue<string>("AppRoles:CustomerRole"))
            {
                return await _service.GetAccountById(id, HttpContext.Items["userId"].ToString());
            }
            return await _service.GetAccountById(id);
        }

        [Route("{ACCOUNT_ID}/{BANK_ID}")]
        [HttpPost]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.BadRequest)]
        [Authorize]
        public async Task<IActionResult> AddAccount(string ACCOUNT_ID, string BANK_ID, [FromBody] AccountRequest account)
        {
            string ownerId = null;
            if (HttpContext.Items["userId"] != null)
            {
                ownerId = HttpContext.Items["userId"].ToString();
            }

            MessageResponse res = await ValidateData(account, ownerId, ACCOUNT_ID, BANK_ID);
            if (res.Code == 200)
            {
                AccountRequest request = new AccountRequest() { Balance = account.Balance, Label = account.Label, Type = account.Type };
                return Ok(await _service.AddAccount(ACCOUNT_ID, BANK_ID, request, ownerId));
            }
            else
            {
                return this.StatusCode(res.Code, res);
            }
        }
        [Route("ForOther/{ACCOUNT_ID}/{BANK_ID}")]
        [HttpPost]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(AccountModel), (int)HttpStatusCode.BadRequest)]
        [Authorize]
        public async Task<IActionResult> AddAccountForOther(string ACCOUNT_ID, string BANK_ID, [FromBody] OtherAccountRequest account)
        {
            AccountRequest request = _mapper.Map<AccountRequest>(account);
            MessageResponse res = await ValidateData(request, account.User_id, ACCOUNT_ID, BANK_ID);
            if (res.Code == 200)
            {
                return Ok(await _service.AddAccount(ACCOUNT_ID, BANK_ID, request, account.User_id));
            }
            else
            {
                return this.StatusCode(res.Code, res);
            }
        }

        [HttpPut("{ACCOUNT_ID}/{BANK_ID}")]
        public async Task<IActionResult> UpdateAccountLabel(string ACCOUNT_ID, string BANK_ID, UpdateLabelRequest request)
        {
            var account = await _service.GetAccountById(ACCOUNT_ID);
            if (account.Id != null)
            {
                request.Id = ACCOUNT_ID;
                return Ok(await _service.UpdateAccount(request));
            }
            else
            {
                return this.StatusCode(404, new MessageResponse() { Code = 404, Message = "OBP-30018: Bank Account not found. Please specify valid values for BANK_ID and ACCOUNT_ID." });
            }
        }

        private async Task<MessageResponse> ValidateData(AccountRequest account, string userId, string ACCOUNT_ID,string BANK_ID)
        {
            int statusCode = 201;
            string message = "Created";

            var bankIdMatch = Regex.Match(BANK_ID, @"^[0-9/a-z/A-Z/'-'/'.'/'_']{5,255}$");
            if (!bankIdMatch.Success || BANK_ID.Length > 255)
            {
                message = "OBP-30111: Invalid Bank Id. The BANK_ID should only contain 0-9/a-z/A-Z/'-'/'.'/'_', the length should be smaller than 255.";
                return new MessageResponse() { Code = 400, Message = message };
            }

            var isValidAccount = Guid.TryParse(ACCOUNT_ID, out _);
            
            if (!isValidAccount)
            {
                message = "OBP-30111: Invalid Account Id. The ACCOUNT_ID should only contain 0-9/a-z/A-Z/'-'/'.'/'_', the length should be smaller than 255.";
                return new MessageResponse() { Code = 400, Message = message };
            }

            var isValid = Guid.TryParse(userId, out _);
            if (!isValid)
            {
                return new MessageResponse()
                {
                    Code = 400,
                    Message = "OBP-30107: Invalid User Id."
                };
            }

            var user = await _userService.GetUserAsync(userId);
            if (user.UserId.Length == 0)
            {
                message = "OBP-20005: User not found. Please specify a valid value for USER_ID.";
                return new MessageResponse() { Code = 404, Message = message };
            }

            try
            {
                string currencySymbole = CurrencyCodeMapper.GetSymbol(account.Balance.Currency.ToUpper());
            }
            catch (Exception ex)
            {
                message = "OBP-30105: Invalid Balance Currency.";
                return new MessageResponse() { Code = 400, Message = message };
            }

            return new MessageResponse() {  Code = statusCode, Message = message  };
        }
    }
}
