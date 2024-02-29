using Branch.API.Models;
using Branch.API.Models.Requests;
using Branch.API.Models.Response;
using Branch.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Branch.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BranchsController : ControllerBase
    {
        private IBranchService _service;
        private ILogger<BranchsController> _logger;

        public BranchsController(IBranchService service, ILogger<BranchsController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(BranchList), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BranchList>> GetAllBranchs()
        {
            if (HttpContext.Request.Query.Count > 0 && HttpContext.Request.Query["q"].ToString() != null)
            {
                return await _service.GetBranchesByFilter(HttpContext.Request.Query["q"].ToString());
            }
            
            return await _service.GetAllBranches();
        }

        [HttpGet("{code}", Name = "GetByCode")]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<BranchModel>> GetBranch(int code)
        {
            return await _service.GetBranch(code);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<bool>> Addbranch([FromBody] BranchRequest branch)
        {
            return await _service.AddBranch(branch);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<bool>> Editbranch([FromBody] BranchModel branch)
        {
            return await _service.UpdateBranch(branch);
        }
    }
}
