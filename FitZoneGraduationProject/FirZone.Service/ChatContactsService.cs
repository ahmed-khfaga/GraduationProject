using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Core.Specifications.CommandSpec.EnrollmentSpec;
using FitZone.Core.Specifications.CommandSpec.ProfileSpec;
using FitZone.Service.DTOs.ChatContactsDTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    public class ChatContactsService : IChatContactsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatContactsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ── Trainee view ─────────────────────────────────────────────────────

        public async Task<TraineeChatContactsDto> GetContactsForTraineeAsync(string appUserId)
        {
            // 1. Resolve trainee
            var trainee = await _unitOfWork.Repository<Trainee>()
                .GetWithSpecAsync(new TraineeByUserIdSpec(appUserId))
                ?? throw new InvalidOperationException("Trainee profile not found.");

            // 2. All active workout enrollments — Coach chain fully loaded
            var workoutEnrollments = await _unitOfWork.Repository<TraineeProgramEnrollment>()
                .GetAllWithSpecAsync(new TraineeActiveEnrollmentsWithCoachSpec(trainee.Id));

            // 3. All active nutrition enrollments — NutritionPlan.Coach chain fully loaded
            var nutritionEnrollments = await _unitOfWork.Repository<TraineeNutritionEnrollment>()
                .GetAllWithSpecAsync(new TraineeActiveNutritionEnrollmentsSpec(trainee.Id));

            // 4. Group by CoachId — one card per coach
            var coaches = workoutEnrollments
                .GroupBy(e => e.WorkoutProgram.CoachId)
                .Select(group =>
                {
                    var coach = group.First().WorkoutProgram.Coach;
                    var appUser = coach.ApplicationUser;

                    // Most recent active workout under this coach
                    var activeWorkout = group.OrderByDescending(e => e.StartDate).First();

                    // Active nutrition plan whose coach matches
                    var activeNutrition = nutritionEnrollments
                        .FirstOrDefault(n => n.NutritionPlan.CoachID == coach.Id);

                    return new CoachContactDto
                    {
                        CoachId = appUser.Id,
                        CoachName = appUser.UserName ?? appUser.Email,
                        CoachAvatarUrl = coach.PhotoUrl,
                        Specialization = coach.About,
                        ActiveWorkoutProgram = MapWorkout(activeWorkout),
                        ActiveNutritionPlan = activeNutrition != null
                            ? MapNutrition(activeNutrition) : null
                    };
                })
                .ToList();

            return new TraineeChatContactsDto { Coaches = coaches };
        }

        // ── Coach view ───────────────────────────────────────────────────────

        public async Task<CoachChatContactsDto> GetContactsForCoachAsync(string appUserId)
        {
            // 1. Resolve coach
            var coach = await _unitOfWork.Repository<Coach>()
                .GetWithSpecAsync(new CoachByUserIdSpec(appUserId))
                ?? throw new InvalidOperationException("Coach profile not found.");

            // 2. All active enrollments in any of this coach's programs
            //    Trainee + ApplicationUser are loaded inside the spec
            var activeEnrollments = await _unitOfWork.Repository<TraineeProgramEnrollment>()
                .GetAllWithSpecAsync(new ActiveEnrollmentsForCoachSpec(coach.Id));

            if (!activeEnrollments.Any())
                return new CoachChatContactsDto { Trainees = new List<TraineeContactDto>() };

            // 3. Build one card per distinct trainee
            var trainees = new List<TraineeContactDto>();

            var distinctTrainees = activeEnrollments
                .GroupBy(e => e.TraineeId);

            foreach (var group in distinctTrainees)
            {
                var traineeEntity = group.First().Trainee;
                var appUser = traineeEntity.ApplicationUser;
                var traineeId = traineeEntity.Id;

                // Current workout = most recent active enrollment under this coach
                var currentWorkout = group.OrderByDescending(e => e.StartDate).First();

                // Current nutrition under this coach
                var allNutritionActive = await _unitOfWork.Repository<TraineeNutritionEnrollment>()
                    .GetAllWithSpecAsync(new TraineeActiveNutritionEnrollmentsSpec(traineeId));

                var currentNutrition = allNutritionActive
                    .FirstOrDefault(n => n.NutritionPlan.CoachID == coach.Id);

                // Past workout enrollments under this coach
                var pastWorkouts = await _unitOfWork.Repository<TraineeProgramEnrollment>()
                    .GetAllWithSpecAsync(new TraineeAllEnrollmentsByCoachSpec(traineeId, coach.Id));

                // Past nutrition enrollments under this coach
                var allNutritionHistory = await _unitOfWork.Repository<TraineeNutritionEnrollment>()
                    .GetAllWithSpecAsync(new TraineeAllNutritionEnrollmentsSpec(traineeId));

                var pastNutrition = allNutritionHistory
                    .Where(n => n.NutritionPlan.CoachID == coach.Id)
                    .ToList();

                trainees.Add(new TraineeContactDto
                {
                    TraineeId = appUser.Id,
                    TraineeName = appUser.UserName ?? appUser.Email,
                    TraineeAvatarUrl = traineeEntity.PhotoUrl,
                    JoinedAt = group.Min(e => e.StartDate),
                    CurrentWorkoutProgram = MapWorkout(currentWorkout),
                    CurrentNutritionPlan = currentNutrition != null
                        ? MapNutrition(currentNutrition) : null,
                    History = new TraineeEnrollmentHistoryDto
                    {
                        WorkoutPrograms = pastWorkouts.Select(MapWorkout).ToList(),
                        NutritionPlans = pastNutrition.Select(MapNutrition).ToList()
                    }
                });
            }

            return new CoachChatContactsDto { Trainees = trainees };
        }

        // ── Private mappers ──────────────────────────────────────────────────

        private static WorkoutEnrollmentDto MapWorkout(TraineeProgramEnrollment e) =>
            new WorkoutEnrollmentDto
            {
                EnrollmentId = e.Id,
                ProgramId = e.WorkoutProgramId,
                ProgramName = e.WorkoutProgram.Name,
                TrackId = e.TrackId,
                TrackName = e.Track?.Name ?? string.Empty,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                CurrentWeek = e.MaxWeekUnlocked,
                TotalWeeks = e.WorkoutProgram.DurationOnWeeks,
                Status = e.Status.ToString()
            };

        private static NutritionEnrollmentDto MapNutrition(TraineeNutritionEnrollment e) =>
            new NutritionEnrollmentDto
            {
                EnrollmentId = e.Id,
                PlanId = e.NutritionPlanID,
                PlanName = e.NutritionPlan.Name,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                GoalType = e.NutritionPlan.TrainingGoal.ToString(),
                Status = e.Status.ToString()
            };
    }
}