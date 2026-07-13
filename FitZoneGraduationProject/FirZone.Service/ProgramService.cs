using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec.ProgramSpec;
using FitZone.Core.Specifications.CommandSpec.SessionSpec;   // ← NEW
using FitZone.Core.Specifications.CommandSpec.EnrollmentSpec;
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

        // ── Read ──────────────────────────────────────────────────────────────

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
            var program = await _uow.Repository<WorkoutProgram>()
                                    .GetWithSpecAsync(new ProgramWithFullDetailSpec(programId));
            return program is null ? null : _mapper.Map<ProgramDetailDto>(program);
        }

        public async Task<IEnumerable<ProgramCardDto>> GetCoachProgramsAsync(int coachId)
        {
            var programs = await _uow.Repository<WorkoutProgram>()
                                     .GetAllWithSpecAsync(new CoachProgramsSpec(coachId));
            return _mapper.Map<IEnumerable<ProgramCardDto>>(programs);
        }

        // ── Coach mutations ───────────────────────────────────────────────────

        public async Task<int> CreateProgramAsync(int coachId, CreateProgramDto dto)
        {
            var program = _mapper.Map<WorkoutProgram>(dto);
            program.CoachId = coachId;
            program.IsPublished = false; // starts unpublished — coach decides when to go live
            _uow.Repository<WorkoutProgram>().Add(program);
            await _uow.CompleteAsync();
            return program.Id;
        }

        public async Task<bool> UpdateProgramAsync(int programId, int coachId, UpdateProgramDto dto)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachId != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            if (dto.TrackID != program.TrackId)
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
            if (program is null || program.CoachId != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            var week = new ProgramWeek
            {
                WorkoutProgramId = programId,
                WeekNumber = dto.WeekNumber,
                WeekDescription = dto.WeekDescription,
                FocusArea = dto.FocusArea,
                ProgressionNote = dto.ProgressionNote,
                NextWeekPreview = dto.NextWeekPreview
            };

            _uow.Repository<ProgramWeek>().Add(week);
            await _uow.CompleteAsync(); // get week.Id

            foreach (var sessionDto in dto.CreateWorkoutSessionDto)
            {
                var session = new WorkoutSession
                {
                    ProgramWeekId = week.Id,
                    SessionTitle = sessionDto.SessionTitle,
                    weekDay = sessionDto.WeekDay,
                    DayOrder = sessionDto.DayOrder,
                    EstimatedDuration = sessionDto.EstimatedDuration,
                    WarmupNotes = sessionDto.WarmupNotes,
                    PrimerNotes = sessionDto.PrimerNotes,
                    CooldownNotes = sessionDto.CooldownNotes
                };

                _uow.Repository<WorkoutSession>().Add(session);
                await _uow.CompleteAsync(); // get session.Id

                foreach (var exDto in sessionDto.CreateSessionExerciseDto)
                {
                    _uow.Repository<SessionExercise>().Add(new SessionExercise
                    {
                        WorkoutSessionId = session.Id,
                        ExerciseId = exDto.ExerciseID,
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
            var spec = new ProgramWeekByIdAndCoachSpec(programWeekId, coachId);
            var week = await _uow.Repository<ProgramWeek>().GetWithSpecAsync(spec);
            if (week is null) return false; // not found OR not owned by this coach → 404

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
            var spec = new ProgramWeekByIdAndCoachSpec(programWeekId, coachId);
            var week = await _uow.Repository<ProgramWeek>().GetWithSpecAsync(spec);
            if (week is null) return false; // not found OR not owned by this coach → 404

            _uow.Repository<ProgramWeek>().Delete(week);
            await _uow.CompleteAsync();
            return true;
        }



        // Coach reads one session individually (with its exercises) — was completely missing
        public async Task<WorkoutSessionDto?> GetSessionForCoachAsync(int sessionId, int coachId)
        {
            var spec = new WorkoutSessionFullByIdAndCoachSpec(sessionId, coachId);
            var session = await _uow.Repository<WorkoutSession>().GetWithSpecAsync(spec);
            if (session is null) return null;

            var dto = _mapper.Map<WorkoutSessionDto>(session);
            // SessionExerciseDto is deliberately .Ignore()'d in MappingProfile — must populate it here.
            dto.SessionExerciseDto = _mapper.Map<List<SessionExerciseDto>>(session.SessionExercises);
            return dto;
        }

        // Coach adds a brand-new session to an existing week — was completely missing
        public async Task<int> AddSessionAsync(int programWeekId, int coachId, CreateWorkoutSessionDto dto)
        {
            var weekSpec = new ProgramWeekByIdAndCoachSpec(programWeekId, coachId);
            var week = await _uow.Repository<ProgramWeek>().GetWithSpecAsync(weekSpec);
            if (week is null)
                throw new InvalidOperationException("Program week not found or access denied.");

            var session = new WorkoutSession
            {
                ProgramWeekId = programWeekId,
                SessionTitle = dto.SessionTitle,
                weekDay = dto.WeekDay,
                DayOrder = dto.DayOrder,
                EstimatedDuration = dto.EstimatedDuration,
                WarmupNotes = dto.WarmupNotes,
                PrimerNotes = dto.PrimerNotes,
                CooldownNotes = dto.CooldownNotes
            };
            _uow.Repository<WorkoutSession>().Add(session);
            await _uow.CompleteAsync(); // get session.Id

            foreach (var exDto in dto.CreateSessionExerciseDto)
            {
                _uow.Repository<SessionExercise>().Add(new SessionExercise
                {
                    WorkoutSessionId = session.Id,
                    ExerciseId = exDto.ExerciseID,
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
            return session.Id;
        }

        public async Task<bool> UpdateSessionAsync(int sessionId, int coachId, UpdateWorkoutSessionDto dto)
        {
            var spec = new WorkoutSessionByIdAndCoachSpec(sessionId, coachId);
            var session = await _uow.Repository<WorkoutSession>().GetWithSpecAsync(spec);
            if (session is null) return false; // not found OR not owned by this coach → 404

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
            var spec = new WorkoutSessionByIdAndCoachSpec(sessionId, coachId);
            var session = await _uow.Repository<WorkoutSession>().GetWithSpecAsync(spec);
            if (session is null) return false; // not found OR not owned by this coach → 404

            _uow.Repository<WorkoutSession>().Delete(session);
            await _uow.CompleteAsync();
            return true;
        }

        // Coach adds a single exercise to an existing session — was completely missing
        public async Task<int> AddSessionExerciseAsync(int sessionId, int coachId, CreateSessionExerciseDto dto)
        {
            var sessionSpec = new WorkoutSessionByIdAndCoachSpec(sessionId, coachId);
            var session = await _uow.Repository<WorkoutSession>().GetWithSpecAsync(sessionSpec);
            if (session is null)
                throw new InvalidOperationException("Session not found or access denied.");

            var sessionExercise = new SessionExercise
            {
                WorkoutSessionId = sessionId,
                ExerciseId = dto.ExerciseID,
                SectionType = dto.SectionType,
                OrderInSection = dto.OrderInSection,
                Sets = dto.Sets,
                Reps = dto.Reps,
                RestSeconds = dto.RestSeconds,
                Tempo = dto.Tempo,
                RPETarget = dto.RPETarget,
                Notes = dto.Notes
            };
            _uow.Repository<SessionExercise>().Add(sessionExercise);
            await _uow.CompleteAsync();
            return sessionExercise.Id;
        }

        // Coach edits a single exercise entry within a session — was completely missing
        public async Task<bool> UpdateSessionExerciseAsync(int sessionExerciseId, int coachId, CreateSessionExerciseDto dto)
        {
            var spec = new SessionExerciseByIdAndCoachSpec(sessionExerciseId, coachId);
            var se = await _uow.Repository<SessionExercise>().GetWithSpecAsync(spec);
            if (se is null) return false; // not found OR not owned by this coach → 404

            se.ExerciseId = dto.ExerciseID;
            se.SectionType = dto.SectionType;
            se.OrderInSection = dto.OrderInSection;
            se.Sets = dto.Sets;
            se.Reps = dto.Reps;
            se.RestSeconds = dto.RestSeconds;
            se.Tempo = dto.Tempo;
            se.RPETarget = dto.RPETarget;
            se.Notes = dto.Notes;

            _uow.Repository<SessionExercise>().Update(se);
            await _uow.CompleteAsync();
            return true;
        }

        // Coach removes a single exercise from a session — was completely missing
        public async Task<bool> DeleteSessionExerciseAsync(int sessionExerciseId, int coachId)
        {
            var spec = new SessionExerciseByIdAndCoachSpec(sessionExerciseId, coachId);
            var se = await _uow.Repository<SessionExercise>().GetWithSpecAsync(spec);
            if (se is null) return false; // not found OR not owned by this coach → 404

            _uow.Repository<SessionExercise>().Delete(se);
            await _uow.CompleteAsync();
            return true;
        }

        // ── Publish / Unpublish / Delete ──────────────────────────

        public async Task<bool> PublishProgramAsync(int programId, int coachId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachId != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            if (program.IsPublished) return true;

            program.IsPublished = true;
            program.PublishedAt = DateTime.UtcNow;
            _uow.Repository<WorkoutProgram>().Update(program);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> UnpublishProgramAsync(int programId, int coachId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachId != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            program.IsPublished = false;
            _uow.Repository<WorkoutProgram>().Update(program);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteProgramAsync(int programId, int coachId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null || program.CoachId != coachId)
                throw new InvalidOperationException("Program not found or access denied.");

            _uow.Repository<WorkoutProgram>().Delete(program);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> AdminDeleteProgramAsync(int programId)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(programId);
            if (program is null) return false;

            // TraineeProgramEnrollment → WorkoutProgram never cascade-deletes (enrollment
            // history must be preserved). If any enrollment exists — active or historical —
            // a hard delete would violate that FK. Instead, soft-delete: trainees already
            // enrolled keep full, unaffected access; the program disappears from the public
            // catalogue and the coach's own list via the IsDeleted filter in those specs.
            var enrollmentSpec = new EnrollmentsByProgramIdSpec(programId);
            var hasEnrollments = await _uow.Repository<TraineeProgramEnrollment>()
                .GetAllWithSpecAsync(enrollmentSpec);

            if (hasEnrollments.Any())
            {
                program.IsDeleted = true;
                _uow.Repository<WorkoutProgram>().Update(program);
                await _uow.CompleteAsync();
                return true;
            }

            _uow.Repository<WorkoutProgram>().Delete(program);
            await _uow.CompleteAsync();
            return true;
        }
    }
}