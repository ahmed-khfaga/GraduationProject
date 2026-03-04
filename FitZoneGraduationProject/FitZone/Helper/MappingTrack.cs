using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs;

namespace FitZone.APIs.Helper
{
    public class MappingTrack : Profile
    {
        public MappingTrack() 
        {
            CreateMap<Track, TrackDto>();
        }
    }
}
