using Identity.API.Applications.Models;
using Identity.API.Models;

namespace Identity.API.Services.Interfaces
{
    public interface ISenderService
    {
        public Task<SenderResponse> SendEmail(Email email);
        public Task<bool> SendSms(IdentityMessage message);
    }
}
