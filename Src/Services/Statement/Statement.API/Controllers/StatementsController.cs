using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Statement.API.Models;
using Statement.API.Services.Interfaces;

namespace Statement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatementsController : ControllerBase
    {
        private readonly IStatementService _service;

        public StatementsController(IStatementService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<ActionResult<StatementModel>> GetStatements([FromQuery] Int64 account_number)
        {
            string queryRequest = "?q={\"$or\":[{\"trans_credited_acc\":{\"$eq\":\"" + account_number + "\"}},{\"trans_debited_acc\":{\"$eq\":\"" + account_number + "\"}}]}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }

        [Route("by-start-date")]
        [HttpGet]
        public async Task<ActionResult<StatementModel>> GetStatementByStartDate([FromQuery] Int64 account_number, [FromQuery] string start_date)
        {
            string queryRequest = "?q={\"$or\":[{\"trans_created_at\":{\"$eq\":{\"$date\":\""+ DateTime.Parse(start_date) +"\"}}},{\"trans_created_at\":{\"$gt\":{\"$date\":\"" + DateTime.Parse(start_date) + "\"}}}],\"$or\":[{\"trans_credited_acc\":{\"$eq\":\""+ account_number +"\"}},{\"trans_debited_acc\":{\"$eq\":\""+ account_number+ "\"}}]}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }

        [Route("by-end-date")]
        [HttpGet]
        public async Task<ActionResult<StatementModel>> GetStatementByEndDate(Int64 account_number, [FromQuery] string end_date)
        {
            string queryRequest = "?q={\"$or\":[{\"trans_created_at\":{\"$lt\":{\"$date\":\"" + DateTime.Parse(end_date) + "\"}}},{\"trans_created_at\":{\"$eq\":{\"$date\":\"" + DateTime.Parse(end_date) + "\"}}}],\"$or\":[{\"trans_credited_acc\":{\"$eq\":\"" + account_number + "\"}},{\"trans_debited_acc\":{\"$eq\":\"" + account_number + "\"}}]}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }

        [Route("between-two-dates")]
        [HttpGet]
        public async Task<ActionResult<StatementModel>> GetStatementBetweenDate([FromQuery] Int64 account_number, 
            [FromQuery] string start_date, [FromQuery] string end_date)
        {
            string queryRequest = "?q={\"$or\":[{\"trans_created_at\":{\"$gt\":{\"$date\":\"" + DateTime.Parse(start_date) + "\"}}},{\"trans_created_at\":{\"$lt\":{\"$date\":\"" + DateTime.Parse(end_date) + "\"}}}],\"$or\":[{\"trans_credited_acc\":{\"$eq\":\"" + account_number + "\"}},{\"trans_debited_acc\":{\"$eq\":\"" + account_number + "\"}}]}";
            return await _service.GetStatementAsync(account_number, queryRequest);
        }
    }
}
