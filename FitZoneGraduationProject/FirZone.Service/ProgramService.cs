using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec.ProgramSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.ProgramDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    public class ProgramService : IProgramService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProgramService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProgramCardDto>> GetPublishedProgramsAsync(ProgramFilterParams filters)
        {
            var spec = new PublishedProgramsSpec(filters);
            var countSpec = new PublishedProgramsSpec(filters, countOnly: true);

            var programs = await _uow.Repository<WorkoutProgram>().GetAllWithSpecAsync(spec);
            var total = await _uow.Repository<WorkoutProgram>().CountAsync(countSpec);

            return new PaginatedResult<ProgramCardDto>
            {
                PageIndex = filters.PageIndex,
                PageSize = filters.PageSize,
                TotalCount = total,
                Data = _mapper.Map<IEnumerable<ProgramCardDto>>(programs)
            };
        }

        public async Task<ProgramDetailDto?> GetProgramDetailAsync(int programId)
        {
            var spec = new ProgramWithFullDetailSpec(programId);
            var program = await _uow.Repository<WorkoutProgram>().GetWithSpecAsync(spec);
            return program is null ? null : _mapper.Map<ProgramDetailDto>(program);
        }

        public async Task<IEnumerable<ProgramCardDto>> GetCoachProgramsAsync(int coachId)
        {
            var spec = new CoachProgramsSpec(coachId);
            var programs = await _uow.Repository<WorkoutProgram>().GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<ProgramCardDto>>(programs);
        }

        public async Task<IEnumerable<ProgramCardDto>> GetPendingProgramsAsync()
        {
            var spec = new PendingProgramsSpec();
            var programs = await _uow.Repository<WorkoutProgram>().GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<ProgramCardDto>>(programs);
        }

        public async Task<int> CreateProgramAsync(int coachId, CreateProgramDto dto)
        {
            var program = _mapper.Map<WorkoutProgram>(dto);
            program.CoachID = coachId;
            program.Status = ProgramStatus.Draft;

            _uow.Repository<WorkoutProgram>().Add(program);
            await _uow.CompleteAsync();

            return program.ID;
        }

        public async Task AddProgramWeekAsync(int programId, int coachId, CreateProgramWeekDto dto)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);

            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            if (program.Status is not (ProgramStatus.Draft or ProgramStatus.Rejected))
                throw new InvalidOperationException("Weeks can only be added to draft or rejected programs.");

            // ── Add the week ─────────────────────────────────────────
            var week = new ProgramWeek
            {
                WorkoutProgramID = programId,
                WeekNumber = dto.WeekNumber,
                WeekDescription = dto.WeekDescription,
                FocusArea = dto.FocusArea
            };

            _uow.Repository<ProgramWeek>().Add(week);

            // Flush the week so it gets a real Id before sessions reference it
            await _uow.CompleteAsync();

            // ── Add all sessions and their exercises in one batch ─────
            foreach (var sessionDto in dto.CreateWorkoutSessionDto)
            {
                var session = new WorkoutSession
                {
                    ProgramWeekID = week.ID,
                    SessionTitle = sessionDto.SessionTitle,
                    weekDay = sessionDto.WeekDay,
                    EstimatedDuration = sessionDto.EstimatedDuration,
                    WarmupNotes = sessionDto.WarmupNotes,
                    PrimerNotes = sessionDto.PrimerNotes,
                    CooldownNotes = sessionDto.CooldownNotes
                };

                _uow.Repository<WorkoutSession>().Add(session);

                // Flush the session so it gets a real Id before exercises reference it
                await _uow.CompleteAsync();

                foreach (var exDto in sessionDto.CreateSessionExerciseDto)
                {
                    _uow.Repository<SessionExercise>().Add(new SessionExercise
                    {
                        WorkoutSessionID = session.ID,
                        ExerciseID = exDto.ExerciseID,
                        SectionType = exDto.SectionType,
                        OrderInSection = exDto.OrderInSection,
                        Sets = exDto.Sets,
                        Reps = exDto.Reps,
                        RestSeconds = exDto.RestSeconds,
                        Tempo = exDto.Tempo,
                        RPETarget = exDto.RPETarget,
                        Notes = exDto.Notes
                    });
                }
                // Save all exercises for this session in one round-trip
                await _uow.CompleteAsync();
            }
        }

        public async Task SubmitForReviewAsync(int programId, int coachId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);

            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            if (program.Status is not (ProgramStatus.Draft or ProgramStatus.Rejected))
                throw new InvalidOperationException("Only draft or rejected programs can be submitted for review.");

            program.Status = ProgramStatus.PendingReview;
            _uow.Repository<WorkoutProgram>().Update(program);
            await _uow.CompleteAsync();
        }

        public async Task ReviewProgramAsync(int programId, AdminReviewDto dto)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);

            if (program is null)
                throw new InvalidOperationException("Program not found.");

            if (program.Status != ProgramStatus.PendingReview)
                throw new InvalidOperationException("Only programs pending review can be approved or rejected.");

            if (dto.Approve)
            {
                program.Status = ProgramStatus.Published;
                program.PublishedAt = DateTime.UtcNow;
                program.RejectionNote = null;
            }
            else
            {
                program.Status = ProgramStatus.Rejected;
                program.RejectionNote = dto.RejectionNote;
            }

            _uow.Repository<WorkoutProgram>().Update(program);
            await _uow.CompleteAsync();
        }
    }
}
