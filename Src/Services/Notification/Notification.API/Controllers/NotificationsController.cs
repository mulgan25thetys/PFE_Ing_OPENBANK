using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notification.API.Features.Commands;
using Notification.API.Models;
using Notification.API.Services.Interfaces;

namespace Notification.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _sender;

        public NotificationsController(IMediator sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendEmail(Email email)
        {
            SendEmailCmd cmd = new SendEmailCmd() { Emailrequest =  email };
            return Ok(await _sender.Send(cmd));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendSms(IdentityMessage message)
        {
            SendSmsCmd cmd = new SendSmsCmd() { SmsMessage = message };
            return Ok(await _sender.Send(cmd));
        }
    }
}
