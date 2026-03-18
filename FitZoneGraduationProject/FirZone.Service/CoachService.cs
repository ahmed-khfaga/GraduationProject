using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec.ProfileSpec;
using FitZone.Service.DTOs.ProfileDTOs;
using FitZone.Service.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service
{
    public class CoachService : ICoachService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CoachService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<IEnumerable<CoachProfileDto>> GetAllCoachesAsync()
        {
            var coaches = await _uow.Repository<Coach>().GetAllWithSpecAsync(new AllCoachesSpec());
            var dtos = _mapper.Map<IEnumerable<CoachProfileDto>>(coaches).ToList();
            for (int i = 0; i < dtos.Count; i++)
                dtos[i].ProgramCount = coaches.ElementAt(i).WorkoutPrograms?.Count ?? 0;
            return dtos;
        }

        public async Task<CoachProfileDto?> GetCoachByIdAsync(int coachId)
        {
            var coach = await _uow.Repository<Coach>().GetWithSpecAsync(new CoachByIdSpec(coachId));
            if (coach is null) return null;
            var dto = _mapper.Map<CoachProfileDto>(coach);
            dto.ProgramCount = coach.WorkoutPrograms?.Count ?? 0;
            return dto;
        }

        public async Task<CoachProfileDto?> GetMyProfileAsync(string appUserId)
        {
            var coach = await _uow.Repository<Coach>().GetWithSpecAsync(new CoachByUserIdSpec(appUserId));
            if (coach is null) return null;
            var dto = _mapper.Map<CoachProfileDto>(coach);
            dto.ProgramCount = coach.WorkoutPrograms?.Count ?? 0;
            return dto;
        }

        public async Task<bool> UpdateMyProfileAsync(string appUserId, UpdateCoachProfileDto dto)
        {
            var coach = await _uow.Repository<Coach>().GetWithSpecAsync(new CoachByUserIdSpec(appUserId));
            if (coach is null) return false;
            coach.About = dto.About;
            coach.YearsOfExperience = dto.YearsOfExperience;
            coach.Price = dto.Price;
            _uow.Repository<Coach>().Update(coach);
            await _uow.CompleteAsync();
            return true;
        }
    }
}
