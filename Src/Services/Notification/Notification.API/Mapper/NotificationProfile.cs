using AutoMapper;
using EventBus.Message.Events;
using Notification.API.Features.Commands;

namespace Notification.API.Mapper
{
    public class NotificationProfile  : Profile
    {
        public NotificationProfile()
        {
            CreateMap<SendSmsCmd, SendSmsEvent>().ReverseMap();
            CreateMap<SendEmailCmd, SendEmailEvent>().ReverseMap();
        }
    }
}
