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

        public ProgramService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        // ── Read ──────

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
            var program = await _uow.Repository<WorkoutProgram>().GetWithSpecAsync(new ProgramWithFullDetailSpec(programId));
            return program is null ? null : _mapper.Map<ProgramDetailDto>(program);
        }

        public async Task<IEnumerable<ProgramCardDto>> GetCoachProgramsAsync(int coachId)
        {
            var programs = await _uow.Repository<WorkoutProgram>().GetAllWithSpecAsync(new CoachProgramsSpec(coachId));
            return _mapper.Map<IEnumerable<ProgramCardDto>>(programs);
        }

        // ── Coach mutations ───────

        public async Task<int> CreateProgramAsync(int coachId, CreateProgramDto dto)
        {
            var program = _mapper.Map<WorkoutProgram>(dto);
            program.CoachID = coachId;
            program.IsPublished = false; // starts unpublished — coach decides when to go live
            _uow.Repository<WorkoutProgram>().Add(program);
            await _uow.CompleteAsync();
            return program.ID;
        }

        public async Task<bool> UpdateProgramAsync(int programId, int coachId, UpdateProgramDto dto)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

         
            if (dto.TrackID != program.TrackID)
                throw new InvalidOperationException(
                    "The track of a program cannot be changed after creation. " +
                    "Delete and recreate the program if a different track is required.");

            _mapper.Map(dto, program);

            _uow.Repository<WorkoutProgram>().Update(program);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task AddProgramWeekAsync(int programId, int coachId, CreateProgramWeekDto dto)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            // ── Persist the week first ───────────────────────────────
            var week = new ProgramWeek
            {
                WorkoutProgramID = programId,
                WeekNumber = dto.WeekNumber,
                WeekDescription = dto.WeekDescription,
                FocusArea = dto.FocusArea,
                ProgressionNote = dto.ProgressionNote,
                NextWeekPreview = dto.NextWeekPreview
            };

            _uow.Repository<ProgramWeek>().Add(week);
            await _uow.CompleteAsync(); // get week.ID

            // ── Persist sessions + exercises ─────────────────────────
            foreach (var sessionDto in dto.CreateWorkoutSessionDto)
            {
                var session = new WorkoutSession
                {
                    ProgramWeekID = week.ID,
                    SessionTitle = sessionDto.SessionTitle,
                    weekDay = sessionDto.WeekDay,
                    DayOrder = sessionDto.DayOrder,
                    EstimatedDuration = sessionDto.EstimatedDuration,
                    WarmupNotes = sessionDto.WarmupNotes,
                    PrimerNotes = sessionDto.PrimerNotes,
                    CooldownNotes = sessionDto.CooldownNotes
                };

                _uow.Repository<WorkoutSession>().Add(session);
                await _uow.CompleteAsync(); // get session.ID

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
                await _uow.CompleteAsync();
            }
        }

        public async Task<bool> UpdateProgramWeekAsync(int programWeekId, int coachId, UpdateProgramWeekDto dto)
        {
            var week = await _uow.Repository<ProgramWeek>().GetAsync(programWeekId);
            if (week is null) return false;

            // Verify ownership through the program
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(week.WorkoutProgramID);
            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Access denied.");

            if (dto.WeekDescription is not null) week.WeekDescription = dto.WeekDescription;
            if (dto.FocusArea is not null) week.FocusArea = dto.FocusArea;
            if (dto.ProgressionNote is not null) week.ProgressionNote = dto.ProgressionNote;
            if (dto.NextWeekPreview is not null) week.NextWeekPreview = dto.NextWeekPreview;

            _uow.Repository<ProgramWeek>().Update(week);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteProgramWeekAsync(int programWeekId, int coachId)
        {
            var week = await _uow.Repository<ProgramWeek>().GetAsync(programWeekId);
            if (week is null) return false;

            var program = await _uow.Repository<WorkoutProgram>().GetAsync(week.WorkoutProgramID);
            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Access denied.");

            _uow.Repository<ProgramWeek>().Delete(week);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateSessionAsync(int sessionId, int coachId, UpdateWorkoutSessionDto dto)
        {
            var session = await _uow.Repository<WorkoutSession>().GetAsync(sessionId);
            if (session is null) return false;

            var week = await _uow.Repository<ProgramWeek>().GetAsync(session.ProgramWeekID);
            var program = week is null ? null : await _uow.Repository<WorkoutProgram>().GetAsync(week.WorkoutProgramID);

            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Access denied.");

            if (dto.SessionTitle is not null) session.SessionTitle = dto.SessionTitle;
            if (dto.WeekDay is not null) session.weekDay = dto.WeekDay.Value;
            if (dto.DayOrder is not null) session.DayOrder = dto.DayOrder.Value;
            if (dto.EstimatedDuration is not null) session.EstimatedDuration = dto.EstimatedDuration.Value;
            if (dto.WarmupNotes is not null) session.WarmupNotes = dto.WarmupNotes;
            if (dto.PrimerNotes is not null) session.PrimerNotes = dto.PrimerNotes;
            if (dto.CooldownNotes is not null) session.CooldownNotes = dto.CooldownNotes;

            _uow.Repository<WorkoutSession>().Update(session);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteSessionAsync(int sessionId, int coachId)
        {
            var session = await _uow.Repository<WorkoutSession>().GetAsync(sessionId);
            if (session is null) return false;

            var week = await _uow.Repository<ProgramWeek>().GetAsync(session.ProgramWeekID);
            var program = week is null ? null : await _uow.Repository<WorkoutProgram>().GetAsync(week.WorkoutProgramID);

            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Access denied.");

            _uow.Repository<WorkoutSession>().Delete(session);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> PublishProgramAsync(int programId, int coachId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            if (program.IsPublished) return true; // already live

            program.IsPublished = true;
            program.PublishedAt = DateTime.UtcNow;
            _uow.Repository<WorkoutProgram>().Update(program);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> UnpublishProgramAsync(int programId, int coachId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            program.IsPublished = false;
            _uow.Repository<WorkoutProgram>().Update(program);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteProgramAsync(int programId, int coachId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachID != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            _uow.Repository<WorkoutProgram>().Delete(program);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> AdminDeleteProgramAsync(int programId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null) return false;

            _uow.Repository<WorkoutProgram>().Delete(program);
            await _uow.CompleteAsync();
            return true;
        }
    }
}
