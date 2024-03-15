using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Statement.API.Models;
using Statement.API.Services.Interfaces;

namespace Statement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class StatementsController : ControllerBase
    {
        private readonly IStatementService _service;

        public StatementsController(IStatementService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("{account_number}", Name = "GetStatement")]
        [Authorize(Roles = "ADMIN,CUSTOMER")]
        public async Task<ActionResult<StatementModel>> GetStatements(Int64 account_number)
        {
            string queryRequest = "?q={\"trans_credited_acc\":{\"$eq\":\"" + account_number + "\"}}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }

        [Route("[action]/{account_number}/{start_date}", Name = "ByStartDate")]
        [HttpGet]
        [Authorize(Roles = "ADMIN,CUSTOMER")]
        public async Task<ActionResult<StatementModel>> ByStartDate(Int64 account_number, string start_date)
        {
            string queryRequest = "?q={\"$or\":[{\"trans_created_at\":{\"$eq\":{\"$date\":\""+ DateTime.Parse(start_date) +"\"}}},{\"trans_created_at\":{\"$gt\":{\"$date\":\"" + DateTime.Parse(start_date) + "\"}}}],\"$or\":[{\"trans_credited_acc\":{\"$eq\":\""+ account_number +"\"}},{\"trans_debited_acc\":{\"$eq\":\""+ account_number+ "\"}}]}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }

        [Route("[action]/{account_number}/{end_date}", Name = "ByEndDate")]
        [HttpGet]
        [Authorize(Roles = "ADMIN,CUSTOMER")]
        public async Task<ActionResult<StatementModel>> ByEndDate(Int64 account_number, string end_date)
        {
            string queryRequest = "?q={\"$or\":[{\"trans_created_at\":{\"$lt\":{\"$date\":\"" + DateTime.Parse(end_date) + "\"}}},{\"trans_created_at\":{\"$eq\":{\"$date\":\"" + DateTime.Parse(end_date) + "\"}}}],\"$or\":[{\"trans_credited_acc\":{\"$eq\":\"" + account_number + "\"}},{\"trans_debited_acc\":{\"$eq\":\"" + account_number + "\"}}]}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }

        [Route("[action]/{account_number}/{start_date}/{end_date}", Name = "BetweenDates")]
        [HttpGet]
        [Authorize(Roles = "ADMIN,CUSTOMER")]
        public async Task<ActionResult<StatementModel>> BetweenDates(Int64 account_number, 
            string start_date, string end_date)
        {
            string queryRequest = "?q={\"$or\":[{\"trans_created_at\":{\"$gt\":{\"$date\":\"" + DateTime.Parse(start_date) + "\"}}},{\"trans_created_at\":{\"$lt\":{\"$date\":\"" + DateTime.Parse(end_date) + "\"}}}],\"$or\":[{\"trans_credited_acc\":{\"$eq\":\"" + account_number + "\"}},{\"trans_debited_acc\":{\"$eq\":\"" + account_number + "\"}}]}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }
    }
}
