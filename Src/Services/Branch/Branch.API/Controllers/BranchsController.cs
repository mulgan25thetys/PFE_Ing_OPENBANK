using Branch.API.Models;
using Branch.API.Models.Requests;
using Branch.API.Models.Response;
using Branch.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Branch.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
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
            int page = 1;
            int size = 10;
            string filter = "";
            if (HttpContext.Request.Query.Count > 0)
            {
                page = HttpContext.Request.Query["page"].ToString() != null ? Int32.Parse(HttpContext.Request.Query["page"].ToString()) : page;
                size = HttpContext.Request.Query["size"].ToString() != null ? Int32.Parse(HttpContext.Request.Query["size"].ToString()) : size;
                filter = HttpContext.Request.Query["q"].ToString() != null ? HttpContext.Request.Query["q"].ToString()  : filter;
            }

            if (filter !=null && filter.Length > 0 )
            {
                return await _service.GetBranchesByFilter(filter, page, size);
            }
            return await _service.GetAllBranches(page, size);
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
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<bool>> Addbranch([FromBody] BranchRequest branch)
        {
            return await _service.AddBranch(branch);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<bool>> Editbranch([FromBody] BranchModel branch)
        {
            return await _service.UpdateBranch(branch);
        }
    }
}
