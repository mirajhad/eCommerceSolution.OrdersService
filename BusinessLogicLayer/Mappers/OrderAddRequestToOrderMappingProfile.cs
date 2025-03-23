using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class OrderAddRequestToOrderMappingProfile : Profile
    {
        public OrderAddRequestToOrderMappingProfile()
        {
            CreateMap<OrderAddRequest, Order>()
              .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
              .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
              .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
              .ForMember(dest => dest.OrderID, opt => opt.Ignore())
              .ForMember(dest => dest._id, opt => opt.Ignore())
              .ForMember(dest => dest.TotalBill, opt => opt.Ignore());
        }
    }
}
