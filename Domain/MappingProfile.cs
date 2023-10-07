using AutoMapper;
using bc_schools_api.Domain.Entities;
using bc_schools_api.Domain.Models;

namespace bc_schools_api.Domain
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Address, OriginAddress>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.FormattedAddress));
        }        
    }
}
