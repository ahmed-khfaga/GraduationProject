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
    public class TraineeService : ITraineeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public TraineeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<TraineeProfileDto?> GetProfileAsync(string appUserId)
        {
            var spec = new TraineeByUserIdSpec(appUserId);
            var trainee = await _uow.Repository<Trainee>().GetWithSpecAsync(spec);
            return trainee is null ? null : _mapper.Map<TraineeProfileDto>(trainee);
        }

        public async Task<bool> UpdateProfileAsync(string appUserId, UpdateTraineeProfileDto dto)
        {
            var spec = new TraineeByUserIdSpec(appUserId);
            var trainee = await _uow.Repository<Trainee>().GetWithSpecAsync(spec);
            if (trainee is null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Gender)) trainee.Gender = dto.Gender;
            if (dto.Weight.HasValue) trainee.Weight = dto.Weight;
            if (dto.Height.HasValue) trainee.Height = dto.Height;
            if (dto.Address is not null) trainee.Address = dto.Address;
            if (dto.DateOfBirth.HasValue) trainee.DateOfBirth = dto.DateOfBirth;

            _uow.Repository<Trainee>().Update(trainee);
            await _uow.CompleteAsync();
            return true;
        }
    }
}
