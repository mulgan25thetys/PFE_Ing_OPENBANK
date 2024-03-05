using Identity.API.Applications.Dtos;
using Identity.API.Applications.Models;
using Identity.API.Applications.Networks;
using Identity.API.Models;
using Identity.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IdentityController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISenderService _sender;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IAuthorizationService _authorization;

        public IdentityController(UserManager<IdentityUser> userManager,
            ISenderService email,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            ILogger<IdentityController> logger,
            SignInManager<IdentityUser> signInManager,
            IAuthorizationService authorization)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = webHostEnvironment ?? throw new ArgumentException(nameof(webHostEnvironment));
            _sender = email ?? throw new ArgumentNullException(nameof(email));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
        }

        #region Register
        [Route("register", Name = "Index")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest authDto)
        {
            IdentityUser user = new IdentityUser();
            user.Email = authDto.Email;

            IdentityUser existedUser = await _userManager.FindByEmailAsync(user.Email);
            if (existedUser != null)
            {
                return this.StatusCode(StatusCodes.Status409Conflict, "Email address is already in used!");
            }

            user.NormalizedEmail = user.Email;
            user.UserName = authDto.UserName;
            user.PhoneNumber = authDto.Phone;
            user.TwoFactorEnabled = true;

            //Add customer role if not exist
            var checkIfRoleExist = await this._roleManager.RoleExistsAsync(_configuration.GetValue<string>("AppRoles:CustomerRole"));

            if (!checkIfRoleExist)
            {
                await this._roleManager.CreateAsync(new IdentityRole()
                {
                    Name = _configuration.GetValue<string>("AppRoles:CustomerRole"),
                    NormalizedName = _configuration.GetValue<string>("AppRoles:CustomerRole"),
                });
            }

            var success = await this._userManager.CreateAsync(user, authDto.Password);

            if (success.Succeeded)
            {
                IEnumerable<string> roles = new List<string>() { _configuration.GetValue<string>("AppRoles:CustomerRole") };

                IdentityResult identityResult = await this._userManager.AddToRolesAsync(user, roles);

                if (identityResult.Succeeded)
                {
                    return await SendEmailTokenToConfirm(user);
                }
                return Problem(identityResult.ToString());
            }
            else
            {
                return BadRequest(success.Errors);
            }
        }
        #endregion 

        #region Authentication
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest authDto)
        {
            IdentityUser user = await _userManager.FindByNameAsync(authDto.Login);

            user = user == null ? await _userManager.FindByEmailAsync(authDto.Login) : user;

            if (user == null || await _userManager.CheckPasswordAsync(user, authDto.Password) == false)
            {
                return this.StatusCode(StatusCodes.Status401Unauthorized, "Unauthorized: Bad Credentials!");
            }

            if (!user.EmailConfirmed)
            {
                return this.StatusCode(StatusCodes.Status401Unauthorized, "Unauthorized: Please confirm your email address: "+user.Email);
            }

            try
            {
                return await SetTwoFactorAuthentication(user);
            }catch(Exception ex)
            {
                _logger.LogError("Two factor authentication failed! "+ex.Message);
                return this.Ok(new AuthenticationResponse()
                {
                    Login = authDto.Login,
                    Email = user.Email,
                    UserName = user.UserName,
                    Token = await this.GenerateJwtToken(user)
                });
            }
        }
        #endregion

        #region Forgot Password
        [HttpPost]
        [Route("Forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest forgotPassword)
        {
            try
            {
                IdentityUser user = await _userManager.FindByEmailAsync(forgotPassword.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    string body = "<p>Hey " + user.UserName + ",</p>" +
                          "</br>" +
                          "<p>A sign in attempt requires further verification. To complete the sign in, use the verification token.</p> " +
                          "</br> " +
                          "<ul> " +
                          "<li>Token : " + token + "</li>" +
                          "</ul> " +
                          "</br>" +
                          "<p>Thanks,</p>";

                    Email email = new Email() { Body = body, Subject = "Please verify your Email", To = user.Email };
                    SenderResponse result = await _sender.SendEmail(email);

                    if (result.Status == false)
                    {
                        return Accepted(result.Message);
                    }
                    return Ok(result.Message + "to " + user.Email);
                }
                return this.StatusCode(StatusCodes.Status404NotFound, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Problem(ex.Message);
            }
        }
        #endregion

        #region Reset Password
        [HttpPut]
        [Route("Reset-Password")]
        public async Task<IActionResult> ResetPassword(PasswordResetRequest passwordReset)
        {
            try
            {
                IdentityUser user = await _userManager.FindByEmailAsync(passwordReset.Email);
                if (user != null)
                {
                    var isPasswordReset = await _userManager.ResetPasswordAsync(user, passwordReset.Token, passwordReset.Password);

                    if (isPasswordReset.Succeeded)
                    {
                        return Ok(new AuthenticationResponse() { Email = user.Email, Login = user.Email, UserName = user.UserName, Token = null });
                    }
                    else
                    {
                        return Problem("Cannot reset your password, Please retry!");
                    }
                }
                return this.StatusCode(StatusCodes.Status404NotFound, passwordReset.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Problem(ex.Message);
            }
        }
        #endregion

        #region Email Confirmation
        [HttpGet]
        [Route("Confirm-Email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            try
            {
                if (email == null || token == null)
                {
                    return BadRequest();
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(user);
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return Ok(new AuthenticationResponse() { Email = user.Email, Login = user.Email, UserName = user.UserName, Token = await this.GenerateJwtToken(user) });
                }

                return BadRequest("Email not confirmed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Problem(ex.Message);
            }
        }
        #endregion

        #region Logout
        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return this.StatusCode(StatusCodes.Status200OK, "You are logged out!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Problem(ex.Message);
            }
        }
        #endregion

        #region Private methods
        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var userRoles = await this._userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<IActionResult> SendEmailTokenToConfirm(IdentityUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            _logger.LogInformation("Sending email to address "+ user.Email+" with token :" + token);
            string body = "<p>Hey " + user.UserName + ",</p>" +
                          "</br>" +
                          "<p>A sign in attempt requires further verification. To complete the sign in, use the verification token.</p> " +
                          "</br> " +
                          "<ul> " +
                          "<li>Token : "+token+"</li>" +
                          "</ul> " +
                          "</br>" +
                          "<p>Thanks,</p>";

            Email email = new Email() { Body = body, Subject = "Please verify your Email", To = user.Email };
            SenderResponse result = await _sender.SendEmail(email);

            if (result.Status == false)
            {
                _logger.LogError("Failed to send email to address: "+email.To.ToString());
                return Accepted(result.Message);
            }
            return Ok(result.Message + "to " + user.Email);
        }

        private async Task<IActionResult> SetTwoFactorAuthentication(IdentityUser user)
        {
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            if (string.IsNullOrEmpty(authenticatorKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            foreach (var code in recoveryCodes)
            {
                System.Console.WriteLine( "code! "+code.ToString());
            }

            return Ok(await _sender.SendSms(user.PhoneNumber));
        }
        #endregion
    }
}
