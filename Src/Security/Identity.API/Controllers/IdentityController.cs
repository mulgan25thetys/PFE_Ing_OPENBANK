using Identity.API.Applications.Dtos;
using Identity.API.Applications.Models;
using Identity.API.Applications.Networks;
using Identity.API.Models;
using Identity.API.Services.Interfaces;
using Identity.API.Utils.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Twilio.Http;

namespace Identity.API.Controllers
{
    [Route("api/v1/[controller]")]
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
        private readonly IJwtUtils _jwtUtils;

        public IdentityController(UserManager<IdentityUser> userManager,
            ISenderService sender, IJwtUtils jwtUtils,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            ILogger<IdentityController> logger,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = webHostEnvironment ?? throw new ArgumentException(nameof(webHostEnvironment));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _jwtUtils = jwtUtils ?? throw new ArgumentNullException(nameof(jwtUtils));
        }

        #region ADD USer
        [Authorize(Roles = "ADMIN" )]
        [Route("add-user")]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest authDto)
        {
            IdentityUser user = new IdentityUser();
            user.Email = authDto.Email;
            user.PhoneNumber = authDto.Phone;

            IdentityUser existedUser = await _userManager.FindByEmailAsync(user.Email);
            if (existedUser != null)
            {
                return this.StatusCode(StatusCodes.Status409Conflict, "Email address is already in used!");
            }

            int indexOfPoint = user.Email.IndexOf('.');
            int indexString = indexOfPoint == -1 ? user.Email.IndexOf('@') : indexOfPoint;
            user.NormalizedEmail = user.Email;
            user.UserName = user.Email.Substring(0, indexString);
            user.TwoFactorEnabled = true;
            string password = GetRandomString(10);

            //Add customer role if not exist
            var checkIfRoleExist = await this._roleManager.RoleExistsAsync(authDto.Role.ToString());

            if (!checkIfRoleExist)
            {
                await this._roleManager.CreateAsync(new IdentityRole()
                {
                    Name = authDto.Role.ToString(),
                    NormalizedName = authDto.Role.ToString(),
                });
            }

            var success = await this._userManager.CreateAsync(user, password);

            if (success.Succeeded)
            {
                IEnumerable<string> roles = new List<string>() { authDto.Role.ToString() };

                IdentityResult identityResult = await this._userManager.AddToRolesAsync(user, roles);

                if (identityResult.Succeeded)
                {
                    return await SendEmailTokenToConfirm(user, true, password);
                }
                return Problem(identityResult.ToString());
            }
            else
            {
                return BadRequest(success.Errors);
            }
        }
        #endregion 

        #region Register
        [Route("register")]
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

            //var result = await _signInManager.PasswordSignInAsync(user, authDto.Password, true, true);

            if (user != null && await _userManager.CheckPasswordAsync(user, authDto.Password))
            {
                if (!user.EmailConfirmed)
                {
                    return await SendEmailTokenToConfirm(user);
                }
                try
                {
                    await SetTwoFactorAuthentication(user);
                    return Ok(new MessageResponse() { Message = "Please check your phone number, a code has been sent to you by sms!", Token = await _jwtUtils.GetNotAuthenticatedToken(user) });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Two factor authentication: sending sms failed! " + ex.Message);
                    return this.Ok(await _jwtUtils.GetToken(user));
                }
            }
            else
            {
                return this.StatusCode(StatusCodes.Status401Unauthorized, "Unauthorized: Bad Credentials!");
            }
        }
        #endregion

        #region Phone Number manager
        [HttpPost]
        [Authorize]
        [Route("add-phone-number")]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberRequest request)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.Number);
            IdentityMessage message = new IdentityMessage
            {
                Destination = request.Number,
                Body = "Your security code is: " + code
            };
            // Send token
            return Ok(await _sender.SendSms(message));
        }
        [HttpPut]
        [Authorize]
        [Route("verify-phone-number")]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberRequest request)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var result = await _userManager.ChangePhoneNumberAsync(user, request.PhoneNumber, request.Code);

            if (result.Succeeded)
            {
                return Ok(await _jwtUtils.GetToken(user));
            }
            _logger.LogError($"Phone verification failed Number: {request.PhoneNumber}");
            return Problem();
        }
        #endregion

        #region Forgot Password
        [HttpPost]
        [Route("Forgot-Password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPassword)
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
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest passwordReset)
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
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            try
            {
                if (request.Email == null || request.Token == null)
                {
                    return BadRequest();
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return NotFound(user);
                }

                var result = await _userManager.ConfirmEmailAsync(user, request.Token);
                if (result.Succeeded)
                {
                    return Ok(new AuthenticationResponse() { Email = user.Email, Login = user.Email, UserName = user.UserName, Token = await _jwtUtils.GetToken(user) });
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
        private async Task<IActionResult> SendEmailTokenToConfirm(IdentityUser user, bool? isAdminUser =false, string? password="" )
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            _logger.LogInformation("Sending email to address "+ user.Email+" with token :" + token);
            string body = "<p>Hey " + user.UserName + ",</p>" +
                          "</br>" +
                          "<p>A sign in attempt requires further verification. To complete the sign in, use the verification token.</p> " +
                          "</br> " +
                          "<ul> " +
                          "<li>Username :" + user.UserName + "</li>";
                                    
            if (isAdminUser == true)
            {
                body += "<li>Password : " + password + "</li>";
            }
            body += "<li>Token : " + token + "</li>"+
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

        private async Task<bool> SetTwoFactorAuthentication(IdentityUser user)
        {
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            IdentityMessage message = new IdentityMessage
            {
                Destination = user.PhoneNumber,
                Body = "Your security code is: " + code
            };

            return await _sender.SendSms(message);
        }

        private string GetRandomString(int number)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[number];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
        #endregion
    }
}
