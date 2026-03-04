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

        public CoachService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CoachProfileDto>> GetAllCoachesAsync()
        {
            var spec = new AllCoachesSpec();
            var coaches = await _uow.Repository<Coach>().GetAllWithSpecAsync(spec);
            var dtos = _mapper.Map<IEnumerable<CoachProfileDto>>(coaches).ToList();

            // Attach program count
            for (int i = 0; i < dtos.Count; i++)
            {
                var coachEntity = coaches.ElementAt(i);
                dtos[i].ProgramCount = coachEntity.WorkoutPrograms?.Count ?? 0;
            }

            return dtos;
        }

        public async Task<CoachProfileDto?> GetCoachByIdAsync(int coachId)
        {
            var spec = new CoachByIdSpec(coachId);
            var coach = await _uow.Repository<Coach>().GetWithSpecAsync(spec);
            if (coach is null) return null;

            var dto = _mapper.Map<CoachProfileDto>(coach);
            dto.ProgramCount = coach.WorkoutPrograms?.Count ?? 0;
            return dto;
        }

        public async Task<CoachProfileDto?> GetMyProfileAsync(string appUserId)
        {
            var spec = new CoachByUserIdSpec(appUserId);
            var coach = await _uow.Repository<Coach>().GetWithSpecAsync(spec);
            if (coach is null) return null;

            var dto = _mapper.Map<CoachProfileDto>(coach);
            dto.ProgramCount = coach.WorkoutPrograms?.Count ?? 0;
            return dto;
        }

        public async Task<bool> UpdateMyProfileAsync(string appUserId, UpdateCoachProfileDto dto)
        {
            var spec = new CoachByUserIdSpec(appUserId);
            var coach = await _uow.Repository<Coach>().GetWithSpecAsync(spec);
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
