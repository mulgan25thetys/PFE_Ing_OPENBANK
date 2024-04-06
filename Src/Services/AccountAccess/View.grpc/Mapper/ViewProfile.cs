using AutoMapper;
using View.grpc.Models;
using View.grpc.Protos;

namespace View.grpc.Mapper
{
    public class ViewProfile : Profile
    {
        public ViewProfile()
        {
            CreateMap<ViewObject, ViewModel>().ReverseMap();
            CreateMap<ViewAccess, ViewAccessModel>().ReverseMap();
        }
    }
}
