using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec.EnrollmentSpec;
using FitZone.Core.Specifications.CommandSpec.ProgramSpec;
using FitZone.Core.Specifications.CommandSpec.SessionSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public EnrollmentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // ── Dashboard: active enrollments ────────────────────────────────────

        public async Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId)
        {
            var enrollments = await _uow.Repository<TraineeProgramEnrollment>()
                .GetAllWithSpecAsync(new TraineeActiveEnrollmentsSpec(traineeId));

            var result = new List<EnrollmentDto>();
            foreach (var e in enrollments)
            {
                await SyncMaxWeekUnlockedAsync(e);
                result.Add(MapToEnrollmentDto(e));
            }
            return result;
        }

        // ── Full history ──────────────────────────────────────────────────────

        public async Task<IEnumerable<EnrollmentHistoryDto>> GetMyEnrollmentHistoryAsync(int traineeId)
        {
            var enrollments = await _uow.Repository<TraineeProgramEnrollment>()
                .GetAllWithSpecAsync(new TraineeAllEnrollmentsSpec(traineeId));

            return enrollments.Select(e => new EnrollmentHistoryDto
            {
                Id = e.Id,
                WorkoutProgramID = e.WorkoutProgramId,
                ProgramName = e.WorkoutProgram.Name,
                TrackName = e.Track.Name,
                MaxWeekUnlocked = e.MaxWeekUnlocked,
                TotalWeeks = e.WorkoutProgram.DurationOnWeeks,
                Status = e.Status.ToString(),
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsActive = e.IsActive
            });
        }

        // ── Week overview cards ──────────────────────────────────────────

        public async Task<IEnumerable<WeekOverviewDto>> GetWeekOverviewAsync(int enrollmentId, int traineeId)
        {
            var enrollment = await _uow.Repository<TraineeProgramEnrollment>()
                .GetWithSpecAsync(new EnrollmentByIdSpec(enrollmentId, traineeId))
                ?? throw new InvalidOperationException("Enrollment not found or access denied.");

            await SyncMaxWeekUnlockedAsync(enrollment);

            var weeks = await _uow.Repository<ProgramWeek>()
                .GetAllWithSpecAsync(new ProgramWeeksByProgramSpec(enrollment.WorkoutProgramId));

            return weeks.Select(w => new WeekOverviewDto
            {
                WeekNumber = w.WeekNumber,
                WeekDescription = w.WeekDescription,
                FocusArea = w.FocusArea,
                ProgressionNote = w.ProgressionNote,
                NextWeekPreview = w.NextWeekPreview,
                SessionCount = w.WorkoutSessions?.Count ?? 0,
                IsUnlocked = w.WeekNumber <= enrollment.MaxWeekUnlocked
            });
        }

        // ── Single week detail ────────────────────────────────────────────────

        public async Task<WeekDetailDto?> GetWeekAsync(int enrollmentId, int weekNumber, int traineeId)
        {
            var enrollment = await _uow.Repository<TraineeProgramEnrollment>()
                .GetWithSpecAsync(new EnrollmentByIdSpec(enrollmentId, traineeId))
                ?? throw new InvalidOperationException("Enrollment not found.");

            await SyncMaxWeekUnlockedAsync(enrollment);

            if (weekNumber > enrollment.MaxWeekUnlocked)
                throw new InvalidOperationException(
                    $"Week {weekNumber} is not yet unlocked. Your current access is up to Week {enrollment.MaxWeekUnlocked}.");

            var week = await _uow.Repository<ProgramWeek>()
                .GetWithSpecAsync(new ProgramWeekByNumberSpec(enrollment.WorkoutProgramId, weekNumber));

            if (week is null) return null;

            return new WeekDetailDto
            {
                WeekNumber = week.WeekNumber,
                WeekDescription = week.WeekDescription,
                FocusArea = week.FocusArea,
                ProgressionNote = week.ProgressionNote,
                NextWeekPreview = week.NextWeekPreview,
                IsUnlocked = true,
                SessionSummaryDto = week.WorkoutSessions
                    .OrderBy(s => s.weekDay)
                    .ThenBy(s => s.DayOrder)
                    .Select(s => new SessionSummaryDto
                    {
                        Id = s.Id,
                        SessionTitle = s.SessionTitle,
                        WeekDay = s.weekDay.ToString(),
                        DayOrder = s.DayOrder,
                        EstimatedDuration = s.EstimatedDuration
                    })
                    .ToList()
            };
        }

        // ── Session detail ────────────────────────────────────────────────────

        public async Task<WorkoutSessionDto?> GetSessionDetailAsync(int sessionId, int traineeId)
        {
            var session = await _uow.Repository<WorkoutSession>()
                .GetWithSpecAsync(new SessionWithExercisesSpec(sessionId));

            if (session is null) return null;

            int programId = session.ProgramWeek.WorkoutProgramId;
            int weekNumber = session.ProgramWeek.WeekNumber;

            var enrollment = await _uow.Repository<TraineeProgramEnrollment>()
                .GetWithSpecAsync(new SessionAccessGateSpec(traineeId, programId))
                ?? throw new InvalidOperationException("You are not enrolled in this program.");

            await SyncMaxWeekUnlockedAsync(enrollment);

            if (weekNumber > enrollment.MaxWeekUnlocked)
                throw new InvalidOperationException(
                    $"Week {weekNumber} is not yet unlocked. Complete the previous week first.");

            var exercises = await _uow.Repository<SessionExercise>()
                .GetAllWithSpecAsync(new SessionExercisesForSessionSpec(sessionId));

            var dto = _mapper.Map<WorkoutSessionDto>(session);
            dto.SessionExerciseDto = _mapper.Map<List<SessionExerciseDto>>(
                exercises.OrderBy(e => SectionOrder(e.SectionType))
                         .ThenBy(e => e.OrderInSection));

            return dto;
        }

        // ── Enroll / Cancel ───────────────────────────────────────────────────

        public async Task<EnrollmentDto> StartProgramAsync(int traineeId, StartProgramDto dto)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(dto.ProgramID);
            if (program is null || !program.IsPublished)
                throw new InvalidOperationException("Program not found or not available.");

            // Check for existing active enrollment on the same track
            var activeSpec = new ActiveEnrollmentByTrackSpec(traineeId, program.TrackId);
            var activeOther = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(activeSpec);

            if (activeOther is not null && activeOther.WorkoutProgramId == dto.ProgramID)
            {
                // Same program already active — sync and return current state
                await SyncMaxWeekUnlockedAsync(activeOther);
                await _uow.CompleteAsync();
                return MapToEnrollmentDto(activeOther);
            }

            if (activeOther is not null)
            {
                // Different program on same track — suspend it (preserve progress)
                activeOther.IsActive = false;
                activeOther.Status = EnrollmentStatus.Cancelled;
                activeOther.EndDate = DateTime.UtcNow;
                _uow.Repository<TraineeProgramEnrollment>().Update(activeOther);
            }

            // Check for a previous enrollment in THIS program (resume)
            var previousSpec = new PreviousEnrollmentInProgramSpec(traineeId, dto.ProgramID);
            var previous = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(previousSpec);

            if (previous is not null)
            {
                // Resume — backdate StartDate so the time-based unlock restores their progress
                previous.IsActive = true;
                previous.Status = EnrollmentStatus.Active;
                previous.EndDate = null;
                previous.StartDate = BackdateStartDate(previous.MaxWeekUnlocked);
                _uow.Repository<TraineeProgramEnrollment>().Update(previous);
                await _uow.CompleteAsync();
                return MapToEnrollmentDto(previous);
            }

            // Fresh enrollment
            var enrollment = new TraineeProgramEnrollment
            {
                TraineeId = traineeId,
                WorkoutProgramId = dto.ProgramID,
                TrackId = program.TrackId,
                StartDate = DateTime.UtcNow,
                MaxWeekUnlocked = 1,
                Status = EnrollmentStatus.Active,
                IsActive = true
            };

            _uow.Repository<TraineeProgramEnrollment>().Add(enrollment);
            await _uow.CompleteAsync();

            // Reload with navigation properties for the DTO
            var loaded = await _uow.Repository<TraineeProgramEnrollment>()
                .GetWithSpecAsync(new EnrollmentByIdSpec(enrollment.Id, traineeId));

            return MapToEnrollmentDto(loaded!);
        }

        public async Task CancelEnrollmentAsync(int enrollmentId, int traineeId)
        {
            var enrollment = await _uow.Repository<TraineeProgramEnrollment>()
                .GetWithSpecAsync(new EnrollmentByIdSpec(enrollmentId, traineeId))
                ?? throw new InvalidOperationException("Enrollment not found.");

            if (!enrollment.IsActive)
                throw new InvalidOperationException("This enrollment is already inactive.");

            enrollment.IsActive = false;
            enrollment.Status = EnrollmentStatus.Cancelled;
            enrollment.EndDate = DateTime.UtcNow;
            _uow.Repository<TraineeProgramEnrollment>().Update(enrollment);
            await _uow.CompleteAsync();
        }

        // ── Private helpers ───────────────────────────────────────────────────

    
        /// Computes how many weeks are due based on elapsed calendar weeks since StartDate,
        /// updates the DB row if the value has gone up, and marks the enrollment Completed
        /// when the trainee has passed the final week.
       
        private async Task SyncMaxWeekUnlockedAsync(TraineeProgramEnrollment enrollment)
        {
            int due = ComputeWeeksDue(
                enrollment.StartDate,
                enrollment.WorkoutProgram?.DurationOnWeeks ?? int.MaxValue);

            if (due <= enrollment.MaxWeekUnlocked) return;

            enrollment.MaxWeekUnlocked = due;

            if (enrollment.WorkoutProgram is not null &&
                due >= enrollment.WorkoutProgram.DurationOnWeeks &&
                enrollment.IsActive)
            {
                enrollment.Status = EnrollmentStatus.Completed;
                enrollment.IsActive = false;
                enrollment.EndDate = DateTime.UtcNow;
            }

            _uow.Repository<TraineeProgramEnrollment>().Update(enrollment);
            await _uow.CompleteAsync();
        }

        /// Week 1 is unlocked immediately on enrollment.
        /// Week N unlocks after (N-1) full calendar weeks (Monday-anchored) have passed.
      
        private static int ComputeWeeksDue(DateTime startDate, int maxWeeks)
        {
            var startMonday = GetWeekMonday(startDate.Date);
            var currentMonday = GetWeekMonday(DateTime.UtcNow.Date);
            int weeksPassed = (int)((currentMonday - startMonday).TotalDays / 7);
            return Math.Min(weeksPassed + 1, maxWeeks);
        }

        private static DateTime GetWeekMonday(DateTime date)
        {
            int offset = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.AddDays(-offset);
        }

        
        /// When resuming, we backdate StartDate so that the time-based unlock
        /// immediately restores the trainee's saved MaxWeekUnlocked.
        
        private static DateTime BackdateStartDate(int savedWeek)
        {
            var currentMonday = GetWeekMonday(DateTime.UtcNow.Date);
            return currentMonday.AddDays(-((savedWeek - 1) * 7));
        }

        private static int SectionOrder(SectionType section) => section switch
        {
            SectionType.Warmup => 0,
            SectionType.Primer => 1,
            SectionType.MainWork => 2,
            SectionType.Cooldown => 3,
            _ => 99
        };

        private static EnrollmentDto MapToEnrollmentDto(TraineeProgramEnrollment e) => new()
        {
            Id = e.Id,
            WorkoutProgramID = e.WorkoutProgramId,
            ProgramName = e.WorkoutProgram?.Name ?? string.Empty,
            TrackName = e.Track?.Name ?? string.Empty,
            MaxWeekUnlocked = e.MaxWeekUnlocked,
            TotalWeeks = e.WorkoutProgram?.DurationOnWeeks ?? 0,
            Status = e.Status.ToString(),
            StartDate = e.StartDate,
            EndDate = e.EndDate
        };
    }
}
