using AutoMapper;
using church_api.Controllers;
using church_api.Models;
using church_api.ViewModels;

namespace church_api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map PrayerCard to PrayerCardViewModel and vice versa
            CreateMap<PrayerCard, PrayerCardViewModel>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Desc)); // Handle different property names

            CreateMap<PrayerCardViewModel, PrayerCard>()
                .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => src.Description));
        }
    }
}
