using AutoMapper;
using bc_schools_api.Domain.Models.Entities;
using bc_schools_api.Domain.Models.Response;
using bc_schools_api.Repository.DatabaseModels;

namespace bc_schools_api.Domain
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DbSchool, School>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.GradeRange, opt => opt.MapFrom(src => src.GradeRange))
                .ForMember(dest => dest.SchoolCategoryId, opt => opt.MapFrom(src => src.SchoolCategoryId))
                .ForMember(dest => dest.SchoolCategoryDesc, opt => opt.MapFrom(src => src.SchoolCategory.Description))
                .ForMember(dest => dest.SchoolTypeId, opt => opt.MapFrom(src => src.SchoolTypeId))
                .ForMember(dest => dest.SchoolTypeDesc, opt => opt.MapFrom(src => src.SchoolType.Description))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ReverseMap();
        }        
    }
}
