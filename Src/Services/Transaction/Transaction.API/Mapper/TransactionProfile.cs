using AutoMapper;
using Transaction.API.Features.Commands;
using Transaction.API.Models;

namespace Transaction.API.Mapper
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<TransactionModel, TransactionCmd>().ReverseMap();
        }
    }
}
