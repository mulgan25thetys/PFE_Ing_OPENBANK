using Identity.API.Applications.Dtos;
using Identity.API.Applications.Models.Entities;
using Identity.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EntitlementsController : ControllerBase
    {
        private readonly IEntitlementService _service;
        private readonly UserManager<UserModel> _userService;

        public EntitlementsController(IEntitlementService service, UserManager<UserModel> userService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService)); 
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAllEntitlements());
        }

        [Route("[action]/{user_id}", Name = "AddEntitlement")]
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> AddEntitlement(string user_id, [FromBody] EntitlementRequest request)
        {
            var user = await _userService.FindByIdAsync(user_id);
            if (user == null)
            {
                return NotFound("User not found!");
            }
            return Ok(_service.AddEntitlementForUserAsync(user_id,request));
        }
        [Route("[action]/{entitlement_id}", Name = "DeleteEntitlement")]
        [HttpDelete]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult DeleteEntitlement(string entitlement_id)
        {
            return Ok(_service.DeleteEntitlementAsync(entitlement_id));
        }
    }
}
