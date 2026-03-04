using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec.EnrollmentSpec;
using FitZone.Core.Specifications.CommandSpec.SessionSpec;
using FitZone.Service.DTOs.EnrollmentDTOs;
using FitZone.Service.DTOs.SessionExerciseDTOs;
using FitZone.Service.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // ── Dashboard ────────────────────────────────────────────────

        public async Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId)
        {
            var spec = new TraineeActiveEnrollmentsSpec(traineeId);
            var enrollments = await _uow.Repository<TraineeProgramEnrollment>().GetAllWithSpecAsync(spec);

            var result = new List<EnrollmentDto>();
            foreach (var e in enrollments)
            {
                await SyncMaxWeekUnlockedAsync(e);  // keep stored value current on every read
                result.Add(MapToEnrollmentDto(e));
            }
            return result;
        }

        // ── History ──────────────────────────────────────────────────

        public async Task<IEnumerable<EnrollmentHistoryDto>> GetMyEnrollmentHistoryAsync(int traineeId)
        {
            var spec = new TraineeAllEnrollmentsSpec(traineeId);
            var enrollments = await _uow.Repository<TraineeProgramEnrollment>().GetAllWithSpecAsync(spec);

            return enrollments.Select(e => new EnrollmentHistoryDto
            {
                Id = e.ID,
                WorkoutProgramID = e.WorkoutProgramID,
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

        // ── Week access (gated) ──────────────────────────────────────

        public async Task<WeekDetailDto?> GetWeekAsync(int enrollmentId, int weekNumber, int traineeId)
        {
            var enrollSpec = new EnrollmentByIdSpec(enrollmentId, traineeId);
            var enrollment = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(enrollSpec);

            if (enrollment is null)
                throw new InvalidOperationException("Enrollment not found.");

            await SyncMaxWeekUnlockedAsync(enrollment);

            // Expired subscription: trainee can view any week they previously unlocked
            // Active subscription: trainee can view up to MaxWeekUnlocked
            // Either way — MaxWeekUnlocked is the gate.
            if (weekNumber > enrollment.MaxWeekUnlocked)
                throw new InvalidOperationException(
                    $"Week {weekNumber} is not yet unlocked. Your current access is up to Week {enrollment.MaxWeekUnlocked}.");

            var weekSpec = new ProgramWeekByNumberSpec(enrollment.WorkoutProgramID, weekNumber);
            var week = await _uow.Repository<ProgramWeek>().GetWithSpecAsync(weekSpec);

            if (week is null) return null;

            return new WeekDetailDto
            {
                WeekNumber = week.WeekNumber,
                WeekDescription = week.WeekDescription,
                FocusArea = week.FocusArea,
                IsUnlocked = true,
                SessionSummaryDto = week.WorkoutSessions
                                      .OrderBy(s => s.weekDay)
                                      .Select(s => new SessionSummaryDto
                                      {
                                          Id = s.ID,
                                          SessionTitle = s.SessionTitle,
                                          WeekDay = s.weekDay.ToString(),
                                          EstimatedDuration = s.EstimatedDuration
                                      })
                                      .ToList()
            };
        }

        // ── Session detail (gated) ───────────────────────────────────

        public async Task<WorkoutSessionDto?> GetSessionDetailAsync(int sessionId, int traineeId)
        {
            // Load session with its week so we can check week number
            var sessionSpec = new SessionWithExercisesSpec(sessionId);
            var session = await _uow.Repository<WorkoutSession>().GetWithSpecAsync(sessionSpec);

            if (session is null) return null;

            int programId = session.ProgramWeek.WorkoutProgramID;
            int weekNumber = session.ProgramWeek.WeekNumber;

            // Find any enrollment (active or historical) this trainee has for this program
            var gateSpec = new SessionAccessGateSpec(traineeId, programId);
            var enrollment = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(gateSpec);

            if (enrollment is null)
                throw new InvalidOperationException("You are not enrolled in this program.");

            await SyncMaxWeekUnlockedAsync(enrollment);

            if (weekNumber > enrollment.MaxWeekUnlocked)
                throw new InvalidOperationException(
                    $"Week {weekNumber} is not yet unlocked. Complete the previous week first.");

            // Load exercises with full exercise detail
            var exSpec = new SessionExercisesForSessionSpec(sessionId);
            var exercises = await _uow.Repository<SessionExercise>().GetAllWithSpecAsync(exSpec);

            var dto = _mapper.Map<WorkoutSessionDto>(session);

            // Group by section in correct training order, then by position within section
            dto.SessionExerciseDto = _mapper.Map<List<SessionExerciseDto>>(
                exercises.OrderBy(e => SectionOrder(e.SectionType))
                         .ThenBy(e => e.OrderInSection));

            return dto;
        }

        // ── Enroll / Resume ──────────────────────────────────────────

        public async Task<EnrollmentDto> StartProgramAsync(int traineeId, StartProgramDto dto)
        {
            var program = await _uow.Repository<WorkoutProgram>().GetAsync(dto.ProgramID);

            if (program is null || program.Status != ProgramStatus.Published)
                throw new InvalidOperationException("Program not found or not available.");

            // ── Step 1: Displace any active enrollment on the same track ──
            var activeSpec = new ActiveEnrollmentByTrackSpec(traineeId, program.TrackID);
            var activeOther = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(activeSpec);

            if (activeOther is not null && activeOther.WorkoutProgramID != dto.ProgramID)
            {
                // Different program on same track → suspend it (preserve progress)
                activeOther.IsActive = false;
                activeOther.Status = EnrollmentStatus.Cancelled;
                activeOther.EndDate = DateTime.UtcNow;
                _uow.Repository<TraineeProgramEnrollment>().Update(activeOther);
            }
            else if (activeOther is not null && activeOther.WorkoutProgramID == dto.ProgramID)
            {
                // Already enrolled in this exact program and it's active — just return current state
                await SyncMaxWeekUnlockedAsync(activeOther);
                await _uow.CompleteAsync();
                return MapToEnrollmentDto(activeOther);
            }

            // ── Step 2: Check for a previous enrollment in THIS program (resume logic) ──
            var previousSpec = new PreviousEnrollmentInProgramSpec(traineeId, dto.ProgramID);
            var previous = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(previousSpec);

            if (previous is not null)
            {
                // Resume: reactivate the old row so all history stays attached.
                // Recalculate StartDate so that unlocking math continues from the saved week.
                // If they were at week 6, we set StartDate = 5 weeks ago so week 6 is already unlocked.
                previous.IsActive = true;
                previous.Status = EnrollmentStatus.Active;
                previous.EndDate = null;
                previous.StartDate = BackdateStartDate(previous.MaxWeekUnlocked);

                _uow.Repository<TraineeProgramEnrollment>().Update(previous);
                await _uow.CompleteAsync();

                return MapToEnrollmentDto(previous);
            }

            // ── Step 3: Fresh enrollment ─────────────────────────────
            var enrollment = new TraineeProgramEnrollment
            {
                TraineeID = traineeId,
                WorkoutProgramID = dto.ProgramID,
                TrackID = program.TrackID,
                StartDate = DateTime.UtcNow,
                MaxWeekUnlocked = 1,
                Status = EnrollmentStatus.Active,
                IsActive = true
            };

            _uow.Repository<TraineeProgramEnrollment>().Add(enrollment);
            await _uow.CompleteAsync();

            // Reload with nav props for mapping
            var reloadSpec = new EnrollmentByIdSpec(enrollment.ID, traineeId);
            var loaded = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(reloadSpec);

            return MapToEnrollmentDto(loaded!);
        }

        // ── Cancel ───────────────────────────────────────────────────

        public async Task CancelEnrollmentAsync(int enrollmentId, int traineeId)
        {
            var spec = new EnrollmentByIdSpec(enrollmentId, traineeId);
            var enrollment = await _uow.Repository<TraineeProgramEnrollment>().GetWithSpecAsync(spec);

            if (enrollment is null)
                throw new InvalidOperationException("Enrollment not found.");

            if (!enrollment.IsActive)
                throw new InvalidOperationException("This enrollment is already inactive.");

            enrollment.IsActive = false;
            enrollment.Status = EnrollmentStatus.Cancelled;
            enrollment.EndDate = DateTime.UtcNow;

            _uow.Repository<TraineeProgramEnrollment>().Update(enrollment);
            await _uow.CompleteAsync();
        }

        // ── Private helpers ──────────────────────────────────────────

       
        /// Computes how many weeks should be unlocked right now based on StartDate,
        /// then updates MaxWeekUnlocked in the DB if it has gone up.
        /// Called on every read — no manual trigger needed.
     
        private async Task SyncMaxWeekUnlockedAsync(TraineeProgramEnrollment enrollment)
        {
            int due = ComputeWeeksDue(enrollment.StartDate, enrollment.WorkoutProgram?.DurationOnWeeks ?? int.MaxValue);

            if (due > enrollment.MaxWeekUnlocked)
            {
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
        }

        /// How many weeks are due based on calendar Mondays elapsed since StartDate.
        /// Week 1 is unlocked immediately (result is always >= 1).
        /// Week N unlocks on the Monday of the Nth calendar week after enrollment.
       
        private static int ComputeWeeksDue(DateTime startDate, int maxWeeks)
        {
            var startMonday = GetWeekMonday(startDate.Date);
            var currentMonday = GetWeekMonday(DateTime.UtcNow.Date);

            int weeksPassed = (int)((currentMonday - startMonday).TotalDays / 7);
            int unlocked = weeksPassed + 1; // week 1 unlocked immediately

            return Math.Min(unlocked, maxWeeks);
        }

        private static DateTime GetWeekMonday(DateTime date)
        {
            int daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.AddDays(-daysFromMonday);
        }

        /// When resuming, we backdated StartDate so that the unlock math
        /// makes MaxWeekUnlocked resolve to the saved week immediately.
        /// If the trainee was at week 6, set StartDate = Monday of 5 weeks ago.
       
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
            Id = e.ID,
            WorkoutProgramID = e.WorkoutProgramID,
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
