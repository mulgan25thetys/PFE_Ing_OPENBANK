using AutoMapper;
using EventBus.Message.Events;
using MassTransit;
using MediatR;
using Notification.API.Features.Commands;

namespace Notification.API.EventBusConsumer
{
    public class SmsNotificationConsumer : IConsumer<SendSmsEvent>
    {
        private readonly ILogger<SmsNotificationConsumer> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _sender;

        public SmsNotificationConsumer(ILogger<SmsNotificationConsumer> logger, IMapper mapper, IMediator sender)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }
        public async Task Consume(ConsumeContext<SendSmsEvent> context)
        {
            _logger.LogInformation($"Sending sms to {context.Message.Destination}...");
            var smsMessage = _mapper.Map<SendSmsCmd>(context.Message);
            var success = await _sender.Send(smsMessage);

            if (success)
            {
                _logger.LogInformation($"Sms sent to {context.Message.Destination}...");
            }
        }
    }
}
