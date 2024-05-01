using Identity.API.Applications.Dtos;
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
        public async Task<IActionResult> GetCurrentUser()
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }
            return Ok(await _userService.GetUserAsync(User.Identity.Name));
        }

        [Route("[action]/{email}", Name = "GetUserByEmail")]
        [HttpGet]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }
            IList<string> requiredRole = new List<string> { "SUPERADMIN", "CanGetAnyUser" };
            string userAuthorisations = (string)(HttpContext.Items["userRoles"] ?? "");

            if (!requiredRole.Intersect(userAuthorisations.Split(",").ToList()).Any())
            {
                return this.StatusCode(403, new MessageResponse() { Message = "OBP-20006: User is missing one or more roles:", Code = 403 });
            }
            return Ok(await _userService.GetUserByEmailAsync(email));
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            if (HttpContext.Items["userId"] == null)
            {
                return this.StatusCode(401, new MessageResponse() { Code = 401, Message = "OBP-20001: User not logged in. Authentication is required!" });
            }
            IList<string> requiredRole = new List<string> { "SUPERADMIN", "CanGetAnyUser" };
            string userAuthorisations = (string)(HttpContext.Items["userRoles"] ?? "");

            if (!requiredRole.Intersect(userAuthorisations.Split(",").ToList()).Any())
            {
                return this.StatusCode(403, new MessageResponse() { Message = "OBP-20006: User is missing one or more roles:", Code = 403 });
            }
            return Ok(await _userService.GetAllUsers());
        }
    }
}
