using Account.API.Models;
using Account.API.Models.Requests;
using AutoMapper;
using EventBus.Message.Events;

namespace Account.API.Mapper
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AccountModel, AccountEvent>().ReverseMap();
        }
    }
}
