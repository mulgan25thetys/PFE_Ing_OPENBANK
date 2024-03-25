using Identity.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            return Ok(await _userService.GetUserAsync(User.Identity.Name));
        }

        [Route("[action]/{email}", Name = "GetUserByEmail")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            return Ok(await _userService.GetUserByEmailAsync(email));
        }
    }
}
