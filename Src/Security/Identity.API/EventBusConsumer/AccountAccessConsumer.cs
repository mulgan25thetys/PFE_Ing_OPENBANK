using EventBus.Message.Events;
using Identity.API.Models;
using Identity.API.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.EventBusConsumer
{
    public class AccountAccessConsumer : IConsumer<AccountAccessEvent>
    {
        private readonly ISenderService _sender;
        private readonly UserManager<IdentityUser> _userManager;    
        private readonly ILogger<AccountAccessConsumer> _logger;

        public AccountAccessConsumer(ISenderService sender, UserManager<IdentityUser> userManager, ILogger<AccountAccessConsumer> logger)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<AccountAccessEvent> context)
        {
            var customer = await _userManager.FindByIdAsync(context.Message.CUSTOMERID);
            if (customer != null)
            {
                Email email = new Email () { To = customer.Email, 
                    Body = "<p>A request for access to your bank account has been made. </p>" +
                           "</br>"+
                           $"<p>Account Id: </p>{context.Message.ACCNUMBER}"+
                           $"<p>Requested On: </p>{DateTime.Now.ToLongDateString()}", 
                    Subject = "" };
                await _sender.SendEmail(email);
                _logger.LogInformation($"Account Access event consumed successfully.");
            }
        }
    }
}
