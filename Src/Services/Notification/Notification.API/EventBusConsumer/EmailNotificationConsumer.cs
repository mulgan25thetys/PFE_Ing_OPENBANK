using AutoMapper;
using EventBus.Message.Events;
using MassTransit;
using MediatR;
using Notification.API.Features.Commands;

namespace Notification.API.EventBusConsumer
{
    public class EmailNotificationConsumer : IConsumer<SendEmailEvent>
    {
        private readonly ILogger<EmailNotificationConsumer> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _sender;

        public EmailNotificationConsumer(ILogger<EmailNotificationConsumer> logger, IMapper mapper, IMediator sender)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public async Task Consume(ConsumeContext<SendEmailEvent> context)
        {
            _logger.LogInformation($"Sending email to {context.Message.To}...");
            var smsMessage = _mapper.Map<SendEmailCmd>(context.Message);
            var result = await _sender.Send(smsMessage);

            if (result.Status == true)
            {
                _logger.LogInformation($"Email sent to {context.Message.To}...");
            }
            else
            {
                _logger.LogError($"Email not sent to {context.Message.To}...");
            }
            
        }
    }
}
