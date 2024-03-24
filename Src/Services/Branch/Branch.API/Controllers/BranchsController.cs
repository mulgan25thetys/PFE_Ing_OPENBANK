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
        [ProducesResponseType(typeof(BranchListResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BranchListResponse>> GetAllBranchs()
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

        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType(typeof(BranchResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BranchResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<BranchResponse>> GetBranch(string id)
        {
            return await _service.GetBranch(id);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BranchResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(BranchResponse), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<BranchResponse>> Addbranch([FromBody] BranchRequest branch)
        {
            return await _service.AddBranch(branch);
        }

        [HttpDelete("{id}", Name = "DeleteBranchById")]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<bool>> DeleteBranch(string id)
        {
            return await _service.DeleteBranch(id);
        }
    }
}
