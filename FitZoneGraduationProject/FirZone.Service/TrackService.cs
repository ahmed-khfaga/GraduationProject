using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Service.DTOs;
using FitZone.Service.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service
{
    public class TrackService : ITrackService
    {
        private readonly IGenericRepository<Track> _trackRepo;
        private readonly IMapper _mapper;

        public TrackService(IGenericRepository<Track> trackRepo, IMapper mapper)
        {
            _trackRepo = trackRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TrackDto>> GetAllTracksAsync()
        {
            var tracks = await _trackRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<TrackDto>>(tracks);
        }
    }
}
