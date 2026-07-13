namespace FitZone.Service.DTOs.ChatContactsDTOs
{
    // ── Shared ───────────────────────────────────────────────────────────────

    public class WorkoutEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public int TrackId { get; set; }
        public string TrackName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CurrentWeek { get; set; }
        public int TotalWeeks { get; set; }
        public string Status { get; set; }
    }

    public class NutritionEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GoalType { get; set; }
        public string Status { get; set; }
    }

    // ── Trainee view ─────────────────────────────────────────────────────────

    public class CoachContactDto
    {
        public string CoachId { get; set; }          // ApplicationUser.Id
        public string CoachName { get; set; }
        public string? CoachAvatarUrl { get; set; }
        public string? Specialization { get; set; }
        public WorkoutEnrollmentDto? ActiveWorkoutProgram { get; set; }
        public NutritionEnrollmentDto? ActiveNutritionPlan { get; set; }
    }

    public class TraineeChatContactsDto
    {
        public string Role { get; set; } = "Trainee";
        public List<CoachContactDto> Coaches { get; set; } = new();
    }

    // ── Coach view ───────────────────────────────────────────────────────────

    public class TraineeEnrollmentHistoryDto
    {
        public List<WorkoutEnrollmentDto> WorkoutPrograms { get; set; } = new();
        public List<NutritionEnrollmentDto> NutritionPlans { get; set; } = new();
    }

    public class TraineeContactDto
    {
        public string TraineeId { get; set; }        // ApplicationUser.Id
        public string TraineeName { get; set; }
        public string? TraineeAvatarUrl { get; set; }
        public DateTime JoinedAt { get; set; }
        public WorkoutEnrollmentDto? CurrentWorkoutProgram { get; set; }
        public NutritionEnrollmentDto? CurrentNutritionPlan { get; set; }
        public TraineeEnrollmentHistoryDto History { get; set; } = new();
    }

    public class CoachChatContactsDto
    {
        public string Role { get; set; } = "Coach";
        public List<TraineeContactDto> Trainees { get; set; } = new();
    }
}